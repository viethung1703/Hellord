using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>Element family of a skill light. Colour follows the element.</summary>
    public enum LightElement { Fire = 0, Ice = 1, Lightning = 2, Bone = 3, Soul = 4, Curse = 5, Grave = 6, Holy = 7 }

    /// <summary>HM hook names (t591u). Pass to <see cref="SkillLightFX.Hook"/>.</summary>
    public static class SkillLightHooks
    {
        public const string FireballFlight = "light_fireball_flight", FireballMuzzle = "light_fireball_muzzle", FireballImpact = "light_fireball_impact",
            HellfirePillarImpact = "light_hellfire_pillar_impact", FrostCleaveImpact = "light_frost_cleave_impact",
            StormRingImpact = "light_storm_ring_impact", ChainLightningImpact = "light_chain_lightning_impact",
            BoneOrbitGlow = "light_bone_orbit_glow", BoneOrbitImpact = "light_bone_orbit_impact", MarrowLaunchFlight = "light_marrow_launch_flight",
            SoulSpearsFlight = "light_soul_spears_flight", SoulSpearsMuzzle = "light_soul_spears_muzzle", SoulSpearsImpact = "light_soul_spears_impact",
            CurseRuneImpact = "light_curse_rune_impact", GravePoolImpact = "light_grave_pool_impact";
        public static readonly string[] All = { FireballFlight, FireballMuzzle, FireballImpact, HellfirePillarImpact, FrostCleaveImpact, StormRingImpact, ChainLightningImpact, BoneOrbitGlow, BoneOrbitImpact, MarrowLaunchFlight, SoulSpearsFlight, SoulSpearsMuzzle, SoulSpearsImpact, CurseRuneImpact, GravePoolImpact };
    }

    /// <summary>
    /// v2.14.0 WIP. Skill LIGHT FX: element-coloured pixel glow under skill visuals. Light only: no hit logic.
    /// Glow (follows a projectile / aura), Flash (one-shot impact: 2-frame hold, then 3 decay frames), Muzzle (one-shot cast burst along dir).
    /// World space, 16 PPU, 1 px grid, pooled, int handles (-1 = pool full; every call accepts -1), one mesh, no per-frame allocation after Prewarm.
    /// Pixels are opaque Endesga 32 colours; "light" falloff = ordered (Bayer 4x4) dither density, not alpha. Dark #181425 outline + bright rim on the flash ring only.
    /// NOT for basic attacks, Skeleton Warrior melee or Skeleton Archer arrows.
    /// </summary>
    public static class SkillLightFX
    {
        static SkillLightRunner R => SkillLightRunner.I;
        public static int SortingOrder = 60; public static string SortingLayer = "Default"; public static Material MaterialOverride;
        public static bool Paused;
        /// <summary>Global light strength 0..1 (scales dither density). Accessibility / perf knob.</summary>
        public static float Intensity = 1f;
        public static float FlashHoldS = 2f / 30f, FlashDecayS = 3f / 30f, MuzzleS = 3f / 30f, GlowFadeS = 0.15f;

        public static void Prewarm(int items = 128) => R.Prewarm(items);
        /// <summary>Persistent glow (projectile flight, aura, orbit, zone). Move it each frame with GlowMove; end with GlowEnd (fades GlowFadeS).</summary>
        public static int Glow(LightElement e, Vector2 posU, float radiusU = 0.75f) => R.Add(0, e, posU, Vector2.right, radiusU);
        public static void GlowMove(int h, Vector2 posU) => R.Move(h, posU);
        public static void GlowRadius(int h, float radiusU) => R.Radius(h, radiusU);
        public static void GlowEnd(int h) => R.End(h);
        /// <summary>One-shot impact flash (2-frame hold + decay, 0.17 s).</summary>
        public static int Flash(LightElement e, Vector2 posU, float radiusU = 1f) => R.Add(1, e, posU, Vector2.right, radiusU);
        /// <summary>One-shot cast / muzzle burst (0.1 s) pointing along dirU.</summary>
        public static int Muzzle(LightElement e, Vector2 posU, Vector2 dirU, float radiusU = 0.5f) => R.Add(2, e, posU, dirU.sqrMagnitude > 1e-6f ? dirU.normalized : Vector2.right, radiusU);
        /// <summary>HM entry point: fire a light by hook name (see <see cref="SkillLightHooks"/>). *_flight / *_glow return a persistent glow handle
        /// (drive it with GlowMove each frame and GlowEnd when the projectile dies); *_muzzle / *_impact are one-shots. Unknown name = -1. scale multiplies the default radius.</summary>
        public static int Hook(string hook, Vector2 posU, Vector2 dirU = default, float scale = 1f)
        {
            switch (hook)
            {
                case SkillLightHooks.FireballFlight: return Glow(LightElement.Fire, posU, 0.75f * scale);
                case SkillLightHooks.FireballMuzzle: return Muzzle(LightElement.Fire, posU, dirU, 0.5f * scale);
                case SkillLightHooks.FireballImpact: return Flash(LightElement.Fire, posU, 1.25f * scale);
                case SkillLightHooks.HellfirePillarImpact: return Flash(LightElement.Fire, posU, 1.5f * scale);
                case SkillLightHooks.FrostCleaveImpact: return Flash(LightElement.Ice, posU, 1.0f * scale);
                case SkillLightHooks.StormRingImpact: return Flash(LightElement.Lightning, posU, 0.75f * scale);
                case SkillLightHooks.ChainLightningImpact: return Flash(LightElement.Lightning, posU, 0.9f * scale);
                case SkillLightHooks.BoneOrbitGlow: return Glow(LightElement.Bone, posU, 0.6f * scale);
                case SkillLightHooks.BoneOrbitImpact: return Flash(LightElement.Bone, posU, 0.75f * scale);
                case SkillLightHooks.MarrowLaunchFlight: return Glow(LightElement.Bone, posU, 0.6f * scale);
                case SkillLightHooks.SoulSpearsFlight: return Glow(LightElement.Soul, posU, 0.7f * scale);
                case SkillLightHooks.SoulSpearsMuzzle: return Muzzle(LightElement.Soul, posU, dirU, 0.5f * scale);
                case SkillLightHooks.SoulSpearsImpact: return Flash(LightElement.Soul, posU, 0.9f * scale);
                case SkillLightHooks.CurseRuneImpact: return Flash(LightElement.Curse, posU, 1.5f * scale);
                case SkillLightHooks.GravePoolImpact: return Flash(LightElement.Grave, posU, 1.0f * scale);
                default: return -1;
            }
        }
        public static void Cancel(int h) { if (SkillLightRunner.Exists) R.Cancel(h); }
        public static void Clear() { if (SkillLightRunner.Exists) R.ClearAll(); }
        public static int ActiveCount => SkillLightRunner.Exists ? R.Count : 0;

        /// <summary>Palette per element: [0] core, [1] mid, [2] outer, [3] spark.</summary>
        public static readonly Color32[][] Palette =
        {
            new[] { C(0xfee761), C(0xfeae34), C(0xf77622), C(0xffffff) }, // Fire: warm
            new[] { C(0xffffff), C(0x2ce8f5), C(0xc0cbdc), C(0xffffff) }, // Ice: cool
            new[] { C(0xffffff), C(0x0099db), C(0x124e89), C(0x2ce8f5) }, // Lightning: electric
            new[] { C(0xffffff), C(0xead4aa), C(0xc28569), C(0xffffff) }, // Bone: bone tint
            new[] { C(0xffffff), C(0x63c74d), C(0x3e8948), C(0xead4aa) }, // Soul: soul green-white
            new[] { C(0xffffff), C(0xb55088), C(0x68386c), C(0xc0cbdc) }, // Curse: necro purple
            new[] { C(0xead4aa), C(0x3e8948), C(0x193c3e), C(0x63c74d) }, // Grave: deep grave green
            new[] { C(0xffffff), C(0xc0cbdc), C(0x8b9bb4), C(0xfee761) }, // Holy: white
        };
        public static readonly Color32 Outline = C(0x181425);
        static Color32 C(int h) => new Color32((byte)(h >> 16), (byte)(h >> 8), (byte)h, 255);
    }

    public class SkillLightRunner : MonoBehaviour
    {
        static SkillLightRunner _i;
        public static bool Exists => _i != null;
        public static SkillLightRunner I { get { if (_i == null) { var go = new GameObject("SkillLightFX"); DontDestroyOnLoad(go); _i = go.AddComponent<SkillLightRunner>(); } return _i; } }

        struct It { public int kind; public LightElement e; public Vector2 p, d; public float r, age, end; public ushort gen; public bool on; }
        It[] _it = new It[0]; int[] _free; int _freeN; public int Count { get; private set; }
        const int MaxPx = 24000; int _n, _last;
        Mesh _m; MeshRenderer _mr; Vector3[] _v; Color32[] _c;
        static readonly int[] Bayer = { 0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5 };

        public void Prewarm(int cap)
        {
            cap = Mathf.Clamp(cap, 8, 1024);
            if (_it.Length < cap)
            {
                var n = new It[cap]; Array.Copy(_it, n, _it.Length);
                _free = new int[cap]; _freeN = 0; for (int i = cap - 1; i >= 0; i--) if (i >= _it.Length || !_it[i].on) _free[_freeN++] = i; _it = n;
            }
            if (_m != null) return;
            var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, hideFlags = HideFlags.DontSave };
            var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = new Color32(255, 255, 255, 255); tex.SetPixels32(px); tex.Apply(false, true);
            var m = SkillLightFX.MaterialOverride;
            if (m == null) { var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"); if (sh == null) sh = Shader.Find("Sprites/Default"); m = sh != null ? new Material(sh) { name = "SkillLightFX (Shared)", hideFlags = HideFlags.DontSave } : null; }
            if (m != null) m.mainTexture = tex; else Debug.LogWarning("[PixelFX] SkillLightFX: no sprite shader; set SkillLightFX.MaterialOverride.");
            var go = new GameObject("SkillLightFX Mesh"); go.transform.SetParent(transform, false);
            _v = new Vector3[MaxPx * 4]; _c = new Color32[MaxPx * 4]; var uv = new Vector2[MaxPx * 4]; var tri = new int[MaxPx * 6];
            for (int i = 0; i < MaxPx; i++) { int a = i * 4, t = i * 6; tri[t] = a; tri[t + 1] = a + 1; tri[t + 2] = a + 2; tri[t + 3] = a; tri[t + 4] = a + 2; tri[t + 5] = a + 3; for (int k = 0; k < 4; k++) uv[a + k] = new Vector2(.5f, .5f); }
            _m = new Mesh { name = "SkillLightFX" }; _m.MarkDynamic(); _m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            go.AddComponent<MeshFilter>().sharedMesh = _m; _mr = go.AddComponent<MeshRenderer>(); if (m != null) _mr.sharedMaterial = m;
            _mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; _mr.receiveShadows = false;
            _mr.sortingOrder = SkillLightFX.SortingOrder; _mr.sortingLayerName = SkillLightFX.SortingLayer;
            _m.vertices = _v; _m.uv = uv; _m.colors32 = _c; _m.triangles = tri; _m.bounds = new Bounds(Vector3.zero, Vector3.one * 100000f);
        }

        int Hd(int i) => i | (_it[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xFFFF; return h >= 0 && i < _it.Length && _it[i].on && _it[i].gen == (ushort)(h >> 16); }
        void Free(int i) { _it[i].on = false; _free[_freeN++] = i; Count--; }

        public int Add(int kind, LightElement e, Vector2 pU, Vector2 d, float rU)
        {
            if (_m == null) Prewarm(128); if (_freeN == 0) return -1;
            int i = _free[--_freeN]; ushort g = (ushort)((_it[i].gen + 1) & 0x7FFF); Count++;
            _it[i] = new It { kind = kind, e = e, p = pU * HellFX.PPU, d = d, r = Mathf.Max(1f, rU * HellFX.PPU), end = -1, gen = g, on = true };
            return Hd(i);
        }
        public void Move(int h, Vector2 pU) { if (Get(h, out int i)) _it[i].p = pU * HellFX.PPU; }
        public void Radius(int h, float rU) { if (Get(h, out int i)) _it[i].r = Mathf.Max(1f, rU * HellFX.PPU); }
        public void End(int h) { if (Get(h, out int i) && _it[i].end < 0) _it[i].end = _it[i].age; }
        public void Cancel(int h) { if (Get(h, out int i)) Free(i); }
        public void ClearAll() { for (int i = 0; i < _it.Length; i++) if (_it[i].on) Free(i); }

        void Px(int x, int y, Color32 c)
        {
            if (_n >= MaxPx) return; float s = 1f / HellFX.PPU, x0 = x * s, y0 = y * s; int a = _n * 4;
            _v[a] = new Vector3(x0, y0); _v[a + 1] = new Vector3(x0, y0 + s); _v[a + 2] = new Vector3(x0 + s, y0 + s); _v[a + 3] = new Vector3(x0 + s, y0);
            _c[a] = _c[a + 1] = _c[a + 2] = _c[a + 3] = c; _n++;
        }
        static bool Dither(int x, int y, float density) => Bayer[(x & 3) + ((y & 3) << 2)] < density * 16f;

        /// <summary>Dithered radial light: core solid (q<0.3), mid 62 %, outer 40 % -> 0 at the rim. strength 0..1.</summary>
        void Disc(int cx, int cy, float r, Color32[] pal, float strength, int seed, bool flicker)
        {
            float k = strength * Mathf.Clamp01(SkillLightFX.Intensity); if (k <= 0.01f) return;
            if (flicker) r += ((seed >> 1) & 1) == 0 ? 0f : -1f;
            int R = Mathf.CeilToInt(r);
            for (int y = -R; y <= R; y++) for (int x = -R; x <= R; x++)
            {
                float q = Mathf.Sqrt(x * x + y * y) / r; if (q > 1f) continue;
                int wx = cx + x, wy = cy + y;
                if (q < 0.3f) { if (k > 0.5f) Px(wx, wy, pal[0]); else if (Dither(wx, wy, k)) Px(wx, wy, pal[1]); }
                else if (q < 0.55f) { if (Dither(wx, wy, 0.62f * k)) Px(wx, wy, pal[1]); }
                else if (Dither(wx, wy, 0.4f * k * Mathf.Min(1f, (1f - q) / 0.45f * 1.4f))) Px(wx, wy, pal[2]);
            }
        }

        void LateUpdate()
        {
            if (_m == null) return; float dt = SkillLightFX.Paused ? 0f : Time.deltaTime; _n = 0;
            for (int i = 0; i < _it.Length; i++)
            {
                if (!_it[i].on) continue; ref var k = ref _it[i]; k.age += dt;
                var pal = SkillLightFX.Palette[(int)k.e]; int cx = Mathf.RoundToInt(k.p.x), cy = Mathf.RoundToInt(k.p.y); int f30 = (int)(k.age * 30f);
                if (k.kind == 0)
                {
                    float s = 1f; if (k.end >= 0) { s = 1f - (k.age - k.end) / SkillLightFX.GlowFadeS; if (s <= 0) { Free(i); continue; } }
                    if (k.e == LightElement.Soul || k.e == LightElement.Grave) s *= 0.8f + 0.2f * Mathf.Sin(k.age * 6f);
                    Disc(cx, cy, k.r, pal, s, f30 / 2 + i, k.e == LightElement.Fire);
                    if (k.e == LightElement.Lightning && (f30 + i) % 3 == 0) { int a = (f30 * 37 + i * 11) & 7; float ang = a * 0.785f; Px(cx + Mathf.RoundToInt(Mathf.Cos(ang) * k.r * 0.8f), cy + Mathf.RoundToInt(Mathf.Sin(ang) * k.r * 0.8f), pal[3]); }
                    if (k.e == LightElement.Holy) { int L = Mathf.RoundToInt(k.r * 0.9f); for (int t = 2; t <= L; t += 2) { Px(cx + t, cy, pal[1]); Px(cx - t, cy, pal[1]); Px(cx, cy + t, pal[1]); Px(cx, cy - t, pal[1]); } }
                    if (k.e == LightElement.Ice && (f30 & 15) == 0) Px(cx + Mathf.RoundToInt(k.r * 0.5f), cy + Mathf.RoundToInt(k.r * 0.4f), pal[3]);
                }
                else if (k.kind == 1)
                {
                    float hold = SkillLightFX.FlashHoldS, tot = hold + SkillLightFX.FlashDecayS; if (k.age >= tot) { Free(i); continue; }
                    if (k.age < hold) { Disc(cx, cy, k.r * 0.7f, pal, 1f, 0, false); Ring(cx, cy, k.r * 0.75f, pal[0]); }
                    else { float u = (k.age - hold) / SkillLightFX.FlashDecayS; Disc(cx, cy, k.r, pal, 0.8f * (1f - u), 0, false); if (u < 0.4f) Ring(cx, cy, k.r, pal[1]); }
                }
                else
                {
                    if (k.age >= SkillLightFX.MuzzleS) { Free(i); continue; }
                    float u = k.age / SkillLightFX.MuzzleS; Vector2 c = k.p + k.d * k.r * (0.5f + u);
                    Disc(Mathf.RoundToInt(c.x), Mathf.RoundToInt(c.y), k.r * (1f + u), pal, 1f - u, 0, false);
                }
            }
            for (int z = _n; z < _last; z++) { int a = z * 4; _v[a] = _v[a + 1] = _v[a + 2] = _v[a + 3] = Vector3.zero; }
            _last = _n; _m.vertices = _v; _m.colors32 = _c;
        }

        /// <summary>1 px bright rim with a 1 px dark #181425 outline outside it (flash ring only).</summary>
        void Ring(int cx, int cy, float r, Color32 c)
        {
            int n = Mathf.Max(12, Mathf.RoundToInt(r * 6.3f));
            for (int j = 0; j < n; j++) { float a = j * 6.2832f / n, co = Mathf.Cos(a), si = Mathf.Sin(a);
                Px(cx + Mathf.RoundToInt(co * (r + 1)), cy + Mathf.RoundToInt(si * (r + 1)), SkillLightFX.Outline);
                Px(cx + Mathf.RoundToInt(co * r), cy + Mathf.RoundToInt(si * r), c); }
        }
    }
}
