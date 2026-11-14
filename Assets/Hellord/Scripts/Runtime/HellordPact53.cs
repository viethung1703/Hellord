// v0.0.53: Ascendancy Pact node mechanics (Docs/Hellord_ascendancy_nodes_full_v0.6.md). One Path per Pact; every Notable/Keystone + Crest.
// All hook sites call into here. Hits/kills/status only QUEUE follow-ups (novas, triggers); PactTick53 runs them, so no shared query list is mutated mid-loop.
using System.Collections.Generic;
using UnityEngine;
using PixelFXPack;

namespace Hellord
{
    public partial class HellordGame
    {
        const PactPath BoneWardenGone539 = (PactPath)(-1); // t539u: Bone Warden Path removed. Its old node code below is DORMANT (never active); kept only as reference for Necromancer.
        public static float SlothCd53 = 1f, SlothArea53 = 1f, PactCdS53 = 1f; // read by SkillInst.CdMul/AreaMul
        static HellordTuning PT53 => HellordTuning.I;
        readonly List<int> _pq53 = new List<int>(64); Proj _lastProj53; Enemy _hurtBy53; float _launchFly53; readonly List<Vector2> _awayPts53 = new List<Vector2>();

        // ---- shared state
        float _empT53; bool _trigEmp53, _inTrig53, _inExtra53; float _healMul53 = 1f;
        struct Nova53 { public Vector2 c; public float r, dmg, t; public SkillInst s; public Status st; public int kind; }
        readonly List<Nova53> _nova53 = new List<Nova53>();
        readonly List<int> _trigQ53 = new List<int>();
        // pyro
        float _heat53, _heatLock53, _trailBurnT53, _emberCd53, _infAuraT53, _infBackCd53; int _fbN53;
        struct Seg53 { public Vector2 a, b; public float t; }
        readonly List<Seg53> _trail53 = new List<Seg53>();
        struct Zone53 { public Vector2 c; public float r, t, dmg, tick; public SkillInst s; }
        readonly List<Zone53> _patch53 = new List<Zone53>(), _steam53 = new List<Zone53>(), _rune53 = new List<Zone53>();
        readonly List<float> _ember53 = new List<float>();
        // bone
        float _launchT53, _stillT53; Vector2 _lastHero53; int _legionK53, _harvestK53, _roomBones53;
        readonly int[] _ossHits53 = new int[32]; readonly float[] _ossDown53 = new float[32];
        class Minion53 { public Vector2 p; public float t, atk; }
        readonly List<Minion53> _legion53 = new List<Minion53>();
        readonly List<float> _crestBone53 = new List<float>();
        struct Launch53 { public Vector2 dir; public float t; public SkillInst s; }
        readonly List<Launch53> _launch53 = new List<Launch53>();
        readonly List<Vector2> _pts53 = new List<Vector2>();
        float _extraAng53; readonly float[] _extraCd53 = new float[600];
        struct Wall53 { public Vector2 a, b; public float t, dmg; }
        readonly List<Wall53> _wall53 = new List<Wall53>();
        SkillInst _legionSk53;
        // storm
        float _energy53 = 100f, _icdShock53, _icdKill53, _cap53, _rodT53; int _trigN53;
        // blood
        float _shield53, _holdDash53, _frLast53, _leechT53; bool _wrath53, _wrathLatch53, _frReady53; int _fr53;
        struct Orb53 { public Vector2 p; public float t; }
        readonly List<Orb53> _orb53 = new List<Orb53>();
        // visuals
        readonly List<SpriteRenderer> _spr53 = new List<SpriteRenderer>(); int _sprN53; Sprite _boneSp53;

        bool Crest53 => _pactActive51 && SelCount51() >= PT.pactCrestAt51;
        bool Path53(PactPath p) => _pactActive51 && _pactPath51 == (int)p;
        SkillInst Own53(SkillId id) { foreach (var s in skills) if (s.def.id == id) return s; return null; }
        bool Melee53(SkillInst s) => s != null && ((s.def.tags & STag.Melee) != 0 || (s.def.id == SkillId.SoulSpears && Has51(PactPath.BloodReaver, 6)));
        static bool Fire53(SkillInst s) => s != null && (s.def.id == SkillId.Fireball || s.def.id == SkillId.HellfirePillar);
        float Inc53 => 1f + incDmg;

        void PactReset53()
        {
            _empT53 = 0; _trigEmp53 = _inTrig53 = _inExtra53 = false; _healMul53 = 1; _nova53.Clear(); _trigQ53.Clear();
            _heat53 = _heatLock53 = _trailBurnT53 = _emberCd53 = _infAuraT53 = _infBackCd53 = 0; _fbN53 = 0; _trail53.Clear(); _patch53.Clear(); _steam53.Clear(); _rune53.Clear(); _ember53.Clear();
            _launchT53 = _stillT53 = 0; _legionK53 = _harvestK53 = _roomBones53 = 0; System.Array.Clear(_ossHits53, 0, 32); System.Array.Clear(_ossDown53, 0, 32);
            _legion53.Clear(); _crestBone53.Clear(); _launch53.Clear(); _wall53.Clear(); _legionSk53 = null;
            _energy53 = 100; _icdShock53 = _icdKill53 = _cap53 = _rodT53 = 0; _trigN53 = 0;
            _shield53 = _holdDash53 = _frLast53 = _leechT53 = 0; _wrath53 = _wrathLatch53 = _frReady53 = false; _fr53 = 0; _orb53.Clear();
            SlothCd53 = SlothArea53 = PactCdS53 = 1f; HideSpr53(0); NecroReset536();
        }

        void PactApply53()
        {   // called from PactApply51 after the per-node stat bonuses
            if (Has51(PactPath.BloodReaver, 8)) maxHp *= PT.martyrMaxHpMul53;
            if (Has51(PactPath.BloodReaver, 7)) maxHp *= PT.lastRitesMaxHpMul53;
            if (Has51(BoneWardenGone539, 8)) speedMul *= PT.slothSpeedMul53;
            if (Has51(PactPath.Pyromancer, 9)) _healMul53 = PT.infernoHealMul53;
            hp = maxHp; _energy53 = 100;
            if (mode == Mode.StartPick) BuildStartCards(); // banished skills (Necro Covenant / Blood Magic) leave the start pool
        }

        public bool PactBanned53(SkillDef d)
        {
            if (d.id == SkillId.FrostBomb && !HellordTuning.I.frostBombEnabled1412) return true; // t1412u: Frost Bomb dropped for now (flag)
            if (Has51(BoneWardenGone539, 9) && (d.tags & STag.Projectile) != 0 && (d.tags & STag.Minion) == 0) return true; // t561u: Skeleton Archers are minions
            if (Has51(PactPath.BloodReaver, 9) && d.id == SkillId.StormRing) return true;
            return false;
        }
        bool PactNoRegen53 => Has51(PactPath.BloodReaver, 4) || Has51(PactPath.Stormcaller, 8);

