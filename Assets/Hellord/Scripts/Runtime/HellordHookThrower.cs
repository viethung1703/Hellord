// v0.0.24: Hook Thrower - Level 1 (Tongue Pulling / Liars) unique enemy, "hook chains drag you" (Core Combat Design v0.2.8).
// FX: Pixel FX Pack 1.9.2 GossipHookFX.HookWindup (whirl over the head) -> HookChainFX lane tell -> flying hook -> latch + drag.
// Gameplay (hit test, damage, pull, struggle-to-snap) lives here; the pack only draws. Every number is in HellordTuning ("Hook Thrower").
// Art: CODE-DRAWN PLACEHOLDER (HookThrowerArt below, Endesga 32, yellow "PH" tag) until Hellord Art makes the real sheet.
using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

namespace Hellord
{
    public partial class HellordGame
    {
        static readonly float ArgHookHp = ArgF("-hookhp", 0f); // v0.0.24 clip/debug only: tougher throwers so a recording shows several throws
        static readonly bool ArgHookThrowers = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-hookthrowers") >= 0;
        readonly List<Enemy> _throwers = new List<Enemy>(8);
        Enemy _dragBy; float _dragT, _struggleT; bool _hookSetup;
        [System.NonSerialized] public int hookThrows, hookHits, hookSnaps, hookMisses; // harness / bot counters
        public bool HeroHooked => _dragBy != null;

        void EnsureHookFX()
        {
            if (_hookSetup) return; _hookSetup = true;
            // same pattern as the boss bullets: the game's own sprite material, drawn above actors (YSort) and below the hero/walls
            var tmp = NewSR("matprobe", null, null, 0); var mat = new Material(tmp.sharedMaterial); Destroy(tmp.gameObject);
            HookChainFX.MaterialOverride = mat; HookChainFX.SortingOrder = 2003;
            GossipHookFX.MaterialOverride = new Material(mat); GossipHookFX.SortingOrder = 2004;
            HookChainFX.Prewarm(8); GossipHookFX.Prewarm(8);
            GossipHookFX.SetFillSorting("Default", HellordTuning.I.gossipFillSortingOrder); // v0.0.26 (pack 2.3.1): shout fill UNDER characters (QC: warning fills never tint an enemy)
            GossipHookFX.OnHookThrown += OnHookThrown;
        }
        void ApplyHookTuning()
        {
            var T = HellordTuning.I;
            HookChainFX.DefaultRangePx = T.hookRangePx; HookChainFX.DefaultSpeedPx = HookSpeedPx49; HookChainFX.DefaultWidthPx = T.hookWidthPx;
            HookChainFX.DefaultBreakCount = Mathf.Max(1, T.hookBreakPips); HookChainFX.DefaultCooldown = T.hookCooldown; HookChainFX.DefaultTell = HookLaneTell49;
            GossipHookFX.WindupSeconds = T.hookWindupSeconds; GossipHookFX.ThrowLaneTell = HookLaneTell49;
        }
        Vector2 HookAnchor(Enemy e) => e.pos + Vector2.up * (GossipHookFX.WindupHeadPx * 0.5f / HArt.PPU);
        Vector2 HeroHurtCenter => _heroPos + Vector2.up * 0.8f;

