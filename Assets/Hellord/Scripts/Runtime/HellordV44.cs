using PixelFXPack;
using System.Collections.Generic;
using UnityEngine;

// v0.0.44: Frost Caster + Fire Wheel demons, hero Chill stacks / Freeze, Ice Gorilla + Qilin Chariot bosses.
// PLACEHOLDER visuals only: recoloured existing sprites (V37Recolor) and generated flat decals (Z37). All numbers: HellordTuning "v0.0.44".
namespace Hellord
{
    public partial class HellordGame
    {
        static HellordTuning T44 => HellordTuning.I;

        // ================= hero Chill stacks / Freeze =================
        int _chill44; float _chillDecay44, _frozen44, _freezeImm44; bool _tmFreeze44;
        int _chillH44 = -1; // v0.0.44 StatusHazardFX chill handle
        public bool HeroFrozen44 => _frozen44 > 0 || _tmFreeze44;
        float HeroChillMul44() => HeroFrozen44 ? 0f : Mathf.Max(0.1f, 1f - T44.heroChillSlowPerStack * _chill44);
        void AddChill44(int n)
        {
            if (n <= 0 || HeroFrozen44 || _freezeImm44 > 0 || _tmGod) return;
            _chill44 += n; _chillDecay44 = T44.heroChillDecayS; HellordAudio.Play("frost_cleave", 1.3f);
            if (_chill44 >= Mathf.Max(1, T44.heroChillFreezeAt))
            {
                _chill44 = 0; _frozen44 = T44.heroFrozenS; HellordAudio.Play("frost_cleave", 0.7f); ScreenShake.Add(1.5f, 0.15f); Toast("FROZEN!");
            }
        }
        void UpdateChill44(float dt)
        {
            if (_frozen44 > 0) { _frozen44 -= dt; if (_frozen44 <= 0) _freezeImm44 = T44.heroFreezeImmuneS; }
            else if (_freezeImm44 > 0) _freezeImm44 -= dt;
            if (_chill44 > 0) { _chillDecay44 -= dt; if (_chillDecay44 <= 0) _chill44 = 0; }
            if (!_hero) return; EnsureFX44();
            // v0.0.44 FX: StatusHazardFX draws the Chill pips / freeze shell / immunity (replaces the opaque freeze disc + hero-cyan pips)
            if (_chillH44 < 0) _chillH44 = StatusHazardFX.ChillBegin();
            if (_chillH44 >= 0 && Chill47On) StatusHazardFX.ChillSet(_chillH44, _heroPos, 0, Mathf.Max(1, T44.heroChillFreezeAt), 0f, T44.chill36KeepPackImmune ? _freezeImm44 : 0f);
            else if (_chillH44 >= 0) StatusHazardFX.ChillSet(_chillH44, _heroPos, _chill44, Mathf.Max(1, T44.heroChillFreezeAt), HeroFrozen44 ? Mathf.Max(_frozen44, 0.01f) : 0f, _freezeImm44);
        }

        // ================= shared hazards (icicles, fireballs, shots, trails) =================
        class Drop44 { public Vector2 p; public float delay, tell, t, r, dmg, stay; public int kind, st; public bool boss; public int fx = -1; }
        class Shot44 { public Vector2 p, v; public float life, r, dmg; public int kind, bounces; public bool boss; public int fx = -1, fx2 = -1, idx; } // v0.0.44: fx = FX handle, fx2 = wheel trail, idx = boulder shard index
        class Trail44 { public Vector2 p; public float life, life0, r, dmg; public int kind; }
        readonly List<Drop44> _drops44 = new List<Drop44>(); readonly List<Shot44> _shots44 = new List<Shot44>(); readonly List<Trail44> _trails44 = new List<Trail44>();
        float _trailHitCd44;

        void ResetV44()
        {
            _chill44 = 0; _frozen44 = _freezeImm44 = 0; _tmFreeze44 = false; _bossKind44 = -1; ClearV44();
            if (_chillH44 >= 0) StatusHazardFX.ChillSet(_chillH44, _heroPos, 0, 3, 0, 0);
        }
        void ClearV44()
        {
            Clear49();
            _drops44.Clear(); _shots44.Clear(); _trails44.Clear(); ClearFireWalls948(); _air44 = false; if (_bossBody) _bossBody.transform.localPosition = Vector3.zero;
            _fxA44 = _fxTr44 = -1; _gTail44 = 0; _pools44.Clear();
            if (_fx44Setup) { IceGorillaFX.Clear(); QilinChariotFX.Clear(); MinionFX.Clear(); HeroSkillFX.Clear(); } SkillLightFX.Clear(); _orbLg667.Clear(); // t667u
            foreach (var e in _en) if (e != null) { e.v44Fx = -1; e.v44Tr = -1; }
            foreach (var pr in _pr) if (pr != null && pr.fx44 >= 0) { pr.fx44 = -1; if (pr.alive && pr.sr) pr.sr.enabled = true; }
        }

        class Pool44 { public Vector2 at; public float r, life, tick, dmg, slow; public SkillInst s; public int fx = -1, sfx928 = -1; public bool anchor53; }
        readonly List<Pool44> _pools44 = new List<Pool44>();
        void UpdatePools44(float dt)
        {
            var T = HellordTuning.I;
            for (int i = _pools44.Count - 1; i >= 0; i--)
            {
                var q = _pools44[i]; q.life -= dt; q.tick -= dt; bool live53 = PactPoolMove53(q, dt); // v0.0.53 Grave Anchor follows, hits only with you inside
                if (q.tick <= 0 && !live53) q.tick += 0.1f;
                else if (q.tick <= 0)
                {
                    q.tick += Mathf.Max(0.05f, T.graveTickSeconds) * SlothCd53; var l = _zoneQ; Query(q.at, q.r, l); SkillArea53(q.at, q.r, q.dmg, (int)q.s.def.id);
                    foreach (int k in l) { var e = _en[k]; if (!e.alive || e.boss) continue; e.slowT = T.graveSlowRefresh; e.slowAmt = q.slow; Hit(e, q.dmg, q.s, Vector2.zero, true); if (SinOn818(7) && q.s != null && q.s.def.id == SkillId.GravePool) ApplyStatus(e, Status.Chill, q.dmg, T.sin7ChillK818, q.s); HeroSkillFX.PoolSlowed(e.pos); } // t818u #7
                }
                if (q.sfx928 >= 0) ShaderFxMove928(q.sfx928, q.at); // t928u follows Grave Anchor
                if (q.life <= 0) { ShaderFxEnd928(q.sfx928); _pools44.RemoveAt(i); }
            }
        }

        // ================= v0.0.44 Pixel FX Pack setup =================
        bool _fx44Setup; float _emb44;
        bool FX44 => T44.fxV44Pack;
        // v0.0.46: tell-only pack glows (slam, roar, wheel lanes, drift ring, charge lane) give way to the crisp Z decals
        bool FXT44 => FX44 && !T44.crispTellsV46;
        float QS46 => IsQilin ? Mathf.Max(0.1f, T44.qilinScale46) : 1f;
        // v0.0.46: Qilin charge lane half width = its contact hit radius, both scaled with the body
        float QContact46 => BossContact * QS46 + 0.3f;
        /// <summary>v0.0.46: the breath cone's origin on the FLOOR under the mouth (hit test and tell both use it).</summary>
        Vector2 QBreathGround46(Vector2 feet, Vector2 dir) { Vector2 m = QMouth44(feet, dir) - feet; return feet + new Vector2(m.x, m.y * T44.qBreathGroundYMul46); }
        void EnsureFX44()
        {
            if (_fx44Setup) return; _fx44Setup = true;
            var tmp = NewSR("matprobe44", null, null, 0); var mat = tmp.sharedMaterial; Destroy(tmp.gameObject);
            // each runner writes its own atlas into mainTexture, so every module needs its own material copy (same as EnsureFX26)
            IceGorillaFX.MaterialOverride = new Material(mat); QilinChariotFX.MaterialOverride = new Material(mat); MinionFX.MaterialOverride = new Material(mat);
            StatusHazardFX.MaterialOverride = new Material(mat); HeroSkillFX.MaterialOverride = new Material(mat);
            IceGorillaFX.Prewarm(); QilinChariotFX.Prewarm(); MinionFX.Prewarm(); StatusHazardFX.Prewarm(); HeroSkillFX.Prewarm();
            
        }
        /// <summary>Every frame: pause the pack runners off Play (level-up cards, menus) and push the tuning timings they mirror.</summary>
        void FxTick44()
        {
            EnsureFX44(); bool pause = mode != Mode.Play; var T = T44;
            IceGorillaFX.Paused = pause; QilinChariotFX.Paused = pause; MinionFX.Paused = pause; StatusHazardFX.Paused = pause; HeroSkillFX.Paused = pause;
            StatusHazardFX.LavaActiveS = T.lavaActiveS; HeroSkillFX.ChainBoltS = T.chainBoltSeconds; HeroSkillFX.ChainHopS = T.chainHopDelay;
            HeroSkillFX.PillarTellS = T.pillarTellSeconds; HeroSkillFX.SpearV2 = T.fxSpearV2_934; HeroSkillFX.SpearHL = T.fxSpearHL934; HeroSkillFX.SpearHW = T.fxSpearHW934; HeroSkillFX.SpearWideAt = T.fxSpearWideAt934; HeroSkillFX.PoolTickS = T.graveTickSeconds; HeroSkillFX.SetHero(_heroPos); HeroSkillFX.SetWhipDark(T.fxWhipDark948); HeroSkillFX.SetBloodRed(T.fxBloodRed934); HeroSkillFX.SetWhipColors(T.whipColDark957, T.whipColMid957, T.whipColHi957); HeroSkillFX.SetWhipRoles(T.whipRoles965, T.whipOutline965, T.whipBody965, T.whipUnder965, T.whipEdge965, T.whipGlint965, T.whipFlashCore965, T.whipFlashRim965, T.whipMoteCore965, T.whipMoteTrail965); // t934u, t957u
            HeroSkillFX.ClassicPillar = T.fxClassicPillar46; HeroSkillFX.ClassicWhip = T.fxClassicWhip46; StatusHazardFX.ClassicIce = T.fxClassicIce46; StatusHazardFX.ClassicLava = T.fxClassicLava46;
            StatusHazardFX.LavaActiveWidthMul = T.lavaActiveWidthMul46; StatusHazardFX.LavaGlowAlphaMul = T.lavaGlowAlphaMul46;
        }
        void EndMinionFx44(Enemy e)
        {
            if (e.v44Tr >= 0) { MinionFX.WheelTrailEnd(e.v44Tr); e.v44Tr = -1; }
            if (e.v44Fx >= 0) { MinionFX.Cancel(e.v44Fx); e.v44Fx = -1; }
        }
        void EndShot44(Shot44 s, bool hit)
        {
            if (s.kind == 0) { if (s.fx >= 0) { if (hit) MinionFX.FrostBoltHit(s.fx, s.p + Vector2.up * 0.6f); else MinionFX.FrostBoltEnd(s.fx); } }
            else if (s.kind == 1) { if (s.fx >= 0) IceGorillaFX.ShardHit(s.fx, s.idx); }
            else { if (s.fx >= 0) QilinChariotFX.WheelEnd(s.fx); if (s.fx2 >= 0) QilinChariotFX.TrailEnd(s.fx2); }
            s.fx = s.fx2 = -1;
        }
        float FloorDmg44 => 1 + (T44.soften679 ? T44.enemyDmgGrowth679 : T44.floorEnemyDmgGrowth) * FloorIdx; // t679u
        void RoomBox44(out Vector2 lo, out Vector2 hi)
        {
            if (_room == null) { lo = _heroPos - Vector2.one * 8; hi = _heroPos + Vector2.one * 8; return; }
            _room.Walkable(16f, out lo, out hi); if (hi.x < lo.x) { float t = lo.x; lo.x = hi.x; hi.x = t; } if (hi.y < lo.y) { float t = lo.y; lo.y = hi.y; hi.y = t; }
        }
        Vector2 ClampBox44(Vector2 p) { RoomBox44(out var lo, out var hi); return new Vector2(Mathf.Clamp(p.x, lo.x, hi.x), Mathf.Clamp(p.y, lo.y, hi.y)); }
        void Hit44(float dmg, string src, bool boss) { HurtHero(dmg, src, boss); }
        void AddTrail44(Vector2 p, int kind, float life, float r, float dmg)
        {
            if (_trails44.Count > 0) { var l = _trails44[_trails44.Count - 1]; if (l.kind == kind && (l.p - p).sqrMagnitude < r * r * 0.5f) return; }
            if (_trails44.Count < 400) _trails44.Add(new Trail44 { p = p, life = life, life0 = life, r = r, dmg = dmg, kind = kind });
        }