        // ================= heal / leech / damage mul
        void Heal53(float amt)
        {
            if (amt <= 0) return; amt *= _healMul53 * SinHealMul818(); float room = maxHp - hp; // t818u #34
            if (amt <= room) { hp += amt; return; }
            hp = maxHp; float cap = ShieldCap53(); if (cap > 0) _shield53 = Mathf.Min(cap, _shield53 + amt - room);
        }
        float ShieldCap53() => maxHp * Mathf.Max(SinShieldCap818(), Has51(PactPath.BloodReaver, 0) ? PT.leechShieldCap53 : 0, Path53(PactPath.BloodReaver) && Crest53 ? PT.crestBloodShieldCap53 : 0);
        float PactLeechMul53() => Has51(PactPath.BloodReaver, 9) ? PT.bloodMagicLeechMul53 : 1f;
        float PactDmgMul53(SkillInst s)
        {
            float m = 1f;
            if (_empT53 > 0 || _trigEmp53) m *= PT.empowerMul53;
            m *= NecroDmgMul536(s); // t536u
            if (Has51(PactPath.BloodReaver, 8)) { float f = hp / Mathf.Max(1, maxHp); if (f < PT.martyrLow53) m *= PT.empowerMul53; else if (f > PT.martyrHigh53) m *= PT.martyrHighMul53; }
            return m;
        }

        // ================= casting
        bool PactCanCast53(SkillInst s)
        {
            if (!_pactActive51) return true;
            if (_heatLock53 > 0 && Has51(PactPath.Pyromancer, 3) && Fire53(s)) return false;
            if (Has51(PactPath.BloodReaver, 9) && s.def.id != SkillId.BoneOrbit && hp <= BloodCost53(s) + 1) { s.cd = 0.2f; return false; }
            return true;
        }
        float BloodCost53(SkillInst s) => maxHp * Mathf.Lerp(PT.bloodMagicCostMin53, PT.bloodMagicCostMax53, Mathf.InverseLerp(0.4f, 3f, s.def.cooldown));
        float PactCd53(SkillInst s)
        {
            if (!_pactActive51) return 1f; float m = 1f;
            if (Has51(PactPath.Pyromancer, 3) && Fire53(s)) m *= PT.overheatCdMul53;
            if (Has51(PactPath.Pyromancer, 6) && s.def.id == SkillId.HellfirePillar && !FireWallOn948) m /= PT.lanceRateMul53; // t948u: Fire Wall marches instead
            if (Has51(PactPath.BloodReaver, 2) && s.def.id == SkillId.BloodWhip) m *= PT.whipReachCdMul53;
            if (Has51(PactPath.BloodReaver, 9)) m *= PT.bloodMagicCdMul53;
            m *= NecroCd536(s); // t536u
            return m;
        }
        void PactAfterCast53(SkillInst s)
        {
            if (!_pactActive51) return;
            if (Has51(PactPath.Pyromancer, 3) && Fire53(s))
            {
                _heat53 += s.def.id == SkillId.Fireball ? PT.heatFireball53 : PT.heatPillar53;
                if (_heat53 >= 100) { _heat53 = 0; _heatLock53 = PT.overheatLockS53; _nova53.Add(new Nova53 { c = _heroPos, r = PT.overheatVentR53, dmg = SkillDamage(s) * PT.overheatVentMul53, s = s, st = Status.Burn, kind = 0 }); Toast("OVERHEAT"); }
            }
            if (Path53(BoneWardenGone539) && Crest53 && (s.def.tags & STag.Zone) != 0 && _crestBone53.Count < PT.crestBoneMax53) _crestBone53.Add(PT.crestBoneLife53);
            if ((Has51(PactPath.Pyromancer, 3) && Fire53(s)) || (Has51(PactPath.BloodReaver, 9) && s.def.id != SkillId.BoneOrbit)) s.cd = Mathf.Max(s.cd, PT.pactMinCastGap53); // no cooldown, only a frame-safety cast gap
            if (_wrath53) hp = Mathf.Max(1, hp - maxHp * PT.wrathHpCost53);
            if (Has51(PactPath.BloodReaver, 9) && s.def.id != SkillId.BoneOrbit) hp = Mathf.Max(1, hp - BloodCost53(s));
            if (Path53(PactPath.BloodReaver) && Crest53 && Melee53(s) && !_inExtra53)
            {
                int n = Mathf.FloorToInt((1f - hp / Mathf.Max(1, maxHp)) / 0.33f); n = Mathf.Min(n, 2);
                for (int i = 0; i < n; i++) After(PT.crestBloodRecastGap53 * (i + 1), () => { if (mode != Mode.Play) return; float keep = s.cd; _inExtra53 = true; Cast(s, 1f); _inExtra53 = false; s.cd = keep; });
            }
        }

        // ================= projectiles
        void PactSpawnProj53(Proj p)
        {
            if (!_pactActive51) return;
            if (Has51(PactPath.Stormcaller, 3) && p.lastHit < 0 && (p.fork || p.bounce > 0)) { p.fork = true; p.bounce = Mathf.Max(p.bounce, PT.staticForkBounce53); p.dmg *= PT.staticForkDmgMul53; }
        }
        bool PactProjEnd53(Proj p)
        {
            if (p.ret53) return false; if (!p.bone53 && (!Has51(PactPath.BloodReaver, 6) || p.s == null || p.s.def.id != SkillId.SoulSpears)) return false;
            p.ret53 = true; p.life = 2.5f; p.pierce = 99; p.lastHit = -1; return true;
        }
        void PactProjFly53(Proj p)
        {
            if (p.ret53)
            {
                Vector2 to = HeroCenter - p.pos; float sp = Mathf.Max(8f, p.vel.magnitude);
                if (to.magnitude < 0.6f) p.life = 0; else p.vel = to.normalized * sp;
                return;
            }
            if (Has51(PactPath.Pyromancer, 2) && !p.emp53 && p.s != null && p.s.def.id == SkillId.Fireball)
                foreach (var g in _trail53) if (SegDist53(p.pos, g.a, g.b) < PT.trailWidth53) { p.emp53 = true; p.dmg *= PT.empowerMul53; p.explodeR *= 1.2f; if (p.sr != null) p.sr.color = new Color(1f, 0.75f, 0.4f); break; }
        }
        void PactProjHit53(Proj p, int hit)
        {
            if ((p.ret53 && !p.bone53) || (Has51(PactPath.BloodReaver, 6) && p.s != null && p.s.def.id == SkillId.SoulSpears)) Heal53(PT.spearHealPerPierce53);
            if (Has51(PactPath.Pyromancer, 0) && p.s != null && p.s.def.id == SkillId.Fireball && _patch53.Count < 24)
                _patch53.Add(new Zone53 { c = p.pos, r = PT.kindlingR53 * p.s.AreaMul, t = PT.kindlingLife53, dmg = p.dmg * PT.kindlingDmg53, s = p.s });
        }
        bool PactMeteor53(SkillInst s, float dmg)
        {
            if (!Has51(PactPath.Pyromancer, 4)) return false;
            int t = Nearest(_heroPos, 11f, -1); if (t < 0) return false;
            if (++_fbN53 % PT.meteorEvery53 != 0) return false;
            _nova53.Add(new Nova53 { c = _en[t].pos, r = PT.meteorR53 * s.AreaMul, dmg = dmg * PT.meteorDmgMul53, s = s, st = Status.Burn, t = PT.meteorDelay53, kind = 1 });
            return true;
        }
        bool PactLance53(SkillInst s, float dmg)
        {
            int t = Nearest(_heroPos, 12f, -1); if (t < 0) return false;
            Vector2 a = HeroCenter, dir = (_en[t].pos + Vector2.up * _en[t].hitY - a).normalized, b = a + dir * PT.lanceLen53;
            float mul = PactTrailEmp53(_heroPos);
            _pq53.Clear(); Query((a + b) * 0.5f, PT.lanceLen53 * 0.5f + 1f, _pq53);
            foreach (var i in _pq53) { var e = _en[i]; if (e.alive && !e.dying && SegDist53(e.pos + Vector2.up * e.hitY, a, b) < PT.lanceWidth53 + e.radius) Hit(e, dmg * mul, s, dir * 0.5f); }
            ProjectileFX.Beam(a, dir, PT.lanceLen53 * HArt.PPU, ProjElement.Hellfire, 1, 8, 0.25f);
            return true;
        }
        float PactTrailEmp53(Vector2 at)
        {
            if (!Has51(PactPath.Pyromancer, 2)) return 1f;
            foreach (var g in _trail53) if (SegDist53(at, g.a, g.b) < PT.trailWidth53 + 0.8f) return PT.empowerMul53;
            return 1f;
        }
        void PactRune53(Vector2 at, float r)
        {
            if (!_pactActive51) return;
            if (Has51(BoneWardenGone539, 4)) { _rune53.Clear(); _rune53.Add(new Zone53 { c = at, r = r, t = 999f }); }
            else { if (_rune53.Count > 6) _rune53.RemoveAt(0); _rune53.Add(new Zone53 { c = at, r = r, t = 2.2f }); }
        }
        float PactOrbitR53() => (Has51(BoneWardenGone539, 1) ? PT.ossuaryOrbitMul53 : 1f) * (Has51(BoneWardenGone539, 8) ? SlothArea53 : 1f);

