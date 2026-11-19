using UnityEngine;
namespace Hellord
{
    /// <summary>t1544 (VH "good, apply"): the menu-theme Sacrifice Altar shows the 4 Paths as gods. Each card's name plate grows to two lines
    /// (NAME in BONE, title under it: CLAY on the centre card, TERRA on the side cards, DIM when locked), drawn as a 9-slice of the centre card's
    /// own plate art. The hanging tab's baked Path name is painted over and redrawn as the god name. Toggle: HellordTuning.pactGods1544.</summary>
    public partial class HellordGame
    {
        /// <summary>stretched blit of a source rect (top-left texel coords) into a dest rect of the 640x360 grid (point textures, no clipping).</summary>
        void AmBlitS(Texture2D t, int sx, int sy, int sw, int sh, int dx, int dy, int dw, int dh)
        {
            if (!_mtRep || t == null || sw <= 0 || sh <= 0 || dw <= 0 || dh <= 0) return;
            var r = new Rect(_mtOx + dx * _mtS, _mtOy + dy * _mtS, dw * _mtS, dh * _mtS);
            var uv = new Rect(sx / (float)t.width, 1f - (sy + sh) / (float)t.height, sw / (float)t.width, sh / (float)t.height);
            GUI.DrawTextureWithTexCoords(r, t, uv);
        }
        /// <summary>9-slice: corners (c x c) 1:1, edges and middle stretched from the source's centre row/column.</summary>
        void AmPlate9(Texture2D t, int sx, int sy, int sw, int sh, int dx, int dy, int dw, int dh, int c)
        {
            int mx = sx + sw / 2, my = sy + sh / 2, iw = dw - 2 * c, ih = dh - 2 * c;
            int[] srcX = { sx, mx, sx + sw - c }, srcW = { c, 1, c }, dstX = { dx, dx + c, dx + dw - c }, dstW = { c, iw, c };
            int[] srcY = { sy, my, sy + sh - c }, srcH = { c, 1, c }, dstY = { dy, dy + c, dy + dh - c }, dstH = { c, ih, c };
            for (int j = 0; j < 3; j++) for (int i = 0; i < 3; i++) AmBlitS(t, srcX[i], srcY[j], srcW[i], srcH[j], dstX[i], dstY[j], dstW[i], dstH[j]);
        }
        static int AmBbW(string s) { int l, r; AmBb(Sanitize(s), out l, out r); return r - l + 1; }
        /// <summary>two-line god plate: 9-slice of the centre card plate (atlas card sprite at +9,+0, 141x15) at (x, top) w x pactPlateH1544; name + title centred.</summary>
        void AmGodPlate(string card, int x, int top, int w, string name, string title, string nameCol, string titleCol)
        {
            int[] r; if (_amAtlas != null && AltarMenuData.Atlas.TryGetValue(card, out r)) AmPlate9(_amAtlas, r[0] + 9, r[1], 141, 15, x, top, w, PT.pactPlateH1544, 4);
            int ny = top + 3; AmTxIn(name, x, 0, w, 0, nameCol, ny);
            if (!string.IsNullOrEmpty(title)) AmTxIn(title, x, 0, w, 0, titleCol, ny + 7 + PT.pactTitleGap1544);
        }
        /// <summary>side plate width: old 89, widened (even steps, stays centred) to fit the longest line plus pactSidePlatePad1544 each side.</summary>
        int AmSidePlateW(int p)
        {
            int w = Mathf.Max(PT.pactSidePlateMinW1544, Mathf.Max(AmBbW(PactData.PathShort[p]), AmBbW(PactData.PathTitle[p])) + 2 * PT.pactSidePlatePad1544);
            if (((w - 89) & 1) != 0) w++; return w;
        }
        static readonly string[] OldTabName1544 = { "PYROMANCER", "NECROMANCER", "STORMCALLER", "BLOOD REAVER" };
        /// <summary>the tab art has the old Path name baked at (tab x + 19, 50): paint it over with the tab fill (MT_INK) and draw the god name.</summary>
        void AmTabName1544(int v, int tabX, int tabIx = -1)
        {
            int x = tabX + 19, w = AmBbW(OldTabName1544[tabIx >= 0 ? tabIx : v]); // t1561: the tab art's baked name (Gonggong uses another Path's tab)
            MtFill(x - 1, 49, w + 2, 9, MT_INK); AmTxAt(PactData.PathShort[v], x, 50, "BONE");
        }
        int[] AmFaceCrop1544(int p)
        {
            var a = PT.pactGodFaceCrop1544; if (!PT.pactGods1544 || a == null || a.Length < 2 * p + 2) return p < AltarMenuData.FaceCrop.Length ? AltarMenuData.FaceCrop[p] : new[] { 32, 28 }; // t1561
            return new[] { Mathf.Clamp(a[2 * p], 12, 52), Mathf.Clamp(a[2 * p + 1], 12, 52) };
        }
    }
}
