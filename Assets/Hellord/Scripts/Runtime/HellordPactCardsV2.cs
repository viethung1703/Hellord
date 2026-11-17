using UnityEngine;
namespace Hellord
{
    /// <summary>t1544 v2 (VH: "i love the avatar frames from the mockup more", "and the font"): the menu-theme Sacrifice Altar pact cards use HA's
    /// t1540 v2 mockup layout and framing (pact_select_v2_still_MOCKUP.png, work/mock2.py): card frame with a header (centre: red YOUR PATH bar,
    /// side: NOT CHOSEN), the 64px avatar in its own bevelled frame, the NAME on a plate BELOW the avatar and the title line under the plate,
    /// all text in the Morta SMALL font (BONE name; CLAY centre title, TERRA side titles; DIM header). The frame art is cut 1:1 from the mockup
    /// into HellordArt/Altar97/pact_cards_v2.png by tools/t1544v2_card_atlas.py; side cards are drawn 1:1, the 2x centre card is a 9-slice of
    /// the mockup card with its avatar frame 9-sliced to 64*scale+6. Toggle HellordTuning.pactCardsV2 (off = the 836660d layout).</summary>
    public partial class HellordGame
    {
        // atlas rects (x, y, w, h) in pact_cards_v2.png, see tools/t1544v2_card_atlas.py
        static readonly int[] V2CardC = { 0, 0, 150, 137 }, V2CardN = { 0, 139, 130, 123 }, V2CardL = { 0, 264, 130, 123 };
        static readonly int[] V3CardN = { 278, 0, 130, 123 }, V3CardL = { 278, 125, 130, 123 };   // t1544 v3: side cards without the name plate
        static readonly System.Collections.Generic.Dictionary<string, int[]> V3Sealed = new System.Collections.Generic.Dictionary<string, int[]> {
            { "seal_sd_h_2_0", new[] { 152, 233, 91, 15 } }, { "seal_sd_h_4_2", new[] { 152, 250, 93, 17 } }, { "seal_sd_h_6_2", new[] { 152, 269, 95, 17 } },
            { "seal_sd_n_0_0", new[] { 152, 288, 89, 15 } }, { "seal_sd_n_m2_0", new[] { 152, 305, 87, 15 } }, { "seal_sd_n_m4_m2", new[] { 152, 322, 85, 13 } } };
        /// <summary>t1544 v3: SEALED button sprites (the altar's UNSEAL sprites with the label repainted, in pact_cards_v2.png); AmSpr routes "seal_sd_*" here.</summary>
        bool AmSprV3(string n, int x, int y, float a) { int[] r; if (!V3Sealed.TryGetValue(n, out r)) return false; AmAtlas(V2Tex, r, x, y, a); return true; }
        bool V3 => PT.pactCardsV3;
        bool V4 => PT.pactCardsV3 && PT.pactCardsV4;
        static readonly int[] V4CardN = { 410, 0, 130, 123 }, V4CardL = { 410, 125, 130, 123 };   // t1544 v4: side cards without plate and avatar frame
        int V4Hx => V8 ? 2 * V8P + 35 - V2C_AvY : V7 ? PT.pactV7CentreTop + 35 + PT.pactV7Gap - V2C_AvY : Mathf.Max(0, V5 ? PT.pactV5HeaderExtra : PT.pactV4HeaderExtra);   // t1544 v5: the framed block needs a taller header
        static readonly int[] V2AvC = { 152, 0, 70, 70 }, V2AvN = { 152, 72, 70, 70 }, V2AvL = { 152, 144, 70, 70 }, V2PlateC = { 152, 216, 120, 15 };
        // mockup geometry (1x, relative to the card's top-left)
        const int V2C_AvY = 23, V2C_HdrY = 9, V2C_GapPlate = 4, V2C_NameDy = 4, V2C_TitleDy = 18;      // centre card (mock card 245,135)
        const int V2S_AvX = 30, V2S_AvY = 18, V2S_HdrY = 5, V2S_NameY = 97, V2S_TitleY = 111;          // side card (mock card 105,142), 130x123

        Texture2D V2Tex => T97("pact_cards_v2");
        /// <summary>9-slice of an atlas rect: caps l/r/t/b drawn 1:1, edges + middle stretched from the 1px source column mx / row my.</summary>
        void AmNine(Texture2D t, int[] r, int cl, int cr, int ct, int cb, int mx, int my, int dx, int dy, int dw, int dh)
        {
            if (t == null) return;
            int[] sx = { r[0], r[0] + mx, r[0] + r[2] - cr }, sw = { cl, 1, cr }, ddx = { dx, dx + cl, dx + dw - cr }, ddw = { cl, dw - cl - cr, cr };
            int[] sy = { r[1], r[1] + my, r[1] + r[3] - cb }, sh = { ct, 1, cb }, ddy = { dy, dy + ct, dy + dh - cb }, ddh = { ct, dh - ct - cb, cb };
            for (int j = 0; j < 3; j++) for (int i = 0; i < 3; i++) AmBlitS(t, sx[i], sy[j], sw[i], sh[j], ddx[i], ddy[j], ddw[i], ddh[j]);
        }
        void AmAtlas(Texture2D t, int[] r, int dx, int dy, float a = 1f) { if (t != null) MtBlit(t, r[0], r[1], r[2], r[3], dx, dy, a); }