        // ================= pools
        bool PactAnchorPool53(SkillInst s, float dmg, float mul)
        {
            if (!Has51(BoneWardenGone539, 2)) return false;
            float r = HData.At(HellordTuning.I.gravePoolRadiusU, s.level, 1.3f) * s.AreaMul;
            foreach (var q in _pools44) if (q.anchor53) { q.dmg = dmg * mul; q.r = r; q.life = Mathf.Max(q.life, 4f); q.s = s; return true; }
            _pools44.Add(new Pool44 { at = _heroPos, r = r, life = 4f, tick = 0, dmg = dmg * mul, slow = 0.3f, s = s, anchor53 = true });
            return true;
        }
        bool PactPoolMove53(Pool44 q, float dt)
        {
            if (!q.anchor53) return true;
            q.life = Mathf.Max(q.life, 0.5f); // persists while owned; refreshed every cast
            q.at = Vector2.MoveTowards(q.at, _heroPos, baseSpeed * speedMul * PT.anchorSpeed53 * dt);
            bool inside = (_heroPos - q.at).magnitude <= q.r;
            ZDisc(q.at, q.r, new Color(0.45f, 0.2f, 0.6f, inside ? 0.3f : 0.12f), 0.75f); ZRing(q.at, q.r, new Color(0.7f, 0.4f, 0.9f, 0.6f), 0.75f);
            return inside;
        }

        // ================= dash
        float PactDashCdMul53() => Has51(PactPath.Pyromancer, 2) ? PT.trailDashCdMul53 : 1f;
        void PactDash53()
        {
            if (!_pactActive51) return;
            Vector2 a = _heroPos, d = _dashDir49.sqrMagnitude > 0.01f ? _dashDir49.normalized : Vector2.right;
            if (Has51(PactPath.Stormcaller, 7))
            {   // Thunder Step: blink, shocking along the path
                Vector2 b = a + d * PT.thunderStepU53; ClampToRoom(ref b, HeroMarginPx); _heroPos = b; _dashT49 = 0;
                _pq53.Clear(); Query((a + b) * 0.5f, PT.thunderStepU53 * 0.5f + 1f, _pq53);
                foreach (var i in _pq53) { var e = _en[i]; if (e.alive && !e.dying && SegDist53(e.pos, a, b) < 1f) { ApplyStatus(e, Status.Shock, 0, 1f); Damage(e, PT.thunderStepDmg53 * Inc53, true, false); } }
                ChainFX.ChainLightningPoints(a + Vector2.up * 0.8f, b + Vector2.up * 0.8f, default, default, 2, new Color(0.6f, 0.85f, 1f), 0.02f, 0.18f);
            }
            if (Has51(PactPath.Pyromancer, 2)) { Vector2 e2 = Has51(PactPath.Stormcaller, 7) ? _heroPos : a + d * T49.dashDistU49; if (_trail53.Count >= 4) _trail53.RemoveAt(0); _trail53.Add(new Seg53 { a = a, b = e2, t = PT.trailLife53 }); }
            if (Has51(PactPath.Stormcaller, 1) && _cap53 >= 1f) { var ring = Own53(SkillId.StormRing); if (ring != null) { _nova53.Add(new Nova53 { c = _heroPos, r = HData.At(HellordTuning.I.stormRadiusU, ring.level, 2.6f) * ring.AreaMul * 1.5f, dmg = SkillDamage(ring) * _cap53 * PT.capDmgPerCharge53, s = ring, st = Status.Shock, kind = 2 }); } _cap53 = 0; }
            if (Has51(BoneWardenGone539, 7) && (_legion53.Count >= 3 || OrbitCount53() >= 6))
            {
                Vector2 n = new Vector2(-d.y, d.x) * PT.wallLen53 * 0.5f; var o = Own53(SkillId.BoneOrbit);
                _wall53.Add(new Wall53 { a = a - n, b = a + n, t = PT.wallLife53, dmg = (o != null ? SkillDamage(o) : 10f) * PT.wallDmgMul53 });
            }
        }

