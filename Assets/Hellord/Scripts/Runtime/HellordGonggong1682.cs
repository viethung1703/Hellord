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
        public static readonly string[] GgKeys1682 = { "ridetheflood", "rimecoat", "floodgate", "frozenriver", "drowningmark", "hypothermia", "shatter", "toppledpillar", "frostwall", "ramofmountbuzhou", "thegreatflood", "eternalwinter", "pillarsofao", "tidalsurge", "whirlpool", "breathofthedeep", "buzhourift", "eyeoftheblackwaters" };
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
        // movement
        [System.NonSerialized] public float GgRideT1682, GgRimeT1682; [System.NonSerialized] public int GgRimeN1682; int _ggPendMove1682; Vector2 _ggDash0_1682;
        public bool GgRiding1682 => GgRideT1682 > 0f;
        class GgWall { public Vector2 a, b; public float t, life, thick; public int hp; public readonly Dictionary<int, float> hitAt = new Dictionary<int, float>(); }
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
        class GgWave { public SkillInst s; public Vector2 p, dir; public float dmg, left, w; public readonly HashSet<int> carry = new HashSet<int>(); }
        readonly List<GgWave> _ggWaves1682 = new List<GgWave>();
        class GgPool { public SkillInst s; public Vector2 p; public float t, dur, r, dmg; }
        readonly List<GgPool> _ggPools1682 = new List<GgPool>();
        class GgBreath { public SkillInst s; public float t, tick; public readonly Dictionary<int, float> held = new Dictionary<int, float>(); }
        GgBreath _ggBreath1682; bool _ggBreathNeedUp1682; public bool GgBreathOn1682 => _ggBreath1682 != null;
        class GgRift { public SkillInst s; public Vector2 a, b; public float t, dmg; public int done; public bool spiked; }
        readonly List<GgRift> _ggRifts1682 = new List<GgRift>();
        class GgEye { public SkillInst s; public Vector2 p, v; public float t, life, dmg, acc, ang; }
        readonly List<GgEye> _ggEyes1682 = new List<GgEye>();
        class GgShardP { public SkillInst s; public Vector2 p, v; public float t, life, dmg; }
        readonly List<GgShardP> _ggShardP1682 = new List<GgShardP>();
        class GgFxO { public Vector2 p, q; public float t, life, r; public int kind; public string mat; public Vector4 rc; public ZP[] ps; }
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
        public static readonly string[] GgFxDefs1682 = { "GGFX_TidalSurge", "GGFX_TidalSurge_Crash", "GGFX_Whirlpool", "GGFX_Whirlpool_Collapse", "GGFX_Breath", "GGFX_Rift_Burst", "GGFX_Rift_Spike", "GGFX_Eye_Orb", "GGFX_Eye_Shard",
            "GGFX_RideTheFlood", "GGFX_RimeCoat", "GGFX_Floodgate", "GGFX_FrozenRiver", "GGFX_DrowningMark", "GGFX_Hypothermia", "GGFX_Hypothermia_Status", "GGFX_Shatter", "GGFX_ToppledPillar", "GGFX_ToppledPillar_Crush",
            "GGFX_FrostWall_Crystal", "GGFX_FrostWall_Boom", "GGFX_Ram_Charge", "GGFX_Ram_Shard", "GGFX_GreatFlood", "GGFX_TideGauge", "GGFX_EternalWinter", "GGFX_Winter_Burst", "GGFX_AoPillar", "GGFX_AoIcicle" };
        /// <summary>one-shot effect: kind 0 expanding ring, 1 flash disc, 2 shatter star, 3 shrinking ring, 4 line flash p -> q; mat = its shader (rect px l,b,w,h).</summary>
        void GgFxO1682(int kind, Vector2 p, float r, float life, string mat = null, Vector4 rc = default, Vector2 q = default) => _ggFx1682.Add(new GgFxO { kind = kind, p = p, q = q, r = r, life = life, mat = mat, rc = rc });
        static Vector4 GgZoneRc1682(float rU) { float R = Mathf.Round(rU * 16f); float h = Mathf.Ceil(R * 0.55f); return new Vector4(-(R + 8), -(h + 8), 2 * R + 16, 2 * h + 16); }
        void DrawGgFx1682(float dt)
        {
            for (int i = _ggFx1682.Count - 1; i >= 0; i--)
            {
                var f = _ggFx1682[i]; f.t += dt; if (f.t >= f.life) { _ggFx1682.RemoveAt(i); continue; } float k = f.t / f.life; int ord = FbGround + YSort(f.p.y) + 40;
                if (f.mat != null && GgFx1682(f.mat, f.p, f.rc.x, f.rc.y, f.rc.z, f.rc.w, YSort(f.p.y) + 60, f.t, 0.37f, ZP.F("_Progress", k), ZP.F("_Radius", f.r), ZP.V("_TargetPx", (f.q - f.p) * 16f))) continue;
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

        // ---- STAGE STUBS (filled in by the next stages)
        void TickGgBasics1682(float dt) { }
        bool CastTidalSurge1682(SkillInst s, float dmg) => false;
        bool CastWhirlpool1682(SkillInst s, float dmg) => false;
        bool CastBreath1682(SkillInst s, float dmg) => false;
        bool CastBuzhouRift1682(SkillInst s, float dmg) => false;
        bool CastEye1682(SkillInst s, float dmg) => false;
        void GgDashStart1682(Vector2 p0) { }
        void GgDashAfter1682() { }
        void TickGgMove1682(float dt) { }
        void GgHeroHit1682(float dmg, bool contact, Enemy by) { }
        bool GgBulletBlock1682(Vector2 p) => false;
        void GgEnemyMove1682(Enemy e, Vector2 prev) { }
        void GgSpecial1682() { }
        void TickGgSpec1682(float dt) { }
        void GgPostHit1682(Enemy e, float dmg, SkillInst s, bool tick) { }
        void GgArea1682(Vector2 c, float r, int key) { }
        public bool GgUltArmed1682 => false;
        public float GgUltProg1682() => 0f;
        void GgUltimate1682() { }
        void TickGgUlt1682(float dt) { }
        void GgOnKill1682(Enemy e) { }
    }
}
