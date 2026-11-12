// v0.0.25: Gossip Imp - Level 1 (Liars) unique enemy, "Shout area + player aim is scrambled" (Core Combat Design v0.2.8).
// FX: Pixel FX Pack 2.5.1 (was 1.9.2) GossipHookFX.Shout (red ring grows + "!" = the warning, then a short burst) and GossipHookFX.Scramble
// (orbiting "?" + counter-rotating foot arrows over the hero = the status). Gameplay (ring hit test at the burst, scramble timer,
// aim bending) lives here; the pack only draws. Every number is in HellordTuning ("Gossip Imp").
// Art: CODE-DRAWN PLACEHOLDER (GossipImpArt below, Endesga 32, yellow "PH" tag) until Hellord Art makes the real sheet.
using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

namespace Hellord
{
    public partial class HellordGame
    {
        static readonly float ArgGossipHp = ArgF("-gossiphp", 0f); // clip/debug only: tougher imps so a recording shows several shouts
        static readonly bool ArgGossipImps = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-gossipimps") >= 0;
        readonly List<Enemy> _gossips = new List<Enemy>(8);
        readonly List<KeyValuePair<int, Vector2>> _orphanShouts = new List<KeyValuePair<int, Vector2>>(4); // shouts whose imp died (gossipDeathCancelsShout off)
        bool _gossipSetup; float _scrambleT; int _scrambleH = -1, _scrambleSign = 1;
        [System.NonSerialized] public int gossipShouts, gossipBursts, gossipHits, gossipIframeBlocks, gossipNoStack; // harness / bot counters
        public bool HeroScrambled => _scrambleT > 0;
        public float ScrambleLeft => Mathf.Max(0, _scrambleT);

        void EnsureGossipFX()
        {
            EnsureHookFX(); // shared GossipHookFX material / sorting / prewarm (v0.0.24)
            if (_gossipSetup) return; _gossipSetup = true;
            GossipHookFX.OnShout += OnGossipShout;
        }
        void InitGossipImp(Enemy e)
        {
            var T = HellordTuning.I; EnsureGossipFX();
            e.maxHp = ArgGossipHp > 0 ? ArgGossipHp : T.gossipHp; e.speed = T.gossipMoveSpeed; e.radius = T.gossipRadiusU; e.hitY = 0.8f; e.dmg = T.gossipContactDamage; e.xp = T.gossipXp;
            e.gossipCd = T.gossipFirstDelay; e.gossipSt = 0; e.gossipT = 0; e.gossipH = -1; e.strafe = Random.value < 0.5f ? -1 : 1;
            if (!_gossips.Contains(e)) _gossips.Add(e);
        }
        /// <summary>Stops the shout owned by this slot (death, despawn, slot reuse, room change). Called from HookCleanup.</summary>
        void GossipCleanup(Enemy e)
        {
            if (e.gossipH >= 0)
            {
                if (HellordTuning.I.gossipDeathCancelsShout || GossipHookFX.GetPhase(e.gossipH) != GHPhase.Tell) GossipHookFX.Cancel(e.gossipH);
                else if (GossipHookFX.GetPhase(e.gossipH) == GHPhase.Tell) _orphanShouts.Add(new KeyValuePair<int, Vector2>(e.gossipH, e.gossipC));
                e.gossipH = -1;
            }
            e.gossipSt = 0;
        }
        void ClearGossip()
        {
            foreach (var e in _gossips) { if (e.gossipH >= 0 && _gossipSetup) GossipHookFX.Cancel(e.gossipH); e.gossipH = -1; e.gossipSt = 0; }
            _gossips.Clear(); _orphanShouts.Clear();
            if (_scrambleH >= 0 && _gossipSetup) GossipHookFX.Cancel(_scrambleH); _scrambleH = -1; _scrambleT = 0;
        }
        int ShoutsInPlay() { int n = _orphanShouts.Count; foreach (var e in _gossips) if (e.alive && e.gossipSt == 2) n++; return n; }
        int GossipsAlive() { int n = 0; foreach (var e in _gossips) if (e.alive && e.type == EType.GossipImp) n++; return n; }

