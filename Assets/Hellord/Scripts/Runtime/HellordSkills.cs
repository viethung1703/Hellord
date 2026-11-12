using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

namespace Hellord
{
    public partial class HellordGame
    {
        readonly List<int> _sq = new List<int>(256), _explQ = new List<int>(32), _zoneQ = new List<int>(32); // v0.0.14 perf: no per-hit List alloc

        void UpdateSkills(float dt)
        {
            TickV36(dt); SinTick818(dt); // t818u sins // v0.0.36: scheduler + Life Leech window
            bool hasOrbit = false, hasRing = false;
            foreach (var s in skills)
            {
                if (s.def.id == SkillId.BoneOrbit) { hasOrbit = true; UpdateOrbit(s, dt); continue; }
                if (HData.Summon561(s.def)) { s.cd = 0; continue; } // t561u: summons are passive (minion tick)
                if (s.def.id == SkillId.StormRing) hasRing = true;
                s.cd -= dt; bool want716 = !Manual716 || Want716(s); if (Manual716 && s.cd < 0) s.cd = 0; // t716u: Manual = cast on key
                if (s.cd <= 0 && want716 && PactCanCast53(s)) { s.cd = s.def.cooldown * s.CdMul * PactCd53(s); float lc53 = _lastCast[(int)s.def.id]; Cast(s, 1f); if (_lastCast[(int)s.def.id] != lc53) PactAfterCast53(s); s.cd *= SinEchoCd818(s); if (s.Has(SupportId.Echo) && !SinNoEcho818(s)) { s.echoLeft = HData.At(HellordTuning.I.supEchoCount, s.Lvl(SupportId.Echo)); if (s.def.id == SkillId.CurseRune) s.echoLeft = Mathf.Min(s.echoLeft, HellordTuning.I.curseEchoMax738); s.echoT = s.echoLeft > 0 ? 0.3f : -1; } /* t738u */ }
                if (s.echoT > 0) { s.echoT -= dt; if (s.echoT <= 0) { _echo738 = true; Cast(s, HData.At(HellordTuning.I.supEchoDmg, s.Lvl(SupportId.Echo), 0.5f) * SinEchoDmg818()); _echo738 = false; if (--s.echoLeft > 0) s.echoT = 0.3f; else s.echoT = -1; } } // v0.0.27: Echo repeats grow with its level
            }
            if (!hasOrbit && (_orbs.Count > 0 || _orbitJobs != null)) { foreach (var o in _orbs) Destroy(o.gameObject); _orbs.Clear(); KillOrbit(); }
            if (!hasRing && _aura != null) { _aura.life = -1; _aura = null; }
        }

