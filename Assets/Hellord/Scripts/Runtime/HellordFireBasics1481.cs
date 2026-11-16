using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    // t1481: FIRE BASIC MAIN ATTACKS - Magma Throw, Rolling Magma Rock, Fire Pillar (SkillId.MagmaThrow .. FirePillar, ids 16-18).
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
        class FbGlob { public Vector2 a, b, dir; public float t, dur, arcPx, dmg, r, patchDmg, age; public int blobs, idx, shot, lastFr; public bool blob; public SkillInst s; }
        class FbRock { public object loop; public Vector2 p, dir; public float dist, range, dmg, trailAcc, age, r, trailDmg; public int tufts, row; public SkillInst s; public readonly HashSet<int> hit = new HashSet<int>(); }
        class FbPatch { public Vector2 p; public float r, life, tick, tickS, dmg; public SkillInst s; }
        readonly List<FbGlob> _fbGlobs = new List<FbGlob>(); readonly List<FbRock> _fbRocks = new List<FbRock>(); readonly List<FbPatch> _fbPatches = new List<FbPatch>(); readonly List<int> _fbQ = new List<int>(64); int _fbShot, _fbPillars;
        void ClearFb1481() { foreach (var r in _fbRocks) HellordAudio.LoopStop(r.loop, 0.1f); _fbGlobs.Clear(); _fbRocks.Clear(); _fbPatches.Clear(); _fbPillars = 0; FireBasicsFX1481.Clear(); }
        const float FbFps = 30f, FbPx = 16f; const int FbGround = -15250; // floor decals: above the floor, under every actor (see D2Feel1434 OrdPool)

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
        void FbPatch1481(Vector2 at, float r, float life, float tickS, float dmg, SkillInst s) { if (_fbPatches.Count < 64) _fbPatches.Add(new FbPatch { p = at, r = r, life = life * SinDuration737, tick = tickS * 0.5f, tickS = tickS, dmg = dmg, s = s }); }

        // ---------------------------------------------------------------- MAGMA THROW
        bool CastMagmaThrow1481(SkillInst s, float dmg)
        {
            if (mode == Mode.StartPick) return false; var T = HellordTuning.I; EnsureFX44();
            int inFlight = 0; foreach (var g0 in _fbGlobs) if (!g0.blob) inFlight++; if (inFlight >= T.mtMaxGlobs1481) { s.cd = 0.1f; return false; } // HFX cap: <= 3 globs in flight
            Vector2 aim = FbAimPoint1481(T.mtRangeU1481); int n = 1 + PCount49; Vector2 hand = HeroCenter + new Vector2(aim.x >= _heroPos.x ? 0.45f : -0.45f, 0.45f);
            float landR = HData.At(T.mtLandR1481, s.level, 1f) * s.AreaMul; int blobs = Mathf.Clamp(HData.At(T.mtBlobs1481, s.level, 5), 1, 5); float a0 = Random.value * 360f;
            for (int i = 0; i < n; i++)
            {
                Vector2 b = i == 0 ? aim : aim + (Vector2)(Quaternion.Euler(0, 0, a0 + i * 360f / Mathf.Max(1, n - 1)) * Vector2.right) * T.mtMultiScatterU1481; ClampToRoom(ref b, 4);
                float distPx = (b - _heroPos).magnitude * FbPx; float frames = Mathf.Clamp(distPx / Mathf.Max(0.5f, T.mtPxPerFrame1481), T.mtFlightMinF1481, T.mtFlightMaxF1481);
                Vector2 dir = b - _heroPos; dir = dir.sqrMagnitude > 0.01f ? dir.normalized : FbAimDir1481();
                _fbGlobs.Add(new FbGlob { a = _heroPos, b = b, dir = dir, dur = frames / FbFps, arcPx = T.mtArcBasePx1481 + T.mtArcPerPx1481 * distPx, dmg = dmg, r = landR, blobs = blobs, s = s, shot = _fbShot++, t = -(i * 0.06f) });
            }
            HellordAudio.Play("magma_throw_cast"); return true;
        }
        void FbGlobLand1481(FbGlob g)
        {
            var T = HellordTuning.I; Vector2 at = g.b; int ord = YSort(at.y);
            if (g.blob)
            {   // child landing: small burst of damage + splash; blobs 1 and 3 leave a small burning pool
                FbArea1481(at, g.r, g.dmg, g.s, T.mtKnock1481 * 0.4f); FireBasicsFX1481.Once("magma_child_splash", at, ord + 30); HellordAudio.Play("magma_throw_splat");
                if (g.idx == 0 || g.idx == 2)
                {
                    FireBasicsFX1481.Decal(g.idx == 0 ? "magma_pool_small_v1" : "magma_pool_small_v2", FireBasicsFX1481.DecalKind.SmallPool, at, FbGround + ord, 2f);
                    FbPatch1481(at, T.mtPatchR1481 * Mathf.Sqrt(g.s.AreaMul), T.mtPatchS1481, T.mtPatchTickS1481, g.patchDmg, g.s);
                }
                return;
            }
            FbArea1481(at, g.r, g.dmg * T.mtLandMul1481, g.s, T.mtKnock1481); FireBasicsFX1481.Once("magma_throw_burst", at, ord + 30); ScreenShake.Add(2, 0.1f);
            FireBasicsFX1481.Decal("magma_pool_v" + (g.shot % 3 + 1), FireBasicsFX1481.DecalKind.BigPool, at, FbGround + ord);
            FbPatch1481(at, T.mtPoolR1481 * Mathf.Sqrt(g.s.AreaMul), T.mtPoolBurnS1481, T.mtPatchTickS1481, g.dmg * T.mtPatchTickMul1481, g.s);
            HellordAudio.Play("magma_throw_land");
            for (int i = 0; i < g.blobs; i++)
            {
                float ang = g.blobs > 1 ? -T.mtBlobFanDeg1481 + 2f * T.mtBlobFanDeg1481 * i / (g.blobs - 1) : 0f; Vector2 d = (Vector2)(Quaternion.Euler(0, 0, ang) * g.dir);
                Vector2 b = at + d * (HData.At(T.mtBlobRangePx1481, i, 30f) / FbPx); ClampToRoom(ref b, 4); float fl = HData.At(T.mtBlobFlightF1481, i, 16f);
                _fbGlobs.Add(new FbGlob { a = at, b = b, dir = d, blob = true, idx = i, dur = fl / FbFps, arcPx = T.mtBlobArcPx1481, dmg = g.dmg * T.mtBlobMul1481, patchDmg = g.dmg * T.mtPatchTickMul1481, r = T.mtBlobR1481 * Mathf.Sqrt(g.s.AreaMul), s = g.s, t = -T.mtBlobDelayF1481 / FbFps });
            }
        }

        // ---------------------------------------------------------------- ROLLING MAGMA ROCK
        bool CastRollingRock1481(SkillInst s, float dmg)
        {
            if (mode == Mode.StartPick) return false; var T = HellordTuning.I; EnsureFX44();
            if (_fbRocks.Count >= T.rrMaxRocks1481) { s.cd = 0.1f; return false; } // HFX cap: <= 3 rocks alive
            Vector2 dir = FbAimDir1481(); int n = 1 + PCount49; float spread = n > 1 ? Mathf.Min(T.rrSpreadDeg1481, 120f / (n - 1)) : 0f;
            float distPx = (MouseWorld() - _heroPos).magnitude * FbPx; float rangeU = Mathf.Clamp(distPx + T.rrRangeExtraPx1481, T.rrRangeMinPx1481, T.rrRangeMaxPx1481) / FbPx * PRange49;
            for (int i = 0; i < n; i++)
            {
                Vector2 d = (Vector2)(Quaternion.Euler(0, 0, (i - (n - 1) * 0.5f) * spread) * dir);
                _fbRocks.Add(new FbRock { p = _heroPos + d * T.rrStartU1481, dir = d, range = rangeU, dmg = dmg, trailDmg = dmg * T.rrTrailTickMul1481, r = T.rrRadiusU1481, s = s,
                    loop = HellordAudio.LoopStart("Audio/SFX/firebasics1481/magma_rock_roll_loop", Random.Range(T.rrLoopVolMin1481, T.rrLoopVolMax1481), 1f + Random.Range(-T.rrLoopPitchSpread1481, T.rrLoopPitchSpread1481)) }); // one roll voice per rock
            }
            HellordAudio.Play("magma_rock_launch"); return true;
        }
        bool FbWall1481(Vector2 p)
        {
            if (_room == null) return false; _room.Walkable(6, out var lo, out var hi); return p.x < lo.x || p.x > hi.x || p.y < lo.y || p.y > hi.y; // room bounds only (like the frost orbs)
        }

        // ---------------------------------------------------------------- FIRE PILLAR
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
            FireBasicsFX1481.Frame(); if (_fbGlobs.Count + _fbRocks.Count + _fbPatches.Count == 0) return; var T = HellordTuning.I;
            // ---- lobbed globs + scattered children
            for (int i = _fbGlobs.Count - 1; i >= 0; i--)
            {
                if (i >= _fbGlobs.Count) continue; var g = _fbGlobs[i]; g.t += dt; if (g.t < 0f) continue; g.age += dt;
                float k = Mathf.Clamp01(g.t / g.dur); Vector2 gp = Vector2.Lerp(g.a, g.b, k);
                if (k >= 1f) { _fbGlobs.RemoveAt(i); FbGlobLand1481(g); continue; } // landing may add children (new entries are above i)
                float zPx = g.blob ? g.arcPx * 4f * k * (1f - k) : T.mtStartZPx1481 * (1f - k) + g.arcPx * 4f * k * (1f - k); int fr = (int)(g.age * FbFps); int ord = YSort(gp.y);
                if (g.blob) { FireBasicsFX1481.Show("magma_child_blob", gp + Vector2.up * (zPx / FbPx), fr % 6, ord + 40); continue; }
                FireBasicsFX1481.Show("magma_throw_glob", gp + Vector2.up * (zPx / FbPx), fr % 8, ord + 40);
                FireBasicsFX1481.Show("magma_throw_shadow", gp, zPx > 20f ? 0 : zPx > 12f ? 1 : zPx > 6f ? 2 : 3, FbGround + ord + 6);
                if (fr != g.lastFr) { g.lastFr = fr; if (fr % 3 == 0) FireBasicsFX1481.Once("magma_throw_ember", gp + Vector2.up * (zPx / FbPx), ord + 38, 0f, 1f, 4); } // ember drip: every 3rd glob frame, max 4 alive
                if ((g.dur - g.t) * FbFps <= 14f) FireBasicsFX1481.Show("magma_throw_marker", g.b, fr % 8, FbGround + YSort(g.b.y) + 7); // subtle landing marker for the last 14 frames
            }
            // ---- rolling rocks
            float spin = T.rrSpeedU1481 * PSpeed49 * FbPx / 96f; // the 10-frame loop is one revolution at 96 px/s
            for (int i = _fbRocks.Count - 1; i >= 0; i--)
            {
                if (i >= _fbRocks.Count) continue; var o = _fbRocks[i]; o.age += dt; float step = Mathf.Max(0.5f, T.rrSpeedU1481 * PSpeed49) * dt; Vector2 prev = o.p; o.p += o.dir * step; o.dist += step; bool wall = FbWall1481(o.p);
                Query(o.p, o.r + 0.6f, _fbQ);
                foreach (int ei in _fbQ)
                {
                    var e = _en[ei]; if (!e.alive || o.hit.Contains(e.id) || (e.pos - o.p).magnitude > o.r + e.radius) continue;
                    o.hit.Add(e.id); Hit(e, o.dmg, o.s, o.dir * T.rrKnock1481); HellordAudio.Play("magma_rock_hit");
                    FireBasicsFX1481.Once("rolling_magma_hit", o.p + o.dir * 0.35f + Vector2.up * (5f / FbPx), YSort(o.p.y) + 20, Mathf.Atan2(o.dir.y, o.dir.x) * Mathf.Rad2Deg); // pierce spark at first contact only
                }
                o.trailAcc += (o.p - prev).magnitude;
                if (o.trailAcc >= T.rrTrailGapU1481)
                {
                    o.trailAcc = 0; int oy = YSort(o.p.y); FireBasicsFX1481.Once("rolling_magma_trail", o.p, oy + 4, 0f, 1f, 3); // tuft every 16 px, max 3 alive
                    if ((o.tufts++ & 1) == 0) FireBasicsFX1481.Decal("rolling_magma_scorch_v" + (_fbShot++ % 2 + 1), FireBasicsFX1481.DecalKind.Scorch, o.p, FbGround + oy); // scorch with every other tuft
                    FbPatch1481(o.p, T.rrTrailR1481 * Mathf.Sqrt(o.s.AreaMul), T.rrTrailS1481, T.rrTrailTickS1481, o.trailDmg, o.s);
                }
                if (wall || o.dist >= o.range)
                {   // crumbles: burst + the HFX crumble strip
                    Vector2 at = o.p; ClampToRoom(ref at, 5); FbArea1481(at, T.rrCrumbleR1481 * Mathf.Sqrt(o.s.AreaMul), o.dmg * T.rrCrumbleMul1481, o.s, T.rrKnock1481 * 0.5f);
                    FireBasicsFX1481.Once("rolling_magma_crumble", at + Vector2.up * (5f / FbPx), YSort(at.y) + 30);
                    float fade = Random.Range(T.rrLoopFadeMin1481, T.rrLoopFadeMax1481); HellordAudio.LoopStop(o.loop, fade); HellordAudio.PlayDelayed("magma_rock_stop", fade); if (i < _fbRocks.Count) _fbRocks.RemoveAt(i); continue; // fade the roll out, then the crumble
                }
                HellordAudio.LoopBeat(o.loop); int ro = YSort(o.p.y);
                FireBasicsFX1481.Show("rolling_magma_roll_dir16", o.p + Vector2.up * (5f / FbPx), ((int)(o.age * FbFps * spin)) % 10, ro + 10, FireBasicsFX1481.DirRow(o.dir));
                FireBasicsFX1481.Show("magma_throw_shadow", o.p, 3, FbGround + ro + 6);
            }
            // ---- burning ground (damage only; the pictures are the HFX decals)
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
