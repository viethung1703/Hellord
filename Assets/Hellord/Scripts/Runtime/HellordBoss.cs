using PixelFXPack;
using UnityEngine;

// v0.6: art = Hellord Art's Club Jailer (boss_oxhead_clubjailer + _bloodrage, 72x72, pivot (36,66), ~52px tall), retuned hitboxes/tells
// for the bigger body (proposals from the design doc gameplay check), new HP bar, ox stampede + bone drop-gate art.
// v0.5 TEST BOSS (not final design or art): a placeholder Ox-Head that runs the Ox Pit tells from Pixel FX Pack v1.8.4 (TellFX)
// so the juice can be felt in real gameplay. Spawns once at 60s or on F4. Uses the last horde slot so skills/aim/statuses hit it
// like any enemy; its SpriteRenderer sits on a CHILD of the root (needed for the stun wobble), and it gets a PixelFX (Uber) component
// so punish / bloodrage outlines can draw. While it lives, a test arena (red border) keeps the hero in and gives the Gore Charge a wall.
namespace Hellord
{
    public partial class HellordGame
    {
        enum BS { Off, Walk, ChargeTell, Charge, Stun, Sweep, Stomp, Herd, Recover, LungeTell, Lunge, IceTell, IceBreath, X44 } // v0.0.27: LungeTell/Lunge = telegraphed gap closer
        // v0.6 retune (proposals): hurt circle 24px above the feet r=19px; contact half-width 20px (= 40px Gore lane);
        // horn origin 38px up, reach 60px; north wall margin 52px (body height). Arena = one 640x352 screen (doc's 1536x864 Ox Pit not built yet).
        // v0.7: the arena is the real Ox Pit room (oxpit_room_clip, 1536x864). North margin retuned: in 3/4 view the body may overlap the
        // wall face, so the feet stop 16px below the 48px wall base (was a full 52px body height against the test line).
        const float BossHeadY = 3.6f, BossRadius = 1.2f, BossHurtY = 1.5f, BossContact = 1.25f, HornY = 2.375f, NorthMargin = 1f; static float HornReach => HellordTuning.I.bossHornReachU; // v0.0.27: reach 6.25 -> 7.5 (tuning)
        static float BossHp => HellordTuning.I.bossHp;
        static float BoneGateX => 960 * HellordRoom.BossSX677; static float HerdLaneX => 984 * HellordRoom.BossSX677; const float HerdSpeedPx = 240, HerdStartY = 40; const int HerdLen = 3; // bone drop-gate top-left x (clip room), oxen lane centre
        const float LanePx = 56f; static float HornRangePx => HellordTuning.I.bossHornReachU * 16f; // the cone tell is drawn at the real reach
        bool _sweepRetracked, _sweepFollow; float _sweepTotal, _lungeLeft;
        // v0.0.9: arena start: hero south-centre, boss north-centre (280 px apart, both framed by the boss camera); 1.5 s intro before any attack/contact
        static float BossHeroStartX => 768 * HellordRoom.BossSX677; static float BossHeroStartY => 580 * HellordRoom.BossSY677; static float BossStartX => 768 * HellordRoom.BossSX677; static float BossStartY => 400 * HellordRoom.BossSY677; const float BossIntro = 1.5f; float _bossIntroT, _bossSpawnT; // t677u scaled with the boss room
                int BossSlot => maxEnemies - 1;
        Enemy _boss; Transform _bossRoot; SpriteRenderer _bossBody; SpriteRenderer _gateGlow; LaneTell _lane;
        BS _bs; float _bsT, _squash; int _atk; bool _bossSpawned, _raging, _arenaOn; Vector2 _chargeDir, _herdA, _herdB; float _sweepDeg;
        int _tellH = -1, _speedH = -1, _dustH = -1, _stunH = -1, _punishH = -1, _rageH = -1;
        readonly float[] _ringT = { -1, -1, -1 }; Vector2 _stompC;
        static readonly BS[] Cycle = { BS.ChargeTell, BS.Sweep, BS.Stomp, BS.ChargeTell, BS.Herd };
        Sprite[] _bossF, _bossRageF, _gateF; SpriteRenderer _gate; string _anim = "idle"; float _animT; bool _phase2, _animDone; int _hpFlashF; float _hpFlashT;
        enum Gate { Closed, Shake, Open, Closing } Gate _gs; float _gateT;
        void Play(string a, bool restart = false) { if (_anim == a && !restart) return; _anim = a; _animT = 0; _animDone = false; }

        bool BossAlive => _boss != null && _boss.alive;
        bool BossStunned => BossAlive && _bs == BS.Stun;
        float Rage => (_raging ? 1.4f : 1f) * (IsHorse ? HellordTuning.I.horseTempo : 1f); // v0.0.37: Horse-Face tempo
        float ChargeTell37() => IsHorse ? Mathf.Max(0.3f, HellordTuning.I.horseChargeTellS / Rage) : Mathf.Max(0.5f, 0.85f / Rage);
        float StunS37 => IsHorse ? HellordTuning.I.horseStunS : 3f;
        bool _spearFollow;

