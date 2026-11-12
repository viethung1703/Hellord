using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

// t986u / t990: VH-locked sample_v7 Frost Cleave (fx_audit/frost_cleave_wire). Behind HellordTuning.frostCleaveShader990.
// Shader draws wind-up blade (Mode 0, off by default) + smear crescent (Mode 1). Particles per PARTICLES.md stay CPU.
namespace Hellord
{
    public class FrostCleave990 : MonoBehaviour
    {
        struct Swing
        {
            public Vector2 origin; // world u = hero + (0, 14/16)
            public float R, a0, a1, span; // R px, image-space radians
            public float t; public bool hitStop; public int stage;
            public SpriteRenderer sr; public Material mat;
        }
        static FrostCleave990 _i;
        static Material _proto; static Sprite _quad; static bool _tried, _ok;
        readonly List<Swing> _sw = new List<Swing>(4);
        static readonly float[] StageDur = { 2f / 30f, 4f / 30f, 1f / 30f, 1f / 30f }; // S1,S2,S2+1,S2+2 (no hit-stop)
        static readonly float[] StageDurHit = { 3f / 30f, 7f / 30f, 1f / 30f, 1f / 30f }; // +1 / +3 hold frames

        public static bool Ok
        {
            get
            {
                if (!HellordTuning.I.frostCleaveShader990) return false;
                if (!_tried)
                {
                    _tried = true;
                    var sh = Resources.Load<Shader>("HellordShaders/HellordPixelFrostCleave");
                    if (sh == null) sh = Shader.Find("Hellord/PixelFrostCleave");
                    if (sh == null || !sh.isSupported) { Debug.LogWarning("[t990] PixelFrostCleave missing/unsupported"); return false; }
                    _proto = new Material(sh) { name = "FrostCleave990" };
                    var ramp = Resources.Load<Texture2D>("HellordFX928/fx990_frostcleave_ramp");
                    var gaps = Resources.Load<Texture2D>("HellordFX928/fx990_frostcleave_gaps");
                    if (ramp == null || gaps == null) { Debug.LogWarning("[t990] frostcleave textures missing"); return false; }
                    ramp.filterMode = FilterMode.Point; ramp.wrapMode = TextureWrapMode.Clamp;
                    gaps.filterMode = FilterMode.Point; gaps.wrapMode = TextureWrapMode.Clamp;
                    _proto.SetTexture("_RampTex", ramp); _proto.SetFloat("_RampCount", 6);
                    _proto.SetTexture("_GapTex", gaps); _proto.SetFloat("_Sq", 0.72f); _proto.SetFloat("_PPU", 16);
                    var tx = new Texture2D(1, 1, TextureFormat.RGBA32, false); tx.SetPixel(0, 0, Color.white); tx.Apply(false, true);
                    _quad = Sprite.Create(tx, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f, 0, SpriteMeshType.FullRect);
                    _ok = true; Debug.Log("[t990] PixelFrostCleave ready");
                }
                return _ok && _proto != null;
            }
        }

        /// <summary>Spawn a smear. c = hero feet/center used by the cast; r = reach in world u; half = half-angle deg; clockwise matches FrostArcFX.</summary>
        public static void Spawn(Vector2 c, Vector2 dir, float r, float half, bool clockwise, bool anyHit)
        {
            if (!Ok) return;
            if (_i == null) { var go = new GameObject("FrostCleave990"); DontDestroyOnLoad(go); _i = go.AddComponent<FrostCleave990>(); }
            float ppu = 16f;
            // Origin = mock O = HC - 14 px image-y-down = hero + (0, +14/16) in world y-up
            Vector2 origin = c + new Vector2(0f, 14f / ppu);
            float R = r * ppu;
            float aim = -Mathf.Atan2(dir.y, dir.x); // image radians
            float halfRad = half >= 179f ? Mathf.PI : half * Mathf.Deg2Rad;
            float span = half >= 179f ? Mathf.PI * 2f : halfRad * 2f;
            // mock: A0 = aim - half, A1 = aim + half (clockwise on screen). Mirror for dir.x < 0 (= !clockwise when clockwise=dir.x>=0)
            float a0, a1;
            if (clockwise) { a0 = aim - halfRad; a1 = aim + halfRad; }
            else { a0 = aim + halfRad; a1 = aim - halfRad; span = -span; }

            var mat = new Material(_proto);
            var sgo = new GameObject("fc990"); sgo.transform.SetParent(_i.transform, false);
            var sr = sgo.AddComponent<SpriteRenderer>();
            sr.sprite = _quad; sr.sharedMaterial = mat; sr.sortingOrder = HellordTuning.I.frostCleaveOrder990;
            // Quad size: 2*(R+5+30) px
            float halfPx = R + 35f; float side = (halfPx * 2f) / ppu;
            sgo.transform.position = origin; sgo.transform.localScale = new Vector3(side, side * 0.72f + 0.5f, 1);

            var sw = new Swing { origin = origin, R = R, a0 = a0, a1 = a1, span = span, t = 0, hitStop = anyHit, stage = 0, sr = sr, mat = mat };
            ApplyStage(ref sw, 0);
            if (_i._sw.Count >= 4) { var old = _i._sw[0]; if (old.sr) Object.Destroy(old.sr.gameObject); if (old.mat) Object.Destroy(old.mat); _i._sw.RemoveAt(0); }
            _i._sw.Add(sw);
            HFX.CleaveOk++;

            // PARTICLES.md: tip shards + mist deferred to stage S2+1 inside Update; soft hit glow now if anyHit
            if (anyHit)
            {
                var T = HellordTuning.I;
                // soft round white/cyan glow stand-in (no 4-point star)
                HordeFX.CheapBurst(c + dir * (r * 0.55f) + Vector2.up * 0.2f, new Color(1f, 1f, 1f), 4);
                HordeFX.CheapBurst(c + dir * (r * 0.55f) + Vector2.up * 0.2f, new Color(0.172f, 0.91f, 0.96f), 3);
            }
        }