        int V2Scale => Mathf.Clamp(PT.pactV2CentreScale, 1, 3);
        int V2F => 64 * V2Scale + 6;                                                                          // avatar frame (3px bevel each side)
        int V2CW => Mathf.Max(PT.pactV2CentreW, V2F + 16);
        int V2CH => V8 && V4 ? 3 * V8P + 35 + V2F : V4 ? V2C_AvY + V4Hx + V2F + PT.pactV4CentrePadB + 3 : V3 ? V2C_AvY + V2F + PT.pactV3CentreTitleGap + 7 + PT.pactV2CentrePadB + 3 : V2C_AvY + V2F + V2C_GapPlate + 15 + 3 + 7 + PT.pactV2CentrePadB + 3;
        int V2CX => 320 - V2CW / 2;
        int V2CY => V8 ? (PT.pactV8BarBottom + 1 + AltarMenuData.BtnY - 1 - (V2CH - 1 + PT.pactV8SealDy + 14)) / 2 : PT.pactV2CentreY;   // t1544 v8: gap under the bar = gap between SEAL (15 rows) and the buttons
        int V2SideH => V8 ? 3 * V8SP + 35 + 70 : 123;
        int V2SideY => V8 ? V2CY + (V2CH - 1) / 2 - (V2SideH - 1) / 2 : PT.pactV2SideY;                                   // t1544 v8: centred on the centre card
        int AmArrY() => V8 && PT.pactV8CentreArrows ? V2CY + (V2CH - 1) / 2 - 11 : AltarMenuData.ArrY;
        bool AmHideSceneFrames() => PT.pactCardsV2 && V8 && PT.pactV8HideSceneFrames;
        int AmV2SealY() => V2CY + V2CH - 1 + (V8 ? PT.pactV8SealDy : PT.pactV2SealDy);
        bool AmV2FacesFit() => AmV2SealY() + 15 + 3 <= AltarMenuData.FaceY;
        int V2SideW => V5 ? V5SideW() : 130;
        int V2SideX(int sd) => sd == 0 ? V2CX - PT.pactV2SideGap - V2SideW : V2CX + V2CW + PT.pactV2SideGap;

