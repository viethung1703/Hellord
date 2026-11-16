using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    // t1481: FIRE BASIC MAIN ATTACKS - Magma Throw, Rolling Magma Rock, Fire Pillar (SkillId.MagmaThrow .. FirePillar, ids 16-18).
    // Gungeon-style: the PLAYER fires them. LEFT MOUSE casts, holding it keeps firing at the skill's fire rate, aim = the cursor.
    // They never auto-fire (see FbWant1481). Every number lives in HellordTuning (t1481 section).
    // Visuals are PLACEHOLDERS (ZDisc discs + existing Pixel FX bursts + the old Hellfire Pillar tell/eruption art) until the HFX strips land.
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
        class FbGlob { public Vector2 a, b; public float t, dur, arcH, dmg, r, patchDmg; public int blobs; public bool blob; public SkillInst s; }
        class FbRock { public object loop; public Vector2 p, dir; public float dist, range, dmg, trailAcc, age, r, trailDmg; public SkillInst s; public readonly HashSet<int> hit = new HashSet<int>(); }
        class FbPatch { public Vector2 p; public float r, life, life0, tick, dmg; public SkillInst s; }
        readonly List<FbGlob> _fbGlobs = new List<FbGlob>(); readonly List<FbRock> _fbRocks = new List<FbRock>(); readonly List<FbPatch> _fbPatches = new List<FbPatch>(); readonly List<int> _fbQ = new List<int>(64);
        void ClearFb1481() { foreach (var r in _fbRocks) HellordAudio.LoopStop(r.loop, 0.1f); _fbGlobs.Clear(); _fbRocks.Clear(); _fbPatches.Clear(); }

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

        // ---------------------------------------------------------------- MAGMA THROW
        bool CastMagmaThrow1481(SkillInst s, float dmg)
        {
            if (mode == Mode.StartPick) return false; var T = HellordTuning.I; EnsureFX44();
            Vector2 aim = FbAimPoint1481(T.mtRangeU1481); int n = 1 + PCount49; Vector2 hand = HeroCenter + new Vector2(aim.x >= _heroPos.x ? 0.45f : -0.45f, 0.45f);
            float landR = HData.At(T.mtLandR1481, s.level, 1f) * s.AreaMul; int blobs = HData.At(T.mtBlobs1481, s.level, 4); float a0 = Random.value * 360f;
            for (int i = 0; i < n; i++)
            {
                Vector2 b = i == 0 ? aim : aim + (Vector2)(Quaternion.Euler(0, 0, a0 + i * 360f / Mathf.Max(1, n - 1)) * Vector2.right) * T.mtMultiScatterU1481; ClampToRoom(ref b, 4);
                float dur = Mathf.Max(T.mtMinFlightS1481, (b - _heroPos).magnitude / Mathf.Max(0.5f, T.mtSpeedU1481));
                _fbGlobs.Add(new FbGlob { a = _heroPos, b = b, dur = dur, arcH = T.mtArcH1481, dmg = dmg, r = landR, blobs = blobs, s = s, t = -(i * 0.06f) });
            }
            HellordAudio.Play("magma_throw_cast"); HordeFX.CheapBurst(hand, FbOrange1481, 4); return true;
        }
        static readonly Color FbOrange1481 = new Color(1f, 0.48f, 0.1f), FbYellow1481 = new Color(1f, 0.85f, 0.3f), FbCrust1481 = new Color(0.28f, 0.15f, 0.1f);
        void FbGlobLand1481(FbGlob g)
        {
            var T = HellordTuning.I; Vector2 at = g.b;
            if (g.blob)
            {
                FbArea1481(at, g.r, g.dmg, g.s, T.mtKnock1481 * 0.4f); HordeFX.CheapBurst(at + Vector2.up * 0.1f, FbOrange1481, 4); HellordAudio.Play("magma_throw_splat");
                if (_fbPatches.Count < 64) _fbPatches.Add(new FbPatch { p = at, r = T.mtPatchR1481 * Mathf.Sqrt(g.s.AreaMul), life = T.mtPatchS1481 * SinDuration737, life0 = T.mtPatchS1481 * SinDuration737, tick = T.mtPatchTickS1481 * 0.5f, dmg = g.patchDmg, s = g.s });
                return;
            }
            FbArea1481(at, g.r, g.dmg * T.mtLandMul1481, g.s, T.mtKnock1481); ProjectileFX.Impact(at + Vector2.up * 0.2f, ProjElement.Fire, Vector2.zero); HordeFX.CheapBurst(at + Vector2.up * 0.2f, FbOrange1481, 8); ScreenShake.Add(2, 0.1f);
            HellordAudio.Play("magma_throw_land");
            float a0 = Random.value * 360f;
            for (int i = 0; i < g.blobs; i++)
            {
                float ang = (a0 + i * 360f / g.blobs + Random.Range(-18f, 18f)) * Mathf.Deg2Rad; float dist = Random.Range(T.mtBlobMinU1481, T.mtBlobMaxU1481);
                Vector2 b = at + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * dist; ClampToRoom(ref b, 4);
                _fbGlobs.Add(new FbGlob { a = at, b = b, blob = true, dur = Mathf.Max(0.1f, T.mtBlobFlightS1481 + Random.Range(-T.mtBlobJitterS1481, T.mtBlobJitterS1481)), arcH = T.mtBlobArcH1481, dmg = g.dmg * T.mtBlobMul1481, patchDmg = g.dmg * T.mtPatchTickMul1481, r = T.mtBlobR1481 * Mathf.Sqrt(g.s.AreaMul), s = g.s });
            }
        }

        // ---------------------------------------------------------------- ROLLING MAGMA ROCK
        bool CastRollingRock1481(SkillInst s, float dmg)
        {
            if (mode == Mode.StartPick) return false; var T = HellordTuning.I; EnsureFX44();
            Vector2 dir = FbAimDir1481(); int n = 1 + PCount49; float spread = n > 1 ? Mathf.Min(T.rrSpreadDeg1481, 120f / (n - 1)) : 0f;
            for (int i = 0; i < n; i++)
            {
                Vector2 d = (Vector2)(Quaternion.Euler(0, 0, (i - (n - 1) * 0.5f) * spread) * dir);
                _fbRocks.Add(new FbRock { p = _heroPos + d * T.rrStartU1481, dir = d, range = T.rrRangeU1481 * PRange49, dmg = dmg, trailDmg = dmg * T.rrTrailTickMul1481, r = T.rrRadiusU1481, s = s,
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
            Vector2 at = FbAimPoint1481(T.fpRangeU1481); float r = HData.At(T.fpRadiusU1481, s.level, 1.2f) * s.AreaMul; float dd = dmg * T.fpDmgMul1481; var sk = s;
            PillarTell934(at, r); HeroSkillFX.PillarCast(at, r, 0f); // the old Hellfire Pillar telegraph + eruption art, now at the cursor
            HellordAudio.Play("fire_pillar_telegraph");
            After(T.fpTellS1481, () =>
            {
                if (mode == Mode.StartPick) return; PillarImpact934(at, r); dd *= PactTrailEmp53(at);
                FbArea1481(at, r, dd, sk, T.fpKnock1481); HellordAudio.Play("fire_pillar_erupt"); ScreenShake.Add(2, 0.12f);
            });
            return true;
        }

        // ---------------------------------------------------------------- per-frame update (called after UpdateIce)
        void UpdateFireBasics1481(float dt)
        {
            if (_fbGlobs.Count + _fbRocks.Count + _fbPatches.Count == 0) return; var T = HellordTuning.I;
            // ---- lobbed globs + scattered blobs
            for (int i = _fbGlobs.Count - 1; i >= 0; i--)
            {
                if (i >= _fbGlobs.Count) continue; var g = _fbGlobs[i]; g.t += dt; if (g.t < 0f) continue;
                float k = Mathf.Clamp01(g.t / g.dur); Vector2 gp = Vector2.Lerp(g.a, g.b, k); float h = g.arcH * 4f * k * (1f - k) + (g.blob ? 0f : 0.7f * (1f - k));
                if (k >= 1f) { _fbGlobs.RemoveAt(i); FbGlobLand1481(g); continue; } // landing may add blobs / clear (kills) - list indices below i are untouched
                float rr = g.blob ? 0.14f : 0.3f; ZDisc(gp, rr * 0.9f, new Color(0, 0, 0, 0.35f), 0.5f, 5); ZDisc(gp + Vector2.up * h, rr, FbOrange1481, 1f, 7); ZDisc(gp + Vector2.up * h, rr * 0.5f, FbYellow1481, 1f, 8);
            }
            // ---- rolling rocks
            for (int i = _fbRocks.Count - 1; i >= 0; i--)
            {
                if (i >= _fbRocks.Count) continue; var o = _fbRocks[i]; o.age += dt; float step = Mathf.Max(0.5f, T.rrSpeedU1481 * PSpeed49) * dt; Vector2 prev = o.p; o.p += o.dir * step; o.dist += step; bool wall = FbWall1481(o.p);
                Query(o.p, o.r + 0.6f, _fbQ);
                foreach (int ei in _fbQ)
                {
                    var e = _en[ei]; if (!e.alive || o.hit.Contains(e.id) || (e.pos - o.p).magnitude > o.r + e.radius) continue;
                    o.hit.Add(e.id); Hit(e, o.dmg, o.s, o.dir * T.rrKnock1481); HordeFX.CheapBurst(e.pos + Vector2.up * 0.6f, FbOrange1481, 4); HellordAudio.Play("magma_rock_hit");
                }
                o.trailAcc += (o.p - prev).magnitude;
                if (o.trailAcc >= T.rrTrailGapU1481)
                {
                    o.trailAcc = 0; if (_fbPatches.Count >= T.rrTrailMax1481 + 64) _fbPatches.RemoveAt(0);
                    float life = T.rrTrailS1481 * SinDuration737; _fbPatches.Add(new FbPatch { p = o.p, r = T.rrTrailR1481 * Mathf.Sqrt(o.s.AreaMul), life = life, life0 = life, tick = 0.05f, dmg = o.trailDmg, s = o.s });
                }
                if (wall || o.dist >= o.range)
                {   // crumbles: small burst + a few embers
                    Vector2 at = o.p; ClampToRoom(ref at, 5); FbArea1481(at, T.rrCrumbleR1481 * Mathf.Sqrt(o.s.AreaMul), o.dmg * T.rrCrumbleMul1481, o.s, T.rrKnock1481 * 0.5f);
                    ProjectileFX.Impact(at + Vector2.up * 0.2f, ProjElement.Fire, Vector2.zero); HordeFX.CheapBurst(at + Vector2.up * 0.2f, FbCrust1481, 6); HordeFX.CheapBurst(at + Vector2.up * 0.3f, FbOrange1481, 5);
                    float fade = Random.Range(T.rrLoopFadeMin1481, T.rrLoopFadeMax1481); HellordAudio.LoopStop(o.loop, fade); HellordAudio.PlayDelayed("magma_rock_stop", fade); if (i < _fbRocks.Count) _fbRocks.RemoveAt(i); continue; // fade the roll out, then the crumble
                }
                HellordAudio.LoopBeat(o.loop); Vector2 rp = o.p + Vector2.up * 0.35f; float spin = o.age * 7f;
                ZDisc(o.p, o.r * 0.9f, new Color(0, 0, 0, 0.35f), 0.5f, 5); ZDisc(rp, o.r, FbCrust1481, 1f, 7);
                ZDisc(rp + new Vector2(Mathf.Cos(spin), Mathf.Sin(spin)) * o.r * 0.4f, o.r * 0.38f, FbOrange1481, 1f, 8); ZDisc(rp - new Vector2(Mathf.Sin(spin * 1.3f), Mathf.Cos(spin * 1.3f)) * o.r * 0.35f, o.r * 0.22f, FbYellow1481, 1f, 8);
            }
            // ---- burning ground (blob puddles + rock embers)
            for (int i = _fbPatches.Count - 1; i >= 0; i--)
            {
                if (i >= _fbPatches.Count) continue; var p = _fbPatches[i]; p.life -= dt; if (p.life <= 0f) { _fbPatches.RemoveAt(i); continue; }
                p.tick -= dt;
                if (p.tick <= 0f)
                {
                    p.tick = p.life0 > 1.5f ? T.mtPatchTickS1481 : T.rrTrailTickS1481; Query(p.p, p.r, _fbQ);
                    foreach (int ei in _fbQ) { var e = _en[ei]; if (e.alive && (e.pos - p.p).magnitude <= p.r + e.radius * 0.5f) Hit(e, p.dmg, p.s, Vector2.zero, true); }
                }
                float f = Mathf.Clamp01(p.life / p.life0), fl = 0.75f + 0.25f * Mathf.Sin(Time.time * 18f + p.p.x * 7f);
                ZDisc(p.p, p.r, new Color(0.45f, 0.1f, 0.05f, 0.55f * f), 0.55f, 4); ZDisc(p.p + Vector2.up * 0.08f, p.r * 0.55f * fl, new Color(1f, 0.5f, 0.1f, 0.9f * f), 0.7f, 6);
            }
        }
    }
}
