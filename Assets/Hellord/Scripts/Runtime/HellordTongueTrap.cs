using System.Collections.Generic;
using UnityEngine;

// v0.0.21: Level 1 "Tongue Pulling" floor hazard (design approved by Viet Hung). Every number is in HellordTuning.
// Hero within trigger radius -> tell (strike point locked at trigger time, so moving dodges) -> strike (circle around the slit
// + lash toward the locked point) -> retract -> cooldown (dim slit). Hero damage uses HurtHero (normal i-frames, non-boss 20% cap).
// ART: HA's 8-direction sheet is a PLACEHOLDER mock-up (not approved), loaded through HellordTuning.tongueSheet / tongueSheetResource.
namespace Hellord
{
    public partial class HellordGame
    {
        enum TTState { Idle, Tell, Strike, Retract, Cooldown }
        class TongueTrap { public Vector2 pos, lockP; public int dir; public TTState st; public float t; public bool hitDone; public SpriteRenderer sr; public HashSet<int> hitEn = new HashSet<int>(); }
        readonly List<TongueTrap> _traps = new List<TongueTrap>();
        Sprite[,] _ttF; bool _ttLoaded; Transform _ttRoot;
        static readonly bool ArgTraps = System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-traps") >= 0;
        static HellordTuning TU => HellordTuning.I;

        void LoadTongueArt()
        {
            if (_ttLoaded) return; _ttLoaded = true;
            var tex = TU.tongueSheet != null ? TU.tongueSheet : Resources.Load<Texture2D>(TU.tongueSheetResource);
            if (tex == null) { Debug.LogWarning("[Hellord] tongue trap sheet missing"); return; }
            tex.filterMode = FilterMode.Point; int fp = TU.tongueFramePx, cols = tex.width / fp, rows = tex.height / fp;
            _ttF = new Sprite[rows, cols];
            for (int r = 0; r < rows; r++) for (int c = 0; c < cols; c++)
                _ttF[r, c] = Sprite.Create(tex, new Rect(c * fp, tex.height - (r + 1) * fp, fp, fp), new Vector2(0.5f, 0.5f), HArt.PPU, 0, SpriteMeshType.FullRect);
            Debug.Log($"[Hellord] tongue trap art: {(TU.tongueSheet != null ? TU.tongueSheet.name : TU.tongueSheetResource)} (PLACEHOLDER) {rows}x{cols}");
        }
        void ClearTongueTraps() { foreach (var t in _traps) if (t.sr) Destroy(t.sr.gameObject); _traps.Clear(); }

        /// <summary>Places 3-6 traps (tunable) near the walls of the current room: never in the open middle, clear of doors and the hero/spawn.</summary>
        public int SpawnTongueTraps(int count = -1)
        {
            if (_room == null) return 0; LoadTongueArt(); ClearTongueTraps();
            if (_ttRoot == null) _ttRoot = new GameObject("TongueTraps").transform;
            var D = _room.D; if (count < 0) count = Random.Range(TU.tongueMinPerRoom, TU.tongueMaxPerRoom + 1);
            float m = 14, x0 = D.wallSide + m, x1 = D.w - D.wallSide - m, y0 = D.wallN + m, y1 = D.h - D.wallS - m, band = TU.tongueEdgeBandPx, clr = TU.tongueDoorSpawnClearU * HArt.PPU;
            var avoid = new List<Vector2> { HeroPx, new Vector2(D.spawnX > 0 ? D.spawnX : D.w / 2, D.spawnY > 0 ? D.spawnY : D.h / 2 + 40) };
            foreach (var d in _doors) { avoid.Add(DoorPivotPx(d.dir)); avoid.Add(ArrivePx(d.dir)); }
            for (int d = 0; d < 4; d++) avoid.Add(ArrivePx(d));
            if (Cur != null && Cur.type == RoomType.Shrine) avoid.Add(new Vector2(D.shrineX > 0 ? D.shrineX : D.w / 2, D.shrineY > 0 ? D.shrineY : D.h / 2));
            int placed = 0;
            for (int tries = 0; tries < 400 && placed < count; tries++)
            {
                var p = new Vector2(Random.Range(x0, x1), Random.Range(y0, y1));
                float edge = Mathf.Min(Mathf.Min(p.x - x0, x1 - p.x), Mathf.Min(p.y - y0, y1 - p.y)); if (edge > band) continue; // never the open middle
                bool ok = true; foreach (var a in avoid) if ((a - p).magnitude < clr) { ok = false; break; }
                if (ok) foreach (var t in _traps) if ((HellordRoom.Px(p.x, p.y) - t.pos).magnitude * HArt.PPU < TU.tongueMinSpacingPx) { ok = false; break; }
                if (!ok) continue;
                var tr = new TongueTrap { pos = HellordRoom.Px(Mathf.Round(p.x), Mathf.Round(p.y)), st = TTState.Idle, dir = 2 };
                tr.sr = NewSR("tongue_trap_PLACEHOLDER", _ttRoot, _ttF != null ? _ttF[2, 0] : null, HellordRoom.PropOrder + 45); tr.sr.transform.position = tr.pos;
                _traps.Add(tr); placed++;
            }
            Debug.Log($"[Hellord] tongue traps placed {placed}/{count} in room {curRoom}");
            return placed;
        }

