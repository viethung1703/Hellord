using UnityEngine;

namespace PixelFXPack
{
    /* v2.10.1 hand-made pixel layer shared by the boss runners (IceGorillaFX, QilinChariotFX). Internal: no public API change.
       Everything is emitted as 3 px cells on the world grid through the runner's own Box / Shadow, so the backing outline,
       the 3 px minimum, opaque colours and the ground / top split are unchanged. No allocation per frame. */
    internal interface ICraftSink { void CBox(bool top, int x, int y, int w, int h, Color32 c); void CShadow(int x, int y, int w, int h, byte a); }

    internal static class Craft
    {
        public const float Two = Mathf.PI * 2f;
        public static float Hs(int a, int b) { unchecked { uint h = (uint)(a * 374761393 + b * 668265263); h = (h ^ (h >> 13)) * 1274126177u; return ((h ^ (h >> 16)) & 0xFFFF) / 65535f; } }
        static int Rnd(float v) => v >= 0 ? Mathf.FloorToInt(v + 0.5f) : -Mathf.FloorToInt(-v + 0.5f);
        public static int Q3(float v) => Mathf.FloorToInt(v / 3f + 0.5f) * 3;
        /* overshoot ease (back out) */
        public static float EaseBack(float u) { u = Mathf.Clamp01(u) - 1f; return 1f + 2.70158f * u * u * u + 1.70158f * u * u; }
        static float Mod(float a, float m) { a %= m; return a < 0 ? a + m : a; }
        /* per-angle wobble, linear between nb hashed buckets */
        static float Noise(float ang, int seed, int nb)
        {
            float f = Mod(ang, Two) / Two * nb; int i0 = Mathf.FloorToInt(f); float t = f - i0;
            return (Hs(seed, i0 % nb) - 0.5f) * (1 - t) + (Hs(seed, (i0 + 1) % nb) - 0.5f) * t;
        }

