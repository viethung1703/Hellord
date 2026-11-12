using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

// v0.0.53 torture gaps (VH): finish the v0.0.52 torture wire.
//  * device collision (set pieces, mortar pit, ice blocks push hero + enemies out, via _blocks42)
//  * watching demons beside devices (idle-row sprites facing the device; decor only, never on / operating a device)
//  * Grinder Monk hauling the Mortar B rope (the ONLY demon touching a device - VH rule)
//  * shootable damned souls on Mortar floors (HP bar, white flash, -12 number, squash + blood on death)
//  * Mortar floor demons: Imp -> Rice Ghost, Ghoul -> Bloater (exploding), Brute -> Grinder Monk
//  * Mortar boss: Horse-Face pestle sheet (MOCK-UP art) on BossKind 2 when the floor is Mortar
//  * Frozen: ice blocks placed in the v131 group-scene layout (5 blocks), each still shatterable
namespace Hellord
{
    public partial class HellordGame
    {
        Sprite[] _bossFPestle53, _bossRageFPestle53;
        bool MortarBoss53 => HellordTuning.I.mortarBoss53 && Mortar52 && !HArt.Missing("boss_horseface_pestle");

        // ---------- collision ----------
        /// <summary>Footprint rect (room px, y down) at the bottom of a prop with pivot at bottom-centre.</summary>
        Rect Collide53(Vector2 at, float wPx, float bandPx, float liftPx = 0f)
        {
            if (!HellordTuning.I.tortureCollide53 || wPx <= 1 || bandPx <= 1) return default;
            float cx = at.x * HArt.PPU, by = -at.y * HArt.PPU - liftPx;
            var r = new Rect(cx - wPx / 2f, by - bandPx, wPx, bandPx); _blocks42.Add(new Block42 { vis = r, col = r }); return r;
        }

        // ---------- watchers + rope monk ----------
        // v0.0.53 pt: PixelLab watcher-only demons (38x32, pivot 19,31, Endesga 32). Combat sheets are never reused here.
        class Watch53 { public SpriteRenderer sr, sh; public Sprite[] f; public int[] idle, act; public float t, fps, actT; }
        readonly List<Watch53> _watch53 = new List<Watch53>();
        static readonly string[] WatchFile53 = { "watcher_imp_8f_38x32", "watcher_warden_8f_38x32", "watcher_overseer_8f_38x32", "watcher_hauler_8f_38x32" };
        static readonly int[][] WatchIdle53 = { new[] { 0, 1, 2, 3, 4, 5 }, new[] { 0, 1, 5, 6, 7 }, new[] { 0, 1, 2, 3, 6, 7 }, new[] { 0, 1, 2, 3, 4, 5, 6, 7 } };
        static readonly int[][] WatchAct53 = { new[] { 6, 7 }, new[] { 2, 3, 4 }, new[] { 4, 5 }, null }; // imp cheer_hop, warden point, overseer cheer_fists
        Sprite[] WatchSheet53(int kind) => Grid52(WatchFile53[kind], 38, 32, 8, 8, 0, new Vector2(19.5f / 38f, 1f / 32f)); // 0 imp, 1 warden, 2 overseer, 3 hauler
        // VH: Ox/Frozen/Burning = watch only (Imp/Warden/Overseer); Mortar B = Overseer + Imp watch (no Warden yet), Hauler only on the rope.
        int[] Crew53(string file) => file == "torture_b" ? new[] { 2, 0 } : file == "torture_ox" ? new[] { 0, 1, 2 } : file == "torture_frozen" ? new[] { 1, 0, 2 } : new[] { 2, 1, 0 };

