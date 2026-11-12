using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    [System.Serializable] public class LwItem { public string key, file; public int w, h, frames; public int[] pivot, ms; public string[] order; }
    [System.Serializable] public class LwSet { public string level; public int side_door_centre_y; public LwItem[] items; }

    /// <summary>v0.0.44: low walls + per-side doors (Ox Pit v30d, Frozen v32, Burning v32). Every side has its own drawing: no rotation, no flip.
    /// Sizes, pivots, frame order and ms come from Resources/HellordArt/LowWalls/lowwalls_&lt;level&gt;.json.</summary>
    public static class LowWalls
    {
        const string Dir = "HellordArt/LowWalls/";
        static readonly Dictionary<string, LwSet> _sets = new Dictionary<string, LwSet>();
        static readonly Dictionary<string, Sprite[]> _frames = new Dictionary<string, Sprite[]>();
        static readonly Dictionary<string, Sprite> _clips = new Dictionary<string, Sprite>();

        public static bool Mortar52; // v0.0.52 torture floors: Ox-logic depth drawn with the Mortar B skin
        public static string Level(int theme) => theme == 0 && Mortar52 ? "mortar" : theme == 1 ? "frozen" : theme == 2 ? "burning" : "oxpit";
        public static LwSet Set(string lv)
        {
            if (_sets.TryGetValue(lv, out var s)) return s;
            var ta = Resources.Load<TextAsset>(Dir + "lowwalls_" + lv);
            s = ta != null ? JsonUtility.FromJson<LwSet>(ta.text) : null; _sets[lv] = s;
            if (s == null) Debug.LogWarning("[Hellord] lowwalls_" + lv + ".json missing");
            return s;
        }
        public static bool Available(int theme) => Set(Level(theme)) != null;
        public static LwItem Item(string lv, string key)
        {
            var s = Set(lv); if (s == null || s.items == null) return null;
            foreach (var i in s.items) if (i.key == key) return i;
            return null;
        }
        /// <summary>Frames of one strip, pivot top-left (place with the top-left room px).</summary>
        public static Sprite[] Frames(string lv, string key)
        {
            string k = lv + "|" + key; if (_frames.TryGetValue(k, out var f)) return f;
            var it = Item(lv, key); f = null;
            if (it != null)
            {
                var t = Resources.Load<Texture2D>(Dir + it.file);
                if (t != null)
                {
                    t.filterMode = FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; int n = Mathf.Max(1, Mathf.Min(it.frames, t.width / Mathf.Max(1, it.w))); f = new Sprite[n];
                    for (int i = 0; i < n; i++) { f[i] = Sprite.Create(t, new Rect(i * it.w, t.height - it.h, it.w, it.h), new Vector2(0, 1), HArt.PPU, 0, SpriteMeshType.FullRect); f[i].name = it.file + "_" + i; }
                }
            }
            _frames[k] = f; return f;
        }
        /// <summary>Rows y0..y1 (from the top, inclusive) and cols x0..x1 of a 1-frame tile, pivot top-left.</summary>
        public static Sprite Clip(string lv, string key, int x0, int x1, int y0, int y1)
        {
            string k = lv + "|" + key + "|" + x0 + "|" + x1 + "|" + y0 + "|" + y1; if (_clips.TryGetValue(k, out var s)) return s;
            var f = Frames(lv, key); s = null;
            if (f != null && f.Length > 0 && x1 >= x0 && y1 >= y0)
            {
                var r = f[0].rect; s = Sprite.Create(f[0].texture, new Rect(r.x + x0, r.yMax - 1 - y1, x1 - x0 + 1, y1 - y0 + 1), new Vector2(0, 1), HArt.PPU, 0, SpriteMeshType.FullRect);
            }
            _clips[k] = s; return s;
        }
        public static int FrameIdx(LwItem it, string name) { if (it == null || it.order == null) return -1; return System.Array.IndexOf(it.order, name); }
        /// <summary>Top-left room px of a door strip for side d (0 N, 1 E, 2 S, 3 W) from its json pivot. doorX/doorY = gap centre.</summary>
        public static Vector2 DoorTopLeft(LwItem it, int d, RoomData D, int doorX, int doorY)
        {
            int px = it.pivot != null && it.pivot.Length > 0 ? it.pivot[0] : it.w / 2, wall = D.wallSide;
            switch (d)
            {
                case 0: return new Vector2(doorX - px, D.wallN - it.h);
                case 2: return new Vector2(doorX - px, D.h - it.h);
                case 1: return new Vector2(D.w - wall / 2 - px, doorY - it.h / 2);
                default: return new Vector2(wall / 2 - px, doorY - it.h / 2);
            }
        }
        /// <summary>Frame for a normal door: closing 1-3 by the json ms, then locked; opening = closing frames reversed, then open.</summary>
        public static int NormalFrame(LwItem it, bool locked, float lockT, float openT)
        {
            int iOpen = Mathf.Max(0, FrameIdx(it, "open")), iLock = FrameIdx(it, "locked"); if (iLock < 0) iLock = it.frames - 1;
            int[] seq = { FrameIdx(it, "closing_1"), FrameIdx(it, "closing_2"), FrameIdx(it, "closing_3") };
            float t = locked ? lockT : openT;
            for (int s = 0; s < 3; s++)
            {
                int fi = seq[locked ? s : 2 - s]; if (fi < 0) continue;
                float ms = it.ms != null && fi < it.ms.Length ? it.ms[fi] / 1000f : 0.07f; if (t < ms) return fi; t -= ms;
            }
            return locked ? iLock : iOpen;
        }
    }
}
