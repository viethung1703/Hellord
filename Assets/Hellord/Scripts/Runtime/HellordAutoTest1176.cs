#if HELLORD_AUTOTEST
// t1178: autotest for the restored simple line UI (Sins t770u panel + v0.0.53 Pact altar) with the kept functions.
// Run alone: -atOnly lines1176 (also runs at the end of -atOnly menus92). The stone suites run under AtStone76 (LineUI1176 = false).
using System.Collections;
using UnityEngine;

namespace Hellord
{
    public partial class HellordGame
    {
        IEnumerator AtStone76(IEnumerator inner) { bool w = LineUI1176, w46 = T1246On; LineUI1176 = false; T1246On = false; yield return inner; LineUI1176 = w; T1246On = w46; } // t1259: stone suites cover the older layouts, AtSins1246 the new default

        IEnumerator AtLines1176()
        {
            yield return AtBegin("lines1176"); bool lineWas = LineUI1176, t46L = T1246On; LineUI1176 = true; T1246On = false;
            SinLoad737(); PactLoad51();
            var own0 = (bool[])_sinOwned737.Clone(); var roll0 = (float[][])_sinRoll737.Clone(); var eq0 = (int[])_sinEq737.Clone(); int sl0 = _sinSlots737, so0 = _souls51, si0 = _sigils51;
            int pp0 = _pactPath51, ps0 = _pactSel51; int[] pu0 = (int[])_unlock51.Clone();
            AtClearWorld(); mode = Mode.MainMenu; _panel69 = 0;
            for (int i = 0; i < HSins.Count; i++) { bool o = i % 9 != 4; _sinOwned737[i] = o; _sinRoll737[i] = o ? SinNewRoll737(HSins.All[i]) : null; }
            int h44 = HSins.All.FindIndex(d => d.name.StartsWith("Lightning Rod")); _sinOwned737[h44] = true; _sinRoll737[h44] = SinNewRoll737(HSins.All[h44]);
            int sc2 = Arr737(ST737.sinSlotSoulCost737, 2, 100), gc2 = Arr737(ST737.sinSlotSigilCost737, 2, 1);
            _sinSlots737 = 2; _sinEq737[0] = h44; _sinEq737[1] = _sinEq737[2] = -1; _souls51 = sc2 + 500; _sigils51 = gc2 + 5; PactSave51(); SinSave737();
            SinOpen737(); yield return AtWait(0.5f);
            ARes("l76_sins_line_panel", _sinPanel737 && !Sin84Ready && !_v56, "line panel open; fire renderer never created; no kit layout");
            yield return AtShot("l76_01_sins_open");
            string[] fl = { "ALL", "MORTAL", "DEMONIC", "HEAVEN-DEFYING" }; int[] want = { 70, 25, 34, 11 };
            for (int f = 1; f < 4; f++) { yield return AtMenuClick91(24 + f * 96 + 46, 85); ARes("l76_filter_" + fl[f], _sinFilter737 == f && SinFiltered737().Count == want[f] && _sinPage737 == 0, "filter " + _sinFilter737 + " count " + SinFiltered737().Count); }
            yield return AtShot("l76_02_filter_HD");
            yield return AtMenuClick91(24 + 46, 85); ARes("l76_filter_ALL", _sinFilter737 == 0 && SinFiltered737().Count == 70, "count " + SinFiltered737().Count);
            yield return AtMenuClick91(124, 367); ARes("l76_next_page", _sinPage737 == 1, "page " + _sinPage737);
            yield return AtMenuClick91(124, 367); ARes("l76_next_disabled_last", _sinPage737 == 1, "page " + _sinPage737);
            yield return AtMenuClick91(44, 367); ARes("l76_prev_page", _sinPage737 == 0, "page " + _sinPage737);
            var L = SinFiltered737(); var c1 = L[1];
            yield return AtMenuClick91(24 + 96 + 46, 98 + 9); ARes("l76_tile_select", _sinSel737 == c1.id, "sel " + _sinSel737 + " want " + c1.id);
            yield return AtWait(0.2f); yield return AtShot("l76_03_selected");
            bool canEq = false; for (int i = 0; i < 2; i++) if (_sinEq737[i] < 0 && SinTierOk737(c1, i)) canEq = true;
            yield return AtMenuClick91(414 + 6 + 60, 98 + 232 + 8); ARes("l76_equip", !canEq || SinEquipped737(c1.id), "eq " + string.Join(",", _sinEq737));
            yield return AtShot("l76_04_equipped");
            if (SinEquipped737(c1.id)) { yield return AtMenuClick91(414 + 6 + 60, 98 + 232 + 8); ARes("l76_unequip", !SinEquipped737(c1.id), "eq " + string.Join(",", _sinEq737)); }
            // denied (cannot afford): red reason, no dialog, nothing spent
            int sv = _souls51; _souls51 = 0; PactSave51(); yield return AtWait(0.1f);
            yield return AtMenuClick91(574, 61); ARes("l76_deny_no_dialog_no_spend", !Confirm49Open && _sinSlots737 == 2 && _souls51 == 0 && _f48DenySlot == 2, $"open={Confirm49Open} denySlot={_f48DenySlot}");
            yield return AtWait(0.1f); yield return AtShot("l76_05_denied");
            _souls51 = sv; PactSave51(); yield return AtWait(SinFeelTune.DenyMsgS);
            int s0 = _souls51, g0 = _sigils51;
            yield return AtMenuClick91(574, 61); ARes("l76_confirm_opens_no_spend", Confirm49Open && _c49Slot == 2 && _sinSlots737 == 2 && _souls51 == s0 && _sigils51 == g0, $"open={Confirm49Open} slot={_c49Slot} souls {s0}->{_souls51}");
            yield return AtWait(0.4f); yield return AtShot("l76_06_confirm_dialog");
            ARes("l76_unlock_not_under_slots", L76Yes.yMin > 72 && L76Yes.xMin >= L76No.xMax, "UNLOCK " + L76Yes + " slots end y72");
            yield return AtMenuClick91(L76No.center.x, L76No.center.y); ARes("l76_cancel_no_spend", !Confirm49Open && _sinSlots737 == 2 && _souls51 == s0 && _sigils51 == g0 && PlayerPrefs.GetInt("hellord.pact51.souls", -1) == s0, $"open={Confirm49Open} souls {_souls51}");
            yield return AtWait(0.7f); yield return AtMenuClick91(574, 61); yield return AtWait(0.02f);
            yield return AtMenuClick91(L76Yes.center.x, L76Yes.center.y); ConfirmKeys49();
            ARes("l76_guard_blocks_early_unlock", Confirm49Open && _sinSlots737 == 2 && _souls51 == s0 && _sigils51 == g0, $"open={Confirm49Open} slots={_sinSlots737} souls {_souls51}");
            yield return AtWait(0.4f);
            yield return AtMenuClick91(L76Yes.center.x, L76Yes.center.y);
            ARes("l76_unlock_slot3_exact", !Confirm49Open && _sinSlots737 == 3 && _souls51 == s0 - sc2 && _sigils51 == g0 - gc2, $"slots={_sinSlots737} souls {s0}->{_souls51} sigils {g0}->{_sigils51}");
            ConfirmKeys49(); yield return AtWait(0.1f);
            ARes("l76_no_double_spend", !Confirm49Open && _sinSlots737 == 3 && _souls51 == s0 - sc2 && _sigils51 == g0 - gc2, $"souls {_souls51}");
            yield return AtShot("l76_07_unlock_flash"); yield return AtWait(1.0f); yield return AtShot("l76_08_unlocked");
            ARes("l76_currency_rolled", Shown48(false, _souls51) == _souls51, "roll finished at " + _souls51);
            yield return AtMenuClick91(650, 367); ARes("l76_close", !_sinPanel737 && mode == Mode.MainMenu && !Sin84Ready, "panel " + _sinPanel737 + " mode " + mode);
            System.Array.Copy(own0, _sinOwned737, own0.Length); System.Array.Copy(roll0, _sinRoll737, roll0.Length); System.Array.Copy(eq0, _sinEq737, 3); _sinSlots737 = sl0; _souls51 = so0; _sigils51 = si0; SinSave737(); PactSave51();

            // ---- Pact: t1179 default = v0.0.95 Altar97 carousel (rolling Path avatars) + Necromancer double tree; Your Pact = v0.0.53 screen
            yield return AtWait(0.3f);
            _souls51 = 1700; _sigils51 = 15; _pactPath51 = -1; _pactSel51 = 0; _unlock51 = new int[4]; PactSave51(); OpenAltar53(); _altarView51 = 0; yield return AtWait(0.6f);
            ARes("l79_pact_altar97_default", Altar97On && mode == Mode.Altar && _altarScr53 == 0, "Altar97 carousel");
            yield return AtShot("l79_10_pact_carousel");
            yield return AtMenuClick91(606, 185); ARes("l79_carousel_right", _altarView51 == 1, "view " + _altarView51); yield return AtWait(0.5f); yield return AtShot("l79_11_carousel_necro");
            yield return AtMenuClick91(94, 185); ARes("l79_carousel_left", _altarView51 == 0, "view " + _altarView51); yield return AtWait(0.4f);
            yield return AtMenuClick91(139, 360); ARes("l79_open_pyro", _altarScr53 == 1 && _altarView51 == 0, "scr " + _altarScr53);
            _focus97 = 0; yield return AtWait(0.1f); int sb = _souls51; yield return AtMenuClick91(154, 360); ARes("l79_unlock_cost", Unlocked51(0, 0) && _souls51 == sb - NodeCost51(PactData.Nodes[0][0]), "souls " + sb + "->" + _souls51);
            yield return AtMenuClick91(154, 360); ARes("l79_take", Taken97(0, 0), "sel " + _pactSel51); yield return AtWait(0.3f); yield return AtShot("l79_12_pact_pyro_nodes");
            yield return AtMenuClick91(583, 360); ARes("l79_nodes_back", _altarScr53 == 0, "scr " + _altarScr53);
            _unlock51[1] = (1 << 18) - 1; PactSave51(); _altarView51 = 1; yield return AtWait(0.2f);
            yield return AtMenuClick91(139, 360); ARes("l79_open_necro", _altarScr53 == 1 && _altarView51 == 1, "scr " + _altarScr53); yield return AtWait(0.5f); yield return AtShot("l79_13_pact_necro_double_tree");
            _altarScr53 = 2; yield return AtWait(0.4f); yield return AtShot("l79_14_your_pact"); ARes("l79_your_pact", _altarScr53 == 2 && mode == Mode.Altar, "scr " + _altarScr53);
            _altarScr53 = 0; mode = Mode.MainMenu; yield return AtWait(0.2f);
            _souls51 = so0; _sigils51 = si0; _pactPath51 = pp0; _pactSel51 = ps0; _unlock51 = pu0; PactSave51(); PactRunReset51();
            SinLoad737(); PactLoad51(); ARes("l76_save_restored", _sinSlots737 == sl0 && _souls51 == so0 && _sigils51 == si0 && _pactSel51 == ps0, $"slots {_sinSlots737} souls {_souls51}");
            LineUI1176 = lineWas; T1246On = t46L; AtEnd("lines1176");
        }
    }
}
#endif
