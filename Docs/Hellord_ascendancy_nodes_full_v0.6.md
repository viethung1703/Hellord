# Hellord — Ascendancy Node Catalog v0.6 (DESIGN ONLY)
> v1.0 RULE UPDATE (VH): 8 nodes per Path (N1–N8) + 2 Keystones; a run selects MAX 6 nodes (max 1 Keystone, counts within the 6; sockets grow 2→4→6). Crest at 4+ from one Path, so effectively 1 Crest per run. Older "8 sockets / 8 per run / 2 Crests" text is superseded. See hellord_damnation_pact_ui_layout_v1.0.md.
> Naming (VH): this catalog is the **Damnation Pact** node catalog. Player-facing name: Damnation Pact (tab DAMNATION PACT, confirm screen "Your Damnation Pact"). Paths: Pyromancer of the Pit, Bone Warden, Stormcaller, Blood Reaver. Never show "Ascendancy" to players.
Expands v0.5 (meta Pit Altar). Relics are deferred. 4 Paths × 12 nodes (8 Notables, 2 Keystones, 2 Hybrids); the 6 Hybrids are shared between path pairs, so there are 42 unique nodes.
- Hybrids are shared, so each one counts once in the 42.
- S = Souls, Sg = Sigils.
- Every node is a rule, conversion, trigger or trade-off. If a number appears, it rides on a behaviour change or a cost.

## 0. Loadout rules
- **Unlock vs Select:**
  - Unlocking is permanent: spend Souls/Sigils in the Pit Altar.
  - Selecting is free and is set before each run in the Loadout bar.
  - Only unlocked nodes can be selected.
- **Max 8 selected per run.** Sockets grow 2 → 4 → 6 → 8 as described in v0.5 §6.
- **Max 1 Keystone selected.**
- **Prerequisites apply twice:** to unlock a node, its prerequisites must be unlocked; to select it, its prerequisites must also be selected in the same loadout.
- **Hybrids need both:** a Hybrid needs 2 nodes from its home path AND the listed node from the other path, all selected.
- **Crest:** selecting 4 or more nodes from one Path grants that Path's Crest for free. It doesn't use a socket. You can hold at most 2 Crests (4 + 4).
- **No path lock:** mixing paths is free.
- **No free nodes:** every node costs Souls (Keystones also cost a Sigil). Opening the Ascendancy tab is still required (v0.5 §6), but it grants no free node. Each path's N1 is its entry node (30–35 S); every other node in the path needs N1, directly or through its chain, both to unlock and to select.
- **Respec:** loadouts change freely between runs. There's no in-run change.

### Tags/terms used
- **Tags:** Projectile, Area, Orbital, Hit, Melee, Aura, Zone.
- **Elements:** Fire, Frost, Storm, Blood, Bone, Soul.
- **Empowered** = the Cast Cadence glow cast: ×1.5 size and dmg, used only as a rule effect.
- **Energy:**
  - A trigger meter with 100 max.
  - It refills by 2 per kill.
  - It is shown only if a selected node uses it.
- **Heat:** a meter used by Overheat only.

---
## 1. PYROMANCER OF THE PIT (Fire / Projectile / Burn)
**Crest — Pit-Born Flame (4+ nodes):**
- Burning enemies count as "Fire-touched": every Fire on-kill effect treats them as killed by a Fire skill, whatever skill landed the kill.
- Burn also spreads its *remaining duration* instead of resetting.
- *Glyph:* a horned flame crest. *Early:* needs 4 nodes, so it comes mid-meta.

**PY-N1 Kindling** — Notable · 30 S · Req: —
- *Effect:* Fireball impacts leave a Burning Patch: radius 1.2 m, lasts 2 s, Burn ticks. The patch has the **Zone** tag.
- *Tags:* Projectile → adds Zone.
- *Reshapes:* Fireball. BiggerArea/Echo/GravePool-style Zone supports now affect patches.
- *Early:* small patch, about baseline Burn dmg. Safe.
- *VFX/UI:* orange ground decal (reuse Burn VFX tinted). Glyph: an ember on the ground.

**PY-N2 Cinder Split** — Notable · 40 S · Req: N1
- *Effect:* when a Burning enemy dies, its Burn jumps to the 2 nearest enemies within 4 m, with the remaining duration.
- *Tags:* Burn status.
- *Reshapes:* the Burn support, and every Fire skill. It turns single-target Burn into a crowd chain.
- *Early:* jumps are capped to 2, and needs Burn first. Safe.
- *VFX/UI:* small ember arc between enemies. Glyph: a forked flame.