        // ================= hit / status / kill / hurt
        float PactPreHit53(Enemy e, float dmg, SkillInst s, bool tick)
        {
            if (!_pactActive51) return dmg;
            dmg = NecroPreHit536(e, dmg, s, tick); // t536u
            if (e.calcB53 && !tick) { dmg *= PT.calcifyNextMul53; e.calcB53 = false; }
            if (Has51(PactPath.Stormcaller, 9) && s != null)
            {
                int el = s.def.innate == Status.Burn ? 1 : s.def.innate == Status.Chill ? 2 : s.def.innate == Status.Shock ? 3 : 0;
                if (runTime < e.eqT53) dmg *= el == e.eqEl53 ? PT.eqSameMul53 : PT.eqOtherMul53;
                e.eqEl53 = el; e.eqT53 = runTime + PT.eqWindow53;
            }
            return dmg;
        }
        void PactPostHit53(Enemy e, float dmg, SkillInst s, bool tick)
        {
            if (!_pactActive51 || tick) return;
            NecroPostHit536(e, dmg, s, tick); // t536u
            if (Has51(PactPath.Pyromancer, 5) && _ember53.Count > 0 && _emberCd53 <= 0 && e.alive)
            {
                _ember53.RemoveAt(0); _emberCd53 = 0.12f; ApplyStatus(e, Status.Burn, dmg, 1f); Damage(e, dmg * PT.emberDmgMul53 + 2, true, false);
                ZSeg(HeroCenter, e.pos + Vector2.up * e.hitY, 0.08f, new Color(1f, 0.55f, 0.15f, 0.9f), 2);
            }
            if (Melee53(s))
            {
                if (Has51(PactPath.BloodReaver, 3) && e.bstore53 > 0 && e.alive) Burst53(e);
                if (Has51(PactPath.BloodReaver, 5))
                {
                    if (runTime - _frLast53 > 1.5f) { _fr53 = 0; _frReady53 = false; }
                    _frLast53 = runTime;
                    if (_frReady53) { _frReady53 = false; _fr53 = 0; _nova53.Add(new Nova53 { c = _heroPos, r = PT.frenzyNovaR53, dmg = dmg * PT.frenzyNovaMul53 + 5, s = s, st = Status.Bleed, kind = 3 }); }
                    else if (++_fr53 >= 10) _frReady53 = true;
                }
            }
            if (Has51(PactPath.BloodReaver, 8) && hp < maxHp * PT.martyrLow53 && _leechT53 < PT.martyrLeechCapPerS53) { float h = dmg * PT.martyrLeech53; _leechT53 += h; Heal53(h); }
        }
        void Burst53(Enemy e)
        {
            float d = e.bstore53 * PT.hemorrhageMul53; e.bstore53 = 0; e.bst53 = 0;
            HordeFX.CheapBurst(e.pos + Vector2.up * e.hitY, new Color(0.8f, 0.05f, 0.1f), 8); Damage(e, d, true, false);
        }
        bool PactStatus53(Enemy e, ref Status st, ref float dmg, ref float k, SkillInst src)
        {
            if (!_pactActive51) return false;
            if (Has51(PactPath.Pyromancer, 8))
            {
                if (st == Status.Chill) { e.slowT = Mathf.Max(e.slowT, 2f); e.slowAmt = Mathf.Max(e.slowAmt, PT.smolderSlow53); st = Status.Burn; }
                else if (st == Status.Shock) { st = Status.Burn; if (Random.value < crit) dmg *= 2f; }
                else if (st == Status.Bleed) st = Status.Burn;
            }
            if (st == Status.Chill && e.burnT > 0 && Has51(PactPath.Pyromancer, 7))
            {
                if (_steam53.Count >= 6) _steam53.RemoveAt(0);
                _steam53.Add(new Zone53 { c = e.pos, r = PT.steamR53, t = PT.steamLife53 });
            }
            if (st == Status.Chill && Has51(BoneWardenGone539, 5) && src != null && (src.def.tags & (STag.Orbital | STag.Zone)) != 0)
            {
                if (++e.calc53 >= 4) { e.calc53 = 0; e.stun53 = Mathf.Max(e.stun53, e.boss ? 0 : PT.calcifyStunS53); e.calcB53 = true; HordeFX.CheapBurst(e.pos + Vector2.up * e.hitY, new Color(0.9f, 0.9f, 0.8f), 6); }
                return true;
            }
            if (st == Status.Bleed && Has51(PactPath.BloodReaver, 3))
            {
                e.bstore53 += dmg * 0.25f * k * 3f; e.bleedT = 3; e.bleedDps = 0;
                if (!HFX.Alive(e.bleedH)) e.bleedH = HFX.BleedStatus(e.tr, 3f, e.radius);
                if (++e.bst53 >= 5) Burst53(e);
                return true;
            }
            if (st == Status.Shock && e.shockT <= 0 && Has51(PactPath.Stormcaller, 2) && _trigQ53.Count < 8) _trigQ53.Add(0);
            return false;
        }
        bool RoomClearing53 => floorMode && Cur != null && Cur.cleared && AliveCount53() == 0;
        int AliveCount53() { int n = 0; for (int i = 0; i < _en.Length; i++) if (_en[i] != null && _en[i].alive && !_en[i].dying) n++; return n; }
        void PactKill53(Enemy e)
        {
            NecroKill536(e); // t536u
            bool burning = e.burnT > 0, shocked = e.shockT > 0;
            e.calc53 = 0; e.calcB53 = false; e.eqT53 = 0; e.eqEl53 = -1; e.bstore53 = 0; e.bst53 = 0;
            if (!_pactActive51 || mode != Mode.Play || (floorMode && Cur != null && Cur.cleared)) return;
            if (burning)
            {
                if ((Path53(PactPath.Pyromancer) && Crest53) || Has51(PactPath.Pyromancer, 1)) _nova53.Add(new Nova53 { c = e.pos, r = PT.cinderR53, dmg = e.burnDps, kind = 4, t = 0, s = null, st = Status.Burn, });
                if (Has51(PactPath.Pyromancer, 5) && _ember53.Count < PT.emberMax53) _ember53.Add(PT.emberLife53);
                if (Has51(PactPath.Pyromancer, 7)) Heal53(PT.pyreHeal53);
            }
            if (Has51(BoneWardenGone539, 3) || Has51(BoneWardenGone539, 9))
            {
                int max = Has51(BoneWardenGone539, 9) ? PT.necroLegionMax53 : PT.legionMax53;
                if (++_legionK53 >= PT.legionKills53) { _legionK53 = 0; if (_legion53.Count < max) _legion53.Add(new Minion53 { p = e.pos, t = Has51(BoneWardenGone539, 9) ? 1e9f : PT.legionLife53 }); }
            }
            if (Has51(BoneWardenGone539, 6) && ++_harvestK53 >= 10) { _harvestK53 = 0; _roomBones53 = Mathf.Min(4, _roomBones53 + 1); }
            if (Path53(PactPath.Stormcaller)) _energy53 = Mathf.Min(EnergyMax53, _energy53 + PT.energyPerKill53);
            if (shocked && Has51(PactPath.Stormcaller, 6) && _trigQ53.Count < 8) _trigQ53.Add(1);
        }
        void PactSkillKill53(Enemy e, SkillInst s)
        {
            if (Has51(PactPath.BloodReaver, 4) && Melee53(s) && _orb53.Count < 20) _orb53.Add(new Orb53 { p = e.pos, t = 8f });
        }
        float PactHurt53(float dmg, bool contact)
        {
            if (!_pactActive51) return dmg;
            var by = _hurtBy53; _hurtBy53 = null;
            if (Has51(PactPath.Pyromancer, 7) && _steam53.Count > 0)
            {   // Steam = blind cloud: an enemy standing in it misses 50%. Non-contact hits (hook, shout) are blamed on the nearest enemy.
                if (by == null && !contact) { int t = Nearest(_heroPos, 12f, -1); if (t >= 0) by = _en[t]; }
                if (by != null && InSteam53(by.pos) && Random.value < PT.steamMiss53) { _iframe = Mathf.Max(_iframe, 0.25f); HordeFX.CheapBurst(HeroCenter, new Color(0.85f, 0.88f, 0.92f), 4); return -1; }
            }
            if (Has51(PactPath.BloodReaver, 5)) _fr53 = Mathf.Max(0, _fr53 - 3);
            if (Has51(PactPath.Pyromancer, 9) && _infBackCd53 <= 0)
            {
                _infBackCd53 = 0.3f; int t = Nearest(_heroPos, 10f, -1);
                if (t >= 0) { var fb = Own53(SkillId.Fireball) ?? new SkillInst { def = HData.Skill(SkillId.Fireball) }; Vector2 dir = (_en[t].pos - _heroPos).normalized; SpawnProj(fb, HeroCenter, dir * 9f, SkillDamage(fb), 0, false, 0, 1.2f); }
            }
            if (_shield53 > 0)
            {
                float a = Mathf.Min(_shield53, dmg); _shield53 -= a; dmg -= a;
                if (_shield53 <= 0.01f && Has51(PactPath.BloodReaver, 0)) { _shield53 = 0; _nova53.Add(new Nova53 { c = _heroPos, r = 3f, dmg = ShieldCap53() * PT.shieldBurstMul53, st = Status.Bleed, kind = 3 }); }
                if (dmg <= 0) { _iframe = Mathf.Max(_iframe, HeroIFrame); return -1; }
            }
            return dmg;
        }

