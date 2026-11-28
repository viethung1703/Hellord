using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

namespace Hellord
{
    /// <summary>
    /// v0.0.36: 5 new skills (Chain Lightning, Hellfire Pillar, Blood Whip, Soul Spears, Grave Pool), 5 new supports
    /// (Executioner, Life Leech, Knockback, Soul Harvest, Explode on Kill) and the "which skill gets this support" second pick.
    /// All new skill visuals are PLACEHOLDERS built from the Pixel FX pack + tinted existing icons (see PhTint / PhTag).
    /// Every number lives in HellordTuning (v0.0.36 sections).
    /// </summary>
    public partial class HellordGame
    {
        // ================= tiny scheduler (delayed lashes / pillars) =================
        struct Sched { public float t; public System.Action a; }
        readonly List<Sched> _sched = new List<Sched>(16), _schedDue = new List<Sched>(16);
        void After(float seconds, System.Action a) { if (seconds <= 0.001f) a(); else _sched.Add(new Sched { t = seconds, a = a }); }
        float _leechHealed, _leechCap = 1f;
        void TickV36(float dt)
        {
            _leechHealed = Mathf.Max(0, _leechHealed - dt * _leechCap);
            if (_sched.Count == 0) return;
            _schedDue.Clear();
            for (int i = _sched.Count - 1; i >= 0; i--) { var q = _sched[i]; q.t -= dt; if (q.t <= 0) { _schedDue.Add(q); _sched.RemoveAt(i); } else _sched[i] = q; }
            for (int i = _schedDue.Count - 1; i >= 0; i--) _schedDue[i].a();
        }
        void ResetV36()
        {
            _sched.Clear(); _leechHealed = 0; _tp = null; _poolEver = false;
        }
        static float NewSkillDmgMul(int ix) { if (ix >= (int)SkillId.MagmaThrow) return FbDmgMul1481(ix); /* t1481 */ if (ix >= (int)SkillId.Frostbolt) { var ia = HellordTuning.I.iceDmgMul; int ik = ix - (int)SkillId.Frostbolt; return ia != null && ik < ia.Length ? ia[ik] : 1f; } /* t1393u ice skills */ var a = HellordTuning.I.newSkillDmgMul; int k = ix - 5; return a != null && k >= 0 && k < a.Length ? a[k] : 1f; }

        // ================= support hooks (called from Hit) =================
        float SupportsBeforeHit(Enemy e, float dmg, SkillInst s, ref Vector2 knock, bool tick)
        {
            _sinExec818 = false; if (s.sup.Count == 0) return dmg; var T = HellordTuning.I;
            if (s.Has(SupportId.Executioner))
            {   // bonus damage below an HP threshold (15% at Lv1 .. 25% at Lv3, t1654), not on elites/bosses by default
                int l = s.Lvl(SupportId.Executioner);
                if (e.maxHp > 0 && e.hp <= e.maxHp * (HData.At(T.supExecThreshold, l, 0.1f) + SinExecAdd818(e)) && !(T.executionerSkipBigs && (e.boss || e.elite))) { dmg *= HData.At(T.supExecDmg, l, 2f) * SinExecDmg818(); _sinExec818 = true; } // t818u #8 #29
            }
            if (s.Has(SupportId.Knockback) && !tick && !e.boss)
            {
                Vector2 d = knock.sqrMagnitude > 0.01f ? knock.normalized : (e.pos - _heroPos).normalized; knock += d * HData.At(T.supKnockback, s.Lvl(SupportId.Knockback), 2f) * SinKnock818(e); // t818u #9
            }
            // t1654: Life Leech heals from physical HITS only (no burn/bleed/DoT, no elemental)
            if (s.Has(SupportId.LifeLeech) && !tick && (s.def.tags & STag.Physical) != 0 && hp > 0 && mode == Mode.Play)
            {
                int l = s.Lvl(SupportId.LifeLeech); float cap = HData.At(T.supLeechCapPerSec, l, 1f) * SinLeechCap818(); _leechCap = cap; // t818u #34
                float lm = PactLeechMul53(); cap *= lm; float heal = Mathf.Min(dmg * HData.At(T.supLeechFrac, l, 0.02f) * lm, Mathf.Max(0, cap - _leechHealed));
                if (heal > 0) { _sinLeechNow818 = true; Heal53(heal); _sinLeechNow818 = false; _leechHealed += heal; AtLeech1651 += heal; /* t1651 probe */ } // v0.0.53 pact: heal mul / overheal shield / Blood Magic x3
            }
            return dmg;
        }
        void OnSkillKill(Enemy e, SkillInst s)
        {
            AtSkillKills1651++; /* t1651 probe */ PactOnKill51(e, s); // v0.0.51 damnation pact
            if (s.sup.Count == 0 || e.boss) return; var T = HellordTuning.I;
            if (s.Has(SupportId.SoulHarvest))
            {
                int l = s.Lvl(SupportId.SoulHarvest);
                DropPickup(e.pos + Random.insideUnitCircle * 0.4f, false, HData.At(T.supHarvestXp, l, 1)); AtKill1651 += HData.At(T.supHarvestXp, l, 1); // t1651 probe
                if (Random.value < HData.At(T.supHarvestBoneChance, l, 0.05f) + SinBoneChance818()) DropPickup(e.pos + Random.insideUnitCircle * 0.5f, true, 1);
            }
            if (s.Has(SupportId.ExplodeOnKill))
            {
                int l = s.Lvl(SupportId.ExplodeOnKill);
                float d = Mathf.Min(T.explodeMaxDmg, e.maxHp * HData.At(T.supExplodeDmg, l, 0.35f) + T.explodeFlatDmg);
                _blasts.Add(new Blast { pos = e.pos + Vector2.up * 0.4f, r = HData.At(T.supExplodeRadiusU, l, 1.4f), dmg = d, kind = 4, skip = e.id }); AtKill1651 += d; AtExplodes1651++; // t1651 probe
            }
        }