        /// <summary>Movement (called from UpdateEnemies): hover in a mid band while recharging, walk in when the shout is ready, stand still while shouting.</summary>
        Vector2 GossipImpMove(Enemy e, Vector2 to, float dist, float sp)
        {
            var T = HellordTuning.I;
            if (e.gossipSt >= 2 || dist < 0.01f) return Vector2.zero;
            Vector2 dir = to / dist;
            if (e.gossipSt == 1) return dist > T.gossipShoutTriggerU * 0.8f ? dir * (T.gossipApproachSpeed * (e.speed > 0 ? sp / e.speed : 1f)) : Vector2.zero;
            if (dist > T.gossipPreferMaxU) return dir * sp;
            if (dist < T.gossipPreferMinU) return -dir * sp;
            return new Vector2(-dir.y, dir.x) * (e.strafe * sp * 0.45f);
        }

        void UpdateGossip(float dt)
        {
            if (_scrambleT > 0) { _scrambleT -= Time.deltaTime; /* same clock as the pack's status FX, so the '?' icons end exactly with the effect */ if (_scrambleT <= 0) { _scrambleT = 0; if (_scrambleH >= 0) { if (GossipHookFX.GetPhase(_scrambleH) != GHPhase.None) GossipHookFX.Cancel(_scrambleH); _scrambleH = -1; } } }
            for (int i = _orphanShouts.Count - 1; i >= 0; i--) if (GossipHookFX.GetPhase(_orphanShouts[i].Key) == GHPhase.None) _orphanShouts.RemoveAt(i);
            if (_gossips.Count == 0) return;
            var T = HellordTuning.I;
            for (int i = _gossips.Count - 1; i >= 0; i--)
            {
                var e = _gossips[i];
                if (!e.alive || e.type != EType.GossipImp) { GossipCleanup(e); _gossips.RemoveAt(i); continue; }
                if (e.gossipH >= 0 && GossipHookFX.GetPhase(e.gossipH) == GHPhase.None) { e.gossipH = -1; if (e.gossipSt == 2) { e.gossipSt = 3; e.gossipT = 0; } }
                float d = (_heroPos - e.pos).magnitude;
                switch (e.gossipSt)
                {
                    case 0: e.gossipCd -= dt * T.enemyAttackRateMul; // v0.0.27: more often
                        if (e.gossipCd <= 0) { e.gossipSt = 1; e.gossipT = 0; } break;
                    case 1:
                        e.gossipT += dt;
                        if (d <= T.gossipShoutTriggerU && e.freezeT <= 0 && ShoutsInPlay() < Mathf.Max(1, T.gossipMaxShouts))
                        {
                            e.gossipC = e.pos; e.gossipH = GossipHookFX.Shout(e.pos, GossipR49 * HArt.PPU, GossipTell49);
                            if (e.gossipH >= 0) { e.gossipSt = 2; gossipShouts++; HellordAudio.Play("gossip_shout_warning"); } else { e.gossipSt = 0; e.gossipCd = 1f; }
                        }
                        else if (e.gossipT > T.gossipApproachMaxSeconds) { e.gossipSt = 0; e.gossipCd = T.gossipCooldown * 0.5f; }
                        break;
                    case 3: e.gossipT += dt; if (e.gossipT >= T.gossipRecoverSeconds) { e.gossipSt = 0; e.gossipCd = T.gossipCooldown; } break;
                }
            }
        }
        // burst start (pack callback): the ring hit test happens here, against the LOCKED centre, so walking out during the tell dodges
        void OnGossipShout(int h)
        {
            foreach (var e in _gossips) if (e.gossipH == h) { HellordAudio.Play("gossip_shout_blast"); e.gossipSt = 3; e.gossipT = 0; ShoutHit(e.gossipC); return; }
            for (int i = 0; i < _orphanShouts.Count; i++) if (_orphanShouts[i].Key == h) { var c = _orphanShouts[i].Value; _orphanShouts.RemoveAt(i); ShoutHit(c); return; }
        }
        void ShoutHit(Vector2 c)
        {
            var T = HellordTuning.I; gossipBursts++;
            if ((_heroPos - c).magnitude > GossipR49 + T.gossipHeroRadiusU || mode != Mode.Play) return;
            if (HeroInvuln969) { gossipIframeBlocks++; return; }
            if (_scrambleT > 0 && !T.gossipRefreshOnHit) { gossipNoStack++; return; }
            gossipHits++;
            if (T.gossipShoutDamage > 0) HurtHero(T.gossipShoutDamage, "shout");
            _scrambleT = GossipScr49; _scrambleSign = Random.value < 0.5f ? -1 : 1;
            if (_scrambleH >= 0 && GossipHookFX.GetPhase(_scrambleH) != GHPhase.None) GossipHookFX.Cancel(_scrambleH);
            GossipHookFX.ScrambleHeadPx = T.gossipScrambleHeadPx; _scrambleH = GossipHookFX.Scramble(_hero, GossipScr49);
            Toast("AIM SCRAMBLED");
        }
        /// <summary>Bends an attack aim direction / offset while scrambled (length preserved). Identity otherwise.</summary>
        Vector2 ScrambleAim(Vector2 d)
        {
            if (_scrambleT <= 0) return d;
            var T = HellordTuning.I; float L = d.magnitude; if (L < 1e-5f) return d; Vector2 n = d / L;
            switch (T.gossipScrambleMode)
            {
                case 1: { float a = _scrambleSign * Random.Range(T.gossipScrambleMinDeg, T.gossipScrambleMaxDeg) * Mathf.Deg2Rad; _scrambleSign = -_scrambleSign; float cs = Mathf.Cos(a), sn = Mathf.Sin(a); return new Vector2(n.x * cs - n.y * sn, n.x * sn + n.y * cs) * L; }
                case 2: return -d;
                default: return new Vector2(-d.x, Mathf.Abs(n.x) < T.gossipMirrorMinX ? -d.y : d.y);
            }
        }

