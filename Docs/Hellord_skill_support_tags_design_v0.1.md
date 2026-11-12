# Hellord skill and support tags: design v0.1 (t623u). **APPROVED by VH (t626u, 2026-11-02 UTC+7), implemented**

## 0. What already exists in code (not changed by t623u)
`HellordData.cs`:
- `[Flags] enum STag { Projectile, Area, Orbital, Hit, Melee, Aura, Zone, Minion, Any }`
- `SkillDef.tags`
- `SupportDef.needs` (a single tag) and `needsLabel`
- `HData.Fits` and `SkillInst.CanSocket()`, which already block mismatches with the warning "needs a X skill".

So the foundation is already there. This doc proposes how to grow it.

## 1. Proposed tag taxonomy
| Group | Tags | Meaning |
|---|---|---|
| Delivery | Projectile, Melee, Area (AoE), Orbital, Aura, Zone (ground), Chain, Strike (single target) | how it hits |
| Behaviour | Hit, Duration, Minion, Curse, Summon (creates units), Channel (later), Movement (later) | what it does |
| Element | Physical, Fire, Cold, Lightning, Blood, Bone, Soul (Hellord flavour; could map to PoE's chaos) | damage flavour |
| Source | Player vs Minion is derived from Minion; no extra tag | |

The new tags would be Chain, Duration, Curse, the elements, and possibly Strike.

### Current skills with proposed tags (NEW tags in bold)
| Skill | Current | Proposed |
|---|---|---|
| Fireball | Projectile, Area, Hit | + **Fire** |
| Frost Cleave | Melee, Area, Hit | + **Cold** |
| Storm Ring | Aura, Area, Hit | + **Lightning** |
| Bone Orbit | Orbital, Hit | + **Bone, Physical** |
| Curse Rune | Zone, Area, Hit | + **Curse, Duration** |
| Chain Lightning | Area, Hit | + **Chain, Lightning** (is Area actually right? it is jumps) |
| Hellfire Pillar | Area, Hit | + **Fire** |
| Blood Whip | Melee, Area, Hit | + **Blood, Physical** |
| Soul Spears | Projectile, Hit | + **Soul** |
| Grave Pool | Area, Zone, Hit | + **Duration**, Bone? |
| Skeleton Warriors | Minion, Melee, Hit | + **Physical, Bone** |
| Skeleton Archers | Minion, Projectile, Hit | + **Physical, Bone** |

## 2. Match rules (proposal)
- Each SupportDef gets:
  - `requiresAny` (default);
  - optional `requiresAll`;
  - optional `excludes`;
  - optional `category`.
- A support fits a skill if **(skill.tags ∩ requiresAny ≠ ∅ or requiresAny == Any) and (requiresAll ⊆ skill.tags) and (skill.tags ∩ excludes = ∅)**.
- **No two supports of the same category on one skill** (only matters later, when tiers or variants exist).
- **Minion rule:** a minion skill's tags describe what the *minions* do. Any support that matches applies to the minions' attacks. **t622u is the first case:** Multi Projectiles, Pierce, Fork, Bounce and Homing match Skeleton Archers via the Projectile tag and act on the arrows. Warriors (Melee) would get future Melee supports the same way.
- Element supports (proposal): e.g. "Searing Heat: +fire dmg, requires Fire". Burn/Chill/Shock/Bleed stay as Hit supports, as they are today. Optionally a Fire skill could get a discount or bonus from Burn (a matching-element bonus); that's VH's call.
- Card offers already only show supports that fit some owned skill (`CanSocket == null`). Bigger tags make this filter matter more.
- UI: show tag chips on the skill and support cards. The warning text lists the missing tag(s).

### Current supports: needs today → proposal
- **Multi Projectiles / Pierce / Fork / Bounce / Homing**: Projectile → same. Archers included, already done in t622u.
- **Bigger Area / Echo**: Area → same. Echo could exclude Minion (it's odd on summons).
- **More Orbitals**: Orbital → same.
- **Burn / Chill / Shock / Bleed / Life Leech / Knockback / Explode on Kill**: Hit → same. Minions qualify via Hit; is that wanted, and is it wired? See open questions.
- **Added Damage, Faster Casting, Executioner, Soul Harvest**: Any → same.
- Possible new supports: Longer Duration (Duration), Chain+ (Chain), Curse Spread (Curse), Minion Life / Feeding Frenzy-style (Minion), Elemental Focus per element.

## 3. Implementation plan once VH approves (not done)
1. Extend `STag` flags (an int has room).
2. Add `requiresAll`, `excludes` and `category` to SupportDef.
3. Update `HData.Fits`.
4. Retag the skills per the table above.
5. Add tag chips in the UI.

This is small, about 1 file plus UI. Not play-tested, and no code was changed for t623u.

## 4. Open questions for VH
1. Do you want element tags as Fire/Cold/Lightning/Physical, plus Hellord flavours (Blood, Bone, Soul), or only the 4 classic ones?
2. Should Chain Lightning be Chain instead of (or as well as) Area? Right now Bigger Area and Echo fit it.
3. Minions plus Hit supports (Burn, Leech, Knockback, Explode): should they apply to minion hits? If yes, I need to audit whether they're wired, like t622u was for the projectile supports.
4. Do you want a PoE2-style "same category can't stack" rule, or is one-of-each per skill enough (the current rule)?
5. Should Echo or Faster Casting exclude Minion skills?
6. Should tags be shown to the player as chips, or kept internal?

## 5. t626u: VH decisions, approved with the design defaults
- Elements: Physical, Fire, Cold and Lightning, plus Blood, Bone and Soul.
- Chain Lightning: Area + Chain + Lightning. It keeps Area, so Bigger Area and Echo still fit.
- Hit supports apply to minions (kept).
- Category no-stack: the `category` field exists and is enforced only when set. No support sets one yet.
- Echo excludes Minion. Faster Casting does not.
- Tag chips are shown to the player.
- Strike tag: the flag exists but no skill uses it yet.
