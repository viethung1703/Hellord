using System.Collections.Generic;
using UnityEngine;

// t737 / t770u: INFERNAL SINS phase 1 (Docs/Hellord_sins_design_v0.4.md, Decisions t729-t769).
// 70 sins (VH t818u): 25 Mortal = 13 singles + 12 pairs (stat sins, LIVE), 34 Demonic + 11 Heaven-Defying (data only, effect not implemented yet).
// Save: PlayerPrefs "hellord.pact51.sin_*" (owned bitstring, equipped slots, unlocked slots). Souls / Sigils = the pact wallet.
// NOT play-tested at runtime.
namespace Hellord
{
    public enum SinTier { Mortal = 0, Demonic = 1, HeavenDefying = 2 }
    public enum SinStat { None = -1, MaxHp, Speed, Pickup, Armor, Crit, Dmg, Fire, Cold, Lightning, Phys, Cooldown, Minion, Area, Duration, Bleed, Xp, DashCd, CurseSoul }

    /// <summary>t784u: one rolled number on a sin. Value = lo + q*(hi-lo), q in 0..1 stored per owned sin.</summary>
    public class SinMod
    {
        public SinStat stat = SinStat.None; public float lo, hi; public bool integer; public string fmt; // fmt: text with {v} placeholder (Demonic/HD) or null (Mortal stat line)
        public float Val(float q) { float v = lo + Mathf.Clamp01(q) * (hi - lo); return integer ? Mathf.Round(v) : v; }
    }
    public class SinDef
    {
        public int id; public string name, vn = ""; public SinTier tier; public int weight; public bool live; public int wIdx = -1;
        public List<SinMod> mods = new List<SinMod>(); public string effFmt; public string[] fixedLines = new string[0]; // effFmt: Demonic/HD effect line with {0},{1}.. = rolled numbers public string[] fixedLines = new string[0]; // drawbacks / notes (not rolled)
        public int W
        {
            get
            {
                var T = HellordTuning.I; int[] a = tier == SinTier.Demonic ? T.sinDemonicW737 : tier == SinTier.HeavenDefying ? T.sinHeavenDefyingW737 : T.sinSingleW737;
                if (a != null && wIdx >= 0 && wIdx < a.Length) return a[wIdx]; return weight;
            }
        }
    }

    public static class HSins
    {
        public static int Count => All.Count; // t818u: 25 Mortal + 34 Demonic + 11 HD = 70
        static List<SinDef> _all;
        public static List<SinDef> All { get { if (_all == null) Build(); return _all; } }
        public static readonly Color[] TierCol = { new Color32(0x8b, 0x9b, 0xb4, 255), new Color32(0xc0, 0x26, 0x33, 255), new Color32(0x8a, 0x3c, 0xd0, 255) };
        public static readonly string[] TierName = { "MORTAL", "DEMONIC", "HEAVEN-DEFYING" };

        static readonly string[] StatTxt = { "MAX HP", "MOVE SPEED", "PICKUP RANGE", "ARMOR", "CRIT CHANCE", "DAMAGE", "FIRE DAMAGE", "COLD DAMAGE", "LIGHTNING DAMAGE", "PHYSICAL/BONE/BLOOD DAMAGE", "COOLDOWN", "MINION DAMAGE AND HP", "AREA", "DURATION", "BLEED DAMAGE", "XP GAIN", "DASH COOLDOWN", "CURSE/SOUL DAMAGE" };
        static readonly string[] Word = { "Greed", "Killing", "Greed", "Delusion", "Slander", "Hatred", "Hatred", "Hatred", "Hatred", "Hatred", "Killing", "Idle Speech", "Wrong View", "Wrong View", "Hatred", "Theft", "Killing", "Hatred" };
        static string Num(float v, bool integer) => integer || Mathf.Abs(v - Mathf.Round(v)) < 0.01f ? Mathf.RoundToInt(v).ToString() : v.ToString("0.0");
        static bool IsPct(SinStat s) => s != SinStat.Armor;
        /// <summary>One mod line, PoE style: "+14% MAX HP [8-20]". withRange=false hides the bracket.</summary>
        public static string ModLine(SinMod m, float q, bool withRange = true)
        {
            float v = m.Val(q);
            if (m.fmt != null)
            {
                string r = withRange ? " [" + Num(m.lo, m.integer) + "-" + Num(m.hi, m.integer) + "]" : "";
                return m.fmt.Replace("{v}", Num(v, m.integer) + r);
            }
            int pv = IsPct(m.stat) ? Mathf.RoundToInt(v * 100) : Mathf.RoundToInt(v);
            string rr = withRange ? " [" + (IsPct(m.stat) ? Mathf.RoundToInt(m.lo * 100) + "-" + Mathf.RoundToInt(m.hi * 100) : Mathf.RoundToInt(m.lo) + "-" + Mathf.RoundToInt(m.hi)) + "]" : "";
            string sign = m.stat == SinStat.Cooldown || m.stat == SinStat.DashCd ? "-" : "+";
            string l = sign + pv + (IsPct(m.stat) ? "% " : " ") + StatTxt[(int)m.stat] + rr;
            return l;
        }
        /// <summary>Applied value of a Mortal stat mod (rounded like the display so tooltip == effect).</summary>
        public static float StatVal(SinMod m, float q) { float v = m.Val(q); return IsPct(m.stat) ? Mathf.Round(v * 100) / 100f : Mathf.Round(v); }
        public static List<string> Lines(SinDef d, float[] q, bool withRange = true)
        {
            var l = new List<string>();
            if (d.effFmt != null)
            {
                string e = d.effFmt;
                for (int i = 0; i < d.mods.Count; i++) { var m = d.mods[i]; float v = m.Val(q != null && i < q.Length ? q[i] : 0.5f); e = e.Replace("{" + i + "}", Num(v, m.integer) + (withRange ? " [" + Num(m.lo, m.integer) + "-" + Num(m.hi, m.integer) + "]" : "")); }
                l.Add(e);
                for (int i = 0; i < d.mods.Count; i++) if (d.mods[i].stat != SinStat.None) l.Add(ModLine(d.mods[i], q != null && i < q.Length ? q[i] : 0.5f, withRange)); // t818u Demonic +1 stat
            }
            else for (int i = 0; i < d.mods.Count; i++) l.Add(ModLine(d.mods[i], q != null && i < q.Length ? q[i] : 0.5f, withRange));
            l.AddRange(d.fixedLines); return l;
        }
        public static float Quality(float[] q) { if (q == null || q.Length == 0) return 1f; float s = 0; foreach (var x in q) s += Mathf.Clamp01(x); return s / q.Length; }
        public static string QualityTxt(float[] q, SinDef d)
        {
            if (d.mods.Count == 0) return "NO ROLLED VALUES"; float v = Quality(q);
            return v >= HellordTuning.I.sinPerfectAt737 ? "ROLL: PERFECT" : "ROLL: " + Mathf.RoundToInt(v * 100) + "% OF MAX";
        }
        // Demonic / HD: every number in the effect text becomes a rolled {v} (base x low .. x high)
        static void AutoRoll(SinDef d, string effect, float lo, float hi)
        {
            var rx = new System.Text.RegularExpressions.Regex(@"\d+(\.\d+)?"); string fmt = ""; int last = 0; var ms = rx.Matches(effect);
            if (ms.Count == 0) { d.fixedLines = Prepend("+ " + effect, d.fixedLines); return; }
            // one mod per line: first number rolls, further numbers in the same line roll too (each its own mod line part); keep it simple: each number = own SinMod sharing the line via chaining
            var parts = new List<SinMod>();
            foreach (System.Text.RegularExpressions.Match m in ms)
            {
                float b = float.Parse(m.Value, System.Globalization.CultureInfo.InvariantCulture); bool integer = !m.Value.Contains(".");
                float l = b * lo, h = b * hi; if (integer) { l = Mathf.Max(1, Mathf.Floor(l)); h = Mathf.Max(l, Mathf.Ceil(h)); }
                parts.Add(new SinMod { lo = l, hi = h, integer = integer, fmt = "" });
                fmt += effect.Substring(last, m.Index - last) + "{" + (parts.Count - 1) + "}"; last = m.Index + m.Length;
            }
            fmt += effect.Substring(last); d.effFmt = "+ " + fmt; d.mods.AddRange(parts);
        }
        static string[] Prepend(string a, string[] r) { var l = new List<string> { a }; l.AddRange(r); return l.ToArray(); }

