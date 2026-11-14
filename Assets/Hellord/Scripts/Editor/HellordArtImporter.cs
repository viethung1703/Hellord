using UnityEditor;
using UnityEngine;

namespace Hellord.EditorTools
{
    /// <summary>Import settings for the Hellord art sheets: Sprite, point filter, no compression, no mips, PPU 16.
    /// Strips are sliced at runtime by HArt (Sprite.Create from the manifest tables), so the importer keeps them as single sprites.</summary>
    public class HellordArtImporter : AssetPostprocessor
    {
        /// <summary>v0.0.45: sheets taller or wider than 2048 px keep their real size (off = old 2048 cap for every sheet).</summary>
        public const bool AllowBigSheets45 = true;
        public override uint GetVersion() => 47;
        static int BigSide(string path)
        {
            try
            {
                using (var f = System.IO.File.OpenRead(path))
                {
                    var b = new byte[24]; if (f.Read(b, 0, 24) < 24 || b[1] != 'P') return 0;
                    int w = (b[16] << 24) | (b[17] << 16) | (b[18] << 8) | b[19], h = (b[20] << 24) | (b[21] << 16) | (b[22] << 8) | b[23];
                    return Mathf.Max(w, h);
                }
            }
            catch { return 0; }
        }
        void OnPreprocessTexture()
        {
            if (assetPath.Replace('\\', '/').Contains("/Hellord/Resources/HellordFX928/"))
            {   // t928u shader FX textures: noise/cell = linear data, Repeat, Bilinear; ramps = Point, Clamp. No mips, no compression.
                var tf = (TextureImporter)assetImporter; bool ramp = assetPath.Contains("_ramp");
                tf.textureType = TextureImporterType.Default; tf.textureShape = TextureImporterShape.Texture2D; tf.isReadable = false; tf.mipmapEnabled = false; tf.textureCompression = TextureImporterCompression.Uncompressed;
                tf.npotScale = TextureImporterNPOTScale.None; tf.sRGBTexture = ramp; tf.alphaIsTransparency = false;
                tf.filterMode = ramp ? FilterMode.Point : FilterMode.Bilinear; tf.wrapMode = ramp ? TextureWrapMode.Clamp : TextureWrapMode.Repeat; return;
            }
            if (!assetPath.Replace('\\', '/').Contains("/Hellord/Resources/HellordArt/")) return;
            var ti = (TextureImporter)assetImporter;
            string fn80 = System.IO.Path.GetFileName(assetPath);
            if (fn80.StartsWith("ui_s80_fire_flow72") || fn80.StartsWith("ui_s79_paper_"))
            {   // step80: Sins-menu shader data textures (exact colour/alpha): Default, sRGB, Point, Clamp, no mips/compression, alpha untouched
                ti.textureType = TextureImporterType.Default; ti.textureShape = TextureImporterShape.Texture2D; ti.sRGBTexture = true; ti.alphaSource = TextureImporterAlphaSource.FromInput;
                ti.alphaIsTransparency = false; ti.filterMode = FilterMode.Point; ti.wrapMode = TextureWrapMode.Clamp; ti.mipmapEnabled = false;
                ti.textureCompression = TextureImporterCompression.Uncompressed; ti.npotScale = TextureImporterNPOTScale.None; ti.maxTextureSize = 2048; ti.isReadable = false; return;
            }
            ti.textureType = TextureImporterType.Sprite; ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 16; ti.filterMode = FilterMode.Point; ti.mipmapEnabled = false;
            ti.maxTextureSize = 2048; // v0.0.9: WebGL-safe; the 5760 px boss strips are re-packed into 2016x216 grids (tools/grid_boss_sheets.py)
            int big = BigSide(assetPath);
            if (AllowBigSheets45 && big > 2048) ti.maxTextureSize = big > 4096 ? 8192 : 4096; // v0.0.45: boss_qilin_chariot is 1440x2736; a 2048 cap shrank it so HArt could not slice 160x144 cells and the boss drew a blank placeholder
            ti.textureCompression = TextureImporterCompression.Uncompressed; ti.npotScale = TextureImporterNPOTScale.None;
            ti.alphaIsTransparency = true;
            ti.isReadable = System.IO.Path.GetFileName(assetPath).StartsWith("hero_") || (assetPath.Contains("/IceSkills/fx_icenova_burst") && assetPath.Contains("_glow")) /* t1419: runtime reads the glow mask to get the per-frame ring radius */ || assetPath.Contains("/HellordArt/FrozenHell/") || assetPath.Contains("/HellordArt/BurningHell/"); // v0.6: hero outline is built from the pixels; v0.0.44: Frozen floor B tiles are composed at runtime
            ti.wrapMode = assetPath.Contains("floor_") ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
        }
    }
}
