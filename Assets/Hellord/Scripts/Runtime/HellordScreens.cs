using System.Collections.Generic;
using UnityEngine;

// v0.8: level-up / start / chest card screens, death and floor-cleared screens on the 640x360 canvas (exported levelup_* + pause_box art, 5x7 font).
namespace Hellord
{
    public partial class HellordGame
    {
        int _sndHover = -1; // v0.0.34 card hover sound
        static readonly Color CDim = new Color32(0x8b, 0x9b, 0xb4, 255), CBody = new Color32(0xc0, 0xcb, 0xdc, 255), CRed = new Color32(0xe4, 0x3b, 0x44, 255), CGold = new Color32(0xfe, 0xe7, 0x61, 255);
        const int CardW = 116, CardH = 164;

        static int SkillIcon(SkillId id) { switch (id) { case SkillId.Fireball: return 0; case SkillId.FrostCleave: return 11; case SkillId.StormRing: return 10; case SkillId.BoneOrbit: return 4; case SkillId.ChainLightning: return 10; case SkillId.HellfirePillar: return 0; case SkillId.BloodWhip: return 4; case SkillId.SoulSpears: return 11; default: return 8; } } // v0.0.36: new skills reuse a base icon (tinted + tagged, see PhTint/PhTag)
        /// <summary>v0.0.9: distinct existing skill-strip icons for some supports/stats (Burn no longer shares the Shock glyph,
        /// Swiftness no longer shares Hide's). -1 = no art yet (room-icon stand-in; missing art).</summary>
        static int SupportIcon(SupportId id) { switch (id) { case SupportId.Burn: return 5; case SupportId.Bleed: return 2; case SupportId.MultipleProjectiles: return 1; default: return -1; } }
        static int StatIcon(StatId id) { switch (id) { case StatId.Armor: return 7; case StatId.Speed: return 9; default: return -1; } }
        /// <summary>Blit with UVs pulled in 1/8 texel so point-sampled upscales never pick up the neighbouring frame (stray marks on cards).</summary>
        static void BlitInset(Texture2D t, float sx, float sy, float sw, float sh, float x, float y, float dw, float dh)
        {
            if (!t) return; const float e = 0.125f;
            GUI.DrawTextureWithTexCoords(new Rect(Mathf.Round(x), Mathf.Round(y), dw, dh), t, new Rect((sx + e) / t.width, 1 - (sy + sh - e) / t.height, (sw - 2 * e) / t.width, (sh - 2 * e) / t.height));
        }
        void DimScreen(Color c)
        {
            var m = GUI.matrix; GUI.matrix = Matrix4x4.identity; GUI.color = c; GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = Color.white; GUI.matrix = m;
        }
        void TextC(string s, float y, Color c, int sc = 1) { s = Sanitize(s); Text(s, Mathf.Round(320 - (s.Length * 6 * sc - sc) / 2f), y, c, true, sc); }
        static List<string> Wrap(string s, int max)
        {
            var lines = new List<string>(); string cur = "";
            foreach (var w0 in Sanitize(s).Split(' '))
            {
                if (w0.Length == 0) continue; string w = w0.Length > max ? w0.Substring(0, max) : w0;
                if (cur.Length == 0) cur = w; else if (cur.Length + 1 + w.Length <= max) cur += " " + w; else { lines.Add(cur); cur = w; }
            }
            if (cur.Length > 0) lines.Add(cur); return lines;
        }