        // ================= Chain Lightning =================
        readonly List<int> _chainHit = new List<int>(12); readonly List<Vector3> _chainPts = new List<Vector3>(12), _chainSeg = new List<Vector3>(8);
        bool CastChainLightning(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; int first = PactPreferShocked53(Nearest(_heroPos, T.chainFirstRangeU, -1), T.chainFirstRangeU); first = ChainFirst716(first); if (first < 0 && NearBrk53(HeroCenter, T.chainFirstRangeU, new HashSet<int>()) < 0) { s.cd = 0.15f; return false; } // v0.0.53: decor alone is a valid target
            int jumps = HData.At(T.chainJumps, s.level, 2) + SinChainJumps818() + s.ChainExtra; /* t1651 Chain support */ // t818u #5
            float range = s.R(HData.At(T.chainJumpRangeU, s.level, 3.2f));
            _chainHit.Clear(); _chainPts.Clear(); _chainPts.Add(HeroCenter); int cur = first; float d = dmg;
            for (int k = 0; k <= jumps && cur >= 0; k++)
            {
                var e = _en[cur]; _chainHit.Add(cur); _chainPts.Add(e.pos + Vector2.up * e.hitY);
                Hit(e, d, s, Vector2.zero); d *= T.chainJumpDmgKeep;
                if (k == 0) SinChainFirst818(s, d / Mathf.Max(0.01f, T.chainJumpDmgKeep), cur, range); // t818u #30 split / #44 through you
                // next: nearest alive demon not yet struck, within the jump reach of this one
                int nx = -1; float bd = range * range; Vector2 from = e.pos;
                for (int i = 0; i < maxEnemies; i++) { var o = _en[i]; if (!o.alive || o.dying || _chainHit.Contains(i)) continue; float q = (o.pos - from).sqrMagnitude; if (q < bd) { bd = q; nx = i; } }
                cur = nx;
            }
            SinChainAfter818(s, d); { Vector2 last = _chainPts[_chainPts.Count - 1]; PactConduit53(s, d, range); ChainBrk53(last, _chainHit.Count == 0 ? T.chainFirstRangeU : range, Mathf.Max(1, T.brkChainJumps53), d, _chainPts); } // v0.0.53: arc into decor/torture props
            // visual: the pooled bolt takes at most 7 points, so long chains are drawn in overlapping segments
            if (T44.fxV44Pack && _chainPts.Count >= 2)
            {   // v0.0.44: HeroSkillFX bolt (16 points max per bolt, long chains split like before)
                EnsureFX44();
                for (int a = 0; a < _chainPts.Count - 1; a += 15)
                {
                    int m = Mathf.Min(16, _chainPts.Count - a); if (m < 2) break; var pts = new Vector2[m];
                    for (int k = 0; k < m; k++) pts[k] = _chainPts[a + k];
                    HeroSkillFX.ChainBolt(pts);
                }
            }
            else for (int a = 0; a < _chainPts.Count - 1; a += 6)
            {
                _chainSeg.Clear(); for (int k = a; k < _chainPts.Count && k < a + 7; k++) _chainSeg.Add(_chainPts[k]);
                if (_chainSeg.Count >= 2) HellFX.ChainLightning(_chainSeg, T.chainColor, T.chainBoltSeconds, T.chainHopDelay);
            }
            HellordAudio.Play("storm_ring"); // reused clip
            return true;
        }