        /// <summary>Debug I: spawn one Gossip Imp ~6u from the hero (placeholder art).</summary>
        Enemy SpawnGossipImpNearHero()
        {
            var p = _heroPos + Random.insideUnitCircle.normalized * 6f; ClampToRoom(ref p, 10);
            return SpawnEnemyAt(EType.GossipImp, p);
        }
        /// <summary>Normal floor spawns: a small chance to mix a Gossip Imp in (tuning gossipImpsInRooms or -gossipimps).</summary>
        bool TryFloorGossipImp(float chance = -1f)
        {
            var T = HellordTuning.I; if (!(T.gossipImpsInRooms || ArgGossipImps)) return false;
            if (Random.value >= (chance >= 0 ? chance : T.gossipSpawnChance) || GossipsAlive() >= T.gossipMaxAlive + FloorIdx) return false;
            SpawnEnemy(EType.GossipImp); return true;
        }
        // bot state: shout rings (locked centre, radius u, phase Tell/Burst) + imps walking in to shout ("Ready", r = shout radius) + scramble seconds left
        void AppendGossip(System.Text.StringBuilder b)
        {
            b.Append(",\"shouts\":["); bool first = true; float R = GossipR49;
            foreach (var e in _gossips)
            {
                if (!e.alive) continue; string ph = null; Vector2 c = e.pos;
                if (e.gossipSt == 2 && e.gossipH >= 0) { var p = GossipHookFX.GetPhase(e.gossipH); if (p == GHPhase.Tell || p == GHPhase.Burst) { ph = p.ToString(); c = e.gossipC; } }
                else if (e.gossipSt == 1) ph = "Ready";
                if (ph == null) continue;
                if (!first) b.Append(','); first = false;
                b.Append("{\"x\":").Append(F(c.x)).Append(",\"y\":").Append(F(c.y)).Append(",\"r\":").Append(F(R)).Append(",\"ph\":\"").Append(ph).Append("\"}");
            }
            foreach (var kv in _orphanShouts) { if (!first) b.Append(','); first = false; b.Append("{\"x\":").Append(F(kv.Value.x)).Append(",\"y\":").Append(F(kv.Value.y)).Append(",\"r\":").Append(F(R)).Append(",\"ph\":\"Tell\"}"); }
            b.Append("],\"scrambled\":").Append(F(ScrambleLeft));
        }
    }

