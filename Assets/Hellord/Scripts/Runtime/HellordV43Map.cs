using System.Collections.Generic;
using UnityEngine;

// v0.0.43: RANDOM FLOOR MAP (VH: "floor will random all rooms position in floor only once when player start the floor").
// When a floor starts, one map is generated from the run seed: room positions on the grid + the type of every room.
// It is then turned into the same layout strings the hand-made v0.8 layouts used, so doors, back-tracking, the minimap
// and the secret cracked wall all work exactly like before. Rooms keep their normal rectangle art.
namespace Hellord
{
    public partial class HellordGame
    {
        string[] _genLayout43;
        /// <summary>v0.0.43: the generated layout of the current floor (rows of chars, legend as HellordFloor.Layouts).</summary>
        public string[] GenLayout43 => _genLayout43;

        string[] GenerateLayout43(int seed)
        {
            var T = HellordTuning.I; var rng = new System.Random(seed);
            int W = Mathf.Clamp(T.mapGridW, 4, 12), H = Mathf.Clamp(T.mapGridH, 4, 10);
            for (int attempt = 0; attempt < 40; attempt++)
            {
                var g = TryGen43(rng, W, H);
                if (g != null) return g;
            }
            Debug.LogWarning("[Hellord] v0.0.43 map generation failed 40 times, using hand-made layout 0");
            return Layouts[0];
        }

        static int Nb43(char[,] m, int x, int y, int W, int H)
        {
            int n = 0;
            for (int d = 0; d < 4; d++) { int nx = x + DX[d], ny = y + DY[d]; if (nx >= 0 && ny >= 0 && nx < W && ny < H && m[nx, ny] != '.') n++; }
            return n;
        }