        // ================= Hellfire Pillar =================
        readonly List<Vector2> _pillarAt = new List<Vector2>(8); readonly List<int> _pillarQ = new List<int>(48);
        bool CastHellfirePillar(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; if (CurAim720) return CastPillar716(s, dmg); Query(_heroPos, T.pillarRangeU, _sq); if (_sq.Count == 0) { s.cd = 0.25f; return false; }
            int n = HData.At(T.pillarCount, s.level, 1); float r = s.R(HData.At(T.pillarRadiusU, s.level, 1.3f)); _pillarAt.Clear();
            for (int p = 0; p < n; p++)
            {   // densest cluster out of a few random samples, not on top of an earlier pillar
                int best = -1, bestScore = -1;
                for (int k = 0; k < T.pillarSamples; k++)
                {
                    int ci = _sq[Random.Range(0, _sq.Count)]; Vector2 cp = _en[ci].pos; bool near = false;
                    foreach (var q in _pillarAt) if ((q - cp).sqrMagnitude < r * r * 1.2f) { near = true; break; }
                    if (near) continue;
                    Query(cp, r, _pillarQ); if (_pillarQ.Count > bestScore) { bestScore = _pillarQ.Count; best = ci; }
                }
                if (best < 0) break; _pillarAt.Add(_en[best].pos);
            }
            if (_pillarAt.Count == 0) { s.cd = 0.25f; return false; }
            for (int p = 0; p < _pillarAt.Count; p++) { Vector2 at = _pillarAt[p]; ClampToRoom(ref at, 4); var sk = s; float dd = dmg * T.pillarDmgMul; After(p * T.pillarStaggerSeconds, () => PillarTell(at, r, dd, sk)); }
            return true;
        }
        void PillarTell(Vector2 at, float r, float dmg, SkillInst s)
        {   // v0.0.44: HeroSkillFX.PillarCast draws the tell + eruption (RuneCircle placeholder removed); damage lands after pillarTellSeconds
            if (mode == Mode.StartPick) return;
            EnsureFX44(); PillarTell934(at, r); HeroSkillFX.PillarCast(at, r, 0f); // t934u mode 10 tell
            After(HellordTuning.I.pillarTellSeconds, () => { PillarImpact934(at, r); PillarErupt(at, r, dmg, s); }); // t934u PixelShock impact + crust
        }
        void PillarErupt(Vector2 at, float r, float dmg, SkillInst s)
        {
            if (mode == Mode.StartPick) return;
            dmg *= PactTrailEmp53(at); Query(at, r, _sq); SkillArea53(at, r, dmg, (int)s.def.id); foreach (int i in _sq) Hit(_en[i], dmg, s, Vector2.zero);
            HellordAudio.Play("fireball_impact"); // reused clip
        }

