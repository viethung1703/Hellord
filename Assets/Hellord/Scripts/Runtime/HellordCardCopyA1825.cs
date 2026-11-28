using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    // t1825 Option A (PoE2 tooltip card): the short card copy for EVERY skill, kit skill, support and stat card.
    // Copy lives here (data), the layout lives in HellordCardsA1825.cs. Numbers are never typed in: every number is a
    // {token} read live from HellordTuning / the skill instance / named game constants (HellordCardEval1825).
    //
    // Line grammar:  [?cond?]K:text
    //   K = S stat line (dim labels, gold numbers)   F fact line (white)   U per-level fact (new + level-up cards)
    //       D red drawback stat line                  X red drawback fact line
    //   ?cond?  the line shows only when the expression is not 0 (e.g. ?T.executionerSkipBigs?).
    //   {expr|fmt}  expr: numbers, + - * / ( ), < > <= >= == !=, !x, min(a,b), max(a,b), len(T.arr)
    //               names: T.field (HellordTuning), G.CONST (HellordGame), D.CONST (HData), S.SkillName (id),
    //                      T.arr@L (card level, clamped) / T.arr@3 / T.arr@MAX, and the context vars
    //                      L (level shown), A (level before), cd (def cooldown x run/support cooldown mul),
    //                      cdm (cooldown mul), dm (duration mul), lvdmg (damage gain A->L), tgt (TO skill id or -1),
    //                      multi (TO skill really adds projectiles), summon (warrior/archer card), maxlv (skill max)
    //               fmt: i int, s 0.## (seconds), f 0.##, p x100 int, x 0.0#
    // Rules (VH t1825): one fact per line, <= 19 characters, <= 5 fact lines, about 25 words a card.
    // Only status keywords are coloured (StatusWords); numbers gold, drawbacks red, the S unit dim.
    // =====================================================================================================================
    public static class CardCopyA1825
    {
        /// <summary>Status keywords and their colour (multi-word first). Only these words get a colour in card copy.</summary>
        public static readonly string[] StatusWords = { "FIRE TRAIL", "BURNING", "BURNS", "BURN", "CHILLING", "CHILLED", "CHILLS", "CHILL", "FREEZES", "FREEZE", "FROZEN", "SHOCKED", "SHOCKS", "SHOCK", "BLEEDING", "BLEEDS", "BLEED", "CURSED", "CURSES", "CURSE" };
        public static Color StatusCol(string w)
        {
            if (w.StartsWith("BURN") || w == "FIRE TRAIL") return new Color32(0xf7, 0x76, 0x22, 255);
            if (w.StartsWith("CHILL") || w.StartsWith("FREEZ") || w == "FROZEN") return new Color32(0x00, 0x99, 0xdb, 255);
            if (w.StartsWith("SHOCK")) return new Color32(0xfe, 0xe7, 0x61, 255);
            if (w.StartsWith("BLEED")) return new Color32(0xe4, 0x3b, 0x44, 255);
            return new Color32(0xb5, 0x50, 0x88, 255); // curse
        }
        /// <summary>Element tags in their own colour on the tag line; mechanic tags are dim.</summary>
        public static bool TagCol(STag t, out Color c)
        {
            switch (t)
            {
                case STag.Fire: c = new Color32(0xf7, 0x76, 0x22, 255); return true;
                case STag.Cold: c = new Color32(0x00, 0x99, 0xdb, 255); return true;
                case STag.Lightning: c = new Color32(0xfe, 0xe7, 0x61, 255); return true;
                case STag.Physical: c = new Color32(0xc0, 0xcb, 0xdc, 255); return true;
                case STag.Bone: c = new Color32(0xea, 0xd4, 0xaa, 255); return true;
                case STag.Blood: c = new Color32(0xe4, 0x3b, 0x44, 255); return true;
                case STag.Soul: c = new Color32(0xb5, 0x50, 0x88, 255); return true;
            }
            c = default; return false;
        }
        /// <summary>Tags never printed on the tag line: the kit slot is the type line ("SPECIAL - RMB"), every kit skill has a
        /// cooldown, and "Conditional" is what the ARMED fact lines explain.</summary>
        public const STag HiddenTags = STag.Movement | STag.Special | STag.Ultimate | STag.Cooldown | STag.Conditional;

        /// <summary>Shared lines of an owned-skill LEVEL card (before the skill's own S and U lines).</summary>
        public static readonly string[] LevelHead = { "S:LV {A} - {L}" };
        public static readonly string[] LevelTail = { "?!summon?S:DAMAGE +{lvdmg|p}%", "?L<=D.MaxSupports?S:SUPPORT SLOTS {L}", "?L>=maxlv?F:MAX LEVEL" };
        /// <summary>Support MAX line + the kit/minion exclusion line are built from data by the layout.</summary>

        public static readonly Dictionary<SkillId, string[]> Skill = new Dictionary<SkillId, string[]>
        {
            // ---- first basics
            { SkillId.Fireball, new[] { "S:COOLDOWN {cd|s}S", "S:FIREBALLS {T.fbProjectiles@L|i}", "F:AIMED SHOT", "F:BURSTS ON IMPACT", "F:LIGHT BURN", "U:MORE FIREBALLS" } },
            { SkillId.FrostCleave, new[] { "S:COOLDOWN {cd|s}S", "F:CHILLS IN FRONT", "F:{G.StFreezeHits1825|i} CHILLS: FREEZE", "F:+{T.frostChilledBonus-1|p}% VS CHILLED", "?T.frostEraseBullets49?F:ERASES BULLETS", "U:WIDER AND LONGER" } },
            { SkillId.StormRing, new[] { "S:COOLDOWN {cd|s}S", "F:RING AROUND YOU", "F:SHOCKS", "F:SHOCKED TAKE +{T.shockBaseAmp|p}%", "U:WIDER RING PER LV" } },
            { SkillId.BoneOrbit, new[] { "S:SKULLS {T.orbitCount@L|i}", "F:SKULLS CIRCLE YOU", "F:BLEEDS", "U:MORE SKULLS PER LV" } },
            { SkillId.CurseRune, new[] { "S:COOLDOWN {cd|s}S", "S:RUNES {T.curseRunes@L|i}", "F:RUNE AT AIM POINT", "F:CURSES DEMONS", "F:CURSED EXPLODE", "F:ON DEATH", "U:BIGGER RUNES" } },
            { SkillId.ChainLightning, new[] { "S:COOLDOWN {cd|s}S", "S:JUMPS {T.chainJumps@L|i}", "F:BOLT JUMPS BETWEEN", "F:NEARBY DEMONS", "F:SHOCKS", "U:MORE JUMPS PER LV" } },
            { SkillId.HellfirePillar, new[] { "S:COOLDOWN {cd|s}S", "S:PILLARS {T.pillarCount@L|i}", "F:ERUPTS UNDER", "F:DEMON PACKS", "F:BURNS", "U:MORE, WIDER PER LV" } },
            { SkillId.BloodWhip, new[] { "S:COOLDOWN {cd|s}S", "S:LASHES {T.whipLashes@L|i}", "F:LONG LASH IN FRONT", "F:HEALS {T.whipHealPerHit@L|f} PER HIT", "F:MAX {T.whipHealCapPerCast|s} HP A CAST", "F:BLEEDS", "U:MORE REACH, LASHES" } },
            { SkillId.SoulSpears, new[] { "S:COOLDOWN {cd|s}S", "S:SPEARS {T.spearCount@L|i}", "S:PIERCE {T.spearPierce@L|i}", "F:SPEARS PIERCE", "F:A LINE TO THE", "F:NEAREST DEMON", "U:MORE SPEARS+PIERCE" } },
            { SkillId.GravePool, new[] { "S:POOLS LAST {T.gravePoolSeconds@L*dm|s}S", "S:SLOW {T.graveSlow@L|p}%", "F:DROPS AS YOU WALK", "F:SLOWS AND DAMAGES", "U:BIGGER AND LONGER" } },
            { SkillId.SkeletonWarriors, new[] { "S:SKELETONS {T.skelWarCount561@L|i}", "S:DAMAGE {T.skelWarDmg561@L|i}", "S:RESPAWN {T.skelWarResp561@L|s}S", "F:SWORD-AND-SHIELD", "F:FIGHT UP FRONT", "F:BLOCK BULLETS", "F:RISE AGAIN ON DEATH", "F:LEFT CLICK: COMMAND" } },
            { SkillId.SkeletonArchers, new[] { "S:ARCHERS {T.skelArcCount561@L|i}", "S:DAMAGE {T.skelArcDmg561@L|i}", "S:RESPAWN {T.skelArcResp561@L|s}S", "F:HANG BACK, SHOOT", "F:BONE ARROWS", "F:RISE AGAIN ON DEATH", "F:LEFT CLICK: COMMAND", "U:MORE ARCHERS" } },
            // ---- ice
            { SkillId.Frostbolt, new[] { "S:COOLDOWN {cd*T.frostbCdMul@L|s}S", "S:ORBS {T.frostbOrbs@L|i}", "F:SLOW FROST ORBS", "F:PIERCE DEMONS", "F:CHILL THE GROUND", "F:BURST ON WALLS", "U:MORE ORBS PER LV" } },
            { SkillId.IceNova, new[] { "S:COOLDOWN {cd*T.novaCdMul@L|s}S", "S:PULSES {T.novaPulses@L|i}", "F:FROST RING PULSES", "F:OUT FROM YOU", "F:CHILLS", "F:SHOVES CLOSE FOES", "U:BIGGER AND FASTER" } },
            { SkillId.FrostBomb, new[] { "S:COOLDOWN {cd*T.bombCdMul@L|s}S", "S:BOMBS {T.bombCount@L|i}", "F:ICE CRYSTAL", "F:LANDS ON A PACK", "F:CHILL PULSES {T.bombSeconds@L*dm|s}S", "F:THEN DETONATES", "F:FOR HEAVY COLD DMG" } },
            { SkillId.Hurricane, new[] { "S:LASTS {T.hurrSeconds@L*dm|s}S", "S:COOLDOWN {cd*T.hurrCdMul@L|s}S", "F:STORM CIRCLES YOU", "F:DAMAGES AND CHILLS", "U:LONGER+WIDER PER LV" } },
            // ---- Zhurong basics
            { SkillId.MagmaThrow, new[] { "S:COOLDOWN {cd|s}S", "F:LOBS A MOLTEN GLOB", "F:BURSTS: {T.msStrands1481|i} SHARDS", "F:SCORCHES GROUND", "F:BURNS", "F:HOLD TO KEEP FIRING" } },
            { SkillId.RollingMagmaRock, new[] { "S:COOLDOWN {cd|s}S", "F:ROLLS TO THE CURSOR", "F:PIERCES AND BURNS", "F:LEAVES A FIRE TRAIL", "F:BREAKS ON WALLS", "F:HOLD TO KEEP FIRING" } },
            { SkillId.FirePillar, new[] { "S:COOLDOWN {cd|s}S", "F:MARKS THE CURSOR", "F:THEN FIRE ERUPTS", "F:BURNS", "F:HOLD TO KEEP FIRING" } },
            { SkillId.FireWall, new[] { "S:COOLDOWN {cd|s}S", "S:LASTS {(T.fireWallSec737+T.fireWallSecPerLvl737*(L-1))*dm|s}S", "F:WALL AT THE CURSOR", "F:ACROSS YOUR AIM", "F:BURNS ALL CROSSING", "F:HOLD: KEEP PLACING", "U:LONGER, STRONGER" } },
            { SkillId.Hydra, new[] { "S:COOLDOWN {cd|s}S", "S:LASTS {T.hyLife1625*dm|s}S", "F:HYDRA AT CURSOR", "F:{G.ZhHydraHeads1825|i} HEADS SPIT FIRE", "F:OWN TARGET EACH", "F:UP TO {T.hyMax1625|i} AT ONCE", "F:BURNS" } },
            { SkillId.ZhurongKit, new[] { "F:KIT DAMAGE SOURCE" } },
            // ---- Xingtian / pact basics
            { SkillId.SparkSpray, new[] { "S:COOLDOWN {cd|s}S", "S:SPARKS {T.sparkCount1567@L|i}", "F:SKITTER TO CURSOR", "F:WEAVE AROUND AIM", "F:EACH HITS ONE FOE", "F:SHOCKS", "U:UP TO {T.sparkCount1567@MAX|i} SPARKS" } },
            { SkillId.BallLightning, new[] { "S:COOLDOWN {cd|s}S", "S:ZAP TARGETS {T.orbZapTargets1567@L|i}", "F:SLOW ORB TO CURSOR", "F:ZAPS AS IT GOES", "F:UP TO {T.orbCap1567|i} AT ONCE", "F:SHOCKS" } },
            { SkillId.ThunderJavelin, new[] { "S:COOLDOWN {cd|s}S", "?T.javChain1567?S:CHAINS {T.javChainJumps1567@L|i}", "F:FAST BOLT TO AIM", "?T.javChain1567?F:CHAINS ON 1ST HIT", "?!T.javChain1567?F:FORKS ON 1ST HIT", "F:SHOCKS" } },
            { SkillId.HeadsmansChop, new[] { "S:COOLDOWN {cd|s}S", "F:SHORT WIDE AXE ARC", "F:KNOCKS DEMONS BACK", "F:BLEEDING FOE BURSTS", "F:MAX {T.chopWaveMax1567|i} BURSTS A CHOP", "F:BLEEDS" } },
            { SkillId.EarthSplittingSlam, new[] { "S:COOLDOWN {cd|s}S", "F:AXE SMASHES AHEAD", "F:CRACKS SLOW DEMONS", "F:ERUPT AFTER {T.eruptDelayS1567|s}S" } },
            { SkillId.TwinAxeThrow, new[] { "S:COOLDOWN {cd|s}S", "F:SPINS TO CURSOR", "F:PIERCES, RETURNS", "F:HITS AGAIN", "F:ONE AXE AT A TIME", "F:BLEEDS" } },
            { SkillId.PhantomAxes, new[] { "S:COOLDOWN {cd|s}S", "S:GHOSTS {T.phCount1567@L|i}", "F:GHOST XINGTIANS", "F:APPEAR AT CURSOR", "F:EACH CHOPS ONCE", "F:BLEEDS" } },
            // ---- Leizu basics
            { SkillId.ThunderDrumStrikes, new[] { "S:COOLDOWN {cd|s}S", "F:STRIKES NEAR AIM", "F:BEAT {G.LzClapBeat1825|i}: THUNDERCLAP", "F:X{T.dsClapShocked1666|x} VS SHOCKED", "F:SHOCKS" } },
            { SkillId.ThunderLash, new[] { "S:COOLDOWN {cd|s}S", "S:DAMAGE X{T.tlDmg1666|x}", "F:WIDE ARC IN FRONT", "F:PUSHES SHOCKED", "F:SHOCKS" } },
            { SkillId.LiveWire, new[] { "S:COOLDOWN {cd|s}S", "S:MAX HOLD {T.lwMaxHold1666|s}S", "F:HOLD: BEAM LOCKS", "F:AFTER {T.lwOverS1666|s}S: OVERLOAD", "F:STRONGER, SHOCKS", "F:MOVE X{T.lwMoveMul1666|x} HOLDING" } },
            { SkillId.ThunderHound, new[] { "S:COOLDOWN {cd|s}S", "S:LASTS {T.hdLife1666*dm|s}S", "F:BEAST AT CURSOR", "F:BITES NEARBY FOES", "F:UP TO {T.hdMax1666|i} AT ONCE", "F:SHOCKS" } },
            // ---- Gonggong basics
            { SkillId.TidalSurge, new[] { "S:COOLDOWN {cd|s}S", "F:WAVE TO THE CURSOR", "F:CARRIES DEMONS", "F:CRASHES AND CHILLS" } },
            { SkillId.Whirlpool, new[] { "S:COOLDOWN {cd|s}S", "F:VORTEX AT CURSOR", "F:SUCKS DEMONS IN", "F:COLLAPSES, CHILLS", "F:AT MOST {T.wpMax1682|i}" } },
            { SkillId.BreathOfTheDeep, new[] { "S:COOLDOWN {cd|s}S", "S:MAX HOLD {T.brMax1682|s}S", "F:HOLD: FROST CONE", "F:CONE CHILLS", "F:{T.brFreezeAt1682|s}S IN IT: FREEZE", "F:MOVE X{T.brMove1682|x} HOLDING" } },
            { SkillId.BuzhouRift, new[] { "S:COOLDOWN {cd|s}S", "F:FISSURE TO CURSOR", "F:ENDS IN BIG SPIKE", "F:SHATTERS FROZEN", "F:FOR X{T.rfFrozen1682|x} DAMAGE" } },
            { SkillId.EyeOfTheBlackWaters, new[] { "S:COOLDOWN {cd|s}S", "F:SLOW ORB PAST AIM", "F:SPIRALS ICE SHARDS", "F:AT MOST {T.eyeMax1682|i}" } },
            // ---- Zhong Kui basic
            { SkillId.Teeth, new[] { "S:COOLDOWN {cd|s}S", "F:SHORT FAN OF BONE", "F:CHIPS TO CURSOR" } },

            // ================= kit skills (cooldowns are the kit tuning x the support cooldown mul; durations x Duration)
            // Zhurong
            { SkillId.ZkEmberEffigy, new[] { "S:COOLDOWN {T.effCd1618*cdm|s}S", "S:DECOY {T.effLife1618*dm|s}S", "F:DASH LEAVES A", "F:BURNING DECOY", "F:IT LURES DEMONS" } },
            { SkillId.ZkEmberAnchor, new[] { "S:COOLDOWN {T.ancCd1618*cdm|s}S", "S:WINDOW {T.ancWindow1618*dm|s}S", "F:DASH MARKS EMBER", "F:DASH AGAIN TO SNAP", "F:BACK TO IT" } },
            { SkillId.ZkBackdraft, new[] { "S:COOLDOWN {T.bdCd1618*cdm|s}S", "S:PULL {T.bdPull1618|p}% OF GAP", "F:DASH START POINT", "F:PULLS DEMONS IN" } },
            { SkillId.ZkPyreMark, new[] { "S:COOLDOWN {T.pmCd1618*cdm|s}S", "S:MARK {T.pmLife1618*dm|s}S", "F:MARK NEAR CURSOR", "F:BURN {T.pmThresh1618|p}% OF ITS HP", "F:+{T.pmBuffMul1618-1|p}% FIRE FOR {T.pmBuffS1618|s}S" } },
            { SkillId.ZkFlammability, new[] { "S:COOLDOWN {T.flCd1618*cdm|s}S", "S:CURSE {T.flDur1618*dm|s}S", "F:CURSE AN AREA", "F:BURN BUILDS X{T.flBurnK1618|x}", "F:BURN TICKS X{T.flBurnDmg1618|x}" } },
            { SkillId.ZkWarCry, new[] { "S:COOLDOWN {T.wcCd1625*cdm|s}S", "S:POWERED BASICS {T.wcCharges1625|i}", "F:BASICS HIT X{T.wcMul1625|x}", "F:ROARED DEMONS", "F:EXPLODE ON DEATH", "F:FOR {T.wcMarkS1625|s}S AFTER ROAR" } },
            { SkillId.ZkSmolderRing, new[] { "S:COOLDOWN {T.srCd1618*cdm|s}S", "S:LASTS {T.srLife1618*dm|s}S", "F:RING AT CURSOR", "F:SLOWS {T.srSlow1618|p}%", "F:SCORCHES DEMONS" } },
            { SkillId.ZkHearthBlessing, new[] { "S:COOLDOWN {T.hbCd1618*cdm|s}S", "S:HEALS {T.hbHeal1618|p}% MAX HP", "F:OVER {T.hbDur1618*dm|s}S", "F:CLEANSES SLOWS", "F:RESISTS DEBUFFS" } },
            { SkillId.ZkSunsWrath, new[] { "S:COOLDOWN {T.swCd1618*cdm|s}S", "S:METER {T.swMeter1618|i} BURN", "F:BURN DMG FILLS IT", "F:THEN A SUN FALLS", "F:BURNING GROUND {T.swAreaS1618*dm|s}S" } },
            { SkillId.ZkRebirthFlame, new[] { "S:COOLDOWN {T.rbCd1618*cdm|s}S", "S:REVIVE {T.rbHp1618|p}% HP", "F:AUTOMATIC", "F:A KILLING HIT", "F:REVIVES YOU", "F:IN A FIRESTORM", "F:{T.rbInv1618|s}S INVULNERABLE" } },
            { SkillId.ZkMoltenHeart, new[] { "S:COOLDOWN {T.mhCd1618*cdm|s}S", "S:LASTS {T.mhDur1618*dm|s}S", "F:ARMED: {T.mhNeed1618|i} BURNING", "F:BASICS CAST TWICE", "F:CAN'T BE SLOWED" } },
            { SkillId.ZkVermilionBird, new[] { "S:COOLDOWN {T.vbCd1618*cdm|s}S", "S:NEAR MISSES {T.vbNeed1618|i}", "F:NEAR-MISS DASHES", "F:ARM A FIREBIRD", "F:SWEEPS YOUR AIM", "F:BURNING STRIP {T.vbStripS1618*dm|s}S" } },
            // Leizu
            { SkillId.LzThunderStep, new[] { "S:COOLDOWN {T.tsCd1666*cdm|s}S", "S:RANGE X{T.tsDistMul1666|x}", "F:DASH: TELEPORT", "F:AIM NEAR BALL", "F:LIGHTNING TO JUMP", "F:INTO IT" } },
            { SkillId.LzGroundingWard, new[] { "S:COOLDOWN {T.gwCd1666*cdm|s}S", "S:WARD {T.gwDur1666*dm|s}S", "F:AFTER A DASH", "F:ABSORBS {T.gwCharges1666|i} HITS", "F:ATTACKERS SHOCKED" } },
            { SkillId.LzStormStride, new[] { "S:COOLDOWN {T.ssCd1666*cdm|s}S", "S:LASTS {T.ssDur1666*dm|s}S", "F:AFTER A DASH", "F:+{T.ssMove1666|p}% MOVE SPEED", "F:+{T.ssAtk1666|p}% BASIC SPEED" } },
            { SkillId.LzMirrorFlash, new[] { "S:COOLDOWN {T.mfCd1666*cdm|s}S", "S:DAZZLE {T.mfDazzle1666*dm|s}S", "F:DASH START FLASHES", "F:NEARBY DEMONS", "F:STOP ATTACKING" } },
            { SkillId.LzMarkOfJudgement, new[] { "S:COOLDOWN {T.mjCd1666*cdm|s}S", "S:MARK {T.mjLife1666*dm|s}S", "F:MARK NEAR CURSOR", "F:LIGHTNING CHARGES", "F:THEN A BOLT HITS", "F:ARCS TO {T.mjArcN1666|i} SHOCKED" } },
            { SkillId.LzConductivity, new[] { "S:COOLDOWN {T.cdCd1666*cdm|s}S", "S:CURSE {T.cdDur1666*dm|s}S", "F:CURSE AN AREA", "F:SHOCKED TAKE +{T.cdAmp1666|p}%", "F:SHOCK LASTS X{T.cdShockDur1666|x}", "F:SHOCK SPREADS" } },
            { SkillId.LzThunderDrum, new[] { "S:COOLDOWN {T.tdCd1666*cdm|s}S", "S:LASTS {T.tdLife1666*dm|s}S", "F:PLACE A DRUM", "F:LIGHTNING ON IT", "F:BOOMS A SHOCKWAVE", "F:BREAKS AFTER {T.tdBooms1666|i}" } },
            { SkillId.LzStaticField, new[] { "S:COOLDOWN {T.sfCd1666*cdm|s}S", "S:CURRENT HP -{T.sfNormal1666|p}%", "S:ELITES -{T.sfBig1666|p}%", "F:PULSE AROUND YOU", "F:NEVER BELOW {T.sfFloor1666|p}%", "F:NEVER KILLS", "F:SHOCKS" } },
            { SkillId.LzDecree, new[] { "S:COOLDOWN {T.dcCd1666*cdm|s}S", "S:BOLTS {T.dcBolts1666|i}", "F:COMBO: BASIC HIT,", "F:MOVE AND SPECIAL", "F:{T.dcNeed1666|i} COMBOS ARM IT", "F:BOLTS SPLIT TARGETS" } },
            { SkillId.LzHeavenArc, new[] { "S:COOLDOWN {T.haCd1666*cdm|s}S", "S:CHAINS {T.haChains1666|i}", "F:SHOCKED FOES DROP", "F:INFUSIONS", "F:COLLECT {T.haNeed1666|i} TO ARM", "F:A HUGE ARC CHAINS" } },
            { SkillId.LzTribunal, new[] { "S:COOLDOWN {T.trCd1666*cdm|s}S", "S:LASTS {T.trDur1666*dm|s}S", "F:SHOCKING CHARGES IT", "F:BOLTS HIT THE", "F:STRONGEST SHOCKED" } },
            { SkillId.LzJudgementWarp, new[] { "S:COOLDOWN {T.jwCd1666*cdm|s}S", "F:ELITE UNDER {T.jwCull1666|p}% HP", "F:TELEPORT INTO IT", "F:IT EXPLODES" } },
            // Gonggong
            { SkillId.GgRideTheFlood, new[] { "S:COOLDOWN {T.rdCd1682*cdm|s}S", "F:DASH: WAVE RIDE", "F:STEER WITH CURSOR", "F:IMMUNE TO CHILL,", "F:FREEZE, KNOCKBACK" } },
            { SkillId.GgRimeCoat, new[] { "S:COOLDOWN {T.rcCd1682*cdm|s}S", "S:WINDOW {T.rcDur1682*dm|s}S", "F:AFTER A DASH", "F:FIRST {T.rcN1682|i} MELEE FOES", "F:WHO HIT YOU FREEZE" } },
            { SkillId.GgFloodgate, new[] { "S:COOLDOWN {T.fgCd1682*cdm|s}S", "S:LASTS {T.fgDur1682*dm|s}S", "F:DASH: ICE WALL", "F:BLOCKS DEMONS", "F:AND BULLETS", "F:BREAKS IN {T.fgHits1682|i} HITS" } },
            { SkillId.GgFrozenRiver, new[] { "S:COOLDOWN {T.frCd1682*cdm|s}S", "S:LASTS {T.frDur1682*dm|s}S", "F:DASH ICES GROUND", "F:FOES SLIDE, -{T.frSlow1682|p}%", "F:YOU RUN +{T.frHero1682|p}%" } },
            { SkillId.GgDrowningMark, new[] { "S:COOLDOWN {T.dmCd1682*cdm|s}S", "S:MARK {T.dmLife1682*dm|s}S", "F:MARK NEAR CURSOR", "F:COLD HITS ON IT", "F:CHILL DEMONS NEAR" } },
            { SkillId.GgHypothermia, new[] { "S:COOLDOWN {T.hyCd1682*cdm|s}S", "S:CURSE {T.hyDur1682*dm|s}S", "F:CURSE AN AREA", "F:FEWER CHILLS", "F:TO FREEZE", "F:FROZEN +{T.hyFreeze1682|s}S LONGER" } },
            { SkillId.GgShatter, new[] { "S:COOLDOWN {T.shCd1682*cdm|s}S", "S:DAMAGE X{T.shDmg1682|x}", "F:FROZEN NEAR CURSOR", "F:EXPLODE", "F:CHILLS AROUND THEM" } },
            { SkillId.GgToppledPillar, new[] { "S:COOLDOWN {T.tpCd1682*cdm|s}S", "S:DAMAGE X{T.tpDmg1682|x}", "F:PILLAR BLOCKS", "F:FALLS AFTER {T.tpDelay1682|s}S", "F:CRUSHES ALONG AIM" } },
            { SkillId.GgFrostWall, new[] { "S:COOLDOWN {T.fwCd1682*cdm|s}S", "S:LASTS {T.fwDur1682*dm|s}S", "F:CURVED ICE WALL", "F:HOLDS DEMONS BACK", "F:HIT CRYSTALS BURST" } },
            { SkillId.GgRamOfBuzhou, new[] { "S:COOLDOWN {T.ramCd1682*cdm|s}S", "S:SHARDS {T.ramShards1682|i}", "F:DAMAGE TAKEN", "F:FILLS WRATH", "F:UNSTOPPABLE CHARGE", "F:THEN SHARDS RAIN" } },
            { SkillId.GgGreatFlood, new[] { "S:COOLDOWN {T.gfCd1682*cdm|s}S", "S:HIGH TIDE {T.gfHigh1682|s}S", "F:ONLY AT HIGH TIDE", "F:SCREEN-WIDE WAVE", "F:SWEEPS SOUTHEAST", "F:PUSHES AND CHILLS" } },
            { SkillId.GgEternalWinter, new[] { "S:COOLDOWN {T.ewCd1682*cdm|s}S", "S:LASTS {T.ewDur1682*dm|s}S", "F:{T.ewNeed1682|i} FROZEN KILLS ARM", "F:FROZEN DEATHS", "F:SHATTER AND CHAIN" } },
            { SkillId.GgPillarsOfAo, new[] { "S:COOLDOWN {T.aoCd1682*cdm|s}S", "S:LASTS {T.aoDur1682*dm|s}S", "F:ARMS BELOW {T.aoArm1682|p}% HP", "F:{G.GgAoPillars1825|i} ICE PILLARS", "F:KEEP DEMONS OUT", "F:AND FIRE ICICLES" } },
            // Zhong Kui
            { SkillId.NkGhostStep, new[] { "S:COOLDOWN {T.nkGhostCd1713*cdm|s}S", "F:UNTARGETABLE", "F:WHILE DASHING", "F:AND {T.nkGhostAfter1713|f}S AFTER", "F:PASS THROUGH FOES" } },
            { SkillId.NkBoneArmour, new[] { "S:COOLDOWN {T.nkArmCd1713*cdm|s}S", "S:LASTS {T.nkArmDur1713*dm|s}S", "F:AFTER A DASH", "F:BONE SHELL ABSORBS", "F:{T.nkArmFrac1713|p}% OF MAX LIFE" } },
            { SkillId.NkBoneWall, new[] { "S:COOLDOWN {T.nkWallCd1713*cdm|s}S", "S:LASTS {T.nkWallDur1713*dm|s}S", "F:DASH: BONE BARRIER", "F:BLOCKS DEMONS", "F:AND BULLETS", "F:BREAKS IN {T.nkWallHits1713|i} HITS" } },
            { SkillId.NkDecrepify, new[] { "S:COOLDOWN {T.nkDecrCd1713*cdm|s}S", "S:CURSE {T.nkDecrDur1713*dm|s}S", "F:CURSE AN AREA", "F:SLOWS {T.nkDecrSlow1713|p}%", "F:+{T.nkDecrVuln1713|p}% DMG TAKEN", "F:NO DEATH BLAST" } },
            { SkillId.NkSoulPlague, new[] { "S:COOLDOWN {T.nkPlagueCd1713*cdm|s}S", "S:LASTS {T.nkPlagueDur1713*dm|s}S", "F:MARK NEAR CURSOR", "F:WITH SOUL ROT", "F:SPREADS ON DEATH" } },
            { SkillId.NkSkeletalMage, new[] { "S:COOLDOWN {T.nkMageCd1713*cdm|s}S", "S:MAGES {T.nkMageCount1713@L|i}", "S:RESPAWN {T.nkMageResp1713|s}S", "F:BACK-LINE MAGES", "F:HEAVY SOUL BOLTS", "F:LIVE BY THEIR HP", "F:AT CAP: BOLTS BURST" } },
            { SkillId.NkClayGolem, new[] { "S:COOLDOWN {T.nkGolemCd1713*cdm|s}S", "S:RESPAWN {T.nkGolemResp1713|s}S", "F:RAISES A JIANGSHI", "F:LIVES BY ITS HP", "F:AT CAP: SLAM AT", "F:THE CURSOR" } },
            { SkillId.NkOssuaryNova, new[] { "S:COOLDOWN {T.nkNovaCd1713*cdm|s}S", "S:BONE HITS {T.nkNovaNeed1713|i}", "S:OR MARROW {G.NkNovaHarvest1825|i}", "F:EITHER ONE CHARGES", "F:BONE SPEAR RING", "F:BURSTS FROM YOU" } },
            { SkillId.NkArmyOfTheDead, new[] { "S:COOLDOWN {T.nkArmyCd1713*cdm|s}S", "S:LASTS {T.nkArmyDur1713*dm|s}S", "F:ARMED: {T.nkArmyNeed1713|i} MINIONS", "F:GHOST WAVE CHARGES", "F:YOUR ARMY FRENZIES" } },
            { SkillId.NkBoneColossus, new[] { "S:COOLDOWN {T.nkColCd1713*cdm|s}S", "F:ARMED: {T.nkColNeed1713|i}+ MINIONS", "F:THEY MERGE INTO", "F:A GIANT SKELETON", "F:UNTIL IT IS KILLED", "F:THEN SPLIT BACK" } },
            { SkillId.NkHellGate, new[] { "S:COOLDOWN {T.nkGateCd1713*cdm|s}S", "S:LASTS {T.nkGateDur1713*dm|s}S", "F:ARMED: {T.nkGateNeed1713|i} MINIONS", "F:GATE AT CURSOR", "F:SPAWNS GHOSTS" } },
            { SkillId.NkBanquet, new[] { "S:COOLDOWN {T.nkBanquetCd1713*cdm|s}S", "S:LASTS {T.nkBanquetDur1713*dm|s}S", "F:MINION KILLS ARM", "F:FRENZIED GHOSTS", "F:GROW PER KILL" } },
            { SkillId.NkGraveLegion, new[] { "S:COOLDOWN {T.nkLegionCd1772*cdm|s}S", "S:LASTS {T.nkLegionDur1772*dm|s}S", "F:ARMED AT CAP", "F:CAP X{G.NkLegionMul1825|i}", "F:ALL SKELETON TYPES", "F:RISE FULL STRENGTH", "F:EXTRAS CRUMBLE" } },
        };

        /// <summary>Support copy at level L (the level this card gives). The MAX line uses @MAX; the exclusion line is generated
        /// from SupportDef.excludes and the boss/elite line from executionerSkipBigs.</summary>
        public static readonly Dictionary<SupportId, string[]> Support = new Dictionary<SupportId, string[]>
        {
            { SupportId.MultipleProjectiles, new[] { "S:EXTRA SHOTS {T.supMultiExtra@L|i}", "?multi?D:-{1-T.multiProjDmgMul@L|p}% DAMAGE", "F:MORE SHOTS A CAST", "F:MAX LV{len(T.supMultiExtra)|i}: +{T.supMultiExtra@MAX|i}, -{1-T.multiProjDmgMul@MAX|p}%" } },
            { SupportId.Pierce, new[] { "S:PIERCES {T.supPierce@L|i}", "D:-{T.pierceFalloff@L|p}% DMG A PIERCE", "F:SHOTS PASS THROUGH", "F:MAX LV{len(T.supPierce)|i}: {T.supPierce@MAX|i}, -{T.pierceFalloff@MAX|p}%" } },
            { SupportId.Fork, new[] { "S:SPLITS IN {T.supForkChildren@L|i}", "S:SPLIT DMG {T.supForkDmg@L|p}%", "F:SPLITS ON 1ST HIT", "F:MAX LV{len(T.supForkChildren)|i}: {T.supForkChildren@MAX|i}, {T.supForkDmg@MAX|p}%" } },
            { SupportId.Bounce, new[] { "S:BOUNCES {T.supBounce@L|i}", "F:RETIRED", "F:BECOMES CHAIN" } },
            { SupportId.Homing, new[] { "S:TURN {T.supHomingTurn@L|s}", "S:SEEK RANGE {T.supHomingRangeU@L|s}", "F:SHOTS CURVE TO", "F:DEMONS", "F:MAX LV{len(T.supHomingTurn)|i}: {T.supHomingTurn@MAX|s}, {T.supHomingRangeU@MAX|s}" } },
            { SupportId.BiggerArea, new[] { "S:+{T.supAreaMul@L-1|p}% AREA", "F:AOE GROWS", "F:MAX LV{len(T.supAreaMul)|i}: +{T.supAreaMul@MAX-1|p}%" } },
            { SupportId.Echo, new[] {
                "?tgt!=S.CurseRune?S:REPEATS {T.supEchoCount@L|i}X", "?tgt!=S.CurseRune?S:REPEAT DMG {T.supEchoDmg@L|p}%",
                "?tgt==S.CurseRune?S:{max(1,T.curseEchoRunes738)|i} SMALLER RUNE", "?tgt==S.CurseRune?S:{T.curseEchoRadiusMul738|p}% SIZE", "?tgt==S.CurseRune?S:{T.supEchoDmg@L*T.curseEchoDmgMul738|p}% DMG",
                "D:+{T.echoCdMul1651@L-1|p}% COOLDOWN", "F:REPEATS THE CAST", "F:MAX LV{len(T.supEchoDmg)|i}: {T.supEchoDmg@MAX|p}% DMG", "F:AND +{T.echoCdMul1651@MAX-1|p}% COOLDOWN" } },
            { SupportId.MoreOrbitals, new[] { "S:EXTRA SKULLS {T.supOrbitals@L|i}", "F:MORE SKULLS ORBIT", "F:MAX LV{len(T.supOrbitals)|i}: +{T.supOrbitals@MAX|i}" } },
            { SupportId.Burn, new[] { "S:BURN X{T.supBurnMul@L|x} FOR {T.burnDuration+T.supBurnExtraSeconds@L|s}S", "?T.supBurnHitDmg@L<1?D:-{1-T.supBurnHitDmg@L|p}% HIT DMG", "F:HITS APPLY BURN", "F:MAX LV{len(T.supBurnMul)|i}: X{T.supBurnMul@MAX|x}, {T.burnDuration+T.supBurnExtraSeconds@MAX|s}S", "?T.supBurnHitDmg@MAX>=1?F:NO HIT PENALTY" } },
            { SupportId.Chill, new[] { "S:CHILL X{T.supChillTime@L|x}", "S:FREEZE IN {T.supChillFreezeHits@L|i}", "F:HITS CHILL", "F:CHILL SLOWS {1-G.EnemyChillMul1825|p}%", "F:MAX LV{len(T.supChillTime)|i}: X{T.supChillTime@MAX|x}", "F:AND FREEZE IN {T.supChillFreezeHits@MAX|i}" } },
            { SupportId.Shock, new[] { "S:+{T.supShockAmp@L|p}% DMG TAKEN", "F:HITS SHOCK", "F:FOR {G.StShockS1825|s}S", "F:MAX LV{len(T.supShockAmp)|i}: +{T.supShockAmp@MAX|p}%" } },
            { SupportId.Bleed, new[] { "S:BLEED X{T.supBleedMul@L|x}", "?T.supBleedHitDmg@L<1?D:-{1-T.supBleedHitDmg@L|p}% HIT DMG", "F:HITS APPLY BLEED", "F:MAX LV{len(T.supBleedMul)|i}: X{T.supBleedMul@MAX|x}", "?T.supBleedHitDmg@MAX>=1?F:NO HIT PENALTY" } },
            { SupportId.AddedDamage, new[] { "S:+{T.supAddedDmg@L-1|p}% DAMAGE", "F:MORE HIT DAMAGE", "F:MAX LV{len(T.supAddedDmg)|i}: +{T.supAddedDmg@MAX-1|p}%" } },
            { SupportId.FasterCasting, new[] { "S:-{1-T.supFasterCd@L|p}% COOLDOWN", "F:MAX LV{len(T.supFasterCd)|i}: -{1-T.supFasterCd@MAX|p}%" } },
            { SupportId.Executioner, new[] { "S:+{T.supExecDmg@L-1|p}% DMG", "S:BELOW {T.supExecThreshold@L|p}% HP", "?T.executionerSkipBigs?F:NOT BOSSES/ELITES", "F:MAX LV{len(T.supExecDmg)|i}: +{T.supExecDmg@MAX-1|p}%", "F:BELOW {T.supExecThreshold@MAX|p}% HP" } },
            { SupportId.LifeLeech, new[] { "S:HEAL {T.supLeechFrac@L|p}% OF HIT", "S:MAX {T.supLeechCapPerSec@L|s} HP/S", "F:PHYSICAL HITS HEAL", "F:NO BURN/BLEED LEECH", "F:MAX LV{len(T.supLeechFrac)|i}: {T.supLeechFrac@MAX|p}%, {T.supLeechCapPerSec@MAX|s}/S" } },
            { SupportId.Knockback, new[] { "S:PUSH {T.supKnockback@L|s}", "F:RETIRED", "F:OWNED IS DROPPED" } },
            { SupportId.SoulHarvest, new[] { "S:+{T.supHarvestXp@L|i} XP A KILL", "S:BONE CHANCE {T.supHarvestBoneChance@L|p}%", "F:KILLS DROP MORE", "F:MAX LV{len(T.supHarvestXp)|i}: +{T.supHarvestXp@MAX|i}, {T.supHarvestBoneChance@MAX|p}%" } },
            { SupportId.ExplodeOnKill, new[] { "S:BLAST {T.supExplodeDmg@L|p}% HP +{T.explodeFlatDmg|i}", "F:KILLS EXPLODE", "F:HIT DEMONS NEAR", "F:MAX {T.explodeMaxDmg|i} DMG A BLAST", "F:MAX LV{len(T.supExplodeDmg)|i}: {T.supExplodeDmg@MAX|p}%" } },
            { SupportId.Chain, new[] { "S:EXTRA JUMPS {T.supChainExtra1651@L|i}", "D:-{1-T.chainDmgMul1651@L|p}% DAMAGE", "F:MORE CHAIN JUMPS", "F:MAX LV{len(T.supChainExtra1651)|i}: +{T.supChainExtra1651@MAX|i}, -{1-T.chainDmgMul1651@MAX|p}%" } },
            { SupportId.Duration, new[] { "S:+{T.supDurMul1651@L-1|p}% DURATION", "F:LASTS LONGER", "F:MAX LV{len(T.supDurMul1651)|i}: +{T.supDurMul1651@MAX-1|p}%" } },
        };

        public static readonly Dictionary<StatId, string[]> Stat = new Dictionary<StatId, string[]>
        {
            { StatId.MaxHP, new[] { "S:+{T.statHpFrac|p}% MAX HP" } },
            { StatId.Speed, new[] { "S:+{T.statSpeed|p}% MOVE SPEED" } },
            { StatId.Pickup, new[] { "S:+{G.StatPickupAdd1825|p}% PICKUP RANGE" } },
            { StatId.Armor, new[] { "S:+{T.statArmor|s} ARMOR" } },
            { StatId.Crit, new[] { "S:+{T.statCrit|p}% CRIT CHANCE" } },
            { StatId.Slot, new[] { "S:NEW SKILL SLOT", "F:UNLOCKS NOW", "F:THEN PICK A SKILL" } },
        };
    }
}