        void Watchers53(Vector2 at, float wPx, float hPx, string file, System.Func<float> F)
        {
            var T = HellordTuning.I; if (T.watchers53 <= 0) return; var crew = Crew53(file);
            _room.Walkable(24f, out var lo, out var hi); if (hi.x < lo.x) { var t = lo; lo = hi; hi = t; } if (hi.y < lo.y) { float t = lo.y; lo.y = hi.y; hi.y = t; }
            for (int i = 0; i < T.watchers53; i++)
            {
                int kind = crew[i % crew.Length]; var f = WatchSheet53(kind); if (f == null || f.Length < 8) continue;
                float side = i % 2 == 0 ? -1 : 1; // left / right of the device, staggered down the front
                var p = at + new Vector2(side * (wPx / 2f + T.watchGapPx53) / HArt.PPU, -(i / 2) * 1.4f + 0.4f + F() * 0.6f);
                if (i >= 2) p = at + new Vector2((F() - 0.5f) * wPx * 0.6f / HArt.PPU, -T.watchGapPx53 / HArt.PPU - 0.5f);
                p.x = Mathf.Clamp(p.x, lo.x, hi.x); p.y = Mathf.Clamp(p.y, lo.y, hi.y);
                int o = Ord52(p); var w = new Watch53 { f = f, idle = WatchIdle53[kind], act = WatchAct53[kind], t = F() * 2f, fps = T.watchFps53, actT = 2f + F() * T.watchActEvery53 };
                w.sr = SR52("watch53", f[w.idle[0]], p, o); w.sr.flipX = p.x > at.x; // face the device
                w.sh = Shadow52(p + Vector2.up * 0.05f, 16, o);
                _watch53.Add(w); var wc = Collide53(p, 12, 6);
                AddBrk53(w.sr, BrkRect53(p, 1.3f, 1.9f), T.brkDemonHp53, Brk53Demon, wc, false, w.sh); // shootable watcher
            }
        }
        void RopeMonk53(Vector2 at, int order) // Mortar B only: Blindfold Rope Hauler (the ONLY demon touching a device)
        {
            var T = HellordTuning.I; if (!T.ropeMonk53) return; var f = WatchSheet53(3); if (f == null) return;
            Vector2 Loc(float x, float y) => at + new Vector2((x - 229f) / HArt.PPU, (222f * 0.96f - y) / HArt.PPU);
            var p = Loc(T.ropeMonkX53, T.ropeMonkY53);
            var w = new Watch53 { f = f, idle = WatchIdle53[3], fps = T.watchFps53 * 1.3f }; w.sr = SR52("ropeHauler53", f[0], p, Ord52(p) + 2); w.sr.flipX = T.ropeHaulerFlip53; w.sh = Shadow52(p, 16, Ord52(p) + 2); _watch53.Add(w);
            var a = p + new Vector2(-0.5f, 1.0f); var b = Loc(77, 12); var line = SR52("rope53", Px53(), a, order + 1);
            line.color = new Color(0.55f, 0.42f, 0.28f, 1f); var d = b - a; line.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
            line.transform.localScale = new Vector3(d.magnitude * HArt.PPU, 1, 1); _watchLines53.Add(line); var mc = Collide53(p, 12, 6);
            AddBrk53(w.sr, BrkRect53(p, 1.3f, 1.9f), T.brkDemonHp53, Brk53Demon, mc, false, w.sh, line);
        }
        readonly List<SpriteRenderer> _watchLines53 = new List<SpriteRenderer>();
        static Sprite _px53;
        static Sprite Px53()
        {
            if (_px53) return _px53; var t = new Texture2D(1, 1) { filterMode = FilterMode.Point }; t.SetPixel(0, 0, Color.white); t.Apply();
            _px53 = Sprite.Create(t, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), HArt.PPU); return _px53;
        }