        void UpdateV44(float dt)
        {
            FxTick44(); UpdateChill44(dt); if (mode != Mode.Play) return;
            _trailHitCd44 -= dt; UpdatePools44(dt); ShaderFxTick928(dt); FireWallTick948(dt); Fx934Tick(dt); FrostCleave989Tick(dt); // leftover 989 (no-op when 990 on)
            for (int i = _trails44.Count - 1; i >= 0; i--)
            {
                var t = _trails44[i]; t.life -= dt; if (t.life <= 0) { _trails44.RemoveAt(i); continue; }
                float a = Mathf.Clamp01(t.life / Mathf.Max(0.1f, t.life0));
                if (!FX44)
                {
                    if (t.kind == 0) { ZDisc(t.p, t.r, E32(0xa22633, 0.5f * a), 0.7f, 2); ZDisc(t.p, t.r * 0.55f, E32(0xe43b44, 0.7f * a), 0.7f, 3); }
                    else { ZDisc(t.p, t.r, E32(0xc0cbdc, 0.45f * a), 0.7f, 2); ZRing(t.p, t.r, E32(0xffffff, 0.4f * a), 0.7f, 3); }
                }
                if ((_heroPos - t.p).magnitude < t.r)
                {
                    if (t.kind == 0) { if (_trailHitCd44 <= 0) { _trailHitCd44 = 0.3f; Hit44(t.dmg * FloorDmg44, "fire_trail", false); } }
                    else ApplyFrostSlow37(T44.gSlideTrailSlowS);
                }
            }
            for (int i = _drops44.Count - 1; i >= 0; i--)
            {
                var d = _drops44[i];
                if (d.st == 0) { d.delay -= dt; if (d.delay <= 0) { d.st = 1; d.t = 0; } continue; }
                if (d.st == 1)
                {
                    d.t += dt; float p = Mathf.Clamp01(d.t / Mathf.Max(0.1f, d.tell));
                    if (T44.crispTellsV46 && (d.fx < 0 || PackHide46)) TellCircle46(d.p, d.r + 0.25f, d.kind == 1 ? E32(0xe43b44) : E32(0xc0cbdc), p, 4, true);
                    if (d.fx < 0)
                    {
                        Color fill = d.kind == 1 ? E32(0xe43b44, 0.15f + 0.3f * p) : E32(0x181425, 0.2f + 0.35f * p), rim = d.kind == 1 ? E32(0xa22633, 0.85f) : E32(0xc0cbdc, 0.85f);
                        if (!T44.crispTellsV46) { ZDisc(d.p, d.r * p, fill, 0.7f, 4); ZRing(d.p, d.r, rim, 0.7f, 5); }
                        if (d.kind == 0 && p > 0.6f) ZSeg(d.p + new Vector2(0, (1f - p) * 10f + 0.3f), d.p + new Vector2(0, (1f - p) * 10f + 1.3f), 0.3f, E32(0xc0cbdc, 0.9f), 6);
                        if (d.kind == 1 && p > 0.5f) ZDisc(d.p + new Vector2(0, (1f - p) * 12f), 0.4f, E32(0xe43b44), 1f, 6);
                    }
                    if (d.t >= d.tell)
                    {
                        if ((_heroPos - d.p).magnitude < d.r + 0.25f * HeroHitK47) Hit44(d.dmg, d.kind == 0 ? "icicle" : d.kind == 1 ? "meteor" : "boulder", d.boss);
                        if (d.fx < 0) HordeFX.CheapBurst(d.p, d.kind == 1 ? E32(0xe43b44) : E32(0xc0cbdc), 8);
                        ScreenShake.Add(0.6f, 0.08f);
                        HellordAudio.Play(d.kind == 1 ? "fireball_impact" : "frost_cleave", d.kind == 0 ? 1.5f + Random.value * 0.3f : 0.9f + Random.value * 0.2f);
                        if (d.kind == 2) SpawnShards44(d.p);
                        if (d.kind == 0 && d.stay > 0) { d.st = 2; d.t = d.stay; } else { _drops44.RemoveAt(i); }
                    }
                    continue;
                }
                d.t -= dt;
                if (d.fx < 0) { ZDisc(d.p, d.r * 0.55f, E32(0xc0cbdc, 0.9f), 0.7f, 4); ZRing(d.p, d.r * 0.55f, E32(0xffffff, 0.9f), 0.7f, 5); ZSeg(d.p, d.p + Vector2.up * 1.1f, 0.35f, E32(0x0099db, 0.95f), 6); }
                Vector2 off = _heroPos - d.p; float rr = d.r * 0.55f + 0.35f;
                if (off.magnitude < rr) _heroPos = d.p + (off.sqrMagnitude > 1e-4f ? off.normalized : Vector2.down) * rr;
                if (d.t <= 0) { if (d.fx < 0) HordeFX.CheapBurst(d.p + Vector2.up * 0.5f, E32(0xffffff), 6); _drops44.RemoveAt(i); }
            }
            RoomBox44(out var lo, out var hi);
            for (int i = _shots44.Count - 1; i >= 0; i--)
            {
                var s = _shots44[i]; s.life -= dt; s.p += s.v * dt;
                if (s.kind == 2)
                {
                    bool b = false;
                    if (s.p.x < lo.x || s.p.x > hi.x) { s.v.x = -s.v.x; s.p.x = Mathf.Clamp(s.p.x, lo.x, hi.x); b = true; }
                    if (s.p.y < lo.y || s.p.y > hi.y) { s.v.y = -s.v.y; s.p.y = Mathf.Clamp(s.p.y, lo.y, hi.y); b = true; }
                    if (b) { s.bounces--; if (s.fx >= 0) QilinChariotFX.WheelBounce(s.fx, s.p); HellordAudio.Play("fireball_impact", 1.4f); if (s.bounces < 0) s.life = 0; }
                    AddTrail44(s.p, 0, T44.fireTrailS * 0.6f, s.r * 0.6f, T44.fireTrailDmg);
                }
                else if (s.p.x < lo.x - 1 || s.p.x > hi.x + 1 || s.p.y < lo.y - 1 || s.p.y > hi.y + 1) s.life = 0;
                if (s.life <= 0) { EndShot44(s, false); _shots44.RemoveAt(i); continue; }
                if (s.kind == 0) { if (s.fx >= 0) MinionFX.FrostBoltMove(s.fx, s.p + Vector2.up * 0.6f); else { ZDisc(s.p + Vector2.up * 0.6f, s.r, E32(0x0099db, 0.9f), 1f, 7); ZDisc(s.p + Vector2.up * 0.6f, s.r * 0.5f, E32(0xffffff), 1f, 8); } }
                else if (s.kind == 1) { if (s.fx < 0) ZSeg(s.p, s.p + s.v.normalized * 0.6f, 0.2f, E32(0xc0cbdc), 7); }
                else if (s.fx >= 0) { QilinChariotFX.WheelMove(s.fx, s.p); if (s.fx2 >= 0) QilinChariotFX.TrailAdd(s.fx2, s.p); }
                else { ZDisc(s.p, s.r, E32(0xa22633, 0.9f), 1f, 6); ZDisc(s.p, s.r * 0.6f, E32(0xe43b44, 0.9f), 1f, 7); }
                if ((_heroPos + Vector2.up * 0.6f - (s.p + (s.kind == 0 ? Vector2.up * 0.6f : Vector2.zero))).magnitude < s.r + 0.35f * HeroHitK47)
                {
                    int hh = heroHits; Hit44(s.dmg, s.kind == 0 ? "frost_bolt" : s.kind == 1 ? "ice_shard" : "fire_wheel", s.boss);
                    if (s.kind == 0 && heroHits != hh) AddChill44(1);
                    if (s.kind != 2) { EndShot44(s, true); _shots44.RemoveAt(i); }
                }
            }
        }
        void SpawnShards44(Vector2 c, int boulderFx = -1)
        {
            var T = T44; Vector2 away = _boss != null ? c - _boss.pos : Vector2.down; float baseDeg = Mathf.Atan2(away.y, away.x) * Mathf.Rad2Deg; int n = Mathf.Max(1, T.gShards);
            for (int k = 0; k < n; k++)
            {
                float a = (baseDeg + (n == 1 ? 0 : Mathf.Lerp(-T.gShardFanDeg, T.gShardFanDeg, k / (float)(n - 1)))) * Mathf.Deg2Rad;
                _shots44.Add(new Shot44 { p = c, v = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * T.gShardSpeed, life = 2f, r = 0.25f, dmg = _boss.dmg * T.gShardDmgMul, kind = 1, boss = true, fx = boulderFx, idx = k });
            }
        }

