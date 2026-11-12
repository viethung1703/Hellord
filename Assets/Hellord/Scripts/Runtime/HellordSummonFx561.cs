// t561u FX (v0.0.53): VH-approved Hellord FX mock-ups wired as runtime primitives.
// Warrior v2 (/workspace/fx_skel_warrior SPEC): downward slash smear (2f), impact star 6->4px + ring + 3 chips (3f hold), Grave Edge bone link,
// walk dust behind the heel, bone/ivory spawn grave ring, death white frame + 9 shards + dashed dust ring.
// Archer v7 (/workspace/fx_skel_archer SPEC): visual release on shoot f6 (+0.5 s), release flash, normal 19px bone arrow 0.1 s flight
// (t605u: 14px tan shaft, ivory head, red fletch, #181425 outline, short fading smear, NO lane line), arrival star + ring + splinters, stuck arrow 0.4 s,
// A6 pierce lane; stepped death sink. Shared: blink-back teleport, bullet-block spark.
// DAMAGE TIMING: warrior = attack frame 0 (synced with slash FX). Archer (t594u) = ON ARROW IMPACT: arrow spawns on shoot f6, flies at archerArrowSpeed594, hits when the tip reaches the enemy.
using System.Collections.Generic;
using UnityEngine;
using PixelFXPack;

namespace Hellord
{
    public partial class HellordGame
    {
        const float Px561 = 1f / 16f, Fr561 = 1f / 30f;
        static readonly Color B1_561 = E32(0xead4aa), B2_561 = E32(0xe4a672), B3_561 = E32(0xc28569), B4_561 = E32(0xb86f50), Ol561 = E32(0x181425), W561 = Color.white;
        Color Dust561 => Theme == 1 && floorMode ? E32(0x8b9bb4) : E32(0x5a6988);
        Color Dust2_561 => Theme == 1 && floorMode ? E32(0x5a6988) : E32(0x3a4466);
        enum F561 { Dust, Slash, Star, Link, Death, Spawn, BlinkOut, BlinkIn, Spark, Release, Arrow, Stuck }
        class Fx561 { public F561 k; public Vector2 a, b; public float t, delay, dur; public bool flip, pierce; public int tgt = -1; public float dmg; public SkillInst sk; public Vector2 tip; public List<int> hit; public int pl = -1, bl; public bool fk; public float hom, bR; } // t622u: pl pierce left (-1 = Marrow unlimited), bl bounces left, fk fork pending, hom homing range, bR bounce range
        readonly List<Fx561> _fx561 = new List<Fx561>();
        void Fx561Add(F561 k, Vector2 a, Vector2 b, float dur, float delay = 0f, bool flip = false) { if (_fx561.Count < 400) _fx561.Add(new Fx561 { k = k, a = a, b = b, dur = dur, delay = delay, flip = flip }); }

