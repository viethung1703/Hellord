using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

// t818u: Demonic (#1-34) + Heaven-Defying (#35-45) sin EFFECTS (Docs/Hellord_sins_design_v0.4.md sec 5).
// Sin doc number n -> HSins.All id n + 24. Rolled numbers: SV(n, k) = k-th number of the effect text (rolled per copy).
// Fixed drawbacks: HellordTuning.sinDraw818[n-1] (D(n)). Every other number: HellordTuning sin*818 fields.
// Code-drawn / reused FX only (no new art). NOT play-tested in Unity.
namespace Hellord
{
    public partial class HellordGame
    {
        // ---- per-skill multipliers (static: SkillInst.CdMul / AreaMul read them)
        public static readonly float[] SinSkCd818 = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, SinSkArea818 = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        public static float SinSkCdAt818(SkillId id) { int i = (int)id; return i >= 0 && i < SinSkCd818.Length ? SinSkCd818[i] : 1f; }
        public static float SinSkAreaAt818(SkillId id) { int i = (int)id; return i >= 0 && i < SinSkArea818.Length ? SinSkArea818[i] : 1f; }
        readonly float[] _sinSkDmg818 = new float[16];
        readonly bool[] _sinOn818 = new bool[46];
        bool SinOn818(int n) => n > 0 && n < _sinOn818.Length && _sinOn818[n];
        float SV818(int n, int k)
        {
            int id = n + 24; if (id < 0 || id >= HSins.All.Count) return 0f; var d = HSins.All[id]; if (k < 0 || k >= d.mods.Count) return 0f;
            var q = _sinRoll737[id]; return d.mods[k].Val(q != null && k < q.Length ? q[k] : 0.5f);
        }
        static float D818(int n) { var a = ST737.sinDraw818; return a != null && n - 1 >= 0 && n - 1 < a.Length ? a[n - 1] : 0f; }
        float P818(int n, int k) => SV818(n, k) / 100f; // percent numbers

        // ---- run state
        int _sinKills818, _sinSouls818, _sinBossK818, _sinGlut818, _sinTrib818, _sinOrbHits818, _sin42Extra818;
        float _sinGhostT818, _sinGhostWave818, _sinGlutT818, _sinRingT818, _sinRingTick818, _sinRingDmg818, _sinMushT818, _sinGlintT818, _sinGlintLast818 = -99f;
        SkillInst _sinRingS818; bool _sinExec818, _sinLeechNow818, _sinStill818; Vector2 _sinLastPos818; Transform _sinAnchor818;
        class SinPatch818 { public Vector2 at; public float t, tick, dmg; public SkillInst s; }
        readonly List<SinPatch818> _sinPatch818 = new List<SinPatch818>();
        readonly Dictionary<SkillId, SkillInst> _sinDummy818 = new Dictionary<SkillId, SkillInst>();
        readonly List<int> _sinQ818 = new List<int>(64);

        void SinRunReset818()
        {
            _sinKills818 = _sinSouls818 = _sinBossK818 = _sinGlut818 = _sinTrib818 = _sinOrbHits818 = _sin42Extra818 = 0;
            _sinGhostT818 = _sinGhostWave818 = _sinGlutT818 = _sinRingT818 = _sinRingTick818 = _sinMushT818 = _sinGlintT818 = 0; _sinGlintLast818 = -99f;
            _sinRingS818 = null; _sinExec818 = _sinLeechNow818 = false; _sinPatch818.Clear();
            for (int i = 0; i < 16; i++) { SinSkCd818[i] = SinSkArea818[i] = 1f; _sinSkDmg818[i] = 1f; }
            for (int i = 0; i < _sinOn818.Length; i++) _sinOn818[i] = false;
        }