        void InitHookThrower(Enemy e)
        {
            var T = HellordTuning.I; EnsureHookFX();
            e.maxHp = ArgHookHp > 0 ? ArgHookHp : T.hookHp; e.speed = T.hookMoveSpeed; e.radius = T.hookRadiusU; e.hitY = 1.0f; e.dmg = T.hookContactDamage; e.xp = T.hookXp;
            e.hookCd = T.hookFirstDelay; e.hookWind = -1; e.hookH = -1; e.hookPrevTip = Vector2.zero; e.hookTipOk = false; e.strafe = Random.value < 0.5f ? -1 : 1;
            if (!_throwers.Contains(e)) _throwers.Add(e);
        }
        /// <summary>Stops any wind-up / hook owned by this slot (death, despawn, slot reuse, room change).</summary>
        void HookCleanup(Enemy e)
        {
            if (e.hookWind >= 0) { GossipHookFX.Cancel(e.hookWind); e.hookWind = -1; }
            if (e.hookH >= 0) { if (HookChainFX.IsAlive(e.hookH)) HookChainFX.Cancel(e.hookH); e.hookH = -1; }
            if (_dragBy == e) _dragBy = null;
            GossipCleanup(e); // v0.0.25
        }
        void ClearHooks()
        {
            foreach (var e in _throwers) HookCleanup(e);
            _throwers.Clear(); _dragBy = null; ClearGossip(); // v0.0.25: imps + scramble status
            if (_hookSetup) { HookChainFX.Clear(); GossipHookFX.Clear(); }
        }
        void OnHookThrown(int windup, int hook)
        {
            foreach (var e in _throwers) if (e.hookWind == windup)
            {
                e.hookWind = -1; e.hookH = hook; e.hookTipOk = false; e.hookAim = HeroHurtCenter;
                if (hook < 0) e.hookCd = HellordTuning.I.hookCooldown; else { hookThrows++; HellordAudio.Play("hook_chain_throw"); }
                return;
            }
            if (hook >= 0) HookChainFX.Cancel(hook); // orphan (owner gone)
        }
        bool HookBusy(Enemy e)
        {
            if (e.hookWind >= 0) return true;
            if (e.hookH < 0) return false;
            var ph = HookChainFX.GetPhase(e.hookH); return ph != HookPhase.None && ph != HookPhase.Dormant;
        }
        int HooksInPlay() { int n = 0; foreach (var e in _throwers) if (e.alive && HookBusy(e)) n++; return n; }
        int ThrowersAlive() { int n = 0; foreach (var e in _throwers) if (e.alive && e.type == EType.HookThrower) n++; return n; }

        /// <summary>Movement for a Hook Thrower (called from UpdateEnemies): hold a distance band, strafe, stand still while throwing.</summary>
        Vector2 HookThrowerMove(Enemy e, Vector2 to, float dist, float sp)
        {
            var T = HellordTuning.I;
            if (HookBusy(e) || dist < 0.01f) return Vector2.zero;
            Vector2 dir = to / dist;
            if (dist > T.hookPreferMaxU) return dir * sp;
            if (dist < T.hookPreferMinU) return -dir * sp;
            return new Vector2(-dir.y, dir.x) * (e.strafe * sp * 0.45f);
        }

        void UpdateHooks(float dt)
        {
            if (_throwers.Count == 0 && _dragBy == null) return;
            var T = HellordTuning.I; ApplyHookTuning();
            for (int i = _throwers.Count - 1; i >= 0; i--)
            {
                var e = _throwers[i];
                if (!e.alive || e.type != EType.HookThrower) { HookCleanup(e); _throwers.RemoveAt(i); continue; }
                if (e.hookH >= 0 && !HookChainFX.IsAlive(e.hookH)) e.hookH = -1;
                if (e.hookH >= 0) HookChainFX.SetAnchor(e.hookH, HookAnchor(e));
                e.hookCd -= dt * HellordTuning.I.enemyAttackRateMul; // v0.0.27: demons act more often
                // start a throw: wind-up whirl (part 1 of the warning); the pack locks the aim at the end and shows the lane (part 2)
                if (!HookBusy(e) && e.hookH < 0 && e.hookCd <= 0 && e.freezeT <= 0 && _dragBy == null && HooksInPlay() < Mathf.Max(1, T.hookMaxActive)
                    && (HeroHurtCenter - HookAnchor(e)).magnitude <= T.hookThrowRangeU)
                {
                    e.hookWind = GossipHookFX.HookWindup(e.tr, _heroCenterT, T.hookWindupSeconds); if (e.hookWind >= 0) HellordAudio.Play("hook_windup_rattle"); // v0.0.34
                    if (e.hookWind < 0) { e.hookAim = HeroHurtCenter; e.hookTipOk = false; e.hookH = HookChainFX.Throw(HookAnchor(e), HeroHurtCenter, -1, T.hookWindupSeconds + HookLaneTell49); if (e.hookH >= 0) { hookThrows++; HellordAudio.Play("hook_chain_throw"); } else e.hookCd = 1f; }
                }
                // flying hook: segment test (last tip -> tip) against the hero's hurt circle
                if (e.hookH >= 0 && HookChainFX.GetPhase(e.hookH) == HookPhase.Fly && HookChainFX.TryGetTip(e.hookH, out var tip))
                {
                    Vector2 a = e.hookTipOk ? e.hookPrevTip : HookAnchor(e); e.hookPrevTip = tip; e.hookTipOk = true;
                    float r = T.hookHeroRadiusU * HeroHitK47 + T.hookWidthPx * 0.5f / HArt.PPU;
                    if (_dragBy == null && SegDistH(HeroHurtCenter, a, tip) <= r)
                    {
                        if (!HeroInvuln969)
                        {
                            HurtHero(T.hookDamage, "hook"); hookHits++; HellordAudio.Play("hook_hit");
                            HookChainFX.Latch(e.hookH, _heroCenterT); _dragBy = e; _dragT = 0; _struggleT = 0;
                        }
                    }
                }
            }
            if (_dragBy != null) UpdateDrag(dt, T);
        }
        static float SegDistH(Vector2 p, Vector2 a, Vector2 b) { var ab = b - a; float l2 = ab.sqrMagnitude; float t = l2 > 1e-6f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / l2) : 0; return (a + ab * t - p).magnitude; }