    /// <summary>v0.0.25 CODE-DRAWN PLACEHOLDER sheet for the Gossip Imp (24 frames: walk S/E/N/W x4, death x8), Endesga 32 palette,
    /// with a yellow "PH" tag so nobody mistakes it for final art. Small pink imp with bat wings and a huge shouting mouth.
    /// Replace by adding a real sheet to EnemyArtMap.</summary>
    public static class GossipImpArt
    {
        const int W = 24, H = 28;
        static Sprite[] _s;
        static readonly Color32 K = C(0x18, 0x14, 0x25), SkD = C(0x68, 0x38, 0x6c), Sk = C(0xb5, 0x50, 0x88), SkL = C(0xf6, 0x75, 0x7a), Horn = C(0xe8, 0xb7, 0x96),
            Wing = C(0x3e, 0x27, 0x31), Eye = C(0xfe, 0xe7, 0x61), Mouth = C(0x9e, 0x28, 0x35), Tongue = C(0xe4, 0x3b, 0x44), Tooth = C(0xff, 0xff, 0xff), Tag = C(0xfe, 0xe7, 0x61);
        static Color32 C(byte r, byte g, byte b) => new Color32(r, g, b, 255);
        public static Sprite[] Placeholder()
        {
            if (_s != null) return _s;
            var tex = new Texture2D(W * 24, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "PLACEHOLDER_gossip_imp" };
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
            for (int f = 0; f < 24; f++) { _s[f] = Sprite.Create(tex, new Rect(f * W, 0, W, H), piv, HArt.PPU, 0, SpriteMeshType.FullRect); _s[f].name = "PLACEHOLDER_gossip_imp_" + f; }
            return _s;
        }
        static void P(Color32[] b, int x, int y, Color32 c) { if (x >= 0 && y >= 0 && x < W && y < H) b[y * W + x] = c; }
        static void Ell(Color32[] b, float cx, float cy, float rx, float ry, Color32 c) { for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) { float dx = (x + 0.5f - cx) / rx, dy = (y + 0.5f - cy) / ry; if (dx * dx + dy * dy <= 1) b[y * W + x] = c; } }
        // dir 0 S, 1 E, 2 N, 3 W ; k = walk frame 0..3 (hop: the imp bobs and flaps)
        static void DrawWalk(Color32[] b, int dir, int k)
        {
            int bob = (k == 1 || k == 3) ? 1 : 0, flap = k % 2 == 0 ? 0 : 2; int cy = 15 + bob;
            // wings (behind)
            for (int s = -1; s <= 1; s += 2)
                for (int i = 0; i < 5; i++) for (int j = 0; j <= i; j++) { int x = 12 + s * (6 + i), y = cy - 5 + j + flap - (dir == 2 ? 1 : 0); P(b, x, y, j == i ? K : Wing); }
            // feet
            int lf = k == 1 ? -1 : 0, rf = k == 3 ? -1 : 0;
            for (int x = 8; x <= 10; x++) { P(b, x, 25 + lf, K); P(b, x, 26 + lf, SkD); }
            for (int x = 13; x <= 15; x++) { P(b, x, 25 + rf, K); P(b, x, 26 + rf, SkD); }
            // round body-head with outline + shading
            Ell(b, 12, cy, 7.2f, 7.6f, K); Ell(b, 12, cy, 6.2f, 6.6f, Sk); Ell(b, 13.5f, cy + 2, 4.4f, 4.4f, Sk); Ell(b, 10, cy - 3, 2.4f, 2f, SkL);
            for (int y = cy - 5; y <= cy + 5; y++) P(b, 6 + (y > cy + 2 ? 1 : 0), y, SkD);
            // horns + pointy ears
            P(b, 8, cy - 7, K); P(b, 8, cy - 8, Horn); P(b, 7, cy - 9, Horn); P(b, 16, cy - 7, K); P(b, 16, cy - 8, Horn); P(b, 17, cy - 9, Horn);
            if (dir == 2) { for (int y = cy - 4; y <= cy + 4; y++) P(b, 12, y, SkD); return; } // back view: seam only
            // eyes + huge shouting mouth (offset to the facing side)
            int fx = dir == 1 ? 14 : dir == 3 ? 10 : 12;
            if (dir == 0) { P(b, fx - 3, cy - 3, Eye); P(b, fx + 2, cy - 3, Eye); P(b, fx - 3, cy - 4, K); P(b, fx + 2, cy - 4, K); }
            else { int ex = fx + (dir == 1 ? 1 : -2); P(b, ex, cy - 3, Eye); P(b, ex, cy - 4, K); }
            Ell(b, fx, cy + 2, dir == 0 ? 4.2f : 3.4f, 3.2f, K); Ell(b, fx, cy + 2, dir == 0 ? 3.2f : 2.4f, 2.3f, Mouth); Ell(b, fx, cy + 3.5f, 2f, 1f, Tongue);
            for (int x = fx - 2; x <= fx + 2; x += 2) P(b, x, cy, Tooth);
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