        // ================= storm triggers
        bool InSteam53(Vector2 p) { foreach (var z in _steam53) if ((z.c - p).magnitude < z.r) return true; return false; }
        void SteamBullets53()
        {   // bullets fired by enemies inside Steam: 50% are blind misses (removed at birth)
            if (_eb49 != null) foreach (var b in _eb49) { if (b == null || !b.alive || b.vis || b.st53) continue; b.st53 = true; if (InSteam53(b.p) && Random.value < PT.steamMiss53) KillBullet49(b, true); }
            if (_bb != null) foreach (var b in _bb) { if (b == null || !b.alive || b.st53) continue; b.st53 = true; if (InSteam53(b.pos) && Random.value < PT.steamMiss53) KillBB(b, true); }
        }
        float EnergyMax53 => Has51(PactPath.Stormcaller, 4) ? 200f : 100f;
        void RunTrig53(int kind)
        {
            bool unbound = Has51(PactPath.Stormcaller, 8);
            float cost = kind == 0 ? PT.trigShockCost53 : PT.trigKillCost53;
            if (!unbound) { if (kind == 0 ? _icdShock53 > 0 : _icdKill53 > 0) return; if (Has51(PactPath.Stormcaller, 4) && _energy53 < PT.overchargeStop53) return; }
            SkillInst pick = null;
            foreach (var s in skills)
            {
                if (s.def.id == SkillId.BoneOrbit) continue;
                if (kind == 1 && (s.def.innate == Status.Shock)) continue;
                if (pick == null || (kind == 0 ? s.level < pick.level : s.level > pick.level)) pick = s;
            }
            if (pick == null) return;
            bool free = Path53(PactPath.Stormcaller) && Crest53 && (++_trigN53 % 4 == 0); /* pt477u: Storm Crest only */ bool emp = Has51(PactPath.Stormcaller, 4) && _energy53 > 100f;
            if (unbound) { if (hp <= PT.unboundHp53 + 1) return; hp -= PT.unboundHp53; }
            else if (!free) { if (_energy53 < cost) return; _energy53 -= cost; }
            if (!unbound) { if (kind == 0) _icdShock53 = PT.trigShockIcd53; else _icdKill53 = PT.trigKillIcd53; }
            float keep = pick.cd; _trigEmp53 = emp; _inTrig53 = true; Cast(pick, 1f); _inTrig53 = false; _trigEmp53 = false; pick.cd = keep;
        }
        int PactPreferShocked53(int first, float range)
        {
            if (!(Path53(PactPath.Stormcaller) && Crest53) || first < 0) return first;
            int best = -1; float bd = range * range;
            _pq53.Clear(); Query(HeroCenter, range, _pq53);
            foreach (var i in _pq53) { var e = _en[i]; if (!e.alive || e.dying || e.shockT <= 0) continue; float d = (e.pos - _heroPos).sqrMagnitude; if (d < bd) { bd = d; best = i; } }
            return best >= 0 ? best : first;
        }
        void PactConduit53(SkillInst s, float d, float range)
        {
            if (!Has51(PactPath.Stormcaller, 0)) return;
            _pts53.Clear(); OrbitPts53(_pts53);
            foreach (var q in _pools44) _pts53.Add(q.at); foreach (var z in _rune53) _pts53.Add(z.c);
            if (Own53(SkillId.StormRing) != null) _pts53.Add(HeroCenter);
            int n = 0;
            foreach (var p in _pts53)
            {
                if (n >= PT.conduitMax53) break;
                int best = -1; float bd = range * range * 0.5f;
                for (int i = 0; i < _en.Length; i++) { var e = _en[i]; if (e == null || !e.alive || e.dying || _chainHit.Contains(i)) continue; float dd = (e.pos - p).sqrMagnitude; if (dd < bd) { bd = dd; best = i; } }
                if (best < 0) continue; n++; _chainHit.Add(best); var en = _en[best];
                Hit(en, d * PT.conduitDmgMul53, s, Vector2.zero);
                ChainFX.ChainLightningPoints(p, en.pos + Vector2.up * en.hitY, default, default, 2, new Color(0.55f, 0.8f, 1f), 0.02f, 0.14f);
            }
        }

        // ================= bones
        void OrbitPts53(List<Vector2> o)
        {
            if (_orbitJobs != null) { foreach (var j in _orbitJobs) if (j != null && j.sr != null && j.sr.gameObject.activeSelf) o.Add(j.sr.transform.position); }
            else foreach (var t in _orbs) if (t != null && t.gameObject.activeSelf) o.Add(t.transform.position);
            o.AddRange(_awayPts53);
        }
        int OrbN53(SkillInst o) { if (o == null) return 0; var T = HellordTuning.I; return HData.At(T.orbitCount, o.level, 3) + (o.Has(SupportId.MoreOrbitals) ? HData.At(T.supOrbitals, o.Lvl(SupportId.MoreOrbitals)) : 0); }
        int OrbitCount53() => OrbN53(Own53(SkillId.BoneOrbit)) + _crestBone53.Count + _roomBones53;
        bool RuneHold53 => Has51(BoneWardenGone539, 4) && _rune53.Count > 0 && Own53(SkillId.BoneOrbit) != null;
        bool PactOrbitAway53() => _pactActive51 && (RuneHold53 || _launchFly53 > 0);
        SkillInst LegionSk53()
        {
            if (_legionSk53 == null) _legionSk53 = new SkillInst { def = HData.Skill(SkillId.BoneOrbit) };
            int lv = 1; _legionSk53.sup.Clear();
            bool necro = Has51(BoneWardenGone539, 9);
            foreach (var s in skills) if (necro || (s.def.tags & (STag.Orbital | STag.Melee)) != 0) { lv = Mathf.Max(lv, s.level); foreach (var u in s.sup) if (!_legionSk53.sup.Contains(u) && _legionSk53.sup.Count < 8) { _legionSk53.sup.Add(u); _legionSk53.supLvl[(int)u] = s.supLvl[(int)u]; } }
            _legionSk53.level = lv; return _legionSk53;
        }