**PY-N3 Scorched Trail** — Notable · 40 S · Req: N1
- *Effect:* your Dash leaves a 3 m fire line for 1.5 s. Fireball/HellfirePillar fired *through* the line become Empowered. Trade-off: Dash cooldown +20%.
- *Tags:* Dash, Zone, Projectile.
- *Reshapes:* turns dash into an offensive set-up. Fireball and Pillar.
- *Early:* cooldown penalty balances it.
- *VFX/UI:* fire line. Glyph: a boot with flame.

**PY-N4 Overheat** — Notable · 60 S · Req: N2
- *Effect:* all Fire skills lose their cooldowns. Each cast adds Heat instead (15–30 per skill). At 100 Heat you **Vent**: a 4 m Fire nova, then you can't cast Fire skills for 1.5 s.
- *Tags:* every Fire skill.
- *Reshapes:* Fireball and Pillar become burst-then-pause rhythms. FasterCasting now fills Heat faster.
- *Early:* the lockout keeps it fair. Shows a Heat bar.
- *VFX/UI:* Heat bar under HP. The hero glows redder as Heat builds. Glyph: a thermometer.

**PY-N5 Meteor Pact** — Notable · 60 S · Req: N3
- *Effect:* every 6th Fireball becomes a **Meteor**. It falls from above at the cursor or nearest pack after 0.6 s, and the Projectile tag is converted to Area (it can't Pierce or Fork).
- *Tags:* Projectile → Area (on that cast).
- *Reshapes:* Fireball. MultipleProjectiles now drops several meteors; BiggerArea scales them.
- *Early:* the delay and conversion keep it fair.
- *VFX/UI:* falling meteor + target ring (reuse Pillar). Glyph: a falling star.

**PY-N6 Ember Orbit** — Notable · 50 S · Req: N2
- *Effect:* Burning enemies killed drop an Ember that orbits you for 5 s (Orbital tag, max 6). Embers fire themselves at the next enemy you hit.
- *Tags:* converts Fire kills into Orbital/Projectile.
- *Reshapes:* links Burn with the BoneOrbit supports (MoreOrbitals raises the cap by +2).
- *Early:* capped at 6. Safe.
- *VFX/UI:* small fire motes. Glyph: a ring of embers.

**PY-N7 Hellfire Lance** — Notable · 70 S · Req: N5
- *Effect:* HellfirePillar no longer drops in place. It fires as a horizontal **beam** along your facing. Beam length scales with BiggerArea, and it Pierces infinitely. Trade-off: cast rate −30%.
- *Tags:* Area → Projectile (line).
- *Reshapes:* HellfirePillar. Pierce and Executioner now matter.
- *Early:* slower cast. Fine.
- *VFX/UI:* a stretched pillar sprite as a beam. Glyph: a lance.

**PY-N8 Pyre Feast** — Notable · 80 S · Req: N4 or N6
- *Effect:* Burning enemies that die heal you 1 HP. Burn can no longer be removed by Chill: Chill on a Burning enemy becomes **Steam**, a 2 m blind cloud where enemies miss 50% of the time.
- *Tags:* Burn × Chill reaction.
- *Reshapes:* mixed Fire/Frost builds (FrostCleave plus the Chill support).
- *Early:* small heal. Safe.
- *VFX/UI:* white steam puff. Glyph: a cauldron.

**PY-K1 Ash Covenant** — Keystone · 150 S + 1 Sg · Req: 2 Pyro Notables
- *Effect:* ALL your damage is Fire. Chill and Shock supports become Burn variants: Chill → "Smolder" (Burn slows 20%); Shock → "Flashburn" (Burn crits). Trade-off: you can never Freeze or Shock, and Frost/Storm skills lose their native status.
- *Tags:* every element → Fire.
- *Reshapes:* the whole kit. Fire resonance is maxed, while FrostCleave and ChainLightning become Fire skills.
- *Early:* Sigil-gated, so floor 2+ meta.
- *VFX/UI:* all skill VFX shift to an orange palette. Glyph: an ash sigil.

**PY-K2 Inferno Heart** — Keystone · 150 S + 1 Sg · Req: N4
- *Effect:* you are always Burning: you take 1% max HP/s, and you can't die from Burn below 1 HP. While Burning, every hit you take casts a Fireball back at the attacker, and your Burn applies to enemies within 2 m (Aura). Healing is −50%.
- *Tags:* adds a self-Aura of Fire.
- *Reshapes:* a melee/close-range Fire tank style. LifeLeech becomes key.
- *Early:* a dangerous self-damage build for experts.
- *VFX/UI:* the hero is engulfed. Glyph: a burning heart.

**PY-H1 Burning Bones** (Hybrid) — 100 S · Req: 2 Pyro nodes + BW-N1
- *Effect:* Orbitals ignite enemies. When a Burning enemy dies, a broken bone regrows instantly.
- *Tags:* Orbital + Burn.
- *Reshapes:* BoneOrbit + the Burn support.
- *Early:* needs 3 nodes. Safe.
- *VFX/UI:* bones with flame tips. Glyph: a flaming bone.

**PY-H2 Plasma Storm** (Hybrid) — 100 S · Req: 2 Pyro nodes + ST-N1
- *Effect:* Shocking a Burning enemy makes **Plasma**: Burn ticks also chain 1 jump of lightning.
- *Tags:* Burn × Shock reaction.
- *Reshapes:* Fireball + ChainLightning + the Shock support.
- *Early:* needs 3 nodes. Safe.
- *VFX/UI:* violet sparks. Glyph: a fire-lightning icon.

| ID | Name | Tier | Cost | Req | Core change |
|---|---|---|---|---|---|
| N1 | Kindling | Notable | 30 | — | Fireball leaves Zone patches |
| N2 | Cinder Split | Notable | 40 | N1 | Burn jumps on death |
| N3 | Scorched Trail | Notable | 40 | N1 | Dash fire line Empowers |
| N4 | Overheat | Notable | 60 | N2 | Heat replaces cooldowns |
| N5 | Meteor Pact | Notable | 60 | N3 | 6th Fireball = Meteor (Area) |
| N6 | Ember Orbit | Notable | 50 | N2 | Fire kills → orbit embers |
| N7 | Hellfire Lance | Notable | 70 | N5 | Pillar → piercing beam |
| N8 | Pyre Feast | Notable | 80 | N4/N6 | Steam reaction + Burn heal |
| K1 | Ash Covenant | Keystone | 150+1Sg | 2 Notables | Everything Fire |
| K2 | Inferno Heart | Keystone | 150+1Sg | N4 | Self-Burn aura tank |
| H1 | Burning Bones | Hybrid | 100 | 2 + BW-N1 | Igniting orbitals |
| H2 | Plasma Storm | Hybrid | 100 | 2 + ST-N1 | Burn+Shock chain |

---
## 2. BONE WARDEN (Orbital / Zone / Minions)
**Crest — Ossuary Lord (4+ nodes):**
- Every Zone you create (GravePool, CurseRune, Kindling patches) spawns 1 temporary bone that orbits the zone's center and hits enemies inside.
- Orbitals count as Zones for Zone supports.
- *Glyph:* a skull crown.

**BW-N1 Marrow Launch** — Notable · 35 S · Req: —
- *Effect:* every 3 s, BoneOrbit bones fire outward 6 m and return. While flying they have the **Projectile** tag, so Pierce/Fork/MultipleProjectiles/Homing apply.
- *Tags:* Orbital → +Projectile.
- *Reshapes:* BoneOrbit becomes a boomerang build.
- *Early:* the 3 s cadence leaves gaps in defense.
- *VFX/UI:* a bone trail. Glyph: an outward bone arrow.

**BW-N2 Ossuary Ward** — Notable · 40 S · Req: N1
- *Effect:* orbitals block enemy projectiles. Each bone breaks after 3 blocks and regrows after 4 s. Trade-off: orbit radius −15%.
- *Tags:* Orbital → defense.
- *Reshapes:* turns BoneOrbit into a shield against ranged demons.
- *Early:* the strongest defensive node. Fine.
- *VFX/UI:* a crack effect on block. Glyph: a shield of bones.

**BW-N3 Grave Anchor** — Notable · 40 S · Req: N1
- *Effect:* GravePool no longer spawns at enemies. It is anchored under you and follows at 60% of your move speed. Trade-off: the pool only deals damage while you are inside it.
- *Tags:* Zone → Aura-like.
- *Reshapes:* GravePool. BiggerArea and the Chill/Bleed supports become a personal field.
- *Early:* encourages slow, deliberate play.
- *VFX/UI:* a pool trailing behind you. Glyph: an anchor.

**BW-N4 Bone Legion** — Notable · 60 S · Req: N2
- *Effect:* every 25th kill raises a Skeleton minion (max 5). It lasts 20 s and copies your Orbital and Melee supports (Bleed, Knockback, LifeLeech apply to it).
- *Tags:* adds Minion.
- *Reshapes:* support choices now feed minions.
- *Early:* a slow ramp. Capped.
- *VFX/UI:* reuse a tinted enemy skeleton sprite. Glyph: a skull with a count.

**BW-N5 Rune Ossuary** — Notable · 50 S · Req: N3
- *Effect:* CurseRune now places a bone circle that lasts until its next cast. Enemies cursed inside take hits from your orbitals at the rune instead of around you.
- *Tags:* Zone + Orbital relocation.
- *Reshapes:* CurseRune + BoneOrbit "turret" play.
- *Early:* safe.
- *VFX/UI:* a rune circle with orbiting bones. Glyph: a rune.

**BW-N6 Calcify** — Notable · 60 S · Req: N2
- *Effect:* the Chill support on Orbital/Zone hits becomes **Calcify**. After 4 stacks the enemy is stunned 1 s and takes Bone damage on the next hit. Chill no longer slows from these sources.
- *Tags:* Chill → stun stacks.
- *Reshapes:* Chill + BoneOrbit/GravePool.
- *Early:* needs 4 stacks.
- *VFX/UI:* grey bone crust. Glyph: a calcified hand.

**BW-N7 Marrow Harvest** — Notable · 70 S · Req: N4
- *Effect:* the SoulHarvest support now also collects bones. Every 10 kills adds +1 temporary orbital for the room, max +4. Clearing the room resets it.
- *Tags:* Orbital scaling per room.
- *Reshapes:* SoulHarvest + MoreOrbitals.
- *Early:* resets each room, so no snowball.
- *VFX/UI:* a counter pip. Glyph: a bone bundle.

**BW-N8 Wall of the Dead** — Notable · 80 S · Req: N5 or N6
- *Effect:* while 3+ minions or 6+ orbitals are active, Dash places a 4 m bone wall for 3 s. The wall blocks enemies and shatters for Area damage when it ends.
- *Tags:* Dash → Zone/Area.
- *Reshapes:* a crowd-control build.
- *Early:* conditional.
- *VFX/UI:* a bone wall sprite (M). Glyph: a wall.

**BW-K1 Sloth Throne** — Keystone · 150 S + 1 Sg · Req: 2 BW Notables
- *Effect:* standing still 0.5 s doubles the radius and hit rate of Zones and Orbitals. Moving shrinks them to 50% radius. Trade-off: move speed −10%.
- *Tags:* Zone/Orbital conditional.
- *Reshapes:* a turret/kite rhythm build.
- *Early:* Sigil-gated.
- *VFX/UI:* a throne ring shown when still. Glyph: a throne.

**BW-K2 Necro Covenant** — Keystone · 150 S + 1 Sg · Req: N4
- *Effect:* you can't hold Projectile skills (they are banished from the card pool). Instead, Bone Legion's max is 12, minions are permanent until they die, and every Projectile support you pick applies to minion attacks.
- *Tags:* Projectile → Minion.
- *Reshapes:* a full summoner build.
- *Early:* a big commitment.
- *VFX/UI:* a minion count HUD. Glyph: a crowned skull.

**BW-H1 Blood Marrow** (Hybrid) — 100 S · Req: 2 BW + BR-N1
- *Effect:* Bleeding enemies hit by orbitals lose 1 bone's worth of HP as Bone damage. LifeLeech from orbitals is doubled.
- *Tags:* Orbital × Bleed.
- *Reshapes:* BloodWhip + BoneOrbit.
- *Early:* safe.
- *VFX/UI:* red-tipped bones. Glyph: a bloody bone.

**BW-H2 Grave Conduit** (Hybrid) — 100 S · Req: 2 BW + ST-N1
- *Effect:* Chain Lightning can chain to your minions and zones; each one re-hits around itself.
- *Tags:* Chain × Zone/Minion.
- *Reshapes:* ChainLightning + GravePool/Legion.
- *Early:* safe.
- *VFX/UI:* lightning to minions. Glyph: a skull with a spark.

| ID | Name | Tier | Cost | Req | Core change |
|---|---|---|---|---|---|
| N1 | Marrow Launch | Notable | 35 | — | Orbitals gain Projectile |
| N2 | Ossuary Ward | Notable | 40 | N1 | Bones block projectiles |
| N3 | Grave Anchor | Notable | 40 | N1 | Pool follows you |
| N4 | Bone Legion | Notable | 60 | N2 | Skeleton minions |
| N5 | Rune Ossuary | Notable | 50 | N3 | Orbit moves to rune |
| N6 | Calcify | Notable | 60 | N2 | Chill → stun stacks |
| N7 | Marrow Harvest | Notable | 70 | N4 | Room-scaling orbitals |
| N8 | Wall of the Dead | Notable | 80 | N5/N6 | Dash bone wall |
| K1 | Sloth Throne | Keystone | 150+1Sg | 2 Notables | Still = ×2 zones |
| K2 | Necro Covenant | Keystone | 150+1Sg | N4 | Summoner, no projectiles |
| H1 | Blood Marrow | Hybrid | 100 | 2 + BR-N1 | Orbit × Bleed |
| H2 | Grave Conduit | Hybrid | 100 | 2 + ST-N1 | Chains via minions/zones |

---
## 3. STORMCALLER (Shock / Chain / Triggers)
**Crest — Eye of the Tempest (4+ nodes):**
- Every 4th trigger proc is free: no Energy is spent.
- Shock on an enemy makes it count as a chain target for *all* your skills: Fork, Bounce and Homing prefer shocked enemies.
- *Glyph:* a storm eye.

**ST-N1 Conduit** — Notable · 35 S · Req: —
- *Effect:* ChainLightning can chain through your own orbitals, zones and StormRing. Each conduit re-emits 1 chain.
- *Tags:* Chain × Orbital/Zone.
- *Reshapes:* ChainLightning + any persistent effect.
- *Early:* safe.
- *VFX/UI:* chain arcs to your objects. Glyph: a lightning node.

**ST-N2 Storm Capacitor** — Notable · 40 S · Req: N1
- *Effect:* StormRing no longer pulses. It stores 1 charge per 0.5 s, up to 10, and releases all of them as one expanding ring on Dash.
- *Tags:* Aura → Dash-triggered Area.
- *Reshapes:* StormRing becomes a burst-on-dash skill. BiggerArea scales the release.
- *Early:* readable. Safe.
- *VFX/UI:* a charge counter on the ring. Glyph: a battery.

**ST-N3 Cast on Shock** — Notable · 60 S · Req: N1
- *Effect:* applying Shock triggers your lowest-level skill at the shocked enemy. It costs 20 Energy, and there's a 0.3 s internal cooldown.
- *Tags:* adds Trigger.
- *Reshapes:* makes low-level skills useful. Shock support becomes the core.
- *Early:* limited by Energy. Shows the Energy bar.
- *VFX/UI:* a flash glyph on the triggered skill icon. Glyph: a bolt + arrow.

**ST-N4 Static Fork** — Notable · 50 S · Req: N1
- *Effect:* the Fork and Bounce supports are merged for Storm skills. Each fork also bounces once, and each bounce forks once (depth 2). Trade-off: per-hit dmg −25% for Storm skills.
- *Tags:* Projectile/Chain.
- *Reshapes:* ChainLightning, SoulSpears infused with Shock.
- *Early:* depth-capped.
- *VFX/UI:* none new. Glyph: a branching bolt.

**ST-N5 Overcharge** — Notable · 60 S · Req: N3
- *Effect:* Energy can go over 100, up to 200. While above 100, triggered skills are Empowered. Below 20 Energy, triggers stop.
- *Tags:* Energy economy.
- *Reshapes:* trigger rhythm. FasterCasting reduces trigger cost instead.
- *Early:* safe.
- *VFX/UI:* the Energy bar glows. Glyph: a double bolt.

**ST-N6 Lightning Rod** — Notable · 50 S · Req: N2
- *Effect:* standing on your own Zone (GravePool, CurseRune) makes it a Rod. Every 1 s, lightning strikes a random enemy within 6 m of the zone.
- *Tags:* Zone → Storm.
- *Reshapes:* Zone skills.
- *Early:* safe.
- *VFX/UI:* a rod sprite. Glyph: a rod.

**ST-N7 Cast on Kill** — Notable · 70 S · Req: N3
- *Effect:* killing a Shocked enemy triggers your *highest*-level non-Storm skill. It costs 35 Energy, with a 0.6 s internal cooldown.
- *Tags:* Trigger.
- *Reshapes:* combo chains with N3, e.g. Shock → Fireball → kill → Pillar.
- *Early:* expensive.
- *VFX/UI:* a chain-link glyph.

**ST-N8 Thunder Step** — Notable · 80 S · Req: N2 or N5
- *Effect:* Dash becomes a blink 4 m along a lightning line that Shocks everything crossed. Trade-off: Dash has 1 charge less (min 1).
- *Tags:* Dash → Storm Area.
- *Reshapes:* mobility. Shock application from dash.
- *Early:* safe.
- *VFX/UI:* a blink line. Glyph: a winged bolt.

**ST-K1 Unbound Chains** — Keystone · 150 S + 1 Sg · Req: 2 Storm Notables
- *Effect:* triggers cost no Energy and have no internal cooldown. Each trigger costs 1 HP, and HP regeneration is disabled.
- *Tags:* Trigger.
- *Reshapes:* builds need LifeLeech or Blood Reaver to sustain.
- *Early:* Sigil-gated. Self-harm.
- *VFX/UI:* red chains on the HUD. Glyph: broken shackles.

**ST-K2 Elemental Equilibrium** — Keystone · 150 S + 1 Sg · Req: N4
- *Effect:* hitting an enemy with one element makes it take ×1.4 damage from *other* elements and ×0.7 from the same element, for 3 s.
- *Tags:* all elements.
- *Reshapes:* forces mixed-element rotations. Infusion/convert ideas from v0.2 fit here.
- *Early:* expert.
- *VFX/UI:* an element icon above enemies. Glyph: a balance scale.

**ST-H1 Plasma Storm** — shared with PY-H2. It can be unlocked from either path with the same requirements.

**ST-H2 Blood Lightning** (Hybrid) — 100 S · Req: 2 Storm + BR-N1
- *Effect:* each chain jump heals 0.5 HP and costs 0.5% max HP when cast. On enemies at low HP, chain jumps apply Bleed.
- *Tags:* Chain × Blood.
- *Reshapes:* ChainLightning + LifeLeech/Bleed.
- *Early:* net neutral.
- *VFX/UI:* red lightning. Glyph: a blood bolt.

| ID | Name | Tier | Cost | Req | Core change |
|---|---|---|---|---|---|
| N1 | Conduit | Notable | 35 | — | Chains through your objects |
| N2 | Storm Capacitor | Notable | 40 | N1 | Ring stores, dash releases |
| N3 | Cast on Shock | Notable | 60 | N1 | Shock triggers low skill |
| N4 | Static Fork | Notable | 50 | N1 | Fork×Bounce merge |
| N5 | Overcharge | Notable | 60 | N3 | Energy 200, Empower |
| N6 | Lightning Rod | Notable | 50 | N2 | Zones strike lightning |
| N7 | Cast on Kill | Notable | 70 | N3 | Shock-kill triggers top skill |
| N8 | Thunder Step | Notable | 80 | N2/N5 | Blink dash shocks |
| K1 | Unbound Chains | Keystone | 150+1Sg | 2 Notables | Free triggers cost HP |
| K2 | Elemental Equilibrium | Keystone | 150+1Sg | N4 | Rotate elements |
| H1 | Plasma Storm | Hybrid | 100 | 2 + PY-N1 | Burn+Shock |
| H2 | Blood Lightning | Hybrid | 100 | 2 + BR-N1 | Chains heal/bleed |

---
## 4. BLOOD REAVER (Melee / Life / Risk)
**Crest — Crimson Pact (4+ nodes):**
- Missing HP fuels you: every 10% missing HP makes Melee skills hit 1 extra time per 3 casts.
- Overheal from any source becomes a Blood Shield, max 30% HP.
- *Glyph:* a blood chalice.

**BR-N1 Leech Convert** — Notable · 30 S · Req: —
- *Effect:* LifeLeech above max HP becomes a Blood Shield, max 20% HP. When the shield breaks, it explodes for Area Blood damage.
- *Tags:* LifeLeech → Area.
- *Reshapes:* LifeLeech support.
- *Early:* defensive. Safe.
- *VFX/UI:* a red bubble. Glyph: a drop with a shield.

**BR-N2 Price of Wrath** — Notable · 40 S · Req: N1
- *Effect:* holding Dash for 0.3 s toggles **Wrath**: all skills cast 2× faster and each cast costs 1% max HP. Wrath can't kill you; it turns off at 10% HP.
- *Tags:* all skills, toggle.
- *Reshapes:* turns HP into a resource.
- *Early:* opt-in. Safe.
- *VFX/UI:* a red screen vignette. Glyph: a horned fist.

**BR-N3 Whip Reach** — Notable · 40 S · Req: N1
- *Effect:* BloodWhip hits in a full 360° circle, and its attack rate is halved. Melee supports (Knockback, Executioner) apply in all directions.
- *Tags:* Melee → Area.
- *Reshapes:* BloodWhip; also FrostCleave if the Blood element is infused.
- *Early:* safe.
- *VFX/UI:* a circular whip arc. Glyph: a spiral whip.

**BR-N4 Hemorrhage** — Notable · 60 S · Req: N3
- *Effect:* Bleed no longer ticks. It stores damage and bursts all of it when the enemy is hit by a Melee skill or reaches 5 stacks.
- *Tags:* Bleed → burst.
- *Reshapes:* Bleed + BloodWhip/FrostCleave.
- *Early:* safe.
- *VFX/UI:* a swelling red pip. Glyph: a burst drop.

**BR-N5 Red Feast** — Notable · 50 S · Req: N1
- *Effect:* killing with Melee spawns a blood orb. Picking it up heals 2 HP and extends Wrath, or gives +5 Energy if a Storm path is present. Trade-off: natural regeneration is disabled.
- *Tags:* Melee kills → pickups.
- *Reshapes:* a sustain style.
- *Early:* the trade-off is mild.
- *VFX/UI:* red orbs. Glyph: a fang.

**BR-N6 Frenzy Chain** — Notable · 60 S · Req: N2
- *Effect:* each Melee hit within 1 s of the last builds Frenzy, up to 10. At 10, your next Melee skill is a **Blood Nova**, then Frenzy resets. Getting hit loses 3 stacks.
- *Tags:* combo / Melee.
- *Reshapes:* rewards staying in melee.
- *Early:* readable counter.
- *VFX/UI:* a counter ring. Glyph: claw marks.

**BR-N7 Sanguine Spears** — Notable · 70 S · Req: N4
- *Effect:* SoulSpears become Blood spears. They return to you, healing 0.5 HP per enemy pierced, and gain the Melee tag for Bleed/Executioner.
- *Tags:* Projectile → +Melee.
- *Reshapes:* SoulSpears.
- *Early:* safe.
- *VFX/UI:* red spear tint. Glyph: a blood spear.

**BR-N8 Last Rites** — Notable · 80 S · Req: N5 or N6
- *Effect:* once per room, lethal damage instead leaves you at 1 HP, Empowers every skill for 4 s and makes you invulnerable for 1 s. Trade-off: max HP −10%.
- *Tags:* death prevention.
- *Reshapes:* enables Martyr and other risk builds.
- *Early:* very forgiving. Good for new players.
- *VFX/UI:* a skull flash. Glyph: a coffin.

**BR-K1 Martyr** — Keystone · 150 S + 1 Sg · Req: 2 BR Notables
- *Effect:* max HP is halved. Below 30% HP every skill is Empowered and leeches 5%. Above 70% HP, skills deal −20%.
- *Tags:* all.
- *Reshapes:* the build seeks low HP. Synergizes with Last Rites and Wrath.
- *Early:* expert.
- *VFX/UI:* the HP bar shows thresholds. Glyph: a broken heart.

**BR-K2 Blood Magic** — Keystone · 150 S + 1 Sg · Req: N2
- *Effect:* Energy/Heat/cooldown meters are replaced by HP. Skills have no cooldown and each cast costs HP: 0.5–2% depending on the skill. LifeLeech is tripled. You can't hold the Aura tag (StormRing is banished).
- *Tags:* all skill costs.
- *Reshapes:* the whole rhythm.
- *Early:* expert.
- *VFX/UI:* the HP bar pulses on each cast. Glyph: a blood sigil.

**BR-H1 Blood Marrow** — shared with BW-H1.

**BR-H2 Crimson Pyre** (Hybrid) — 100 S · Req: 2 BR + PY-N1
- *Effect:* Burn on Bleeding enemies cauterizes. The enemy stops Bleeding, takes all the stored Bleed as Fire damage at once, and you heal 1 HP.
- *Tags:* Bleed × Burn.
- *Reshapes:* BloodWhip + Fireball / the Burn support.
- *Early:* safe.
- *VFX/UI:* a red-orange burst. Glyph: a burning drop.

| ID | Name | Tier | Cost | Req | Core change |
|---|---|---|---|---|---|
| N1 | Leech Convert | Notable | 30 | — | Overheal → exploding shield |
| N2 | Price of Wrath | Notable | 40 | N1 | HP-for-speed toggle |
| N3 | Whip Reach | Notable | 40 | N1 | Whip 360°, slower |
| N4 | Hemorrhage | Notable | 60 | N3 | Bleed stores, bursts |
| N5 | Red Feast | Notable | 50 | N1 | Melee kills drop orbs, no regen |
| N6 | Frenzy Chain | Notable | 60 | N2 | Combo → Blood Nova |
| N7 | Sanguine Spears | Notable | 70 | N4 | Spears return, Melee tag |
| N8 | Last Rites | Notable | 80 | N5/N6 | Cheat death per room |
| K1 | Martyr | Keystone | 150+1Sg | 2 Notables | Low HP power |
| K2 | Blood Magic | Keystone | 150+1Sg | N2 | HP replaces cooldowns |
| H1 | Blood Marrow | Hybrid | 100 | 2 + BW-N1 | Orbit × Bleed |
| H2 | Crimson Pyre | Hybrid | 100 | 2 + PY-N1 | Bleed+Burn cauterize |

---
## 5. Hybrids overview (unique set: 6 hybrids, each reachable from both parent paths)
| Hybrid | Paths | Requires (selected) | Effect |
|---|---|---|---|
| Burning Bones | Pyro + Bone | 2 Pyro + BW-N1 (or 2 BW + PY-N1) | Orbitals ignite; Burn kills regrow bones |
| Plasma Storm | Pyro + Storm | 2 Pyro + ST-N1 (or 2 ST + PY-N1) | Burn × Shock → chaining Burn ticks |
| Blood Marrow | Bone + Blood | 2 BW + BR-N1 (or 2 BR + BW-N1) | Orbitals feed on Bleed, LifeLeech ×2 |
| Grave Conduit | Bone + Storm | 2 BW + ST-N1 (or 2 ST + BW-N1) | Chains through minions/zones |
| Blood Lightning | Storm + Blood | 2 ST + BR-N1 (or 2 BR + ST-N1) | Chains heal/Bleed, cost HP |
| Crimson Pyre | Blood + Pyro | 2 BR + PY-N1 (or 2 PY + BR-N1) | Bleed+Burn cauterize burst |

All 6 path pairs are covered.
- **Per-path count:** each path lists 3 hybrids it can reach. Its "2 hybrid nodes" in the 12-node count are the two that use that path as home. The 6 unique hybrids are shared.
- **Unlocking:** each hybrid is unlocked once, for 100 S, and can be bought from either parent path.
- **Grand total: 42 unique nodes:**
  - 32 Notables.
  - 8 Keystones.
  - 6 Hybrids.

## 6. Example 8-node loadouts
- **Meteor Burner:** PY-N1, N3, N5, N2, N4 (Crest), ST-N1, PY-H2 Plasma Storm, PY-K1 Ash Covenant.
- **Bone Boomerang:** BW-N1, N2, N3, N5 (Crest), ST-N1, BW-H2 Grave Conduit, ST-N4 Static Fork, BW-K1 Sloth Throne.
- **Trigger Chain:** ST-N1, N3, N5, N7 (Crest), PY-N1, Plasma Storm, ST-N8, ST-K1 Unbound Chains.
- **Blood Martyr Whip:** BR-N1, N2, N3, N6 (Crest), N8 Last Rites, BW-N1, BR-H1 Blood Marrow, BR-K1 Martyr (8 nodes).

## 7. UI notes
- **Pit Altar Ascendancy tab:**
  - 4 columns, one per path. Each column lists N1 at the top, then the chains, then Keystones at the bottom (sealed with a Sigil lock).
  - The Hybrids row sits between columns as diamonds linking 2 paths.
- **Node card:**
  - Name, tier frame, cost, Req chips (red if not selected), effect text, tag chips, and "Reshapes: [skill icons]".
- **Loadout bar:**
  - 8 sockets, 1 gold Keystone socket, Crest slots lighting up at 4.
  - Invalid loadouts are shown with red reasons.
- **In-run HUD:**
  - 8 tiny glyphs under the XP bar; each flashes on proc.
  - Heat/Energy/Frenzy meters appear only if a selected node needs them.
- **Glyph art:**
  - About 42 node icons at 16×16 or 24×24 pixel art, plus 4 Crests: S each, M total.
  - New VFX needed (M): beam, meteor, bone wall, blood shield, blink line, steam. The rest reuse existing VFX with tints.

## 8. Open questions (max 5)
1. Should hybrids be shared single nodes (6 total, 42 unique), or 8 distinct per-path nodes (48)?
2. Entry N1 cost: 30–35 S means a first run (about 40 S) buys exactly 1 path entry. OK, or should N1 be cheaper (about 20 S) so new players can try 2 paths sooner?
3. Should Keystones that banish skills (Necro Covenant, Blood Magic) be allowed, or are they too restrictive?
4. Energy/Heat/Frenzy meters: OK to add HUD meters, or keep at most 1 extra meter per run?
5. Are Souls costs (40–80 Notables, 150 + Sigil Keystones) the right pace for about 25–35 runs?