        static void ApplyStage(ref Swing sw, int st)
        {
            // Stage table fractions of span (mock 4.05 rad offsets / span)
            float R = sw.R, a0 = sw.a0, a1 = sw.a1, span = sw.span;
            float rout, at, al, thick, gap;
            switch (st)
            {
                case 0: rout = R; at = a0; al = a0 + 0.574f * span; thick = 22; gap = -1; break;
                case 1: rout = R + 2; at = a0 + 0.049f * span; al = a1; thick = 26; gap = -1; break;
                case 2: rout = R + 4; at = a0 + 0.494f * span; al = a1 + 0.10f * Mathf.Sign(span == 0 ? 1 : span); thick = 9; gap = 0; break;
                default: rout = R + 5; at = a1 - 0.247f * span; al = a1 + 0.15f * Mathf.Sign(span == 0 ? 1 : span); thick = 4; gap = 1; break;
            }
            // absolute overshoots for leftover/tail are in radians (SPEC: +0.10 and +0.15 absolute)
            if (st == 2) al = a1 + 0.10f * (span >= 0 ? 1 : -1);
            if (st >= 3) al = a1 + 0.15f * (span >= 0 ? 1 : -1);

            var m = sw.mat;
            m.SetFloat("_Mode", 1);
            m.SetVector("_Origin", sw.origin);
            m.SetFloat("_ROut", rout);
            m.SetFloat("_ATail", at);
            m.SetFloat("_ALead", al);
            m.SetFloat("_Thick", thick);
            m.SetFloat("_GapRow", gap);
            m.SetFloat("_Sq", 0.72f);
            m.SetFloat("_PPU", 16);
            sw.sr.enabled = true;
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime; if (dt > 0.1f) dt = 0.1f;
            for (int i = _sw.Count - 1; i >= 0; i--)
            {
                var sw = _sw[i]; sw.t += dt;
                var dur = sw.hitStop ? StageDurHit : StageDur;
                float acc = 0; int st = 0;
                for (; st < 4; st++) { acc += dur[st]; if (sw.t < acc) break; }
                if (st >= 4) { if (sw.sr) Destroy(sw.sr.gameObject); if (sw.mat) Destroy(sw.mat); _sw.RemoveAt(i); continue; }
                if (st != sw.stage)
                {
                    sw.stage = st; ApplyStage(ref sw, st);
                    // Trailing shards + mist at S2+1 (stage 2)
                    if (st == 2)
                    {
                        float ppu = 16f;
                        for (int k = 0; k < 8; k++)
                        {
                            float a = sw.a0 + 1.6f + k * 0.14f; // image rad
                            // convert image ang to world: world = -image for atan2 convention used
                            float aw = -a;
                            float rr = (sw.R + Random.Range(-6f, 6f)) / ppu;
                            Vector2 p = sw.origin + new Vector2(Mathf.Cos(aw), Mathf.Sin(aw) * 0.72f) * rr;
                            HordeFX.CheapBurst(p, k % 2 == 0 ? Color.white : new Color(0.172f, 0.91f, 0.96f), 1);
                        }
                    }
                }
                _sw[i] = sw;
            }
        }
    }
}
