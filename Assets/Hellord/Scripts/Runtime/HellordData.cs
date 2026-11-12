using System;
using Mathf = UnityEngine.Mathf;
using System.Collections.Generic;

// Hellord prototype v0.1 - data tables (static, data-driven). All numbers are PROPOSALS for tuning.
namespace Hellord
{
    [Flags] public enum STag { None = 0, Projectile = 1, Area = 2, Orbital = 4, Hit = 8, Melee = 16, Aura = 32, Zone = 64, Minion = 128, Chain = 256, Duration = 512, Curse = 1024, Strike = 2048, Physical = 4096, Fire = 8192, Cold = 16384, Lightning = 32768, Blood = 65536, Bone = 131072, Soul = 262144, Any = 1 << 20 } // t626u: approved tag design v0.1 // t536u Minion: summoned units' hits (never a player skill)
    public enum SkillId { Fireball, FrostCleave, StormRing, BoneOrbit, CurseRune, ChainLightning, HellfirePillar, BloodWhip, SoulSpears, GravePool, SkeletonWarriors, SkeletonArchers } // v0.0.36: last 5 are new (placeholder icons)
    public enum SupportId { MultipleProjectiles, Pierce, Fork, Bounce, Homing, BiggerArea, Echo, MoreOrbitals, Burn, Chill, Shock, Bleed, AddedDamage, FasterCasting, Executioner, LifeLeech, Knockback, SoulHarvest, ExplodeOnKill } // v0.0.36: last 5 are new
    public enum StatId { MaxHP, Speed, Pickup, Armor, Crit, Slot }
    public enum Status { None, Burn, Chill, Shock, Bleed, Curse }

    public class SkillDef
    {
        public SkillId id; public string name, desc; public STag tags; public float baseDmg, cooldown; public bool aimable; public Status innate;
    }
    public class SupportDef
    {
        public SupportId id; public string name, desc; public STag needs; public string needsLabel; public STag requiresAll, excludes; public string category; // t626u: needs = requiresAny; category: no two supports of the same category on one skill (only when set)
        public STag requiresAny => needs;
    }
    public class StatDef { public StatId id; public string name, desc; }