        void BuildBossSlot(Transform root)
        {
            _boss = _en[BossSlot]; Destroy(_boss.sr.gameObject);
            _bossRoot = new GameObject("BOSS_OxHead_ClubJailer").transform; _bossRoot.SetParent(root, false);
            _bossF = HArt.Sheet("boss_oxhead_clubjailer"); _bossRageF = HArt.Sheet("boss_oxhead_clubjailer_bloodrage");
            // SpriteRenderer on a CHILD (pack stun wobble); sheet pivot = feet, so the child sits at the root (feet) position
            _bossBody = NewSR("body", _bossRoot, _bossF[0], 0); _bossBody.transform.localPosition = Vector3.zero; HordeFX.Prepare(_bossBody);
            _gateF = HArt.Sheet("gate_bone_dropgate");
            _boss.sr = _bossBody; _boss.tr = _bossRoot; _boss.boss = true;
            _bossRoot.gameObject.SetActive(false);
            var wr = new GameObject("BossRoomProps").transform;
            // Gate B: opaque 48x48 wall section at the clip room's north gate (top-left px (960,0)); sheet pivot = (24,47) bottom-centre
            _gate = NewSR("gate_bone_dropgate", wr, _gateF[HArt.GateClosed], HellordRoom.GateOrder); _gate.gameObject.SetActive(false);
            _lane = new LaneTell(-29000); _lane.Root.SetParent(wr, false);
            TellFX.OnTellFire = OnTellFire;
        }

        void UpdateBossSpawn() { if (!_bossSpawned && runTime >= 60f) SpawnBoss(); }

        void SpawnBoss()
        {
            if (BossAlive) { Debug.Log($"[HB] SpawnBoss skipped: boss already alive hp {_boss.hp:0} pos {_boss.pos}"); Toast("Test boss already alive"); return; }
            _bossSpawned = true; _bossEntryT = 2.0f; var e = _boss;
            // v0.7: move to the Ox Pit boss room; the horde is cleared (the room is the arena; walls block movement)
            foreach (var o in _en) if (o != e && o.alive) { StopStatuses(o); DetachFx(o); o.alive = false; o.dying = false; o.sr.gameObject.SetActive(false); }
            if (_room == null || _room.Name != ClipRoom) { LoadRoom(ClipRoom); _heroPos = HellordRoom.Px(BossHeroStartX, BossHeroStartY); _hero.position = _heroPos; SnapCamera(); } // v0.8: already there when entered through the boss door
            _arenaOn = true; SetWalls(true);
            e.pos = HellordRoom.Px(BossStartX, BossStartY); _bossIntroT = BossIntro; e.type = EType.Elite; e.elite = true; e.boss = true;
            e.maxHp = e.hp = SinBossHpMul818() * BossHp * (1 + HellordTuning.I.bossHpPerMinute * (runTime / 60f)) * (1 + HellordTuning.I.floorBossHpGrowth * FloorIdx); e.speed = HellordTuning.I.bossP1Speed * (1 + 0.04f * FloorIdx); e.radius = BossRadius; e.hitY = BossHurtY; e.dmg = 20; e.xp = 60; e.alive = true; e.dying = false;
            SetBossLook37(); e.frames = _bossF; _phase2 = false; Play("roar", true); HellordAudio.Play("oxhead_roar"); _gs = Gate.Closed;
            e.burnT = e.chillT = e.shockT = e.bleedT = e.curseT = e.freezeT = e.orbitCd = e.auraCd = 0; e.chillHits = 0; e.iceBuild = 0; e.push = Vector2.zero; e.hitT = 0;
            _bossRoot.position = e.pos; _bossRoot.localScale = Vector3.one; _bossBody.transform.localPosition = Vector3.zero; /* t502u */ _bossRoot.gameObject.SetActive(true); _bossBody.gameObject.SetActive(true); _bossBody.enabled = true; _bossBody.color = Color.white;
            HordeFX.Prepare(_bossBody); if (e.fx == null) AttachFx(e);
            _raging = false; _atk = 0; Go(BS.Walk, 1.5f); ClearBBullets(); _bbCd = 2.0f; _bbPat = 0; _spiralA = 0;
            ScreenShake.Add(3, 0.3f); TellFX.DustPuff(e.pos, 7, 14);
            SetupBossArena1006(); // t1006u: smaller lit arena + trash adds
            SnapCamera(); // v0.0.16: frame the boss AFTER it is placed (EnterRoom snapped before the boss existed -> head clipped at the top)
            PostBanner("BOSS: " + BossName37(), BossSub44(), BannerKind.Danger, HellordTuning.I.bannerHold + 0.3f); _bossSpawnT = runTime; Debug.Log($"[HB] SpawnBoss ok room {_room.Name} hero {_heroPos} boss {e.pos} hp {e.hp:0} t {runTime:0.0}");
        }

