using UnityEngine;
namespace Hellord
{
    /// <summary>t1544 v5 (VH: "The name should have frame or something, try me", then "name and title"): the god's NAME and title sit together in
    /// one framed two-line block at the top of every Altar pact card. HellordTuning.pactNameFrame5 picks the frame:
    /// Plate = the bevelled name plate from HA's mockup (pact_cards_v2.png plate_c, 9-sliced to two lines); Banner = a dark crimson ribbon with
    /// folded, notched tails; Tag = a thin box in the card-border colours with corner studs. None = the a80f467 v4 header (no frame).
    /// All colours are Endesga 32 (the game palette); the art is code-drawn or cut from the mockup, no new generated art.</summary>
    public partial class HellordGame
    {
        bool V5 => V4 && PT.pactNameFrame5 != HellordTuning.PactNameFrame.None;
        /// <summary>t1544 v6: HA's t1552 frame 2, lacquer cloud-scroll (VH's pick), 129x35, 9-slice L25 R25 T15 B5, stretch col 64 / row 22.</summary>
        bool V6 => V5 && PT.pactNameFrame5 == HellordTuning.PactNameFrame.Lacquer;
        bool V6NoBar => V6 && PT.pactV6NoRedBar;
        static readonly int[] V6Frame = { 0, 0, 129, 35 }, V6CardC = { 542, 0, 150, 137 };   // pact_frame_lacquer.png; card_c5 (YOUR PATH card without the red bar) in pact_cards_v2.png
        const int V5ZoneH = 25;                                    // tallest block (Banner 21-row body + 4-row tails); Plate 24, Tag 22, centred in the zone
        static readonly Color32 F5_INK = new Color32(0x0e, 0x0a, 0x14, 255), F5_NAVY = new Color32(0x18, 0x14, 0x25, 255), F5_P1 = new Color32(0x3e, 0x27, 0x31, 255),
            F5_WINE = new Color32(0x6e, 0x1c, 0x2a, 255), F5_RED = new Color32(0xa2, 0x26, 0x33, 255), F5_BORDER = new Color32(0x8f, 0x4f, 0x43, 255),
            F5_CLAY = new Color32(0xe4, 0xa6, 0x72, 255), F5_TERRA = new Color32(0xb8, 0x6f, 0x50, 255), F5_BROWN = new Color32(0x73, 0x3e, 0x39, 255);
        /// <summary>frame width shared by every card: the widest name/title line of all Paths + pactV5FramePad each side (THE HEADLESS WAR GOD 113 -> 123).</summary>
        int V5FrameW()
        {
            int w = 0; for (int p = 0; p < NP97; p++) w = Mathf.Max(w, Mathf.Max(AmBbW(PactData.PathShort[p]), AmBbW(PactData.PathTitle[p])));
            w += 2 * (V6 ? PT.pactV6FramePad : PT.pactV5FramePad); if ((w & 1) == 0) w++; return w;                  // odd: centres on the odd-width glyph runs
        }
        /// <summary>side card width in v5: wide enough for the frame (+ banner tails) inside the card border; never under the mockup's 130.</summary>
        int V5SideW() { int w = Mathf.Max(130, V5FrameW() + 2 * PT.pactV5SideMargin); if ((w & 1) != 0) w++; return w; }
        /// <summary>the framed NAME + title block, centred on cx, zone top y (V5ZoneH rows). Name BONE, title CLAY (centre) / TERRA (sides).</summary>
        void AmNameBlock5(int cx, int top, int p, bool centre)
        {
            int w = V5FrameW(), x0 = cx - w / 2, ny; string nm = PactData.PathShort[p], tl = PactData.PathTitle[p], tc = centre ? "CLAY" : "TERRA";
            switch (PT.pactNameFrame5)
            {
                case HellordTuning.PactNameFrame.Banner:
                {
                    int y0 = top, bh = 21, tail = PT.pactV5BannerTail, sh = PT.pactV5BannerInset; x0 += sh; w -= 2 * sh; tc = "CLAY"; // body a bit narrower so the tails show; CLAY title (TERRA is too dark on wine)
                    for (int sd = 0; sd < 2; sd++)                                                         // tails behind the body: 4 rows lower, notched outer end
                    {
                        int tx0 = sd == 0 ? x0 - tail : x0 + w - 4, tw = tail + 4, ty = y0 + 4;
                        MtFill(tx0, ty, tw, bh, F5_INK); MtFill(tx0 + 1, ty + 1, tw - 2, bh - 2, F5_WINE); MtFill(tx0 + 1, ty + 1, tw - 2, 1, F5_RED); MtFill(tx0 + 1, ty + bh - 2, tw - 2, 1, F5_P1);
                        int mid = ty + bh / 2;
                        for (int yy = ty; yy < ty + bh; yy++) { int d = Mathf.Max(0, 4 - Mathf.Abs(yy - mid)); if (d > 0) { if (sd == 0) MtFill(tx0, yy, d, 1, F5_INK); else MtFill(tx0 + tw - d, yy, d, 1, F5_INK); } }
                        int fx = sd == 0 ? x0 : x0 + w - 3; MtFill(fx, y0 + bh, 3, 3, F5_INK); MtFill(sd == 0 ? fx + 1 : fx, y0 + bh, 2, 2, F5_WINE); // fold under the body
                    }
                    MtFill(x0, y0, w, bh, F5_INK); MtFill(x0 + 1, y0 + 1, w - 2, bh - 2, F5_WINE); MtFill(x0 + 1, y0 + 1, w - 2, 1, F5_RED); MtFill(x0 + 1, y0 + bh - 2, w - 2, 1, F5_P1);
                    ny = y0 + 3; break;
                }
                case HellordTuning.PactNameFrame.Tag:
                {
                    int y0 = top + 1, th = 22;
                    MtFill(x0, y0, w, th, F5_BORDER); MtFill(x0 + 1, y0 + 1, w - 2, th - 2, F5_INK); MtFill(x0 + 2, y0 + 2, w - 4, th - 4, F5_NAVY);
                    int[] sx = { x0, x0 + w - 2 }, sy = { y0, y0 + th - 2 };                                // corner studs on the border corners
                    foreach (int a in sx) foreach (int b in sy) { MtFill(a - 1, b - 1, 4, 4, F5_INK); MtFill(a, b, 2, 2, F5_TERRA); MtFill(a, b, 1, 1, F5_CLAY); } // 2x2 studs, ink rim, lit top-left
                    ny = y0 + 3; break;
                }
                case HellordTuning.PactNameFrame.Lacquer:                                                  // t1544 v6: HA's lacquer cloud-scroll, name +9 / title +19 (3px leading)
                {
                    AmNine(T97("pact_frame_lacquer"), V6Frame, 25, 25, 15, 5, 64, 22, x0, top, w, 35);
                    AmTxIn(nm, x0, 0, w, 0, "BONE", top + PT.pactV6NameDy); AmTxIn(tl, x0, 0, w, 0, tc, top + PT.pactV6TitleDy);
                    return;
                }
                default:                                                                                   // Plate: the mockup plate, 9-sliced to two lines
                {
                    int y0 = top; AmNine(V2Tex, V2PlateC, 4, 4, 4, 4, 60, 7, x0, y0, w, 24);
                    ny = y0 + 4; break;
                }
            }
            AmTxIn(nm, x0, 0, w, 0, "BONE", ny); AmTxIn(tl, x0, 0, w, 0, tc, ny + 7 + PT.pactV4TitleGap);
        }
    }
}