        /// <summary>Called at the start of SinRecalc737 (after SinLoad737): which Demonic/HD sins are on + per-skill multipliers.</summary>
        void SinRecalcPre818()
        {
            for (int i = 0; i < _sinOn818.Length; i++) _sinOn818[i] = false;
            if (ST737.sinEnabled737 && ST737.sinDemonicHdLive818)
                for (int i = 0; i < Mathf.Min(_sinSlots737, 3); i++)
                {
                    int id = _sinEq737[i]; if (id < 25 || id > 69 || id >= HSins.All.Count || !HSins.All[id].live) continue; _sinOn818[id - 24] = true;
                }
            for (int i = 0; i < 16; i++) { SinSkCd818[i] = SinSkArea818[i] = 1f; _sinSkDmg818[i] = 1f; }
            // cooldown
            if (SinOn818(2)) SinSkCd818[(int)SkillId.CurseRune] *= 1 + D818(2);
            if (SinOn818(14)) SinSkCd818[(int)SkillId.CurseRune] *= 1 + D818(14);
            if (SinOn818(10)) foreach (var d in HData.Skills) { int ix = (int)d.id; if (ix < 16) SinSkCd818[ix] *= (d.tags & STag.Fire) != 0 ? Mathf.Max(0.1f, 1 - P818(10, 0)) : 1 + D818(10); }
            if (SinOn818(30)) SinSkCd818[(int)SkillId.ChainLightning] *= 1 + D818(30);
            if (SinOn818(38)) SinSkCd818[(int)SkillId.Fireball] *= Mathf.Max(0.1f, D818(38));
            // area
            if (SinOn818(13)) SinSkArea818[(int)SkillId.StormRing] *= Mathf.Max(0.1f, 1 - D818(13));
            if (SinOn818(35)) SinSkArea818[(int)SkillId.StormRing] *= Mathf.Max(0.1f, 1 - D818(35));
            if (SinOn818(39)) SinSkArea818[(int)SkillId.CurseRune] *= Mathf.Max(0.1f, 1 - D818(39));
            if (SinOn818(15)) SinSkArea818[(int)SkillId.BoneOrbit] *= Mathf.Max(0.1f, 1 - D818(15));
            // damage
            void Dm(SkillId id, int n) { _sinSkDmg818[(int)id] *= Mathf.Max(0.05f, 1 - D818(n)); }
            if (SinOn818(5)) Dm(SkillId.ChainLightning, 5); if (SinOn818(7)) Dm(SkillId.GravePool, 7); if (SinOn818(11)) Dm(SkillId.BloodWhip, 11);
            if (SinOn818(22)) foreach (var d in HData.Skills) if ((d.tags & STag.Projectile) != 0 && (int)d.id < 16) _sinSkDmg818[(int)d.id] *= Mathf.Max(0.05f, 1 - D818(22));
            if (SinOn818(33)) Dm(SkillId.BoneOrbit, 33); if (SinOn818(36)) Dm(SkillId.SoulSpears, 36); if (SinOn818(40)) Dm(SkillId.HellfirePillar, 40);
            if (SinOn818(43)) Dm(SkillId.FrostCleave, 43); if (SinOn818(45)) Dm(SkillId.BoneOrbit, 45);
        }
        /// <summary>Fixed (non-rolled) stat drawbacks, folded into SinRecalc737's sums.</summary>
        void SinRecalcFixed818(ref float sp, ref float pk, ref float cr, ref float xp, ref float dash, float[] tg)
        {
            if (SinOn818(3)) pk -= D818(3); if (SinOn818(6)) cr -= D818(6); if (SinOn818(9)) sp -= D818(9); if (SinOn818(12)) xp -= D818(12);
            if (SinOn818(18)) sp -= D818(18); if (SinOn818(19)) dash -= D818(19); if (SinOn818(23)) sp -= D818(23); if (SinOn818(25)) xp -= D818(25);
            if (SinOn818(4)) tg[1] -= D818(4); if (SinOn818(27)) tg[0] -= D818(27);
            if (SinOn818(37)) sp -= D818(37) * _sinGlut818;
        }
        float SinSkDmg818(SkillInst s)
        {
            int ix = (int)s.def.id; float m = ix >= 0 && ix < 16 ? _sinSkDmg818[ix] : 1f;
            if (SinOn818(20)) m *= skills.Count > 0 && skills[0] == s ? 1 + P818(20, 0) : Mathf.Max(0.05f, 1 - D818(20));
            if (SinOn818(28) && (s.def.tags & STag.Minion) == 0) m *= Mathf.Max(0.05f, 1 - D818(28));
            if (SinOn818(32)) m *= 1 + Mathf.Min(P818(32, 1), P818(32, 0) * _sinBossK818);
            return m;
        }

        // ---- HD trigger glint (HUD sins frame)
        void SinGlint818() { if (runTime - _sinGlintLast818 < ST737.sinGlintCd818) return; _sinGlintLast818 = runTime; _sinGlintT818 = Mathf.Max(0.1f, ST737.sinGlintS818); }

        SkillInst SinSkill818(SkillId id) { foreach (var s in skills) if (s.def.id == id) return s; return null; }
        SkillInst SinInst818(SkillId id)
        {
            var o = SinSkill818(id); if (o != null) return o;
            if (!_sinDummy818.TryGetValue(id, out var d)) { d = new SkillInst { def = HData.Skill(id), level = 1 }; _sinDummy818[id] = d; }
            return d;
        }