        void Go(BS s, float t) { if ((_bs == BS.Sweep || _bs == BS.IceTell) && s != _bs) HellordAudio.Stop("oxhead_horn_windup"); _bs = s; _bsT = t; }
        Vector2 BossC => _boss.pos + Vector2.up * HornY;   // horn height (sweep origin)

        float _bossEntryT; // v0.0.18: entry beat - hero safe-area bottom margin yields so the boss head clears the bar
        void UpdateBoss(Enemy e, float dt)
        {
            if (_bossEntryT > 0) _bossEntryT -= dt;
            if (_bossIntroT > 0)
            {   // v0.0.9 intro: roar in place, no movement / attacks / contact damage
                _bossIntroT -= dt; Play("roar");
                _bossRoot.position = new Vector3(Snap(e.pos.x), Snap(e.pos.y), 0); _bossBody.sortingOrder = YSort(e.pos.y);
                AnimateBoss(dt, _heroPos.x < e.pos.x); UpdateGate(dt); return;
            }
            _bsT -= dt; UpdateBossPatterns(dt); UpdateBossAdds1006(dt); // v0.0.17 bullet layer; t1006u trash
            Vector2 to = _heroPos - e.pos; float dist = to.magnitude; Vector2 dir = dist > 0.01f ? to / dist : Vector2.right;
            if (!_raging && e.hp < e.maxHp * 0.25f) StartBloodrage();          // v0.6: 25% = Bloodrage notch on the HP bar / art note
            else if (!_phase2 && e.hp < e.maxHp * HellordTuning.I.bossPhase2At && (_bs == BS.Walk || _bs == BS.Recover)) { _phase2 = true; Play("phase_change", true); HellordAudio.Stop("oxhead_horn_windup"); HellordAudio.Play("oxhead_phase_change"); HellordAudio.DuckMusic(6f, 2.2f); _hpFlashF = 2; _hpFlashT = 0.6f; Go(BS.Recover, 1.0f); ScreenShake.Add(2f, 0.25f); }
            switch (_bs)
            {
                case BS.Walk:
                    e.pos += dir * e.speed * Rage * dt;
                    Play(Mathf.Abs(dir.x) >= Mathf.Abs(dir.y) ? "move_E" : dir.y > 0 ? "move_N" : "move_S");
                    if (_bsT <= 0 && !SchedBusy46) StartCycleAttack37(_cycle37[_atk++ % _cycle37.Length], dist);
                    break;
                case BS.ChargeTell:
                    _squash = Mathf.MoveTowards(_squash, -0.12f, dt); // wind-up crouch
                    if (_bsT <= 0)
                    {
                        Go(BS.Charge, 3f); _squash = 0.15f; Play(GoreSide ? "gore_loop_E" : "gore_loop_S");
                        _speedH = TellFX.SpeedLines(_bossRoot, 0f, 24, LanePx); _dustH = TellFX.DustTrail(_bossRoot, 0f, 5, 0);
                        if (ScreenShake.Main) ScreenShake.Main.Kick(_chargeDir * 2f * ProjectileFX.ShakeScale);
                    }
                    break;
                case BS.IceTell: case BS.IceBreath: UpdateIce37(e, dt); break;
                case BS.X44: UpdateAtk44(e, dt, dist); break;
                case BS.Charge:
                {
                    e.pos += _chargeDir * 15f * Rage * dt;
                    BossBounds(out var lo, out var hi); Vector2 n = Vector2.zero;
                    if (e.pos.x < lo.x) { e.pos.x = lo.x; n.x = 1; } else if (e.pos.x > hi.x) { e.pos.x = hi.x; n.x = -1; }
                    if (e.pos.y < lo.y) { e.pos.y = lo.y; n.y = 1; } else if (e.pos.y > hi.y) { e.pos.y = hi.y; n.y = -1; }
                    if (n != Vector2.zero || _bsT <= 0) WallStun(e, n == Vector2.zero ? -_chargeDir : n.normalized);
                    break;
                }
                case BS.Stun: Play("stun_loop"); if (_bsT <= 0) { StopH(ref _stunH); if (_stunBar) _stunBar.End(); StopH(ref _punishH); Go(BS.Recover, 0.4f); } break;
                case BS.LungeTell:
                    _squash = Mathf.MoveTowards(_squash, -0.12f, dt); // crouch; the red lane on the floor is the locked line of the dash
                    if (_bsT <= 0)
                    {
                        Go(BS.Lunge, 2f); HellordAudio.Play("oxhead_lunge_dash"); _squash = 0.15f; Play(GoreSide ? "gore_loop_E" : "gore_loop_S");
                        _speedH = TellFX.SpeedLines(_bossRoot, 0f, 24, LanePx); _dustH = TellFX.DustTrail(_bossRoot, 0f, 5, 0);
                        if (ScreenShake.Main) ScreenShake.Main.Kick(_chargeDir * 1.5f * ProjectileFX.ShakeScale);
                    }
                    break;
                case BS.Lunge:
                {   // v0.0.27: short telegraphed dash toward where the hero stood, then a follow-up horn sweep
                    var TL = HellordTuning.I; float step = Mathf.Min(_lungeLeft, (_spear ? TL.horseSpearSpeed : TL.bossLungeSpeed) * Rage * dt); e.pos += _chargeDir * step; _lungeLeft -= step;
                    BossBounds(out var llo, out var lhi); bool wall = false;
                    if (e.pos.x < llo.x) { e.pos.x = llo.x; wall = true; } else if (e.pos.x > lhi.x) { e.pos.x = lhi.x; wall = true; }
                    if (e.pos.y < llo.y) { e.pos.y = llo.y; wall = true; } else if (e.pos.y > lhi.y) { e.pos.y = lhi.y; wall = true; }
                    if (dist < (_spear ? TL.horseSpearHalfWidthU + 0.3f : TL.bossLungeHalfWidthU)) HurtHero(e.dmg * (_spear ? TL.horseSpearDmgMul : TL.bossLungeDmgMul), "boss_lunge", true);
                    if (_lungeLeft <= 0.01f || wall || _bsT <= 0)
                    {
                        StopH(ref _speedH); StopH(ref _dustH); _squash = -0.1f; TellFX.DustPuff(e.pos, 5, 10, Mathf.Atan2(_chargeDir.y, _chargeDir.x) * Mathf.Rad2Deg);
                        if (_spear) SpearEnd37(); else BeginSweep(true, TL.bossFollowupTell); // always ends in a swing (visible tell, then the strike)
                    }
                    break;
                }
                case BS.Sweep:
                {
                    var TS = HellordTuning.I;
                    if (_bsT > 0)
                    {   // v0.0.27: the boss creeps toward the hero during the wind-up and, halfway, re-aims the cone ONCE (a second visible tell)
                        if (dist > 1.8f) e.pos += dir * TS.bossSweepCreepSpeed * Rage * dt;
                        if (TS.bossSweepRetrack && !_sweepRetracked && !_sweepFollow && _bsT <= _sweepTotal * 0.5f)
                        {
                            _sweepRetracked = true; StopH(ref _tellH); StopTimed46(); AimSweep(); float tl = Mathf.Max(TS.bossRetrackTellMin, _bsT);
                            if (HellordTuning.I.crispTellsV46) TimedCone46(BossC, _sweepDeg, 65f, HornRangePx / 16f, tl); else _tellH = TellFX.ConeTelegraph(BossC, _sweepDeg, 130, HornRangePx, tl, _bossRoot, null, HellordTuning.I.bossWarnOrder948); _bsT = tl; Play("horn_windup", true); HellordAudio.Stop("oxhead_horn_windup"); HellordAudio.Play("oxhead_horn_windup");
                        }
                    }
                    if (_bsT <= 0)
                    {
                        Vector2 d = _heroPos + Vector2.up * 0.5f - BossC; float a = Vector2.Angle(new Vector2(Mathf.Cos(_sweepDeg * Mathf.Deg2Rad), Mathf.Sin(_sweepDeg * Mathf.Deg2Rad)), d);
                        bool inCone = d.magnitude < HornReach && a < 65f;
                        if (inCone || HorseSlamHit53(_bossBody && _bossBody.flipX)) HurtHero(18, "boss_horn", true); /* t501u: + Horse-Face pestle impact circle */ Play("horn_strike", true); HellordAudio.Stop("oxhead_horn_windup"); HellordAudio.Play("oxhead_horn_strike");
                        Vector2 f = new Vector2(Mathf.Cos(_sweepDeg * Mathf.Deg2Rad), Mathf.Sin(_sweepDeg * Mathf.Deg2Rad));
                        if (ScreenShake.Main) ScreenShake.Main.Kick(f * 2f * ProjectileFX.ShakeScale);
                        TellFX.DustPuff(BossC + f * HornReach * 0.6f, 5, 10, _sweepDeg); _squash = 0.12f;
                        // v0.0.27: sidestepped it and still close = a quick second swing (one follow-up per sweep)
                        if (!inCone && !_sweepFollow && dist < TS.bossFollowupRangeU && Random.value < TS.bossFollowupChance) BeginSweep(true, TS.bossFollowupTell);
                        else Go(BS.Recover, 0.6f);
                    }
                    break;
                }
                case BS.Stomp:
                    Stomp984Tick(dt);
                    for (int i = 0; i < 3; i++)
                    {
                        if (_ringT[i] < 0) continue; _ringT[i] += dt; float r = 7f * Mathf.Clamp01(_ringT[i] / 0.55f); // v0.0.17: 112px rim in 0.55s (was 64px)
                        Vector2 d = _heroPos - _stompC; float de = new Vector2(d.x, d.y / 0.6f).magnitude;
                        if (Mathf.Abs(de - r) < 0.6f && !(HorseFace53 && HellordTuning.I.horseNoRings53)) HurtHero(12, "boss_stomp", true);
                        if (i == 0 && !_stompWarned && OldStomp46 && _ringT[0] >= 0.55f - BBulletTell) BBStompWarn(); // v0.0.21: PatternWarnFX lead before the stomp burst
                        if (_ringT[i] > 0.55f) { _ringT[i] = -1; if (i == 0 && OldStomp46 && !(HorseFace53 && HellordTuning.I.horseNoRings53)) BBStompBurst(); }
                    }
                    if (_bsT <= 0) Go(BS.Recover, 0.5f);
                    break;
                case BS.Herd:
                    if (_bsT < 0 && _gs == Gate.Shake) { _gs = Gate.Open; _gateT = 0; }
                {
                    // v0.7: oxen burst out of the bone gate and run south down the lane (3 oxen, 0.18 s apart, 240 px/s)
                    float run = -_bsT, len = (_herdA.y - _herdB.y) * 16f;
                    if (run > 0)
                    {
                        float hx = _heroPos.x * 16f, hy = -_heroPos.y * 16f;
                        for (int i = 0; i < HerdLen; i++)
                        {
                            float oy = HerdStartY + (run - i * 0.18f) * HerdSpeedPx; if (oy < HerdStartY) continue;
                            if (Mathf.Abs(hx - HerdLaneX) < 19f && Mathf.Abs(hy - oy) < 14f) { HurtHero(16, "boss_herd", true); break; } // ox 30 wide, ~28 tall
                        }
                    }
                    if (_gateGlow) _gateGlow.enabled = _gs == Gate.Open;
                    if (run * HerdSpeedPx > len + (HerdLen - 1) * 0.18f * HerdSpeedPx + 32) { Go(BS.Recover, 0.3f); _gs = Gate.Closing; _gateT = 0; if (_gateGlow) _gateGlow.enabled = false; }
                    break;
                }
                case BS.Recover: if (_animDone || BossAnimLoops) Play("idle"); if (_bsT <= 0) Go(BS.Walk, Random.Range(HellordTuning.I.bossWalkRestMin, HellordTuning.I.bossWalkRestMax) / Rage); break;
            }
            if (_bs != BS.Stun && _bs != BS.Lunge && !_air44 && dist < (_bs == BS.Charge ? LanePx / 32f : BossContact)) HurtHero(e.dmg, _bs == BS.Charge ? "boss_charge" : "boss_contact", true); // v0.0.17: charge hits the full 56px lane // contact half-width 20px = Gore lane 40px
            // squash-stretch on the root (game side, per HELLORD_FX_MAP) - the SR child stays free for the pack's stun wobble
            _squash = Mathf.MoveTowards(_squash, 0, dt * 0.8f);
            _bossRoot.localScale = new Vector3(1 - _squash, 1 + _squash, 1);
            _bossRoot.position = new Vector3(Snap(e.pos.x), Snap(e.pos.y), 0);
            _bossBody.sortingOrder = YSort(e.pos.y);
            AnimateBoss(dt, _bs == BS.Charge || _bs == BS.ChargeTell || _bs == BS.Lunge || _bs == BS.LungeTell ? _chargeDir.x < 0 : to.x < 0); UpdateGate(dt);
        }