        // ================= demons: variant 3 Frost Caster, 4 Fire Wheel =================
        readonly Dictionary<int, Sprite[]> _v44F = new Dictionary<int, Sprite[]>();
        Sprite[] Frames44(EType t, int variant)
        {
            int key = (int)t * 8 + variant; if (_v44F.TryGetValue(key, out var f) && f != null && f.Length > 0 && f[0]) return f;
            f = variant == 3 ? V37Recolor.Sprites(_eSprites[t], V37Recolor.Frost, T37.spriteRecolorGain * 1.35f, false) : V37Recolor.Sprites(_eSprites[t], V37Recolor.Fire, T37.spriteRecolorGain * 1.2f, false);
            _v44F[key] = f; return f;
        }
        /// <summary>Makes e a v0.0.44 demon (3 Frost Caster, 4 Fire Wheel).</summary>
        void MakeVariant44(Enemy e, int variant)
        {
            var T = T44; e.variant = (byte)variant; e.v44St = 0; e.v44T = Random.value * (variant == 3 ? T.frostCasterCooldownS : T.fireWheelCooldownS);
            if (variant == 3) { e.maxHp *= T.frostCasterHpMul; e.speed *= T.frostCasterSpeedMul; }
            else { e.maxHp *= T.fireWheelHpMul; e.speed *= T.fireWheelSpeedMul; }
            e.hp = e.maxHp; e.frames = Frames44(e.type, variant); e.sr.sprite = e.frames[0]; e.v44HurtT = 0; e.v44CastT = -1; e.v44AnimT = 0; e.v44Face = 0; e.v44West = false;
        }
        /// <summary>Rolled on spawn before the v0.0.37 variants. True = it became a v0.0.44 demon.</summary>
        bool ApplyVariant44(Enemy e)
        {
            int th = Theme; var T = T44;
            if (th == 1 && (e.type == EType.Imp || e.type == EType.Ghoul) && Random.value < T.frostCasterChance) { MakeVariant44(e, 3); return true; }
            if (th == 2 && (e.type == EType.Brute || e.type == EType.Ghoul) && Random.value < T.fireWheelChance) { MakeVariant44(e, 4); return true; }
            return false;
        }
        /// <summary>Movement / attacks of the v0.0.44 demons. Called from UpdateEnemies after the base velocity is set.</summary>
        void Think44(Enemy e, ref Vector2 v, Vector2 to, float dist, float sp, float dt, ref Color tint)
        {
            var T = T44; Vector2 dir = dist > 0.01f ? to / dist : Vector2.right; bool frozen = e.freezeT > 0;
            if (e.variant == 3)
            {
                if (e.v44St == 0)
                {
                    if (dist < T.frostCasterMinU) v = -dir * sp; else if (dist > T.frostCasterMaxU) v = dir * sp; else v = new Vector2(-dir.y, dir.x) * sp * 0.35f * e.strafe;
                    if (!frozen) e.v44T -= dt;
                    if (e.v44T <= 0 && dist < T.frostCasterMaxU + 3f) { e.v44St = 1; e.v44T = T.frostCasterTellS; HellordAudio.Play("curse_rune_arm", 1.5f); if (FX44) e.v44Fx = MinionFX.FrostCharge(e.pos + Vector2.up * 0.6f, dir, Mathf.Min(dist, T.frostBoltSpeed * T.frostBoltLifeS), T.frostCasterTellS); if (Random.value < 0.3f) e.strafe = -e.strafe; }
                }
                else
                {
                    v = Vector2.zero; if (!frozen) e.v44T -= dt; float p = 1f - Mathf.Clamp01(e.v44T / Mathf.Max(0.1f, T.frostCasterTellS));
                    e.v44Dir = dir; Vector2 o = e.pos + Vector2.up * 0.6f;
                    if (e.v44Fx < 0) { ZSeg(o, o + dir * Mathf.Min(dist, T.frostBoltSpeed * T.frostBoltLifeS), 0.08f + 0.12f * p, E32(0x0099db, 0.25f + 0.4f * p), 2); ZDisc(o, 0.25f + 0.35f * p, E32(0xffffff, 0.5f + 0.4f * p), 1f, 7); }
                    tint = Color.Lerp(Color.white, new Color(0.6f, 0.95f, 1f), p);
                    if (e.v44T <= 0)
                    {
                        e.v44Fx = -1;
                        if (V49) FcFan49(e, dir, o); else _shots44.Add(new Shot44 { p = e.pos, v = dir * T.frostBoltSpeed, life = T.frostBoltLifeS, r = T.frostBoltRadiusU, dmg = T.frostBoltDmg * FloorDmg44, kind = 0, fx = FX44 ? MinionFX.FrostBolt(o, dir) : -1 });
                        HellordAudio.Play("frost_cleave", 1.6f); e.v44St = 0; e.v44T = T.frostCasterCooldownS / Cd44_49; e.v44CastT = 0;
                    }
                }
                return;
            }
            // Fire Wheel: 0 wander, 1 wind-up spin, 2 roll, 3 recover
            switch (e.v44St)
            {
                case 0:
                    e.tr.rotation = Quaternion.identity; e.v44T -= dt;
                    if (e.v44T <= 0 && dist < T.fireWheelTriggerU && !frozen) { e.v44St = 1; e.v44T = T.fireWheelWindS; e.v44Dir = V49 ? LeadDir49(e.pos, T.fireWheelRollSpeed * FwMul49) : dir; HellordAudio.Play("oxhead_horn_windup", 1.6f); if (FX44) e.v44Fx = MinionFX.WheelSpin(e.pos, e.v44Dir, Mathf.Min(T.fireWheelRollMaxU, RayToBox44(e.pos, e.v44Dir)), T.fireWheelHalfWidthU, T.fireWheelWindS); }
                    break;
                case 1:
                {
                    v = Vector2.zero; if (!frozen) e.v44T -= dt; float p = 1f - Mathf.Clamp01(e.v44T / Mathf.Max(0.1f, T.fireWheelWindS));
                    WheelRot44(e, -Time.time * (300f + 900f * p));
                    if (e.v44Fx < 0)
                    {
                        float len = Mathf.Min(T.fireWheelRollMaxU, RayToBox44(e.pos, e.v44Dir)); Vector2 end = e.pos + e.v44Dir * len;
                        ZSeg(e.pos, end, T.fireWheelHalfWidthU * 2f, E32(0xa22633, 0.12f + 0.15f * p), 0); ZSeg(e.pos, Vector2.Lerp(e.pos, end, p), T.fireWheelHalfWidthU * 2f, E32(0xe43b44, 0.3f), 1);
                    }
                    tint = Color.Lerp(Color.white, new Color(1f, 0.6f, 0.4f), p);
                    if (e.v44T <= 0)
                    {
                        e.v44St = 2; e.v44T = T.fireWheelRollMaxU; e.v44Prev = e.pos; e.v44Hit = false; HellordAudio.Play("oxhead_lunge_dash", 1.4f);
                        e.v44Fx = -1; if (FX44) { EnsureFX44(); e.v44Tr = MinionFX.WheelTrailBegin(T.fireTrailRadiusU, T.fireTrailS); }
                    }
                    break;
                }
                case 2:
                {
                    float moved = (e.pos - e.v44Prev).magnitude; e.v44T -= moved; e.v44Prev = e.pos;
                    bool stuck = e.v44T < T.fireWheelRollMaxU - 0.5f && moved < T.fireWheelRollSpeed * FwMul49 * dt * 0.3f;
                    v = e.v44Dir * T.fireWheelRollSpeed * FwMul49; WheelRot44(e, -Time.time * 1200f);
                    AddTrail44(e.pos, 0, T.fireTrailS, T.fireTrailRadiusU, T.fireTrailDmg); if (e.v44Tr >= 0) MinionFX.WheelTrailAdd(e.v44Tr, e.pos);
                    if (!e.v44Hit && dist < T.fireWheelHalfWidthU + 0.4f * HeroHitK47) { int hh = heroHits; Hit44(T.fireWheelDmg * FloorDmg44, "fire_wheel", false); e.v44Hit = heroHits != hh; }
                    if (e.v44T <= 0 || stuck || frozen) { e.v44St = 3; e.v44T = T.fireWheelRecoverS; v = Vector2.zero; EndMinionFx44(e); TellFX.DustPuff(e.pos, 4, 8); if (stuck) { HellordAudio.Play("fireball_impact", 1.2f); ScreenShake.Add(0.8f, 0.1f); } }
                    break;
                }
                default:
                    v = Vector2.zero; WheelRot44(e, 90f * Mathf.Sign(e.v44Dir.x == 0 ? 1 : e.v44Dir.x)); e.v44T -= dt; tint = new Color(0.75f, 0.75f, 0.75f);
                    if (e.v44T <= 0) { e.v44St = 0; e.v44T = T.fireWheelCooldownS / Cd44_49; e.tr.rotation = Quaternion.identity; }
                    break;
            }
        }
        // ================= v0.0.44 demon sheets: Frost Caster (38x32, 8x12) and Fire Wheel (40x40, 8x5, side view) =================
        // Frost Caster rows: 0-2 idle S/E/N, 3-5 walk S/E/N, 6-8 cast S/E/N, 9-10 hurt S/E, 11 death S. W = E flipped.
        // Fire Wheel rows: 0 roll, 1 spin (wind-up), 2 dizzy, 3 hurt, 4 death. All E, W = flipped.
        static readonly int[] FcMsIdle = { 140, 140, 140, 140, 140, 140, 140, 140 }, FcMsWalk = { 100, 100, 100, 100, 100, 100, 100, 100 };
        static readonly int[] FcMsCast = { 150, 150, 150, 150, 70, 90, 110, 130 }, FcMsHurt = { 60, 80, 80, 80, 80, 100 }, FcMsDeath = { 90, 90, 70, 70, 80, 90, 120, 400 };
        static readonly int[] FwMsRoll = { 60, 60, 60, 60, 60, 60, 60, 60 }, FwMsSpin = { 120, 110, 100, 90, 80, 70, 65, 65 }, FwMsDizzy = { 150, 150, 150, 150, 150, 150, 150, 150 };
        static readonly int[] FwMsHurt = { 60, 60, 70, 80, 80, 100 }, FwMsDeath = { 60, 90, 70, 70, 80, 90, 150, 400 };
        Sprite[] _fcF44, _fwF44; bool _fcTried44, _fwTried44;
        Sprite[] CasterFrames44() { if (!_fcTried44) { _fcTried44 = true; if (!HArt.Missing("enemy_frost_caster")) _fcF44 = HArt.Sheet("enemy_frost_caster"); } return _fcF44; }
        Sprite[] WheelFrames44() { if (!_fwTried44) { _fwTried44 = true; if (!HArt.Missing("enemy_fire_wheel")) _fwF44 = HArt.Sheet("enemy_fire_wheel"); } return _fwF44; }
        bool CasterSheetOn44 => T44.frostCasterSheet && CasterFrames44() != null && CasterFrames44().Length >= 96;
        bool FireWheelSheetOn44 => T44.fireWheelSheet && WheelFrames44() != null && WheelFrames44().Length >= 40;
        void WheelRot44(Enemy e, float z) { e.tr.rotation = FireWheelSheetOn44 ? Quaternion.identity : Quaternion.Euler(0, 0, z); }
        /// <summary>v0.0.44: overrides the recoloured walk frame with the Frost Caster / Fire Wheel sheet. Runs after Think44 in UpdateEnemies.</summary>
        void Animate44(Enemy e, Vector2 v, Vector2 to, float dt)
        {
            var T = T44; bool frozen = e.freezeT > 0; if (!frozen) e.v44AnimT += dt;
            if (e.v44HurtT > 0) e.v44HurtT -= dt; if (e.v44CastT >= 0) e.v44CastT += dt;
            bool newHit = e.hitT > 0.08f - dt - 1e-4f && e.hitT > 0;
            if (e.variant == 3)
            {
                var F = CasterFrames44(); if (!CasterSheetOn44) return;
                Vector2 d = e.v44St == 1 ? e.v44Dir : v.sqrMagnitude > 0.01f ? v : to;
                if (d.sqrMagnitude > 0.0001f) { e.v44Face = Mathf.Abs(d.x) >= Mathf.Abs(d.y) * 0.7f ? 1 : d.y > 0 ? 2 : 0; if (e.v44Face == 1) e.v44West = d.x < 0; }
                if (newHit && e.v44St != 1 && e.v44HurtT <= 0) e.v44HurtT = FcMsHurt.Sum44() / 1000f;
                int row, col;
                if (e.v44St == 1) { row = 6 + e.v44Face; col = MsSpan44(FcMsCast, 0, 3, 1f - Mathf.Clamp01(e.v44T / Mathf.Max(0.1f, T.frostCasterTellS))); }
                else if (e.v44CastT >= 0 && e.v44CastT < FcMsCast.Sum44() / 1000f - 0.6f) { row = 6 + e.v44Face; col = 4 + MsCol44(new[] { 70, 90, 110, 130 }, e.v44CastT, false); }
                else if (e.v44HurtT > 0) { row = e.v44Face == 1 ? 10 : 9; col = MsCol44(FcMsHurt, FcMsHurt.Sum44() / 1000f - e.v44HurtT, false); }
                else if (v.sqrMagnitude > 0.01f) { row = 3 + e.v44Face; col = MsCol44(FcMsWalk, e.v44AnimT, true); }
                else { row = e.v44Face; col = MsCol44(FcMsIdle, e.v44AnimT, true); }
                if (e.v44CastT >= FcMsCast.Sum44() / 1000f) e.v44CastT = -1;
                bool flip = (row == 1 || row == 4 || row == 7 || row == 10) && e.v44West;
                e.sr.sprite = F[row * 8 + Mathf.Clamp(col, 0, 7)]; e.sr.flipX = flip;
            }
            else
            {
                var F = WheelFrames44(); if (!FireWheelSheetOn44) return;
                Vector2 d = e.v44St == 1 || e.v44St == 2 || e.v44St == 3 ? e.v44Dir : v.sqrMagnitude > 0.01f ? v : to;
                if (Mathf.Abs(d.x) > 0.05f) e.v44West = d.x < 0;
                if (newHit && (e.v44St == 0) && e.v44HurtT <= 0) e.v44HurtT = FwMsHurt.Sum44() / 1000f;
                int row, col;
                if (e.v44St == 1) { row = 1; col = MsSpan44(FwMsSpin, 0, 7, 1f - Mathf.Clamp01(e.v44T / Mathf.Max(0.1f, T.fireWheelWindS))); }
                else if (e.v44St == 2) { row = 0; col = MsCol44(FwMsRoll, e.v44AnimT, true); }
                else if (e.v44St == 3) { row = 2; col = MsCol44(FwMsDizzy, T.fireWheelRecoverS - e.v44T, true); }
                else if (e.v44HurtT > 0) { row = 3; col = MsCol44(FwMsHurt, FwMsHurt.Sum44() / 1000f - e.v44HurtT, false); }
                else { row = 0; col = v.sqrMagnitude > 0.01f ? MsCol44(FwMsRoll, e.v44AnimT * T.fireWheelWanderAnimMul, true) : 0; }
                e.sr.sprite = F[row * 8 + Mathf.Clamp(col, 0, 7)]; e.sr.flipX = e.v44West;
            }
        }
        /// <summary>v0.0.44: sheet death for the Frost Caster (row 11, ice shatter, last frame held) / Fire Wheel (row 4). False = use the normal death strip.</summary>
        bool Death44(Enemy e, float dt)
        {
            bool caster = e.variant == 3; if (caster ? !CasterSheetOn44 : !FireWheelSheetOn44) return false;
            var F = caster ? CasterFrames44() : WheelFrames44(); var ms = caster ? FcMsDeath : FwMsDeath;
            e.tr.rotation = Quaternion.identity; e.deathT += dt;
            if (e.deathT >= ms.Sum44() / 1000f) { e.dying = false; e.sr.enabled = false; e.sr.gameObject.SetActive(false); return true; }
            e.sr.sprite = F[(caster ? 11 : 4) * 8 + MsCol44(ms, e.deathT, false)]; e.sr.flipX = caster ? false : e.v44West;
            return true;
        }
        float RayToBox44(Vector2 p, Vector2 d)
        {
            RoomBox44(out var lo, out var hi); float t = 99;
            if (d.x > 1e-4f) t = Mathf.Min(t, (hi.x - p.x) / d.x); else if (d.x < -1e-4f) t = Mathf.Min(t, (lo.x - p.x) / d.x);
            if (d.y > 1e-4f) t = Mathf.Min(t, (hi.y - p.y) / d.y); else if (d.y < -1e-4f) t = Mathf.Min(t, (lo.y - p.y) / d.y);
            return Mathf.Max(0.5f, t);
        }