        string[] TryGen43(System.Random rng, int W, int H)
        {
            var T = HellordTuning.I;
            var m = new char[W, H];
            for (int x = 0; x < W; x++) for (int y = 0; y < H; y++) m[x, y] = '.';
            int lo = Mathf.Max(3, T.mapRoomsMin), hi = Mathf.Max(lo, T.mapRoomsMax);
            int want = Mathf.Min(rng.Next(lo, hi + 1), W * H / 2);
            int sx = W / 2, sy = H / 2 + (H > 4 ? 1 : 0);
            var cells = new List<Vector2Int> { new Vector2Int(sx, sy) }; m[sx, sy] = 'R';
            // grow: attach new rooms next to existing ones; a new cell touching >1 room makes a loop (only with mapLoopChance)
            for (int tries = 0; cells.Count < want && tries < 2000; tries++)
            {
                var c = cells[rng.Next(cells.Count)]; int d = rng.Next(4);
                int nx = c.x + DX[d], ny = c.y + DY[d];
                if (nx < 0 || ny < 1 || nx >= W || ny >= H || m[nx, ny] != '.') continue; // row 0 stays free for secret rooms
                int nb = Nb43(m, nx, ny, W, H);
                if (nb > 1 && rng.NextDouble() >= T.mapLoopChance) continue;
                if (nb >= 3) continue;
                m[nx, ny] = 'R'; cells.Add(new Vector2Int(nx, ny));
            }
            if (cells.Count < lo) return null;
            // distances from the start (all grid neighbours are linked, like the old layouts)
            var dist = new int[W, H]; for (int x = 0; x < W; x++) for (int y = 0; y < H; y++) dist[x, y] = -1;
            var q = new Queue<Vector2Int>(); q.Enqueue(cells[0]); dist[sx, sy] = 0;
            while (q.Count > 0)
            {
                var c = q.Dequeue();
                for (int d = 0; d < 4; d++)
                {
                    int nx = c.x + DX[d], ny = c.y + DY[d];
                    if (nx < 0 || ny < 0 || nx >= W || ny >= H || m[nx, ny] == '.' || dist[nx, ny] >= 0) continue;
                    dist[nx, ny] = dist[c.x, c.y] + 1; q.Enqueue(new Vector2Int(nx, ny));
                }
            }
            // boss: an empty cell touching exactly one room, as far from the start as possible (a dead end)
            int bx = -1, by = -1, bd = -1;
            for (int x = 0; x < W; x++)
                for (int y = 1; y < H; y++)
                {
                    if (m[x, y] != '.' || Nb43(m, x, y, W, H) != 1) continue;
                    int dd = 0;
                    for (int d = 0; d < 4; d++) { int nx = x + DX[d], ny = y + DY[d]; if (nx >= 0 && ny >= 0 && nx < W && ny < H && m[nx, ny] != '.') dd = dist[nx, ny] + 1; }
                    if (dd == 1) continue; // never next to the start
                    if (dd > bd || (dd == bd && rng.Next(2) == 0)) { bd = dd; bx = x; by = y; }
                }
            if (bx < 0 || bd < Mathf.Max(2, T.mapBossMinDist)) return null;
            m[bx, by] = 'B';
            m[sx, sy] = 'S';
            // room types: specials first (dead ends preferred, never next to the start), then fights
            var free = new List<Vector2Int>();
            for (int i = 1; i < cells.Count; i++) free.Add(cells[i]);
            for (int i = free.Count - 1; i > 0; i--) { int j = rng.Next(i + 1); var t = free[i]; free[i] = free[j]; free[j] = t; }
            int bossNbX = -1, bossNbY = -1;
            for (int d = 0; d < 4; d++) { int nx = bx + DX[d], ny = by + DY[d]; if (nx >= 0 && ny >= 0 && nx < W && ny < H && m[nx, ny] == 'R') { bossNbX = nx; bossNbY = ny; } }
            int fightsMin = Mathf.Max(1, T.mapMinFights);
            int specialsLeft = free.Count - fightsMin;
            PlaceSpecial43(rng, m, free, dist, W, H, 'H', RollCount43(rng, T.mapShops), ref specialsLeft, bossNbX, bossNbY);
            PlaceSpecial43(rng, m, free, dist, W, H, 'E', RollCount43(rng, T.mapElites), ref specialsLeft, bossNbX, bossNbY);
            PlaceSpecial43(rng, m, free, dist, W, H, 'T', RollCount43(rng, T.mapTreasures), ref specialsLeft, bossNbX, bossNbY);
            PlaceSpecial43(rng, m, free, dist, W, H, 'C', RollCount43(rng, T.mapCurses), ref specialsLeft, bossNbX, bossNbY);
            int nests = FloorIdx == 0 ? 0 : RollCount43(rng, T.mapNests), nestsPut = 0; // v0.0.95 t1014u: never hive on floor 1
            foreach (var c in free)
            {
                if (m[c.x, c.y] != 'R') continue;
                if (nestsPut < nests && dist[c.x, c.y] >= 2) { m[c.x, c.y] = '3'; nestsPut++; continue; }
                float ws = Mathf.Max(0, T.mapSurviveWeight), wh = Mathf.Max(0, T.mapHuntWeight);
                m[c.x, c.y] = (ws + wh <= 0.0001f || rng.NextDouble() * (ws + wh) < ws) ? '1' : '2';
            }
            // the shop must exist (if asked for): if every candidate failed, turn any fight into the shop
            if (T.mapShops.x > 0 && !Has43(m, W, H, 'H'))
                foreach (var c in free) if (m[c.x, c.y] == '1' || m[c.x, c.y] == '2') { m[c.x, c.y] = 'H'; break; }
            // secret room: above a fight room (cracked north wall), in an empty cell
            if (rng.NextDouble() < T.secretRoomChance)
            {
                var hosts = new List<Vector2Int>();
                foreach (var c in free) { char ch = m[c.x, c.y]; if ((ch == '1' || ch == '2' || ch == '3') && c.y > 0 && m[c.x, c.y - 1] == '.') hosts.Add(c); }
                if (hosts.Count > 0) { var h = hosts[rng.Next(hosts.Count)]; m[h.x, h.y - 1] = 'X'; }
            }
            // crop to the used box
            int x0 = W, x1 = -1, y0 = H, y1 = -1;
            for (int x = 0; x < W; x++) for (int y = 0; y < H; y++) if (m[x, y] != '.') { x0 = Mathf.Min(x0, x); x1 = Mathf.Max(x1, x); y0 = Mathf.Min(y0, y); y1 = Mathf.Max(y1, y); }
            var rows = new string[y1 - y0 + 1];
            for (int y = y0; y <= y1; y++) { var sb = new System.Text.StringBuilder(); for (int x = x0; x <= x1; x++) sb.Append(m[x, y]); rows[y - y0] = sb.ToString(); }
            return rows;
        }
        static bool Has43(char[,] m, int W, int H, char c) { for (int x = 0; x < W; x++) for (int y = 0; y < H; y++) if (m[x, y] == c) return true; return false; }
        static int RollCount43(System.Random rng, Vector2Int mm) { int a = Mathf.Max(0, mm.x), b = Mathf.Max(a, mm.y); return rng.Next(a, b + 1); }

        void PlaceSpecial43(System.Random rng, char[,] m, List<Vector2Int> free, int[,] dist, int W, int H, char c, int n, ref int left, int bnx, int bny)
        {
            var T = HellordTuning.I;
            for (int k = 0; k < n && left > 0; k++)
            {
                int best = -1; float bs = -1;
                for (int i = 0; i < free.Count; i++)
                {
                    var p = free[i]; if (m[p.x, p.y] != 'R') continue;
                    int dd = dist[p.x, p.y];
                    if (dd < 2) continue;                                                   // rooms next to the start are plain fights
                    if ((c == 'E' || c == 'C') && dd < Mathf.Max(2, T.mapEliteMinDist)) continue;
                    if (p.x == bnx && p.y == bny && c != 'H') continue;                     // room before the boss: shop or fight
                    float s = (float)rng.NextDouble();
                    if (Nb43(m, p.x, p.y, W, H) == 1) s += T.mapDeadEndBias;               // specials like dead ends
                    if (c == 'H' && p.x == bnx && p.y == bny) s += T.mapShopNearBossBias;
                    if (s > bs) { bs = s; best = i; }
                }
                if (best < 0) return;
                m[free[best].x, free[best].y] = c; left--;
            }
        }
    }
}