        void UpdateDrag(float dt, HellordTuning T)
        {
            var e = _dragBy;
            if (e == null || !e.alive || e.hookH < 0 || HookChainFX.GetPhase(e.hookH) != HookPhase.Drag) { _dragBy = null; return; }
            _dragT += dt;
            Vector2 to = e.pos - _heroPos; float d = to.magnitude; Vector2 dir = d > 0.01f ? to / d : Vector2.zero;
            if (d <= T.hookReleaseDistU || _dragT >= T.hookMaxDragSeconds) { HookChainFX.Release(e.hookH); _dragBy = null; return; }
            // struggle: holding away from the thrower fills break pips; enough pips snap the chain
            var mv = HInput.Move(); if (_botMove.sqrMagnitude > 0.01f) mv = _botMove;
            if (mv.sqrMagnitude > 0.25f && Vector2.Dot(mv.normalized, -dir) > 0.5f)
            {
                _struggleT += dt;
                if (_struggleT >= T.hookStruggleSecondsPerPip) { _struggleT = 0; if (HookChainFX.AddBreak(e.hookH)) { hookSnaps++; _dragBy = null; Toast("CHAIN SNAPPED"); return; } }
            }
            var p = _heroPos + dir * Mathf.Min(d - T.hookReleaseDistU * 0.9f, T.hookPullSpeed * dt);
            ClampToRoom(ref p, HeroMarginPx); ClampToArena(ref p); BlockShrineStatue(ref p);
            _heroPos = p; _hero.position = new Vector3(Snap(p.x), Snap(p.y), 0);
        }
        /// <summary>Multiplier on the hero's own movement (UpdateHero): slowed while dragged.</summary>
        float HookMoveMul() => _dragBy != null ? HellordTuning.I.hookDragMoveMul : 1f;
        /// <summary>Harness-only scripted move (autotest dodger); zero in normal play.</summary>
        [System.NonSerialized] public Vector2 _botMove;

        /// <summary>Debug J: spawn one Hook Thrower ~7u from the hero (placeholder art).</summary>
        Enemy SpawnHookThrowerNearHero()
        {
            var p = _heroPos + Random.insideUnitCircle.normalized * 7f; ClampToRoom(ref p, 10);
            return SpawnEnemyAt(EType.HookThrower, p);
        }
        /// <summary>Normal floor spawns: a small chance to mix a Hook Thrower in (tuning hookThrowersInRooms or -hookthrowers).</summary>
        bool TryFloorHookThrower(float chance = -1f)
        {
            var T = HellordTuning.I; if (!(T.hookThrowersInRooms || ArgHookThrowers)) return false;
            if (Random.value >= (chance >= 0 ? chance : T.hookSpawnChance) || ThrowersAlive() >= T.hookMaxAlive + FloorIdx) return false; // v0.0.26: +1 allowed per floor
            SpawnEnemy(EType.HookThrower); return true;
        }