        // ================= bosses =================
        // kind: 0 Ox-Head, 1 Frost Ox-Head (legacy), 2 Horse-Face (future floor), 3 Ice Gorilla, 4 Qilin Chariot
        int _bossKind44 = -1;
        int BossKind44 => _bossKind44 >= 0 ? _bossKind44 : MortarBoss53 ? 2 : Theme == 1 ? 3 : Theme == 2 ? 4 : 0;
        bool IsGorilla => BossKind44 == 3; bool IsQilin => BossKind44 == 4;
        string BossSub44() => BossKind44 switch { 1 => "THE FROZEN JAILER - WATCH THE ICE BREATH", 2 => MortarBoss53 ? "HORSE-FACE, THE PESTLE WARDEN - FAST THRUSTS AND CHARGES" : "THE SPEAR WARDEN - FAST THRUSTS AND CHARGES", 3 => "KING OF THE GLACIER - WATCH THE SKY", 4 => "THE BURNING WAR CART - MIND THE FIRE", _ => "THE CLUB JAILER" };

        char _atk44; int _ph44; float _t44, _tot44, _tick44, _deg44; bool _enr44, _air44, _second44; Vector2 _tgt44, _dir44, _from44; float _len44;
        float BossDmg44(float mul) => _boss.dmg * mul;