        // ================= tick
        void PactTick53(float dt)
        {
            _sprN53 = 0;
            if (!_pactActive51) { SlothCd53 = SlothArea53 = PactCdS53 = 1f; BoneAreaS536 = 1f; HideSpr53(0); NecroTick536(dt); HideSk561(); return; } // t561u: summon skills live without a Pact
            if (_boneSp53 == null) { var a = HArt.Proj("proj_bone"); if (a != null && a.Length > 0) _boneSp53 = a[0]; }
            _empT53 -= dt; _heatLock53 -= dt; _emberCd53 -= dt; _infBackCd53 -= dt; _icdShock53 -= dt; _icdKill53 -= dt; _leechT53 = Mathf.Max(0, _leechT53 - PT.martyrLeechCapPerS53 * dt);
            Vector2 hc = HeroCenter; bool moving = (_heroPos - _lastHero53).sqrMagnitude > 1e-5f; _lastHero53 = _heroPos;

            // --- queued triggers & novas
            int tq = Mathf.Min(_trigQ53.Count, 4); for (int i = 0; i < tq; i++) RunTrig53(_trigQ53[i]); _trigQ53.Clear();
            int nn = _nova53.Count;
            for (int i = 0; i < nn && i < _nova53.Count; i++)
            {
                var v = _nova53[i]; v.t -= dt;
                if (v.t > 0) { _nova53[i] = v; ZRing(v.c, v.r, new Color(1f, 0.4f, 0.1f, 0.8f), 0.75f); ZDisc(v.c, v.r * (1f - v.t / PT.meteorDelay53), new Color(1f, 0.3f, 0.1f, 0.25f), 0.75f); continue; }
                _nova53.RemoveAt(i); i--; nn--;
                if (v.kind == 4)
                {   // Cinder Split / crest spread: jump burn to nearest enemies
                    int jumps = Has51(PactPath.Pyromancer, 1) ? 2 : 1; float r = Has51(PactPath.Pyromancer, 1) ? PT.cinderR53 : 6f;
                    for (int j = 0; j < jumps; j++)
                    {
                        int best = -1; float bd = r * r;
                        for (int q = 0; q < _en.Length; q++) { var e = _en[q]; if (e == null || !e.alive || e.dying || e.burnT > 0) continue; float dd = (e.pos - v.c).sqrMagnitude; if (dd < bd) { bd = dd; best = q; } }
                        if (best < 0) break; var en = _en[best];
                        en.burnDps = Mathf.Max(en.burnDps, v.dmg); en.burnT = HellordTuning.I.burnDuration; IgniteBurn(en);
                        ZSeg(v.c + Vector2.up * 0.5f, en.pos + Vector2.up * en.hitY, 0.1f, new Color(1f, 0.5f, 0.1f, 0.9f), 2);
                    }
                    continue;
                }
                HellFX.HellfireNova(v.c, v.r * HArt.PPU, 0.4f, false);
                _pq53.Clear(); Query(v.c, v.r + 0.5f, _pq53);
                foreach (var q in _pq53) { var e = _en[q]; if (!e.alive || e.dying || (e.pos - v.c).magnitude > v.r + e.radius) continue; if (v.s != null) Hit(e, v.dmg, v.s, (e.pos - v.c).normalized * 0.4f); else Damage(e, v.dmg, true, false); if (v.st != Status.None) ApplyStatus(e, v.st, v.dmg, 1f, v.s); }
            }

            PyroTick53(dt, hc);
            BoneTick53(dt, hc, moving);
            StormTick53(dt, hc);
            BloodTick53(dt, hc);
            NecroTick536(dt); HideSk561(); // t536u / t561u
            HudBars53();
            HideSpr53(_sprN53);
        }

        void PyroTick53(float dt, Vector2 hc)
        {
            if (!Path53(PactPath.Pyromancer)) return;
            _heat53 = Mathf.Max(0, _heat53 - PT.heatDecay53 * dt);
            for (int i = _trail53.Count - 1; i >= 0; i--) { var g = _trail53[i]; g.t -= dt; if (g.t <= 0) { _trail53.RemoveAt(i); continue; } _trail53[i] = g; ZSeg(g.a, g.b, PT.trailWidth53 * 2f, new Color(1f, 0.45f, 0.1f, 0.35f * Mathf.Min(1, g.t)), 0); }
            if (_trail53.Count > 0 && (_trailBurnT53 -= dt) <= 0)
            {
                _trailBurnT53 = 0.5f; var fb = Own53(SkillId.Fireball); float bd = fb != null ? SkillDamage(fb) : 6f * Inc53;
                foreach (var g in _trail53) { _pq53.Clear(); Query((g.a + g.b) * 0.5f, (g.a - g.b).magnitude * 0.5f + 1f, _pq53); foreach (var q in _pq53) { var e = _en[q]; if (e.alive && !e.dying && SegDist53(e.pos, g.a, g.b) < PT.trailWidth53 + e.radius) ApplyStatus(e, Status.Burn, bd, 1f, fb); } }
            }
            for (int i = _patch53.Count - 1; i >= 0; i--)
            {
                var z = _patch53[i]; z.t -= dt; z.tick -= dt; if (z.t <= 0) { _patch53.RemoveAt(i); continue; }
                ZDisc(z.c, z.r, new Color(1f, 0.35f, 0.05f, 0.28f), 0.75f);
                if (z.tick <= 0) { z.tick = 0.5f; _pq53.Clear(); Query(z.c, z.r + 0.5f, _pq53); foreach (var q in _pq53) { var e = _en[q]; if (e.alive && !e.dying && (e.pos - z.c).magnitude < z.r + e.radius) ApplyStatus(e, Status.Burn, z.dmg, 1f, z.s); } }
                _patch53[i] = z;
            }
            if (_steam53.Count > 0) SteamBullets53();
            for (int i = _steam53.Count - 1; i >= 0; i--) { var z = _steam53[i]; z.t -= dt; if (z.t <= 0) { _steam53.RemoveAt(i); continue; } _steam53[i] = z; ZDisc(z.c, z.r, new Color(0.85f, 0.88f, 0.92f, 0.25f), 0.75f); }
            for (int i = _ember53.Count - 1; i >= 0; i--) { _ember53[i] -= dt; if (_ember53[i] <= 0) _ember53.RemoveAt(i); }
            for (int i = 0; i < _ember53.Count; i++) { float a = runTime * 3f + i * Mathf.PI * 2f / _ember53.Count; ZDisc(hc + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.75f) * 1.1f, 0.12f, new Color(1f, 0.6f, 0.15f, 0.95f), 1f, 3); }
            if (Has51(PactPath.Pyromancer, 9))
            {
                if (hp > 1) hp = Mathf.Max(1, hp - maxHp * PT.infernoSelfBurn53 * dt);
                ZRing(_heroPos, PT.infernoAuraR53, new Color(1f, 0.35f, 0.05f, 0.45f), 0.75f);
                if ((_infAuraT53 -= dt) <= 0)
                {
                    _infAuraT53 = 0.5f; float d = PT.infernoAuraDmg53 * Inc53;
                    _pq53.Clear(); Query(_heroPos, PT.infernoAuraR53 + 0.5f, _pq53);
                    foreach (var q in _pq53) { var e = _en[q]; if (e.alive && !e.dying && (e.pos - _heroPos).magnitude < PT.infernoAuraR53 + e.radius) { ApplyStatus(e, Status.Burn, d, 1f); Damage(e, d, false, true, true); } }
                }
            }
        }