        // v0.0.10 FX self-check (Shift+9 overlay): last cast time per skill
        readonly float[] _lastCast = new float[12]; // v0.0.36 (t561u: +2 summons): one slot per SkillId
        void Cast(SkillInst s, float mul)
        {
            float dmg = SkillDamage(s) * mul; // v0.0.13: _lastCast is stamped only after a real cast (early 'no target' returns no longer count -> no false FX WARN)
            Vector2 c = HeroCenter;
            switch (s.def.id)
            {
                case SkillId.Fireball:
                {
                    if (PactMeteor53(s, dmg)) break; // v0.0.53 Meteor Pact
                    if (SinOn818(38) && !(Nearest(_heroPos, 11, -1) < 0 && !Aiming())) { SinSun818(s, dmg, c); break; } // t818u #38 Divine Sunstrike
                    if (Nearest(_heroPos, 11, -1) < 0 && !BrkNear53(_heroPos, 11) && !Aiming()) { s.cd = 0.1f; return; } // hold fire when nothing to shoot
                    var T = HellordTuning.I; Vector2 dir = AimDir();
                    int n = HData.At(T.fbProjectiles, s.level) + HellordGame.PCount49 + (s.Has(SupportId.MultipleProjectiles) ? HData.At(T.supMultiExtra, s.Lvl(SupportId.MultipleProjectiles)) : 0); // v0.0.27: projectiles grow with skill level + Multi level
                    float spread = n > 1 ? Mathf.Min(T.fbSpreadDeg, 150f / (n - 1)) : 0f;
                    int pierce = s.Has(SupportId.Pierce) ? HData.At(T.supPierce, s.Lvl(SupportId.Pierce)) : 0, bounce = s.Has(SupportId.Bounce) ? HData.At(T.supBounce, s.Lvl(SupportId.Bounce)) : 0;
                    float expl = HData.At(T.fbSplashRadiusU, s.level, 0.8f) * s.AreaMul;
                    for (int i = 0; i < n; i++)
                    {
                        float a = (i - (n - 1) * 0.5f) * spread;
                        var d = (Vector2)(Quaternion.Euler(0, 0, a) * dir);
                        SpawnProj(s, c + dir * 0.4f, d * 11f, dmg, pierce, s.Has(SupportId.Fork), bounce, expl);
                    }
                    ProjectileFX.CastFlash(c + dir * 0.5f, dir, ProjElement.Fire); Flash661(c + dir * 0.5f, ElemCol661(s)); /* t661u muzzle */ SL667(SkillLightHooks.FireballMuzzle, c + dir * 0.5f, dir); /* t667u */ HellordAudio.Play("fireball_cast");
                    break;
                }
                case SkillId.FrostCleave:
                {
                    var T = HellordTuning.I; float r = HData.At(T.frostRangeU, s.level, 2.3f) * s.AreaMul, half = SinOn818(43) ? 180f : HData.At(T.frostHalfAngle, s.level, 65f); if (SinOn818(43)) SinGlint818(); // t818u #43 // v0.0.27: reach + arc grow with skill level
                    if (Nearest(_heroPos, r + 1.5f, -1) < 0 && !BrkNear53(_heroPos, r + 1.5f) && !Aiming()) { s.cd = 0.1f; return; }
                    Vector2 dir = AimDir();
                    Query(c, r, _sq); SkillArea53(c, r, dmg, (int)s.def.id); // v0.0.53 AoE hits decor
                    bool anyHit = false;
                    foreach (int i in _sq) { var e = _en[i]; Vector2 to = e.pos + Vector2.up * e.hitY - c; if (half >= 179f || to.sqrMagnitude < 0.5f || Vector2.Angle(dir, to) < half) { Hit(e, (e.chillT > 0 || e.freezeT > 0) ? dmg * T.frostChilledBonus : dmg, s, dir * T.frostKnock); anyHit = true; } }
                    HFX.FrostCleaveAnyHit990 = anyHit; HFX.FrostCleave(c, dir, r, s.AreaMul, half); EraseCone49(c, dir, r, half); // v0.0.49 / t990u
                    HellordAudio.Play("frost_cleave");
                    break;
                }
                case SkillId.StormRing:
                {
                    float r = HData.At(HellordTuning.I.stormRadiusU, s.level, 2.6f) * s.AreaMul; // v0.0.27: radius grows with skill level (2.2 u at Lv1 to 7 u at Lv5)
                    bool cur818 = SinOn818(35); Vector2 rc = cur818 ? SinRingAnchor818().position : _heroPos; // t818u #35 Wrath of Heaven: ring at the cursor
                    if (_aura == null || Mathf.Abs(_auraR - r) > 0.01f || _auraCur818 != cur818)
                    {
                        if (_aura != null) _aura.life = -1; _auraCur818 = cur818;
                        _auraR = r; _aura = HellFX.AuraField(cur818 ? SinRingAnchor818() : _hero, r * HArt.PPU, new Color(0.55f, 0.8f, 1f, Mathf.Clamp(0.55f * 3.2f / r, 0.22f, 0.55f)), 1e6f, 0.5f); // fainter fill when the ring is huge so the screen stays readable
                    }
                    Query(rc, r, _sq); SkillArea53(rc, r, dmg, (int)s.def.id); int arcs = 0; if (cur818 && _sq.Count > 0) SinGlint818(); Vector3 a1 = default, a2 = default, a3 = default;
                    foreach (int i in _sq)
                    {
                        var e = _en[i]; Vector3 top = e.pos + Vector2.up * e.hitY; Hit(e, dmg, s, Vector2.zero);
                        if (arcs < 3 && Random.value < 0.5f) { arcs++; if (arcs == 1) a1 = top; else if (arcs == 2) a2 = top; else a3 = top; }
                    }
                    // pooled ChainFX bolt: hero -> up to 3 struck enemies (points version; our enemies have no Collider2D)
                    if (_sq.Count > 0) HellordAudio.Play("storm_ring"); // once per pulse that zaps something
                    if (arcs > 0) ChainFX.ChainLightningPoints(cur818 ? rc + Vector2.up * 0.8f : c, a1, a2, a3, arcs + 1, new Color(0.7f, 0.9f, 1f), 0.03f, 0.14f);
                    break;
                }
                case SkillId.CurseRune:
                {
                    Vector2 at;
                    if (CurAim720) at = GroundAim716(); else if (Aiming()) { at = MouseWorld(); var st = HInput.AimStick(); if (st.sqrMagnitude > 0.09f) at = _heroPos + st.normalized * 4; if ((at - _heroPos).magnitude > 7) at = _heroPos + (at - _heroPos).normalized * 7; }
                    else { int n = Nearest(_heroPos, 9, -1); if (n < 0) { s.cd = 0.3f; return; } at = _en[n].pos; }
                    if (_scrambleT > 0) { at = _heroPos + ScrambleAim(at - _heroPos); ClampToRoom(ref at, 4); } // v0.0.25: Gossip Imp scramble
                    var T = HellordTuning.I; float r = CurseRadius738(s); var sk = s; float d = dmg; bool ech = _echo738; if (ech) { r *= T.curseEchoRadiusMul738; d *= T.curseEchoDmgMul738; } /* t738u */
                    HellordAudio.Play("curse_rune_arm"); // once per cast, not per rune
                    int runes = ech ? Mathf.Min(HData.At(T.curseRunes, s.level), Mathf.Max(1, T.curseEchoRunes738)) : HData.At(T.curseRunes, s.level); float a0 = Random.value * 360f; // v0.0.27: more runes per cast with skill level, scattered around the aim point
                    for (int k = 0; k < runes; k++)
                    {
                        Vector2 at2 = at; if (k > 0) { at2 = at + (Vector2)(Quaternion.Euler(0, 0, a0 + k * 360f / Mathf.Max(1, runes - 1)) * Vector2.right) * T.curseScatterU * Random.Range(0.75f, 1f); ClampToRoom(ref at2, 4); }
                        if (k == 0) PactRune53(at2, r); HellFX.RuneCircle(at2, CursePx738(r), new Color(0.75f, 0.4f, 1f), 0.6f, () => { RuneBurst(at2, r, d, sk); CurseZone(at2, r, d, sk); });
                    }
                    break;
                }
                case SkillId.ChainLightning: if (!CastChainLightning(s, dmg)) return; break;
                case SkillId.HellfirePillar: if (SinOn818(40)) { SinFireRing818(s, dmg); break; } if (FireWallOn948) { if (!CastFireWall948(s, dmg)) return; break; } /* t948u Fire Wall (Lance = marching wall) */ /* t818u #40 */ if (Has51(PactPath.Pyromancer, 6)) { if (!PactLance53(s, dmg)) return; break; } if (!CastHellfirePillar(s, dmg)) return; break; // v0.0.53 Hellfire Lance
                case SkillId.BloodWhip: if (!CastBloodWhip(s, dmg)) return; break;
                case SkillId.SoulSpears: if (!CastSoulSpears(s, dmg)) return; break;
                case SkillId.GravePool: if (!CastGravePool(s, dmg, mul)) return; break;
            }
            _lastCast[(int)s.def.id] = Time.time;
        }
        // t738u: Curse Rune radius. Area sources add (1 + sum of bonuses) instead of compounding, then capped.
        bool _echo738;
        float CurseRadius738(SkillInst s)
        {
            var T = HellordTuning.I; float b = HData.At(T.curseRadiusU, s.level, 1.8f), m;
            if (T.curseAreaAdditive738)
            {
                float big = s.Has(SupportId.BiggerArea) ? HData.At(T.supAreaMul, s.Lvl(SupportId.BiggerArea), 1.25f) : 1f;
                m = 1f + (big - 1f) + (PArea49 - 1f) + (SlothArea53 - 1f) + (BoneArea536(s.def) - 1f);
            }
            else m = s.AreaMul;
            m = Mathf.Clamp(m, 0.5f, Mathf.Max(1f, T.curseAreaMaxMul738));
            return Mathf.Min(b * m, Mathf.Max(b, T.curseRadiusMaxU738));
        }
        static int CursePx738(float r) { int st = Mathf.Max(1, HellordTuning.I.curseRadiusPxStep738); return Mathf.Max(st, Mathf.RoundToInt(r * 16f / st) * st); }
        int CurseZonesAlive738() { int n = 0; foreach (var z in _zoneAnchors) if (z.gameObject.activeSelf) n++; return n; }
        void RuneBurst(Vector2 at, float r, float dmg, SkillInst s)
        {
            if (mode == Mode.StartPick) return;
            r *= SinRuneR818(); dmg *= SinRuneDmg818(); // t818u #2
            Query(at, r, _sq); SkillArea53(at, r, dmg * 2.5f, (int)s.def.id);
            foreach (int i in _sq) Hit(_en[i], dmg * 2.5f, s, Vector2.zero);
            ProjectileFX.Impact(at, ProjElement.Curse, Vector2.zero); HellordAudio.Play("curse_rune_burst");
        }

