#if HELLORD_AUTOTEST
using System.Collections;
using UnityEngine;
namespace Hellord
{
    /// <summary>t1544 pact-select clip ("pact1544"): menu-theme Sacrifice Altar overview, all Paths unlocked, YOUR PATH = Stormcaller (Leizu);
    /// stills of every carousel position (pact1544_view{v}.png), then a recorded pass that holds each view 2.16 s (12 avatar frames) and clicks the right arrow (MenuThemeFake, the real AmBtn path) to scroll
    /// through all Paths and back to the start; finally one still with Blood Reaver (Xingtian) locked (pact1544_locked.png), the sealed Leizu card (pact1544_sealed.png) and the stone-theme fallback carousel (pact1544_stone.png).</summary>
    public partial class HellordGame
    {
        int _r1544;
        IEnumerator AtPact1544()
        {
            yield return AtBegin("pact1544"); HellordUITheme.Requested = HellordUITheme.Theme.MenuTheme; PactLoad51();
            for (int p = 0; p < NP97; p++) _unlock51[p] = (1 << PactData.Nodes[p].Length) - 1;
            _souls51 = 1700; _sigils51 = 15; _pactPath51 = 2; _pactSel51 = 3; _pactSealed51 = false;
            OpenAltar53(); _altarView51 = 0; _altarScr53 = 0; yield return AtWait(1.2f);
            for (int v = 0; v < NP97; v++) { _altarView51 = v; _slide97 = 0; yield return AtWait(0.4f); yield return AtShot("pact1544_view" + v); }
            _altarView51 = 0; _slide97 = 0; yield return AtWait(0.3f); _r1544 = 0; float hold = 2.16f; int clicks = 0;
            AtAudioLog1481.Frame = 0; AtAudioLog1481.Sb.Length = 0; HellordAudio.LoopLog = (kind, hid, a, b) => AtAudioLog1481.Log(kind == "play" ? hid : kind + "|" + hid, a, b);
            for (int step = 0; step <= NP97; step++)
            {
                float t = 0; while (t < hold) { yield return null; AtFrameChecks(); t += Time.deltaTime; AtAudioLog1481.Frame = _r1544; AtRecFrame("pact1544", ref _r1544); }
                if (step < NP97) { int was = _altarView51; _mtFakeUsed = false; MenuThemeFake = new Vector2(AltarMenuData.ArrR + 8, AmArrY() + 11); float w = 0; while (MenuThemeFake.HasValue && !_mtFakeUsed && w < 1f) { yield return null; w += Time.unscaledDeltaTime; AtAudioLog1481.Frame = _r1544; AtRecFrame("pact1544", ref _r1544); } MenuThemeFake = null; if (_altarView51 != was) clicks++; else Scroll97(1); }
            }
            HellordAudio.LoopLog = null; AtAudioLog1481.Frame = -1; if (_atRecFps > 0) System.IO.File.WriteAllText(System.IO.Path.Combine(_atOut, "rec_pact1544", "audio_events.txt"), AtAudioLog1481.Sb.ToString());
            _unlock51[3] = 0; _altarView51 = 3; _slide97 = 0; yield return AtWait(0.4f); yield return AtShot("pact1544_locked");
            _altarView51 = 2; _slide97 = 0; yield return AtWait(0.4f); yield return AtShot("pact1544_locked_side");
            _unlock51[3] = (1 << PactData.Nodes[3].Length) - 1; _pactSealed51 = true; _altarView51 = 2; _slide97 = 0; yield return AtWait(0.4f); yield return AtShot("pact1544_sealed");
            bool amOk = AmActive() && !_amBroken; _pactSealed51 = false;
            { var T = HellordTuning.I; bool was = T.pactCardsV4; T.pactCardsV4 = !was; _altarView51 = 2; _slide97 = 0; yield return AtWait(0.4f); yield return AtShot("pact1544_toggle_v4_" + (!was ? "on" : "off")); T.pactCardsV4 = was; }
            { var T = HellordTuning.I; var was = T.pactNameFrame5; bool sw = _pactSealed51; _pactSealed51 = false; // t1544 v5: one still per name frame option, centre = the longest title, then the longest name
              foreach (var o in new[] { HellordTuning.PactNameFrame.Plate, HellordTuning.PactNameFrame.Banner, HellordTuning.PactNameFrame.Tag, HellordTuning.PactNameFrame.Lacquer })
              { T.pactNameFrame5 = o; _altarView51 = 3; _slide97 = 0; yield return AtWait(0.4f); yield return AtShot("pact1544_frame" + (int)o + "_xingtian"); _altarView51 = 2; _slide97 = 0; yield return AtWait(0.3f); yield return AtShot("pact1544_frame" + (int)o + "_leizu"); _altarView51 = 1; _slide97 = 0; yield return AtWait(0.3f); yield return AtShot("pact1544_frame" + (int)o + "_zhongkui"); }
              T.pactNameFrame5 = was; _pactSealed51 = sw; }
            { var T = HellordTuning.I; bool mw = T.pactV7MatchAvatar; bool sw = _pactSealed51; _pactSealed51 = false; T.pactV7MatchAvatar = false; _altarView51 = 2; _slide97 = 0; yield return AtWait(0.4f); yield return AtShot("pact1544_v7_off_leizu"); T.pactV7MatchAvatar = mw; _pactSealed51 = sw; }
            { var T = HellordTuning.I; bool ev = T.pactV8Even; bool sw = _pactSealed51; _pactSealed51 = false; T.pactV8Even = false; _altarView51 = 2; _slide97 = 0; yield return AtWait(0.4f); yield return AtShot("pact1544_v8_off_leizu"); T.pactV8Even = ev; _pactSealed51 = sw; } // t1544 v8: before/after
            HellordUITheme.Requested = HellordUITheme.Theme.Stone1246; _altarView51 = 3; _slide97 = 0; yield return AtWait(0.6f); yield return AtShot("pact1544_stone");
            HellordUITheme.Requested = HellordUITheme.Theme.MenuTheme; yield return AtWait(0.3f);
            ARes("pact1544", amOk, $"menuTheme altar active {AmActive()} broken {_amBroken} paths {NP97} frames {_r1544} arrowClicks {clicks}/{NP97} names {string.Join("|", PactData.PathShort)}");
            PactLoad51(); AtEnd("pact1544");
        }
    }
}
#endif
