using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    //  t1409u: Hellord FX v3 (RESTRAINED) ice VFX manager. Source: /workspace/fx_audit/ice_skills_v3 (draft finished + wired in).
    //  DRAWS ONLY: mechanics, timings and radii are untouched (every size comes from the live values the IceVfx* hooks receive).
    //  Shader: Resources/HellordShaders/HellordPixelIce.shader (pipeline-agnostic CGPROGRAM like the other Hellord pixel shaders: no LightMode,
    //  opaque Endesga 32 colours + clip(), integer world-pixel grid at 16 PPU, so it renders in URP 2D as SRPDefaultUnlit).
    //  Textures: Resources/HellordFX1400/ice_noise128 + ice_cell10_128 (sRGB off, bilinear, repeat, no mips, uncompressed).
    //  Switch: HellordTuning.iceShader1400 (default on). Off / missing shader = no ice shader effects (CPU particles still play).
    // =====================================================================================================================
    public class IceFX3 : MonoBehaviour
    {
        public const float PPU = 16f, SQ = 0.75f;
        public const int OrdGround = -29490, OrdSpike = -29480;   // above the Z37 decals (-29500+), below every y-sorted actor, so demons are never covered
        static IceFX3 _i; static Material _mat; static Sprite _quad; static bool _tried, _ok;
        static readonly int Mode = Shader.PropertyToID("_Mode"), Ctr = Shader.PropertyToID("_Ctr"), R_ = Shader.PropertyToID("_R"), Reach = Shader.PropertyToID("_Reach"), Melt = Shader.PropertyToID("_Melt"),
            Bright = Shader.PropertyToID("_Bright"), BareT = Shader.PropertyToID("_BareT"), Seed = Shader.PropertyToID("_Seed"), T_ = Shader.PropertyToID("_T"), Dis = Shader.PropertyToID("_Dis"), Amp = Shader.PropertyToID("_Amp"),
            HMax = Shader.PropertyToID("_HMax"), TFront = Shader.PropertyToID("_TFront"), Gap = Shader.PropertyToID("_Gap"), RIn = Shader.PropertyToID("_RIn"), Band = Shader.PropertyToID("_Band"), Dir = Shader.PropertyToID("_Dir"),
            Pop = Shader.PropertyToID("_Pop"), Foot = Shader.PropertyToID("_Foot"), Half = Shader.PropertyToID("_Half"), Lvl = Shader.PropertyToID("_Level"), SpTex = Shader.PropertyToID("_SpTex"),
            SpRect = Shader.PropertyToID("_SpRect"), SpPiv = Shader.PropertyToID("_SpPiv"), SpPos = Shader.PropertyToID("_SpPos"), SpTexSize = Shader.PropertyToID("_SpTexSize"), SpFlip = Shader.PropertyToID("_SpFlip");

        public static bool Ok
        {
            get
            {
                if (HellordTuning.I != null && !HellordTuning.I.iceShader1400) return false;
                if (!_tried)
                {
                    _tried = true;
                    var sh = Resources.Load<Shader>("HellordShaders/HellordPixelIce"); if (sh == null) sh = Shader.Find("Hellord/PixelIce");
                    var nz = Resources.Load<Texture2D>("HellordFX1400/ice_noise128"); var cl = Resources.Load<Texture2D>("HellordFX1400/ice_cell10_128");
                    if (sh == null || !sh.isSupported || nz == null || cl == null) { Debug.LogWarning("[ice3] PixelIce shader/textures missing or unsupported -> no ice shader effects"); return false; }
                    _mat = new Material(sh) { name = "IceFX3" }; _mat.SetTexture("_NoiseTex", nz); _mat.SetTexture("_CellTex", cl);
                    var tx = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point }; tx.SetPixel(0, 0, Color.white); tx.Apply(false, true);
                    _quad = Sprite.Create(tx, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f, 0, SpriteMeshType.FullRect);
                    var go = new GameObject("IceFX3"); DontDestroyOnLoad(go); _i = go.AddComponent<IceFX3>(); _ok = true;
                }
                return _ok && _mat != null && _i != null;
            }
        }

        // ---------- quad pool: per-frame quads (Take) + self-timed instances for effects that outlive their hook ----------
        class Q { public SpriteRenderer sr; public MaterialPropertyBlock mb = new MaterialPropertyBlock(); }
        readonly List<Q> _pool = new List<Q>(); int _used; int _frame = -1;
        public class Inst { public int mode; public Vector2 c; public float r, age, life, seed, amp, hmax, tfront, gap, rin, dis, reach, melt, bright, bare = .26f; public Vector4 band = new Vector4(-1, 7, 0, 0); public int order = OrdGround; public float meltAfter = -1, meltDur = 1, growFrames = 1; }
        readonly List<Inst> _inst = new List<Inst>(64);

        void BeginFrame() { if (_frame != Time.frameCount) { _frame = Time.frameCount; _used = 0; } }
        Q Take()
        {
            BeginFrame();
            if (_used >= _pool.Count) { var go = new GameObject("ice3"); go.transform.SetParent(transform, false); var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = _quad; sr.sharedMaterial = _mat; _pool.Add(new Q { sr = sr }); }
            var q = _pool[_used++]; if (!q.sr.gameObject.activeSelf) q.sr.gameObject.SetActive(true); return q;
        }
        /// <summary>Ground-style quad: half extent in px, squashed by SQ plus 2 u of headroom for the spike-lift loop.</summary>
        static void Place(Q q, Vector2 c, float halfPx, int order)
        { float w = halfPx * 2f / PPU; q.sr.transform.position = new Vector3(c.x, c.y, 0); q.sr.transform.localScale = new Vector3(w, w * SQ + 2f, 1); q.sr.sortingOrder = order; }
        static void PlaceSq(Q q, Vector2 c, float halfPx, int order)
        { float w = halfPx * 2f / PPU; q.sr.transform.position = new Vector3(c.x, c.y, 0); q.sr.transform.localScale = new Vector3(w, w, 1); q.sr.sortingOrder = order; }

        void LateUpdate()
        {
            BeginFrame(); float dt = Time.deltaTime;
            for (int i = _inst.Count - 1; i >= 0; i--) { var n = _inst[i]; n.age += dt * 30f; if (n.age >= n.life) { _inst.RemoveAt(i); continue; } Draw(n); }
            for (int k = _used; k < _pool.Count; k++) if (_pool[k].sr.gameObject.activeSelf) _pool[k].sr.gameObject.SetActive(false);   // anything not drawn this frame disappears
        }
        void Draw(Inst n)
        {
            var q = Take(); float reach = n.reach <= 0 ? n.r * Mathf.Clamp01((n.age + 1f) / n.growFrames) : n.reach;
            float melt = n.meltAfter < 0 ? 0 : Mathf.Max(0, (n.age - n.meltAfter) / n.meltDur);
            float ex = n.r * 1.25f + 8f + (n.mode == 1 ? n.hmax : 0); Place(q, n.c, ex, n.order);
            var mb = q.mb; mb.Clear(); mb.SetFloat(Mode, n.mode); mb.SetVector(Ctr, new Vector4(n.c.x, n.c.y, 0, 0)); mb.SetFloat(R_, n.r); mb.SetFloat(Reach, reach); mb.SetFloat(Melt, melt); mb.SetFloat(Bright, n.bright); mb.SetFloat(BareT, n.bare);
            mb.SetFloat(Seed, n.seed); mb.SetFloat(T_, n.age); mb.SetFloat(Dis, Mathf.Clamp01(n.dis)); mb.SetFloat(Amp, n.amp); mb.SetFloat(HMax, n.hmax); mb.SetFloat(TFront, n.tfront); mb.SetFloat(Gap, n.gap); mb.SetFloat(RIn, n.rin);
            Vector4 band = n.band; if (band.x <= -1.5f) band.x = 4f + (n.r - 4f) * Mathf.Pow(Mathf.Clamp01(n.age / 9f), .8f);   // pulse band travels to the rim in 9 f
            mb.SetVector(Band, band); if (n.mode == 1) mb.SetFloat(Dis, Mathf.Clamp01(melt)); q.sr.SetPropertyBlock(mb);
        }
        static Inst Add(Inst n) { if (_i._inst.Count < 64) _i._inst.Add(n); return n; }
        public static void Clear() { if (_i == null) return; _i._inst.Clear(); foreach (var q in _i._pool) if (q.sr != null) q.sr.gameObject.SetActive(false); _i._used = 0; }

        // =================== API used by the IceVfx* hooks ===================
        /// <summary>Frost plates that creep to rPx, then melt (plates vanish whole). mode 0. Returns the instance (bomb wind-up flicker edits .bright) or null.</summary>
        public static Inst Plates(Vector2 c, float rPx, float growFrames, float lifeFrames, float meltAfter, float meltDur, float seed, float bare = .26f)
        { if (!Ok) return null; return Add(new Inst { mode = 0, c = c, r = rPx, life = lifeFrames, seed = seed, growFrames = growFrames, meltAfter = meltAfter, meltDur = meltDur, bare = bare, order = OrdGround }); }
        /// <summary>A few small ice spikes (mode 1): rise as the front passes (tfront frames), dissolve from meltAfter over meltDur.</summary>
        public static void Columns(Vector2 c, float rPx, float hmax, float tfront, float gap, float rin, float amp, float seed, float lifeFrames, float meltAfter, float meltDur)
        { if (!Ok) return; Add(new Inst { mode = 1, c = c, r = rPx, hmax = hmax, tfront = tfront, gap = gap, rin = rin, amp = amp, seed = seed, life = lifeFrames, order = OrdSpike, meltAfter = meltAfter, meltDur = meltDur }); }
        /// <summary>Bomb pulse = a thin travelling band of small spikes (mode 1 + _Band).</summary>
        public static void PulseBand(Vector2 c, float rPx, float seed)
        { if (!Ok) return; Add(new Inst { mode = 1, c = c, r = rPx, hmax = 4, tfront = 99, gap = .88f, rin = .05f, amp = .85f, seed = seed, life = 10, order = OrdSpike, band = new Vector4(-2, 5, 0, 0) }); }

        // ---------- per-frame immediate quads ----------
        /// <summary>Frostbolt orb (mode 2). p = ground position, v = velocity (world, y up).</summary>
        public static void Orb(Vector2 p, Vector2 v, float age, bool pop, int order)
        {
            if (!Ok) return; var q = _i.Take(); Vector2 at = p + Vector2.up * 0.6f; PlaceSq(q, at, 36f, order); Vector2 d = new Vector2(v.x, -v.y).normalized;   // shader works in y-down image space
            var mb = q.mb; mb.Clear(); mb.SetFloat(Mode, 2); mb.SetVector(Ctr, new Vector4(at.x, at.y, 0, 0)); mb.SetFloat(R_, 8f); mb.SetFloat(Seed, 0.4f); mb.SetFloat(T_, age * 30f); mb.SetVector(Dir, new Vector4(d.x, d.y, 0, 0)); mb.SetFloat(Pop, pop ? 1.3f : 1f); q.sr.SetPropertyBlock(mb);
        }
        /// <summary>Diablo 2 Frozen Armor: 7 small crystals orbit the hero on a ground ellipse at 0.80-0.95 of the REAL zone radius. A crystal that would overlap the hero or a demon is skipped (blocked).</summary>
        public static void Armor(Vector2 c, float rPx, float age, float fade, System.Func<Vector2, bool> blocked)
        {
            if (!Ok || fade <= 0f) return; float up = Mathf.Clamp01(age * 30f / 15f) * (fade < 1f ? Mathf.Clamp01(fade * 2f) : 1f);
            for (int i = 0; i < 7; i++)
            {
                float a0 = i * Mathf.PI * 2f / 7f + i * .13f, w = (.055f + .012f * (i % 3)) * 30f;                                  // rad / s
                float a = a0 + age * w, sn = Mathf.Sin(a), rr = rPx * (.80f + .05f * (i % 4)) * up, hh = 8f + (i * 5) % 22 + 3f * Mathf.Sin(age * 2.7f + a0);
                Vector2 gnd = c + new Vector2(Mathf.Cos(a) * rr, -sn * rr * SQ) / PPU, pos = gnd + Vector2.up * hh / PPU;          // world y is UP
                if (blocked != null && blocked(pos)) continue;
                var q = _i.Take(); PlaceSq(q, pos, 6f, -(int)(gnd.y * PPU) - 432 + 1);                                             // y-sorted like an actor (YSort)
                var mb = q.mb; mb.Clear(); mb.SetFloat(Mode, 3); mb.SetVector(Ctr, new Vector4(pos.x, pos.y, 0, 0)); q.sr.SetPropertyBlock(mb);
            }
        }
        /// <summary>Chill / freeze (level 1 chill, 2 chill+, 3 freeze) or the hero's Frozen-Armor rim (level 4) drawn over a sprite WITHOUT covering it: dark tint of its own pixels + sparse rim + chips.</summary>
        public static void StatusSprite(Sprite sp, Vector3 spritePos, Vector2 feet, float halfPx, int level, float seed, float ageFrames, int order, bool flipX = false)
        {
            if (!Ok || sp == null || sp.texture == null) return; var q = _i.Take(); Rect rc = sp.rect; Vector2 pv = sp.pivot; const float PAD = 8f;
            Vector2 ctr = (Vector2)spritePos + new Vector2(rc.width * .5f - pv.x, rc.height * .5f - pv.y) / PPU;
            q.sr.transform.position = new Vector3(ctr.x, ctr.y, 0); q.sr.transform.localScale = new Vector3((rc.width + PAD * 2f) / PPU, (rc.height + PAD * 2f) / PPU, 1); q.sr.sortingOrder = order;
            var mb = q.mb; mb.Clear(); mb.SetFloat(Mode, 5); mb.SetFloat(Lvl, level); mb.SetFloat(Seed, seed); mb.SetFloat(T_, ageFrames); mb.SetFloat(Half, halfPx); mb.SetFloat(SpFlip, flipX ? 1f : 0f);
            mb.SetTexture(SpTex, sp.texture); mb.SetVector(SpTexSize, new Vector4(sp.texture.width, sp.texture.height, 0, 0)); mb.SetVector(SpRect, new Vector4(rc.x, rc.y, rc.width, rc.height));
            mb.SetVector(SpPiv, new Vector4(pv.x, pv.y, 0, 0)); mb.SetVector(SpPos, new Vector4(spritePos.x, spritePos.y, 0, 0)); mb.SetVector(Foot, new Vector4(feet.x, feet.y, 0, 0)); q.sr.SetPropertyBlock(mb);
        }
    }
}