        void DrawCardsB(string title, string sub, bool levelUp)
        {
            if (_tp != null) { DrawTargetPickB(); return; } // v0.0.36: second pick - which skill gets the support
            FloorTex();
            DimScreen(new Color(HInk.r, HInk.g, HInk.b, 0.69f * (levelUp ? FX26Dim : 1f))); // v0.0.26 LevelUpFX dim ramp
            _guiA = levelUp ? FX26Reveal(-1) : 1f; TextC(title, 40, HCyan, 2); TextC(sub, 62, Color.white); _guiA = 1f;
            int n = cards.Count; int gap = n > 3 ? 6 : 8; float x0 = Mathf.Round((640 - (n * CardW + (n - 1) * gap)) / 2f);
            var ev = Event.current; Card picked = null;
            for (int i = 0; i < n; i++)
            {
                var c = cards[i]; float x = x0 + i * (CardW + gap); var rect = new Rect(x, 80, CardW, CardH);
                float ra = levelUp ? FX26Reveal(i) : 1f; if (ra <= 0.001f) continue; _guiA = ra; GUI.color = new Color(1, 1, 1, ra); // v0.0.26 cards reveal one by one
                bool hover = rect.Contains(ev.mousePosition); if (ev.type == EventType.Repaint) { if (hover && _sndHover != i) { _sndHover = i; HellordAudio.Play("ui_card_select"); } else if (!hover && _sndHover == i) _sndHover = -1; } /* v0.0.34 */ float y = (hover ? 76 : 80) + (levelUp ? FX26Slide(i) : 0);
                int rar = c.kind == CardKind.Stat && c.stat.id == StatId.Slot ? 3 : c.kind == CardKind.NewSkill ? 2 : c.kind == CardKind.Support ? 1 : (c.kind == CardKind.SkillLevel && skills[c.skillIndex].level + 1 >= HData.MaxSkillLevel) ? 3 : 0;
                Blit(_tCard, (rar * 2 + (hover ? 1 : 0)) * CardW, 0, CardW, CardH, x, y);
                // name (1-2 lines, centred at y+7)
                string name = c.kind == CardKind.NewSkill || c.kind == CardKind.SkillLevel ? (c.kind == CardKind.SkillLevel ? skills[c.skillIndex].def.name : c.skill.name) : c.kind == CardKind.Support ? c.support.name : c.stat.name;
                { string nm = Sanitize(name); if (nm.Length > 18) nm = nm.Substring(0, 17) + "."; Text(nm, x + Mathf.Round((CardW - TextW(nm)) / 2f), y + 7, Color.white); } // banner is one line tall
                // icon 16 -> 32 at (42,27)
                int icon = c.kind == CardKind.NewSkill ? SkillIcon(c.skill.id) : c.kind == CardKind.SkillLevel ? SkillIcon(skills[c.skillIndex].def.id) : -1;
                if (icon < 0) icon = c.kind == CardKind.Support ? SupportIcon(c.support.id) : StatIcon(c.stat.id);
                bool newSup = c.kind == CardKind.Support && icon < 0; SkillBarTex(); // v0.0.36: supports without a 16px icon use the approved 20px support sheet at 2x
                SkillId? nsk = c.kind == CardKind.NewSkill ? c.skill.id : c.kind == CardKind.SkillLevel ? skills[c.skillIndex].def.id : (SkillId?)null;
                if (_icons44 && nsk.HasValue && nsk.Value >= SkillId.ChainLightning && _tSkill24 != null) { GUI.color = new Color(1, 1, 1, ra); BlitInset(_tSkill24, (int)nsk.Value * 28, 0, 24, 24, x + 34, y + 19, 48, 48); }
                else if (icon >= 0 && !newSup)
                {
                    SkillId? psk = c.kind == CardKind.NewSkill ? c.skill.id : c.kind == CardKind.SkillLevel ? skills[c.skillIndex].def.id : (SkillId?)null; var pt = psk.HasValue ? PhTint(psk.Value) : Color.white;
                    GUI.color = new Color(pt.r, pt.g, pt.b, ra); BlitInset(_tSkillIcons, icon * 16, 0, 16, 16, x + 42, y + 27, 32, 32); GUI.color = new Color(1, 1, 1, ra);
                    var ptg = psk.HasValue ? PhTag(psk.Value) : null; if (ptg != null) Text(ptg, x + 44, y + 55, Color.white);
                }
                else if (newSup && _tSup20 != null) { int si = SupIx(c.support.id); BlitInset(_tSup20, (si % 10) * 20, (si / 10) * 20, 20, 20, x + 38, y + 23, 40, 40); }
                else BlitInset(_tRoomIcons, (c.kind == CardKind.Support ? 5 : 0) * 8, 0, 8, 8, x + 42, y + 27, 32, 32); // no support/stat icon art yet: room icons stand in
                if (c.kind == CardKind.NewSkill) Blit(_tNew, 0, 0, 25, 9, x + 84, y + 24);
                // description lines at (7, 70 + 10i)
                var desc = new List<string>();
                switch (c.kind)
                {
                    case CardKind.NewSkill: { var tg = Wrap(HData.TagText(c.skill.tags, " ", true, true), 17); var dl = Wrap(c.skill.desc, 17); int room = 7 - tg.Count; if (dl.Count > room) dl.RemoveRange(room, dl.Count - room); desc.AddRange(dl); desc.AddRange(tg); } break; // t626u: tag chips (last lines)
                    case CardKind.SkillLevel: { var s = skills[c.skillIndex]; int lv = s.level + 1; desc.Add($"LV {s.level} - {lv}"); desc.AddRange(Wrap(HData.Summon561(s.def) ? SummonLvText561(s.def, lv) : lv >= HData.MaxSkillLevel ? "MAX: widest reach, most projectiles" : $"More reach and coverage. support slot {Mathf.Min(lv, 4)} opens", 17)); break; }
                    case CardKind.Stat: desc.AddRange(Wrap(HData.StatDesc(c.stat), 17)); break;
                    case CardKind.Support:
                    {
                        var tl = SupportTargets(c.support); int t = tl.Count > 0 ? tl[0] : -1; int cur = t >= 0 ? skills[t].Lvl(c.support.id) : 0; // v0.0.27: owned = upgrade (LV a - b), else NEW LV 1
                        if (tl.Count > 1) { bool anyUp = false; foreach (int q in tl) if (skills[q].Has(c.support.id)) anyUp = true; cur = 0; desc.Add(anyUp ? "UPGRADE OR NEW" : "NEW  LV 1"); } // v0.0.36: several skills can take it -> chosen after the pick
                        else desc.Add(cur > 0 ? $"UPGRADE LV {cur} - {cur + 1}" : "NEW  LV 1");
                        desc.AddRange(Wrap(HData.SupportEffect(c.support.id, cur + 1, tl.Count == 1 ? (int)skills[t].def.id : -1), 17));
                        var gen = Wrap(c.support.desc, 17); int room = 5 - desc.Count; if (gen.Count > room) gen.RemoveRange(Mathf.Max(0, room), gen.Count - Mathf.Max(0, room)); desc.AddRange(gen);
                        if (desc.Count < 6) desc.Add("NEEDS " + HData.TagText(c.support.needs) + (c.support.excludes != 0 ? " NOT " + HData.TagText(c.support.excludes) : "")); // t626u tag chip
                        desc.Add(tl.Count > 1 ? $"TO: PICK 1 OF {tl.Count}" : t >= 0 ? "TO: " + Sanitize(skills[t].def.name) : "NO OPEN SLOT"); break;
                    }
                }
                for (int k = 0; k < desc.Count && k < 7; k++) Text(desc[k], x + 7, y + 70 + 10 * k, k == desc.Count - 1 && c.kind == CardKind.Support ? HCyan : CBody);
                // level dots (5) at y+148 for skills
                if (c.kind == CardKind.NewSkill || c.kind == CardKind.SkillLevel || c.kind == CardKind.Support)
                {
                    int sti = c.kind == CardKind.Support ? (SupportTargets(c.support).Count == 1 ? SupportTarget(c.support) : -1) : -1; int lvl = c.kind == CardKind.NewSkill ? 1 : c.kind == CardKind.Support ? (sti >= 0 ? skills[sti].Lvl(c.support.id) + 1 : 1) : skills[c.skillIndex].level + 1; float dx = x + Mathf.Round((CardW - 43) / 2f);
                    for (int k = 0; k < 5; k++) Blit(_tDots, (k < lvl ? rar + 1 : 0) * 7, 0, 7, 7, dx + k * 9, y + 148);
                }
                Text((i + 1).ToString(), x + 4, y + CardH - 12, CDim);
                if (ev.type == EventType.MouseDown && ev.button == 0 && hover && !FX26CardsLocked) { picked = c; ev.Use(); }
                _guiA = 1f; GUI.color = Color.white;
            }
            string foot;
            if (levelUp)
            {
                // t1058u: show free count or bone cost; disabled state is via Space/Reroll() deny
                string rr = freeRerolls > 0 ? $"SPACE REROLL FREE x{freeRerolls}" : (CanReroll1058() ? $"SPACE REROLL {RerollBoneCost1058()} BONES" : $"SPACE REROLL {RerollBoneCost1058()} BONES (NEED MORE)");
                foot = $"{rr}   ESC SKIP +10 BONES   1-{n} PICK";
            }
            else foot = mode == Mode.Chest ? (_bossReward37 ? $"1-{n} TAKE" : $"1-{n} TAKE   ESC SKIP +10 BONES") : $"1-{n} PICK";
            TextC(foot, 262, CDim);
            if (!string.IsNullOrEmpty(warning)) TextC(warning, 276, CRed);
            if (mode == Mode.Chest) TextC(_bossReward37 ? "BOSS REWARD: THIS PICK CANNOT BE SKIPPED" : "PICK A SUPPORT. IF SEVERAL SKILLS CAN TAKE IT YOU CHOOSE WHICH", 290, CDim);
            if (picked != null)
            {
                if (picked.kind == CardKind.Support) { var tl = SupportTargets(picked.support); if (tl.Count == 1) Choose(picked, tl[0]); else if (tl.Count > 1) OpenTargetPick(picked.support, tl, false, tIx => Choose(picked, tIx)); else warning = "NO SKILL CAN TAKE IT"; }
                else Choose(picked);
            }
        }

