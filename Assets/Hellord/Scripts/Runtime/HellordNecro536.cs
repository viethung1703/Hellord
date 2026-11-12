// t536u (v0.0.53): Necromancer Pact, 18 nodes. Group A SUMMON (idx 0..8) acts on MINIONS, Group B BONE SKILLS (idx 9..17) acts on skills listed in PT.necroBoneSkills536.
// t561u: permanent minions come from the Skeleton Warriors / Skeleton Archers skills (HellordSummon561.cs); temp ones from A7 Ossuary Order. Minions block demons physically by baseline (demon contact hurts them).
using System.Collections.Generic;
using UnityEngine;
using PixelFXPack;

namespace Hellord
{
    public partial class HellordGame
    {
        public static float BoneAreaS536 = 1f; static int[] _boneIds536;
        public static bool BoneDef536(SkillDef d) { if (d == null || (d.tags & STag.Minion) != 0 || _boneIds536 == null) return false; return System.Array.IndexOf(_boneIds536, (int)d.id) >= 0; }
        public static float BoneArea536(SkillDef d) => BoneAreaS536 != 1f && BoneDef536(d) ? BoneAreaS536 : 1f;
        bool Bone536(SkillInst s) => s != null && BoneDef536(s.def);
        bool NHas536(int i) => Has51(PactPath.Necromancer, i);
        bool Necro536 => Path53(PactPath.Necromancer);

        class NMin536 { public bool sin818; public float blk818 = -99f; public int blkN818; /* t818u #39 risen / #18 free blocks */ public Vector2 p; public int kind; public float hp, maxHp, atk, life; public bool temp; public int shots; public Vector2 prev, face = Vector2.down; public float atkT = -9f, moveT; public int tgt = -1; public int st; public float stT, roamT, stuckT; public Vector2 roam, stuckP; public Vector2 atkFace = Vector2.down, wantFace = Vector2.down; public float walkHold, hurtT, hurtCd, lastHp = -1f; public int dirK = -1; public bool dirFlip; }
        readonly List<NMin536> _nmin536 = new List<NMin536>();
        readonly float[][] _nresp536 = { new float[16], new float[16] }; readonly int[] _nrespN536 = new int[2];
        struct Tr536 { public Vector2 a, b; public float t; public Color c; }
        readonly List<Tr536> _ntr536 = new List<Tr536>();
        bool _nTest536; int _ossK536, _harvK536, _harvC536; float _splCd536; bool _nInX536; SkillInst _nSk536;

