using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

// v0.0.46 boss tells, owner pick pt341u: CONES and LANES (charges) use the Hellord FX design (fx_hfx_tells/SPEC.md),
// CIRCLES and RINGS use the MoneyMaker design (fx_redo/tells/SPEC.md). Colours, frames, timing and layer order follow each SPEC.
// Every tell is one texture at the game's 16 px/unit, re-rasterised each frame on the CPU, never rotated or scaled, snapped to the
// pixel grid. "Body = floor x k" (MoneyMaker) is drawn as black at alpha 1 - k, which is the same thing over any floor.
// The callers stay immediate (they pass p = 0..1 each frame). A tell that stops being drawn after p >= tellReleaseMinP46 plays its
// release (hit / hold / dissolve, ring wave, lane streak, cone smear) on its own; a tell dropped earlier (cancelled) just goes.
// Hatch and wash patterns are written in the mock's y-down pixel coordinates (Y = -world pixel y) so they lean the same way.
namespace Hellord
{
    public partial class HellordGame
    {
        const int Px46 = 16;
        bool Crisp46 => HellordTuning.I.crispTellsV46;
        bool PackHide46 => HellordTuning.I.crispTellsV46 && HellordTuning.I.packTellsHideV46;
        static Color32 C46(int h) => new Color32((byte)((h >> 16) & 255), (byte)((h >> 8) & 255), (byte)(h & 255), 255);

        // ---------- MoneyMaker palette (circles, rings): rim/clock, fill/scorch, hold frame 1 (hot), ring lead, ring band ----------
        static readonly Color32 MInk46 = C46(0x181425), White46 = C46(0xffffff), Steel46 = C46(0xc0cbdc);
        static readonly Color32[] MRim46 = { C46(0xf77622), C46(0x2ce8f5), C46(0xfee761) };
        static readonly Color32[] MFill46 = { C46(0xbe4a2f), C46(0x0099db), C46(0xb86f50) };
        static readonly Color32[] MHot46 = { C46(0xfeae34), C46(0xffffff), C46(0xffffff) };
        static readonly Color32[] MLead46 = { C46(0xfeae34), C46(0x2ce8f5), C46(0xfee761) };
        static readonly Color32[] MBand46 = { C46(0xf77622), C46(0x0099db), C46(0xfeae34) };
        // ---------- Hellord FX palette (cones, lanes): rim, clock, signature, wash ----------
        static readonly Color32[] HRim46 = { C46(0xf77622), C46(0x0099db), C46(0xfeae34) };
        static readonly Color32[] HClock46 = { C46(0xfeae34), C46(0xc0cbdc), C46(0xfee761) };
        static readonly Color32[] HSig46 = { C46(0xead4aa), C46(0xc0cbdc), C46(0xfee761) };
        static readonly Color32[] HWash46 = { C46(0x733e39), C46(0x124e89), C46(0x3e2731) };
        static readonly Color HDark46 = new Color32(0x18, 0x14, 0x1f, 255), HBack46 = new Color32(0x10, 0x00, 0x10, 255);

        /// <summary>Spec palette row: 0 Ox (also Horse), 1 Ice Gorilla (also Frost Ox), 2 Qilin. The specs only define these three bosses.</summary>
        int Pal46 => IsGorilla || IsFrostOx ? 1 : IsQilin ? 2 : 0;

        const int KCircle = 0, KDrop = 1, KLane = 2, KCone = 3, KRing = 4;
        class TI46
        {
            public int kind, pal, ox, oy, lx0, ly0, w, h, sub; public float a1, a2, a3; public bool touched; public float p, age, rel = -1f;
            public float[] f0, f1; public Color32[] px; public Texture2D tex; public SpriteRenderer sr;
        }
        class TT46 { public int kind; public Vector2 a, b; public float w, deg, half, reach, t, tot; }
        readonly List<TI46> _ti46 = new List<TI46>();
        readonly List<TT46> _tt46 = new List<TT46>();