        // bot state: hook lanes (anchor, direction, length u, phase; windups as phase "Windup" from the thrower to the hero)
        // v0.0.24: the FX runners step on their own clock - freeze them while a level-up / pause / menu is up so hooks don't move under a card screen.
        void HookPauseTick()
        {
            if (!_hookSetup) return; bool on = mode == Mode.Play;
            if (HookChainRunner.I.enabled != on) HookChainRunner.I.enabled = on;
            if (GossipHookRunner.I.enabled != on) GossipHookRunner.I.enabled = on;
        }
        void AppendHooks(System.Text.StringBuilder b)
        {
            b.Append(",\"hooks\":["); bool first = true;
            foreach (var e in _throwers)
            {
                if (!e.alive) continue; string ph = null; Vector2 a = HookAnchor(e), tgt = HeroHurtCenter;
                if (e.hookWind >= 0) ph = "Windup";
                else if (e.hookH >= 0) { var p = HookChainFX.GetPhase(e.hookH); if (p == HookPhase.Tell || p == HookPhase.Fly || p == HookPhase.Drag) ph = p.ToString(); if (p == HookPhase.Fly && HookChainFX.TryGetTip(e.hookH, out var t)) tgt = t; else if (p != HookPhase.Drag) tgt = e.hookAim; }
                if (ph == null) continue;
                if (!first) b.Append(','); first = false;
                Vector2 dv = (tgt - a); float L = dv.magnitude; if (L > 1e-3f) dv /= L;
                b.Append("{\"x\":").Append(F(a.x)).Append(",\"y\":").Append(F(a.y)).Append(",\"dx\":").Append(F(dv.x)).Append(",\"dy\":").Append(F(dv.y))
                 .Append(",\"len\":").Append(F(HellordTuning.I.hookRangePx / HArt.PPU)).Append(",\"ph\":\"").Append(ph).Append("\",\"w\":").Append(F(HellordTuning.I.hookWidthPx / HArt.PPU)).Append('}');
            }
            b.Append("],\"hooked\":").Append(_dragBy != null ? 1 : 0);
        }
    }

    /// <summary>v0.0.24 CODE-DRAWN PLACEHOLDER sheet for the Hook Thrower (24 frames: walk S/E/N/W x4, death x8), Endesga 32 palette,
    /// with a yellow "PH" tag so nobody mistakes it for final art. Replace by adding a real sheet to EnemyArtMap.</summary>
    public static class HookThrowerArt
    {
        const int W = 24, H = 34;
        static Sprite[] _s;
        static readonly Color32 K = C(0x18, 0x14, 0x25), RobeD = C(0x3e, 0x27, 0x31), Robe = C(0x68, 0x38, 0x6c), RobeL = C(0xb5, 0x50, 0x88), Rim = C(0xbe, 0x4a, 0x2f),
            Eye = C(0xfe, 0xae, 0x34), Ch = C(0x8b, 0x9b, 0xb4), ChD = C(0x5a, 0x69, 0x88), Hook = C(0xc0, 0xcb, 0xdc), Tag = C(0xfe, 0xe7, 0x61), Skin = C(0x73, 0x3e, 0x39);
        static Color32 C(byte r, byte g, byte b) => new Color32(r, g, b, 255);
        public static Sprite[] Placeholder()
        {
            if (_s != null) return _s;
            var tex = new Texture2D(W * 24, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "PLACEHOLDER_hook_thrower" };
            var all = new Color32[W * 24 * H];
            for (int f = 0; f < 24; f++)
            {
                var fr = new Color32[W * H];
                if (f < 16) DrawWalk(fr, f / 4, f % 4);
                else { var b = new Color32[W * H]; DrawWalk(b, 0, 0); Death(b, fr, f - 16); }
                for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) all[(H - 1 - y) * W * 24 + f * W + x] = fr[y * W + x]; // fr is y-down
            }
            tex.SetPixels32(all); tex.Apply(false, false);
            _s = new Sprite[24]; var piv = new Vector2(12f / W, 1f / H);
            for (int f = 0; f < 24; f++) { _s[f] = Sprite.Create(tex, new Rect(f * W, 0, W, H), piv, HArt.PPU, 0, SpriteMeshType.FullRect); _s[f].name = "PLACEHOLDER_hook_thrower_" + f; }
            return _s;
        }
        static void P(Color32[] b, int x, int y, Color32 c) { if (x >= 0 && y >= 0 && x < W && y < H) b[y * W + x] = c; }
        static void Ell(Color32[] b, float cx, float cy, float rx, float ry, Color32 c) { for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) { float dx = (x + 0.5f - cx) / rx, dy = (y + 0.5f - cy) / ry; if (dx * dx + dy * dy <= 1) b[y * W + x] = c; } }
        // dir 0 S, 1 E, 2 N, 3 W ; k = walk frame 0..3
        static void DrawWalk(Color32[] b, int dir, int k)
        {
            int bob = (k == 1 || k == 3) ? 1 : 0; int hy = 8 + bob;
            // robe (trapezoid) with outline
            for (int y = 13 + bob; y <= 30; y++) { float t = (y - 13f) / 17f; int hw = Mathf.RoundToInt(4 + 4 * t); for (int x = 12 - hw; x < 12 + hw; x++) P(b, x, y, x == 12 - hw || x == 12 + hw - 1 ? K : (x < 12 - hw + 2 ? RobeD : x > 12 + hw - 3 ? RobeL : Robe)); }
            for (int x = 4; x < 20; x++) P(b, x, 31, K);
            // feet (alternate)
            int lf = k == 1 ? -1 : 0, rf = k == 3 ? -1 : 0;
            for (int x = 8; x <= 10; x++) { P(b, x, 31 + lf, K); P(b, x, 32 + lf, Skin); }
            for (int x = 13; x <= 15; x++) { P(b, x, 31 + rf, K); P(b, x, 32 + rf, Skin); }
            // hood
            Ell(b, 12, hy + 1, 6.6f, 6.6f, K); Ell(b, 12, hy + 1, 5.6f, 5.6f, dir == 2 ? Robe : RobeD);
            for (int x = 7; x <= 16; x++) P(b, x, hy + 6, Rim);
            if (dir != 2)
            {   // face hole + eyes
                int fx = dir == 1 ? 14 : dir == 3 ? 10 : 12;
                Ell(b, fx, hy + 2, 3.4f, 2.6f, K);
                if (dir == 0) { P(b, fx - 2, hy + 2, Eye); P(b, fx + 1, hy + 2, Eye); } else { P(b, fx + (dir == 1 ? 1 : -2), hy + 2, Eye); }
            }
            else for (int y = hy - 3; y <= hy + 4; y++) P(b, 12, y, RobeD); // hood seam (back)
            // chain coil + hook held at the side (east hand for S/E, west for N/W)
            int side = dir == 3 || dir == 2 ? -1 : 1; int cx = 12 + side * 8, cy = 20 + bob;
            for (int a = 0; a < 10; a++) { float an = a / 10f * Mathf.PI * 2; P(b, cx + Mathf.RoundToInt(Mathf.Cos(an) * 2.5f), cy + Mathf.RoundToInt(Mathf.Sin(an) * 2.5f), a % 2 == 0 ? Ch : ChD); }
            for (int y = cy - 7; y <= cy - 3; y++) P(b, cx, y, ChD);
            P(b, cx, cy - 8, Hook); P(b, cx + side, cy - 9, Hook); P(b, cx + side * 2, cy - 8, Hook); P(b, cx + side * 2, cy - 7, Hook); P(b, cx + side, cy - 6, K);
            // v0.0.53 B13/B18: "PH" placeholder tag removed
        }
        static void Death(Color32[] src, Color32[] dst, int i)
        {   // squash toward the feet + fade (8 frames)
            float sy = 1f - i * 0.11f; byte a = (byte)Mathf.Clamp(255 - i * 32, 0, 255);
            for (int y = 0; y < H; y++)
            {
                int srcY = H - 1 - Mathf.RoundToInt((H - 1 - y) / Mathf.Max(0.12f, sy)); if (srcY < 0) continue;
                for (int x = 0; x < W; x++) { var c = src[srcY * W + x]; if (c.a == 0) continue; c.a = (byte)(c.a * a / 255); dst[y * W + x] = c; }
            }
        }
    }
}