        void AimSweep() { Vector2 to = _heroPos - _boss.pos; _sweepDeg = Mathf.Atan2(_heroPos.y + 0.5f - BossC.y, to.x) * Mathf.Rad2Deg; } // aimed from the horns
        void BeginSweep(bool follow, float tell)
        {
            _sweepFollow = follow; _sweepRetracked = follow; AimSweep();
            float tl = follow ? Mathf.Max(0.4f, tell / Rage) : Mathf.Max(0.5f, tell / Rage); _sweepTotal = tl;
            StopH(ref _tellH); StopTimed46(); if (HellordTuning.I.crispTellsV46) TimedCone46(BossC, _sweepDeg, 65f, HornRangePx / 16f, tl); else _tellH = TellFX.ConeTelegraph(BossC, _sweepDeg, 130, HornRangePx, tl, _bossRoot, null, HellordTuning.I.bossWarnOrder948); Play("horn_windup", true); HellordAudio.Stop("oxhead_horn_windup"); HellordAudio.Play("oxhead_horn_windup");
            Go(BS.Sweep, tl);
        }
        /// <summary>v0.0.27: a sweep is swapped for a lunge when the hero is out of the horn reach (always) or far-ish (chance), so a kiter gets caught up with.</summary>
        BS PickAttack(BS s, float dist)
        {
            var T = HellordTuning.I; if (s != BS.Sweep) return s;
            if (dist > HornReach * 0.85f) return BS.LungeTell;
            return dist > T.bossLungeMinDistU && Random.value < T.bossLungeChance ? BS.LungeTell : s;
        }