    public static class HData
    {
        public const int MaxSkillLevel = 5, MaxSupports = 4;
        public static readonly SkillDef[] Skills =
        {
            new SkillDef { id = SkillId.Fireball,   name = "Fireball",    tags = STag.Projectile | STag.Area | STag.Hit | STag.Fire, baseDmg = 10, cooldown = 0.75f, aimable = true,  innate = Status.Burn,  desc = "Aimed shot, bursts on impact. Light burn." },
            new SkillDef { id = SkillId.FrostCleave,name = "Frost Cleave",tags = STag.Melee | STag.Area | STag.Hit | STag.Cold,      baseDmg = 14, cooldown = 0.9f,  aimable = true,  innate = Status.Chill, desc = "Wide icy swing in front. Chills (3 chills = freeze)." },
            new SkillDef { id = SkillId.StormRing,  name = "Storm Ring",  tags = STag.Aura | STag.Area | STag.Hit | STag.Lightning,       baseDmg = 5,  cooldown = 0.5f,  aimable = false, innate = Status.Shock, desc = "Crackling ring around you. Shocks (+25% dmg taken)." },
            new SkillDef { id = SkillId.BoneOrbit,  name = "Bone Orbit",  tags = STag.Orbital | STag.Hit | STag.Bone | STag.Physical,               baseDmg = 8,  cooldown = 0.4f,  aimable = false, innate = Status.Bleed, desc = "Skulls circle you. Bleeds." },
            new SkillDef { id = SkillId.CurseRune,  name = "Curse Rune",  tags = STag.Zone | STag.Area | STag.Hit | STag.Curse | STag.Duration,       baseDmg = 6,  cooldown = 3.0f,  aimable = true,  innate = Status.Curse, desc = "Rune at aim point. Cursed enemies explode on death." },
            new SkillDef { id = SkillId.ChainLightning, name = "Chain Lightning", tags = STag.Area | STag.Hit | STag.Chain | STag.Lightning,                   baseDmg = 7,  cooldown = 1.1f,  aimable = false, innate = Status.Shock, desc = "A bolt jumps between nearby demons. More jumps per level. Shocks." },
            new SkillDef { id = SkillId.HellfirePillar, name = "Hellfire Pillar", tags = STag.Area | STag.Hit | STag.Fire,                   baseDmg = 9,  cooldown = 2.2f,  aimable = false, innate = Status.Burn,  desc = "Fire pillars erupt under demon clusters. More and wider per level. Burns." },
            new SkillDef { id = SkillId.BloodWhip,      name = "Blood Whip",      tags = STag.Melee | STag.Area | STag.Hit | STag.Blood | STag.Physical,      baseDmg = 8,  cooldown = 1.0f,  aimable = true,  innate = Status.Bleed, desc = "Long lash in front. Heals a little per hit. More reach and lashes per level. Bleeds." },
            new SkillDef { id = SkillId.SoulSpears,     name = "Soul Spears",     tags = STag.Projectile | STag.Hit | STag.Soul,             baseDmg = 8,  cooldown = 1.0f,  aimable = true,  innate = Status.None,  desc = "Spears pierce a line toward the nearest demon. More spears and pierce per level." },
            new SkillDef { id = SkillId.GravePool,      name = "Grave Pool",      tags = STag.Area | STag.Zone | STag.Hit | STag.Duration,       baseDmg = 5,  cooldown = 1.2f,  aimable = false, innate = Status.None,  desc = "Leaves slowing, damaging puddles where you walk. Bigger and longer per level." },
            new SkillDef { id = SkillId.SkeletonWarriors, name = "Skeleton Warriors", tags = STag.Minion | STag.Melee | STag.Hit | STag.Physical | STag.Bone, baseDmg = 6, cooldown = 0.5f, aimable = false, innate = Status.None, desc = "Raise sword-and-shield skeletons beside you. They fight up front, block bullets and rise again after dying." }, // t561u
            new SkillDef { id = SkillId.SkeletonArchers,  name = "Skeleton Archers",  tags = STag.Minion | STag.Projectile | STag.Hit | STag.Physical | STag.Bone, baseDmg = 5, cooldown = 0.5f, aimable = false, innate = Status.None, desc = "Raise skeleton archers that hang back and shoot bone arrows. They rise again after dying." }, // t561u
        };
        public static readonly SupportDef[] Supports =
        {
            new SupportDef { id = SupportId.MultipleProjectiles, name = "Multi Projectiles", needs = STag.Projectile, needsLabel = "Projectile", desc = "More projectiles per cast (+1 at Lv1, up to +4), slightly less damage" },
            new SupportDef { id = SupportId.Pierce,       name = "Pierce",         needs = STag.Projectile, needsLabel = "Projectile", desc = "Shots pierce more enemies per level (1 to 5), -15% dmg per pierce" },
            new SupportDef { id = SupportId.Fork,         name = "Fork",           needs = STag.Projectile, needsLabel = "Projectile", desc = "Splits on first hit: 2 children at Lv1, up to 5, stronger per level" },
            new SupportDef { id = SupportId.Bounce,       name = "Bounce",         needs = STag.Projectile, needsLabel = "Projectile", desc = "Bounces to nearby enemies: 1 at Lv1, up to 5, longer reach" },
            new SupportDef { id = SupportId.Homing,       name = "Homing",         needs = STag.Projectile, needsLabel = "Projectile", desc = "Projectiles curve toward enemies, tighter and farther per level" },
            new SupportDef { id = SupportId.BiggerArea,   name = "Bigger Area",    needs = STag.Area,       needsLabel = "Area",       desc = "Bigger area: +25% at Lv1, up to +90%" },
            new SupportDef { id = SupportId.Echo,         name = "Echo",           needs = STag.Area,       needsLabel = "Area", excludes = STag.Minion, desc = "Repeats the cast: once at Lv1, 3 times at Lv5, stronger repeats" },
            new SupportDef { id = SupportId.MoreOrbitals, name = "More Orbitals",  needs = STag.Orbital,    needsLabel = "Orbital",    desc = "More skulls: +1 at Lv1, up to +4" },
            new SupportDef { id = SupportId.Burn,         name = "Burn",           needs = STag.Hit,        needsLabel = "Hit",        desc = "Hits burn; burn is stronger and lasts longer per level" },
            new SupportDef { id = SupportId.Chill,        name = "Chill",          needs = STag.Hit,        needsLabel = "Hit",        desc = "Hits chill (slow 35%); chills last longer, Lv4 freezes in 2 chills" },
            new SupportDef { id = SupportId.Shock,        name = "Shock",          needs = STag.Hit,        needsLabel = "Hit",        desc = "Hits shock: +25% damage taken at Lv1, up to +50%" },
            new SupportDef { id = SupportId.Bleed,        name = "Bleed",          needs = STag.Hit,        needsLabel = "Hit",        desc = "Hits bleed; bleed is stronger per level" },
            new SupportDef { id = SupportId.AddedDamage,  name = "Added Damage",   needs = STag.Any,        needsLabel = "Any",        desc = "More damage: +15% at Lv1, up to +50%" },
            new SupportDef { id = SupportId.FasterCasting,name = "Faster Casting", needs = STag.Any,        needsLabel = "Any",        desc = "Shorter cooldown: -10% at Lv1, up to -35%" },
            new SupportDef { id = SupportId.Executioner,  name = "Executioner",    needs = STag.Any,        needsLabel = "Any",        desc = "Bonus damage to demons below an HP threshold: 10% HP at Lv1, up to 30% at Lv5" },
            new SupportDef { id = SupportId.LifeLeech,    name = "Life Leech",     needs = STag.Hit,        needsLabel = "Hit",        desc = "Hits heal you a little (capped per second)" },
            new SupportDef { id = SupportId.Knockback,    name = "Knockback",      needs = STag.Hit,        needsLabel = "Hit",        desc = "Hits push demons away" },
            new SupportDef { id = SupportId.SoulHarvest,  name = "Soul Harvest",   needs = STag.Any,        needsLabel = "Any",        desc = "Kills by this skill drop extra XP and sometimes bones" },
            new SupportDef { id = SupportId.ExplodeOnKill,name = "Explode on Kill",needs = STag.Hit,        needsLabel = "Hit",        desc = "Demons this skill kills explode for a small area hit" },
        };
        public static readonly StatDef[] Stats =
        {
            new StatDef { id = StatId.MaxHP,  name = "Vitality", desc = "+10% max HP" },
            new StatDef { id = StatId.Speed,  name = "Swiftness",desc = "+8% move speed" },
            new StatDef { id = StatId.Pickup, name = "Hunger",   desc = "+15% pickup range" },
            new StatDef { id = StatId.Armor,  name = "Hide",     desc = "+5 armor" },
            new StatDef { id = StatId.Crit,   name = "Malice",   desc = "+5% crit chance" },
        };
        /// <summary>Short concrete effect of a support at a level (for the level-up card), read from the tuning tables.</summary>
        public static string SupportEffect(SupportId id, int l, int skillId = -1)
        {
            var T = HellordTuning.I; string P(float f) => Mathf.RoundToInt(f * 100f) + "%";
            switch (id)
            {
                case SupportId.MultipleProjectiles: return $"+{At(T.supMultiExtra, l)} projectiles";
                case SupportId.Pierce: return $"pierces {At(T.supPierce, l)} enemies";
                case SupportId.Fork: return $"splits in {At(T.supForkChildren, l)} ({P(At(T.supForkDmg, l))} dmg)";
                case SupportId.Bounce: return $"bounces {At(T.supBounce, l)}x";
                case SupportId.Homing: return $"homing range {At(T.supHomingRangeU, l):0.#}";
                case SupportId.BiggerArea: return $"+{P(At(T.supAreaMul, l) - 1f)} area";
                case SupportId.Echo: if (skillId == (int)SkillId.CurseRune) return $"repeats {At(T.supEchoCount, l)}x: {Mathf.Max(1, T.curseEchoRunes738)} smaller rune ({P(T.curseEchoRadiusMul738)} size, {P(At(T.supEchoDmg, l) * T.curseEchoDmgMul738)} dmg)"; // t818u: Curse Rune echo is its own rule (t738u)
                    return $"repeats {At(T.supEchoCount, l)}x ({P(At(T.supEchoDmg, l))} dmg)";
                case SupportId.MoreOrbitals: return $"+{At(T.supOrbitals, l)} skulls";
                case SupportId.Burn: return $"burn x{At(T.supBurnMul, l):0.0}, {T.burnDuration + T.burnExtraSecondsPerLevel * (l - 1):0.#}s";
                case SupportId.Chill: return $"chill x{At(T.supChillTime, l):0.0}, freeze in {At(T.supChillFreezeHits, l)}";
                case SupportId.Shock: return $"+{P(At(T.supShockAmp, l))} damage taken";
                case SupportId.Bleed: return $"bleed x{At(T.supBleedMul, l):0.0}";
                case SupportId.AddedDamage: return $"+{P(At(T.supAddedDmg, l) - 1f)} damage";
                case SupportId.FasterCasting: return $"-{P(1f - At(T.supFasterCd, l))} cooldown";
                case SupportId.Executioner: return $"+{P(At(T.supExecDmg, l) - 1f)} dmg below {P(At(T.supExecThreshold, l))} HP";
                case SupportId.LifeLeech: return $"heal {P(At(T.supLeechFrac, l))} of hit, max {At(T.supLeechCapPerSec, l):0.#}/s";
                case SupportId.Knockback: return $"push {At(T.supKnockback, l):0.#}";
                case SupportId.SoulHarvest: return $"+{At(T.supHarvestXp, l)} XP, {P(At(T.supHarvestBoneChance, l))} bone";
                case SupportId.ExplodeOnKill: return $"explode r{At(T.supExplodeRadiusU, l):0.#}, {P(At(T.supExplodeDmg, l))} dmg";
            }
            return "";
        }
        public static string StatDesc(StatDef d)
        {
            var T = HellordTuning.I;
            switch (d.id)
            {
                case StatId.MaxHP: return $"+{Mathf.RoundToInt(T.statHpFrac * 100)}% max HP";
                case StatId.Speed: return $"+{Mathf.RoundToInt(T.statSpeed * 100)}% move speed";
                case StatId.Armor: return $"+{T.statArmor:0.#} armor";
                case StatId.Crit: return $"+{Mathf.RoundToInt(T.statCrit * 100)}% crit chance";
                case StatId.Slot: return "Unlocks your next skill slot now, then pick the skill";
            }
            return d.desc;
        }
        public static SkillDef Skill(SkillId id) => Skills[(int)id];
        public static SupportDef Support(SupportId id) => Supports[(int)id];