        static void Build()
        {
            _all = new List<SinDef>(); var T = HellordTuning.I;
            // ---- Mortal singles (13, t784u: one sin per stat, value ROLLS in a min-max range)
            var single = new (SinStat st, string nm)[] {
                (SinStat.MaxHp, "Greed of the Ox Pit"), (SinStat.Speed, "Killing of Naihe Bridge"), (SinStat.Pickup, "Greed of the Needle-Throat"),
                (SinStat.Armor, "Delusion of the Iron Bed"), (SinStat.Crit, "Slander of the Tongue Court"), (SinStat.Dmg, "Hatred of the Sawing Court"),
                (SinStat.Fire, "Hatred of the Burning Hell"), (SinStat.Cold, "Hatred of the Frozen Hell"), (SinStat.Lightning, "Hatred of the Thunder Court"),
                (SinStat.Phys, "Hatred of the Knife Mountain"), (SinStat.Cooldown, "Killing of the Gong"), (SinStat.Minion, "Idle Speech of the Grave"),
                (SinStat.Area, "Wrong View of the Nine Springs") };
            for (int k = 0; k < single.Length; k++)
            {
                var d = new SinDef { name = single[k].nm, tier = SinTier.Mortal, live = true, wIdx = k, weight = 100 };
                d.mods.Add(new SinMod { stat = single[k].st, lo = A(T.sinSingleMin737, k, 0.03f), hi = A(T.sinSingleMax737, k, 0.06f) }); Add(d);
            }
            // ---- Mortal pairs (12, VH t818u: no twins, one sin per combo; rolled stats give the variety). Old v2 twins merge on load (SinMigrateV2toV3_737).
            var pairs = new (SinStat a, SinStat b, string nm)[] {
                (SinStat.MaxHp, SinStat.Armor, "Greed and Delusion of the Iron City"),
                (SinStat.MaxHp, SinStat.Speed, "Greed and Killing of the Ghost Road"),
                (SinStat.Dmg, SinStat.Crit, "Hatred and Slander of the Judge's Desk"),
                (SinStat.Fire, SinStat.Area, "Hatred and Wrong View of the Burning City"),
                (SinStat.Cold, SinStat.Cooldown, "Hatred and Killing of the Eight Cold Hells"),
                (SinStat.Lightning, SinStat.Crit, "Hatred and Slander of the Storm Gate"),
                (SinStat.Phys, SinStat.Bleed, "Hatred of the Blood Pool"),
                (SinStat.Minion, SinStat.MaxHp, "Idle Speech and Greed of the Ghost Festival"),
                (SinStat.Pickup, SinStat.Xp, "Greed and Theft of the Hungry Ghost"),
                (SinStat.Speed, SinStat.DashCd, "Killing of the Black Wind"),
                (SinStat.Area, SinStat.Duration, "Wrong View of the Wheel of Rebirth"),
                (SinStat.CurseSoul, SinStat.Cooldown, "Hatred and Killing of the Hanged Ghost") };
            for (int k = 0; k < pairs.Length; k++)
            {
                var p = pairs[k];
                var d = new SinDef { name = p.nm, tier = SinTier.Mortal, live = true, weight = T.sinPairW737 != null && k < T.sinPairW737.Length ? T.sinPairW737[k] : 100 };
                d.mods.Add(new SinMod { stat = p.a, lo = A(T.sinPairMin737, (int)p.a, 0.05f), hi = A(T.sinPairMax737, (int)p.a, 0.1f) });
                d.mods.Add(new SinMod { stat = p.b, lo = A(T.sinPairMin737, (int)p.b, 0.05f), hi = A(T.sinPairMax737, (int)p.b, 0.1f) }); Add(d);
            }
            // ---- Demonic (34): doc sec 5 (effect not yet implemented)
            string[,] dem = {
                {"Spear of the Tongue-Ripping Court","Soul Spears +2 pierce","-10% Soul Spear speed"},
                {"Meng Po's Bitter Ladle","Curse Rune explosions +25% area and +10% damage","Curse Rune cooldown +10%"},
                {"Mouth of the Needle-Throat Ghost","6% chance per kill to heal 1 HP","-20% pickup range"},
                {"Veil of the Sea of Fire","Burn lasts +2s; burning enemies take +5% damage","-15% Cold damage"},
                {"Thunder Drum of the Fifth Court","Chain Lightning +2 jumps","Chain Lightning -10% damage"},
                {"Hook of the Blood Pool","+15% Physical damage to Bleeding enemies","-4% crit chance"},
                {"Lantern of the Naihe Ferry","Grave Pool slows +20% more; hits chill","Grave Pool -15% damage"},
                {"Brush of the Book of Life and Death","Executioner threshold +5% HP on Burning/Bleeding targets","-6% damage to enemies above 50% HP"},
                {"Ox-Head's Iron Fork","Knockback +40%, knocked enemies take +5% damage for 1s","-6% move speed"},
                {"Cinder Beads of the Burning Hell","Fire skills -10% cooldown","Non-Fire skills +6% cooldown"},
                {"Cleaver of the Sawing Court","Blood Whip heal per hit +50%","Blood Whip -10% damage"},
                {"Bone Abacus of the Karma Ledger","Soul Harvest bone chance +6%, +1 XP per bone","-6% XP gain"},
                {"Incense of the Ghost Festival","Storm Ring hits have 20% chance to Bleed","Storm Ring radius -10%"},
                {"Death Bell of White Impermanence","Every 25th kill drops a free Curse Rune at that spot","Curse Rune cooldown +15%"},
                {"Ribs of the Knife Mountain","Bone Orbit +1 skull","Orbit radius -15%"},
                {"Joss Paper of the Burned Offering","Fork children +20% damage and Burn","Fork shots -10% speed"},
                {"Shard of the Karma Mirror","Echo repeats +12% damage","Echo skill cooldown +10%"},
                {"Oath of the Yin Soldiers","Skeleton Warriors +30% HP and block +1 bullet","-5% move speed"},
                {"Fire Wheel of Nezha's Ghost","Dash rolls a fire wheel that hits and knocks back demons on its path, leaving a burn trail (3s, 40% Fireball damage/s)","Dash cooldown +15%"},
                {"Hell Bank Note of the First Hand","First skill slot +12% damage","Other skills -5% damage"},
                {"Dirge of the Unburied","Skeleton Archers' arrows Burn and +15% damage","Archers -15% attack speed"},
                {"The Broken Talisman of Zhong Kui","Projectiles gain +2 Bounce","Projectiles -15% damage"},
                {"Nails of the Bronze Pillar","Bleed +40% damage while you stand still","-10% move speed"},
                {"Red Thread of the Ghost Bride","Shock +2s and +10% shock effect","Take +10% damage while a shocked enemy is within 2u"},
                {"Second Belly of the Hungry Ghost","+1 card option on every 3rd level-up","-10% XP gain"},
                {"The Unmourned Child","Cursed enemies' death explosion +40% damage, +15% area","Explosions within 1.5u deal 4 damage to you"},
                {"Hand of the Frozen Hell","Frozen enemies take +30% damage; frozen kills shatter","Fire damage -25%"},
                {"Black Impermanence's Chain","Minions rise again 50% faster, +15% minion damage","Your own skills -12% damage"},
                {"Blade of the Beheading Platform","Executioner kills refund 20% of the skill's cooldown","Executioner damage -10%"},
                {"Nine-Forked Lie of the Tongue Court","Chain Lightning first bolt splits into 3 at 50% damage","Chain Lightning cooldown +20%"},
                {"Heart of the Oil Cauldron","Burn stacks up to 3","Burn duration -30%"},
                {"Debt Ledger of the Ten Courts","+6% damage per boss killed this run (max +24%)","Boss HP +15%"},
                {"Bone Throne of the Pit of Ox-Head","Every 4th Bone Orbit skull hit fires a Soul Spear outward","Bone Orbit -15% damage"},
                {"Breath of the Hanged Ghost","Life Leech cap per second +50%; overheal becomes a shield (max 10% HP)","Other healing -30%"} };
            string[] demVn = { "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "Co Hon", "", "", "", "", "Vong Nhi", "", "", "", "", "", "", "", "Ma That Co" };
            for (int i = 0; i < 34; i++)
            {
                var d = new SinDef { name = dem[i, 0], vn = demVn[i], tier = SinTier.Demonic, wIdx = i, weight = 50, live = T.sinDemonicHdLive818,
                    fixedLines = new[] { "- " + dem[i, 2] } }; // t818u: effect live (HellordSinFx818.cs); drawback size = sinDraw818
                AutoRoll(d, dem[i, 1], T.sinRollLowDemonic737, T.sinRollHighDemonic737);
                // t818u: the '+1 stat' is a real rolled stat mod (stat choice = sinDemBonusStat818; #1-20 single range, #21-34 pair range)
                var bs = (SinStat)Mathf.Clamp(T.sinDemBonusStat818 != null && i < T.sinDemBonusStat818.Length ? T.sinDemBonusStat818[i] : 5, 0, (int)SinStat.CurseSoul);
                int sk = -1; for (int k = 0; k < single.Length; k++) if (single[k].st == bs) sk = k;
                if (i < 20 && sk >= 0) d.mods.Add(new SinMod { stat = bs, lo = A(T.sinSingleMin737, sk, 0.03f), hi = A(T.sinSingleMax737, sk, 0.06f) });
                else d.mods.Add(new SinMod { stat = bs, lo = A(T.sinPairMin737, (int)bs, 0.05f), hi = A(T.sinPairMax737, (int)bs, 0.1f) });
                Add(d);
            }
            // ---- Heaven-Defying (11): doc sec 5 (effect not yet implemented)
            string[,] hd = {
                {"Pearl of the Dragon King","Long Vuong Chau","Storm Ring","Storm Ring is centred on your mouse cursor","Radius -25%"},
                {"Ten Thousand Ghost Banner","Van Hon Phien","Soul Spears","Kills store souls; at 20 they swarm as ghosts for 45s","Soul Spears -30% damage"},
                {"Tongue of the Needle-Throat Ghost","Nga Quy","Blood Whip","Whip pulls demons in and devours those under 15% HP","-4% move speed per Gluttony stack"},
                {"Golden Crow of the Sun","Kim O","Fireball","Fireball fires one slow sun that grows per demon passed","Fireball cooldown x2"},
                {"Lamp of the Nine Nether Springs","Cuu U","Curse Rune","Cursed demons rise as your minions for 8s","Curse Rune area -40%; can't be Echoed"},
                {"Ring of the Burning City","Hoa Thanh","Fire Wall","Fire Wall becomes a closed ring of fire that moves with you","Can't be aimed; -30% damage"},
                {"Ghost Mushrooms of Fengdu","Nam Quy Phong Do","Grave Pool","Drop a demon mushroom every 3s: burst + poison + slow","Lose 3% current HP per mushroom"},
                {"Ox-Head and Horse-Face Answer the Call","Nguu Dau Ma Dien","Skeleton Warriors","Warriors become 2 guardians (6x HP, taunt, cleave)","Max 2 Skeleton Warriors"},
                {"Blade of the Eight Cold Hells","Bat Han","Frost Cleave","Frost Cleave becomes a full spin; Burn becomes Freeze buildup","-35% damage; you cannot Burn"},
                {"Lightning Rod of the Heavenly Tribulation","Dan Loi Thien Kiep","Chain Lightning","Chains bounce through you; 20 charges = Tribulation bolt","0.5% max HP per pass; full strike hits you"},
                {"Samsara Wheel of Six Rings","Luc Dao Luan","Bone Orbit","Bone Orbit becomes a huge 6-spoke wheel of rebirth","-25% damage; slow spin"} };
            for (int i = 0; i < 11; i++)
            {
                var d = new SinDef { name = hd[i, 0], vn = hd[i, 1], tier = SinTier.HeavenDefying, wIdx = i, weight = 20, live = T.sinDemonicHdLive818,
                    fixedLines = new[] { "- " + hd[i, 4], "TARGETS: " + hd[i, 2] } }; // t818u: effect live
                AutoRoll(d, hd[i, 3], T.sinRollLowHD737, T.sinRollHighHD737); Add(d);
            }
        }
        static float A(float[] a, int i, float def) => a != null && i >= 0 && i < a.Length ? a[i] : def;
        static void Add(SinDef d) { d.id = _all.Count; _all.Add(d); }
    }