        // ======================================================================== public API (immediate; call every frame of the tell)
        /// <summary>Ground circle (MoneyMaker). drop = falling object: adds the growing drop shadow.</summary>
        void TellCircle46(Vector2 c, float r, Color col, float p, int sub, bool drop = false)
        {
            if (!Crisp46) { p = Mathf.Clamp01(p); ZDisc(c, r * p, new Color(col.r, col.g, col.b, 0.3f), 0.6f, sub); ZRing(c, r, col, 0.6f, sub + 1); return; }
            Touch46(drop ? KDrop : KCircle, c, Mathf.Max(2f, r * Px46), 0, 0, p, sub);
        }
        /// <summary>Ring closing onto c from radius R (MoneyMaker). gapDeg / gapHalfDeg: safe gap (no ring there, steel posts at its edges).</summary>
        void TellRing46(Vector2 c, float R, Color col, float p, int sub, float gapDeg = 0f, float gapHalfDeg = 0f)
        {
            if (!Crisp46) { ZRing(c, R * (0.6f + 0.4f * Mathf.Clamp01(p)), col, 0.6f, sub); return; }
            Touch46(KRing, c, Mathf.Max(24f, R * Px46), gapDeg, gapHalfDeg, p, sub);
        }
        /// <summary>Cone from apex o (Hellord FX). halfDeg = half the opening.</summary>
        void TellWedge46(Vector2 o, float deg, float halfDeg, float reach, Color col, float p, int sub)
        {
            if (!Crisp46) { ZWedge(o, deg, halfDeg, reach, new Color(col.r, col.g, col.b, 0.2f + 0.35f * Mathf.Clamp01(p)), sub); return; }
            Touch46(KCone, o, Mathf.Max(8f, reach * Px46), Mathf.Repeat(deg, 360f), Mathf.Clamp(halfDeg, 3f, 175f), p, sub);
        }
        /// <summary>Lane / charge from a to b, width wU units (Hellord FX). Closed at the source, round cap at the far end.</summary>
        void TellLane46(Vector2 a, Vector2 b, float wU, Color col, float p, int sub)
        {
            Vector2 d = b - a; if (d.magnitude < 0.05f) return;
            if (!Crisp46) { ZSeg(a, b, wU, new Color(col.r, col.g, col.b, 0.25f + 0.4f * Mathf.Clamp01(p)), sub); return; }
            Touch46(KLane, a, Mathf.Max(4f, d.magnitude * Px46), Mathf.Max(3f, Mathf.Round(wU * Px46 * 0.5f)), Mathf.Repeat(Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, 360f), p, sub);
        }
        /// <summary>Self-timed HFX lane (replaces LaneTell.Begin for charges when the v0.0.46 tells are on).</summary>
        void TimedLane46(Vector2 a, Vector2 b, float widthPx, float seconds) => _tt46.Add(new TT46 { kind = KLane, a = a, b = b, w = widthPx / Px46, tot = Mathf.Max(0.2f, seconds) });
        /// <summary>Self-timed HFX cone (replaces TellFX.ConeTelegraph for the Ox horn sweep when the v0.0.46 tells are on).</summary>
        void TimedCone46(Vector2 o, float deg, float halfDeg, float reachU, float seconds) => _tt46.Add(new TT46 { kind = KCone, a = o, deg = deg, half = halfDeg, reach = reachU, tot = Mathf.Max(0.2f, seconds) });
        void StopTimed46() => _tt46.Clear();
        void Tells46Clear()
        {
            _tt46.Clear();
            foreach (var t in _ti46) Kill46(t);
            _ti46.Clear();
        }