        void BoneTick53(float dt, Vector2 hc, bool moving)
        {
            if (!Path53(BoneWardenGone539)) { SlothCd53 = SlothArea53 = 1f; return; }
            // Sloth Throne
            if (Has51(BoneWardenGone539, 8)) { _stillT53 = moving ? 0 : _stillT53 + dt; bool still = _stillT53 >= 0.5f; SlothArea53 = still ? PT.slothStillArea53 : PT.slothMoveArea53; SlothCd53 = still ? PT.slothStillCd53 : 1f; if (still) ZRing(_heroPos, 0.9f, new Color(0.8f, 0.75f, 0.6f, 0.5f), 0.75f); }
            else SlothCd53 = SlothArea53 = 1f;
            var orb = Own53(SkillId.BoneOrbit);
            // Ossuary Ward: hero orbit bones block bullets
            if (Has51(BoneWardenGone539, 1))
            {
                _pts53.Clear(); OrbitPts53(_pts53);
                for (int k = 0; k < _pts53.Count && k < 32; k++)
                {
                    if (_ossDown53[k] > 0) { _ossDown53[k] -= dt; if (_ossDown53[k] <= 0) _ossHits53[k] = 0; continue; }
                    Vector2 bp = _pts53[k];
                    if (_eb49 != null) foreach (var b in _eb49) { if (b == null || !b.alive || b.vis) continue; if ((b.p - bp).sqrMagnitude < 0.3f) { KillBullet49(b, true); if (++_ossHits53[k] >= 3) { _ossDown53[k] = PT.ossuaryRegrow53; break; } } }
                    if (_ossDown53[k] <= 0 && _bb != null) foreach (var b in _bb) { if (b == null || !b.alive) continue; if ((b.pos - bp).sqrMagnitude < 0.3f) { KillBB(b, true); if (++_ossHits53[k] >= 3) { _ossDown53[k] = PT.ossuaryRegrow53; break; } } }
                }
            }
            // extra bones: crest zone bones + Marrow Harvest room bones
            for (int i = _crestBone53.Count - 1; i >= 0; i--) { _crestBone53[i] -= dt; if (_crestBone53[i] <= 0) _crestBone53.RemoveAt(i); }
            int extra = _crestBone53.Count + _roomBones53;
            if (extra > 0)
            {
                _extraAng53 += dt * 3.2f; float R = 1.9f * PactOrbitR53(); var src = orb ?? Own53(SkillId.GravePool) ?? Own53(SkillId.CurseRune); float dmg = src != null ? SkillDamage(src) * PT.extraBoneDmg53 : 6f * Inc53;
                for (int k = 0; k < extra; k++)
                {
                    float a = _extraAng53 + k * Mathf.PI * 2f / extra; Vector2 p = hc + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.75f) * R;
                    Spr53(p, a * Mathf.Rad2Deg, 1f, Color.white);
                    BoneHit53(p, 0.55f, dmg, src ?? LegionSk53());
                }
            }
            // Marrow Launch: the Bone Orbit bones fly out 6 m as real projectiles (Projectile supports: Pierce/Fork/Homing/Multiple) and come back
            _awayPts53.Clear(); _launchFly53 = 0;
            if (Has51(BoneWardenGone539, 0) && orb != null)
            {
                if ((_launchT53 -= dt) <= 0)
                {
                    _launchT53 = PT.launchEvery53; int n = Mathf.Max(1, OrbN53(orb)); float sp = PT.launchDist53 / (PT.launchTime53 * 0.5f);
                    float dmg = SkillDamage(orb) * PactSkillMul51(orb); bool fork = orb.Has(SupportId.Fork);
                    for (int k = 0; k < n; k++)
                    {
                        float a = runTime * 2f + k * Mathf.PI * 2f / n; Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                        _lastProj53 = null; SpawnProj(orb, hc, d * sp, dmg, 99, fork, 0, 0f);
                        var p = _lastProj53; if (p == null) continue; p.bone53 = true; p.life = PT.launchTime53 * 0.5f; p.vel = d * sp;
                        if (_boneSp53 != null) p.sr.sprite = _boneSp53;
                    }
                }
                foreach (var p in _pr) if (p.alive && p.bone53) { _launchFly53 = 1; _awayPts53.Add(p.pos); }
            }
            // Rune Ossuary: a bone circle on the last rune hits cursed enemies inside
            if (Has51(BoneWardenGone539, 4) && _rune53.Count > 0 && orb != null)
            {
                var z = _rune53[_rune53.Count - 1]; int n = Mathf.Max(1, OrbN53(orb));
                for (int k = 0; k < n; k++) { float a = runTime * 2.5f + k * Mathf.PI * 2f / n; Vector2 bp = z.c + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.75f) * z.r; _awayPts53.Add(bp); Spr53(bp, a * Mathf.Rad2Deg, 0.9f, new Color(0.85f, 0.75f, 1f)); }
                _pq53.Clear(); Query(z.c, z.r + 0.5f, _pq53);
                foreach (var q in _pq53) { var e = _en[q]; if (!e.alive || e.dying || e.curseT <= 0 || e.orbitCd > 0 || (e.pos - z.c).magnitude > z.r + e.radius) continue; e.orbitCd = orb.def.cooldown * orb.CdMul; Hit(e, SkillDamage(orb) * PactSkillMul51(orb), orb, (e.pos - z.c).normalized * 3); }
            }
            // Legion
            if (_legion53.Count > 0)
            {
                var sk = LegionSk53(); float dmg = SkillDamage(sk) * PT.legionDmgMul53;
                for (int i = _legion53.Count - 1; i >= 0; i--)
                {
                    var m = _legion53[i]; m.t -= dt; m.atk -= dt; if (m.t <= 0) { HordeFX.CheapBurst(m.p, new Color(0.9f, 0.9f, 0.8f), 5); _legion53.RemoveAt(i); continue; }
                    int t = Nearest(m.p, 7f, -1);
                    if (t >= 0) { var e = _en[t]; Vector2 to = e.pos - m.p; if (to.magnitude > 0.9f) m.p += to.normalized * 5f * dt; else if (m.atk <= 0) { m.atk = 0.7f; Hit(e, dmg, sk, to.normalized * 0.3f); } }
                    else { float a = i * 2.4f; Vector2 home = _heroPos + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 1.6f; m.p = Vector2.MoveTowards(m.p, home, 5f * dt); }
                    ZDisc(m.p, 0.3f, new Color(0, 0, 0, 0.35f), 0.5f); Spr53(m.p + Vector2.up * 0.5f, 90f, 1.7f, new Color(0.9f, 0.88f, 0.8f));
                }
            }
            // Wall of the Dead
            for (int i = _wall53.Count - 1; i >= 0; i--)
            {
                var w = _wall53[i]; w.t -= dt; Vector2 ab = w.b - w.a, nrm = new Vector2(-ab.y, ab.x).normalized;
                ZSeg(w.a, w.b, 0.35f, new Color(0.92f, 0.9f, 0.82f, 0.9f), 2);
                for (int k = 0; k <= 4; k++) Spr53(Vector2.Lerp(w.a, w.b, k / 4f) + Vector2.up * 0.3f, 90f, 1.4f, Color.white);
                _pq53.Clear(); Query((w.a + w.b) * 0.5f, ab.magnitude * 0.5f + 1f, _pq53);
                foreach (var q in _pq53) { var e = _en[q]; if (!e.alive || e.dying || e.boss) continue; float d = SegDist53(e.pos, w.a, w.b); if (d < 0.6f) { float side = Vector2.Dot(e.pos - w.a, nrm) >= 0 ? 1 : -1; e.pos += nrm * side * (0.6f - d); } }
                if (w.t <= 0)
                {
                    _wall53.RemoveAt(i); HordeFX.CheapBurst((w.a + w.b) * 0.5f, new Color(0.92f, 0.9f, 0.82f), 12);
                    foreach (var q in _pq53) { var e = _en[q]; if (e.alive && !e.dying && SegDist53(e.pos, w.a, w.b) < 1.6f) Damage(e, w.dmg, true, false); }
                    continue;
                }
                _wall53[i] = w;
            }
        }
        void BoneHit53(Vector2 p, float r, float dmg, SkillInst s)
        {
            _pq53.Clear(); Query(p, r + 0.6f, _pq53);
            foreach (var q in _pq53) { var e = _en[q]; if (!e.alive || e.dying) continue; if ((e.pos + Vector2.up * e.hitY * 0.5f - p).magnitude > r + e.radius) continue; if (q < _extraCd53.Length && _extraCd53[q] > runTime) continue; if (q < _extraCd53.Length) _extraCd53[q] = runTime + 0.4f; Hit(e, dmg, s, (e.pos - _heroPos).normalized * 0.3f); }
        }

        void StormTick53(float dt, Vector2 hc)
        {
            if (!Path53(PactPath.Stormcaller)) return;
            _energy53 = Mathf.Min(EnergyMax53, _energy53 + PT.energyRegen53 * dt);
            var ring = Own53(SkillId.StormRing);
            if (Has51(PactPath.Stormcaller, 1) && ring != null) _cap53 = Mathf.Min(10f, _cap53 + dt / 0.5f);
            if (Has51(PactPath.Stormcaller, 5) && (_rodT53 -= dt) <= 0)
            {
                bool inZone = false;
                foreach (var q in _pools44) if ((q.at - _heroPos).magnitude <= q.r) inZone = true;
                foreach (var z in _rune53) if ((z.c - _heroPos).magnitude <= z.r) inZone = true;
                if (inZone)
                {
                    _rodT53 = 1f; _pq53.Clear(); Query(_heroPos, PT.rodRange53, _pq53); int pick = -1, seen = 0;
                    foreach (var q in _pq53) { var e = _en[q]; if (!e.alive || e.dying) continue; if (Random.Range(0, ++seen) == 0) pick = q; }
                    if (pick >= 0)
                    {
                        var e = _en[pick]; var s = Own53(SkillId.ChainLightning) ?? ring ?? (skills.Count > 0 ? skills[0] : null);
                        if (s != null) { Hit(e, SkillDamage(s) * PT.rodDmgMul53, s, Vector2.zero); ApplyStatus(e, Status.Shock, 0, 1f, s); }
                        Vector3 top = e.pos + Vector2.up * 5f; ChainFX.ChainLightningPoints(top, e.pos + Vector2.up * e.hitY, default, default, 2, new Color(0.6f, 0.85f, 1f), 0.02f, 0.2f);
                    }
                }
                else _rodT53 = 0.1f;
            }
        }

        void BloodTick53(float dt, Vector2 hc)
        {
            if (!Path53(PactPath.BloodReaver)) { PactCdS53 = 1f; _shield53 = 0; return; }
            if (Has51(PactPath.BloodReaver, 1))
            {
                if (HInput.DashHeld()) { _holdDash53 += dt; if (_holdDash53 >= PT.wrathHoldS53 && !_wrathLatch53) { _wrathLatch53 = true; _wrath53 = !_wrath53; Toast(_wrath53 ? "PRICE OF WRATH: ON" : "PRICE OF WRATH: OFF"); } }
                else { _holdDash53 = 0; _wrathLatch53 = false; }
                if (_wrath53 && hp < maxHp * 0.1f) { _wrath53 = false; Toast("PRICE OF WRATH: OFF"); }
            }
            else _wrath53 = false;
            PactCdS53 = _wrath53 ? PT.wrathCdMul53 : 1f;
            if (_wrath53) ZRing(_heroPos, 0.8f, new Color(0.9f, 0.1f, 0.15f, 0.7f), 0.75f);
            for (int i = _orb53.Count - 1; i >= 0; i--)
            {
                var o = _orb53[i]; o.t -= dt; if (o.t <= 0) { _orb53.RemoveAt(i); continue; }
                float d = (o.p - _heroPos).magnitude; if (d < 0.6f) { Heal53(PT.redFeastHeal53); _orb53.RemoveAt(i); continue; }
                if (d < 2.5f * pickupMul) o.p = Vector2.MoveTowards(o.p, _heroPos, 9f * dt);
                _orb53[i] = o; ZDisc(o.p + Vector2.up * 0.2f, 0.16f, new Color(0.85f, 0.05f, 0.12f, 1f), 1f, 3);
            }
            if (_frReady53) ZRing(_heroPos, 1.2f, new Color(0.9f, 0.1f, 0.2f, 0.8f), 0.75f);
        }

        void HudBars53()
        {
            Vector2 b = _heroPos + Vector2.down * 0.55f; float w = 1.1f; int row = 0;
            void Bar(float f, Color c) { Vector2 o = b + Vector2.down * 0.14f * row++; ZSeg(o - Vector2.right * w * 0.5f, o + Vector2.right * w * 0.5f, 0.08f, new Color(0, 0, 0, 0.6f), 4); f = Mathf.Clamp01(f); if (f > 0) ZSeg(o - Vector2.right * w * 0.5f, o + Vector2.right * (f - 0.5f) * w, 0.07f, c, 5); }
            if (Has51(PactPath.Pyromancer, 3)) Bar(_heatLock53 > 0 ? 1 : _heat53 / 100f, _heatLock53 > 0 ? new Color(1, 0.2f, 0.1f) : new Color(1f, 0.6f, 0.1f));
            if (Path53(PactPath.Stormcaller) && !Has51(PactPath.Stormcaller, 8)) Bar(_energy53 / EnergyMax53, _energy53 > 100 ? new Color(0.9f, 0.95f, 1f) : new Color(0.4f, 0.7f, 1f));
            if (Has51(PactPath.Stormcaller, 1) && _cap53 > 0) Bar(_cap53 / 10f, new Color(0.6f, 0.85f, 1f));
            if (_shield53 > 0) Bar(_shield53 / Mathf.Max(1, ShieldCap53()), new Color(1f, 0.85f, 0.4f));
            if (Has51(PactPath.BloodReaver, 5)) Bar(_fr53 / 10f, new Color(0.85f, 0.1f, 0.15f));
        }

        // ================= room hooks
        void PactRoomClear53() { _roomBones53 = 0; _harvestK53 = 0; }
        void PactRoomEnter53() { NecroRoomEnter536(); _lastRitesUsed51 = false; _trail53.Clear(); _patch53.Clear(); _steam53.Clear(); _rune53.Clear(); _wall53.Clear(); _launch53.Clear(); _orb53.Clear(); _nova53.Clear(); _trigQ53.Clear(); foreach (var m in _legion53) m.p = _heroPos; foreach (var q in _pools44) if (q.anchor53) q.at = _heroPos; }

        // ================= helpers
        static float SegDist53(Vector2 p, Vector2 a, Vector2 b) { Vector2 ab = b - a; float t = ab.sqrMagnitude < 1e-6f ? 0 : Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude); return (p - (a + ab * t)).magnitude; }
        void Spr53(Vector2 p, float rot, float scale, Color c)
        {
            if (_boneSp53 == null) { ZDisc(p, 0.15f * scale, c, 1f, 3); return; }
            if (_sprN53 >= _spr53.Count) _spr53.Add(NewSR("pact53_bone", null, _boneSp53, 0));
            var sr = _spr53[_sprN53++]; if (!sr.gameObject.activeSelf) sr.gameObject.SetActive(true);
            sr.transform.position = p; sr.transform.rotation = Quaternion.Euler(0, 0, rot); sr.transform.localScale = Vector3.one * scale; sr.color = c; sr.sortingOrder = YSort(p.y) + 30;
        }
        void HideSpr53(int from) { for (int i = from; i < _spr53.Count; i++) if (_spr53[i] != null && _spr53[i].gameObject.activeSelf) _spr53[i].gameObject.SetActive(false); }
    }
}