        void DrawEndBox(string title, Color tc, string line, string line2)
        {
            DimScreen(title == "YOU DIED" ? new Color(0.25f, 0.02f, 0.04f, 0.7f) : new Color(HInk.r, HInk.g, HInk.b, 0.75f));
            TextC(title, 110, tc, 3); TextC(line, 150, Color.white); if (line2 != null) TextC(line2, 162, CDim);
            float bw = 120, bh = 40, bx = 320 - bw / 2, by = 184;
            Nine(_tPause, 0, 90, 60, 5, 5, 4, 4, bx, by, bw, bh);
            Blit(_tPauseCur, 0, 0, _tPauseCur ? _tPauseCur.width : 0, _tPauseCur ? _tPauseCur.height : 0, bx + 13, by + 16);
            Text("R  RESTART", bx + 22, by + 16, Color.white);
        }
        string RunLine() { int m = (int)(runTime / 60), s = (int)(runTime % 60); int vis = 0; foreach (var r in floor) if (r.visited) vis++; return $"{m:00}:{s:00}  ROOMS {vis}/{floor.Count}  LV {level}  KILLS {kills}  BONES {bones}"; }
        void DrawDeathB() { FloorTex(); DrawEndBox("YOU DIED", CRed, RunLine(), PactEndLine51()); } // v0.0.51 souls line
        void DrawWonB() { FloorTex(); DrawEndBox("RUN COMPLETE", HCyan, RunLine(), FloorCount + " FLOORS CLEARED. " + PactEndLine51()); } // v0.0.51
    }
}