    public partial class HellordGame
    {
        // ---- statics read by SkillInst.CdMul / AreaMul (HellordData.cs)
        public static float SinCd737 = 1f, SinArea737 = 1f, SinDuration737 = 1f; // t818u Duration
        float _sinDashMul737 = 1f, _sinXpMul737 = 1f, _sinBleedMul737 = 1f, _sinMinionMul737 = 1f;
        float[] _sinTagMul737 = new float[5]; // fire, cold, lightning, phys/bone/blood, curse/soul (additive bonus)
        // previously applied additive totals (so SinRecalc737 is idempotent / can run mid-run)
        float _sinPHp737, _sinPSpeed737, _sinPPick737, _sinPArmor737, _sinPCrit737, _sinPDmg737;

        const int SinCap737 = 128; bool[] _sinOwned737 = new bool[SinCap737]; float[][] _sinRoll737 = new float[SinCap737][]; // t784u: rolled q (0..1) per mod of each owned sin
        bool _sinPerfect737; // cheat: force perfect rolls
        SinDef _sinBetter737; List<string> _sinBetterOld737, _sinBetterNew737; float _sinBetterT737, _sinBetterQ0737, _sinBetterQ1737; // BETTER ROLL notice
        int[] _sinEq737 = { -1, -1, -1 }; int _sinSlots737; bool _sinLoaded737;
        bool _sinCheats737, _sinForce737;