        void NecroReset536() { _nmin536.Clear(); _ntr536.Clear(); _nrespN536[0] = _nrespN536[1] = 0; _ossK536 = _harvK536 = _harvC536 = 0; _splCd536 = 0; _nInX536 = false; } // keeps _nTest536 for the run
        SkillInst NSk536(int kind)
        {
            var real = NSkill561(kind); if (real != null) return real; // v0.0.62 t614u: minion hits carry the summon skill's socketed supports (was a support-less dummy)
            if (_nSk536 == null) _nSk536 = new SkillInst { def = new SkillDef { id = SkillId.BoneOrbit, name = "Skeleton", tags = STag.Hit | STag.Minion, baseDmg = 0, cooldown = 1, innate = Status.None } };
            _nSk536.def.tags = STag.Hit | STag.Minion | (kind == 0 ? STag.Melee : STag.Projectile); return _nSk536;
        }
        // ---- counts & stats
        int NBase536(int kind) { if (NHas536(16)) return 0; int lv = NSkillLv561(kind); if (lv <= 0) return 0; int n = HData.At(kind == 0 ? PT.skelWarCount561 : PT.skelArcCount561, lv, 1); if (n > 0 && NHas536(0)) n++; return SinWarCount818(kind, n); } /* t818u #42 */ // t561u: from summon skill level // A1 Grave Muster: +1 per type you have
        int NCap536() => PT.minionCap561 + (NHas536(8) ? PT.legionCap536 : 0);
        bool NCrest536 => Necro536 && Crest53;
        float NHpMul536() => (NHas536(1) ? PT.ironMarrowHp536 : 1f) * (NHas536(7) ? PT.cryptHp536 : 1f) * (NCrest536 ? PT.necroCrestMinion536 : 1f);
        float NDmg536(int kind) => NBaseDmg561(kind) * (1 + incDmg * PT.minionPlayerDmgShare561) * PDmg49() * (NCrest536 ? PT.necroCrestMinion536 : 1f) * (NHas536(7) ? PT.cryptDmg536 : 1f) * (NHas536(8) ? PT.legionDmg536 : 1f) * (kind == 0 && NHas536(4) ? PT.graveEdgeMul536 : 1f) * NSupDmg614(kind);
        float NSupDmg614(int kind) { var r = NSkill561(kind); return r != null && r.Has(SupportId.AddedDamage) ? HData.At(HellordTuning.I.supAddedDmg, r.Lvl(SupportId.AddedDamage), 1.15f) : 1f; } // v0.0.62 t614u
        float NRate536(int kind) => SinMinRate818(kind) * (kind == 0 ? PT.skelWarRate561 : PT.skelArcRate561) / (NHas536(2) ? PT.hollowHaste536 : 1f) * NSupRate614(kind);
        float NSupRate614(int kind) { var r = NSkill561(kind); return r != null && r.Has(SupportId.FasterCasting) ? HData.At(HellordTuning.I.supFasterCd, r.Lvl(SupportId.FasterCasting), 0.9f) : 1f; } // v0.0.62 t614u: Faster Casting = shorter minion attack interval
        int NCount536(int kind, bool temp) { int c = 0; foreach (var m in _nmin536) if (m.kind == kind && m.temp == temp) c++; return c; }
        void NSpawn536(int kind, bool temp, Vector2 at, float life = -1f)
        {
            if (_nmin536.Count >= NCap536() || NHas536(16)) return;
            float hp = NBaseHp561(kind) * NHpMul536();
            _nmin536.Add(new NMin536 { p = at, prev = at, kind = kind, hp = hp, maxHp = hp, life = temp ? (life > 0 ? life : PT.ossuaryLife536) : 1e9f, temp = temp });
            FxSpawn561(at); // t561u: bone/ivory grave ring (was green CheapBurst)
        }

        // ---- hooks (called from HellordPact53)
        float NecroDmgMul536(SkillInst s)
        {
            if (!Necro536 || s == null || (s.def.tags & STag.Minion) != 0) return 1f; float m = 1f;
            if (NHas536(7)) m *= PT.cryptSelf536; // AK Crypt King: non-minion skills
            if (Bone536(s)) { if (NHas536(9)) m *= PT.marrowBiteMul536; if (Crest53) m *= PT.necroCrestBone536; if (NHas536(16)) m *= PT.tyrantMul536; if (NHas536(17)) m *= PT.colossusMul536; }
            return m;
        }
        float NecroCd536(SkillInst s) { if (!Necro536 || !Bone536(s)) return 1f; return (NHas536(12) ? PT.dryOssuaryCd536 : 1f) * (NHas536(17) ? PT.colossusCd536 : 1f); }
        float NecroPreHit536(Enemy e, float dmg, SkillInst s, bool tick)
        {
            if (!Necro536) return dmg;
            if (e.brit536 > runTime) dmg *= PT.brittleMul536; // B3 any damage source
            if (!tick && !_nInX536 && Bone536(s) && NHas536(15) && _harvC536 > 0) { dmg *= 1f + PT.harvestPer536 * _harvC536; _harvC536 = 0; HordeFX.CheapBurst(e.pos + Vector2.up * e.hitY, new Color(0.95f, 0.9f, 0.7f), 10); }
            return dmg;
        }
        void NecroPostHit536(Enemy e, float dmg, SkillInst s, bool tick)
        {
            if (!Necro536 || tick || _nInX536 || !Bone536(s)) return;
            if (NHas536(11)) e.brit536 = runTime + PT.brittleS536;
            if (NHas536(14) && e.alive && !e.dying)
            {
                ApplyStatus(e, Status.Chill, dmg, 1f, null);
                if (++e.calc53 >= PT.calcifyStacks536) { e.calc53 = 0; if (!e.boss) e.stun53 = Mathf.Max(e.stun53, PT.calcifyStun536); HordeFX.CheapBurst(e.pos + Vector2.up * e.hitY, new Color(0.9f, 0.9f, 0.8f), 6); }
            }
            _nInX536 = true;
            try
            {
                if (NHas536(16) && e.alive && !e.dying) Hit(e, dmg * PT.tyrantEcho536, s, Vector2.zero); // BK 2nd hit
                if (NHas536(10) && _splCd536 <= 0)
                {
                    _splCd536 = PT.splinterIcd536; _pq53.Clear(); Query(e.pos, PT.splinterR536, _pq53); int n = 0; Vector2 o = e.pos + Vector2.up * e.hitY;
                    foreach (var q in _pq53) { var x = _en[q]; if (x == e || !x.alive || x.dying) continue; Hit(x, dmg * PT.splinterDmg536, s, Vector2.zero); _ntr536.Add(new Tr536 { a = o, b = x.pos + Vector2.up * x.hitY, t = 0.08f, c = new Color(0.76f, 0.52f, 0.41f, 0.6f) } /* t605u: bone-tan, translucent, shorter (was a 1.0-alpha near-white line) */); if (++n >= PT.splinterN536) break; }
                }
            }
            finally { _nInX536 = false; }
        }
        void NecroKill536(Enemy e)
        {
            if (!Necro536) return;
            if (NHas536(15) && ++_harvK536 >= PT.harvestKills536) { _harvK536 = 0; if (_harvC536 < PT.harvestMax536) { _harvC536++; Toast("BONE CHARGE " + _harvC536); } }
            if (NHas536(6) && NSkillLv561(0) > 0 && ++_ossK536 >= PT.ossuaryKills536) { _ossK536 = 0; if (NCount536(0, true) < PT.ossuaryMax536) NSpawn536(0, true, e.pos); }
        }

