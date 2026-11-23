using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

// t1563 (VH: "remake chain lightning to be as left click basic, other lightning skill need to research and rework later"): Chain Lightning as the run's
// LMB basic (Leizu's basic). It rides the t1517 basic path unchanged (fires only while LMB is held, its own cooldown, cursor ring / HUD sweep / not-ready
// click, t1528 tail cast + spark, origin = tail tip). The cast itself is D2-style scripted (no physics): from the tail tip toward the cursor, the first
// demon along the aim line (within chainLmbHitHalfWU1563 of it, up to chainLmbRangeU1563), else the demon nearest the aim line inside chainLmbConeDeg1563;
// then the OLD chain rules (chainJumps / chainJumpRangeU / chainJumpDmgKeep, Sins + Pact hooks, decor arcs, same bolt FX + sound).
// Nothing in reach = a short miss bolt to full reach so the click reads (no damage). The old auto Chain Lightning (CastChainLightning) is untouched and is
// still what a level-up Chain Lightning or chainLmb1563 = off uses.
namespace Hellord
{
    public partial class HellordGame
    {
        public int ChainLmbCasts1563, ChainLmbMisses1563, ChainLmbLastHits1563;
        bool IsLmbBasic1563(SkillId id)
        {
            var T = HellordTuning.I; if (T == null || T.lmbBasicIds1517 == null || System.Array.IndexOf(T.lmbBasicIds1517, id) < 0) return false;
            if (IsPb1567(id) && !PbOn1567(id)) return false; // t1567 pact basics (pbOn1567 / pbSkillOn1567)
            return id != SkillId.ChainLightning || T.chainLmb1563;
        }
        /// <summary>first target of the aimed bolt: nearest along the aim line, else nearest to the line inside the cone; -1 = miss.</summary>
        int ChainAimTarget1563(Vector2 o, Vector2 dir, float reach)
        {
            var T = HellordTuning.I; int best = -1; float bAlong = 1e9f; float hw = T.chainLmbHitHalfWU1563;
            if (BodyOn1595)
            {   // t1595: the drawn bolt line (cast origin -> reach) vs each demon's body box; nearest along the aim wins
                Vector2 e1 = o + dir * reach; float bw = T.chainBodyHalfWU1595;
                for (int i = 0; i < maxEnemies; i++) { var e = _en[i]; if (!e.alive || e.dying) continue; if (!BodyHit1595(e, o, e1, bw)) continue; float al = Vector2.Dot(BodyCentre1595(e) - o, dir); if (al < bAlong) { bAlong = al; best = i; } }
                if (best >= 0 || T.chainLmbConeDeg1563 <= 0f) return best;
            }
            else
            for (int i = 0; i < maxEnemies; i++)
            {
                var e = _en[i]; if (!e.alive || e.dying) continue; Vector2 v = e.pos + Vector2.up * e.hitY - o; float along = Vector2.Dot(v, dir); if (along < 0f || along > reach) continue;
                float perp = Mathf.Abs(dir.x * v.y - dir.y * v.x); if (perp <= hw && along < bAlong) { bAlong = along; best = i; }
            }
            if (best >= 0 || T.chainLmbConeDeg1563 <= 0f) return best;
            float bPerp = 1e9f, cosMax = Mathf.Cos(T.chainLmbConeDeg1563 * Mathf.Deg2Rad);
            for (int i = 0; i < maxEnemies; i++)
            {
                var e = _en[i]; if (!e.alive || e.dying) continue; Vector2 v = e.pos + Vector2.up * e.hitY - o; float m = v.magnitude; if (m < 0.001f || m > reach) continue;
                if (Vector2.Dot(v / m, dir) < cosMax) continue; float perp = Mathf.Abs(dir.x * v.y - dir.y * v.x); if (perp < bPerp) { bPerp = perp; best = i; }
            }
            return best;
        }
        /// <summary>the aimed LMB Chain Lightning. Always casts (hit = chain, nothing in reach = miss bolt) so the basic's cooldown, pose and sound always play.</summary>
        void CastChainAim1563(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; if (T.chainLmbCdMul1563 > 0f) s.cd *= T.chainLmbCdMul1563;
            Vector2 dir = AimDir(); if (dir.sqrMagnitude < 0.0001f) dir = _lastDir; dir.Normalize(); Vector2 o = CastOrigin1528(HeroCenter, ref dir); dir.Normalize();
            float reach = T.chainLmbRangeU1563; int first = ChainAimTarget1563(o, dir, reach); ChainLmbCasts1563++;
            _chainHit.Clear(); _chainPts.Clear(); _chainPts.Add(o);
            if (first < 0)
            {   // miss bolt: tail tip -> full reach along the aim, a few jittered points (deterministic per cast, no RNG state)
                int n = Mathf.Max(2, T.chainLmbMissPts1563); Vector2 nrm = new Vector2(-dir.y, dir.x);
                for (int k = 1; k < n; k++) { float u = k / (float)(n - 1); float j = k == n - 1 ? 0f : ((k & 1) == 0 ? 1f : -1f) * T.chainLmbMissJitterU1563; _chainPts.Add(o + dir * reach * u + nrm * j); }
                ChainLmbMisses1563++; ChainLmbLastHits1563 = 0;
            }
            else
            {
                int jumps = HData.At(T.chainJumps, s.level, 2) + SinChainJumps818();
                float range = s.R(HData.At(T.chainJumpRangeU, s.level, 3.2f)); int cur = first; float d = dmg;
                for (int k = 0; k <= jumps && cur >= 0; k++)
                {
                    var e = _en[cur]; _chainHit.Add(cur); _chainPts.Add(e.pos + Vector2.up * e.hitY);
                    Hit(e, d, s, Vector2.zero); d *= T.chainJumpDmgKeep;
                    if (k == 0) SinChainFirst818(s, d / Mathf.Max(0.01f, T.chainJumpDmgKeep), cur, range);
                    int nx = -1; float bd = range * range; Vector2 from = e.pos;
                    for (int i = 0; i < maxEnemies; i++) { var q0 = _en[i]; if (!q0.alive || q0.dying || _chainHit.Contains(i)) continue; float q = (q0.pos - from).sqrMagnitude; if (q < bd) { bd = q; nx = i; } }
                    cur = nx;
                }
                ChainLmbLastHits1563 = _chainHit.Count;
                SinChainAfter818(s, d); { Vector2 last = _chainPts[_chainPts.Count - 1]; PactConduit53(s, d, range); ChainBrk53(last, range, Mathf.Max(1, T.brkChainJumps53), d, _chainPts); }
            }
            if (T44.fxV44Pack && _chainPts.Count >= 2)
            {
                EnsureFX44();
                for (int a = 0; a < _chainPts.Count - 1; a += 15) { int m = Mathf.Min(16, _chainPts.Count - a); if (m < 2) break; var pts = new Vector2[m]; for (int k = 0; k < m; k++) pts[k] = _chainPts[a + k]; HeroSkillFX.ChainBolt(pts); }
            }
            else for (int a = 0; a < _chainPts.Count - 1; a += 6)
            {
                _chainSeg.Clear(); for (int k = a; k < _chainPts.Count && k < a + 7; k++) _chainSeg.Add(_chainPts[k]);
                if (_chainSeg.Count >= 2) HellFX.ChainLightning(_chainSeg, T.chainColor, T.chainBoltSeconds, T.chainHopDelay);
            }
            HellordAudio.Play("storm_ring"); // same reused clip as the old Chain Lightning
        }
    }
}