        // ================= Blood Whip =================
        bool CastBloodWhip(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; float reach = s.R(HData.At(T.whipReachU, s.level, 5f)), hw = HData.At(T.whipHalfWidthU, s.level, 0.6f) * s.RA(1f);
            if (Nearest(_heroPos, reach + 1f, -1) < 0 && !BrkNear53(_heroPos, reach + 1f) && !Aiming()) { s.cd = 0.1f; return false; }
            Vector2 dir = AimDir(); int n = HData.At(T.whipLashes, s.level, 1); var healed = new float[1]; var sk = s;
            HellordAudio.Play("whip_cast", T.whipSfxPitch); // v0.0.38: quiet reused frost-cleave clip, pitched up
            for (int k = 0; k < n; k++)
            {
                float a = (k - (n - 1) * 0.5f) * T.whipFanDeg; var d = (Vector2)(Quaternion.Euler(0, 0, a) * dir);
                After(k * T.whipLashDelay, () => WhipLash(d, reach, hw, dmg, sk, healed));
            }
            return true;
        }
        void WhipLash(Vector2 d, float reach, float hw, float dmg, SkillInst s, float[] healed)
        {
            if (mode != Mode.Play) return; var T = HellordTuning.I; Vector2 o = HeroCenter; EnsureFX44();
            Query(o + d * (reach * 0.5f), reach * 0.5f + hw + 0.5f, _sq); for (float a = 0.25f; a <= reach; a += Mathf.Max(0.4f, hw)) SkillArea53(o + d * a, hw + 0.3f, dmg, (int)s.def.id); float perHit = HData.At(T.whipHealPerHit, s.level, 0.4f);
            foreach (int i in _sq)
            {
                var e = _en[i]; Vector2 to = e.pos + Vector2.up * e.hitY - o; float along = Vector2.Dot(to, d), perp = Mathf.Abs(to.x * d.y - to.y * d.x);
                if (Has51(PactPath.BloodReaver, 2)) { if (to.magnitude > reach + e.radius) continue; } else if (along < -0.3f || along > reach + e.radius || perp > hw + e.radius) continue; // v0.0.53 Whip Reach: 360 deg
                Hit(e, dmg, s, d * T.whipKnock); SinWhipHit818(e, s);
                bool healedNow = false;
                if (hp > 0 && healed[0] < T.whipHealCapPerCast) { float h = Mathf.Min(perHit, T.whipHealCapPerCast - healed[0]); healed[0] += h; Heal53(h * SinWhipHeal818()); healedNow = h > 0; }
                HeroSkillFX.WhipHit(e.pos + Vector2.up * e.hitY, healedNow);
            }
            EnsureFX44(); if (Has51(PactPath.BloodReaver, 2)) for (int w = 0; w < 6; w++) HeroSkillFX.WhipLash(Quaternion.Euler(0, 0, w * 60f) * d, reach, hw, 0f); else HeroSkillFX.WhipLash(d, reach, hw, 0f); // v0.0.44: replaces the Blood beam placeholder
        }

        // ================= Soul Spears =================
        bool CastSoulSpears(SkillInst s, float dmg)
        {
            if (Nearest(_heroPos, 11, -1) < 0 && !BrkNear53(_heroPos, 11) && !Aiming()) { s.cd = 0.1f; return false; }
            var T = HellordTuning.I; Vector2 dir = AimDir(); Vector2 c = HeroCenter; Vector2 o1528 = CastOrigin1528(c + dir * 0.4f, ref dir); // t1528: the basic leaves the tail tip
            int n = HData.At(T.spearCount, s.level, 1) + HellordGame.PCount49 + s.ProjExtra; // t1651 stat
            int pierce = SinSpearPierce818() + HData.At(T.spearPierce, s.level, 1) + s.PierceN, bounce = s.BounceN; // t1651 stats
            float spread = n > 1 ? Mathf.Min(T.spearFanDeg, 150f / (n - 1)) : 0f;
            for (int i = 0; i < n; i++)
            {
                float a = (i - (n - 1) * 0.5f) * spread; var d = (Vector2)(Quaternion.Euler(0, 0, a) * dir);
                SpawnProj(s, o1528, d * T.spearSpeed * SinSpearSpeed818(), dmg, pierce, s.ForkN > 0, bounce, 0f);
            }
            if (!T44.fxV44Pack) ProjectileFX.CastFlash(c + dir * 0.5f, dir, ProjElement.Soul); Flash661(c + dir * 0.5f, ElemCol661(s)); SL667(SkillLightHooks.SoulSpearsMuzzle, c + dir * 0.5f, dir); // t661u/t667u muzzle light
            HellordAudio.Play("spear_cast", T.spearSfxPitch); // v0.0.38: quiet reused fireball-cast clip, pitched up
            return true;
        }