        // ---------- shootable damned souls ----------
        class Soul53 { public SpriteRenderer sr, sh, bar, barBg; public Vector2 c; public float hp, t, deadT; public bool dead; }
        readonly List<Soul53> _souls53 = new List<Soul53>();
        static Sprite[] _soulF53;
        void Souls53(System.Func<float> F, SpotFn52 spot)
        {
            var T = HellordTuning.I; if (T.souls53 <= 0) return;
            if (_soulF53 == null) _soulF53 = Grid52("damned_rice_16x24_8f", 16, 24, 8, 8, 0, new Vector2(0.5f, 1f / 24f)); if (_soulF53 == null) return;
            int n = T.souls53 + (F() < 0.5f ? 1 : 0);
            for (int i = 0; i < n; i++)
            {
                if (!spot(1.2f, 1.6f, 0.4f, out var at)) break; int o = Ord52(at);
                var s = new Soul53 { c = at, hp = T.soulHp53, t = F() * 3f };
                s.sh = Shadow52(at + Vector2.up * 0.05f, 14, o); s.sr = SR52("soul53", _soulF53[0], at, o); s.sr.flipX = F() < 0.5f; HordeFX.Prepare(s.sr);
                s.barBg = SR52("soul53_bg", Px53(), at + new Vector2(-0.5f, 1.75f), o + 1); s.barBg.color = new Color(0, 0, 0, 0.75f); s.barBg.transform.localScale = new Vector3(16, 2, 1);
                s.bar = SR52("soul53_hp", Px53(), at + new Vector2(-0.5f + 1f / 16f, 1.75f), o + 2); s.bar.color = new Color(0.9f, 0.15f, 0.15f); s.bar.transform.localScale = new Vector3(14, 1, 1);
                s.bar.enabled = s.barBg.enabled = false; _souls53.Add(s);
            }
        }
        bool SoulHit53(Vector2 p, float extra, float dmg)
        {
            var T = HellordTuning.I; bool any = false;
            foreach (var s in _souls53)
            {
                if (s.dead || !s.sr) continue; if ((s.c + Vector2.up * 0.75f - p).magnitude > T.soulHitR53 + extra) continue;
                float d = T.soulDmg53 > 0 ? T.soulDmg53 : dmg; s.hp -= d; any = true; s.t = 0;
                HordeFX.Hit(s.sr, Mathf.RoundToInt(d)); s.bar.enabled = s.barBg.enabled = true;
                s.bar.transform.localScale = new Vector3(14 * Mathf.Clamp01(s.hp / Mathf.Max(1, T.soulHp53)), 1, 1);
                if (s.hp <= 0)
                {
                    s.dead = true; s.deadT = 0; s.sr.sprite = _soulF53[5]; BrkDrop53(s.c + Vector2.up * 0.2f, false); s.bar.enabled = s.barBg.enabled = false;
                    HordeFX.CheapBurst(s.c + Vector2.up * 0.5f, new Color(0.75f, 0.05f, 0.08f), 9); HellordAudio.Play("fireball_impact");
                    if (T.breakables53) BreakFX53(s.c + Vector2.up * 0.6f, Brk53Flesh, T.brkSoulMul53, 1f); // v0.0.53 breakables: stronger gore pop
                }
                break;
            }
            return any;
        }

