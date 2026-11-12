# Hellord: Necromancer & Summoner skills (design v0.2)
*t516u + t517u + t518u, 2026-11-01 (UTC+7). Replaces v0.1. Skill set = 4, all chosen by VH: Skeleton Warriors, Skeleton Archers (Necro); Wolf, Bear (Summoner). Design only: nothing is coded or wired. All numbers are placeholders for VH to tune.*

> **STANDING NOTE (VH, t516u): design skills WITH VH.** Next time, propose options and **wait for VH to choose or design** before writing them up or applying anything. Don't ship a full skill list on our own.

**What changed from v0.1 (VH ruling):**
- **Removed:** Raise Ossuant, every corpse mechanic, Corpse Pop, Grave Lantern, Dirge of Ash and Bone Harvest. Their evolutions (Rattling Host, Pyre of the Fallen, Wailing Choir, Funeral Rune, the Bone Harvest/Marrow evo) and corpse-only supports (Feast of Bones) are gone too.
- **Why:** if a minion skill is your first pick, you have nothing to kill with, so a "kill, then raise" loop can't start. Minions now work like PoE2: cast once, they exist, and if one dies it **comes back after a cooldown**.
- **The Necromancer now has exactly two skills:** Warrior Skeleton and Archer Skeleton.

---

## 0. Locked by VH (t518u)

- **Respawn:** applies to **all four skills** (Skeleton Warriors, Skeleton Archers, Wolf, Bear). A dead minion reappears **beside the player** after that skill's respawn cooldown. No corpses, no recast.
- **No evolutions:** none for these skills. Bone Phalanx and Bone Volley are dropped.
- **Art: 4 separate sets**, one per skill:
  - a **warrior skeleton** (Skeleton Warriors);
  - an **archer skeleton** (Skeleton Archers), not a shared skeleton with a weapon swap;
  - a **wolf** (Summon Wolf);
  - a **bear** (Summon Bear).
- **What "Necro Covenant" is:** the **Bone Warden Pact Keystone K2** (meta-progression node), not a summoner system. When taken:
  - Bone Legion becomes bigger and permanent (12 skeletons);
  - the global minion cap goes to 20;
  - **player** projectile skills are banned.
  - Recommended default (not yet decided by VH): minion attacks are not "player projectiles", so Skeleton Archers still shoot under Covenant. Wolf and Bear are melee and are unaffected.

---

## 1. Fantasy: a PoE2-style standing bone army