        // ================= Grave Pool =================
        Vector2 _lastPool; bool _poolEver;
        bool CastGravePool(SkillInst s, float dmg, float mul)
        {
            var T = HellordTuning.I; bool echo = mul < 0.99f;
            if (CurAim720 && !echo) { Vector2 ga = GroundAim716(); if (PactAnchorPool53(s, dmg, mul)) return true; GravePoolZone(ga, s.R(HData.At(T.gravePoolRadiusU, s.level, 1.3f)), HData.At(T.gravePoolSeconds, s.level, 3f), dmg, HData.At(T.graveSlow, s.level, 0.3f), s); _lastPool = ga; _poolEver = true; HellordAudio.Play("pool_cast", T.poolSfxPitch); return true; } // t716u: Manual = ground AoE at the cursor
            if (Nearest(_heroPos, 12f, -1) < 0 && !BrkNear53(_heroPos, 12f)) { s.cd = 0.3f; return false; }                                  // no demons around: keep the trail for when it matters
            if (!echo && !Has51(BoneWardenGone539, 2) && _poolEver && (_heroPos - _lastPool).magnitude < T.graveMinSpacingU) { s.cd = 0.15f; return false; } // pools mark where the hero WALKED
            int alive = _pools44.Count;
            if (alive >= T.graveMaxPools && !Has51(BoneWardenGone539, 2)) { s.cd = 0.3f; return false; }
            Vector2 at = _heroPos + (echo ? Random.insideUnitCircle * 1.2f : Vector2.zero); ClampToRoom(ref at, 4); if (PactAnchorPool53(s, dmg, mul)) return true; // v0.0.53 Grave Anchor
            GravePoolZone(at, s.R(HData.At(T.gravePoolRadiusU, s.level, 1.3f)), HData.At(T.gravePoolSeconds, s.level, 3f), dmg, HData.At(T.graveSlow, s.level, 0.3f), s);
            if (!echo) { _lastPool = _heroPos; _poolEver = true; }
            HellordAudio.Play("pool_cast", T.poolSfxPitch); // v0.0.38: quiet reused curse-rune clip, pitched down
            return true;
        }
        void GravePoolZone(Vector2 at, float r, float secs, float dmg, float slow, SkillInst s)
        {   // v0.0.44: HeroSkillFX.GravePool look (AuraField placeholder removed); ticks run in UpdatePools44 (slow + Hit, so supports apply)
            EnsureFX44();
            slow = SinPoolSlow818(slow); secs *= SinDuration737 * (s != null ? s.DurMul : 1f); // t818u #7 + Duration; t1651 duration stat
            int pk930 = _sfxMush930 && ShaderFx928(5) ? 5 : 1; // t930u ghost mushroom pool look
            _pools44.Add(new Pool44 { at = at, r = r, life = secs, tick = HellordTuning.I.graveTickSeconds, dmg = dmg, slow = slow, s = s, fx = ShaderFx928(pk930) ? -1 : HeroSkillFX.GravePool(at, r, secs), sfx928 = ShaderFxSpawn928(pk930, at, r, secs) }); // t928u: shader pool, pack pool = fallback
        }

        // ================= placeholder icons for the new skills =================
        /// <summary>24px skill-bar tile: SkillId order matches the approved sheet for the first five; new skills borrow a tile.</summary>
        static int BarTile(SkillId id) { switch (id) { case SkillId.ChainLightning: return (int)SkillId.StormRing; case SkillId.HellfirePillar: return (int)SkillId.Fireball; case SkillId.BloodWhip: return (int)SkillId.BoneOrbit; case SkillId.SoulSpears: return (int)SkillId.FrostCleave; case SkillId.GravePool: return (int)SkillId.CurseRune; default: return (int)id; } }
        static Color PhTint(SkillId id) { switch (id) { case SkillId.ChainLightning: return new Color(1f, 0.95f, 0.5f); case SkillId.HellfirePillar: return new Color(1f, 0.45f, 0.25f); case SkillId.BloodWhip: return new Color(1f, 0.4f, 0.45f); case SkillId.SoulSpears: return new Color(0.75f, 1f, 0.9f); case SkillId.GravePool: return new Color(0.55f, 1f, 0.55f); case SkillId.MagmaThrow: case SkillId.RollingMagmaRock: case SkillId.FirePillar: return new Color(1f, 0.7f, 0.45f); default: return Color.white; } }
        static string PhTag(SkillId id) { switch (id) { case SkillId.ChainLightning: return "CL"; case SkillId.HellfirePillar: return "HP"; case SkillId.BloodWhip: return "BW"; case SkillId.SoulSpears: return "SS"; case SkillId.GravePool: return "GP"; case SkillId.MagmaThrow: return "MT"; case SkillId.RollingMagmaRock: return "RR"; case SkillId.FirePillar: return "FP"; default: return null; } }