        // swap card (mid-run, all slots full)
        SinDef _sinSwap737; float _sinSwapT737; bool _sinSwapArmed737; float _sinPrevTs737 = 1f;
        // menu panel
        bool _sinPanel737; int _sinFilter737, _sinSel737 = -1, _sinPage737; string _sinMsg737 = ""; float _sinMsgT737;

        static HellordTuning ST737 => HellordTuning.I;
        bool SinCheatsOn737 => ST737.sinDebugCheats737 || _sinCheats737;

        // ================= save =================
        const int SinSaveVer737 = 3; // t818u: v3 = 70-sin id list (Mortal twins merged). v2 (82 sins) is migrated; v1 (95 fixed sins) is cleared on load (slots kept)
        /// <summary>t818u: v2 ids (13 singles, 24 twin pairs at 13+2k+t, 45 Demonic/HD at 37..81) -> v3 ids (13, 12 pairs, 45 at 25..69). Owned twins merge, better roll kept.</summary>
        static int SinV2toV3_737(int i) => i < 13 ? i : i < 37 ? 13 + (i - 13) / 2 : i < 82 ? i - 12 : -1;
        void SinMigrateV2toV3_737()
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture; string o = PlayerPrefs.GetString(PK + "sin_owned", "");
            var own = new bool[SinCap737]; var roll = new float[SinCap737][];
            for (int i = 0; i < 82 && i < o.Length; i++)
            {
                if (o[i] != '1') continue; int n = SinV2toV3_737(i); if (n < 0 || n >= HSins.Count) continue;
                var parts = PlayerPrefs.GetString(PK + "sin_r" + i, "").Split(','); var q = new float[HSins.All[n].mods.Count];
                for (int j = 0; j < q.Length; j++) q[j] = j < parts.Length && float.TryParse(parts[j], System.Globalization.NumberStyles.Float, inv, out var f) ? Mathf.Clamp01(f) : 0.5f;
                if (!own[n] || HSins.Quality(q) > HSins.Quality(roll[n])) roll[n] = q; own[n] = true;
            }
            for (int i = 0; i < SinCap737; i++) PlayerPrefs.DeleteKey(PK + "sin_r" + i);
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < HSins.Count; i++)
            {
                sb.Append(own[i] ? '1' : '0');
                if (own[i]) { var t = new string[roll[i].Length]; for (int j = 0; j < t.Length; j++) t[j] = roll[i][j].ToString("0.###", inv); PlayerPrefs.SetString(PK + "sin_r" + i, string.Join(",", t)); }
            }
            PlayerPrefs.SetString(PK + "sin_owned", sb.ToString());
            var eq = new List<int>();
            for (int i = 0; i < 3; i++) { int v = PlayerPrefs.GetInt(PK + "sin_eq" + i, -1); int n = v >= 0 ? SinV2toV3_737(v) : -1; if (n >= 0 && n < HSins.Count && own[n] && !eq.Contains(n)) eq.Add(n); }
            for (int i = 0; i < 3; i++) PlayerPrefs.SetInt(PK + "sin_eq" + i, i < eq.Count ? eq[i] : -1); // merged twins equipped together -> one slot freed
            PlayerPrefs.SetInt(PK + "sin_v", SinSaveVer737); PlayerPrefs.Save();
        }
        void SinLoad737()
        {
            if (_sinLoaded737) return; _sinLoaded737 = true;
            int sv = PlayerPrefs.GetInt(PK + "sin_v", 1);
            if (sv == 2) SinMigrateV2toV3_737();
            else if (sv != SinSaveVer737)
            {
                PlayerPrefs.DeleteKey(PK + "sin_owned"); for (int i = 0; i < 3; i++) PlayerPrefs.DeleteKey(PK + "sin_eq" + i);
                for (int i = 0; i < SinCap737; i++) PlayerPrefs.DeleteKey(PK + "sin_r" + i);
                PlayerPrefs.SetInt(PK + "sin_v", SinSaveVer737); PlayerPrefs.Save(); // sin_slots (paid with Souls/Sigils) is kept
            }
            string o = PlayerPrefs.GetString(PK + "sin_owned", "");
            for (int i = 0; i < HSins.Count; i++)
            {
                _sinOwned737[i] = i < o.Length && o[i] == '1'; if (!_sinOwned737[i]) continue;
                var d = HSins.All[i]; var q = new float[d.mods.Count]; var parts = PlayerPrefs.GetString(PK + "sin_r" + i, "").Split(',');
                for (int j = 0; j < q.Length; j++) q[j] = j < parts.Length && float.TryParse(parts[j], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var f) ? Mathf.Clamp01(f) : 0.5f;
                _sinRoll737[i] = q;
            }
            for (int i = 0; i < 3; i++) { int v = PlayerPrefs.GetInt(PK + "sin_eq" + i, -1); _sinEq737[i] = v >= 0 && v < HSins.Count && _sinOwned737[v] ? v : -1; }
            _sinSlots737 = Mathf.Clamp(PlayerPrefs.GetInt(PK + "sin_slots", 0), 0, SlotsMax737);
            if (System.Environment.CommandLine.Contains("-sincheats")) _sinCheats737 = true;
        }
        void SinSave737()
        {
            var sb = new System.Text.StringBuilder(); for (int i = 0; i < HSins.Count; i++) sb.Append(_sinOwned737[i] ? '1' : '0');
            PlayerPrefs.SetString(PK + "sin_owned", sb.ToString());
            for (int i = 0; i < HSins.Count; i++)
                if (_sinOwned737[i] && _sinRoll737[i] != null) { var t = new string[_sinRoll737[i].Length]; for (int j = 0; j < t.Length; j++) t[j] = _sinRoll737[i][j].ToString("0.###", System.Globalization.CultureInfo.InvariantCulture); PlayerPrefs.SetString(PK + "sin_r" + i, string.Join(",", t)); }
                else PlayerPrefs.DeleteKey(PK + "sin_r" + i);
            for (int i = 0; i < 3; i++) PlayerPrefs.SetInt(PK + "sin_eq" + i, _sinEq737[i]);
            PlayerPrefs.SetInt(PK + "sin_slots", _sinSlots737); PlayerPrefs.SetInt(PK + "sin_v", SinSaveVer737); PlayerPrefs.Save();
        }
        float[] SinNewRoll737(SinDef d) { var q = new float[d.mods.Count]; for (int i = 0; i < q.Length; i++) q[i] = _sinPerfect737 && SinCheatsOn737 ? 1f : Random.value; return q; }
        float[] SinQ737(int id) => _sinRoll737[id];
        int SlotsMax737 => Mathf.Clamp(ST737.sinSlotsMax737, 0, 3);
        int SinOwnedCount737 { get { int n = 0; foreach (var b in _sinOwned737) if (b) n++; return n; } }
        bool SinEquipped737(int id) { for (int i = 0; i < 3; i++) if (_sinEq737[i] == id) return true; return false; }