        static int Dir8(Vector2 v) { float a = Mathf.Atan2(-v.y, v.x) * Mathf.Rad2Deg; int k = Mathf.RoundToInt(a / 45f); return ((k % 8) + 8) % 8; } // rows E,SE,S,SW,W,NW,N,NE (y down)

        static float SegDist(Vector2 p, Vector2 a, Vector2 b) { var ab = b - a; float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(1e-5f, ab.sqrMagnitude)); return (a + ab * t - p).magnitude; }
        bool InStrike(TongueTrap t, Vector2 p)
        {
            if ((p - t.pos).magnitude <= TU.tongueStrikeRadiusU) return true;
            var d = t.lockP - t.pos; if (d.sqrMagnitude < 1e-4f) return false;
            return SegDist(p, t.pos, t.pos + d.normalized * TU.tongueLashLengthU) <= TU.tongueLashHalfWidthU;
        }

        void UpdateTongueTraps(float dt)
        {
            if (_traps.Count == 0) return;
            Vector2 hc = _heroPos + Vector2.up * 0.3f; // body point used for traps (feet-ish, matches the floor hazard)
            foreach (var t in _traps)
            {
                t.t += dt;
                switch (t.st)
                {
                    case TTState.Idle:
                        if ((hc - t.pos).magnitude * HArt.PPU <= TU.tongueTriggerRadiusPx && mode == Mode.Play)
                        { t.st = TTState.Tell; t.t = 0; t.lockP = hc; t.dir = Dir8(t.lockP - t.pos); t.hitDone = false; t.hitEn.Clear(); }
                        break;
                    case TTState.Tell: if (t.t >= TU.tongueTellSeconds) { t.st = TTState.Strike; t.t = 0; } break;
                    case TTState.Strike:
                        if (!t.hitDone && mode == Mode.Play && InStrike(t, hc)) { t.hitDone = true; HurtHero(TU.tongueDamage, "tongue_trap"); }
                        if (TU.tongueHitsEnemies)
                            for (int i = 0; i < maxEnemies; i++) { var e = _en[i]; if (e.alive && !e.boss && !t.hitEn.Contains(e.id) && InStrike(t, e.pos)) { t.hitEn.Add(e.id); Damage(e, TU.tongueEnemyDamage, false, false); } }
                        if (t.t >= TU.tongueActiveSeconds) { t.st = TTState.Retract; t.t = 0; }
                        break;
                    case TTState.Retract: if (t.t >= TU.tongueRetractSeconds) { t.st = TTState.Cooldown; t.t = 0; } break;
                    case TTState.Cooldown: if (t.t >= TU.tongueCooldown) { t.st = TTState.Idle; t.t = 0; } break;
                }
                // animation (sheet cols: 0 idle, 1 cooldown, 2-3 tell, 4-6 whip, 7-8 retract)
                int col = 0; float k;
                switch (t.st)
                {
                    case TTState.Tell: col = t.t < TU.tongueTellSeconds * 0.5f ? 2 : 3; break;
                    case TTState.Strike: k = t.t / Mathf.Max(0.01f, TU.tongueActiveSeconds); col = k < 0.25f ? 4 : k < 0.5f ? 5 : 6; break;
                    case TTState.Retract: col = t.t < TU.tongueRetractSeconds * 0.5f ? 7 : 8; break;
                    case TTState.Cooldown: col = 1; break;
                }
                if (_ttF != null) t.sr.sprite = _ttF[Mathf.Min(t.dir, _ttF.GetLength(0) - 1), Mathf.Min(col, _ttF.GetLength(1) - 1)];
                var c = t.st == TTState.Cooldown ? new Color(0.55f, 0.5f, 0.5f, 1f) : Color.white; if (t.sr.color != c) t.sr.color = c; // dim slit while recharging
                t.sr.sortingOrder = t.st == TTState.Strike || t.st == TTState.Retract ? YSort(t.pos.y) + 1 : HellordRoom.PropOrder + 45;
            }
        }
    }
}