        /// <summary>centre card: YOUR PATH = mock red card + bar; other unlocked = NOT CHOSEN card; locked = dim card (no header, as the mock's right card).</summary>
        void AmCardsV2Centre(int v, bool locked, bool yours, bool sealed_)
        {
            var t = V2Tex; int X = V2CX, Y = V2CY, W = V2CW, Hh = V2CH, F = V2F, s = V2Scale;
            int fx = X + (W - F) / 2, fy = Y + V2C_AvY + (V4 ? V4Hx : 0);
            if (yours && !locked)
            {
                AmNine(t, V6NoBar ? V6CardC : V2CardC, 25, 24, 20, 10, 25, 21, X, Y, W, Hh); // t1544 v6: lacquer frame = no red header bar
                if (V4 && !V6NoBar) AmNine(t, new[] { V2CardC[0] + 4, V2CardC[1] + 4, 142, 15 }, 4, 4, 3, 3, 70, 7, X + 4, Y + 4, W - 8, 15 + V4Hx); // t1544 v4: header bar grown to two lines
                AmNine(t, V2AvC, 4, 4, 4, 4, 35, 35, fx, fy, F, F);
            }
            else
            {
                AmNine(t, locked ? V2CardL : V2CardN, 10, 10, 16, 14, 65, 16, X, Y, W, Hh); AmNine(t, locked ? V2AvL : V2AvN, 4, 4, 4, 4, 35, 35, fx, fy, F, F);
            }
            AmBlitN(T97(Av97(v, locked, true)), fx + 3, fy + 3, s);
            if (locked) AmSpr("lock", fx + F - 18, fy + F - 18);
            if (V4) // t1544 v4 (VH "bring the title up there too"): name + title in the (grown) header, nothing under the avatar
            {
                if (V5) { AmNameBlock5(X + W / 2, Y + (V8 ? V8P : V7 ? PT.pactV7CentreTop : V6 ? PT.pactV6CentreTop : 8), v, true, V8 ? W - 2 * V8P : V7 ? F : 0); if (sealed_) AmSpr("chip", X + 4, Y + 4); return; } // t1544 v5: framed name + title
                int ny = Y + 8 + (V4Hx - 9) / 2; AmTxIn(PactData.PathShort[v], X, 0, W, 0, "BONE", ny);
                AmTxIn(PactData.PathTitle[v], X, 0, W, 0, "CLAY", ny + 7 + PT.pactV4TitleGap);
                if (sealed_) AmSpr("chip", X + 4, Y + 4);
                return;
            }
            if (V3) // t1544 v3: the god's NAME in the header (red bar on your Path), title below the avatar, no plate, no path/choice labels
            {
                AmTxIn(PactData.PathShort[v], X, 0, W, 0, "BONE", Y + (yours && !locked ? V2C_HdrY : V2S_HdrY));
                AmTxIn(PactData.PathTitle[v], X, 0, W, 0, "CLAY", fy + F + PT.pactV3CentreTitleGap);
                if (sealed_) AmSpr("chip", X + 4, Y + 4);
                return;
            }
            if (yours && !locked) AmTxIn("YOUR PATH", X, 0, W, 0, "BONE", Y + V2C_HdrY);
            else if (!locked) AmTxIn("NOT CHOSEN", X, 0, W, 0, "DIM", Y + V2S_HdrY);
            int py = fy + F + V2C_GapPlate; AmAtlas(t, V2PlateC, X + (W - V2PlateC[2]) / 2, py);
            AmTxIn(PactData.PathShort[v], X, 0, W, 0, "BONE", py + V2C_NameDy);
            AmTxIn(PactData.PathTitle[v], X, 0, W, 0, "CLAY", py + V2C_TitleDy);
            if (sealed_) AmSpr("chip", X + 4, Y + 4);
        }
        /// <summary>side cards 1:1 from the mock: NOT CHOSEN card (header NOT CHOSEN, or YOUR PATH in amber on your Path), locked = dim card without header.</summary>
        void AmCardsV2Sides(int l, int r, bool free)
        {
            var t = V2Tex;
            for (int sd = 0; sd < 2; sd++)
            {
                int p = sd == 0 ? l : r, x = V2SideX(sd), y = V2SideY, sh = V2SideH; bool lk = PathLocked97(p), mine = _pactPath51 == p;
                int sw = V2SideW, avx = V5 ? (sw - 70) / 2 : V2S_AvX;
                string sid = "amside" + sd; bool sov = free && MtOver(sid, x, y, sw, sh), spr_ = MtPressed(sid, sov); int slv = AmLevel(sid, sov, spr_);
                int avy = V8 ? 2 * V8SP + 35 : V6 ? PT.pactV6SideFrameY : V5 ? PT.pactV5SideFrameY : V4 ? PT.pactV4SideFrameY : V2S_AvY;
                if (V5) AmNine(t, lk ? V4CardL : V4CardN, 10, 10, 16, 14, 65, 16, x, y, sw, sh); // t1544 v5: plain mockup card, widened for the framed block
                else AmAtlas(t, V4 ? (lk ? V4CardL : V4CardN) : V3 ? (lk ? V3CardL : V3CardN) : (lk ? V2CardL : V2CardN), x, y);
                if (V4) AmAtlas(t, lk ? V2AvL : V2AvN, x + avx, y + avy);
                AmBlitN(T97(Av97(p, lk, PT.pactGods1544 && PT.pactGodSideIdle1544)), x + avx + 3, y + avy + 3, 1);
                if (lk) AmSpr("lock", x + avx + 3 + 49, y + avy + 3 + 49);
                if (V5) AmNameBlock5(x + sw / 2, y + (V8 ? V8SP : V6 ? PT.pactV6SideTop : PT.pactV5SideBlockY), p, false, V8 ? sw - 2 * V8SP : 0); // t1544 v5
                else if (V4) { AmTxIn(PactData.PathShort[p], x, 0, 130, 0, "BONE", y + PT.pactV4SideNameY); AmTxIn(PactData.PathTitle[p], x, 0, 130, 0, "TERRA", y + PT.pactV4SideNameY + 7 + PT.pactV4TitleGap); } // t1544 v4
                else if (V3) { AmTxIn(PactData.PathShort[p], x, 0, 130, 0, "BONE", y + V2S_HdrY); AmTxIn(PactData.PathTitle[p], x, 0, 130, 0, "TERRA", y + PT.pactV3SideTitleY); } // t1544 v3
                else
                {
                    if (!lk) AmTxIn(mine ? "YOUR PATH" : "NOT CHOSEN", x, 0, 130, 0, mine ? "AMBER" : "DIM", y + V2S_HdrY);
                    AmTxIn(PactData.PathShort[p], x, 0, 130, 0, "BONE", y + V2S_NameY);
                    AmTxIn(PactData.PathTitle[p], x, 0, 130, 0, "TERRA", y + V2S_TitleY);
                }
                if (sov || slv > 0) MtBrackets(x - slv, y - slv, sw + 2 * slv, sh + 2 * slv);
                if (MtClick(sid, x, y, sw, sh, free)) Scroll97(sd == 0 ? -1 : 1);
            }
        }
    }
}
