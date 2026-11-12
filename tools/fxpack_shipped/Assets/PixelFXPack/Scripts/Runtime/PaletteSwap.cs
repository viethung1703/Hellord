using UnityEngine;

namespace PixelFXPack
{
    /// <summary>
    /// Palette swap via a small LUT texture. Either assign a LUT texture (row 0 = source colors at the TOP,
    /// following rows = replacements; import with Point filter, no compression, no mipmaps) or fill
    /// sourceColors + palettes and a LUT is built at runtime.
    /// </summary>
    [RequireComponent(typeof(PixelFX))]
    [AddComponentMenu("Pixel FX/Palette Swap")]
    public class PaletteSwap : MonoBehaviour
    {
        [System.Serializable] public class Palette { public string name = "Alt"; public Color[] colors = new Color[0]; }

        public Texture2D lut;
        public Color[] sourceColors = new Color[0];
        public Palette[] palettes = new Palette[0];
        [Tooltip("0 = original colors")] public int activePalette = 0;

        PixelFX _fx; Texture2D _built;

        void OnEnable()
        {
            _fx = GetComponent<PixelFX>();
            if (lut == null && sourceColors.Length > 0) _built = BuildLUT(sourceColors, palettes);
            Set(activePalette);
        }

        void OnDestroy() { if (_built != null) Destroy(_built); }

        public int PaletteCount => (lut != null ? lut.height : (_built != null ? _built.height : 1)) - 1;

        /// <summary>0 = original, 1..PaletteCount = replacement palette.</summary>
        public void Set(int index)
        {
            activePalette = index;
            if (_fx == null) _fx = GetComponent<PixelFX>();
            _fx.paletteLUT = lut != null ? lut : _built;
            _fx.paletteRow = index;
        }

        public void Next() { Set(PaletteCount <= 0 ? 0 : (activePalette + 1) % (PaletteCount + 1)); }

        /// <summary>Builds a LUT: width = colors (max 16), height = 1 + palettes. Row 0 (top) = source.</summary>
        public static Texture2D BuildLUT(Color[] source, Palette[] pals)
        {
            int w = Mathf.Clamp(source.Length, 1, 16), h = 1 + (pals != null ? pals.Length : 0);
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false)
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "PixelFX Runtime LUT" };
            for (int x = 0; x < w; x++)
            {
                tex.SetPixel(x, h - 1, source[x]); // texture y=0 is bottom, so the top row is h-1
                for (int p = 0; p < h - 1; p++)
                {
                    var cols = pals[p].colors;
                    tex.SetPixel(x, h - 2 - p, (cols != null && x < cols.Length) ? cols[x] : source[x]);
                }
            }
            tex.Apply(false, false);
            return tex;
        }
    }
}