        void ClearTorture53()
        {
            foreach (var w in _watch53) { if (w.sr) Object.Destroy(w.sr.gameObject); if (w.sh) Object.Destroy(w.sh.gameObject); }
            foreach (var l in _watchLines53) if (l) Object.Destroy(l.gameObject);
            foreach (var s in _souls53) { if (s.sr) Object.Destroy(s.sr.gameObject); if (s.sh) Object.Destroy(s.sh.gameObject); if (s.bar) Object.Destroy(s.bar.gameObject); if (s.barBg) Object.Destroy(s.barBg.gameObject); }
            _watch53.Clear(); _watchLines53.Clear(); _souls53.Clear(); ClearBrk53();
        }
        void UpdateTorture53(float dt)
        {
            UpdateBrk53(dt);
            foreach (var w in _watch53)
            {
                if (!w.sr) continue; w.t += dt; int k = (int)(w.t * w.fps);
                if (w.act != null) { w.actT -= dt; if (w.actT < -HellordTuning.I.watchActS53) w.actT = HellordTuning.I.watchActEvery53 * (0.6f + 0.8f * Mathf.Abs(Mathf.Sin(w.t * 7.3f))); }
                w.sr.sprite = w.f[w.act != null && w.actT < 0 ? w.act[k % w.act.Length] : w.idle[k % w.idle.Length]];
            }
            var T = HellordTuning.I;
            foreach (var s in _souls53)
            {
                if (!s.sr) continue; s.t += dt;
                if (s.dead)
                {
                    s.deadT += dt; float a = 1f - Mathf.Clamp01((s.deadT - T.soulCorpseS53) / 0.6f); s.sr.color = new Color(1, 1, 1, a); if (s.sh) s.sh.color = new Color(1, 1, 1, a * T.tortureShadowA52);
                    if (a <= 0) s.sr.enabled = false; continue;
                }
                int k = s.t < 0.5f ? 4 : (int)(s.t * T.soulFps53) % 4; // scream after a hit, else idle/idle/flail/flail
                if (k == 3 && (int)(s.t * 0.5f) % 3 == 2) k = 6 + (int)(s.t * T.soulFps53) % 2; // now and then pushes against the floor
                s.sr.sprite = _soulF53[k];
            }
        }

        // ---------- Mortar demons ----------
        bool ApplyMortar53(Enemy e)
        {
            var T = HellordTuning.I; if (!T.mortarEnemies53 || !Mortar52 /* t495u: Mortar theme only */ || Random.value >= T.mortarEnemyChance53) return false;
            int k = e.type == EType.Imp ? 1 : e.type == EType.Ghoul ? 0 : e.type == EType.Brute ? 2 : -1; if (k < 0) return false;
            if (MortarArt49e.Frames((MortarKind49e)k) == null) return false;
            e.m53 = (byte)(k + 1);
            if (k == 1) { e.speed *= T.riceGhostSpeed53; e.maxHp *= T.riceGhostHp53; }
            else if (k == 0) { e.variant = 2; e.fuseT = -1; e.maxHp *= T.bloaterHp53; e.speed *= T.bloaterSpeed53; }
            else { e.maxHp *= T.grinderHp53; e.speed *= T.grinderSpeed53; e.dmg *= T.grinderDmg53; }
            e.hp = e.maxHp; var f = MortarArt49e.Frames((MortarKind49e)k); e.sr.sprite = f[MortarArt49e.Index((MortarKind49e)k, MortarAnim49e.Idle, 0, 0)]; return true;
        }
        void Animate53(Enemy e, Vector2 v, float dt)
        {
            var k = (MortarKind49e)(e.m53 - 1); var f = MortarArt49e.Frames(k); if (f == null) return;
            int dir = Mathf.Abs(v.x) > Mathf.Abs(v.y) ? (v.x > 0 ? 1 : 3) : (v.y > 0 ? 2 : 0);
            var a = e.hitT > 0 ? MortarAnim49e.Hurt : v.sqrMagnitude > 0.01f ? MortarAnim49e.Walk : MortarAnim49e.Idle;
            e.sr.sprite = f[MortarArt49e.Index(k, a, dir, a == MortarAnim49e.Hurt ? 0.3f - e.hitT : e.animT / HArt.EnemyWalkFps)]; e.sr.flipX = MortarArt49e.FlipX(dir);
        }
        bool Death53(Enemy e, float dt)
        {
            var k = (MortarKind49e)(e.m53 - 1); var f = MortarArt49e.Frames(k); if (f == null) return false;
            e.deathT += dt; int n = MortarArt49e.FrameCount(k, 11);
            if (e.deathT * HArt.EnemyDeathFps >= n + 1 || e.deathT > 2f) { e.dying = false; e.sr.enabled = false; e.sr.gameObject.SetActive(false); return true; }
            e.sr.flipX = false; e.sr.sprite = f[MortarArt49e.Index(k, MortarAnim49e.Death, 0, e.deathT, HArt.EnemyDeathFps)]; return true;
        }
    }
}