        // ---- tick
        void NecroTick536(float dt)
        {
            _boneIds536 = PT.necroBoneSkills536;
            BoneAreaS536 = Necro536 ? (NHas536(13) ? PT.graveReachArea536 : 1f) * (NHas536(17) ? PT.colossusArea536 : 1f) : 1f;
            _splCd536 -= dt;
            if (!Necro536 && !HasSummon561 && !SinRisen818()) { // t818u #39 risen demons keep the tick alive
                if (_nmin536.Count > 0) _nmin536.Clear(); _dead561.Clear(); _fx561.Clear(); return; }
            if (NHas536(16) && _nmin536.Count > 0) _nmin536.Clear(); // BK Ossuary Tyrant: cannot have minions
            // respawn / fill permanent minions
            for (int k = 0; k < 2; k++)
            {
                int want = NBase536(k), have = NCount536(k, false) + _nrespN536[k];
                if (have > want) { for (int i = _nmin536.Count - 1; i >= 0 && have > want; i--) if (_nmin536[i].kind == k && !_nmin536[i].temp) { _nmin536.RemoveAt(i); have--; } }
                while (have < want && _nrespN536[k] < 16) { _nresp536[k][_nrespN536[k]++] = 0.3f; have++; }
                for (int j = _nrespN536[k] - 1; j >= 0; j--) { _nresp536[k][j] -= dt; if (_nresp536[k][j] <= 0) { _nresp536[k][j] = _nresp536[k][--_nrespN536[k]]; NSpawn536(k, false, _heroPos + (k == 1 ? Random.insideUnitCircle.normalized * 2f : Random.insideUnitCircle * 1.2f)); } }
            }
            for (int i = _nmin536.Count - 1; i >= 0; i--)
            {
                var m = _nmin536[i]; m.life -= dt; m.atk -= dt; float mx = NBaseHp561(m.kind) * NHpMul536(); if ((m.p - _heroPos).magnitude > PT.minionTeleport561) { Vector2 o = m.p; m.p = _heroPos + Random.insideUnitCircle; ClampToRoom(ref m.p, 6); m.prev = m.p; FxBlink561(o, m.p); } if (Mathf.Abs(mx - m.maxHp) > 0.01f) { m.hp *= mx / m.maxHp; m.maxHp = mx; }
                // baseline: demons touching a minion hurt it
                _pq53.Clear(); Query(m.p, 1.2f, _pq53); foreach (var q in _pq53) { var e = _en[q]; if (e.alive && !e.dying && e.stun53 <= 0 && (e.pos - m.p).magnitude < e.radius + 0.35f) m.hp -= e.dmg * PT.minionHurtMul536 * dt; }
                Block561(m);
                if (m.lastHp >= 0 && m.hp < m.lastHp - 0.01f && m.hurtCd <= 0) { m.hurtT = PT.minionHurtFlash602; m.hurtCd = PT.minionHurtCd602; } m.lastHp = m.hp; m.hurtT -= dt; m.hurtCd -= dt; // t602u: tint flash only, never an anim reset
                if (m.hp <= 0 || m.life <= 0)
                {
                    FxDeath561(m.p); _nmin536.RemoveAt(i); _dead561.Add(new Dead561 { p = m.p, kind = m.kind, t = 0f, face = m.face });
                    if (!m.temp && _nrespN536[m.kind] < 16) _nresp536[m.kind][_nrespN536[m.kind]++] = NResp561(m.kind) * (NHas536(3) ? PT.quickGrave536 : 1f);
                    continue;
                }
                var sk = NSk536(m.kind); float dmg = NDmg536(m.kind);
                Brain590(m, i, dt, sk, dmg); // t590u: PoE2-style FSM (ROAM / SEEK / FIGHT / RETURN / RECOVER)
                Sep582(m, i, dt);
                Color c = m.temp ? new Color(0.75f, 0.95f, 1f) : m.kind == 0 ? new Color(0.92f, 0.9f, 0.8f) : new Color(0.75f, 1f, 0.7f);
                ZDisc(m.p, 0.3f, new Color(0, 0, 0, 0.35f), 0.5f); if (!Skel561(m, i, dt)) Spr53(m.p + Vector2.up * 0.5f, m.kind == 0 ? 90f : 0f, m.kind == 0 ? 1.7f : 1.4f, c);
                if (m.hp < m.maxHp) { Vector2 hb = m.p + Vector2.up * 1.05f; ZSeg(hb - Vector2.right * 0.3f, hb + Vector2.right * 0.3f, 0.06f, new Color(0, 0, 0, 0.6f), 4); ZSeg(hb - Vector2.right * 0.3f, hb + Vector2.right * (m.hp / m.maxHp - 0.5f) * 0.6f, 0.05f, new Color(0.5f, 1f, 0.5f), 5); }
            }
            Dead561Tick(dt); FxTick561(dt);
            for (int i = _ntr536.Count - 1; i >= 0; i--) { var r = _ntr536[i]; r.t -= dt; if (r.t <= 0) { _ntr536.RemoveAt(i); continue; } _ntr536[i] = r; ZSeg(r.a, r.b, Px561, r.c, 3); }
            if (NHas536(15) && _harvC536 > 0) for (int k = 0; k < _harvC536; k++) ZDisc(_heroPos + Vector2.up * 1.6f + Vector2.right * (k - (_harvC536 - 1) * 0.5f) * 0.25f, 0.08f, new Color(0.95f, 0.92f, 0.8f), 1f, 5);
        }
        // ================= t590u minion brain =================
        // States: 0 ROAM (no enemy: wander points on a wide ring around the hero), 1 SEEK (run to the chosen enemy),
        // 2 FIGHT (in range: warriors swing from a flank slot, archers hold standoff / kite / shoot), 3 RETURN (too far / target gone: jog back to the ring),
        // RECOVER = stuck check every tick (no progress -> new point; long stuck -> blink to the ring).
        const int R590 = 0, S590 = 1, F590 = 2, T590 = 3;
        float ArcRange590() => (NSkillLv561(1) > 0 ? PT.skelArcRange561 : PT.minionRange536) + (NHas536(5) ? PT.marrowShotRange536 : 0);
        int PickT590(NMin536 m)
        {
            float seek = PT.minionSeekR590;
            if (m.tgt >= 0 && m.tgt < maxEnemies && m.tgt < _en.Length) { var c = _en[m.tgt]; if (c.alive && !c.dying && (c.pos - _heroPos).magnitude < seek + 1f) return m.tgt; }
            int t = -1; var hb = _hurtBy53; // PoE: minions answer whoever is hitting the summoner first
            if (hb != null && hb.alive && !hb.dying && hb.id >= 0 && hb.id < maxEnemies && _en[hb.id] == hb && (hb.pos - _heroPos).magnitude < PT.minionThreatR582 * 1.5f) t = hb.id;
            if (t < 0) t = Nearest(_heroPos, PT.minionThreatR582, -1); // then demons in the hero's face
            if (t < 0)
            {   // nearest to ME, but only demons inside the seek radius around the hero; warriors avoid a target already swarmed by 3+
                float bd = float.MaxValue;
                for (int k = 0; k < maxEnemies; k++)
                {
                    var e = _en[k]; if (!e.alive || e.dying || (e.pos - _heroPos).magnitude > seek) continue;
                    float d = (e.pos - m.p).sqrMagnitude; if (m.kind == 0) { int c = 0; foreach (var o in _nmin536) if (o != m && o.kind == 0 && o.tgt == k) c++; if (c >= 3) d += 9f; }
                    if (d < bd) { bd = d; t = k; }
                }
            }
            m.tgt = t; return t;
        }
        bool Move590(NMin536 m, Vector2 goal, float speed, float dt, float tol = 0.15f)
        {
            Vector2 d = goal - m.p; if (d.magnitude <= Mathf.Max(tol, PT.minionMoveDead602)) return false; if (AtkLock602(m) && m.kind == 0) return false; m.p = Vector2.MoveTowards(m.p, goal, speed * dt); m.wantFace = d; m.walkHold = PT.minionWalkHold602; return true;
        }
        Vector2 RingPt590(NMin536 m, int i, float rMin, float rMax)
        {
            float a = i * 2.4f + Random.Range(-0.9f, 0.9f), r = Random.Range(rMin, rMax); Vector2 p = _heroPos + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r; ClampToRoom(ref p, 10); return p;
        }
        // t602u: attack / shoot anim lock (the swing or draw+release must finish before moving / leaving FIGHT)
        bool AtkLock602(NMin536 m) => Time.time - m.atkT < (m.kind == 0 ? PT.warAtkLock602 : PT.arcAtkLock602);
        void SetSt590(NMin536 m, int s) { if (m.st != s) { m.st = s; m.stT = 0; } }
        void Brain590(NMin536 m, int i, float dt, SkillInst sk, float dmg)
        {
            m.stT += dt; float hd = (m.p - _heroPos).magnitude; bool war = m.kind == 0; bool moving = false;
            float spd = PT.skelWarSpeed561, rng = war ? PT.skelWarReach561 : ArcRange590();
            int t = PickT590(m);
            // transitions (every tick, explicit)
            if (t >= 0) { var e0 = _en[t]; float d0 = (e0.pos - m.p).magnitude; float inR = war ? PT.skelWarReach561 + e0.radius * 0.5f : rng * 0.9f;
                if (m.st == F590) { if (d0 > inR + PT.minionFightExit602 && m.stT > PT.minionStateMin602 && !AtkLock602(m)) SetSt590(m, S590); } else if (m.st != S590 || m.stT > 0.1f || d0 <= inR) SetSt590(m, d0 <= inR ? F590 : S590); }
            else if (((m.st == S590 || m.st == F590) && !AtkLock602(m)) || (m.st == R590 && hd > PT.minionReturnR590)) SetSt590(m, T590);
            else if (m.st == T590 && hd <= PT.minionRoamMax590) SetSt590(m, R590);
            // t604u: a target can die while the t602u attack lock holds S/F -> t == -1 here. Never index _en with it.
            if ((m.st == S590 || m.st == F590) && !LiveEn594(t)) { m.tgt = -1; if (AtkLock602(m)) return; SetSt590(m, T590); t = -1; }
            switch (m.st)
            {
                case S590:
                {
                    var e = _en[t]; Vector2 goal = e.pos;
                    if (war) { float a = i * 2.4f, reach = PT.skelWarReach561 + e.radius * 0.5f; Vector2 slot = e.pos + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * reach * 0.8f; goal = (slot - m.p).magnitude < (e.pos - m.p).magnitude ? slot : e.pos; }
                    else { Vector2 to = e.pos - m.p; goal = e.pos - to.normalized * PT.archerStandoff582; }
                    moving = Move590(m, goal, spd * PT.minionChaseMul582, dt, 0.1f); m.wantFace = e.pos - m.p;
                    if (!war && (e.pos - m.p).magnitude <= rng) Shoot590(m, t, dmg, sk, rng); // archers shoot while closing in
                    break;
                }
                case F590:
                {
                    var e = _en[t]; Vector2 to = e.pos - m.p; float d = to.magnitude; m.wantFace = to;
                    if (war) Swing590(m, t, dmg, sk);
                    else
                    {
                        if (d < PT.archerKiteR582) { Vector2 np = m.p - to.normalized * PT.archerKiteSpeed582 * dt; Vector2 c = np; ClampToRoom(ref c, 6); if ((c - np).sqrMagnitude < 0.0001f && (np - _heroPos).magnitude < PT.minionSeekR590) { m.p = np; moving = true; m.walkHold = PT.minionWalkHold602; } else { m.walkHold = PT.minionWalkHold602; Vector2 side = new Vector2(-to.y, to.x).normalized * ((i & 1) == 0 ? 1 : -1); m.p += side * PT.archerKiteSpeed582 * dt; moving = true; } } // kite back, or strafe if a wall/leash is behind
                        Shoot590(m, t, dmg, sk, rng);
                    }
                    break;
                }
                case T590:
                    if (m.roamT <= 0 || (m.roam - _heroPos).magnitude > PT.minionRoamMax590) { m.roam = RingPt590(m, i, PT.minionRoamMin590, PT.minionRoamMax590); m.roamT = 3f; }
                    m.roamT -= dt; moving = Move590(m, m.roam, spd * PT.minionChaseMul582, dt, 0.3f);
                    break;
                default: // ROAM
                    m.roamT -= dt;
                    if (m.roamT <= 0 || (m.roam - _heroPos).magnitude > PT.minionRoamMax590 + 1f || (m.roam - _heroPos).magnitude < PT.minionRoamMin590 - 0.5f) { m.roam = RingPt590(m, i, PT.minionRoamMin590, PT.minionRoamMax590); m.roamT = Random.Range(PT.minionRoamWaitMin590, PT.minionRoamWaitMax590); }
                    moving = Move590(m, m.roam, spd * PT.minionRoamSpeed590, dt, 0.3f);
                    break;
            }
            // RECOVER: wanted to move but made no progress
            if (moving) { if ((m.p - m.stuckP).magnitude > 0.25f) { m.stuckP = m.p; m.stuckT = 0; } else m.stuckT += dt; } else { m.stuckP = m.p; m.stuckT = 0; }
            if (m.stuckT > PT.minionStuckS590) { m.roam = RingPt590(m, i + 3, PT.minionRoamMin590, PT.minionRoamMax590); m.roamT = 2f; if (m.st == S590) { m.tgt = -1; SetSt590(m, T590); } }
            if (m.stuckT > PT.minionStuckBlinkS590) { Vector2 o = m.p; m.p = RingPt590(m, i, PT.minionRoamMin590, PT.minionRoamMax590); m.prev = m.p; m.stuckP = m.p; m.stuckT = 0; FxBlink561(o, m.p); }
        }
        void Swing590(NMin536 m, int t, float dmg, SkillInst sk)
        {
            if (m.atk > 0 || !LiveEn594(t)) return; var e = _en[t]; Vector2 to = e.pos - m.p;
            m.atk = NRate536(0); m.atkT = Time.time; m.atkFace = to; Hit(e, dmg, sk, to.normalized * 0.3f); FxWarHit561(m, e); SinCleave818(e, t, dmg, sk); // t818u #42 // damage stays on attack frame 0 (VH open)
            if (NHas536(4)) { int t2 = Nearest(e.pos, PT.graveEdgeR536, t); if (t2 >= 0) { Hit(_en[t2], dmg, NSk536(0), Vector2.zero); FxEdge561(e, _en[t2]); } } // A5 +1 target
        }
        void Shoot590(NMin536 m, int t, float dmg, SkillInst sk, float rng)
        {
            if (m.atk > 0 || !LiveEn594(t)) return; var e = _en[t];
            m.atk = NRate536(1); m.atkT = Time.time; m.atkFace = e.pos - m.p; Vector2 dir = (e.pos - m.p).normalized;
            // t594u: NO damage here. A real bone arrow is released on shoot f6 and deals damage on impact (FxTick561 / ArrowHit594).
            bool pierce = NHas536(5) && ++m.shots % 3 == 0; // A6 Marrow Shot: every 3rd arrow pierces down the lane to max range
            Vector2 end = pierce ? m.p + dir * rng : e.pos + Vector2.up * e.hitY;
            ArcMulti622(m, t, end, pierce, ref dmg, sk, rng); // t622u Multi Projectiles (extra arrows first; dmg scaled for all)
            FxArcShot561(m, t, end, pierce, dmg, sk);
        }
        // t582u target pick: keep the current target while valid (focus), else demons near the hero (threat), else nearest to the minion inside the leash
        int PickT582(NMin536 m, float r)
        {
            if (m.tgt >= 0 && m.tgt < maxEnemies && m.tgt < _en.Length) { var c = _en[m.tgt]; if (c.alive && !c.dying && (c.pos - _heroPos).magnitude < PT.minionLeash561 + 1f) return m.tgt; }
            int t = Nearest(_heroPos, PT.minionThreatR582, -1);
            if (t < 0) { t = Nearest(m.p, PT.minionAggroR582, -1); if (t >= 0 && (_en[t].pos - _heroPos).magnitude > Mathf.Max(r, PT.minionLeash561)) t = -1; }
            m.tgt = t; return t;
        }
        // t582u spacing: minions push off each other and off the hero so they don't stack
        void Sep582(NMin536 m, int i, float dt)
        {
            Vector2 push = Vector2.zero;
            for (int j = 0; j < _nmin536.Count; j++) { if (j == i) continue; Vector2 d = m.p - _nmin536[j].p; float l = d.magnitude; if (l < PT.minionSepR582) push += (l < 0.01f ? new Vector2(Mathf.Cos(i), Mathf.Sin(i)) : d / l) * (PT.minionSepR582 - l); }
            Vector2 h = m.p - _heroPos; float hl = h.magnitude; if (hl < PT.minionHeroGap582) push += (hl < 0.01f ? Vector2.right : h / hl) * (PT.minionHeroGap582 - hl) * 2f;
            m.p += Vector2.ClampMagnitude(push * PT.minionSepPush582, PT.skelWarSpeed561) * dt;
            // t584u hard body vs hero: never inside the hero's gap (soft push above, hard projection here)
            Vector2 hh = m.p - _heroPos; float hd = hh.magnitude; if (hd < PT.minionHeroHard584) m.p = _heroPos + (hd < 0.01f ? new Vector2(Mathf.Cos(i * 2.4f), Mathf.Sin(i * 2.4f)) : hh / hd) * PT.minionHeroHard584;
            // t584u walls / room-shape pits / blocks: same clamp as demons (slides along the edge)
            ClampToRoom(ref m.p, 6);
        }
        // t584u: demons aggro the nearest minion when it is within enemyMinionAggro584 and closer than the hero
        Vector2 EnemyGoal584(Vector2 ep, float heroDist)
        {
            float best = Mathf.Min(PT.enemyMinionAggro584, heroDist); int bi = -1;
            for (int j = 0; j < _nmin536.Count; j++) { float d = (_nmin536[j].p - ep).magnitude; if (d < best) { best = d; bi = j; } }
            return bi >= 0 ? _nmin536[bi].p : _heroPos;
        }
        void Home536(NMin536 m, int i, float dt) { float a = i * 2.4f + 1.2f; Vector2 home = _heroPos + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (m.kind == 1 ? PT.skelArcRing561 : 1.8f); if ((home - m.p).magnitude > 0.25f) m.p = Vector2.MoveTowards(m.p, home, PT.skelWarSpeed561 * dt); }
        void NecroRoomEnter536() { foreach (var m in _nmin536) m.p = _heroPos + Random.insideUnitCircle; }
    }
}