        // ================= effects =================
        void SinRunReset737() { _sinPHp737 = _sinPSpeed737 = _sinPPick737 = _sinPArmor737 = _sinPCrit737 = _sinPDmg737 = 0; _sinSwap737 = null; SinCd737 = SinArea737 = SinDuration737 = 1f; SinRunReset818(); _sinDashMul737 = _sinXpMul737 = _sinBleedMul737 = _sinMinionMul737 = 1f; for (int i = 0; i < 5; i++) _sinTagMul737[i] = 0; }
        /// <summary>Called at run start (PactApply51) and whenever the loadout changes mid-run. Applies the DIFFERENCE vs what was applied before.</summary>
        void SinRecalc737()
        {
            SinLoad737();
            float dur = 0; SinRecalcPre818(); float hpP = 0, sp = 0, pk = 0, ar = 0, cr = 0, dm = 0, cd = 0, area = 0, dash = 0, xp = 0, bl = 0, mn = 0; float[] tg = new float[5];
            if (ST737.sinEnabled737)
                for (int i = 0; i < Mathf.Min(_sinSlots737, 3); i++)
                {
                    int id = _sinEq737[i]; if (id < 0) continue; var d = HSins.All[id]; if (!d.live) continue;
                    var q = _sinRoll737[id];
                    for (int j = 0; j < d.mods.Count; j++)
                    {
                        var s = d.mods[j].stat; if (s == SinStat.None) continue; float v = HSins.StatVal(d.mods[j], q != null && j < q.Length ? q[j] : 0.5f);
                        switch (s)
                        {
                            case SinStat.MaxHp: hpP += v; break; case SinStat.Speed: sp += v; break; case SinStat.Pickup: pk += v; break;
                            case SinStat.Armor: ar += v; break; case SinStat.Crit: cr += v; break; case SinStat.Dmg: dm += v; break;
                            case SinStat.Fire: tg[0] += v; break; case SinStat.Cold: tg[1] += v; break; case SinStat.Lightning: tg[2] += v; break;
                            case SinStat.Phys: tg[3] += v; break; case SinStat.CurseSoul: tg[4] += v; break;
                            case SinStat.Cooldown: cd += v; break; case SinStat.Area: area += v; break; case SinStat.DashCd: dash += v; break;
                            case SinStat.Xp: xp += v; break; case SinStat.Duration: dur += v; break; case SinStat.Bleed: bl += v; break; case SinStat.Minion: mn += v; break;
                        }
                    }
                }
            SinRecalcFixed818(ref sp, ref pk, ref cr, ref xp, ref dash, tg); SinDuration737 = Mathf.Clamp(1 + dur, 0.1f, Mathf.Max(1f, ST737.sinDurationMax818)); // t818u drawbacks + Duration
            // additive player stats: apply delta
            if (Mathf.Abs(hpP - _sinPHp737) > 1e-5f) { float m = (1 + hpP) / (1 + _sinPHp737); float add = maxHp * (m - 1); maxHp *= m; if (add > 0) hp += add; hp = Mathf.Min(hp, maxHp); }
            speedMul += sp - _sinPSpeed737; pickupMul += pk - _sinPPick737; armor += ar - _sinPArmor737; crit += cr - _sinPCrit737; incDmg += dm - _sinPDmg737;
            _sinPHp737 = hpP; _sinPSpeed737 = sp; _sinPPick737 = pk; _sinPArmor737 = ar; _sinPCrit737 = cr; _sinPDmg737 = dm;
            SinCd737 = Mathf.Max(0.1f, 1 - cd); SinArea737 = 1 + area; _sinDashMul737 = Mathf.Max(0.1f, 1 - dash); _sinXpMul737 = 1 + xp; _sinBleedMul737 = 1 + bl; _sinMinionMul737 = 1 + mn;
            _sinTagMul737 = tg;
        }
        /// <summary>Tag damage multiplier for a player skill (Fire / Cold / Lightning / Physical|Bone|Blood / Curse|Soul). Additive inside the sin bucket, max of matching tags summed.</summary>
        float SinSkillMul737(SkillInst s)
        {
            if (s == null || s.def == null) return 1f; var t = s.def.tags; float b = 0;
            if ((t & STag.Fire) != 0) b += _sinTagMul737[0]; if ((t & STag.Cold) != 0) b += _sinTagMul737[1]; if ((t & STag.Lightning) != 0) b += _sinTagMul737[2];
            if ((t & (STag.Physical | STag.Bone | STag.Blood)) != 0) b += _sinTagMul737[3]; if ((t & (STag.Curse | STag.Soul)) != 0) b += _sinTagMul737[4];
            return Mathf.Max(0.05f, (1 + b) * SinSkDmg818(s)); // t818u per-skill sin damage
        }
        int SinXp737(int v) => _sinXpMul737 == 1f ? v : _sinXpMul737 > 1f ? Mathf.Max(v, Mathf.RoundToInt(v * _sinXpMul737)) : Mathf.Max(0, StochRound(v * _sinXpMul737)); // t818u: XP drawbacks can lower it

