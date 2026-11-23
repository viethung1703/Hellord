using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    // t1481: FIRE BASIC MAIN ATTACKS - Molten Spit (SkillId.MagmaThrow), Infernal Boulder (SkillId.RollingMagmaRock), Fire Pillar (ids 16-18; the ids keep their old names, only the display names changed).
    // Gungeon-style: the PLAYER fires them. LEFT MOUSE casts, holding it keeps firing at the skill's fire rate, aim = the cursor.
    // They never auto-fire (see FbWant1481). Every number lives in HellordTuning (t1481 section).
    // Visuals: HFX strips via FireBasicsFX1481 (HellordFireBasicsFX1481.cs); this file is gameplay + timing.
    // =====================================================================================================================
    public partial class HellordGame
    {
        public static bool IsFb1481(SkillId id) => id >= SkillId.MagmaThrow && id <= SkillId.FirePillar;
        /// <summary>Fire-rate multiplier on the SkillDef cooldown (1.0) - read by SkillInst.CdMul so the belt cooldown ring matches.</summary>
        public static float FbCd1481(SkillId id)
        {
            if (!IsFb1481(id)) return 1f; var a = HellordTuning.I != null ? HellordTuning.I.fbCd1481 : null; int i = (int)id - (int)SkillId.MagmaThrow;
            return a != null && i < a.Length ? Mathf.Max(0.05f, a[i]) : 1f;
        }
        static float FbDmgMul1481(int ix) { var a = HellordTuning.I.fbDmgMul1481; int i = ix - (int)SkillId.MagmaThrow; return a != null && i >= 0 && i < a.Length ? a[i] : 1f; }

        /// <summary>Does the player want this skill cast this frame? Manual mode: the belt binding (Want716). Otherwise: left mouse HELD (not over the belt). Never true by itself = no auto-fire.</summary>
        bool FbWant1481(SkillInst s)
        {
            if (Manual716) return Want716(s);
            var T = HellordTuning.I; if (!T.fbLmbFire1481 || mode != Mode.Play || _tmOpen) return false;
            return HInput.LmbHeld716() && !MouseOverBelt716() && _pickCool716 <= 0f;
        }
        /// <summary>True when an LMB-slot basic attack is owned (Manual + mouse-move: LMB then fires instead of walking).</summary>
        bool FbLmbSkill1481()
        {
            for (int i = 0; i < NSlot716; i++) { var k = _slot716[i]; if (k != null && IsFb1481(k.def.id) && Bind716((Act716)i) == "LMB") return true; }
            return false;
        }

        // ---------------------------------------------------------------- icons (HA t1481: Resources/HellordArt/FireBasics/icon_*_24.png)
        static readonly Dictionary<SkillId, Texture2D> _fbIcons1481 = new Dictionary<SkillId, Texture2D>();
        static Texture2D FbIcon1481(SkillId id)
        {
            if (_fbIcons1481.TryGetValue(id, out var t)) return t; string n = id == SkillId.MagmaThrow ? "icon_magmathrow_24" : id == SkillId.RollingMagmaRock ? "icon_rollingmagmarock_24" : "icon_firepillar_24";
            t = Resources.Load<Texture2D>("HellordArt/FireBasics/" + n); if (t != null) { t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; } return _fbIcons1481[id] = t;
        }

        // ---------------------------------------------------------------- entities
        class FbPend { public int kind; public float t, distU; public Vector2 dir; public float dmg; public bool west; public SkillInst s; }                       // wind-up (spit gouge) / cast ring (boulder) before the projectile appears
        class FbRock { public Vector2 p, a, b, dir; public float t, dur, age, dmg; public SkillInst s; }                                               // Molten Spit rock (t1511 scripted lob: linear ground point + preset height arch)
        class FbCast { public readonly HashSet<int> shardHit = new HashSet<int>(); public int embers, pops; }                                          // t1514: one Spit cast (shrapnel hit-once set, ember / pop caps)
        class FbStrand { public Vector2 a, b, dir; public float t, dmg, pdmg, speedU, flyS; public int idx, row; public bool landed; public FbCast cast; public SkillInst s; public readonly HashSet<int> hit = new HashSet<int>(); }
        class FbBall { public object loop; public Vector2 p, dir; public float dist, range, age, dmg, trailAcc, z, bandAt; public bool touched; public int tiles, row, seg; public SkillInst s; public readonly HashSet<int> hit = new HashSet<int>(); }
        class FbRecoil { public Enemy e; public Vector2 dir; public float f; public int next; }
        class FbPatch { public Vector2 p; public float rx, ry, life, tick, tickS, dmg, delay; public SkillInst s; }
        readonly List<FbPend> _fbPend = new List<FbPend>(); readonly List<FbRock> _fbRocks = new List<FbRock>(); readonly List<FbStrand> _fbStrands = new List<FbStrand>(); readonly List<FbBall> _fbBalls = new List<FbBall>();
        readonly List<FbRecoil> _fbRecoils = new List<FbRecoil>(); readonly List<FbPatch> _fbPatches = new List<FbPatch>(); readonly List<int> _fbQ = new List<int>(64); int _fbShot, _fbPillars;
        void ClearFb1481() { foreach (var b in _fbBalls) HellordAudio.LoopStop(b.loop, 0.1f); _fbPend.Clear(); _fbRocks.Clear(); _fbStrands.Clear(); _fbBalls.Clear(); _fbRecoils.Clear(); _fbPatches.Clear(); _fbPillars = 0; FireBasicsFX1481.Clear(); }
        // draw order (HFX v3 section 3): floor decals (heat, patches) < band tiles < shadows < imps/hero (YSort) < ball +14 / strand +20 / rock +30 < impact, star, blast +40. YSort adds 1 per px, so "+N" = N px in front.
        // t1644 (VH): fire ground ABOVE every corpse / blood pool / splat at any y, still UNDER every living actor (was -15250 / -14500 = under corpses).
        //  YSort(y) = room px - 432; rooms are <= 864 px tall (-432..432), checked here with the wider +-1000 (-1432..568) for safety.
        //  Fire ground = FbGround + YSort + (-100..+40) -> -9532..-7392; trail (airborne shadows) = FbTrail + YSort + 3 -> -8679..-6679.
        //  Corpse stack = D2Feel1434.OrdPool - 1 .. OrdCorpse + YSort -> at most -14432. Actors = YSort (-20 pickups, -2 outlines) -> at least -1452.
        //  Same 750 step between FbGround and FbTrail as before. The two const checks below stop the build if a value ever breaks that.
        const float FbFps = 30f, FbPx = 16f; const int FbGround = -8000, FbTrail = -7250;
        const int FbYsMin1644 = -1432, FbYsMax1644 = 568;
        const uint FbAboveCorpses1644 = (uint)(FbGround + FbYsMin1644 - 100 - (D2Feel1434.OrdCorpse + FbYsMax1644) - 1); // compile error if fire ground could touch a corpse
        const uint FbUnderActors1644 = (uint)((FbYsMin1644 - 20) - (FbTrail + FbYsMax1644 + 3) - 1);                        // compile error if fire ground / trail could touch an actor
        const int FbDarkBase1644 = -15950; // floor darkening keeps its old place (= old FbGround - 700): above the floor, under corpses and fire decals

        // ---------------------------------------------------------------- aim / helpers
        Vector2 FbAimPoint1481(float range)
        {
            Vector2 at = MouseWorld(); if ((at - _heroPos).magnitude > range) at = _heroPos + (at - _heroPos).normalized * range; ClampToRoom(ref at, 4); return at;
        }
        Vector2 FbAimDir1481() { Vector2 d = MouseWorld() - _heroPos; return d.sqrMagnitude > 0.01f ? d.normalized : (_lastDir.sqrMagnitude > 0.01f ? _lastDir.normalized : Vector2.right); }
        // t1511b declutter: 0 = not a fire-basic hit, 1 = primary (number, no white shards), 2 = secondary (no number, no white shards)
        int _fbHitMode1481;
        bool FbNoNumber1481() => _fbHitMode1481 == 2 && HellordTuning.I.fbDeclutter1481;
        bool FbNoBlob1481() => _fbHitMode1481 != 0 && HellordTuning.I.fbNoHitBlob1481;
        void FbHit1481(Enemy e, float dmg, SkillInst s, Vector2 knock, int mode, bool tick = false) { int o = _fbHitMode1481; _fbHitMode1481 = mode; try { Hit(e, dmg, s, knock, tick); } finally { _fbHitMode1481 = o; } }
        int FbDarkOrd1481(float y) => FbDarkBase1644 + YSort(y); // floor darkening: above the floor (-29xxx), under the fire decals and corpses
        bool FbArea1481(Vector2 at, float r, float dmg, SkillInst s, float knock, int mode = 1, float yScale = 1f)
        {
            Query(at, r, _fbQ); SkillArea53(at, r, dmg, (int)s.def.id); bool any = false;
            foreach (int ei in _fbQ) { var e = _en[ei]; if (!e.alive) continue; Vector2 d = e.pos - at; float dist = d.magnitude; if (dist > r + e.radius) continue; if (yScale < 0.999f) { float nx = d.x / (r + e.radius), ny = d.y / (r * yScale + e.radius); if (nx * nx + ny * ny > 1f) continue; } /* t1514: flattened ellipse */ any = true; FbHit1481(e, dmg, s, (dist > 0.01f ? d / dist : Vector2.up) * knock, mode); }
            return any;
        }
        /// <summary>Burning ground (damage only; the picture is an HFX strip). Ellipse rx / ry (m), life + delay (s), tick (s), tick damage.</summary>
        void FbPatch1481(Vector2 at, float rx, float ry, float life, float delayS, float tickS, float dmg, SkillInst s)
        {
            if (_fbPatches.Count < HellordTuning.I.fbPatchMax1481) _fbPatches.Add(new FbPatch { p = at, rx = rx, ry = ry, life = life * SinDuration737, delay = delayS, tick = tickS * 0.5f, tickS = tickS, dmg = dmg, s = s });
        }
        static float FbLobZ1481(float k, HellordTuning T) => T.msRockZPx1481 * (1f - k) + 4f * T.msArcPeakPx1481 * k * (1f - k); // px above the ground
        static Vector2 FbUnit(float deg, Vector2 d) => (Vector2)(Quaternion.Euler(0, 0, deg) * d);
        static float FbSpread(int i, int n, float deg) => (i - (n - 1) * 0.5f) * (n > 1 ? Mathf.Min(deg, 120f / (n - 1)) : 0f);
        int FbCasts1481(int kind) { int c = 0; foreach (var p in _fbPend) if (p.kind == kind) c++; return c + (kind == 0 ? _fbRocks.Count : _fbBalls.Count); }

        // ---------------------------------------------------------------- MOLTEN SPIT (SkillId.MagmaThrow)
        // t0: gouge wind-up on the caster (msWindupF, the 16-frame strip stretched, flip X for west); then the rock leaves the hand on a scripted lob (linear ground point + height arch) and lands on the aimed point.
        bool CastMagmaThrow1481(SkillInst s, float dmg)
        {
            if (mode == Mode.StartPick) return false; var T = HellordTuning.I; EnsureFX44();
            if (FbCasts1481(0) >= T.msMaxCasts1481) { s.cd = 0.1f; return false; }
            int n = 1 + PCount49; Vector2 d0 = FbAimDir1481(); bool west = d0.x < 0f; float distU = Mathf.Clamp((MouseWorld() - _heroPos).magnitude * FbPx, T.msRangeMinPx1481, T.msRangeMaxPx1481) / FbPx; // aimed point, clamped to the range
            for (int i = 0; i < n; i++) _fbPend.Add(new FbPend { kind = 0, t = 0f, distU = distU, dir = FbUnit(FbSpread(i, n, T.msSpreadDeg1481), d0), dmg = dmg, west = west, s = s });
            return true;
        }
        void FbRockRelease1481(FbPend p)
        {
            var T = HellordTuning.I; float side = p.west ? -1f : 1f; Vector2 hand = _heroPos + new Vector2(side * T.msHandXPx1481 / FbPx, 0f); // hand = pivot + 9 px toward the target, lifted 11 px (drawn)
            Vector2 target = _heroPos + p.dir * p.distU; ClampToRoom(ref target, 4); // lands exactly here (the aimed point, from the hero's feet at release)
            float nF = T.msFlightPxPerF1481 > 0f ? Mathf.Clamp(Mathf.Round((target - hand).magnitude * FbPx / T.msFlightPxPerF1481), T.msFlightMinF1481, T.msFlightMaxF1481) : T.msFlightF1481; // HFX v4: N = clamp(round(dist / 4.75), 14, 26)
            _fbRocks.Add(new FbRock { p = hand, a = hand, b = target, dir = p.dir, dur = Mathf.Max(2f, nF) / FbFps, dmg = p.dmg, s = p.s }); HellordAudio.Play("magma_throw_cast"); // ONE cast sound per throw, at the release
        }
        void FbSpitImpact1481(FbRock r)
        {
            var T = HellordTuning.I; Vector2 at = r.p; ClampToRoom(ref at, 4); int ord = YSort(at.y); var s = r.s; float dmg = r.dmg;
            FireBasicsFX1481.Once("molten_spit_impact", at, ord + 40, 0f, T.msImpactSpeed1481, 0, false, 0f, T.msBlastSx1481, T.msBlastSy1481); // HFX v5 impact 71x44 (42x27 blob), 7 f: 2 core + 5 fade, native
            var cast = new FbCast { embers = 1 };
            if (T.fbDark1481 && T.msDarkArt1481 && FireBasicsFX1481.Frames("molten_spit_darken") > 0) FireBasicsFX1481.Once("molten_spit_darken", at, FbDarkOrd1481(at.y), 0f, 1f, 0, false, T.msDarkArtDelayF1481, T.msDarkArtSx1481, T.msDarkArtSy1481); // HFX v5 darken b0-5
            else FireBasicsFX1481.Dark(at + r.dir * (T.msDarkFwdPx1481 / FbPx), T.msDarkRxPx1481, T.msDarkRyPx1481, T.msDarkAlpha1481, T.msDarkHoldS1481, FbDarkOrd1481(at.y));
            /* t1511b */ ScreenShake.Add(2, 0.1f); HellordAudio.Play("magma_throw_land");
            After(T.msImpactDelayF1481 / FbFps, () => { if (mode != Mode.StartPick) FbArea1481(at, s.RA(T.msHitRU1481), dmg * T.msImpactMul1481, s, T.msKnock1481, 1, T.msHitYScale1481); }); // t1514: flattened ellipse, the only hit with a number
            Vector2 tg = new Vector2(r.dir.x, r.dir.y * T.msStrandYScale1481); Vector2 b2 = at + tg * (T.msB2OutPx1481 / FbPx); ClampToRoom(ref b2, 4);
            After(T.msB2DelayF1481 / FbFps, () =>
            {   // t1514 B2: one medium burst ~20 px, 42 px out along the throw, small damage, no number
                if (mode == Mode.StartPick) return; FireBasicsFX1481.Once("molten_spit_burst_med", b2, YSort(b2.y) + 40, 0f, T.msB2Speed1481, 0, false, 0f, T.msB2Scale1481, T.msB2Scale1481); // HFX v5 burst_med (~20 px), 6 f
                HellordAudio.Play("magma_throw_splat"); FbArea1481(b2, s.RA(T.msB2RU1481), dmg * T.msB2Mul1481, s, 0f, 2, T.msHitYScale1481);
            });
            float area = s.RA(1f);
            if (T.msHeatS1481 > 0f) FbPatch1481(at, T.msHeatRxU1481 * area, T.msHeatRyU1481 * area, T.msHeatS1481, T.msHeatDelayF1481 / FbFps, T.msPatchTickS1481, dmg * T.msPatchTickMul1481, s); // HFX v4 dropped the heat haze (msHeatS 0 = off)
            FireBasicsFX1481.Decal("molten_spit_scorch", FireBasicsFX1481.DecalKind.SpitScorch, at, FbGround + ord, T.msScorchDelayF1481);                    // HFX v5 scorch: 6 stages x 6 f from b2
            FireBasicsFX1481.Once("molten_spit_ember_patch_v1", at + FbPxOff1481(T.msEmber1OffPx1481), FbGround + ord + 4, 0f, 1f, T.msEmberMax1481, false, T.msEmberDelayF1481);  // HFX v5: ember v1 at b3 (-5,+1), v2 at b4 (+6,-1), 30 f once
            FireBasicsFX1481.Once("molten_spit_ember_patch_v2", at + FbPxOff1481(T.msEmber2OffPx1481), FbGround + ord + 4, 0f, 1f, T.msEmberMax1481, false, T.msEmber2DelayF1481); cast.embers = 2;
            FbPatch1481(at, T.msEmberRU1481 * area, T.msEmberRU1481 * area, T.msEmberS1481, T.msEmberDelayF1481 / FbFps, T.msPatchTickS1481, dmg * T.msPatchTickMul1481, s);
            int n = Mathf.Min(T.msStrands1481, Mathf.Min(T.msStrandAngle1481.Length, T.msStrandReachPx1481.Length)); var sm = T.msShardSpeedMul1481; var ff = T.msShardFlightF1481;
            for (int i = 0; i < n; i++)
            {   // 8 shrapnel: cone table, reach table (hitbox, dc1090b), ground y x0.7, launch at b2; flight frames per piece from HFX v5 (else base speed x multiplier)
                Vector2 d = FbUnit(T.msStrandAngle1481[i], r.dir); Vector2 gd = new Vector2(d.x, d.y * T.msStrandYScale1481); Vector2 b = at + gd * (T.msStrandReachPx1481[i] / FbPx); ClampToRoom(ref b, 4);
                Vector2 a0 = at + gd * (T.msStrandStartPx1481 / FbPx); Vector2 sd = b - at; float spd = ff != null && i < ff.Length && ff[i] > 0f ? (b - a0).magnitude / (ff[i] / FbFps) : Mathf.Max(1f, T.msShardSpeedPx1481 * (sm != null && i < sm.Length ? sm[i] : 1f)) / FbPx; // constant speed per piece (u/s)
                _fbStrands.Add(new FbStrand { a = a0, b = b, dir = sd, idx = i, cast = cast, speedU = spd, flyS = Mathf.Max(1f / FbFps, (b - a0).magnitude / spd), row = FireBasicsFX1481.DirRow(sd), dmg = dmg * T.msStrandMul1481, pdmg = dmg * T.msPatchTickMul1481, s = s, t = -(T.msStrandStartF1481 + (i & 1) * T.msStrandOddF1481) / FbFps });
            }
        }
        void FbStrandLand1481(FbStrand st)
        {
            var T = HellordTuning.I; st.landed = true; Vector2 b = st.b; int ord = YSort(b.y); HellordAudio.Play("magma_throw_splat");
            var c = st.cast; bool pop = st.idx != T.msPopSkipIdx1481 && (c == null || c.pops < T.msPopMax1481);
            if (pop) { if (c != null) c.pops++; FireBasicsFX1481.Once((st.idx & 1) == 0 ? "molten_spit_pop_v1" : "molten_spit_pop_v2", b, ord + 40, 0f, 1f, 0, false, 0f, T.msPopScale1481, T.msPopScale1481); } // HFX v5 small pop on 7 landings (all but index 4), v1 even / v2 odd, 3 f
            int ek = T.msEmberShards1481 != null ? System.Array.IndexOf(T.msEmberShards1481, st.idx) : -1;
            if (ek >= 0 && (c == null || c.embers < T.msEmberCastMax1481))
            {   // HFX v5: shards 0, 3, 5, 7 leave an ember patch (alternating v1 / v2, 30 f once); <= msEmberCastMax per cast incl. the 2 impact ones. Damage patch 1.5 s (gameplay, dc1090b)
                if (c != null) c.embers++; FireBasicsFX1481.Once("molten_spit_ember_patch_v" + (1 + (ek & 1)), b, FbGround + ord + 4, 0f, 1f, T.msEmberMax1481);
                FbPatch1481(b, st.s.RA(T.msEmberRU1481), st.s.RA(T.msEmberRU1481), T.msEmberS1481, 0f, T.msPatchTickS1481, st.pdmg, st.s);
            }
        }

        static Vector2 FbPxOff1481(Vector2 px) => new Vector2(px.x, -px.y) / FbPx; // HFX px offset (y down) -> world

        // ---------------------------------------------------------------- INFERNAL BOULDER (SkillId.RollingMagmaRock)
        // t0: cast ring on the caster (24 f); then the ball drops in 12 px in front on a small scripted arc with one tiny bounce and rolls (D2R pace, rrSpeedU / rrRangePx, ease-out over the last rrEaseFrac) until the range or a wall.
        bool CastRollingRock1481(SkillInst s, float dmg)
        {
            if (mode == Mode.StartPick) return false; var T = HellordTuning.I; EnsureFX44();
            if (FbCasts1481(1) >= T.rrMaxRocks1481) { s.cd = 0.1f; return false; }
            Vector2 dir = FbAimDir1481(); int n = 1 + PCount49; float spread = T.rrSpreadDeg1481;
            _fbPend.Add(new FbPend { kind = 1, t = 0f, dir = dir, dmg = dmg, s = s });
            for (int i = 1; i < n; i++) _fbPend.Add(new FbPend { kind = 2, t = 0f, dir = FbUnit(FbSpread(i, n, spread), dir), dmg = dmg, s = s }); // extra balls share the one ring (kind 2 draws nothing)
            return true;
        }
        void FbBallLaunch1481(FbPend p)
        {
            var T = HellordTuning.I; Vector2 dir = p.dir; Vector2 at = _heroPos + dir * (T.rrStartPx1481 / FbPx); ClampToRoom(ref at, 4);
            var b = new FbBall { p = at, dir = dir, range = T.rrRangePx1481 / FbPx * PRange49, z = T.rrDropPx1481, dmg = p.dmg, s = p.s, row = FireBasicsFX1481.DirRow(dir),
                loop = HellordAudio.LoopStart("Audio/SFX/firebasics1481/magma_rock_roll_loop", Random.Range(T.rrLoopVolMin1481, T.rrLoopVolMax1481), 1f + Random.Range(-T.rrLoopPitchSpread1481, T.rrLoopPitchSpread1481)) }; // one roll voice per ball
            _fbBalls.Add(b); AtProj1651++; /* t1651 probe */ HellordAudio.Play("magma_rock_launch");
            if (!T.rrBandFromTouch1528) for (int i = T.rrBandPre1481; i >= 1; i--) FbBandTile1481(b, at - dir * ((i * T.rrBandGapPx1481 + T.rrBandBehindPx1481) / FbPx), -i); // HFX v4: segments -2, -1 laid at launch behind the spawn // 4 tiles behind at -24..-6 px: the band starts ~1.3 balls long
        }
        void FbBandTile1481(FbBall b, Vector2 at, int seg)
        {   // HFX v4: band / band_b alternate by segment parity, life_idx(age, 60, ign 4, loop 12) with phase = seg x 5 mod 12, y-sorted with units
            var T = HellordTuning.I; int oy = YSort(at.y); string key = (seg & 1) == 0 || FireBasicsFX1481.Frames("infernal_boulder_band_b") == 0 ? "infernal_boulder_band" : "infernal_boulder_band_b";
            FireBasicsFX1481.Tile(key, at, oy + T.rrBandSort1481, T.rrBandS1481 * SinDuration737, 0f, T.rrBandMax1481, T.rrBandIgnF1481, ((seg * T.rrBandPhaseStep1481) % 12 + 12) % 12);
            if (T.rrDarkBandEvery1481 > 0 && b.tiles % T.rrDarkBandEvery1481 == 0) FireBasicsFX1481.Dark(at, T.rrDarkBandRxPx1481, T.rrDarkBandRyPx1481, T.rrDarkBandAlpha1481, Mathf.Max(0f, T.rrBandS1481 * SinDuration737 - T.fbDarkOutS1481), FbDarkOrd1481(at.y)); // t1511b darkening along the band
            if ((b.tiles++ & 1) == 0) FbPatch1481(at, b.s.RA(T.rrBandRU1481), b.s.RA(T.rrBandRU1481), T.rrBandS1481, 0f, T.rrBandTickS1481, b.dmg * T.rrBandTickMul1481, b.s);
        }
        void FbBallEnd1481(FbBall b)
        {
            var T = HellordTuning.I; Vector2 at = b.p; ClampToRoom(ref at, 5); int ord = YSort(at.y); float area = b.s.RA(1f);
            FbArea1481(at, T.rrBlastRU1481 * area, b.dmg * T.rrBlastMul1481, b.s, 0f); ScreenShake.Add(2, 0.12f);
            FireBasicsFX1481.Once("infernal_boulder_end_blast", at, ord + 40); FireBasicsFX1481.Dark(at, T.rrDarkRxPx1481, T.rrDarkRyPx1481, T.rrDarkAlpha1481, T.rrDarkHoldS1481, FbDarkOrd1481(at.y)); // t1511b                                                                         // flash, chunks, sparks (30 f)
            FireBasicsFX1481.Tile("infernal_boulder_fire_patch", at, FbGround + ord, T.rrPatchS1481 * SinDuration737, T.rrPatchDelayF1481, 0, T.rrPatchIgnF1481);              // fire patch from blast + 2 f, 75 f
            FbPatch1481(at, T.rrPatchRxU1481 * area, T.rrPatchRyU1481 * area, T.rrPatchS1481, T.rrPatchDelayF1481 / FbFps, T.rrPatchTickS1481, b.dmg * T.rrPatchTickMul1481, b.s);
            float fade = Random.Range(T.rrLoopFadeMin1481, T.rrLoopFadeMax1481); HellordAudio.LoopStop(b.loop, fade); HellordAudio.PlayDelayed("magma_rock_stop", fade); // fade the roll out, then the stop
        }
        bool FbWall1481(Vector2 p)
        {
            if (_room == null) return false; _room.Walkable(6, out var lo, out var hi); return p.x < lo.x || p.x > hi.x || p.y < lo.y || p.y > hi.y; // room bounds only (like the frost orbs)
        }

        // ---------------------------------------------------------------- FIRE PILLAR (unchanged by HFX v2)
        bool CastFirePillar1481(SkillInst s, float dmg)
        {
            if (mode == Mode.StartPick) return false; var T = HellordTuning.I; EnsureFX44();
            if (_fbPillars >= T.fpMax1481) { s.cd = 0.1f; return false; } // HFX cap: <= 3 pillars alive
            Vector2 at = FbAimPoint1481(T.fpRangeU1481); float r = s.R(HData.At(T.fpRadiusU1481, s.level, 1.2f)); float dd = dmg * T.fpDmgMul1481; var sk = s; float tell = Mathf.Max(0.05f, T.fpTellS1481); int ord = YSort(at.y); int shot = _fbShot++; _fbPillars++;
            FireBasicsFX1481.Once("fire_pillar_telegraph", at, FbGround + ord + 5, 0f, (FireBasicsFX1481.Frames("fire_pillar_telegraph") / FbFps) / tell); // telegraph re-timed to the code's tell seconds
            HellordAudio.Play("fire_pillar_telegraph");
            After(tell, () =>
            {
                if (mode == Mode.StartPick) { _fbPillars = Mathf.Max(0, _fbPillars - 1); return; }
                FireBasicsFX1481.Once("fire_pillar_erupt", at, ord + 30); FireBasicsFX1481.Decal("fire_pillar_scorch_v" + (shot % 2 + 1), FireBasicsFX1481.DecalKind.PillarScorch, at, FbGround + ord, 2f);
                HellordAudio.Play("fire_pillar_erupt"); ScreenShake.Add(2, 0.12f);
                After(T.fpHitF1481 / FbFps, () => { dd *= PactTrailEmp53(at); FbArea1481(at, r, dd, sk, T.fpKnock1481); }); // HFX hit-test window f3-f12
                After(18f / FbFps, () => _fbPillars = Mathf.Max(0, _fbPillars - 1));
            });
            return true;
        }

        // ---------------------------------------------------------------- per-frame update (called after UpdateIce)
        void UpdateFireBasics1481(float dt)
        {
            FireBasicsFX1481.Frame(); if (_fbPend.Count + _fbRocks.Count + _fbStrands.Count + _fbBalls.Count + _fbRecoils.Count + _fbPatches.Count == 0) return; var T = HellordTuning.I;
            // ---- wind-up (gouge) / cast ring: follow the caster, release the projectile when the strip is done
                        for (int i = _fbPend.Count - 1; i >= 0; i--)
            {
                if (i >= _fbPend.Count) continue; var p = _fbPend[i]; p.t += dt; float fr = p.t * FbFps; int ho = YSort(_heroPos.y);
                if (p.kind == 0)
                {
                    if (fr >= T.msWindupF1481) { _fbPend.RemoveAt(i); FbRockRelease1481(p); continue; }
                    FireBasicsFX1481.Show("molten_spit_gouge", _heroPos, Mathf.Min(FireBasicsFX1481.Frames("molten_spit_gouge") - 1, (int)(fr * FireBasicsFX1481.Frames("molten_spit_gouge") / Mathf.Max(1f, T.msWindupF1481))), ho + 20, 0, p.west); // strip stretched over the wind-up
                }
                else
                {
                    if (fr >= T.rrLaunchF1481) { _fbPend.RemoveAt(i); if (p.kind == 1) FbBallLaunch1481(p); else FbBallLaunch1481(p); continue; }
                    if (p.kind == 1) { FireBasicsFX1481.Show("infernal_boulder_cast_ring", _heroPos, (int)(fr * FireBasicsFX1481.Frames("infernal_boulder_cast_ring") / Mathf.Max(1f, T.rrLaunchF1481)), ho + 16); } // t1517: the 24 f ring compressed into the quick launch flash
                }
            }
            // ---- Molten Spit rocks: scripted lob. Ground point linear hand -> target, height = hand height (1-k) + 4 peak k(1-k); lands exactly on the target or bursts early on an enemy when low
            for (int i = _fbRocks.Count - 1; i >= 0; i--)
            {
                if (i >= _fbRocks.Count) continue; var r = _fbRocks[i]; r.age += dt; r.t += dt; float k = Mathf.Clamp01(r.t / r.dur); r.p = Vector2.Lerp(r.a, r.b, k);
                float z = FbLobZ1481(k, T); bool boom = k >= 1f;
                if (!boom && z <= T.msLowZPx1481) { Query(r.p, T.msTouchRU1481 + 0.6f, _fbQ); foreach (int ei in _fbQ) { var e = _en[ei]; if (e.alive && (e.pos - r.p).magnitude <= T.msTouchRU1481 + e.radius) { boom = true; break; } } }
                if (boom) { if (k >= 1f) r.p = r.b; _fbRocks.RemoveAt(i); FbSpitImpact1481(r); continue; }
                float dk = (1f / FbFps) / r.dur, k2 = Mathf.Min(1f, k + dk), k1 = k2 - dk; // screen velocity incl. the height -> direction row (the tail trails the arc)
                Vector2 v = (Vector2.Lerp(r.a, r.b, k2) + Vector2.up * (FbLobZ1481(k2, T) / FbPx)) - (Vector2.Lerp(r.a, r.b, k1) + Vector2.up * (FbLobZ1481(k1, T) / FbPx));
                int ord = YSort(r.p.y); float zMax = Mathf.Max(1f, T.msRockZPx1481 + T.msArcPeakPx1481), hz = Mathf.Clamp01(z / zMax);
                FireBasicsFX1481.Show("molten_spit_rock_dir16", r.p + Vector2.up * (z / FbPx), (int)(r.age * FbFps) % Mathf.Max(1, FireBasicsFX1481.Frames("molten_spit_rock_dir16")), ord + 30 + (int)(z * T.msRockSortZ1481), FireBasicsFX1481.DirRow(v));   // sort = ground y + 30 + height: an airborne rock is never hidden by imps
                int shF = T.msShadowZ1481 != null && T.msShadowZ1481.Length >= 2 ? (z < T.msShadowZ1481[0] ? 0 : z < T.msShadowZ1481[1] ? 1 : 2) : 0; // HFX v4 shadow frame by height
                FireBasicsFX1481.Show("molten_spit_shadow", r.p, Mathf.Min(shF, FireBasicsFX1481.Frames("molten_spit_shadow") - 1), FbTrail + ord + 3, 0, false, Mathf.Lerp(1f, T.msShadowPeakScale1481, hz), Mathf.Lerp(1f, T.msShadowPeakAlpha1481, hz)); // shadow stays on the ground, smaller / fainter when high
            }
            // ---- strands: eased 10 f flight (ground y x0.75, lifted 6 px), then 8 fade frames at the tip; a 6 px hit along the flight
            for (int i = _fbStrands.Count - 1; i >= 0; i--)
            {
                if (i >= _fbStrands.Count) continue; var st = _fbStrands[i]; st.t += dt; if (st.t < 0f) continue; float flight = st.flyS; Vector2 lift = Vector2.up * (T.msStrandLiftPx1481 / FbPx);
                if (!st.landed && st.t < flight)
                {   // t1514: constant speed per piece (190-210 px/s +-30 %), frames 0-7 spread over the flight; non-piercing: stops at the first enemy body
                    float k = st.t / flight; Vector2 g = Vector2.Lerp(st.a, st.b, k); int ord = YSort(g.y);
                    Query(g, T.msStrandRU1481 + 0.6f, _fbQ); bool stop = false;
                    foreach (int ei in _fbQ)
                    {
                        var e = _en[ei]; if (!e.alive || st.hit.Contains(e.id) || (e.pos - g).magnitude > T.msStrandRU1481 + e.radius * 0.5f) continue;
                        st.hit.Add(e.id); bool fresh = st.cast == null || st.cast.shardHit.Add(e.id); if (fresh) FbHit1481(e, st.dmg, st.s, Vector2.zero, 2); // max 1 shrapnel hit per enemy per cast, no knock-back
                        if (!T.msShardPierce1481) { stop = true; break; }
                    }
                    if (stop) { st.b = g; st.t = flight; FbStrandLand1481(st); }
                    else { int tf = (int)(st.t * FbFps); FireBasicsFX1481.Show("molten_spit_strand_dir16", g + lift * (1f - k), tf < 2 ? tf : 2 + (tf - 2) % 4, ord + T.msStrandSortAir1481, st.row); continue; } // HFX v5: f0-1 launch, f2-5 flight loop; head pivot
                }
                if (!st.landed) FbStrandLand1481(st);
                int fa = (int)((st.t - flight) * FbFps * 18f / Mathf.Max(1f, T.msShardFadeF1481)); if (fa > 17) { _fbStrands.RemoveAt(i); continue; }
                FireBasicsFX1481.Show("molten_spit_strand_dir16", st.b, 6 + fa, YSort(st.b.y) + T.msStrandSortLand1481, st.row);   // HFX v5 landed fade f6-23 (18 f)
            }
            // ---- Infernal Boulder balls: scripted launch drop + tiny bounce, then a roll at the D2R pace with an ease-out over the last part of the range
            for (int i = _fbBalls.Count - 1; i >= 0; i--)
            {
                if (i >= _fbBalls.Count) continue; var o = _fbBalls[i]; o.age += dt; float fA = o.age * FbFps, dropF = Mathf.Max(1f, T.rrDropF1481), bounceF = Mathf.Max(1f, T.rrBounceF1481);
                if (fA < dropF) { float k = fA / dropF; o.z = T.rrDropPx1481 * (1f - k * k) + T.rrDropArcPx1481 * 4f * k * (1f - k); }   // falls from rrDropPx with a small toss
                else if (fA < dropF + bounceF) { float k = (fA - dropF) / bounceF; o.z = T.rrBouncePx1481 * 4f * k * (1f - k); }          // one tiny bounce
                else o.z = 0f;
                float rem = o.range - o.dist, easeU = Mathf.Max(0.01f, T.rrEaseFrac1481 * o.range), vMul = rem < easeU ? Mathf.Lerp(T.rrEaseMinV1481, 1f, rem / easeU) : 1f;
                float step = Mathf.Min(rem, T.rrSpeedU1481 * PSpeed49 * vMul * dt); Vector2 prev = o.p; o.p += o.dir * step; o.dist += step; bool wall = FbWall1481(o.p);
                float sc = Mathf.Max(0.1f, T.rrBallScale1481), hitR = T.rrBallU1481 * 0.5f * sc; Query(o.p, hitR + 0.6f, _fbQ);
                foreach (int ei in _fbQ)
                {
                    var e = _en[ei]; if (!e.alive || o.hit.Contains(e.id) || (e.pos - o.p).magnitude > hitR + e.radius) continue;
                    o.hit.Add(e.id); FbHit1481(e, o.dmg, o.s, Vector2.zero, 1); HellordAudio.Play("magma_rock_hit"); _fbRecoils.Add(new FbRecoil { e = e, dir = o.dir }); // knock-back = the HFX recoil table along the roll direction
                }
                if (T.rrBandFromTouch1528)
                {   // t1528 v2: no fire under the caster; the band starts where the ball first touches down and runs unbroken behind it (bounce included)
                    if (!o.touched && fA >= dropF) { o.touched = true; o.bandAt = o.dist; }
                    if (o.touched) { float gap = Mathf.Max(0.05f, T.rrBandGapPx1481 / FbPx), lim = Mathf.Max(o.bandAt == o.dist ? o.dist : 0f, o.dist - T.rrBandBehindPx1481 / FbPx); int guard = 0; while (o.bandAt <= lim + 0.0001f && guard++ < 64) { FbBandTile1481(o, o.p - o.dir * (o.dist - o.bandAt), o.seg++); o.bandAt += gap; } }
                }
                else if (o.z <= 0.5f) { o.trailAcc += (o.p - prev).magnitude; float gap = T.rrBandGapPx1481 / FbPx; while (o.trailAcc >= gap) { o.trailAcc -= gap; FbBandTile1481(o, o.p - o.dir * (T.rrBandBehindPx1481 / FbPx), o.seg++); } } // the band only burns where the ball touches the ground
                if (wall || o.age >= T.rrMaxS1481 || o.dist >= o.range - 0.001f) { _fbBalls.RemoveAt(i); FbBallEnd1481(o); continue; }
                HellordAudio.LoopBeat(o.loop); int ro = YSort(o.p.y);
                FireBasicsFX1481.Show("infernal_boulder_roll_dir16", o.p + Vector2.up * ((T.rrCentrePx1481 * sc + o.z) / FbPx), ((int)(o.dist * FbPx / (T.rrRollPxPerFrame1481 * sc))) % 32, ro + 14, o.row, false, sc);  // roll tied to travel (bigger ball turns slower)
                float hz = Mathf.Clamp01(o.z / Mathf.Max(1f, T.rrDropPx1481 + T.rrDropArcPx1481));
                FireBasicsFX1481.Show("infernal_boulder_shadow", o.p + Vector2.up * (T.rrShadowPx1481 * sc / FbPx), 0, FbTrail + ro + 3, 0, false, sc * Mathf.Lerp(1f, T.rrShadowHighScale1481, hz), Mathf.Lerp(1f, T.rrShadowHighAlpha1481, hz));
            }
            // ---- recoil table (px / frame) on enemies the ball touched
            for (int i = _fbRecoils.Count - 1; i >= 0; i--)
            {
                var rc = _fbRecoils[i]; rc.f += dt * FbFps; var tab = T.rrRecoilPx1481; int upto = Mathf.Min(tab.Length, (int)rc.f + 1);
                while (rc.next < upto) { if (rc.e != null && rc.e.alive) rc.e.pos += rc.dir * (tab[rc.next] / FbPx); rc.next++; }
                if (rc.next >= tab.Length || rc.e == null || !rc.e.alive) _fbRecoils.RemoveAt(i);
            }
            // ---- burning ground (damage only; the pictures are the HFX strips)
            for (int i = _fbPatches.Count - 1; i >= 0; i--)
            {
                if (i >= _fbPatches.Count) continue; var p = _fbPatches[i]; if (p.delay > 0f) { p.delay -= dt; continue; } p.life -= dt; if (p.life <= 0f) { _fbPatches.RemoveAt(i); continue; }
                p.tick -= dt;
                if (p.tick <= 0f)
                {
                    p.tick = p.tickS; Query(p.p, p.rx + 0.6f, _fbQ);
                    foreach (int ei in _fbQ) { var e = _en[ei]; if (!e.alive) continue; Vector2 d = e.pos - p.p; float nx = d.x / (p.rx + e.radius * 0.5f), ny = d.y / (p.ry + e.radius * 0.5f); if (nx * nx + ny * ny <= 1f) FbHit1481(e, p.dmg, p.s, Vector2.zero, 2, true); }
                }
            }
        }
    }
}