        /* Irregular shaded blob: shadow / body / highlight / rim, checker dither on the tone boundaries.
           use bits: 1 shadow, 2 highlight, 4 rim (body always). keep < 1 dissolves cells by hash. */
        public static void Blob(ICraftSink s, bool top, Vector2 c, float r, Color32 sh, Color32 body, Color32 hi, Color32 rim, int use, int seed,
            float rough = 0.22f, float sx = 1f, float sy = 1f, float keep = 1f, int kseed = 0, float lx = -0.6f, float ly = 0.8f)
        {
            if (r < 3) return; float R = r * (1 + rough) + 3;
            int x0 = Mathf.FloorToInt((c.x - R * sx) / 3f) * 3, x1 = Mathf.CeilToInt((c.x + R * sx) / 3f) * 3, y0 = Mathf.FloorToInt((c.y - R * sy) / 3f) * 3, y1 = Mathf.CeilToInt((c.y + R * sy) / 3f) * 3;
            for (int y = y0; y < y1; y += 3)
            {
                int runX = 0, runC = -1;
                for (int x = x0; x <= x1; x += 3)
                {
                    int cls = -1;
                    if (x < x1)
                    {
                        float dx = (x + 1.5f - c.x) / sx, dy = (y + 1.5f - c.y) / sy, d = Mathf.Sqrt(dx * dx + dy * dy);
                        float rr = r * (1 + 2 * rough * Noise(Mathf.Atan2(dy, dx), seed, 12));
                        if (d < rr && (keep >= 1f || Hs(kseed + x, y) < keep))
                        {
                            float f = d / Mathf.Max(rr, 1e-3f), L = (dx * lx + dy * ly) / Mathf.Max(d, 1e-3f) * Mathf.Min(1f, d / Mathf.Max(rr * 0.5f, 1e-3f));
                            bool ck = (((x / 3) + (y / 3)) & 1) == 1; /* matches the Python port for the cells we use (x,y >= 0 in practice) */
                            cls = 1;
                            if ((use & 2) != 0 && (L > 0.45f || (L > 0.28f && ck)) && f < 0.78f) cls = 2;
                            if ((use & 1) != 0 && (L < -0.35f || (L < -0.18f && ck))) cls = 0;
                            if ((use & 4) != 0 && f > 0.74f && L > 0.3f) cls = 3;
                        }
                    }
                    if (cls != runC)
                    {
                        if (runC >= 0) s.CBox(top, runX, y, x - runX, 3, runC == 0 ? sh : runC == 1 ? body : runC == 2 ? hi : rim);
                        runC = cls; runX = x;
                    }
                }
            }
        }
        /* soft-edged ground shadow with a wobbly outline (alpha only, dark) */
        public static void ShadowBlob(ICraftSink s, Vector2 c, float r, byte a, int seed, float rough = 0.2f, float sy = 0.55f, float keep = 1f)
        {
            if (r < 3) return; float R = r * (1 + rough) + 3;
            int x0 = Mathf.FloorToInt((c.x - R) / 3f) * 3, x1 = Mathf.CeilToInt((c.x + R) / 3f) * 3, y0 = Mathf.FloorToInt((c.y - R * sy) / 3f) * 3, y1 = Mathf.CeilToInt((c.y + R * sy) / 3f) * 3;
            for (int y = y0; y < y1; y += 3)
            {
                int run = int.MinValue;
                for (int x = x0; x <= x1; x += 3)
                {
                    bool m = false;
                    if (x < x1) { float dx = x + 1.5f - c.x, dy = (y + 1.5f - c.y) / sy, d = Mathf.Sqrt(dx * dx + dy * dy); m = d < r * (1 + 2 * rough * Noise(Mathf.Atan2(dy, dx), seed, 12)) && (keep >= 1f || Hs(seed + x, y) < keep); }
                    if (m && run == int.MinValue) run = x; else if (!m && run != int.MinValue) { s.CShadow(run, y, x - run, 3, a); run = int.MinValue; }
                }
            }
        }
        /* ring band with a hand-drawn wobble (same offset on both edges, thickness kept), optional broken chunks;
           breaks < 0 = dashed (every other bucket). */
        public static void RoughBand(ICraftSink s, bool top, Vector2 c, float r0, float r1, float a0, float span, Color32 col, int seed,
            float roughPx = 3f, float breaks = 0f, float keep = 1f, int kseed = 0, int nb = 24, float sy = 1f)
        {
            if (r1 <= 0) return; float R = r1 + roughPx + 3; r0 = Mathf.Max(0, r0);
            int x0 = Mathf.FloorToInt((c.x - R) / 3f) * 3, x1 = Mathf.CeilToInt((c.x + R) / 3f) * 3, y0 = Mathf.FloorToInt((c.y - R * sy) / 3f) * 3, y1 = Mathf.CeilToInt((c.y + R * sy) / 3f) * 3;
            for (int y = y0; y < y1; y += 3)
            {
                int run = int.MinValue;
                for (int x = x0; x <= x1; x += 3)
                {
                    bool m = false;
                    if (x < x1)
                    {
                        float dx = x + 1.5f - c.x, dy = (y + 1.5f - c.y) / sy, d = Mathf.Sqrt(dx * dx + dy * dy), A = Mathf.Atan2(dy, dx);
                        float off = Noise(A, seed, nb) * 2 * roughPx; m = d >= r0 + off && d < r1 + off;
                        if (m && span < Two - 1e-4f) m = Mod(A - a0, Two) <= span;
                        if (m && breaks != 0) { int b = Mathf.FloorToInt(Mod(A, Two) / Two * nb) % nb; m = breaks < 0 ? (b % 2 == 0) : Hs(seed + 77, b) >= breaks; }
                        if (m && keep < 1f) m = Hs(kseed + x, y) < keep;
                    }
                    if (m && run == int.MinValue) run = x; else if (!m && run != int.MinValue) { s.CBox(top, run, y, x - run, 3, col); run = int.MinValue; }
                }
            }
        }
        static readonly float[] TongueH = { 1f, 0.72f, 0.88f };
        /* flame tongue: tapered stack of 3 px rows that sways and changes height on a 3-frame cycle (15 fps), not a circle */
        public static void Tongue(ICraftSink s, bool top, float bx, float by, float h, float w, Color32 outer, Color32 inner, int seed, float t, float lean = 0f)
        {
            int fr = (Mathf.FloorToInt(t * 15f) + (int)(Hs(seed, 1) * 3)) % 3; int hh = Mathf.Max(6, Q3(h * TongueH[fr])), n = hh / 3;
            for (int k = 0; k < n; k++)
            {
                float u = k / (float)Mathf.Max(1, n); int wd = Mathf.Max(3, Q3(w * Mathf.Pow(1 - u, 0.75f)));
                int sw = Q3(Mathf.Sin(u * 3.1f + fr * 2.1f + seed * 0.37f) * u * w * 0.3f + lean * u * hh);
                int x0 = Rnd(bx) - wd / 2 + sw, y = Rnd(by) + k * 3;
                s.CBox(top, x0, y, wd, 3, outer); if (wd >= 9) s.CBox(top, x0 + 3, y, wd - 6, 3, inner);
            }
            if (fr == 1 && Hs(seed, 9) < 0.7f) s.CBox(top, Rnd(bx) + Q3(lean * hh) - 1, Rnd(by) + hh + 3, 3, 3, outer);
        }
        /* impact frames: frame 1 white irregular burst + star arms, frame 2 dark core with a broken white rim */
        public static void Impact(ICraftSink s, Vector2 c, float r, float t, int seed, Color32 wh, Color32 dark)
        {
            if (t < 0.034f)
            {
                Blob(s, true, c, r * 0.55f, wh, wh, wh, wh, 0, seed, 0.35f); int L = Q3(r * 1.5f), cx = Rnd(c.x) - 1, cy = Rnd(c.y) - 1;
                s.CBox(true, cx, cy, L, 3, wh); s.CBox(true, cx - L, cy, L, 3, wh); s.CBox(true, cx, cy, 3, L, wh); s.CBox(true, cx, cy - L, 3, L, wh);
            }
            else if (t < 0.067f) { RoughBand(s, true, c, r * 0.55f, r * 0.55f + 3, 0, Two, wh, seed + 3, 2f, 0.25f); Blob(s, true, c, r * 0.5f, dark, dark, dark, dark, 0, seed + 1, 0.3f); }
        }
        /* textured scorch: rough stain, radial cracks, a few cooling dots (non-hazard colours) */
        public static void DecalScorch(ICraftSink s, Vector2 c, float r, float t, float life, int seed, Color32 d0, Color32 d1)
        {
            float u = t / life; if (u >= 1) return; float keep = u < 0.5f ? 1f : 1f - (u - 0.5f) / 0.5f;
            ShadowBlob(s, c, r, 120, seed, 0.3f, 0.6f, keep);
            for (int k = 0; k < 5; k++)
            {
                float a = (k + Hs(seed, k + 20) * 0.7f) / 5f * Two, L = r * (0.7f + 0.5f * Hs(seed, k + 40));
                for (int q = 1; q <= (int)(L / 4); q++) { if (Hs(seed + k, q) >= keep) continue; int j = Q3((Hs(seed + k, q + 9) - 0.5f) * 4); s.CShadow(Rnd(c.x + Mathf.Cos(a) * q * 4) + j - 1, Q3(c.y + Mathf.Sin(a) * q * 4 * 0.6f), 3, 3, 170); }
            }
            if (u < 0.5f) for (int k = 0; k < 4; k++) { if (Hs(seed, k + 60) < u * 2) continue; s.CBox(false, Rnd(c.x + (Hs(seed, k + 70) - 0.5f) * r * 1.2f) - 1, Q3(c.y + (Hs(seed, k + 80) - 0.5f) * r * 0.6f), 3, 3, k % 2 == 0 ? d0 : d1); }
        }
        /* textured frost patch: faint base, crystal spokes with side branches, sparkle cells */
        public static void DecalFrost(ICraftSink s, Vector2 c, float r, float t, float life, int seed, Color32 spoke, Color32 sparkle)
        {
            float u = t / life; if (u >= 1) return; float keep = u < 0.5f ? 1f : 1f - (u - 0.5f) / 0.5f;
            ShadowBlob(s, c, r, 55, seed, 0.3f, 0.6f, keep);
            for (int k = 0; k < 6; k++)
            {
                float a = (k + Hs(seed, k + 20) * 0.5f) / 6f * Two, L = r * (0.6f + 0.5f * Hs(seed, k + 40)); int n = Mathf.Max(2, (int)(L / 4));
                for (int q = 1; q <= n; q++)
                {
                    if (Hs(seed + k, q) >= keep) continue; float px = c.x + Mathf.Cos(a) * q * 4, py = c.y + Mathf.Sin(a) * q * 4 * 0.6f; s.CBox(false, Rnd(px) - 1, Q3(py), 3, 3, spoke);
                    if (q == n / 2 + 1 && n >= 3) for (int sg = -1; sg <= 1; sg += 2) s.CBox(false, Rnd(px + Mathf.Cos(a + sg * 0.9f) * 5) - 1, Q3(py + Mathf.Sin(a + sg * 0.9f) * 3), 3, 3, spoke);
                }
            }
            for (int k = 0; k < 3; k++) { if (Hs(seed, k + 60) >= keep) continue; s.CBox(false, Rnd(c.x + (Hs(seed, k + 70) - 0.5f) * r) - 1, Q3(c.y + (Hs(seed, k + 80) - 0.5f) * r * 0.5f), 3, 3, sparkle); }
        }
    }

