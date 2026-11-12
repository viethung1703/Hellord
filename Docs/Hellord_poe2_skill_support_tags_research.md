# PoE2 / PoE1 skill and support gem tagging: research (t623u, 2026-11-01 UTC+7)

Sources:
- PoE wiki "Gem tag": https://pathofexile.fandom.com/wiki/Gem_tag
- poe2wiki Support gem: https://www.poe2wiki.net/wiki/Support_gem
- poe2wiki Feeding Frenzy I: https://www.poe2wiki.net/wiki/Feeding_Frenzy_I
- expcarry PoE2 gems guide.

The rest is from general game knowledge. Where something is not confirmed this session, it says "(unverified)".

## 1. Gem tags (what the player sees)
- Each skill gem lists keywords on its first tooltip line. Examples:
  - **Form / delivery**: Attack, Spell, Projectile, AoE, Melee, Strike, Slam, Chaining, Channelling, Nova, Grenade (PoE2), Barrage, Totem, Trap/Mine (PoE1), Movement, Travel, Warcry, Mark, Herald, Remnant (PoE2).
  - **Behaviour**: Duration, Persistent (PoE2, which reserves Spirit), Buff, Aura, Curse, Minion, Companion (PoE2), Meta / Invocation / Trigger (PoE2).
  - **Element**: Physical, Fire, Cold, Lightning, Chaos.
  - **Weapon**: Bow, Crossbow, Spear, Mace, and so on.
- The wiki states this explicitly: **visible tags categorise skills, but they are NOT the compatibility rule.** They don't decide which stats apply or which supports fit, "even if it often does line up". Tags are mainly used for things like "+1 to level of all Minion skills".

## 2. How supports actually decide compatibility
- Each support has a text line like "Supports Projectile skills" or "Supports skills which create Minions". It may also have exclusions such as "Cannot support skills which create undamageable Minions" (Feeding Frenzy) or "Cannot support Persistent skills".
- In the game data this is hidden *skill types* (ActiveSkillType):
  - a support has a list of types it **requires** (any-of, sometimes with combinations);
  - a list of types it **excludes**;
  - types it **adds** to the skill (e.g. a support that makes a skill a totem adds Totem). (unverified detail, from PoE1 data mining)
- A skill can have a hidden type without the visible tag. For example, a skill that only *sometimes* fires projectiles can still take projectile supports.
- If a support is socketed but doesn't fit, it is greyed out or disabled.

## 3. Minions
- A support on a minion skill can affect either the *summon* or the *minion's own attacks*:
  - "Supports skills which create Minions" covers minion damage, life and so on.
  - Generic supports such as Projectile ones apply to the minion's attacks when those attacks are of the right type. In PoE1, for example, Multiple Projectiles linked to a ranged minion like Skeleton Mages / Holy Relic makes the minion's projectiles split (PoE1 behaviour, partly unverified for PoE2).
- So the rule is "check the skill types of whatever deals the damage", not "check the summoning gem's visible tags only".

## 4. Other PoE2 socketing rules
- Each skill starts with 2 support sockets, up to 5 with Jeweller's Orbs.
- **You cannot socket two supports of the same Category in one skill**, e.g. Feeding Frenzy I/II share a category. Earlier PoE2 versions had a stricter "one copy of each support across all skills" rule. The current one is per-skill category plus attribute capacity, which per the expcarry guide is 5 attribute per support. (Rule history unverified.)
- Persistent-reservation supports are limited to one copy across skills.
- Meta gems (Cast on X, Invocations) socket *skills* and get supported themselves.

## 5. Takeaways for Hellord
1. Hellord already uses tags as the compatibility check. That's simpler than PoE and fine for a roguelite card UI. Keep one set of tags that serves both display and matching. Don't add PoE's hidden second layer unless needed.
2. Supports need **requires-any**, optional **requires-all**, and **excludes** (PoE's "Cannot support").
3. Minion skills should carry the tags of what the minions *do* (archers = Projectile). Then projectile supports work by the same rule, which t622u already wired.
4. A "category" (no two supports of the same family on one skill) is useful once tiers or variants exist.
