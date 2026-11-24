#if HELLORD_AUTOTEST
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PixelFXPack;
using UnityEngine;
namespace Hellord
{
    /// <summary>t1651 support matrix (scenario sup1651). For EVERY pickable skill (the pacts' start basics + the Zhurong kit skills) and EVERY support whose
    /// tags fit, cast the skill once in a fixed dummy field with no support, with the support at Lv1 and at max level, and measure the support's own metric
    /// (SupportDef.metric1651). FAIL when Lv1 does not change the metric or max level does not beat Lv1 (= the tag lies: the support is offered but dead).
    /// WARN when a support the tags do NOT allow would have changed the metric (a missing tag). Static checks: every support has a category and a metric,
    /// card text is generated from tuning. New skills / supports join automatically (pact basics list, HData.Supports). Fixed dt (captureFramerate 30),
    /// fixed RNG seed, no crits, cameras off while measuring. Output: summary lines + sup1651_matrix.txt.</summary>
    public partial class HellordGame
    {
        enum SmFix1651 { Std, Dense, Homing, Exec, Kill, Leech, Dur, SparseLine, SparseSide, StdFar, AreaBlast, AreaChain, AreaReach, AreaLure, AreaFissure, AreaBurst, AreaSweep }
        static readonly SmFix1651[] SmAreaFix1651 = { SmFix1651.Dense, SmFix1651.SparseLine, SmFix1651.SparseSide, SmFix1651.AreaBlast, SmFix1651.AreaChain, SmFix1651.AreaReach, SmFix1651.AreaLure, SmFix1651.AreaFissure, SmFix1651.AreaBurst, SmFix1651.AreaSweep }; /* appended layouts only run when earlier ones fail, so already-passing Area pairs stay on Dense / Sparse */ static readonly SmFix1651[] SmDistFix1651 = { SmFix1651.Std, SmFix1651.StdFar }; /* StdFar: + demons past the end of a long roll / flight (bounce at the end) */
        static bool SmLv1Ok1651(double a, double b) => System.Math.Abs(b - a) > System.Math.Max(System.Math.Abs(a) < 0.05 ? 1e-5 : 1e-3, System.Math.Abs(a) * 0.002); /* small metrics (1 / 180 s cooldown) need a small floor */
        class SmM1651 { public double hits, firstHit, kills, dmg, affected, potency, disp, heal, span, casts, proj, cdInv, kill, probe; }
        readonly List<Enemy> _smL1651 = new List<Enemy>(128); readonly List<float> _smHp0 = new List<float>(128); readonly List<Vector2> _smP0 = new List<Vector2>(128);
        readonly List<float> _smPot = new List<float>(128);
        Vector2 _smH1651;
        public static System.Func<HellordGame, SkillId, IEnumerator> SmKitCast1651; // stage 4 hook: kit skills cast through this
        static SmFix1651 SmFixFor1651(SupMetric1651 m) => m == SupMetric1651.Area ? SmFix1651.Dense : m == SupMetric1651.Homing ? SmFix1651.Homing : m == SupMetric1651.Execute ? SmFix1651.Exec : m == SupMetric1651.KillHook ? SmFix1651.Kill : m == SupMetric1651.Leech ? SmFix1651.Leech : m == SupMetric1651.Duration ? SmFix1651.Dur : SmFix1651.Std;
        static double SmVal1651(SupportDef sd, SmM1651 m)
        {
            switch (sd.metric1651)
            {
                case SupMetric1651.Proj: return m.proj;
                case SupMetric1651.Distinct: return m.hits; // direct hits (pierce / fork / bounce / chain jumps each add one)
                case SupMetric1651.Homing: return m.firstHit > 0 ? 1.0 / m.firstHit : 0; // reaches the off-axis pack (sooner = better)
                case SupMetric1651.Area: return m.affected;
                case SupMetric1651.Repeat: return m.dmg; // + the cast count must rise (checked in the verdict)
                case SupMetric1651.Status: return m.potency;
                case SupMetric1651.Damage: return m.dmg;
                case SupMetric1651.Execute: return m.hits > 0 ? m.dmg / m.hits : m.dmg; /* per direct hit: Executioner is a per-hit mul. Total damage over the window is noisy when a skill's cadence uses Random (Hydra heads), because Hit / status rolls consume the seed differently at each mul and the bolt count in 2.5 s drifts; Lv3 total already matched none x 1.75 */
                case SupMetric1651.Cooldown: return m.cdInv;
                case SupMetric1651.Leech: return m.heal;
                case SupMetric1651.Knock: return m.disp;
                case SupMetric1651.KillHook: return m.kills > 0 ? m.kill / m.kills : 0; // support output per kill by the skill
                case SupMetric1651.Duration: return m.probe > 0 ? m.probe : m.span;
            }
            return 0;
        }
        static float SmPotency1651(Enemy e, Status st)
        {
            switch (st)
            {
                case Status.Burn: return e.burnT > 0 ? e.burnDps * e.burnT : 0f;
                case Status.Bleed: return e.bleedT > 0 ? e.bleedDps * e.bleedT : 0f;
                case Status.Chill: return Mathf.Max(0f, e.chillT) + (e.freezeT > 0 ? 5f : 0f);
                case Status.Shock: return e.shockT > 0 ? e.shockAmp * e.shockT * 10f : 0f;
            }
            return 0f;
        }
        List<SkillId> SmSkills1651()
        {
            var l = new List<SkillId>(); for (int p = 0; p < NP97; p++) foreach (var id in DfBasics1555(p, false)) if (!l.Contains(id)) l.Add(id);
            return l;
        }
        bool SmIsKit1651(SkillId id) => SmKitIds1651 != null && System.Array.IndexOf(SmKitIds1651, id) >= 0;
        public static SkillId[] SmKitIds1651; // stage 4: the 12 Zhurong kit SkillIds
        void SmClear1651()
        {
            foreach (var e in _en) { if (e.boss || !e.alive && !e.dying) continue; StopStatuses(e); DetachFx(e); e.alive = false; e.dying = false; e.sr.gameObject.SetActive(false); }
            _smL1651.Clear(); foreach (var p in _pr) KillProj(p); _blasts.Clear(); _sched.Clear(); ClearV44(); ZhUltCd = ZhMoveCd = ZhSpecCd = 0f; ZhSunMeter1618 = 0f; ZhNearMiss1618 = 0;
            foreach (var p in _pk) { var j = p.job; p.job = null; p.magnet = false; if (j != null) HellFXRunner.I.Kill(j); p.alive = false; p.sr.gameObject.SetActive(false); }
            ClearLz1666(); LzRunReset1666(); /* t1666 */ ClearGg1682(); GgRunReset1682(); /* t1682 */ _leechHealed = 0f; foreach (var b in _fbBalls) HellordAudio.LoopStop(b.loop, 0.05f); _fbBalls.Clear(); _fbPend.Clear(); /* no boulder from the last run rolls into this one */
        }
        void SmField1651(SmFix1651 f)
        {
            Vector2 H = _smH1651; _smHp0.Clear(); _smP0.Clear(); _smPot.Clear();
            void Mk(Vector2 at, float hpMax, float hpNow) { var e = ZMk1618(_smL1651, EType.Imp, H + at, 0f, hpMax); if (e == null) return; e.hp = hpNow; }
            switch (f)
            {
                case SmFix1651.Dense: for (int i = 0; i < 12; i++) for (int j = 0; j < 9; j++) Mk(new Vector2(0.8f + i * 0.5f, -2f + j * 0.5f), 1e6f, 1e6f); break;
                case SmFix1651.SparseLine: Mk(new Vector2(1.5f, 0f), 1e6f, 1e6f); Mk(new Vector2(4.95f, 0f), 1e6f, 1e6f); Mk(new Vector2(9.0f, 0f), 1e6f, 1e6f); break; /* growing gaps 3.45 / 4.05 along the aim: a jump reach of 3.2 (chain Lv1 skill) stops at 1, x1.15 reaches 2, x1.4 reaches 3 */
                case SmFix1651.SparseSide: Mk(new Vector2(1.5f, 2.6f), 1e6f, 1e6f); Mk(new Vector2(4.0f, 3.1f), 1e6f, 1e6f); Mk(new Vector2(6.5f, 3.6f), 1e6f, 1e6f); break; /* off the aim line, each a bit further out: a zap / pulse radius of ~2.75 reaches 1, x1.15 2, x1.4 3 */
                case SmFix1651.AreaBlast: Mk(new Vector2(12.0f, 0f), 1e6f, 1e6f); Mk(new Vector2(12.0f, -2.20f), 1e6f, 1e6f); Mk(new Vector2(13.2f, -2.05f), 1e6f, 1e6f); break; /* Infernal Boulder end blast at 12 u. The end patch (ellipse 2.5 x 0.875 * RA) already covers the aim line past the blast, so the Lv steps sit off that axis: on the blast; (12,-2.20) feet 2.20 (none 2.125 miss, Lv1 2.280 hit, outside the ball 1.31 and the patch height); (13.2,-2.05) feet 2.375 (Lv1 miss, Lv3 2.423 hit). The two step imps are >1 u apart so they do not separate into each other */
                case SmFix1651.AreaChain: Mk(new Vector2(2.0f, 0f), 1e6f, 1e6f); Mk(new Vector2(2.0f, 6.7f), 1e6f, 1e6f); Mk(new Vector2(10.0f, 2.7f), 1e6f, 1e6f); break; /* Thunder Javelin built-in chain (4 jumps, s.R(6.5)=6.5 / 7.8 / 9.1). A on the aim line; B 6.7 from A (between 6.5 and Lv1 7.8); C 8.94 from B and 8.44 from A (only Lv3 reaches C from B; the spear itself does not) */
                case SmFix1651.AreaReach: Mk(new Vector2(4.2f, 0f), 1e6f, 1e6f); Mk(new Vector2(4.2f, 2.67f), 1e6f, 1e6f); Mk(new Vector2(4.2f, -3.00f), 1e6f, 1e6f); break; /* Phantom Axes: 2 ghosts at cursor ±1.8 u, chop toward the cursor, reach RA(2.6)+0.5 = 3.10 / 3.35 / 3.58. On the cursor (always); (4.2, 2.67) dist 3.220 miss none hit Lv1; (4.2, -3.00) on the other side, dist 3.498 miss Lv1 hit Lv3. Opposite sides so the step imps (radius 0.5) do not overlap and get separated */
                case SmFix1651.AreaLure: Mk(new Vector2(3.0f, 0f), 1e6f, 1e6f); Mk(new Vector2(6.5f, 0f), 1e6f, 1e6f); Mk(new Vector2(7.6f, 0f), 1e6f, 1e6f); break; /* Ember Effigy lure is s.R(6) from the decoy at the dash start (the hero): 6 / 7.2 / 8.4. Inside always / between none and Lv1 / between Lv1 and Lv3 */
                case SmFix1651.AreaFissure: Mk(new Vector2(3.0f, 0f), 1e6f, 1e6f); Mk(new Vector2(4.5f, 0.89f), 1e6f, 1e6f); Mk(new Vector2(4.5f, -0.95f), 1e6f, 1e6f); break; /* Earth-Splitting Slam v2 width W = 28 px * kW / 16 * RA. SmAim is (6, 0.8) so chest-aim is +x, row 0, kW=0.7 (squash 0.7), fissure on y=0 (the default 4.2,0 cursor would aim slightly down from the chest). hw+0.25 = 0.863 / 0.921 / 0.975. On the line (always); (4.5, 0.89) miss none hit Lv1; (4.5, -0.95) miss Lv1 hit Lv3. Opposite sides, >1 u apart. Length does not scale; the impact circle at P (0.56, 0) does not reach x=4.5 */
                case SmFix1651.AreaBurst: Mk(new Vector2(3.0f, 0f), 1e6f, 1e6f); Mk(new Vector2(15.89f, -1.57f), 1e6f, 1e6f); Mk(new Vector2(15.40f, -2.60f), 1e6f, 1e6f); break; /* Frostbolt burst at expiry (the lanes room is wide enough that the orb does not hit a wall). Tail tip, re-aimed at the cursor, 72 or 73 fixed 30 fps frames (the spawn frame may also tick) -> end about (14.19, -0.99) or (14.37, -1.01). Burst reach s.R(1.1)+0.5 = 1.60 / 1.82 / 2.04 (feet, full radius). (3,0) is pierced by the orb (always). A first try sat under the end and the 2.31-tall Imp body boxed the orb (3/3/3); a second sat past it but was tuned only for 72 frames (2/3/3). Steps are now past the later end (left edge > end.x + orb r) and valid for both ends: (15.89, -1.57) feet 1.80/1.62 miss none hit Lv1; (15.40, -2.60) feet 2.02/1.90 miss Lv1 hit Lv3. >1 u apart */
                case SmFix1651.AreaSweep: Mk(new Vector2(7.0f, 0f), 1e6f, 1e6f); Mk(new Vector2(4.0f, 1.08f), 1e6f, 1e6f); Mk(new Vector2(4.0f, -1.22f), 1e6f, 1e6f); break; /* Vermilion Bird corridor is s.R(0.75)+0.25 = 1.00 / 1.15 / 1.30 (feet), from the hero along +x (MouseWorld - feet). Length 14 and the fire strips (0.9 x 0.55, unscaled, height reach 0.80) do not grow, so y=1.08 stays outside the strips. (7,0) always; (4, 1.08) miss none hit Lv1; (4, -1.22) miss Lv1 hit Lv3. Opposite sides, >1 u apart so they do not separate */
                case SmFix1651.Homing: for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) Mk(new Vector2(3.8f + i * 0.7f, 2.4f + j * 0.7f), 1e6f, 1e6f); break;
                case SmFix1651.Kill: for (int i = 0; i < 8; i++) for (int j = 0; j < 5; j++) Mk(new Vector2(1.2f + i * 0.9f, -1.8f + j * 0.9f), 1f, 1f); break;
                case SmFix1651.StdFar: for (int i = 0; i < 8; i++) for (int j = 0; j < 5; j++) Mk(new Vector2(1.2f + i * 0.9f, -1.8f + j * 0.9f), 1e6f, 1e6f); Mk(new Vector2(12.5f, 2f), 1e6f, 1e6f); Mk(new Vector2(12.5f, -2f), 1e6f, 1e6f); Mk(new Vector2(14f, 0f), 1e6f, 1e6f); break;
                default: { float hpNow = f == SmFix1651.Exec ? 8e4f : 1e6f; for (int i = 0; i < 8; i++) for (int j = 0; j < 5; j++) Mk(new Vector2(1.2f + i * 0.9f, -1.8f + j * 0.9f), 1e6f, hpNow); break; }
            }
            foreach (var e in _smL1651) { _smHp0.Add(e.hp); _smP0.Add(e.pos); _smPot.Add(0f); }
        }
        Vector2 SmAim1651(SmFix1651 f) => _smH1651 + (f == SmFix1651.Homing ? new Vector2(5f, -0.6f) : f == SmFix1651.AreaFissure ? new Vector2(6f, 0.8f) : new Vector2(4.2f, 0f)); /* AreaFissure: cursor at chest height so AimDir is +x and the slam row is 0 */
        /// <summary>one measured cast. sup = null: no support. lvl 1..5.</summary>
        IEnumerator SmRun1651(SkillId id, SupportDef sup, int lvl, SmFix1651 f, SmM1651 m)
        {
            SmClear1651(); _heroPos = _smH1651; SnapCamera(); var T = HellordTuning.I;
            var s = new SkillInst { def = HData.Skill(id), level = 1 }; if (sup != null) { s.AddSupport(sup.id); s.supLvl[(int)sup.id] = lvl; }
            bool kit = SmIsKit1651(id);
            if (!kit) { skills.Clear(); skills.Add(s); } else SmKitSet1651?.Invoke(this, s);
            SmField1651(f); Random.InitState(1651); crit = 0f; atMouse986 = SmAim1651(f);
            float hp0 = f == SmFix1651.Leech ? maxHp * 0.6f : maxHp; hp = hp0;
            yield return SmFrames1651(2, f, m, false, hp0);
            float lh0 = AtLeech1651; int c0 = AtCasts1651, p0 = AtProj1651, h0 = AtHits1651, sk0 = AtSkillKills1651; float k0 = AtKill1651; _pbSparkSeed1567 = 1; _fbShot = 0; AtCd1651 = 0f; s.cd = 0f; float t0 = runTime; m.probe = 0;
            if (kit) { if (SmKitCast1651 != null) yield return SmKitCast1651(this, id); }
            else { HInput.AtLmb = true; yield return SmFrames1651(2, f, m, false, hp0); HInput.AtLmb = id == SkillId.LiveWire || id == SkillId.BreathOfTheDeep; /* t1682: Breath of the Deep is held too */ /* t1666: Live Wire is a held beam: hold LMB through the window */ }
            float cdAt = AtCd1651; if (kit) m.probe = SmKitProbe1651 != null ? SmKitProbe1651(this, id) : 0f;
            float win = f == SmFix1651.Dur ? 12f : 2.5f; /* Dur: long enough that a 4 s wall + its burn ends inside the window */ int frames = Mathf.CeilToInt(win * 30f);
            double last = SmDmg1651(); float lastT = 0f, firstT = 0f;
            for (int i = 0; i < frames; i++)
            {
                yield return SmFrames1651(1, f, m, true, hp0); double d = SmDmg1651(); if (d > last + 0.01) { last = d; lastT = runTime - t0; if (firstT <= 0f) firstT = lastT; }
            }
            for (int x = 0; x < 75 && (_fbBalls.Count > 0 || _fbPend.Count > 0); x++) /* a rolling boulder (and its bounces) finishes its roll + end blast inside the window */
            {
                yield return SmFrames1651(1, f, m, true, hp0); double d = SmDmg1651(); if (d > last + 0.01) { last = d; lastT = runTime - t0; if (firstT <= 0f) firstT = lastT; }
            }
            m.dmg = SmDmg1651(); m.casts = AtCasts1651 - c0; m.proj = AtProj1651 - p0; m.kill = AtKill1651 - k0; m.cdInv = cdAt > 0.0001f ? 1.0 / cdAt : 0; m.span = lastT; m.firstHit = firstT; m.hits = AtHits1651 - h0; m.kills = AtSkillKills1651 - sk0;
            m.affected = 0; /* affected = damaged, chilled (chill ground), or marked by the skill */ m.disp = 0; m.potency = 0;
            for (int i = 0; i < _smL1651.Count; i++)
            {
                var e = _smL1651[i]; bool hitE = e.hp < _smHp0[i] - 0.01f || !e.alive; float dp = (e.pos - _smP0[i]).magnitude; if (e.alive) m.disp += dp;
                if (hitE || e.chillT > 0f || e.freezeT > 0f || e.zFlam1618 > runTime - win || e.zCry1625 > 0f || e.lured1651) m.affected++; // damaged or marked by the skill (displacement is its own metric)
                m.potency += _smPot[i];
            }
            if (kit && SmKitProbe1651 != null) m.probe = System.Math.Max(m.probe, SmKitProbe1651(this, id));
            m.heal = AtLeech1651 - lh0; /* leeched life (a rebirth / regen does not count) */ HInput.AtLmb = false; skills.Clear(); if (kit) { ResetKit1651(); ZhRunMove1639 = ZhRunSpec1639 = ZhRunUlt1639 = 0; _pactPath51 = _smPath0; /* t1666 */ }
        }
        double SmDmg1651() { double d = 0; for (int i = 0; i < _smL1651.Count; i++) { var e = _smL1651[i]; d += _smHp0[i] - (e.alive ? Mathf.Max(0f, e.hp) : 0f); } return d; }
        IEnumerator SmFrames1651(int n, SmFix1651 f, SmM1651 m, bool sample, float hp0)
        {
            for (int k = 0; k < n; k++)
            {
                yield return null; if (mode != Mode.Play) mode = Mode.Play; pendingLevels = 0; _heroPos = _smH1651; atMouse986 = SmAim1651(f); AtMove1396 = Vector2.zero;
                if (f != SmFix1651.Leech) hp = maxHp;
                foreach (var e in _en) if (e.alive && !e.boss && !_smL1651.Contains(e)) { StopStatuses(e); DetachFx(e); e.alive = false; e.dying = false; e.sr.gameObject.SetActive(false); } // waves / splits
                foreach (var p in _pk) if (p.alive) p.magnet = false;
                if (sample && _smSt1651 != Status.None) for (int i = 0; i < _smL1651.Count; i++) { var e = _smL1651[i]; if (e.alive) _smPot[i] = Mathf.Max(_smPot[i], SmPotency1651(e, _smSt1651)); }
            }
        }
        Status _smSt1651;
        /// <summary>t1651 stage 6: one DoT applied by a supported skill, no hits after. r = { leech heal, dmg with Executioner (low HP), dmg without, DoT kills, harvest XP, kill blasts }.</summary>
        IEnumerator SmDot1651(Status st, double[] r)
        {
            var def = HData.Skill(SkillId.Fireball);
            for (int pass = 0; pass < 3; pass++)
            {
                SmClear1651(); _heroPos = _smH1651; var s = new SkillInst { def = def, level = 1 };
                if (pass == 0) { s.AddSupport(SupportId.LifeLeech); s.supLvl[(int)SupportId.LifeLeech] = SkillInst.MaxSupLevel; }
                if (pass == 1) { s.AddSupport(SupportId.Executioner); s.supLvl[(int)SupportId.Executioner] = SkillInst.MaxSupLevel; }
                if (pass == 2) { s.AddSupport(SupportId.SoulHarvest); s.AddSupport(SupportId.ExplodeOnKill); }
                _smHp0.Clear(); _smP0.Clear(); _smPot.Clear(); var H = _smH1651;
                for (int i = 0; i < 4; i++) { var e = ZMk1618(_smL1651, EType.Imp, H + new Vector2(2f + i * 1.5f, 0f), 0f, 1000f); if (e == null) continue; e.hp = pass == 1 ? 150f : pass == 2 ? 6f : 1000f; _smHp0.Add(e.hp); _smP0.Add(e.pos); _smPot.Add(0f); }
                float hp0 = maxHp * 0.5f; hp = hp0; _leechHealed = 0f; int dk0 = AtDotKills1651, ex0 = AtExplodes1651; float lq0 = AtLeech1651; float k0 = AtKill1651; int bl0 = 0;
                foreach (var e in _smL1651) ApplyStatus(e, st, 40f, 2f, s);
                for (int f = 0; f < 90; f++) { yield return null; if (mode != Mode.Play) mode = Mode.Play; pendingLevels = 0; _heroPos = _smH1651; if (pass == 2) bl0 = Mathf.Max(bl0, _blasts.Count); }
                double dmg = SmDmg1651();
                if (pass == 0) r[0] = AtLeech1651 - lq0;
                if (pass == 1)
                {   // same DoT without Executioner on the same low-HP demons
                    r[1] = dmg; SmClear1651(); _smHp0.Clear(); var s2 = new SkillInst { def = def, level = 1 };
                    for (int i = 0; i < 4; i++) { var e = ZMk1618(_smL1651, EType.Imp, H + new Vector2(2f + i * 1.5f, 0f), 0f, 1000f); if (e == null) continue; e.hp = 150f; _smHp0.Add(e.hp); }
                    foreach (var e in _smL1651) ApplyStatus(e, st, 40f, 2f, s2);
                    for (int f = 0; f < 90; f++) { yield return null; if (mode != Mode.Play) mode = Mode.Play; pendingLevels = 0; _heroPos = _smH1651; }
                    r[2] = SmDmg1651();
                }
                if (pass == 2) { r[3] = AtDotKills1651 - dk0; r[4] = AtKill1651 - k0; r[5] = System.Math.Max(bl0, AtExplodes1651 - ex0); }
            }
            SmClear1651(); hp = maxHp;
        }
        // ---- stage 4: kit skills in the matrix (cast directly; conditions pre-met; probes read the state the cast set)
        int _smK1651 = -1; float _smKT0;
        int _smPath0;
        void SmKitSetI1651(SkillInst si)
        {
            int gg = System.Array.IndexOf(GgIds1682, si.def.id); // t1682: Gonggong kit (0-12 = 4 movement, 5 specials, 4 ultimates; a Gonggong run)
            if (gg >= 0) { _smK1651 = GgK0 + gg; ResetKit1651(); _zkI1651[GgK0 + gg] = si; _pactPath51 = 4; ZhRunMove1639 = gg < 4 ? gg + 1 : 0; ZhRunSpec1639 = gg >= 4 && gg < 9 ? gg - 3 : 0; ZhRunUlt1639 = gg >= 9 ? gg - 8 : 0; return; }
            int lz = System.Array.IndexOf(LzIds1666, si.def.id); // t1666: Leizu kit (index 0-11 = 4 movement, 4 specials, 4 ultimates; a Leizu run)
            if (lz >= 0) { _smK1651 = LzK0 + lz; ResetKit1651(); _zkI1651[LzK0 + lz] = si; _pactPath51 = 2; ZhRunMove1639 = lz < 4 ? lz + 1 : 0; ZhRunSpec1639 = lz >= 4 && lz < 8 ? lz - 3 : 0; ZhRunUlt1639 = lz >= 8 ? lz - 7 : 0; return; }
            int k = System.Array.IndexOf(ZkIds1651, si.def.id); _smK1651 = k; ResetKit1651(); _zkI1651[k] = si; _pactPath51 = 0;
            ZhRunMove1639 = k < 3 ? k + 1 : 0; ZhRunSpec1639 = k >= 3 && k < 8 ? k - 2 : 0; ZhRunUlt1639 = k >= 8 ? k - 7 : 0;
        }
        IEnumerator SmKitCastI1651(SkillId id)
        {
            int k = _smK1651; _smKT0 = runTime;
            if (k >= GgK0) { yield return SmGgCast1682(k - GgK0); yield break; } // t1682
            if (k >= LzK0) { yield return SmLzCast1666(k - LzK0); yield break; } // t1666
            if (k < 3) { ZhDashStart1618(_heroPos); AtCd1651 = ZhMoveCd; }
            else if (k < 8) { ZhSpecial1618(); AtCd1651 = ZhSpecCd; if (k == 5) { yield return null; SmCryKill1651(); } }
            else if (k == 9) { hp = maxHp; ZhRebirth1618(); AtCd1651 = ZhUltCd; }
            else
            {
                if (k == 8) ZhSunMeter1618 = TZ.swMeter1618; if (k == 11) ZhNearMiss1618 = Mathf.Max(1, TZ.vbNeed1618);
                if (k == 10) { int n = 0; foreach (var e in _smL1651) if (e.alive && n++ < TZ.mhNeed1618 + 1) { e.burnT = 30f; e.burnDps = 0f; } yield return null; yield return null; }
                ZhUltimate1618(); AtCd1651 = ZhUltCd;
            }
            yield return null;
        }
        /// <summary>War Cry's damage is the death blast of roared demons: kill the roared demon nearest the hero (a plain kill, not a skill hit).</summary>
        void SmCryKill1651()
        {
            Enemy best = null; float bd = 1e9f; foreach (var e in _smL1651) { if (!e.alive || e.zCry1625 <= runTime) continue; float d = (e.pos - _heroPos).sqrMagnitude; if (d < bd) { bd = d; best = e; } }
            if (best != null) { int ix = _smL1651.IndexOf(best); Damage(best, best.hp + 10f, false, true); if (ix >= 0) _smHp0[ix] = 0f; } // the killed one is not the skill's damage
        }
        float SmKitProbeI1651(SkillId id)
        {
            if (_smK1651 >= GgK0) return SmGgProbe1682(_smK1651 - GgK0); // t1682
            if (_smK1651 >= LzK0) return SmLzProbe1666(_smK1651 - LzK0); // t1666
            switch (_smK1651)
            {
                case 0: return ZhEffT1618; case 1: return ZhAncT1618; case 3: return ZhMarkT1618; case 7: return ZhHearthT1618; case 10: return ZhHeartT1618;
                case 4: { float mx = 0f; foreach (var e in _smL1651) mx = Mathf.Max(mx, e.zFlam1618 - _smKT0); return mx; } // curse end, from the cast
            }
            return 0f;
        }
        // ---- t1666: Leizu kit casts with their conditions pre-met (mark charged, drum struck, meters full, a cull-ready elite)
        float _smLzP1666;
        IEnumerator SmLzCast1666(int i)
        {
            _smLzP1666 = 0f; var T = TZ;
            if (i < 4)
            {
                _dashDir49 = Vector2.right; LzDashStart1666(_heroPos); AtCd1651 = ZhMoveCd; LzDashAfter1666(); _dashT49 = 0f;
                if (i == 1) { _smLzP1666 = LzWardT1666; Enemy a = null; foreach (var e in _smL1651) if (e.alive) { a = e; break; } _hurtBy53 = a; LzWardAbsorb1666(a != null); } // one absorbed contact hit shocks the attacker
                if (i == 2) _smLzP1666 = LzStrideT1666;
                if (i == 3) foreach (var e in _smL1651) _smLzP1666 = Mathf.Max(_smLzP1666, e.stun53);
            }
            else if (i < 8)
            {
                LzSpecial1666(); AtCd1651 = ZhSpecCd;
                if (i == 4) { _smLzP1666 = LzMarkT1666; LzMarkPay1666(); }
                if (i == 6 && _lzDrum1666 != null) { _smLzP1666 = _lzDrum1666.life; LzDrumBoom1666(); }
            }
            else
            {
                if (i == 8) LzCombos1666 = T.dcNeed1666; if (i == 9) LzInfusions1666 = T.haNeed1666; if (i == 10) LzEnergy1666 = T.trNeed1666;
                if (i == 11) { Enemy best = null; float bd = 1e9f; foreach (var e in _smL1651) { if (!e.alive) continue; float d = (e.pos - _heroPos).sqrMagnitude; if (d < bd) { bd = d; best = e; } } if (best != null) { best.elite = true; best.hp = best.maxHp * T.jwCull1666 * 0.5f; int ix = _smL1651.IndexOf(best); if (ix >= 0) _smHp0[ix] = 0f; /* its cull is not the measured damage */ } }
                LzUltimate1666(); AtCd1651 = ZhUltCd; if (i == 10) _smLzP1666 = LzTribT1666;
                if (i == 11) foreach (var g in _lzGround1666) _smLzP1666 = Mathf.Max(_smLzP1666, g.life);
                if (i == 9) foreach (var g in _lzGround1666) _smLzP1666 = Mathf.Max(_smLzP1666, g.life);
            }
            yield return null;
        }
        float SmLzProbe1666(int i)
        {
            if (i == 5) { float mx = 0f; foreach (var e in _smL1651) mx = Mathf.Max(mx, e.lzCond1666 - _smKT0); return mx; } // curse end, from the cast
            return _smLzP1666;
        }
        public static System.Action<HellordGame, SkillInst> SmKitSet1651; public static System.Func<HellordGame, SkillId, float> SmKitProbe1651;

