using System.Collections.Generic;
using UnityEngine;

// v0.0.26: floor index + palette, big centred banners (important notices), the bottom skill bar.
// All numbers come from HellordTuning. Text is the exported 5x7 font via Text(); the bar uses the exported icons_skill_16 art
// (Hellord Art), locked slots / cooldown sweep / pips are code-drawn.
namespace Hellord
{
    public enum BannerKind { Info, Good, Danger, Floor }

    public partial class HellordGame
    {
        // ================= floors =================
        int _floorIdx;
        public int FloorIdx => floorMode ? _floorIdx : 0;
        public int FloorCount => Mathf.Max(1, HellordTuning.I.floorCount);
        Color FloorTint() { var a = HellordTuning.I.floorTints; if (a == null || a.Length == 0) return Color.white; return a[Mathf.Clamp(Theme, 0, a.Length - 1)]; } // v0.0.42: tint = theme
        Color FloorGradeLinear()
        {
            var T = HellordTuning.I; Color g = T.gradeTint; var a = T.floorGradeMul;
            if (a != null && a.Length > 0) { var m = a[Mathf.Clamp(Theme, 0, a.Length - 1)]; g = new Color(g.r * m.r, g.g * m.g, g.b * m.b, 1); } // v0.0.42: grade = theme
            if (floorMode && Mortar52) { float k = T.mortarGradeK677; g = new Color(g.r * k, g.g * k, g.b * k, 1); } // t677u Mortar only
            if (T.d2Dark606 && !T.v2Light586) { float k = T.d2Ambient606; g = new Color(g.r * k, g.g * k, g.b * k, 1); } // t606u D2 ambient darkness
            return g;
        }
        void UpdateRoomGrade() { if (_room != null) _room.ApplyGrade(FloorTint(), FloorGradeLinear(), atmosOn); }