        // ================= drops =================
        void SinOnKill737(Enemy e)
        {
            if (!ST737.sinEnabled737 || mode != Mode.Play || e == null) return; SinLoad737();
            var T = ST737; bool f = SinCheatsOn737 && _sinForce737; float fc = T.sinDebugForceChance737;
            SinTier? hit = null;
            if (e.boss)
            {
                if (Random.value < (f ? fc : T.sinBossHeavenDefyingChance737) && SinPoolOk737(SinTier.HeavenDefying)) hit = SinTier.HeavenDefying;
                else if (Random.value < (f ? fc : T.sinBossRareChance737) && SinPoolOk737(SinTier.Demonic)) hit = SinTier.Demonic;
                else if (Random.value < (f ? fc : T.sinBossNormalChance737)) hit = SinTier.Mortal;
            }
            else
            {
                if (Random.value < (f ? fc : T.sinDemonRareChance737) && SinPoolOk737(SinTier.Demonic)) hit = SinTier.Demonic;
                else if (Random.value < (f ? fc : T.sinDemonNormalChance737)) hit = SinTier.Mortal;
            }
            if (hit.HasValue) SinGrant737(SinRoll737(hit.Value), true);
        }
        bool SinDroppable737(SinDef d) => d.live || ST737.sinDropUnimplemented737;
        bool SinPoolOk737(SinTier t) { foreach (var d in HSins.All) if (d.tier == t && SinDroppable737(d) && d.W > 0) return true; return false; }
        SinDef SinPick737(SinTier t)
        {
            int sum = 0; foreach (var d in HSins.All) if (d.tier == t && SinDroppable737(d)) sum += Mathf.Max(0, d.W);
            if (sum <= 0) return null; int r = Random.Range(0, sum);
            foreach (var d in HSins.All) if (d.tier == t && SinDroppable737(d)) { r -= Mathf.Max(0, d.W); if (r < 0) return d; }
            return null;
        }
        SinDef SinRoll737(SinTier t)
        {
            var d = SinPick737(t);
            for (int i = 0; i < ST737.sinDupRerolls737 && d != null && _sinOwned737[d.id]; i++) d = SinPick737(t);
            return d;
        }
        /// <summary>t784u: gives a sin with a fresh roll. Owned -> compare rolls: better (higher average normalized roll) REPLACES the stored roll (live if equipped) + BETTER ROLL notice;
        /// worse/equal -> Souls (+Sigil for HD). New -> mid-run auto-equip / swap card.</summary>
        void SinGrant737(SinDef d, bool inRun, float[] forced = null)
        {
            if (d == null) return; var T = ST737; var q = forced ?? SinNewRoll737(d);
            if (_sinOwned737[d.id])
            {
                var old = _sinRoll737[d.id]; float q0 = HSins.Quality(old), q1 = HSins.Quality(q);
                if (d.mods.Count > 0 && q1 > q0 + 1e-4f)
                {
                    _sinBetterOld737 = HSins.Lines(d, old, false); _sinBetterNew737 = HSins.Lines(d, q, false); _sinBetter737 = d; _sinBetterQ0737 = q0; _sinBetterQ1737 = q1; _sinBetterT737 = T.sinBetterCardS737;
                    _sinRoll737[d.id] = q; SinSave737(); if (SinEquipped737(d.id) && mode == Mode.Play) SinRecalc737();
                    HellordAudio.Play(string.IsNullOrEmpty(T.sinBetterSfx737) ? "ui_confirm" : T.sinBetterSfx737);
                    if (mode != Mode.Play) { _sinMsg737 = "BETTER ROLL! " + d.name + " " + Mathf.RoundToInt(q0 * 100) + "% -> " + Mathf.RoundToInt(q1 * 100) + "%"; _sinMsgT737 = 3.5f; }
                    return;
                }
                int[] a = T.sinDupSouls737; int s = a != null && (int)d.tier < a.Length ? a[(int)d.tier] : 20;
                PactLoad51(); _souls51 += s; if (d.tier == SinTier.HeavenDefying) _sigils51 += T.sinDupHeavenDefyingSigils737; PactSave51();
                SinToast737("WORSE ROLL -> +" + s + " SOULS" + (d.tier == SinTier.HeavenDefying ? " +" + T.sinDupHeavenDefyingSigils737 + " SIGIL" : ""));
                return;
            }
            _sinOwned737[d.id] = true; _sinRoll737[d.id] = q; SinSave737();
            HellordAudio.Play("ui_confirm");
            if (!inRun || mode != Mode.Play) { SinToast737("SIN GAINED: " + d.name); return; }
            int unlocked = Mathf.Min(_sinSlots737, 3);
            if (unlocked <= 0) { SinToast737("NEW SIN: " + d.name + " (UNLOCK A SIN SLOT IN THE MENU)"); return; }
            for (int i = 0; i < unlocked; i++) if (_sinEq737[i] < 0 && SinTierOk737(d, i)) { _sinEq737[i] = d.id; SinSave737(); SinRecalc737(); SinToast737("SIN EQUIPPED: " + d.name); return; }
            _sinSwap737 = d; _sinSwapT737 = 0; _sinSwapArmed737 = false;
            if (T.sinSwapPause737) { _sinPrevTs737 = Time.timeScale > 0 ? Time.timeScale : 1f; Time.timeScale = 0f; }
        }
        /// <summary>t784u BETTER ROLL notice (HUD 640x360, top centre, does not pause). Tier-coloured frame, old vs new values.</summary>
        void DrawSinBetter737()
        {
            if (_sinBetter737 == null || _sinBetterT737 <= 0) return;
            if (Event.current.type == EventType.Repaint) _sinBetterT737 -= Time.unscaledDeltaTime;
            var d = _sinBetter737; var tc = HSins.TierCol[(int)d.tier]; int n = Mathf.Max(_sinBetterOld737.Count, _sinBetterNew737.Count);
            float w = 380, h = 34 + n * 20, x = (HW - w) / 2, y = 40; float a = Mathf.Clamp01(_sinBetterT737 / 0.4f); float pa = _guiA; _guiA = a;
            Rect51(x - 2, y - 2, w + 4, h + 4, new Color(tc.r, tc.g, tc.b, a)); Rect51(x, y, w, h, new Color(U_PANEL.r, U_PANEL.g, U_PANEL.b, a));
            string t = "BETTER ROLL!  " + Mathf.RoundToInt(_sinBetterQ0737 * 100) + "% -> " + (_sinBetterQ1737 >= ST737.sinPerfectAt737 ? "PERFECT" : Mathf.RoundToInt(_sinBetterQ1737 * 100) + "%");
            Text(Sanitize(t), x + (w - TextW(Sanitize(t))) / 2, y + 5, U_YEL);
            string nm = Sanitize(d.name); Text(nm, x + (w - TextW(nm)) / 2, y + 16, tc);
            for (int i = 0; i < n; i++)
            {
                if (i < _sinBetterOld737.Count) Text(Sanitize("OLD " + _sinBetterOld737[i]), x + 8, y + 30 + i * 20, U_GREY);
                if (i < _sinBetterNew737.Count) Text(Sanitize("NEW " + _sinBetterNew737[i]), x + 8, y + 39 + i * 20, U_AMB);
            }
            _guiA = pa;
        }
        bool SinTierOk737(SinDef d, int slot)
        {
            int cap = d.tier == SinTier.HeavenDefying ? ST737.sinHeavenDefyingMax737 : d.tier == SinTier.Demonic ? ST737.sinDemonicMax737 : 3; int n = 0;
            for (int i = 0; i < 3; i++) if (i != slot && _sinEq737[i] >= 0 && HSins.All[_sinEq737[i]].tier == d.tier && d.tier != SinTier.Mortal) n++;
            return n < cap;
        }
        void SinToast737(string s) { if (mode == Mode.Play) Toast(s); else { _sinMsg737 = s; _sinMsgT737 = 2.5f; } }

