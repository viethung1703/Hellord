using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

// v0.0.46 (VH-approved): per-boss projectile patterns replacing the shared ring / fan / double ring / spiral cycle.
// Master toggle HellordTuning.bossPatternsV46 (off = old cycle in HellordBossBullets.cs). Every number lives in HellordTuning (bp46*, ox*, g*, q*, fo*, hLasso*).
// Art: ALL projectile sprites here are code-generated PLACEHOLDERS (Endesga 32), named "PLACEHOLDER46_*", so real art can replace them.
// Tells are ground decals in the boss colour (Ox orange, Gorilla cyan, Qilin gold, Frost Ox pale blue, Horse-Face purple), never shorter than bp46MinTellS.
namespace Hellord
{
    public partial class HellordGame
    {
        // kinds: 0 straight bullet, 1 bone chunk (splits on landing), 2 sky drop (icicle / meteor), 3 boulder (bounces), 4 fire puddle, 5 ground spike (blocks), 6 orbit bead (spokes / lasso)
        class P46
        {
            public bool alive; public int kind, pat, look, bounces; public Vector2 p, v, c, tgt; public float life, r, dmg, t, delay, ang, rad, w, radSpd, revAt, home, tick, fall; public bool hit, chill, follow, xSplit;
            public SpriteRenderer sr;
        }
        class Tell46 { public int shape, pat; public Vector2 a, b; public float r, t, tot, deg, gap; public Color col; }
        readonly List<P46> _p46 = new List<P46>(); readonly List<Tell46> _t46 = new List<Tell46>();
        Transform _p46Root; Sprite[] _p46Spr; int _bp46Next = -1, _bp46Cnt; float _bp46Cd = 2.5f, _bp46TellT = -1, _wake46, _trail46; int _trailN46, _bp46Waves; float _bp46WaveT; bool _qCharging46;
        static HellordTuning T46 => HellordTuning.I;
        bool Rage46 => _raging || _enr44;
        float TellS46(float s) => Mathf.Max(T46.bp46MinTellS, Rage46 ? s * T46.bp46RageTempo : s);
        float Dmg46(float mul = 1f) => BBulletDmg * T46.bp46DmgMul * mul;
        Color BossCol46 => IsGorilla ? E32(0x2ce8f5) : IsQilin ? E32(0xfeae34) : IsFrostOx ? E32(0xc0cbdc) : IsHorse ? E32(0xb55088) : E32(0xf77622);
        Vector2 HeroC46 => _heroPos + Vector2.up * 0.6f;

