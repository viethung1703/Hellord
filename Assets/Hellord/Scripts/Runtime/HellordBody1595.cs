using System.Collections.Generic;
using UnityEngine;
namespace Hellord
{
    /// <summary>t1595 BODY hitbox. Every demon keeps its feet circle (pos, radius) for movement, separation and walking collision, and ground skills keep
    /// their old tests. Aimed airborne projectiles test a separate body box instead: x = pos.x +- w/2, y = pos.y .. pos.y + h (feet to head, sized from the
    /// demon's walk frames, HellordTuning.bodySizeU1595), against the projectile's DRAWN position (circle of radius r swept along its drawn segment).
    /// HellordTuning.bodyHitbox1595 off = every switched site runs its old test (feet-only fallback).</summary>
    public partial class HellordGame
    {
        public static int BodyTests1595, BodyHits1595;
        static bool BodyOn1595 => HellordTuning.I != null && HellordTuning.I.bodyHitbox1595;
        /// <summary>body box size (u): width, height above the feet</summary>
        public Vector2 BodySize1595(Enemy e)
        {
            var T = HellordTuning.I; Vector2 wh = Vector2.zero;
            if (e.boss) wh = new Vector2(BossRadius * 2f, BossHeadY);
            else if (!e.nest && T.bodySizeU1595 != null && (int)e.type < T.bodySizeU1595.Length) wh = T.bodySizeU1595[(int)e.type];
            if (wh.x <= 0f) wh.x = Mathf.Max(0.3f, e.radius * 2f); if (wh.y <= 0f) wh.y = Mathf.Max(0.5f, e.hitY * 2f);
            float sc = !e.boss && EnemyArtMap.Map.TryGetValue(e.type, out var en) ? en.scale : 1f;
            return new Vector2(wh.x * sc * T.bodyWMul1595, wh.y * sc * T.bodyHMul1595);
        }
        /// <summary>body box min corner and max corner (world, drawn space)</summary>
        public void BodyBox1595(Enemy e, out Vector2 lo, out Vector2 hi) { var s = BodySize1595(e); lo = new Vector2(e.pos.x - s.x * 0.5f, e.pos.y); hi = new Vector2(e.pos.x + s.x * 0.5f, e.pos.y + s.y); }
        public Vector2 BodyCentre1595(Enemy e) { var s = BodySize1595(e); return e.pos + Vector2.up * (s.y * 0.5f); }
        static float PtBox1595(Vector2 p, Vector2 lo, Vector2 hi) { float dx = Mathf.Max(lo.x - p.x, 0f, p.x - hi.x), dy = Mathf.Max(lo.y - p.y, 0f, p.y - hi.y); return Mathf.Sqrt(dx * dx + dy * dy); }
        static float PtSeg1595(Vector2 p, Vector2 a, Vector2 b) { Vector2 ab = b - a; float l = ab.sqrMagnitude; float k = l < 1e-8f ? 0f : Mathf.Clamp01(Vector2.Dot(p - a, ab) / l); return (a + ab * k - p).magnitude; }
        /// <summary>segment a-b crosses the box (Liang-Barsky)</summary>
        static bool SegCrossBox1595(Vector2 a, Vector2 b, Vector2 lo, Vector2 hi)
        {
            float t0 = 0f, t1 = 1f; Vector2 d = b - a;
            for (int ax = 0; ax < 2; ax++)
            {
                float p = ax == 0 ? d.x : d.y, q0 = ax == 0 ? a.x - lo.x : a.y - lo.y, q1 = ax == 0 ? hi.x - a.x : hi.y - a.y;
                if (Mathf.Abs(p) < 1e-8f) { if (q0 < 0f || q1 < 0f) return false; continue; }
                float u0 = -q0 / p, u1 = q1 / p; if (u0 > u1) { float tmp = u0; u0 = u1; u1 = tmp; }
                t0 = Mathf.Max(t0, u0); t1 = Mathf.Min(t1, u1); if (t0 > t1) return false;
            }
            return true;
        }
        /// <summary>shortest distance between the drawn segment a-b and the body box (0 = touching / inside)</summary>
        public static float SegBoxDist1595(Vector2 a, Vector2 b, Vector2 lo, Vector2 hi)
        {
            if (SegCrossBox1595(a, b, lo, hi)) return 0f;
            float d = Mathf.Min(PtBox1595(a, lo, hi), PtBox1595(b, lo, hi));
            d = Mathf.Min(d, PtSeg1595(lo, a, b)); d = Mathf.Min(d, PtSeg1595(hi, a, b)); d = Mathf.Min(d, PtSeg1595(new Vector2(lo.x, hi.y), a, b)); d = Mathf.Min(d, PtSeg1595(new Vector2(hi.x, lo.y), a, b));
            return d;
        }
        /// <summary>circle of radius r swept along the DRAWN segment a-b touches the demon's body box</summary>
        public bool BodyHit1595(Enemy e, Vector2 a, Vector2 b, float r) { BodyBox1595(e, out var lo, out var hi); BodyTests1595++; bool h = SegBoxDist1595(a, b, lo, hi) <= r; if (h) BodyHits1595++; return h; }
        /// <summary>alive demons whose body box touches the swept drawn segment a-b (radius r). Broad phase on the feet grid (a body is above its feet).</summary>
        public void QueryBody1595(Vector2 a, Vector2 b, float r, List<int> outList)
        {
            outList.Clear(); var T = HellordTuning.I; float mw = 1.5f * T.bodyWMul1595, mh = 4.0f * T.bodyHMul1595; // biggest body (boss 2.4 x 3.6) + margin
            int x0 = Mathf.FloorToInt((Mathf.Min(a.x, b.x) - r - mw) / Cell), x1 = Mathf.FloorToInt((Mathf.Max(a.x, b.x) + r + mw) / Cell);
            int y0 = Mathf.FloorToInt((Mathf.Min(a.y, b.y) - r - mh) / Cell), y1 = Mathf.FloorToInt((Mathf.Max(a.y, b.y) + r + 0.1f) / Cell);
            if ((x1 - x0 + 1) * (y1 - y0 + 1) > 600) { for (int i = 0; i < maxEnemies; i++) { var e = _en[i]; if (e != null && e.alive && BodyHit1595(e, a, b, r)) outList.Add(i); } return; }
            for (int cx = x0; cx <= x1; cx++) for (int cy = y0; cy <= y1; cy++)
                for (int i = _head[Bucket(cx, cy)]; i >= 0; i = _next[i])
                {
                    var e = _en[i]; if (!e.alive || e.cx != cx || e.cy != cy) continue;
                    if (BodyHit1595(e, a, b, r)) outList.Add(i);
                }
        }
        // debug overlay (HellordTuning.bodyDebug1595): feet circle green, body box magenta, projectile hit circles yellow; drawn over the actors
        const int BodyDbgOrder1595 = 32000;
        static Sprite _dbgPx1595; static Sprite DbgPx1595 => _dbgPx1595 != null ? _dbgPx1595 : (_dbgPx1595 = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f * HArt.PPU, 0, SpriteMeshType.FullRect)); // centred 1 art px
        void BodyDbgSeg1595(Vector2 a, Vector2 b, Color c) { Vector2 d = b - a; float len = d.magnitude; if (len < 0.01f) return; var sr = Z37SR(DbgPx1595, (a + b) * 0.5f, new Vector2(len * HArt.PPU + 1f, 1f), Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, c, 0); sr.sortingOrder = BodyDbgOrder1595; }
        void BodyDbgCircle1595(Vector2 c, float r, Color col) { const int n = 20; for (int i = 0; i < n; i++) { float a0 = i * Mathf.PI * 2f / n, a1 = (i + 1) * Mathf.PI * 2f / n; BodyDbgSeg1595(c + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * r, c + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * r, col); } }
        void BodyDbgBox1595(Vector2 lo, Vector2 hi, Color c) { BodyDbgSeg1595(lo, new Vector2(hi.x, lo.y), c); BodyDbgSeg1595(new Vector2(hi.x, lo.y), hi, c); BodyDbgSeg1595(hi, new Vector2(lo.x, hi.y), c); BodyDbgSeg1595(new Vector2(lo.x, hi.y), lo, c); }
        void BodyDebug1595()
        {
            var T = HellordTuning.I; if (T == null || !T.bodyDebug1595 || mode != Mode.Play || _en == null) return;
            var feet = new Color(0.2f, 1f, 0.3f, 0.9f); var body = new Color(1f, 0.25f, 0.95f, 0.95f); var pr = new Color(1f, 0.95f, 0.2f, 0.95f);
            for (int i = 0; i < maxEnemies; i++) { var e = _en[i]; if (e == null || !e.alive || e.dying) continue; BodyDbgCircle1595(e.pos, e.radius, feet); BodyBox1595(e, out var lo, out var hi); BodyDbgBox1595(lo, hi, body); }
            BodyDbgProj1595(pr);
        }
        /// <summary>t1597: lightning ground glow keys are skipped (never loaded or drawn) while lightningGroundGlow1597 is off</summary>
        static bool LtGroundOff1597(string key)
        {
            var T = HellordTuning.I; if (T == null || T.lightningGroundGlow1597 || T.lightningGroundKeys1597 == null) return false;
            foreach (var k in T.lightningGroundKeys1597) if (k == key) return true; return false;
        }
    }
}