        IEnumerator AtSup1651()
        {
            string scen = "sup1651"; var T = HellordTuning.I;
            { var ki = new List<SkillId>(ZkIds1651); ki.AddRange(LzIds1666); for (int gi = 0; gi < GgIds1682.Length; gi++) if (GgIdOn1682(gi)) ki.Add(GgIds1682[gi]); /* t1682; t1687: only the skills on the kit */ SmKitIds1651 = ki.ToArray(); } /* t1666: + the 12 Leizu kit skills */ _smPath0 = 0; SmKitSet1651 = (g, si) => g.SmKitSetI1651(si); SmKitCast1651 = (g, id) => g.SmKitCastI1651(id); SmKitProbe1651 = (g, id) => g.SmKitProbeI1651(id); // t1651 stage 4: kit skills join the matrix
            yield return ZhStart1618(scen, null, SkillId.Fireball); _zKeepHp = false;
            float crit0 = crit; int cf0 = Time.captureFramerate; Time.captureFramerate = 30; bool pa0 = _pactActive51; _pactActive51 = false; bool ko0 = TZ.kitSlotOverride1639; TZ.kitSlotOverride1639 = false; /* kit slots come from the run picks the test sets, not the testing override */ // neutral: no pact node effects (Pyromancer turns statuses into burn, etc.)
            var cams = new List<Camera>(); foreach (var c in Camera.allCameras) if (c.enabled && System.Environment.GetCommandLineArgs().Length > 0 && !System.Array.Exists(System.Environment.GetCommandLineArgs(), a => a == "-smRender")) { c.enabled = false; cams.Add(c); }
            if (_room != null) { _room.Walkable(6, out var lo, out var hi); _smH1651 = new Vector2(Mathf.Lerp(lo.x, hi.x, 0.5f) - 3.8f, Mathf.Lerp(lo.y, hi.y, 0.5f)); } else _smH1651 = _heroPos;
            // ---- static checks
            var stat = new StringBuilder(); bool statOk = true;
            int offeredN = 0;
            foreach (var sd in HData.Supports)
            {
                if (!HData.Offered1654(sd)) continue; offeredN++;
                if (string.IsNullOrEmpty(sd.category)) { statOk = false; stat.Append(sd.name).Append(": no category; "); }
                if (sd.metric1651 == SupMetric1651.None) { statOk = false; stat.Append(sd.name).Append(": no test metric; "); }
                string bad = (SmCardCheck1651 ?? SmCard1651)(sd); if (bad != null) { statOk = false; stat.Append(sd.name).Append(": ").Append(bad).Append("; "); }
            }
            ARes(scen + ":static", statOk, stat.Length == 0 ? $"{offeredN} supports: every one has a category and a test metric; card text checks ok" : stat.ToString());
            if (System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-smStatic") >= 0) { /* t1661: static / card-text only, skip the DoT probe and the matrix */ SmClear1651(); foreach (var c in cams) if (c != null) c.enabled = true; Time.captureFramerate = cf0; crit = crit0; _zKeepHp = true; _pactActive51 = pa0; TZ.kitSlotOverride1639 = ko0; ZhEnd1618(scen, null); yield break; }
            // ---- stage 6: burn / bleed carry their skill (leech, executioner, kill hooks on DoT)
            { var dl = new StringBuilder(); bool dok = true; foreach (var st in new[] { Status.Burn, Status.Bleed }) { var r = new double[6]; yield return SmDot1651(st, r); bool ok = r[0] <= 0.01 && r[1] > r[2] * 1.05 && r[3] > 0 && r[4] > 0 && r[5] > 0; dok &= ok; dl.Append($"{st}: leech heal {r[0]:0.##} (DoT must not leech), exec dmg {r[1]:0.#} vs {r[2]:0.#} without, DoT kills {r[3]:0}, harvest XP {r[4]:0}, blasts {r[5]:0} {(ok ? "ok" : "FAIL")} | "); }
              ARes(scen + ":dot", dok, dl.ToString()); }
            // ---- matrix
            var ids = SmSkills1651(); if (SmKitIds1651 != null) ids.AddRange(SmKitIds1651);
            { var av = System.Environment.GetCommandLineArgs(); int ai = System.Array.IndexOf(av, "-smSkills"); if (ai >= 0 && ai + 1 < av.Length) { var keep = new List<string>(av[ai + 1].Split(',')); ids.RemoveAll(x => !keep.Contains(x.ToString())); } } // quick subset runs
            var mx = new StringBuilder(); mx.AppendLine("skill\tsupport\tfit\tmetric\tnone\tlv1\tlvMax\tverdict");
            int pass = 0, fail = 0, warn = 0, pairs = 0, noise = 0; var failL = new List<string>(); var warnL = new List<string>();
            foreach (var id in ids)
            {
                var def = HData.Skill(id); var baseM = new Dictionary<SmFix1651, SmM1651>(); var line = new StringBuilder(); bool skOk = true; bool det = true;
                { _smSt1651 = Status.None; var n1 = new SmM1651(); var n2 = new SmM1651(); yield return SmRun1651(id, null, 1, SmFix1651.Std, n1); yield return SmRun1651(id, null, 1, SmFix1651.Std, n2);
                  det = n1.dmg == n2.dmg && n1.hits == n2.hits && n1.affected == n2.affected; if (!det) { skOk = false; noise++; } line.Append(det ? "deterministic | " : $"NOISE dmg {n1.dmg:0.##}/{n2.dmg:0.##} hits {n1.hits}/{n2.hits} | "); mx.AppendLine($"{def.name}\t(repeat)\t-\tnoise\t{n1.dmg:0.###}\t-\t{n2.dmg:0.###}\t{(det ? "deterministic" : "NOISE")}"); }
                foreach (var sd in HData.Supports)
                {
                    if (!HData.Offered1654(sd) || sd.metric1651 == SupMetric1651.None) continue; bool fit = HData.Fits(sd, def);
                    if (!fit && (sd.needs == STag.Any || sd.needs == STag.Hit || sd.metric1651 == SupMetric1651.Orbitals || sd.metric1651 == SupMetric1651.Repeat)) continue; // Echo needs Area by RULE (not a capability), so no missing-tag probe // missing-tag probe only for the shape tags (Projectile / Area / Duration / Chain)
                    if (!fit && SmKitIds1651 != null && SmIsKit1651(id) && sd.metric1651 != SupMetric1651.Area && sd.metric1651 != SupMetric1651.Duration) continue;
                    var fxs = sd.metric1651 == SupMetric1651.Area ? SmAreaFix1651 : sd.metric1651 == SupMetric1651.Distinct ? SmDistFix1651 : new[] { SmFixFor1651(sd.metric1651) }; /* Area: dense pack first; reach-type areas (chain jump reach, nearest-first zaps) only show on the sparse layouts */
                    double v0 = 0, v1 = 0, v5 = 0; bool repOk = true; SmFix1651 fx = fxs[0]; var tried = new StringBuilder();
                    for (int fi = 0; fi < fxs.Length; fi++)
                    {
                        fx = fxs[fi]; _smSt1651 = sd.metric1651 == SupMetric1651.Status ? sd.status1651 : Status.None; repOk = true; v1 = 0;
                        if (!baseM.TryGetValue(fx, out var b0) || _smSt1651 != Status.None) { b0 = new SmM1651(); yield return SmRun1651(id, null, 1, fx, b0); baseM[fx] = b0; }
                        v0 = SmVal1651(sd, b0);
                        if (fit) { var m1 = new SmM1651(); yield return SmRun1651(id, sd, 1, fx, m1); v1 = SmVal1651(sd, m1); if (sd.metric1651 == SupMetric1651.Repeat) repOk = m1.casts > b0.casts; }
                        var m5 = new SmM1651(); yield return SmRun1651(id, sd, SkillInst.MaxSupLevel, fx, m5); v5 = SmVal1651(sd, m5); tried.Append($"{fx}:{v0:0.##}/{v1:0.##}/{v5:0.##} ");
                        bool okF = fit ? SmLv1Ok1651(v0, v1) && repOk && v5 > v1 + System.Math.Max(System.Math.Abs(v1) < 0.05 ? 1e-5 : 1e-3, System.Math.Abs(v1) * 0.002) : !(v5 > v0 + System.Math.Max(1e-3, System.Math.Abs(v0) * (det ? 0.002 : 0.25)) + (det ? 0 : 1)); if (okF) break; /* stop at the first layout that shows it (fit) / keep probing while no change (missing tag) */
                    }
                    string verdict;
                    if (fit)
                    {
                        pairs++; bool lv1 = SmLv1Ok1651(v0, v1), lv5 = v5 > v1 + System.Math.Max(System.Math.Abs(v1) < 0.05 ? 1e-5 : 1e-3, System.Math.Abs(v1) * 0.002);
                        lv1 &= repOk; verdict = lv1 && lv5 ? "PASS" : !lv1 ? "FAIL lv1=none" : "FAIL lvMax<=lv1"; if (lv1 && lv5) pass++; else { fail++; skOk = false; failL.Add($"{def.name}/{sd.name} ({verdict.Substring(5)}: {v0:0.##}/{v1:0.##}/{v5:0.##})"); }
                    }
                    else
                    {
                        bool chg = v5 > v0 + System.Math.Max(1e-3, System.Math.Abs(v0) * (det ? 0.002 : 0.25)) + (det ? 0 : 1); /* a socketed support only ever raises its metric; on a noisy skill it must beat the noise */ verdict = chg ? "WARN missing tag" : "-";
                        if (chg) { warn++; warnL.Add($"{def.name}/{sd.name} ({v0:0.##}->{v5:0.##})"); }
                    }
                    mx.AppendLine($"{def.name}\t{sd.name}\t{(fit ? "fit" : "no")}\t{sd.metric1651}\t{v0:0.###}\t{(fit ? v1.ToString("0.###") : "-")}\t{v5:0.###}\t{verdict}\t{(fxs.Length > 1 ? tried.ToString().Trim() : fx.ToString())}");
                    if (fit) line.Append(sd.name).Append(verdict == "PASS" ? " ok " : " DEAD ").Append($"({v0:0.#}/{v1:0.#}/{v5:0.#}) | ");
                }
                ARes(scen + ":" + def.name, skOk, line.ToString());
                System.IO.File.WriteAllText(System.IO.Path.Combine(_atOut, "sup1651_matrix.txt"), mx.ToString());
            }
            ARes(scen + ":totals", fail == 0 && noise == 0, $"pairs {pairs}: pass {pass}, fail {fail}; missing-tag warnings {warn}; non-deterministic skills {noise} | FAIL: {string.Join("; ", failL)} | WARN: {string.Join("; ", warnL)}");
            SmClear1651(); foreach (var c in cams) if (c != null) c.enabled = true; Time.captureFramerate = cf0; crit = crit0; _zKeepHp = true; _pactActive51 = pa0; TZ.kitSlotOverride1639 = ko0;
            ZhEnd1618(scen, null);
        }
        public static System.Func<SupportDef, string> SmCardCheck1651; // stage 3: generated card text vs tuning
        /// <summary>t1651 stage 3: card text comes from the tuning. Lv1 and max-level effect text exist and differ (or a mod is flat on purpose); stat supports' desc is the generated one and shows the Lv1 + max values. t1654: max is Lv3.</summary>
        static string SmCard1651(SupportDef sd)
        {
            int mx = SkillInst.MaxSupLevel;
            string e1 = HData.SupportEffect(sd.id, 1), e5 = HData.SupportEffect(sd.id, mx);
            if (string.IsNullOrEmpty(e1) || string.IsNullOrEmpty(e5)) return "no effect text";
            if (e1 == e5) return $"Lv1 and Lv{mx} text are the same ({e1})";
            if (sd.mods != null)
            {
                string gen = HData.SupportDesc1651(sd); if (sd.desc != gen) return "desc is not the generated text";
                string m1 = HData.ModText1651(sd.mods, 1, null), m5 = HData.ModText1651(sd.mods, mx, null);
                if (!sd.desc.Contains(m1) || !sd.desc.Contains(m5)) return $"desc misses the Lv1/Lv{mx} values";
                if (sd.drawbacks != null && !sd.desc.Contains("Drawback")) return "drawback not on the card";
                var T = HellordTuning.I; bool moved = false; foreach (var m in sd.mods) if (Mathf.Abs(m.v(T, 1) - m.v(T, mx)) > 1e-4f) moved = true;
                if (!moved) return $"no mod grows from Lv1 to Lv{mx}";
            }
            return null;
        }

        // ---------------------------------------------------------------- t1651 clip: pairs that were dead before the rebuild, now working
        IEnumerator AtSupClip1651()
        {
            string scen = "supclip1651", clip = scen; var T = HellordTuning.I; _zClip = clip;
            yield return ZhStart1618(scen, clip, SkillId.Fireball);
            bool pa0 = _pactActive51; _pactActive51 = false; float crit0 = crit; crit = 0f;
            Vector2 H0 = _heroPos; var L = new List<Enemy>(); var log = new StringBuilder();
            void Pack() { ZKill1618(L); for (int i = 0; i < 5; i++) for (int j = 0; j < 4; j++) ZMk1618(L, EType.Imp, H0 + new Vector2(2.2f + i * 1.1f, -1.9f + j * 1.25f), 0f, 1e6f); }
            IEnumerator Shot(SkillId id, string label, float sec, params (SupportId, int)[] sups)
            {
                Pack(); var s = new SkillInst { def = HData.Skill(id), level = 3 }; foreach (var (sid, lv) in sups) { s.AddSupport(sid); s.supLvl[(int)sid] = lv; }
                skills.Clear(); skills.Add(s); s.cd = 0f; Toast(label); int p0 = AtProj1651, h0 = AtHits1651;
                yield return ZF1618(12); _heroPos = H0; atMouse986 = H0 + new Vector2(5f, 0f); HInput.AtLmb = true; yield return ZF1618(3); HInput.AtLmb = false;
                yield return ZT1618(sec); log.Append($" | {label}: proj {AtProj1651 - p0} hits {AtHits1651 - h0}");
                foreach (var b in _fbBalls) HellordAudio.LoopStop(b.loop, 0.05f); _fbBalls.Clear(); _fbPend.Clear(); skills.Clear();
            }
            yield return Shot(SkillId.RollingMagmaRock, "INFERNAL BOULDER - NO SUPPORT", 2.6f);
            yield return Shot(SkillId.RollingMagmaRock, "BOULDER + MULTI LV3 + FORK LV3", 3.0f, (SupportId.MultipleProjectiles, 3), (SupportId.Fork, 3));
            yield return Shot(SkillId.SparkSpray, "SPARK SPRAY - NO SUPPORT", 1.6f);
            yield return Shot(SkillId.SparkSpray, "SPARK SPRAY + MULTI LV3", 1.8f, (SupportId.MultipleProjectiles, 3));
            // kit skill with a support: Backdraft (dash slot 3) pull, without and with Bigger Area at max level
            for (int pass = 0; pass < 2; pass++)
            {
                Pack(); ResetKit1651(); T.kitMove1618 = 3; var ki = ZkI1651(2); if (pass == 1) { ki.AddSupport(SupportId.BiggerArea); ki.supLvl[(int)SupportId.BiggerArea] = SkillInst.MaxSupLevel; }
                Toast(pass == 0 ? "BACKDRAFT (KIT DASH) - NO SUPPORT" : "BACKDRAFT + BIGGER AREA LV5"); yield return ZF1618(12);
                _heroPos = H0 + new Vector2(4.4f, 0f); ZhMoveCd = 0f; int pl0 = ZhPulled1618; ZhDashStart1618(_heroPos); yield return ZT1618(1.6f);
                log.Append($" | backdraft{(pass == 1 ? "+area5" : "")}: pulled {ZhPulled1618 - pl0}");
            }
            ResetKit1651(); ZKill1618(L); _pactActive51 = pa0; crit = crit0; _heroPos = H0;
            ARes(scen + ":clip", _r1561 > 0 || _atRecFps <= 0, $"frames {_r1561}{log}");
            _zClip = null; ZhEnd1618(scen, clip);
        }
    }
}
#endif