        // ================= which skill gets the support (second pick) =================
        class TargetPick { public SupportDef sd; public List<int> targets; public bool swap; public System.Action<int> onPick; }
        TargetPick _tp;
        /// <summary>Every owned skill this support could go to: it fits and either has an open slot or already owns it below max level (an upgrade).</summary>
        public List<int> SupportTargets(SupportDef sd)
        {
            var l = new List<int>(4); for (int i = 0; i < skills.Count; i++) if (skills[i].CanSocket(sd) == null) l.Add(i); AddKitTargets1651(sd, l, false); /* t1651 kit skills */ return l;
        }
        void OpenTargetPick(SupportDef sd, List<int> targets, bool swap, System.Action<int> onPick) { _tp = new TargetPick { sd = sd, targets = targets, swap = swap, onPick = onPick }; _sndHover = -1; HellordAudio.Play("ui_card_select"); }
        /// <summary>t1831u (VH): only supports the current skills can use are offered. When the player picks one, apply it
        /// if exactly one skill can take it; if more than one can, the WHICH SKILL? picker lists only those skills, in
        /// skill-bar order (SupportTargets already walks the bar then the kit slots). Zero matches should be impossible
        /// because of the offer filter: no empty picker (that would softlock), log it, and refund as bones or leave the
        /// card screen up so the player can skip / pick another card.</summary>
        void PickSupTarget1831u(SupportDef sd, List<int> valid, bool swap, System.Action<int> onPick)
        {
            if (sd == null) return;
            int n = valid != null ? valid.Count : 0;
            if (n == 1) { onPick(valid[0]); return; }
            if (n > 1) { OpenTargetPick(sd, valid, swap, onPick); return; }
            Debug.LogWarning("[t1831u] no skill can take " + sd.name + " (the offer filter should have prevented this)");
            HellordAudio.Play("ui_denied");
            if (mode == Mode.Chest)
            {
                bool boss = _bossReward37; int pay = boss ? HellordTuning.I.bossRewardBones : 15;
                bones += pay; cards.Clear(); warning = null; _bossReward37 = false; mode = Mode.Play;
                Toast(boss ? ("BOSS REWARD: +" + pay + " BONES") : ("Chest: no open support slot - got " + pay + " bones"));
                return;
            }
            if (mode == Mode.LevelUp || mode == Mode.StartPick) { warning = "NO SKILL CAN TAKE IT"; Toast("NO SKILL CAN TAKE " + sd.name); return; }
            Toast("NO SKILL CAN TAKE " + sd.name); // shrine shop already toasts and returns before this; safety net only
        }
        void ConfirmTarget(int idx)
        {
            var tp = _tp; if (tp == null || idx < 0 || idx >= tp.targets.Count) return; _tp = null; HellordAudio.Play("ui_card_select"); tp.onPick(tp.targets[idx]);
        }
        void UpdateTargetKeys()
        {
            var tp = _tp; if (tp == null) return;
            if (HInput.Down(HKey.Escape)) { _tp = null; return; } // back to the cards / the shop
            HKey[] keys = { HKey.One, HKey.Two, HKey.Three, HKey.Four, HKey.Five };
            for (int i = 0; i < tp.targets.Count && i < 5; i++) if (HInput.Down(keys[i])) { ConfirmTarget(i); return; }
        }
        void DrawTargetPickB()
        {
            if (HellordTuning.I.cardsD1831) { DrawTargetPickD1831(); return; } // t1831 frame D
            if (HellordTuning.I.cardsA1825) { DrawTargetPickA1825(); return; } // t1825 Option A
            var tp = _tp; if (tp == null) return; FloorTex(); SkillBarTex();
            DimScreen(new Color(HInk.r, HInk.g, HInk.b, 0.8f));
            TextC("WHICH SKILL?", CardScreenTitleY, HCyan, 2); TextC(Sanitize(tp.sd.name) + (tp.swap ? " - SWAP" : ""), CardScreenSubY, Color.white);
            int n = tp.targets.Count; int x0 = CardRowX0(n), rowY = CardRowY();
            var ev = Event.current; int picked = -1;
            var copies = new CardCopy[n]; var titles = new List<string>[n];
            int maxBody = 0, maxTitle = 1;
            for (int i = 0; i < n; i++)
            {
                int ti = tp.targets[i]; var sk = SupInst1651(ti);
                titles[i] = sk != null ? WrapCard(sk.def.name) : new List<string>();
                copies[i] = new CardCopy();
                if (sk == null) continue;
                int cur = sk.Lvl(tp.sd.id);
                if (tp.swap && sk.sup.Count > 0)
                {
                    copies[i].AddSection(CardTone.Header, "SWAP OUT");
                    copies[i].AddSection(CardTone.Stat, WrapCard(HData.Support(sk.sup[sk.sup.Count - 1]).name));
                }
                else copies[i].AddSection(CardTone.Header, cur > 0 ? $"UPGRADE LV {cur} - {cur + 1}" : "NEW LV 1");
                copies[i].AddSection(CardTone.Stat, WrapCard(HData.SupportEffect(tp.sd.id, tp.swap ? 1 : cur + 1, (int)sk.def.id)));
                copies[i].AddSection(CardTone.Need, $"SKILL LV {sk.level}");
                copies[i].AddMixed(false, CardTone.Need, WrapCard($"SLOTS {sk.sup.Count}/{Mathf.Min(sk.level, 4)}"));
                if (titles[i].Count > maxTitle) maxTitle = titles[i].Count;
                int bh = copies[i].BodyH(); if (bh > maxBody) maxBody = bh;
            }
            int plateH = CardPlateH(maxTitle), cardH = CardHeight(maxTitle, maxBody);
            for (int i = 0; i < n; i++)
            {
                int ti = tp.targets[i]; var sk = SupInst1651(ti); if (sk == null) continue;
                int x = x0 + i * (CardW + CardGap); var rect = new Rect(x, rowY, CardW, cardH);
                bool hover = rect.Contains(ev.mousePosition); if (ev.type == EventType.Repaint) { if (hover && _sndHover != i) { _sndHover = i; HellordAudio.Play("ui_card_select"); } else if (!hover && _sndHover == i) _sndHover = -1; }
                int y = rowY - (hover ? CardHover : 0);
                int artX = (1 * 2 + (hover ? 1 : 0)) * CardArtW;
                DrawCardChrome(artX, x, y, CardW, cardH, plateH);
                DrawCardTitle(x, y, plateH, titles[i]);
                SkillBarTex();
                int ix = BoxCenterX(x, CardW, CardIcon), iy = y + plateH + CardPad;
                StampCardWell(0, ix, iy);
                GUI.color = Color.white;
                if (IceIconBlit(sk.def.id, ix, iy, CardIcon)) { }
                else if (ZhCardIcon1625(sk.def.id) != null) GUI.DrawTexture(new Rect(ix, iy, CardIcon, CardIcon), ZhCardIcon1625(sk.def.id));
                else if (_icons44 && sk.def.id >= SkillId.ChainLightning && sk.def.id <= SkillId.PhantomAxes && _tSkill24 != null) BlitInset(_tSkill24, (int)sk.def.id * 28, 0, 24, 24, ix, iy, CardIcon, CardIcon);
                else
                {
                    var pt = PhTint(sk.def.id); GUI.color = new Color(pt.r, pt.g, pt.b, 1); BlitInset(_tSkillIcons, SkillIcon(sk.def.id) * 16, 0, 16, 16, ix, iy, CardIcon, CardIcon); GUI.color = Color.white;
                }
                int textY = y + plateH + CardPad + CardIcon + CardPad;
                DrawCardCopy(copies[i], x, textY);
                int cur = sk.Lvl(tp.sd.id); int lvl = tp.swap ? 1 : cur + 1;
                DrawCardDots(x, y, cardH, lvl, 1);
                DrawCardIndex(x, y, cardH, i);
                if (ev.type == EventType.MouseDown && ev.button == 0 && hover) { picked = i; ev.Use(); }
            }
            TextC($"{CardKeyRange1831(n)} PICK THE SKILL - ESC BACK", rowY + cardH + CardPad, CDim);
            if (picked >= 0) ConfirmTarget(picked);
        }
    }
}