        void StartAttack(BS s)
        {
            var e = _boss; Vector2 to = _heroPos - e.pos; Vector2 dir = to.sqrMagnitude > 0.01f ? to.normalized : Vector2.right;
            switch (s)
            {
                case BS.ChargeTell:
                {
                    _chargeDir = dir; Vector2 end = e.pos + dir * RayToWall(e.pos, dir);
                    if (HellordTuning.I.crispTellsV46) TimedLane46(e.pos, end, LanePx, ChargeTell37()); else _lane.Begin(e.pos + Vector2.up * 0.5f, end + Vector2.up * 0.5f, LanePx, ChargeTell37()); // v0.7: outline-style tell (HellordLaneTell.cs)
                    Play(GoreSide ? "gore_windup_E" : "gore_windup_S", true);
                    Go(BS.ChargeTell, ChargeTell37()); break;
                }
                case BS.Sweep: BeginSweep(false, 0.75f); break;
                case BS.LungeTell:
                {   // v0.0.27: locked red lane from the boss to just past the hero's spot (max lungeMaxU); a sidestep dodges it
                    var TL = HellordTuning.I; float len = Mathf.Min(TL.bossLungeMaxU, Mathf.Max(2f, to.magnitude + TL.bossLungeOvershootU)); _chargeDir = dir; _lungeLeft = len;
                    float tl = Mathf.Max(0.4f, TL.bossLungeTell / Rage), lw = TL.bossLungeHalfWidthU;
                    if (_spear) { len = Mathf.Min(TL.horseSpearLenU, RayToWall(e.pos, dir)); _lungeLeft = len; lw = TL.horseSpearHalfWidthU; tl = Mathf.Max(0.25f, (_spearFollow ? TL.horseComboTellS : TL.horseSpearTellS) / Rage); } // v0.0.37 Horse-Face spear thrust
                    if (HellordTuning.I.crispTellsV46) TimedLane46(e.pos, e.pos + dir * len, lw * 2f * 16f, tl); else _lane.Begin(e.pos + Vector2.up * 0.5f, e.pos + dir * len + Vector2.up * 0.5f, lw * 2f * 16f, tl);
                    Play(GoreSide ? "gore_windup_E" : "gore_windup_S", true); Go(BS.LungeTell, tl); break;
                }
                case BS.Stomp:
                    _stompC = e.pos; for (int i = 0; i < 3; i++) _ringT[i] = -1; _squash = -0.15f; _st984 = false;
                    if (HorseFace53 && HellordTuning.I.horseNoRings53 && HellordTuning.I.horseNoFootRings957) { _tellH = -1; float tw = 0.8f; After(tw, () => { if (_bs == BS.Stomp && _bossSpawned) { _squash = 0.12f; Play("stomp_strike", true); HellordAudio.Play("oxhead_stomp_strike"); HorseThrowSack53(_bossBody && _bossBody.flipX); } }); } /* t957u: no 20 px foot rings during the windup, sack throw kept on the same 0.8 s beat */
                    else if (Stomp984On && !HorseFace53) { _tellH = -1; Stomp984Begin(); } /* t974u: PixelBossWarn windup + wave bands, same strike beat */
                    else _tellH = TellFX.StompRings(e.pos, 3, HorseFace53 && HellordTuning.I.horseNoRings53 ? 20 : 112, 0.8f, 0.3f, 0.6f, null, HellordTuning.I.bossWarnOrder948) /* t948u under the boss */; Play("stomp_windup", true); /* t502u: Horse-Face throws a sack; rings shrink to a foot cue (still time the throw) */ HellordAudio.Play("oxhead_stomp_windup");
                    Go(BS.Stomp, 0.8f + 0.6f + 0.5f); break;
                case BS.Herd:
                {
                    // v0.7: out of the bone gate, straight south through the lane to the south wall
                    _herdA = HellordRoom.Px(HerdLaneX, HerdStartY); _herdB = HellordRoom.Px(HerdLaneX, _room != null ? _room.D.h : 864);
                    _tellH = TellFX.Stampede(_herdA, _herdB, 30, 1f, HerdLen, (int)HerdSpeedPx, HArt.Sheet("hazard_ox_run_south"));
                    Play("roar", true); _gs = Gate.Shake; _gateT = 0;
                    _squash = 0.1f; ScreenShake.Add(1.5f, 0.3f); Toast("Call the Herd!");
                    Go(BS.Herd, 1f); break;
                }
            }
        }