        // ================= swap card (in run) =================
        /// <summary>Update hook: returns true while the swap card is open (game logic frozen).</summary>
        bool SinSwapTick737()
        {
            if (_sinSwap737 == null) return false;
            _sinSwapT737 += Time.unscaledDeltaTime;
            bool anyHeld = SinAnyHeld737();
            if (!_sinSwapArmed737) { if (_sinSwapT737 >= ST737.sinSwapDelay737 && !anyHeld) _sinSwapArmed737 = true; return true; }
            int n = Mathf.Min(_sinSlots737, 3);
            if (HInput.Down(HKey.One) && n > 0) SinSwapPick737(0); else if (HInput.Down(HKey.Two) && n > 1) SinSwapPick737(1);
            else if (HInput.Down(HKey.Three) && n > 2) SinSwapPick737(2); else if (HInput.Down(HKey.Escape)) SinSwapPick737(-1);
            return true;
        }
        static bool SinAnyHeld737()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.anyKey;
#else
            return false;
#endif
        }
        float SinWrap737(string s, float x, float y, float w, Color c)
        {
            int m = Mathf.Max(8, Mathf.FloorToInt((w * K69 + 1) / 6)); string cur = "";
            foreach (var word in Sanitize(s).Split(' '))
            {
                if (cur.Length > 0 && cur.Length + 1 + word.Length > m) { Tx69(cur, x, y, c); y += 10; cur = word; }
                else cur = cur.Length > 0 ? cur + " " + word : word;
            }
            if (cur.Length > 0) { Tx69(cur, x, y, c); y += 10; }
            return y;
        }
        void SinSwapPick737(int slot)
        {
            var d = _sinSwap737; if (d == null) return;
            if (slot >= 0 && SinTierOk737(d, slot)) { _sinEq737[slot] = d.id; SinSave737(); SinRecalc737(); Toast("SIN SWAPPED IN: " + d.name); }
            else Toast("SIN KEPT IN COLLECTION: " + d.name);
            _sinSwap737 = null; if (ST737.sinSwapPause737) Time.timeScale = _sinPrevTs737;
            HellordAudio.Play("ui_confirm");
        }
        /// <summary>HUD space (640x360). Drawn from OnGUI after the HUD.</summary>
        void DrawSinSwap737()
        {
            var d = _sinSwap737; if (d == null) return;
            Rect51(0, 0, HW, 360, new Color(0, 0, 0, 0.6f));
            float w = 420, h = 200, x = (HW - w) / 2, y = 70; var tc = HSins.TierCol[(int)d.tier];
            Rect51(x - 2, y - 2, w + 4, h + 4, tc); Rect51(x, y, w, h, U_PANEL);
            Text(Sanitize("NEW " + HSins.TierName[(int)d.tier] + " SIN - ALL SLOTS FULL"), x + 8, y + 6, U_AMB);
            Text(Sanitize(d.name), x + 8, y + 20, tc == HSins.TierCol[0] ? U_SAND : tc);
            float ly = y + 32; foreach (var l in HSins.Lines(d, _sinRoll737[d.id])) { Text(Sanitize(l), x + 12, ly, U_SAND); ly += 10; }
            ly = y + 82; int n = Mathf.Min(_sinSlots737, 3);
            for (int i = 0; i < n; i++)
            {
                var cur = _sinEq737[i] >= 0 ? HSins.All[_sinEq737[i]] : null; bool ok = SinTierOk737(d, i);
                var r = new Rect(x + 8, ly, w - 16, 22); bool ov = r.Contains(Event.current.mousePosition);
                Rect51(r.x, r.y, r.width, r.height, ov && ok ? U_PLUM : U_INK);
                Text(Sanitize("[" + (i + 1) + "] SWAP OUT: " + (cur != null ? cur.name : "EMPTY")), r.x + 4, r.y + 3, ok ? U_SAND : U_GREY);
                if (cur != null) Text(Sanitize(string.Join("  ", HSins.Lines(cur, _sinRoll737[cur.id], false))), r.x + 4, r.y + 12, U_GREY);
                if (_sinSwapArmed737 && ok && Event.current.type == EventType.MouseDown && ov) { Event.current.Use(); SinSwapPick737(i); return; }
                ly += 26;
            }
            var kr = new Rect(x + 8, y + h - 20, w - 16, 14); bool ko = kr.Contains(Event.current.mousePosition);
            Rect51(kr.x, kr.y, kr.width, kr.height, ko ? U_PLUM : U_INK); Text("[ESC] KEEP CURRENT SINS (NEW ONE GOES TO COLLECTION)", kr.x + 4, kr.y + 4, U_SAND);
            if (_sinSwapArmed737 && Event.current.type == EventType.MouseDown && ko) { Event.current.Use(); SinSwapPick737(-1); }
        }

        // ================= menu panel (700x392 art space) =================
        void SinOpen737() { if(Time.frameCount==_sinBackFrame87)return; SinLoad737(); PactLoad51(); _sinPanel737 = true; _sinSel737 = -1; _sinPage737 = 0; HellordAudio.Play("ui_confirm"); }
        int _sinBackFrame87=-1;
        void SinClose87(){if(!_sinPanel737)return;_sinPanel737=false;_sinBackFrame87=Time.frameCount;_s74Click=null;_s74Fake=null;DisposeSin84();HellordAudio.Play("ui_back");}
        void SinPanelTick737()
        {
            if (_sinMsgT737 > 0) _sinMsgT737 -= Time.unscaledDeltaTime;
            if (MenuThemeEscape()) return; // t1300: -uitheme menu: the unlock confirm owns Esc (no-op for Stone1246)
            if (ConfirmKeys49()) return; // t1149: Esc/Enter belong to the unlock confirm while it is open
            if (HInput.Down(HKey.Escape)) { SinClose87(); return; }
        }
        List<SinDef> SinFiltered737() { var l = new List<SinDef>(); foreach (var d in HSins.All) if (_sinFilter737 == 0 || (int)d.tier == _sinFilter737 - 1) l.Add(d); return l; }
        bool SinBtn737(float x, float y, float w, float h, string s, bool on, bool sel = false)
        {
            bool ov = on && Over51(x, y, w, h);
            R53(x, y, w, h, sel ? U_DRED : ov ? U_PLUM : U_INK); Ol53(x, y, w, h, on ? U_SLATE : U_LOCK);
            TxC69(s, x + w / 2, y + (h - 8) / 2, on ? U_SAND : U_GREY);
            return on && Click53(x, y, w, h);
        }
        void DrawSinPanel737()
        {
            if (HellordUITheme.Active == HellordUITheme.Theme.MenuTheme && DrawSinPanelMenuTheme()) return; // t1277: -uitheme menu (placeholder, falls back to Stone1246 until the art exists)
            if (T1246On && T1246Ready()) { DrawSinPanel1246(); return; } // t1259 v0.0.99 default: approved t1246b pass3e screen (HellordSin1246.cs); -sins1246off = v0.0.98 line screen
            if (LineUI1176) { DrawSinPanelLine1176(); return; } // t1178 default: this original line panel + kept functions (HellordLineUI1176.cs); no fire
            if (SinArt74On && SinArt74Ready()) { DrawSinPanel74(); return; } // step74 stone-guardian (HellordSinsArt74.cs), opt-in with -stoneui
            if (SinArt64On && SinArt64Ready()) { DrawSinPanel64(); return; } // step64 trial (HellordSinsArt64.cs)
            SinLoad737();
            R53(0, 0, 700, 392, new Color(0, 0, 0, 0.82f)); R53(14, 14, 672, 364, U_PANEL); Ol53(14, 14, 672, 364, U_DRED);
            Tx69("INFERNAL SINS", 24, 22, U_AMB);
            string cnt = "COLLECTED " + SinOwnedCount737 + "/" + HSins.Count + "    SOULS " + _souls51 + "   SIGILS " + _sigils51; Tx69(cnt, 676 - TW69(cnt), 22, U_SAND);
            if (SinBtn737(620, 360, 60, 14, "CLOSE", true)) { _sinPanel737 = false; return; }
            // slots row
            for (int i = 0; i < 3; i++)
            {
                float x = 24 + i * 222, y = 38; bool unl = i < _sinSlots737 && i < SlotsMax737;
                Ol53(x, y, 212, 34, unl ? U_SLATE : U_LOCK);
                if (unl)
                {
                    int id = _sinEq737[i]; var d = id >= 0 ? HSins.All[id] : null;
                    Tx69("SLOT " + (i + 1), x + 4, y + 3, U_GREY);
                    if (d != null) { R53(x + 1, y + 13, 4, 20, HSins.TierCol[(int)d.tier]); Tx69(Fit69(d.name, 150), x + 8, y + 14, U_SAND); if (SinBtn737(x + 166, y + 20, 42, 12, "REMOVE", true)) { _sinEq737[i] = -1; SinSave737(); } }
                    else Tx69("EMPTY", x + 8, y + 16, U_GREY);
                }
                else if (i < SlotsMax737)
                {
                    int sc = Arr737(ST737.sinSlotSoulCost737, i, 100), gc = Arr737(ST737.sinSlotSigilCost737, i, 1); bool next = i == _sinSlots737, afford = _souls51 >= sc && _sigils51 >= gc;
                    Tx69("SLOT " + (i + 1) + " LOCKED", x + 4, y + 3, U_GREY);
                    if (SinBtn737(x + 4, y + 16, 204, 14, "UNLOCK: " + sc + " SOULS + " + gc + " SIGIL" + (gc == 1 ? "" : "S"), next && afford))
                    { PactLoad51(); _souls51 -= sc; _sigils51 -= gc; PactSave51(); _sinSlots737 = i + 1; SinSave737(); _sinMsg737 = "SIN SLOT " + (i + 1) + " UNLOCKED"; _sinMsgT737 = 2.5f; }
                }
            }
            // filters
            string[] fl = { "ALL", "MORTAL", "DEMONIC", "HEAVEN-DEFYING" };
            for (int i = 0; i < 4; i++) if (SinBtn737(24 + i * 96, 78, 92, 14, fl[i], true, _sinFilter737 == i)) { _sinFilter737 = i; _sinPage737 = 0; _sinSel737 = -1; }
            // grid: 6 cols x 8 rows per page on the left
            var list = SinFiltered737(); const int cols = 4, rows = 13, per = cols * rows; int pages = Mathf.Max(1, (list.Count + per - 1) / per); _sinPage737 = Mathf.Clamp(_sinPage737, 0, pages - 1);
            for (int k = 0; k < per; k++)
            {
                int li = _sinPage737 * per + k; if (li >= list.Count) break; var d = list[li];
                float x = 24 + (k % cols) * 96, y = 98 + (k / cols) * 20; bool own = _sinOwned737[d.id]; var tc = HSins.TierCol[(int)d.tier];
                bool ov = Over51(x, y, 92, 18);
                R53(x, y, 92, 18, _sinSel737 == d.id ? U_PLUM : ov ? U_INK : new Color(0.08f, 0.06f, 0.1f)); Ol53(x, y, 92, 18, own ? tc : U_LOCK);
                Tx69(own ? Fit69(d.name, 84) : "? ? ?", x + 4, y + 5, own ? (SinEquipped737(d.id) ? U_YEL : U_SAND) : U_LOCK);
                if (Click53(x, y, 92, 18)) _sinSel737 = d.id;
            }
            if (pages > 1)
            {
                if (SinBtn737(24, 360, 40, 14, "<", _sinPage737 > 0)) _sinPage737--;
                Tx69((_sinPage737 + 1) + "/" + pages, 72, 363, U_GREY);
                if (SinBtn737(104, 360, 40, 14, ">", _sinPage737 < pages - 1)) _sinPage737++;
            }
            // detail
            float dx = 414, dy = 98; Ol53(dx, dy, 266, 256, U_SLATE);
            if (_sinSel737 >= 0)
            {
                var d = HSins.All[_sinSel737]; bool own = _sinOwned737[d.id]; var tc = HSins.TierCol[(int)d.tier];
                R53(dx + 1, dy + 1, 264, 4, tc);
                Tx69(HSins.TierName[(int)d.tier] + " SIN  #" + (d.id + 1), dx + 6, dy + 10, tc);
                if (own)
                {
                    float y = SinWrap737(d.name, dx + 6, dy + 24, 254, U_SAND);
                    if (d.vn != "") { Tx69("(" + d.vn + ")", dx + 6, y + 2, U_GREY); y += 12; }
                    var q = _sinRoll737[d.id]; float ql = HSins.Quality(q); Tx69(HSins.QualityTxt(q, d), dx + 6, y + 2, ql >= ST737.sinPerfectAt737 ? U_YEL : U_SAND); y += 12;
                    y += 4; foreach (var l in HSins.Lines(d, q)) y = SinWrap737(l, dx + 6, y, 254, l.StartsWith("-") ? C69_RED : l.StartsWith("EFFECT NOT") ? U_GREY : U_AMB) + 3;
                    bool eq = SinEquipped737(d.id);
                    if (eq) { if (SinBtn737(dx + 6, dy + 232, 120, 16, "UNEQUIP", true)) { for (int i = 0; i < 3; i++) if (_sinEq737[i] == d.id) _sinEq737[i] = -1; SinSave737(); } }
                    else
                    {
                        int free = -1; for (int i = 0; i < Mathf.Min(_sinSlots737, 3); i++) if (_sinEq737[i] < 0 && SinTierOk737(d, i)) { free = i; break; }
                        if (SinBtn737(dx + 6, dy + 232, 120, 16, _sinSlots737 <= 0 ? "NO SLOT UNLOCKED" : free < 0 ? "SLOTS FULL" : "EQUIP", free >= 0)) { _sinEq737[free] = d.id; SinSave737(); }
                    }
                }
                else { Tx69("UNKNOWN SIN", dx + 6, dy + 24, U_GREY); Tx69("Not found yet. Sins drop from demons and bosses.", dx + 6, dy + 40, U_LOCK); }
            }
            else Tx69("Select a sin.", dx + 6, dy + 10, U_GREY);
            if (_sinMsgT737 > 0) TxC69(_sinMsg737, 350, 362, U_AMB);
        }
        static int Arr737(int[] a, int i, int def) => a != null && i < a.Length ? a[i] : def;

        // ================= debug (test menu) =================
        void SinCheatGrantAll737() { SinLoad737(); for (int i = 0; i < HSins.Count; i++) if (!_sinOwned737[i]) { _sinOwned737[i] = true; _sinRoll737[i] = SinNewRoll737(HSins.All[i]); } SinSave737(); }
        /// <summary>Cheat: drops a duplicate of the first equipped sin (fresh roll, or perfect if Force perfect is on) -> better-roll / souls path.</summary>
        string SinCheatDupEquipped737()
        {
            SinLoad737(); for (int i = 0; i < 3; i++) if (_sinEq737[i] >= 0) { var d = HSins.All[_sinEq737[i]]; SinGrant737(d, mode == Mode.Play); return "Duplicate of " + d.name; }
            return "No sin equipped";
        }
        void SinCheatReset737() { SinLoad737(); for (int i = 0; i < SinCap737; i++) { _sinOwned737[i] = false; _sinRoll737[i] = null; } for (int i = 0; i < 3; i++) _sinEq737[i] = -1; _sinSlots737 = 0; SinSave737(); if (mode == Mode.Play) SinRecalc737(); }
        void SinCheatRandom737(SinTier t) { SinLoad737(); var d = SinPick737(t) ?? HSins.All.Find(x => x.tier == t); SinGrant737(d, mode == Mode.Play); }
    }
}