        // Curse zone: lingering HellFX.AuraField on a pooled anchor; ticks curse + small damage (visual + our own hit logic)
        readonly List<Transform> _zoneAnchors = new List<Transform>();
        void CurseZone(Vector2 at, float r, float dmg, SkillInst s)
        {
            if (CurseZonesAlive738() >= Mathf.Max(1, HellordTuning.I.curseZoneMax738)) return; // t738u cap
            Transform t = null; foreach (var z in _zoneAnchors) if (!z.gameObject.activeSelf) { t = z; break; }
            if (t == null) { t = new GameObject("curse_zone").transform; _zoneAnchors.Add(t); }
            t.position = at; t.gameObject.SetActive(true); var tt = t; int sfx928 = -1;
            var job = HellFX.AuraField(t, CursePx738(r) * HArt.PPU / 16f, new Color(0.6f, 0.25f, 0.9f, 0.5f), 1.6f * SinDuration737, HellordTuning.I.curseZoneTick738, () =>
            {
                if (sfx928 >= 0) ShaderFxPulse928(sfx928); // t928u
                if (mode != Mode.Play) return; var l = _zoneQ; Query(at, r, l); SkillArea53(at, r, dmg * 0.15f, 21);
                foreach (int i in l) { var e = _en[i]; e.curseT = Mathf.Max(e.curseT, 1f); Damage(e, dmg * 0.15f, false, true); }
            }, 4);
            if (job == null) { t.gameObject.SetActive(false); return; }
            sfx928 = ShaderFxSpawn928(0, at, CursePx738(r) / 16f, 1.6f * SinDuration737); // t928u: shader veins, same radius as the aura sprite; AuraField stays as tick/damage driver
            var jsr = job.sr; if (sfx928 >= 0 && jsr) jsr.enabled = false; int sid = sfx928;
            job.end = _ => { tt.gameObject.SetActive(false); if (jsr) jsr.enabled = true; ShaderFxEnd928(sid); };
        }