- **No corpses needed.** Taking the skill gives skeletons straight away, so it works as a first pick in an empty room and in boss fights.
- **Persistent:** skeletons stay until killed. A dead skeleton **reappears at your side after its respawn cooldown** (bone-dust rise animation). No raise button, no corpse targeting.
- **Leveling the skill** = **more skeletons + more damage** per skeleton. **Max level 5**, same as every other skill (t587u, VH).
- **Auto-play:** skeletons auto-target the nearest enemy to the *player*, never stray more than **7 m**, and teleport back beyond 10 m. No micro-management.
- **Colour:** bone white + sickly green (#e8d5a0 / #5ac54f), with a sand outline (#ead4aa) so they never read as enemies.

---

## 2. Skill A: **Summon Skeleton Warriors**

**Tags:** `Minion`, `Melee`, `Bone`

**What it does:**
- Keeps a squad of sword-and-shield skeleton warriors around you. They charge the nearest enemy and slash (short cleave, small knockback).
- They are the frontline: tanky, they body-block enemies and soak bullets aimed at you.

**Respawn rule:**
- Each warrior that dies starts its **own respawn timer (placeholder 6 s)**, then reappears beside the player.
- There is no global recast; the count always tops back up to the current level's cap.
- Level-ups apply instantly: new bodies spawn right away and existing ones gain the damage.

| Lv | Warriors | Dmg per hit (placeholder) | Respawn |
|---|---|---|---|
| 1 | 2 | 6 | 6.0 s |
| 2 | 3 | 9 | 5.5 s |
| 3 | 3 | 12 | 5.0 s |
| 4 | 4 | 15 | 5.0 s |
| 5 | 5 | 18 | 4.5 s |

**Base stats (placeholder):** life 40 (+10% per level), attack every 0.9 s, move speed 1.1x the player's.


---

## 3. Skill B: **Summon Skeleton Archers**

**Tags:** `Minion`, `Projectile`, `Bone`

**What it does:**
- Keeps skeleton archers in a loose ring **3–4 m behind/around you**. They shoot bone arrows at the nearest enemy in range (8 m).
- They are the backline: fragile, steady ranged damage, and they keep a distance from enemies.

**Respawn rule:** same as warriors, with a per-archer **respawn timer (placeholder 8 s)** that tops back up to the current cap.

| Lv | Archers | Dmg per arrow (placeholder) | Respawn |
|---|---|---|---|
| 1 | 1 | 5 | 8.0 s |
| 2 | 2 | 7 | 7.5 s |
| 3 | 3 | 9 | 7.0 s |
| 4 | 3 | 11 | 6.0 s |
| 5 | 4 | 14 | 5.5 s |

**Base stats (placeholder):** life 20 (+10% per level), shoots every 1.2 s, arrow speed 9 m/s.


---

## 4. Shared minion rules (still apply)

- **Caps:**
  - Global minion cap is **12** (warriors + archers + wolves + bear + Bone Legion).
  - Necro Covenant (K2) raises it to **20**.
  - If the cap is full, the skill waits for space: nothing is culled and nothing is queued.
- **MINION tag (VH t522u):** every summoned unit (Skeleton Warriors, Skeleton Archers, wolves, bear, and any future summon) carries the `Minion` tag. Any node or support that says "minion" applies to all of them. Skeletons also carry `Undead`; wolves and the bear carry `Beast`. `Melee` and `Projectile` follow how the minion attacks.
- **Bullet blocking is baseline (VH t521u):** every summon (warriors, archers, wolves, bear) already blocks enemy bullets in game. No Pact node sells bullet-block or body-block.
- **Stay near the player:** 7 m leash, teleport back beyond 10 m (see section 1).
- **Damage:** minions use the skill's own damage and level, plus **50%** of the player's generic Added Dmg / crit. They don't use the player's Speed.
- **Supports copy to minions** (matches Bone Legion's "COPIES SUPPORTS"):
  - **Archers:** projectile supports (Multiple Proj, Pierce, Fork, Bounce, Homing) work normally.
  - **Warriors:** projectile supports convert: Multiple Proj = +1 cleave target, Pierce = armour break.
  - **Elemental supports** (Burn, Chill, Shock, Bleed) always copy.
  - **Echo:** the respawn brings back 2 at once when 2 or more are dead.
- **Bone Legion synergy:** Legion skeletons count as minions and use the global cap. Kills by any minion count toward Legion's 25-kill counter. Legion skeletons get a bone-gold crest so the player can tell them apart.
- **Readability:**
  - Archers are the only minion projectile type.
  - Minions use the sand outline, never enemy red.
  - Minion hit sounds are -6 dB vs player hits.
- **Bosses:** no special rule is needed now that there is no corpse dependency.
- **Supports kept from v0.1 (minion-only, still valid):** Gravebound (+30% life, bone burst on death), Thrall's Haste, Legion Banner (+1 cap, -10% dmg each), Soul Leash.
  - Feast of Bones is removed (it used corpses).
  - Blood Tithe is removed (Tithe superseded).

---

## 5. Bone Warden Pact mapping (what still applies)

| Node | Use with skeletons |
|---|---|
| **N2 Ossuary Ward** | Fits the "army around you" defence of warriors body-blocking. |
| **N4 Bone Legion** | Core node: minion kills feed the counter, and Legion skeletons share the global cap. |
| **N6 Calcify** | **Proposal:** warrior hits and archer arrows add Calcify stacks. |
| **N8 Wall of the Dead** | **Proposal:** the dash wall spends 2 skeletons, which then go on respawn. A real sacrifice with no permanent loss. |
| **K1 Sloth Throne** | Minions near a player who is standing still get x1.5. |
| **K2 Necro Covenant** | Bans *player* projectile skills; Legion is 12 permanent. **Recommended:** archers are minions, so they are **not** banned. |

- **Dropped from the mapping:** N1 Marrow Launch, N3 Grave Anchor, N5 Rune Ossuary and N7 Marrow Harvest. Their v0.1 links were to removed corpse/raise skills. The nodes themselves are unchanged.

---

## 6. Summoner side (VH t517u): Wolf + Bear

- **Superseded:** the v0.1 Hellhound, Imp Rabble, Gorger and Tithe ideas are replaced by Wolf + Bear. No other Summoner skills.
- **Shared rules:** Wolf and Bear follow every rule in section 4: global cap, leash, damage formula, supports copy, and respawn after a cooldown (no corpses).
- **Colour:** Hell red + brimstone gold (#a22633 / #feae34), sand outline.

### Skill C: **Summon Wolf**
*Fantasy: a hell-wolf pack that runs down whatever you point at.*

**Tags:** `Minion`, `Melee`, `Beast`

**What it does:**
- Fast wolves circle near you, dart at the nearest enemy, bite, and dart out again (hit-and-run).
- **Respawn:** each dead wolf comes back beside you after **7 s** (placeholder).
- **Leveling (VH rule):** increases wolf **damage and count**.

| Lv | Wolves | Bite dmg (placeholder) | Respawn |
|---|---|---|---|
| 1 | 1 | 7 | 7.0 s |
| 2 | 2 | 9 | 6.5 s |
| 3 | 3 | 11 | 6.5 s |
| 4 | 4 | 13 | 6.0 s |
| 5 | 5 | 16 | 5.5 s |

**Base stats (placeholder):** life 30 (+10% per level), bite every 0.7 s, move speed 1.4x the player's.

### Skill D: **Summon Bear**
*Fantasy: one huge hell-bear that slams the ground.*

**Tags:** `Minion`, `Melee`, `Area`, `Beast`

**What it does:**
- **Always exactly 1 bear.** It walks with you and draws enemy attention.
- **One default skill only: Slam** (VH t520u lock). The bear pounds the ground: area damage around it plus a short stun. Maul and Roar are removed.
- **Respawn:** comes back beside the player after **15 s** (placeholder).
- **Leveling:** **Slam damage + bigger Slam area**. The count never goes up.
- **Changes to Slam itself come later**, in the Summoner Pact (deferred, not designed now).

| Lv | Bears | Slam dmg (placeholder) | Slam radius | Every |
|---|---|---|---|---|
| 1 | 1 | 25 | 2.5 m | 2.5 s |
| 2 | 1 | 34 | 3.0 m | 2.5 s |
| 3 | 1 | 45 | 3.5 m | 2.5 s |
| 4 | 1 | 57 | 4.0 m | 2.5 s |
| 5 | 1 | 70 | 4.6 m | 2.5 s |

**Base stats (placeholder):** life 200 (+15% per level), stun 0.4 s, move speed 0.9x the player's.

**Support notes:**
- **Legion Banner:** the Bear is fixed at 1, so Banner gives it +15% damage instead of +1 cap.
- **Bigger Area:** applies to the Slam radius.
- **Echo:** Slam repeats at half damage.

### Bone Warden notes
- Wolves and the Bear are path-neutral.
- They count toward the global cap and toward Bone Legion kills.
- Under **Necro Covenant**, beasts are allowed but share the 20 cap.
- **Sloth Throne** (x1.5 near a player who is standing still) applies to all minions.

---

## 7. Build sketches (warrior/archer only)

1. **Bone Wall:**
   - Skills: Warriors + Archers.
   - Pact: Bone Legion → Necro Covenant (archers are still allowed).
   - Supports: Legion Banner, Gravebound.
   - Up to 20 bodies; warriors hold the line while archers shoot from behind.
2. **Marksman's Escort:**
   - Skills: Archers + Fireball or Chain Lightning.
   - Supports: Multiple Proj and Pierce on the archers.
   - The player kites while the archer ring adds steady ranged damage.

---

## 8. Open questions for VH (still unanswered)

1. **Lv counts:** Lv5 (max) = 5 warriors, 4 archers, 5 wolves, 1 bear (Slam 4.6 m). OK?
2. **Covenant + projectile minions:** may Skeleton Archers keep shooting under Necro Covenant? (Recommended yes.)
3. **Global cap 12** (20 with Covenant): keep it, so a full summon build has to choose which minions to keep, or raise it?

---

## 9. Pact Paths (VH t520u, restructured t531u + t532u): NECROMANCER (Bone Warden merged in) and SUMMONER

### 9.0 The 18/8 rule (VH t531u; t535u: 18 nodes)
- **t535u update:** Necromancer has **18 nodes = 2 groups x (7 Notables + 2 Keystones)**. You still pick max 8, with max 1 Keystone. The 16/8 wording below is the original rule; the node count is now 18.
- **One Pact Path = 18 nodes (2 groups of 9: 7 Notables + 2 Keystones each). You pick at most 8 per run, 1 Keystone max.** The nodes are split into groups inside the one Path, not into separate Paths. More skill types can add more groups to the same Path later.
- Every group has its own root node (no requirement), so builds can be pure or mixed within the 8 picks. Players discover the builds.
- **Max 1 Keystone** in your 8. The 16 include 2 Keystones, one per group.
- **Unlock before select** (Souls; a Keystone also costs +1 Sigil). One Path per Pact.
- **Crest at 4+ nodes** (recommended: 4 is half of 8, so mixed builds still get it). Open question: 5+?
- Paths only: no Hybrids, no Combos. The banned name is never shown in game. Design only; numbers are placeholders.

### 9.1 Bone Warden is merged into Necromancer (VH t532u)
- **There is no separate Bone Warden Pact Path any more.** Its orbital, zone and bone ideas become Group B of the Necromancer Path.
- Bone Legion (kill-spawned skeletons) is a minion idea, so it joins **Group A as A7 OSSUARY ORDER**.
- Ossuary Ward's bullet-block is **removed** (blocking is baseline), and the node is reworked.
- Necro Covenant (Legion 12, permanent) is **dropped**. Crypt King covers the army Keystone. No Second Burial.
- Result: 4 Paths again (Pyromancer, NECROMANCER, Stormcaller, Blood Reaver). Necromancer takes Bone Warden's slot.

**Code implications (later, not this turn):**
- Max selection becomes 8 for 16-node Paths, so `pactMaxSelect51` needs a per-Path value (6 for the 10-node Paths, 8 for Necromancer).
- Remove `PactPath.BoneWarden` / the 5th-Path stub (t525u). Necromancer takes index 1, and saves move Bone Warden unlocks over — DONE in t539u (see Decisions_for_VH).
- Group B needs a **Bone skill tag** (STag). Calcify and Marrow Harvest can reuse parts of the existing Bone Warden code; the rest are new generic tag hooks. Group A needs the minion summon skills to exist first.
- The Altar97 tree shows **two groups of 9** (Summon | Bone, toggled with a tab button or Tab) and needs 18 icons (placeholders wired in t536u). The v107 Bone Warden bust can become the Necromancer portrait.

### NECROMANCER, "THE GRAVECALLER" (18 nodes, pick 8)
**Crest (4+ nodes):** undead minions +15% damage and +15% life; bone skills +15% damage.

**Naming rule (VH t522u):** no node names a specific skeleton type. The Minion tag is on all summons. No bullet-block nodes.

#### Group A: SUMMON (9 = 7 Notables + 2 Keystones)
| Id | Name | Souls | Req | Effect |
|---|---|---|---|---|
| A1 | GRAVE MUSTER | 35 | — | +1 TO EACH MINION TYPE YOU HAVE |
| A2 | IRON MARROW | 40 | A1 | UNDEAD MINIONS +40% LIFE |
| A3 | HOLLOW HASTE | 40 | A1 | MINION ATTACK SPEED +20% |
| A4 | QUICK GRAVE | 60 | A2/A3 | UNDEAD MINION RESPAWN -30% |
| A5 | GRAVE EDGE | 50 | A2 | MELEE MINIONS HIT +1 TARGET. +25% MELEE MINION DMG |
| A6 | MARROW SHOT | 60 | A3 | MINION PROJECTILES +2M RANGE. EVERY 3RD PIERCES |
| A7 | OSSUARY ORDER | 80 | A5/A6 | 20 KILLS = +1 TEMP SKELETON MINION (MAX 3, 15S) (was Bone Legion) |
| AK ◆ | CRYPT KING | 150 + 1 SIGIL | A4 | UNDEAD MINIONS +60% DMG, +40% LIFE. YOUR NON-MINION SKILLS -50% DMG |
| AK2 ◆ | LEGION OF ONE HUNDRED | 150 + 1 SIGIL | 2A | MINION CAP +4. EACH MINION -20% DMG (t535u) |

#### Group B: BONE SKILLS (9 = 7 Notables + 2 Keystones), remade for VH t533u, +1 Keystone t535u
Every node applies to **any skill with the Bone tag**: bone projectiles, bone melee, bone areas/zones, bone orbits and future bone skills. No node is orbit-only. Bone Warden's old orbit-only nodes (Marrow Launch, Ossuary Ward, Rune Ossuary, Grave Anchor) are retired as nodes.

| Id | Name | Souls | Req | Effect |
|---|---|---|---|---|
| B1 | MARROW BITE | 35 | — | BONE SKILLS +20% DMG |
| B2 | SPLINTERING | 40 | B1 | BONE HITS SHATTER: 3 SHARDS FLY OUT (25% DMG) |
| B3 | BRITTLE MARK | 40 | B1 | BONE HITS: ENEMY TAKES +10% DMG FOR 3S |
| B4 | DRY OSSUARY | 60 | B2/B3 | BONE SKILLS -20% COOLDOWN |
| B5 | GRAVE REACH | 50 | B2 | BONE SKILLS +25% AREA, PROJECTILE RANGE AND MELEE REACH |
| B6 | CALCIFY | 60 | B3 | BONE HITS CHILL: 4 STACKS = 1S STUN |
| B7 | MARROW HARVEST | 70 | B4 | 10 KILLS = +1 BONE CHARGE (MAX 4). NEXT BONE SKILL SPENDS ALL: +25% DMG EACH |
| BK ◆ | OSSUARY TYRANT | 150 + 1 SIGIL | B4 | BONE SKILLS +60% DMG, HIT TWICE (2ND HIT 40%). YOU CANNOT HAVE MINIONS |
| BK2 ◆ | GRAVE COLOSSUS | 150 + 1 SIGIL | 2B | BONE SKILLS +40% DMG, +50% AREA/REACH. BONE SKILL COOLDOWNS +40% (t535u) |

**Crest:** now reads "undead minions +15% damage and +15% life; **bone skills +15% damage**". The old orbit-only "zone casts add orbit bones" is removed.

**Dropped:** Marching Dead, Necro Covenant, Sloth Throne (orbit/zone only), Marrow Launch, Ossuary Ward, Grave Anchor, Rune Ossuary and Wall of the Dead (they can come back as a 3rd group or as swaps if VH wants).

**Example builds (8 picks):**
- Army: A1–A7 + Crypt King.
- Bone: B1–B7 + Ossuary Tyrant.
- Mixed: A1, A2, A3, A4 + B1, B2, B4, B6.

**Open questions (t532u):**
1. Crest at 4+, or 5+ with 8 picks?
2. Crest = minion buff + bone skills +15% DMG: OK? Which skills get the Bone tag (Bone Warden orbit/zone skills + future bone skills)?
3. Should Pyro/Storm/Blood also move to 16/8 later?
4. Should any dropped node (Marching Dead, Necro Covenant) come back?

### SUMMONER, "THE BEASTBINDER" (Summon Wolf + Summon Bear)
**Crest (4+ nodes):** beasts +15% damage, and the Bear +20% life.

| Id | Name | Souls | Req | Effect |
|---|---|---|---|---|
| N1 | PACK CALL | 35 | — | +1 WOLF |
| N2 | BLOODED FANGS | 40 | N1 | WOLF BITE +30% DMG. BITES BLEED 2S |
| N3 | HEAVY PAW | 40 | N1 | BEAR SLAM +30% DMG |
| N4 | WILD RETURN | 60 | N2/N3 | WOLF AND BEAR RESPAWN -30% |
| N5 | HUNTING HOWL | 50 | N2 | WOLVES +25% SPEED. FIRST BITE ON A TARGET X2 |
| N6 | QUAKE HIDE | 60 | N3 | SLAM +1M RADIUS. BEAR +30% LIFE |
| N7 | ALPHA'S TITHE | 70 | N4 | 15 BEAST KILLS = +1 TEMP WOLF (MAX 2, 15S) |
| N8 | TREMOR CALL | 80 | N5/N6 | SLAM EVERY 2S (WAS 2.5S). STUN +0.2S |
| K1 ◆ | ONE GREAT BEAST | 150 + 1 SIGIL | N6 | NO WOLVES. BEAR X2 SIZE, SLAM X2 DMG |
| K2 ◆ | THE ENDLESS PACK | 150 + 1 SIGIL | 2N | WOLF CAP +3. BEAR SLAM -30% DMG |

> **Draft, not wired (VH still reviewing).** It will move to the same 16/8 rule later (Group A Wolf/Pack + Group B Bear/Slam, or Beasts + an attack group).

**Slam hooks:** Heavy Paw, Quake Hide, Tremor Call and One Great Beast are the first Summoner Pact Slam upgrades. Any further Slam changes are deferred and will be designed together with VH.

**Open questions for these Paths:**
1. Are 2 extra Paths OK? That makes 6 in total (4 existing + 2 new). Should Bone Warden's Bone Legion stay as it is, given that the NECROMANCER Path overlaps it?
2. Keystone bans or trade-offs: OK as drafted, or should they be softer?
