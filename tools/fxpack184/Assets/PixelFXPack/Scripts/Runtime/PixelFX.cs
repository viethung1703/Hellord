using UnityEngine;

namespace PixelFXPack
{
    /// <summary>
    /// Drives the "PixelFX/Sprite Uber" shader on a SpriteRenderer through a MaterialPropertyBlock,
    /// so many sprites can share one material while having different effect values.
    /// Helper components (HitFlash, DissolveFX, ShineSweep, PaletteSwap) write into this.
    /// </summary>
    [ExecuteAlways, DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    [AddComponentMenu("Pixel FX/Pixel FX (Sprite)")]
    public class PixelFX : MonoBehaviour
    {
        public const string ShaderName = "PixelFX/Sprite Uber";

        [Header("Outline")]
        public Color outlineColor = Color.black;
        [Range(0, 4)] public int outlineSize = 0;
        public bool innerOutline = false;
        public bool outlineDiagonals = false;

        [Header("Hit Flash")]
        public Color flashColor = Color.white;
        [Range(0, 1)] public float flashAmount = 0;

        [Header("Dissolve")]
        [Range(0, 1)] public float dissolveAmount = 0;
        [Range(1, 8)] public int dissolveCell = 1;
        public Color dissolveEdgeColor = new Color(1f, 0.5f, 0f, 1f);
        [Range(0, 0.2f)] public float dissolveEdgeWidth = 0.06f;
        public float dissolveSeed = 0;

        [Header("Palette Swap (LUT: row 0 = source colors)")]
        public Texture2D paletteLUT;
        [Tooltip("0 = off. 1..rows-1 picks a replacement row.")] public int paletteRow = 0;

        [Header("Silhouette")]
        public Color silhouetteColor = new Color(0.2f, 0.6f, 1f, 0.6f);
        [Range(0, 1)] public float silhouetteAmount = 0;

        [Header("Wave / Wobble")]
        [Range(0, 8)] public float waveAmplitude = 0;
        public float waveFrequency = 0.5f;
        public float waveSpeed = 4f;

        [Header("Shine Sweep")]
        public Color shineColor = Color.white;
        [Tooltip("-1 = off, 0..1 sweeps across the sprite")] public float shinePosition = -1;
        [Range(0.01f, 0.5f)] public float shineWidth = 0.12f;
        [Range(-2, 2)] public float shineSlant = 0.5f;

        [Header("Pixelate")]
        [Range(1, 32)] public int pixelateSize = 1;

        [Header("Rim Light (v1.2)")]
        public Color rimColor = new Color(1f, 0.75f, 0.3f, 1f);
        [Range(0, 1)] public float rimAmount = 0;
        [Tooltip("Direction the light comes from, in texels (x=+1: from the right, y=+1: from above).")] public Vector2Int rimDirection = new Vector2Int(1, 1);

        [Header("Emission (v1.2) - pair with URP HDR + Bloom for real glow")]
        public Color emissionColor = Color.white;
        [Range(0, 4)] public float emissionAmount = 0;
        [Range(1, 8)] public float dissolveEdgeGlow = 1;

        [Header("Wind Sway (v1.3) - top of sprite sways, bottom pinned; world units")]
        [Range(0, 0.5f)] public float windAmount = 0;
        public float windSpeed = 2;
        [Header("Glitch / Motion Blur / Chromatic (v1.3)")]
        [Range(0, 1)] public float glitchAmount = 0;
        [Range(0, 8)] public float motionBlur = 0;
        public Vector2 motionBlurDirection = new Vector2(1, 0);
        [Range(0, 4)] public float chromaticOffset = 0;
        [Header("Hologram / Frozen / Burn (v1.3)")]
        [Range(0, 1)] public float hologramAmount = 0;
        public Color hologramColor = new Color(0.17f, 0.91f, 0.96f, 1);
        [Range(0, 1)] public float frozenAmount = 0;
        [Range(0, 1)] public float burnAmount = 0;
        public Color burnColor = new Color(1, 0.55f, 0.1f, 1);
        [Header("Tint Pulse - poison, buff, low HP (v1.3)")]
        public Color pulseColor = new Color(0.39f, 0.78f, 0.3f, 1);
        [Range(0, 1)] public float pulseAmount = 0;
        public float pulseSpeed = 6;
        [Header("Colour grades (v1.3)")]
        [Range(0, 1)] public float gradientMapAmount = 0;
        public Color gradientDark = new Color(0.1f, 0.08f, 0.15f, 1);
        public Color gradientMid = new Color(0.71f, 0.31f, 0.53f, 1);
        public Color gradientLight = new Color(1, 0.9f, 0.6f, 1);
        [Range(0, 1)] public float greyscale = 0;
        [Range(0, 1)] public float sepia = 0;
        [Range(0, 1)] public float negative = 0;
        [Range(0, 1)] public float rainbow = 0;
        public float rainbowSpeed = 1;
        [Range(0, 1)] public float hueShift = 0;
        [Header("Dither Fade (v1.3) - 0 visible, 1 gone")]
        [Range(0, 1)] public float ditherFade = 0;

        [Header("Drop Shadow (needs transparent padding in the sprite)")]
        public Color shadowColor = new Color(0, 0, 0, 0.5f);
        public Vector2Int shadowOffset = Vector2Int.zero;

        SpriteRenderer _sr;
        MaterialPropertyBlock _mpb;

        static readonly int ID_OutlineColor = Shader.PropertyToID("_OutlineColor");
        static readonly int ID_OutlineSize = Shader.PropertyToID("_OutlineSize");
        static readonly int ID_OutlineInner = Shader.PropertyToID("_OutlineInner");
        static readonly int ID_OutlineDiagonal = Shader.PropertyToID("_OutlineDiagonal");
        static readonly int ID_FlashColor = Shader.PropertyToID("_FlashColor");
        static readonly int ID_FlashAmount = Shader.PropertyToID("_FlashAmount");
        static readonly int ID_DissolveAmount = Shader.PropertyToID("_DissolveAmount");
        static readonly int ID_DissolvePixel = Shader.PropertyToID("_DissolvePixel");
        static readonly int ID_DissolveEdgeColor = Shader.PropertyToID("_DissolveEdgeColor");
        static readonly int ID_DissolveEdgeWidth = Shader.PropertyToID("_DissolveEdgeWidth");
        static readonly int ID_DissolveSeed = Shader.PropertyToID("_DissolveSeed");
        static readonly int ID_PaletteTex = Shader.PropertyToID("_PaletteTex");
        static readonly int ID_PaletteRow = Shader.PropertyToID("_PaletteRow");
        static readonly int ID_PaletteRows = Shader.PropertyToID("_PaletteRows");
        static readonly int ID_PaletteColors = Shader.PropertyToID("_PaletteColors");
        static readonly int ID_SilColor = Shader.PropertyToID("_SilhouetteColor");
        static readonly int ID_SilAmount = Shader.PropertyToID("_SilhouetteAmount");
        static readonly int ID_WaveAmp = Shader.PropertyToID("_WaveAmp");
        static readonly int ID_WaveFreq = Shader.PropertyToID("_WaveFreq");
        static readonly int ID_WaveSpeed = Shader.PropertyToID("_WaveSpeed");
        static readonly int ID_ShineColor = Shader.PropertyToID("_ShineColor");
        static readonly int ID_ShinePos = Shader.PropertyToID("_ShinePos");
        static readonly int ID_ShineWidth = Shader.PropertyToID("_ShineWidth");
        static readonly int ID_ShineSlant = Shader.PropertyToID("_ShineSlant");
        static readonly int ID_Pixelate = Shader.PropertyToID("_PixelateSize");
        static readonly int ID_RimColor = Shader.PropertyToID("_RimColor");
        static readonly int ID_RimAmount = Shader.PropertyToID("_RimAmount");
        static readonly int ID_RimDir = Shader.PropertyToID("_RimDir");
        static readonly int ID_EmColor = Shader.PropertyToID("_EmissionColor");
        static readonly int ID_EmAmount = Shader.PropertyToID("_EmissionAmount");
        static readonly int ID_EdgeGlow = Shader.PropertyToID("_DissolveEdgeGlow");
        static readonly int ID13_windAmount = Shader.PropertyToID("_WindAmount");
        static readonly int ID13_windSpeed = Shader.PropertyToID("_WindSpeed");
        static readonly int ID13_glitchAmount = Shader.PropertyToID("_GlitchAmount");
        static readonly int ID13_motionBlur = Shader.PropertyToID("_MotionBlur");
        static readonly int ID13_motionBlurDirection = Shader.PropertyToID("_MotionBlurDir");
        static readonly int ID13_chromaticOffset = Shader.PropertyToID("_ChromaAmount");
        static readonly int ID13_hologramAmount = Shader.PropertyToID("_HoloAmount");
        static readonly int ID13_hologramColor = Shader.PropertyToID("_HoloColor");
        static readonly int ID13_frozenAmount = Shader.PropertyToID("_FrozenAmount");
        static readonly int ID13_burnAmount = Shader.PropertyToID("_BurnAmount");
        static readonly int ID13_burnColor = Shader.PropertyToID("_BurnColor");
        static readonly int ID13_pulseColor = Shader.PropertyToID("_PulseColor");
        static readonly int ID13_pulseAmount = Shader.PropertyToID("_PulseAmount");
        static readonly int ID13_pulseSpeed = Shader.PropertyToID("_PulseSpeed");
        static readonly int ID13_gradientMapAmount = Shader.PropertyToID("_GradMapAmount");
        static readonly int ID13_gradientDark = Shader.PropertyToID("_GradA");
        static readonly int ID13_gradientMid = Shader.PropertyToID("_GradB");
        static readonly int ID13_gradientLight = Shader.PropertyToID("_GradC");
        static readonly int ID13_greyscale = Shader.PropertyToID("_GreyAmount");
        static readonly int ID13_sepia = Shader.PropertyToID("_SepiaAmount");
        static readonly int ID13_negative = Shader.PropertyToID("_NegativeAmount");
        static readonly int ID13_rainbow = Shader.PropertyToID("_RainbowAmount");
        static readonly int ID13_rainbowSpeed = Shader.PropertyToID("_RainbowSpeed");
        static readonly int ID13_hueShift = Shader.PropertyToID("_HueShift");
        static readonly int ID13_ditherFade = Shader.PropertyToID("_DitherFade");
        static readonly int ID_ShadowColor = Shader.PropertyToID("_ShadowColor");
        static readonly int ID_ShadowOffset = Shader.PropertyToID("_ShadowOffset");

        public SpriteRenderer SpriteRenderer => _sr != null ? _sr : (_sr = GetComponent<SpriteRenderer>());

        /// <summary>Assigns a shared material using the Uber shader if the renderer does not use it yet.</summary>
        public void EnsureMaterial()
        {
            var sr = SpriteRenderer;
            if (sr.sharedMaterial != null && sr.sharedMaterial.shader != null && sr.sharedMaterial.shader.name == ShaderName) return;
            var mat = PixelFXMaterials.Shared;
            if (mat != null) sr.sharedMaterial = mat;
        }

        void OnEnable() { EnsureMaterial(); Apply(); }
        void OnValidate() { if (isActiveAndEnabled) Apply(); }
        static readonly int ID_MainTex = Shader.PropertyToID("_MainTex");
        void LateUpdate() { Apply(); }

        /// <summary>Pushes all values to the renderer. Called every LateUpdate; cheap for typical sprite counts.</summary>
        public void Apply()
        {
            var sr = SpriteRenderer;
            if (sr == null) return;
            if (_mpb == null) _mpb = new MaterialPropertyBlock();
            sr.GetPropertyBlock(_mpb);
            _mpb.SetColor(ID_OutlineColor, outlineColor);
            _mpb.SetFloat(ID_OutlineSize, outlineSize);
            _mpb.SetFloat(ID_OutlineInner, innerOutline ? 1 : 0);
            _mpb.SetFloat(ID_OutlineDiagonal, outlineDiagonals ? 1 : 0);
            _mpb.SetColor(ID_FlashColor, flashColor);
            _mpb.SetFloat(ID_FlashAmount, flashAmount);
            _mpb.SetFloat(ID_DissolveAmount, dissolveAmount);
            _mpb.SetFloat(ID_DissolvePixel, dissolveCell);
            _mpb.SetColor(ID_DissolveEdgeColor, dissolveEdgeColor);
            _mpb.SetFloat(ID_DissolveEdgeWidth, dissolveEdgeWidth);
            _mpb.SetFloat(ID_DissolveSeed, dissolveSeed);
            if (paletteLUT != null)
            {
                _mpb.SetTexture(ID_PaletteTex, paletteLUT);
                _mpb.SetFloat(ID_PaletteRows, paletteLUT.height);
                _mpb.SetFloat(ID_PaletteColors, Mathf.Min(16, paletteLUT.width));
                _mpb.SetFloat(ID_PaletteRow, Mathf.Clamp(paletteRow, 0, paletteLUT.height - 1));
            }
            else _mpb.SetFloat(ID_PaletteRow, 0);
            _mpb.SetColor(ID_SilColor, silhouetteColor);
            _mpb.SetFloat(ID_SilAmount, silhouetteAmount);
            _mpb.SetFloat(ID_WaveAmp, waveAmplitude);
            _mpb.SetFloat(ID_WaveFreq, waveFrequency);
            _mpb.SetFloat(ID_WaveSpeed, waveSpeed);
            _mpb.SetColor(ID_ShineColor, shineColor);
            _mpb.SetFloat(ID_ShinePos, shinePosition);
            _mpb.SetFloat(ID_ShineWidth, shineWidth);
            _mpb.SetFloat(ID_ShineSlant, shineSlant);
            _mpb.SetFloat(ID_Pixelate, pixelateSize);
            _mpb.SetColor(ID_RimColor, rimColor);
            _mpb.SetFloat(ID_RimAmount, rimAmount);
            _mpb.SetVector(ID_RimDir, new Vector4(rimDirection.x, rimDirection.y, 0, 0));
            _mpb.SetColor(ID_EmColor, emissionColor);
            _mpb.SetFloat(ID_EmAmount, emissionAmount);
            _mpb.SetFloat(ID_EdgeGlow, dissolveEdgeGlow);
            _mpb.SetColor(ID_ShadowColor, shadowColor);
            _mpb.SetVector(ID_ShadowOffset, new Vector4(shadowOffset.x, shadowOffset.y, 0, 0));
            _mpb.SetFloat(ID13_windAmount, windAmount);
            _mpb.SetFloat(ID13_windSpeed, windSpeed);
            _mpb.SetFloat(ID13_glitchAmount, glitchAmount);
            _mpb.SetFloat(ID13_motionBlur, motionBlur);
            _mpb.SetVector(ID13_motionBlurDirection, new Vector4(motionBlurDirection.x, motionBlurDirection.y, 0, 0));
            _mpb.SetFloat(ID13_chromaticOffset, chromaticOffset);
            _mpb.SetFloat(ID13_hologramAmount, hologramAmount);
            _mpb.SetColor(ID13_hologramColor, hologramColor);
            _mpb.SetFloat(ID13_frozenAmount, frozenAmount);
            _mpb.SetFloat(ID13_burnAmount, burnAmount);
            _mpb.SetColor(ID13_burnColor, burnColor);
            _mpb.SetColor(ID13_pulseColor, pulseColor);
            _mpb.SetFloat(ID13_pulseAmount, pulseAmount);
            _mpb.SetFloat(ID13_pulseSpeed, pulseSpeed);
            _mpb.SetFloat(ID13_gradientMapAmount, gradientMapAmount);
            _mpb.SetColor(ID13_gradientDark, gradientDark);
            _mpb.SetColor(ID13_gradientMid, gradientMid);
            _mpb.SetColor(ID13_gradientLight, gradientLight);
            _mpb.SetFloat(ID13_greyscale, greyscale);
            _mpb.SetFloat(ID13_sepia, sepia);
            _mpb.SetFloat(ID13_negative, negative);
            _mpb.SetFloat(ID13_rainbow, rainbow);
            _mpb.SetFloat(ID13_rainbowSpeed, rainbowSpeed);
            _mpb.SetFloat(ID13_hueShift, hueShift);
            _mpb.SetFloat(ID13_ditherFade, ditherFade);
            // v1.8.5: GetPropertyBlock returns the renderer's cached _MainTex; re-applying it after the sprite changed to another texture
            // made the new frame sample the old sheet (invisible / half-drawn sprite). Always push the current sprite's texture.
            var spr = sr.sprite; if (spr != null && spr.texture != null) _mpb.SetTexture(ID_MainTex, spr.texture);
            sr.SetPropertyBlock(_mpb);
        }
    }

    /// <summary>Lazily creates one shared material for the Uber shader.</summary>
    public static class PixelFXMaterials
    {
        /// <summary>Resources path of the bundled material (Assets/PixelFXPack/Resources/PixelFX/PixelFX_Uber.mat).</summary>
        public const string ResourcePath = "PixelFX/PixelFX_Uber";
        public const string AdditiveResourcePath = "PixelFX/PixelFX_Additive";
        public const string AdditiveShaderName = "PixelFX/Additive";
        static Material _additive;
        /// <summary>Additive material (glows, slashes, sparks, afterimages). Loaded from Resources so it ships in builds.</summary>
        public static Material Additive
        {
            get
            {
                if (_additive != null) return _additive;
                var res = Resources.Load<Material>(AdditiveResourcePath);
                if (res != null && res.shader != null && res.shader.isSupported) return _additive = res;
                var sh = Shader.Find(AdditiveShaderName);
                if (sh == null) { Debug.LogWarning("[PixelFX] Additive shader not found; glows fall back to alpha blending."); return null; }
                return _additive = new Material(sh) { name = "PixelFX Additive (Shared)", hideFlags = HideFlags.DontSave };
            }
        }
        static Material _shared;
        public static Material Shared
        {
            get
            {
                if (_shared != null) return _shared;
                // 1) Material shipped in Resources/ - guarantees the shader is included in player builds (WebGL, mobile).
                var res = Resources.Load<Material>(ResourcePath);
                if (res != null && res.shader != null && res.shader.isSupported) { _shared = res; return _shared; }
                // 2) Shader.Find - works in the Editor, or in builds if the shader is in Always Included Shaders.
                var sh = Shader.Find(PixelFX.ShaderName);
                if (sh == null)
                {
                    Debug.LogWarning("[PixelFX] Shader '" + PixelFX.ShaderName + "' not found. Assign a material using it, or add the shader to Always Included Shaders for builds.");
                    return null;
                }
                _shared = new Material(sh) { name = "PixelFX Uber (Shared)", hideFlags = HideFlags.DontSave };
                return _shared;
            }
        }
    }
}
