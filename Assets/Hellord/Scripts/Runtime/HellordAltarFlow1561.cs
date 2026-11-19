using UnityEngine;

// t1561 (VH: "press descend or play or anything mean go, then show sacrifice altar, then player can choose which pact to seal, if they want to edit nodes,
// then they can too just like sacrifice and start, and then there will be an option for them to press to play the game" + "if player do not seal anything
// pact they can not play"): main menu DESCEND opens the existing menu-theme Sacrifice Altar (seal / OPEN TREE unchanged). The altar's bottom row gets a
// third button, DESCEND (green, key Y): with a SEALED pact it starts the run, the old in-game 'CHOOSE YOUR STARTING SKILL' pick shows only that pact's LMB
// basics (DfStartCards1559) and the sealed pact's nodes apply (PactApply51, as before); with nothing sealed it shows "YOU MUST SEAL A PACT FIRST." and
// stays. Sealing alone never starts a run. The 5316082/8c2cd2a pact picker on DESCEND is off while HellordTuning.altarFlow1561 is on (code kept).
// A sealed pact without a basic (Leizu) falls back to the old unfiltered start pick (DfStartCards1559 returns false on an empty list).
namespace Hellord
{
    public partial class HellordGame
    {
        bool _altarRun1561; public bool AltarRun1561 => _altarRun1561; public string AltarBtnGaps1561 = "";
        bool AltarFlowWanted1561() => PT.altarFlow1561 && HellordUITheme.Active == HellordUITheme.Theme.MenuTheme && AmActive();
        bool AltarPlayOn1561() => PT.altarFlow1561;
        void AltarOpen1561() { PactLoad51(); OpenAltar53(); HellordAudio.Play("ui_confirm"); }
        /// <summary>altar DESCEND: sealed pact -> run (filtered start pick + its nodes); nothing sealed -> the notice, no run.</summary>
        void AltarPlay1561()
        {
            PactLoad51();
            if (!(_pactSealed51 && _pactPath51 >= 0)) { Msg51(PT.altarNoSealMsg1561); if (!Denied611(PT.altarNoSealMsg1561)) HellordAudio.Play("ui_denied"); return; }
            int p = _pactPath51; _altarRun1561 = true; _dfRunPick1559 = p; DescendRunPact1555 = p; _confirm97 = -1; _altarScr53 = 0;
            Descend51(); // sting + StartPick + PactApply51 (sealed -> the pact's nodes)
            if (skills.Count > 0) { skills.Clear(); SetHeroLayers(); }
            BuildStartCards();
            Debug.Log("[Hellord] t1561 altar descend: pact " + PactData.PathShort[p] + " pactNodes " + (_pactActive51 ? SelCount51() : 0) + " startCards " + cards.Count);
        }
        /// <summary>bottom row x: OPEN TREE (105) | DESCEND (altarPlayW1561) | BACK (105), gaps altarBtnGap1561, row centred in the 640 grid (an odd total would leave the extra px on the right).</summary>
        int AltarBtnX1561(int i)
        {
            int g = PT.altarBtnGap1561, w1 = PT.altarPlayW1561, tot = 105 + w1 + 105 + 2 * g, x0 = (640 - tot) / 2;
            AltarBtnGaps1561 = "left " + x0 + " | open 105 | " + g + " | descend " + w1 + " | " + g + " | back 105 | right " + (640 - x0 - tot);
            return i == 0 ? x0 : i == 1 ? x0 + 105 + g : x0 + 105 + g + w1 + g;
        }
        /// <summary>the green OPEN TREE button art at width w (left cap + key cap 1:1, label stripe repeated, right cap 1:1), its baked key letter A repainted as Y, free label.</summary>
        bool AmPlayBtn1561(string id, int x, int y, int w, string label, bool free)
        {
            bool over = free && MtOver(id, x, y, w, 19), pr = MtPressed(id, over); int lv = AmLevel(id, over, pr);
            int dw = 2 * lv, dh = 2 * (lv / 2), ox = x - dw / 2, oy = y - dh / 2 + (lv < 0 ? 1 : 0), ex = w - 105;
            string nm = "open" + (lv >= 1 ? "_h_" : "_n_") + AmD(dw) + "_" + AmD(dh); int[] r;
            if (_mtRep && _amAtlas != null && AltarMenuData.Atlas.TryGetValue(nm, out r))
            {
                int rw = r[2], rh = r[3], s0 = rw - 17;
                MtBlit(_amAtlas, r[0], r[1], 20, rh, ox, oy);
                for (int c = 20; c < rw - 6 + ex; c++) { int sc = ((c - s0) & 1) == 0 ? s0 : s0 + 1; MtBlit(_amAtlas, r[0] + sc, r[1], 1, rh, ox + c, oy); }
                MtBlit(_amAtlas, r[0] + rw - 6, r[1], 6, rh, ox + rw - 6 + ex, oy);
                for (int c = 8; c <= 12; c++) MtBlit(_amAtlas, r[0] + 6 + ((c - 6) & 1), r[1], 1, rh, ox + c, oy);   // erase the baked A (cols 8..12)
                MtTx(PT.altarPlayKey1561, ox + 8, oy + (rh - 7) / 2, "DIM");
                string ls = Sanitize(label); int tw = rw + ex; MtTx(ls, ox + 19 + (tw - 19 - 3 - MtW(ls, false)) / 2, oy + (rh - 7) / 2, "BONE", true);
            }
            if (over) MtBrackets(ox, oy, w + dw, 19 + dh);
            return MtClick(id, x, y, w, 19, free);
        }
        // ---------------------------------------------------------------- Gonggong (5th pact) placeholders
        int AmTabIx1561(int v) => v < 4 ? v : Mathf.Clamp(PT.ggTabFrom1561, 0, 3);
        SkillId GgNodeSkill1561(int i) { var a = PT.ggNodeIcon1561; return a != null && i >= 0 && i < a.Length ? a[i] : SkillId.Frostbolt; }
        /// <summary>a skill's 24 px icon centre-cropped to size (no resampling) at (x,y) in the 640x360 grid.</summary>
        void GgIcon1561(SkillId id, int x, int y, int size, float a)
        {
            int c0 = Mathf.Max(0, (24 - size) / 2); GUI.color = new Color(1, 1, 1, a);
            if (IceArt.IsIce(id)) { var it = IceArt.Icon24(id); if (it != null) AmBlitS(it, c0, c0, size, size, x, y, size, size); }
            else { SkillBarTex(); if (_tSkill24 != null) { int c = _icons44 ? (int)id : BarTile(id); AmBlitS(_tSkill24, c * 28 + c0, c0, size, size, x, y, size, size); } }
            GUI.color = Color.white;
        }
        /// <summary>node icon in the Path tree (700x392 GUI space): the node's frost skill icon, dimmed while locked.</summary>
        void GgIcon97(int i, float x, float y, bool un)
        {
            var id = GgNodeSkill1561(i); GUI.color = new Color(1, 1, 1, un ? 1f : 0.45f);
            if (IceArt.IsIce(id)) { var it = IceArt.Icon24(id); if (it != null) GUI.DrawTexture(new Rect(x, y, 24, 24), it); }
            else { SkillBarTex(); var t = _tSkill24; if (t != null) { int c = _icons44 ? (int)id : BarTile(id); GUI.DrawTextureWithTexCoords(new Rect(x, y, 24, 24), t, new Rect(c * 28f / t.width, 1f - 24f / t.height, 24f / t.width, 24f / t.height)); } }
            GUI.color = Color.white;
        }
    }
}