        // ---- hooks
        void FxWarHit561(NMin536 m, Enemy e) { Vector2 tp = e.pos + Vector2.up * e.hitY; Fx561Add(F561.Slash, m.p, tp, 2 * Fr561, 0, tp.x < m.p.x); Fx561Add(F561.Star, tp, (tp - m.p).normalized, 3 * Fr561); }
        void FxEdge561(Enemy a, Enemy b) { Vector2 pa = a.pos + Vector2.up * a.hitY, pb = b.pos + Vector2.up * b.hitY; Fx561Add(F561.Link, pa, pb, 3 * Fr561); Fx561Add(F561.Star, pb, Vector2.zero, 3 * Fr561); _fx561[_fx561.Count - 1].pierce = true; /* 0.7x star */ }
        void FxArcShot561(NMin536 m, int tgt, Vector2 end, bool pierce, float dmg, SkillInst sk)
        {
            float rel = PT.archerReleaseS594; // shoot f6 (after full draw f5) at 12 fps
            Vector2 bow = m.p + new Vector2((m.face.x < 0 ? -1 : 1) * 0.35f, 1.1f);
            Fx561Add(F561.Release, bow, end, Fr561 * 2, rel);
            var f = new Fx561 { k = F561.Arrow, a = bow, b = end, tip = bow, dur = PT.archerArrowLife594, delay = rel, pierce = pierce, tgt = tgt, dmg = dmg, sk = sk, hit = new List<int>(4) }; ArrowSup622(f); _fx561.Add(f);
        }
        // ===== t622u: player projectile supports on skeleton archer arrows (same numbers as SpawnProj / player projectiles)
        void ArrowSup622(Fx561 f)
        {
            var s = f.sk; f.pl = f.pierce ? -1 : 0; if (s == null) return; var T = HellordTuning.I;
            if (s.Has(SupportId.Pierce)) f.pl = f.pierce ? -1 : HData.At(T.supPierce, s.Lvl(SupportId.Pierce), 1);
            if (s.Has(SupportId.Bounce)) { f.bl = HData.At(T.supBounce, s.Lvl(SupportId.Bounce), 1); f.bR = HData.At(T.supBounceRangeU, s.Lvl(SupportId.Bounce), 4f); }
            f.fk = s.Has(SupportId.Fork);
            f.hom = s.Has(SupportId.Homing) ? HData.At(T.supHomingRangeU, s.Lvl(SupportId.Homing), 5f) : 0f;
        }
        /// <summary>t622u Multi Projectiles: extra arrows per shot at other demons near the target (or the same one), each at multiProjDmgMul damage.</summary>
        void ArcMulti622(NMin536 m, int t, Vector2 end, bool pierce, ref float dmg, SkillInst sk, float rng)
        {
            if (sk == null || !sk.Has(SupportId.MultipleProjectiles)) return; var T = HellordTuning.I;
            int extra = HData.At(T.supMultiExtra, sk.Lvl(SupportId.MultipleProjectiles), 1); dmg *= T.multiProjDmgMul;
            _aq622.Clear(); Query(_en[t].pos, 4f, _aq622); var used = new List<int>(4) { t };
            Vector2 dir = (end - m.p).normalized;
            for (int i = 0; i < extra; i++)
            {
                int bt = -1; float bd = float.MaxValue;
                foreach (var q in _aq622) { if (!LiveEn594(q) || used.Contains(q)) continue; float d = (_en[q].pos - m.p).magnitude; if (d > rng + 1f) continue; if (d < bd) { bd = d; bt = q; } }
                if (bt >= 0) used.Add(bt);
                int tt = bt >= 0 ? bt : t; Vector2 e2 = pierce ? m.p + (Vector2)(Quaternion.Euler(0, 0, (i % 2 == 0 ? 1 : -1) * 8f * (i / 2 + 1)) * dir) * rng : _en[tt].pos + Vector2.up * _en[tt].hitY;
                FxArcShot561(m, tt, e2, pierce, dmg, sk);
            }
        }
        readonly List<int> _aq622 = new List<int>(32);
        int NearUnhit622(Vector2 p, float r, List<int> hit)
        {
            _aq622.Clear(); Query(p, r, _aq622); int bt = -1; float bd = r * r;
            foreach (var q in _aq622) { if (!LiveEn594(q) || hit.Contains(q)) continue; float d = (_en[q].pos - p).sqrMagnitude; if (d < bd) { bd = d; bt = q; } }
            return bt;
        }
        /// <summary>t622u: after an arrow strikes, apply Fork / Pierce / Bounce. Returns true when the arrow keeps flying.</summary>
        bool ArrowOnHit622(Fx561 f, Vector2 hp, Vector2 dir)
        {
            var T = HellordTuning.I;
            if (f.fk)
            {   // Fork: split once into N children that fly on in a fan (each stops at its first hit, no further fork)
                f.fk = false; ProjectileFX.HitSplit(hp, ProjElement.Bone); /* t659u */ var s = f.sk; int n = HData.At(T.supForkChildren, s.Lvl(SupportId.Fork), 2); float cd = f.dmg * HData.At(T.supForkDmg, s.Lvl(SupportId.Fork), 0.55f);
                for (int c = 0; c < n && _fx561.Count < 400; c++)
                {
                    float a = (c - (n - 1) * 0.5f) * T.forkSpreadDeg; Vector2 d2 = Quaternion.Euler(0, 0, a == 0 ? T.forkSpreadDeg * 0.5f : a) * dir;
                    var ch = new Fx561 { k = F561.Arrow, a = hp, b = hp + d2 * 5f, tip = hp, dur = PT.archerArrowLife594, delay = 0, pierce = true, tgt = -1, dmg = cd, sk = f.sk, hit = new List<int>(f.hit), pl = 0, bl = 0, fk = false, hom = f.hom };
                    _fx561.Add(ch);
                }
            }
            if (f.pl < 0) return true; // Marrow Shot lane arrow: unlimited pierce (unchanged)
            if (f.pl > 0) { f.t = 0; f.pl--; f.dmg *= 1f - T.pierceFalloff; if (!f.pierce) { f.pierce = true; f.tgt = -1; f.b = hp + dir * 4f; } return true; } // Pierce: fly straight on through
            if (f.bl > 0) { int nt = NearUnhit622(hp, f.bR, f.hit); if (nt >= 0) { ProjectileFX.HitBounce(hp, ProjElement.Bone); /* t659u */ f.t = 0; f.bl--; f.pierce = false; f.tgt = nt; f.b = _en[nt].pos + Vector2.up * _en[nt].hitY; return true; } } // Bounce: jump to the next demon
            return false;
        }
        readonly List<int> _aq594 = new List<int>(32);
        bool LiveEn594(int t) => _en != null && t >= 0 && t < maxEnemies && t < _en.Length && _en[t] != null && _en[t].alive && !_en[t].dying;
        // t594u: advance a live arrow; damage is dealt only here, when the tip touches an enemy
        bool ArrowStep594(Fx561 f, float dt)
        {
            if (!f.pierce)
            {   // homing on the target; if it died mid-flight retarget the nearest demon near where it was, else fly on to the last point
                if (!LiveEn594(f.tgt)) { int n = f.hom > 0 ? NearUnhit622(f.tip, Mathf.Max(PT.archerRetargetR594, f.hom), f.hit) : Nearest(f.b, PT.archerRetargetR594, -1); f.tgt = n; } // t622u Homing: wider re-seek
                if (LiveEn594(f.tgt)) f.b = _en[f.tgt].pos + Vector2.up * _en[f.tgt].hitY;
            }
            else if (f.hom > 0 && f.pl >= 0)
            {   // t622u Homing on straight (pierce / fork) arrows: curve onto the nearest unhit demon in range (Marrow lane arrows stay straight)
                int n = NearUnhit622(f.tip, f.hom, f.hit); if (n >= 0) { Vector2 tp = _en[n].pos + Vector2.up * _en[n].hitY; Vector2 cur = (f.b - f.tip).normalized, want = (tp - f.tip).normalized; Vector2 nd = Vector3.Slerp(cur, want, Mathf.Clamp01(8f * dt)).normalized; f.b = f.tip + nd * Mathf.Max(1f, (f.b - f.tip).magnitude); }
            }
            Vector2 to = f.b - f.tip; float l = to.magnitude, step = PT.archerArrowSpeed594 * dt; Vector2 prev = f.tip;
            f.tip = l <= step ? f.b : f.tip + to / l * step; Vector2 dir = l > 0.001f ? to / l : Vector2.right;
            _aq594.Clear(); Query((prev + f.tip) * 0.5f, (f.tip - prev).magnitude * 0.5f + 1.5f, _aq594);
            for (int z = 0; z < _aq594.Count; z++)
            {
                int q = _aq594[z]; if (!LiveEn594(q) || f.hit.Contains(q)) continue; var x = _en[q];
                if (!f.pierce && q != f.tgt) continue; // a normal arrow only strikes its own target
                if (SegDist53(x.pos + Vector2.up * x.hitY, prev, f.tip) > x.radius + 0.25f) continue;
                f.hit.Add(q); Hit(x, f.dmg, f.sk, dir * 0.2f); // hit reaction + damage ON IMPACT
                Vector2 hp = x.pos + Vector2.up * x.hitY; Fx561Add(F561.Star, hp, dir, 3 * Fr561); if (f.pierce) _fx561[_fx561.Count - 1].pierce = true; HordeFX.CheapBurst(hp, B1_561, 3);
                if (ArrowOnHit622(f, hp, dir)) { if (!f.pierce) return false; continue; } // t622u fork / pierce / bounce
                if (f.pierce && f.pl == 0) { f.b = hp; return true; } // t622u: finite-pierce / fork child is spent
                if (!f.pierce) { f.b = hp; Fx561Add(F561.Stuck, hp, dir, 0.4f); return true; }
            }
            if ((f.tip - f.b).sqrMagnitude < 1e-4f) { Fx561Add(F561.Star, f.b, dir, 3 * Fr561); if (!f.pierce && f.hit.Count == 0) Fx561Add(F561.Stuck, f.b, dir, 0.4f); return true; } // reached the end (missed / lane end)
            return false;
        }
        void FxSpawn561(Vector2 p) => Fx561Add(F561.Spawn, p, Vector2.zero, 0.7f);
        void FxDeath561(Vector2 p) => Fx561Add(F561.Death, p, Vector2.zero, 0.35f);
        void FxBlink561(Vector2 from, Vector2 to) { Fx561Add(F561.BlinkOut, from, Vector2.zero, 0.15f); Fx561Add(F561.BlinkIn, to, Vector2.zero, 0.25f, 0.15f); }
        void FxSpark561(Vector2 p) => Fx561Add(F561.Spark, p, Vector2.zero, 3 * Fr561);
        void FxDust561(Vector2 foot, Vector2 face) { Vector2 back = face.sqrMagnitude > 0.0001f ? -face.normalized : Vector2.zero; Fx561Add(F561.Dust, foot + back * 11 * Px561, Vector2.zero, 0.35f); }