        void SetupBoss44()
        {
            _enr44 = false; _air44 = false; _second44 = false; if (_bossBody) _bossBody.transform.localPosition = Vector3.zero;
        }
        void StartAtk44(char c)
        {
            var T = T44; var e = _boss;
            if (!_enr44 && e.hp < e.maxHp * (IsGorilla ? T.gorillaEnrageAt : T.qilinEnrageAt))
            {
                _enr44 = true; Toast(IsGorilla ? "THE GORILLA IS ENRAGED!" : "THE CHARIOT IS ENRAGED!"); HellordAudio.Play("oxhead_roar", 0.85f); ScreenShake.Add(2f, 0.25f);
                if (IsGorilla) { var s = (T.gorillaCycleEnraged ?? "").ToUpperInvariant().Replace(" ", ""); if (s.Length > 0) { _cycle37 = s.ToCharArray(); _atk = 0; c = _cycle37[_atk++ % _cycle37.Length]; } }
            }
            _atk44 = c; _ph44 = 0; _second44 = false; Vector2 to = _heroPos - e.pos; _dir44 = to.sqrMagnitude > 0.01f ? to.normalized : Vector2.down;
            if (IsGorilla)
            {
                switch (c)
                {
                    case 'B': _t44 = _tot44 = T.gBoulderTellS / Rage; _tgt44 = _heroPos; if (FX44) _fxA44 = IceGorillaFX.Boulder(e.pos, _tgt44, T.gBoulderRadiusU, _tot44, T.gBoulderFlightS); Play("stomp_windup", true); HellordAudio.Play("oxhead_stomp_windup", 0.8f); break;
                    case 'L': _t44 = _tot44 = T.gLeapCrouchS / Rage; _squash = -0.15f; Play("stomp_windup", true); HellordAudio.Play("oxhead_stomp_windup", 1.1f); break;
                    case 'S': SlideTell44(T.gSlideTellS, T.gSlideHalfWidthU); break;
                    case 'R': _t44 = _tot44 = T.gRoarTellS / Rage; if (FXT44) _fxA44 = IceGorillaFX.Roar(e.pos, T.gRoarRadiusU, _tot44); Play("roar", true); HellordAudio.Play("oxhead_horn_windup", 0.8f); break;
                    default: _atk44 = 'G'; _t44 = _tot44 = T.gSlamTellS / Rage; _squash = -0.18f; if (FXT44) _fxA44 = IceGorillaFX.Slam(e.pos, T.gSlamRingU, _tot44); Play("stomp_windup", true); HellordAudio.Play("oxhead_stomp_windup", 0.7f); break;
                }
            }
            else
            {
                switch (c)
                {
                    case 'W': _t44 = _tot44 = T.qWheelTellS / Rage; if (FXT44) _fxA44 = QilinChariotFX.WheelTell(e.pos, Mathf.Atan2(_dir44.y, _dir44.x) * Mathf.Rad2Deg, Mathf.Max(1, T.qWheels), T.qWheelSpreadDeg, Mathf.Min(T.qWheelTellLenU, RayToBox44(e.pos, _dir44)), T.qWheelRadiusU, _tot44); Play("horn_windup", true); HellordAudio.Play("oxhead_horn_windup", 1.2f); break;
                    case 'F': _t44 = _tot44 = T.qBreathTellS / Rage; _deg44 = Mathf.Atan2(_dir44.y, _dir44.x) * Mathf.Rad2Deg; if (FX44) _fxA44 = QilinChariotFX.Breath(QMouth44(e.pos, _dir44), _deg44, T.qBreathSweepDeg, T.qBreathHalfDeg, T.qBreathReachU * QS46, _tot44, T.qBreathS); Play("horn_windup", true); HellordAudio.Play("oxhead_horn_windup", 0.9f); break;
                    case 'D':
                    {
                        _tgt44 = ClampBox44(_heroPos); _deg44 = Random.value * 360f; _t44 = _tot44 = T.qDriftTellS / Rage;
                        Vector2 st = _tgt44 + new Vector2(Mathf.Cos(_deg44 * Mathf.Deg2Rad), Mathf.Sin(_deg44 * Mathf.Deg2Rad)) * T.qDriftRadiusU; _from44 = ClampBox44(st);
                        if (FXT44) _fxA44 = QilinChariotFX.DriftRing(_tgt44, _deg44 + 180f, T.qDriftRadiusU, T.qDriftGapDeg, T.qDriftWidthU, _tot44, T.qDriftDriveS, T.qDriftCollapseS);
                        Play("roar", true); HellordAudio.Play("oxhead_roar", 1.3f); break;
                    }
                    case 'M' when T.bossPatternsV46 && !T.qilinOldMeteorsV46: _atk44 = 'F'; _t44 = _tot44 = T.qBreathTellS / Rage; _deg44 = Mathf.Atan2(_dir44.y, _dir44.x) * Mathf.Rad2Deg; if (FX44) _fxA44 = QilinChariotFX.Breath(QMouth44(e.pos, _dir44), _deg44, T.qBreathSweepDeg, T.qBreathHalfDeg, T.qBreathReachU * QS46, _tot44, T.qBreathS); Play("horn_windup", true); break;
                    case 'M': _t44 = _tot44 = T.qMeteorTellS / Rage; _squash = -0.15f; Play("stomp_windup", true); HellordAudio.Play("oxhead_stomp_windup", 1.2f); break;
                    default: _atk44 = 'C'; SlideTell44(T.qChargeTellS, QContact46); break;
                }
            }
            Go(BS.X44, 99f);
        }
        void SlideTell44(float tell, float halfW)
        {
            var e = _boss; _dir44 = (_heroPos - e.pos).sqrMagnitude > 0.01f ? (_heroPos - e.pos).normalized : Vector2.down; _chargeDir = _dir44;
            _t44 = _tot44 = Mathf.Max(0.3f, tell / Rage); Vector2 end = e.pos + _dir44 * RayToWall(e.pos, _dir44);
            if (IsQilin && FXT44) _fxA44 = QilinChariotFX.ChargeTell(e.pos, _dir44, RayToWall(e.pos, _dir44), halfW, _tot44);
            else { float lift = T44.crispTellsV46 ? 0f : 0.5f; if (T44.crispTellsV46) TimedLane46(e.pos, end, halfW * 2f * 16f, _tot44); else _lane.Begin(e.pos + Vector2.up * lift, end + Vector2.up * lift, halfW * 2f * 16f, _tot44); }
            Play(GoreSide ? "gore_windup_E" : "gore_windup_S", true); HellordAudio.Play("oxhead_horn_windup", IsGorilla ? 0.8f : 1.1f);
        }
        void EndAtk44(float rest)
        {
            _air44 = false; if (_bossBody) _bossBody.transform.localPosition = Vector3.zero; StopH(ref _speedH); StopH(ref _dustH); EndTrail44();
            if (IsGorilla && (_atk44 == 'L' || _atk44 == 'S')) { _gTail44 = _atk44 == 'L' ? 0.4f : 0.18f; _gTailAtk44 = _atk44; }
            _fxA44 = -1; Go(BS.Recover, rest);
        }
        int _fxA44 = -1, _fxTr44 = -1; float _gTail44; char _gTailAtk44; // v0.0.44 FX handles for the current boss attack; Gorilla landing / slide-stop tail frames
        void EndTrail44()
        {
            if (_fxTr44 < 0) return;
            if (IsGorilla) IceGorillaFX.SlideEnd(_fxTr44); else QilinChariotFX.TrailEnd(_fxTr44);
            _fxTr44 = -1;
        }
        bool MoveDash44(Enemy e, float speed, float dt)
        {
            e.pos += _dir44 * speed * Rage * dt; BossBounds(out var lo, out var hi); bool wall = false;
            if (e.pos.x < lo.x) { e.pos.x = lo.x; wall = true; } else if (e.pos.x > hi.x) { e.pos.x = hi.x; wall = true; }
            if (e.pos.y < lo.y) { e.pos.y = lo.y; wall = true; } else if (e.pos.y > hi.y) { e.pos.y = hi.y; wall = true; }
            return wall;
        }
        void UpdateAtk44(Enemy e, float dt, float dist)
        {
            var T = T44; _t44 -= dt; float p = 1f - Mathf.Clamp01(_t44 / Mathf.Max(0.05f, _tot44));
            if (IsQilin && FX44 && T.qEmbersS > 0) { _emb44 -= dt; if (_emb44 <= 0) { _emb44 = T.qEmbersS; QilinChariotFX.Embers(e.pos + Vector2.up * 0.8f * T.qilinScale46, 3); } }
            if (IsGorilla) Gorilla44(e, dt, dist, p, T); else Qilin44(e, dt, dist, p, T);
        }

        void Gorilla44(Enemy e, float dt, float dist, float p, HellordTuning T)
        {
            switch (_atk44)
            {
                case 'G':
                    if (_ph44 == 0)
                    {
                        if (_fxA44 < 0 && T.crispTellsV46) TellCircle46(e.pos, T.gSlamRingU, E32(0x2ce8f5), p, 4);
                        else if (_fxA44 < 0) { ZDisc(e.pos, T.gSlamRingU * p, E32(0x3a4466, 0.25f), 0.6f, 4); ZRing(e.pos, T.gSlamRingU, E32(0xc0cbdc, 0.9f), 0.6f, 5); }
                        _squash = Mathf.MoveTowards(_squash, -0.18f, dt);
                        if (_t44 <= 0)
                        {
                            Play("stomp_strike", true); HellordAudio.Play("oxhead_stomp_strike", 0.8f); ScreenShake.Add(3f, 0.35f); _squash = 0.2f; TellFX.DustPuff(e.pos, 7, 14);
                            if (dist < T.gSlamRingU) Hit44(BossDmg44(T.gSlamRingDmgMul), "glacier_slam", true);
                            SlamSpikes46(e.pos);
                            int n = !T.bossPatternsV46 || T.gorillaSlamIciclesV46 ? (_enr44 ? T.gIciclesEnraged : T.gIcicles) : 0; int aimed = Mathf.RoundToInt(n * T.gIcicleAimedFrac); RoomBox44(out var lo, out var hi);
                            for (int k = 0; k < n; k++)
                            {
                                Vector2 at = k == 0 ? _heroPos : k < aimed ? _heroPos + Random.insideUnitCircle * T.gIcicleAimSpreadU : new Vector2(Random.Range(lo.x, hi.x), Random.Range(lo.y, hi.y));
                                _drops44.Add(new Drop44 { p = ClampBox44(at), delay = k * T.gIcicleStaggerS, tell = T.gIcicleTellS, r = T.gIcicleRadiusU, dmg = BossDmg44(T.gIcicleDmgMul), stay = T.gIcicleStayS, kind = 0, boss = true, fx = FX44 ? IceGorillaFX.Icicle(ClampBox44(at), k * T.gIcicleStaggerS, T.gIcicleRadiusU, T.gIcicleTellS, T.gIcicleStayS) : -1 });
                            }
                            _ph44 = 1; _t44 = 0.5f;
                        }
                    }
                    else if (_t44 <= 0) EndAtk44(T.gRecoverS);
                    break;
                case 'B':
                    if (_ph44 == 0)
                    {
                        if (T.crispTellsV46 && (_fxA44 < 0 || PackHide46)) TellCircle46(_tgt44, T.gBoulderRadiusU + 0.25f, E32(0x2ce8f5), p * 0.5f, 4, true);
                        if (_fxA44 < 0)
                        {
                            if (!T.crispTellsV46) { ZRing(_tgt44, T.gBoulderRadiusU, E32(0xc0cbdc, 0.5f + 0.4f * p), 0.7f, 5); ZDisc(_tgt44, T.gBoulderRadiusU * p * 0.5f, E32(0x181425, 0.3f), 0.7f, 4); }
                            ZDisc(e.pos + new Vector2(0, 2.5f + p), 0.6f + 0.4f * p, E32(0xc0cbdc), 1f, 8);
                        }
                        if (_t44 <= 0) { _ph44 = 1; _t44 = _tot44 = T.gBoulderFlightS; _from44 = e.pos + new Vector2(0, 3.5f); Play("horn_strike", true); HellordAudio.Play("oxhead_horn_strike", 0.7f); }
                    }
                    else
                    {
                        Vector2 pos = Vector2.Lerp(_from44, _tgt44, p) + Vector2.up * Mathf.Sin(p * Mathf.PI) * 3f;
                        if (T.crispTellsV46 && (_fxA44 < 0 || PackHide46)) TellCircle46(_tgt44, T.gBoulderRadiusU + 0.25f, E32(0x2ce8f5), 0.5f + 0.5f * p, 4, true);
                        if (_fxA44 < 0) { if (!T.crispTellsV46) { ZDisc(_tgt44, T.gBoulderRadiusU * p, E32(0x181425, 0.3f + 0.3f * p), 0.7f, 4); ZRing(_tgt44, T.gBoulderRadiusU, E32(0xffffff, 0.9f), 0.7f, 5); } ZDisc(pos, 1f, E32(0xc0cbdc), 1f, 9); ZRing(pos, 1f, E32(0x3a4466), 1f, 10); }
                        if (_t44 <= 0)
                        {
                            if ((_heroPos - _tgt44).magnitude < T.gBoulderRadiusU + 0.25f * HeroHitK47) Hit44(BossDmg44(T.gBoulderDmgMul), "ice_boulder", true);
                            if (_fxA44 < 0) HordeFX.CheapBurst(_tgt44, E32(0xc0cbdc), 16);
                            ScreenShake.Add(2f, 0.2f); HellordAudio.Play("frost_cleave", 0.7f); SpawnShards44(_tgt44, _fxA44); EndAtk44(T.gRecoverS);
                        }
                    }
                    break;
                case 'L':
                    if (_ph44 == 0)
                    {
                        _squash = Mathf.MoveTowards(_squash, -0.18f, dt);
                        if (_t44 <= 0) { _ph44 = 1; _t44 = _tot44 = T.gLeapAirS; _air44 = true; _from44 = e.pos; _tgt44 = _heroPos; if (FX44) _fxA44 = IceGorillaFX.Leap(_tgt44, T.gLeapRingU, T.gLeapAirS, T.gLeapLockS); HellordAudio.Play("oxhead_lunge_dash", 0.7f); TellFX.DustPuff(e.pos, 5, 10); }
                    }
                    else
                    {
                        bool locked = _t44 <= T.gLeapLockS; if (!locked) { _tgt44 = Vector2.MoveTowards(_tgt44, _heroPos, 12f * dt); IceGorillaFX.LeapTrack(_fxA44, _tgt44); }
                        Vector2 shadow = Vector2.Lerp(_from44, _tgt44, p); e.pos = shadow; float lift = Mathf.Sin(p * Mathf.PI);
                        if (_bossBody) _bossBody.transform.localPosition = new Vector3(0, lift * T.gLeapHeightU, 0);
                        ZDisc(shadow, T.gLeapShadowU * Mathf.Lerp(1f, T.gLeapShadowPeakMul, lift), E32(0x181425, T.gLeapShadowAlpha), 0.6f, 4);
                        if (_fxA44 < 0 && T.crispTellsV46) TellCircle46(_tgt44, T.gLeapRingU, E32(0x2ce8f5), locked ? 1f : lift * 0.8f, 4, true);
                        else if (_fxA44 < 0) { ZRing(_tgt44, T.gLeapRingU, locked ? E32(0xffffff, 0.95f) : E32(0xc0cbdc, 0.5f), 0.6f, 5); if (locked) ZDisc(_tgt44, T.gLeapRingU, E32(0x3a4466, 0.25f), 0.6f, 4); }
                        if (_t44 <= 0)
                        {
                            e.pos = _tgt44; _air44 = false; if (_bossBody) _bossBody.transform.localPosition = Vector3.zero; _squash = 0.2f;
                            if ((_heroPos - e.pos).magnitude < T.gLeapRingU) Hit44(BossDmg44(T.gLeapDmgMul), "leap_smash", true);
                            Play("stomp_strike", true); HellordAudio.Play("oxhead_stomp_strike", 0.9f); ScreenShake.Add(2.5f, 0.25f); TellFX.DustPuff(e.pos, 7, 14);
                            EndAtk44(T.gRecoverS);
                        }
                    }
                    break;
                case 'S':
                    if (_ph44 == 0)
                    {
                        _squash = Mathf.MoveTowards(_squash, -0.12f, dt);
                        if (_t44 <= 0) { _ph44 = 1; _t44 = 3f; Play(GoreSide ? "gore_loop_E" : "gore_loop_S"); HellordAudio.Play("oxhead_lunge_dash", 0.8f); _speedH = TellFX.SpeedLines(_bossRoot, 0f, 24, LanePx); _second44 = false; if (FX44) _fxTr44 = IceGorillaFX.SlideBegin(T.gSlideHalfWidthU, T.gSlideTrailS); }
                    }
                    else
                    {
                        bool wall = MoveDash44(e, T.gSlideSpeed, dt); AddTrail44(e.pos, 1, T.gSlideTrailS, T.gSlideHalfWidthU, 0); if (_fxTr44 >= 0) IceGorillaFX.SlideAdd(_fxTr44, e.pos);
                        if (!_second44 && dist < T.gSlideHalfWidthU + 0.4f) { _second44 = true; Hit44(BossDmg44(T.gSlideDmgMul), "belly_slide", true); }
                        if (wall || _t44 <= 0) { TellFX.DustPuff(e.pos, 5, 10); ScreenShake.Add(1.5f, 0.15f); HellordAudio.Play("oxhead_stomp_strike", 1.1f); EndAtk44(T.gRecoverS); }
                    }
                    break;
                default:
                    if (_ph44 == 0)
                    {
                        if (_fxA44 < 0 && T.crispTellsV46) TellRing46(e.pos, T.gRoarRadiusU, E32(0x2ce8f5), p, 4);
                        else if (_fxA44 < 0) { ZDisc(e.pos, T.gRoarRadiusU * p, E32(0x3a4466, 0.2f), 0.6f, 4); ZRing(e.pos, T.gRoarRadiusU, E32(0xc0cbdc, 0.6f + 0.3f * p), 0.6f, 5); }
                        if (_t44 <= 0)
                        {
                            if ((_heroPos - e.pos).magnitude < T.gRoarRadiusU) AddChill44(T.gRoarStacks);
                            HellordAudio.Play("oxhead_roar", 0.8f); ScreenShake.Add(2f, 0.3f); _ph44 = 1; _t44 = 0.4f;
                        }
                    }
                    else { if (_fxA44 < 0) ZRing(e.pos, T.gRoarRadiusU * (1f + (0.4f - _t44)), E32(0xffffff, Mathf.Clamp01(_t44 * 2f)), 0.6f, 5); if (_t44 <= 0) EndAtk44(T.gRecoverS); }
                    break;
            }
        }