        /// <summary>v0.0.27 skill damage curve: 1 + step x (level - 1) (step 0.06, was 0.3), no max-level spike (was x1.5). Skill level now grows COVERAGE, not damage.</summary>
        public static float LevelMul(int lvl) { var T = HellordTuning.I; return (1f + T.skillLevelStep * (lvl - 1)) * (lvl >= MaxSkillLevel ? T.skillMaxSpike : 1f); }
        public static int BonusSlots; // v0.0.37: boss reward "New Skill Slot" cards (the bar shows 3 slots, so the total stays <= 3)
        public static int SlotsUnlocked(int playerLevel) { var T = HellordTuning.I; return Mathf.Min(Mathf.Max(1, T.skillSlotsMax818), 1 + (playerLevel >= T.slot2Level ? 1 : 0) + (playerLevel >= T.slot3Level ? 1 : 0) + (playerLevel >= T.slot4Level818 ? 1 : 0) + (playerLevel >= T.slot5Level818 ? 1 : 0) + BonusSlots); } // t818u: up to 5 skills
        /// <summary>v0.0.27 XP curve (tuning): (95 + 22L + 0.5L^2) x xpCostMul; v0.0.26 was 5 + 4L + L^2/4.</summary>
        public static int XpToNext(int level) { var T = HellordTuning.I; return Math.Max(1, (int)Math.Round((T.xpBase + T.xpPerLevel * level + T.xpQuad * level * level) * T.xpCostMul)); }
        /// <summary>Safe table lookup by level (1-based), clamped to the table; a missing/short table falls back to its last/first value.</summary>
        public static float At(float[] a, int lvl, float fb = 1f) { if (a == null || a.Length == 0) return fb; return a[Math.Max(0, Math.Min(a.Length - 1, lvl - 1))]; }
        public static int At(int[] a, int lvl, int fb = 1) { if (a == null || a.Length == 0) return fb; return a[Math.Max(0, Math.Min(a.Length - 1, lvl - 1))]; }
        public static bool Summon561(SkillDef d) => d != null && (d.id == SkillId.SkeletonWarriors || d.id == SkillId.SkeletonArchers); // t561u
        public static int MaxLv(SkillDef d) => Summon561(d) ? Math.Max(1, HellordTuning.I.summonMaxLv561) : MaxSkillLevel; // t587u: summons max Lv5 like every skill
        public static bool Fits(SupportDef s, SkillDef k) => (s.needs == STag.Any || (k.tags & s.needs) != 0) && (k.tags & s.requiresAll) == s.requiresAll && (k.tags & s.excludes) == 0; // t626u
        static readonly STag[] _tagOrder626 = { STag.Projectile, STag.Melee, STag.Strike, STag.Area, STag.Chain, STag.Orbital, STag.Aura, STag.Zone, STag.Minion, STag.Duration, STag.Curse, STag.Hit, STag.Physical, STag.Fire, STag.Cold, STag.Lightning, STag.Blood, STag.Bone, STag.Soul };
        /// <summary>t626u: tag chips text, e.g. "PROJECTILE FIRE". short = drop Hit (shown on every skill).</summary>
        public static string TagText(STag t, string sep = " ", bool upper = true, bool skipHit = false)
        {
            if (t == STag.Any) return upper ? "ANY" : "Any"; var sb = new System.Text.StringBuilder();
            foreach (var x in _tagOrder626) if ((t & x) != 0 && !(skipHit && x == STag.Hit)) { if (sb.Length > 0) sb.Append(sep); string n = x == STag.Area ? "AoE" : x.ToString(); sb.Append(upper ? n.ToUpperInvariant() : n); }
            return sb.ToString();
        }
        /// <summary>t626u: null when it fits; otherwise which tags are missing / blocking.</summary>
        public static string Mismatch(SupportDef s, SkillDef k)
        {
            var parts = new List<string>();
            if (s.needs != STag.Any && (k.tags & s.needs) == 0) parts.Add("needs " + TagText(s.needs, " or ", false));
            STag miss = s.requiresAll & ~k.tags; if (miss != 0) parts.Add("needs " + TagText(miss, " + ", false));
            STag bad = s.excludes & k.tags; if (bad != 0) parts.Add("can't support " + TagText(bad, "/", false) + " skills");
            return parts.Count == 0 ? null : string.Join(", ", parts);
        }
    }