        void OnTellFire(int handle, int index)
        {
            if (handle != _tellH || _bs != BS.Stomp) return;
            StompStrike(index);
        }
        void StompStrike(int index)
        {
            if (index >= 0 && index < 3) { _ringT[index] = 0; _squash = 0.12f; if (index == 0) { Play("stomp_strike", true); HellordAudio.Play("oxhead_stomp_strike"); HorseThrowSack53(_bossBody && _bossBody.flipX); /* t501u */ } }
        }

        void WallStun(Enemy e, Vector2 normal)
        {
            StopH(ref _speedH); StopH(ref _dustH);
            // v0.6: north wall hit lands at the head (52px up); side/south walls at the body centre edge
            Vector2 contact = normal.y < -0.5f ? new Vector2(e.pos.x, HellordRoom.Px(0, _room != null ? _room.D.wallN : 48).y) : e.pos + Vector2.up * BossHurtY - normal * BossRadius;
            TellFX.WallImpact(contact, normal, _bossRoot, 1.2f);
            StunBar().BeginWithStars(StunS37, _bossRoot); // v0.0.21: StunBarFX #114 on the HP bar + its in-world StunStars (replaces TellFX.StunStars + blinking frame)
            _punishH = TellFX.PunishGlow(_bossRoot, StunS37, 26, 24); Play("stun_loop", true);
            _squash = Mathf.Abs(normal.x) > 0.5f ? 0.18f : -0.18f;
            Go(BS.Stun, StunS37); Toast("STUNNED - hit it! (2x damage)");
        }