        // ---- draw (Z37 primitives are immediate: called every frame from NecroTick536)
        static float Step561(float t, float dur, int steps) => Mathf.Floor(Mathf.Clamp01(t / dur) * steps) / steps;
        void Star561(Vector2 c, float px, int rays, Color col) { for (int i = 0; i < rays; i++) { float a = i * Mathf.PI * 2f / rays; Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * px * Px561 * (i % 2 == 0 ? 1f : 0.6f); ZSeg(c, c + d, Px561, col, 9); } ZDisc(c, Px561, W561, 1f, 10); }
        void DashRing561(Vector2 c, float r, Color col, int dashes) { for (int i = 0; i < dashes; i++) { float a0 = i * Mathf.PI * 2f / dashes, a1 = a0 + Mathf.PI / dashes; ZSeg(c + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0) * 0.5f) * r, c + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1) * 0.5f) * r, Px561, col, 1); } }
        // t605u: clean 14px pixel bone arrow (Endesga 32): #181425 outline, tan bone shaft (not white), ivory head, red #e43b44 fletching,
        // short 4px fading smear behind the nock. No full-length lane line. Marrow Shot = same arrow, 2px shaft + longer smear.
        static readonly Color Fl605 = E32(0xe43b44), Fl2_605 = E32(0xa22633);
        void Arrow561(Vector2 tip, Vector2 dir, float thick)
        {
            Vector2 n = new Vector2(-dir.y, dir.x); float P = Px561, L = 14 * P, w = P * Mathf.Max(1f, thick); Vector2 nock = tip - dir * L, hb = tip - dir * 3 * P;
            // outline pass
            ZSeg(nock - dir * P, tip + dir * P, w + 2 * P, Ol561, 11);
            ZSeg(hb + n * 2f * P, hb - n * 2f * P, 3 * P, Ol561, 11);
            ZSeg(nock + dir * 4 * P + n * 2 * P, nock - n * 0f, 3 * P, Ol561, 11); ZSeg(nock + dir * 4 * P - n * 2 * P, nock, 3 * P, Ol561, 11);
            // shaft (bone tan), head (ivory, no white bar), fletching (red)
            ZSeg(nock, hb, w, B3_561, 12);
            ZSeg(hb + n * 1.5f * P, tip, P, B1_561, 13); ZSeg(hb - n * 1.5f * P, tip, P, B2_561, 13);
            ZSeg(nock + dir * 4 * P + n * 2 * P, nock, P, Fl605, 13); ZSeg(nock + dir * 4 * P - n * 2 * P, nock, P, Fl2_605, 13);
        }
        void Smear605(Vector2 tip, Vector2 dir, float lenPx)
        {   // fading motion smear behind the arrow (replaces the old white lane line)
            Vector2 nock = tip - dir * 14 * Px561; ZSeg(nock - dir * lenPx * Px561, nock, Px561, new Color(B3_561.r, B3_561.g, B3_561.b, 0.45f), 10);
        }
        void FxTick561(float dt)
        {
            for (int i = _fx561.Count - 1; i >= 0; i--)
            {
                var f = _fx561[i];
                if (f.delay > 0) { f.delay -= dt; if (f.delay > 0) continue; }
                if (f.k == F561.Arrow) { f.t += dt; if (ArrowStep594(f, dt) || f.t >= f.dur) { _fx561.RemoveAt(i); continue; } }
                else f.t += dt;
                float u = f.t / f.dur;
                if (u >= 1f)
                {
                    _fx561.RemoveAt(i); continue;
                }
                switch (f.k)
                {
                    case F561.Dust: { float s = 3 - Step561(f.t, f.dur, 3) * 3; ZDisc(f.a, Mathf.Max(1, s) * Px561, Dust561, 0.6f, -1); if (s > 2) ZDisc(f.a + Vector2.up * Px561, Px561, Dust2_561, 1, -1); break; }
                    case F561.Slash:
                    {   // v2 downward crescent: frame 1 high arc (-115..-5 deg), frame 2 low arc (-35..75 deg), thick head + white tip at the bottom end
                        bool f2 = f.t >= Fr561; float s = f.flip ? -1 : 1; Vector2 c = f.a + Vector2.up * 0.6f; float r = 11 * Px561;
                        float a0 = f2 ? -35 : -115, a1 = f2 ? 75 : -5; Vector2 prev = Vector2.zero;
                        for (int k = 0; k <= 8; k++)
                        {   // angle 0 = up, positive = clockwise toward facing side
                            float a = Mathf.Lerp(a0, a1, k / 8f) * Mathf.Deg2Rad; Vector2 p = c + new Vector2(Mathf.Sin(a) * s * r, Mathf.Cos(a) * r * 0.65f);
                            if (k > 0) { float w = Mathf.Lerp(1, 3, k / 8f) * Px561; ZSeg(prev, p, w + Px561, Ol561, 9); ZSeg(prev, p, w, k > 6 ? W561 : k > 3 ? B1_561 : B2_561, 10); }
                            prev = p;
                        }
                        break;
                    }
                    case F561.Star: { float px = (f.pierce ? 0.7f : 1f) * (f.t < Fr561 * 1.5f ? 6 : 4); Star561(f.a, px, 8, B1_561); if (f.t < Fr561) ZRing(f.a, 5 * Px561, B1_561, 1, 9); if (f.b != Vector2.zero) for (int k = 0; k < 3; k++) { Vector2 d = Quaternion.Euler(0, 0, -40 + k * 40) * f.b; ZDisc(f.a + d * (2 + u * 6) * Px561, Px561, B2_561, 1, 10); } break; }
                    case F561.Link: ZSeg(f.a, f.b, 2 * Px561, Ol561, 8); ZSeg(f.a, f.b, Px561, B1_561, 9); break;
                    case F561.Death:
                    {
                        if (f.t < Fr561) ZDisc(f.a + Vector2.up * 0.6f, 8 * Px561, W561, 1, 10);
                        DashRing561(f.a, Mathf.Lerp(4, 14, u) * Px561, Dust561, 10);
                        for (int k = 0; k < 9; k++) { float a = k * 40f * Mathf.Deg2Rad + 0.3f; Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a)); ZDisc(f.a + Vector2.up * (0.6f - u * 0.5f) + d * (3 + u * 14) * Px561, Px561, k % 2 == 0 ? B1_561 : B3_561, 1, 9); }
                        for (int k = 0; k < 4; k++) ZDisc(f.a + new Vector2((k - 1.5f) * 6, 0) * Px561, (3 - Step561(f.t, f.dur, 3) * 2) * Px561, Dust2_561, 0.6f, -1);
                        break;
                    }
                    case F561.Spawn:
                    {   // 0.3 s dashed grave ring closing + 6 bone motes converging, white frame, puffs
                        if (f.t < 0.3f) { float v = f.t / 0.3f; DashRing561(f.a, Mathf.Lerp(16, 6, v) * Px561, B1_561, 12); for (int k = 0; k < 6; k++) { float a = k * 60f * Mathf.Deg2Rad; ZDisc(f.a + Vector2.up * 0.5f + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (1 - v) * 14 * Px561, Px561, k % 2 == 0 ? B1_561 : B2_561, 1, 9); } }
                        else if (f.t < 0.3f + Fr561) ZDisc(f.a + Vector2.up * 0.6f, 9 * Px561, W561, 1, 10);
                        else { float s = 3 - Step561(f.t - 0.33f, 0.37f, 3) * 2; ZDisc(f.a + Vector2.left * 5 * Px561, s * Px561, B3_561, 0.6f, -1); ZDisc(f.a + Vector2.right * 5 * Px561, s * Px561, B3_561, 0.6f, -1); }
                        break;
                    }
                    case F561.BlinkOut: { if (f.t < Fr561) ZDisc(f.a + Vector2.up * 0.6f, 8 * Px561, W561, 1, 10); ZRing(f.a, Mathf.Lerp(4, 12, u) * Px561, B1_561, 0.5f, 1); for (int k = 0; k < 6; k++) { float a = k * 60f * Mathf.Deg2Rad; ZDisc(f.a + Vector2.up * 0.5f + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * u * 12 * Px561, Px561, B1_561, 1, 9); } break; }
                    case F561.BlinkIn: { float v = Mathf.Clamp01(f.t / 0.1f); if (f.t < 0.1f) for (int k = 0; k < 6; k++) { float a = k * 60f * Mathf.Deg2Rad; ZDisc(f.a + Vector2.up * 0.5f + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (1 - v) * 12 * Px561, Px561, B1_561, 1, 9); } else if (f.t < 0.1f + Fr561) ZDisc(f.a + Vector2.up * 0.6f, 8 * Px561, W561, 1, 10); else { ZDisc(f.a + Vector2.left * 4 * Px561, 2 * Px561, B3_561, 0.6f, -1); ZDisc(f.a + Vector2.right * 4 * Px561, 2 * Px561, B3_561, 0.6f, -1); } break; }
                    case F561.Spark: Star561(f.a, 4 - Step561(f.t, f.dur, 3) * 2, 4, B1_561); break;
                    case F561.Release: { ZDisc(f.a, Px561, W561, 1, 12); ZSeg(f.a + Vector2.left * 2 * Px561, f.a + Vector2.right * 2 * Px561, Px561, B1_561, 11); ZSeg(f.a + Vector2.down * 2 * Px561, f.a + Vector2.up * 2 * Px561, Px561, B1_561, 11); break; }
                    case F561.Arrow:
                    {
                        Vector2 d = (f.b - f.tip); float len = d.magnitude; if (len < 0.01f) d = (f.b - f.a); len = d.magnitude; if (len < 0.01f) break; d /= len;
                        Smear605(f.tip, d, f.pierce ? 8f : 4f); // t605u: no full-length white lane line
                        Arrow561(f.tip, d, f.pierce ? 2f : 1f); break;
                    }
                    case F561.Stuck: if (f.t < f.dur * 0.75f) Arrow561(f.a + f.b * 3 * Px561, f.b, 1f); break;
                }
            }
        }
    }
}
