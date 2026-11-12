using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>Bullet size class. Small = 7px orb (dense patterns), Large = 11px orb (slow "wall" bullets).</summary>
    public enum BossBulletSize { Small = 0, Large = 1 }

    /// <summary>
    /// #112 Boss Bullet (v1.8.5). Enemy projectile built to read as DANGER and never as a player shot:
    /// round orb (player shots are elongated bolts), white-hot core, toxic-lime ring and a hard 1px near-black rim
    /// (PAL14 'tox' + 'mouth'), 2-frame core pulse, 2-frame spawn flash and a 3-frame despawn pop.
    /// Every bullet is one quad in ONE mesh, ONE atlas, ONE material: 1 draw call, no per-bullet GameObject and no
    /// per-frame allocation after Prewarm. Visual only - hit tests stay in game code (use GetPosition or your own sim).
    /// </summary>
    public static class BossBulletFX
    {
        /// <summary>Fired when a bullet expires by lifetime or leaves <see cref="CullRect"/> (handle, last position).</summary>
        public static Action<int, Vector2> OnExpire;
        /// <summary>World-space rect; bullets outside it despawn silently. Default: none (lifetime only).</summary>
        public static Rect? CullRect;
        static BossBulletRunner R => BossBulletRunner.I;

        /// <summary>Allocate pool, atlas, mesh and material up front (call at level load). Default capacity 256.</summary>
        public static void Prewarm(int capacity = 256) => R.Prewarm(capacity);
        /// <summary>Spawn one bullet. <paramref name="velocity"/> in world units/s (0 if your code moves it with SetPosition). Returns handle or -1 when full.</summary>
        public static int Spawn(Vector2 pos, Vector2 velocity, BossBulletSize size = BossBulletSize.Small, float lifetime = 6f, bool spawnFlash = true) => R.Spawn(pos, velocity, size, lifetime, spawnFlash);
        /// <summary>Remove a bullet. pop = play the 3-frame despawn pop (hit / blocked); false = vanish (off-screen).</summary>
        public static void Despawn(int handle, bool pop = true) => R.Despawn(handle, pop);
        public static bool IsAlive(int handle) => R.Alive(handle);
        public static Vector2 GetPosition(int handle) => R.GetPos(handle);
        public static void SetPosition(int handle, Vector2 pos) => R.SetPos(handle, pos);
        public static void SetVelocity(int handle, Vector2 velocity) => R.SetVel(handle, velocity);
        /// <summary>Despawn every live bullet (phase change / boss death). pop=true staggers nothing - all pop at once.</summary>
        public static void Clear(bool pop = true) => R.ClearAll(pop);
        public static int AliveCount => R.Count;

        /// <summary>Ring of <paramref name="count"/> bullets, evenly spaced, speed in px/s.</summary>
        public static void Ring(Vector2 pos, int count, float speedPx, float offsetDeg = 0, BossBulletSize size = BossBulletSize.Small, float lifetime = 6f)
        {
            float u = 1f / HellFX.PPU;
            for (int i = 0; i < count; i++) { float a = (offsetDeg + 360f * i / Mathf.Max(1, count)) * Mathf.Deg2Rad; Spawn(pos, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * speedPx * u, size, lifetime); }
        }
        /// <summary>Fan of <paramref name="count"/> bullets across <paramref name="arcDeg"/> around <paramref name="dir"/>.</summary>
        public static void Spread(Vector2 pos, Vector2 dir, int count, float arcDeg, float speedPx, BossBulletSize size = BossBulletSize.Small, float lifetime = 6f)
        {
            float u = 1f / HellFX.PPU, b = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            for (int i = 0; i < count; i++) { float a = (b + (count > 1 ? -arcDeg * 0.5f + arcDeg * i / (count - 1) : 0)) * Mathf.Deg2Rad; Spawn(pos, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * speedPx * u, size, lifetime); }
        }

        /// <summary>Optional material override (must sample _MainTex and multiply vertex colour, alpha blended). Set before the first Spawn/Prewarm.</summary>
        public static Material MaterialOverride;
        public static int SortingOrder = 70;
        public static string SortingLayer = "Default";

        // PAL14 ramp: rim (mouth #100010), ring (tox #40e040), inner (tox #c0ff60), core (white)
        public static readonly Color32 RimColor = new Color32(0x10, 0x00, 0x10, 255), RingColor = new Color32(0x40, 0xe0, 0x40, 255),
                                       InnerColor = new Color32(0xc0, 0xff, 0x60, 255), CoreColor = new Color32(255, 255, 255, 255);
    }

    [AddComponentMenu("")]
    public class BossBulletRunner : MonoBehaviour
    {
        static BossBulletRunner _i;
        public static BossBulletRunner I { get { if (_i == null) { var go = new GameObject("BossBulletFX"); DontDestroyOnLoad(go); _i = go.AddComponent<BossBulletRunner>(); } return _i; } }

        struct B { public Vector2 p, v; public float age, life, popT; public byte size, state; public ushort gen; public bool flash; } // state 0 free, 1 live, 2 popping
        B[] _b = new B[0]; int[] _free; int _freeN, _count;
        Mesh _mesh; Vector3[] _vtx; Vector2[] _uv; Color32[] _col; int[] _tri; MeshRenderer _mr; Texture2D _atlas;
        const int Cell = 12, Cells = 9; // 0-1 small pulse, 2-3 large pulse, 4-5 spawn flash, 6-8 pop
        const float FlashT = 0.067f, PopT = 0.1f;
        public int Count => _count;

        public void Prewarm(int cap)
        {
            if (_b.Length >= cap && _mesh != null) return;
            cap = Mathf.Max(cap, 16);
            var nb = new B[cap]; Array.Copy(_b, nb, _b.Length);
            _free = new int[cap]; _freeN = 0; for (int i = cap - 1; i >= 0; i--) if (i >= _b.Length || _b[i].state == 0) _free[_freeN++] = i;
            _b = nb;
            _vtx = new Vector3[cap * 4]; _uv = new Vector2[cap * 4]; _col = new Color32[cap * 4]; _tri = new int[cap * 6];
            for (int i = 0; i < cap; i++) { int v = i * 4, t = i * 6; _tri[t] = v; _tri[t + 1] = v + 1; _tri[t + 2] = v + 2; _tri[t + 3] = v; _tri[t + 4] = v + 2; _tri[t + 5] = v + 3; }
            for (int i = 0; i < _col.Length; i++) _col[i] = new Color32(255, 255, 255, 255);
            if (_mesh == null)
            {
                _mesh = new Mesh { name = "BossBullets" }; _mesh.MarkDynamic();
                gameObject.AddComponent<MeshFilter>().sharedMesh = _mesh;
                _mr = gameObject.AddComponent<MeshRenderer>();
                BuildAtlas();
                var m = BossBulletFX.MaterialOverride;
                if (m == null)
                {
                    var sh = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                    if (sh == null) sh = Shader.Find("Sprites/Default");
                    m = sh != null ? new Material(sh) { name = "BossBullet (Shared)", hideFlags = HideFlags.DontSave } : null;
                }
                if (m != null) { m.mainTexture = _atlas; _mr.sharedMaterial = m; } else Debug.LogWarning("[PixelFX] BossBulletFX: no sprite shader found; set BossBulletFX.MaterialOverride.");
                _mr.sortingOrder = BossBulletFX.SortingOrder; _mr.sortingLayerName = BossBulletFX.SortingLayer;
                _mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; _mr.receiveShadows = false;
            }
            _mesh.Clear();
            _mesh.vertices = _vtx; _mesh.uv = _uv; _mesh.colors32 = _col; _mesh.triangles = _tri;
        }

        void BuildAtlas()
        {
            var px = new Color32[Cell * Cells * Cell]; var clr = new Color32(0, 0, 0, 0);
            for (int i = 0; i < px.Length; i++) px[i] = clr;
            Color32 K = BossBulletFX.RimColor, G = BossBulletFX.RingColor, Y = BossBulletFX.InnerColor, W = BossBulletFX.CoreColor;
            void P(int cell, int x, int y, Color32 c) { if (x >= 0 && y >= 0 && x < Cell && y < Cell) px[y * Cell * Cells + cell * Cell + x] = c; }
            // orb: concentric discs around cell centre (5.5,5.5)
            void Orb(int cell, float r, float core, bool hot)
            {
                for (int y = 0; y < Cell; y++) for (int x = 0; x < Cell; x++)
                {
                    float d = Mathf.Sqrt((x - 5.5f) * (x - 5.5f) + (y - 5.5f) * (y - 5.5f));
                    if (d <= core) P(cell, x, y, W); else if (d <= core + 1.1f) P(cell, x, y, hot ? W : Y); else if (d <= r - 1) P(cell, x, y, G); else if (d <= r) P(cell, x, y, K);
                }
            }
            Orb(0, 3.6f, 1.0f, false); Orb(1, 3.6f, 1.0f, true);   // small 7-8px: pulse = inner ring flashes white
            Orb(2, 5.6f, 1.9f, false); Orb(3, 5.6f, 1.9f, true);   // large 11-12px
            // spawn flash: white disc then white 1px ring (frame 2 sits under the real orb)
            for (int y = 0; y < Cell; y++) for (int x = 0; x < Cell; x++)
            {
                float d = Mathf.Sqrt((x - 5.5f) * (x - 5.5f) + (y - 5.5f) * (y - 5.5f));
                if (d <= 4.6f) P(4, x, y, d > 3.6f ? Y : W);
                if (d > 4.6f && d <= 5.7f) P(5, x, y, W);
            }
            // pop: 3 frames - lime cross burst, 4 diagonal sparks out, last 4 single pixels (no ring, so it never reads as a new bullet)
            for (int k = -2; k <= 2; k++) { P(6, 5 + k, 5, W); P(6, 6 + k, 6, W); P(6, 5, 5 + k, W); P(6, 6, 6 + k, W); }
            P(6, 5, 5, Y); P(6, 6, 6, Y); P(6, 5, 6, Y); P(6, 6, 5, Y);
            int[] dx = { -1, 1, -1, 1 }, dy = { -1, -1, 1, 1 };
            for (int q = 0; q < 4; q++) { int bx = dx[q] < 0 ? 5 : 6, by = dy[q] < 0 ? 5 : 6;
                P(7, bx + dx[q] * 3, by + dy[q] * 3, W); P(7, bx + dx[q] * 2, by + dy[q] * 2, Y); P(7, bx + dx[q] * 4, by + dy[q] * 4, G);
                P(8, bx + dx[q] * 5, by + dy[q] * 5, G); }
            _atlas = new Texture2D(Cell * Cells, Cell, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "PFX BossBullet Atlas", hideFlags = HideFlags.DontSave };
            _atlas.SetPixels32(px); _atlas.Apply(false, true);
        }

        int H(int i) => i | (_b[i].gen << 16);
        bool Get(int h, out int i) { i = h & 0xFFFF; return h >= 0 && i < _b.Length && _b[i].state == 1 && _b[i].gen == (ushort)(h >> 16); }
        public bool Alive(int h) => Get(h, out _);
        public Vector2 GetPos(int h) => Get(h, out int i) ? _b[i].p : Vector2.zero;
        public void SetPos(int h, Vector2 p) { if (Get(h, out int i)) _b[i].p = p; }
        public void SetVel(int h, Vector2 v) { if (Get(h, out int i)) _b[i].v = v; }

        public int Spawn(Vector2 p, Vector2 v, BossBulletSize size, float life, bool flash)
        {
            if (_mesh == null) Prewarm(256);
            if (_freeN == 0) return -1; // full: never allocate mid-fight
            int i = _free[--_freeN]; ref var b = ref _b[i];
            b.p = p; b.v = v; b.age = 0; b.life = Mathf.Max(0.05f, life); b.size = (byte)size; b.state = 1; b.gen = (ushort)((b.gen + 1) & 0x7FFF); b.flash = flash; b.popT = 0;
            _count++; return H(i);
        }
        public void Despawn(int h, bool pop) { if (Get(h, out int i)) Kill(i, pop); }
        void Kill(int i, bool pop)
        {
            if (pop) { _b[i].state = 2; _b[i].popT = 0; }
            else { _b[i].state = 0; _free[_freeN++] = i; }
            _count--;
        }
        public void ClearAll(bool pop) { for (int i = 0; i < _b.Length; i++) if (_b[i].state == 1) Kill(i, pop); }

        void LateUpdate()
        {
            if (_mesh == null) return;
            float dt = Time.deltaTime, ppu = HellFX.PPU, u = 1f / ppu, tf = Time.time; int frame = Time.frameCount;
            var cull = BossBulletFX.CullRect; float cw = 1f / Cells;
            for (int i = 0; i < _b.Length; i++)
            {
                ref var b = ref _b[i]; int v = i * 4; int cell = -1;
                if (b.state == 1)
                {
                    b.age += dt; b.p += b.v * dt;
                    bool outside = cull.HasValue && !cull.Value.Contains(b.p);
                    if (b.age >= b.life || outside) { var h = H(i); var pos = b.p; Kill(i, !outside); BossBulletFX.OnExpire?.Invoke(h, pos); }
                    else
                    {
                        bool pulse = (((int)(tf * 10f) + i) & 1) == 1; // ~5 Hz pulse, phase per bullet so a wall shimmers instead of strobing
                        cell = b.flash && b.age < FlashT ? (b.age < FlashT * 0.5f ? 4 : 5) : (b.size == 1 ? 2 : 0) + (pulse ? 1 : 0);
                    }
                }
                if (b.state == 2)
                {
                    b.popT += dt;
                    if (b.popT >= PopT) { b.state = 0; _free[_freeN++] = i; }
                    else cell = 6 + Mathf.Min(2, (int)(b.popT / PopT * 3));
                }
                if (cell < 0) { _vtx[v] = _vtx[v + 1] = _vtx[v + 2] = _vtx[v + 3] = Vector3.zero; continue; }
                // snap centre so the 12px cell's pixel grid lines up with the world pixel grid
                float cx = Mathf.Round(b.p.x * ppu) * u, cy = Mathf.Round(b.p.y * ppu) * u, h6 = 6 * u;
                _vtx[v] = new Vector3(cx - h6, cy - h6, 0); _vtx[v + 1] = new Vector3(cx - h6, cy + h6, 0);
                _vtx[v + 2] = new Vector3(cx + h6, cy + h6, 0); _vtx[v + 3] = new Vector3(cx + h6, cy - h6, 0);
                float u0 = cell * cw, u1 = u0 + cw;
                _uv[v] = new Vector2(u0, 0); _uv[v + 1] = new Vector2(u0, 1); _uv[v + 2] = new Vector2(u1, 1); _uv[v + 3] = new Vector2(u1, 0);
            }
            _mesh.vertices = _vtx; _mesh.uv = _uv; // same array refs every frame: no GC
            _mesh.RecalculateBounds();
        }
        void OnDestroy() { if (_i == this) _i = null; }
    }
}