        void Qilin44(Enemy e, float dt, float dist, float p, HellordTuning T)
        {
            switch (_atk44)
            {
                case 'C':
                    if (_ph44 == 0)
                    {
                        _squash = Mathf.MoveTowards(_squash, -0.12f, dt);
                        if (_t44 <= 0) { _ph44 = 1; _t44 = 3f; _tick44 = 0; Play(GoreSide ? "gore_loop_E" : "gore_loop_S"); HellordAudio.Play("oxhead_lunge_dash", 1.1f); _speedH = TellFX.SpeedLines(_bossRoot, 0f, 24, LanePx); _dustH = TellFX.DustTrail(_bossRoot, 0f, 5, 0); _fxA44 = -1; if (FX44) _fxTr44 = QilinChariotFX.TrailBegin(T.fireTrailRadiusU * 1.5f * QS46, T.fireTrailS * 1.5f); }
                    }
                    else if (_ph44 == 1)
                    {
                        bool wall = MoveDash44(e, T.qChargeSpeed, dt); AddTrail44(e.pos, 0, T.fireTrailS * 1.5f, T.fireTrailRadiusU * 1.5f * QS46, T.fireTrailDmg * 1.5f); if (_fxTr44 >= 0) QilinChariotFX.TrailAdd(_fxTr44, e.pos);
                        if (_tick44 == 0 && dist < QContact46) { _tick44 = 1; Hit44(BossDmg44(T.qChargeDmgMul), "blazing_charge", true); }
                        if (wall || _t44 <= 0)
                        {
                            StopH(ref _speedH); StopH(ref _dustH); EndTrail44(); TellFX.WallImpact(e.pos + Vector2.up * BossHurtY, -_dir44, _bossRoot, 1f); e.pos -= _dir44 * 0.8f; ScreenShake.Add(2f, 0.2f); HellordAudio.Play("oxhead_stomp_strike", 1.2f);
                            if (_enr44 && !_second44) { _second44 = true; SlideTell44(T.qChargeRetellS, QContact46); _ph44 = 0; }
                            else { _ph44 = 2; _t44 = T.qChargeStaggerS; Play("stun_loop", true); }
                        }
                    }
                    else if (_t44 <= 0) EndAtk44(T.qRecoverS);
                    break;
                case 'W':
                {
                    int n = Mathf.Max(1, T.qWheels); float baseDeg = Mathf.Atan2(_dir44.y, _dir44.x) * Mathf.Rad2Deg;
                    if (_ph44 == 0)
                    {
                        if (_fxA44 < 0)
                        {
                            for (int k = 0; k < n; k++)
                            {
                                float a = (baseDeg + (n == 1 ? 0 : Mathf.Lerp(-T.qWheelSpreadDeg, T.qWheelSpreadDeg, k / (float)(n - 1)))) * Mathf.Deg2Rad; Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                                if (T.crispTellsV46) TellLane46(e.pos, e.pos + d * Mathf.Min(T.qWheelTellLenU, RayToBox44(e.pos, d)), T.qWheelRadiusU * 2f + 0.5f, E32(0xfeae34), p, 1);
                                else ZSeg(e.pos, e.pos + d * T.qWheelTellLenU * p, T.qWheelRadiusU * 2f, E32(0xa22633, 0.25f + 0.25f * p), 1);
                            }
                        }
                        if (_t44 <= 0)
                        {
                            for (int k = 0; k < n; k++)
                            {
                                float a = (baseDeg + (n == 1 ? 0 : Mathf.Lerp(-T.qWheelSpreadDeg, T.qWheelSpreadDeg, k / (float)(n - 1)))) * Mathf.Deg2Rad;
                                _shots44.Add(new Shot44 { p = e.pos, v = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * T.qWheelSpeed, life = 20f, r = T.qWheelRadiusU, dmg = BossDmg44(T.qWheelDmgMul), kind = 2, bounces = T.qWheelBounces, boss = true, fx = FX44 ? QilinChariotFX.Wheel(e.pos, T.qWheelRadiusU) : -1, fx2 = FX44 ? QilinChariotFX.TrailBegin(T.qWheelRadiusU * 0.6f, T.fireTrailS * 0.6f) : -1 });
                            }
                            HellordAudio.Play("fireball_impact", 0.8f); Play("horn_strike", true); _ph44 = 1; _t44 = 0.4f;
                        }
                    }
                    else if (_t44 <= 0) EndAtk44(T.qRecoverS);
                    break;
                }
                case 'F':
                {
                    Vector2 o = T.crispTellsV46 ? QBreathGround46(e.pos, _dir44) : QMouth44(e.pos, _dir44); float reach = T.qBreathReachU * QS46;
                    if (_ph44 == 0)
                    {
                        float half = T.qBreathSweepDeg * 0.5f + T.qBreathHalfDeg;
                        if ((_fxA44 < 0 || PackHide46) && T.crispTellsV46) TellWedge46(o, _deg44, half, reach, E32(0xe43b44), p, 0);
                        else if (_fxA44 < 0) { ZWedge(o, _deg44, half, reach, E32(0xa22633, 0.15f + 0.1f * p), 0); ZWedge(o, _deg44 - T.qBreathSweepDeg * 0.5f, T.qBreathHalfDeg, reach, E32(0xe43b44, 0.35f), 2); }
                        if (_t44 <= 0) { _ph44 = 1; _t44 = _tot44 = T.qBreathS; _tick44 = 0; Play("roar", true); HellordAudio.Play("fireball_impact", 0.6f); }
                    }
                    else
                    {
                        float cur = _deg44 - T.qBreathSweepDeg * 0.5f + T.qBreathSweepDeg * p;
                        if (_fxA44 < 0) { ZWedge(o, _deg44, T.qBreathSweepDeg * 0.5f + T.qBreathHalfDeg, reach, E32(0xa22633, 0.1f), 0); ZWedge(o, cur, T.qBreathHalfDeg, reach, E32(0xe43b44, 0.55f), 2); }
                        Bs950Breath(true, o, cur, reach, T.qBreathHalfDeg); // t986u PixelBossGround mode 3
                        _tick44 -= dt; Vector2 d = T.crispTellsV46 ? _heroPos - o : _heroPos + Vector2.up * 0.5f - o;
                        if (_tick44 <= 0 && d.magnitude < reach && Vector2.Angle(new Vector2(Mathf.Cos(cur * Mathf.Deg2Rad), Mathf.Sin(cur * Mathf.Deg2Rad)), d) < T.qBreathHalfDeg) { _tick44 = 0.25f; Hit44(BossDmg44(T.qBreathDmgMul), "flame_breath", true); }
                        if (_t44 <= 0) { Bs950Breath(false, o, cur, reach, T.qBreathHalfDeg); EndAtk44(T.qRecoverS); }
                    }
                    break;
                }
                case 'D':
                {
                    float R = T.qDriftRadiusU, gapC = _deg44 + 180f;
                    if (_ph44 == 0)
                    {
                        e.pos = Vector2.MoveTowards(e.pos, _from44, 18f * dt); if (_fxA44 < 0) { if (T.crispTellsV46) TellRing46(_tgt44, R, E32(0xfeae34), p, 4, gapC, T.qDriftGapDeg * 0.5f); else { ZRing(_tgt44, R, E32(0xa22633, 0.35f + 0.3f * p), 1f, 5); DrawGap44(_tgt44, R, gapC, T); } }
                        if (_t44 <= 0) { _ph44 = 1; _t44 = _tot44 = T.qDriftDriveS; _tick44 = 0; Play("gore_loop_E"); HellordAudio.Play("oxhead_lunge_dash", 1.2f); }
                    }
                    else if (_ph44 == 1)
                    {
                        float sweep = 360f - T.qDriftGapDeg, a = (gapC + T.qDriftGapDeg * 0.5f + sweep * p) * Mathf.Deg2Rad;
                        e.pos = ClampBox44(_tgt44 + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * R); _chargeDir = new Vector2(-Mathf.Sin(a), Mathf.Cos(a));
                        if (_fxA44 < 0) { DrawFireArc44(_tgt44, R, gapC + T.qDriftGapDeg * 0.5f, sweep * p, 0.9f); DrawGap44(_tgt44, R, gapC, T); }
                        if (_tick44 == 0 && dist < QContact46) { _tick44 = 1; Hit44(BossDmg44(T.qDriftDmgMul), "drift_circle", true); }
                        if (_t44 <= 0) { _ph44 = 2; _t44 = _tot44 = T.qDriftCollapseS; _tick44 = 0; HellordAudio.Play("fireball_impact", 0.7f); }
                    }
                    else
                    {
                        float r = Mathf.Lerp(R, 0.6f, p); float sweep = 360f - T.qDriftGapDeg; if (_fxA44 < 0) { DrawFireArc44(_tgt44, r, gapC + T.qDriftGapDeg * 0.5f, sweep, 1f); DrawGap44(_tgt44, r, gapC, T); }
                        Vector2 d = _heroPos - _tgt44; float hd = Mathf.Abs(Mathf.DeltaAngle(Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, gapC));
                        _tick44 -= dt; if (_tick44 <= 0 && Mathf.Abs(d.magnitude - r) < T.qDriftWidthU && hd > T.qDriftGapDeg * 0.5f) { _tick44 = 0.3f; Hit44(BossDmg44(T.qDriftDmgMul), "fire_ring", true); }
                        if (_t44 <= 0) { if (_fxA44 < 0) HordeFX.CheapBurst(_tgt44, E32(0xe43b44), 16); EndAtk44(T.qRecoverS); }
                    }
                    break;
                }
                default:
                    if (_ph44 == 0)
                    {
                        _squash = Mathf.MoveTowards(_squash, -0.18f, dt);
                        if (_t44 <= 0)
                        {
                            int n = Random.Range(Mathf.Min(T.qMeteorMin, T.qMeteorMax), Mathf.Max(T.qMeteorMin, T.qMeteorMax) + 1);
                            for (int k = 0; k < n; k++)
                            {
                                Vector2 mp = ClampBox44(k == 0 ? _heroPos : _heroPos + Random.insideUnitCircle * T.qMeteorSpreadU);
                                _drops44.Add(new Drop44 { p = mp, delay = k * 0.08f, tell = T.qMeteorFallS, r = T.qMeteorRadiusU, dmg = BossDmg44(T.qMeteorDmgMul), kind = 1, boss = true, fx = FX44 ? QilinChariotFX.Meteor(mp, k * 0.08f, T.qMeteorRadiusU, T.qMeteorFallS) : -1 });
                            }
                            Play("stomp_strike", true); HellordAudio.Play("oxhead_stomp_strike", 1.1f); ScreenShake.Add(2f, 0.2f); _squash = 0.2f; TellFX.DustPuff(e.pos, 6, 12); _ph44 = 1; _t44 = T.qMeteorAfterS;
                        }
                    }
                    else if (_t44 <= 0) EndAtk44(T.qRecoverS);
                    break;
            }
        }
        // ================= Ice Gorilla PixelLab sheet (boss_ice_gorilla: 80x80 cells, 8 cols x 16 rows) =================
        // v0.0.44 rows: 0 idle, 1-3 walk S/E/N, 4 slam, 5-6 throw S/E, 7 leap, 8-9 slide S/E, 10 roar, 11 throw N, 12 slide N, 13 hurt, 14 stun, 15 death (13-15 S only, used for all facings)
        Sprite[] _gorF44; bool _gorTried44; float _gIdleT44, _gHurtT44, _gHurtCd44, _gHpLast44 = -1;
        static readonly int[] GMsIdle = { 150, 150, 150, 150, 150, 150 }, GMsWalk = { 83, 83, 83, 83, 83, 83, 83, 83 }, GMsRoar = { 150, 170, 280, 280, 200 };
        static readonly int[] GMsHurt = { 60, 90, 110, 120 }, GMsStun = { 130, 130, 130, 130, 130, 130 }, GMsDeath = { 90, 120, 140, 160, 160, 200, 300, 500 };
        Sprite[] GorillaFrames44()
        {
            if (!_gorTried44) { _gorTried44 = true; if (!HArt.Missing("boss_ice_gorilla")) _gorF44 = HArt.Sheet("boss_ice_gorilla"); }
            return _gorF44;
        }
        bool GorillaSheetOn44 => IsGorilla && T44.gorillaSheet && GorillaFrames44() != null && GorillaFrames44().Length >= 128;
        static int MsCol44(int[] ms, float t, bool loop)
        {
            int tot = 0; foreach (int m in ms) tot += m; int x = Mathf.Max(0, (int)(t * 1000f)); if (loop && tot > 0) x %= tot;
            for (int i = 0; i < ms.Length; i++) { if (x < ms[i]) return i; x -= ms[i]; }
            return ms.Length - 1;
        }
        /// <summary>v0.0.44: maps progress p (0..1) onto frames from..to, weighted by the sheet's per-frame ms, so a stretched tell still lands its last frame on the logic moment.</summary>
        static int MsSpan44(int[] ms, int from, int to, float p)
        {
            from = Mathf.Clamp(from, 0, ms.Length - 1); to = Mathf.Clamp(to, from, ms.Length - 1);
            int tot = 0; for (int i = from; i <= to; i++) tot += ms[i];
            float x = Mathf.Clamp01(p) * tot;
            for (int i = from; i <= to; i++) { if (x < ms[i]) return i; x -= ms[i]; }
            return to;
        }
        /// <summary>v0.0.44: the boss root stays up until the PixelLab death row has played and held (Gorilla). Other bosses: true.</summary>
        bool Death44Done() => QilinSheetOn44 ? (!QilinHsd44 || _animT >= QMsDeath.Sum44() / 1000f + T44.qDeathHoldS) : (!GorillaSheetOn44 || _animT >= GMsDeath.Sum44() / 1000f + T44.gDeathHoldS);
        /// <summary>Picks the Gorilla sheet frame from the attack state (tell frames during tells, hit frame on impact, airborne frames in the leap). No-op when not the Gorilla, toggle off or sheet missing (recolour stays).</summary>
        void AnimateGorilla44(float dt, bool faceWest)
        {
            if (_bossBody == null || _boss == null || !GorillaSheetOn44) return;
            var F = GorillaFrames44();
            _gIdleT44 += dt; if (_gTail44 > 0) _gTail44 -= dt; if (_gHurtT44 > 0) _gHurtT44 -= dt; if (_gHurtCd44 > 0) _gHurtCd44 -= dt;
            if (_gHpLast44 >= 0 && _boss.hp < _gHpLast44 && _gHurtCd44 <= 0 && _bs != BS.X44 && !_boss.dying) { _gHurtT44 = GMsHurt.Sum44() / 1000f; _gHurtCd44 = T44.gHurtAnimCooldownS; }
            _gHpLast44 = _boss.hp;
            int row = 0, col = MsCol44(GMsIdle, _gIdleT44, true); bool side = false, west = faceWest;
            float p = 1f - Mathf.Clamp01(_t44 / Mathf.Max(0.05f, _tot44));
            if (_boss.dying) { row = 15; col = MsCol44(GMsDeath, _animT, false); }
            else if (_bs == BS.Stun) { row = 14; col = MsCol44(GMsStun, _animT, true); }
            else if (_bs == BS.X44)
            {
                switch (_atk44)
                {
                    case 'G':
                        row = 4;
                        if (_ph44 == 0) col = p < 0.1f ? 0 : p > 0.94f ? 4 : 1 + Mathf.Min(2, (int)((p - 0.1f) / 0.84f * 3f));
                        else { float k = 0.5f - _t44; col = k < 0.18f ? 5 : k < 0.38f ? 6 : 7; }
                        break;
                    case 'B':
                        side = Mathf.Abs(_dir44.x) >= Mathf.Abs(_dir44.y) * 0.7f; row = side ? 6 : _dir44.y > 0 ? 11 : 5; west = _dir44.x < 0;
                        if (_ph44 == 0) col = p < 0.25f ? 0 : p < 0.45f ? 1 : p < 0.75f ? 2 : 3;
                        else { float k = _tot44 - _t44; col = k < 0.08f ? 4 : k < 0.25f ? 5 : 6; }
                        break;
                    case 'L':
                        row = 7; col = _ph44 == 0 ? (p < 0.35f ? 0 : 1) : 2 + Mathf.Min(2, (int)(p * 3f));
                        break;
                    case 'S':
                        side = GoreSide; row = side ? 9 : _dir44.y > 0 ? 12 : 8; west = _dir44.x < 0;
                        col = _ph44 == 0 ? 0 : 1 + (int)(_gIdleT44 / 0.09f) % 4;
                        break;
                    default:
                        row = 10; col = _ph44 == 0 ? (p < 0.5f ? 0 : 1) : 2 + Mathf.Min(2, (int)((0.4f - _t44) / 0.14f));
                        break;
                }
            }
            else if (_gHurtT44 > 0) { row = 13; col = MsCol44(GMsHurt, GMsHurt.Sum44() / 1000f - _gHurtT44, false); }
            else if (_gTail44 > 0 && _gTailAtk44 == 'L') { row = 7; col = _gTail44 > 0.2f ? 5 : 6; }
            else if (_gTail44 > 0 && _gTailAtk44 == 'S') { side = GoreSide; row = side ? 9 : _chargeDir.y > 0 ? 12 : 8; col = 5; west = _chargeDir.x < 0; }
            else if (_anim.StartsWith("move_"))
            {
                int c = MsCol44(GMsWalk, _animT, true);
                if (_anim.EndsWith("_N")) { row = 3; col = c; } else if (_anim.EndsWith("_S")) { row = 1; col = c; } else { row = 2; col = c; side = true; }
            }
            else if (_anim == "roar" || _anim == "phase_change") { row = 10; col = MsCol44(GMsRoar, _animT, false); }
            bool flipS = row == 0 || row == 4 || row == 7 || row == 10 || row >= 13;
            _bossBody.sprite = F[row * 8 + Mathf.Clamp(col, 0, 7)];
            _bossBody.flipX = side ? west : flipS && faceWest;
        }