    /* Debris with arcs and gravity: chips and angular shards bounce once and settle; embers rise and sway; snow drifts.
       Fixed pool, staggered by delay, no allocation after construction. */
    internal sealed class CraftDebris
    {
        public const byte Chip = 0, Shard = 1, Ember = 2, Snow = 3;
        struct P { public byte k, land, sh, size; public float x, y, z, vx, vy, vz, age, life; public int sd; public Color32 c0, c1, c2, c3; }
        readonly P[] _p; int _n;
        public CraftDebris(int cap) { _p = new P[cap]; }
        public void Clear() { _n = 0; }
        public void Spawn(byte kind, Vector2 pos, int n, float speed, float vz, Color32 c0, Color32 c1, Color32 c2, Color32 c3, int seed, float delayStep = 0f, float life = 0.9f, int size = 1)
        {
            for (int j = 0; j < n && _n < _p.Length; j++)
            {
                float a = (j + Craft.Hs(seed, j) * 0.8f) / n * Craft.Two, v = speed * (0.55f + 0.45f * Craft.Hs(seed, j + 50));
                _p[_n++] = new P { k = kind, x = pos.x + Mathf.Cos(a) * 3, y = pos.y + Mathf.Sin(a) * 2, vx = Mathf.Cos(a) * v, vy = Mathf.Sin(a) * v * 0.6f, vz = vz * (0.6f + 0.4f * Craft.Hs(seed, j + 90)),
                    age = -j * delayStep, life = life * (0.75f + 0.25f * Craft.Hs(seed, j + 130)), sd = (seed * 31 + j) & 0xFFFF, sh = (byte)(j % 4), size = (byte)size, c0 = c0, c1 = c1, c2 = c2, c3 = c3 };
            }
        }
        public void Step(float dt)
        {
            int w = 0;
            for (int i = 0; i < _n; i++)
            {
                ref P p = ref _p[i]; p.age += dt;
                if (p.age >= 0)
                {
                    if (p.k == Ember) { p.x += p.vx * dt * 0.4f + Mathf.Sin(p.age * 7 + p.sd) * 12 * dt; p.z += (28 + p.vz * 0.2f) * dt; p.y += p.vy * dt * 0.3f; }
                    else if (p.k == Snow) { p.x += p.vx * dt + Mathf.Sin(p.age * 4 + p.sd) * 10 * dt; p.y += p.vy * dt; p.z = Mathf.Max(0, p.z + p.vz * dt); p.vz -= 60 * dt; }
                    else if (p.land < 2)
                    {
                        p.x += p.vx * dt; p.y += p.vy * dt; p.z += p.vz * dt; p.vz -= 420 * dt;
                        if (p.z < 0) { p.z = 0; p.vz = -p.vz * 0.32f; p.vx *= 0.45f; p.vy *= 0.45f; p.land++; }
                    }
                }
                if (p.age < p.life) _p[w++] = p;
            }
            _n = w;
        }
        static readonly int[] ShX = { 0, 1, 2, 0, 1, 0,   0, 1, 1, 2, 2,   0, 1, 2, 1, 3,   0, 1, 2, 3, 1, 2 };
        static readonly int[] ShY = { 0, 0, 0, 1, 1, 2,   0, 0, 1, 1, 2,   0, 1, 2, 0, 3,   0, 0, 0, 0, 1, 1 };
        static readonly int[] ShO = { 0, 6, 11, 16, 22 };
        Color32 C(ref P p, int i) { i %= 4; return i == 0 ? p.c0 : i == 1 ? p.c1 : i == 2 ? p.c2 : p.c3; }
        public void Draw(ICraftSink s)
        {
            for (int i = 0; i < _n; i++)
            {
                ref P p = ref _p[i]; if (p.age < 0) continue; float u = p.age / p.life;
                if (u > 0.65f && Craft.Hs(p.sd, 3) < (u - 0.65f) / 0.35f) continue;
                int x = p.x >= 0 ? Mathf.FloorToInt(p.x + 0.5f) : -Mathf.FloorToInt(-p.x + 0.5f), y = Craft.Q3(p.y), z = Craft.Q3(p.z);
                if ((p.k == Chip || p.k == Shard) && p.z > 2) s.CShadow(x - 1, y - 1, 3, 3, 90);
                if (p.k == Chip) s.CBox(true, x - 1, y + z, (p.sh % 2 == 0 && p.size > 1) ? 6 : 3, 3, C(ref p, p.sh));
                else if (p.k == Shard)
                {
                    int sh = p.sd % 4, rot = p.sh + (p.land == 0 ? Mathf.FloorToInt(p.age / 0.08f) : 0), n = ShO[sh + 1] - ShO[sh];
                    for (int j = 0; j < n; j++)
                    {
                        int cx = ShX[ShO[sh] + j], cy = ShY[ShO[sh] + j];
                        for (int r = 0; r < (rot & 3); r++) { int t = cx; cx = -cy; cy = t; }
                        s.CBox(true, x + cx * 3 - 1, y + z + cy * 3, 3, 3, j == 0 ? p.c0 : j < n - 1 ? p.c1 : p.c2);
                    }
                }
                else if (p.k == Ember) s.CBox(true, x - 1, y + z, 3, 3, C(ref p, (Mathf.FloorToInt(p.age * 12) + p.sd) % 3));
                else s.CBox(true, x - 1, y + z, 3, 3, C(ref p, p.sh));
            }
        }
    }
}
