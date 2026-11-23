using System;
using Mathf = UnityEngine.Mathf;
using System.Collections.Generic;

// Hellord prototype v0.1 - data tables (static, data-driven). All numbers are PROPOSALS for tuning.
namespace Hellord
{
    [Flags] public enum STag { None = 0, Projectile = 1, Area = 2, Orbital = 4, Hit = 8, Melee = 16, Aura = 32, Zone = 64, Minion = 128, Chain = 256, Duration = 512, Curse = 1024, Strike = 2048, Physical = 4096, Fire = 8192, Cold = 16384, Lightning = 32768, Blood = 65536, Bone = 131072, Soul = 262144, Any = 1 << 20, Movement = 1 << 21, Special = 1 << 22, Ultimate = 1 << 23, Cooldown = 1 << 24, Mark = 1 << 25, Warcry = 1 << 26, Buff = 1 << 27, Conditional = 1 << 28, Triggered = 1 << 29, Decoy = 1 << 30 } /* t1651: kit tags 21-30 */ // t626u: approved tag design v0.1 // t536u Minion: summoned units' hits (never a player skill)
    public enum SkillId { Fireball, FrostCleave, StormRing, BoneOrbit, CurseRune, ChainLightning, HellfirePillar, BloodWhip, SoulSpears, GravePool, SkeletonWarriors, SkeletonArchers, Frostbolt, IceNova, FrostBomb, Hurricane, MagmaThrow, RollingMagmaRock, FirePillar, SparkSpray, BallLightning, ThunderJavelin, HeadsmansChop, EarthSplittingSlam, TwinAxeThrow, PhantomAxes, FireWall, Hydra /* t1625: was VolcanicOrb (t1618) */, ZhurongKit /* t1618 */, ZkEmberEffigy, ZkEmberAnchor, ZkBackdraft, ZkPyreMark, ZkFlammability, ZkWarCry, ZkSmolderRing, ZkHearthBlessing, ZkSunsWrath, ZkRebirthFlame, ZkMoltenHeart, ZkVermilionBird /* t1651: the 12 Zhurong kit skills (never offered as basics) */ } // t1481: ids 16-18 fire basic main attacks (LMB); t1393u: ids 12-15 are the ice skills; v0.0.36: last 5 are new (placeholder icons)
    public enum SupportId { MultipleProjectiles, Pierce, Fork, Bounce, Homing, BiggerArea, Echo, MoreOrbitals, Burn, Chill, Shock, Bleed, AddedDamage, FasterCasting, Executioner, LifeLeech, Knockback, SoulHarvest, ExplodeOnKill, Chain, Duration } // v0.0.36: last 5 are new; t1651: + Chain, Duration
    /// <summary>t1651: what the support autotest (sup1651) measures for a support. A support without one fails the static check.</summary>
    public enum SupMetric1651 { None, Proj, Distinct, Homing, Area, Repeat, Orbitals, Status, Damage, Cooldown, Execute, Leech, Knock, KillHook, Duration }
    /// <summary>t1651: the named skill stats supports change. Mul stats start at 1 and multiply; the others start at 0 and add.</summary>
    public enum SStat1651 { DmgMul, CdMul, AreaMul, DurMul, ProjExtra, Pierce, ForkN, ForkDmg, Bounce, BounceR, HomingR, HomingTurn, ChainExtra, RepeatN, RepeatDmg, Orbitals, PierceLoss }
    /// <summary>t1651: one stat change of a support at a level (values read from HellordTuning arrays, so the tuning stays the one source). when = skills it applies to (null = all).</summary>
    public class SMod1651
    {
        public SStat1651 stat; public Func<HellordTuning, int, float> v; public Func<SkillDef, bool> when; public bool hidden; // hidden = not printed on the card (a secondary value)
        public static bool IsMul(SStat1651 s) => s <= SStat1651.DurMul;
        public static SMod1651 Arr(SStat1651 s, Func<HellordTuning, float[]> a, float fb, bool hidden = false, Func<SkillDef, bool> when = null) => new SMod1651 { stat = s, v = (T, l) => HData.At(a(T), l, fb), hidden = hidden, when = when };
        public static SMod1651 ArrI(SStat1651 s, Func<HellordTuning, int[]> a, int fb, bool hidden = false, Func<SkillDef, bool> when = null) => new SMod1651 { stat = s, v = (T, l) => HData.At(a(T), l, fb), hidden = hidden, when = when };
        public static SMod1651 Val(SStat1651 s, Func<HellordTuning, float> f, Func<SkillDef, bool> when = null) => new SMod1651 { stat = s, v = (T, l) => f(T), when = when };
    }
    public enum StatId { MaxHP, Speed, Pickup, Armor, Crit, Slot }
    public enum Status { None, Burn, Chill, Shock, Bleed, Curse }