        // ================= Qilin Chariot sheet (boss_qilin_chariot: 160x144 cells, 9 cols x 13 rows, VH PixelLab object 27bb782f) =================
        // v0.0.44 rows (from the json): 0 idle S, 1-3 drive E/N/S, 4-6 charge S/E/N, 7-9 stomp S/E/N, 10-12 breath S/E/N, 13-14 hurt S/E, 15-16 stun S/E, 17-18 death S/E (hold f8). W = E flipped.
        Sprite[] _qilF44; bool _qilTried44; float _qIdleT44, _qHurtT44, _qHurtCd44, _qHpLast44 = -1; int _qLastFace44;
        static readonly int[] QMsHurt = { 60, 80, 90, 110 }, QMsStun = { 120, 120, 120, 120, 120, 120, 120, 120 }, QMsDeath = { 90, 70, 80, 90, 110, 130, 150, 200, 400 };
        /// <summary>v0.0.44: the 19-row sheet (hurt / stun / death rows) is loaded.</summary>
        bool QilinHsd44 => QilinFrames44() != null && QilinFrames44().Length >= 171;
        static readonly int[] QMsIdle = { 125, 125, 125, 125, 125, 125, 125, 125 }, QMsDrive = { 80, 80, 80, 80, 80, 80, 80, 80 };
        static readonly int[] QMsCharge = { 100, 200, 260, 70, 70, 70, 70, 120, 120 }, QMsStomp = { 100, 160, 220, 280, 80, 60, 200, 160, 140 };
        static readonly int[] QMsBreath = { 100, 160, 200, 240, 200, 220, 160, 120, 100 }, QMsBreathN = { 100, 160, 200, 240, 200, 220, 180, 120, 100 };
        Sprite[] QilinFrames44()
        {
            if (!_qilTried44) { _qilTried44 = true; if (!HArt.Missing("boss_qilin_chariot")) _qilF44 = HArt.Sheet("boss_qilin_chariot"); }
            return _qilF44;
        }
        bool QilinSheetOn44 => IsQilin && T44.qilinSheet && QilinFrames44() != null && QilinFrames44().Length >= 117 && QilinFrames44()[0] != null && QilinFrames44()[0].rect.width >= 160;
        /// <summary>0 S, 1 E (W = flip), 2 N.</summary>
        static int QFace44(Vector2 d) => Mathf.Abs(d.x) >= Mathf.Abs(d.y) * 0.7f ? 1 : d.y > 0 ? 2 : 0;
        static int QDriveRow44(int f) => f == 1 ? 1 : f == 2 ? 2 : 3;
        /// <summary>v0.0.44: fire-breath origin at the Qilin's mouth for the facing (tunable offsets, x mirrored for W).</summary>
        Vector2 QMouth44(Vector2 pos, Vector2 dir)
        {
            if (!QilinSheetOn44) return pos + Vector2.up * 1.5f;
            int f = QFace44(dir); Vector2 o = (f == 1 ? T44.qMouthE : f == 2 ? T44.qMouthN : T44.qMouthS) * T44.qilinScale46;
            if (f == 1 && dir.x < 0) o.x = -o.x;
            return pos + o;
        }
        void AnimateQilin44(float dt, bool faceWest)
        {
            if (_bossBody == null || _boss == null) return;
            if (!QilinSheetOn44) { if (_bossBody.transform.localScale != Vector3.one) _bossBody.transform.localScale = Vector3.one; return; }
            float qs = Mathf.Max(0.1f, T44.qilinScale46); _bossBody.transform.localScale = new Vector3(qs, qs, 1f); _boss.radius = T44.qilinHitRadiusAt1 * qs; _boss.hitY = T44.qilinHitYAt1 * qs;
            var F = QilinFrames44(); var T = T44; _qIdleT44 += dt;
            if (_qHurtT44 > 0) _qHurtT44 -= dt; if (_qHurtCd44 > 0) _qHurtCd44 -= dt;
            bool hsd = QilinHsd44;
            if (hsd && _qHpLast44 >= 0 && _boss.hp < _qHpLast44 && _qHurtCd44 <= 0 && _bs != BS.X44 && _bs != BS.Stun && !_boss.dying) { _qHurtT44 = QMsHurt.Sum44() / 1000f; _qHurtCd44 = T.qHurtAnimCooldownS; }
            _qHpLast44 = _boss.hp;
            int row = 0, col = MsCol44(QMsIdle, _qIdleT44, true), face = 0; bool west = faceWest;
            float p = 1f - Mathf.Clamp01(_t44 / Mathf.Max(0.05f, _tot44));
            int hf = _qLastFace44 == 1 ? 1 : 0;
            if (_boss.dying && hsd) { row = 17 + hf; face = hf; col = Mathf.Min(8, MsCol44(QMsDeath, _animT, false)); }
            else if (_bs == BS.Stun && hsd) { row = 15 + hf; face = hf; col = MsCol44(QMsStun, _animT, true); }
            else if (_boss.dying || _bs == BS.Stun) { row = 0; }
            else if (_bs == BS.X44)
            {
                face = QFace44(_dir44); west = _dir44.x < 0;
                switch (_atk44)
                {
                    case 'C':
                        row = 4 + (face == 1 ? 1 : face == 2 ? 2 : 0);
                        if (_ph44 == 0) col = MsSpan44(QMsCharge, 0, 2, p);
                        else if (_ph44 == 1) col = 3 + (int)(_qIdleT44 / 0.07f) % 4;
                        else col = MsSpan44(QMsCharge, 7, 8, Mathf.Clamp01((T.qChargeStaggerS - _t44) / 0.24f));
                        break;
                    case 'W':
                        row = 4 + (face == 1 ? 1 : face == 2 ? 2 : 0);
                        col = _ph44 == 0 ? MsSpan44(QMsCharge, 0, 2, p) : MsSpan44(QMsCharge, 7, 8, Mathf.Clamp01((0.4f - _t44) / 0.4f));
                        break;
                    case 'F':
                    {
                        row = 10 + (face == 1 ? 1 : face == 2 ? 2 : 0); var ms = face == 2 ? QMsBreathN : QMsBreath; int rel = face == 2 ? T.qBreathReleaseFN : T.qBreathReleaseF;
                        col = _ph44 == 0 ? MsSpan44(ms, 0, rel - 1, p) : MsSpan44(ms, rel, 8, p);
                        break;
                    }
                    case 'D':
                    {
                        Vector2 d = _ph44 == 0 ? _from44 - _boss.pos : _chargeDir; if (d.sqrMagnitude < 0.01f) d = _dir44;
                        face = QFace44(d); west = d.x < 0; row = _ph44 == 2 ? 0 : QDriveRow44(face);
                        col = row == 0 ? MsCol44(QMsIdle, _qIdleT44, true) : MsCol44(QMsDrive, _qIdleT44, true);
                        break;
                    }
                    default:
                    {
                        row = 7 + (face == 1 ? 1 : face == 2 ? 2 : 0); int hit = face == 2 ? T.qStompHitFN : T.qStompHitF;
                        col = _ph44 == 0 ? MsSpan44(QMsStomp, 0, hit - 1, p) : MsSpan44(QMsStomp, hit, 8, Mathf.Clamp01((T.qMeteorAfterS - _t44) / Mathf.Max(0.05f, T.qMeteorAfterS)));
                        break;
                    }
                }
            }
            else if (_qHurtT44 > 0 && hsd) { row = 13 + hf; face = hf; col = MsCol44(QMsHurt, QMsHurt.Sum44() / 1000f - _qHurtT44, false); }
            else if (_anim.StartsWith("move_"))
            {
                face = _anim.EndsWith("_N") ? 2 : _anim.EndsWith("_S") ? 0 : 1; row = QDriveRow44(face); col = MsCol44(QMsDrive, _animT, true);
            }
            if (row < 13 && row != 0) _qLastFace44 = face;
            if (row >= F.Length / 9) row = 0;
            _bossBody.sprite = F[row * 9 + Mathf.Clamp(col, 0, 8)];
            _bossBody.flipX = (face == 1 || row == 0) && west;
        }

