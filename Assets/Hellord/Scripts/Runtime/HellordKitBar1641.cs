using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    // t1641 (VH approved plan): the bottom skill bar = the 4-slot kit: LMB basic | SPACE movement | RMB special | R ultimate.
    //  Same belt art (black outline, #3a4466 frame, #181425 fill, 26 px icon frame, 2x2 support gems + level on every unlocked slot, sweep masks).
    //  Each cell: key label on top (centred), icon below (icons_v2 for kit skills, the existing icon for the basic), cooldown sweep.
    //  Kit slots are empty + locked (padlock, dim label) until picked on level-up. Replaces the t1618 MOV / SPC / ULT debug text.
    //  Geometry (HUD px, 640x360), measured for VH: cell 57 x 48 (inner 55 x 46, odd width so every 5x7 label centres exactly), gap 2.
    //   rows: border | 3 pad | 7 label | 3 gap | 30 content | 3 pad | border (kit icon frame: 5 | 26 | 5 between label and border).   basic content: 2 pad | 26 frame | 4 | 21 gems | 2 pad.
    // =====================================================================================================================
    public partial class HellordGame
    {
        bool KitBar1641 => HellordTuning.I.kitBar1641 && HellordTuning.I.skillBelt720;
        const int KbN1641 = 4, KbLabelY1641 = 4, KbContentY1641 = 14;
        readonly float[] _kbFull1641 = new float[4], _kbPrev1641 = new float[4];

        // ---------------------------------------------------------------- t1646 (VH plan): ultimate progress on the R slot
        //  cooldown: sweep + countdown (above). After it: the icon is dimmed and an orange fill rises from the icon's bottom row as the condition builds
        //  (fill = exactly the 24 x 24 icon box inside the 26 px frame: rows iy + 24 - h .. iy + 23, h = floor(24 * progress)). Armed: full-bright icon,
        //  pulsing gold border (the basic's ready-pop colours) + one ui_confirm on the transition to armed only. Vermilion Bird: count label "n/N"
        //  (outlined 3x5 digits, even width 4 * digits + 4, so it centres exactly: 6 | 12 | 6 for "3/5"; rows = the countdown number's rows).
        //  Rebirth Flame: cooldown, then a steady glow (no pulse, no sound). Molten Heart follows the live burning count (up and down).
        bool _kbArmedPrev1646; int _kbUltPrev1646;
        void KbUltProgress1646(float ix, float iy, bool cooling)
        {
            int u = KitUlt1666; bool armed = !cooling && KitUltArmed1666; bool lz = LzOn1666 || GgOn1682 || NkOn1713; // t1666: + Leizu; t1682: + Gonggong; t1713: + Zhong Kui (no automatic ultimate)
            if (Event.current.type == EventType.Repaint)
            {
                if (u != _kbUltPrev1646) { _kbUltPrev1646 = u; _kbArmedPrev1646 = armed; } // a new pick never chimes
                else if (armed && !_kbArmedPrev1646 && (u != 2 || lz)) { HellordAudio.Play("ui_confirm"); _kbArmedPrev1646 = true; }
                else _kbArmedPrev1646 = armed;
            }
            if (cooling) return;
            if (u == 2 && !lz) { KbGlow1646(ix, iy, 0.85f); return; } // Rebirth Flame: automatic -> steady glow
            if (armed) { KbGlow1646(ix, iy, 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 7f)); return; }
            float prog = NkOn1713 ? NkUltProg1713() : GgOn1682 ? GgUltProg1682() : lz ? LzUltProg1666() : u == 1 ? ZhSunMeter1618 / Mathf.Max(1f, TZ.swMeter1618) : u == 3 ? ZhBurning1618 / (float)Mathf.Max(1, TZ.mhNeed1618) : u == 4 ? ZhNearMiss1618 / (float)Mathf.Max(1, TZ.vbNeed1618) : 0f;
            int h = Mathf.Clamp(Mathf.FloorToInt(24f * Mathf.Clamp01(prog)), 0, 23); // 24 rows only when armed (above)
            if (h < 24) Fill716(ix, iy, 24, 24 - h, new Color32(24, 20, 37, 150));             // still to build: dimmed (sweep-mask ink)
            if (h > 0) { Fill716(ix, iy + 24 - h, 24, h, C716(0xf77622, 0.5f)); Fill716(ix, iy + 24 - h, 24, 1, C716(0xfee761, 0.9f)); } // orange fill + bright top edge
            if (NkOn1713) { if (u == 1) KbCount1646(Mathf.Min(NkBoneHits1713, TZ.nkNovaNeed1713), TZ.nkNovaNeed1713, ix, iy); else if (u == 3) { int cap = NCapBase536(); KbCount1646(Mathf.Min(NkLiveMinions1713(), cap), Mathf.Max(1, cap), ix, iy); } } /* t1772 */ else if (GgOn1682) { if (u == 3) KbCount1646(Mathf.Min(GgFrozenKills1682, TZ.ewNeed1682), TZ.ewNeed1682, ix, iy); } /* t1682 */ else if (lz) { if (u == 1) KbCount1646(Mathf.Min(LzCombos1666, TZ.dcNeed1666), TZ.dcNeed1666, ix, iy); else if (u == 2) KbCount1646(Mathf.Min(LzInfusions1666, TZ.haNeed1666), TZ.haNeed1666, ix, iy); } /* t1666 */ else if (u == 4) KbCount1646(Mathf.Min(ZhNearMiss1618, TZ.vbNeed1618), TZ.vbNeed1618, ix, iy);
            GUI.color = Color.white;
        }
        void KbGlow1646(float ix, float iy, float a) { Frame716(ix - 1, iy - 1, 26, 26, C716(0xfee761, a)); Frame716(ix - 2, iy - 2, 28, 28, C716(0xfeae34, a * 0.6f)); GUI.color = Color.white; }
        /// <summary>"n/N" in the countdown digits (5x7 cells incl. outline, advance 4) + a 2 px slash; width 4 * digits + 4 (always even) -> centred exactly in the 24 px icon.</summary>
        void KbCount1646(int n, int need, float ix, float iy)
        {
            string a = n.ToString(), b = need.ToString(); int w = 4 * (a.Length + b.Length) + 4; float x = ix + (24 - w) / 2, y = iy + 8; // y = CdNumber's centred row
            GUI.color = Color.white; float p = x;
            foreach (char ch in a) { Blit(_tCdDigits, (ch - '0') * 5, 0, 5, 7, p, y); p += 4; }
            int[] sx = { 2, 2, 1, 1, 1 }; var ol = Color.black; var fg = new Color32(255, 248, 190, 255); // slash ink cols p+1..p+2, rows y+1..y+5
            for (int r = 0; r < 5; r++) Fill716(p + sx[r] - 1, y + r, 3, 3, ol);
            for (int r = 0; r < 5; r++) Fill716(p + sx[r], y + 1 + r, 1, 1, fg);
            p += 3; foreach (char ch in b) { GUI.color = Color.white; Blit(_tCdDigits, (ch - '0') * 5, 0, 5, 7, p, y); p += 4; }
            GUI.color = Color.white;
        }

        /// <summary>t1812: 2x2 support gem sockets + "L#" under them. Same origin as the LMB group
        /// (fx = left of the 26 px icon frame; gems start fx+30, gy0 = content y). Pixel-identical for every unlocked slot.</summary>
        void KbDrawGems1812(SkillInst k, float fx, float cy, float gm)
        {
            if (k == null) return;
            float gx0 = fx + 26 + 4, gy0 = cy;
            for (int p = 0; p < 4; p++)
            {
                float sx = gx0 + (p % 2) * (gm + 1), sy = gy0 + (p / 2) * (gm + 1); bool open = p < Mathf.Min(k.level, 4), fill = p < k.sup.Count;
                if (fill && _tSup20 != null) { int si = SupIx(k.sup[p]); GUI.color = Color.white; Blit(_tSup20, (si % 10) * 20, (si / 10) * 20, 20, 20, sx, sy, gm, gm); }
                else { Frame716(sx, sy, gm, gm, open ? C716(0x5a6988) : C716(0x262b44)); if (!open) Fill716(sx + 1, sy + 1, gm - 2, gm - 2, C716(0x262b44)); }
            }
            GUI.color = Color.white; Text("L" + k.level, gx0, gy0 + 2 * (gm + 1) + 1, k.level >= 5 ? C716(0xfee761) : C716(0x8b9bb4));
        }

        string KbLabel1641(int i) => i == 0 ? "LMB" : i == 1 ? (Manual716 ? Label716(Bind716(Act716.Dash)) : "SPACE") : i == 2 ? "RMB" : "R";

        void DrawKitBelt1641()
        {
            var g = GUI.color; float gm = HellordTuning.I.beltGem716; Vector2 mp = Event.current.mousePosition; int hov = -1;
            int[] pick = { 0, KitMove1666, KitSpec1666, KitUlt1666 }; /* t1666: running pact's kit */ float[] cd = { 0f, ZhMoveCd, ZhSpecCd, ZhUltCd };
            string[] key = { null, KitMoveKey1666(), KitSpecKey1666(), KitUltKey1666() };
            for (int i = 0; i < KbN1641; i++)
            {
                var r = Cell716(i); float x = r.x, y = r.y; if (r.Contains(mp)) hov = i;
                Fill716(x - 1, y - 1, r.width + 2, r.height + 2, Color.black); Fill716(x, y, r.width, r.height, C716(0x3a4466)); Fill716(x + 1, y + 1, r.width - 2, r.height - 2, C716(0x181425));
                if (hov == i && i == 0) Frame716(x, y, r.width, r.height, C716(0xead4aa));
                var k = i == 0 ? (_slot716[0] ?? (skills.Count > 0 ? skills[0] : null)) : null;
                bool filled = i == 0 ? k != null : pick[i] > 0;
                string lb = KbLabel1641(i); GUI.color = Color.white;
                Text(lb, x + Mathf.Floor((r.width - TextW(lb)) / 2f), y + KbLabelY1641, filled ? C716(0xc0cbdc) : C716(0x5a6988));
                float cy = y + KbContentY1641;
                if (i == 0)
                {   // basic: icon frame + 2x2 support gems + level, the group centred in the cell
                    float grp = 26 + 4 + (2 * gm + 1), fx = x + Mathf.Floor((r.width - grp) / 2f), ix = fx + 1, iy = cy + 3;
                    Frame716(ix - 1, iy - 1, 26, 26, C716(0x3a4466, 0.9f));
                    if (k == null) { Fill716(ix + 11, iy + 7, 2, 10, C716(0x3a4466)); Fill716(ix + 7, iy + 11, 10, 2, C716(0x3a4466)); continue; }
                    int ic = _icons44 ? (int)k.def.id : BarTile(k.def.id);
                    if (IceArt.IsIce(k.def.id) || IsFb1481(k.def.id) || IsPb1567(k.def.id)) { GUI.color = Color.white; if (!IceIconBlit(k.def.id, ix, iy, 24) && _tSkill24 != null) Blit(_tSkill24, 0, 0, 24, 24, ix, iy); }
                    else if (ZhCardIcon1625(k.def.id) != null) { GUI.color = Color.white; GUI.DrawTexture(new Rect(ix, iy, 24, 24), ZhCardIcon1625(k.def.id)); }
                    else if (_tSkill24 != null) { var pt = _icons44 ? Color.white : PhTint(k.def.id); GUI.color = pt; Blit(_tSkill24, ic * 28, 0, 24, 24, ix, iy); }
                    float full = Mathf.Max(0.05f, k.def.cooldown * k.CdMul); bool cont = k.def.id == SkillId.BoneOrbit || (HData.Summon561(k.def) && k != _basic1517);
                    float cdf = cont ? 0 : Mathf.Clamp01(k.cd / full);
                    if ((cdf > 0.02f && k.cd > 0.15f || BeltSweep1517(k)) && _tCdMask != null) { int nf = _tCdMask.Length - 1; int fr = Mathf.Clamp(Mathf.FloorToInt((1 - cdf) * nf), 0, nf); GUI.color = Color.white; GUI.DrawTexture(new Rect(ix, iy, 24, 24), _tCdMask[fr]); if (full >= 1.5f) CdNumber(Mathf.CeilToInt(k.cd), ix, iy, true); }
                    float rdy = BeltReady1517(k); if (rdy > 0f) { Frame716(ix - 1, iy - 1, 26, 26, C716(0xfee761, rdy)); Frame716(ix - 2, iy - 2, 28, 28, C716(0xfeae34, rdy * 0.6f)); }
                    KbDrawGems1812(k, fx, cy, gm);
                    continue;
                }
                {   // kit slot: locked = icon-centred padlock (unchanged). Unlocked = LMB's group (26 frame + 4 + 21 gems), same fx/ix/iy.
                    var kk = filled ? ZkSlotInst1651(i - 1) : null;
                    float grp = 26 + 4 + (2 * gm + 1);
                    float fx = kk != null ? x + Mathf.Floor((r.width - grp) / 2f) : 0f;
                    float ix = kk != null ? fx + 1 : x + Mathf.Floor((r.width - 24) / 2f), iy = cy + 3;
                    Frame716(ix - 1, iy - 1, 26, 26, C716(0x3a4466, 0.9f));
                    if (!filled)
                    {   // padlock (10 x 12, centred in the 24 px icon box: 7 | 10 | 7, 6 | 12 | 6), bar ink like the empty '+'
                        var lc = C716(0x3a4466); Fill716(ix + 9, iy + 6, 6, 1, lc); Fill716(ix + 9, iy + 7, 1, 3, lc); Fill716(ix + 14, iy + 7, 1, 3, lc);
                        Fill716(ix + 7, iy + 10, 10, 8, lc); Fill716(ix + 11, iy + 12, 2, 3, C716(0x181425));
                        _kbFull1641[i] = _kbPrev1641[i] = 0f; continue;
                    }
                    var tex = KitIcon1666(key[i]); GUI.color = Color.white;
                    if (tex != null) GUI.DrawTexture(new Rect(ix, iy, 24, 24), tex); else BlitInset(_tRoomIcons, 0, 0, 8, 8, ix + 4, iy + 4, 16, 16);
                    float c = Mathf.Max(0f, cd[i]); if (c > _kbPrev1641[i] + 0.01f) _kbFull1641[i] = c; _kbPrev1641[i] = c; float fl = Mathf.Max(0.05f, _kbFull1641[i]); // full = the cooldown just set
                    if (_tCdMask != null && c > 0.05f)
                    {   // on cooldown: the existing dark sweep + countdown (all three kit slots)
                        int nf = _tCdMask.Length - 1; int fr = Mathf.Clamp(Mathf.FloorToInt((1 - Mathf.Clamp01(c / fl)) * nf), 0, nf);
                        GUI.color = Color.white; GUI.DrawTexture(new Rect(ix, iy, 24, 24), _tCdMask[fr]); if (fl >= 1.5f) CdNumber(Mathf.CeilToInt(c), ix, iy, true);
                    }
                    if (i == 2 && NkSpecSummonHud1713(out float _, out float _, out int shave, out int _, out float sresp))
                    {   // t1812: live HP lives on the Option D2 strip. RMB keeps only the death-respawn overlay when none are up and the press is ready.
                        if (shave <= 0 && c <= 0.05f && sresp > 0.05f)
                        {
                            if (_tCdMask != null)
                            {
                                float rfull = Mathf.Max(0.05f, NkSpec1713 == 4 ? TZ.nkGolemResp1713 : TZ.nkMageResp1713);
                                int nf = _tCdMask.Length - 1; int fr = Mathf.Clamp(Mathf.FloorToInt((1 - Mathf.Clamp01(sresp / rfull)) * nf), 0, nf);
                                GUI.color = Color.white; GUI.DrawTexture(new Rect(ix, iy, 24, 24), _tCdMask[fr]);
                            }
                            CdNumber(Mathf.CeilToInt(sresp), ix, iy, true);
                        }
                    }
                    if (i == 3) KbUltProgress1646(ix, iy, c > 0.05f); // t1646: R slot condition fill / armed glow (replaces the t1641 'dark when not armed')
                    if (kk != null) KbDrawGems1812(kk, fx, cy, gm); // t1812: 2x2 gems + L# , same pixels as LMB
                }
            }
            DrawSinHud818(Cell716(0).x, Cell716(0).y - 1); // t818u sins frame above the belt, left-aligned (unchanged)
            if (hov == 0 && _slot716[0] != null && HellordTuning.I.beltTooltip716 && !_ctrlOpen716) DrawTip716(_slot716[0], Cell716(0));
            GUI.color = g;
        }

        // t1812: Play-mode kit-bar verification. Flag: -kitbarSupportsPlaytest
        System.Collections.IEnumerator KitBarSupportsPlaytestCo1812()
        {
            const string tag = "[t1812]";
            const string shot = "/workspace/tmp/kitbar_supports_playtest.png";
            void Log(string m) { Debug.Log(tag + " " + m); }
            yield return null;
#if UNITY_EDITOR
            try
            {
                var t = System.Type.GetType("Hellord.EditorTools.HellordMenu, Hellord.Editor");
                t?.GetMethod("ForceGameView1920x1080_1808", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.Invoke(null, null);
            }
            catch (System.Exception e) { Debug.LogWarning("[t1812] ForceGameView invoke failed: " + e.Message); }
            for (int _gv = 0; _gv < 8; _gv++) yield return null;
            Debug.Log("[t1812] screen after GameView force " + Screen.width + "x" + Screen.height);
#endif
            var T = HellordTuning.I;
            T.kit1618 = true; T.nkKit1713 = true; T.kitAnyPact1618 = false; T.kitSlotOverride1639 = false;
            T.kitBar1641 = true; T.skillBelt720 = true;
            T.nkMove1713 = T.nkSpecial1713 = T.nkUlt1713 = 0;
            PreloadStop1675();
            if (_unlock51 != null) for (int p = 0; p < _unlock51.Length; p++) _unlock51[p] = int.MaxValue;
            _souls51 = 1700; _sigils51 = 15; _pactPath51 = 1; _pactSel51 = 1; _pactSealed51 = true; PactSave51();
            ResetRun();
            _pactPath51 = 1; _pactSel51 = 1; _pactSealed51 = true; PactApply51();
            PreloadStop1675();
            pendingLevels = 0; mode = Mode.Play;
            skills.Clear(); skills.Add(new SkillInst { def = HData.Skill(SkillId.SkeletonWarriors), level = 1 }); SetHeroLayers();
            foreach (var e in _en) if (e != null && e.alive) { e.dmg = 0f; e.speed = 0f; }
            ChooseKit1639(new Card { kind = CardKind.NewSkill, kit = 2, kitPick = 4 });
            SnapCamera();
            Log("picked Jiangshi; ZhRunSpec1639=" + ZhRunSpec1639 + " NkSpec1713=" + NkSpec1713 + " kitBar=" + KitBar1641);
            for (int i = 0; i < 30; i++) yield return null;
            int jn = NCount536(3, false);
            NMin536 golem = null; foreach (var m in _nmin536) if (m.kind == 3 && !m.temp) { golem = m; break; }
            if (golem != null) { golem.p = _heroPos + new Vector2(1.85f, 0.15f); golem.prev = golem.p; ClampToRoom(ref golem.p, 6); }
            SnapCamera();
            for (int i = 0; i < 8; i++) yield return null;
            System.IO.Directory.CreateDirectory("/workspace/tmp");
            if (System.IO.File.Exists(shot)) System.IO.File.Delete(shot);
            var lmb = skills.Count > 0 ? skills[0] : null;
            var rmb = ZkSlotInst1651(1);
            Log("screen " + Screen.width + "x" + Screen.height + " fullscreen=" + Screen.fullScreen);
            int warHave = NCount536(0, false), warWant = NBase536(0);
            Log("D2 warriors " + warHave + "/" + warWant + " jiangshi " + jn + " NkSpec=" + NkSpec1713);
            Log("kit LMB lv=" + (lmb != null ? lmb.level : 0) + " RMB lv=" + (rmb != null ? rmb.level : 0) + " SPACE pick=" + KitMove1666 + " R pick=" + KitUlt1666);
            ScreenCapture.CaptureScreenshot(shot);
            yield return new WaitForEndOfFrame();
            for (int i = 0; i < 20 && !System.IO.File.Exists(shot); i++) yield return null;
            Log("screenshot " + (System.IO.File.Exists(shot) ? ("ok " + new System.IO.FileInfo(shot).Length + " bytes") : "MISSING"));
            bool ok = Screen.width == 1920 && Screen.height == 1080 && jn > 0 && golem != null && golem.hp > 0f && lmb != null && lmb.level >= 1 && rmb != null && rmb.level >= 1 && KitMove1666 == 0 && KitUlt1666 == 0 && System.IO.File.Exists(shot);
            Log(ok ? "PASS" : "FAIL");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(ok ? 0 : 1);
#else
            Application.Quit(ok ? 0 : 1);
#endif
        }
    }
}
