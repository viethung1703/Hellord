using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    // t1831 frame D (Bronze Leiwen, VH pick): card copy for the headline + stat table layout. Data only; the layout lives in
    // HellordCardsD1831.cs. No level info, no LINKS TO. Every number is a {token} read live (HellordCardEval1825).
    //
    // Line grammar:  [?cond?]K:text
    //   H  headline (white, wrapped to the card, statuses in their colour, numbers gold)
    //   R  table row  "LABEL ~ VALUE"  (label grey, left at the 16 px inset; value white/gold, right at the 16 px inset)
    //   F  detail line (silver, centred)        X  red detail line (cost / limit)
    //   (t1831 task 9: every card is written out here in full; the old A:n borrowing of Option A lines is gone, the
    //    A:n grammar still parses but no card uses it. Copy source: tmp/cardsD_work/copyD.py -> gen.py.)
    //   Context vars: CardEval1825 (L, A, cd, cdm, dm, dmg = live hit damage at the shown level, kit = live kit hit (ZkHit1651 at the shown level), tgt, multi, summon).
    // =====================================================================================================================
    public static class CardCopyD1831
    {
        public static readonly Dictionary<SkillId, string[]> Skill = new Dictionary<SkillId, string[]>
        {
            { SkillId.Fireball, new[] { "H:A FIREBALL THAT BURSTS AND BURNS.", "R:DAMAGE ~ {dmg|i}", "R:COOLDOWN ~ {cd|s}S", "R:FIREBALLS ~ {T.fbProjectiles@L|i}" } },
            { SkillId.FrostCleave, new[] { "H:A WIDE ICY SWING THAT CHILLS DEMONS.", "R:DAMAGE ~ {dmg|i}", "R:COOLDOWN ~ {cd|s}S", "F:{G.StFreezeHits1825|i} CHILLS FREEZE.", "F:DEALS {T.frostChilledBonus-1|p}% MORE TO CHILLED DEMONS.", "?T.frostEraseBullets49?F:ERASES BULLETS." } },
            { SkillId.StormRing, new[] { "H:A RING AROUND YOU THAT SHOCKS DEMONS.", "R:DAMAGE ~ {dmg|i}", "R:COOLDOWN ~ {cd|s}S", "F:SHOCKED DEMONS TAKE {T.shockBaseAmp|p}% MORE DAMAGE." } },
            { SkillId.BoneOrbit, new[] { "H:SKULLS CIRCLE YOU, DEMONS BLEED.", "R:DAMAGE ~ {dmg|i}", "R:SKULLS ~ {T.orbitCount@L|i}" } },
            { SkillId.CurseRune, new[] { "H:A RUNE CURSES DEMONS AT YOUR AIM.", "R:DAMAGE ~ {dmg|i}", "R:COOLDOWN ~ {cd|s}S", "R:RUNES ~ {T.curseRunes@L|i}", "F:CURSED DEMONS EXPLODE ON DEATH." } },
            { SkillId.ChainLightning, new[] { "H:A BOLT JUMPS FROM DEMON TO DEMON.", "R:DAMAGE ~ {dmg|i}", "R:COOLDOWN ~ {cd|s}S", "R:JUMPS ~ {T.chainJumps@L|i}", "F:HITS CAUSE SHOCK." } },
            { SkillId.HellfirePillar, new[] { "H:FIRE ERUPTS UNDER PACKS AND BURNS.", "R:DAMAGE ~ {dmg*T.pillarDmgMul|i}", "R:COOLDOWN ~ {cd|s}S", "R:PILLARS ~ {T.pillarCount@L|i}" } },
            { SkillId.BloodWhip, new[] { "H:A LONG LASH THAT HEALS AS IT HITS.", "R:DAMAGE ~ {dmg|i}", "R:COOLDOWN ~ {cd|s}S", "R:LASHES ~ {T.whipLashes@L|i}", "R:HEAL PER HIT ~ {T.whipHealPerHit@L|f}", "F:HITS CAUSE BLEED.", "X:UP TO {T.whipHealCapPerCast|s} LIFE PER CAST." } },
            { SkillId.SoulSpears, new[] { "H:SPEARS PIERCE A LINE OF DEMONS.", "R:DAMAGE ~ {dmg|i}", "R:COOLDOWN ~ {cd|s}S", "R:SPEARS ~ {T.spearCount@L|i}", "R:PIERCES ~ {T.spearPierce@L|i}" } },
            { SkillId.GravePool, new[] { "H:YOU LEAVE POOLS THAT SLOW AND HURT.", "R:DAMAGE ~ {dmg|i} PER TICK", "R:POOLS LAST ~ {T.gravePoolSeconds@L*dm|s}S", "F:DEMONS IN A POOL ARE {T.graveSlow@L|p}% SLOWER." } },
            { SkillId.SkeletonArchers, new[] { "?T.skelArcCount561@L>1?H:{T.skelArcCount561@L|i} ARCHERS SHOOT BONE ARROWS.", "?T.skelArcCount561@L<=1?H:AN ARCHER SHOOTS BONE ARROWS.", "R:DAMAGE ~ {T.skelArcDmg561@L|i}", "R:RISE AGAIN ~ {T.skelArcResp561@L|s}S", "F:CLICK: COMMAND THEM" } },
            { SkillId.SkeletonWarriors, new[] { "?T.skelWarCount561@L>1?H:{T.skelWarCount561@L|i} SKELETONS FIGHT BESIDE YOU.", "?T.skelWarCount561@L<=1?H:A SKELETON FIGHTS BESIDE YOU.", "R:DAMAGE ~ {T.skelWarDmg561@L|i}", "R:LIFE ~ {T.skelWarHp561|i}", "R:RISE AGAIN ~ {T.skelWarResp561@L|s}S", "F:THEY BLOCK BULLETS.", "F:CLICK: COMMAND THEM" } },
            { SkillId.Frostbolt, new[] { "H:SLOW FROST ORBS PIERCE AND CHILL.", "R:DAMAGE ~ {dmg|i}", "R:COOLDOWN ~ {cd*T.frostbCdMul@L|s}S", "R:ORBS ~ {T.frostbOrbs@L|i}", "F:ORBS BURST WHEN THEY HIT A WALL." } },
            { SkillId.IceNova, new[] { "H:FROST PULSES OUT AND CHILLS DEMONS.", "R:DAMAGE ~ {dmg|i}", "R:COOLDOWN ~ {cd*T.novaCdMul@L|s}S", "R:PULSES ~ {T.novaPulses@L|i}", "F:IT PUSHES CLOSE DEMONS AWAY." } },
            { SkillId.FrostBomb, new[] { "H:AN ICE BOMB CHILLS, THEN BURSTS.", "R:DAMAGE ~ {dmg*T.bombBlastMul|i}", "R:COOLDOWN ~ {cd*T.bombCdMul@L|s}S", "R:BOMBS ~ {T.bombCount@L|i}", "R:CHILLS FOR ~ {T.bombSeconds@L*dm|s}S", "F:THE BURST DEALS HEAVY COLD DAMAGE." } },
            { SkillId.Hurricane, new[] { "H:A STORM AROUND YOU CHILLS DEMONS.", "R:DAMAGE ~ {dmg|i} PER HIT", "R:LASTS ~ {T.hurrSeconds@L*dm|s}S", "R:COOLDOWN ~ {cd*T.hurrCdMul@L|s}S", "F:CHILLED DEMONS ARE {1-G.EnemyChillMul1825|p}% SLOWER." } },
            { SkillId.MagmaThrow, new[] { "H:MOLTEN GLOBS BURST INTO SHARDS.", "R:DAMAGE ~ {dmg|i}", "R:COOLDOWN ~ {cd|s}S", "R:SHARDS ~ {T.msStrands1481|i}", "F:SHARDS BURN DEMONS AND THE GROUND.", "F:HOLD: KEEP FIRING" } },
            { SkillId.RollingMagmaRock, new[] { "H:A ROLLING ROCK THAT BURNS DEMONS.", "R:DAMAGE ~ {dmg|i}", "R:BURN ~ {dmg*T.burnDpsFromHit|i}/S FOR {T.burnDuration|s}S", "R:COOLDOWN ~ {cd|s}S", "F:LEAVES FIRE BEHIND.", "F:BREAKS ON WALLS." } },
            { SkillId.FirePillar, new[] { "H:FIRE ERUPTS WHERE YOU AIM AND BURNS.", "R:DAMAGE ~ {dmg*T.fpDmgMul1481|i}", "R:COOLDOWN ~ {cd|s}S", "F:HOLD: KEEP FIRING" } },
            { SkillId.FireWall, new[] { "H:A FIRE WALL BURNS ALL WHO CROSS.", "R:DAMAGE ~ {dmg/lvm*(1+T.fireWallDmgPerLvl737*(L-1))*T.fireWallTickDmgMul948|i} PER TICK", "R:COOLDOWN ~ {cd|s}S", "R:LASTS ~ {(T.fireWallSec737+T.fireWallSecPerLvl737*(L-1))*dm|s}S", "F:HOLD: KEEP PLACING" } },
            { SkillId.Hydra, new[] { "H:A HYDRA SPITS FIRE BOLTS AT DEMONS.", "R:DAMAGE ~ {dmg*T.hyBoltDmg1625|i}", "R:COOLDOWN ~ {cd|s}S", "R:LASTS ~ {T.hyLife1625*dm|s}S", "F:{G.ZhHydraHeads1825|i} HEADS PICK THEIR OWN TARGETS.", "F:BOLTS CAUSE BURN.", "X:UP TO {T.hyMax1625|i} HYDRAS AT ONCE." } },
            { SkillId.ZhurongKit, new[] { "H:ZHURONG'S KIT DAMAGE SOURCE." } },
            { SkillId.SparkSpray, new[] { "H:SPARKS SKITTER TO YOUR AIM AND SHOCK.", "R:DAMAGE ~ {dmg*T.sparkDmgMul1567|i}", "R:COOLDOWN ~ {cd|s}S", "R:SPARKS ~ {T.sparkCount1567@L|i}", "F:EACH SPARK HITS ONE DEMON." } },
            { SkillId.BallLightning, new[] { "H:A SLOW ORB ZAPS DEMONS IT PASSES.", "R:DAMAGE ~ {dmg*T.orbZapMul1567|i}", "R:COOLDOWN ~ {cd|s}S", "R:DEMONS PER ZAP ~ {T.orbZapTargets1567@L|i}", "F:ZAPS CAUSE SHOCK.", "X:UP TO {T.orbCap1567|i} ORBS AT ONCE." } },
            { SkillId.ThunderJavelin, new[] { "?T.javChain1567?H:A FAST BOLT THAT CHAINS ON A HIT.", "?!T.javChain1567?H:A FAST BOLT THAT SPLITS ON A HIT.", "R:DAMAGE ~ {dmg|i}", "R:COOLDOWN ~ {cd|s}S", "?T.javChain1567?R:JUMPS ~ {T.javChainJumps1567@L|i}", "F:HITS CAUSE SHOCK." } },
            { SkillId.HeadsmansChop, new[] { "H:A WIDE AXE ARC THAT KNOCKS DEMONS BACK.", "R:DAMAGE ~ {dmg|i}", "R:COOLDOWN ~ {cd|s}S", "F:HITS CAUSE BLEED.", "F:BLEEDING DEMONS BURST IN BLOOD.", "X:UP TO {T.chopWaveMax1567|i} PER CHOP." } },
            { SkillId.EarthSplittingSlam, new[] { "H:THE AXE CRACKS THE GROUND AHEAD.", "R:DAMAGE ~ {dmg|i}", "R:COOLDOWN ~ {cd|s}S", "F:THE CRACKS SLOW DEMONS, THEN ERUPT AFTER {T.eruptDelayS1567|s}S." } },
            { SkillId.TwinAxeThrow, new[] { "H:A SPINNING AXE THAT FLIES BACK TO YOU.", "R:DAMAGE ~ {dmg|i}", "R:COOLDOWN ~ {cd|s}S", "F:IT PIERCES AND HITS AGAIN COMING BACK.", "F:HITS CAUSE BLEED.", "X:ONE AXE AT A TIME." } },
            { SkillId.PhantomAxes, new[] { "H:GHOST WARRIORS CHOP WHERE YOU AIM.", "R:DAMAGE ~ {dmg*T.phDmgMul1567|i}", "R:COOLDOWN ~ {cd|s}S", "R:GHOSTS ~ {T.phCount1567@L|i}", "F:EACH GHOST CHOPS ONCE.", "F:HITS CAUSE BLEED." } },
            { SkillId.ThunderDrumStrikes, new[] { "H:LIGHTNING STRIKES NEAR YOUR AIM.", "R:DAMAGE ~ {dmg*T.dsStrikeDmg1666|i}", "R:COOLDOWN ~ {cd|s}S", "F:{G.LzClapBeat1825|i} QUICK CASTS END IN A THUNDERCLAP.", "F:IT DEALS {T.dsClapShocked1666-1|p}% MORE TO SHOCKED DEMONS.", "F:HITS CAUSE SHOCK." } },
            { SkillId.ThunderLash, new[] { "H:A WIDE ARC OF LIGHTNING IN FRONT.", "R:DAMAGE ~ {dmg*T.tlDmg1666|i}", "R:COOLDOWN ~ {cd|s}S", "F:SHOCKED DEMONS ARE KNOCKED BACK.", "F:HITS CAUSE SHOCK." } },
            { SkillId.LiveWire, new[] { "H:HOLD TO LOCK A BEAM ONTO A DEMON.", "R:DAMAGE ~ {dmg*T.lwDmg1666|i} PER TICK", "R:COOLDOWN ~ {cd|s}S", "R:HOLD UP TO ~ {T.lwMaxHold1666|s}S", "F:AFTER {T.lwOverS1666|s}S ON ONE DEMON IT HITS HARDER AND SHOCKS.", "X:YOU MOVE {1-T.lwMoveMul1666|p}% SLOWER WHILE HOLDING." } },
            { SkillId.ThunderHound, new[] { "H:A THUNDER HOUND BITES DEMONS.", "R:DAMAGE ~ {dmg*T.hdDmg1666|i}", "R:COOLDOWN ~ {cd|s}S", "R:LASTS ~ {T.hdLife1666*dm|s}S", "F:BITES CAUSE SHOCK.", "X:UP TO {T.hdMax1666|i} HOUNDS AT ONCE." } },
            { SkillId.TidalSurge, new[] { "H:A WAVE SWEEPS DEMONS AND CHILLS.", "R:DAMAGE ~ {dmg*T.tsCrash1682|i}", "R:COOLDOWN ~ {cd|s}S" } },
            { SkillId.Whirlpool, new[] { "H:A VORTEX DRAGS DEMONS IN, CHILLS.", "R:DAMAGE ~ {dmg*T.wpDmg1682|i}", "R:COOLDOWN ~ {cd|s}S", "X:UP TO {T.wpMax1682|i} VORTEXES AT ONCE." } },
            { SkillId.BreathOfTheDeep, new[] { "H:HOLD TO BREATHE A CONE OF FROST.", "R:DAMAGE ~ {dmg*T.brDmg1682|i} PER TICK", "R:COOLDOWN ~ {cd|s}S", "R:HOLD UP TO ~ {T.brMax1682|s}S", "F:DEMONS IN THE CONE FOR {T.brFreezeAt1682|s}S FREEZE.", "X:YOU MOVE {1-T.brMove1682|p}% SLOWER WHILE HOLDING." } },
            { SkillId.BuzhouRift, new[] { "H:AN ICE FISSURE ENDS IN A BIG SPIKE.", "R:DAMAGE ~ {dmg*T.rfBurst1682|i}", "R:COOLDOWN ~ {cd|s}S", "F:THE SPIKE DEALS {T.rfFrozen1682-1|p}% MORE TO FROZEN DEMONS." } },
            { SkillId.EyeOfTheBlackWaters, new[] { "H:A SLOW ORB SPRAYS ICE SHARDS.", "R:DAMAGE ~ {dmg*T.eyeDmg1682|i}", "R:COOLDOWN ~ {cd|s}S", "X:UP TO {T.eyeMax1682|i} ORBS AT ONCE." } },
            { SkillId.Teeth, new[] { "H:A SHORT FAN OF BONE CHIPS.", "R:DAMAGE ~ {dmg*T.nkTeethDmg1713|i}", "R:COOLDOWN ~ {cd|s}S" } },
            { SkillId.ZkEmberEffigy, new[] { "H:YOUR DASH LEAVES A DECOY FOR DEMONS.", "R:COOLDOWN ~ {T.effCd1618*cdm|s}S", "R:DECOY LASTS ~ {T.effLife1618*dm|s}S" } },
            { SkillId.ZkEmberAnchor, new[] { "H:DASH AGAIN TO SNAP BACK TO YOUR EMBER.", "R:COOLDOWN ~ {T.ancCd1618*cdm|s}S", "R:RETURN WITHIN ~ {T.ancWindow1618*dm|s}S" } },
            { SkillId.ZkBackdraft, new[] { "H:YOUR DASH PULLS DEMONS IN BEHIND.", "R:COOLDOWN ~ {T.bdCd1618*cdm|s}S", "F:DEMONS ARE PULLED {T.bdPull1618|p}% OF THE WAY TO YOUR DASH START." } },
            { SkillId.ZkPyreMark, new[] { "H:MARK A DEMON, BURN IT FOR A FIRE BUFF.", "R:COOLDOWN ~ {T.pmCd1618*cdm|s}S", "R:MARK LASTS ~ {T.pmLife1618*dm|s}S", "F:BURN IT FOR {T.pmThresh1618|p}% OF ITS LIFE TO GAIN {T.pmBuffMul1618-1|p}% MORE FIRE DAMAGE FOR {T.pmBuffS1618|s}S." } },
            { SkillId.ZkFlammability, new[] { "H:CURSE AN AREA SO BURNS HIT HARDER.", "R:COOLDOWN ~ {T.flCd1618*cdm|s}S", "R:CURSE LASTS ~ {T.flDur1618*dm|s}S", "F:CURSED DEMONS CATCH BURN {T.flBurnK1618-1|p}% FASTER AND BURNS DEAL {T.flBurnDmg1618-1|p}% MORE." } },
            { SkillId.ZkWarCry, new[] { "H:ROAR TO POWER UP YOUR BASIC ATTACKS.", "R:DAMAGE ~ {kit*T.wcBoomDmg1625|i}", "R:COOLDOWN ~ {T.wcCd1625*cdm|s}S", "R:POWERED BASICS ~ {T.wcCharges1625|i}", "F:POWERED BASICS DEAL {T.wcMul1625-1|p}% MORE DAMAGE.", "F:ROARED DEMONS EXPLODE IF THEY DIE WITHIN {T.wcMarkS1625|s}S." } },
            { SkillId.ZkSmolderRing, new[] { "H:A BURNING RING THAT SLOWS DEMONS.", "R:DAMAGE ~ {kit*T.srDmg1618|i} PER TICK", "R:COOLDOWN ~ {T.srCd1618*cdm|s}S", "R:LASTS ~ {T.srLife1618*dm|s}S", "F:DEMONS IN IT ARE {T.srSlow1618|p}% SLOWER AND TAKE FIRE DAMAGE." } },
            { SkillId.ZkHearthBlessing, new[] { "H:HEAL OVER TIME AND SHAKE OFF SLOWS.", "R:COOLDOWN ~ {T.hbCd1618*cdm|s}S", "F:HEALS {T.hbHeal1618|p}% OF YOUR MAX LIFE OVER {T.hbDur1618*dm|s}S.", "F:CLEARS SLOWS AND CHILLS, AND BLOCKS NEW ONES." } },
            { SkillId.ZkSunsWrath, new[] { "H:BURN DAMAGE CALLS DOWN A FALLING SUN.", "R:DAMAGE ~ {kit*T.swDmg1618|i}", "R:COOLDOWN ~ {T.swCd1618*cdm|s}S", "F:READY AFTER {T.swMeter1618|i} BURN DAMAGE.", "F:IT LEAVES BURNING GROUND FOR {T.swAreaS1618*dm|s}S." } },
            { SkillId.ZkRebirthFlame, new[] { "H:CHEAT DEATH ONCE IN A FIRESTORM.", "R:DAMAGE ~ {kit*T.rbDmg1618|i}", "R:COOLDOWN ~ {T.rbCd1618*cdm|s}S", "F:YOU REVIVE WITH {T.rbHp1618|p}% LIFE AND CANNOT BE HIT FOR {T.rbInv1618|s}S." } },
            { SkillId.ZkMoltenHeart, new[] { "H:A MOLTEN HEART POWERS YOUR BASICS.", "R:COOLDOWN ~ {T.mhCd1618*cdm|s}S", "R:LASTS ~ {T.mhDur1618*dm|s}S", "F:READY WHEN {T.mhNeed1618|i} DEMONS ARE BURNING.", "F:YOUR BASICS CAST TWICE AND YOU CANNOT BE SLOWED." } },
            { SkillId.ZkVermilionBird, new[] { "H:NEAR MISSES CALL A SWEEPING FIREBIRD.", "R:DAMAGE ~ {kit*T.vbDmg1618|i}", "R:COOLDOWN ~ {T.vbCd1618*cdm|s}S", "F:READY AFTER {T.vbNeed1618|i} NEAR-MISS DASHES.", "F:IT LEAVES A BURNING STRIP FOR {T.vbStripS1618*dm|s}S." } },
            { SkillId.LzThunderStep, new[] { "H:YOUR DASH BECOMES A TELEPORT.", "R:COOLDOWN ~ {T.tsCd1666*cdm|s}S", "R:DISTANCE ~ +{T.tsDistMul1666-1|p}%", "F:AIM NEAR A BALL LIGHTNING ORB TO JUMP INTO IT." } },
            { SkillId.LzGroundingWard, new[] { "H:AFTER A DASH, A WARD ABSORBS HITS.", "R:COOLDOWN ~ {T.gwCd1666*cdm|s}S", "R:WARD LASTS ~ {T.gwDur1666*dm|s}S", "F:IT ABSORBS {T.gwCharges1666|i} HITS AND SHOCKS THE ATTACKERS." } },
            { SkillId.LzStormStride, new[] { "H:DASH TO MOVE AND ATTACK FASTER.", "R:COOLDOWN ~ {T.ssCd1666*cdm|s}S", "R:LASTS ~ {T.ssDur1666*dm|s}S", "F:YOU MOVE {T.ssMove1666|p}% FASTER AND YOUR BASICS FIRE {T.ssAtk1666|p}% FASTER." } },
            { SkillId.LzMirrorFlash, new[] { "H:YOUR DASH DAZZLES NEARBY DEMONS.", "R:COOLDOWN ~ {T.mfCd1666*cdm|s}S", "R:DAZZLE LASTS ~ {T.mfDazzle1666*dm|s}S", "F:DAZZLED DEMONS STOP ATTACKING AND CHASING." } },
            { SkillId.LzMarkOfJudgement, new[] { "H:MARK A DEMON FOR A JUDGEMENT BOLT.", "R:DAMAGE ~ {kit*T.mjDmg1666|i}", "R:COOLDOWN ~ {T.mjCd1666*cdm|s}S", "R:MARK LASTS ~ {T.mjLife1666*dm|s}S", "F:LIGHTNING DAMAGE ON THE MARK CHARGES THE BOLT.", "F:THE BOLT ARCS TO {T.mjArcN1666|i} SHOCKED DEMONS." } },
            { SkillId.LzConductivity, new[] { "H:CURSE AN AREA TO STRENGTHEN SHOCK.", "R:COOLDOWN ~ {T.cdCd1666*cdm|s}S", "R:CURSE LASTS ~ {T.cdDur1666*dm|s}S", "F:SHOCKED DEMONS TAKE {T.cdAmp1666|p}% MORE DAMAGE.", "F:SHOCK LASTS {T.cdShockDur1666-1|p}% LONGER AND SPREADS." } },
            { SkillId.LzThunderDrum, new[] { "H:PLACE A DRUM THAT BOOMS WHEN STRUCK.", "R:DAMAGE ~ {kit*T.tdDmg1666|i}", "R:COOLDOWN ~ {T.tdCd1666*cdm|s}S", "R:LASTS ~ {T.tdLife1666*dm|s}S", "X:IT BREAKS AFTER {T.tdBooms1666|i} BOOMS." } },
            { SkillId.LzStaticField, new[] { "H:A PULSE TAKES A SHARE OF LIFE.", "R:DAMAGE ~ {T.sfNormal1666|p}% OF LIFE", "R:COOLDOWN ~ {T.sfCd1666*cdm|s}S", "R:ELITES ~ -{T.sfBig1666|p}%", "F:HITS CAUSE SHOCK.", "X:NEVER BELOW {T.sfFloor1666|p}% OF MAX LIFE." } },
            { SkillId.LzDecree, new[] { "H:SKILL COMBOS CALL DOWN A BOLT STORM.", "R:DAMAGE ~ {kit*T.dcDmg1666|i}", "R:COOLDOWN ~ {T.dcCd1666*cdm|s}S", "R:BOLTS ~ {T.dcBolts1666|i}", "F:READY AFTER {T.dcNeed1666|i} SKILL COMBOS.", "F:EACH BOLT STRIKES A DIFFERENT DEMON." } },
            { SkillId.LzHeavenArc, new[] { "H:COLLECT INFUSIONS FOR A HUGE ARC.", "R:DAMAGE ~ {kit*T.haDmg1666|i}", "R:COOLDOWN ~ {T.haCd1666*cdm|s}S", "R:JUMPS ~ {T.haChains1666|i}", "F:READY AFTER {T.haNeed1666|i} INFUSIONS FROM SHOCKED DEMONS." } },
            { SkillId.LzTribunal, new[] { "H:SHOCKING DEMONS CALLS DOWN BOLTS.", "R:DAMAGE ~ {kit*T.trDmg1666|i}", "R:COOLDOWN ~ {T.trCd1666*cdm|s}S", "R:LASTS ~ {T.trDur1666*dm|s}S" } },
            { SkillId.LzJudgementWarp, new[] { "H:A JUDGEMENT WARP FINISHES ELITES.", "R:DAMAGE ~ {kit*T.jwDmg1666|i}", "R:COOLDOWN ~ {T.jwCd1666*cdm|s}S", "F:ONCE AN ELITE IS BELOW {T.jwCull1666|p}% LIFE, WARP INTO IT AND IT EXPLODES." } },
            { SkillId.GgRideTheFlood, new[] { "H:YOUR DASH BECOMES A WAVE YOU STEER.", "R:COOLDOWN ~ {T.rdCd1682*cdm|s}S", "F:WHILE RIDING YOU IGNORE CHILL, FREEZE AND KNOCKBACK." } },
            { SkillId.GgRimeCoat, new[] { "H:AFTER A DASH, MELEE ATTACKERS FREEZE.", "R:COOLDOWN ~ {T.rcCd1682*cdm|s}S", "R:LASTS ~ {T.rcDur1682*dm|s}S", "X:ONLY THE FIRST {T.rcN1682|i} ATTACKERS FREEZE." } },
            { SkillId.GgFloodgate, new[] { "H:YOUR DASH LEAVES AN ICE WALL BEHIND.", "R:COOLDOWN ~ {T.fgCd1682*cdm|s}S", "R:LASTS ~ {T.fgDur1682*dm|s}S", "F:IT BLOCKS DEMONS AND BULLETS.", "X:IT BREAKS AFTER {T.fgHits1682|i} HITS." } },
            { SkillId.GgFrozenRiver, new[] { "H:YOUR DASH FREEZES THE GROUND.", "R:COOLDOWN ~ {T.frCd1682*cdm|s}S", "R:LASTS ~ {T.frDur1682*dm|s}S", "F:DEMONS ON IT SLIDE AND ARE {T.frSlow1682|p}% SLOWER.", "F:YOU MOVE {T.frHero1682|p}% FASTER ON IT." } },
            { SkillId.GgDrowningMark, new[] { "H:COLD HITS ON A MARK CHILL ITS PACK.", "R:COOLDOWN ~ {T.dmCd1682*cdm|s}S", "R:MARK LASTS ~ {T.dmLife1682*dm|s}S" } },
            { SkillId.GgHypothermia, new[] { "H:CURSED DEMONS FREEZE FASTER.", "R:COOLDOWN ~ {T.hyCd1682*cdm|s}S", "R:CURSE LASTS ~ {T.hyDur1682*dm|s}S", "F:THEY STAY FROZEN {T.hyFreeze1682|s}S LONGER." } },
            { SkillId.GgShatter, new[] { "H:FROZEN DEMONS NEAR YOUR AIM EXPLODE.", "R:DAMAGE ~ {kit*T.shDmg1682|i}", "R:COOLDOWN ~ {T.shCd1682*cdm|s}S", "F:EACH BLAST CHILLS DEMONS AROUND IT." } },
            { SkillId.GgToppledPillar, new[] { "H:A PILLAR FALLS AND CRUSHES A LINE.", "R:DAMAGE ~ {kit*T.tpDmg1682|i}", "R:COOLDOWN ~ {T.tpCd1682*cdm|s}S", "F:THE PILLAR BLOCKS DEMONS, THEN FALLS ALONG YOUR AIM AFTER {T.tpDelay1682|s}S." } },
            { SkillId.GgFrostWall, new[] { "H:A CURVED ICE WALL HOLDS DEMONS BACK.", "R:DAMAGE ~ {kit*T.fwDmg1682|i}", "R:COOLDOWN ~ {T.fwCd1682*cdm|s}S", "R:LASTS ~ {T.fwDur1682*dm|s}S", "F:ITS CRYSTALS BURST WHEN THEY ARE HIT." } },
            { SkillId.GgRamOfBuzhou, new[] { "H:WRATH FUELS AN UNSTOPPABLE CHARGE.", "R:DAMAGE ~ {kit*T.ramDmg1682|i}", "R:COOLDOWN ~ {T.ramCd1682*cdm|s}S", "R:SHARDS ~ {T.ramShards1682|i}", "F:READY AFTER TAKING {T.ramFill1682|p}% OF YOUR LIFE IN DAMAGE.", "F:THEN ICE SHARDS RAIN DOWN." } },
            { SkillId.GgGreatFlood, new[] { "H:AT HIGH TIDE A WAVE SWEEPS THE SCREEN.", "R:DAMAGE ~ {kit*T.gfDmg1682|i}", "R:COOLDOWN ~ {T.gfCd1682*cdm|s}S", "R:HIGH TIDE ~ {T.gfHigh1682|s}S", "F:THE WAVE PUSHES AND CHILLS DEMONS." } },
            { SkillId.GgEternalWinter, new[] { "H:WINTER SHATTERS FROZEN DEMONS.", "R:DAMAGE ~ {kit*T.ewDmg1682|i}", "R:COOLDOWN ~ {T.ewCd1682*cdm|s}S", "R:LASTS ~ {T.ewDur1682*dm|s}S", "F:READY AFTER {T.ewNeed1682|i} FROZEN KILLS.", "F:FROZEN DEMONS THAT DIE SHATTER AND CHAIN." } },
            { SkillId.GgPillarsOfAo, new[] { "H:AT LOW LIFE, ICE PILLARS GUARD YOU.", "R:DAMAGE ~ {kit*T.aoDmg1682|i}", "R:COOLDOWN ~ {T.aoCd1682*cdm|s}S", "R:LASTS ~ {T.aoDur1682*dm|s}S", "F:READY BELOW {T.aoArm1682|p}% LIFE.", "F:{G.GgAoPillars1825|i} ICE PILLARS KEEP DEMONS OUT AND FIRE ICICLES." } },
            { SkillId.NkGhostStep, new[] { "H:YOU CANNOT BE HIT WHILE DASHING.", "R:COOLDOWN ~ {T.nkGhostCd1713*cdm|s}S", "F:THIS LASTS {T.nkGhostAfter1713|f}S AFTER THE DASH, AND YOU PASS THROUGH DEMONS." } },
            { SkillId.NkBoneArmour, new[] { "H:A BONE SHELL GUARDS YOU AFTER DASHES.", "R:COOLDOWN ~ {T.nkArmCd1713*cdm|s}S", "R:LASTS ~ {T.nkArmDur1713*dm|s}S", "F:IT ABSORBS DAMAGE UP TO {T.nkArmFrac1713|p}% OF YOUR MAX LIFE." } },
            { SkillId.NkBoneWall, new[] { "H:YOUR DASH LEAVES A BONE WALL BEHIND.", "R:COOLDOWN ~ {T.nkWallCd1713*cdm|s}S", "R:LASTS ~ {T.nkWallDur1713*dm|s}S", "F:IT BLOCKS DEMONS AND BULLETS.", "X:IT BREAKS AFTER {T.nkWallHits1713|i} HITS." } },
            { SkillId.NkDecrepify, new[] { "H:CURSE AN AREA TO SLOW AND WEAKEN.", "R:COOLDOWN ~ {T.nkDecrCd1713*cdm|s}S", "R:CURSE LASTS ~ {T.nkDecrDur1713*dm|s}S", "F:CURSED DEMONS ARE {T.nkDecrSlow1713|p}% SLOWER AND TAKE {T.nkDecrVuln1713|p}% MORE DAMAGE.", "F:THEY DO NOT EXPLODE ON DEATH." } },
            { SkillId.NkSoulPlague, new[] { "H:A SOUL ROT THAT SPREADS ON DEATH.", "R:DAMAGE ~ {kit*T.nkPlagueDps1713|i}/S", "R:COOLDOWN ~ {T.nkPlagueCd1713*cdm|s}S", "R:LASTS ~ {T.nkPlagueDur1713*dm|s}S" } },
            { SkillId.NkSkeletalMage, new[] { "H:RAISE MAGES THAT FIRE SOUL BOLTS.", "R:DAMAGE ~ {T.nkMageDmg1713@L|i}", "R:COOLDOWN ~ {T.nkMageCd1713*cdm|s}S", "R:MAGES ~ {T.nkMageCount1713@L|i}", "R:RISE AGAIN ~ {T.nkMageResp1713|s}S", "F:THEY STAY UNTIL KILLED.", "F:WHEN ALL ARE UP, PRESS AGAIN TO MAKE THEIR BOLTS BURST." } },
            { SkillId.NkClayGolem, new[] { "H:RAISE A JIANGSHI TO FIGHT FOR YOU.", "R:DAMAGE ~ {T.nkGolemDmg1713*(1+T.nkGolemDmgPerLv1713*(L-1))|i}", "R:SLAM ~ {kit*T.nkGolemSlamDmg1713|i}", "R:COOLDOWN ~ {T.nkGolemCd1713*cdm|s}S", "R:RISE AGAIN ~ {T.nkGolemResp1713|s}S", "F:IT STAYS UNTIL KILLED.", "F:WHEN IT IS UP, PRESS AGAIN TO SLAM AT YOUR AIM." } },
            { SkillId.NkOssuaryNova, new[] { "H:BONE SPEARS BURST OUT IN A RING.", "R:DAMAGE ~ {kit*T.nkNovaDmg1713|i}", "R:COOLDOWN ~ {T.nkNovaCd1713*cdm|s}S", "F:READY AFTER {T.nkNovaNeed1713|i} BONE HITS OR {G.NkNovaHarvest1825|i} BONE CHARGES." } },
            { SkillId.NkArmyOfTheDead, new[] { "H:GHOSTS CHARGE AND YOUR ARMY FRENZIES.", "R:COOLDOWN ~ {T.nkArmyCd1713*cdm|s}S", "R:LASTS ~ {T.nkArmyDur1713*dm|s}S", "F:READY WHEN YOU HAVE {T.nkArmyNeed1713|i} MINIONS." } },
            { SkillId.NkBoneColossus, new[] { "H:YOUR MINIONS MERGE INTO ONE GIANT.", "R:DAMAGE ~ {kit*T.nkColDmg1713*(T.nkColDmgBase1713+T.nkColDmgPer1713*T.nkColNeed1713)|i}", "R:COOLDOWN ~ {T.nkColCd1713*cdm|s}S", "F:READY WHEN YOU HAVE {T.nkColNeed1713|i} MINIONS.", "F:WHEN THE GIANT IS KILLED, IT SPLITS BACK INTO THEM." } },
            { SkillId.NkHellGate, new[] { "H:A GATE AT YOUR AIM SPAWNS GHOSTS.", "R:COOLDOWN ~ {T.nkGateCd1713*cdm|s}S", "R:LASTS ~ {T.nkGateDur1713*dm|s}S", "F:READY WHEN YOU HAVE {T.nkGateNeed1713|i} MINIONS." } },
            { SkillId.NkBanquet, new[] { "H:MINION KILLS CALL A GHOST RETINUE.", "R:COOLDOWN ~ {T.nkBanquetCd1713*cdm|s}S", "R:LASTS ~ {T.nkBanquetDur1713*dm|s}S", "F:READY AFTER {T.nkBanquetNeed1713|i} MINION KILLS. AN ELITE COUNTS AS {T.nkBanquetElite1713|i}.", "F:GHOSTS GROW WITH EVERY KILL." } },
            { SkillId.NkGraveLegion, new[] { "H:AT MINION CAP EVERY SKELETON RISES.", "R:COOLDOWN ~ {T.nkLegionCd1772*cdm|s}S", "R:LASTS ~ {T.nkLegionDur1772*dm|s}S", "R:MINION CAP ~ +{G.NkLegionMul1825-1|p}%", "F:THEY RISE AT FULL STRENGTH.", "X:EXTRA SKELETONS CRUMBLE WHEN IT ENDS." } },
        };
        public static readonly Dictionary<SupportId, string[]> Support = new Dictionary<SupportId, string[]>
        {
            { SupportId.MultipleProjectiles, new[] { "H:THE LINKED SKILL FIRES MORE SHOTS.", "R:EXTRA SHOTS ~ {T.supMultiExtra@L|i}", "?multi?X:HITS DEAL {1-T.multiProjDmgMul@L|p}% LESS." } },
            { SupportId.Pierce, new[] { "H:SHOTS PASS THROUGH MORE DEMONS.", "R:PIERCES ~ {T.supPierce@L|i}", "X:HITS DEAL {T.pierceFalloff@L|p}% LESS PER PIERCE." } },
            { SupportId.Fork, new[] { "H:SHOTS SPLIT ON THE FIRST HIT.", "R:SPLITS INTO ~ {T.supForkChildren@L|i}", "F:EACH SPLIT SHOT DEALS {T.supForkDmg@L|p}% DAMAGE." } },
            { SupportId.Bounce, new[] { "H:RETIRED: IT BECOMES CHAIN.", "R:BOUNCES ~ {T.supBounce@L|i}" } },
            { SupportId.Homing, new[] { "H:SHOTS CURVE TOWARD DEMONS.", "R:TURN SPEED ~ {T.supHomingTurn@L|s}", "R:SEEK RANGE ~ {T.supHomingRangeU@L|s}" } },
            { SupportId.BiggerArea, new[] { "H:THE LINKED SKILL HITS A WIDER AREA.", "R:AREA ~ +{T.supAreaMul@L-1|p}%" } },
            { SupportId.Echo, new[] { "H:THE LINKED SKILL CASTS AGAIN." } },
            { SupportId.MoreOrbitals, new[] { "H:MORE SKULLS ORBIT AROUND YOU.", "R:EXTRA SKULLS ~ {T.supOrbitals@L|i}" } },
            { SupportId.Chill, new[] { "H:YOUR HITS LEAVE DEMONS CHILLED.", "R:CHILL LASTS ~ +{T.supChillTime@L-1|p}%", "F:{T.supChillFreezeHits@L|i} CHILLS FREEZE.", "F:CHILLED DEMONS ARE {1-G.EnemyChillMul1825|p}% SLOWER." } },
            { SupportId.Shock, new[] { "H:YOUR HITS LEAVE DEMONS SHOCKED.", "R:SHOCK LASTS ~ {G.StShockS1825|s}S", "F:SHOCKED DEMONS TAKE {T.supShockAmp@L|p}% MORE DAMAGE." } },
            { SupportId.AddedDamage, new[] { "H:THE LINKED SKILL HITS HARDER.", "R:DAMAGE ~ +{T.supAddedDmg@L-1|p}%" } },
            { SupportId.LifeLeech, new[] { "H:PHYSICAL HITS HEAL YOU A LITTLE.", "R:HEALS ~ {T.supLeechFrac@L|p}% OF HIT", "X:HEALS UP TO {T.supLeechCapPerSec@L|s} LIFE PER SECOND.", "X:BURN AND BLEED DAMAGE DO NOT HEAL." } },
            { SupportId.Knockback, new[] { "H:RETIRED: AN OWNED ONE IS DROPPED.", "R:PUSH ~ {T.supKnockback@L|s}" } },
            { SupportId.SoulHarvest, new[] { "H:KILLS DROP MORE XP AND BONES.", "R:XP PER KILL ~ +{T.supHarvestXp@L|i}", "R:BONE CHANCE ~ {T.supHarvestBoneChance@L|p}%" } },
            { SupportId.ExplodeOnKill, new[] { "H:DEMONS IT KILLS EXPLODE ON DEATH.", "F:THE BLAST DEALS {T.supExplodeDmg@L|p}% OF THE DEMON'S LIFE, PLUS {T.explodeFlatDmg|i}.", "X:UP TO {T.explodeMaxDmg|i} DAMAGE PER BLAST." } },
            { SupportId.Chain, new[] { "H:THE LINKED SKILL CHAINS MORE TIMES.", "R:EXTRA JUMPS ~ {T.supChainExtra1651@L|i}", "X:HITS DEAL {1-T.chainDmgMul1651@L|p}% LESS." } },
            { SupportId.Duration, new[] { "H:THE LINKED SKILL LASTS LONGER.", "R:DURATION ~ +{T.supDurMul1651@L-1|p}%" } },
            { SupportId.FasterCasting, new[] { "H:SHORTENS THE LINKED SKILL'S COOLDOWN.", "R:COOLDOWN ~ -{1-T.supFasterCd@L|p}%", "F:SO THE SKILL FIRES MORE OFTEN." } },
            { SupportId.Burn, new[] { "H:YOUR HITS LEAVE DEMONS BURNING.", "R:BURN ~ {T.supBurnMul@L*T.burnDpsFromHit*(T.burnDuration+T.supBurnExtraSeconds@L)|p}% OF HIT", "R:LASTS ~ {T.burnDuration+T.supBurnExtraSeconds@L|s}S", "?T.supBurnHitDmg@L<1?X:HITS DEAL {1-T.supBurnHitDmg@L|p}% LESS." } },
            { SupportId.Executioner, new[] { "H:HITS FINISH OFF WOUNDED DEMONS.", "R:DAMAGE ~ +{T.supExecDmg@L-1|p}%", "R:BELOW ~ {T.supExecThreshold@L|p}% LIFE", "?T.executionerSkipBigs?X:BOSSES AND ELITES ARE IMMUNE." } },
            { SupportId.Bleed, new[] { "H:YOUR HITS LEAVE DEMONS BLEEDING.", "R:BLEED ~ {T.supBleedMul@L*G.StBleedDps1825*G.StBleedS1825|p}% OF HIT", "R:LASTS ~ {G.StBleedS1825|s}S", "?T.supBleedHitDmg@L<1?X:HITS DEAL {1-T.supBleedHitDmg@L|p}% LESS." } },
        };
        public static readonly Dictionary<StatId, string[]> Stat = new Dictionary<StatId, string[]>
        {
            { StatId.MaxHP, new[] { "H:YOU GAIN MORE MAXIMUM LIFE.", "R:MAX LIFE ~ +{T.statHpFrac|p}%" } },
            { StatId.Speed, new[] { "H:YOU MOVE FASTER.", "R:MOVE SPEED ~ +{T.statSpeed|p}%" } },
            { StatId.Pickup, new[] { "H:YOU PICK UP LOOT FROM FARTHER.", "R:PICKUP RANGE ~ +{G.StatPickupAdd1825|p}%" } },
            { StatId.Armor, new[] { "H:YOU TAKE LESS DAMAGE FROM HITS.", "R:ARMOR ~ +{T.statArmor|s}" } },
            { StatId.Crit, new[] { "H:YOUR HITS CRIT MORE OFTEN.", "R:CRIT CHANCE ~ +{T.statCrit|p}%" } },
            { StatId.Slot, new[] { "H:A NEW SKILL SLOT OPENS NOW.", "F:THEN PICK A SKILL." } },
        };

        /// <summary>Status keywords (multi-word first) and their legend hue (wr3 legend: burn orange, chill blue, shock cyan,
        /// bleed pink, curse magenta).</summary>
        public static readonly string[] StatusWords = { "FIRE TRAIL", "BURNING", "BURNS", "BURN", "CHILLING", "CHILLED", "CHILLS", "CHILL", "FREEZES", "FREEZE", "FROZEN",
            "SHOCKED", "SHOCKING", "SHOCKS", "SHOCK", "BLEEDING", "BLEEDS", "BLEED", "CURSED", "CURSES", "CURSE" };
        static readonly Color CBurn = new Color32(0xf7, 0x76, 0x22, 255), CChill = new Color32(0x00, 0x99, 0xdb, 255), CShock = new Color32(0x2c, 0xe8, 0xf5, 255),
            CBleed = new Color32(0xf6, 0x75, 0x7a, 255), CCurse = new Color32(0xb5, 0x50, 0x88, 255);
        public static Color StatusCol(string w)
        {
            if (w.StartsWith("BURN") || w == "FIRE TRAIL") return CBurn;
            if (w.StartsWith("CHILL") || w.StartsWith("FREEZ") || w == "FROZEN") return CChill;
            if (w.StartsWith("SHOCK")) return CShock;
            if (w.StartsWith("BLEED")) return CBleed;
            return CCurse;
        }
    }
}