        void DrawFireArc44(Vector2 c, float r, float fromDeg, float sweepDeg, float a)
        {
            int segs = Mathf.Max(1, Mathf.CeilToInt(sweepDeg / 10f));
            for (int k = 0; k < segs; k++)
            {
                float a0 = (fromDeg + sweepDeg * k / segs) * Mathf.Deg2Rad, a1 = (fromDeg + sweepDeg * (k + 1) / segs) * Mathf.Deg2Rad;
                Vector2 p0 = c + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * r, p1 = c + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * r;
                ZSeg(p0, p1, T44.qDriftWidthU * 2f, E32(0xa22633, 0.6f * a), 2); ZSeg(p0, p1, T44.qDriftWidthU, E32(0xe43b44, 0.7f * a), 3);
            }
        }
        void DrawGap44(Vector2 c, float r, float gapDeg, HellordTuning T)
        {
            for (int s = -1; s <= 1; s += 2)
            {
                float a = (gapDeg + s * T.qDriftGapDeg * 0.5f) * Mathf.Deg2Rad; Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                ZSeg(c + d * (r - 0.8f), c + d * (r + 0.8f), 0.15f, E32(0x63c74d, 0.9f), 6);
            }
        }
    }
    static class Ms44Ext
    {
        /// <summary>v0.0.44: total ms of a frame-duration table.</summary>
        public static int Sum44(this int[] ms) { int t = 0; foreach (int m in ms) t += m; return t; }
    }
}