        // ================= per-frame =================
        void SinTick818(float dt)
        {
            if (dt <= 0) return; var T = ST737;
            _sinStill818 = (_heroPos - _sinLastPos818).magnitude / dt < T.sin23StillSpeed818; _sinLastPos818 = _heroPos;
            if (_sinGlintT818 > 0) _sinGlintT818 -= dt;
            if (mode != Mode.Play) return;
            // #35 ring anchor follows the cursor (clamped like ground aim)
            if (SinOn818(35)) SinRingAnchor818().position = GroundAim716();
            // #36 ghost swarm
            if (_sinGhostT818 > 0)
            {
                _sinGhostT818 -= dt; _sinGhostWave818 -= dt;
                if (_sinGhostWave818 <= 0)
                {
                    _sinGhostWave818 = Mathf.Max(0.1f, T.sin36Interval818); var sk = SinInst818(SkillId.SoulSpears); float dm = SkillDamage(sk);
                    for (int k = 0; k < Mathf.Max(1, T.sin36PerWave818); k++)
                    {
                        int n = Nearest(_heroPos, 9f, -1); Vector2 dir = n >= 0 ? (_en[n].pos + Vector2.up * _en[n].hitY - HeroCenter).normalized : Random.insideUnitCircle.normalized; dir = (Vector2)(Quaternion.Euler(0, 0, Random.Range(-40f, 40f)) * dir);
                        SpawnProj(sk, HeroCenter + Random.insideUnitCircle * 0.6f, dir * HellordTuning.I.spearSpeed * 0.8f, dm, Mathf.Max(0, T.sin36Pierce818), false, 0, 0f);
                        var p = _lastProj53; if (p != null && p.s == sk) { p.sin818 = 2; p.homing = true; p.homingTurn = T.sin36Turn818; p.homingR = 8f; p.life = T.sin36Life818; p.sr.color = new Color(0.6f, 1f, 0.85f, 0.75f); ShaderGhost930(p); } // t930u ghost wisp
                    }
                }
            }
            // #37 Gluttony stacks fall off
            if (_sinGlut818 > 0 && (_sinGlutT818 -= dt) <= 0) { _sinGlut818--; _sinGlutT818 = T.sin37StackDecayS818; SinRecalc737(); }
            // #40 ring of fire
            if (_sinRingT818 > 0 && _sinRingS818 != null)
            {
                _sinRingT818 -= dt; float R = T.sin40R818 * _sinRingS818.AreaMul, W = T.sin40W818; Vector2 c = _heroPos + Vector2.up * 0.3f;
                if (!ShaderCity930(R, _sinRingT818)) for (int k = 0; k < 28; k++) { float a0 = k * Mathf.PI * 2 / 28, a1 = (k + 1) * Mathf.PI * 2 / 28; ZSeg(c + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0) * 0.75f) * R, c + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1) * 0.75f) * R, W * 0.6f, new Color(1f, 0.45f + 0.15f * Mathf.Sin(runTime * 9 + k), 0.1f, 0.75f), 2); }
                if ((_sinRingTick818 -= dt) <= 0)
                {
                    _sinRingTick818 = Mathf.Max(0.05f, T.sin40Tick818); Query(_heroPos, R + W, _sinQ818);
                    foreach (int i in _sinQ818) { var e = _en[i]; float d = (e.pos - _heroPos).magnitude; if (d >= R - W) Hit(e, _sinRingDmg818 * _sinRingTick818, _sinRingS818, (e.pos - _heroPos).normalized * 0.5f, true); }
                }
            }
            // #41 ghost mushrooms
            if (SinOn818(41)) { var gp = SinSkill818(SkillId.GravePool); if (gp != null && (_sinMushT818 -= dt) <= 0) { _sinMushT818 = Mathf.Max(0.3f, SV818(41, 0)); SinMushroom818(gp); } }
            // #19 dash burn patches
            for (int i = _sinPatch818.Count - 1; i >= 0; i--)
            {
                var q = _sinPatch818[i]; q.t -= dt; if (q.t <= 0) { _sinPatch818.RemoveAt(i); continue; }
                ZDisc(q.at, T.sin19R818, new Color(1f, 0.4f, 0.08f, 0.22f + 0.08f * Mathf.Sin(runTime * 10 + i)), 0.6f, 1);
                if ((q.tick -= dt) <= 0) { q.tick = Mathf.Max(0.05f, T.sin19Tick818); Query(q.at, T.sin19R818, _sinQ818); foreach (int k in _sinQ818) Hit(_en[k], q.dmg * q.tick, q.s, Vector2.zero, true); }
            }
        }
        Transform SinRingAnchor818() { if (_sinAnchor818 == null) { _sinAnchor818 = new GameObject("sin35_ring_anchor").transform; _sinAnchor818.position = _heroPos; } return _sinAnchor818; }

        // ================= hit hooks =================
        float SinPreHit818(Enemy e, float dmg, SkillInst s, bool tick)
        {
            if (SinOn818(4) && e.burnT > 0) dmg *= 1 + P818(4, 1);
            if (SinOn818(6) && e.bleedT > 0 && (s.def.tags & (STag.Physical | STag.Bone | STag.Blood)) != 0) dmg *= 1 + P818(6, 0);
            if (SinOn818(8) && e.maxHp > 0 && e.hp > e.maxHp * 0.5f) dmg *= Mathf.Max(0.05f, 1 - D818(8));
            if (SinOn818(9) && e.knock818 > runTime) dmg *= 1 + P818(9, 1);
            if (SinOn818(27) && e.freezeT > 0) dmg *= 1 + P818(27, 0);
            return dmg;
        }
        void SinPostHit818(Enemy e, float dmg, SkillInst s, bool tick)
        {
            if (SinOn818(13) && !tick && s.def.id == SkillId.StormRing && e.alive && Random.value < P818(13, 0)) ApplyStatus(e, Status.Bleed, dmg, 1f, s);
            if (SinOn818(21) && s.def.id == SkillId.SkeletonArchers && e.alive) ApplyStatus(e, Status.Burn, dmg, 1f, s);
            if (SinOn818(29) && _sinExec818 && !e.alive) s.cd = Mathf.Max(0, s.cd - s.def.cooldown * s.CdMul * P818(29, 0));
            _sinExec818 = false;
        }
        float SinExecAdd818(Enemy e) => SinOn818(8) && (e.burnT > 0 || e.bleedT > 0) ? P818(8, 0) : 0f;
        float SinExecDmg818() => SinOn818(29) ? Mathf.Max(0.05f, 1 - D818(29)) : 1f;
        float SinKnock818(Enemy e) { if (!SinOn818(9)) return 1f; e.knock818 = runTime + Mathf.Max(0.1f, SV818(9, 2)); return 1 + P818(9, 0); }

        // ---- statuses
        float SinBurnDps818(Enemy e, float nb)
        {
            float old = e.burnT > 0 ? e.burnDps : 0;
            if (!SinOn818(31) || old <= 0) { e.burnStk818 = 1; return Mathf.Max(old, nb); }
            int mx = Mathf.Max(1, Mathf.RoundToInt(SV818(31, 0))); float per = Mathf.Max(old / Mathf.Max(1, e.burnStk818), nb); e.burnStk818 = Mathf.Min(mx, e.burnStk818 + 1); return per * e.burnStk818;
        }
        float SinBurnDurMul818() => SinOn818(31) ? Mathf.Max(0.1f, 1 - D818(31)) : 1f;
        float SinBurnAddS818() => SinOn818(4) ? SV818(4, 0) : 0f;
        float SinShockDur818() => SinOn818(24) ? (3f + SV818(24, 0)) / 3f : 1f;
        float SinShockAmp818() => SinOn818(24) ? 1 + P818(24, 1) : 1f;
        float SinBleedStill818() => SinOn818(23) && _sinStill818 ? 1 + P818(23, 0) : 1f;
        float SinHurtMul818()
        {
            if (!SinOn818(24)) return 1f; Query(_heroPos, ST737.sin24R818, _sinQ818);
            foreach (int i in _sinQ818) if (_en[i].shockT > 0) return 1 + D818(24); return 1f;
        }
        float SinShatterMul818() => SinOn818(27) ? 1 + P818(27, 0) : 1f;
        float SinCurseBlastR818() => SinOn818(26) ? 1 + P818(26, 1) : 1f;
        float SinCurseBlastDmg818() => SinOn818(26) ? 1 + P818(26, 0) : 1f;
        float SinBossHpMul818() => SinOn818(32) ? 1 + D818(32) : 1f;

        // ---- kills
        void SinCurseKill818(Enemy e, Vector2 c)
        {
            if (mode != Mode.Play) return;
            if (SinOn818(26) && (HeroCenter - c).magnitude < ST737.sin26R818 * SinCurseBlastR818()) HurtHero(D818(26), "Unmourned Child");
            if (SinOn818(39) && !e.boss && !e.nest)
            {
                int n = 0; foreach (var m in _nmin536) if (m.sin818) n++;
                if (n < ST737.sin39MaxRisen818) { int before = _nmin536.Count; NSpawn536(0, true, e.pos, Mathf.Max(0.5f, SV818(39, 0)) * SinDuration737); if (_nmin536.Count > before) { _nmin536[_nmin536.Count - 1].sin818 = true; SinGlint818(); } }
            }
        }
        bool SinRisen818() { if (SinOn818(39)) return true; foreach (var m in _nmin536) if (m.sin818) return true; return false; }
        void SinKill818(Enemy e)
        {
            if (mode != Mode.Play) return; _sinKills818++;
            if (SinOn818(3) && Random.value < P818(3, 0)) Heal53(SV818(3, 1));
            if (SinOn818(14) && _sinKills818 % Mathf.Max(1, Mathf.RoundToInt(SV818(14, 0))) == 0)
            {
                SinBell818(e.pos);
            }
            if (SinOn818(36) && _sinGhostT818 <= 0 && ++_sinSouls818 >= Mathf.Max(1, Mathf.RoundToInt(SV818(36, 0))))
            { _sinSouls818 = 0; _sinGhostT818 = Mathf.Max(1f, SV818(36, 1)) * SinDuration737; _sinGhostWave818 = 0; SinGlint818(); Toast("THE GHOST BANNER UNFURLS"); }
        }
        void SinBell818(Vector2 at) // t930u: split out of SinKill818 so the autotest can ring it; body unchanged
        {
            var sk = SinInst818(SkillId.CurseRune); float r = CurseRadius738(sk), d = SkillDamage(sk); ClampToRoom(ref at, 4);
            System.Action cast = () => { RuneBurst(at, r, d, sk); CurseZone(at, r, d, sk); };
            if (!Bell934(at, CursePx738(r), 0.6f, cast) && !ShaderBell930(at, CursePx738(r), 0.6f, cast)) HellFX.RuneCircle(at, CursePx738(r), new Color(0.75f, 0.4f, 1f), 0.6f, cast); // t930u Death Bell shader telegraph, same callback point
        }
        void SinBone818(int v) { if (SinOn818(12) && v > 0) AddXp(Mathf.RoundToInt(SV818(12, 1)) * v); }
        float SinBoneChance818() => SinOn818(12) ? P818(12, 0) : 0f;

        // ---- skills
        float SinEchoCd818(SkillInst s) => SinOn818(17) && s.Has(SupportId.Echo) ? 1 + D818(17) : 1f;
        bool SinNoEcho818(SkillInst s) => SinOn818(39) && s.def.id == SkillId.CurseRune;
        float SinEchoDmg818() => SinOn818(17) ? 1 + P818(17, 0) : 1f;
        float SinRuneR818() => SinOn818(2) ? 1 + P818(2, 0) : 1f;
        float SinRuneDmg818() => SinOn818(2) ? 1 + P818(2, 1) : 1f;
        int SinSpearPierce818() => SinOn818(1) ? Mathf.RoundToInt(SV818(1, 0)) : 0;
        float SinSpearSpeed818() => SinOn818(1) ? Mathf.Max(0.1f, 1 - D818(1)) : 1f;
        float SinPoolSlow818(float slow) => SinOn818(7) ? Mathf.Clamp(slow * (1 + P818(7, 0)), 0f, 0.9f) : slow;
        int SinChainJumps818() => SinOn818(5) ? Mathf.RoundToInt(SV818(5, 0)) : 0;
        float SinWhipHeal818() => SinOn818(11) ? 1 + P818(11, 0) : 1f;
        float SinForkDmg818() => SinOn818(16) ? 1 + P818(16, 0) : 1f;
        float SinLeechCap818() => SinOn818(34) ? 1 + P818(34, 0) : 1f;
        float SinShieldCap818() => SinOn818(34) ? P818(34, 1) : 0f;
        float SinHealMul818() => SinOn818(34) && !_sinLeechNow818 ? Mathf.Max(0f, 1 - D818(34)) : 1f;
        int SinExtraCard818()
        {
            if (!SinOn818(25)) return 0; int every = Mathf.Max(1, Mathf.RoundToInt(SV818(25, 1)));
            return level % every == 0 ? Mathf.Clamp(Mathf.RoundToInt(SV818(25, 0)), 0, 2) : 0; // draw supports up to 5 cards
        }
        void SinOrbit818(ref int n, ref float orad, ref float rev)
        {
            if (SinOn818(15)) { n += Mathf.RoundToInt(SV818(15, 0)); orad *= SinSkAreaAt818(SkillId.BoneOrbit); }
            if (SinOn818(45)) { n = Mathf.Max(2, Mathf.RoundToInt(SV818(45, 0))); orad *= ST737.sin45RMul818; rev *= Mathf.Max(0.05f, ST737.sin45Spin818); }
        }
        void SinOrbitHit818(SkillInst s, Enemy e, float dmg)
        {
            if (SinOn818(45)) SinGlint818();
            if (!SinOn818(33)) return;
            if (++_sinOrbHits818 % Mathf.Max(1, Mathf.RoundToInt(SV818(33, 0))) != 0) return;
            var sp = SinInst818(SkillId.SoulSpears); Vector2 dir = (e.pos + Vector2.up * e.hitY - HeroCenter); if (dir.sqrMagnitude < 0.01f) dir = Vector2.right; dir.Normalize();
            SpawnProj(sp, e.pos + Vector2.up * e.hitY, dir * HellordTuning.I.spearSpeed, SkillDamage(sp), HData.At(HellordTuning.I.spearPierce, sp.level, 1), false, 0, 0f, e.id);
        }
        void SinSpawnProj818(Proj p, int lastHit)
        {
            if (SinOn818(16) && p.fork) p.vel *= Mathf.Max(0.1f, 1 - D818(16));
            if (SinOn818(22) && lastHit < 0) p.bounce += Mathf.RoundToInt(SV818(22, 0));
        }
        void SinProjHit818(Proj p, Enemy e)
        {
            if (p.sin818 == 1 && e.alive) ApplyStatus(e, Status.Burn, p.dmg, 1f, p.s);
            if (p.sin818 == 3 && p.sinN818 < ST737.sin38MaxGrow818)
            {   // sun grows per demon passed (pre-compensates the pierce falloff applied right after)
                var T = ST737; p.sinN818++; p.dmg *= (1 + T.sin38GrowDmg818) / Mathf.Max(0.05f, 1 - HellordTuning.I.pierceFalloff); p.explodeR += T.sin38GrowR818;
                p.sr.transform.localScale = Vector3.one * (1.6f + 0.05f * p.sinN818);
            }
            if (p.sin818 == 3) SunPierce934(p); // t934u pierce flash
        }
        void SinSun818(SkillInst s, float dmg, Vector2 c)
        {
            var T = HellordTuning.I; Vector2 dir = AimDir(); float expl = HData.At(T.fbSplashRadiusU, s.level, 0.8f) * s.AreaMul;
            SpawnProj(s, c + dir * 0.4f, dir * 11f * Mathf.Max(0.05f, T.sin38Speed818), dmg, 9999, false, 0, expl);
            var p = _lastProj53; if (p != null && p.s == s) { p.sin818 = 3; p.life = T.sin38Life818; p.homing = false; p.sr.transform.localScale = Vector3.one * 1.6f; p.sr.color = new Color(1f, 0.9f, 0.45f); Sun934(p); } // t934u v2 sun
            ProjectileFX.CastFlash(c + dir * 0.5f, dir, ProjElement.Fire); HellordAudio.Play("fireball_cast"); SinGlint818();
        }
        void SinFireRing818(SkillInst s, float dmg)
        {
            _sinRingS818 = s; _sinRingDmg818 = dmg * HellordTuning.I.pillarDmgMul * (FireWallOn948 ? HellordTuning.I.sin40WallDmgMul948 : 1f); /* t948u -30% with Fire Wall */ _sinRingT818 = ST737.sin40Secs818 * SinDuration737; _sinRingTick818 = 0;
            HellordAudio.Play("fireball_impact"); SinGlint818();
        }
        void SinMushroom818(SkillInst gp)
        {
            var T = HellordTuning.I; Vector2 at = _heroPos + Random.insideUnitCircle * 1.5f; ClampToRoom(ref at, 4); float dmg = SkillDamage(gp), r = T.sin41R818 * gp.AreaMul;
            _sfxMush930 = true; GravePoolZone(at, r, T.sin41Secs818, dmg, HData.At(T.graveSlow, gp.level, 0.3f), gp); _sfxMush930 = false; // t930u: kind 5 mushroom pool
            Query(at, r, _sinQ818); foreach (int i in _sinQ818) { var e = _en[i]; Hit(e, dmg * T.sin41Burst818, gp, Vector2.zero); if (e.alive) ApplyStatus(e, Status.Bleed, dmg, 1f, gp); } // Bleed = poison stand-in (no Poison status yet)
            HordeFX.CheapBurst(at + Vector2.up * 0.3f, ShaderFx928(5) ? T.fxMushBurstCol930 : new Color(0.55f, 0.9f, 0.35f), 6);
            hp = Mathf.Max(1f, hp - hp * D818(41)); SinGlint818(); // non-lethal
        }
        void SinWhipHit818(Enemy e, SkillInst s)
        {
            if (!SinOn818(37) || e.boss) return;
            if (e.alive && !e.elite) e.push += (_heroPos - e.pos).normalized * ST737.sin37Pull818;
            if (e.alive && e.maxHp > 0 && e.hp <= e.maxHp * P818(37, 0)) { Damage(e, e.hp + 1f, false, false, true); if (!e.alive) { _sinGlut818 = Mathf.Min(ST737.sin37StackMax818, _sinGlut818 + 1); _sinGlutT818 = ST737.sin37StackDecayS818; SinRecalc737(); SinGlint818(); } }
        }
        void SinChainFirst818(SkillInst s, float d, int first, float range)
        {
            if (first < 0) return; var e0 = _en[first];
            if (SinOn818(30))
            {
                int n = Mathf.RoundToInt(SV818(30, 0)); float dd = d * P818(30, 1);
                for (int k = 0; k < n; k++)
                {
                    int t = -1; float bd = range * range; for (int i = 0; i < maxEnemies; i++) { var o = _en[i]; if (!o.alive || o.dying || _chainHit.Contains(i)) continue; float q = (o.pos - e0.pos).sqrMagnitude; if (q < bd) { bd = q; t = i; } }
                    if (t < 0) break; _chainHit.Add(t); var e = _en[t]; Hit(e, dd, s, Vector2.zero);
                    EnsureFX44(); HeroSkillFX.ChainBolt(new[] { e0.pos + Vector2.up * e0.hitY, e.pos + Vector2.up * e.hitY }); if (k == 0) SplitNode934(e0.pos + Vector2.up * e0.hitY); // t934u
                }
            }
            if (SinOn818(44))
            {   // the bolt passes through you: costs HP, builds Tribulation charges
                _chainPts.Add(HeroCenter); hp = Mathf.Max(1f, hp - maxHp * D818(44)); SinGlint818();
                if (++_sinTrib818 >= Mathf.Max(1, Mathf.RoundToInt(SV818(44, 0))))
                {
                    _sinTrib818 = 0; var T = ST737; Vector2 at = e0.pos; Query(at, T.sin44TribR818, _sinQ818);
                    foreach (int i in _sinQ818) Hit(_en[i], d * T.sin44TribMul818, s, Vector2.zero);
                    EnsureFX44(); HeroSkillFX.ChainBolt(new[] { at + Vector2.up * 7f, at + Vector2.up * 3f + Vector2.right * 0.4f, at }); HeroSkillFX.ChainBolt(new[] { HeroCenter + Vector2.up * 7f, HeroCenter });
                    Tribulation934(at, T.sin44TribR818); ScreenShake.Add(5, 0.35f); HurtHero(maxHp * T.sin44TribSelf818, "Tribulation"); Toast("HEAVENLY TRIBULATION"); // t934u v2 ripple + scorch
                }
            }
        }
        void SinChainAfter818(SkillInst s, float d) { }

        // ---- dash / minions
        void SinDash818()
        {
            if (!SinOn818(19)) return; if (NezhaDashStart934()) return; var fb = SinInst818(SkillId.Fireball); // t934u Fire Wheel (off = old patch)
            _sinPatch818.Add(new SinPatch818 { at = _heroPos, t = Mathf.Max(0.3f, SV818(19, 0)) * SinDuration737, tick = 0, dmg = SkillDamage(fb) * P818(19, 1), s = fb });
        }
        int SinWarCount818(int kind, int n)
        {
            if (kind != 0) return n; if (!SinOn818(42)) { _sin42Extra818 = 0; return n; }
            int cap = Mathf.Max(1, Mathf.RoundToInt(D818(42))); _sin42Extra818 = Mathf.Max(0, n - cap); return Mathf.Min(n, cap);
        }
        float SinMinRate818(int kind) => kind == 1 && SinOn818(21) ? 1 + D818(21) : 1f;
        float SinMinHp818(int kind)
        {
            float m = 1f; if (kind == 0 && SinOn818(18)) m *= 1 + P818(18, 0);
            if (kind == 0 && SinOn818(42)) m *= Mathf.Max(1f, SV818(42, 1)); return m;
        }
        float SinMinDmg818(int kind)
        {
            float m = 1f; if (kind == 1 && SinOn818(21)) m *= 1 + P818(21, 0); if (SinOn818(28)) m *= 1 + P818(28, 1);
            if (kind == 0 && SinOn818(42)) m *= 1 + _sin42Extra818 * ST737.sin42ExtraDmg818; return m;
        }
        float SinResp818() => SinOn818(28) ? 1 + P818(28, 0) : 1f;
        bool SinFreeBlock818(NMin536 m)
        {
            if (m.kind != 0 || !SinOn818(18)) return false;
            if (runTime - m.blk818 >= ST737.sin18BlockWindowS818) { m.blk818 = runTime; m.blkN818 = 0; }
            if (m.blkN818 < Mathf.RoundToInt(SV818(18, 1))) { m.blkN818++; return true; } return false;
        }
        void SinCleave818(Enemy e, int t, float dmg, SkillInst sk)
        {
            if (!SinOn818(42)) return; Query(e.pos, ST737.sin42CleaveR818, _sinQ818);
            bool any = false; foreach (int i in _sinQ818) if (i != t) { Hit(_en[i], dmg * ST737.sin42CleaveMul818, sk, (_en[i].pos - e.pos).normalized * 0.6f); any = true; }
            if (any) SinGlint818();
        }

        // ================= HUD: sins frame above the belt =================
        // t902u follow-up v0.0.72: VH-approved step49d/step50p slot24 sprites (ui_v52_sin_slots_24 + ui_v52_sin_frame), HD running light loops
        Texture2D _tSinSlots903, _tSinFrame903; bool _sinSlotsTried903; float _sinRunT903;
        void SinSprite903(Texture2D t, float x, float y, int col, int row, float cell)
        {
            GUI.color = Color.white; float tw = t.width, th = t.height;
            GUI.DrawTextureWithTexCoords(new Rect(x, y, cell, cell), t, new Rect(col * cell / tw, 1f - (row + 1) * cell / th, cell / tw, cell / th));
        }
        void DrawSinHud818(float beltX, float beltTop)
        {
            if (!ST737.sinEnabled737) return; SinLoad737(); var T = HellordTuning.I;
            if (!_sinSlotsTried903) { _sinSlotsTried903 = true; _tSinSlots903 = UiTex("v52_sin_slots_24"); _tSinFrame903 = UiTex("v52_sin_frame"); }
            if (!T.sinSlotSprites903 || _tSinSlots903 == null || _tSinFrame903 == null) { DrawSinHudOld818(beltX, beltTop); return; }
            float cell = T.sinSlotCell903, w = _tSinFrame903.width, h = _tSinFrame903.height, x = beltX + T.sinFrameOffX903, y = beltTop - ST737.sinHudGap818 - h;
            GUI.color = Color.white; GUI.DrawTexture(new Rect(x, y, w, h), _tSinFrame903);
            _sinRunT903 += Event.current.type == EventType.Repaint ? Time.unscaledDeltaTime : 0;
            int nGl = Mathf.Max(1, T.sinRunFrames903); float frS = Mathf.Max(0.01f, T.sinRunFrameS903), cyc = nGl * frS + Mathf.Max(0, T.sinRunPauseS903);
            float ph = Mathf.Repeat(_sinRunT903, cyc); int gf = ph < nGl * frS ? Mathf.FloorToInt(ph / frS) : -1;
            for (int i = 0; i < 3; i++)
            {
                float sx = x + T.sinSlotOrigin903 + i * T.sinSlotStride903, sy = y + T.sinSlotOrigin903;
                if (i >= _sinSlots737) { SinSprite903(_tSinSlots903, sx, sy, 4, 0, cell); continue; }
                int id = _sinEq737[i];
                if (id < 0 || id >= HSins.All.Count) { SinSprite903(_tSinSlots903, sx, sy, 3, 0, cell); continue; }
                var d = HSins.All[id]; int tier = d.tier == SinTier.HeavenDefying ? 2 : d.tier == SinTier.Demonic ? 1 : 0;
                int col = tier == 2 ? 0xffd34a : tier == 1 ? 0xd8364a : 0x9aa6b8;
                SinSprite903(_tSinSlots903, sx, sy, tier, 0, cell);
                int ic = SinIconCol902(d.name); float io = T.sinIconInset903;
                if (ic >= 0) { float ip = T.sinIconPx902, c2 = Mathf.Max(1, T.sinIconCell902), tw = _tSinIcons902.width; GUI.color = Color.white;
                    GUI.DrawTextureWithTexCoords(new Rect(sx + io + T.sinIconOffX902, sy + io + T.sinIconOffY902, ip, ip), _tSinIcons902, new Rect(ic * c2 / tw, 0, c2 / tw, 1)); }
                else if (T.sinSlotText903) { string ab = SinAbbr818(d.name); Text(ab, sx + Mathf.Floor((cell - ab.Length * 4 + 1) / 2), sy + Mathf.Floor((cell - 5) / 2), C716(col)); }
                if (tier == 2 && T.sinRunLight903 && gf >= 0) SinSprite903(_tSinSlots903, sx, sy, gf, 1, cell);
            }
            SinGlintDraw818(x, y, w, h); GUI.color = Color.white;
        }
        void SinGlintDraw818(float x, float y, float w, float h)
        {
            if (_sinGlintT818 > 0 && ST737.sinFrameGlint934)
            {   // one-shot trigger glint around the whole sins frame (t818u); t934u VH: off by default
                float t = 1f - _sinGlintT818 / Mathf.Max(0.1f, ST737.sinGlintS818), per = 2 * (w + h);
                for (int k = 0; k < 6; k++) { var pt = SinPerim818(x, y, w, h, Mathf.Repeat(t * per - k * 2, per)); Fill716(pt.x - 1, pt.y - 1, 2, 2, new Color(1f, 0.97f, 0.8f, 1f - k / 6f)); }
            }
        }
        // old v0.0.71 drawing, fallback when the sprites are missing or sinSlotSprites903 = false
        void DrawSinHudOld818(float beltX, float beltTop)
        {
            if (!ST737.sinEnabled737) return; SinLoad737();
            float sz = Mathf.Max(8, ST737.sinHudSlot818), g = 2, w = 3 * sz + 2 * g + 4, h = sz + 4, x = beltX, y = beltTop - ST737.sinHudGap818 - h;
            Fill716(x, y, w, h, C716(0x0e0b14, 0.82f)); Frame716(x, y, w, h, C716(0x5a4a6a));
            for (int i = 0; i < 3; i++)
            {
                float sx = x + 2 + i * (sz + g), sy = y + 2;
                if (i >= _sinSlots737) { Fill716(sx, sy, sz, sz, C716(0x050308, 0.9f)); Fill716(sx + sz / 2 - 2, sy + sz / 2, 4, 1, C716(0x3a3040)); continue; }
                int id = _sinEq737[i]; Fill716(sx, sy, sz, sz, C716(0x1a1422, 0.95f));
                if (id < 0 || id >= HSins.All.Count) { Frame716(sx, sy, sz, sz, C716(0x3a3040)); continue; }
                var d = HSins.All[id]; int col = d.tier == SinTier.HeavenDefying ? 0xffd34a : d.tier == SinTier.Demonic ? 0xd8364a : 0x9aa6b8;
                Frame716(sx, sy, sz, sz, C716(col)); Fill716(sx + 1, sy + 1, sz - 2, 2, C716(col, 0.35f));
                int ic = SinIconCol902(d.name);
                if (ic >= 0) { var T9 = HellordTuning.I; float ip = T9.sinIconPx902, cell = Mathf.Max(1, T9.sinIconCell902), tw = _tSinIcons902.width; GUI.color = Color.white;
                    GUI.DrawTextureWithTexCoords(new Rect(sx + Mathf.Floor((sz - ip) / 2) + T9.sinIconOffX902, sy + Mathf.Floor((sz - ip) / 2) + T9.sinIconOffY902, ip, ip), _tSinIcons902, new Rect(ic * cell / tw, 0, cell / tw, 1));
                    Frame716(sx, sy, sz, sz, C716(col)); continue; }
                string ab = SinAbbr818(d.name); Text(ab, sx + Mathf.Floor((sz - ab.Length * 4 + 1) / 2), sy + Mathf.Floor((sz - 5) / 2), C716(col));
            }
            if (_sinGlintT818 > 0 && ST737.sinFrameGlint934)
            {   // one bright glint running once around the frame border (no glow)
                float t = 1f - _sinGlintT818 / Mathf.Max(0.1f, ST737.sinGlintS818), per = 2 * (w + h);
                for (int k = 0; k < 6; k++) { var pt = SinPerim818(x, y, w, h, Mathf.Repeat(t * per - k * 2, per)); Fill716(pt.x - 1, pt.y - 1, 2, 2, new Color(1f, 0.97f, 0.8f, 1f - k / 6f)); }
            }
            GUI.color = Color.white;
        }
        Texture2D _tSinIcons902; bool _sinIconsTried902;
        /// <summary>t902u: column of the sin in ui_v52_sin_icons_20, or -1 (text label).</summary>
        int SinIconCol902(string name)
        {
            var T = HellordTuning.I; if (!T.sinIconsOn902 || T.sinIconNames902 == null || string.IsNullOrEmpty(name)) return -1;
            if (!_sinIconsTried902) { _sinIconsTried902 = true; _tSinIcons902 = UiTex("v53_sin_icons_20"); if (_tSinIcons902 == null) _tSinIcons902 = UiTex("v52_sin_icons_20"); }
            if (_tSinIcons902 == null) return -1; int n = Mathf.FloorToInt(_tSinIcons902.width / Mathf.Max(1, T.sinIconCell902));
            for (int i = 0; i < T.sinIconNames902.Length && i < n; i++) if (T.sinIconNames902[i] == name) return i; return -1;
        }
        static Vector2 SinPerim818(float x, float y, float w, float h, float d)
        {
            if (d < w) return new Vector2(x + d, y); d -= w; if (d < h) return new Vector2(x + w - 1, y + d); d -= h;
            if (d < w) return new Vector2(x + w - 1 - d, y + h - 1); d -= w; return new Vector2(x, y + h - 1 - d);
        }
        static string SinAbbr818(string name)
        {
            if (string.IsNullOrEmpty(name)) return "?"; var p = name.Split(' '); string a = ""; foreach (var w in p) if (w.Length > 0 && char.IsUpper(w[0]) && w != "Of" && a.Length < 2) a += w[0];
            return a.Length > 0 ? a : name.Substring(0, 1).ToUpperInvariant();
        }

        // ================= logic self-check (test menu / dev) =================
        /// <summary>Pure-logic sanity checks for t818u (no scene needed). Returns "" when all pass, else the failures.</summary>
        public static string SinSelfTest818()
        {
            var f = new System.Text.StringBuilder(); var T = HellordTuning.I;
            if (T.sinDraw818 == null || T.sinDraw818.Length != 45) f.Append("sinDraw818 must have 45 entries; ");
            if (T.sinDemBonusStat818 == null || T.sinDemBonusStat818.Length != 34) f.Append("sinDemBonusStat818 must have 34 entries; ");
            if (HSins.All.Count < 70) f.Append("HSins.All < 70; ");
            if (T.sinIconNames902 != null) { var nm = new System.Collections.Generic.HashSet<string>(); foreach (var d in HSins.All) nm.Add(d.name); foreach (var n in T.sinIconNames902) if (!nm.Contains(n)) f.Append("sin icon name not a sin: " + n + "; "); foreach (var d in HSins.All) if (System.Array.IndexOf(T.sinIconNames902, d.name) < 0) f.Append("no icon: " + d.name + "; "); }
            for (int n = 1; n <= 45 && n + 24 < HSins.All.Count; n++)
            {
                var d = HSins.All[n + 24]; var want = n <= 34 ? SinTier.Demonic : SinTier.HeavenDefying; if (d.tier != want) f.Append("#" + n + " tier; ");
                foreach (var l in HSins.Lines(d, null, true)) if (l.Contains("NOT YET") || l.Contains("NO EFFECT") || l.Contains("TBD")) f.Append("#" + n + " text '" + l + "'; ");
                if (n <= 34) { bool st = false; foreach (var m in d.mods) if (m.stat != SinStat.None) st = true; if (!st) f.Append("#" + n + " no +1 stat; "); }
            }
            float a = 0; for (int i = 0; i < 16; i++) a += SinSkCd818[i]; if (a <= 0) f.Append("cd table; ");
            if (SinAbbr818("Pearl of the Dragon King") != "PD") f.Append("abbr; ");
            return f.ToString();
        }
    }
}
