using System.Collections.Generic;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>
    /// Builds sprites from ASCII pixel maps at runtime (used by the demo; handy for prototyping).
    /// Each char maps to a color; '.' or ' ' = transparent. Rows are listed top to bottom.
    /// </summary>
    public static class PixelSpriteFactory
    {
        public static Sprite FromRows(string[] rows, Dictionary<char, Color32> palette, float ppu = 16f, int padding = 2, string name = "PixelSprite")
        {
            int h = rows.Length, w = 0;
            foreach (var r in rows) w = Mathf.Max(w, r.Length);
            int W = w + padding * 2, H = h + padding * 2;
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false, false)
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = name };
            var px = new Color32[W * H];
            for (int y = 0; y < h; y++)
            {
                string row = rows[y];
                for (int x = 0; x < row.Length; x++)
                {
                    char ch = row[x];
                    if (ch == '.' || ch == ' ') continue;
                    if (palette.TryGetValue(ch, out var c))
                        px[(H - 1 - (y + padding)) * W + (x + padding)] = c;
                }
            }
            tex.SetPixels32(px); tex.Apply(false, false);
            // Pivot on a whole texel so odd-sized sprites stay on the pixel grid (v1.1; 0.5 put them half a pixel off).
            var pivot = new Vector2(Mathf.Floor(W * 0.5f) / W, Mathf.Floor(H * 0.5f) / H);
            var s = Sprite.Create(tex, new Rect(0, 0, W, H), pivot, ppu, 0, SpriteMeshType.FullRect);
            s.name = name;
            return s;
        }

        /// <summary>Sprite from raw pixels (bottom row first), point-filtered, pivot on a whole texel.</summary>
        public static Sprite FromPixels(Color32[] px, int w, int h, float ppu = 16f, string name = "PixelSprite")
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false)
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = name };
            tex.SetPixels32(px); tex.Apply(false, false);
            var s = Sprite.Create(tex, new UnityEngine.Rect(0, 0, w, h), new Vector2(Mathf.Floor(w * 0.5f) / w, Mathf.Floor(h * 0.5f) / h), ppu, 0, SpriteMeshType.FullRect);
            s.name = name;
            return s;
        }

        public static Sprite Rect(int w, int h, Color32 c, float ppu = 16f, string name = "Rect")
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false, false) { filterMode = FilterMode.Point, name = name };
            var px = new Color32[w * h];
            for (int i = 0; i < px.Length; i++) px[i] = c;
            tex.SetPixels32(px); tex.Apply(false, false);
            return Sprite.Create(tex, new UnityEngine.Rect(0, 0, w, h), new Vector2(Mathf.Floor(w * 0.5f) / w, Mathf.Floor(h * 0.5f) / h), ppu, 0, SpriteMeshType.FullRect);
        }
    }
}