        // ---------- placeholder sprites ----------
        // looks: 0 bone chunk, 1 shard, 2 icicle, 3 boulder, 4 meteor, 5 fireball, 6 frost puff, 7 ember, 8 fire puddle, 9 ice spike, 10 lasso bead
        static readonly string[] Look46 = { "bone_chunk", "shard", "icicle", "boulder", "meteor", "fireball", "frost_puff", "ember", "fire_puddle", "ice_spike", "lasso_bead" };
        Sprite MakeLook46(int look)
        {
            int W = 8, H = 8; Color o = E32(0x181425), a = Color.white, b = Color.white, cc = Color.white;
            switch (look)
            {
                case 0: W = 10; H = 8; a = E32(0xead4aa); b = E32(0xe4a672); cc = E32(0xf77622); break;
                case 1: W = 7; H = 3; a = E32(0xffffff); b = E32(0xc0cbdc); cc = E32(0x8b9bb4); break;
                case 2: W = 5; H = 12; a = E32(0xffffff); b = E32(0x2ce8f5); cc = E32(0x0099db); break;
                case 3: W = 16; H = 16; a = E32(0xc0cbdc); b = E32(0x8b9bb4); cc = E32(0x5a6988); break;
                case 4: W = 12; H = 12; a = E32(0xfee761); b = E32(0xf77622); cc = E32(0xbe4a2f); break;
                case 5: W = 8; H = 8; a = E32(0xfee761); b = E32(0xfeae34); cc = E32(0xf77622); break;
                case 6: W = 10; H = 10; a = E32(0xffffff); b = E32(0xc0cbdc); cc = E32(0x8b9bb4); break;
                case 7: W = 4; H = 4; a = E32(0xfee761); b = E32(0xf77622); cc = E32(0xe43b44); break;
                case 8: W = 24; H = 12; a = E32(0xfeae34, 0.8f); b = E32(0xf77622, 0.75f); cc = E32(0xe43b44, 0.7f); o = E32(0x3e2731, 0.6f); break;
                case 9: W = 6; H = 14; a = E32(0xffffff); b = E32(0x2ce8f5); cc = E32(0x124e89); break;
                default: W = 6; H = 6; a = E32(0xf6757a); b = E32(0xb55088); cc = E32(0x68386c); break;
            }
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "PLACEHOLDER46_" + Look46[look] };
            var px = new Color[W * H]; float cx = (W - 1) / 2f, cy = (H - 1) / 2f;
            for (int y = 0; y < H; y++) for (int x = 0; x < W; x++)
            {
                float nx = (x - cx) / (W / 2f), ny = (y - cy) / (H / 2f), d;
                if (look == 2 || look == 9) d = Mathf.Abs(nx) / Mathf.Max(0.05f, (look == 2 ? 1f - (ny + 1f) / 2f : (ny + 1f) / 2f)) * 0.9f;
                else if (look == 1) d = Mathf.Abs(ny) / Mathf.Max(0.05f, 1f - Mathf.Abs(nx)) * 0.9f;
                else if (look == 0) d = Mathf.Max(Mathf.Abs(nx) * 0.85f, Mathf.Abs(ny) * (Mathf.Abs(nx) > 0.6f ? 0.9f : 1.4f));
                else d = Mathf.Sqrt(nx * nx + ny * ny);
                Color col = Color.clear;
                if (d < 1f) col = d > 0.78f ? o : (nx + ny < -0.2f ? a : d < 0.5f ? b : cc);
                if (look == 3 && d < 0.78f && ((x * 7 + y * 3) % 11 == 0)) col = cc;
                px[y * W + x] = col;
            }
            tex.SetPixels(px); tex.Apply();
            var s = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), HArt.PPU, 0, SpriteMeshType.FullRect); s.name = tex.name; return s;
        }
        void Ensure46()
        {
            if (_p46Spr != null) return;
            _p46Spr = new Sprite[Look46.Length]; for (int i = 0; i < Look46.Length; i++) _p46Spr[i] = MakeLook46(i);
            _p46Root = new GameObject("BossPatterns46_PLACEHOLDER").transform;
        }
        P46 New46(int kind, int pat, int look, Vector2 p)
        {
            Ensure46(); P46 b = null;
            foreach (var q in _p46) if (!q.alive) { b = q; break; }
            if (b == null) { b = new P46(); b.sr = NewSR("p46", _p46Root, null, 2001); _p46.Add(b); }
            var sr = b.sr; var keep = sr;
            b.alive = true; b.kind = kind; b.pat = pat; b.look = look; b.p = p; b.v = Vector2.zero; b.c = p; b.tgt = p; b.life = T46.bp46LifeS; b.r = T46.bp46BulletR; b.dmg = Dmg46(); b.t = 0; b.delay = 0; b.ang = b.rad = b.w = b.radSpd = 0; b.revAt = -1; b.home = 0; b.tick = 0; b.fall = 0; b.bounces = 0; b.hit = false; b.chill = false; b.follow = false; b.xSplit = false;
            keep.sprite = _p46Spr[look]; keep.color = Color.white; keep.sortingOrder = kind == 4 ? HellordTuning.I.bossWarnOrder948 : 2001; /* t948u puddle under units */ keep.enabled = true; keep.gameObject.SetActive(true); keep.transform.position = p; keep.transform.rotation = Quaternion.identity; keep.transform.localScale = Vector3.one;
            bossBulletsFired++; return b;
        }
        void Kill46(P46 b) { b.alive = false; if (b.sr) b.sr.gameObject.SetActive(false); }
        void Clear46() { foreach (var b in _p46) if (b.alive) Kill46(b); _t46.Clear(); _bp46Next = -1; _bp46TellT = -1; _bp46Waves = 0; _qCharging46 = false; }
        P46 Shot46(int pat, int look, Vector2 p, Vector2 v) { var b = New46(0, pat, look, p); b.v = v * BSpeedMul; return b; }
        void AddTell46(int shape, int pat, Vector2 a, Vector2 b, float r, float tot, float deg = 0, float gap = 0)
        {
            // v0.0.46: crisp tells sit on the floor. Shots leave the muzzle (feet + 1.6) and hit the hero's chest (feet + 0.6), so a tell
            // that starts at the muzzle is drawn from the boss's feet and its far end 0.6 lower, i.e. the shot's ground shadow.
            if (Crisp46 && (a - BBMuzzle).sqrMagnitude < 1e-4f) { bool same = (b - a).sqrMagnitude < 1e-4f; a = _boss.pos; b = same ? a : b - Vector2.up * 0.6f; }
            _t46.Add(new Tell46 { shape = shape, pat = pat, a = a, b = b, r = r, t = tot, tot = tot, deg = deg, gap = gap, col = BossCol46 });
        }
        static Vector2 Dir46(float deg) => new Vector2(Mathf.Cos(deg * Mathf.Deg2Rad), Mathf.Sin(deg * Mathf.Deg2Rad));
        float AimDeg46(Vector2 from) { Vector2 d = HeroC46 - from; return Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg; }
        /// <summary>Gap (deg) of at least bp46GapMul x hero width at distance dist.</summary>
        float MinGapDeg46(float dist) => 2f * Mathf.Asin(Mathf.Clamp01(T46.bp46HeroWidthU * T46.bp46GapMul * 0.5f / Mathf.Max(0.5f, dist))) * Mathf.Rad2Deg;

        // ---------- scheduler ----------
        // pattern ids: 1 horn toss, 2 stomp quake, 3 bull wake, 4 icicle rain, 5 boulder roll, 6 shard fan, 7 hoof meteors, 8 wheel spokes, 9 flame trail, 10 frost puffs, 11 lasso, 12 fire wheel summon, 13 slam spikes
        int[] Set46() => IsGorilla ? new[] { 4, 5, 6 } : IsQilin ? new[] { 7, 8 } : IsFrostOx ? new[] { 10, 2 } : IsHorse ? new[] { 11, 1 } : new[] { 1, 2 };
        int ActiveTypes46(int except)
        {
            int n = 0; var seen = new HashSet<int>();
            foreach (var b in _p46) if (b.alive && b.kind != 5 && b.pat != except && seen.Add(b.pat)) n++;
            foreach (var t in _t46) if (t.pat != except && seen.Add(t.pat)) n++;
            return n;
        }
        bool CanStart46(int pat) => ActiveTypes46(pat) < Mathf.Max(1, T46.bp46MaxTypes);
        float _walk46, _busy46;
        bool OldStomp46 => !T46.bossPatternsV46 || T46.oldStompBurstV46;
        bool Riders46 => !T46.bossSchedV46 || T46.bossChargeRidersV46;
        /// <summary>v0.0.46 scheduler: true while a pattern is telling or its volley just fired; BS.Walk holds its melee start until false.</summary>
        bool SchedBusy46 => T46.bossPatternsV46 && T46.bossSchedV46 && (_busy46 > 0 || _bp46TellT >= 0 || _bp46Waves > 0);
        void UpdateBossPatterns46(float dt)
        {
            Ensure46(); var T = T46; var e = _boss;
            if (_bs == BS.Walk) _walk46 += dt; else _walk46 = 0;
            if (_busy46 > 0) { _busy46 -= dt; if (_busy46 <= 0 && _bs == BS.Walk) _bsT = Mathf.Max(_bsT, T.bossMeleeMinWalkS); }
            // passive charge patterns
            if (!IsGorilla && !IsQilin && !IsFrostOx && !IsHorse && _bs == BS.Charge && Riders46 && CanStart46(3))
            {
                _wake46 -= dt;
                if (_wake46 <= 0)
                {
                    _wake46 = T.oxWakeEveryS; Vector2 side = new Vector2(-_chargeDir.y, _chargeDir.x);
                    for (int s = -1; s <= 1; s += 2) for (int k = 0; k < T.oxWakePerSide; k++)
                    { var b = Shot46(3, 7, e.pos + Vector2.up * 0.4f - _chargeDir * 0.6f, side * s * T.oxWakeSpeed * (1f + 0.25f * k)); b.life = T.oxWakeLifeS; b.home = Rage46 ? T.oxWakeRageHome : 0; }
                }
            }
            bool qCharge = IsQilin && _bs == BS.X44 && _atk44 == 'C' && _ph44 == 1;
            if (qCharge && !_qCharging46) { _trailN46 = 0; _trail46 = 0; }
            _qCharging46 = qCharge;
            if (qCharge && Riders46 && _trailN46 < T.qTrailVolleys && CanStart46(9))
            {
                _trail46 -= dt;
                if (_trail46 <= 0)
                {
                    _trail46 = T.qTrailEveryS; _trailN46++; Vector2 side = new Vector2(-_dir44.y, _dir44.x);
                    for (int s = -1; s <= 1; s += 2) for (int k = 0; k < T.qTrailPerSide; k++) Shot46(9, 5, e.pos + Vector2.up * 0.5f, (side * s + _dir44 * 0.15f * k).normalized * T.qTrailSpeed);
                }
            }
            // second waves (icicle rage, frost puffs)
            if (_bp46Waves > 0) { _bp46WaveT -= dt; if (_bp46WaveT <= 0) { _bp46Waves--; _bp46WaveT = IsGorilla ? T.gRainWave2S : T.foPuffWaveS; if (IsGorilla) Fire46(4, true); else Fire46(10, true); } }
            if (_bs == BS.Stun) { _bp46TellT = -1; _bp46Next = -1; return; }
            if (_bp46TellT >= 0)
            {
                _bp46TellT -= dt; _squash = Mathf.MoveTowards(_squash, -0.08f, dt);
                if (_bp46TellT < 0 && _bp46Next >= 0) { Fire46(_bp46Next, false); _bp46Next = -1; ScreenShake.Add(0.6f, 0.1f); }
                return;
            }
            bool idle = _bs == BS.Walk || _bs == BS.Recover || _bs == BS.Sweep || (_bs == BS.X44 && _atk44 != 'C');
            if (T.bossSchedV46)
            {
                // v0.0.46 shared scheduler: only from a plain walk that has lasted bossGapAfterMeleeS. While the tell and volley run,
                // SchedBusy46 holds the next melee move in BS.Walk, so the two never overlap.
                idle = _bs == BS.Walk && _walk46 >= T.bossGapAfterMeleeS;
            }
            if (!idle) return;
            _bp46Cd -= dt; if (_bp46Cd > 0) return;
            int[] set = Set46(); int pat = set[_bp46Cnt % set.Length];
            if (IsQilin && Rage46 && T.qRageSummonEvery > 0 && _bp46Cnt % T.qRageSummonEvery == T.qRageSummonEvery - 1) pat = 12;
            if (!CanStart46(pat)) { _bp46Cd = 0.3f; return; }
            _bp46Cnt++; _bp46Next = pat; _bp46TellT = Tell46Start(pat); _busy46 = _bp46TellT + T.bossGapAfterPatternS; TellFX.DustPuff(BBMuzzle, 5, 10);
            float cd = Rage46 ? T.bp46CdRage * T.bp46RageTempo : _phase2 ? T.bp46CdP2 : T.bp46CdP1;
            _bp46Cd = _bp46TellT + cd / (1f + T.floorBossPatternRate * FloorIdx);
        }
        Vector2 _bp46Aim; float _bp46Deg; readonly List<Vector2> _bp46Pts = new List<Vector2>();
        /// <summary>Starts the tell decals for a pattern, stores its aim, returns the tell length.</summary>
        float Tell46Start(int pat)
        {
            var T = T46; var e = _boss; Vector2 m = BBMuzzle; _bp46Aim = HeroC46; _bp46Deg = AimDeg46(m); _bp46Pts.Clear(); float tl;
            switch (pat)
            {
                case 1:
                {
                    tl = TellS46(T.oxTossTellS); int n = _phase2 || Rage46 ? T.oxTossChunksP2 : T.oxTossChunks; float range = Mathf.Min(T.oxTossRangeU, (HeroC46 - m).magnitude);
                    for (int i = 0; i < n; i++) { Vector2 l = m + Dir46(_bp46Deg + (i - (n - 1) / 2f) * T.oxTossSpreadDeg) * range; _bp46Pts.Add(l); AddTell46(1, pat, Crisp46 ? l - Vector2.up * 0.6f : l, Crisp46 ? l - Vector2.up * 0.6f : l, 0.8f, tl); AddTell46(2, pat, m, l, 0.15f, tl); }
                    return tl;
                }
                case 2: tl = TellS46(T.oxQuakeTellS); AddTell46(1, pat, e.pos, e.pos, 2.5f, tl); if (!Crisp46) AddTell46(2, pat, e.pos, e.pos + (HeroC46 - e.pos).normalized * 3f, 0.25f, tl); return tl;
                case 4:
                {
                    tl = TellS46(T.gRainGrowS); RainPoints46(); foreach (var p in _bp46Pts) AddTell46(0, pat, p, p, T.gRainR, tl); return tl;
                }
                case 5:
                {
                    tl = TellS46(T.gRollTellS); int n = _phase2 || Rage46 ? 2 : 1;
                    for (int i = 0; i < n; i++) { float d = _bp46Deg + (n == 1 ? 0 : (i == 0 ? -T.gRollVDeg : T.gRollVDeg)); Vector2 dir = Dir46(d); AddTell46(2, pat, m, m + dir * RayToBox44(m, dir), T.gRollR * 2f, tl); }
                    return tl;
                }
                case 6:
                {
                    // t986u VH: Shard Fan tell + shots from feet (not BBMuzzle chest +1.6)
                    Vector2 fanO = (T.gFanAtFeet986 ? _boss.pos : m);
                    tl = TellS46(T.gFanTellS);
                    if (_phase2 || Rage46) { AddTell46(3, pat, fanO, fanO, 7f, tl, _bp46Deg - T.gFanMirrorDeg, T.gFanDeg * 0.5f); AddTell46(3, pat, fanO, fanO, 7f, tl, _bp46Deg + T.gFanMirrorDeg, T.gFanDeg * 0.5f); }
                    else AddTell46(3, pat, fanO, fanO, 7f, tl, _bp46Deg, T.gFanDeg * 0.5f);
                    return tl;
                }
                case 7:
                {
                    tl = TellS46(T.qMetTellS); MeteorPoints46();
                    if (!Rage46) foreach (var p in _bp46Pts) AddTell46(0, pat, p, p, T.qMetR, tl);
                    else AddTell46(0, pat, _bp46Aim, _bp46Aim, T.qMetR, tl);
                    return tl;
                }
                case 8:
                {
                    tl = TellS46(T.qSpokeTellS); int n = _phase2 || Rage46 ? T.qSpokeNP2 : T.qSpokeN;
                    for (int i = 0; i < n; i++) AddTell46(2, pat, m, m + Dir46(i * 360f / n) * (T.qSpokeBalls * T.qSpokeStepU + 0.6f), 0.18f, tl);
                    AddTell46(1, pat, m, m, 1.2f, tl); return tl;
                }
                case 10: tl = TellS46(T.foPuffTellS); AddTell46(3, pat, m, m, 6f, tl, _bp46Deg, T.foPuffDeg * 0.5f); return tl;
                case 11:
                {
                    tl = TellS46(T.hLassoTellS); AddTell46(1, pat, _bp46Aim, _bp46Aim, T.hLassoR, tl); return tl;
                }
                case 12:
                {
                    tl = TellS46(T.qRageSummonTellS);
                    for (int i = 0; i < T.qRageWheels; i++) { Vector2 p = ClampBox44(e.pos + Dir46(_bp46Deg + 90f + i * 180f) * 2.5f); _bp46Pts.Add(p); AddTell46(0, pat, p, p, 0.9f, tl); }
                    return tl;
                }
            }
            return TellS46(0.5f);
        }
        void RainPoints46()
        {
            var T = T46; _bp46Pts.Clear(); Vector2 h = _heroPos;
            if (_phase2 || Rage46)
            {
                int n = T.gRainP2; int cols = 4;
                for (int i = 0; i < n; i++) { int r = i / cols, c = i % cols; Vector2 p = h + new Vector2((c - 1.5f + (r % 2) * 0.5f) * T.gRainGridU, (r - 1f) * T.gRainGridU * 0.8f); _bp46Pts.Add(ClampBox44(p)); }
            }
            else
            {
                int n = Random.Range(T.gRainMin, T.gRainMax + 1); _bp46Pts.Add(h);
                for (int i = 1; i < n; i++) _bp46Pts.Add(ClampBox44(h + Random.insideUnitCircle * T.gRainScatterU));
            }
        }
        void MeteorPoints46()
        {
            var T = T46; var e = _boss; _bp46Pts.Clear(); Vector2 h = _heroPos;
            if (Rage46) { int n = _phase2 ? T.qMetNP2 : T.qMetN; for (int i = 0; i < n; i++) _bp46Pts.Add(h); return; }
            if (_phase2)
            {
                float s = T.qMetXSpacingU; float[] ta = { -2, -1, 1 }, tb = { -1, 1, 2 };
                foreach (var t in ta) _bp46Pts.Add(ClampBox44(h + new Vector2(1, 1).normalized * t * s));
                foreach (var t in tb) _bp46Pts.Add(ClampBox44(h + new Vector2(1, -1).normalized * t * s));
                return;
            }
            for (int i = 0; i < T.qMetN; i++) _bp46Pts.Add(ClampBox44(Vector2.Lerp(e.pos, h, (i + 1f) / T.qMetN)));
        }
        void Fire46(int pat, bool wave2)
        {
            var T = T46; var e = _boss; Vector2 m = BBMuzzle;
            switch (pat)
            {
                case 1:
                {
                    foreach (var l in _bp46Pts)
                    {
                        Vector2 d = l - m; float len = d.magnitude; var b = New46(1, 1, 0, m); b.v = (len > 0.01f ? d / len : Vector2.down) * T.oxTossSpeed * BSpeedMul; b.tgt = l; b.life = len / Mathf.Max(0.1f, b.v.magnitude); b.r = T.oxTossChunkR; b.xSplit = Rage46;
                    }
                    break;
                }
                case 2:
                {
                    int rings = _phase2 || Rage46 ? T.oxQuakeRingsP2 : T.oxQuakeRings;
                    for (int k = 0; k < rings; k++)
                    {
                        float gapC = AimDeg46(e.pos) + (Rage46 ? k * T.oxQuakeRageRotDeg : 0); float gap = Mathf.Max(T.oxQuakeGapDeg, MinGapDeg46((HeroC46 - e.pos).magnitude));
                        for (int i = 0; i < T.oxQuakeBullets; i++)
                        {
                            float a = gapC + gap * 0.5f + i * (360f - gap) / Mathf.Max(1, T.oxQuakeBullets - 1);
                            var b = Shot46(2, 1, e.pos + Vector2.up * 0.3f, Dir46(a) * T.oxQuakeSpeed); b.delay = k * T.oxQuakeEveryS; b.sr.enabled = k == 0; b.sr.transform.rotation = Quaternion.Euler(0, 0, a);
                        }
                    }
                    ScreenShake.Add(1.5f, 0.2f); break;
                }
                case 4:
                {
                    if (wave2) { RainPoints46(); }
                    for (int i = 0; i < _bp46Pts.Count; i++) { var b = New46(2, 4, 2, _bp46Pts[i]); b.fall = wave2 ? TellS46(T.gRainGrowS) : 0.3f; b.t = 0; b.r = T.gRainR; b.life = b.fall + 0.15f; if (wave2) AddTell46(0, 4, _bp46Pts[i], _bp46Pts[i], T.gRainR, b.fall); }
                    if (!wave2 && Rage46) { _bp46Waves = 1; _bp46WaveT = T.gRainWave2S; }
                    break;
                }
                case 5:
                {
                    int n = _phase2 || Rage46 ? 2 : 1;
                    for (int i = 0; i < n; i++) { float d = _bp46Deg + (n == 1 ? 0 : (i == 0 ? -T.gRollVDeg : T.gRollVDeg)); var b = New46(3, 5, 3, m); b.v = Dir46(d) * T.gRollSpeed * BSpeedMul; b.r = T.gRollR; b.bounces = T.gRollBounces; b.life = 12f; }
                    break;
                }
                case 6:
                {
                    // t986u VH: shards spawn at feet
                    Vector2 fanO = (T.gFanAtFeet986 ? _boss.pos : m);
                    int n = Rage46 ? T.gFanNRage : T.gFanN; float a0 = AimDeg46(fanO);
                    float[] centres = _phase2 || Rage46 ? new[] { a0 - T.gFanMirrorDeg, a0 + T.gFanMirrorDeg } : new[] { a0 };
                    foreach (var c in centres) for (int i = 0; i < n; i++) { float a = c + (n == 1 ? 0 : (i - (n - 1) / 2f) * T.gFanDeg / (n - 1)); var b = Shot46(6, 1, fanO, Dir46(a) * T.gFanSpeed); b.sr.transform.rotation = Quaternion.Euler(0, 0, a); b.r = T.bp46BulletR * 0.8f; }
                    break;
                }
                case 7:
                {
                    for (int i = 0; i < _bp46Pts.Count; i++)
                    {
                        var b = New46(2, 7, 4, _bp46Pts[i]); b.r = T.qMetR; b.follow = Rage46;
                        b.delay = Rage46 ? i * T.qMetRageEveryS + 0.001f : i * T.qMetStaggerS; b.fall = Rage46 ? TellS46(T.qMetTellS) : 0.35f; b.life = b.delay + b.fall + 0.15f; b.sr.enabled = false;
                    }
                    break;
                }
                case 8:
                {
                    int n = _phase2 || Rage46 ? T.qSpokeNP2 : T.qSpokeN;
                    for (int i = 0; i < n; i++) for (int k = 0; k < T.qSpokeBalls; k++)
                    { var b = New46(6, 8, 5, m); b.c = m; b.ang = i * 360f / n; b.rad = 0.6f + k * T.qSpokeStepU; b.w = T.qSpokeDegS; b.radSpd = T.qSpokeSpeed * BSpeedMul; b.revAt = Rage46 ? T.qSpokeRageRevS : -1; }
                    break;
                }
                case 10:
                {
                    float a0 = AimDeg46(m);
                    for (int i = 0; i < T.foPuffN; i++) { float a = a0 + (T.foPuffN == 1 ? 0 : (i - (T.foPuffN - 1) / 2f) * T.foPuffDeg / (T.foPuffN - 1)); var b = Shot46(10, 6, m, Dir46(a) * T.foPuffSpeed); b.r = T.foPuffR; b.chill = true; }
                    if (!wave2 && (_phase2 || Rage46) && T.foPuffWavesP2 > 1) { _bp46Waves = T.foPuffWavesP2 - 1; _bp46WaveT = T.foPuffWaveS; }
                    break;
                }
                case 11:
                {
                    Vector2 c = _bp46Aim; float toBoss = Mathf.Atan2(e.pos.y - c.y, e.pos.x - c.x) * Mathf.Rad2Deg;
                    float gap = Mathf.Max(T.hLassoGapDeg, MinGapDeg46(T.hLassoR)); float travel = 180f - gap * 0.5f;
                    float arm = 60f;
                    for (int s = -1; s <= 1; s += 2) for (int i = 0; i < T.hLassoBeads; i++)
                    {
                        var b = New46(6, 11, 10, c); b.c = c; b.rad = T.hLassoR; b.ang = toBoss + s * (i * arm / Mathf.Max(1, T.hLassoBeads - 1)); b.w = s * T.hLassoDegS * BSpeedMul;
                        float head = travel - arm; b.life = Mathf.Max(0.2f, head / Mathf.Max(1f, T.hLassoDegS * BSpeedMul)) + T.hLassoHoldS; b.revAt = b.life - T.hLassoHoldS; b.radSpd = 0;
                    }
                    break;
                }
                case 12:
                {
                    int alive = 0; foreach (var q in _en) if (q.alive && !q.boss && q.variant == 4) alive++;
                    foreach (var p in _bp46Pts)
                    {
                        if (alive >= T.qRageWheelsMax) break;
                        var w = SpawnEnemyAt(EType.Brute, p); if (w == null) continue; MakeVariant44(w, 4); alive++; TellFX.DustPuff(p, 6, 12);
                    }
                    break;
                }
            }
        }
        /// <summary>v0.0.46: Glacier Slam adds a ring of ground spikes that block movement for gSpikeLifeS.</summary>
        void SlamSpikes46(Vector2 c)
        {
            if (!T46.bossPatternsV46) return; var T = T46;
            for (int i = 0; i < T.gSpikeN; i++)
            {
                Vector2 p = ClampBox44(c + Dir46(i * 360f / Mathf.Max(1, T.gSpikeN)) * T.gSpikeRingU); var b = New46(5, 13, 9, p); b.r = T.gSpikeR; b.life = T.gSpikeLifeS;
                if ((_heroPos - p).magnitude < T.gSpikeR + 0.3f * HeroHitK47) HurtHero(Dmg46(T.gSpikeDmgMul), "glacier_spike", true);
            }
        }
        /// <summary>v0.0.46: live ground spikes push the hero out (called after the room clamp).</summary>
        void BlockSpikes46(ref Vector2 hp)
        {
            foreach (var b in _p46)
            {
                if (!b.alive || b.kind != 5) continue; Vector2 d = hp - b.p; float r = b.r + 0.3f; float m = d.magnitude;
                if (m < r) hp = b.p + (m > 1e-3f ? d / m : Vector2.down) * r;
            }
        }
        void Split46(Vector2 c, int n, float spd, float rot, int pat, int look)
        {
            for (int i = 0; i < n; i++) { float a = rot + i * 360f / Mathf.Max(1, n); var b = Shot46(pat, look, c, Dir46(a) * spd); b.sr.transform.rotation = Quaternion.Euler(0, 0, a); }
        }

        /// <summary>Called from UpdateBBullets: moves / hits / draws pattern projectiles and tells.</summary>
        int UpdateP46(float dt)
        {
            if (_p46Spr == null) return 0;
            if (!BossAlive) { if (_p46.Count > 0 || _t46.Count > 0) Clear46(); return 0; }
            var T = T46; Vector2 hc = HeroC46; RoomBox44(out var lo, out var hi); Rect room = _room != null ? _room.WorldRect : new Rect(-1e4f, -1e4f, 2e4f, 2e4f); int live = 0;
            for (int i = _t46.Count - 1; i >= 0; i--)
            {
                var t = _t46[i]; t.t -= dt; if (t.t <= 0) { _t46.RemoveAt(i); continue; }
                float p = 1f - t.t / Mathf.Max(0.01f, t.tot); float al = 0.35f + 0.5f * p; var c = t.col; c.a = al;
                switch (t.shape)
                {
                    case 0: TellCircle46(t.a, t.r, t.col, p, 4); break;
                    case 1:
                        if (t.pat == 0 || t.pat == 1) TellCircle46(t.a, t.r, t.col, p, 4, true);
                        else if (t.pat == 2 && Crisp46) TellRing46(t.a, T.oxQuakeTellRU, t.col, p, 4, AimDeg46(_boss.pos), Mathf.Max(T.oxQuakeGapDeg, MinGapDeg46((HeroC46 - _boss.pos).magnitude)) * 0.5f);
                        else TellRing46(t.a, t.r, t.col, p, 4);
                        break;
                    case 2: TellLane46(t.a, t.b, t.r, t.col, p, 4); break;
                    case 3: TellWedge46(t.a, t.deg, t.gap, t.r, t.col, p, 4); break;
                }
            }
            for (int bi = 0, bn = _p46.Count; bi < bn; bi++) // t970u: index loop - Split46 (Ox toss shards) appends to _p46 mid-loop (was 'Collection was modified')
            {
                var b = _p46[bi];
                if (!b.alive) continue;
                if (b.delay > 0) { b.delay -= dt; if (b.delay <= 0) { b.sr.enabled = b.kind != 2 || b.look != 4; if (b.follow) { b.p = ClampBox44(_heroPos); AddTell46(0, b.pat, b.p, b.p, b.r, b.fall); } } live++; continue; }
                b.t += dt; b.life -= dt; var tr = b.sr.transform;
                switch (b.kind)
                {
                    case 0:
                        if (b.home > 0) { Vector2 to = (hc - b.p).normalized; b.v += to * b.home * dt; b.v = Vector2.ClampMagnitude(b.v, T.oxWakeSpeed * 2f); }
                        b.p += b.v * dt; if (!room.Contains(b.p)) b.life = 0; break;
                    case 1:
                        b.p += b.v * dt; tr.rotation = Quaternion.Euler(0, 0, b.t * 540f);
                        if (b.life <= 0) { Split46(b.p, T.oxTossShards, T.oxTossShardSpeed, b.xSplit ? 45f : 0f, 1, 1); ScreenShake.Add(0.4f, 0.08f); }
                        break;
                    case 2:
                    {
                        float f = Mathf.Clamp01(b.t / Mathf.Max(0.05f, b.fall)); b.sr.enabled = true;
                        if (!b.follow && b.look == 4 && b.t <= dt) AddTell46(0, b.pat, b.p, b.p, b.r, Mathf.Max(0.05f, b.fall));
                        tr.position = b.p + Vector2.up * (1f - f) * 9f; tr.rotation = Quaternion.Euler(0, 0, b.look == 4 ? -30f : 0f);
                        if (f >= 1f && !b.hit)
                        {
                            b.hit = true; b.life = 0.1f; ScreenShake.Add(0.5f, 0.08f); TellFX.DustPuff(b.p, 4, 8);
                            if ((_heroPos - b.p).magnitude < b.r + 0.35f * HeroHitK47 && mode == Mode.Play) { int h0 = heroHits; HurtHero(b.dmg, b.look == 4 ? "hoof_meteor" : "icicle_rain", true); if (heroHits > h0) bossBulletHits++; }
                            if (b.look == 4) { var pd = New46(4, 7, 8, b.p); pd.r = b.r; pd.life = T.qMetPuddleS; pd.dmg = Dmg46(T.qMetPuddleDmgMul); pd.tick = 0; }
                        }
                        if (b.life <= 0) break;
                        continue;
                    }
                    case 3:
                    {
                        b.p += b.v * dt; bool bounce = false;
                        if (b.p.x < lo.x || b.p.x > hi.x) { b.v.x = -b.v.x; b.p.x = Mathf.Clamp(b.p.x, lo.x, hi.x); bounce = true; }
                        if (b.p.y < lo.y || b.p.y > hi.y) { b.v.y = -b.v.y; b.p.y = Mathf.Clamp(b.p.y, lo.y, hi.y); bounce = true; }
                        tr.rotation = Quaternion.Euler(0, 0, -b.t * 360f * Mathf.Sign(b.v.x + 1e-4f));
                        if (bounce)
                        {
                            ScreenShake.Add(0.8f, 0.1f); b.bounces--;
                            if (b.bounces < 0) { Split46(b.p, T.gRollShards, T.gRollShardSpeed, Random.value * 360f, 5, 2); b.life = 0; }
                            else if (Rage46 && T.gRollRageBounceShards > 0) Split46(b.p, T.gRollRageBounceShards, T.gRollShardSpeed, Random.value * 360f, 5, 1);
                        }
                        break;
                    }
                    case 4:
                        b.tick -= dt; b.sr.color = new Color(1, 1, 1, Mathf.Clamp01(b.life / 0.3f));
                        if (b.tick <= 0 && (_heroPos - b.p).magnitude < b.r && mode == Mode.Play) { b.tick = T.qMetPuddleTickS; HurtHero(b.dmg, "fire_puddle", true); }
                        if (b.life <= 0) Kill46(b); else { tr.position = b.p; live++; }
                        continue;
                    case 5:
                    {
                        float rise = Mathf.Clamp01(b.t / 0.12f); tr.localScale = new Vector3(1, rise, 1); tr.position = b.p + Vector2.up * 0.4f * rise;
                        if (b.life <= 0) Kill46(b); else live++;
                        continue;
                    }
                    case 6:
                    {
                        float w = b.w; if (b.revAt >= 0 && b.t >= b.revAt) w = b.pat == 11 ? 0 : -w;
                        b.ang += w * dt; b.rad += b.radSpd * dt; b.p = b.c + Dir46(b.ang) * b.rad;
                        if (!room.Contains(b.p) && b.pat != 11) b.life = 0;
                        break;
                    }
                }
                if (b.life <= 0) { Kill46(b); continue; }
                if (b.kind != 2) tr.position = b.p;
                if ((b.p - hc).sqrMagnitude < (b.r + 0.35f * HeroHitK47) * (b.r + 0.35f * HeroHitK47) && mode == Mode.Play)
                {
                    int h0 = heroHits; HurtHero(b.dmg, "boss_p46_" + b.pat, true); if (heroHits > h0) { bossBulletHits++; if (b.chill) AddChill44(T.foPuffChill); }
                    if (b.kind != 3) { Kill46(b); continue; }
                }
                live++;
            }
            return live;
        }
    }
}
