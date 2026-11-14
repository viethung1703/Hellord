using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    //  ICE SKILLS (t1393u): Frostbolt, Ice Nova, Frost Bomb, Hurricane  +  shared CHILL / FREEZE helper.
    //  Inspired by PoE 2 / Diablo 2 (Docs/skill_research_poe2_d2.md entries 6, 14, 20, 53). All four are auto-fire skills
    //  (SkillId.Frostbolt .. SkillId.Hurricane, ids 12-15) so level-up cards and the test menu pick them up from HData.Skills.
    //
    //  ART / VFX SLOTS  (everything a later pass has to touch lives in the "ICE ART & VFX SLOTS" region below)
    //   Resources/HellordArt/IceSkills/   (PNG, point filter, no compression, 16 px per world unit, NEVER resampled)
    //     icon_frostbolt_24 / icon_icenova_24 / icon_frostbomb_24 / icon_hurricane_24   24x24 framed tile like ui_v51k_skill_icons_24 (HA)
    //     fx_frostbolt_orb       16x16  x4 frames, loop 12 fps     orb in flight (round, drawn unrotated)                      (HFX, optional)
    //     fx_frostbolt_burst     48x48  x6 frames, once 16 fps     burst on wall / expiry, centred                              (HFX, optional)
    //     fx_chill_ground        32x32  x3 frames, loop 6 fps      chill patch left behind by Frostbolt (flat, ground)           (HFX, optional)
    //     fx_frostbomb_crystal   24x32  x4 frames, loop 8 fps      crystal while ticking, feet at the bottom-centre pivot       (HFX, optional)
    //   Scalable effects (radius changes with level / Bigger Area) are NOT strips; they are code hooks IceVfx* that get the live
    //   radius, so a shader / procedural pack effect can replace the placeholder without ever resampling pixel art.
    // =====================================================================================================================
    public static class IceArt
    {
        public const string Dir = "HellordArt/IceSkills/";
        /// <summary>Endesga 32 blues used by every placeholder.</summary>
        public static readonly Color Deep = Hex(0x124e89), Mid = Hex(0x0099db), Cyan = Hex(0x2ce8f5), Pale = Hex(0xc0cbdc), White = Color.white;
        public static Color Hex(int rgb, float a = 1f) => new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, a);

        public sealed class Strip { public Sprite[] f; public float fps; public bool loop; public Sprite At(float age) { int n = f.Length; int i = (int)(age * fps); i = loop ? i % n : Mathf.Min(i, n - 1); return f[i]; } public bool Done(float age) => !loop && (int)(age * fps) >= f.Length; }
        static readonly Dictionary<string, Strip> _strips = new Dictionary<string, Strip>(); static readonly Dictionary<string, Texture2D> _icons = new Dictionary<string, Texture2D>();
        static Texture2D Load(string n) { var t = Resources.Load<Texture2D>(Dir + n); if (t != null) { t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; } return t; }
        /// <summary>Optional strip; null when the file is not there yet (caller draws the placeholder). Frames are laid out left to right, frame size fw x fh.</summary>
        public static Strip Get(string name, int fw, int fh, float fps, bool loop, float pivotY = 0.5f)
        {
            if (_strips.TryGetValue(name, out var s)) return s;
            var t = Load(name); if (t != null && t.height >= fh && t.width >= fw)
            {
                int n = t.width / fw; s = new Strip { f = new Sprite[n], fps = fps, loop = loop };
                for (int i = 0; i < n; i++) s.f[i] = Sprite.Create(t, new Rect(i * fw, t.height - fh, fw, fh), new Vector2(0.5f, pivotY), HArt.PPU, 0, SpriteMeshType.FullRect);
            }
            return _strips[name] = s;
        }
        public static Texture2D Icon24(SkillId id)
        {
            string n; switch (id) { case SkillId.Frostbolt: n = "icon_frostbolt_24"; break; case SkillId.IceNova: n = "icon_icenova_24"; break; case SkillId.FrostBomb: n = "icon_frostbomb_24"; break; case SkillId.Hurricane: n = "icon_hurricane_24"; break; default: return null; }
            if (!_icons.TryGetValue(n, out var t)) _icons[n] = t = Load(n); return t;
        }
        public static bool IsIce(SkillId id) => id >= SkillId.Frostbolt && id <= SkillId.Hurricane;
        /// <summary>Placeholder crystal (procedural, 11x19 px).</summary>
        static Sprite _crystal;
        public static Sprite CrystalPlaceholder()
        {
            if (_crystal != null) return _crystal;
            const int W = 11, H = 19; var t = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp }; var px = new Color32[W * H];
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    float hw = y < 12 ? 1f + y * 0.45f : Mathf.Max(0f, (H - y) * 0.75f); float dx = x - 5f; bool inside = Mathf.Abs(dx) <= hw && y < H - 1; if (!inside) continue;
                    bool edge = Mathf.Abs(dx) > hw - 1f || y == 0 || y == H - 2; Color c = edge ? Deep : dx < -0.5f ? Cyan : dx > 1.5f ? Mid : Pale; if (y > 14 && !edge) c = White; px[y * W + x] = c;
                }
            t.SetPixels32(px); t.Apply(false, true); return _crystal = Sprite.Create(t, new Rect(0, 0, W, H), new Vector2(0.5f, 0f), HArt.PPU, 0, SpriteMeshType.FullRect);
        }
    }

    public partial class HellordGame
    {
        // ---------------------------------------------------------------- entities
        class IceOrb { public Vector2 p, v; public float life, age, dmg, r, burstR, trailLife, trailAcc; public SkillInst s; public readonly HashSet<int> hit = new HashSet<int>(); }
        class IcePatch { public Vector2 p; public float r, life, life0, tick; public SkillInst s; public bool drawn; }
        class IceNovaRing { public Vector2 c; public float r, rMax, t, dur, dmg, kClose, kFar, chillS, build; public bool spawned; public int pulse; public SkillInst s; public readonly HashSet<int> hit = new HashSet<int>(); }
        class IceBombE { public Vector2 p; public float t, dur, pulseT, r, dmg, pulseDmg; public SkillInst s; public IceFX3.Inst fx; public Enemy tgt; }
        class IceStormE { public Vector2 c; public float t, dur, r, tick, dmg, age; public SkillInst s; }
        readonly List<IceOrb> _iceOrbs = new List<IceOrb>(); readonly List<IcePatch> _icePatches = new List<IcePatch>(); readonly List<IceNovaRing> _iceNovas = new List<IceNovaRing>();
        readonly List<IceBombE> _iceBombs = new List<IceBombE>(); IceStormE _iceStorm;
        readonly List<int> _iceQ = new List<int>(64), _iceQ2 = new List<int>(64), _iceQ3 = new List<int>(32); readonly List<SpriteRenderer> _iceSr = new List<SpriteRenderer>(); int _iceSrN;

        void ClearIce()
        {
            _iceOrbs.Clear(); _icePatches.Clear(); _iceNovas.Clear(); _iceBombs.Clear(); _iceStripOnce.Clear(); StopIceStorm(); _iceSrN = 0; IceHideSr(); IceFX3.Clear();
        }
        void StopIceStorm() { _iceStorm = null; }

        // ================================================================ shared CHILL / FREEZE
        // CHILL: move speed x0.65 (existing rule in UpdateEnemies) + cool tint, duration REFRESHES (never shortens).
        // FREEZE: IceChill adds `build` per application; at HellordTuning.iceFreezeAt the enemy freezes (cannot move) for a short time,
        //         then needs iceFreezeGraceBuild extra build before it can freeze again. Bosses freeze for only 40% as long.
        // Frozen demons that die shatter (existing rule: 35% max HP blast). Reusable by any future cold skill / pact / support.
        void IceChill(Enemy e, float secs, float build, float freezeSecs = -1f)
        {
            if (e == null || !e.alive) return; var T = HellordTuning.I;
            if (e.chillT <= 0 && e.freezeT <= 0) e.iceBuild = Mathf.Min(0f, e.iceBuild); // buildup lapses once the chill is gone (a freeze grace debt is kept)
            e.chillT = Mathf.Max(e.chillT, secs); e.iceBuild += build;
            if (e.iceBuild >= T.iceFreezeAt && e.freezeT <= 0f) { e.iceBuild = -T.iceFreezeGraceBuild; IceFreeze(e, freezeSecs > 0 ? freezeSecs : T.iceFreezeSeconds); return; }
            e.iceBuild = Mathf.Min(e.iceBuild, T.iceFreezeAt); if (!StatusFX.IsAlive(e.chillH)) e.chillH = HFX.ChillStatus(e);
        }
        void IceFreeze(Enemy e, float secs)
        {
            if (e == null || !e.alive) return; if (e.boss) secs *= 0.4f;
            e.freezeT = Mathf.Max(e.freezeT, secs); e.chillT = Mathf.Max(e.chillT, secs + 0.5f);
            if (!StatusFX.IsAlive(e.chillH)) e.chillH = HFX.ChillStatus(e); if (e.chillH >= 0) StatusFX.SetFrozen(e.chillH, true);
        }

        // ================================================================ casts
        bool CastFrostbolt(SkillInst s, float dmg)
        {
            var T = HellordTuning.I;
            if (Nearest(_heroPos, T.frostbRangeU, -1) < 0 && !Aiming()) { s.cd = 0.1f; return false; } // hold fire when nothing is near
            EnsureFX44(); Vector2 dir = AimDir(); Vector2 c = HeroCenter + new Vector2(dir.x >= 0f ? 0.45f : -0.45f, 0.45f); // t1410u: leaves the hero's casting hand (on the facing side), not the body centre
            int n = HData.At(T.frostbOrbs, s.level, 1) + PCount49; float spread = n > 1 ? Mathf.Min(T.frostbSpreadDeg, 120f / (n - 1)) : 0f;
            float burstR = HData.At(T.frostbBurstU, s.level, 1.2f) * s.AreaMul, trail = HData.At(T.frostbTrailSeconds, s.level, 1.5f) * SinDuration737;
            for (int i = 0; i < n; i++)
            {
                Vector2 d = (Vector2)(Quaternion.Euler(0, 0, (i - (n - 1) * 0.5f) * spread) * dir); Vector2 p0 = c + d * 0.25f;
                var o = new IceOrb { p = p0, v = d * HData.At(T.frostbSpeedU, s.level, 5.5f), life = T.frostbLifeS, dmg = dmg, r = T.frostbRadiusU, burstR = burstR, trailLife = trail, s = s };
                _iceOrbs.Add(o);
            }
            HellordAudio.Play("frost_cleave", 1.25f); IceVfxCast(c + dir * 0.35f);
            if (!_echo738) s.cd *= HData.At(T.frostbCdMul, s.level, 1f); return true;
        }
        bool CastIceNova(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; float rMax = HData.At(T.novaRadiusU, s.level, 3.2f) * s.AreaMul;
            Query(_heroPos, rMax * 1.15f, _iceQ); if (_iceQ.Count == 0 && !Aiming()) { s.cd = 0.15f; return false; } // pulse only when something is in reach
            int pulses = HData.At(T.novaPulses, s.level, 1);
            for (int k = 0; k < pulses; k++) { float dd = dmg * (k == 0 ? 1f : T.novaPulse2Mul); var sk = s; int pk = k; After(k * T.novaPulseGapS, () => SpawnIceNova(sk, dd, rMax, pk)); }
            if (!_echo738) s.cd *= HData.At(T.novaCdMul, s.level, 1f); return true;
        }
        void SpawnIceNova(SkillInst s, float dmg, float rMax, int pulse = 0)
        {
            if (mode == Mode.StartPick) return; var T = HellordTuning.I; Vector2 c = _heroPos + Vector2.up * 0.3f;
            _iceNovas.Add(new IceNovaRing { c = c, r = 0, rMax = rMax, dur = T.novaExpandS, dmg = dmg, kClose = T.novaKnockClose, kFar = T.novaKnockFar, chillS = T.novaChillS, build = T.novaBuild, pulse = pulse, s = s });
            SkillArea53(c, rMax, dmg, (int)s.def.id); HellordAudio.Play("frost_cleave", 0.8f); if (pulse == 0) { IceVfxNovaCast(c, rMax); ScreenShake.Add(3, 0.18f); } // t1412u: small kick on cast
        }
        bool CastFrostBomb(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; Query(_heroPos, T.bombRangeU, _iceQ); if (_iceQ.Count == 0) { s.cd = 0.25f; return false; }
            float r = HData.At(T.bombRadiusU, s.level, 2f) * s.AreaMul; int n = HData.At(T.bombCount, s.level, 1) + PCount49; int placed = 0;
            for (int b = 0; b < n; b++)
            {   // densest of a few random samples, not on top of a live bomb or one placed this cast
                int best = -1, bestScore = -1;
                for (int k = 0; k < T.bombSamples; k++)
                {
                    int ci = _iceQ[Random.Range(0, _iceQ.Count)]; Vector2 cp = _en[ci].pos; bool near = false;
                    foreach (var q in _iceBombs) if ((q.p - cp).sqrMagnitude < r * r) { near = true; break; }
                    if (near) continue; Query(cp, r, _iceQ2); if (_iceQ2.Count > bestScore) { bestScore = _iceQ2.Count; best = ci; }
                }
                if (best < 0) break; Vector2 at = _en[best].pos; ClampToRoom(ref at, 6);
                var be = new IceBombE { tgt = _en[best], p = at, dur = HData.At(T.bombSeconds, s.level, 2f) * SinDuration737, pulseT = T.bombPulseS, r = r, dmg = dmg * T.bombBlastMul, pulseDmg = dmg * T.bombPulseDmgMul, s = s }; _iceBombs.Add(be); placed++;
                HellordAudio.Play("pool_cast", 1.3f); be.fx = IceVfxBombDrop(at, r, be.dur);
            }
            if (placed == 0) { s.cd = 0.3f; return false; }
            if (!_echo738) s.cd *= HData.At(T.bombCdMul, s.level, 1f); return true;
        }
        bool CastHurricane(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; Query(_heroPos, T.hurrRangeU, _iceQ); if (_iceQ.Count == 0 && _iceStorm == null && !Aiming()) { s.cd = 0.25f; return false; }
            float r = HData.At(T.hurrRadiusU, s.level, 2.4f) * s.AreaMul, secs = HData.At(T.hurrSeconds, s.level, 4f) * SinDuration737;
            if (_iceStorm != null && _iceStorm.s == s) { _iceStorm.t = _iceStorm.dur = secs; _iceStorm.r = r; _iceStorm.dmg = dmg; }
            else
            {
                StopIceStorm(); _iceStorm = new IceStormE { c = _heroPos, t = secs, dur = secs, r = r, tick = 0.15f, dmg = dmg, s = s };
                IceVfxHurricaneStart(r, secs);
            }
            HellordAudio.Play("frost_cleave", 0.6f); if (!_echo738) s.cd *= HData.At(T.hurrCdMul, s.level, 1f); return true;
        }

        // ================================================================ per-frame update (called from Update, inside Z37Begin/End)
        void UpdateIce(float dt)
        {
            IceStatusPass();
            if (_iceOrbs.Count + _icePatches.Count + _iceNovas.Count + _iceBombs.Count + _iceStripOnce.Count == 0 && _iceStorm == null) { if (_iceSrN > 0) { _iceSrN = 0; IceHideSr(); } return; }
            var T = HellordTuning.I; _iceSrN = 0;
            // ---- Frostbolt orbs: pierce everything, drop chill ground, burst on wall / expiry
            for (int i = _iceOrbs.Count - 1; i >= 0; i--)
            {
                var o = _iceOrbs[i]; o.age += dt; o.life -= dt; Vector2 prev = o.p; o.p += o.v * dt; bool wall = false;
                if (_room != null) { _room.Walkable(6, out var lo, out var hi); if (o.p.x < lo.x || o.p.x > hi.x || o.p.y < lo.y || o.p.y > hi.y) wall = true; } // room bounds only (orbs fly over pits)
                Query(o.p, o.r + 0.6f, _iceQ);
                foreach (int ei in _iceQ)
                {
                    var e = _en[ei]; if (!e.alive || o.hit.Contains(e.id) || (e.pos - o.p).magnitude > o.r + e.radius) continue;
                    o.hit.Add(e.id); Hit(e, o.dmg, o.s, o.v.normalized * T.frostbKnock); IceChill(e, T.frostbChillS, T.frostbBuild); IceVfxOrbHit(o.p);
                }
                o.trailAcc += (o.p - prev).magnitude; if (o.trailAcc >= T.frostbTrailGapU && o.trailLife > 0.05f)
                {
                    o.trailAcc = 0; if (_icePatches.Count >= T.frostbMaxPatches) _icePatches.RemoveAt(0);
                    _icePatches.Add(new IcePatch { p = o.p, r = T.frostbPatchU * Mathf.Sqrt(o.s.AreaMul), life = o.trailLife, life0 = o.trailLife, tick = 0.05f, s = o.s });
                }
                if (wall || o.life <= 0f) { IceOrbBurst(o); _iceOrbs.RemoveAt(i); continue; }
                IceVfxOrb(o.p, o.v, o.age);
            }
            // ---- chill ground
            for (int i = _icePatches.Count - 1; i >= 0; i--)
            {
                var p = _icePatches[i]; p.life -= dt; if (p.life <= 0f) { _icePatches.RemoveAt(i); continue; }
                p.tick -= dt; if (p.tick <= 0f) { p.tick = T.frostbPatchTickS; Query(p.p, p.r, _iceQ); foreach (int ei in _iceQ) { var e = _en[ei]; if (e.alive && (e.pos - p.p).magnitude <= p.r + e.radius * 0.5f) IceChill(e, T.frostbPatchChillS, T.frostbPatchBuild); } }
                IceVfxChillGround(p);
            }
            // ---- Ice Nova rings: hit each demon once as the ring passes it, knock-back falls off with distance
            for (int i = _iceNovas.Count - 1; i >= 0; i--)
            {
                var n = _iceNovas[i]; n.t += dt; float k = Mathf.Clamp01(n.t / n.dur); n.r = n.rMax * (1f - (1f - k) * (1f - k)); // ease-out
                Query(n.c, n.r + 0.8f, _iceQ);
                foreach (int ei in _iceQ)
                {
                    var e = _en[ei]; if (!e.alive || n.hit.Contains(e.id)) continue; Vector2 d = e.pos - n.c; float dist = d.magnitude; if (dist > n.r + e.radius) continue;
                    n.hit.Add(e.id); Vector2 dir = dist > 0.01f ? d / dist : Vector2.up; float kn = Mathf.Lerp(n.kClose, n.kFar, Mathf.Clamp01(dist / n.rMax));
                    Hit(e, n.dmg, n.s, dir * kn); IceChill(e, n.chillS, n.build);
                }
                IceVfxNova(n); if (k >= 1f) _iceNovas.RemoveAt(i);
            }
            // ---- Frost Bombs: pulse chill while ticking, then detonate
            for (int i = _iceBombs.Count - 1; i >= 0; i--)
            {
                var b = _iceBombs[i]; b.t += dt; b.pulseT -= dt;
                // t1410u: the crystal sticks to its demon and rides with it (it used to stay where the cluster WAS and the demons walked out of the blast: 0 hit, 0 kills). If that demon dies it hops to the nearest one in reach.
                if (b.tgt != null && (!b.tgt.alive || b.tgt.dying)) { int nx = Nearest(b.p, 2.5f, -1); b.tgt = nx >= 0 ? _en[nx] : null; }
                if (b.tgt != null) { b.p = b.tgt.pos; ClampToRoom(ref b.p, 6); }
                if (b.fx != null) b.fx.c = b.p;
                if (b.pulseT <= 0f && b.t < b.dur)
                {
                    b.pulseT += T.bombPulseS; float pr = b.r * T.bombPulseRadiusMul; Query(b.p, pr, _iceQ);
                    foreach (int ei in _iceQ) { var e = _en[ei]; if (e.alive && (e.pos - b.p).magnitude <= pr + e.radius) { Hit(e, b.pulseDmg, b.s, Vector2.zero, true); IceChill(e, T.bombChillS, T.bombPulseBuild); } }
                    IceVfxBombPulse(b.p, pr);
                }
                if (b.t >= b.dur)
                {
                    Query(b.p, b.r, _iceQ); SkillArea53(b.p, b.r, b.dmg, (int)b.s.def.id);
                    foreach (int ei in _iceQ)
                    {
                        var e = _en[ei]; if (!e.alive) continue; Vector2 d = e.pos - b.p; float dist = d.magnitude; if (dist > b.r + e.radius) continue;
                        float dm = b.dmg * (e.chillT > 0f || e.freezeT > 0f ? T.bombChilledBonus : 1f); Hit(e, dm, b.s, (dist > 0.01f ? d / dist : Vector2.up) * T.bombKnock); IceFreeze(e, T.bombFreezeS);
                    }
                    IceVfxBombBurst(b.p, b.r, b.fx); HellordAudio.Play("fireball_impact", 0.85f); _iceBombs.RemoveAt(i); continue;
                }
                IceVfxBombCrystal(b.p, b.t / b.dur, b.t, b.dur, b.fx);
            }
            // ---- Hurricane: follows the hero, ticking damage + chill
            if (_iceStorm != null)
            {
                var st = _iceStorm; st.t -= dt; st.age += dt; st.c = _heroPos + Vector2.up * 0.3f;
                if (st.t <= 0f) StopIceStorm();
                else
                {
                    st.tick -= dt;
                    if (st.tick <= 0f)
                    {
                        st.tick = T.hurrTickS; Query(st.c, st.r, _iceQ);
                        foreach (int ei in _iceQ) { var e = _en[ei]; if (e.alive && (e.pos - st.c).magnitude <= st.r + e.radius * 0.5f) { Hit(e, st.dmg, st.s, Vector2.zero, true); IceChill(e, T.hurrChillS, T.hurrBuild); } }
                    }
                    IceVfxHurricane(st.c, st.r, st.age, Mathf.Clamp01(st.t / 0.5f));
                }
            }
            UpdateIceStripOnce(dt); IceHideSr();
        }
        void IceOrbBurst(IceOrb o)
        {
            var T = HellordTuning.I; Query(o.p, o.burstR, _iceQ); float dm = o.dmg * T.frostbBurstMul;
            foreach (int ei in _iceQ) { var e = _en[ei]; if (!e.alive) continue; Vector2 d = e.pos - o.p; if (d.magnitude > o.burstR + e.radius) continue; Hit(e, dm, o.s, d.normalized * T.frostbKnock); IceChill(e, T.frostbChillS, T.frostbBuild); }
            IceVfxOrbBurst(o.p, o.burstR); HellordAudio.Play("fireball_impact", 1.4f);
        }

        // ================================================================ ICE VFX  (t1410u: CPU pixel-art sprites, see HellordIceFX3.cs; replaces the t1409u shader)
        // Radii / durations come from the live gameplay values the callers pass in (level, Bigger Area, Duration scale automatically). HellordTuning.iceVfx1410 = off -> minimal fallbacks.
        SpriteRenderer IceSR(Sprite sp, Vector2 pos, int order, Color c)
        {
            if (_iceSrN >= _iceSr.Count) _iceSr.Add(NewSR("ice_sr", null, null, 0)); var sr = _iceSr[_iceSrN++]; if (!sr.gameObject.activeSelf) sr.gameObject.SetActive(true);
            sr.sprite = sp; sr.color = c; sr.sortingOrder = order; sr.transform.position = pos; return sr;
        }
        void IceHideSr() { for (int i = _iceSrN; i < _iceSr.Count; i++) if (_iceSr[i] != null && _iceSr[i].gameObject.activeSelf) _iceSr[i].gameObject.SetActive(false); }

        /// <summary>ONCE per cast: small muzzle puff at the casting hand.</summary>
        void IceVfxCast(Vector2 p) { HordeFX.CheapBurst(p, IceArt.Cyan, 4); }
        /// <summary>EVERY FRAME per orb in flight: crystal-cored cold bolt with a comet tail. Fallback (effect off): two small discs.</summary>
        void IceVfxOrb(Vector2 p, Vector2 v, float age)
        {
            if (IceFX3.Ok) { IceFX3.Orb(p + Vector2.up * 0.08f, v, age, YSort(p.y) + 30); return; }
            Vector2 at = p + Vector2.up * 0.1f; ZDisc(at, 0.3f, IceArt.Hex(0x0099db, 0.9f), 1f, 7); ZDisc(at, 0.15f, Color.white, 1f, 8);
        }
        /// <summary>ONCE per enemy pierced.</summary>
        void IceVfxOrbHit(Vector2 p) { HordeFX.CheapBurst(p + Vector2.up * 0.6f, IceArt.Cyan, 4); }
        /// <summary>ONCE when an orb dies on a wall or at the end of its life. r = live burst radius: ring of crystal shards + a brief frost film.</summary>
        void IceVfxOrbBurst(Vector2 p, float r)
        {
            float rp = Mathf.Clamp(r * HArt.PPU, 8f, 28f); if (!IceFX3.Ok) { ReactionFX.FrostShatter(p + Vector2.up * 0.4f, 8, rp); return; }
            IceFX3.ShardBurst(p + Vector2.up * 0.1f, rp, p.x * 0.37f + 2.3f); HordeFX.CheapBurst(p + Vector2.up * 0.3f, IceArt.Cyan, 5);
        }
        /// <summary>EVERY FRAME per chill patch: a few flat frost plates + 2 tiny spikes that melt away over the last 40 % of the life. The chill AREA (p.r) is unchanged.</summary>
        void IceVfxChillGround(IcePatch pt)
        {
            if (!IceFX3.Ok) return; IceFX3.Patch(pt.p, Mathf.Max(8f, pt.r * HArt.PPU * 0.8f), pt.life / pt.life0, (int)(pt.p.x * 7f + pt.p.y * 13f));
        }
        /// <summary>ONCE per pulse (the cast): six small shard squares around the hero. No ring.</summary>
        void IceVfxNovaCast(Vector2 c, float rMax) { for (int k = 0; k < 6; k++) { float a = k * Mathf.PI / 3f; HordeFX.CheapBurst(c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 0.8f, IceArt.Pale, 4); } }
        /// <summary>EVERY FRAME while a nova pulse expands; acts on the FIRST frame only (the effect owns its time after that): a wave of small ice spikes rising as the front passes + light frost film, a thin travelling front line.</summary>
        void IceVfxNova(IceNovaRing n)
        {
            if (n.spawned) return; n.spawned = true; if (!IceFX3.Ok) return;
            float rp = n.rMax * HArt.PPU, front = Mathf.Max(3f, n.dur * 30f); bool second = n.pulse > 0;
            IceFX3.Shock(n.c, rp, front, YSort(n.c.y - n.rMax) + 80, n.c.x * 0.31f + 3f); // t1412u impulse ring above the actors
            IceFX3.Field(n.c + Vector2.down * 0.05f, rp * 0.9f, 52f, 26f, 18f, n.c.x * 0.13f + 4f, second ? 0.2f : 0.75f);
            IceFX3.Crest(n.c, rp, Mathf.Clamp((int)(rp / 1.6f), 26, 64), Mathf.Clamp(rp * 0.2f, 9f, 15f), front, 0.22f, 0.06f, second ? 0.8f : 1f, n.c.x * 0.21f + 9f + (second ? 5f : 0f), 40f, 24f, 14f);
        }
        /// <summary>ONCE when a bomb lands: puff + a light frost film under the crystal that lives for the fuse and melts after the blast. Returns it so the crystal hook can grow / flicker it.</summary>
        IceFX3.Inst IceVfxBombDrop(Vector2 p, float r, float dur)
        {
            HordeFX.CheapBurst(p + Vector2.up * 0.3f, IceArt.Pale, 6); if (!IceFX3.Ok) return null; float f = dur * 30f;
            return IceFX3.Field(p, r * HArt.PPU, f + 30f, f + 8f, 16f, p.x * 0.17f + 7f, 0.55f);
        }
        /// <summary>EVERY FRAME per bomb while it ticks (life01 = elapsed / duration, age in s). The crystal strip fx_frostbomb_crystal is kept. Frost creeps wider each pulse; the last 0.5 s the plates flicker lit / dark.</summary>
        void IceVfxBombCrystal(Vector2 p, float life01, float age, float dur, IceFX3.Inst fx)
        {
            var st = IceArt.Get("fx_frostbomb_crystal", 24, 32, 8f, true, 0f);
            float flicker = 0.75f + 0.25f * Mathf.Sin(age * (8f + 14f * life01));
            if (st != null) IceSR(st.At(age), p, YSort(p.y) + 10, Color.white); else IceSR(IceArt.CrystalPlaceholder(), p, YSort(p.y) + 10, Color.Lerp(Color.white, IceArt.Cyan, 1f - flicker));
            if (fx == null) return; float f = age * 30f; int pulses = Mathf.Clamp(Mathf.FloorToInt(age / Mathf.Max(0.05f, HellordTuning.I.bombPulseS)), 0, 3);
            fx.reach = fx.r * 0.85f * (pulses == 0 ? 0.45f : pulses == 1 ? 0.65f : pulses == 2 ? 0.85f : 1f) * Mathf.Min(1f, (f + 1f) / 4f);
            float rem = dur - age; fx.bright = rem < 0.5f && ((int)f % 6) < 3 ? 1f : 0f;
        }
        /// <summary>ONCE per chill pulse (r = pulse radius, 1.1 x blast radius): a thin travelling band of dots and tiny spikes + 5 CPU squares.</summary>
        void IceVfxBombPulse(Vector2 p, float r)
        {
            HordeFX.CheapBurst(p + Vector2.up * 0.4f, IceArt.Pale, 5); if (!IceFX3.Ok) return;
            IceFX3.Band(p, r * HArt.PPU, p.x * 0.11f + 1f);
        }
        /// <summary>ONCE on detonation (r = blast radius): bright flash, ring of ice spikes, flying crystal chunks, 8 CPU shards. Clearly readable (~0.6 s).</summary>
        void IceVfxBombBurst(Vector2 p, float r, IceFX3.Inst fx)
        {
            if (fx != null) { fx.reach = fx.r * 0.85f; fx.bright = 0f; }
            float rp = r * HArt.PPU; ReactionFX.FrostShatter(p + Vector2.up * 0.4f, 8, Mathf.Clamp(rp * 0.3f, 8f, 16f));
            for (int k = 0; k < 6; k++) { float a = k * Mathf.PI / 3f + 0.3f; HordeFX.CheapBurst(p + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r * 0.6f, IceArt.Cyan, 4); }
            if (!IceFX3.Ok) return; IceFX3.BlastBurst(p, rp, p.x * 0.19f + 21f);
        }
        /// <summary>ONCE when the Hurricane starts (the wind vortex is drawn every frame by IceVfxHurricane).</summary>
        void IceVfxHurricaneStart(float r, float secs) { }
        /// <summary>EVERY FRAME while the storm lasts (c follows the hero; r live; age in s; fade 0..1 over the last 0.5 s): swirling wind streaks, dotted rim arcs and blown snow spiralling up round the hero. Thin, nothing filled.</summary>
        void IceVfxHurricane(Vector2 c, float r, float age, float fade)
        {
            if (!IceFX3.Ok) { ZRing(c, r, IceArt.Hex(0x2ce8f5, 0.55f * fade), 0.85f, 3); return; }
            IceFX3.Vortex(_heroPos, r * HArt.PPU, age, fade, (_heroBase != null ? _heroBase.sortingOrder : YSort(_heroPos.y)) + 3);
        }
        /// <summary>Chill / freeze chips on demons (non-boss; bosses keep the pack overlay): a few tiny ice crystals at the feet and shoulders. The demon sprite itself gets a light cold tint (UpdateEnemies).</summary>
        void IceStatusPass()
        {
            if (!IceFX3.Ok) return; int drawn = 0;
            for (int i = 0; i < maxEnemies && drawn < 40; i++)
            {
                var e = _en[i]; if (!e.alive || e.dying || e.boss || e.sr == null || e.sr.sprite == null || (e.chillT <= 0f && e.freezeT <= 0f) || !e.sr.gameObject.activeInHierarchy) continue;
                drawn++; IceFX3.Chips(e.tr.position, e.radius * HArt.PPU, e.freezeT > 0f ? 3 : 1, e.id, runTime, e.sr.sortingOrder + 1);
            }
        }
        void IceStripOnce(IceArt.Strip st, Vector2 p) { _iceStripOnce.Add(new IceOnce { st = st, p = p }); }
        class IceOnce { public IceArt.Strip st; public Vector2 p; public float t; }
        readonly List<IceOnce> _iceStripOnce = new List<IceOnce>();
        void UpdateIceStripOnce(float dt) { for (int i = _iceStripOnce.Count - 1; i >= 0; i--) { var o = _iceStripOnce[i]; o.t += dt; if (o.st.Done(o.t)) { _iceStripOnce.RemoveAt(i); continue; } IceSR(o.st.At(o.t), o.p, YSort(o.p.y) + 40, Color.white); } }

        // ---------------------------------------------------------------- 24 px tile (skill bar / cards / support target pick)
        /// <summary>Draws the 24x24 tile for an ice skill at 1x (dw = 24) or 2x (dw = 48). False when the skill is not an ice skill.</summary>
        bool IceIconBlit(SkillId id, float x, float y, float dw)
        {
            if (!IceArt.IsIce(id)) return false; var t = IceArt.Icon24(id); if (t == null) return true; // missing art: draw nothing (never garbage)
            if (dw <= 24.5f) Blit(t, 0, 0, 24, 24, x, y); else BlitInset(t, 0, 0, 24, 24, x, y, dw, dw); return true;
        }
    }
}
