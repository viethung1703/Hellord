using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    //  t1432: Frostbolt in the D2R Ice Bolt look (HFX strips, 30 fps, alpha 0/255, drawn unrotated: 16 pre-rendered directions).
    //   flight: 800x1280 = 10 frames x 16 rows of 80x80, pivot (40,40) at the head, loop 0.33 s, row r = r x 22.5 deg (0 = east, clockwise on screen)
    //   hit:    1440x72  = 20 frames of 72x72, pivot (36,36) at the enemy's chest, plays once per enemy hit / pierced
    //   cast:   400x40   = 10 frames of 40x40, pivot (20,20) at the muzzle (casting hand), plays once (optional, weak; HellordTuning.frostbolt1432Cast)
    //   glow masks (flight_glow_dir16 / hit_glow / cast_glow, same layout + pivot) are drawn additive by Hellord/FrostGlow1430 (pale ramp, built-in pipeline).
    //  Toggle: HellordTuning.frostbolt1432 (default on). Off, strips missing -> the old IceFX3 orb path. Glow shader missing / unsupported -> sprites only.
    //  Gameplay (speed, damage, hit radius, pierce) is untouched: this file only DRAWS. Integer 1x, nearest, never scaled.
    // =====================================================================================================================
    public class FrostboltFX1432 : MonoBehaviour
    {
        const string Dir = "HellordArt/IceSkills/"; const float FPS = 30f, PPU = 16f;
        static FrostboltFX1432 _i; static bool _tried, _ok;
        static Sprite[,] _fl, _flG; static Sprite[] _hit, _hitG, _cast, _castG; static Shader _sh; static bool _glowOk; static Material _mFl, _mHit, _mCast;
        // glow strength: HFX suggested 0.7 / 0.4 / 0.6 (numpy gaussian sim). The real 3 px-tap shader at those values smeared the bolt into a white blob and hid the glitter (t1432 play-check), so ~0.45x:
        public const float FlightAdd = 0.32f, HitAdd = 0.2f, CastAdd = 0.3f;
        public static bool ForceNoGlow; public static string Status = "frostbolt1432 not tried";

        public static bool Ok { get { var T = HellordTuning.I; if (T != null && !T.frostbolt1432) return false; if (!_tried) Init(); return _ok && _i != null; } }
        static Texture2D Tex(string n) { var t = Resources.Load<Texture2D>(Dir + n); if (t != null) { t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; } return t; }
        static Sprite[] Row(Texture2D t, int fw, int fh, int n, int row) { var a = new Sprite[n]; for (int c = 0; c < n; c++) a[c] = Sprite.Create(t, new Rect(c * fw, t.height - (row + 1) * fh, fw, fh), new Vector2(0.5f, 0.5f), PPU, 0, SpriteMeshType.FullRect); return a; }
        static Sprite[,] Grid(Texture2D t, int fw, int fh, int cols, int rows) { var g = new Sprite[rows, cols]; for (int r = 0; r < rows; r++) { var rw = Row(t, fw, fh, cols, r); for (int c = 0; c < cols; c++) g[r, c] = rw[c]; } return g; }
        static Material Mat(Color tint, float add) { var m = new Material(_sh) { name = "frostglow1432" }; m.SetColor("_Tint", tint); m.SetFloat("_Add", add); m.SetFloat("_Gain", 1.6f); return m; }
        static void Init()
        {
            _tried = true; var f = Tex("fx_frostbolt_flight_dir16"); if (f == null || f.width < 800 || f.height < 1280) { Status = "frostbolt1432 flight strip missing -> old visual"; return; }
            _fl = Grid(f, 80, 80, 10, 16); var fg = Tex("fx_frostbolt_flight_glow_dir16"); if (fg != null && fg.width >= 800 && fg.height >= 1280) _flG = Grid(fg, 80, 80, 10, 16);
            var h = Tex("fx_frostbolt_hit"); if (h != null && h.width >= 72 * 20) _hit = Row(h, 72, 72, 20, 0); var hg = Tex("fx_frostbolt_hit_glow"); if (hg != null && hg.width >= 72 * 20) _hitG = Row(hg, 72, 72, 20, 0);
            var c = Tex("fx_frostbolt_cast"); if (c != null && c.width >= 400) _cast = Row(c, 40, 40, 10, 0); var cg = Tex("fx_frostbolt_cast_glow"); if (cg != null && cg.width >= 400) _castG = Row(cg, 40, 40, 10, 0);
            _sh = Resources.Load<Shader>("HellordShaders/HellordFrostGlow"); if (_sh == null) _sh = Shader.Find("Hellord/FrostGlow1430");
            _glowOk = !ForceNoGlow && _flG != null && _sh != null && _sh.isSupported;
            if (_glowOk) { _mFl = Mat(new Color32(0xc0, 0xcb, 0xdc, 255), FlightAdd); _mHit = Mat(new Color32(0xaa, 0xe8, 0xf0, 255), HitAdd); _mCast = Mat(new Color32(0xc0, 0xcb, 0xdc, 255), CastAdd); }
            var go = new GameObject("FrostboltFX1432"); if (Application.isPlaying) DontDestroyOnLoad(go); _i = go.AddComponent<FrostboltFX1432>(); _ok = true;
            Status = "frostbolt1432 ok: flight dir16, hit " + (_hit != null ? "20f" : "none") + ", cast " + (_cast != null ? "10f" : "none") + ", glow " + (_glowOk ? "shader" : "OFF(" + (ForceNoGlow ? "forced" : _flG == null ? "no mask" : _sh == null ? "missing" : "unsupported") + ")");
        }

        // ------------------------------------------------------------------ instances
        sealed class Fl { public object key; public SpriteRenderer sp, gl; public bool live; public float seen; public int frame; }
        sealed class Once { public SpriteRenderer sp, gl; public bool live; public float age; public Sprite[] a, g; public Vector3 pos; public int order; }
        readonly List<Fl> _fl_ = new List<Fl>(); readonly List<Once> _once = new List<Once>();
        static SpriteRenderer NewSR(Transform p, string n) { var go = new GameObject(n); go.transform.SetParent(p, false); return go.AddComponent<SpriteRenderer>(); }
        static float Snap(float u) => Mathf.Round(u * PPU) / PPU;
        /// <summary>Row of the 16-direction strip for a velocity (world, y up): nearest 22.5 deg, 0 = east, clockwise on screen.</summary>
        public static int DirRow(Vector2 v) { float a = Mathf.Atan2(-v.y, v.x) * Mathf.Rad2Deg; if (a < 0f) a += 360f; return Mathf.RoundToInt(a / 22.5f) & 15; }

        /// <summary>EVERY FRAME per orb in flight. `age` = seconds since launch (drives the 0.33 s loop).</summary>
        public static void Orb(object key, Vector2 p, Vector2 v, float age, int order)
        {
            if (!Ok) return; Fl s = null; foreach (var x in _i._fl_) if (x.live && x.key == key) { s = x; break; }
            if (s == null) { foreach (var x in _i._fl_) if (!x.live) { s = x; break; } if (s == null) { s = new Fl { sp = NewSR(_i.transform, "fb"), gl = NewSR(_i.transform, "fbg") }; _i._fl_.Add(s); if (_glowOk) s.gl.sharedMaterial = _mFl; } s.live = true; s.key = key; }
            int row = DirRow(v), f = ((int)(age * FPS)) % 10; var pos = new Vector3(Snap(p.x), Snap(p.y), 0f); s.seen = Time.unscaledTime;
            s.sp.enabled = true; s.sp.sprite = _fl[row, f]; s.sp.transform.position = pos; s.sp.sortingOrder = order;
            if (_glowOk) { s.gl.enabled = true; s.gl.sprite = _flG[row, f]; s.gl.transform.position = pos; s.gl.sortingOrder = order + 1; s.gl.color = Color.white; } else s.gl.enabled = false;
        }
        public static void Release(object key) { if (_i == null) return; foreach (var x in _i._fl_) if (x.live && x.key == key) { HideFl(x); return; } }
        static void HideFl(Fl x) { x.live = false; x.key = null; x.sp.enabled = false; x.gl.enabled = false; }
        static void Play(Sprite[] a, Sprite[] g, Vector2 p, int order, Material m)
        {
            if (!Ok || a == null) return; Once s = null; foreach (var x in _i._once) if (!x.live) { s = x; break; }
            if (s == null) { s = new Once { sp = NewSR(_i.transform, "fh"), gl = NewSR(_i.transform, "fhg") }; _i._once.Add(s); }
            s.live = true; s.age = 0f; s.a = a; s.g = _glowOk ? g : null; s.pos = new Vector3(Snap(p.x), Snap(p.y), 0f); s.order = order; s.gl.sharedMaterial = m;
        }
        /// <summary>ONCE per enemy pierced: chest position (world), sorting order a little over the enemy.</summary>
        public static void Hit(Vector2 chest, int order) { if (Ok) Play(_hit, _hitG, chest, order, _mHit); }
        /// <summary>ONCE per cast at the casting hand. Returns false if the cast strip is not there (caller keeps the old puff).</summary>
        public static bool Cast(Vector2 muzzle, int order) { if (!Ok || _cast == null) return false; var T = HellordTuning.I; if (T != null && !T.frostbolt1432Cast) return false; Play(_cast, _castG, muzzle, order, _mCast); return true; }
        public static void Clear() { if (_i == null) return; foreach (var x in _i._fl_) HideFl(x); foreach (var x in _i._once) HideOnce(x); }
        static void HideOnce(Once x) { x.live = false; x.sp.enabled = false; x.gl.enabled = false; }
        public static bool HasHit => Ok && _hit != null;
        void LateUpdate() { Tick(Time.deltaTime); }
        public static void EditorStep(float dt) { if (Ok) _i.Tick(dt); }
        void Tick(float dt)
        {
            foreach (var x in _fl_) if (x.live && Time.unscaledTime - x.seen > 3f) HideFl(x); // safety: an orb nobody drives any more
            foreach (var s in _once)
            {
                if (!s.live) continue; s.age += dt; int n = s.a.Length, fi = (int)(s.age * FPS); if (fi >= n) { HideOnce(s); continue; }
                s.sp.enabled = true; s.sp.sprite = s.a[fi]; s.sp.transform.position = s.pos; s.sp.sortingOrder = s.order;
                if (s.g != null) { s.gl.enabled = true; s.gl.sprite = s.g[Mathf.Min(fi, s.g.Length - 1)]; s.gl.transform.position = s.pos; s.gl.sortingOrder = s.order + 1; s.gl.color = Color.white; } else s.gl.enabled = false;
            }
        }
    }
}