        // ======================================================================== instances
        void Touch46(int kind, Vector2 c, float a1, float a2, float a3, float p, int sub)
        {
            int pal = Pal46, ox = Mathf.FloorToInt(c.x * Px46), oy = Mathf.FloorToInt(c.y * Px46);
            p = Mathf.Clamp(p, 0f, 0.9999f);
            foreach (var t in _ti46)
            {
                if (t.touched || t.rel >= 0 || t.kind != kind || t.pal != pal) continue;
                if (Mathf.Abs(t.a1 - a1) > 1.5f) continue;
                if (kind != KRing && (Mathf.Abs(Mathf.DeltaAngle(t.a2, a2)) > 2f || Mathf.Abs(Mathf.DeltaAngle(t.a3, a3)) > 2f)) continue;
                if (Mathf.Abs(t.ox - ox) > 12 || Mathf.Abs(t.oy - oy) > 12) continue;
                if (kind == KRing) { t.a2 = a2; t.a3 = a3; }
                t.ox = ox; t.oy = oy; t.touched = true; t.p = p; t.sub = sub; return;
            }
            var n = new TI46 { kind = kind, pal = pal, ox = ox, oy = oy, a1 = a1, a2 = a2, a3 = a3, touched = true, p = p, sub = sub };
            Build46(n); _ti46.Add(n);
        }
        void Kill46(TI46 t)
        {
            if (t.sr) Object.Destroy(t.sr.gameObject);
            if (t.tex) Object.Destroy(t.tex);
            t.sr = null; t.tex = null;
        }
        void Build46(TI46 t)
        {
            int m;
            if (t.kind == KCircle || t.kind == KDrop) { m = Mathf.CeilToInt(t.a1) + 3; t.lx0 = t.ly0 = -m; t.w = t.h = 2 * m + 1; }
            else if (t.kind == KRing) { m = Mathf.CeilToInt(t.a1) + 9; t.lx0 = t.ly0 = -m; t.w = t.h = 2 * m + 1; }
            else if (t.kind == KCone) { m = Mathf.CeilToInt(t.a1 * 1.12f) + 6; t.lx0 = t.ly0 = -m; t.w = t.h = 2 * m + 1; }
            else
            {
                float L = t.a1, hw = t.a2 * 1.12f + 6f, r = t.a3 * Mathf.Deg2Rad; Vector2 f = new Vector2(Mathf.Cos(r), Mathf.Sin(r)), nn = new Vector2(-f.y, f.x);
                float x0 = 1e9f, x1 = -1e9f, y0 = 1e9f, y1 = -1e9f;
                foreach (float s in new[] { -6f, L + hw })
                    foreach (float v in new[] { -hw, hw }) { Vector2 q = f * s + nn * v; x0 = Mathf.Min(x0, q.x); x1 = Mathf.Max(x1, q.x); y0 = Mathf.Min(y0, q.y); y1 = Mathf.Max(y1, q.y); }
                t.lx0 = Mathf.FloorToInt(x0) - 1; t.ly0 = Mathf.FloorToInt(y0) - 1; t.w = Mathf.CeilToInt(x1) + 2 - t.lx0; t.h = Mathf.CeilToInt(y1) + 2 - t.ly0;
            }
            int N = t.w * t.h; t.px = new Color32[N];
            if (t.kind == KCircle || t.kind == KDrop || t.kind == KRing)
            {
                t.f0 = new float[N]; t.f1 = new float[N];
                for (int j = 0; j < t.h; j++) for (int i = 0; i < t.w; i++)
                {
                    float dx = t.lx0 + i, dy = t.ly0 + j, d = Mathf.Sqrt(dx * dx + dy * dy); int k = j * t.w + i;
                    t.f0[k] = d; t.f1[k] = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                }
            }
            t.tex = new Texture2D(t.w, t.h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            Z37Init(); var g = new GameObject("tell46"); g.transform.SetParent(_z37Root, false); t.sr = g.AddComponent<SpriteRenderer>();
            t.sr.sprite = Sprite.Create(t.tex, new Rect(0, 0, t.w, t.h), Vector2.zero, Px46, 0, SpriteMeshType.FullRect);
        }
        float Life46(TI46 t) => t.kind == KLane || t.kind == KCone ? 0.267f : t.kind == KRing ? 5f / 60f + 0.55f + 15f / 60f : 20f / 60f;

        /// <summary>Called from Z37End once a frame: ticks the self-timed lanes / cones, then renders or releases every tell.</summary>
        void Tells46End(float dt)
        {
            for (int i = _tt46.Count - 1; i >= 0; i--)
            {
                var q = _tt46[i]; float p = q.t / q.tot; q.t += dt;
                if (p >= 1f) { _tt46.RemoveAt(i); continue; }
                if (q.kind == KLane) TellLane46(q.a, q.b, q.w, Color.white, p, 1); else TellWedge46(q.a, q.deg, q.half, q.reach, Color.white, p, 1);
            }
            IceGorillaFX.HideTell = QilinChariotFX.HideTell = PackHide46;
            float minP = HellordTuning.I.tellReleaseMinP46;
            for (int i = _ti46.Count - 1; i >= 0; i--)
            {
                var t = _ti46[i];
                if (!t.touched)
                {
                    if (t.rel < 0 && t.p < minP) { Kill46(t); _ti46.RemoveAt(i); continue; }
                    if (t.rel < 0) t.rel = 0f;
                    if (t.rel >= Life46(t)) { Kill46(t); _ti46.RemoveAt(i); continue; }
                }
                Render46(t);
                if (t.rel >= 0) t.rel += dt; else t.age += dt;
                t.touched = false;
            }
        }
        void Render46(TI46 t)
        {
            t.sr.sortingOrder = Z37Order + t.sub; // t950: shader path keeps the same under-boss order
            if (Bs950Tell(t, new Vector3((t.ox + t.lx0) / (float)Px46, (t.oy + t.ly0) / (float)Px46, 0f))) return;
            var px = t.px; for (int k = 0; k < px.Length; k++) px[k] = default;
            bool any;
            if (t.kind == KLane || t.kind == KCone) any = RenderHFX46(t); else if (t.kind == KRing) any = RenderRing46(t); else any = RenderCircle46(t);
            t.sr.enabled = any;
            if (!any) return;
            t.tex.SetPixels32(px); t.tex.Apply(false, false);
            t.sr.sortingOrder = Z37Order + t.sub;
            t.sr.transform.position = new Vector3((t.ox + t.lx0) / (float)Px46, (t.oy + t.ly0) / (float)Px46, 0f);
        }

        // ======================================================================== shared helpers
        static float Smooth46(float x) { x = Mathf.Clamp01(x); return x * x * (3f - 2f * x); }
        static int PMod46(int a, int m) { int r = a % m; return r < 0 ? r + m : r; }
        static float Hash46(int wx, int wy)
        {
            double v = System.Math.Sin((wx + 0.5) * 12.9898 + (-wy + 0.5) * 78.233 + 37.719) * 43758.5453; return (float)(v - System.Math.Floor(v));
        }
        static Color32 Dark46(float k) => new Color32(0, 0, 0, (byte)Mathf.Clamp(Mathf.RoundToInt((1f - k) * 255f), 0, 255));
        static Color32 Mul46(Color32 c, float k) => new Color32((byte)(c.r * k), (byte)(c.g * k), (byte)(c.b * k), 255);
        static readonly float[] Pop46 = { 3f, -1f, 0f };
        static bool Blink46(float p) => (p >= 0.85f && p < 0.90f) || (p >= 0.95f && p < 1f);

        // ======================================================================== MoneyMaker circle / drop
        bool RenderCircle46(TI46 t)
        {
            float L = t.a1; int pal = t.pal; Color32 rimC = MRim46[pal], fillC = MFill46[pal]; var px = t.px;
            bool rel = t.rel >= 0; int f0 = Mathf.FloorToInt(t.age * 60f + 1e-4f), f = rel ? Mathf.FloorToInt(t.rel * 60f + 1e-4f) : 0;
            float k = !rel && f0 < 3 ? Pop46[f0] : 0f, p = t.p, clock = p < 0.85f ? Smooth46(p / 0.85f) : 1f; bool bl = Blink46(p);
            float shR = 2f + 0.45f * L * p, q = (f - 5) / 15f;
            if (rel && f >= 5 && q >= 1f) return false;
            float dimQ = q < 0 ? 0 : new[] { 0.5f, 0.65f, 0.8f, 0.9f }[Mathf.Min(3, (int)(q * 4f))];
            for (int j = 0; j < t.h; j++) for (int i = 0; i < t.w; i++)
            {
                int ix = j * t.w + i; float d = t.f0[ix], sd = d - L + k, u = d / L; bool inside = sd <= 0f;
                bool rim = inside && sd > -1f, inner = inside && sd > -2f && sd <= -1f, outl = sd > 0f && sd <= 1f;
                if (!inside && !outl) continue;
                int wx = t.ox + t.lx0 + i, wy = t.oy + t.ly0 + j; Color32 c = default;
                if (!rel)
                {
                    float body = 1f; bool fill = false;
                    if (inside) body = 0.62f;
                    if (inside && !rim && !inner && PMod46(wx - (-wy), 6) == 0 && u <= clock) fill = true;
                    if (t.kind == KDrop && inside) { float dx = t.lx0 + i, dy = (t.ly0 + j) * 1.6f; if (Mathf.Sqrt(dx * dx + dy * dy) <= shR) body *= 0.45f; }
                    if (inside) c = fill ? (body < 0.6f ? Mul46(fillC, 0.45f) : fillC) : Dark46(body);
                    float e = u * L - clock * L;
                    if (inside && e > 0.5f && e <= 1.5f) c = MInk46;
                    if (inside && Mathf.Abs(e) <= 0.5f && clock < 0.999f) c = bl ? White46 : rimC;
                    if (outl || inner) c = MInk46;
                    if (rim) c = bl ? White46 : rimC;
                }
                else if (f < 2)
                {
                    if (inside) c = Dark46(0.5f);
                    if (inside && Mathf.Abs(u - 0.55f) * L <= 0.5f) c = White46;
                    if (outl) c = MInk46;
                    if (rim) c = White46;
                }
                else if (f < 5)
                {
                    float hs = Hash46(wx, wy);
                    if (inside) c = Dark46(0.4f);
                    if (inside && u < 0.6f && hs < 0.10f) c = fillC;
                    if (outl) c = MInk46;
                    if (rim && hs < 0.8f) c = f == 2 ? MHot46[pal] : rimC;
                }
                else
                {
                    if (inside) c = Dark46(dimQ);
                    if (rim && Hash46(wx, wy) < 0.8f * (1f - q)) c = rimC;
                }
                px[ix] = c;
            }
            return true;
        }

        // ======================================================================== MoneyMaker ring
        bool RenderRing46(TI46 t)
        {
            float R = t.a1, r0 = 18f, gap = t.a2, gh = t.a3; int pal = t.pal; var px = t.px;
            Color32 rimC = MRim46[pal], lead = MLead46[pal], band = MBand46[pal], hot = MHot46[pal];
            bool rel = t.rel >= 0; int f = rel ? Mathf.FloorToInt(t.rel * 60f + 1e-4f) : 0; float tw = rel ? t.rel : 0f;
            int f0 = Mathf.FloorToInt(t.age * 60f + 1e-4f); float k = !rel && f0 < 3 ? Pop46[f0] : 0f, p = t.p; bool bl = Blink46(p);
            float clock = p < 0.85f ? Smooth46(p / 0.85f) : 1f, rr = R + k - (R - r0) * clock;
            const int hold = 5; float rw = r0 + 3f, keepT = 2f; bool waveFade = false;
            if (rel && f >= hold)
            {
                float q = Mathf.Min(1f, (tw - hold / 60f) / 0.55f); rw = r0 + 3f + (R - r0 - 3f) * (1f - (1f - q) * (1f - q));
                if (q >= 1f) { float qq = (tw - hold / 60f - 0.55f) / 0.25f; if (qq >= 1f) return false; keepT = 1f - qq; waveFade = true; }
            }
            for (int j = 0; j < t.h; j++) for (int i = 0; i < t.w; i++)
            {
                int ix = j * t.w + i; float d = t.f0[ix], ang = t.f1[ix];
                bool ingap = gh > 0f && Mathf.Abs(Mathf.DeltaAngle(ang, gap)) < gh;
                bool org = d <= r0, orim = org && d > r0 - 1f, oout = d > r0 && d <= r0 + 1f;
                int wx = t.ox + t.lx0 + i, wy = t.oy + t.ly0 + j; Color32 c = default;
                if (!rel)
                {
                    int arc = Mathf.FloorToInt(ang * Mathf.Deg2Rad * R / 3f);
                    bool even = PMod46(arc, 2) == 0, dots = Mathf.Abs(d - (R + k) + 0.5f) <= 1f && even && !ingap, dback = Mathf.Abs(d - (R + k) + 0.5f) <= 2f && even && !ingap;
                    bool crO = Mathf.Abs(d - rr) <= 0.5f && !ingap, crI = Mathf.Abs(d - rr + 1f) <= 0.5f && !ingap;
                    bool crb = (Mathf.Abs(d - rr - 1f) <= 0.5f || Mathf.Abs(d - rr + 2f) <= 0.5f) && !ingap;
                    if (org) c = Dark46(0.6f);
                    if (dback) c = MInk46;
                    if (dots) c = bl ? White46 : band;
                    if (crb) c = MInk46;
                    if (crI) c = bl ? White46 : band;
                    if (crO) c = bl ? White46 : lead;
                    if (oout) c = MInk46;
                    if (orim) c = bl ? White46 : rimC;
                }
                else if (f < 2)
                {
                    if (org) c = Dark46(0.5f);
                    if (oout) c = MInk46;
                    if (orim) c = White46;
                    float near = Mathf.Round(ang / 45f) * 45f;
                    if (d <= r0 + 7f && d > r0 + 1f && Mathf.Abs(near - ang) * d * Mathf.Deg2Rad <= 0.5f) c = White46;
                }
                else
                {
                    bool bnd = !ingap, keep = !waveFade || Hash46(wx, wy) < keepT;
                    bool e0 = Mathf.Abs(d - rw) <= 0.5f && bnd, e1 = d < rw - 0.5f && d >= rw - 2.5f && bnd, e2 = d < rw - 2.5f && d >= rw - 3.5f && bnd, e3 = d > rw + 0.5f && d <= rw + 1.5f && bnd;
                    if (f >= hold) { e0 &= keep; e1 &= keep; }
                    if (e3 || e2) c = MInk46;
                    if (e1) c = band;
                    if (e0) c = f >= hold ? lead : hot;
                    if (org) c = Dark46(0.5f);
                    if (orim && Hash46(wx, wy) < 0.7f) c = rimC;
                }
                px[ix] = c;
            }
            if (gh > 0f && !(rel && f < 2))
            {
                for (int sg = -1; sg <= 1; sg += 2)
                {
                    float a = (gap + sg * gh) * Mathf.Deg2Rad, ca = Mathf.Cos(a), sa = Mathf.Sin(a);
                    for (int r = (int)(R - 6); r < (int)(R + 7); r++)
                    {
                        int x = Mathf.RoundToInt(r * ca), y = Mathf.RoundToInt(r * sa);
                        for (int yy = -1; yy <= 1; yy++) for (int xx = -1; xx <= 1; xx++) Put46(t, x + xx, y + yy, MInk46);
                    }
                    for (int r = (int)(R - 5); r < (int)(R + 6); r++) Put46(t, Mathf.RoundToInt(r * ca), Mathf.RoundToInt(r * sa), Steel46);
                }
            }
            return true;
        }
        static void Put46(TI46 t, int lx, int ly, Color32 c)
        {
            int i = lx - t.lx0, j = ly - t.ly0; if (i < 0 || j < 0 || i >= t.w || j >= t.h) return; t.px[j * t.w + i] = c;
        }

        // ======================================================================== Hellord FX cone / lane
        // Marks are point lists (rasterised by sampling, de-duplicated in order, as in the mock), so stride / phase dashes match it.
        Color32[] _hm46 = new Color32[0]; bool[] _hs46 = new bool[0]; float[] _hd46 = new float[0];
        readonly List<Vector2Int> _hp46 = new List<Vector2Int>(); readonly HashSet<long> _hu46 = new HashSet<long>();
        TI46 _hT46;
        void PBegin46() { _hp46.Clear(); _hu46.Clear(); }
        void PAdd46(float x, float y) { int ix = Mathf.RoundToInt(x), iy = Mathf.RoundToInt(y); long key = ((long)ix << 32) ^ (uint)iy; if (_hu46.Add(key)) _hp46.Add(new Vector2Int(ix, iy)); }
        void PLine46(Vector2 p, Vector2 q)
        {
            int n = (int)Mathf.Max(Mathf.Abs(q.x - p.x), Mathf.Abs(q.y - p.y)) + 1;
            for (int i = 0; i <= n; i++) PAdd46(p.x + (q.x - p.x) * i / n, p.y + (q.y - p.y) * i / n);
        }
        void PArc46(Vector2 c, float r, float a0, float a1)
        {
            int n = Mathf.Max(16, (int)(8f * (2f * r) * Mathf.Abs(a1 - a0) / 6.28f));
            for (int i = 0; i <= n; i++) { float a = a0 + (a1 - a0) * i / n; PAdd46(c.x + r * Mathf.Cos(a), c.y + r * Mathf.Sin(a)); }
        }
        List<Vector2Int> PTake46() => new List<Vector2Int>(_hp46);
        int HIdx46(int lx, int ly) { var t = _hT46; int i = lx - t.lx0, j = ly - t.ly0; return i < 0 || j < 0 || i >= t.w || j >= t.h ? -1 : j * t.w + i; }
        void HPut46(List<Vector2Int> pts, Color32 c, int stride = 1, int phase = 0)
        {
            for (int i = 0; i < pts.Count; i++)
            {
                if (stride != 1 && (i + phase) % stride != 0) continue;
                int k = HIdx46(pts[i].x, pts[i].y); if (k < 0) continue; _hm46[k] = c; _hs46[k] = true;
            }
        }
        void HPut46(Vector2Int pt, Color32 c) { int k = HIdx46(pt.x, pt.y); if (k >= 0) { _hm46[k] = c; _hs46[k] = true; } }

        bool RenderHFX46(TI46 t)
        {
            int N = t.w * t.h; if (_hm46.Length < N) { _hm46 = new Color32[N]; _hs46 = new bool[N]; _hd46 = new float[N]; }
            for (int k = 0; k < N; k++) { _hs46[k] = false; _hd46[k] = 0f; }
            _hT46 = t; int pal = t.pal; bool lane = t.kind == KLane; bool rel = t.rel >= 0;
            float s = !rel && t.age < 0.067f ? 1.12f : !rel && t.age < 0.133f ? 0.96f : 1f;
            float dir = (lane ? t.a3 : t.a2) * Mathf.Deg2Rad; Vector2 fw = new Vector2(Mathf.Cos(dir), Mathf.Sin(dir)), nr = new Vector2(-fw.y, fw.x);
            float L = t.a1, hw = lane ? Mathf.Round(t.a2 * s) : 0f, R = lane ? 0f : t.a1 * s, half = lane ? 0f : t.a3 * Mathf.Deg2Rad, a0 = dir - half, a1 = dir + half;
            Vector2 W(float x, float y) => fw * x + nr * y;
            // rim
            PBegin46();
            if (lane)
            {
                PLine46(W(0, -hw), W(L, -hw)); PLine46(W(0, hw), W(L, hw));
                int n = Mathf.Max(16, (int)(8f * 2f * hw * Mathf.PI / 6.28f));
                for (int i = 0; i <= n; i++) { float a = -Mathf.PI / 2f + Mathf.PI * i / n; Vector2 q = W(L + hw * Mathf.Cos(a), hw * Mathf.Sin(a)); PAdd46(q.x, q.y); }
                PLine46(W(0, -hw), W(0, hw));
            }
            else { PLine46(Vector2.zero, new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * R); PLine46(Vector2.zero, new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * R); PArc46(Vector2.zero, R, a0, a1); }
            var rimPts = PTake46();
            bool Inside(int x, int y)
            {
                if (lane) { float sx = x * fw.x + y * fw.y, v = -x * fw.y + y * fw.x; return (sx > 0 && sx < L && Mathf.Abs(v) < hw) || (sx >= L && (sx - L) * (sx - L) + v * v < hw * hw); }
                float d = Mathf.Sqrt(x * x + y * y); if (d >= R || d < 1e-3f) return false; float da = Mathf.DeltaAngle(dir * Mathf.Rad2Deg, Mathf.Atan2(y, x) * Mathf.Rad2Deg) * Mathf.Deg2Rad; return Mathf.Abs(da) < half;
            }
            Color32 rimC = HRim46[pal], clockC = HClock46[pal], sigC = HSig46[pal], washC = HWash46[pal];
            if (!rel)
            {
                float u = t.p; bool now = (u >= 0.855f && u < 0.88f) || (u >= 0.92f && u < 0.95f);
                foreach (var q in rimPts)
                {
                    for (int dd = 0; dd < 4; dd++)
                    {
                        int k = HIdx46(q.x + (dd == 0 ? 1 : dd == 1 ? -1 : 0), q.y + (dd == 2 ? 1 : dd == 3 ? -1 : 0)); if (k >= 0 && !_hs46[k]) _hd46[k] = 0.75f;
                    }
                }
                HPut46(rimPts, now ? White46 : rimC);
                // signature marks on the anchors
                var anch = new List<Vector4>();
                if (lane) { for (int i = 1; i <= 5; i++) { Vector2 q = W(L * i / 6f, -hw); anch.Add(new Vector4(q.x, q.y, -nr.x, -nr.y)); } for (int i = 1; i <= 5; i++) { Vector2 q = W(L * i / 6f, hw); anch.Add(new Vector4(q.x, q.y, nr.x, nr.y)); } }
                else for (int i = 0; i < 5; i++) { float a = a0 + (a1 - a0) * i / 4f; anch.Add(new Vector4(R * Mathf.Cos(a), R * Mathf.Sin(a), Mathf.Cos(a), Mathf.Sin(a))); }
                Color32 sc = now ? White46 : sigC;
                foreach (var an in anch)
                {
                    int ax = Mathf.RoundToInt(an.x), ay = Mathf.RoundToInt(an.y); float nx = an.z, ny = an.w;
                    if (pal == 0) { HPut46(new Vector2Int(Mathf.RoundToInt(ax - nx), Mathf.RoundToInt(ay - ny)), sc); HPut46(new Vector2Int(Mathf.RoundToInt(ax - nx * 2), Mathf.RoundToInt(ay - ny * 2)), sc); }
                    else if (pal == 1) { for (int i = 1; i <= 3; i++) HPut46(new Vector2Int(Mathf.RoundToInt(ax + nx * i), Mathf.RoundToInt(ay + ny * i)), sc); }
                    else
                    {
                        HPut46(new Vector2Int(ax + 1, ay), sc); HPut46(new Vector2Int(ax - 1, ay), sc); HPut46(new Vector2Int(ax, ay + 1), sc); HPut46(new Vector2Int(ax, ay - 1), sc);
                        int k = HIdx46(ax, ay); if (k >= 0) { _hd46[k] = 0.9f; _hs46[k] = false; }
                    }
                }
                // clock front: reaches the edge at 85 %
                float f = Mathf.Min(1f, u / 0.85f);
                PBegin46();
                if (lane) PLine46(W(L * f, -hw + 2), W(L * f, hw - 2)); else PArc46(Vector2.zero, Mathf.Max(2f, R * f), a0 + 0.03f, a1 - 0.03f);
                HPut46(PTake46(), u < 0.85f ? clockC : (now ? White46 : clockC));
                // wash: opaque sparse dither, denser behind the clock (never on a mark)
                for (int j = 0; j < t.h; j++) for (int i = 0; i < t.w; i++)
                {
                    int x = t.lx0 + i, y = t.ly0 + j; if (!Inside(x, y)) continue;
                    bool behind = lane ? (x * fw.x + y * fw.y) < L * f : Mathf.Sqrt(x * x + y * y) < R * f;
                    int X = t.ox + x, Y = -(t.oy + y);
                    if ((behind && PMod46(X + 2 * Y, 4) == 0) || (PMod46(X + 2 * Y, 8) == 0 && PMod46(Y, 2) == 0)) { int k = j * t.w + i; if (!_hs46[k]) t.px[k] = washC; }
                }
                if (lane)
                {
                    float fx = L * f;
                    for (int jj = 0; jj < 3; jj++)
                    {
                        int cxp = Mathf.RoundToInt(fx - 10 - 9 * jj); if (cxp < 4) continue;
                        PBegin46();
                        for (int i = 0; i < 5; i++) { Vector2 q = W(cxp - i, -i); PAdd46(q.x, q.y); }
                        for (int i = 1; i < 5; i++) { Vector2 q = W(cxp - i, i); PAdd46(q.x, q.y); }
                        HPut46(PTake46(), sc);
                    }
                }
            }
            else
            {
                float tr = t.rel; int fr = Mathf.RoundToInt(tr * 30f);
                if (fr <= 1)
                {
                    HPut46(rimPts, White46);
                    if (fr == 0)
                        for (int j = 0; j < t.h; j++) for (int i = 0; i < t.w; i++)
                        {
                            int x = t.lx0 + i, y = t.ly0 + j;
                            if (Inside(x, y) && PMod46((t.ox + x) + (-(t.oy + y)), 3) == 0) { int k = j * t.w + i; _hm46[k] = White46; _hs46[k] = true; }
                        }
                }
                else if (tr < 0.2f + 0.067f) { float k = (tr - 0.067f) / 0.2f; if (k < 0.5f) HPut46(rimPts, rimC, 2, fr); else HPut46(rimPts, washC, 3, fr); }
                if (lane && fr < 4) { PBegin46(); PLine46(Vector2.zero, W(L * Mathf.Min(1f, (fr + 1) / 3f), 0)); HPut46(PTake46(), fr < 2 ? White46 : clockC); }
                if (!lane && fr < 3)
                    for (float rr = R * 0.6f; rr < R; rr += 3f) { PBegin46(); PArc46(Vector2.zero, rr, a0 + 0.04f, a1 - 0.04f); HPut46(PTake46(), fr < 1 ? White46 : clockC, 2, (int)rr); }
            }
            // compose: 3x3 backing (#100010 60 %) under every mark, dark outline (#18141f), wash already in px, marks on top
            bool anyMark = false;
            for (int j = 0; j < t.h; j++) for (int i = 0; i < t.w; i++)
            {
                int k = j * t.w + i; if (_hs46[k]) { anyMark = true; t.px[k] = _hm46[k]; continue; }
                bool bk = false;
                for (int yy = -1; yy <= 1 && !bk; yy++) for (int xx = -1; xx <= 1; xx++)
                {
                    int ii = i + xx, jj = j + yy; if (ii < 0 || jj < 0 || ii >= t.w || jj >= t.h) continue; if (_hs46[jj * t.w + ii]) { bk = true; break; }
                }
                Color32 w0 = t.px[k]; if (w0.a == 255) continue;
                float a = 0f; Color cc = Color.clear;
                if (bk) { a = 0.6f; cc = HBack46 * 0.6f; }
                float av = _hd46[k]; if (av > 0f) { cc = HDark46 * av + cc * (1f - av); a = av + a * (1f - av); }
                if (a > 0f) { Color o = cc / a; o.a = a; t.px[k] = o; }
            }
            return anyMark;
        }
    }
}