        // ================= banners =================
        class BannerItem { public string title, sub; BannerKind kind; public float t, hold; public BannerItem(string a, string b, BannerKind k, float h) { title = a; sub = b; kind = k; hold = h; } public BannerKind Kind => kind; }
        readonly List<BannerItem> _banners = new List<BannerItem>(4);
        /// <summary>Every banner ever posted this run (tests / report).</summary>
        public readonly List<string> bannerLog = new List<string>();
        public string BannerTitle => _banners.Count > 0 ? _banners[0].title : null;
        public bool BannerActive => _banners.Count > 0;
        public float BannerAlpha { get; private set; }
        public Rect BannerRect { get; private set; }
        /// <summary>Important notice: big, centred, dark band. Queued one after another; a newer banner skips the queue if 3 are waiting.</summary>
        public void PostBanner(string title, string sub = null, BannerKind kind = BannerKind.Good, float hold = -1)
        {
            var T = HellordTuning.I; if (hold < 0) hold = kind == BannerKind.Info ? T.bannerGoalHold : T.bannerHold;
            title = Sanitize(title); sub = string.IsNullOrEmpty(sub) ? null : Sanitize(sub);
            bannerLog.Add(title);
            if (kind == BannerKind.Floor) HellordAudio.Play("banner_whoosh_floor"); else if (title.StartsWith("BOSS")) HellordAudio.Play("banner_whoosh_boss"); // v0.0.34
            if (_banners.Count > 0 && _banners[_banners.Count - 1].title == title && _banners[_banners.Count - 1].t < 0.3f) return; // same banner twice in a frame
            while (_banners.Count >= 3) _banners.RemoveAt(1);
            _banners.Add(new BannerItem(title, sub, kind, hold));
        }
        void UpdateBanners(float dt)
        {
            if (_banners.Count == 0) return;
            if (mode != Mode.Play && mode != Mode.Transition) return;
            var b = _banners[0]; b.t += dt; var T = HellordTuning.I;
            // a long queue shortens the current banner so notices never pile up
            float total = T.bannerFade + b.hold + T.bannerFade; if (_banners.Count > 1 && b.t > b.hold * 0.6f + T.bannerFade) b.t = Mathf.Max(b.t, total - T.bannerFade);
            if (b.t >= total) _banners.RemoveAt(0);
        }
        void ClearBanners() { _banners.Clear(); BannerAlpha = 0; }
        void DrawBannerB()
        {
            if (_banners.Count == 0 || (mode != Mode.Play && mode != Mode.Transition && mode != Mode.Won)) { BannerAlpha = 0; return; }
            var T = HellordTuning.I; var b = _banners[0]; float fade = Mathf.Max(0.01f, T.bannerFade), total = fade + b.hold + fade;
            float a = Mathf.Clamp01(Mathf.Min(b.t / fade, (total - b.t) / fade)); BannerAlpha = a; if (a <= 0.001f) return;
            int sc = 3; while (sc > 1 && b.title.Length * 6 * sc > 600) sc--;
            int ssc = 0; if (b.sub != null) { ssc = 2; while (ssc > 1 && b.sub.Length * 6 * ssc > 600) ssc--; }
            float tH = 7 * sc, sH = ssc > 0 ? 7 * ssc : 0, pad = 9, gap = ssc > 0 ? 7 : 0, h = Mathf.Round((pad * 2 + tH + gap + sH) * Mathf.Lerp(0.55f, 1f, a)), full = pad * 2 + tH + gap + sH;
            float y = Mathf.Round(112 - full / 2 + (full - h) / 2); // centre of the band stays at canvas y 112 (clear of the boss bar, hero and counters)
            Color accent = b.Kind == BannerKind.Danger ? new Color32(0xe4, 0x3b, 0x44, 255) : b.Kind == BannerKind.Floor ? new Color32(0xfe, 0xe7, 0x61, 255) : b.Kind == BannerKind.Info ? new Color32(0xc0, 0xcb, 0xdc, 255) : HCyan;
            var m = GUI.matrix; var col = GUI.color; float prevA = _guiA; _guiA = a;
            GUI.color = new Color(HInk.r, HInk.g, HInk.b, T.bannerBandAlpha * a); GUI.DrawTexture(new Rect(-1200, y, 3040, h), Texture2D.whiteTexture); // dark band, full width (also over the letterbox)
            GUI.color = new Color(accent.r, accent.g, accent.b, a); GUI.DrawTexture(new Rect(-1200, y, 3040, 2), Texture2D.whiteTexture); GUI.DrawTexture(new Rect(-1200, y + h - 2, 3040, 2), Texture2D.whiteTexture);
            BannerRect = new Rect(0, y, 640, h);
            if (h >= full - 1)
            {
                float tw = b.title.Length * 6 * sc - sc, tx = Mathf.Round(320 - tw / 2), ty = y + pad;
                Color tc = b.Kind == BannerKind.Danger ? new Color32(0xff, 0x6a, 0x5a, 255) : Color.white;
                for (int dx = -1; dx <= 1; dx++) for (int dy = -1; dy <= 1; dy++) if (dx != 0 || dy != 0) Text(b.title, tx + dx * sc * 0.66f, ty + dy * sc * 0.66f, HInk, false, sc); // outline
                Text(b.title, tx, ty + sc, new Color(accent.r, accent.g, accent.b, 1), false, sc); // chunky accent drop
                Text(b.title, tx, ty, tc, false, sc);
                if (b.sub != null)
                {
                    float sw = b.sub.Length * 6 * ssc - ssc, sx = Mathf.Round(320 - sw / 2), sy = ty + tH + gap;
                    Text(b.sub, sx, sy, new Color32(0xe8, 0xe0, 0xd0, 255), true, ssc);
                }
            }
            _guiA = prevA; GUI.color = col; GUI.matrix = m;
        }

        // ================= minor notice: small, near the hero =================
        void DrawToastNearHero()
        {
            string t = Sanitize(toast); float w = TextW(t);
            Vector2 c = _cam != null ? WorldToHud(_heroPos + Vector2.up * 2.6f) : new Vector2(320, 120);
            float rise = Mathf.Clamp01(1 - toastT / 2.5f) * 10f; // drifts up while it fades
            float x = Mathf.Round(Mathf.Clamp(c.x - w / 2, 6, HW - w - 6)), y = Mathf.Round(Mathf.Clamp(c.y - rise, 58, 300));
            float a = Mathf.Clamp01(toastT / 0.5f);
            MsgRect(new Rect(x - 3, y - 2, w + 6, 12), "toast");
            GUI.color = new Color(HInk.r, HInk.g, HInk.b, 0.7f * a); GUI.DrawTexture(new Rect(x - 3, y - 2, w + 6, 12), Texture2D.whiteTexture); GUI.color = Color.white;
            float pa = _guiA; _guiA = a; Text(t, x, y, Color.white); _guiA = pa;
        }

