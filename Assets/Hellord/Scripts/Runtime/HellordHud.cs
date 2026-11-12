using PixelFXPack;
using UnityEngine;

// v0.7: HUD Option B (Art/export/ui/hud_layout_optionB_640x360.json) on a 640x360 virtual canvas, integer zoom, point-filtered art.
// All text is drawn in code with ui_font_5x7 (white glyphs tinted, 1px #181425 shadow). No low-HP pulse (frame 1 "low" art only).
// Kill counter is off by default (K toggles). Debug text + skill slots are behind F9 (old 960x540 canvas).
namespace Hellord
{
    public partial class HellordGame
    {
        const int HW = 640, HH = 360;
        const string FontMap = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ:/+-%!.x'? ";
        static readonly Color HInk = new Color32(0x18, 0x14, 0x25, 255), HCyan = new Color32(0x2c, 0xe8, 0xf5, 255);
        Texture2D _tHp, _tFill, _tXp, _tGoal, _tPanel, _tCells, _tIcons5, _tCorr, _tCIcons, _tCBg, _tFont; float _ghostHp = -1;

        static Texture2D UiTex(string n) { var t = Resources.Load<Texture2D>("HellordArt/ui_" + n); if (t) { t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; } return t; }
        void HudBegin()
        {
            if (_tFont == null)
            {
                _tHp = UiTex("hud_hp_widget"); _tFill = UiTex("hud_hp_bar_fill"); _tXp = UiTex("hud_xp_bar"); _tGoal = UiTex("hud_goal_box");
                _tPanel = UiTex("minimap_panel"); _tCells = UiTex("minimap_cells"); _tIcons5 = UiTex("minimap_icons_5"); _tCorr = UiTex("minimap_corridor");
                _tCIcons = UiTex("hud_counter_icons"); _tCBg = UiTex("hud_counter_bg"); _tFont = UiTex("font_5x7");
            }
            float z = Mathf.Min(Mathf.Floor(Screen.width / (float)HW), Mathf.Floor(Screen.height / (float)HH));
            if (z < 1) z = Mathf.Min(Screen.width / (float)HW, Screen.height / (float)HH);
            GUI.matrix = Matrix4x4.TRS(new Vector3(Mathf.Round((Screen.width - HW * z) / 2), Mathf.Round((Screen.height - HH * z) / 2), 0), Quaternion.identity, new Vector3(z, z, 1));
        }