    /// <summary>A skill owned by the player: level + socketed supports.</summary>
    public class SkillInst
    {
        public const int MaxSupLevel = 5;
        public SkillDef def; public int level = 1; public readonly List<SupportId> sup = new List<SupportId>(4);
        public readonly int[] supLvl = new int[32]; // v0.0.36: 32 (19 supports now); v0.0.27: level of each socketed support (0 or missing = Lv1 when socketed through sup.Add)
        public float cd, echoT = -1; public int echoLeft;
        public int OpenSlots => Math.Min(level, HData.MaxSupports) - sup.Count;
        public bool Has(SupportId s) => sup.Contains(s);
        /// <summary>Support level 1..5 (0 = not socketed).</summary>
        public int Lvl(SupportId s) => sup.Contains(s) ? Math.Max(1, Math.Min(MaxSupLevel, supLvl[(int)s])) : 0;
        public bool CanUpgrade(SupportDef s) => Has(s.id) && Lvl(s.id) < MaxSupLevel;
        public void AddSupport(SupportId s) { if (Has(s)) { supLvl[(int)s] = Math.Min(MaxSupLevel, Lvl(s) + 1); } else { sup.Add(s); supLvl[(int)s] = 1; } }
        /// <summary>null = can socket, otherwise the warning text shown to the player.</summary>
        public string CanSocket(SupportDef s)
        {
            if (!HData.Fits(s, def)) return $"{s.name} can't go on {def.name}: {HData.Mismatch(s, def)}."; // t626u: lists missing/blocking tags
            if (!string.IsNullOrEmpty(s.category) && !Has(s.id)) foreach (var o in sup) { var od = HData.Supports[(int)o]; if (od.category == s.category) return $"{def.name} already has a {s.category} support ({od.name})."; } // t626u: category no-stack (only when set)
            if (Has(s.id)) return Lvl(s.id) < MaxSupLevel ? null : $"{def.name}: {s.name} is already max level."; // owned + below Lv5 = an UPGRADE
            if (OpenSlots <= 0) return level < HData.MaxSupports ? $"{def.name} has no open slot (level it up to Lv{level + 1})." : $"{def.name} support slots are full.";
            return null;
        }
        public float More()
        {
            var T = HellordTuning.I; float m = 1;
            if (Has(SupportId.AddedDamage)) m *= HData.At(T.supAddedDmg, Lvl(SupportId.AddedDamage), 1.15f);
            if (Has(SupportId.MultipleProjectiles)) m *= T.multiProjDmgMul;
            return m;
        }
        public float CdMul => HellordGame.SinSkCdAt818(def.id) * (Has(SupportId.FasterCasting) ? HData.At(HellordTuning.I.supFasterCd, Lvl(SupportId.FasterCasting), 0.9f) : 1f) * HellordGame.PCd49 * ((def.tags & (STag.Zone | STag.Orbital)) != 0 ? HellordGame.SlothCd53 : 1f) * HellordGame.PactCdS53 * HellordGame.SinCd737; // v0.0.49 Intensity; v0.0.53 Sloth Throne / Price of Wrath
        public float AreaMul => HellordGame.SinSkAreaAt818(def.id) * (Has(SupportId.BiggerArea) ? HData.At(HellordTuning.I.supAreaMul, Lvl(SupportId.BiggerArea), 1.25f) : 1f) * HellordGame.PArea49 * ((def.tags & (STag.Zone | STag.Orbital)) != 0 ? HellordGame.SlothArea53 : 1f) * HellordGame.BoneArea536(def) * HellordGame.SinArea737; // t536u Grave Reach / Grave Colossus // v0.0.49 aggression x Intensity
    }
}
