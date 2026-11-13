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
        class IceOrb { public Vector2 p, v; public float life, age, dmg, r, burstR, trailLife, trailAcc; public SkillInst s; public int fx = -1; public readonly HashSet<int> hit = new HashSet<int>(); }
        class IcePatch { public Vector2 p; public float r, life, life0, tick; public SkillInst s; }
        class IceNovaRing { public Vector2 c; public float r, rMax, t, dur, dmg, kClose, kFar, chillS, build; public SkillInst s; public readonly HashSet<int> hit = new HashSet<int>(); }
        class IceBombE { public Vector2 p; public float t, dur, pulseT, r, dmg, pulseDmg; public SkillInst s; }
        class IceStormE { public Vector2 c; public float t, dur, r, tick, dmg, age; public SkillInst s; public HellFXRunner.Job aura; }
        class IceFlash { public Vector2 p; public float r, t, dur; public Color col; public float fill; }
        readonly List<IceOrb> _iceOrbs = new List<IceOrb>(); readonly List<IcePatch> _icePatches = new List<IcePatch>(); readonly List<IceNovaRing> _iceNovas = new List<IceNovaRing>();
        readonly List<IceBombE> _iceBombs = new List<IceBombE>(); readonly List<IceFlash> _iceFlash = new List<IceFlash>(); IceStormE _iceStorm;
        readonly List<int> _iceQ = new List<int>(64), _iceQ2 = new List<int>(64); readonly List<SpriteRenderer> _iceSr = new List<SpriteRenderer>(); int _iceSrN;

        void ClearIce()
        {
            foreach (var o in _iceOrbs) if (o.fx >= 0) MinionFX.FrostBoltEnd(o.fx);
            _iceOrbs.Clear(); _icePatches.Clear(); _iceNovas.Clear(); _iceBombs.Clear(); _iceFlash.Clear(); _iceStripOnce.Clear(); StopIceStorm(); _iceSrN = 0; IceHideSr();
        }
        void StopIceStorm() { if (_iceStorm != null && _iceStorm.aura != null) _iceStorm.aura.life = -1; _iceStorm = null; }

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
            if (!StatusFX.IsAlive(e.chillH)) e.chillH = HFX.ChillStatus(e); StatusFX.SetFrozen(e.chillH, true);
        }

        // ================================================================ casts
        bool CastFrostbolt(SkillInst s, float dmg)
        {
            var T = HellordTuning.I;
            if (Nearest(_heroPos, T.frostbRangeU, -1) < 0 && !Aiming()) { s.cd = 0.1f; return false; } // hold fire when nothing is near
            EnsureFX44(); Vector2 dir = AimDir(); Vector2 c = HeroCenter;
            int n = HData.At(T.frostbOrbs, s.level, 1) + PCount49; float spread = n > 1 ? Mathf.Min(T.frostbSpreadDeg, 120f / (n - 1)) : 0f;
            float burstR = HData.At(T.frostbBurstU, s.level, 1.2f) * s.AreaMul, trail = HData.At(T.frostbTrailSeconds, s.level, 1.5f) * SinDuration737;
            for (int i = 0; i < n; i++)
            {
                Vector2 d = (Vector2)(Quaternion.Euler(0, 0, (i - (n - 1) * 0.5f) * spread) * dir); Vector2 p0 = c + d * 0.4f;
                var o = new IceOrb { p = p0, v = d * HData.At(T.frostbSpeedU, s.level, 5.5f), life = T.frostbLifeS, dmg = dmg, r = T.frostbRadiusU, burstR = burstR, trailLife = trail, s = s };
                if (FX44 && IceArt.Get("fx_frostbolt_orb", 16, 16, 12f, true) == null) o.fx = MinionFX.FrostBolt(p0 + Vector2.up * 0.6f, d);
                _iceOrbs.Add(o);
            }
            HellordAudio.Play("frost_cleave", 1.25f); IceVfxCast(c + dir * 0.5f);
            if (!_echo738) s.cd *= HData.At(T.frostbCdMul, s.level, 1f); return true;
        }
        bool CastIceNova(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; float rMax = HData.At(T.novaRadiusU, s.level, 3.2f) * s.AreaMul;
            Query(_heroPos, rMax * 1.15f, _iceQ); if (_iceQ.Count == 0 && !Aiming()) { s.cd = 0.15f; return false; } // pulse only when something is in reach
            int pulses = HData.At(T.novaPulses, s.level, 1);
            for (int k = 0; k < pulses; k++) { float dd = dmg * (k == 0 ? 1f : T.novaPulse2Mul); var sk = s; After(k * T.novaPulseGapS, () => SpawnIceNova(sk, dd, rMax)); }
            if (!_echo738) s.cd *= HData.At(T.novaCdMul, s.level, 1f); return true;
        }
        void SpawnIceNova(SkillInst s, float dmg, float rMax)
        {
            if (mode == Mode.StartPick) return; var T = HellordTuning.I; Vector2 c = _heroPos + Vector2.up * 0.3f;
            _iceNovas.Add(new IceNovaRing { c = c, r = 0, rMax = rMax, dur = T.novaExpandS, dmg = dmg, kClose = T.novaKnockClose, kFar = T.novaKnockFar, chillS = T.novaChillS, build = T.novaBuild, s = s });
            SkillArea53(c, rMax, dmg, (int)s.def.id); HellordAudio.Play("frost_cleave", 0.8f); IceVfxNovaCast(c, rMax);
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
                _iceBombs.Add(new IceBombE { p = at, dur = HData.At(T.bombSeconds, s.level, 2f) * SinDuration737, pulseT = T.bombPulseS, r = r, dmg = dmg * T.bombBlastMul, pulseDmg = dmg * T.bombPulseDmgMul, s = s }); placed++;
                HellordAudio.Play("pool_cast", 1.3f); IceVfxBombDrop(at);
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
                EnsureFX44(); _iceStorm.aura = IceVfxHurricaneStart(r, secs);
            }
            HellordAudio.Play("frost_cleave", 0.6f); if (!_echo738) s.cd *= HData.At(T.hurrCdMul, s.level, 1f); return true;
        }

        // ================================================================ per-frame update (called from Update, inside Z37Begin/End)
        void UpdateIce(float dt)
        {
            if (_iceOrbs.Count + _icePatches.Count + _iceNovas.Count + _iceBombs.Count + _iceFlash.Count + _iceStripOnce.Count == 0 && _iceStorm == null) { if (_iceSrN > 0) { _iceSrN = 0; IceHideSr(); } return; }
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
                if (wall || o.life <= 0f) { IceOrbBurst(o); if (o.fx >= 0) MinionFX.FrostBoltHit(o.fx, o.p + Vector2.up * 0.6f); _iceOrbs.RemoveAt(i); continue; }
                if (o.fx >= 0) MinionFX.FrostBoltMove(o.fx, o.p + Vector2.up * 0.6f); IceVfxOrb(o.p, o.v, o.age, o.fx >= 0);
            }
            // ---- chill ground
            for (int i = _icePatches.Count - 1; i >= 0; i--)
            {
                var p = _icePatches[i]; p.life -= dt; if (p.life <= 0f) { _icePatches.RemoveAt(i); continue; }
                p.tick -= dt; if (p.tick <= 0f) { p.tick = T.frostbPatchTickS; Query(p.p, p.r, _iceQ); foreach (int ei in _iceQ) { var e = _en[ei]; if (e.alive && (e.pos - p.p).magnitude <= p.r + e.radius * 0.5f) IceChill(e, T.frostbPatchChillS, T.frostbPatchBuild); } }
                IceVfxChillGround(p.p, p.r, p.life / p.life0, p.life0 - p.life);
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
                IceVfxNova(n.c, n.r, n.rMax, k); if (k >= 1f) _iceNovas.RemoveAt(i);
            }
            // ---- Frost Bombs: pulse chill while ticking, then detonate
            for (int i = _iceBombs.Count - 1; i >= 0; i--)
            {
                var b = _iceBombs[i]; b.t += dt; b.pulseT -= dt;
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
                    IceVfxBombBurst(b.p, b.r); HellordAudio.Play("fireball_impact", 0.85f); _iceBombs.RemoveAt(i); continue;
                }
                IceVfxBombCrystal(b.p, b.t / b.dur, b.t);
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
            // ---- transient rings (orb burst, bomb pulse / burst)
            for (int i = _iceFlash.Count - 1; i >= 0; i--)
            {
                var f = _iceFlash[i]; f.t += dt; if (f.t >= f.dur) { _iceFlash.RemoveAt(i); continue; }
                float k = f.t / f.dur, rr = f.r * (0.35f + 0.65f * (1f - (1f - k) * (1f - k)));
                if (f.fill > 0f) ZDisc(f.p, rr, new Color(f.col.r, f.col.g, f.col.b, f.fill * (1f - k)), 1f, 2); ZRing(f.p, rr, new Color(f.col.r, f.col.g, f.col.b, f.col.a * (1f - k)), 1f, 3);
            }
            UpdateIceStripOnce(dt); IceHideSr();
        }
        void IceOrbBurst(IceOrb o)
        {
            var T = HellordTuning.I; Query(o.p, o.burstR, _iceQ); float dm = o.dmg * T.frostbBurstMul;
            foreach (int ei in _iceQ) { var e = _en[ei]; if (!e.alive) continue; Vector2 d = e.pos - o.p; if (d.magnitude > o.burstR + e.radius) continue; Hit(e, dm, o.s, d.normalized * T.frostbKnock); IceChill(e, T.frostbChillS, T.frostbBuild); }
            IceVfxOrbBurst(o.p, o.burstR); HellordAudio.Play("fireball_impact", 1.4f);
        }

        // ================================================================ ICE ART & VFX SLOTS  (HA icons / HFX VFX drop in HERE; nothing else needs to change)
        // Placeholder look = pack FX where it fits (Frost Caster bolt, FrostShatter, HellFX.AuraField) + Z37 decals in Endesga blues.
        // Each hook says what it draws every frame vs once. Radii are live (level / Bigger Area), so scalable art must be procedural.
        SpriteRenderer IceSR(Sprite sp, Vector2 pos, int order, Color c)
        {
            if (_iceSrN >= _iceSr.Count) _iceSr.Add(NewSR("ice_sr", null, null, 0)); var sr = _iceSr[_iceSrN++]; if (!sr.gameObject.activeSelf) sr.gameObject.SetActive(true);
            sr.sprite = sp; sr.color = c; sr.sortingOrder = order; sr.transform.position = pos; return sr;
        }
        void IceHideSr() { for (int i = _iceSrN; i < _iceSr.Count; i++) if (_iceSr[i] != null && _iceSr[i].gameObject.activeSelf) _iceSr[i].gameObject.SetActive(false); }
        void IceFlashAdd(Vector2 p, float r, float dur, Color col, float fill = 0f) { if (_iceFlash.Count < 48) _iceFlash.Add(new IceFlash { p = p, r = r, dur = dur, col = col, fill = fill }); }

        /// <summary>ONCE per cast: muzzle puff at the hero.</summary>
        void IceVfxCast(Vector2 p) { HordeFX.CheapBurst(p, IceArt.Cyan, 5); }
        /// <summary>EVERY FRAME per orb in flight. Strip: fx_frostbolt_orb 16x16 x4 loop. Placeholder: MinionFX.FrostBolt (updated by the caller) or two discs.</summary>
        void IceVfxOrb(Vector2 p, Vector2 v, float age, bool packFx)
        {
            var st = IceArt.Get("fx_frostbolt_orb", 16, 16, 12f, true); Vector2 at = p + Vector2.up * 0.6f; ZDisc(p, 0.28f, new Color(0, 0, 0, 0.25f), 0.5f, 2); // ground shadow
            if (st != null) { IceSR(st.At(age), at, YSort(p.y) + 30, Color.white); return; }
            if (packFx) return; ZDisc(at, 0.36f, IceArt.Hex(0x0099db, 0.9f), 1f, 7); ZDisc(at, 0.18f, Color.white, 1f, 8);
        }
        /// <summary>ONCE per enemy pierced.</summary>
        void IceVfxOrbHit(Vector2 p) { HordeFX.CheapBurst(p + Vector2.up * 0.6f, IceArt.Cyan, 4); }
        /// <summary>ONCE when an orb dies on a wall or at the end of its life. r = live burst radius. Strip: fx_frostbolt_burst 48x48 x6 once.</summary>
        void IceVfxOrbBurst(Vector2 p, float r)
        {
            var st = IceArt.Get("fx_frostbolt_burst", 48, 48, 16f, false); if (st != null) { IceStripOnce(st, p + Vector2.up * 0.3f); return; }
            ReactionFX.FrostShatter(p + Vector2.up * 0.4f, 10, Mathf.Clamp(r * HArt.PPU, 8, 28)); IceFlashAdd(p, r, 0.25f, IceArt.Hex(0x2ce8f5, 0.9f), 0.18f);
        }
        /// <summary>EVERY FRAME per chill patch. life01 = remaining 1..0, age in s. Strip: fx_chill_ground 32x32 x3 loop (ground layer).</summary>
        void IceVfxChillGround(Vector2 p, float r, float life01, float age)
        {
            var st = IceArt.Get("fx_chill_ground", 32, 32, 6f, true); float a = Mathf.Clamp01(life01 * 2f);
            if (st != null) { Z37SR(st.At(age), p, Vector2.one, 0, new Color(1, 1, 1, a), 1); return; }
            ZDisc(p, r, IceArt.Hex(0x0099db, 0.22f * a), 0.6f, 1); ZRing(p, r, IceArt.Hex(0x2ce8f5, 0.35f * a), 0.6f, 2);
        }
        /// <summary>ONCE when a nova starts (c = centre, rMax = full radius).</summary>
        void IceVfxNovaCast(Vector2 c, float rMax) { for (int k = 0; k < 6; k++) { float a = k * Mathf.PI / 3f; HordeFX.CheapBurst(c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 0.8f, IceArt.Pale, 4); } }
        /// <summary>EVERY FRAME while the nova ring expands: r = current radius, rMax = final radius, t01 = 0..1. Scalable: procedural / shader.</summary>
        void IceVfxNova(Vector2 c, float r, float rMax, float t01)
        {
            float a = 1f - t01 * 0.7f; ZDisc(c, r, IceArt.Hex(0x0099db, 0.18f * (1f - t01)), 0.85f, 2);
            ZRing(c, r, IceArt.Hex(0x2ce8f5, 0.9f * a), 0.85f, 3); ZRing(c, Mathf.Max(0.1f, r - 0.15f), IceArt.Hex(0xffffff, 0.8f * a), 0.85f, 4);
            for (int k = 0; k < 14; k++) { float an = k * Mathf.PI * 2f / 14f + 0.2f; ZDisc(c + new Vector2(Mathf.Cos(an), Mathf.Sin(an) * 0.85f) * r, 0.1f + 0.06f * (1f - t01), IceArt.Pale, 1f, 5); }
        }
        /// <summary>ONCE when a bomb lands.</summary>
        void IceVfxBombDrop(Vector2 p) { HordeFX.CheapBurst(p + Vector2.up * 0.3f, IceArt.Pale, 6); IceFlashAdd(p, 0.9f, 0.2f, IceArt.Hex(0xc0cbdc, 0.9f)); }
        /// <summary>EVERY FRAME per bomb while it ticks. life01 = elapsed / duration (0..1), age in s. Strip: fx_frostbomb_crystal 24x32 x4 loop, pivot = bottom centre.</summary>
        void IceVfxBombCrystal(Vector2 p, float life01, float age)
        {
            var st = IceArt.Get("fx_frostbomb_crystal", 24, 32, 8f, true, 0f); ZDisc(p, 0.45f, new Color(0, 0, 0, 0.3f), 0.5f, 2);
            float flicker = 0.75f + 0.25f * Mathf.Sin(age * (8f + 14f * life01)); var tint = new Color(1f, 1f, 1f, 1f);
            if (st != null) IceSR(st.At(age), p, YSort(p.y) + 10, tint); else IceSR(IceArt.CrystalPlaceholder(), p, YSort(p.y) + 10, Color.Lerp(Color.white, IceArt.Cyan, 1f - flicker));
            ZRing(p, 0.7f + 0.5f * life01, IceArt.Hex(0x2ce8f5, 0.25f + 0.35f * life01), 0.6f, 3);
        }
        /// <summary>ONCE per chill pulse (r = pulse radius). Scalable.</summary>
        void IceVfxBombPulse(Vector2 p, float r) { IceFlashAdd(p, r, 0.3f, IceArt.Hex(0x2ce8f5, 0.8f), 0.12f); HordeFX.CheapBurst(p + Vector2.up * 0.4f, IceArt.Cyan, 4); }
        /// <summary>ONCE on detonation (r = blast radius). Scalable: procedural / shader.</summary>
        void IceVfxBombBurst(Vector2 p, float r)
        {
            IceFlashAdd(p, r, 0.4f, IceArt.Hex(0xffffff, 1f), 0.35f); IceFlashAdd(p, r * 0.7f, 0.3f, IceArt.Hex(0x2ce8f5, 1f), 0.2f);
            ReactionFX.FrostShatter(p + Vector2.up * 0.4f, 16, Mathf.Clamp(r * HArt.PPU * 0.5f, 10, 28)); for (int k = 0; k < 8; k++) { float a = k * Mathf.PI / 4f; HordeFX.CheapBurst(p + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r * 0.7f, IceArt.Pale, 5); }
        }
        /// <summary>Placeholder fill: HellFX.AuraField following the hero.</summary>
        HellFXRunner.Job IceVfxHurricaneStart(float r, float secs) => _hero != null ? HellFX.AuraField(_hero, r * HArt.PPU, new Color(0.55f, 0.85f, 1f, 0.22f), secs, HellordTuning.I.hurrTickS) : null;
        /// <summary>EVERY FRAME while the storm lasts (c follows the hero; r live; age in s; fade 0..1 over the last 0.5 s). Placeholder: orbiting shards + edge ring (the AuraField fill comes from IceVfxHurricaneStart). Scalable.</summary>
        void IceVfxHurricane(Vector2 c, float r, float age, float fade)
        {
            ZRing(c, r, IceArt.Hex(0x2ce8f5, 0.55f * fade), 0.85f, 3);
            for (int arm = 0; arm < 3; arm++)
                for (int k = 0; k < 7; k++)
                {
                    float u = k / 6f, an = age * 3.2f + arm * Mathf.PI * 2f / 3f + u * 1.7f, rr = r * (0.25f + 0.75f * u);
                    ZDisc(c + new Vector2(Mathf.Cos(an), Mathf.Sin(an) * 0.85f) * rr, 0.07f + 0.07f * u, new Color(0.75f, 0.92f, 1f, (0.35f + 0.5f * u) * fade), 1f, 5);
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