        /// <summary>Draws source rect (sx,sy,w,h; y from the top) of t at (x,y), optionally stretched to dw x dh.</summary>
        static void Blit(Texture2D t, float sx, float sy, float sw, float sh, float x, float y, float dw = -1, float dh = -1)
        {
            if (!t || sw <= 0 || sh <= 0) return;
            GUI.DrawTextureWithTexCoords(new Rect(x, y, dw < 0 ? sw : dw, dh < 0 ? sh : dh), t, new Rect(sx / t.width, 1 - (sy + sh) / t.height, sw / t.width, sh / t.height));
        }
        static void Nine(Texture2D t, float fx, float fw, float fh, int l, int r, int tp, int b, float x, float y, float w, float h)
        {
            float cw = fw - l - r, ch = fh - tp - b, iw = w - l - r, ih = h - tp - b;
            Blit(t, fx, 0, l, tp, x, y); Blit(t, fx + l, 0, cw, tp, x + l, y, iw, tp); Blit(t, fx + fw - r, 0, r, tp, x + w - r, y);
            Blit(t, fx, tp, l, ch, x, y + tp, l, ih); Blit(t, fx + l, tp, cw, ch, x + l, y + tp, iw, ih); Blit(t, fx + fw - r, tp, r, ch, x + w - r, y + tp, r, ih);
            Blit(t, fx, fh - b, l, b, x, y + h - b); Blit(t, fx + l, fh - b, cw, b, x + l, y + h - b, iw, b); Blit(t, fx + fw - r, fh - b, r, b, x + w - r, y + h - b);
        }
        static int TextW(string s) => s.Length * 6 - 1;
        /// <summary>5x7 text: uppercase; chars outside the map become '?'; 1px shadow below in #181425.</summary>
        void Text(string s, float x, float y, Color c, bool shadow = true, int sc = 1)
        {
            if (!_tFont) return; // v0.0.14 perf: per-char upper (no string alloc per call)
            for (int pass = shadow ? 0 : 1; pass < 2; pass++)
            {
                var tc = pass == 0 ? HInk : c; tc.a *= _guiA; GUI.color = tc;
                for (int i = 0; i < s.Length; i++)
                {
                    char ch = char.ToUpperInvariant(s[i]); if (ch == ' ') continue; int k = FontMap.IndexOf(ch == 'X' ? 'X' : ch); if (k < 0) k = FontMap.IndexOf('?');
                    Blit(_tFont, k * 5, 0, 5, 7, x + i * 6 * sc, y + (pass == 0 ? sc : 0), 5 * sc, 7 * sc);
                }
            }
            GUI.color = new Color(1, 1, 1, _guiA);
        }
        /// <summary>v0.0.26: alpha multiplier for Text/blits (LevelUpFX card reveal); 1 everywhere else.</summary>
        static float _guiA = 1f;
        static string Sanitize(string s)
        {
            var sb = new System.Text.StringBuilder(); foreach (char c0 in s.ToUpperInvariant()) { char c = c0 == ',' || c0 == ';' ? '.' : c0 == '=' ? ':' : c0 == '(' || c0 == '[' ? ' ' : c0; if (c0 == ')' || c0 == ']') continue; sb.Append(c); }
            for (int i = sb.Length - 1; i > 0; i--) if (sb[i] == ' ' && (sb[i - 1] == ' ' || (i + 1 < sb.Length && (sb[i + 1] == '.' || sb[i + 1] == ':')))) sb.Remove(i, 1);
            return sb.ToString();
        }