        // ================= skill bar =================
        // v0.0.26 (art pass): the approved step14 skill icons (24x24, ui_v26_skill_icons_24) with cooldown option A (dark clockwise sweep + tiny 3x5 corner number),
        // and the approved step13 support icons (20x20, ui_v26_supports_20, style B) as the support pips. Both are approved MOCK-UP art copied from Art/previews;
        // the padlock, the FREE SLOT frame, the sweep mask and the digit font are code-drawn PH. Sizes come from HellordTuning (skillCdDigitScale, skillBarSupportPx).
        const float SbGap = 4, SbY0 = 356; // bar bottom edge (canvas y)
        static readonly int[] SupIcon = { 0, 1, 2, 3, 4, 6, 7, 9, 11, 12, 13, 14, 19, 20, 41, 21, 22, 35, 28 }; // v0.0.36: + Executioner(Ruthless), Life Leech, Knockback, Soul Harvest(Thrill of the Kill), Explode on Kill(Detonate) stand-ins // SupportId order -> sheet index (supports_all.json)
        float SbSup => Mathf.Clamp(Mathf.Round(HellordTuning.I.skillBarSupportPx), 8, 20);
        float SbW => 4 + 24 + 3 + 2 * SbSup + 1 + 4;
        float SbH => Mathf.Max(4 + 24 + 2 + 7 + 3, 8 + 2 * SbSup + 1);
        float SbY => HellordTuning.I.hudCornerV45 ? 360 - HellordTuning.I.hudSkillBarBottomPx - SbH : SbY0 - SbH;
        float SbX0 => HellordTuning.I.hudCornerV45 ? HellordTuning.I.hudSkillBarLeftPx : 320 - (3 * SbW + 2 * SbGap) / 2f; /* v0.0.45: bottom-left corner */
        public Rect SkillBarRect => new Rect(SbX0 - 2, SbY - 2, 3 * SbW + 2 * SbGap + 4, SbH + 4);
        float _sbA = 1f; public bool skillBarFaded => _sbA < 0.99f; public int skillBarSlotsShown;
        float _sbFlash; int _sbLastLevel; readonly float[] _sbReadyFlash = new float[3], _sbPrevCd = new float[3];
        /// <summary>v0.0.44: approved step26 icons loaded (10-skill 24px sheet, supports sheet with Executioner / Soul Harvest / Explode on Kill in cells 53-55).</summary>
        bool _icons44;
        int SupIx(SupportId id)
        {
            if (_icons44) switch (id) { case SupportId.Executioner: return 53; case SupportId.SoulHarvest: return 54; case SupportId.ExplodeOnKill: return 55; }
            return SupIcon[(int)id];
        }
        Texture2D _tSkill24, _tSup20, _tCdDigits; Texture2D[] _tCdMask;
        // 3x5 digit font (rows top->bottom, 3 bits per row, MSB = left). '8' keeps its pinched waist per the art note.
        static readonly int[][] Dg = {
            new[]{7,5,5,5,7}, new[]{2,6,2,2,7}, new[]{7,1,7,4,7}, new[]{7,1,7,1,7}, new[]{5,5,7,1,1},
            new[]{7,4,7,1,7}, new[]{7,4,7,5,7}, new[]{7,1,1,2,2}, new[]{7,5,2,5,7}, new[]{7,5,7,1,7} };
        void SkillBarTex()
        {
            if (_tSkill24 == null)
            {
                bool v44 = HellordTuning.I.newIconsV44;
                _tSkill24 = v44 ? UiTex("v51k_skill_icons_24") : null; if (v44 && _tSkill24 == null) _tSkill24 = UiTex("v44_skill_icons_24"); /* t899u v0.0.70: VH-approved step51k sheet (Blood Whip = R2 tentacle, dark red frame); v44 = fallback */ _tSup20 = v44 ? UiTex("v44_supports_20") : null; _icons44 = _tSkill24 != null && _tSup20 != null;
                if (!_icons44) { _tSkill24 = UiTex("v26_skill_icons_24"); _tSup20 = UiTex("v26_supports_20"); }
            }
            if (_tCdDigits == null)
            {   // 10 glyphs, cell 5x7 (3x5 + 1px black outline), warm white (255,248,190)
                var t = new Texture2D(50, 7, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
                var px = new Color32[50 * 7]; var fg = new Color32(255, 248, 190, 255); var ol = new Color32(0, 0, 0, 255);
                for (int d = 0; d < 10; d++)
                    for (int gy = 0; gy < 5; gy++) for (int gx = 0; gx < 3; gx++)
                        if ((Dg[d][gy] >> (2 - gx) & 1) != 0)
                        {
                            for (int oy = -1; oy <= 1; oy++) for (int ox = -1; ox <= 1; ox++) { int X = d * 5 + 1 + gx + ox, Y = 1 + gy + oy; if (px[(6 - Y) * 50 + X].a == 0) px[(6 - Y) * 50 + X] = ol; }
                        }
                for (int d = 0; d < 10; d++)
                    for (int gy = 0; gy < 5; gy++) for (int gx = 0; gx < 3; gx++)
                        if ((Dg[d][gy] >> (2 - gx) & 1) != 0) px[(6 - (1 + gy)) * 50 + d * 5 + 1 + gx] = fg;
                t.SetPixels32(px); t.Apply(false, true); _tCdDigits = t;
            }
            int nf = Mathf.Clamp(HellordTuning.I.skillCdFrames, 4, 48);
            if (_tCdMask == null || _tCdMask.Length != nf + 1)
            {   // sweep masks: frame i = i/nf of the cooldown elapsed; dark = the part still to go, clockwise from 12 o'clock (24,20,37, alpha 150)
                _tCdMask = new Texture2D[nf + 1];
                for (int f = 0; f <= nf; f++)
                {
                    var t = new Texture2D(24, 24, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp }; var px = new Color32[24 * 24]; float el = f / (float)nf;
                    for (int y = 0; y < 24; y++) for (int x = 0; x < 24; x++)
                    {
                        float dx = x + 0.5f - 12, dy = 12 - (y + 0.5f); float ang = Mathf.Atan2(dx, dy); if (ang < 0) ang += Mathf.PI * 2; float fr = ang / (Mathf.PI * 2);
                        if (fr >= el) px[(23 - y) * 24 + x] = new Color32(24, 20, 37, 150);
                    }
                    t.SetPixels32(px); t.Apply(false, true); _tCdMask[f] = t;
                }
            }
        }
        void CdNumber(int n, float x, float y, bool centre = false)
        {   // x,y = tile origin; digits drawn bottom-right (or centred in the 24px icon: t818u belt), scale = skillCdDigitScale (tunable)
            string s = n.ToString(); float sc = Mathf.Clamp(Mathf.Round(HellordTuning.I.skillCdDigitScale), 1, 4);
            float right = x + 20f, top = y + 20f - 5 * sc - 0; float left = right - s.Length * 4 * sc + sc;
            if (centre) { float tw = s.Length * 4 * sc + sc; left = x + Mathf.Round((24f - tw) / 2f) + sc; top = y + Mathf.Round((24f - 7 * sc) / 2f) + sc; }
            for (int i = 0; i < s.Length; i++) { int d = s[i] - '0'; Blit(_tCdDigits, d * 5, 0, 5, 7, Mathf.Round(left + i * 4 * sc - sc), Mathf.Round(top - sc), 5 * sc, 7 * sc); }
        }
        void DrawSkillBarB()
        {
            if (!HellordTuning.I.skillBarEnabled) return;
            FloorTex(); SkillBarTex(); float dt = Event.current.type == EventType.Repaint ? Time.unscaledDeltaTime : 0;
            // fade to ~35% while the hero stands behind it (small rooms put the south wall under the bar)
            bool under = false; if (_cam != null && mode == Mode.Play) { var hp2 = WorldToHud(_heroPos + Vector2.up * 1.0f); var rr = SkillBarRect; rr.yMin -= 14; under = rr.Contains(hp2) || rr.Contains(WorldToHud(_heroPos)); }
            _sbA = Mathf.MoveTowards(_sbA, under ? 0.35f : 1f, dt * 4f);
            if (level != _sbLastLevel) { if (_sbLastLevel > 0 && HData.SlotsUnlocked(level) > HData.SlotsUnlocked(_sbLastLevel)) _sbFlash = 1.2f; _sbLastLevel = level; }
            _sbFlash = Mathf.Max(0, _sbFlash - dt);
            int unlocked = HData.SlotsUnlocked(level); skillBarSlotsShown = 3;
            float SbW_ = SbW, SbH_ = SbH, SbY_ = SbY, SbX0_ = SbX0, sup = SbSup;
            var g = GUI.color;
            for (int i = 0; i < 3; i++)
            {
                float x = Mathf.Round(SbX0_ + i * (SbW_ + SbGap)), y = SbY_;
                bool has = i < skills.Count, locked = i >= unlocked;
                GUI.color = new Color(0.55f, 0.4f, 0.32f, _sbA); GUI.DrawTexture(new Rect(x - 1, y - 1, SbW_ + 2, SbH_ + 2), Texture2D.whiteTexture);
                GUI.color = new Color(HInk.r, HInk.g, HInk.b, 0.9f * _sbA); GUI.DrawTexture(new Rect(x, y, SbW_, SbH_), Texture2D.whiteTexture);
                float pa = _guiA; _guiA = _sbA;
                if (has)
                {
                    var k = skills[i]; int ic = _icons44 ? (int)k.def.id : BarTile(k.def.id); float ix = x + 4, iy = y + 4; /* SkillId order == approved 24px sheet order */
                    float full = Mathf.Max(0.05f, k.def.cooldown * k.CdMul); bool cont = k.def.id == SkillId.BoneOrbit || k.def.id == SkillId.StormRing;
                    float cdf = cont ? 0 : Mathf.Clamp01(k.cd / full); bool cooling = cdf > 0.02f && k.cd > 0.15f;
                    if (Event.current.type == EventType.Repaint)
                    {
                        if (_sbPrevCd[i] > 0.15f && !cooling) _sbReadyFlash[i] = HellordTuning.I.skillReadyFlash; // ready flash (white ring + tint)
                        _sbPrevCd[i] = cooling ? k.cd : 0; _sbReadyFlash[i] = Mathf.Max(0, _sbReadyFlash[i] - dt);
                    }
                    float fl = HellordTuning.I.skillReadyFlash > 0 ? Mathf.Clamp01(_sbReadyFlash[i] / HellordTuning.I.skillReadyFlash) : 0;
                    GUI.color = new Color(1, 1, 1, _sbA);
                    if (_tSkill24 != null) { var pt = _icons44 ? Color.white : PhTint(k.def.id); GUI.color = new Color(pt.r, pt.g, pt.b, _sbA); Blit(_tSkill24, ic * 28, 0, 24, 24, ix, iy); GUI.color = new Color(1, 1, 1, _sbA); var tg = _icons44 ? null : PhTag(k.def.id); if (tg != null) Text(tg, ix + 1, iy + 16, Color.white); } // v0.0.36: PLACEHOLDER icon = tinted base tile + 2-letter tag
                    if (fl > 0) { GUI.color = new Color(1, 1, 1, 0.6f * fl * _sbA); GUI.DrawTexture(new Rect(ix, iy, 24, 24), Texture2D.whiteTexture); GUI.color = new Color(1, 1, 1, fl * _sbA); GUI.DrawTexture(new Rect(ix - 2, iy - 2, 28, 1), Texture2D.whiteTexture); GUI.DrawTexture(new Rect(ix - 2, iy + 25, 28, 1), Texture2D.whiteTexture); GUI.DrawTexture(new Rect(ix - 2, iy - 2, 1, 28), Texture2D.whiteTexture); GUI.DrawTexture(new Rect(ix + 25, iy - 2, 1, 28), Texture2D.whiteTexture); }
                    if (cooling)
                    {
                        int nf = _tCdMask.Length - 1; int fr = Mathf.Clamp(Mathf.FloorToInt((1 - cdf) * nf), 0, nf);
                        GUI.color = new Color(1, 1, 1, _sbA); GUI.DrawTexture(new Rect(ix, iy, 24, 24), _tCdMask[fr]);
                        if (full >= 1.5f) CdNumber(Mathf.CeilToInt(k.cd), ix, iy);
                    }
                    GUI.color = Color.white; Text("L" + k.level, x + 4, y + 4 + 24 + 2, k.level >= 5 ? (Color)new Color32(0xfe, 0xe7, 0x61, 255) : Color.white);
                    for (int p = 0; p < 4; p++)
                    {   // support pips: filled = the support's approved icon, dim frame = open slot, dark = locked until the skill levels up
                        float sx = x + 4 + 24 + 3 + (p % 2) * (sup + 1), sy = y + 4 + (p / 2) * (sup + 1);
                        bool open = p < Mathf.Min(k.level, 4), fill = p < k.sup.Count;
                        if (fill && _tSup20 != null)
                        {
                            int si = SupIx(k.sup[p]); GUI.color = new Color(1, 1, 1, _sbA); Blit(_tSup20, (si % 10) * 20, (si / 10) * 20, 20, 20, sx, sy, sup, sup);
                            { int sl = k.Lvl(k.sup[p]); float ga = _guiA; _guiA = _sbA; Text(sl.ToString(), sx + sup - 5, sy + sup - 7, sl >= 5 ? (Color)new Color32(0xfe, 0xe7, 0x61, 255) : Color.white); _guiA = ga; } // v0.0.27: support level digit (code-drawn)
                        }
                        else
                        {
                            GUI.color = open ? new Color(0.45f, 0.36f, 0.34f, _sbA) : new Color(0.16f, 0.12f, 0.14f, _sbA);
                            GUI.DrawTexture(new Rect(sx, sy, sup, 1), Texture2D.whiteTexture); GUI.DrawTexture(new Rect(sx, sy + sup - 1, sup, 1), Texture2D.whiteTexture);
                            GUI.DrawTexture(new Rect(sx, sy, 1, sup), Texture2D.whiteTexture); GUI.DrawTexture(new Rect(sx + sup - 1, sy, 1, sup), Texture2D.whiteTexture);
                            if (!open) GUI.DrawTexture(new Rect(sx + 1, sy + 1, sup - 2, sup - 2), Texture2D.whiteTexture);
                        }
                    }
                }
                else if (locked)
                {   // PH: code-drawn padlock + unlock level
                    float cx = x + SbW_ / 2f; GUI.color = new Color(0.5f, 0.52f, 0.62f, _sbA);
                    GUI.DrawTexture(new Rect(cx - 6, y + 6, 12, 3), Texture2D.whiteTexture); GUI.DrawTexture(new Rect(cx - 6, y + 6, 3, 9), Texture2D.whiteTexture); GUI.DrawTexture(new Rect(cx + 3, y + 6, 3, 9), Texture2D.whiteTexture);
                    GUI.DrawTexture(new Rect(cx - 9, y + 14, 18, 12), Texture2D.whiteTexture);
                    GUI.color = new Color(HInk.r, HInk.g, HInk.b, _sbA); GUI.DrawTexture(new Rect(cx - 1, y + 17, 2, 5), Texture2D.whiteTexture);
                    GUI.color = Color.white; string lv = "LV " + (i == 1 ? HellordTuning.I.slot2Level : HellordTuning.I.slot3Level); Text(lv, cx - TextW(lv) / 2f, y + 32, new Color32(0x8b, 0x9b, 0xb4, 255));
                }
                else
                {   // unlocked but empty: next level-up offers a skill
                    float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4f); float cx = x + SbW_ / 2f;
                    GUI.color = new Color(1f, 0.85f, 0.4f, (0.35f + 0.4f * pulse) * _sbA); float fx = x + 4, fy = y + 4;
                    GUI.DrawTexture(new Rect(fx, fy, SbW_ - 8, 1), Texture2D.whiteTexture); GUI.DrawTexture(new Rect(fx, fy + 27, SbW_ - 8, 1), Texture2D.whiteTexture); GUI.DrawTexture(new Rect(fx, fy, 1, 28), Texture2D.whiteTexture); GUI.DrawTexture(new Rect(fx + SbW_ - 9, fy, 1, 28), Texture2D.whiteTexture);
                    GUI.color = Color.white; Text("FREE", cx - TextW("FREE") / 2f, y + 10, new Color32(0xfe, 0xe7, 0x61, 255)); Text("SLOT", cx - TextW("SLOT") / 2f, y + 32, new Color32(0x8b, 0x9b, 0xb4, 255));
                }
                if (_sbFlash > 0 && i == unlocked - 1) { GUI.color = new Color(1, 0.95f, 0.5f, Mathf.Clamp01(_sbFlash) * 0.5f); GUI.DrawTexture(new Rect(x, y, SbW_, SbH_), Texture2D.whiteTexture); }
                _guiA = pa;
            }
            GUI.color = g;
        }
    }
}
