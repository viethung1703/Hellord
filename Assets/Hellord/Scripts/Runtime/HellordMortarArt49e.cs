using System.Collections.Generic;
using UnityEngine;

// v0.0.49e: PixelLab v14 Mortar enemy sheets (Bloater, Rice Ghost, Grinder Monk). Art only: no Mortar floor / spawns / combat yet.
// Layout (same as Frost Caster): 38x32 cells, 8 cols x 12 rows, pivot (19,27), PPU 16. Rows from the top:
// 0-2 idle S/E/N, 3-5 walk S/E/N, 6-8 cast S/E/N, 9-10 hurt S/E, 11 death S. W = flip E.
namespace Hellord
{
    public enum MortarKind49e { Bloater, RiceGhost, GrinderMonk }
    public enum MortarAnim49e { Idle = 0, Walk = 3, Cast = 6, Hurt = 9, Death = 11 }

    public static class MortarArt49e
    {
        static readonly string[] Names = { "enemy_bloater", "enemy_rice_ghost", "enemy_grinder_monk" };
        /// <summary>Real frame counts per row (from the v14 json), so loops skip empty cells.</summary>
        static readonly int[][] RowFrames =
        {
            new[] { 8, 8, 8, 8, 8, 8, 8, 8, 8, 6, 6, 8 },
            new[] { 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8 },
            new[] { 8, 8, 8, 8, 8, 8, 8, 8, 8, 6, 6, 7 },
        };
        static readonly Dictionary<int, Sprite[]> _cache = new Dictionary<int, Sprite[]>();
        public static string SheetName(MortarKind49e k) => Names[(int)k];
        /// <summary>All 96 frames, or null when the sheet is off / missing.</summary>
        public static Sprite[] Frames(MortarKind49e k)
        {
            if (!HellordTuning.I.mortarSheets49e) return null;
            if (_cache.TryGetValue((int)k, out var f)) return f;
            f = HArt.Missing(Names[(int)k]) ? null : HArt.Sheet(Names[(int)k]); _cache[(int)k] = f; return f;
        }
        /// <summary>dir: 0 S, 1 E, 2 N, 3 W (flipX). Hurt has no N (uses S); death is S only.</summary>
        public static int Row(MortarAnim49e a, int dir)
        {
            int d = dir == 3 ? 1 : dir;
            if (a == MortarAnim49e.Death) return 11;
            if (a == MortarAnim49e.Hurt) return 9 + (d == 1 ? 1 : 0);
            return (int)a + d;
        }
        public static int FrameCount(MortarKind49e k, int row) => Mathf.Min(8, RowFrames[(int)k][row]);
        /// <summary>Sprite index for anim time t (seconds) at fps; loops unless Death.</summary>
        public static int Index(MortarKind49e k, MortarAnim49e a, int dir, float t, float fps = 8f)
        {
            int row = Row(a, dir), n = FrameCount(k, row), f = Mathf.FloorToInt(t * fps);
            f = a == MortarAnim49e.Death || a == MortarAnim49e.Hurt ? Mathf.Min(f, n - 1) : ((f % n) + n) % n;
            return row * 8 + f;
        }
        public static bool FlipX(int dir) => dir == 3;
    }
}