        void UpdateOrbit(SkillInst s, float dt)
        {
            if (PactOrbitAway53()) { if (_orbs.Count > 0) { foreach (var o in _orbs) Destroy(o.gameObject); _orbs.Clear(); } if (_orbitJobs != null) KillOrbit(); return; } // v0.0.53 Rune Ossuary / Marrow Launch: bones are elsewhere
            var T = HellordTuning.I; int n = HData.At(T.orbitCount, s.level, 3) + (s.Has(SupportId.MoreOrbitals) ? HData.At(T.supOrbitals, s.Lvl(SupportId.MoreOrbitals)) : 0); // v0.0.27: skulls + radius grow with skill level
            float orad = HData.At(T.orbitRadiusU, s.level, 2f) * (T.combatV49 ? T.orbitAreaMul49d : 1f) * PactOrbitR53(), hitR = T.orbitHitRadiusU;
            float rev = 200f / 360f / s.CdMul; SinOrbit818(ref n, ref orad, ref rev); // t818u #15 / #45
            if (_orbitJobs == null || _orbitN != n || Mathf.Abs(_orbitRev - rev) > 0.001f || Mathf.Abs(_orbitR - orad) > 0.001f) RecastOrbit(n, rev, orad); // allocates: only on recast/level-up
            if (_orbitJobs != null && _orbitJobs.Count == n)
            {   // visuals from HellFX.OrbitWeapons; hit test at the visual's actual positions (our own hit logic)
                float dm = SkillDamage(s);
                if (_orbs.Count > 0) { foreach (var o in _orbs) Destroy(o.gameObject); _orbs.Clear(); }
                for (int k = 0; k < n; k++)
                {
                    var j = _orbitJobs[k]; if (j == null || j.sr == null || !j.sr.gameObject.activeSelf) { RecastOrbit(n, rev, orad); return; }
                    Query(j.sr.transform.position, hitR, _sq); SkillArea53(j.sr.transform.position, hitR, dm, (int)s.def.id);
                    foreach (int i in _sq) { var e = _en[i]; if (e.orbitCd > 0) continue; e.orbitCd = s.def.cooldown * s.CdMul; Hit(e, dm, s, (e.pos - _heroPos).normalized * 3); SinOrbitHit818(s, e, dm); HellordAudio.Play("bone_orbit_hit"); }
                }
                return;
            }
            KillOrbit(); _orbitN = n; _orbitRev = rev; _orbitR = orad; _orbitJobs = new List<HellFXRunner.Job>(); // budget full: fallback to own sprites below, retry next recast
            while (_orbs.Count < n) { var o = NewSR("bone_orb", null, HArt.Proj("proj_bone")[0], 0); _orbs.Add(o); }
            while (_orbs.Count > n) { Destroy(_orbs[_orbs.Count - 1].gameObject); _orbs.RemoveAt(_orbs.Count - 1); }
            _orbAngle += dt * 200f / s.CdMul * (SinOn818(45) ? Mathf.Max(0.05f, HellordTuning.I.sin45Spin818) : 1f);
            float r = orad; float dmg = SkillDamage(s);
            for (int k = 0; k < n; k++)
            {
                float a = (_orbAngle + k * 360f / n) * Mathf.Deg2Rad;
                Vector2 p = HeroCenter + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.75f) * r;
                var tr = _orbs[k].transform; tr.position = p; tr.rotation = Quaternion.Euler(0, 0, _orbAngle * 2); _orbs[k].sortingOrder = YSort(p.y) + 30;
                Query(p, hitR, _sq); SkillArea53(p, hitR, dmg, (int)s.def.id);
                foreach (int i in _sq) { var e = _en[i]; if (e.orbitCd > 0) continue; e.orbitCd = s.def.cooldown * s.CdMul; Hit(e, dmg, s, (e.pos - _heroPos).normalized * 3); SinOrbitHit818(s, e, dmg); HellordAudio.Play("bone_orbit_hit"); }
            }
        }