        void DrawHudB()
        {
            float dt = Event.current.type == EventType.Repaint ? Time.unscaledDeltaTime : 0;
            // HP widget (4,4): frame 0 full / 1 low (<30%) / 2 empty; fill 70x6 at (14,5); damage ghost shrinks over ~0.3 s
            float f = maxHp > 0 ? Mathf.Clamp01(hp / maxHp) : 0; if (_ghostHp < 0 || hp > _ghostHp) _ghostHp = hp;
            _ghostHp = Mathf.MoveTowards(_ghostHp, hp, Mathf.Max(maxHp * 0.05f, (_ghostHp - hp) / 0.3f * 3f) * dt);
            int hf = hp <= 0 ? 2 : f < 0.3f ? 1 : 0; Blit(_tHp, hf * 86, 0, 86, 16, 4, 4);
            float gw = Mathf.Round(70 * Mathf.Clamp01(_ghostHp / maxHp)), fw = Mathf.Round(70 * f);
            if (gw > fw) Blit(_tFill, 70 + fw, 0, gw - fw, 6, 18 + fw, 9);
            Blit(_tFill, 0, 0, fw, 6, 18, 9);
            string hn = Mathf.CeilToInt(Mathf.Max(0, hp)).ToString();
            GUI.color = new Color(HInk.r, HInk.g, HInk.b, 0.84f); GUI.DrawTexture(new Rect(90, 7, TextW(hn) + 4, 11), Texture2D.whiteTexture); GUI.color = Color.white;
            Text(hn, 92, 9, Color.white, false);
            // XP bar (17,21) 98x9: empty frame, then the full frame cropped to the fill (x 25..96)
            float xf = Mathf.Clamp01((float)xp / Mathf.Max(1, HData.XpToNext(level))); if (pendingLevels > 0) xf = 1; // queued level-ups: bar full until the last card
            float xpx = HellordTuning.I.hudCornerV45 ? HellordTuning.I.hudXpBarX : 17; /* v0.0.45: LV / XP left edge = HP widget left edge (4) */
            Blit(_tXp, 0, 0, 98, 9, xpx, 21); Blit(_tXp, 98, 0, 25 + Mathf.Round(72 * xf), 9, xpx, 21);
            Text(ShownLevel < 10 ? "LV" + ShownLevel : "L" + Mathf.Min(ShownLevel, 99), xpx + 1, 22, HCyan, false);
            // v0.8: goal box shows the current room's goal; minimap is built from the floor layout (HellordFloor.cs)
            if (floorMode)
            {
                { float gs = HellordTuning.I.hudCornerV45 ? Mathf.Clamp(Mathf.Round(HellordTuning.I.hudGoalScale), 1, 3) : 1; var gm0 = GUI.matrix; /* v0.0.45: goal box scaled by a whole number around top-centre (crisp) */
                  if (gs != 1) GUIUtility.ScaleAroundPivot(new Vector2(gs, gs), new Vector2(320, 4));
                  DrawGoalB(); GUI.matrix = gm0;
                  if (HellordTuning.I.hudFloorName) { string fn = FloorLabel53(ThemeSlot53, true); int fs = Mathf.Clamp(Mathf.RoundToInt(HellordTuning.I.hudFloorNameScale), 1, 3); /* v0.0.45: hell floor name under the minimap + FLOOR label, right-aligned */
                    float mms = Mathf.Clamp(HellordTuning.I.hudMinimapScale, 1f, 3f), fnw = TextW(fn) * fs, fnx = 636 - fnw, fy = Mathf.Round(4 + (floorRows * 8 + 9) * mms + HellordTuning.I.hudFloorNameGapPx + 2); /* v0.0.45: top edge right under the map frame, right edge = frame right edge (636); +2 = backing pad */
                    if (HellordTuning.I.hudFloorNameBg) { GUI.color = new Color(HInk.r, HInk.g, HInk.b, 0.7f); GUI.DrawTexture(new Rect(fnx - 3, fy - 2, fnw + 6, 7 * fs + 5), Texture2D.whiteTexture); GUI.color = Color.white; }
                    Text(fn, fnx, fy, Theme == 0 ? (Color)new Color32(0xf7, 0x76, 0x22, 255) : Theme == 1 ? (Color)new Color32(0x2c, 0xe8, 0xf5, 255) : (Color)new Color32(0xfe, 0xae, 0x34, 255), true, fs); }
                }
                float ms = Mathf.Clamp(HellordTuning.I.hudMinimapScale, 1f, 3f); var gm = GUI.matrix; /* v0.0.45: minimap + floor label scaled around the top-right corner */
                if (ms != 1f) GUIUtility.ScaleAroundPivot(new Vector2(ms, ms), new Vector2(636, 4));
                DrawMinimapB(); GUI.matrix = gm;
            }
            // Counters bottom-left: blood, bone, (kills if K)
            if (HellordTuning.I.hudCornerV45)
            {   // v0.0.45: counters directly under the LV / XP bar, top-left (blood, bone, kills)
                float cy = HellordTuning.I.hudCounterTopPx;
                float tw = CounterW(blood) + CounterW(bones) + (showKills ? CounterW(kills) : 0) - 4;
                float bx = HellordTuning.I.hudCounterLeftPx; Counter(ref bx, 0, blood, cy); Counter(ref bx, 1, bones, cy); if (showKills) Counter(ref bx, 2, kills, cy);
            }
            else { float bx = 4; Counter(ref bx, 0, blood); Counter(ref bx, 1, bones); if (showKills) Counter(ref bx, 2, kills); }
            float lblUp = 0;
            DrawBossHudB(); DrawBossArrowB();
            if (glowLabelT > 0) { glowLabelT -= Time.unscaledDeltaTime; string gl = "CRT GLOW " + crtGlow.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + (!crtEnabled ? " (CRT OFF)" : ""); Text(gl, 640 - TextW(gl) - 6, 350 - lblUp, new Color32(0xfe, 0xe7, 0x61, 255)); } // v0.0.21 G key label
            if (atmosLabelT > 0) { atmosLabelT -= Time.unscaledDeltaTime; string al = "HELL ATMOSPHERE " + (atmosOn ? "ON" : "OFF"); Text(al, 640 - TextW(al) - 6, 340 - lblUp, new Color32(0xfe, 0xe7, 0x61, 255)); } // v0.0.22 H key label
        }
        /// <summary>v0.0.45: the FLOOR n/N label drops below the big floor name (minimap-local px, the minimap is drawn scaled).</summary>
        float FloorLblDy45()
        {
            if (!HellordTuning.I.hudFloorName) return 0;
            int fs = Mathf.Clamp(Mathf.RoundToInt(HellordTuning.I.hudFloorNameScale), 1, 3); float mms = Mathf.Clamp(HellordTuning.I.hudMinimapScale, 1f, 3f);
            return Mathf.Ceil((HellordTuning.I.hudFloorNameGapPx + 7 * fs + 5) / mms);
        }
        static readonly string[] FloorNames45 = { "OX PIT", "FROZEN HELL", "BURNING HELL" };
        float CounterW(int v) { float w = 14 + TextW(v.ToString()); return Mathf.Max(40, w + 4); }
        void Counter(ref float x, int icon, int v) => Counter(ref x, icon, v, 346);
        void Counter(ref float x, int icon, int v, float cy)
        {
            string s = v.ToString(); float w = 14 + TextW(s);
            Blit(_tCBg, 0, 0, 16, 11, x, cy, w, 11); Blit(_tCIcons, icon * 8, 0, 8, 8, x + 1, cy + 1); Text(s, x + 11, cy + 2, Color.white, false);
            x = Mathf.Max(x + 40, x + w + 4); // layout json: 4, 44, 84 (grows if a number gets long)
        }
        void DrawToastB()
        {
            string t = Sanitize(toast); float w = TextW(t); float x = Mathf.Round(320 - w / 2), y = MsgSlotY(0); MsgRect(new Rect(x - 3, y - 2, w + 6, 12), "toast");
            GUI.color = new Color(HInk.r, HInk.g, HInk.b, 0.7f); GUI.DrawTexture(new Rect(x - 3, y - 2, w + 6, 12), Texture2D.whiteTexture); GUI.color = Color.white;
            Text(t, x, y, Color.white);
        }
        /// <summary>Boss bar on the 640 canvas: 216x16 art at 1x, centred under the goal box (y 20), 5x7 name below.</summary>
        /// <summary>v0.0.9: off-screen boss pointer - a 7px chevron + BOSS at the canvas edge toward the boss.</summary>
        int _bossArrowFrame = -9;
        bool _bossArrowDrawn => Time.frameCount - _bossArrowFrame <= 2;
        void DrawBossArrowB()
        {
            if (!BossAlive || _cam == null || mode != Mode.Play) return;
            if (!BossOffView()) return;
            Vector3 v = _cam.WorldToViewportPoint(_boss.pos + Vector2.up * 1.5f);
            _bossArrowFrame = Time.frameCount;
            Vector2 d = new Vector2(v.x - 0.5f, 0.5f - v.y); if (d.sqrMagnitude < 1e-6f) return; d.Normalize();
            float t = Mathf.Min((HW / 2f - 18) / Mathf.Max(0.001f, Mathf.Abs(d.x)), (HH / 2f - 18) / Mathf.Max(0.001f, Mathf.Abs(d.y)));
            Vector2 c = new Vector2(HW / 2f, HH / 2f) + d * t; c.y = Mathf.Max(c.y, Mathf.Abs(c.x - HW / 2f) < 110 ? MsgStackBottom() + 10 : 20 + HArt.HpBarH + 22); // v0.0.17: never inside the message stack; v0.0.14: stay below the boss bar + name
            float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            bool blink = ((int)(Time.unscaledTime * 4) & 1) == 0;
            var m = GUI.matrix; _bossArrowPos = c;
            // v0.0.16: explicit pivot rotation INSIDE the zoomed HUD matrix. GUIUtility.RotateAroundPivot treats the pivot as
            // screen pixels, so under our zoom/letterbox matrix it swung the chevron off-canvas (banner showed, arrow didn't).
            GUI.matrix = m * Matrix4x4.TRS(new Vector3(c.x, c.y, 0), Quaternion.Euler(0, 0, ang), Vector3.one) * Matrix4x4.TRS(new Vector3(-c.x, -c.y, 0), Quaternion.identity, Vector3.one);
            GUI.color = HInk; GUI.DrawTexture(new Rect(c.x - 5, c.y - 5, 10, 10), Texture2D.whiteTexture);
            GUI.color = blink ? new Color(0.95f, 0.2f, 0.15f) : new Color(1f, 0.86f, 0.62f);
            for (int i = 0; i < 4; i++) GUI.DrawTexture(new Rect(c.x - 3 + i, c.y - 3 + i, 2, 8 - 2 * i), Texture2D.whiteTexture);
            GUI.matrix = m; GUI.color = Color.white;
            if (_bossOffT > 0.4f) { const string w = "BOSS OFF-SCREEN"; float tw = TextW(w); GUI.color = new Color(HInk.r, HInk.g, HInk.b, 0.7f); float sy = MsgSlotY(1); MsgRect(new Rect(320 - tw / 2 - 3, sy - 2, tw + 6, 12), "offscreen"); GUI.DrawTexture(new Rect(320 - tw / 2 - 3, sy - 2, tw + 6, 12), Texture2D.whiteTexture); GUI.color = Color.white; Text(w, Mathf.Round(320 - tw / 2), sy, blink ? new Color(1f, 0.35f, 0.3f) : new Color(1f, 0.86f, 0.62f)); }
            Text("BOSS", Mathf.Round(c.x - d.x * 18 - TextW("BOSS") / 2f), Mathf.Round(c.y + (d.y < -0.5f ? 8 : -d.y * 14 - 3)), new Color(1f, 0.86f, 0.62f));
        }
        // v0.0.15: 'BLOODRAGE!' banner anchored above the boss (HUD canvas coords; clamped on-screen, below the boss bar)
        public Vector2 _bossArrowPos; public float _rageBannerT; public Vector2 _rageBannerPos; public int _rageBannerFrame = -9;
        public Vector2 WorldToHud(Vector2 w)
        {
            float z = Mathf.Min(Mathf.Floor(Screen.width / (float)HW), Mathf.Floor(Screen.height / (float)HH));
            if (z < 1) z = Mathf.Min(Screen.width / (float)HW, Screen.height / (float)HH);
            float ox = Mathf.Round((Screen.width - HW * z) / 2), oy = Mathf.Round((Screen.height - HH * z) / 2);
            var v = _cam.WorldToViewportPoint(w); float sx = v.x * Screen.width, sy = (1 - v.y) * Screen.height;
            return new Vector2((sx - ox) / z, (sy - oy) / z);
        }
        void DrawRageBannerB()
        {
            if (_rageBannerT <= 0 || !BossAlive || _cam == null || mode != Mode.Play) return;
            const string t = "BLOODRAGE!"; float w = TextW(t);
            Vector2 c = WorldToHud(_boss.pos + Vector2.up * 3.4f);
            c.x = Mathf.Clamp(c.x, w / 2 + 6, HW - w / 2 - 6); c.y = _bossOffT > 0 ? MsgSlotY(2) + 4 : Mathf.Clamp(c.y, MsgStackBottom() + 8, HH - 24); // v0.0.17: slot 2 when off view, else below the stack // v0.0.16: below the OFF-SCREEN banner when the boss is off view
            _rageBannerPos = c; _rageBannerFrame = Time.frameCount;
            float x = Mathf.Round(c.x - w / 2), y = Mathf.Round(c.y - 4); MsgRect(new Rect(x - 3, y - 2, w + 6, 12), "rage");
            GUI.color = new Color(HInk.r, HInk.g, HInk.b, 0.75f); GUI.DrawTexture(new Rect(x - 3, y - 2, w + 6, 12), Texture2D.whiteTexture); GUI.color = Color.white;
            Text(t, x, y, ((int)(Time.unscaledTime * 6) & 1) == 0 ? new Color(1f, 0.3f, 0.22f) : new Color(1f, 0.86f, 0.62f));
        }
        // ---- v0.0.17: one top-centre message stack with fixed slots (0 toast, 1 BOSS OFF-SCREEN, 2 BLOODRAGE when off view) ----
        public int bossBarState = -1; public float bossBarFrac;
        public readonly System.Collections.Generic.List<(Rect r, string id)> msgRects = new System.Collections.Generic.List<(Rect, string)>();
        int _msgFrame = -1;
        void MsgRect(Rect r, string id) { if (_msgFrame != Time.frameCount) { msgRects.Clear(); _msgFrame = Time.frameCount; } msgRects.Add((r, id)); }
        float MsgStackTop() => BossAlive ? 20 + HArt.HpBarH + 14 : 52;
        float MsgSlotY(int i) => MsgStackTop() + i * 14;
        float MsgStackBottom() => MsgSlotY(3);
        float _bossBarA = 1f; public bool bossBarFaded;
        void DrawBossHudB()
        {
            if (!BossAlive) return;
            if (_hpTex == null) _hpTex = HArt.HpBar(); if (_hpTex == null) return;
            float frac = Mathf.Clamp01(_boss.hp / _boss.maxHp), x = 320 - HArt.HpBarW / 2, y = 20;
            // v0.0.19: when Ox-Head walks up under the bar (head/horns inside the bar + name band) the bar fades to 35% so the boss stays readable
            Vector2 bhd = WorldToHud(_boss.pos + Vector2.up * (BossHeadY + 0.4f)), bft = WorldToHud(_boss.pos);
            bool under = bhd.y < y + HArt.HpBarH + 12 && bft.y > y - 4 && Mathf.Abs(bhd.x - 320) < HArt.HpBarW / 2 + 24;
            _bossBarA = Mathf.MoveTowards(_bossBarA, under ? 0.35f : 1f, Time.unscaledDeltaTime * 4f); bossBarFaded = _bossBarA < 0.99f;
            GUI.color = new Color(1, 1, 1, _bossBarA);
            int st = _hpFlashT > 0 ? _hpFlashF : _raging ? 3 : _phase2 ? 2 : 0; // v0.0.17: the fill always shows real HP; stun is an overlay (was the hatched stun art = looked like ~25% HP)
            Blit(_hpTex, 5 * HArt.HpBarW, 0, HArt.HpBarW, HArt.HpBarH, x, y);
            float cut = Mathf.Round(HArt.HpBarCap + HArt.HpBarFill * frac);
            Blit(_hpTex, st * HArt.HpBarW, 0, cut, HArt.HpBarH, x, y);
            if (_stunBar != null && _stunBar.IsVisible && _stunBar.Texture != null)
            {   // v0.0.21: StunBarFX #114 overlay (stripes / badge / stars over the 200x10 fill; HP fill untouched). Replaces the blinking frame.
                var sz = _stunBar.TextureSizePx; GUI.color = Color.white;
                GUI.DrawTexture(new Rect(x + HArt.HpBarCap - PixelFXPack.StunBarFX.PadX, y + 3 - PixelFXPack.StunBarFX.PadTop, sz.x, sz.y), _stunBar.Texture);
            }
            GUI.color = Color.white;
            bossBarState = st; bossBarFrac = Mathf.Round(HArt.HpBarFill * frac) / Mathf.Max(1f, HArt.HpBarFill);
            string n = BossName37() + (BossStunned ? "  STUNNED X2" : _raging ? "  BLOODRAGE" : "");
            MsgRect(new Rect(320 - TextW(n) / 2f, y + HArt.HpBarH + 2, TextW(n), 8), "name");
            Text(n, Mathf.Round(320 - TextW(n) / 2f), y + HArt.HpBarH + 2, BossStunned ? new Color32(0xfe, 0xe7, 0x61, 255) : _raging ? new Color32(0xe4, 0x3b, 0x44, 255) : Color.white);
        }
    }
}
