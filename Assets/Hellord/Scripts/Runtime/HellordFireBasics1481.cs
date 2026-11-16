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
        class FbGlob { public Vector2 a, b, dir; public float t, dur, arcPx, startZPx, dmg, r, age; public int shot, lastFr; public SkillInst s; }
        class FbShard { public Vector2 a, b; public float t, dur, age, scarAcc, dmg, patchDmg, r; public int idx, shot; public SkillInst s; }
        class FbRock { public object loop; public Vector2 p, dir; public float dist, range, dmg, trailAcc, age, r, trailDmg; public int tiles, tufts, row; public SkillInst s; public readonly HashSet<int> hit = new HashSet<int>(); }
        class FbPatch { public Vector2 p; public float r, life, tick, tickS, dmg; public SkillInst s; }
        readonly List<FbGlob> _fbGlobs = new List<FbGlob>(); readonly List<FbShard> _fbShards = new List<FbShard>(); readonly List<FbRock> _fbRocks = new List<FbRock>(); readonly List<FbPatch> _fbPatches = new List<FbPatch>(); readonly List<int> _fbQ = new List<int>(64); int _fbShot, _fbPillars;
        void ClearFb1481() { foreach (var r in _fbRocks) HellordAudio.LoopStop(r.loop, 0.1f); _fbGlobs.Clear(); _fbShards.Clear(); _fbRocks.Clear(); _fbPatches.Clear(); _fbPillars = 0; FireBasicsFX1481.Clear(); }
        // draw order (HFX v2 section 5): decals < trail tiles < imps/hero (YSort) < rock +12 / glob +30 / shard +20 < burst, crumble, hit +40. YSort adds 1 per px, so "+N" = N px in front.
        const float FbFps = 30f, FbPx = 16f; const int FbGround = -15250, FbTrail = -14500; // floor decals above the floor, trail tiles above decals, both under every actor (see D2Feel1434 OrdPool)

        // ---------------------------------------------------------------- aim
        Vector2 FbAimPoint1481(float range)
        {
            Vector2 at = MouseWorld(); if ((at - _heroPos).magnitude > range) at = _heroPos + (at - _heroPos).normalized * range; ClampToRoom(ref at, 4); return at;
        }
        Vector2 FbAimDir1481() { Vector2 d = MouseWorld() - HeroCenter; return d.sqrMagnitude > 0.01f ? d.normalized : (_lastDir.sqrMagnitude > 0.01f ? _lastDir.normalized : Vector2.right); }
        bool FbArea1481(Vector2 at, float r, float dmg, SkillInst s, float knock)
        {
            Query(at, r, _fbQ); SkillArea53(at, r, dmg, (int)s.def.id); bool any = false;
            foreach (int ei in _fbQ) { var e = _en[ei]; if (!e.alive) continue; Vector2 d = e.pos - at; float dist = d.magnitude; if (dist > r + e.radius) continue; any = true; Hit(e, dmg, s, (dist > 0.01f ? d / dist : Vector2.up) * knock); }
            return any;
        }
        void FbPatch1481(Vector2 at, float r, float life, float tickS, float dmg, SkillInst s) { if (_fbPatches.Count < HellordTuning.I.rrTrailMax1481) _fbPatches.Add(new FbPatch { p = at, r = r, life = life * SinDuration737, tick = tickS * 0.5f, tickS = tickS, dmg = dmg, s = s }); }
        static Vector2 FbUnit(float deg, Vector2 d) => (Vector2)(Quaternion.Euler(0, 0, deg) * d);

        // ---------------------------------------------------------------- MOLTEN SPIT (SkillId.MagmaThrow)
        // glob screen position at lob progress k (ground point + height above it)
        static Vector2 FbGlobScreen(FbGlob g, float k, float startZ) { float z = startZ * (1f - k) + g.arcPx * 4f * k * (1f - k); return Vector2.Lerp(g.a, g.b, k) + Vector2.up * (z / FbPx); }
        bool CastMagmaThrow1481(SkillInst s, float dmg)
        {
            if (mode == Mode.StartPick) return false; var T = HellordTuning.I; EnsureFX44();
            int inFlight = 0; foreach (var g0 in _fbGlobs) inFlight++; if (inFlight >= T.mtMaxGlobs1481) { s.cd = 0.1f; return false; } // HFX cap: <= 3 globs in flight
            Vector2 aim = FbAimPoint1481(T.mtRangeU1481); int n = 1 + PCount49; float side = aim.x >= _heroPos.x ? 1f : -1f;
            Vector2 hand = _heroPos + new Vector2(side * T.msHandXPx1481 / FbPx, T.msHandYPx1481 / FbPx); // HFX: hand 10 px toward the cursor, 4 px in y, 13 px above the feet
            float landR = HData.At(T.mtLandR1481, s.level, 1f) * s.AreaMul; float a0 = Random.value * 360f;
            for (int i = 0; i < n; i++)
            {
                Vector2 b = i == 0 ? aim : aim + (Vector2)(Quaternion.Euler(0, 0, a0 + i * 360f / Mathf.Max(1, n - 1)) * Vector2.right) * T.mtMultiScatterU1481; ClampToRoom(ref b, 4);
                float distPx = (b - hand).magnitude * FbPx; float frames = Mathf.Clamp(distPx / Mathf.Max(0.5f, T.mtPxPerFrame1481), T.mtFlightMinF1481, T.mtFlightMaxF1481);
                Vector2 dir = b - hand; dir = dir.sqrMagnitude > 0.01f ? dir.normalized : FbAimDir1481();
                _fbGlobs.Add(new FbGlob { a = hand, b = b, dir = dir, dur = frames / FbFps, arcPx = T.mtArcBasePx1481 + T.mtArcPerPx1481 * distPx, startZPx = T.mtStartZPx1481, dmg = dmg, r = landR, s = s, shot = _fbShot++, t = -(i * 0.06f) });
            }
            HellordAudio.Play("magma_throw_cast"); return true; // ONE cast sound per throw (not per glob / shard)
        }
        void FbGlobLand1481(FbGlob g)
        {
            var T = HellordTuning.I; Vector2 at = g.b; int ord = YSort(at.y);
            FbArea1481(at, g.r, g.dmg * T.mtLandMul1481, g.s, T.mtKnock1481); FireBasicsFX1481.Once("molten_spit_burst", at, ord + 40); ScreenShake.Add(2, 0.1f);
            FireBasicsFX1481.Decal("molten_spit_scorch_v" + (g.shot % 3 + 1), FireBasicsFX1481.DecalKind.SpitScorch, at, FbGround + ord, 2f); // scorch from burst frame 2, draws below
            FbPatch1481(at, T.mtPoolR1481 * Mathf.Sqrt(g.s.AreaMul), T.mtPoolBurnS1481, T.mtPatchTickS1481, g.dmg * T.mtPatchTickMul1481, g.s);
            HellordAudio.Play("magma_throw_land");
            Vector2 d0 = g.dir; int n = Mathf.Min(T.msShards1481, T.msShardAngle1481.Length);
            for (int i = 0; i < n; i++)
            {   // 8 shards (HFX v2): throw direction + fixed angles, ground reach table, ground y x0.6, flight table, start 2 frames after the burst
                Vector2 d = FbUnit(T.msShardAngle1481[i], d0); Vector2 off = new Vector2(d.x, d.y * T.msGroundYScale1481) * (HData.At(T.msShardReachPx1481, i, 50f) / FbPx);
                Vector2 b = at + off; ClampToRoom(ref b, 4); float fl = HData.At(T.msShardFlightF1481, i, 12f);
                _fbShards.Add(new FbShard { a = at, b = b, idx = i, dur = fl / FbFps, t = -T.msShardDelayF1481 / FbFps, dmg = g.dmg * T.mtBlobMul1481, patchDmg = g.dmg * T.mtPatchTickMul1481, r = T.mtBlobR1481 * Mathf.Sqrt(g.s.AreaMul), s = g.s, shot = g.shot });
            }
        }
        static float FbShardEase(float k) => 1f - (1f - k) * (1f - k);
        static Vector2 FbShardGround(FbShard sh, float k) => Vector2.Lerp(sh.a, sh.b, FbShardEase(Mathf.Clamp01(k)));
        static Vector2 FbShardScreen(FbShard sh, float k, HellordTuning T) { k = Mathf.Clamp01(k); return FbShardGround(sh, k) + Vector2.up * ((T.msShardArcPx1481 * 4f * k * (1f - k) + T.msShardLiftPx1481 * (1f - k)) / FbPx); }
        void FbShardLand1481(FbShard sh)
        {
            var T = HellordTuning.I; Vector2 at = sh.b; int ord = YSort(at.y);
            FbArea1481(at, sh.r, sh.dmg, sh.s, T.mtKnock1481 * 0.4f); FireBasicsFX1481.Once("molten_spit_shard_land", at, ord + 20); HellordAudio.Play("magma_throw_splat");
            if ((sh.idx & 1) == 0)
            {   // every second shard leaves a small scorch (from stage 2 of the burst, HFX "sb 2") and a small burning patch
                FireBasicsFX1481.Decal("molten_spit_scorch_small_v" + ((sh.idx >> 1) % 2 + 1), FireBasicsFX1481.DecalKind.SpitScorchSmall, at, FbGround + ord, 2f);
                FbPatch1481(at, T.mtPatchR1481 * Mathf.Sqrt(sh.s.AreaMul), T.mtPatchS1481, T.mtPatchTickS1481, sh.patchDmg, sh.s);
            }
        }

        // ---------------------------------------------------------------- INFERNAL BOULDER (SkillId.RollingMagmaRock)
        bool CastRollingRock1481(SkillInst s, float dmg)
        {
            if (mode == Mode.StartPick) return false; var T = HellordTuning.I; EnsureFX44();
            if (_fbRocks.Count >= T.rrMaxRocks1481) { s.cd = 0.1f; return false; } // HFX v1 cap kept: <= 3 rocks alive (v2 notes: no cap on distinct rocks; raise rrMaxRocks1481 if wanted)
            Vector2 dir = FbAimDir1481(); int n = 1 + PCount49; float spread = n > 1 ? Mathf.Min(T.rrSpreadDeg1481, 120f / (n - 1)) : 0f;
            float distPx = (MouseWorld() - _heroPos).magnitude * FbPx; float rangeU = Mathf.Clamp(distPx + T.rrRangeExtraPx1481, T.rrRangeMinPx1481, T.rrRangeMaxPx1481) / FbPx * PRange49;
            for (int i = 0; i < n; i++)
            {
                Vector2 d = FbUnit((i - (n - 1) * 0.5f) * spread, dir);
                _fbRocks.Add(new FbRock { p = _heroPos + d * T.rrStartU1481, dir = d, range = rangeU, dmg = dmg, trailDmg = dmg * T.rrTrailTickMul1481, r = T.rrRadiusU1481, s = s,
                    loop = HellordAudio.LoopStart("Audio/SFX/firebasics1481/magma_rock_roll_loop", Random.Range(T.rrLoopVolMin1481, T.rrLoopVolMax1481), 1f + Random.Range(-T.rrLoopPitchSpread1481, T.rrLoopPitchSpread1481)) }); // one roll voice per rock
            }
            HellordAudio.Play("magma_rock_launch"); return true;
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
            Vector2 at = FbAimPoint1481(T.fpRangeU1481); float r = HData.At(T.fpRadiusU1481, s.level, 1.2f) * s.AreaMul; float dd = dmg * T.fpDmgMul1481; var sk = s; float tell = Mathf.Max(0.05f, T.fpTellS1481); int ord = YSort(at.y); int shot = _fbShot++; _fbPillars++;
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
            FireBasicsFX1481.Frame(); if (_fbGlobs.Count + _fbShards.Count + _fbRocks.Count + _fbPatches.Count == 0) return; var T = HellordTuning.I;
            // ---- Molten Spit globs
            for (int i = _fbGlobs.Count - 1; i >= 0; i--)
            {
                if (i >= _fbGlobs.Count) continue; var g = _fbGlobs[i]; g.t += dt; if (g.t < 0f) continue; g.age += dt;
                float k = Mathf.Clamp01(g.t / g.dur); Vector2 gp = Vector2.Lerp(g.a, g.b, k);
                if (k >= 1f) { _fbGlobs.RemoveAt(i); FbGlobLand1481(g); continue; } // landing adds shards (separate list)
                float dk = (1f / FbFps) / g.dur; Vector2 sp = FbGlobScreen(g, k, g.startZPx), v = k + dk <= 1f ? FbGlobScreen(g, k + dk, g.startZPx) - sp : sp - FbGlobScreen(g, k - dk, g.startZPx);
                float zPx = (sp.y - gp.y) * FbPx; int fr = (int)(g.age * FbFps); int ord = YSort(gp.y);
                FireBasicsFX1481.Show("molten_spit_glob_dir16", sp, fr % 8, ord + 30, FireBasicsFX1481.DirRow(v)); // row from the SCREEN velocity incl. height
                FireBasicsFX1481.Show("molten_spit_shadow", gp, zPx > 20f ? 0 : zPx > 12f ? 1 : zPx > 6f ? 2 : 3, FbGround + ord + 6);
                if (fr != g.lastFr) { g.lastFr = fr; if (fr % 3 == 0) FireBasicsFX1481.Once("molten_spit_ember", sp, ord + 28, 0f, 1f, 4); } // ember drip: every 3rd glob frame, max 4 alive
                if ((g.dur - g.t) * FbFps <= 14f) FireBasicsFX1481.Show("molten_spit_marker", g.b, fr % 8, FbGround + YSort(g.b.y) + 7); // subtle landing marker for the last 14 frames
            }
            // ---- Molten Spit shards
            for (int i = _fbShards.Count - 1; i >= 0; i--)
            {
                if (i >= _fbShards.Count) continue; var sh = _fbShards[i]; sh.t += dt; if (sh.t < 0f) continue; sh.age += dt; float k = Mathf.Clamp01(sh.t / sh.dur);
                if (k >= 1f) { _fbShards.RemoveAt(i); FbShardLand1481(sh); continue; }
                float dk = (1f / FbFps) / sh.dur; Vector2 gp = FbShardGround(sh, k), sp = FbShardScreen(sh, k, T); int ord = YSort(gp.y);
                Vector2 v = k + dk <= 1f ? FbShardScreen(sh, k + dk, T) - sp : sp - FbShardScreen(sh, k - dk, T); Vector2 gv = k + dk <= 1f ? FbShardGround(sh, k + dk) - gp : gp - FbShardGround(sh, k - dk);
                FireBasicsFX1481.Show("molten_spit_shard_dir16", sp, ((int)(sh.age * FbFps)) % 6, ord + 20, FireBasicsFX1481.DirRow(v));
                sh.scarAcc += dt * FbFps; int scarRow = FireBasicsFX1481.DirRow(gv); // a scar segment on the GROUND position every flight frame (ground direction row, no arc)
                while (sh.scarAcc >= 1f) { sh.scarAcc -= 1f; FireBasicsFX1481.Decal("molten_spit_shard_scar_dir16", FireBasicsFX1481.DecalKind.ShardScar, gp, FbGround + ord + 3, 0f, scarRow); }
            }
            // ---- Infernal Boulder
            float spdPx = T.rrSpeedU1481 * PSpeed49 * FbPx; // px/s (HFX v2 game scale 75)
            for (int i = _fbRocks.Count - 1; i >= 0; i--)
            {
                if (i >= _fbRocks.Count) continue; var o = _fbRocks[i]; o.age += dt; float step = Mathf.Max(0.5f, T.rrSpeedU1481 * PSpeed49) * dt; Vector2 prev = o.p; o.p += o.dir * step; o.dist += step; bool wall = FbWall1481(o.p);
                Query(o.p, o.r + 0.6f, _fbQ);
                foreach (int ei in _fbQ)
                {
                    var e = _en[ei]; if (!e.alive || o.hit.Contains(e.id) || (e.pos - o.p).magnitude > o.r + e.radius) continue;
                    o.hit.Add(e.id); Hit(e, o.dmg, o.s, o.dir * T.rrKnock1481); HellordAudio.Play("magma_rock_hit");
                    float sx = o.dir.x >= 0f ? 1f : -1f; FireBasicsFX1481.Once("infernal_boulder_hit", o.p + new Vector2(sx * 6f / FbPx, 0f), YSort(o.p.y) + 40, 0f, 1f, 0, o.dir.x < 0f); // first overlap only, 6 px forward at ground y, flipped for west
                }
                o.trailAcc += (o.p - prev).magnitude;
                while (o.trailAcc >= T.rrTrailGapU1481)
                {   // a flame tile every 8 px at the ground point (below everything); scorch decal (4 f late) + damaging patch on every 2nd tile
                    o.trailAcc -= T.rrTrailGapU1481; int oy = YSort(o.p.y);
                    if (o.tiles++ < T.rrTrailTiles1481) FireBasicsFX1481.Tile("infernal_boulder_trail", o.p, FbTrail + oy, T.rrTrailS1481 * SinDuration737);
                    if ((o.tufts++ & 1) == 0)
                    {
                        FireBasicsFX1481.Decal("infernal_boulder_scorch_v" + (_fbShot++ % 2 + 1), FireBasicsFX1481.DecalKind.BoulderScorch, o.p, FbGround + oy, 4f);
                        FbPatch1481(o.p, T.rrTrailR1481 * Mathf.Sqrt(o.s.AreaMul), T.rrTrailS1481, T.rrTrailTickS1481, o.trailDmg, o.s);
                    }
                }
                if (wall || o.dist >= o.range)
                {   // end: remove the roll sprite, crumble at the rock pivot + 3 short trail tiles at x-10 / 0 / +10 px
                    Vector2 at = o.p; ClampToRoom(ref at, 5); FbArea1481(at, T.rrCrumbleR1481 * Mathf.Sqrt(o.s.AreaMul), o.dmg * T.rrCrumbleMul1481, o.s, T.rrKnock1481 * 0.5f);
                    FireBasicsFX1481.Once("infernal_boulder_crumble", at + Vector2.up * (5f / FbPx), YSort(at.y) + 40);
                    for (int tx = -1; tx <= 1; tx++) FireBasicsFX1481.Tile("infernal_boulder_trail", at + new Vector2(tx * 10f / FbPx, 0f), FbTrail + YSort(at.y), 36f / FbFps);
                    float fade = Random.Range(T.rrLoopFadeMin1481, T.rrLoopFadeMax1481); HellordAudio.LoopStop(o.loop, fade); HellordAudio.PlayDelayed("magma_rock_stop", fade); if (i < _fbRocks.Count) _fbRocks.RemoveAt(i); continue; // fade the roll out, then the crumble
                }
                HellordAudio.LoopBeat(o.loop); int ro = YSort(o.p.y);
                FireBasicsFX1481.Show("infernal_boulder_roll_dir16", o.p + Vector2.up * (5f / FbPx), ((int)(o.dist * FbPx / 2.47f)) % 16, ro + 12, FireBasicsFX1481.DirRow(o.dir)); // roll tied to travel: 2.47 px per frame, pivot = centre (ground + 5 px)
            }
            // ---- burning ground (damage only; the pictures are the HFX decals / tiles)
            for (int i = _fbPatches.Count - 1; i >= 0; i--)
            {
                if (i >= _fbPatches.Count) continue; var p = _fbPatches[i]; p.life -= dt; if (p.life <= 0f) { _fbPatches.RemoveAt(i); continue; }
                p.tick -= dt;
                if (p.tick <= 0f)
                {
                    p.tick = p.tickS; Query(p.p, p.r, _fbQ);
                    foreach (int ei in _fbQ) { var e = _en[ei]; if (e.alive && (e.pos - p.p).magnitude <= p.r + e.radius * 0.5f) Hit(e, p.dmg, p.s, Vector2.zero, true); }
                }
            }
        }
    }
}