    /// <summary>t1651: projectile mechanics a skill's projectile cannot use (HData.Fits then never offers that support there).</summary>
    [Flags] public enum ProjCap1651 { None = 0, NoMulti = 1, NoPierce = 2, NoFork = 4, NoBounce = 8, NoHoming = 16 }
    public class SkillDef
    {
        public SkillId id; public string name, desc; public STag tags; public float baseDmg, cooldown; public bool aimable; public Status innate; public bool kit1651; public ProjCap1651 projCaps1651; // t1651: a Zhurong kit skill (own slot, own cooldown; reads only the support part of CdMul / AreaMul)
    }
    public class SupportDef
    {
        public SupportId id; public string name; string _desc; public STag needs;
        /// <summary>t1651: card description. Stat supports: generated from the mods + drawbacks and the tuning numbers (never drifts); hook supports: the written text.</summary>
        public string desc { get => HData.SupportDesc1651(this) ?? _desc; set => _desc = value; }
        public string rawDesc => _desc;
        public SMod1651[] mods, drawbacks; // t1651: stat changes (supports as data); drawbacks = PoE 2-style downsides (same stats)
        public string needsLabel; public STag requiresAll, excludes; public string category; public SupMetric1651 metric1651; public Status status1651; public bool retired1654; // t1654: retired = left in the table for index stability, never offered // t1651: autotest metric (+ the status for status supports) // t626u: needs = requiresAny; category: no two supports of the same category on one skill (only when set)
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
            // t1393u ice skills (PoE 2 / D2 inspired). innate = None: chill is applied by the skills themselves (stacking CHILL -> FREEZE, see IceChill) so tick hits don't re-freeze every frame.
            new SkillDef { id = SkillId.Frostbolt, name = "Frostbolt",  tags = STag.Area | STag.Hit | STag.Cold,                      baseDmg = 12, cooldown = 1.25f, aimable = true,  innate = Status.None, desc = "Slow frost orbs pierce through demons and leave chilling ground. They burst on walls. More orbs per level." },
            new SkillDef { id = SkillId.IceNova,   name = "Ice Nova",   tags = STag.Area | STag.Hit | STag.Cold,                      baseDmg = 16, cooldown = 3.0f,  aimable = false, innate = Status.None, desc = "A ring of frost pulses out from you. Chills, and shoves close demons hardest. Bigger and faster per level." },
            new SkillDef { id = SkillId.FrostBomb, name = "Frost Bomb", tags = STag.Area | STag.Zone | STag.Hit | STag.Cold | STag.Duration, baseDmg = 20, cooldown = 4.5f, aimable = false, innate = Status.None, desc = "Drops an ice crystal on a demon cluster. It pulses chill for 2s, then detonates for heavy cold damage." },
            new SkillDef { id = SkillId.Hurricane, name = "Hurricane",  tags = STag.Aura | STag.Area | STag.Hit | STag.Cold | STag.Duration, baseDmg = 4, cooldown = 9.0f, aimable = false, innate = Status.None, desc = "A cold storm circles you for several seconds, ticking damage and chilling. Lasts longer and wider per level." },
            // t1481 fire basic main attacks: player-aimed (cursor), LEFT MOUSE, hold = keep firing. SkillDef cooldown is 1.0; the real fire rate is HellordTuning.fbCd1481 (via SkillInst.CdMul).
            new SkillDef { id = SkillId.MagmaThrow, name = "Molten Spit", tags = STag.Projectile | STag.Area | STag.Hit | STag.Fire, baseDmg = 10, cooldown = 1f, aimable = true, innate = Status.Burn, desc = "Lobs a molten glob to the cursor. It bursts and throws 8 burning shards that scatter and scorch the ground. Hold to keep firing." },
            new SkillDef { projCaps1651 = ProjCap1651.NoPierce /* t1651: rolls through everything already */, id = SkillId.RollingMagmaRock, name = "Infernal Boulder", tags = STag.Projectile | STag.Area | STag.Hit | STag.Fire, /* t1651: + Area (blast / band / patch read AreaMul) */ baseDmg = 12, cooldown = 1f, aimable = true, innate = Status.Burn, desc = "Rolls a burning boulder toward the cursor. It pierces demons, leaves a burning line of ground and crumbles on walls. Hold to keep firing." },
            new SkillDef { id = SkillId.FirePillar, name = "Fire Pillar", tags = STag.Area | STag.Hit | STag.Fire, baseDmg = 8, cooldown = 1f, aimable = true, innate = Status.Burn, desc = "Marks the cursor, then a pillar of fire erupts there. Medium rate. Hold to keep firing." },
            // t1567 pact LMB basics (HellordPactBasics1567.cs): aimed at the cursor, LEFT MOUSE only. baseDmg / cooldown are overwritten from HellordTuning.pbBaseDmg1567 / pbCd1567 (PbDefs1567); the values here are the same first-pass numbers.
            new SkillDef { id = SkillId.SparkSpray, name = "Spark Spray", tags = STag.Projectile | STag.Hit | STag.Lightning, baseDmg = 4, cooldown = 0.6f, aimable = true, innate = Status.Shock, desc = "Aim with the cursor: 3-5 sparks skitter along the ground toward it, weaving around the aim line. Each spark hits one demon. Shocks." },
            new SkillDef { id = SkillId.BallLightning, name = "Ball Lightning", tags = STag.Area | STag.Hit | STag.Lightning | STag.Duration, baseDmg = 6, cooldown = 1.6f, aimable = true, innate = Status.Shock, desc = "Aim with the cursor: a slow orb drifts to it and past it, zapping nearby demons as it goes. Up to 3 orbs at once. Shocks." },
            new SkillDef { id = SkillId.ThunderJavelin, name = "Thunder Javelin", tags = STag.Projectile | STag.Area | STag.Hit | STag.Lightning | STag.Chain /* t1651: it chains */, baseDmg = 12, cooldown = 0.9f, aimable = true, innate = Status.Shock, desc = "Aim with the cursor: a fast bolt that bursts into short forked bolts on the first demon it hits. Shocks." },
            new SkillDef { id = SkillId.HeadsmansChop, name = "Headsman's Chop", tags = STag.Melee | STag.Area | STag.Hit | STag.Physical | STag.Blood, baseDmg = 14, cooldown = 0.85f, aimable = true, innate = Status.Bleed, desc = "Aim with the cursor: a short, wide axe arc that knocks demons back. Bleeding demons burst in a blood shockwave. Bleeds." },
            new SkillDef { id = SkillId.EarthSplittingSlam, name = "Earth-Splitting Slam", tags = STag.Melee | STag.Area | STag.Zone | STag.Hit | STag.Physical, baseDmg = 16, cooldown = 1.25f, aimable = true, innate = Status.None, desc = "Aim with the cursor: the axe smashes the ground just ahead. The cracks slow demons, then erupt again." },
            new SkillDef { projCaps1651 = ProjCap1651.NoPierce | ProjCap1651.NoFork | ProjCap1651.NoBounce | ProjCap1651.NoHoming /* t1651: one returning axe that cuts through everything; Multi = more axes */, id = SkillId.TwinAxeThrow, name = "Twin Axe Throw", tags = STag.Projectile | STag.Hit | STag.Physical, baseDmg = 9, cooldown = 0.7f, aimable = true, innate = Status.Bleed, desc = "Aim with the cursor: the axe spins out to it, piercing, then returns to your hand, hitting again. One axe out at a time. Bleeds." },
            new SkillDef { id = SkillId.PhantomAxes, name = "Phantom Axes", tags = STag.Melee | STag.Area | STag.Hit | STag.Physical | STag.Blood, baseDmg = 10, cooldown = 1.4f, aimable = true, innate = Status.Bleed, desc = "Aim with the cursor: 2-3 ghost Xingtians appear around it and each chops once. Bleeds." },
            // t1618 Zhurong kit: Fire Wall split out of Hellfire Pillar (the t948 wall code, now its own LMB basic), Volcanic Orb (new LMB basic),
            // ZhurongKit = damage source for the movement / special / ultimate slots (never offered, never in the belt; HellordZhurong1618.cs)
            new SkillDef { id = SkillId.FireWall, name = "Fire Wall", tags = STag.Area | STag.Zone | STag.Hit | STag.Fire | STag.Duration, baseDmg = 9, cooldown = 2.2f, aimable = true, innate = Status.Burn, desc = "Wall of flame at your cursor (in range), across your aim. Burns everything crossing it. Hold to keep placing." },
            new SkillDef { projCaps1651 = ProjCap1651.NoHoming /* t1651: its bolts already lock onto the nearest demon */, id = SkillId.Hydra, name = "Hydra", tags = STag.Projectile | STag.Hit | STag.Fire | STag.Duration, baseDmg = 4, cooldown = 1.0f, aimable = true, innate = Status.Burn, desc = "Places a fire hydra at the cursor for 5 s. Each of its 3 heads spits fire bolts at its own target. Up to 2 at once." }, // t1625: D2-style Hydra replaced Volcanic Orb
            new SkillDef { id = SkillId.ZhurongKit, name = "Zhurong Kit", tags = STag.Area | STag.Hit | STag.Fire, baseDmg = 10, cooldown = 0f, aimable = true, innate = Status.Burn, desc = "Zhurong's movement / special / ultimate skills (damage source)." },
            new SkillDef { id = SkillId.ZkEmberEffigy, name = "Ember Effigy", tags = STag.Movement | STag.Cooldown | STag.Duration | STag.Area | STag.Decoy, baseDmg = 10, cooldown = 0f, aimable = true, innate = Status.Burn, desc = "Dash leaves a decoy that lures demons", kit1651 = true },
            new SkillDef { id = SkillId.ZkEmberAnchor, name = "Ember Anchor", tags = STag.Movement | STag.Cooldown | STag.Duration, baseDmg = 10, cooldown = 0f, aimable = true, innate = Status.Burn, desc = "Dash marks an ember; dash again to snap back", kit1651 = true },
            new SkillDef { id = SkillId.ZkBackdraft, name = "Backdraft", tags = STag.Movement | STag.Cooldown | STag.Area, baseDmg = 10, cooldown = 0f, aimable = true, innate = Status.Burn, desc = "Dash pulls nearby demons to where you left", kit1651 = true },
            new SkillDef { id = SkillId.ZkPyreMark, name = "Pyre Mark", tags = STag.Special | STag.Cooldown | STag.Duration | STag.Mark | STag.Fire, baseDmg = 10, cooldown = 0f, aimable = true, innate = Status.Burn, desc = "Mark one demon; burn it down for a fire buff", kit1651 = true },
            new SkillDef { id = SkillId.ZkFlammability, name = "Flammability", tags = STag.Special | STag.Cooldown | STag.Duration | STag.Area | STag.Curse | STag.Zone, baseDmg = 10, cooldown = 0f, aimable = true, innate = Status.Burn, desc = "Curse an area: burns there hit harder", kit1651 = true },
            new SkillDef { id = SkillId.ZkWarCry, name = "War Cry", tags = STag.Special | STag.Cooldown | STag.Area | STag.Warcry | STag.Hit | STag.Fire, baseDmg = 10, cooldown = 0f, aimable = true, innate = Status.Burn, desc = "Roar: powered basics; roared demons explode on death", kit1651 = true },
            new SkillDef { id = SkillId.ZkSmolderRing, name = "Smolder Ring", tags = STag.Special | STag.Cooldown | STag.Duration | STag.Area | STag.Zone | STag.Hit | STag.Fire, baseDmg = 10, cooldown = 0f, aimable = true, innate = Status.Burn, desc = "A burning ring that slows and scorches", kit1651 = true },
            new SkillDef { id = SkillId.ZkHearthBlessing, name = "Hearth Blessing", tags = STag.Special | STag.Cooldown | STag.Duration | STag.Buff, baseDmg = 10, cooldown = 0f, aimable = true, innate = Status.Burn, desc = "Heal over a short window, cleanse slows", kit1651 = true },
            new SkillDef { id = SkillId.ZkSunsWrath, name = "Sun's Wrath", tags = STag.Ultimate | STag.Cooldown | STag.Conditional | STag.Area | STag.Hit | STag.Fire | STag.Duration, baseDmg = 10, cooldown = 0f, aimable = true, innate = Status.Burn, desc = "Charge with burn damage; drop a sun", kit1651 = true },
            new SkillDef { id = SkillId.ZkRebirthFlame, name = "Rebirth Flame", tags = STag.Ultimate | STag.Cooldown | STag.Triggered | STag.Area | STag.Hit | STag.Fire, baseDmg = 10, cooldown = 0f, aimable = true, innate = Status.Burn, desc = "Cheat death once; revive in a firestorm", kit1651 = true },
            new SkillDef { id = SkillId.ZkMoltenHeart, name = "Molten Heart", tags = STag.Ultimate | STag.Cooldown | STag.Conditional | STag.Buff | STag.Duration, baseDmg = 10, cooldown = 0f, aimable = true, innate = Status.Burn, desc = "Basics cast twice; you cannot be slowed", kit1651 = true },
            new SkillDef { id = SkillId.ZkVermilionBird, name = "Vermilion Bird", tags = STag.Ultimate | STag.Cooldown | STag.Conditional | STag.Area | STag.Hit | STag.Fire | STag.Duration, baseDmg = 10, cooldown = 0f, aimable = true, innate = Status.Burn, desc = "Charge with near misses; a firebird sweeps the room", kit1651 = true },
        };
        public static readonly SupportDef[] Supports =
        {
            new SupportDef { id = SupportId.MultipleProjectiles, mods = new[] { SMod1651.ArrI(SStat1651.ProjExtra, T => T.supMultiExtra, 1) }, drawbacks = new[] { SMod1651.Arr(SStat1651.DmgMul, T => T.multiProjDmgMul, 0.85f, false, d => HData.MultiHonoured1651(d.id)) }, metric1651 = SupMetric1651.Proj, category = "Projectile Count", name = "Multi Projectiles", needs = STag.Projectile, needsLabel = "Projectile", desc = "More projectiles per cast" },
            new SupportDef { id = SupportId.Pierce, mods = new[] { SMod1651.ArrI(SStat1651.Pierce, T => T.supPierce, 1) }, drawbacks = new[] { SMod1651.Arr(SStat1651.PierceLoss, T => T.pierceFalloff, 0.15f) }, metric1651 = SupMetric1651.Distinct, category = "Pierce",       name = "Pierce",         needs = STag.Projectile, needsLabel = "Projectile", desc = "Shots pierce more enemies" },
            new SupportDef { id = SupportId.Fork, mods = new[] { SMod1651.ArrI(SStat1651.ForkN, T => T.supForkChildren, 2), SMod1651.Arr(SStat1651.ForkDmg, T => T.supForkDmg, 0.6f) }, metric1651 = SupMetric1651.Distinct, category = "Fork",         name = "Fork",           needs = STag.Projectile, needsLabel = "Projectile", desc = "Splits on first hit" },
            new SupportDef { id = SupportId.Bounce, retired1654 = true, mods = new[] { SMod1651.ArrI(SStat1651.Bounce, T => T.supBounce, 1), SMod1651.Arr(SStat1651.BounceR, T => T.supBounceRangeU, 3f, true) }, metric1651 = SupMetric1651.Distinct, category = "Bounce",       name = "Bounce",         needs = STag.Projectile, needsLabel = "Projectile", desc = "Retired: owned Bounce converts to Chain" },
            new SupportDef { id = SupportId.Homing, mods = new[] { SMod1651.Arr(SStat1651.HomingR, T => T.supHomingRangeU, 4f), SMod1651.Arr(SStat1651.HomingTurn, T => T.supHomingTurn, 6f, true) }, metric1651 = SupMetric1651.Homing, category = "Homing",       name = "Homing",         needs = STag.Projectile, needsLabel = "Projectile", desc = "Projectiles curve toward enemies, tighter and farther per level" },
            new SupportDef { id = SupportId.BiggerArea, mods = new[] { SMod1651.Arr(SStat1651.AreaMul, T => T.supAreaMul, 1.25f) }, metric1651 = SupMetric1651.Area, category = "Area",   name = "Bigger Area",    needs = STag.Area,       needsLabel = "Area",       desc = "Bigger area: +15% at Lv1, up to +40%" },
            new SupportDef { id = SupportId.Echo, mods = new[] { SMod1651.ArrI(SStat1651.RepeatN, T => T.supEchoCount, 1), SMod1651.Arr(SStat1651.RepeatDmg, T => T.supEchoDmg, 0.5f) }, drawbacks = new[] { SMod1651.Arr(SStat1651.CdMul, T => T.echoCdMul1651, 1.15f) } /* t1654 per-level drawback */, metric1651 = SupMetric1651.Repeat, category = "Repeat",         name = "Echo",           needs = STag.Area,       needsLabel = "Area", excludes = STag.Minion | STag.Movement | STag.Special | STag.Ultimate, desc = "Repeats the cast once" },
            new SupportDef { id = SupportId.MoreOrbitals, mods = new[] { SMod1651.ArrI(SStat1651.Orbitals, T => T.supOrbitals, 1) }, metric1651 = SupMetric1651.Orbitals, category = "Orbital Count", name = "More Orbitals",  needs = STag.Orbital,    needsLabel = "Orbital",    desc = "More skulls: +1 at Lv1, up to +4" },
            new SupportDef { id = SupportId.Burn, drawbacks = new[] { SMod1651.Arr(SStat1651.DmgMul, T => T.supBurnHitDmg, 1f) }, metric1651 = SupMetric1651.Status, status1651 = Status.Burn, category = "Burn",         name = "Burn",           needs = STag.Hit,        needsLabel = "Hit",        desc = "Hits burn; burn is stronger and lasts longer per level. Hits deal less damage at Lv1-2." },
            new SupportDef { id = SupportId.Chill, metric1651 = SupMetric1651.Status, status1651 = Status.Chill, category = "Chill",        name = "Chill",          needs = STag.Hit,        needsLabel = "Hit",        desc = "Hits chill (slow 35%); chills last longer, Lv3 freezes in 2 chills" },
            new SupportDef { id = SupportId.Shock, metric1651 = SupMetric1651.Status, status1651 = Status.Shock, category = "Shock",        name = "Shock",          needs = STag.Hit,        needsLabel = "Hit",        desc = "Hits shock: +25% damage taken at Lv1, up to +40%" },
            new SupportDef { id = SupportId.Bleed, drawbacks = new[] { SMod1651.Arr(SStat1651.DmgMul, T => T.supBleedHitDmg, 1f) }, metric1651 = SupMetric1651.Status, status1651 = Status.Bleed, category = "Bleed",        name = "Bleed",          needs = STag.Hit,        needsLabel = "Hit",        desc = "Hits bleed; bleed is stronger per level. Hits deal less damage at Lv1-2." },
            new SupportDef { id = SupportId.AddedDamage, mods = new[] { SMod1651.Arr(SStat1651.DmgMul, T => T.supAddedDmg, 1.15f) }, metric1651 = SupMetric1651.Damage, category = "Damage",  name = "Added Damage",   needs = STag.Hit,        needsLabel = "Hit",        desc = "More damage: +15% at Lv1, up to +50%" },
            new SupportDef { id = SupportId.FasterCasting, mods = new[] { SMod1651.Arr(SStat1651.CdMul, T => T.supFasterCd, 0.9f) }, metric1651 = SupMetric1651.Cooldown, category = "Cooldown",name = "Faster Casting", needs = STag.Any,        needsLabel = "Any",        desc = "Shorter cooldown: -10% at Lv1, up to -22%" },
            new SupportDef { id = SupportId.Executioner, metric1651 = SupMetric1651.Execute, category = "Execute",  name = "Executioner",    needs = STag.Hit,        needsLabel = "Hit",        desc = "Bonus damage to demons below an HP threshold: 15% HP at Lv1, up to 25% at Lv3" },
            new SupportDef { id = SupportId.LifeLeech, metric1651 = SupMetric1651.Leech, category = "Leech",    name = "Life Leech",     needs = STag.Physical,   needsLabel = "Physical",   desc = "Physical hits heal you a little (capped per second). Burn and bleed do not leech." },
            new SupportDef { id = SupportId.Knockback, metric1651 = SupMetric1651.Knock, category = "Knockback",    name = "Knockback",      needs = STag.Hit,        needsLabel = "Hit",        desc = "Hits push demons away" },
            new SupportDef { id = SupportId.SoulHarvest, metric1651 = SupMetric1651.KillHook, category = "Harvest",  name = "Soul Harvest",   needs = STag.Hit,        needsLabel = "Hit",        desc = "Kills by this skill drop extra XP and sometimes bones" },
            new SupportDef { id = SupportId.ExplodeOnKill, metric1651 = SupMetric1651.KillHook, category = "On-Kill Blast",name = "Explode on Kill",needs = STag.Hit,        needsLabel = "Hit",        desc = "Demons this skill kills explode for a small area hit" },
            new SupportDef { id = SupportId.Chain, mods = new[] { SMod1651.ArrI(SStat1651.ChainExtra, T => T.supChainExtra1651, 1) }, drawbacks = new[] { SMod1651.Arr(SStat1651.DmgMul, T => T.chainDmgMul1651, 0.85f) }, metric1651 = SupMetric1651.Distinct, category = "Chain", name = "Chain", needs = STag.Chain, needsLabel = "Chain", desc = "More chain jumps" }, // t1651 stage 7; t1654 per-level drawback
            new SupportDef { id = SupportId.Duration, mods = new[] { SMod1651.Arr(SStat1651.DurMul, T => T.supDurMul1651, 1.15f) }, metric1651 = SupMetric1651.Duration, category = "Duration", name = "Duration", needs = STag.Duration, needsLabel = "Duration", desc = "Lasts longer" }, // t1651 stage 7
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
        /// <summary>t1651: card text for a list of stat mods at a level (kd = the skill, to skip mods that do not apply to it; null = show all).</summary>
        public static string ModText1651(SMod1651[] ms, int l, SkillDef kd)
        {
            if (ms == null) return ""; var T = HellordTuning.I; var sb = new System.Text.StringBuilder();
            string P(float f) => Mathf.RoundToInt(Math.Abs(f) * 100f) + "%";
            foreach (var m in ms)
            {
                if (m.hidden || (kd != null && m.when != null && !m.when(kd))) continue; float v = m.v(T, l); int n = Mathf.RoundToInt(v); string t;
                switch (m.stat)
                {
                    case SStat1651.DmgMul: t = (v >= 1f ? "+" : "-") + P(v - 1f) + " damage"; break;
                    case SStat1651.CdMul: t = (v <= 1f ? "-" : "+") + P(v - 1f) + " cooldown"; break;
                    case SStat1651.AreaMul: t = (v >= 1f ? "+" : "-") + P(v - 1f) + " area"; break;
                    case SStat1651.DurMul: t = (v >= 1f ? "+" : "-") + P(v - 1f) + " duration"; break;
                    case SStat1651.ProjExtra: t = $"+{n} projectiles"; break;
                    case SStat1651.Pierce: t = $"pierces {n} enemies"; break;
                    case SStat1651.PierceLoss: t = $"-{P(v)} dmg per pierce"; break;
                    case SStat1651.ForkN: t = $"splits in {n}"; break;
                    case SStat1651.ForkDmg: t = $"({P(v)} dmg)"; break;
                    case SStat1651.Bounce: t = $"bounces {n}x"; break;
                    case SStat1651.HomingR: t = $"homing range {v:0.#}"; break;
                    case SStat1651.ChainExtra: t = $"+{n} chain jumps"; break;
                    case SStat1651.RepeatN: t = $"repeats {n}x"; break;
                    case SStat1651.RepeatDmg: t = $"({P(v)} dmg)"; break;
                    case SStat1651.Orbitals: t = $"+{n} skulls"; break;
                    default: t = $"{m.stat} {v:0.##}"; break;
                }
                if (sb.Length > 0) sb.Append(t.StartsWith("(") ? " " : ", "); sb.Append(t);
            }
            return sb.ToString();
        }
        /// <summary>t1651: generated card description of a stat support (Lv1 and max-level values from the tuning, then its drawback). null = hook support (keeps its written text). t1654: max is Lv3.</summary>
        public static string SupportDesc1651(SupportDef sd)
        {
            if (sd == null || sd.mods == null || HellordTuning.I == null) return null;
            int mx = SkillInst.MaxSupLevel;
            string d1 = ModText1651(sd.drawbacks, 1, null), dx = ModText1651(sd.drawbacks, mx, null);
            string r = $"Lv1 {ModText1651(sd.mods, 1, null)}; Lv{mx} {ModText1651(sd.mods, mx, null)}";
            if (d1.Length > 0) r += d1 == dx ? $". Drawback: {d1}" : $". Drawback: {d1} (Lv{mx} {dx})";
            return r;
        }
        public static string SupportEffect(SupportId id, int l, int skillId = -1)
        {
            var T = HellordTuning.I; string P(float f) => Mathf.RoundToInt(f * 100f) + "%";
            if (id == SupportId.Echo && skillId == (int)SkillId.CurseRune) return $"repeats {At(T.supEchoCount, l)}x: {Mathf.Max(1, T.curseEchoRunes738)} smaller rune ({P(T.curseEchoRadiusMul738)} size, {P(At(T.supEchoDmg, l) * T.curseEchoDmgMul738)} dmg)"; // t818u: Curse Rune echo is its own rule (t738u)
            var sd0 = (int)id < Supports.Length ? Supports[(int)id] : null;
            if (sd0 != null && sd0.mods != null) { var kd = skillId >= 0 && skillId < Skills.Length ? Skills[skillId] : null; string e = ModText1651(sd0.mods, l, kd), dr = ModText1651(sd0.drawbacks, l, kd); return dr.Length > 0 ? e + ", " + dr : e; } // t1651: stat supports print their mods
            switch (id)
            {
                case SupportId.Burn: { string b = $"burn x{At(T.supBurnMul, l):0.0}, {T.burnDuration + At(T.supBurnExtraSeconds, l, 0f):0.#}s"; float h = At(T.supBurnHitDmg, l, 1f); if (h < 0.999f) b += $", -{P(1f - h)} hit dmg"; return b; }
                case SupportId.Chill: return $"chill x{At(T.supChillTime, l):0.0}, freeze in {At(T.supChillFreezeHits, l)}";
                case SupportId.Shock: return $"+{P(At(T.supShockAmp, l))} damage taken";
                case SupportId.Bleed: { string b = $"bleed x{At(T.supBleedMul, l):0.0}"; float h = At(T.supBleedHitDmg, l, 1f); if (h < 0.999f) b += $", -{P(1f - h)} hit dmg"; return b; }
                case SupportId.Executioner: return $"+{P(At(T.supExecDmg, l) - 1f)} dmg below {P(At(T.supExecThreshold, l))} HP";
                case SupportId.LifeLeech: return $"heal {P(At(T.supLeechFrac, l))} of physical hit, max {At(T.supLeechCapPerSec, l):0.#}/s";
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
        /// <summary>t1654: supports still in the table for index stability, but never offered (Bounce).</summary>
        public static bool Offered1654(SupportDef s) => s != null && !s.retired1654;
        /// <summary>t1654: Pierce support's per-level damage loss, or the innate base when the skill has no Pierce support.</summary>
        public static float PierceFalloffOf1651(SkillInst s) => s != null && s.Has(SupportId.Pierce) ? s.Stat(SStat1651.PierceLoss) : (HellordTuning.I != null ? HellordTuning.I.pierceFalloffBase : 0.15f);
        /// <summary>t1654: Multi's per-level damage multiplier when the skill really adds projectiles (fork children inherit this through the parent hit's damage, and archer extras apply it per arrow).</summary>
        public static float MultiDmgMul1651(SkillInst s) => s != null && s.ProjExtra > 0 ? At(HellordTuning.I.multiProjDmgMul, s.Lvl(SupportId.MultipleProjectiles), 0.85f) : 1f;

        /// <summary>v0.0.27 skill damage curve: 1 + step x (level - 1) (step 0.06, was 0.3), no max-level spike (was x1.5). Skill level now grows COVERAGE, not damage.</summary>
        public static float LevelMul(int lvl) { var T = HellordTuning.I; return (1f + T.skillLevelStep * (lvl - 1)) * (lvl >= MaxSkillLevel ? T.skillMaxSpike : 1f); }
        public static int BonusSlots; // v0.0.37: boss reward "New Skill Slot" cards (the bar shows 3 slots, so the total stays <= 3)
        public static int SlotsUnlocked(int playerLevel) { var T = HellordTuning.I; return Mathf.Min(Mathf.Max(1, T.skillSlotsMax818), 1 + (playerLevel >= T.slot2Level ? 1 : 0) + (playerLevel >= T.slot3Level ? 1 : 0) + (playerLevel >= T.slot4Level818 ? 1 : 0) + (playerLevel >= T.slot5Level818 ? 1 : 0) + BonusSlots); } // t818u: up to 5 skills
        /// <summary>v0.0.27 XP curve (tuning): (95 + 22L + 0.5L^2) x xpCostMul; v0.0.26 was 5 + 4L + L^2/4.</summary>
        public static int XpToNext(int level) { var T = HellordTuning.I; return Math.Max(1, (int)Math.Round((T.xpBase + T.xpPerLevel * level + T.xpQuad * level * level) * T.xpCostMul)); }
        /// <summary>Safe table lookup by level (1-based), clamped to the table; a missing/short table falls back to its last/first value.</summary>
        public static float At(float[] a, int lvl, float fb = 1f) { if (a == null || a.Length == 0) return fb; return a[Math.Max(0, Math.Min(a.Length - 1, lvl - 1))]; }
        public static int At(int[] a, int lvl, int fb = 1) { if (a == null || a.Length == 0) return fb; return a[Math.Max(0, Math.Min(a.Length - 1, lvl - 1))]; }
        /// <summary>t1651: skills whose cast really reads Multi Projectiles (more projectiles). Only these pay its damage penalty.</summary>
        public static bool MultiHonoured1651(SkillId id) => id == SkillId.Fireball || id == SkillId.SoulSpears || id == SkillId.SkeletonArchers || id == SkillId.RollingMagmaRock || id == SkillId.SparkSpray || id == SkillId.ThunderJavelin || id == SkillId.TwinAxeThrow || id == SkillId.Hydra; // t1651 stage 5: + the shared-helper skills
        public static bool IsKit1651(SkillId id) => id >= SkillId.ZkEmberEffigy && id <= SkillId.ZkVermilionBird;
        public static bool Summon561(SkillDef d) => d != null && (d.id == SkillId.SkeletonWarriors || d.id == SkillId.SkeletonArchers); // t561u
        public static int MaxLv(SkillDef d) => Summon561(d) ? Math.Max(1, HellordTuning.I.summonMaxLv561) : MaxSkillLevel; // t587u: summons max Lv5 like every skill
        public static bool Fits(SupportDef s, SkillDef k) => (s.needs == STag.Any || (k.tags & s.needs) != 0) && (k.tags & s.requiresAll) == s.requiresAll && (k.tags & s.excludes) == 0 && (k.projCaps1651 & ProjCapOf1651(s.id)) == 0; // t1651: + projectile caps
        public static ProjCap1651 ProjCapOf1651(SupportId id) => id == SupportId.MultipleProjectiles ? ProjCap1651.NoMulti : id == SupportId.Pierce ? ProjCap1651.NoPierce : id == SupportId.Fork ? ProjCap1651.NoFork : id == SupportId.Bounce ? ProjCap1651.NoBounce : id == SupportId.Homing ? ProjCap1651.NoHoming : ProjCap1651.None; // t626u
        static readonly STag[] _tagOrder626 = { STag.Projectile, STag.Melee, STag.Strike, STag.Area, STag.Chain, STag.Orbital, STag.Aura, STag.Zone, STag.Minion, STag.Duration, STag.Curse, STag.Hit, STag.Physical, STag.Fire, STag.Cold, STag.Lightning, STag.Blood, STag.Bone, STag.Soul, STag.Movement, STag.Special, STag.Ultimate, STag.Cooldown, STag.Mark, STag.Warcry, STag.Buff, STag.Conditional, STag.Triggered, STag.Decoy };
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
            if ((k.projCaps1651 & ProjCapOf1651(s.id)) != 0) parts.Add($"{k.name}'s projectile can't use it"); // t1651
            return parts.Count == 0 ? null : string.Join(", ", parts);
        }
    }

    /// <summary>A skill owned by the player: level + socketed supports.</summary>
    public class SkillInst
    {
        public const int MaxSupLevel = 3; // t1654: was 5
        public SkillDef def; public int level = 1; public readonly List<SupportId> sup = new List<SupportId>(4);
        public readonly int[] supLvl = new int[32]; // v0.0.36: 32 (19 supports now); v0.0.27: level of each socketed support (0 or missing = Lv1 when socketed through sup.Add)
        public float cd, echoT = -1; public int echoLeft;
        bool _san1654;
        public int OpenSlots => Math.Min(level, HData.MaxSupports) - sup.Count;
        public bool Has(SupportId s) { Sanitize1654(); return sup.Contains(s); }
        /// <summary>t1654: clamp levels above max; convert a saved Bounce socket to Chain (same level, or the higher of the two if Chain is already on). If the skill can't take Chain, Bounce is dropped and the slot is freed.</summary>
        public void Sanitize1654()
        {
            if (_san1654) return; _san1654 = true;
            for (int i = 0; i < supLvl.Length; i++) if (supLvl[i] > MaxSupLevel) supLvl[i] = MaxSupLevel;
            int bi = (int)SupportId.Bounce; if (!sup.Contains(SupportId.Bounce)) return;
            int bl = Math.Max(1, Math.Min(MaxSupLevel, supLvl[bi]));
            sup.Remove(SupportId.Bounce); supLvl[bi] = 0;
            var chain = HData.Support(SupportId.Chain);
            if (sup.Contains(SupportId.Chain)) supLvl[(int)SupportId.Chain] = Math.Max(Math.Max(1, Math.Min(MaxSupLevel, supLvl[(int)SupportId.Chain])), bl);
            else if (def != null && chain != null && HData.Fits(chain, def)) { sup.Add(SupportId.Chain); supLvl[(int)SupportId.Chain] = bl; }
        }
        /// <summary>Support level 1..MaxSupLevel (0 = not socketed). t1654: max is 3.</summary>
        public int Lvl(SupportId s) { Sanitize1654(); return sup.Contains(s) ? Math.Max(1, Math.Min(MaxSupLevel, supLvl[(int)s])) : 0; }
        public bool CanUpgrade(SupportDef s) => Has(s.id) && Lvl(s.id) < MaxSupLevel;
        public void AddSupport(SupportId s) { Sanitize1654(); if (s == SupportId.Bounce) { var ch = HData.Support(SupportId.Chain); if (def != null && ch != null && (sup.Contains(SupportId.Chain) || HData.Fits(ch, def))) s = SupportId.Chain; else return; } if (Has(s)) { supLvl[(int)s] = Math.Min(MaxSupLevel, Lvl(s) + 1); } else { sup.Add(s); supLvl[(int)s] = 1; } }
        /// <summary>null = can socket, otherwise the warning text shown to the player.</summary>
        public string CanSocket(SupportDef s)
        {
            Sanitize1654();
            if (s == null || s.retired1654) return s == null ? "Unknown support." : $"{s.name} is no longer used.";
            if (!HData.Fits(s, def)) return $"{s.name} can't go on {def.name}: {HData.Mismatch(s, def)}."; // t626u: lists missing/blocking tags
            if (!string.IsNullOrEmpty(s.category) && !Has(s.id)) foreach (var o in sup) { var od = HData.Supports[(int)o]; if (od.category == s.category) return $"{def.name} already has a {s.category} support ({od.name})."; } // t626u: category no-stack (only when set)
            if (Has(s.id)) return Lvl(s.id) < MaxSupLevel ? null : $"{def.name}: {s.name} is already max level."; // owned + below max = an UPGRADE
            if (OpenSlots <= 0) return level < HData.MaxSupports ? $"{def.name} has no open slot (level it up to Lv{level + 1})." : $"{def.name} support slots are full.";
            return null;
        }
        /// <summary>t1651: a named stat summed over the socketed supports' mods and drawbacks (Mul stats start at 1, others at 0). Skill code reads these instead of naming supports.</summary>
        public float Stat(SStat1651 st)
        {
            Sanitize1654(); var T = HellordTuning.I; bool mul = SMod1651.IsMul(st); float v = mul ? 1f : 0f;
            for (int i = 0; i < sup.Count; i++) { var d = HData.Supports[(int)sup[i]]; int l = Lvl(sup[i]); v = Ap1651(d.mods, st, l, v, mul, T); v = Ap1651(d.drawbacks, st, l, v, mul, T); }
            return v;
        }
        float Ap1651(SMod1651[] ms, SStat1651 st, int l, float v, bool mul, HellordTuning T)
        {
            if (ms == null) return v;
            foreach (var m in ms) if (m.stat == st && (m.when == null || m.when(def))) { float x = m.v(T, l); v = mul ? v * x : v + x; }
            return v;
        }
        public int StatI(SStat1651 st) => Mathf.RoundToInt(Stat(st));
        public int ProjExtra => StatI(SStat1651.ProjExtra);
        public int PierceN => StatI(SStat1651.Pierce);
        public int ForkN => StatI(SStat1651.ForkN);
        public int BounceN => StatI(SStat1651.Bounce);
        public float HomingR => Stat(SStat1651.HomingR);
        public int RepeatN => StatI(SStat1651.RepeatN);
        public int ChainExtra => StatI(SStat1651.ChainExtra);
        /// <summary>t1651: support-made duration multiplier (sins etc. stay separate).</summary>
        public float DurMul => Stat(SStat1651.DurMul);
        public float Dur(float secs) => secs * DurMul;
        /// <summary>t1651: THE area helper. R = a length that scales linearly with area (ranges, reaches, wall size); RA = a radius that scales with the square root (blast discs). Both read AreaMul.</summary>
        public float R(float r) => r * AreaMul;
        public float RA(float r) => r * Mathf.Sqrt(Mathf.Max(0f, AreaMul));
        public float More() => Stat(SStat1651.DmgMul); // t1651: Added Damage, Multi's penalty (only where Multi adds projectiles), future damage mods
        public float CdMul => def.kit1651 ? Stat(SStat1651.CdMul) : HellordGame.SinSkCdAt818(def.id) * HellordGame.FbCd1481(def.id) * Stat(SStat1651.CdMul) * HellordGame.PCd49 * ((def.tags & (STag.Zone | STag.Orbital)) != 0 ? HellordGame.SlothCd53 : 1f) * HellordGame.PactCdS53 * HellordGame.SinCd737; // v0.0.49 Intensity; v0.0.53 Sloth Throne / Price of Wrath
        public float AreaMul => def.kit1651 ? Stat(SStat1651.AreaMul) : HellordGame.SinSkAreaAt818(def.id) * Stat(SStat1651.AreaMul) * HellordGame.PArea49 * ((def.tags & (STag.Zone | STag.Orbital)) != 0 ? HellordGame.SlothArea53 : 1f) * HellordGame.BoneArea536(def) * HellordGame.SinArea737; // t536u Grave Reach / Grave Colossus // v0.0.49 aggression x Intensity
    }
}
