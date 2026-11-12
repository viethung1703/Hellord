using UnityEngine;

// t1277 (VH: "keep that UI, do not delete it, just deactivate it or something, I want to try another UI theme ... that fit the main menu theme and feel"):
// one switch for the UI theme of the Sins screen (the only screen that uses the t1246 stone kit). Nothing is deleted.
//   Stone1246 = the v0.1.0 look (HellordSin1246.cs + ui_t1246_* + HellordSinFX1246), kept fully intact incl. every fallback flag (-sins1246off, -embers1272off, ...). DEFAULT.
//   MenuTheme = the new look designed to fit the main menu (HellordMenuTheme.cs: D2 Morta frame, lava background, main-menu fire). t1300: MenuThemeReady = true, drawn only when requested with -uitheme menu.
//               (If the art cannot load, DrawSinPanelMenuTheme() returns false and the Stone1246 screen is drawn.)
// Command line: -uitheme stone | menu   (also -uitheme=stone|menu). Unknown value = stone. The default stays Stone1246 until VH approves the new theme as default.
namespace Hellord
{
    public static class HellordUITheme
    {
        public enum Theme { Stone1246 = 0, MenuTheme = 1 }
        /// <summary>flip to true when the MenuTheme art + DrawSinPanelMenuTheme() exist; until then MenuTheme falls back to Stone1246.</summary>
        public const bool MenuThemeReady = true;   // t1300: the menu-theme Sins screen exists (HellordMenuTheme.cs); selected only with -uitheme menu, Stone1246 stays the default
        public static Theme Requested = Parse(System.Environment.GetCommandLineArgs());
        public static Theme Active => Resolve(Requested, MenuThemeReady);
        static bool _logged;

        public static Theme Parse(string[] args)
        {
            if (args == null) return Theme.Stone1246;
            for (int i = 0; i < args.Length; i++)
            {
                string v = null;
                if (args[i] == "-uitheme" && i + 1 < args.Length) v = args[i + 1];
                else if (args[i].StartsWith("-uitheme=")) v = args[i].Substring(9);
                if (v != null) return FromName(v);
            }
            return Theme.Stone1246;
        }
        public static Theme FromName(string v) { v = (v ?? "").Trim().ToLowerInvariant(); return v == "menu" || v == "menutheme" ? Theme.MenuTheme : Theme.Stone1246; }
        public static Theme Resolve(Theme requested, bool menuReady)
        {
            if (requested == Theme.MenuTheme && !menuReady)
            {
                if (!_logged) { _logged = true; Debug.Log("[uitheme] MenuTheme requested but its art is not ready yet: using Stone1246"); }
                return Theme.Stone1246;
            }
            return requested;
        }
    }
}
