using UnityEngine;

// v0.0.61 t611u: menu music + menu/altar UI SFX (Hellord Sound, AUDIO_MAP MENU_AUDIO, Docs/Hellord_menu_audio_map.md).
// State-diff driven: one tick per frame compares menu state with last frame, so keyboard, pad and mouse paths all sound the same.
namespace Hellord
{
    public partial class HellordGame
    {
        Mode _ma611Mode = (Mode)(-1); int _ma611Sel = -9, _ma611Foc = -9, _ma611View = -9, _ma611Scr = -9, _ma611Path = -9, _ma611SelBits, _ma611Unl, _ma611Panel, _ma611Conf = -1;

        static int Bits611(int v) { int c = 0; while (v != 0) { c += v & 1; v >>= 1; } return c; }
        int UnlCount611() { int c = 0; for (int p = 0; p < 4; p++) c += Bits611(_unlock51[p]); return c; }

        /// <summary>v0.0.61: called every frame after PactTick51.</summary>
        void MenuAudioTick611()
        {
            HellordAudio.MenuMusic611 = mode == Mode.MainMenu ? 1 : mode == Mode.Altar ? 2 : mode == Mode.Dead ? 3 : mode == Mode.Won ? 4 : 0;
            int foc = Altar97On ? _focus97 : _focus53, unl = UnlCount611();
            bool same = mode == _ma611Mode && !_tmOpen;
            if (same && mode == Mode.MainMenu)
            {
                if (_sel69 != _ma611Sel) HellordAudio.Play("ui_hover");
                if (_panel69 == 1 && _ma611Panel != 1) HellordAudio.Play("ui_confirm");
                else if (_panel69 == 2 && _ma611Panel == 1) HellordAudio.Play("ui_back");
            }
            if (mode == Mode.Altar)
            {
                if (_ma611Mode == Mode.MainMenu) HellordAudio.Play("ui_confirm"); // altar opened (row, seal or pact panel)
                else if (same)
                {
                    if (_pactPath51 != _ma611Path) HellordAudio.Play("path_switch");
                    else if (Bits611(_pactSel51) > Bits611(_ma611SelBits)) HellordAudio.Play("pact_sign");
                    else if (Bits611(_pactSel51) < Bits611(_ma611SelBits)) HellordAudio.Play("pact_remove");
                    if (unl > _ma611Unl) HellordAudio.Play("pact_unlock");
                    if (_altarView51 != _ma611View) HellordAudio.Play("altar_carousel");
                    else if (_altarScr53 > _ma611Scr) HellordAudio.Play("ui_confirm");     // OPEN / YOUR PACT
                    else if (_altarScr53 < _ma611Scr) HellordAudio.Play("ui_back");        // BACK / EDIT
                    else if (foc != _ma611Foc && foc >= 0) HellordAudio.Play("ui_hover");
                    if (_confirm97 >= 0 && _ma611Conf < 0) HellordAudio.Play("ui_confirm"); // switch-path dialog opens
                    else if (_confirm97 < 0 && _ma611Conf >= 0 && _pactPath51 == _ma611Path) HellordAudio.Play("ui_back"); // NO
                }
            }
            if (mode == Mode.MainMenu && _ma611Mode == Mode.Altar) HellordAudio.Play("ui_back");
            _ma611Mode = mode; _ma611Sel = _sel69; _ma611Foc = foc; _ma611View = _altarView51; _ma611Scr = _altarScr53; _ma611Path = _pactPath51;
            _ma611SelBits = _pactSel51; _ma611Unl = unl; _ma611Panel = _panel69; _ma611Conf = _confirm97;
        }

        /// <summary>v0.0.61: Msg51 toasts that are refusals (COMING SOON, NOT ENOUGH, LOCKED, FULL...) get ui_denied.</summary>
        static bool Denied611(string s) => s != null && !s.StartsWith("UNLOCKED ") && !s.StartsWith("YOUR PATH");
    }
}
