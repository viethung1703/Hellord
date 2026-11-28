using PixelFXPack;
using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    // t1666 (VH GO, t1665): Leizu's 4-slot kit, mirroring Zhurong's (HellordZhurong1618.cs / HellordKit1651.cs / HellordKitCards1639.cs).
    //  Design: research/t1664_leizu_kit/report.md (+ its t1665 section). FIRST-PASS numbers, all in HellordTuning (t1666 section).
    //  LMB basics (start pick): Spark Spray, Ball Lightning, Thunder Javelin (t1567) + NEW Thunder Drum Strikes, Thunder Lash, Live Wire
    //    (held beam), Thunder Hound (summon). Chain Lightning left Leizu's pool (descendBasicsLeizu1555; code + SkillId kept).
    //  SPACE movement (own cooldown, no damage): Thunder Step, Grounding Ward, Storm Stride, Mirror Flash.
    //  RMB special: Mark of Judgement (one demon), Conductivity (area curse), Thunder Ministry Drum, Static Field.
    //  R ultimate (armed, then pressed): Decree of the 24 Thunder Lords (combo), Heaven-Splitting Arc (3 infusion pickups),
    //    Tribunal of the Nine Heavens (energy from shocking), Judgement Warp (a cull-ready elite; boss-fight fallback).
    //  Active in a Leizu run (pact path 2) while kit1618 + lzKit1666 are on (kitAnyPact1618 = Zhurong's kit everywhere, so it wins).
    //  The kit slots share Zhurong's run picks (ZhRun*1639), slot cooldowns (ZhMoveCd / ZhSpecCd / ZhUltCd), level cards and the
    //  4-slot bar. Kit skill k = 12 + index in LzIds1666 (HellordKit1651.cs: ZkI1651 / ZkCd1651 / ZkR1651 / ZkDur1651 / ZkHit1651).
    //  Ultimate meters (energy / combos / infusions) carry over rooms (like 0908dfd) and reset per run (LzRunReset1666 in ResetV44).
    //  ICON HOOK: Resources/<lzIconDir1666>/icon_<key>_24.png (keys LzKeys1666); missing = a procedural placeholder icon.
    //  HOUND ART (HA t1666): Resources/HellordArt/Leizu1666/thunder_hound/thunder_hound_all_<idle|run|bite>_<south|east|north>.png, 64x64 frames,
    //    feet pivot (32,58), idle 8f@8, run 6f@12, bite 6f@12, west = flipped east (Summon561 layout). Missing = the procedural placeholder.
    //  FX (HFX t1666, spec Art/fx_leizu_t1666/spec.md, in Resources/LeizuFX): LzFx1666("LZFX_<name>", ...) draws LeizuFX/Materials/LZFX_<name>.mat (+ _Glow twin) through the Zhurong
    //    shader-quad route when present (lzFx1666 on); otherwise the procedural PLACEHOLDERS below (Endesga-32 blues + ChainFX bolts).
    // =====================================================================================================================
    public partial class HellordGame
    {
        public static readonly string[] LzKeys1666 = { "thunderstep", "groundingward", "stormstride", "mirrorflash", "markofjudgement", "conductivity", "thunderministrydrum", "staticfield", "decree24thunderlords", "heavensplittingarc", "tribunalnineheavens", "judgementwarp", "thunderdrumstrikes", "thunderlash", "livewire", "thunderhound" };
        public static readonly string[] LzMoveNames1666 = { "NONE", "THUNDER STEP", "GROUNDING WARD", "STORM STRIDE", "MIRROR FLASH" };
        public static readonly string[] LzSpecNames1666 = { "NONE", "MARK OF JUDGEMENT", "CONDUCTIVITY", "THUNDER MINISTRY DRUM", "STATIC FIELD" };
        public static readonly string[] LzUltNames1666 = { "NONE", "DECREE OF THE 24 THUNDER LORDS", "HEAVEN-SPLITTING ARC", "TRIBUNAL OF THE NINE HEAVENS", "JUDGEMENT WARP" };
        static readonly string[] LzMoveDesc1666 = { "", "Dash becomes an instant teleport. Aim near your Ball Lightning to jump into it.", "After a dash the next 2 hits on you are absorbed; attackers are shocked.", "After a dash: faster movement and faster basic attacks.", "Dash start flashes: nearby demons are dazzled and stop attacking." };
        static readonly string[] LzSpecDesc1666 = { "", "Mark the demon near the cursor. Lightning hits charge it, then a bolt strikes and arcs.", "Curse an area: shock is stronger, lasts longer and spreads.", "Place a drum. Your lightning hitting it booms a shockwave.", "Pulse around you: takes a share of current HP and shocks. Never kills." };
        static readonly string[] LzUltDesc1666 = { "", "Chain basic hit, movement and special twice: 24 bolts on different demons.", "Collect 3 lightning drops from shocked demons: a huge arc chains 12 times.", "Shock demons to charge: bolts strike the strongest shocked demon.", "A weakened elite: teleport into it and it explodes." };
        public static readonly SkillId[] LzIds1666 = { SkillId.LzThunderStep, SkillId.LzGroundingWard, SkillId.LzStormStride, SkillId.LzMirrorFlash, SkillId.LzMarkOfJudgement, SkillId.LzConductivity, SkillId.LzThunderDrum, SkillId.LzStaticField, SkillId.LzDecree, SkillId.LzHeavenArc, SkillId.LzTribunal, SkillId.LzJudgementWarp };
        public const int LzK0 = 12; // kit index of Thunder Step (after Zhurong's 12)
        public static bool IsLzBasic1666(SkillId id) => id >= SkillId.ThunderDrumStrikes && id <= SkillId.ThunderHound;
        static readonly Color LY = new Color32(0xfe, 0xe7, 0x61, 255), LC = new Color32(0x2c, 0xe8, 0xf5, 255), LB = new Color32(0x00, 0x99, 0xdb, 255), LD = new Color32(0x12, 0x4e, 0x89, 255),
            LK = new Color32(0x18, 0x14, 0x25, 255), LW = new Color32(0xff, 0xff, 0xff, 255), LP = new Color32(0xb5, 0x50, 0x88, 255), LG = new Color32(0xc0, 0xcb, 0xdc, 255);

        // ---------------------------------------------------------------- slots
        public bool LzOn1666 => TZ != null && TZ.kit1618 && TZ.lzKit1666 && !TZ.kitAnyPact1618 && _pactPath51 == 2;
        public bool KitOn1666 => ZhOn1618 || LzOn1666 || GgOn1682 || NkOn1713; // t1713: + Zhong Kui // t1682: + Gonggong
        public int LzMove1666 => LzOn1666 ? Mathf.Clamp(ZhSlotOverride1639 ? TZ.lzMove1666 : ZhRunMove1639, 0, 4) : 0;
        public int LzSpec1666 => LzOn1666 ? Mathf.Clamp(ZhSlotOverride1639 ? TZ.lzSpecial1666 : ZhRunSpec1639, 0, 4) : 0;
        public int LzUlt1666 => LzOn1666 ? Mathf.Clamp(ZhSlotOverride1639 ? TZ.lzUlt1666 : ZhRunUlt1639, 0, 4) : 0;
        public int KitMove1666 => NkOn1713 ? NkMove1713 : GgOn1682 ? GgMove1682 : LzOn1666 ? LzMove1666 : ZhMove1618;
        public int KitSpec1666 => NkOn1713 ? NkSpec1713 : GgOn1682 ? GgSpec1682 : LzOn1666 ? LzSpec1666 : ZhSpec1618;
        public int KitUlt1666 => NkOn1713 ? NkUlt1713 : GgOn1682 ? GgUlt1682 : LzOn1666 ? LzUlt1666 : ZhUlt1618;
        public bool KitUltArmed1666 => NkOn1713 ? NkUltArmed1713 : GgOn1682 ? GgUltArmed1682 : LzOn1666 ? LzUltArmed1666 : ZhUltArmed1618;
        /// <summary>kit index (ZkI1651) of slot 0/1/2 pick p for the running pact.</summary>
        public int KitK1666(int slot, int pick) => NkOn1713 ? NkK1713(slot, pick) : GgOn1682 ? GgK1682(slot, pick) : LzOn1666 ? LzK0 + slot * 4 + pick - 1 : ZkK1651(slot, pick);
        static int LzK1666(int slot, int pick) => LzK0 + slot * 4 + pick - 1;
        string KitMoveKey1666() => NkOn1713 ? (NkMove1713 > 0 ? NkKeys1713[NkMove1713 - 1] : null) : GgOn1682 ? (GgMove1682 > 0 ? GgKeys1682[GgMove1682 - 1] : null) : LzOn1666 ? (LzMove1666 > 0 ? LzKeys1666[LzMove1666 - 1] : null) : MoveKey1618();
        string KitSpecKey1666() => NkOn1713 ? (NkSpec1713 > 0 ? NkKeys1713[2 + NkSpec1713] : null) : GgOn1682 ? (GgSpec1682 > 0 ? GgKeys1682[3 + GgSpec1682] : null) : LzOn1666 ? (LzSpec1666 > 0 ? LzKeys1666[3 + LzSpec1666] : null) : SpecKey1618();
        string KitUltKey1666() => NkOn1713 ? (NkUlt1713 > 0 ? NkKeys1713[6 + NkUlt1713] : null) : GgOn1682 ? (GgUlt1682 > 0 ? GgKeys1682[8 + GgUlt1682] : null) : LzOn1666 ? (LzUlt1666 > 0 ? LzKeys1666[7 + LzUlt1666] : null) : UltKey1618();
        Texture2D KitIcon1666(string key) => NkOn1713 ? NkIcon1713(key) : GgOn1682 ? GgIcon1682(key) : LzOn1666 ? LzIcon1666(key) : ZhIcon1618(key);

        // ---------------------------------------------------------------- state
        [System.NonSerialized] public int LzCasts1666, LzBlinks1666, LzWardAbsorbs1666, LzDazzled1666, LzMarkPays1666, LzCursed1666, LzSpreads1666, LzDrumBooms1666, LzStaticHits1666,
            LzDecreeBolts1666, LzArcHits1666, LzTribBolts1666, LzWarps1666, LzBeats1666, LzClaps1666, LzLashHits1666, LzLashKnocks1666, LzWireTicks1666, LzWireOverloads1666, LzBites1666, LzHoundsMade1666, LzInfDropped1666;
        // movement
        bool _lzBlinkPend1666; float _lzWardPulse1666 = -9f; [System.NonSerialized] public float LzWardT1666, LzStrideT1666; [System.NonSerialized] public int LzWardN1666; [System.NonSerialized] public Vector2 LzBlinkFrom1666, LzBlinkTo1666;
        // specials
        [System.NonSerialized] public Enemy LzMark1666; int _lzMarkId1666; [System.NonSerialized] public float LzMarkT1666, LzMarkCharge1666; [System.NonSerialized] public int LzMarkHits1666;
        class LzCond { public Vector2 p; public float t, r, dur; public bool done; }
        readonly List<LzCond> _lzCond1666 = new List<LzCond>(); bool _lzSpreading1666;
        class LzDrum { public Vector2 p; public float t, life, last = -9f; public int booms; }
        LzDrum _lzDrum1666; public bool LzDrumOut1666 => _lzDrum1666 != null;
        [System.NonSerialized] public float LzFloorHp1666 = -1f; // Static Field: >= 0 = exact damage (no shock amp / crit) that never takes HP below this
        // ultimates (carry over rooms; LzRunReset1666 per run)
        [System.NonSerialized] public int LzCombos1666, LzInfusions1666; [System.NonSerialized] public float LzEnergy1666, LzBossT1666;
        readonly float[] _lzComboT1666 = { -99f, -99f, -99f }; float _lzComboLast1666 = -99f;
        class LzDecreeS { public float t, next; public int fired; public readonly HashSet<int> struck = new HashSet<int>(); }
        readonly List<LzDecreeS> _lzDecree1666 = new List<LzDecreeS>();
        [System.NonSerialized] public float LzTribT1666; float _lzTribNext1666, _lzTribDur1669 = 1f, _lzTribBolt1669 = -9f; // t1669 FX: eye _Form / _Pulse
        class LzInf { public Vector2 p; public float t; }
        readonly List<LzInf> _lzInf1666 = new List<LzInf>();
        class LzGround { public Vector2 p; public float r, t, life, tick; public int k; }
        readonly List<LzGround> _lzGround1666 = new List<LzGround>();
        // basics
        [System.NonSerialized] public int LzBeat1666; float _lzBeatT1666 = -99f;
        class LzWire { public SkillInst s; public Enemy tgt; public float t, tgtT, tick; public Vector2 end; }
        LzWire _lzWire1666; bool _lzWireNeedUp1666; public bool LzWireOn1666 => _lzWire1666 != null;
        class LzHound { public Vector2 p, home, face = Vector2.right; public float t, life, bite, dmg, seed, bitT = -9f; public bool moving; public Vector2 vel; /* t1669 FX: hound aura _Vel */ public SkillInst s; public Enemy tgt; public readonly Dictionary<int, float> bitAt = new Dictionary<int, float>(); }
        readonly List<LzHound> _lzHounds1666 = new List<LzHound>(); public int LzHounds1666 => _lzHounds1666.Count;
        class LzFxO { public Vector2 p, q; public float t, life, r, delay, plife, p0, r0, r1, seed = 0.41f; public bool haze; public int kind; public string mat; public Vector4 rc; public int ord = int.MinValue; public ZP[] ps; } // t1669: plife/p0 progress curve, r0/r1 haze radii, seed
        readonly List<LzFxO> _lzFx1666 = new List<LzFxO>();
        readonly List<int> _lzq1666 = new List<int>(64);

        void ClearLz1666()
        {   // per room (ClearZh1618): effects / entities. Meters + the ultimate cooldown carry over (t1648 rule).
            _lzBlinkPend1666 = false; LzWardT1666 = LzStrideT1666 = 0f; LzWardN1666 = 0; LzMark1666 = null; LzMarkT1666 = LzMarkCharge1666 = 0f; LzMarkHits1666 = 0;
            _lzCond1666.Clear(); _lzDrum1666 = null; LzFloorHp1666 = -1f; _lzDecree1666.Clear(); LzTribT1666 = 0f; _lzInf1666.Clear(); _lzGround1666.Clear(); LzBossT1666 = 0f;
            LzBeat1666 = 0; _lzBeatT1666 = -99f; _lzWire1666 = null; _lzWireNeedUp1666 = false; _lzHounds1666.Clear(); _lzFx1666.Clear();
        }
        void LzRunReset1666() { LzCombos1666 = LzInfusions1666 = 0; LzEnergy1666 = 0f; LzBossT1666 = 0f; for (int i = 0; i < 3; i++) _lzComboT1666[i] = -99f; _lzComboLast1666 = -99f; }
        bool LzOk1666(Enemy e) => e != null && e.alive && !e.dying;
        /// <summary>Basics' damage / cooldown from HellordTuning (lzBaseDmg1666 / lzCd1666).</summary>
        void LzDefs1666()
        {
            var T = TZ; if (T == null) return;
            foreach (var d in HData.Skills) { if (!IsLzBasic1666(d.id)) continue; int i = d.id - SkillId.ThunderDrumStrikes; if (T.lzBaseDmg1666 != null && i < T.lzBaseDmg1666.Length) d.baseDmg = T.lzBaseDmg1666[i]; if (T.lzCd1666 != null && i < T.lzCd1666.Length) d.cooldown = T.lzCd1666[i]; }
        }

        // ---------------------------------------------------------------- icon + sprite + FX hooks
        readonly Dictionary<string, Texture2D> _lzIcon1666 = new Dictionary<string, Texture2D>();
        /// <summary>Icon hook: Resources/&lt;lzIconDir1666&gt;/icon_&lt;key&gt;_24.png; missing = a procedural placeholder (dark cell + a tinted bolt).</summary>
        public Texture2D LzIcon1666(string key)
        {
            if (key == null) return null; if (_lzIcon1666.TryGetValue(key, out var t)) return t;
            string dir = TZ != null ? TZ.lzIconDir1666 : ""; if (!string.IsNullOrEmpty(dir)) t = Resources.Load<Texture2D>(dir + "/icon_" + key + "_24");
            if (t != null) t.filterMode = FilterMode.Point; else t = LzIconPh1666(key);
            return _lzIcon1666[key] = t;
        }
        public bool LzIconIsArt1666(string key) { var t = LzIcon1666(key); return t != null && t.name != "lz_ph"; }
        static Texture2D LzIconPh1666(string key)
        {   // PLACEHOLDER: 24x24 dark-blue cell, 1 px frame, a zigzag bolt tinted by slot (movement cyan, special yellow, ultimate white, basic blue)
            int ix = System.Array.IndexOf(LzKeys1666, key); Color tint = ix < 4 ? (Color)new Color32(0x2c, 0xe8, 0xf5, 255) : ix < 8 ? (Color)new Color32(0xfe, 0xe7, 0x61, 255) : ix < 12 ? Color.white : (Color)new Color32(0x00, 0x99, 0xdb, 255);
            var t = new Texture2D(24, 24, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "lz_ph" }; Color bg = new Color32(0x26, 0x2b, 0x44, 255), fr = new Color32(0x3a, 0x44, 0x66, 255);
            for (int y = 0; y < 24; y++) for (int x = 0; x < 24; x++) t.SetPixel(x, y, x == 0 || y == 0 || x == 23 || y == 23 ? fr : bg);
            int[] bx = { 14, 13, 12, 11, 10, 11, 12, 13, 14, 13, 12, 11, 10, 9, 8 }; int sh = ix < 0 ? 0 : ix % 3 - 1;
            for (int i = 0; i < bx.Length; i++) { int x = bx[i] + sh, y = 20 - i; t.SetPixel(x, y, tint); t.SetPixel(x + 1, y, tint); }
            for (int i = 0; i < 6; i++) t.SetPixel(9 + i + sh, 12, tint); t.Apply(); return t;
        }
        class LzAnim { public Sprite[] f; public float fps; }
        Dictionary<string, LzAnim> _lzAnim1666;
        /// <summary>Thunder Hound sheets (HA t1666): thunder_hound_all_&lt;anim&gt;_&lt;dir&gt; (64x64, feet pivot 32,58). null = not imported.</summary>
        LzAnim LzHoundAnim1666(string anim, string dir)
        {
            if (_lzAnim1666 == null)
            {
                _lzAnim1666 = new Dictionary<string, LzAnim>(); const int fw = 64, fh = 64, px = 32, py = 58;
                foreach (var r in new[] { "idle:8", "run:12", "bite:12" }) foreach (var d in new[] { "south", "east", "north" })
                {
                    var kv = r.Split(':'); var tx = Resources.Load<Texture2D>("HellordArt/Leizu1666/thunder_hound/thunder_hound_all_" + kv[0] + "_" + d); if (tx == null) continue;
                    tx.filterMode = FilterMode.Point; int n = Mathf.Max(1, tx.width / fw); var a = new LzAnim { f = new Sprite[n], fps = float.Parse(kv[1]) };
                    for (int i = 0; i < n; i++) a.f[i] = Sprite.Create(tx, new Rect(i * fw, 0, fw, fh), new Vector2(px / (float)fw, 1f - py / (float)fh), 16f);
                    _lzAnim1666[kv[0] + "_" + d] = a;
                }
            }
            return _lzAnim1666.TryGetValue(anim + "_" + dir, out var x) ? x : null;
        }
        readonly Dictionary<string, Material> _lzMat1666 = new Dictionary<string, Material>();
        IEnumerable<Material> LzFxMats1675() { foreach (var m in _lzMat1666.Values) if (m != null) yield return m; } // t1675 preloader warm list
        Material LzFxMat1666(string n) { if (_lzMat1666.TryGetValue(n, out var m)) return m; m = LzFxLoad1669(n); return _lzMat1666[n] = m; } // t1669: folder + renames from tuning (v2 swap), HellordLeizuFx1669.cs
        /// <summary>HFX shader hook (same quad route + logging as ZhFx1625). True = the shader drew it (skip the placeholder).</summary>
        bool LzFx1666(string mat, Vector2 pivot, float l, float b, float w, float h, int order, float fxTime, float seed, params ZP[] ps)
        {
            if (ZhFxLogOn1625) { _zfxSeen1625.Add(mat); _zfxLog1625[mat + "|_FxTime"] = fxTime; if (ps != null) foreach (var p in ps) _zfxLog1625[mat + "|" + p.n] = p.v.x; }
            if (TZ == null || !TZ.lzFx1666) return false; var m = LzFxMat1666(mat); if (m == null) return false;
            ZhFxQuad1625(m, pivot, l, b, w, h, order, fxTime, seed, ps); var g = LzFxMat1666(mat + "_Glow"); if (g != null) ZhFxQuad1625(g, pivot, l, b, w, h, order + 1, fxTime, seed, ps);
            return true;
        }
        bool LzFxLive1666(string mat) => TZ != null && TZ.lzFx1666 && LzFxMat1666(mat) != null;
        /// <summary>one-shot effect: kind 0 expanding ring, 1 flash disc, 2 strike flash, 3 shrinking ring, 4 dazzle star; mat = its shader (rect px l,b,w,h).</summary>
        void LzFxO1666(int kind, Vector2 p, float r, float life, string mat = null, Vector4 rc = default, int ord = int.MinValue, params ZP[] ps) => _lzFx1666.Add(new LzFxO { kind = kind, p = p, r = r, life = life, mat = mat, rc = rc, ord = ord, ps = ps });
        /// <summary>HFX one-shot that needs no placeholder (kind -1 quad, kind -2 endpoint bolt p -> q); starts after delay s.</summary>
        void LzFxShot1666(string mat, Vector2 p, Vector4 rc, float life, int ord, float delay = 0f, params ZP[] ps) { if (LzFxLive1666(mat)) _lzFx1666.Add(new LzFxO { kind = -1, p = p, life = life, mat = mat, rc = rc, ord = ord, ps = ps, delay = delay }); }
        void LzBoltShot1666(string mat, Vector2 a, Vector2 b, float life, int ord, float delay = 0f) { if (LzFxLive1666(mat)) _lzFx1666.Add(new LzFxO { kind = -2, p = a, q = b, life = life, mat = mat, ord = ord, delay = delay }); }
        /// <summary>HFX endpoint API (spec.md): pivot a, _TargetPx = (b - a) x 16, rect padded from the material's _Amp / _ForkLen / _Forks / _Width. False = no material.</summary>
        bool LzBoltFx1666(string mat, Vector2 a, Vector2 b, int order, float t, float seed, float overload = 0f, params ZP[] extra)
        {
            var m = TZ != null && TZ.lzFx1666 ? LzFxMat1666(mat) : null; if (m == null) return false;
            float amp = m.HasProperty("_Amp") ? m.GetFloat("_Amp") : 4f, fl = m.HasProperty("_ForkLen") ? m.GetFloat("_ForkLen") : 0f, w = m.HasProperty("_Width") ? m.GetFloat("_Width") : 0f; bool forks = m.HasProperty("_Forks") && m.GetFloat("_Forks") > 0f;
            Vector2 d = (b - a) * 16f; float L = d.magnitude; float pad = Mathf.Ceil(amp * (1f + 0.75f * overload) + 12f + (forks ? 0.7f * fl * L : 0f) + 4f * w);
            var ps = new List<ZP>(extra ?? new ZP[0]) { ZP.V("_TargetPx", new Vector2(Mathf.Round(d.x), Mathf.Round(d.y))), ZP.F("_Overload", overload) };
            return LzFx1666(mat, a, Mathf.Min(0f, d.x) - pad, Mathf.Min(0f, d.y) - pad, Mathf.Abs(d.x) + 2f * pad, Mathf.Abs(d.y) + 2f * pad, order, t, seed, ps.ToArray());
        }
        static Vector4 LzZoneRc1666(float rU) { float R = Mathf.Round(rU * 16f); float h = Mathf.Ceil(R * HellFX.GroundYSquash); return new Vector4(-(R + 8), -(h + 8), 2 * R + 16, 2 * h + 16); } // spec.md zone rect
        static Vector4 LzWaveRc1666(float rU) { float R = Mathf.Round(rU * 16f); float h = Mathf.Ceil(R * HellFX.GroundYSquash); return new Vector4(-(R + 11), -(h + 10), 2 * (R + 11), 2 * h + 24); } // Thunderclap / DrumBoom
        void LzBolt1666(Vector2 a, Vector2 b, Color? c = null) => ChainFX.ChainLightningPoints(a, b, default, default, 2, c ?? new Color(0.75f, 0.92f, 1f), 0.03f, 0.16f);
        void LzSky1666(Vector2 p) { LzBolt1666(p + new Vector2(0.25f, 5.5f), p, new Color(1f, 0.97f, 0.7f)); LzFxO1666(2, p, 0.5f, 0.18f); }

        // ---------------------------------------------------------------- update (inside UpdateZh1618's FX begin / end)
        void UpdateLz1666(float dt)
        {
            TickLzBasics1666(dt);
            if (LzOn1666)
            {
                ZhMoveCd = Mathf.Max(0f, ZhMoveCd - dt); ZhSpecCd = Mathf.Max(0f, ZhSpecCd - dt); ZhUltCd = Mathf.Max(0f, ZhUltCd - dt);
                bool free = mode == Mode.Play && !_tmOpen && _pickCool716 <= 0f;
                if (free && HInput.SpecialDown1618()) LzSpecial1666();
                if (free && HInput.UltDown1618()) LzUltimate1666();
                LzWardT1666 -= dt; LzStrideT1666 -= dt; if (LzWardT1666 <= 0f) LzWardN1666 = 0;
                LzDrawBuffs1666();
                TickLzMark1666(dt); TickLzCond1666(dt); TickLzDrum1666(dt);
                TickLzCombo1666(); TickLzDecree1666(dt); TickLzTrib1666(dt); TickLzInf1666(dt); TickLzGround1666(dt); TickLzWarp1666(dt);
            }
            if (LzOn1666) { LzStrideTrail1669(); LzDrawOverlays1669(); } // t1669 sub-effects
            DrawLzFx1666(dt);
        }
        void DrawLzFx1666(float dt)
        {
            LzFxKeyCheck1669();
            for (int i = _lzFx1666.Count - 1; i >= 0; i--)
            {
                var f = _lzFx1666[i]; if (f.delay > 0f) { f.delay -= dt; continue; } f.t += dt; if (f.t >= f.life) { _lzFx1666.RemoveAt(i); continue; } float k = f.t / f.life; int ord = FbGround + YSort(f.p.y) + 40;
                if (f.kind == -2) { LzBoltFx1666(f.mat, f.p, f.q, f.ord != int.MinValue ? f.ord : YSort(f.p.y) + 60, f.t, f.seed, 0f, ZP.F("_Progress", k)); continue; }
                if (f.mat != null) { float pk = f.plife > 0f ? f.t / f.plife + f.p0 : k; var ps = new List<ZP> { ZP.F("_Progress", pk) }; if (f.haze) { float e = 1f - (1f - k) * (1f - k); ps.Add(ZP.F("_Radius", f.r0 + (f.r1 - f.r0) * e)); ps.Add(ZP.F("_Fade", 1f - Mathf.Clamp01((k - 0.5f) / 0.5f))); if (TZ.lzHazeMode1669 >= 0f) ps.Add(ZP.F("_HazeMode", TZ.lzHazeMode1669)); } if (f.ps != null) ps.AddRange(f.ps); if (LzFx1666(f.mat, f.p, f.rc.x, f.rc.y, f.rc.z, f.rc.w, f.ord != int.MinValue ? f.ord : ord, f.t, 0.37f, ps.ToArray())) continue; }
                if (f.kind < 0) continue;
                switch (f.kind)
                {
                    case 0: ZRingO1618(f.p, f.r * (0.3f + 0.7f * k), new Color(LC.r, LC.g, LC.b, 1f - k), ord, 0.6f); ZRingO1618(f.p, f.r * (0.2f + 0.6f * k), new Color(LW.r, LW.g, LW.b, (1f - k) * 0.7f), ord + 1, 0.6f); break;
                    case 1: ZDiscO1618(f.p, f.r * (0.5f + 0.5f * k), new Color(LY.r, LY.g, LY.b, (1f - k) * 0.55f), ord, 0.6f); break;
                    case 2: ZDiscO1618(f.p, f.r * (1f - 0.5f * k), new Color(LW.r, LW.g, LW.b, (1f - k) * 0.9f), ord, 0.5f); ZRingO1618(f.p, f.r * (0.6f + k), new Color(LY.r, LY.g, LY.b, 1f - k), ord + 1, 0.5f); break;
                    case 3: ZRingO1618(f.p, f.r * (1f - k), new Color(LB.r, LB.g, LB.b, 0.5f + 0.5f * k), ord, 0.6f); break;
                    case 4: { int o = YSort(f.p.y) + 300; float a = 1f - k; for (int q = 0; q < 8; q++) { float an = q * 45f * Mathf.Deg2Rad; Vector2 d = new Vector2(Mathf.Cos(an), Mathf.Sin(an) * 0.7f); ZSegO1618(f.p, f.p + d * f.r * (0.4f + 0.6f * k) * (q % 2 == 0 ? 1f : 0.55f), 0.09f, new Color(1f, 1f, 0.85f, a), o); } ZDiscO1618(f.p, 0.35f * (1f - k) + 0.1f, new Color(1, 1, 1, a), o + 1); break; }
                }
            }
        }

        // =============================================================== BASICS (LMB; Cast switch in HellordSkills.cs)
        bool LzBasicHeld1666 => HInput.LmbHeld716() && LmbFree1517();
        /// <summary>a target near p (lightning targets the nearest demon within pickR, else the point itself).</summary>
        Vector2 LzPick1666(Vector2 p, float pickR, out Enemy e)
        {
            e = null; float best = pickR * pickR; Query(p, pickR, _lzq1666);
            foreach (int i in _lzq1666) { var o = _en[i]; if (!LzOk1666(o)) continue; float d = (o.pos - p).sqrMagnitude; if (d <= best) { best = d; e = o; } }
            return e != null ? e.pos : p;
        }
        /// <summary>Thunder Drum Strikes: beats 1-2 strike lightning at the cursor demon, beat 3 = Thunderclap (bigger on shocked demons). Count resets after dsWindow1666.</summary>
        bool CastDrumStrikes1666(SkillInst s, float dmg)
        {
            var T = TZ; if (runTime - _lzBeatT1666 > T.dsWindow1666) LzBeat1666 = 0; Vector2 cur = PbCursor1567(10f); Vector2 at = LzPick1666(cur, s.R(T.dsPickR1666), out var tgt);
            LzBeat1666++; _lzBeatT1666 = runTime; LzBeats1666++;
            if (LzBeat1666 < LzClapBeat1825)
            {
                float r = s.R(T.dsStrikeR1666); if (!LzStrike1669("DrumStrike", at, r)) LzSky1666(at); Query(at, r, _lzq1666); SkillArea53(at, r, dmg, (int)s.def.id);
                foreach (int i in _lzq1666.ToArray()) { var e = _en[i]; if (!LzOk1666(e) || (e.pos - at).magnitude > r + e.radius) continue; Hit(e, dmg * T.dsStrikeDmg1666, s, (e.pos - at).normalized * 1.5f); }
                HellordAudio.Play("storm_ring", 1.6f + 0.15f * LzBeat1666);
            }
            else
            {
                float r = s.R(T.dsClapR1666); LzBeat1666 = 0; LzClaps1666++; Query(at, r, _lzq1666); SkillArea53(at, r, dmg, (int)s.def.id, true); LzFxO1666(0, at, r, LzFxLive1666("LZFX_Thunderclap") ? 0.7f : 0.35f, "LZFX_Thunderclap", LzWaveRc1666(r)); if (!LzPlay1669("clapFlash", at)) LzSky1666(at); LzPlay1669("clapHaze", at, r * 16f / 40f);
                foreach (int i in _lzq1666.ToArray()) { var e = _en[i]; if (!LzOk1666(e) || !InGroundOval(at, e.pos, r, e.radius)) continue; Hit(e, dmg * (e.shockT > 0f ? T.dsClapShocked1666 : T.dsClapOther1666), s, (e.pos - at).normalized * 3f); }
                HellordAudio.Play("storm_ring", 0.9f); ScreenShake.Add(1.5f, 0.12f);
            }
            return true;
        }
        void DrawDrumPips1666()
        {   // beat counter over the hero while Drum Strikes is the basic (placeholder UI: 3 pips)
            if (_basic1517 == null || _basic1517.def.id != SkillId.ThunderDrumStrikes || LzBeat1666 == 0 || runTime - _lzBeatT1666 > TZ.dsWindow1666) return;
            float pa = runTime - _lzBeatT1666; if (LzFx1666("LZFX_DrumStrike_Pips", _heroPos, -12, 18, 24, 14, ZhHeroOrd1625 + 1, runTime, 0.3f, ZP.F("_Charge", LzBeat1666 / 3f), ZP.F("_Pulse", Mathf.Clamp01(1f - pa / 0.2f)), ZP.F("_Intensity", Mathf.Clamp01((TZ.dsWindow1666 - pa) / 0.2f)))) return; // t1669 HFX pips
            int o = YSort(_heroPos.y) + 320; for (int i = 0; i < 3; i++) ZDiscO1618(_heroPos + new Vector2(-0.3f + i * 0.3f, 2.1f), 0.07f, i < LzBeat1666 ? (Color)LY : new Color(LD.r, LD.g, LD.b, 0.8f), o);
        }
        /// <summary>Thunder Lash: a whip arc in front; demons ALREADY shocked are knocked back (its own knockback, not the retired support).</summary>
        bool CastThunderLash1666(SkillInst s, float dmg)
        {
            var T = TZ; float r = s.R(T.tlReach1666), half = T.tlArc1666 * 0.5f; Vector2 dir = PbAim1567(), c = _heroPos; Query(c, r + 1f, _lzq1666); SkillArea53(c + dir * r * 0.5f, r * 0.6f, dmg, (int)s.def.id);
            foreach (int i in _lzq1666.ToArray()) // copy: a hit can run a Query (Mark of Judgement payoff)
            {
                var e = _en[i]; if (!LzOk1666(e) || !PbInArc1567(e, c, dir, r, half, 0.4f)) continue; bool sh = e.shockT > 0f; Vector2 kd = (e.pos - c).normalized;
                Hit(e, dmg * T.tlDmg1666, s, Vector2.zero); LzLashHits1666++;
                if (sh && e.alive && !e.boss) { e.push += kd * T.tlKnock1666 * 8f; LzLashKnocks1666++; e.lured1651 = true; /* probe: knocked */ } // push decays at 8/s: total move = push / 8
            }
            if (LzFxLive1666("LZFX_ThunderLash")) LzFxShot1666("LZFX_ThunderLash", _heroPos + new Vector2(4f, 10f) / 16f /* v2: spec pivot = hand height */, new Vector4(-r * 16 - 10, -r * 16 - 10, r * 32 + 20, r * 32 + 20) /* v2: rim + glow reach ~42 px */, 0.33f, YSort(_heroPos.y) + 60, 0f, ZP.F("_Angle", Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg), ZP.F("_Arc", T.tlArc1666)); // HFX: 0.33 s one-shot driving _Progress
            else
            {   // PLACEHOLDER: a jagged arc of bolt segments
                Vector2 prev = c + Vector2.up * 0.6f + PbRot(dir, -half) * r * 0.3f;
                for (int k = 0; k <= 6; k++) { Vector2 p = c + Vector2.up * 0.5f + PbRot(dir, -half + k * (2f * half / 6f)) * r * (0.85f + 0.1f * Mathf.Sin(k * 2.1f)); LzBolt1666(prev, p, LC); prev = p; }
            }
            HellordAudio.Play("storm_ring", 1.8f); return true;
        }
        /// <summary>Live Wire: hold LMB = a beam locked on the demon under the aim line; after lwOverS1666 on one target it overloads (x dmg). Max hold, then release.</summary>
        bool CastLiveWire1666(SkillInst s, float dmg)
        {
            if (_lzWire1666 != null || _lzWireNeedUp1666) { s.cd = 0.05f; return false; }
            _lzWire1666 = new LzWire { s = s, tick = 0f }; HellordAudio.Play("storm_ring", 2.2f); return true;
        }
        void TickLiveWire1666(float dt)
        {
            if (_lzWireNeedUp1666 && !HInput.LmbHeld716()) _lzWireNeedUp1666 = false;
            var w = _lzWire1666; if (w == null) return; var T = TZ; var s = w.s;
            bool stillBasic = _basic1517 == s && s.def.id == SkillId.LiveWire;
            float maxHold = s.Dur(T.lwMaxHold1666); // Duration supports hold longer
            if (!stillBasic || !LzBasicHeld1666 || w.t >= maxHold) { if (w.t >= maxHold) _lzWireNeedUp1666 = true; _lzWire1666 = null; s.cd = s.def.cooldown * s.CdMul * LzBasicCdK1666(s); return; }
            w.t += dt; s.cd = 99f; // blocked while the beam is held; the real cooldown starts on release
            float range = s.R(T.lwRange1666); Vector2 o = HeroCenter, dir = PbAim1567(), end = o + dir * range;
            // keep the lock while the target stays near the aim line + in range; else re-lock on the closest body along the line
            if (w.tgt != null && (!LzOk1666(w.tgt) || (w.tgt.pos - _heroPos).magnitude > range + 0.5f || PbSegDist(BodyCentre1595(w.tgt), o, end) > T.lwWidth1666 + w.tgt.radius + 0.4f)) { w.tgt = null; w.tgtT = 0f; }
            if (w.tgt == null)
            {
                float bestA = 1e9f; Query(o + dir * range * 0.5f, range * 0.5f + 1f, _lzq1666);
                foreach (int i in _lzq1666) { var e = _en[i]; if (!LzOk1666(e)) continue; Vector2 bc = BodyCentre1595(e); float al = Vector2.Dot(bc - o, dir); if (al < 0f || al > range + e.radius) continue; if (PbSegDist(bc, o, end) > T.lwWidth1666 + e.radius) continue; if (al < bestA) { bestA = al; w.tgt = e; } }
                w.tgtT = 0f;
            }
            if (w.tgt != null) { end = BodyCentre1595(w.tgt); w.tgtT += dt; }
            w.end = end; w.tick -= dt;
            if (w.tgt != null && w.tick <= 0f)
            {
                w.tick = T.lwTick1666; bool over = w.tgtT >= T.lwOverS1666; float d = SkillDamage(s) * T.lwDmg1666 * (over ? T.lwOverMul1666 : 1f);
                Hit(w.tgt, d, s, Vector2.zero); LzWireTicks1666++; if (w.tgt != null) LzPlay1669("wireHit", BodyCentre1595(w.tgt)); if (over) LzWireOverloads1666++; _lastCast[(int)s.def.id] = Time.time;
                if (LzDrumOut1666 && PbSegDist(_lzDrum1666.p + Vector2.up * 0.5f, o, end) < T.tdHitR1666) LzDrumBoom1666();
            }
            bool overNow = w.tgt != null && w.tgtT >= T.lwOverS1666; int ord = YSort(_heroPos.y) + 80;
            float ovl = overNow ? Mathf.Clamp01((w.tgtT - T.lwOverS1666) / 0.15f) : 0f;
            if (LzBoltFx1666("LZFX_LiveWire", o, end, ord, w.t, 0.5f, ovl)) { LzFx1666("LZFX_LiveWire_Hand", o, -9, -9, 18, 18, ord + 1, w.t, 0.5f); if (overNow && w.tgt != null && w.tgt.alive) LzFx1666("LZFX_LiveWire_Overload", w.tgt.pos, -18, -6, 36, 46, (w.tgt.sr != null ? w.tgt.sr.sortingOrder : YSort(w.tgt.pos.y)) + 1, w.tgtT, 0.6f, ZP.F("_Intensity", ovl)); } // HFX endpoint beam + hand glow (+ t1669 overload body aura)
            else
            {   // PLACEHOLDER: a flickering bolt (white-yellow when overloaded) + a contact spark
                if (Mathf.Repeat(w.t, 0.06f) < 0.03f || overNow) LzBolt1666(o, end, overNow ? (Color?)LY : LC);
                ZSegO1618(o, end, overNow ? 0.08f : 0.05f, new Color(LC.r, LC.g, LC.b, 0.55f), ord); if (w.tgt != null) ZDiscO1618(end, overNow ? 0.28f : 0.18f, overNow ? (Color)LY : LW, ord + 1);
            }
        }
        /// <summary>Thunder Hound: a lightning hound appears at the cursor, darts between demons within its leash and bites (shock). hdMax1666 alive.</summary>
        bool CastThunderHound1666(SkillInst s, float dmg)
        {
            var T = TZ; Vector2 at = PbCursor1567(9f); var h = new LzHound { p = at, home = at, life = s.Dur(T.hdLife1666), dmg = dmg * T.hdDmg1666, s = s, seed = Random.value * 10f, bite = 0.1f };
            _lzHounds1666.Add(h); while (_lzHounds1666.Count > Mathf.Max(1, T.hdMax1666)) _lzHounds1666.RemoveAt(0); LzHoundsMade1666++;
            if (!LzPlay1669("houndArrive", at)) LzSky1666(at); HellordAudio.Play("storm_ring", 1.3f); return true;
        }
        void TickHounds1666(float dt)
        {
            var T = TZ;
            for (int i = _lzHounds1666.Count - 1; i >= 0; i--)
            {
                var h = _lzHounds1666[i]; h.t += dt; if (h.t >= h.life) { HordeFX.CheapBurst(h.p + Vector2.up * 0.3f, LC, 6); _lzHounds1666.RemoveAt(i); continue; }
                float leash = h.s.R(T.hdLeash1666);
                if (h.tgt == null || !LzOk1666(h.tgt) || (h.tgt.pos - h.home).magnitude > leash + 0.5f)
                {   // next target: the closest demon in the leash it has not bitten in the last 0.8 s (spreads bites)
                    h.tgt = null; float best = 1e9f; Query(h.home, leash, _lzq1666);
                    foreach (int ei in _lzq1666) { var e = _en[ei]; if (!LzOk1666(e) || (e.pos - h.home).magnitude > leash) continue; float d = (e.pos - h.p).sqrMagnitude + (h.bitAt.TryGetValue(e.id, out var bt) && runTime - bt < 0.8f ? 100f : 0f); if (d < best) { best = d; h.tgt = e; } }
                }
                Vector2 goal = h.tgt != null ? h.tgt.pos : h.home + new Vector2(Mathf.Cos(h.t * 2f + h.seed), Mathf.Sin(h.t * 2f + h.seed) * 0.6f) * 0.6f;
                Vector2 hp0 = h.p, dv = goal - h.p; float dm = dv.magnitude; h.moving = dm > 0.05f; if (dm > 0.01f) { h.face = dv / dm; h.p += h.face * Mathf.Min(dm, T.hdSpeed1666 * dt * (h.tgt != null ? 1f : 0.3f)); }
                if ((h.p - h.home).magnitude > leash) h.p = h.home + (h.p - h.home).normalized * leash; if (dt > 0f) h.vel = (h.p - hp0) / dt;
                h.bite -= dt;
                if (h.tgt != null && h.bite <= 0f && (h.tgt.pos - h.p).magnitude <= T.hdReach1666 + h.tgt.radius)
                {
                    h.bite = T.hdBite1666; h.bitT = h.t; float d = SkillDamage(h.s) * T.hdDmg1666; Hit(h.tgt, d, h.s, h.face * 1.2f); LzBites1666++; h.bitAt[h.tgt.id] = runTime; _lastCast[(int)h.s.def.id] = Time.time;
                    if (!LzPlay1669("houndBite", h.p, 1f, 0f, h.face.x < 0f, null, new[] { ZP.V("_Dir", new Vector2(h.face.x < 0f ? -1f : 1f, 0f)) })) LzBolt1666(h.p + Vector2.up * 0.3f, BodyCentre1595(h.tgt), LC); h.tgt = null;
                }
                LzDrawHound1666(h);
            }
        }
        void LzDrawHound1666(LzHound h)
        {
            int ord = YSort(h.p.y); bool biting = h.t - h.bitT < 0.5f; string an = biting ? "bite" : h.moving ? "run" : "idle";
            LzFx1666("LZFX_Hound_Aura", h.p, -34, -24, 68, 56, ord + 1, h.t, h.seed % 1f, ZP.V("_Vel", h.vel * 16f), ZP.F("_Intensity", Mathf.Clamp01((h.life - h.t) / 0.3f))); // t1669 HFX aura over the HA sprite
            bool flip = h.face.x < 0f; string dir = Mathf.Abs(h.face.x) > Mathf.Abs(h.face.y) * 1.1f ? "east" : h.face.y > 0f ? "north" : "south";
            var a = LzHoundAnim1666(an, dir);
            if (a != null && a.f.Length > 0)
            {   // HA sheet (bite plays once from the bite, idle / run loop)
                float at = biting ? h.t - h.bitT : h.t; int fi = (int)(at * a.fps); fi = biting ? Mathf.Min(fi, a.f.Length - 1) : fi % a.f.Length; float fa = Mathf.Clamp01((h.life - h.t) / 0.3f);
                ZDiscO1618(h.p, 0.32f, new Color(0, 0, 0, 0.28f * fa), ord - 2, 0.4f);
                var sr = Z37SR(a.f[fi], ZSnap1618(h.p), Vector2.one, 0f, new Color(1f, 1f, 1f, fa), ord - Z37Order); sr.flipX = flip; return;
            }
            // PLACEHOLDER: a small bright quadruped silhouette made of discs + a crackle
            float bob = Mathf.Abs(Mathf.Sin(h.t * 14f)) * 0.08f, fx = h.face.x < 0f ? -1f : 1f; float fade = Mathf.Clamp01((h.life - h.t) / 0.3f);
            ZDiscO1618(h.p, 0.3f, new Color(0, 0, 0, 0.3f * fade), ord - 2, 0.4f);
            ZDiscO1618(h.p + new Vector2(0f, 0.32f + bob), 0.26f, new Color(LB.r, LB.g, LB.b, fade), ord, 0.7f);
            ZDiscO1618(h.p + new Vector2(0.28f * fx, 0.5f + bob), 0.15f, new Color(LC.r, LC.g, LC.b, fade), ord + 1);
            ZPx1618(h.p + new Vector2(0.34f * fx, 0.53f + bob), 1, new Color(LY.r, LY.g, LY.b, fade), ord + 2);
            ZSegO1618(h.p + new Vector2(-0.25f * fx, 0.4f + bob), h.p + new Vector2(-0.45f * fx, 0.62f + bob), 0.07f, new Color(LC.r, LC.g, LC.b, fade), ord + 1);
            for (int k = 0; k < 4; k++) { float lx = (k < 2 ? 0.15f : -0.15f) * fx + (k % 2 == 0 ? 0.04f : -0.04f); ZSegO1618(h.p + new Vector2(lx, 0.25f + bob), h.p + new Vector2(lx + Mathf.Sin(h.t * 14f + k) * 0.06f, 0.02f), 0.05f, new Color(LD.r, LD.g, LD.b, fade), ord - 1); }
            if (Mathf.Repeat(h.t + h.seed, 0.25f) < 0.05f) LzBolt1666(h.p + new Vector2(-0.2f, 0.5f), h.p + new Vector2(0.2f, 0.25f), LC);
        }
        void TickLzBasics1666(float dt) { TickLiveWire1666(dt); TickHounds1666(dt); DrawDrumPips1666(); }
        /// <summary>BasicCast1517 after a real cast: a lightning basic aimed at the drum strikes it.</summary>
        void LzAfterBasicCast1666(SkillInst s)
        {
            if (!LzOn1666 || s == null || !LzDrumOut1666) return; var a = TZ.castLightningIds1567; if (a == null || System.Array.IndexOf(a, s.def.id) < 0 || s.def.id == SkillId.LiveWire) return;
            if ((MouseWorld() - (_lzDrum1666.p + Vector2.up * 0.5f)).magnitude <= TZ.tdHitR1666 + 0.3f) LzDrumBoom1666();
        }
        /// <summary>hero move speed x: Live Wire held (slower), Storm Stride (faster).</summary>
        float LzMoveMul1666() { float m = 1f; if (_lzWire1666 != null) m *= TZ.lwMoveMul1666; if (LzStrideT1666 > 0f && LzOn1666) m *= 1f + TZ.ssMove1666; return m; }
        /// <summary>basic cooldown x: Storm Stride attack speed.</summary>
        float LzBasicCdK1666(SkillInst s) => LzStrideT1666 > 0f && LzOn1666 && s != null && s == _basic1517 ? 1f / (1f + TZ.ssAtk1666) : 1f;

        // =============================================================== MOVEMENT (SPACE layer on Dash49; own cooldown ZhMoveCd; no damage)
        static bool LzLightning1666(SkillInst s) => s != null && s.def != null && (s.def.tags & STag.Lightning) != 0;
        /// <summary>top of ZhDashStart1618 (a real dash start, before the hero moves).</summary>
        void LzDashStart1666(Vector2 p0)
        {
            if (!LzOn1666) return; int m = LzMove1666; if (m == 0 || ZhMoveCd > 0f) return; // on cooldown = plain dash
            LzCasts1666++; LzComboMark1666(1); int k = LzK1666(0, m);
            if (m == 1) { _lzBlinkPend1666 = true; ZhMoveCd = ZkCd1651(k, TZ.tsCd1666); }
            else if (m == 2) { LzWardT1666 = ZkDur1651(k, TZ.gwDur1666); LzWardN1666 = Mathf.Max(1, TZ.gwCharges1666); ZhMoveCd = ZkCd1651(k, TZ.gwCd1666); HellordAudio.Play("curse_rune_arm", 1.6f); LzFxO1666(3, p0, 1.1f, 0.3f); }
            else if (m == 3) { LzStrideT1666 = ZkDur1651(k, TZ.ssDur1666); ZhMoveCd = ZkCd1651(k, TZ.ssCd1666); HellordAudio.Play("storm_ring", 2.4f); }
            else if (m == 4)
            {
                ZhMoveCd = ZkCd1651(k, TZ.mfCd1666); float R = ZkR1651(k, TZ.mfR1666); Query(p0, R, _lzq1666);
                foreach (int i in _lzq1666) { var e = _en[i]; if (!LzOk1666(e) || (e.pos - p0).magnitude > R + e.radius) continue; float st = e.boss ? TZ.mfBoss1666 : ZkDur1651(k, TZ.mfDazzle1666); e.stun53 = Mathf.Max(e.stun53, st); LzDazzled1666++; e.lured1651 = true; /* probe */ e.lzDazzT1669 = runTime + st; e.lzDazzS1669 = st; /* t1669 FX */ if (!LzFxLive1666("LZFX_MirrorDazzled")) LzFxO1666(4, BodyCentre1595(e) + Vector2.up * 0.4f, 0.4f, Mathf.Min(st, 0.6f)); }
                LzFxO1666(2, p0 + Vector2.up * 0.6f, R * 0.5f, 0.22f, "LZFX_MirrorFlash", new Vector4(-R * 16, -R * 10, R * 32, R * 24)); LzFxShot1666("LZFX_MirrorFlash_Glints", p0, new Vector4(-68, -40, 136, 80), 0.5f, FbGround + YSort(p0.y) + 40); HellordAudio.Play("pact_unlock", 1.8f);
            }
        }
        /// <summary>Dash49 after the dash started (direction set): Thunder Step turns it into an instant blink (to a Ball Lightning orb near the cursor if any).</summary>
        void LzDashAfter1666()
        {
            if (!_lzBlinkPend1666) return; _lzBlinkPend1666 = false; if (!LzOn1666) return;
            Vector2 from = _heroPos, to = _heroPos + _dashDir49 * T49.dashDistU49 * TZ.tsDistMul1666; Vector2 cur = MouseWorld(); float best = TZ.tsOrbR1666;
            foreach (var o in _pbOrbs) { if (o.die >= 0f) continue; float d = (o.p - cur).magnitude; if (d <= best) { best = d; to = o.p; } }
            ClampToRoom(ref to, HeroMarginPx); _heroPos = to; _dashT49 = 0f; LzBlinks1666++; LzBlinkFrom1666 = from; LzBlinkTo1666 = to;
            LzBolt1666(from + Vector2.up * 0.6f, to + Vector2.up * 0.6f, LW); bool tsFx = LzFxLive1666("LZFX_ThunderStep"); LzFxO1666(2, tsFx ? from : from + Vector2.up * 0.5f, 0.5f, tsFx ? 0.4f : 0.2f, "LZFX_ThunderStep", new Vector4(-16, -10, 32, 46), YSort(from.y) + 60);
            if (LzFxLive1666("LZFX_ThunderStep_In")) { LzFxShot1666("LZFX_ThunderStep_In", to, new Vector4(-16, -10, 32, 50), 0.4f, YSort(to.y) + 60, 0.15f); LzBoltShot1666("LZFX_ThunderStep_Path", from + Vector2.up * 10f / 16f, to + Vector2.up * 10f / 16f, 0.25f, YSort(to.y) + 60, 0.15f); } else LzFxO1666(2, to + Vector2.up * 0.5f, 0.6f, 0.25f);
            HellordAudio.Play("storm_ring", 2.6f);
        }
        /// <summary>HurtHero (top): Grounding Ward absorbs the hit and shocks the attacker (the contact demon, else the nearest within 6 u).</summary>
        bool LzWardAbsorb1666(bool contact)
        {
            if (!LzOn1666 || LzWardT1666 <= 0f || LzWardN1666 <= 0) return false;
            LzWardN1666--; LzWardAbsorbs1666++; _lzWardPulse1666 = runTime; _iframe = Mathf.Max(_iframe, 0.25f); var k = LzK1666(0, 2);
            Enemy a = contact && _hurtBy53 != null && LzOk1666(_hurtBy53) ? _hurtBy53 : null;
            if (a == null) { int ni = Nearest(_heroPos, 6f, -1); if (ni >= 0) a = _en[ni]; }
            if (a != null) { ApplyStatus(a, Status.Shock, ZkHit1651(k), 1f, ZkI1651(k)); a.lured1651 = true; if (!LzLink1669("wardZap", _heroPos, BodyCentre1595(a))) LzBolt1666(HeroCenter, BodyCentre1595(a), LC); }
            LzFxO1666(0, _heroPos, 1.2f, 0.3f); HellordAudio.Play("storm_ring", 2.8f); if (LzWardN1666 <= 0) LzWardT1666 = 0f;
            return true;
        }
        void LzDrawBuffs1666()
        {
            int o = YSort(_heroPos.y) + 30;
            if (LzWardT1666 > 0f && LzWardN1666 > 0) { float pul = 0.9f + 0.1f * Mathf.Sin(runTime * 12f); if (!LzFx1666("LZFX_GroundingWard", _heroPos, -22, -6, 44, 34, ZhHeroOrd1625 + 1, runTime, 0.2f, ZP.F("_Charges", LzWardN1666), ZP.F("_Pulse", Mathf.Clamp01(1f - (runTime - _lzWardPulse1666) / 0.25f)))) { ZRingO1618(_heroPos + Vector2.up * 0.6f, 0.75f * pul, new Color(LC.r, LC.g, LC.b, 0.7f), o + 260); for (int i = 0; i < LzWardN1666; i++) ZDiscO1618(_heroPos + new Vector2(-0.15f + i * 0.3f, 1.75f), 0.08f, LC, o + 261); } }
            if (LzStrideT1666 > 0f && !LzFx1666("LZFX_StormStride", _heroPos, -30, -6, 50, 32, ZhHeroOrd1625 + 1, runTime, 0.3f, ZP.V("_Vel", _heroVel37 * 16f), ZP.F("_Intensity", Mathf.Clamp01(LzStrideT1666 / 0.3f)))) { if (Mathf.Repeat(runTime, 0.08f) < 0.04f) LzBolt1666(_heroPos + new Vector2(-0.3f, 0.1f), _heroPos + new Vector2(0.3f, 0.05f), LC); ZDiscO1618(_heroPos, 0.45f, new Color(LC.r, LC.g, LC.b, 0.25f), FbGround + YSort(_heroPos.y) + 4, 0.45f); }
        }

        // =============================================================== SPECIALS (RMB, ZhSpecCd)
        void LzSpecial1666()
        {
            int sp = LzSpec1666; if (sp == 0) return; if (ZhSpecCd > 0f) { HellordAudio.Play("basic_not_ready", 0.7f); return; }
            int k = LzK1666(1, sp); var T = TZ; Vector2 cur = MouseWorld(); ClampToRoom(ref cur, 4);
            if (sp == 1)
            {
                LzPick1666(cur, ZkR1651(k, T.mjPickR1666), out var e); if (e == null) { HellordAudio.Play("basic_not_ready", 0.7f); return; }
                LzMark1666 = e; _lzMarkId1666 = e.id; LzMarkT1666 = ZkDur1651(k, T.mjLife1666); LzMarkCharge1666 = 0f; LzMarkHits1666 = 0; ZhSpecCd = ZkCd1651(k, T.mjCd1666); HellordAudio.Play("curse_rune_arm", 1.2f); LzSky1666(e.pos);
            }
            else if (sp == 2) { _lzCond1666.Add(new LzCond { p = cur, r = ZkR1651(k, T.cdR1666), dur = ZkDur1651(k, T.cdDur1666) }); ZhSpecCd = ZkCd1651(k, T.cdCd1666); HellordAudio.Play("curse_rune_arm", 0.9f); }
            else if (sp == 3) { _lzDrum1666 = new LzDrum { p = cur, life = ZkDur1651(k, T.tdLife1666) }; ZhSpecCd = ZkCd1651(k, T.tdCd1666); HellordAudio.Play("fire_pillar_telegraph", 1.4f); LzFxO1666(0, cur, 1f, 0.3f); }
            else if (sp == 4) { LzStaticField1666(k); ZhSpecCd = ZkCd1651(k, T.sfCd1666); }
            LzCasts1666++; LzComboMark1666(2);
        }
        // ---- Mark of Judgement
        void TickLzMark1666(float dt)
        {
            if (LzMark1666 == null) return; var e = LzMark1666; LzMarkT1666 -= dt;
            if (!LzOk1666(e) || e.id != _lzMarkId1666 || LzMarkT1666 <= 0f) { LzMark1666 = null; return; } // the demon died or the mark ran out: lost, no payoff
            float need = e.boss ? Mathf.Max(1, TZ.mjBossHits1666) : TZ.mjThresh1666 * e.maxHp, have = e.boss ? LzMarkHits1666 : LzMarkCharge1666; float f = Mathf.Clamp01(have / Mathf.Max(1e-3f, need));
            Vector2 top = e.pos + Vector2.up * (e.boss ? BossHeadY + 0.4f : e.hitY * 2f + 0.5f); int o = YSort(e.pos.y) + 300;
            if (!LzFx1666("LZFX_MarkOfJudgement", top, -12, -12, 24, 26, o, runTime, 0.1f, ZP.F("_Charge", f)))
            { ZRingO1618(top, 0.32f, new Color(LY.r, LY.g, LY.b, 0.9f), o); ZDiscO1618(top, 0.08f + 0.2f * f, new Color(LW.r, LW.g, LW.b, 0.6f + 0.4f * f), o + 1); ZSegO1618(top + new Vector2(-0.18f, 0.18f), top + new Vector2(0.18f, -0.18f), 0.06f, LY, o + 2); }
        }
        void LzMarkPay1666()
        {
            var e = LzMark1666; LzMark1666 = null; if (e == null) return; int k = LzK1666(1, 1); var s = ZkI1651(k); var T = TZ; Vector2 bc = BodyCentre1595(e); LzMarkPays1666++;
            if (!LzStrike1669("Judgement", e.pos)) LzSky1666(e.pos); ScreenShake.Add(2f, 0.15f); HellordAudio.Play("pact_unlock", 1.1f); if (LzOk1666(e)) Hit(e, ZkHit1651(k) * T.mjDmg1666, s, Vector2.zero);
            Query(bc, ZkR1651(k, T.mjArcR1666), _lzq1666); int n = 0; var tg = new List<Enemy>();
            foreach (int i in _lzq1666) { var o = _en[i]; if (o == e || !LzOk1666(o) || o.shockT <= 0f || (o.pos - e.pos).magnitude > ZkR1651(k, T.mjArcR1666)) continue; tg.Add(o); if (++n >= T.mjArcN1666) break; }
            int ai = 0; foreach (var o in tg) { if (!LzLink1669("judgeArc", bc, BodyCentre1595(o), ai++)) LzBolt1666(bc, BodyCentre1595(o), LY); Hit(o, ZkHit1651(k) * T.mjArcDmg1666, s, Vector2.zero); }
        }
        // ---- Conductivity
        void TickLzCond1666(float dt)
        {
            for (int i = _lzCond1666.Count - 1; i >= 0; i--)
            {
                var c = _lzCond1666[i]; c.t += dt; float delay = TZ.cdDelay1666; int o = FbGround + YSort(c.p.y) + 5;
                if (!c.done && c.t >= delay)
                {
                    c.done = true; Query(c.p, c.r, _lzq1666); LzFxO1666(0, c.p, c.r, 0.3f);
                    foreach (int ei in _lzq1666) { var e = _en[ei]; if (!LzOk1666(e) || !InGroundOval(c.p, e.pos, c.r, e.radius)) continue; e.lzCond1666 = runTime + c.dur; LzCursed1666++; e.lured1651 = true; /* probe */ if (e.shockT > 0f) e.shockAmp = Mathf.Min(TZ.cdAmpCap1666, Mathf.Max(e.shockAmp, TZ.cdAmp1666)); }
                    HellordAudio.Play("storm_ring", 0.8f);
                }
                if (c.t >= delay + 0.4f) { _lzCond1666.RemoveAt(i); continue; }
                float k = Mathf.Clamp01(c.t / delay);
                var zr = LzZoneRc1666(c.r);
                if (!LzFx1666("LZFX_Conductivity", c.p, zr.x, zr.y, zr.z, zr.w, o, c.t, 0.4f, ZP.F("_Progress", k), ZP.F("_Ignite", k), ZP.F("_Fade", Mathf.Clamp01((c.t - delay) / 0.4f)), ZP.F("_Radius", Mathf.Round(c.r * 16f))))
                { ZRingO1618(c.p, c.r, new Color(LP.r, LP.g, LP.b, c.done ? 0.3f : 0.4f + 0.4f * k), o, 0.6f); ZDiscO1618(c.p, c.r * k, new Color(LP.r, LP.g, LP.b, 0.18f), o - 1, 0.6f); }
            }
            // cursed demons: a small violet rune under them (placeholder)
            for (int i = 0; i < maxEnemies; i++) { var e = _en[i]; if (!e.alive || e.lzCond1666 <= runTime) continue; if (LzFx1666("LZFX_Conductivity_Cursed", e.pos, -18, -6, 36, 52, (e.sr != null ? e.sr.sortingOrder : YSort(e.pos.y)) + 1, runTime, (e.id * 0.137f) % 1f, ZP.F("_Intensity", Mathf.Clamp01((e.lzCond1666 - runTime) / 0.3f)))) continue; ZRingO1618(e.pos, 0.35f + e.radius * 0.5f, new Color(LP.r, LP.g, LP.b, 0.75f), FbGround + YSort(e.pos.y) + 6, 0.5f); }
        }
        /// <summary>ApplyStatus shock case (after it set shockT / shockAmp): Conductivity (stronger + longer + spreads once) and Tribunal energy.</summary>
        void LzOnShock1666(Enemy e, SkillInst src)
        {
            if (!LzOn1666) return; var T = TZ;
            if (e.lzCond1666 > runTime)
            {
                e.shockAmp = Mathf.Min(T.cdAmpCap1666, Mathf.Max(e.shockAmp, T.cdAmp1666)); e.shockT *= T.cdShockDur1666;
                if (!_lzSpreading1666)
                {
                    Enemy best = null; float bd = T.cdSpreadR1666; Query(e.pos, bd, _lzq1666);
                    foreach (int i in _lzq1666) { var o = _en[i]; if (o == e || !LzOk1666(o) || o.lzCond1666 <= runTime || o.shockT > 0f) continue; float d = (o.pos - e.pos).magnitude; if (d <= bd) { bd = d; best = o; } }
                    if (best != null) { _lzSpreading1666 = true; try { ApplyStatus(best, Status.Shock, 0f, 1f, src); } finally { _lzSpreading1666 = false; } LzSpreads1666++; if (!LzLink1669("condJump", BodyCentre1595(e), BodyCentre1595(best))) LzBolt1666(BodyCentre1595(e), BodyCentre1595(best), LP); }
                }
            }
            if (LzUlt1666 == 3 && LzTribT1666 <= 0f && LzEnergy1666 < T.trNeed1666) LzEnergy1666 = Mathf.Min(T.trNeed1666, LzEnergy1666 + (e.boss ? T.trBoss1666 : e.elite ? T.trElite1666 : T.trNormal1666));
        }
        // ---- Thunder Ministry Drum
        void TickLzDrum1666(float dt)
        {
            var d = _lzDrum1666; if (d == null) return; d.t += dt; if (d.t >= d.life || d.booms >= Mathf.Max(1, TZ.tdBooms1666)) { HordeFX.CheapBurst(d.p + Vector2.up * 0.5f, LY, 10); _lzDrum1666 = null; return; }
            int o = YSort(d.p.y); float hit = Mathf.Clamp01(1f - (runTime - d.last) / 0.2f);
            if (!LzFx1666("LZFX_ThunderDrum", d.p, -24, -8, 48, 48, o, d.t, 0.8f, ZP.F("_Hit", hit), ZP.F("_Booms", d.booms), ZP.F("_Break", Mathf.Clamp01((d.t - (d.life - 0.3f)) / 0.3f)), ZP.F("_Life", 1f - d.t / d.life)))
            {   // PLACEHOLDER: a drum on a stand (dark body, yellow rim, flashes when struck)
                ZDiscO1618(d.p, 0.45f, new Color(0, 0, 0, 0.35f), o - 2, 0.4f); ZSegO1618(d.p + new Vector2(-0.25f, 0f), d.p + new Vector2(-0.2f, 0.4f), 0.07f, LK, o - 1); ZSegO1618(d.p + new Vector2(0.25f, 0f), d.p + new Vector2(0.2f, 0.4f), 0.07f, LK, o - 1);
                ZDiscO1618(d.p + Vector2.up * 0.62f, 0.36f, Color.Lerp(LP, LW, hit), o); ZRingO1618(d.p + Vector2.up * 0.62f, 0.37f, LY, o + 1); ZRingO1618(d.p + Vector2.up * 0.62f, 0.2f, new Color(LY.r, LY.g, LY.b, 0.6f), o + 2);
            }
        }
        void LzDrumBoom1666()
        {
            var d = _lzDrum1666; if (d == null || runTime - d.last < TZ.tdGap1666) return; d.last = runTime; d.booms++; LzDrumBooms1666++;
            int k = LzK1666(1, 3); var s = ZkI1651(k); float R = ZkR1651(k, TZ.tdR1666); Query(d.p, R, _lzq1666); SkillArea53(d.p, R, ZkHit1651(k), (int)s.def.id, true);
            var tg = new List<Enemy>(); foreach (int i in _lzq1666) { var e = _en[i]; if (LzOk1666(e) && InGroundOval(d.p, e.pos, R, e.radius)) tg.Add(e); }
            foreach (var e in tg) Hit(e, ZkHit1651(k) * TZ.tdDmg1666, s, (e.pos - d.p).normalized * 2f);
            LzFxO1666(0, d.p, R, LzFxLive1666("LZFX_DrumBoom") ? 0.6f : 0.35f, "LZFX_DrumBoom", LzWaveRc1666(R)); LzPlay1669("boomHaze", d.p, R * 16f / 45f); HellordAudio.Play("storm_ring", 0.7f); ScreenShake.Add(1f, 0.1f);
        }
        // ---- Static Field
        void LzStaticField1666(int k)
        {
            var s = ZkI1651(k); float R = ZkR1651(k, TZ.sfR1666); Vector2 c = _heroPos; Query(c, R, _lzq1666); var tg = new List<Enemy>();
            foreach (int i in _lzq1666) { var e = _en[i]; if (LzOk1666(e) && InGroundOval(c, e.pos, R, e.radius)) tg.Add(e); }
            foreach (var e in tg)
            {
                float floor = TZ.sfFloor1666 * e.maxHp, share = (e.boss || e.elite ? TZ.sfBig1666 : TZ.sfNormal1666) * e.hp * s.More(); /* damage supports scale the share; the floor still holds */ float dmg = Mathf.Min(share, e.hp - floor);
                if (dmg <= 0f) { ApplyStatus(e, Status.Shock, 0f, 1f, s); continue; }
                LzFloorHp1666 = floor; try { Hit(e, dmg, s, Vector2.zero); } finally { LzFloorHp1666 = -1f; } LzStaticHits1666++; if (!LzFxLive1666("LZFX_StaticField")) LzBolt1666(HeroCenter, BodyCentre1595(e), LC); /* t1669: the HFX field + core carry it */
            }
            { float Rp = Mathf.Round(R * 16f), hh = Mathf.Ceil((Rp + 16f) * HellFX.GroundYSquash); LzFxO1666(0, c, R, 0.4f, "LZFX_StaticField", new Vector4(-(Rp + 20), -(hh + 4), 2 * (Rp + 20), 2 * hh + 12)); LzPlay1669("sfCore", c); } HellordAudio.Play("storm_ring", 1.1f);
        }
        /// <summary>Hit (after Damage): basic-hit combo part, Mark of Judgement charge, the drum struck by lightning near it.</summary>
        void LzPostHit1666(Enemy e, float dmg, SkillInst s, bool tick)
        {
            if (!LzOn1666 || s == null) return;
            if (!tick && s == _basic1517) LzComboMark1666(0);
            if (LzMark1666 != null && e == LzMark1666 && LzLightning1666(s) && s.def.id != SkillId.LzMarkOfJudgement) { LzMarkCharge1666 += dmg * (e.shockT > 0f ? 1f + e.shockAmp : 1f); LzMarkHits1666++; float need = e.boss ? Mathf.Max(1, TZ.mjBossHits1666) : TZ.mjThresh1666 * e.maxHp; if ((e.boss ? LzMarkHits1666 : LzMarkCharge1666) >= need) LzMarkPay1666(); else if (!e.alive) LzMark1666 = null; } // charged on the killing hit = still pays (arcs from where it fell)
            if (LzDrumOut1666 && LzLightning1666(s) && s.def.id != SkillId.LzThunderDrum && (e.pos - _lzDrum1666.p).magnitude <= TZ.tdHitR1666) LzDrumBoom1666();
        }
        /// <summary>KillEnemy (top): Heaven-Splitting Arc infusion drops from shocked demons.</summary>
        void LzOnKill1666(Enemy e)
        {
            if (!LzOn1666) return;
            if (LzUlt1666 == 2 && e.shockT > 0f && LzInfusions1666 + _lzInf1666.Count < TZ.haNeed1666 && Random.value < (e.elite || e.boss ? TZ.haEliteDrop1666 : TZ.haNormalDrop1666)) { _lzInf1666.Add(new LzInf { p = e.pos }); LzInfDropped1666++; }
        }

        // =============================================================== ULTIMATES (R, ZhUltCd; meters carry over rooms)
        public bool LzUltArmed1666
        {
            get
            {
                if (ZhUltCd > 0f) return false; var T = TZ;
                switch (LzUlt1666) { case 1: return LzCombos1666 >= Mathf.Max(1, T.dcNeed1666); case 2: return LzInfusions1666 >= Mathf.Max(1, T.haNeed1666); case 3: return LzEnergy1666 >= T.trNeed1666 - 0.01f && LzTribT1666 <= 0f; case 4: return LzWarpTarget1666() != null; default: return false; }
            }
        }
        /// <summary>0..1 meter of the slotted ultimate (kit bar).</summary>
        public float LzUltProg1666()
        {
            var T = TZ;
            switch (LzUlt1666) { case 1: return Mathf.Clamp01((float)LzCombos1666 / Mathf.Max(1, T.dcNeed1666)); case 2: return Mathf.Clamp01((float)LzInfusions1666 / Mathf.Max(1, T.haNeed1666)); case 3: return LzTribT1666 > 0f ? 1f : Mathf.Clamp01(LzEnergy1666 / Mathf.Max(1f, T.trNeed1666)); case 4: return LzWarpTarget1666() != null ? 1f : BossAlive ? Mathf.Clamp01(LzBossT1666 / Mathf.Max(1f, T.jwBossFallback1666)) : 0f; default: return 0f; }
        }
        void LzUltimate1666()
        {
            int u = LzUlt1666; if (u == 0) return; if (!LzUltArmed1666) { HellordAudio.Play("basic_not_ready", 0.6f); return; }
            int k = LzK1666(2, u); var T = TZ; LzCasts1666++;
            if (u == 1) { _lzDecree1666.Add(new LzDecreeS()); LzCombos1666 = 0; ZhUltCd = ZkCd1651(k, T.dcCd1666); HellordAudio.Play("oxhead_roar", 1.5f); Toast("DECREE OF THE 24 THUNDER LORDS"); }
            else if (u == 2) { LzInfusions1666 = 0; ZhUltCd = ZkCd1651(k, T.haCd1666); LzHeavenArc1666(k); Toast("HEAVEN-SPLITTING ARC"); }
            else if (u == 3) { LzEnergy1666 = 0f; LzTribT1666 = ZkDur1651(k, T.trDur1666); _lzTribDur1669 = LzTribT1666; _lzTribBolt1669 = -9f; _lzTribNext1666 = 0f; ZhUltCd = ZkCd1651(k, T.trCd1666); HellordAudio.Play("oxhead_roar", 1.2f); Toast("TRIBUNAL OF THE NINE HEAVENS"); }
            else if (u == 4) { var e = LzWarpTarget1666(); if (e == null) return; LzWarp1666(k, e); ZhUltCd = ZkCd1651(k, T.jwCd1666); Toast("JUDGEMENT WARP"); }
        }
        // ---- Decree: combo = a basic hit + the movement layer + a special inside dcWindow1666
        void LzComboMark1666(int part)
        {
            if (!LzOn1666 || LzUlt1666 != 1 || part < 0 || part > 2) return; var T = TZ; if (LzCombos1666 >= Mathf.Max(1, T.dcNeed1666)) return;
            _lzComboT1666[part] = runTime; _lzComboLast1666 = runTime;
            float lo = Mathf.Min(_lzComboT1666[0], Mathf.Min(_lzComboT1666[1], _lzComboT1666[2]));
            if (runTime - lo <= T.dcWindow1666) { LzCombos1666++; for (int i = 0; i < 3; i++) _lzComboT1666[i] = -99f; LzFxO1666(0, _heroPos, 1.4f, 0.35f); HellordAudio.Play("pact_unlock", 1.4f + 0.2f * LzCombos1666); }
        }
        void TickLzCombo1666()
        {   // unarmed combos decay after dcDecay1666 s without progress; armed ones stay
            var T = TZ; if (LzUlt1666 != 1 || LzCombos1666 <= 0 || LzCombos1666 >= Mathf.Max(1, T.dcNeed1666)) return;
            if (runTime - _lzComboLast1666 > T.dcDecay1666) LzCombos1666 = 0;
        }
        void TickLzDecree1666(float dt)
        {
            var T = TZ; int k = LzK1666(2, 1); var s = ZkI1651(k); int n = Mathf.Max(1, T.dcBolts1666); float gap = ZkDur1651(k, T.dcDur1666) / n;
            for (int i = _lzDecree1666.Count - 1; i >= 0; i--)
            {
                var d = _lzDecree1666[i]; d.t += dt;
                while (d.fired < n && d.t >= d.next)
                {
                    d.next += gap; d.fired++; Enemy best = LzDecreePick1666(d, T.dcRange1666, true) ?? LzDecreePick1666(d, T.dcRange1666, false); // a NEW demon first; out of new ones = re-target
                    if (best == null) continue; d.struck.Add(best.id); Vector2 at = best.pos; float R = ZkR1651(k, T.dcBoltR1666); if (!LzStrike1669("Decree", at, R)) LzSky1666(at); LzDecreeBolts1666++;
                    Query(at, R, _lzq1666); var tg = new List<Enemy>(); foreach (int ei in _lzq1666) { var e = _en[ei]; if (LzOk1666(e) && (e.pos - at).magnitude <= R + e.radius) tg.Add(e); }
                    foreach (var e in tg) Hit(e, ZkHit1651(k) * T.dcDmg1666, s, Vector2.zero); if (d.fired % 3 == 0) HellordAudio.Play("storm_ring", 1.2f + Random.value * 0.4f);
                }
                if (d.fired >= n) _lzDecree1666.RemoveAt(i);
            }
        }
        Enemy LzDecreePick1666(LzDecreeS d, float range, bool fresh)
        {
            Enemy best = null; float bs = -1f; Query(_heroPos, range, _lzq1666);
            foreach (int i in _lzq1666) { var e = _en[i]; if (!LzOk1666(e) || (e.pos - _heroPos).magnitude > range) continue; if (fresh && d.struck.Contains(e.id)) continue; float sc = Mathf.Repeat(Mathf.Sin(e.id * 12.9898f + d.fired * 78.233f) * 43758.5453f, 1f) + (e.elite || e.boss ? 0.5f : 0f); /* scattered but deterministic */ if (sc > bs) { bs = sc; best = e; } }
            return best;
        }
        // ---- Heaven-Splitting Arc
        void LzHeavenArc1666(int k)
        {
            var T = TZ; var s = ZkI1651(k); Vector2 from = HeroCenter; var hit = new HashSet<int>(); int chains = Mathf.Max(1, T.haChains1666) + s.ChainExtra; /* Chain support: more jumps */ float jr = ZkR1651(k, T.haJumpR1666);
            Vector2 cur = MouseWorld(); int first = Nearest(cur, 6f, -1); if (first < 0) first = Nearest(_heroPos, 8f, -1);
            Enemy e = first >= 0 ? _en[first] : null; HellordAudio.Play("pact_unlock", 0.8f); ScreenShake.Add(3f, 0.25f);
            if (e == null) { if (!LzLink1669("arcLink", from, cur + Vector2.up * 0.5f)) LzBolt1666(from, cur + Vector2.up * 0.5f, LW); return; }
            for (int c = 0; c < chains && e != null; c++)
            {
                Vector2 to = BodyCentre1595(e); if (LzLink1669("arcLink", from, to, c)) LzPlay1669("arcNode", to, 1f, 0.06f * c); else LzBolt1666(from, to, c % 2 == 0 ? (Color?)LW : LY); hit.Add(e.id); LzArcHits1666++;
                Vector2 gp = e.pos; Hit(e, ZkHit1651(k) * T.haDmg1666, s, Vector2.zero); _lzGround1666.Add(new LzGround { p = gp, r = ZkR1651(k, T.haGroundR1666), life = T.haGroundS1666, k = k }); /* no Duration tag: fixed */
                from = to; Enemy nx = null; float bd = jr; Query(gp, jr, _lzq1666);
                foreach (int i in _lzq1666) { var o = _en[i]; if (!LzOk1666(o) || hit.Contains(o.id)) continue; float d = (o.pos - gp).magnitude; if (d <= bd) { bd = d; nx = o; } }
                if (nx == null) foreach (int i in _lzq1666) { var o = _en[i]; if (!LzOk1666(o) || o == e) continue; float d = (o.pos - gp).magnitude; if (d <= bd) { bd = d; nx = o; } } // out of new demons: bounce back
                e = nx;
            }
        }
        void TickLzInf1666(float dt)
        {
            var T = TZ;
            for (int i = _lzInf1666.Count - 1; i >= 0; i--)
            {
                var f = _lzInf1666[i]; f.t += dt; if (f.t >= T.haLife1666) { _lzInf1666.RemoveAt(i); continue; }
                if ((f.p - _heroPos).magnitude <= T.haPickR1666) { _lzInf1666.RemoveAt(i); LzInfusions1666 = Mathf.Min(T.haNeed1666, LzInfusions1666 + 1); LzFxO1666(2, f.p + Vector2.up * 0.4f, 0.5f, 0.2f); HellordAudio.Play("pact_unlock", 1.6f + 0.15f * LzInfusions1666); continue; }
                Vector2 p = f.p + Vector2.up * (0.45f + 0.08f * Mathf.Sin(f.t * 5f)); int o = YSort(f.p.y) + 50; float blink = T.haLife1666 - f.t < 2f && Mathf.Repeat(f.t, 0.2f) < 0.1f ? 0.4f : 1f;
                if (!LzFx1666("LZFX_Infusion", p, -8, -8, 16, 16, o, f.t, 0.9f)) { ZDiscO1618(f.p, 0.2f, new Color(0, 0, 0, 0.3f), o - 2, 0.4f); ZDiscO1618(p, 0.16f, new Color(LY.r, LY.g, LY.b, blink), o); ZRingO1618(p, 0.24f, new Color(LC.r, LC.g, LC.b, 0.8f * blink), o + 1); if (Mathf.Repeat(f.t, 0.3f) < 0.05f) LzBolt1666(p + new Vector2(-0.15f, 0.2f), p + new Vector2(0.12f, -0.15f), LW); }
            }
        }
        /// <summary>shocked ground (Heaven-Splitting Arc / Judgement Warp): shocks demons standing in it (no damage).</summary>
        void TickLzGround1666(float dt)
        {
            for (int i = _lzGround1666.Count - 1; i >= 0; i--)
            {
                var g = _lzGround1666[i]; g.t += dt; if (g.t >= g.life) { _lzGround1666.RemoveAt(i); continue; } g.tick -= dt; int o = FbGround + YSort(g.p.y) + 3; float fade = Mathf.Clamp01((g.life - g.t) / 0.5f);
                if (g.tick <= 0f) { g.tick = 0.5f; Query(g.p, g.r, _lzq1666); foreach (int ei in _lzq1666) { var e = _en[ei]; if (LzOk1666(e) && (e.pos - g.p).magnitude <= g.r + e.radius) { ApplyStatus(e, Status.Shock, 0f, 1f, ZkI1651(g.k)); e.lured1651 = true; } } }
                var gr = LzZoneRc1666(g.r);
                if (!LzFx1666("LZFX_ShockedGround", g.p, gr.x, gr.y, gr.z, gr.w, o, g.t, g.p.x * 0.37f, ZP.F("_Fade", fade), ZP.F("_Radius", Mathf.Round(g.r * 16f)), ZP.F("_Ignite", Mathf.Clamp01(g.t / 0.25f))))
                { ZDiscO1618(g.p, g.r, new Color(LB.r, LB.g, LB.b, 0.22f * fade), o, 0.55f); if (Mathf.Repeat(g.t + g.p.x, 0.35f) < 0.05f) { float a = Random.value * 6.28f; LzBolt1666(g.p + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * 0.55f) * g.r * 0.8f, g.p, LC); } }
            }
        }
        // ---- Tribunal of the Nine Heavens
        void TickLzTrib1666(float dt)
        {
            if (LzTribT1666 <= 0f) return; var T = TZ; LzTribT1666 -= dt; _lzTribNext1666 -= dt; int k = LzK1666(2, 3); var s = ZkI1651(k);
            if (!LzFx1666("LZFX_Tribunal", _heroPos, -24, 40, 48, 24, YSort(_heroPos.y) + 400, LzTribT1666, 0.5f, ZP.F("_Form", Mathf.Min(Mathf.Clamp01((_lzTribDur1669 - LzTribT1666) / 0.25f), Mathf.Clamp01(LzTribT1666 / 0.3f))), ZP.F("_Pulse", Mathf.Clamp01(1f - (runTime - _lzTribBolt1669) / 0.2f)))) { float a = 0.35f + 0.15f * Mathf.Sin(runTime * 8f); ZDiscO1618(_heroPos + Vector2.up * 3.4f, 1.1f, new Color(LD.r, LD.g, LD.b, a), YSort(_heroPos.y) + 400, 0.35f); }
            if (_lzTribNext1666 > 0f) return; _lzTribNext1666 = T.trEvery1666;
            Enemy best = null; float bh = -1f; Query(_heroPos, T.trRange1666, _lzq1666); // the strongest SHOCKED demon (highest current HP); none shocked = the strongest
            foreach (int i in _lzq1666) { var e = _en[i]; if (!LzOk1666(e) || (e.pos - _heroPos).magnitude > T.trRange1666) continue; float sc = e.hp + (e.shockT > 0f ? 1e6f : 0f); if (sc > bh) { bh = sc; best = e; } }
            if (best == null) return; Vector2 at = best.pos; float R = ZkR1651(k, T.trSplash1666); if (!LzStrike1669("Tribunal", at, R)) LzSky1666(at); LzTribBolts1666++; _lzTribBolt1669 = runTime;
            Query(at, R, _lzq1666); var tg = new List<Enemy>(); foreach (int ei in _lzq1666) { var e = _en[ei]; if (LzOk1666(e) && (e.pos - at).magnitude <= R + e.radius) tg.Add(e); }
            foreach (var e in tg) Hit(e, ZkHit1651(k) * T.trDmg1666 * (e == best ? 1f : 0.5f), s, Vector2.zero); HellordAudio.Play("storm_ring", 1f);
        }
        // ---- Judgement Warp
        Enemy LzWarpTarget1666()
        {
            if (!LzOn1666 || LzUlt1666 != 4) return null; var T = TZ; Enemy best = null; float bd = T.jwSight1666;
            for (int i = 0; i < maxEnemies; i++) { var e = _en[i]; if (!LzOk1666(e) || !e.elite || e.boss || e.hp > T.jwCull1666 * e.maxHp) continue; float d = (e.pos - _heroPos).magnitude; if (d <= bd) { bd = d; best = e; } }
            if (best == null && BossAlive && LzBossT1666 >= T.jwBossFallback1666 && LzOk1666(_boss)) best = _boss;
            return best;
        }
        void TickLzWarp1666(float dt)
        {
            if (LzUlt1666 != 4) return; if (BossAlive) LzBossT1666 += dt;
            var e = LzWarpTarget1666(); if (e == null) return; Vector2 top = e.pos + Vector2.up * (e.boss ? BossHeadY + 0.6f : e.hitY * 2f + 0.6f); int o = YSort(e.pos.y) + 300;
            if (!LzFx1666("LZFX_WarpTarget", top, -8, -8, 16, 16, o, runTime, 0.2f)) { float p = 0.8f + 0.2f * Mathf.Sin(runTime * 10f); ZRingO1618(e.pos, (e.radius + 0.4f) * p, new Color(LY.r, LY.g, LY.b, 0.8f), FbGround + YSort(e.pos.y) + 7, 0.55f); ZSegO1618(top + new Vector2(0f, 0.25f), top, 0.1f, LY, o); }
        }
        void LzWarp1666(int k, Enemy e)
        {
            var T = TZ; var s = ZkI1651(k); Vector2 from = _heroPos, at = e.pos; float R = ZkR1651(k, T.jwR1666); LzWarps1666++;
            Vector2 land = at + (from - at).normalized * Mathf.Min((from - at).magnitude, e.radius + 0.4f); ClampToRoom(ref land, HeroMarginPx); _heroPos = land; _iframe = Mathf.Max(_iframe, 0.4f); _heroVel37 = Vector2.zero; _dashT49 = 0f;
            if (LzPlay1669("jwOut", from)) { float sc = R / 3f; LzPlay1669("jwIn", at, sc); LzPlay1669("jwFlash", at, sc); LzPlay1669("jwHaze", at, sc); for (int a = 0; a < 5; a++) { float an = (a * 72f + 18f) * Mathf.Deg2Rad; LzLink1669("jwArc", at, at + new Vector2(Mathf.Cos(an) * 40f, Mathf.Sin(an) * 22f) * sc / 16f, a); } } else { LzBolt1666(from + Vector2.up * 0.6f, land + Vector2.up * 0.6f, LW); LzSky1666(at); } ScreenShake.Add(4f, 0.3f); /* t1669 HFX warp timeline (spec 16) */ HitStop.Play(0.08f, 0.2f, 0.4f); HellordAudio.Play("pact_unlock", 0.7f);
            if (e.boss) { Hit(e, ZkHit1651(k) * T.jwBossDmg1666, s, Vector2.zero); LzBossT1666 = 0f; }
            else Hit(e, e.hp * 4f + 1f, s, Vector2.zero); // the cull-ready elite explodes
            Query(at, R, _lzq1666); var tg = new List<Enemy>(); foreach (int ei in _lzq1666) { var o = _en[ei]; if (o != e && LzOk1666(o) && (o.pos - at).magnitude <= R + o.radius) tg.Add(o); }
            foreach (var o in tg) Hit(o, ZkHit1651(k) * T.jwDmg1666, s, (o.pos - at).normalized * 4f);
            _lzGround1666.Add(new LzGround { p = at, r = R, life = T.jwGroundS1666, k = k }); /* no Duration tag: fixed */ if (!LzPlay1669("jwGround", at, R / 3f)) LzFxO1666(0, at, R, 0.45f);
        }
    }
}