        void StartBloodrage()
        {
            _raging = true; HellordAudio.Play("oxhead_roar"); _boss.frames = _bossRageF; _hpFlashF = 3; _hpFlashT = 0.6f; Play("roar", true); _rageH = TellFX.BloodrageTrail(_bossRoot, 0f, 7, 1.2f, 0);
            ScreenShake.Add(3.5f, 0.35f); _squash = 0.2f; _rageBannerT = 3f; // v0.0.15: world-anchored over the boss (was a centre-screen Toast = read as over the hero)
        }

        float RayToWall(Vector2 p, Vector2 d)
        {
            BossBounds(out var lo, out var hi); float t = 99;
            if (d.x > 1e-4f) t = Mathf.Min(t, (hi.x - p.x) / d.x); else if (d.x < -1e-4f) t = Mathf.Min(t, (lo.x - p.x) / d.x);
            if (d.y > 1e-4f) t = Mathf.Min(t, (hi.y - p.y) / d.y); else if (d.y < -1e-4f) t = Mathf.Min(t, (lo.y - p.y) / d.y);
            return Mathf.Max(0.5f, t);
        }

        void ClampToArena(ref Vector2 p) { if (!_arenaOn) return; if (_bossArenaOn1006) { p.x = Mathf.Clamp(p.x, _bossArenaLo1006.x, _bossArenaHi1006.x); p.y = Mathf.Clamp(p.y, _bossArenaLo1006.y, _bossArenaHi1006.y); } } // t1006u clamp; else room walls
        /// <summary>v0.7: shows the bone drop-gate (and its open glow) on the clip room's north gate; no test walls any more.</summary>
        void SetWalls(bool on)
        {
            if (!_gate) return;
            if (!on) { if (_boss == null || !_boss.dying) _gate.gameObject.SetActive(false); if (_gateGlow) _gateGlow.enabled = false; return; }
            _gate.gameObject.SetActive(true); _gate.sprite = _gateF[0];
            _gate.transform.position = HellordRoom.Px(BoneGateX + 24, 47); // pivot (24,47) -> top-left lands on (960,0)
            if (_room != null && (_gateGlow == null)) { _gateGlow = _room.AddLight(new RoomLight { s = "oxpit_gate_open_glow", x = (int)BoneGateX - 16, y = 0, mul = 0, k = 0.9f }, HellordRoom.LightOrder + 200); }
            if (_gateGlow) _gateGlow.enabled = false;
        }

        // Per-hit juice (v1.8.1+): BossHit sparks/silhouette on elites and the boss; screen kick + short hit-stop on big hits.
        float _bigHitT;
        void HitJuice(Enemy e, float dmg, bool crit)
        {
            Vector2 d = e.pos - _heroPos; d = d.sqrMagnitude > 1e-4f ? d.normalized : Vector2.right;
            if (e.elite) TellFX.BossHit(e.tr, d, e.pos + Vector2.up * e.hitY - d * e.radius * 0.7f, crit);
            if (e.boss && _hpFlashT <= 0) { _hpFlashT = 0.08f; _hpFlashF = 1; } // HP bar "hit" state frame
            if ((dmg >= 60f || (crit && dmg > 40f)) && Time.unscaledTime > _bigHitT)
            {
                _bigHitT = Time.unscaledTime + 0.15f;
                if (ScreenShake.Main) ScreenShake.Main.Kick(d * 1.5f * ProjectileFX.ShakeScale);
                ScreenShake.Add(1.5f, 0.1f);
                if (TellFX.JuiceHitStop) HitStop.Play(0.035f * ProjectileFX.HitStopScale);
            }
        }

