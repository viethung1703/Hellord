using PixelFXPack;
using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    // t1682 (VH GO, Nov 25 00:12 Asia/Saigon): Gonggong, Breaker of Heaven's 4-slot kit, built like Leizu's (HellordLeizu1666.cs).
    //  Design: research/t1681_gonggong_kit/report.md, VH's 18 picks only. Numbers = the report's; where it was silent the simplest
    //  option, marked "(pick)" in the HellordTuning t1682 section and listed in research/t1682_gonggong_kit/progress.md.
    //  LMB basics (start pick, descendBasicsGonggong1561): Frostbolt + Frost Cleave (kept) + NEW Tidal Surge, Whirlpool,
    //    Breath of the Deep (held cone), Buzhou Rift, Eye of the Black Waters.
    //  SPACE movement (own cooldown, no damage): Ride the Flood, Rime Coat, Floodgate, Frozen River.
    //  RMB special: Drowning Mark (one demon), Hypothermia (area curse), Shatter, Toppled Pillar, Frost Wall.
    //  R ultimate (armed, then pressed): Ram of Mount Buzhou (wrath from damage taken), The Great Flood (High Tide window),
    //    Eternal Winter (25 frozen kills), Pillars of Ao (below 35% HP; disarmed above 60%).
    //  Active in a Gonggong run (pact path 4) while kit1618 + ggKit1682 are on. Shares Zhurong's run picks (ZhRun*1639), slot
    //  cooldowns (ZhMoveCd / ZhSpecCd / ZhUltCd), level cards and the kit bar. Kit skill k = GgK0 (24) + index in GgIds1682
    //  (4 movement, 5 specials, 4 ultimates; HellordKit1651.cs ZkI1651 / ZkCd1651 / ZkR1651 / ZkDur1651 / ZkHit1651).
    //  Rules (report section 6): displacement skips bosses and is halved on elites; chills-to-freeze never below 2; bosses are
    //    interrupted (stun) instead of frozen by the kit's own freezes.
    //  ICON HOOK: Resources/<ggIconDir1682>/icon_<key>_24.png (keys GgKeys1682); missing = a procedural placeholder.
    //  FX HOOK: GgFx1682("GGFX_<name>", pivot, rect px, order, time, seed, ZP...) draws Resources/<ggFxDir1682>/GGFX_<name>.mat
    //    (+ _Glow twin) through the shader-quad route when present (ggFx1682 on); otherwise the procedural PLACEHOLDERS below.
    //    Every effect and its drive values: Art/fx_gonggong_t1682/spec.md (GgFxDefs1682 lists the names).
    // =====================================================================================================================
    public partial class HellordGame
    {
        public static readonly string[] GgKeys1682 = { "ridetheflood", "rimecoat", "floodgate", "frozenriver", "drowningmark", "hypothermia", "shatter", "toppledpillar", "frostwall", "ramofmountbuzhou", "greatflood", "eternalwinter", "pillarsofao", "tidalsurge", "whirlpool", "breathofthedeep", "buzhourift", "eyeblackwaters" };
        public static readonly string[] GgMoveNames1682 = { "NONE", "RIDE THE FLOOD", "RIME COAT", "FLOODGATE", "FROZEN RIVER" };
        public static readonly string[] GgSpecNames1682 = { "NONE", "DROWNING MARK", "HYPOTHERMIA", "SHATTER", "TOPPLED PILLAR", "FROST WALL" };
        public static readonly string[] GgUltNames1682 = { "NONE", "RAM OF MOUNT BUZHOU", "THE GREAT FLOOD", "ETERNAL WINTER", "PILLARS OF AO" };
        static readonly string[] GgMoveDesc1682 = { "", "Dash becomes a short wave ride you steer with the cursor. No chill, freeze or knockback on it.", "For 3 s after a dash, the first 2 melee attackers that hit you are frozen.", "Dash leaves an ice wall across your path. It blocks demons and their bullets.", "Dash freezes the ground on its path: demons slide and slow, you run faster." };
        static readonly string[] GgSpecDesc1682 = { "", "Mark the demon near the cursor. Cold hits on it chill the demons around it.", "Curse an area: demons freeze with one fewer chill and stay frozen longer.", "Every frozen demon near the cursor explodes and chills the demons around it.", "Raise a pillar that blocks, then falls along your aim and crushes a line.", "A curved wall of ice holds demons back. Crystals your skills hit explode." };
        static readonly string[] GgUltDesc1682 = { "", "Taking damage fills wrath: charge unhurt, then shards rain from the cracked sky.", "Only at High Tide: a wave sweeps the screen southeast, hits, pushes and chills.", "Kill 25 frozen demons: 6 s of winter, and frozen deaths shatter and chain.", "Below 35% HP: four ice pillars keep demons out and fire icicles." };
        public static readonly SkillId[] GgIds1682 = { SkillId.GgRideTheFlood, SkillId.GgRimeCoat, SkillId.GgFloodgate, SkillId.GgFrozenRiver, SkillId.GgDrowningMark, SkillId.GgHypothermia, SkillId.GgShatter, SkillId.GgToppledPillar, SkillId.GgFrostWall, SkillId.GgRamOfBuzhou, SkillId.GgGreatFlood, SkillId.GgEternalWinter, SkillId.GgPillarsOfAo };
        public const int GgK0 = 24; // kit index of Ride the Flood (after Zhurong's 12 + Leizu's 12)
        public static bool IsGgBasic1682(SkillId id) => id >= SkillId.TidalSurge && id <= SkillId.EyeOfTheBlackWaters;
        // Endesga-32 water / ice
        static readonly Color GW = new Color32(0xff, 0xff, 0xff, 255), GI = new Color32(0xc0, 0xcb, 0xdc, 255), GC = new Color32(0x2c, 0xe8, 0xf5, 255), GB = new Color32(0x00, 0x99, 0xdb, 255),
            GD = new Color32(0x12, 0x4e, 0x89, 255), GN = new Color32(0x26, 0x2b, 0x44, 255), GS = new Color32(0x5a, 0x69, 0x88, 255), GP = new Color32(0x8b, 0x9b, 0xb4, 255);

        // ---------------------------------------------------------------- slots
        public bool GgOn1682 => TZ != null && TZ.kit1618 && TZ.ggKit1682 && !TZ.kitAnyPact1618 && _pactPath51 == 4;
        public int GgMove1682 => GgOn1682 ? Mathf.Clamp(ZhSlotOverride1639 ? TZ.ggMove1682 : ZhRunMove1639, 0, 4) : 0;
        public int GgSpec1682 => GgOn1682 ? Mathf.Clamp(ZhSlotOverride1639 ? TZ.ggSpecial1682 : ZhRunSpec1639, 0, 5) : 0;
        public int GgUlt1682 => GgOn1682 ? Mathf.Clamp(ZhSlotOverride1639 ? TZ.ggUlt1682 : ZhRunUlt1639, 0, 4) : 0;
        /// <summary>kit index (ZkI1651) of slot 0/1/2 pick p: 4 movement, 5 specials, 4 ultimates.</summary>
        public static int GgK1682(int slot, int pick) => GgK0 + (slot == 0 ? pick - 1 : slot == 1 ? 3 + pick : 8 + pick);

        // ---------------------------------------------------------------- state
        [System.NonSerialized] public int GgCasts1682, GgSurgeCarried1682, GgSurgeCrashes1682, GgPoolPulls1682, GgPoolCollapses1682, GgBreathTicks1682, GgBreathFreezes1682, GgRiftBursts1682, GgRiftSpikes1682, GgRiftShatters1682,
            GgEyeShards1682, GgEyeHits1682, GgRides1682, GgRimeFreezes1682, GgGateBlocks1682, GgGateBullets1682, GgGateBroken1682, GgRiverSlows1682, GgMarkRelays1682, GgMarkFreezes1682, GgCursed1682, GgHypoFreezes1682,
            GgShatters1682, GgPillarBlocks1682, GgPillarHits1682, GgCrystalBooms1682, GgWallBlocks1682, GgRamHits1682, GgRamShardHits1682, GgFloodHits1682, GgWinterChills1682, GgWinterBursts1682, GgAoPushes1682, GgAoIcicles1682, GgFrozeN1682;
        readonly List<int> _ggq1682 = new List<int>(64);
        bool GgOk1682(Enemy e) => e != null && e.alive && !e.dying;
        float GgCK1682(SkillInst s) => s != null ? StatusK(s, SupportId.Chill) : 1f; // the kit's own chills last longer with the Chill support (like hit chills)
        // movement
        [System.NonSerialized] public float GgRideT1682, GgRimeT1682; [System.NonSerialized] public int GgRimeN1682; int _ggPendMove1682; Vector2 _ggDash0_1682;
        public bool GgRiding1682 => GgRideT1682 > 0f;
        class GgWall { public Vector2 a, b, dir; public float t, life, thick; public int hp; public readonly Dictionary<int, float> hitAt = new Dictionary<int, float>(); }
        GgWall _ggGate1682;
        class GgStrip { public Vector2 a, b; public float t, life, w; }
        readonly List<GgStrip> _ggRiver1682 = new List<GgStrip>();
        // specials
        [System.NonSerialized] public Enemy GgMark1682; int _ggMarkId1682; [System.NonSerialized] public float GgMarkT1682; bool _ggRelaying1682;
        class GgCurse { public Vector2 p; public float t, r, dur; public bool done; }
        readonly List<GgCurse> _ggCurse1682 = new List<GgCurse>();
        class GgPillar { public Vector2 p, dir; public float t, delay, r; public bool fell; public int k; }
        readonly List<GgPillar> _ggPillar1682 = new List<GgPillar>();
        class GgCrystal { public Vector2 p; public bool alive = true; }
        class GgFrostWall { public readonly List<GgCrystal> c = new List<GgCrystal>(); public float t, life, r; public Vector2 mid, dir; }
        GgFrostWall _ggFw1682;
        // ultimates (meters carry over rooms; GgRunReset1682 per run)
        [System.NonSerialized] public float GgWrath1682, GgTide1682, GgWinterT1682, GgAoT1682; [System.NonSerialized] public int GgFrozenKills1682; float _ggLastHurt1682 = -99f; [System.NonSerialized] public bool GgAoArmedLatch1682;
        class GgRam { public Vector2 a, dir, p; public float t, dur, len; public readonly HashSet<int> hit = new HashSet<int>(); }
        GgRam _ggRam1682; public bool GgInvuln1682 => _ggRam1682 != null;
        class GgShard { public Vector2 p; public float delay, t; public bool done; }
        readonly List<GgShard> _ggShards1682 = new List<GgShard>();
        class GgFlood { public Vector2 o; public float t, s0, s1; public readonly HashSet<int> hit = new HashSet<int>(); }
        GgFlood _ggFlood1682; float _ggWinterNext1682;
        readonly List<Vector2> _ggBurstQ1682 = new List<Vector2>();
        Vector2 _ggAoC1682; float _ggAoNext1682;
        // basics
        class GgWave { public SkillInst s; public Vector2 p, p0, dir; public float dmg, left, w, t; public readonly HashSet<int> carry = new HashSet<int>(); }
        readonly List<GgWave> _ggWaves1682 = new List<GgWave>();
        class GgPool { public SkillInst s; public Vector2 p; public float t, dur, r, dmg; }
        readonly List<GgPool> _ggPools1682 = new List<GgPool>();
        class GgBreath { public SkillInst s; public float t, tick; public readonly Dictionary<int, float> held = new Dictionary<int, float>(); }
        GgBreath _ggBreath1682; bool _ggBreathNeedUp1682; public bool GgBreathOn1682 => _ggBreath1682 != null;
        class GgRift { public SkillInst s; public Vector2 a, b; public float t, dmg; public int done; public bool spiked; }
        readonly List<GgRift> _ggRifts1682 = new List<GgRift>();
        class GgEye { public SkillInst s; public Vector2 p, v; public float t, life, dmg, acc, ang, pulse = -9f; }
        readonly List<GgEye> _ggEyes1682 = new List<GgEye>();
        class GgShardP { public SkillInst s; public Vector2 p, v; public float t, life, dmg; }
        readonly List<GgShardP> _ggShardP1682 = new List<GgShardP>();
        class GgFxO { public Vector2 p, q; public float t, life, r, delay, t0; public int kind, ord = int.MinValue; public string mat; public Vector4 rc; public ZP[] ps; public System.Func<GgFxO, ZP[]> pf; public bool dart; }
        readonly List<GgFxO> _ggFx1682 = new List<GgFxO>();

        void ClearGg1682()
        {   // per room (ClearZh1618): effects / entities. Meters + the ultimate cooldown carry over (t1648 rule).
            GgRideT1682 = GgRimeT1682 = 0f; GgRimeN1682 = 0; _ggPendMove1682 = 0; _ggGate1682 = null; _ggRiver1682.Clear();
            GgMark1682 = null; GgMarkT1682 = 0f; _ggCurse1682.Clear(); _ggPillar1682.Clear(); _ggFw1682 = null;
            _ggRam1682 = null; _ggShards1682.Clear(); _ggFlood1682 = null; GgWinterT1682 = 0f; _ggBurstQ1682.Clear(); GgAoT1682 = 0f;
            _ggWaves1682.Clear(); _ggPools1682.Clear(); _ggBreath1682 = null; _ggBreathNeedUp1682 = false; _ggRifts1682.Clear(); _ggEyes1682.Clear(); _ggShardP1682.Clear(); _ggFx1682.Clear();
        }
        void GgRunReset1682() { GgWrath1682 = 0f; GgTide1682 = 0f; GgFrozenKills1682 = 0; _ggLastHurt1682 = -99f; GgAoArmedLatch1682 = false; }
        /// <summary>Basics' damage / cooldown from HellordTuning (ggBaseDmg1682 / ggCd1682).</summary>
        void GgDefs1682()
        {
            var T = TZ; if (T == null) return;
            foreach (var d in HData.Skills) { if (!IsGgBasic1682(d.id)) continue; int i = d.id - SkillId.TidalSurge; if (T.ggBaseDmg1682 != null && i < T.ggBaseDmg1682.Length) d.baseDmg = T.ggBaseDmg1682[i]; if (T.ggCd1682 != null && i < T.ggCd1682.Length) d.cooldown = T.ggCd1682[i]; }
        }
        /// <summary>test menu / clips: reset the kit cooldowns and meet the slotted ultimate's condition.</summary>
        public void GgFillUlt1682()
        {
            var T = TZ; ZhMoveCd = ZhSpecCd = ZhUltCd = 0f; GgWrath1682 = 1f; _ggLastHurt1682 = runTime; GgTide1682 = T.gfRise1682 + 0.01f; GgFrozenKills1682 = T.ewNeed1682;
            if (GgUlt1682 == 4) { hp = Mathf.Min(hp, maxHp * T.aoArm1682 * 0.9f); GgAoArmedLatch1682 = true; }
        }

        // ---------------------------------------------------------------- icon + FX hooks
        readonly Dictionary<string, Texture2D> _ggIcon1682 = new Dictionary<string, Texture2D>();
        /// <summary>Icon hook: Resources/&lt;ggIconDir1682&gt;/icon_&lt;key&gt;_24.png; missing = a procedural placeholder (dark cell + a tinted wave).</summary>
        public Texture2D GgIcon1682(string key)
        {
            if (key == null) return null; if (_ggIcon1682.TryGetValue(key, out var t)) return t;
            string dir = TZ != null ? TZ.ggIconDir1682 : ""; if (!string.IsNullOrEmpty(dir)) t = Resources.Load<Texture2D>(dir + "/icon_" + key + "_24");
            if (t != null) t.filterMode = FilterMode.Point; else t = GgIconPh1682(key);
            return _ggIcon1682[key] = t;
        }
        public bool GgIconIsArt1682(string key) { var t = GgIcon1682(key); return t != null && t.name != "gg_ph"; }
        static Texture2D GgIconPh1682(string key)
        {   // PLACEHOLDER: 24x24 navy cell, 1 px frame, a 2-crest wave tinted by slot (movement cyan, special ice, ultimate white, basic blue) + a slot pip
            int ix = System.Array.IndexOf(GgKeys1682, key); Color tint = ix < 4 ? GC : ix < 9 ? GI : ix < 13 ? GW : GB;
            var t = new Texture2D(24, 24, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "gg_ph" }; Color bg = GN, fr = new Color32(0x3a, 0x44, 0x66, 255);
            for (int y = 0; y < 24; y++) for (int x = 0; x < 24; x++) t.SetPixel(x, y, x == 0 || y == 0 || x == 23 || y == 23 ? fr : bg);
            float ph = ix < 0 ? 0f : ix * 0.9f;
            for (int x = 3; x < 21; x++) { int y = 11 + Mathf.RoundToInt(3f * Mathf.Sin(x * 0.55f + ph)); t.SetPixel(x, y, tint); t.SetPixel(x, y - 1, tint); t.SetPixel(x, y - 4, GD); }
            for (int i = 0; i <= (ix < 0 ? 0 : ix % 5); i++) t.SetPixel(4 + i * 2, 19, tint);
            t.Apply(); return t;
        }
        readonly Dictionary<string, Material> _ggMat1682 = new Dictionary<string, Material>(); string _ggMatKey1682; Dictionary<string, string> _ggRename1682 = new Dictionary<string, string>();
        IEnumerable<Material> GgFxMats1682() { foreach (var m in _ggMat1682.Values) if (m != null) yield return m; } // t1675 preloader warm list
        /// <summary>GGFX material by name: Resources/&lt;ggFxDir1682&gt; + renames (ggFxRename1682). null = not shipped (placeholder draws).</summary>
        Material GgFxMat1682(string n)
        {
            var T = TZ; string dir = T != null && !string.IsNullOrEmpty(T.ggFxDir1682) ? T.ggFxDir1682 : "GonggongFX/Materials/", ren = T != null ? T.ggFxRename1682 : null, key = dir + "|" + ren;
            if (key != _ggMatKey1682)
            {   // folder / rename changed (F1 tuning or an FX drop): reload everything
                _ggMatKey1682 = key; _ggMat1682.Clear(); _ggRename1682 = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(ren)) foreach (var p in ren.Split(';')) { int e = p.IndexOf('='); if (e > 0) _ggRename1682[p.Substring(0, e).Trim()] = p.Substring(e + 1).Trim(); }
            }
            if (_ggMat1682.TryGetValue(n, out var m)) return m;
            string fn = n; if (_ggRename1682.TryGetValue(n, out var rn)) fn = rn; else if (n.EndsWith("_Glow") && _ggRename1682.TryGetValue(n.Substring(0, n.Length - 5), out var rb)) fn = rb + "_Glow";
            m = Resources.Load<Material>(dir + fn); return _ggMat1682[n] = m;
        }
        /// <summary>HFX shader hook (same quad route + logging as ZhFx1625 / LzFx1666). True = the shader drew it (skip the placeholder).</summary>
        bool GgFx1682(string mat, Vector2 pivot, float l, float b, float w, float h, int order, float fxTime, float seed, params ZP[] ps)
        {
            if (ZhFxLogOn1625) { _zfxSeen1625.Add(mat); _zfxLog1625[mat + "|_FxTime"] = fxTime; if (ps != null) foreach (var p in ps) _zfxLog1625[mat + "|" + p.n] = p.v.x; }
            if (TZ == null || !TZ.ggFx1682) return false; var m = GgFxMat1682(mat); if (m == null) return false;
            ZhFxQuad1625(m, pivot, l, b, w, h, order, fxTime, seed, ps); var g = GgFxMat1682(mat + "_Glow"); if (g != null) ZhFxQuad1625(g, pivot, l, b, w, h, order + 1, fxTime, seed, ps);
            return true;
        }
        bool GgFxLive1682(string mat) => TZ != null && TZ.ggFx1682 && GgFxMat1682(mat) != null;
        /// <summary>every GGFX effect the kit calls (name, what drives it): the spec / preload list (Art/fx_gonggong_t1682/spec.md).</summary>
        public static readonly string[] GgFxDefs1682 = { "GGFX_TidalSurge", "GGFX_TidalSurge_Crash", "GGFX_Whirlpool", "GGFX_Whirlpool_Collapse", "GGFX_BreathOfTheDeep", "GGFX_BuzhouRift_Burst", "GGFX_BuzhouRift_Spike", "GGFX_EyeBlackWaters", "GGFX_EyeShard",
            "GGFX_RideTheFlood", "GGFX_RimeCoat", "GGFX_Floodgate", "GGFX_FrozenRiver", "GGFX_DrowningMark", "GGFX_Hypothermia", "GGFX_Hypothermia_Cursed", "GGFX_Shatter_Burst", "GGFX_ToppledPillar", "GGFX_ToppledPillar_Crush",
            "GGFX_FrostWall", "GGFX_FrostWall_Pop", "GGFX_Ram_Charge", "GGFX_Ram_Shard", "GGFX_GreatFlood", "GGFX_TideGauge", "GGFX_EternalWinter", "GGFX_EternalWinter_Shatter", "GGFX_PillarsOfAo", "GGFX_Icicle",
            /* t1682 FX pass extras */ "GGFX_TidalSurge_Crest", "GGFX_BreathOfTheDeep_Ground", "GGFX_BuzhouRift_Consume", "GGFX_EyeShard_Hit", "GGFX_Ram_Head", "GGFX_EternalWinter_Pulse", "GGFX_Icicle_Hit" };
        /// <summary>one-shot effect: kind 0 expanding ring, 1 flash disc, 2 shatter star, 3 shrinking ring, 4 line flash p -> q; mat = its shader (rect px l,b,w,h).</summary>
        ///   kind -1 = shader only (no placeholder). Returned so a caller can set: ps (fixed params), pf (params from the live state), ord (sort order; default over = YSort + 60),
        ///   delay (hidden until then), t0 (fxTime offset), dart (pivot flies p -> q, _Dir passed).
        GgFxO GgFxO1682(int kind, Vector2 p, float r, float life, string mat = null, Vector4 rc = default, Vector2 q = default) { var o = new GgFxO { kind = kind, p = p, q = q, r = r, life = life, mat = mat, rc = rc }; _ggFx1682.Add(o); return o; }
        readonly List<ZP> _ggPs1682 = new List<ZP>();
        // t1682 FX pass: HFX rect helpers (Art/fx_gonggong_t1682/spec.md + _gen/preview_gg.py)
        static Vector4 GgSegRc1682(Vector2 tpx, float pad) { float tx = Mathf.Round(tpx.x), ty = Mathf.Round(tpx.y); return new Vector4(Mathf.Min(0f, tx) - pad, Mathf.Min(0f, ty) - pad, Mathf.Abs(tx) + 2f * pad, Mathf.Abs(ty) + 2f * pad); }
        static Vector4 GgRotRc1682(Vector4 rc, float deg)
        {   // AABB of the rect rotated by deg about the pivot
            float c = Mathf.Cos(deg * Mathf.Deg2Rad), s = Mathf.Sin(deg * Mathf.Deg2Rad), x0 = 1e9f, y0 = 1e9f, x1 = -1e9f, y1 = -1e9f;
            for (int i = 0; i < 4; i++) { float x = i % 2 == 0 ? rc.x : rc.x + rc.z, y = i < 2 ? rc.y : rc.y + rc.w, rx = x * c - y * s, ry = x * s + y * c; x0 = Mathf.Min(x0, rx); y0 = Mathf.Min(y0, ry); x1 = Mathf.Max(x1, rx); y1 = Mathf.Max(y1, ry); }
            return new Vector4(Mathf.Floor(x0), Mathf.Floor(y0), Mathf.Ceil(x1 - x0) + 1f, Mathf.Ceil(y1 - y0) + 1f);
        }
        static Vector4 GgIceWallRc1682(float deg, float L = 48f, float B = 0f, float S = 14f)
        {   // r_ice mode 1 (Floodgate)
            float dx = Mathf.Cos(deg * Mathf.Deg2Rad), dy = Mathf.Sin(deg * Mathf.Deg2Rad); int ex = (int)(Mathf.Abs(dy) * L / 2f + Mathf.Abs(dx) * B + 18f), ey = (int)(Mathf.Abs(dx) * L / 2f + Mathf.Abs(dy) * B + 4f);
            return new Vector4(-ex - 6, -ey - 10, 2 * ex + 12, 2 * ey + (int)(1.5f * S) + 24);
        }
        static Vector4 GgIcePillarRc1682(float deg, float H = 64f, float S = 16f)
        {   // r_ice mode 2 (Toppled Pillar)
            float fx = Mathf.Cos(deg * Mathf.Deg2Rad) * H, fy = Mathf.Sin(deg * Mathf.Deg2Rad) * H * 0.62f, x0 = Mathf.Min(-S - 10f, fx - 16f), x1 = Mathf.Max(S + 10f, fx + 16f), y0 = Mathf.Min(-14f, fy - 14f), y1 = Mathf.Max(H + 10f, fy + 18f);
            return new Vector4((int)x0, (int)y0, (int)(x1 - x0), (int)(y1 - y0));
        }
        static Vector4 GgConeRc1682(float deg, float R = 64f, float arc = 40f, float pad = 10f)
        {   // cone_rect: AABB of the cone at 1.2 R, padded, +6 down
            float x0 = 0f, y0 = 0f, x1 = 0f, y1 = 0f; foreach (float s in new[] { -arc / 2f - 4f, 0f, arc / 2f + 4f }) { float a = (deg + s) * Mathf.Deg2Rad, x = Mathf.Cos(a) * R * 1.2f, y = Mathf.Sin(a) * R * 1.2f; x0 = Mathf.Min(x0, x); x1 = Mathf.Max(x1, x); y0 = Mathf.Min(y0, y); y1 = Mathf.Max(y1, y); }
            return new Vector4((int)x0 - pad, (int)y0 - pad - 6f, (int)(x1 - x0) + 2f * pad, (int)(y1 - y0) + 2f * pad + 6f);
        }
        static float GgDeg1682(Vector2 d) => Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        int GgGround1682(Vector2 p) => FbGround + YSort(p.y) + 40; // HFX "ground" tier: under every actor
        static Vector4 GgZoneRc1682(float rU) { float R = Mathf.Round(rU * 16f); float h = Mathf.Ceil(R * 0.55f); return new Vector4(-(R + 8), -(h + 8), 2 * R + 16, 2 * h + 16); }
        void DrawGgFx1682(float dt)
        {
            for (int i = _ggFx1682.Count - 1; i >= 0; i--)
            {
                var f = _ggFx1682[i]; f.t += dt; if (f.t < f.delay) continue; float lt = f.t - f.delay; if (lt >= f.life) { _ggFx1682.RemoveAt(i); continue; } float k = lt / f.life; int ord = FbGround + YSort(f.p.y) + 40;
                Vector2 pv = f.dart ? Vector2.Lerp(f.p, f.q, k) : f.p;
                if (f.mat != null)
                {
                    _ggPs1682.Clear(); _ggPs1682.Add(ZP.F("_Progress", k)); if (f.kind == 4 && !f.dart) _ggPs1682.Add(ZP.V("_TargetPx", (f.q - f.p) * 16f)); if (f.dart) _ggPs1682.Add(ZP.V("_Dir", (f.q - f.p).sqrMagnitude > 1e-6f ? (f.q - f.p).normalized : Vector2.right));
                    if (f.ps != null) _ggPs1682.AddRange(f.ps); if (f.pf != null) _ggPs1682.AddRange(f.pf(f));
                    if (GgFx1682(f.mat, pv, f.rc.x, f.rc.y, f.rc.z, f.rc.w, f.ord != int.MinValue ? f.ord : YSort(pv.y) + 60, lt + f.t0, 0.37f, _ggPs1682.ToArray())) continue;
                }
                if (f.dart) { ZSegO1618(pv - (f.q - f.p).normalized * 0.3f, pv, 0.08f, GW, YSort(pv.y) + 200); continue; }
                switch (f.kind)
                {
                    case 0: ZRingO1618(f.p, f.r * (0.3f + 0.7f * k), new Color(GC.r, GC.g, GC.b, 1f - k), ord, 0.6f); ZRingO1618(f.p, f.r * (0.2f + 0.6f * k), new Color(GW.r, GW.g, GW.b, (1f - k) * 0.7f), ord + 1, 0.6f); break;
                    case 1: ZDiscO1618(f.p, f.r * (0.5f + 0.5f * k), new Color(GB.r, GB.g, GB.b, (1f - k) * 0.55f), ord, 0.6f); break;
                    case 2: { int o = YSort(f.p.y) + 300; float a = 1f - k; for (int q = 0; q < 6; q++) { float an = (q * 60f + 15f) * Mathf.Deg2Rad; Vector2 d = new Vector2(Mathf.Cos(an), Mathf.Sin(an) * 0.6f); ZSegO1618(f.p, f.p + d * f.r * (0.3f + 0.7f * k), 0.1f, new Color(GI.r, GI.g, GI.b, a), o); } ZDiscO1618(f.p, f.r * 0.35f * (1f - k) + 0.08f, new Color(1, 1, 1, a), o + 1); break; }
                    case 3: ZRingO1618(f.p, f.r * (1f - k), new Color(GB.r, GB.g, GB.b, 0.5f + 0.5f * k), ord, 0.6f); break;
                    case 4: ZSegO1618(f.p, f.q, 0.1f * (1f - k) + 0.04f, new Color(GW.r, GW.g, GW.b, 1f - k), YSort(Mathf.Min(f.p.y, f.q.y)) + 200); break;
                }
            }
        }

        // ---------------------------------------------------------------- update (inside UpdateZh1618's FX begin / end)
        void UpdateGg1682(float dt)
        {
            TickGgBasics1682(dt);
            if (GgOn1682)
            {
                ZhMoveCd = Mathf.Max(0f, ZhMoveCd - dt); ZhSpecCd = Mathf.Max(0f, ZhSpecCd - dt); ZhUltCd = Mathf.Max(0f, ZhUltCd - dt);
                bool free = mode == Mode.Play && !_tmOpen && _pickCool716 <= 0f;
                if (free && HInput.SpecialDown1618()) GgSpecial1682();
                if (free && HInput.UltDown1618()) GgUltimate1682();
                TickGgMove1682(dt); TickGgSpec1682(dt); TickGgUlt1682(dt);
            }
            DrawGgFx1682(dt);
        }
        /// <summary>hero move speed x: Breath of the Deep held (slower), on a Frozen River strip (faster).</summary>
        float GgMoveMul1682() { float m = 1f; if (_ggBreath1682 != null) m *= TZ.brMove1682; if (GgOn1682 && GgOnRiver1682(_heroPos, 0f)) m *= 1f + TZ.frHero1682; return m; }
        bool GgOnRiver1682(Vector2 p, float pad) { foreach (var r in _ggRiver1682) if (PbSegDist(p, r.a, r.b) <= r.w * 0.5f + pad) return true; return false; }
        /// <summary>enemy knock friction x (UpdateEnemies): 1, or frSlide1682 on a Frozen River strip (knocked demons slide further).</summary>
        float GgFriction1682(Enemy e) => _ggRiver1682.Count > 0 && GgOnRiver1682(e.pos, e.radius) ? TZ.frSlide1682 : 1f;

        // ---------------------------------------------------------------- shared rules
        /// <summary>Displacement rule (report section 6): bosses are not moved, elites take half.</summary>
        static Vector2 GgForce1682(Enemy e, Vector2 d) => e.boss ? Vector2.zero : e.elite ? d * 0.5f : d;
        void GgDisplace1682(Enemy e, Vector2 d) { if (e.boss) return; e.pos += GgForce1682(e, d); ClampToRoom(ref e.pos, 6); }
        /// <summary>The kit's own freeze: bosses are interrupted (stun) instead.</summary>
        void GgFreeze1682(Enemy e, float secs)
        {
            if (!GgOk1682(e)) return; if (e.boss) { e.stun53 = Mathf.Max(e.stun53, TZ.ggBossStun1682); return; }
            IceFreeze(e, secs); e.chillHits = 0;
        }
        /// <summary>ApplyStatus Chill: chills needed to freeze. Hypothermia -1 on cursed non-bosses; never below 2 (report section 6).</summary>
        int GgChillNeed1682(Enemy e, int n) { if (GgOn1682 && !e.boss && e.ggHypo1682 > runTime) n--; return Mathf.Max(2, n); }
        /// <summary>freeze duration x: Hypothermia (+30%) and Drowning Mark (+50%) stack; never on bosses.</summary>
        float GgFreezeMul1682(Enemy e)
        {
            if (!GgOn1682 || e == null || e.boss) return 1f; float m = 1f;
            if (e.ggHypo1682 > runTime) m *= 1f + TZ.hyFreeze1682;
            if (GgMark1682 == e && GgMarkT1682 > 0f) m *= 1f + TZ.dmFreeze1682;
            return m;
        }
        void GgFroze1682(Enemy e)
        {
            if (!GgOn1682 || e == null) return; GgFrozeN1682++;
            if (GgMark1682 == e && GgMarkT1682 > 0f) GgMarkFreezes1682++; if (e.ggHypo1682 > runTime) GgHypoFreezes1682++;
        }
        static bool GgCold1682(SkillInst s) => s != null && s.def != null && (s.def.tags & STag.Cold) != 0;
        bool GgInLine1682(Enemy e, Vector2 a, Vector2 b, float halfW) => PbSegDist(e.pos, a, b) <= halfW + e.radius;

        // ---------------------------------------------------------------- t1675 loading phase: Gonggong runs (pact path 4)
        void PlGonggong1682(HellordPreload.Ctx c)
        {
            if (c.pact != 4) return; var T = TZ;
            c.Step("GGFX materials", () =>
            {
                string dir = T != null && !string.IsNullOrEmpty(T.ggFxDir1682) ? T.ggFxDir1682 : "GonggongFX/Materials/";
                foreach (var m in Resources.LoadAll<Material>(dir.TrimEnd('/'))) { var got = GgFxMat1682(m.name); if (got != null) c.Warm(got); }
                foreach (var n in GgFxDefs1682) { var m = GgFxMat1682(n); if (m != null) c.Warm(m); var g = GgFxMat1682(n + "_Glow"); if (g != null) c.Warm(g); } // also caches the misses: no Resources lookup at first cast
            });
            c.Step("kit icons", () => { foreach (var k in GgKeys1682) GgIcon1682(k); });
        }

        // =============================================================== BASICS (LMB; Cast switch in HellordSkills.cs)
        bool GgBasicHeld1682 => HInput.LmbHeld716() && LmbFree1517();
        void TickGgBasics1682(float dt) { TickGgWaves1682(dt); TickGgPools1682(dt); TickGgBreath1682(dt); TickGgRifts1682(dt); TickGgEyes1682(dt); }
        // ---- Tidal Surge: a wide wave rolls toward the aim, CARRIES the demons in front of it, then crashes (x1.2, innate chill)
        bool CastTidalSurge1682(SkillInst s, float dmg)
        {
            var T = TZ; Vector2 dir = PbAim1567();
            _ggWaves1682.Add(new GgWave { s = s, p = _heroPos + dir * 0.5f, p0 = _heroPos + dir * 0.5f, dir = dir, dmg = dmg, left = T.tsDist1682, w = s.R(T.tsW1682) }); HellordAudio.Play("frost_cleave", 0.75f); return true;
        }
        void TickGgWaves1682(float dt)
        {
            var T = TZ;
            for (int i = _ggWaves1682.Count - 1; i >= 0; i--)
            {
                var w = _ggWaves1682[i]; float step = Mathf.Min(T.tsSpeed1682 * dt, w.left); Vector2 d = w.dir * step, n = new Vector2(-w.dir.y, w.dir.x); w.left -= step;
                Vector2 np = w.p + d, cl = np; ClampToRoom(ref cl, 6); bool wall = (cl - np).sqrMagnitude > 0.0004f; w.p = cl;
                Query(w.p, w.w * 0.5f + 1f, _ggq1682);
                foreach (int ei in _ggq1682)
                {   // the wave front band (0.9 u deep): demons in it ride along (bosses stay, elites half: GgDisplace1682)
                    var e = _en[ei]; if (!GgOk1682(e)) continue; Vector2 rel = e.pos - w.p; float al = Vector2.Dot(rel, w.dir), ac = Mathf.Abs(Vector2.Dot(rel, n));
                    if (al < -0.9f || al > 0.35f + e.radius || ac > w.w * 0.5f + e.radius) continue;
                    if (w.carry.Add(ei)) GgSurgeCarried1682++; GgDisplace1682(e, d);
                }
                w.t += dt; int ord = YSort(w.p.y) + 40; Vector2 a = w.p - n * w.w * 0.5f, b = w.p + n * w.w * 0.5f; float prog = 1f - w.left / Mathf.Max(0.01f, T.tsDist1682), ang = GgDeg1682(w.dir);
                // HFX: pivot = the cast start (the front moves inside the shader), _Angle, _Progress 0.8 x travelled while rolling (0.8 -> 1 = the crash, below)
                Vector4 wrc = GgRotRc1682(new Vector4(-34, -34, 177, 83), ang), crc = GgRotRc1682(new Vector4(-34, -34, 177, 101), ang);
                if (GgFx1682("GGFX_TidalSurge_Crest", w.p0, crc.x, crc.y, crc.z, crc.w, YSort(w.p0.y) + 60, w.t, 0.3f, ZP.F("_Angle", ang), ZP.F("_Progress", 0.8f * prog))) { }
                if (!GgFx1682("GGFX_TidalSurge", w.p0, wrc.x, wrc.y, wrc.z, wrc.w, GgGround1682(w.p0), w.t, 0.3f, ZP.F("_Angle", ang), ZP.F("_Progress", 0.8f * prog)))
                {   // PLACEHOLDER: a blue wall of water with a white foam crest and a pale wake
                    ZSegO1618(a - w.dir * 0.35f, b - w.dir * 0.35f, 0.42f, new Color(GB.r, GB.g, GB.b, 0.75f), ord); ZSegO1618(a, b, 0.18f, new Color(GW.r, GW.g, GW.b, 0.9f), ord + 1);
                    ZSegO1618(a - w.dir * 0.8f, b - w.dir * 0.8f, 0.14f, new Color(GC.r, GC.g, GC.b, 0.35f), ord - 1);
                    for (int q = 0; q < 4; q++) { float f = Mathf.Repeat(q * 0.27f + runTime * 3f, 1f); ZPx1618(Vector2.Lerp(a, b, f) + w.dir * 0.12f, 1, GW, ord + 2); }
                }
                if (w.left > 0.001f && !wall) continue;
                // crash: everything near the front + everything it carried
                float r = w.s.R(T.tsCrashR1682); SkillArea53(w.p, r, w.dmg * T.tsCrash1682, (int)w.s.def.id); Query(w.p, r + 1f, _ggq1682);
                var hitL = new HashSet<int>(); foreach (int ei in _ggq1682) if (GgOk1682(_en[ei]) && (_en[ei].pos - w.p).magnitude <= r + _en[ei].radius) hitL.Add(ei); foreach (int ei in w.carry) if (GgOk1682(_en[ei])) hitL.Add(ei);
                foreach (int ei in hitL) Hit(_en[ei], w.dmg * T.tsCrash1682, w.s, w.dir * 0.6f);
                { var aP = new[] { ZP.F("_Angle", ang) };
                  var wb = GgFxO1682(-1, w.p0, 0f, 0.475f, "GGFX_TidalSurge", wrc); wb.ps = aP; wb.ord = GgGround1682(w.p0); wb.t0 = w.t; wb.pf = o => new[] { ZP.F("_Progress", 0.8f + 0.2f * Mathf.Min(1f, (o.t - o.delay) / 0.125f)), ZP.F("_Fade", Mathf.Clamp01((o.t - o.delay - 0.125f) / 0.35f)) };
                  var wc = GgFxO1682(-1, w.p0, 0f, 0.125f, "GGFX_TidalSurge_Crest", crc); wc.ps = aP; wc.t0 = w.t; wc.pf = o => new[] { ZP.F("_Progress", 0.8f + 0.2f * Mathf.Min(1f, (o.t - o.delay) / 0.125f)) }; }
                GgSurgeCrashes1682++; GgFxO1682(0, w.p, r, 0.45f, "GGFX_TidalSurge_Crash", new Vector4(-36, -22, 72, 73)); GgFxO1682(1, w.p, r * 0.8f, 0.25f); HellordAudio.Play("frost_cleave", 0.55f); ScreenShake.Add(1f, 0.08f);
                _ggWaves1682.RemoveAt(i);
            }
        }
        // ---- Whirlpool: a vortex at the cursor pulls demons inward for 1 s, then collapses (x1.4, innate chill). At most 2 (a 3rd collapses the oldest now)
        bool CastWhirlpool1682(SkillInst s, float dmg)
        {
            var T = TZ; if (_ggPools1682.Count >= Mathf.Max(1, T.wpMax1682)) { GgPoolCollapse1682(_ggPools1682[0]); _ggPools1682.RemoveAt(0); }
            Vector2 at = PbCursor1567(T.wpRange1682); _ggPools1682.Add(new GgPool { s = s, p = at, dur = s.Dur(T.wpDur1682), r = s.R(T.wpR1682), dmg = dmg }); HellordAudio.Play("frost_bomb_pulse", 0.8f); return true;
        }
        void GgPoolCollapse1682(GgPool w)
        {
            var T = TZ; SkillArea53(w.p, w.r, w.dmg * T.wpDmg1682, (int)w.s.def.id); Query(w.p, w.r, _ggq1682);
            foreach (int ei in _ggq1682.ToArray()) { var e = _en[ei]; if (!GgOk1682(e) || (e.pos - w.p).magnitude > w.r + e.radius) continue; Hit(e, w.dmg * T.wpDmg1682, w.s, Vector2.zero); }
            GgPoolCollapses1682++; GgFxO1682(3, w.p, w.r, 0.55f, "GGFX_Whirlpool_Collapse", new Vector4(-30, -18, 60, 68)); GgFxO1682(1, w.p, 0.6f, 0.2f); HellordAudio.Play("frost_cleave", 0.9f);
        }
        void TickGgPools1682(float dt)
        {
            var T = TZ;
            for (int i = _ggPools1682.Count - 1; i >= 0; i--)
            {
                var w = _ggPools1682[i]; w.t += dt; Query(w.p, w.r, _ggq1682);
                foreach (int ei in _ggq1682)
                {   // pull toward the centre (bosses stay, elites half); never past it
                    var e = _en[ei]; if (!GgOk1682(e)) continue; Vector2 to = w.p - e.pos; float m = to.magnitude; if (m > w.r + e.radius || m < 0.05f) continue;
                    GgDisplace1682(e, to / m * Mathf.Min(m, T.wpPull1682 * dt)); GgPoolPulls1682++;
                }
                float k = Mathf.Clamp01(w.t / Mathf.Max(0.01f, w.dur)); int ord = FbGround + YSort(w.p.y) + 40;
                float wk = w.r * 16f / 40f; // HFX rect (-54,-34,108,68) is for R 40 px; scaled with the (Area) radius
                if (!GgFx1682("GGFX_Whirlpool", w.p, Mathf.Floor(-54f * wk), Mathf.Floor(-34f * wk), Mathf.Ceil(108f * wk), Mathf.Ceil(68f * wk), ord, w.t, 0.5f, ZP.F("_Progress", k), ZP.F("_Radius", w.r * 16f)))
                {   // PLACEHOLDER: a dark disc, a tightening ring and 3 spinning arms
                    ZDiscO1618(w.p, w.r * (1f - 0.25f * k), new Color(GD.r, GD.g, GD.b, 0.35f), ord, 0.55f); ZRingO1618(w.p, w.r * (1f - 0.4f * k), new Color(GC.r, GC.g, GC.b, 0.8f), ord + 1, 0.55f);
                    for (int q = 0; q < 3; q++) { float an = runTime * 9f + q * 2.094f; Vector2 u = new Vector2(Mathf.Cos(an), Mathf.Sin(an) * 0.55f), v2 = new Vector2(Mathf.Cos(an + 0.9f), Mathf.Sin(an + 0.9f) * 0.55f); ZSegO1618(w.p + u * w.r * 0.85f, w.p + v2 * w.r * 0.35f, 0.08f, new Color(GW.r, GW.g, GW.b, 0.7f), ord + 2); }
                }
                if (w.t < w.dur) continue; GgPoolCollapse1682(w); _ggPools1682.RemoveAt(i);
            }
        }
        // ---- Breath of the Deep: HOLD LMB, a frost cone (4 u, 40 deg). Ticks every 0.2 s keep the chill on; a demon held in it 1 s freezes. You move 50% slower.
        bool CastBreath1682(SkillInst s, float dmg)
        {
            if (_echo738) { GgBreathPulse1682(s, dmg * TZ.brDmg1682); return true; } // Echo (t1682 choice): one extra cone pulse at the echo's damage, held or not
            if (_ggBreath1682 != null || _ggBreathNeedUp1682) { s.cd = 0.05f; return false; }
            _ggBreath1682 = new GgBreath { s = s, tick = 0f }; HellordAudio.Play("frost_cleave", 1.4f); return true;
        }
        readonly List<int> _ggCone1682 = new List<int>();
        void GgBreathPulse1682(SkillInst s, float d)
        {
            var T = TZ; float R = s.R(T.brR1682), half = T.brArc1682 * 0.5f; Vector2 dir = PbAim1567(), o = _heroPos; var hit = new List<int>();
            Query(o + dir * R * 0.5f, R * 0.5f + 1f, hit); SkillArea53(o + dir * R * 0.5f, R * 0.45f, d, (int)s.def.id);
            foreach (int ei in hit) { var e = _en[ei]; if (!GgOk1682(e) || !PbInArc1567(e, o, dir, R, half, 0.3f)) continue; Hit(e, d, s, Vector2.zero); e.chillT = Mathf.Max(e.chillT, 0.6f); }
            GgBreathTicks1682++; _lastCast[(int)s.def.id] = Time.time;
        }
        void TickGgBreath1682(float dt)
        {
            if (_ggBreathNeedUp1682 && !HInput.LmbHeld716()) _ggBreathNeedUp1682 = false;
            var b = _ggBreath1682; if (b == null) return; var T = TZ; var s = b.s;
            bool still = _basic1517 == s && s.def.id == SkillId.BreathOfTheDeep; float maxHold = s.Dur(T.brMax1682); // Duration supports hold longer
            if (!still || !GgBasicHeld1682 || b.t >= maxHold) { if (b.t >= maxHold) _ggBreathNeedUp1682 = true; _ggBreath1682 = null; s.cd = s.def.cooldown * s.CdMul; return; }
            b.t += dt; s.cd = 99f; b.tick -= dt; // blocked while held; the real cooldown starts on release
            float R = s.R(T.brR1682), half = T.brArc1682 * 0.5f; Vector2 dir = PbAim1567(), o = _heroPos;
            _ggCone1682.Clear(); Query(o + dir * R * 0.5f, R * 0.5f + 1f, _ggq1682);
            foreach (int ei in _ggq1682) { var e = _en[ei]; if (GgOk1682(e) && PbInArc1567(e, o, dir, R, half, 0.3f)) _ggCone1682.Add(ei); }
            var gone = new List<int>(); foreach (var kv in b.held) if (!_ggCone1682.Contains(kv.Key)) gone.Add(kv.Key); foreach (int g in gone) b.held.Remove(g); // left the cone: the hold timer restarts
            foreach (int ei in _ggCone1682)
            {
                var e = _en[ei]; b.held.TryGetValue(ei, out float h); h += dt;
                if (h >= T.brFreezeAt1682 && e.freezeT <= 0f) { GgFreeze1682(e, T.brFreezeS1682); GgBreathFreezes1682++; h = 0f; }
                b.held[ei] = h;
            }
            if (b.tick <= 0f)
            {
                b.tick = T.brTick1682; float d = SkillDamage(s) * T.brDmg1682; SkillArea53(o + dir * R * 0.5f, R * 0.45f, d, (int)s.def.id);
                foreach (int ei in _ggCone1682) { var e = _en[ei]; if (!GgOk1682(e)) continue; Hit(e, d, s, Vector2.zero); e.chillT = Mathf.Max(e.chillT, 0.6f); if (!StatusFX.IsAlive(e.chillH)) e.chillH = HFX.ChillStatus(e); }
                GgBreathTicks1682++; _lastCast[(int)s.def.id] = Time.time;
            }
            int ord = YSort(_heroPos.y) + 80; float fl = 0.85f + 0.15f * Mathf.Sin(runTime * 31f);
            float bAng = GgDeg1682(dir); Vector4 brc = GgConeRc1682(bAng); var bPs = new[] { ZP.F("_Angle", bAng), ZP.F("_Hold", b.t), ZP.F("_Intensity", Mathf.Clamp01(b.t / 0.15f)) }; // HFX: _Hold in seconds held
            GgFx1682("GGFX_BreathOfTheDeep_Ground", _heroPos, brc.x, brc.y, brc.z, brc.w, GgGround1682(_heroPos), b.t, 0.3f, bPs);
            if (!GgFx1682("GGFX_BreathOfTheDeep", _heroPos + new Vector2(7f, 16f) / 16f, brc.x, brc.y, brc.z, brc.w, YSort(_heroPos.y) + 60, b.t, 0.4f, bPs))
            {   // PLACEHOLDER: 5 frosty rays fanning out of the hand + drifting flakes
                Vector2 c = HeroCenter; for (int q = 0; q < 5; q++) { float an = (q - 2) * half * 0.5f; Vector2 d2 = PbRot(dir, an); ZSegO1618(c + d2 * 0.3f, c + d2 * R * fl, 0.12f - 0.02f * Mathf.Abs(q - 2), new Color(GI.r, GI.g, GI.b, 0.45f - 0.07f * Mathf.Abs(q - 2)), ord); }
                for (int q = 0; q < 6; q++) { float f = Mathf.Repeat(q * 0.17f + runTime * 2.2f, 1f); Vector2 d2 = PbRot(dir, (Mathf.PerlinNoise(q, runTime) - 0.5f) * half * 2f); ZPx1618(c + d2 * R * f, 1, GW, ord + 1); }
            }
        }
        // ---- Buzhou Rift: 6 ice bursts travel to the cursor (0.6 u, x0.5), then a big spike (1.2 u, x1.8). The spike CONSUMES Freeze for x3.
        bool CastBuzhouRift1682(SkillInst s, float dmg)
        {
            var T = TZ; Vector2 at = PbCursor1567(T.rfLen1682, 1f); _ggRifts1682.Add(new GgRift { s = s, a = _heroPos, b = at, dmg = dmg }); HellordAudio.Play("frost_cleave", 1.1f); return true;
        }
        void TickGgRifts1682(float dt)
        {
            var T = TZ; int N = Mathf.Max(1, T.rfBursts1682);
            for (int i = _ggRifts1682.Count - 1; i >= 0; i--)
            {
                var r = _ggRifts1682[i]; r.t += dt; int want = Mathf.Min(N, Mathf.FloorToInt(r.t / Mathf.Max(0.01f, T.rfStep1682)) + 1);
                while (r.done < want)
                {
                    int q = r.done++; Vector2 p = Vector2.Lerp(r.a, r.b, (q + 1f) / (N + 1f)); float R = r.s.R(T.rfBurstR1682); SkillArea53(p, R, r.dmg * T.rfBurst1682, (int)r.s.def.id); Query(p, R, _ggq1682);
                    foreach (int ei in _ggq1682.ToArray()) { var e = _en[ei]; if (!GgOk1682(e) || (e.pos - p).magnitude > R + e.radius) continue; Hit(e, r.dmg * T.rfBurst1682, r.s, Vector2.zero); }
                    GgRiftBursts1682++; GgFxO1682(2, p, R, 0.3f, "GGFX_BuzhouRift_Burst", new Vector4(-24, -20, 48, 54)).ord = YSort(p.y); HellordAudio.Play("frost_cleave", 1.5f + 0.05f * q);
                }
                if (!r.spiked && r.t >= (N + 1) * T.rfStep1682)
                {
                    r.spiked = true; float R = r.s.R(T.rfSpikeR1682); SkillArea53(r.b, R, r.dmg * T.rfSpike1682, (int)r.s.def.id); Query(r.b, R, _ggq1682);
                    foreach (int ei in _ggq1682.ToArray())
                    {
                        var e = _en[ei]; if (!GgOk1682(e) || (e.pos - r.b).magnitude > R + e.radius) continue; bool fr = e.freezeT > 0f;
                        if (fr) { e.freezeT = 0f; e.chillHits = 0; if (StatusFX.IsAlive(e.chillH)) StatusFX.SetFrozen(e.chillH, false); GgRiftShatters1682++; if (GgFxLive1682("GGFX_BuzhouRift_Consume")) GgFxO1682(-1, e.pos + new Vector2(0f, 4f / 16f), 0.7f, 0.45f, "GGFX_BuzhouRift_Consume", new Vector4(-44, -13, 88, 60)); else GgFxO1682(2, BodyCentre1595(e), 0.7f, 0.3f); } // the freeze is consumed
                        Hit(e, r.dmg * T.rfSpike1682 * (fr ? T.rfFrozen1682 : 1f), r.s, (e.pos - r.b).normalized * 1f);
                    }
                    GgRiftSpikes1682++; GgFxO1682(2, r.b, R * 1.3f, 0.45f, "GGFX_BuzhouRift_Spike", new Vector4(-33, -29, 66, 81)).ord = YSort(r.b.y); GgFxO1682(0, r.b, R, 0.3f); HellordAudio.Play("ice_shatter", 1f); ScreenShake.Add(1.2f, 0.1f);
                }
                // PLACEHOLDER crack: a jagged dark-blue line up to the newest burst, fading after the spike
                float fade = r.spiked ? Mathf.Clamp01(1f - (r.t - (N + 1) * T.rfStep1682) / 0.4f) : 1f; Vector2 tip = Vector2.Lerp(r.a, r.b, Mathf.Min(1f, r.done / (N + 1f))); Vector2 prev = r.a; int ordc = FbGround + YSort(r.a.y) + 30;
                for (int q = 1; q <= 6; q++) { Vector2 pt = Vector2.Lerp(r.a, tip, q / 6f) + (q < 6 ? PbRot((r.b - r.a).normalized, 90f) * ((q % 2 == 0 ? 1f : -1f) * 0.12f) : Vector2.zero); ZSegO1618(prev, pt, 0.07f, new Color(GD.r, GD.g, GD.b, 0.8f * fade), ordc); ZSegO1618(prev, pt, 0.03f, new Color(GC.r, GC.g, GC.b, 0.7f * fade), ordc + 1); prev = pt; }
                if (r.spiked && fade <= 0f) _ggRifts1682.RemoveAt(i);
            }
        }
        // ---- Eye of the Black Waters: a slow orb that never hits, spraying ice shards in a spiral (8/s, x0.3) as it drifts past the cursor (2 s). At most 2. Multi = more shards per spray.
        bool CastEye1682(SkillInst s, float dmg)
        {
            var T = TZ; if (_ggEyes1682.Count >= Mathf.Max(1, T.eyeMax1682)) _ggEyes1682.RemoveAt(0); Vector2 dir = PbAim1567();
            _ggEyes1682.Add(new GgEye { s = s, p = _heroPos + dir * 0.6f, v = dir * T.eyeSpeed1682, life = s.Dur(T.eyeLife1682), dmg = dmg, ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg }); HellordAudio.Play("frost_bomb_pulse", 1.3f); return true;
        }
        void TickGgEyes1682(float dt)
        {
            var T = TZ; float every = 1f / Mathf.Max(0.5f, T.eyeRate1682);
            for (int i = _ggEyes1682.Count - 1; i >= 0; i--)
            {
                var o = _ggEyes1682[i]; o.t += dt; o.p += o.v * dt; Vector2 cl = o.p; ClampToRoom(ref cl, 6); if ((cl - o.p).sqrMagnitude > 0.0004f) { o.p = cl; o.v = Vector2.zero; } o.acc += dt;
                while (o.acc >= every)
                {
                    o.acc -= every; int n = ProjCount1651(o.s, 1);
                    for (int q = 0; q < n; q++) { float a = (o.ang + q * 360f / n) * Mathf.Deg2Rad; _ggShardP1682.Add(new GgShardP { s = o.s, p = o.p, v = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * T.eyeShardSpeed1682, life = T.eyeShardLife1682, dmg = o.dmg * T.eyeDmg1682 }); }
                    AtProj1651 += n; GgEyeShards1682 += n; o.ang += T.eyeSpin1682; o.pulse = o.t;
                }
                int ord = YSort(o.p.y) + 120; Vector2 c = o.p + Vector2.up * 0.6f;
                if (!GgFx1682("GGFX_EyeBlackWaters", o.p, -28, -10, 56, 50, YSort(o.p.y) + 60, o.t, 0.6f, ZP.F("_Progress", Mathf.Clamp01(o.t / Mathf.Max(0.01f, o.life))), ZP.F("_Pulse", Mathf.Clamp01(1f - (o.t - o.pulse) / 0.1f))))
                {   // PLACEHOLDER: a black-water eye: dark core, cyan iris ring, a white pupil glint that turns with the spray
                    ZDiscO1618(c, 0.32f, new Color(GN.r, GN.g, GN.b, 0.95f), ord, 1f); ZRingO1618(c, 0.36f, new Color(GC.r, GC.g, GC.b, 0.9f), ord + 1, 1f);
                    float an = o.ang * Mathf.Deg2Rad; ZPx1618(c + new Vector2(Mathf.Cos(an), Mathf.Sin(an)) * 0.15f, 2, GW, ord + 2);
                }
                if (o.t >= o.life) { GgFxO1682(3, o.p, 0.6f, 0.2f); _ggEyes1682.RemoveAt(i); }
            }
            for (int i = _ggShardP1682.Count - 1; i >= 0; i--)
            {
                var sh = _ggShardP1682[i]; Vector2 a = sh.p; sh.p += sh.v * dt; sh.t += dt; bool hit = false;
                _ggq1682.Clear(); QueryBody1595(a, sh.p, T.eyeShardR1682, _ggq1682);
                Enemy he = null; float hd = 1e9f; foreach (int ei in _ggq1682) { var e = _en[ei]; if (!GgOk1682(e)) continue; float d = (e.pos - a).sqrMagnitude; if (d < hd) { hd = d; he = e; } } // the nearest body along the step (not query order)
                if (he != null) { Hit(he, sh.dmg, sh.s, sh.v.normalized * 0.3f); GgEyeHits1682++; hit = true; }
                if (!hit && GgCrystalAt1682(sh.p, T.eyeShardR1682)) hit = true; // a Frost Wall crystal hit by a shard explodes
                if (hit || sh.t >= sh.life) { if (hit) { if (he != null && GgFxLive1682("GGFX_EyeShard_Hit")) GgFxO1682(-1, he.pos + new Vector2(0f, 14f / 16f), 0.2f, 0.22f, "GGFX_EyeShard_Hit", new Vector4(-16, -20, 32, 36)); else GgFxO1682(1, sh.p, 0.2f, 0.12f); } _ggShardP1682.RemoveAt(i); continue; }
                int ord = YSort(sh.p.y) + 60; Vector2 dn = sh.v.normalized, c = sh.p + Vector2.up * 0.75f; // released 12 px up
                if (!GgFx1682("GGFX_EyeShard", c, -13, -13, 26, 26, ord, sh.t, 0.2f, ZP.V("_Dir", dn))) { ZPx1618(c, 2, GW, ord); ZPx1618(c - dn * (2.5f / 16f), 1, GC, ord - 1); }
            }
        }

        // =============================================================== MOVEMENT (SPACE layer on Dash49; own cooldown ZhMoveCd; no damage)
        /// <summary>top of ZhDashStart1618 (a real dash start, before the hero moves). On cooldown = a plain dash.</summary>
        void GgDashStart1682(Vector2 p0)
        {
            if (!GgOn1682) return; int m = GgMove1682; if (m == 0 || ZhMoveCd > 0f) return;
            var T = TZ; int k = GgK1682(0, m); GgCasts1682++; _ggPendMove1682 = m; _ggDash0_1682 = p0;
            if (m == 1) { ZhMoveCd = ZkCd1651(k, T.rdCd1682); HellordAudio.Play("frost_cleave", 0.7f); }
            else if (m == 2) { GgRimeT1682 = ZkDur1651(k, T.rcDur1682); _ggRimeDur1682 = GgRimeT1682; GgRimeN1682 = Mathf.Max(1, T.rcN1682); ZhMoveCd = ZkCd1651(k, T.rcCd1682); HellordAudio.Play("curse_rune_arm", 1.5f); GgFxO1682(3, p0, 1.1f, 0.3f); }
            else if (m == 3) { ZhMoveCd = ZkCd1651(k, T.fgCd1682); }
            else if (m == 4) { ZhMoveCd = ZkCd1651(k, T.frCd1682); }
        }
        /// <summary>Dash49 after the dash started (direction set): Ride the Flood lengthens + steers it; Floodgate / Frozen River are placed along it.</summary>
        void GgDashAfter1682()
        {
            int m = _ggPendMove1682; _ggPendMove1682 = 0; if (m == 0 || !GgOn1682) return; var T = TZ; int k = GgK1682(0, m);
            Vector2 dir = _dashDir49.sqrMagnitude > 1e-4f ? _dashDir49.normalized : Vector2.right, n = new Vector2(-dir.y, dir.x);
            if (m == 1) { GgRideT1682 = T.rideDur1682; _dashT49 = Mathf.Max(_dashT49, T.rideDur1682); _chill44 = 0; _frozen44 = 0f; GgRides1682++; }
            else if (m == 3)
            {   // one wall at a time (report caps: walls 1), across the path at the dash start
                Vector2 c = _ggDash0_1682 + dir * 0.2f; float L = T.fgLen1682;
                _ggGate1682 = new GgWall { dir = dir, a = c - n * L * 0.5f, b = c + n * L * 0.5f, life = ZkDur1651(k, T.fgDur1682), thick = T.fgThick1682, hp = Mathf.Max(1, T.fgHits1682) }; HellordAudio.Play("frost_cleave", 0.5f); GgFxO1682(1, c, 1f, 0.2f);
            }
            else if (m == 4)
            {
                Vector2 a = _ggDash0_1682, b = a + dir * T49.dashDistU49; ClampToRoom(ref b, HeroMarginPx);
                _ggRiver1682.Clear(); _ggRiver1682.Add(new GgStrip { a = a - dir * 0.3f, b = b + dir * 0.3f, life = ZkDur1651(k, T.frDur1682), w = T.frW1682 }); HellordAudio.Play("frost_bomb_pulse", 0.7f);
            }
        }
        Vector2 _ggRidePrev1682 = new Vector2(-1e9f, 0f); float _ggRimeDur1682 = 1f, _ggRimeProcT1682 = -9f;
        void TickGgMove1682(float dt)
        {
            var T = TZ;
            if (GgRideT1682 > 0f)
            {   // steer the ride toward the cursor; no chill / freeze / hook drag while riding
                GgRideT1682 -= dt; Vector2 want = MouseWorld() - _heroPos;
                if (want.sqrMagnitude > 0.04f) { want.Normalize(); Vector2 nd = Vector2.Lerp(_dashDir49, want, 1f - Mathf.Exp(-T.rideTurn1682 * dt)); if (nd.sqrMagnitude > 1e-6f) _dashDir49 = nd.normalized; }
                _chill44 = 0; _frozen44 = 0f; _dragBy = null; if (GgRideT1682 <= 0f) { _dashT49 = Mathf.Min(_dashT49, 0f); _ggRidePrev1682 = new Vector2(-1e9f, 0f); }
                int o = ZhHeroOrd1625 - 1; float k = 1f - Mathf.Clamp01(GgRideT1682 / Mathf.Max(0.01f, T.rideDur1682));
                Vector2 vel = dt > 1e-5f && _ggRidePrev1682.x > -1e8f ? (_heroPos - _ggRidePrev1682) / dt * 16f : _dashDir49 * 16f * T49.dashDistU49 / Mathf.Max(0.05f, T49.dashTimeS49); _ggRidePrev1682 = _heroPos; // HFX _Vel: hero px/s
                if (!GgFx1682("GGFX_RideTheFlood", _heroPos, -52, -40, 104, 80, GgGround1682(_heroPos), k * T.rideDur1682, 0.3f, ZP.V("_Vel", vel), ZP.F("_Progress", k)))
                {   // PLACEHOLDER: a wave crest under the feet + spray behind
                    Vector2 nn = new Vector2(-_dashDir49.y, _dashDir49.x); ZSegO1618(_heroPos - nn * 0.6f - _dashDir49 * 0.2f, _heroPos + nn * 0.6f - _dashDir49 * 0.2f, 0.3f, new Color(GB.r, GB.g, GB.b, 0.8f), o); ZSegO1618(_heroPos - nn * 0.55f, _heroPos + nn * 0.55f, 0.12f, GW, o + 1);
                    for (int q = 0; q < 3; q++) ZPx1618(_heroPos - _dashDir49 * (0.5f + q * 0.3f) + nn * (q - 1) * 0.3f, 1, GC, o);
                }
            }
            if (GgRimeT1682 > 0f)
            {
                GgRimeT1682 -= dt; if (GgRimeT1682 <= 0f || GgRimeN1682 <= 0) { GgRimeT1682 = 0f; GgRimeN1682 = 0; }
                else if (!GgFx1682("GGFX_RimeCoat", _heroPos, -17, -8, 34, 48, ZhHeroOrd1625 + 1, runTime, 0.2f, ZP.F("_Charges", GgRimeN1682), ZP.F("_Form", Mathf.Clamp01((_ggRimeDur1682 - GgRimeT1682) / 0.2f)), ZP.F("_Intensity", Mathf.Clamp01(GgRimeT1682 / 0.3f)), ZP.F("_Pulse", Mathf.Clamp01(1f - (runTime - _ggRimeProcT1682) / 0.2f))))
                {   // PLACEHOLDER: a frosty ring + one ice pip per charge
                    int o = ZhHeroOrd1625 + 260; ZRingO1618(_heroPos + Vector2.up * 0.6f, 0.7f, new Color(GI.r, GI.g, GI.b, 0.75f), o); for (int q = 0; q < GgRimeN1682; q++) ZPx1618(_heroPos + new Vector2(-0.15f + q * 0.3f, 1.6f), 2, GW, o + 1);
                }
            }
            var g = _ggGate1682;
            if (g != null)
            {
                g.t += dt; if (g.t >= g.life || g.hp <= 0) { if (g.hp <= 0) { GgGateBroken1682++; HellordAudio.Play("ice_shatter", 1.2f); } Vector2 gm = (g.a + g.b) * 0.5f; float ga = GgDeg1682(g.dir);
                    if (GgFxLive1682("GGFX_Floodgate")) { var bo = GgFxO1682(-1, gm, 1.2f, 0.3f, "GGFX_Floodgate", GgIceWallRc1682(ga)); bo.ord = YSort(gm.y); bo.t0 = g.t; float dk = 1f - g.hp / (float)Mathf.Max(1, T.fgHits1682); bo.ps = new[] { ZP.F("_Angle", ga), ZP.F("_Damage", Mathf.Clamp01(dk)) }; bo.pf = o2 => new[] { ZP.F("_Progress", 1f), ZP.F("_Break", Mathf.Clamp01((o2.t - o2.delay) / o2.life)) }; } // HFX _Break 0 -> 1 over 0.3 s
                    else GgFxO1682(2, gm, 1.2f, 0.3f); _ggGate1682 = null; }
                else
                {
                    float dmgK = 1f - g.hp / (float)Mathf.Max(1, T.fgHits1682); Vector2 m = (g.a + g.b) * 0.5f; int o = YSort(Mathf.Min(g.a.y, g.b.y)) + 20;
                    float ga = GgDeg1682(g.dir); Vector4 grc = GgIceWallRc1682(ga); // HFX: _Angle = dash direction (the wall runs across it)
                    if (!GgFx1682("GGFX_Floodgate", m, grc.x, grc.y, grc.z, grc.w, YSort(m.y), g.t, 0.5f, ZP.F("_Angle", ga), ZP.F("_Progress", Mathf.Clamp01(g.t / 1f)), ZP.F("_Damage", dmgK), ZP.F("_Break", 0f)))
                    {   // PLACEHOLDER: a pale ice slab (two tones) with cracks as it takes hits
                        ZSegO1618(g.a, g.b, g.thick * 2f, new Color(GP.r, GP.g, GP.b, 0.95f), o); ZSegO1618(g.a + Vector2.up * 0.25f, g.b + Vector2.up * 0.25f, g.thick * 1.6f, new Color(GI.r, GI.g, GI.b, 0.95f), o + 1); ZSegO1618(g.a + Vector2.up * 0.45f, g.b + Vector2.up * 0.45f, 0.06f, GW, o + 2);
                        int cr = Mathf.FloorToInt(dmgK * 5f); for (int q = 0; q < cr; q++) { Vector2 p = Vector2.Lerp(g.a, g.b, 0.15f + 0.17f * q) + Vector2.up * 0.25f; ZSegO1618(p, p + new Vector2(0.1f, 0.18f), 0.04f, GD, o + 3); }
                    }
                }
            }
            for (int i = _ggRiver1682.Count - 1; i >= 0; i--)
            {
                var r = _ggRiver1682[i]; r.t += dt; if (r.t >= r.life) { _ggRiver1682.RemoveAt(i); continue; }
                Vector2 m = (r.a + r.b) * 0.5f; Query(m, (r.b - r.a).magnitude * 0.5f + r.w, _ggq1682);
                foreach (int ei in _ggq1682) { var e = _en[ei]; if (!GgOk1682(e) || PbSegDist(e.pos, r.a, r.b) > r.w * 0.5f + e.radius) continue; if (e.slowT <= 0f || e.slowAmt < T.frSlow1682) e.slowAmt = T.frSlow1682; e.slowT = Mathf.Max(e.slowT, 0.12f); e.chillT = Mathf.Max(e.chillT, 0.12f); GgRiverSlows1682++; }
                float fade = Mathf.Clamp01((r.life - r.t) / 0.4f); int o = FbGround + YSort(m.y) + 20; Vector2 tp = (r.b - r.a) * 16f; Vector4 rrc = GgSegRc1682(tp, 20f);
                if (!GgFx1682("GGFX_FrozenRiver", r.a, rrc.x, rrc.y, rrc.z, rrc.w, GgGround1682(r.a), r.t, 0.4f, ZP.V("_TargetPx", tp), ZP.F("_Ignite", Mathf.Clamp01(r.t / Mathf.Max(0.05f, T49.dashTimeS49))), ZP.F("_Fade", 1f - fade)))
                {   // PLACEHOLDER: a pale ice strip with a bright centre line and moving glints
                    ZSegO1618(r.a, r.b, r.w, new Color(GI.r, GI.g, GI.b, 0.35f * fade), o); ZSegO1618(r.a, r.b, 0.06f, new Color(GW.r, GW.g, GW.b, 0.6f * fade), o + 1);
                    for (int q = 0; q < 4; q++) ZPx1618(Vector2.Lerp(r.a, r.b, Mathf.Repeat(q * 0.25f + runTime * 0.6f, 1f)) + new Vector2(0f, (q % 2 - 0.5f) * r.w * 0.5f), 1, GW, o + 2);
                }
            }
        }
        /// <summary>HurtHero (after the damage landed): Rime Coat freezes the contact attacker; Ram of Mount Buzhou wrath.</summary>
        void GgHeroHit1682(float dmg, bool contact, Enemy by)
        {
            if (!GgOn1682) return; var T = TZ;
            if (GgRimeT1682 > 0f && GgRimeN1682 > 0 && contact && GgOk1682(by))
            {
                GgFreeze1682(by, T.rcFreeze1682); by.lured1651 = true; /* probe */ GgRimeN1682--; GgRimeFreezes1682++; GgFxO1682(2, BodyCentre1595(by), 0.6f, 0.3f); _ggRimeProcT1682 = runTime; HellordAudio.Play("ice_shatter", 1.4f);
                if (GgRimeN1682 <= 0) GgRimeT1682 = 0f;
            }
            GgUltHurt1682(dmg);
        }
        /// <summary>Floodgate blocks enemy bullets (UpdateBullets49).</summary>
        bool GgBulletBlock1682(Vector2 p)
        {
            var g = _ggGate1682; if (g == null || PbSegDist(p, g.a, g.b) > g.thick + 0.15f) return false;
            GgGateBullets1682++; return true;
        }
        /// <summary>push a demon out of segment a-b (half-thickness th) to the side it came from. True = it was blocked.</summary>
        bool GgSegBlock1682(Enemy e, Vector2 prev, Vector2 a, Vector2 b, float th)
        {
            Vector2 ab = b - a; float l2 = ab.sqrMagnitude; float R = th + e.radius; if (l2 < 1e-6f) return GgDiscBlock1682(e, prev, a, R - e.radius);
            float k = Mathf.Clamp01(Vector2.Dot(e.pos - a, ab) / l2); Vector2 c = a + ab * k, off = e.pos - c; float d = off.magnitude; if (d >= R) return false;
            Vector2 nrm = new Vector2(-ab.y, ab.x).normalized; float side = Vector2.Dot(prev - a, nrm); if (Mathf.Abs(side) < 1e-4f) side = Vector2.Dot(e.pos - a, nrm); if (Mathf.Abs(side) < 1e-4f) side = 1f;
            Vector2 outD = k > 0f && k < 1f ? nrm * Mathf.Sign(side) : (d > 1e-4f ? off / d : nrm * Mathf.Sign(side));
            e.pos = c + outD * R; if (Vector2.Dot(e.push, outD) < 0f) e.push -= outD * Vector2.Dot(e.push, outD); return true;
        }
        bool GgDiscBlock1682(Enemy e, Vector2 prev, Vector2 c, float r)
        {
            float R = r + e.radius; Vector2 off = e.pos - c; float d = off.magnitude; if (d >= R) return false;
            Vector2 outD = d > 1e-4f ? off / d : ((prev - c).sqrMagnitude > 1e-6f ? (prev - c).normalized : Vector2.right); e.pos = c + outD * R; if (Vector2.Dot(e.push, outD) < 0f) e.push -= outD * Vector2.Dot(e.push, outD); return true;
        }
        /// <summary>UpdateEnemies after a demon moved (bosses never come here): walls / pillars hold it back.</summary>
        void GgEnemyMove1682(Enemy e, Vector2 prev)
        {
            var g = _ggGate1682;
            if (g != null && GgSegBlock1682(e, prev, g.a, g.b, g.thick))
            {   // each blocked demon chips the wall once per fgHitEvery1682 s
                GgGateBlocks1682++; if (!g.hitAt.TryGetValue(e.id, out float lt) || runTime - lt >= TZ.fgHitEvery1682) { g.hitAt[e.id] = runTime; g.hp--; }
            }
            GgBlockSpec1682(e, prev); GgBlockUlt1682(e, prev);
        }

        // =============================================================== SPECIALS (RMB, ZhSpecCd)
        void GgSpecial1682()
        {
            int sp = GgSpec1682; if (sp == 0) return; if (ZhSpecCd > 0f) { HellordAudio.Play("basic_not_ready", 0.7f); return; }
            int k = GgK1682(1, sp); var T = TZ; Vector2 cur = MouseWorld(); ClampToRoom(ref cur, 4);
            if (sp == 1)
            {   // Drowning Mark: the demon nearest the cursor within the pick radius (fixed: Mark has no Area tag)
                Enemy best = null; float bd = T.dmPickR1682 * T.dmPickR1682; Query(cur, T.dmPickR1682, _ggq1682);
                foreach (int ei in _ggq1682) { var e = _en[ei]; if (!GgOk1682(e)) continue; float d = (e.pos - cur).sqrMagnitude; if (d <= bd) { bd = d; best = e; } }
                if (best == null) { HellordAudio.Play("basic_not_ready", 0.7f); return; }
                GgMark1682 = best; _ggMarkId1682 = best.id; GgMarkT1682 = ZkDur1651(k, T.dmLife1682); _ggMarkDur1682 = GgMarkT1682; best.lured1651 = true; /* probe */ ZhSpecCd = ZkCd1651(k, T.dmCd1682); HellordAudio.Play("curse_rune_arm", 0.8f); GgFxO1682(3, best.pos, 0.9f, 0.3f);
            }
            else if (sp == 2) { _ggCurse1682.Add(new GgCurse { p = cur, r = ZkR1651(k, T.hyR1682), dur = ZkDur1651(k, T.hyDur1682) }); ZhSpecCd = ZkCd1651(k, T.hyCd1682); HellordAudio.Play("curse_rune_arm", 0.7f); }
            else if (sp == 3)
            {
                if (!GgShatter1682(k, cur)) { HellordAudio.Play("basic_not_ready", 0.7f); return; } // nothing frozen near the cursor: no cast, no cooldown
                ZhSpecCd = ZkCd1651(k, T.shCd1682);
            }
            else if (sp == 4)
            {
                Vector2 dir = cur - _heroPos; dir = dir.sqrMagnitude > 1e-4f ? dir.normalized : PbAim1567();
                _ggPillar1682.Add(new GgPillar { p = cur, dir = dir, delay = T.tpDelay1682, r = T.tpR1682, k = k }); ZhSpecCd = ZkCd1651(k, T.tpCd1682); HellordAudio.Play("fire_pillar_telegraph", 0.7f); GgFxO1682(1, cur, 0.8f, 0.25f);
            }
            else if (sp == 5)
            {   // one wall at a time: crystals on a chord across the aim at the cursor, bowed away from you
                Vector2 dir = cur - _heroPos; dir = dir.sqrMagnitude > 1e-4f ? dir.normalized : PbAim1567(); Vector2 n = new Vector2(-dir.y, dir.x); float L = ZkR1651(k, T.fwLen1682); int N = Mathf.Max(2, T.fwN1682);
                var w = new GgFrostWall { life = ZkDur1651(k, T.fwDur1682), r = ZkR1651(k, T.fwBoomR1682), mid = cur, dir = dir };
                for (int i = 0; i < N; i++) { float f = i / (N - 1f) * 2f - 1f; Vector2 p = cur + n * f * L * 0.5f + dir * T.fwBow1682 * (1f - f * f); ClampToRoom(ref p, 4); w.c.Add(new GgCrystal { p = p }); }
                _ggFw1682 = w; ZhSpecCd = ZkCd1651(k, T.fwCd1682); HellordAudio.Play("frost_cleave", 0.45f);
            }
            GgCasts1682++;
        }
        /// <summary>Shatter: every frozen demon within shR of the cursor explodes (shBlastR, x2.5); its freeze is used up and the blast chills the demons around it.</summary>
        bool GgShatter1682(int k, Vector2 cur)
        {
            var T = TZ; float R = ZkR1651(k, T.shR1682), br = ZkR1651(k, T.shBlastR1682); var s = ZkI1651(k); var fr = new List<Enemy>(); Query(cur, R, _ggq1682);
            foreach (int ei in _ggq1682) { var e = _en[ei]; if (GgOk1682(e) && e.freezeT > 0f && (e.pos - cur).magnitude <= R + e.radius) fr.Add(e); }
            if (fr.Count == 0) return false;
            foreach (var e in fr)
            {
                Vector2 c = e.pos; e.freezeT = 0f; e.chillHits = 0; if (StatusFX.IsAlive(e.chillH)) StatusFX.SetFrozen(e.chillH, false); GgShatters1682++;
                Query(c, br, _ggq1682);
                foreach (int oi in _ggq1682.ToArray()) { var o = _en[oi]; if (!GgOk1682(o) || (o.pos - c).magnitude > br + o.radius) continue; Hit(o, ZkHit1651(k) * T.shDmg1682, s, (o.pos - c).normalized * 0.8f); if (o != e && GgOk1682(o)) ApplyStatus(o, Status.Chill, 0f, GgCK1682(s), s); }
                GgFxO1682(2, c, br, 0.5f, "GGFX_Shatter_Burst", new Vector4(-57, -15, 114, 72)); GgFxO1682(0, c, br, 0.3f);
            }
            HellordAudio.Play("ice_shatter", 0.9f); ScreenShake.Add(1.5f, 0.12f); return true;
        }
        void GgPillarFall1682(GgPillar p)
        {
            var T = TZ; var s = ZkI1651(p.k); float len = ZkR1651(p.k, T.tpLen1682), hw = ZkR1651(p.k, T.tpW1682) * 0.5f; Vector2 a = p.p, b = p.p + p.dir * len; ClampToRoom(ref b, 4);
            for (int q = 0; q < 3; q++) SkillArea53(Vector2.Lerp(a, b, (q + 0.5f) / 3f), hw + len / 6f, ZkHit1651(p.k) * T.tpDmg1682, (int)SkillId.GgToppledPillar);
            Query((a + b) * 0.5f, len * 0.5f + hw + 1f, _ggq1682);
            foreach (int ei in _ggq1682.ToArray())
            {
                var e = _en[ei]; if (!GgOk1682(e) || !GgInLine1682(e, a, b, hw)) continue; Hit(e, ZkHit1651(p.k) * T.tpDmg1682, s, p.dir * 0.6f); GgPillarHits1682++;
                for (int c = 0; c < T.tpChills1682 && GgOk1682(e); c++) ApplyStatus(e, Status.Chill, 0f, GgCK1682(s), s);
            }
            HellordAudio.Play("oxhead_stomp_strike", 0.8f); ScreenShake.Add(2.5f, 0.18f); { var cr = GgFxO1682(4, a, hw, 0.35f, "GGFX_ToppledPillar_Crush", GgSegRc1682((b - a) * 16f, 20f), b); cr.ord = GgGround1682(a); cr.pf = o2 => new[] { ZP.F("_Ignite", 1f), ZP.F("_Fade", Mathf.Clamp01(((o2.t - o2.delay) / o2.life - 0.4f) / 0.6f)) }; }
        }
        void GgCrystalBoom1682(GgCrystal c)
        {
            var w = _ggFw1682; if (w == null || !c.alive) return; c.alive = false; var T = TZ; int k = GgK1682(1, 5); var s = ZkI1651(k);
            Query(c.p, w.r, _ggq1682);
            foreach (int ei in _ggq1682.ToArray()) { var o = _en[ei]; if (!GgOk1682(o) || (o.pos - c.p).magnitude > w.r + o.radius) continue; Hit(o, ZkHit1651(k) * T.fwDmg1682, s, (o.pos - c.p).normalized * 0.8f); if (GgOk1682(o)) ApplyStatus(o, Status.Chill, 0f, GgCK1682(s), s); }
            GgCrystalBooms1682++; GgFxO1682(2, c.p + Vector2.up * 0.375f, w.r, 0.4f, "GGFX_FrostWall_Pop", new Vector4(-38, -12, 76, 54)); HellordAudio.Play("ice_shatter", 1.2f);
        }
        /// <summary>SkillArea53 (any of your areas): Frost Wall crystals inside it explode.</summary>
        void GgArea1682(Vector2 c, float r, int key)
        {
            var w = _ggFw1682; if (w == null || key == (int)SkillId.GgFrostWall) return; float cr = TZ.fwCrR1682;
            foreach (var x in w.c.ToArray()) if (x.alive && (x.p - c).magnitude <= r + cr) GgCrystalBoom1682(x);
        }
        /// <summary>a projectile at p (radius r): a crystal it touches explodes. True = it hit one.</summary>
        bool GgCrystalAt1682(Vector2 p, float r)
        {
            var w = _ggFw1682; if (w == null) return false; float cr = TZ.fwCrR1682;
            foreach (var x in w.c) if (x.alive && (x.p - p).magnitude <= r + cr) { GgCrystalBoom1682(x); return true; }
            return false;
        }
        void TickGgSpec1682(float dt)
        {
            var T = TZ;
            // ---- Drowning Mark
            if (GgMark1682 != null)
            {
                var e = GgMark1682; GgMarkT1682 -= dt;
                if (!GgOk1682(e) || e.id != _ggMarkId1682 || GgMarkT1682 <= 0f) { GgMark1682 = null; GgMarkT1682 = 0f; }
                else
                {
                    Vector2 top = e.pos + Vector2.up * (e.boss ? BossHeadY + 0.4f : e.hitY * 2f + 0.5f); int o = YSort(e.pos.y) + 300;
                    if (!GgFx1682("GGFX_DrowningMark", e.pos, -24, -10, 48, 66, YSort(e.pos.y) + 60, runTime, 0.1f, ZP.F("_Form", Mathf.Clamp01((_ggMarkDur1682 - GgMarkT1682) / 0.2f)), ZP.F("_Intensity", Mathf.Clamp01(GgMarkT1682 / 0.3f)), ZP.F("_Pulse", Mathf.Clamp01(1f - (runTime - _ggRelayT1682) / 0.25f))))
                    {   // PLACEHOLDER: a blue water drop over the head + a faint relay ring on the ground
                        ZDiscO1618(top, 0.16f, new Color(GB.r, GB.g, GB.b, 0.95f), o); ZSegO1618(top + new Vector2(0f, 0.12f), top + new Vector2(0f, 0.34f), 0.12f, GB, o); ZPx1618(top + new Vector2(-0.05f, 0.03f), 1, GW, o + 1);
                        ZRingO1618(e.pos, T.dmRelayR1682, new Color(GC.r, GC.g, GC.b, 0.15f + 0.35f * Mathf.Clamp01(1f - (runTime - _ggRelayT1682) / 0.3f)), FbGround + YSort(e.pos.y) + 30, 0.55f);
                    }
                }
            }
            // ---- Hypothermia (delayed area curse)
            for (int i = _ggCurse1682.Count - 1; i >= 0; i--)
            {
                var c = _ggCurse1682[i]; c.t += dt; int o = FbGround + YSort(c.p.y) + 30; float hk = c.r * 16f / 40f; // HFX rect (-52,-34,104,68) is for R 40 px
                bool hyFx = GgFx1682("GGFX_Hypothermia", c.p, Mathf.Floor(-52f * hk), Mathf.Floor(-34f * hk), Mathf.Ceil(104f * hk), Mathf.Ceil(68f * hk), GgGround1682(c.p), c.t, 0.2f, ZP.F("_Ignite", Mathf.Clamp01(c.t / Mathf.Max(0.01f, T.hyDelay1682))), ZP.F("_Fade", Mathf.Clamp01((c.t - T.hyDelay1682 - 1.6f) / 0.4f)), ZP.F("_Radius", c.r * 16f));
                if (!c.done)
                {
                    float k = Mathf.Clamp01(c.t / Mathf.Max(0.01f, T.hyDelay1682));
                    if (!hyFx)
                    { ZRingO1618(c.p, c.r, new Color(GI.r, GI.g, GI.b, 0.5f), o, 0.55f); ZRingO1618(c.p, c.r * (1f - k), new Color(GC.r, GC.g, GC.b, 0.85f), o + 1, 0.55f); }
                    if (c.t >= T.hyDelay1682)
                    {
                        c.done = true; Query(c.p, c.r, _ggq1682);
                        _ggHypoDur1682 = c.dur; foreach (int ei in _ggq1682) { var e = _en[ei]; if (!GgOk1682(e) || (e.pos - c.p).magnitude > c.r + e.radius) continue; e.ggHypo1682 = runTime + c.dur; e.lured1651 = true; /* probe */ GgCursed1682++; }
                        if (!hyFx) GgFxO1682(0, c.p, c.r, 0.35f); HellordAudio.Play("curse_rune_burst", 0.8f);
                    }
                }
                else if (c.t >= T.hyDelay1682 + (hyFx ? 2f : 0.4f)) _ggCurse1682.RemoveAt(i); // HFX: the frost ground stays 2 s after the curse lands (0.4 s fade)
            }
            if (GgOn1682 && GgSpec1682 == 2)
                for (int i = 0; i < maxEnemies; i++)
                {   // cursed overlay: a pale snowflake over the head
                    var e = _en[i]; if (e == null || !GgOk1682(e) || e.ggHypo1682 <= runTime) continue; Vector2 top = e.pos + Vector2.up * (e.boss ? BossHeadY + 0.3f : e.hitY * 2f + 0.35f); int o = YSort(e.pos.y) + 290;
                    float hl = e.ggHypo1682 - runTime;
                    if (!GgFx1682("GGFX_Hypothermia_Cursed", e.pos, -20, -8, 42, 46, YSort(e.pos.y) + 60, runTime, e.id * 0.13f, ZP.F("_Form", Mathf.Clamp01((_ggHypoDur1682 - hl) / 0.25f)), ZP.F("_Intensity", Mathf.Clamp01(hl / 0.3f)))) { ZSegO1618(top + new Vector2(-0.14f, 0f), top + new Vector2(0.14f, 0f), 0.05f, GI, o); ZSegO1618(top + new Vector2(0f, -0.14f), top + new Vector2(0f, 0.14f), 0.05f, GI, o); }
                }
            // ---- Toppled Pillar
            for (int i = _ggPillar1682.Count - 1; i >= 0; i--)
            {
                var p = _ggPillar1682[i]; p.t += dt; if (!p.fell && p.t >= p.delay) { p.fell = true; GgPillarFall1682(p); }
                float len = ZkR1651(p.k, T.tpLen1682); int o = YSort(p.p.y) + 10; float pa = GgDeg1682(p.dir); Vector4 prc = GgIcePillarRc1682(pa);
                if (!p.fell)
                {
                    float k = Mathf.Clamp01(p.t / Mathf.Max(0.01f, p.delay)), rise = Mathf.Clamp01(p.t / 0.15f);
                    if (!GgFx1682("GGFX_ToppledPillar", p.p, prc.x, prc.y, prc.z, prc.w, YSort(p.p.y), p.t, 0.7f, ZP.F("_Angle", pa), ZP.F("_Progress", k), ZP.F("_Break", 0f)))
                    {   // PLACEHOLDER: a stone pillar with an ice band rising out of the ground + the fall line telegraph
                        ZSegO1618(p.p, p.p + Vector2.up * 2.4f * rise, 0.55f, GS, o); ZSegO1618(p.p + Vector2.up * 0.9f * rise, p.p + Vector2.up * 1.5f * rise, 0.6f, GI, o + 1); ZSegO1618(p.p + Vector2.up * 2.4f * rise, p.p + Vector2.up * 2.5f * rise, 0.45f, GW, o + 1);
                        ZSegO1618(p.p, p.p + p.dir * len, ZkR1651(p.k, T.tpW1682), new Color(GD.r, GD.g, GD.b, 0.15f + 0.25f * k), FbGround + YSort(p.p.y) + 20);
                    }
                }
                else
                {
                    float f = Mathf.Clamp01((p.t - p.delay) / 0.85f); Vector2 b = p.p + p.dir * len; // HFX _Break 0 -> 1 over 0.85 s after the fall
                    if (!GgFx1682("GGFX_ToppledPillar", p.p, prc.x, prc.y, prc.z, prc.w, YSort(p.p.y), p.t, 0.7f, ZP.F("_Angle", pa), ZP.F("_Progress", 1f), ZP.F("_Break", f)))
                    { ZSegO1618(p.p, b, 0.5f, new Color(GS.r, GS.g, GS.b, 1f - f), YSort(Mathf.Min(p.p.y, b.y)) + 10); ZSegO1618(Vector2.Lerp(p.p, b, 0.4f), Vector2.Lerp(p.p, b, 0.62f), 0.55f, new Color(GI.r, GI.g, GI.b, 1f - f), YSort(Mathf.Min(p.p.y, b.y)) + 11); }
                    if (f >= 1f) _ggPillar1682.RemoveAt(i);
                }
            }
            // ---- Frost Wall
            var w = _ggFw1682;
            if (w != null)
            {
                w.t += dt; bool any = false; foreach (var x in w.c) any |= x.alive;
                if (w.t >= w.life || !any) { foreach (var x in w.c) if (x.alive) GgFxO1682(3, x.p, 0.4f, 0.2f); _ggFw1682 = null; }
                else
                    for (int ci = 0; ci < w.c.Count; ci++)
                    {
                        var x = w.c[ci]; if (!x.alive) continue; int o = YSort(x.p.y) + 10; float fade = Mathf.Clamp01((w.life - w.t) / 0.3f);
                        float bt = w.t - 0.1f * Mathf.Abs(ci - (w.c.Count - 1) * 0.5f), left = w.life - w.t, fp = left < 0.4f ? 0.75f + 0.25f * (1f - left / 0.4f) : Mathf.Min(0.74f, 0.1f * Mathf.Max(0f, bt) / 0.15f); // HFX: centre first, crumble over the last 0.4 s
                        if (!GgFx1682("GGFX_FrostWall", x.p, -23, -19, 46, 51, YSort(x.p.y), w.t, x.p.x * 0.37f, ZP.F("_Progress", fp)))
                        { ZSegO1618(x.p, x.p + Vector2.up * 1.3f, 0.5f, new Color(GP.r, GP.g, GP.b, fade), o); ZSegO1618(x.p + Vector2.up * 0.2f, x.p + Vector2.up * 1.25f, 0.3f, new Color(GI.r, GI.g, GI.b, fade), o + 1); ZPx1618(x.p + new Vector2(-0.06f, 1.0f), 1, GW, o + 2); }
                    }
            }
        }
        float _ggRelayT1682 = -9f, _ggMarkDur1682 = 1f, _ggHypoDur1682 = 1f;
        /// <summary>Hit (after damage): a cold hit on the Drowning-Marked demon chills every demon within dmRelayR1682 of it.</summary>
        void GgPostHit1682(Enemy e, float dmg, SkillInst s, bool tick)
        {
            if (!GgOn1682 || _ggRelaying1682 || GgMark1682 == null || e != GgMark1682 || GgMarkT1682 <= 0f || !GgCold1682(s)) return;
            _ggRelaying1682 = true; var src = ZkI1651(GgK1682(1, 1)); float R = TZ.dmRelayR1682; Query(e.pos, R, _ggq1682);
            foreach (int ei in _ggq1682.ToArray()) { var o = _en[ei]; if (o == e || !GgOk1682(o) || (o.pos - e.pos).magnitude > R + o.radius) continue; ApplyStatus(o, Status.Chill, 0f, GgCK1682(src), src); o.lured1651 = true; GgMarkRelays1682++; }
            _ggRelayT1682 = runTime; _ggRelaying1682 = false;
        }
        /// <summary>enemy move: a standing pillar and the Frost Wall crystals hold demons back.</summary>
        void GgBlockSpec1682(Enemy e, Vector2 prev)
        {
            foreach (var p in _ggPillar1682) if (!p.fell && GgDiscBlock1682(e, prev, p.p, p.r)) GgPillarBlocks1682++;
            var w = _ggFw1682; if (w != null) { float cr = TZ.fwCrR1682; foreach (var x in w.c) if (x.alive && GgDiscBlock1682(e, prev, x.p, cr)) GgWallBlocks1682++; }
        }

        // =============================================================== ULTIMATES (R, ZhUltCd; meters carry over rooms, GgRunReset1682 per run)
        public bool GgHighTide1682 => TZ != null && GgTide1682 >= TZ.gfRise1682;
        public bool GgUltArmed1682
        {
            get
            {
                if (ZhUltCd > 0f || _ggRam1682 != null) return false; var T = TZ;
                switch (GgUlt1682) { case 1: return GgWrath1682 >= 0.999f; case 2: return GgHighTide1682; case 3: return GgFrozenKills1682 >= Mathf.Max(1, T.ewNeed1682) && GgWinterT1682 <= 0f; case 4: return GgAoArmedLatch1682 && GgAoT1682 <= 0f; default: return false; }
            }
        }
        /// <summary>0..1 meter of the slotted ultimate (kit bar). The Great Flood: the tide gauge (full = High Tide).</summary>
        public float GgUltProg1682()
        {
            var T = TZ;
            switch (GgUlt1682)
            {
                case 1: return Mathf.Clamp01(GgWrath1682);
                case 2: return GgHighTide1682 ? 1f : Mathf.Clamp01(GgTide1682 / Mathf.Max(1f, T.gfRise1682));
                case 3: return Mathf.Clamp01((float)GgFrozenKills1682 / Mathf.Max(1, T.ewNeed1682));
                case 4: return GgAoArmedLatch1682 ? 1f : Mathf.Clamp01((1f - hp / Mathf.Max(1f, maxHp)) / Mathf.Max(0.01f, 1f - T.aoArm1682));
                default: return 0f;
            }
        }
        /// <summary>HurtHero: Ram of Mount Buzhou wrath fills from damage taken (full at ramFill1682 x max HP).</summary>
        void GgUltHurt1682(float dmg)
        {
            if (GgUlt1682 != 1 || dmg <= 0f) return; GgWrath1682 = Mathf.Clamp01(GgWrath1682 + dmg / Mathf.Max(1f, maxHp * TZ.ramFill1682)); _ggLastHurt1682 = runTime;
        }
        void GgUltimate1682()
        {
            int u = GgUlt1682; if (u == 0) return; if (!GgUltArmed1682) { HellordAudio.Play("basic_not_ready", 0.6f); return; }
            int k = GgK1682(2, u); var T = TZ; GgCasts1682++;
            if (u == 1)
            {
                Vector2 dir = PbAim1567(); _ggRam1682 = new GgRam { a = _heroPos, p = _heroPos, dir = dir, dur = T.ramTime1682, len = T.ramLen1682 }; GgWrath1682 = 0f; ZhUltCd = ZkCd1651(k, T.ramCd1682);
                HellordAudio.Play("oxhead_lunge_dash", 0.8f); HellordAudio.Play("oxhead_roar", 0.8f); Toast("RAM OF MOUNT BUZHOU");
            }
            else if (u == 2) { _ggFlood1682 = new GgFlood { o = _heroPos, s0 = -ZkR1651(k, T.gfReach1682), s1 = ZkR1651(k, T.gfReach1682) }; ZhUltCd = ZkCd1651(k, T.gfCd1682); HellordAudio.Play("pact_seal_descend", 0.7f); ScreenShake.Add(2f, 0.4f); Toast("THE GREAT FLOOD"); }
            else if (u == 3) { GgFrozenKills1682 = 0; GgWinterT1682 = ZkDur1651(k, T.ewDur1682); _ggWinterDur1682 = GgWinterT1682; _ggWinterNext1682 = 0f; ZhUltCd = ZkCd1651(k, T.ewCd1682); HellordAudio.Play("frost_bomb_pulse", 0.5f); Toast("ETERNAL WINTER"); }
            else if (u == 4) { _ggAoC1682 = _heroPos; GgAoT1682 = ZkDur1651(k, T.aoDur1682); _ggAoDur1682 = GgAoT1682; _ggAoNext1682 = 0f; ZhUltCd = ZkCd1651(k, T.aoCd1682); HellordAudio.Play("oxhead_stomp_strike", 0.6f); ScreenShake.Add(2f, 0.2f); Toast("PILLARS OF AO"); }
        }
        float _ggWinterDur1682 = 1f, _ggAoDur1682 = 1f; readonly float[] _ggAoShotT1682 = { -9f, -9f, -9f, -9f };
        void TickGgUlt1682(float dt)
        {
            var T = TZ;
            // ---- meters
            if (runTime - _ggLastHurt1682 > T.ramGrace1682) GgWrath1682 = Mathf.Max(0f, GgWrath1682 - T.ramDecay1682 * dt);
            float cyc = T.gfRise1682 + T.gfHigh1682; GgTide1682 += dt; if (cyc > 0.1f && GgTide1682 >= cyc) GgTide1682 -= cyc; // the tide keeps cycling; pressing does not reset it
            if (hp < maxHp * T.aoArm1682) GgAoArmedLatch1682 = true; else if (hp > maxHp * T.aoDisarm1682) GgAoArmedLatch1682 = false;
            if (GgUlt1682 == 2) GgDrawTide1682();
            // ---- Ram of Mount Buzhou: head-first charge (unhurt), x8 along the path, then 12 shards rain around the end
            var r = _ggRam1682;
            if (r != null)
            {
                int k = GgK1682(2, 1); var s = ZkI1651(k); r.t += dt; float f = Mathf.Min(1f, r.t / Mathf.Max(0.05f, r.dur)); Vector2 prev = r.p, np = r.a + r.dir * r.len * f, cl = np; ClampToRoom(ref cl, HeroMarginPx); bool wall = (cl - np).sqrMagnitude > 0.0004f; r.p = cl;
                _heroPos = r.p; _iframe = Mathf.Max(_iframe, 0.25f); _dashT49 = 0f; float hw = ZkR1651(k, T.ramW1682);
                Query((prev + r.p) * 0.5f, (r.p - prev).magnitude * 0.5f + hw + 1f, _ggq1682);
                foreach (int ei in _ggq1682.ToArray()) { var e = _en[ei]; if (!GgOk1682(e) || r.hit.Contains(ei) || PbSegDist(e.pos, prev, r.p) > hw + e.radius) continue; r.hit.Add(ei); Hit(e, ZkHit1651(k) * T.ramDmg1682, s, Vector2.zero); GgRamHits1682++; }
                SkillArea53(r.p, hw, ZkHit1651(k) * T.ramDmg1682, (int)SkillId.GgRamOfBuzhou);
                int o = ZhHeroOrd1625 + 2; Vector2 rtp = r.dir * r.len * 16f; Vector4 rrc = GgSegRc1682(rtp, 24f); // HFX: pivot = charge start, _TargetPx = the full path
                GgFx1682("GGFX_Ram_Head", r.p, -36, -22, 72, 72, YSort(r.p.y) + 60, r.t, 0.5f, ZP.V("_Dir", r.dir), ZP.F("_Form", Mathf.Clamp01(r.t / 0.08f)), ZP.F("_Intensity", 1f));
                if (!GgFx1682("GGFX_Ram_Charge", r.a, rrc.x, rrc.y, rrc.z, rrc.w, GgGround1682(r.a), r.t, 0.5f, ZP.V("_TargetPx", rtp), ZP.F("_Progress", f), ZP.F("_Fade", 0f)))
                { Vector2 n = new Vector2(-r.dir.y, r.dir.x); ZSegO1618(r.p + Vector2.up * 0.7f + r.dir * 0.5f - n * 0.5f, r.p + Vector2.up * 0.7f + r.dir * 0.5f + n * 0.5f, 0.18f, GW, o); ZSegO1618(r.a, r.p, hw * 2f, new Color(GB.r, GB.g, GB.b, 0.25f), FbGround + YSort(r.p.y) + 30); }
                if (f >= 1f || wall)
                {
                    _ggRam1682 = null; float R = ZkR1651(k, T.ramRain1682); int N = Mathf.Max(1, T.ramShards1682);
                    if (GgFxLive1682("GGFX_Ram_Charge")) { var rc = GgFxO1682(-1, r.a, 0f, 0.5f, "GGFX_Ram_Charge", rrc); rc.ord = GgGround1682(r.a); rc.t0 = r.t; rc.ps = new[] { ZP.V("_TargetPx", rtp), ZP.F("_Progress", f) }; rc.pf = o2 => new[] { ZP.F("_Fade", Mathf.Clamp01((o2.t - o2.delay) / o2.life)) }; } // the torrent fades over 0.5 s
                    for (int i = 0; i < N; i++)
                    {
                        float an = i * 137.5f * Mathf.Deg2Rad, rr = R * Mathf.Sqrt((i + 0.5f) / N); Vector2 p = r.p + new Vector2(Mathf.Cos(an), Mathf.Sin(an) * 0.7f) * rr; ClampToRoom(ref p, 4); float dl = 0.1f + T.ramRainS1682 * i / N; _ggShards1682.Add(new GgShard { p = p, delay = dl });
                        if (GgFxLive1682("GGFX_Ram_Shard")) { float st = dl - 0.3f; var so = GgFxO1682(-1, p, 0f, 0.6f - Mathf.Max(0f, -st), "GGFX_Ram_Shard", new Vector4(-30, -14, 60, 140)); so.delay = Mathf.Max(0f, st); so.t0 = Mathf.Max(0f, -st); so.pf = o2 => new[] { ZP.F("_Progress", Mathf.Clamp01((o2.t - o2.delay + o2.t0) / 0.6f)) }; } // HFX: spawned 0.3 s before the impact, 0.6 s life (falls, then shatters)
                    }
                    GgFxO1682(0, r.p, 1.5f, 0.35f); HellordAudio.Play("oxhead_stomp_strike", 1f); ScreenShake.Add(3f, 0.25f);
                }
            }
            for (int i = _ggShards1682.Count - 1; i >= 0; i--)
            {
                var sh = _ggShards1682[i]; sh.t += dt; int k = GgK1682(2, 1);
                if (!sh.done && sh.t >= sh.delay)
                {
                    sh.done = true; float R = ZkR1651(k, T.ramShardR1682); SkillArea53(sh.p, R, ZkHit1651(k) * T.ramShardDmg1682, (int)SkillId.GgRamOfBuzhou); Query(sh.p, R, _ggq1682);
                    foreach (int ei in _ggq1682.ToArray()) { var e = _en[ei]; if (!GgOk1682(e) || (e.pos - sh.p).magnitude > R + e.radius) continue; Hit(e, ZkHit1651(k) * T.ramShardDmg1682, ZkI1651(k), (e.pos - sh.p).normalized * 0.6f); GgRamShardHits1682++; }
                    if (!GgFxLive1682("GGFX_Ram_Shard")) GgFxO1682(2, sh.p, R, 0.3f); HellordAudio.Play("ice_shatter", 0.8f + 0.03f * i);
                }
                if (!sh.done) { float h = 5f * (1f - sh.t / Mathf.Max(0.01f, sh.delay)); if (sh.t > sh.delay - 0.25f && !GgFxLive1682("GGFX_Ram_Shard")) ZSegO1618(sh.p + Vector2.up * (h + 0.5f), sh.p + Vector2.up * h, 0.14f, GI, YSort(sh.p.y) + 400); }
                else if (sh.t >= sh.delay + 0.2f) _ggShards1682.RemoveAt(i);
            }
            // ---- The Great Flood: a wave line sweeps NW -> SE over everything within reach of the cast point
            var fl = _ggFlood1682;
            if (fl != null)
            {
                int k = GgK1682(2, 2); var s = ZkI1651(k); fl.t += dt; float f = Mathf.Clamp01(fl.t / Mathf.Max(0.05f, T.gfSweep1682)), sl = Mathf.Lerp(fl.s0, fl.s1, f), reach = fl.s1; Vector2 d = new Vector2(1f, -1f).normalized, n = new Vector2(1f, 1f).normalized;
                for (int i = 0; i < maxEnemies; i++)
                {
                    var e = _en[i]; if (e == null || !GgOk1682(e) || fl.hit.Contains(i) || (e.pos - fl.o).magnitude > reach + e.radius || Vector2.Dot(e.pos - fl.o, d) > sl) continue;
                    fl.hit.Add(i); Hit(e, ZkHit1651(k) * T.gfDmg1682, s, Vector2.zero); GgFloodHits1682++; if (!GgOk1682(e)) continue;
                    for (int c = 0; c < T.gfChills1682 && GgOk1682(e); c++) ApplyStatus(e, Status.Chill, 0f, GgCK1682(s), s);
                    e.push += GgForce1682(e, d * T.gfPush1682 * 8f); // the push decays at 8/s: about gfPush1682 u (non-boss; elites half)
                }
                Vector2 m = fl.o + d * sl; int o = YSort(m.y) + 300;
                float Rp = Mathf.Round(reach * 16f); // HFX: pivot = the cast point, the shader moves the front; rect covers the reach; _Length = reach px (Area-scaled)
                if (!GgFx1682("GGFX_GreatFlood", fl.o, -Rp - 130f, -Rp - 40f, 2f * Rp + 260f, 2f * Rp + 80f, GgGround1682(fl.o), fl.t, 0.5f, ZP.F("_Progress", f), ZP.F("_Length", Rp)))
                { ZSegO1618(m - n * reach - d * 0.6f, m + n * reach - d * 0.6f, 1.1f, new Color(GD.r, GD.g, GD.b, 0.55f), o - 1); ZSegO1618(m - n * reach, m + n * reach, 0.5f, new Color(GB.r, GB.g, GB.b, 0.85f), o); ZSegO1618(m - n * reach + d * 0.25f, m + n * reach + d * 0.25f, 0.14f, GW, o + 1); }
                if (f >= 1f) _ggFlood1682 = null;
            }
            // ---- Eternal Winter: 1 chill per second on everything within reach; frozen deaths shatter (queued by GgOnKill1682, chained next frame)
            if (GgWinterT1682 > 0f)
            {
                int k = GgK1682(2, 3); var s = ZkI1651(k); GgWinterT1682 -= dt; _ggWinterNext1682 -= dt; float reach = ZkR1651(k, T.ewReach1682);
                if (_ggWinterNext1682 <= 0f) { _ggWinterNext1682 = T.ewEvery1682; if (GgFxLive1682("GGFX_EternalWinter_Pulse")) GgFxO1682(-1, _heroPos, 0f, 0.8f, "GGFX_EternalWinter_Pulse", new Vector4(-226, -132, 452, 264)).ord = GgGround1682(_heroPos); for (int i = 0; i < maxEnemies; i++) { var e = _en[i]; if (e == null || !GgOk1682(e) || (e.pos - _heroPos).magnitude > reach + e.radius) continue; ApplyStatus(e, Status.Chill, 0f, GgCK1682(s), s); e.lured1651 = true; GgWinterChills1682++; } }
                if (!GgFx1682("GGFX_EternalWinter", _heroPos, -330, -190, 660, 380, 30000, runTime, 0.3f, ZP.F("_Ignite", Mathf.Min(1f, (_ggWinterDur1682 - GgWinterT1682) / 0.5f)), ZP.F("_Fade", 1f - Mathf.Min(1f, GgWinterT1682 / 0.5f)), ZP.F("_Intensity", 1f)))
                    for (int q = 0; q < 24; q++) { float x = Mathf.Repeat(q * 7.3f + runTime * 0.8f, 26f) - 13f, y = 8f - Mathf.Repeat(q * 3.1f + runTime * (1.5f + q % 3 * 0.4f), 16f); ZPx1618(_heroPos + new Vector2(x, y), 1, GW, 30000); }
                if (GgWinterT1682 <= 0f) GgWinterT1682 = 0f;
            }
            if (_ggBurstQ1682.Count > 0)
            {
                int k = GgK1682(2, 3); var s = ZkI1651(k); float R = ZkR1651(k, T.ewR1682); var q = _ggBurstQ1682.ToArray(); _ggBurstQ1682.Clear();
                foreach (var c in q)
                {
                    Query(c, R, _ggq1682); GgWinterBursts1682++;
                    foreach (int ei in _ggq1682.ToArray()) { var e = _en[ei]; if (!GgOk1682(e) || (e.pos - c).magnitude > R + e.radius) continue; Hit(e, ZkHit1651(k) * T.ewDmg1682, s, Vector2.zero); if (GgOk1682(e)) ApplyStatus(e, Status.Chill, 0f, GgCK1682(s), s); }
                    if (GgFxLive1682("GGFX_EternalWinter_Shatter")) GgFxO1682(-1, c, R, 0.55f, "GGFX_EternalWinter_Shatter", new Vector4(-68, -17, 137, 83)); else { GgFxO1682(2, c + Vector2.up * 0.5f, R, 0.3f); GgFxO1682(0, c, R, 0.3f); }
                }
                HellordAudio.Play("ice_shatter", 1.1f);
            }
            // ---- Pillars of Ao: 4 pillars on a square around the cast point; icicles at the nearest demon
            if (GgAoT1682 > 0f)
            {
                int k = GgK1682(2, 4); var s = ZkI1651(k); GgAoT1682 -= dt; _ggAoNext1682 -= dt; float h = ZkR1651(k, T.aoHalf1682), rng = ZkR1651(k, T.aoRange1682);
                for (int q = 0; q < 4; q++)
                {
                    Vector2 p = _ggAoC1682 + new Vector2(q % 2 == 0 ? -h : h, q < 2 ? -h : h); int o = YSort(p.y) + 10;
                    float ael = _ggAoDur1682 - GgAoT1682; // HFX: _Progress rise, _Pulse per shot, _Break 0 -> 0.25 over the last 0.3 s
                    if (!GgFx1682("GGFX_PillarsOfAo", p, -24, -14, 82, 66, YSort(p.y), runTime, q * 0.25f, ZP.F("_Progress", Mathf.Clamp01(ael / 1f)), ZP.F("_Pulse", Mathf.Clamp01(1f - (runTime - _ggAoShotT1682[q]) / 0.2f)), ZP.F("_Break", 0.25f * Mathf.Clamp01(1f - GgAoT1682 / 0.3f))))
                    { ZSegO1618(p, p + Vector2.up * 2.2f, 0.5f, GP, o); ZSegO1618(p + Vector2.up * 0.1f, p + Vector2.up * 2.1f, 0.28f, GI, o + 1); ZSegO1618(p + Vector2.up * 2.1f, p + Vector2.up * 2.3f, 0.4f, GW, o + 2); }
                    if (_ggAoNext1682 > 0f) continue;
                    int ni = Nearest(p, rng, -1); if (ni < 0 || !GgOk1682(_en[ni])) continue; var e = _en[ni];
                    Hit(e, ZkHit1651(k) * T.aoDmg1682, s, Vector2.zero); GgAoIcicles1682++; _ggAoShotT1682[q] = runTime;
                    { Vector2 tip = p + Vector2.up * (72f / 16f), to = BodyCentre1595(e); float fly = Mathf.Max(0.05f, (to - tip).magnitude * 16f / 220f); // HFX: a dart flying tip -> target (220 px/s), _Dir; the hit flash on arrival
                      var ic = GgFxO1682(4, tip, 0.2f, fly, "GGFX_Icicle", new Vector4(-16, -16, 32, 32), to); ic.dart = true;
                      if (GgFxLive1682("GGFX_Icicle_Hit")) GgFxO1682(-1, e.pos + new Vector2(0f, 14f / 16f), 0.2f, 0.22f, "GGFX_Icicle_Hit", new Vector4(-18, -22, 36, 40)).delay = fly; }
                }
                if (_ggAoNext1682 <= 0f) _ggAoNext1682 = T.aoEvery1682;
                ZRingO1618(_ggAoC1682, 0.1f, GI, FbGround + YSort(_ggAoC1682.y) + 30, 1f);
                if (GgAoT1682 <= 0f) { GgAoT1682 = 0f; GgFxO1682(1, _ggAoC1682, h, 0.3f); }
            }
        }
        /// <summary>The tide gauge (The Great Flood slotted): a small bar over the hero; bright + pulsing at High Tide.</summary>
        void GgDrawTide1682()
        {
            var T = TZ; float f = GgHighTide1682 ? 1f : Mathf.Clamp01(GgTide1682 / Mathf.Max(1f, T.gfRise1682)); Vector2 a = _heroPos + new Vector2(-0.6f, 2.35f); int o = ZhHeroOrd1625 + 300;
            if (GgFx1682("GGFX_TideGauge", a + new Vector2(0.6f, 0f), -12, -3, 24, 6, o, runTime, 0f, ZP.F("_Fill", f), ZP.F("_High", GgHighTide1682 ? 1f : 0f))) return;
            ZSegO1618(a, a + Vector2.right * 1.2f, 0.14f, new Color(GN.r, GN.g, GN.b, 0.8f), o); Color c = GgHighTide1682 ? Color.Lerp(GC, GW, 0.5f + 0.5f * Mathf.Sin(runTime * 8f)) : GB;
            if (f > 0.01f) ZSegO1618(a, a + Vector2.right * 1.2f * f, 0.1f, c, o + 1);
        }
        /// <summary>KillEnemy (top, statuses still on): Eternal Winter counts frozen kills (elites / bosses ewElite1682) and, while active, queues the shatter burst.</summary>
        void GgOnKill1682(Enemy e)
        {
            if (!GgOn1682 || e == null || e.freezeT <= 0f) return; var T = TZ;
            if (GgWinterT1682 > 0f) { _ggBurstQ1682.Add(e.pos); return; }
            if (GgUlt1682 == 3) GgFrozenKills1682 = Mathf.Min(Mathf.Max(1, T.ewNeed1682), GgFrozenKills1682 + (e.elite || e.boss ? Mathf.Max(1, T.ewElite1682) : 1));
        }
        /// <summary>enemy move: Pillars of Ao keep non-boss demons out of the square (pushed to the nearest edge).</summary>
        void GgBlockUlt1682(Enemy e, Vector2 prev)
        {
            if (GgAoT1682 <= 0f || e.boss) return; int k = GgK1682(2, 4); float H = ZkR1651(k, TZ.aoHalf1682) + e.radius; Vector2 d = e.pos - _ggAoC1682;
            if (Mathf.Abs(d.x) >= H || Mathf.Abs(d.y) >= H) return;
            if (H - Mathf.Abs(d.x) < H - Mathf.Abs(d.y)) e.pos.x = _ggAoC1682.x + (d.x >= 0f ? H : -H); else e.pos.y = _ggAoC1682.y + (d.y >= 0f ? H : -H);
            e.lured1651 = true; /* probe */ GgAoPushes1682++;
        }
    }
}