        // ================= projectiles =================
        void SpawnProj(SkillInst s, Vector2 pos, Vector2 vel, float dmg, int pierce, bool fork, int bounce, float explodeR, int lastHit = -1)
        {
            foreach (var p in _pr)
            {
                if (p.alive) continue;
                p.alive = true; p.s = s; p.pos = pos; p.vel = vel; p.dmg = dmg; p.pierce = pierce; p.fork = fork; p.bounce = bounce; p.explodeR = explodeR;
                p.ret53 = false; p.emp53 = false; p.bone53 = false; _lastProj53 = p; PactSpawnProj53(p); p.homing = s.Has(SupportId.Homing); p.life = 1.6f * PRange49; p.vel *= PSpeed49;
                { var T = HellordTuning.I; int hl = s.Lvl(SupportId.Homing), bl = s.Lvl(SupportId.Bounce), fl = s.Lvl(SupportId.Fork); // v0.0.27: support levels
                  p.homingTurn = HData.At(T.supHomingTurn, hl, 6f); p.homingR = HData.At(T.supHomingRangeU, hl, 5f); p.bounceR = HData.At(T.supBounceRangeU, bl, 4f);
                  p.forkN = fork ? HData.At(T.supForkChildren, fl, 2) : 2; p.forkDmg = HData.At(T.supForkDmg, fl, 0.55f); } p.lastHit = lastHit; p.animT = 0; p.sin818 = 0; p.sinN818 = 0; p.sr.transform.localScale = Vector3.one; SinSpawnProj818(p, lastHit); // t818u #16 #22
                p.frames = HArt.Proj(ProjSheet(s.def.id)); p.sr.sprite = p.frames[0]; p.sr.color = s.def.id == SkillId.SoulSpears ? HellordTuning.I.spearTint : Color.white; p.sr.sortingOrder = 2000; // v0.7: above the y-sorted band
                p.sr.gameObject.SetActive(true); p.sr.enabled = true; // v0.0.10: was swallowed by the comment above -> every projectile was invisible
                p.trail = HellFX.ProjectileTrail(p.sr.transform, s.def.id == SkillId.SoulSpears ? HellordTuning.I.spearTint : new Color(1f, 0.55f, 0.15f), 2f); // pooled job; null when over budget
                p.sr.transform.position = pos; p.sr.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg);
                p.fx44 = -1; p.lg667 = -1; { string fh = FlightHook667(s); if (fh != null) p.lg667 = SL667(fh, pos, vel); } // t667u
                if (s.def.id == SkillId.SoulSpears && T44.fxV44Pack)
                {   // v0.0.44: HeroSkillFX spear replaces the tinted bone sprite + trail
                    EnsureFX44(); p.fx44 = HeroSkillFX.Spear(pos, vel);
                    if (p.fx44 >= 0) { p.sr.enabled = false; if (p.trail != null) { HellFXRunner.I.Kill(p.trail); p.trail = null; } }
                }
                return;
            }
        }
        static void KillProj(Proj p) { p.alive = false; if (p.lg667 >= 0) { SkillLightFX.GlowEnd(p.lg667); p.lg667 = -1; } // t667u
            if (p.fx44 >= 0) { HeroSkillFX.SpearEnd(p.fx44); p.fx44 = -1; } if (p.trail != null) { HellFXRunner.I.Kill(p.trail); p.trail = null; } if (p.sr) p.sr.gameObject.SetActive(false); }
        static string ProjSheet(SkillId id) => id == SkillId.FrostCleave ? "proj_frost" : id == SkillId.StormRing ? "proj_storm" : id == SkillId.BoneOrbit ? "proj_bone" : id == SkillId.CurseRune ? "proj_curse" : id == SkillId.SoulSpears ? "proj_bone" : "proj_fireball";
        void UpdateProjectiles(float dt)
        {
            foreach (var p in _pr)
            {
                if (!p.alive) continue;
                if (!p.sr.gameObject.activeSelf) p.sr.gameObject.SetActive(true); // v0.0.10 guard: a live projectile is always drawn
                p.life -= dt; if (p.life <= 0) { if (PactProjEnd53(p)) continue; KillProj(p); continue; } PactProjFly53(p);
                if (p.homing)
                {
                    int n = Nearest(p.pos, p.homingR, p.lastHit);
                    if (n >= 0) { Vector2 want = (_en[n].pos + Vector2.up * _en[n].hitY - p.pos).normalized * p.vel.magnitude; p.vel = Vector2.Lerp(p.vel, want, 1 - Mathf.Exp(-p.homingTurn * dt)); }
                }
                p.pos += p.vel * dt; p.animT += dt * HArt.ProjFps;
                p.sr.sprite = p.frames[(int)p.animT & 3]; // art faces +X, rotated to velocity below
                p.sr.transform.position = p.pos; p.sr.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(p.vel.y, p.vel.x) * Mathf.Rad2Deg);
                if (p.fx44 >= 0) HeroSkillFX.SpearMove(p.fx44, p.pos); if (p.lg667 >= 0) SkillLightFX.GlowMove(p.lg667, p.pos);
                if (TortureProjHit52(p)) { HellordAudio.Play("fireball_impact"); KillProj(p); continue; } // v0.0.52 ice blocks
                Query(p.pos, 0.3f, _sq);
                int hit = -1; foreach (int i in _sq) if (i != p.lastHit) { hit = i; break; }
                if (hit < 0) continue;
                var e = _en[hit];
                Hit(e, p.dmg, p.s, p.vel.normalized * 2.5f); PactProjHit53(p, hit); if (p.sin818 != 0) SinProjHit818(p, e); // t818u
                if (p.explodeR > 0) { var l = _explQ; Query(p.pos, p.explodeR, l); foreach (int i in l) if (i != hit) Damage(_en[i], p.dmg * HellordTuning.I.fbSplashFrac, true, false); }
                bool spear = p.s.def.id == SkillId.SoulSpears; var el659 = spear ? ProjElement.Soul : p.s.def.id == SkillId.BoneOrbit ? ProjElement.Bone : ProjElement.Fire; /* t659u */ if (p.fx44 >= 0) HeroSkillFX.SpearPierce(p.fx44, p.pos); else {
                    // t1001u/t1002u Fireball hit: HellfireNova puffs sized to splash (readable fire). Soft Glow/Ring skipped (they read as a purple/pink disc under CRT/URP).
                    // PixelAoE mode-2 radial disc is OFF by default (fxShaderFireball928) — same disc look VH rejected. No GrabPass on this path.
                    bool fireExpl = p.explodeR > 0 && el659 == ProjElement.Fire;
                    bool sb928 = fireExpl && HellordTuning.I.fxShaderFireball928 && ShaderFxSpawn928(2, p.pos, p.explodeR, HellordTuning.I.fxFireLifeS928) >= 0;
                    ProjectileFX.SkipNova928 = sb928 && !HellordTuning.I.fxFireKeepNova928; // shader disc replaces nova only if explicitly enabled
                    ProjectileFX.SkipPackFlash988 = fireExpl && HellordTuning.I.fxFireSkipPackFlash988; // always skip Glow/Ring wash on fire splash
                    // t1004u: Fireball (and Fire splash) skip hit-stop by default — many projectiles stacking HitStop felt like lag. Shake stays.
                    ProjectileFX.SkipHitStop = el659 == ProjElement.Fire && !HellordTuning.I.fireballHitStop1004;
                    if (fireExpl) ProjectileFX.NovaRadiusPx = Mathf.Max(10f, p.explodeR * HArt.PPU);
                    ProjectileFX.Impact(p.pos, el659, p.vel);
                    ProjectileFX.SkipNova928 = false; ProjectileFX.SkipPackFlash988 = false; ProjectileFX.SkipHitStop = false; ProjectileFX.NovaRadiusPx = 0;
                } HellordAudio.Play(spear ? "bone_orbit_hit" : "fireball_impact"); // v0.0.36 spears: soul impact, reused bone hit clip
                // order: Pierce > Fork > Bounce
                if (p.pierce > 0) { p.pierce--; p.dmg *= 1f - HellordTuning.I.pierceFalloff * (spear ? HellordTuning.I.spearFalloffMul : 1f); p.lastHit = hit; if (p.fx44 < 0) ProjectileFX.HitPierce(p.pos, p.vel, el659); continue; }
                if (p.fork)
                {
                    p.fork = false; ProjectileFX.HitSplit(p.pos, el659);
                    int fn = Mathf.Max(2, p.forkN); for (int k = 0; k < fn; k++) { SpawnProj(p.s, p.pos, (Vector2)(Quaternion.Euler(0, 0, (k - (fn - 1) * 0.5f) * HellordTuning.I.forkSpreadDeg) * p.vel), p.dmg * p.forkDmg * SinForkDmg818(), 0, false, p.bounce, p.explodeR, hit); if (SinOn818(16) && _lastProj53 != null) _lastProj53.sin818 = 1; } // t818u #16 burning fork children
                    KillProj(p); continue;
                }
                if (p.bounce > 0)
                {
                    int n = Nearest(p.pos, p.bounceR, hit);
                    if (n >= 0) { p.bounce--; p.lastHit = hit; p.vel = (_en[n].pos + Vector2.up * _en[n].hitY - p.pos).normalized * p.vel.magnitude; p.life = 1f; ProjectileFX.HitBounce(p.pos, el659); continue; }
                }
                if (PactProjEnd53(p)) continue; KillProj(p);
            }
        }
    }
}