        void OnBossKilled(Enemy e)
        {
            Debug.Log($"[HB] boss killed {runTime - _bossSpawnT:0.0}s after spawn"); StopBossFx(); ClearBossArena1006(); _arenaOn = false; SetWalls(false); FloorBossKilled();
            e.dying = true; e.deathT = 0; Play("death", true); // plays the sheet death (10 frames), then hides the root
        }
        void StopBossFx() { ClearV44(); Clear984(); HellordAudio.Stop("oxhead_horn_windup"); if (_lane != null) _lane.Stop(); StopTimed46(); StopH(ref _tellH); StopH(ref _speedH); StopH(ref _dustH); StopH(ref _stunH); StopH(ref _punishH); StopH(ref _rageH); if (_stunBar) _stunBar.End(true); _bs = BS.Off; }
        PixelFXPack.StunBarFX _stunBar;
        PixelFXPack.StunBarFX StunBar() { if (_stunBar == null) { _stunBar = new GameObject("BossStunBarFX").AddComponent<PixelFXPack.StunBarFX>(); _stunBar.createSpriteRenderer = false; _stunBar.barWidthPx = HArt.HpBarFill; _stunBar.barHeightPx = 10; _stunBar.bossHeadYPx = 56; } return _stunBar; }
        static void StopH(ref int h) { if (h >= 0) TellFX.Stop(h); h = -1; }
        void ResetBoss()
        {
            StopBossFx(); ClearBossArena1006(); ClearSacks53(); _bossSpawned = false; _raging = false; _arenaOn = false; SetWalls(false);
            if (_bossRoot) { _bossRoot.gameObject.SetActive(false); _bossRoot.localScale = Vector3.one; }
            if (_boss != null) { _boss.dying = false; _boss.frames = _bossF; } _phase2 = false;
        }

        // ---- v0.6 helpers ----
        bool GoreSide => Mathf.Abs(_chargeDir.x) >= Mathf.Abs(_chargeDir.y) * 0.7f; // E/W gore art; mostly-vertical charges use gore_S
        bool BossAnimLoops => HArt.BossAnims[_anim].loop;
        void BossBounds(out Vector2 lo, out Vector2 hi)
        {   // feet bounds for the 52px body: side/south margin = body radius, north margin = full height (no clipping into the north wall)
            // v0.7: room floor; sides/south = body radius, north = wall base + 16px (NorthMargin)
            if (_room == null) { lo = _boss.pos - Vector2.one * 10; hi = _boss.pos + Vector2.one * 10; return; }
            if (_bossArenaOn1006) { float m = BossRadius; lo = _bossArenaLo1006 + Vector2.one * m; hi = _bossArenaHi1006 - Vector2.one * m; if (lo.x > hi.x) { var c = (_bossArenaLo1006.x + _bossArenaHi1006.x) * 0.5f; lo.x = hi.x = c; } if (lo.y > hi.y) { var c = (_bossArenaLo1006.y + _bossArenaHi1006.y) * 0.5f; lo.y = hi.y = c; } return; }
            _room.Walkable(BossRadius * 16f, out lo, out hi); hi.y = Mathf.Min(hi.y, HellordRoom.Px(0, _room.D.wallN).y - NorthMargin);
        }
        void AnimateBoss(float dt, bool faceWest)
        {
            UpdateSacks53(dt); // t501u
            _animT += dt; var a = HArt.BossAnims[_anim]; int f = HArt.BossFrame(a, _animT, out _animDone);
            var frames = _raging ? _bossRageF : _bossF; _bossBody.sprite = frames[f];
            _bossBody.flipX = faceWest && (_anim.EndsWith("_E") || _anim.StartsWith("horn") || _anim.StartsWith("stomp") || _anim == "idle" || _anim == "roar" || _anim == "hurt" || _anim == "stun_loop" || _anim == "phase_change" || _anim == "death");
            AnimateGorilla44(dt, faceWest); AnimateQilin44(dt, faceWest); AnimateHorse53(dt, faceWest); /* t502u */ // v0.0.44: PixelLab Gorilla / Qilin sheets override the recoloured Ox-Head frame
            if (_hpFlashT > 0) _hpFlashT -= dt;
        }
        void UpdateBossDeath(Enemy e, float dt)
        {
            AnimateBoss(dt, _bossBody.flipX); UpdateGate(dt);
            if (_animDone && Death44Done()) { e.dying = false; if (_bossRoot) _bossRoot.gameObject.SetActive(false); if (_gate) _gate.gameObject.SetActive(false); }
        }
        void UpdateGate(float dt)
        {
            if (!_gate || !_gate.gameObject.activeSelf) return; _gateT += dt; int f = HArt.GateClosed;
            switch (_gs)
            {
                case Gate.Shake: f = HArt.GateShake0 + (int)(_gateT * 14) % 2; break;
                case Gate.Open: f = HArt.GateOpen0 + Mathf.Min(HArt.GateOpenN - 1, (int)(_gateT * 12)); break;
                case Gate.Closing: { int[] seq = { 5, 4, 3, 0 }; int k = (int)(_gateT * 16); f = seq[Mathf.Min(3, k)]; if (k >= 4) _gs = Gate.Closed; break; }
            }
            _gate.sprite = _gateF[f];
        }

        static Texture2D _hpTex; // v0.7: boss bar drawn by DrawBossHudB (HellordHud.cs) on the 640x360 canvas
    }
}
