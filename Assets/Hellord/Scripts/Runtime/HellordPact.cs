// v0.0.51 damnation pact: Sacrifice Altar meta progression (spec: Docs/hellord_damnation_pact_ui_layout_v1.6.md + Hellord_ascendancy_nodes_full_v0.6.md).
// Player-facing name is "Damnation Pact" / "Path" / "Sacrifice Altar". Never show the word Ascendancy.
// Rules (v1.6 LOCKED): one Path per Pact, max 6 selected (Keystone uses 1), max 1 Keystone, node must be UNLOCKED (Souls) before SELECTED,
// Unlock and Select are separate modes, Crest at 4+ selected. Paths only (VH Oct 31): no Hybrids, Combos or prebuilt kits.
using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    public enum PactPath { Pyromancer = 0, Necromancer = 1, Stormcaller = 2, BloodReaver = 3 } // t539u: Bone Warden REMOVED as a Path; Necromancer took its index 1 (saves migrated in PactMigrate539)

    public class PactNode
    {
        public string id, name, desc, req; public bool keystone; public int cost; public bool live, partial;
    }

    public static class PactData
    {
        public static readonly string[] PathName = { "PYROMANCER OF THE PIT", "NECROMANCER THE GRAVECALLER", "STORMCALLER", "BLOOD REAVER" };
        public static readonly string[] PathShort = { "PYROMANCER", "NECROMANCER", "STORMCALLER", "BLOOD REAVER" };
        public static readonly Color[] PathCol = { new Color32(0xf7, 0x76, 0x22, 255), new Color32(0x5a, 0xc5, 0x4f, 255), new Color32(0x3c, 0x9c, 0xff, 255), new Color32(0xe4, 0x3b, 0x44, 255) };
        public static readonly string[] PathPassive = { "+INC DAMAGE PER NODE", "SUMMON + BONE SKILLS", "+MOVE SPEED PER NODE", "+MAX HP PER NODE" };
        public static readonly string[] CrestText = { "CREST: BURN SPREADS ON DEATH", "CREST: MINIONS +15% DMG/LIFE, BONE SKILLS +15% DMG", "CREST: EVERY 4TH TRIGGER FREE, SEEKS SHOCKED", "CREST: MELEE RECASTS AT LOW HP, 30% SHIELD" };
        // v0.0.53: live = the node's mechanic really runs in game (HellordPact53.cs). partial = runs, but simpler than the design text (see Decisions_for_VH).
        static PactNode N(string id, string n, int c, string r, string d, bool live = true, bool partial = false) => new PactNode { id = id, name = n, cost = c, req = r, desc = d, keystone = id.IndexOf('K') >= 0, live = live, partial = partial };
        public static readonly PactNode[][] Nodes = {
            new[] { N("N1","KINDLING",30,"","FIREBALL IMPACTS LEAVE BURNING PATCHES"), N("N2","CINDER SPLIT",40,"N1","BURNING DEATHS JUMP BURN TO 2 NEAR"), N("N3","SCORCHED TRAIL",40,"N1","DASH FIRE LINE EMPOWERS FIRE. DASH CD +20%"),
                    N("N4","OVERHEAT",60,"N2","FIRE: NO COOLDOWN. HEAT 100 = VENT + LOCK"), N("N5","METEOR PACT",60,"N3","EVERY 6TH FIREBALL IS A METEOR"), N("N6","EMBER ORBIT",50,"N2","BURN KILLS SPAWN EMBERS THAT FIRE ON HIT"),
                    N("N7","HELLFIRE LANCE",70,"N5","PILLAR IS A PIERCING BEAM. RATE -30%"), N("N8","PYRE FEAST",80,"N4/N6","BURN KILLS HEAL. CHILL ON BURN = BLIND STEAM"),
                    N("K1","ASH COVENANT",150,"2N","ALL FIRE. SMOLDER + FLASHBURN. NO FREEZE"), N("K2","INFERNO HEART",150,"N4","SELF-BURN, BURN AURA, HITS FIRE BACK. HEAL -50%") },
            // t536u: NECROMANCER / THE GRAVECALLER, 18 nodes in 2 groups (Docs/Hellord_necromancer_summoner_skills_v0.2.md sec 9). Pick max PT.necroMaxSelect536 (8), 1 Keystone.
            // idx 0..8 = Group A SUMMON (A1..A7, AK, AK2), idx 9..17 = Group B BONE SKILLS (B1..B7, BK, BK2). Generic MINION wording.
            new[] { N("A1","GRAVE MUSTER",35,"","+1 TO EACH MINION TYPE YOU HAVE"), N("A2","IRON MARROW",40,"A1","UNDEAD MINIONS +40% LIFE"), N("A3","HOLLOW HASTE",40,"A1","MINION ATTACK SPEED +20%"),
                    N("A4","QUICK GRAVE",60,"A2/A3","UNDEAD MINION RESPAWN -30%"), N("A5","GRAVE EDGE",50,"A2","MELEE MINIONS HIT +1 TARGET. +25% MELEE MINION DMG"), N("A6","MARROW SHOT",60,"A3","MINION PROJECTILES +2M RANGE. EVERY 3RD PIERCES"),
                    N("A7","OSSUARY ORDER",80,"A5/A6","20 KILLS = +1 TEMP SKELETON MINION (MAX 3, 15S)"),
                    N("AK","CRYPT KING",150,"A4","UNDEAD MINIONS +60% DMG, +40% LIFE. YOUR NON-MINION SKILLS -50% DMG"), N("AK2","LEGION OF ONE HUNDRED",150,"2A","MINION CAP +4. EACH MINION -20% DMG"),
                    N("B1","MARROW BITE",35,"","BONE SKILLS +20% DMG"), N("B2","SPLINTERING",40,"B1","BONE HITS SHATTER: 3 SHARDS FLY OUT (25% DMG)"), N("B3","BRITTLE MARK",40,"B1","BONE HITS: ENEMY TAKES +10% DMG FOR 3S"),
                    N("B4","DRY OSSUARY",60,"B2/B3","BONE SKILLS -20% COOLDOWN"), N("B5","GRAVE REACH",50,"B2","BONE SKILLS +25% AREA, PROJECTILE RANGE AND MELEE REACH",true,true), N("B6","CALCIFY",60,"B3","BONE HITS CHILL: 4 STACKS = 1S STUN"),
                    N("B7","MARROW HARVEST",70,"B4","10 KILLS = +1 BONE CHARGE (MAX 4). NEXT BONE HIT SPENDS ALL: +25% DMG EACH"),
                    N("BK","OSSUARY TYRANT",150,"B4","BONE SKILLS +60% DMG, HIT TWICE (2ND HIT 40%). YOU CANNOT HAVE MINIONS"), N("BK2","GRAVE COLOSSUS",150,"2B","BONE SKILLS +40% DMG, +50% AREA/REACH. BONE COOLDOWNS +40%",true,true) },
            new[] { N("N1","CONDUIT",35,"","CHAINS RE-EMIT FROM YOUR BONES/ZONES/RING"), N("N2","STORM CAPACITOR",40,"N1","RING STORES CHARGE. DASH RELEASES"), N("N3","CAST ON SHOCK",60,"N1","SHOCK CASTS LOWEST SKILL. 20 ENERGY"),
                    N("N4","STATIC FORK",50,"N1","FORK AND BOUNCE MERGE. -25% DMG"), N("N5","OVERCHARGE",60,"N3","ENERGY 200. >100 EMPOWERS TRIGGERS"), N("N6","LIGHTNING ROD",50,"N2","ON A ZONE: STRIKE EVERY 1S"),
                    N("N7","CAST ON KILL",70,"N3","SHOCK KILL CASTS TOP SKILL. 35 ENERGY"), N("N8","THUNDER STEP",80,"N2/N5","DASH IS A 4M BLINK THAT SHOCKS"),
                    N("K1","UNBOUND CHAINS",150,"2N","TRIGGERS FREE, NO ICD, COST 1 HP. NO REGEN"), N("K2","ELEMENTAL EQUILIBRIUM",150,"N4","OTHER ELEMENT X1.4, SAME X0.7") },
            new[] { N("N1","LEECH CONVERT",30,"","OVERHEAL = SHIELD THAT EXPLODES"), N("N2","PRICE OF WRATH",40,"N1","HOLD DASH: 2X CAST SPEED FOR HP"), N("N3","WHIP REACH",40,"N1","WHIP 360 DEG. HALF RATE"),
                    N("N4","HEMORRHAGE",60,"N3","BLEED STORES. MELEE OR 5 STACKS BURSTS"), N("N5","RED FEAST",50,"N1","MELEE KILLS DROP ORBS. NO REGEN"), N("N6","FRENZY CHAIN",60,"N2","10 MELEE COMBO = BLOOD NOVA"),
                    N("N7","SANGUINE SPEARS",70,"N4","SPEARS RETURN, HEAL, COUNT AS MELEE"), N("N8","LAST RITES",80,"N5/N6","CHEAT DEATH ONCE PER ROOM. MAX HP -10%"),
                    N("K1","MARTYR",150,"2N","HP /2. <30%: EMPOWER + LEECH. >70%: -20%"), N("K2","BLOOD MAGIC",150,"N2","HP PAYS FOR CASTS. NO CD. NO STORM RING") },
        };
    }

    public partial class HellordGame
    {
        // ---- meta save (PlayerPrefs) ----
        const string PK = "hellord.pact51.";
        int _souls51, _sigils51, _pactPath51 = -1, _pactSel51; int[] _unlock51 = new int[4]; bool _pactLoaded51;
        // ---- run state ----
        bool _pactPaid51, _lastRitesUsed51; int _pactBossKills51, _soulsEarned51, _sigilsEarned51; bool _pactActive51;
        // ---- ui state ----
        int _altarView51; bool _altarSelectMode51; int _confirmSwitch51 = -1; string _altarMsg51; int _hoverNode51 = -1; float _altarMsgT51;

        static HellordTuning PT => HellordTuning.I;
        static bool ArgPresent51(string k) => System.Array.IndexOf(System.Environment.GetCommandLineArgs(), k) >= 0;

        void PactLoad51()
        {
            if (_pactLoaded51) return; _pactLoaded51 = true;
            _souls51 = PlayerPrefs.GetInt(PK + "souls", PT.startSouls51); _sigils51 = PlayerPrefs.GetInt(PK + "sigils", 0);
            _pactPath51 = PlayerPrefs.GetInt(PK + "path", -1); _pactSel51 = PlayerPrefs.GetInt(PK + "sel", 0);
            for (int p = 0; p < 4; p++) _unlock51[p] = PlayerPrefs.GetInt(PK + "unlock" + p, 0);
            PactMigrate539();
            if (_pactPath51 < 0 || _pactPath51 > 3) { _pactPath51 = -1; _pactSel51 = 0; }
            _pactSel51 &= _pactPath51 >= 0 ? _unlock51[_pactPath51] : 0; // never select a locked node (save repair)
            _altarView51 = Mathf.Max(0, _pactPath51);
        }
        // t539u: one-time save migration. Old layout: 0 Pyro, 1 BONE WARDEN (10 nodes), 2 Storm, 3 Blood, 4 Necromancer (18 nodes). New: 1 = Necromancer.
        // Bone Warden nodes that live on in Necromancer carry over (N4 Bone Legion -> A7 Ossuary Order, N6 Calcify -> B6, N7 Marrow Harvest -> B7);
        // every other unlocked Bone Warden node is REFUNDED (its Souls, + Sigils for a Keystone). A Bone Warden Pact selection is cleared (Path stays Necromancer).
        static readonly int[] BwCost539 = { 35, 40, 40, 60, 50, 60, 70, 80, 150, 150 }; static readonly int[] BwMap539 = { -1, -1, -1, 6, -1, 14, 15, -1, -1, -1 };
        void PactMigrate539()
        {
            if (PlayerPrefs.GetInt(PK + "mig539", 0) == 1) return;
            int bw = PlayerPrefs.GetInt(PK + "unlock1", 0), nec = PlayerPrefs.GetInt(PK + "unlock4", 0), refS = 0, refG = 0, moved = 0;
            if (!PlayerPrefs.HasKey(PK + "unlock4")) nec = 0;
            bool fromBw = PlayerPrefs.HasKey(PK + "unlock4") || bw != 0; // a pre-t525u save has no unlock4: its unlock1 is Bone Warden too
            if (fromBw) for (int i = 0; i < 10; i++) if ((bw & (1 << i)) != 0) { if (BwMap539[i] >= 0) { nec |= 1 << BwMap539[i]; moved++; } else { refS += BwCost539[i]; if (i >= 8) refG += PT.pactKeystoneSigils51; } }
            _unlock51[1] = nec; _souls51 += refS; _sigils51 += refG;
            if (_pactPath51 == 1) { _pactPath51 = 1; _pactSel51 = 0; } else if (_pactPath51 == 4) _pactPath51 = 1; else if (_pactPath51 > 4) { _pactPath51 = -1; _pactSel51 = 0; }
            PlayerPrefs.DeleteKey(PK + "unlock4"); PlayerPrefs.SetInt(PK + "mig539", 1); _pactLoaded51 = true; PactSave51();
            Debug.Log("[Hellord] t539u Bone Warden -> Necromancer migration: moved " + moved + " nodes, refunded " + refS + " souls + " + refG + " sigils");
        }
        void PactSave51()
        {
            PlayerPrefs.SetInt(PK + "souls", _souls51); PlayerPrefs.SetInt(PK + "sigils", _sigils51);
            PlayerPrefs.SetInt(PK + "path", _pactPath51); PlayerPrefs.SetInt(PK + "sel", _pactSel51);
            for (int p = 0; p < 4; p++) PlayerPrefs.SetInt(PK + "unlock" + p, _unlock51[p]);
            PlayerPrefs.Save();
        }

        bool PactMenuOn51 => PT.pactMainMenu51 && !ArgPresent51("-hellordAutotest") && (!ArgPresent51("-botstate") || ArgPresent51("-botmenu")); // t508u: -botmenu keeps the menu for recorded bot runs
        void PactBoot51() { PactLoad51(); PactRunReset51(); if (PactMenuOn51) mode = Mode.MainMenu; }
        void PactAfterReset51() { PactRunReset51(); if (PactMenuOn51) mode = Mode.MainMenu; else PactApply51(); }
        void PactRunReset51() { SinRunReset737(); PactReset53(); _pactPaid51 = false; _lastRitesUsed51 = false; _pactBossKills51 = 0; _soulsEarned51 = 0; _sigilsEarned51 = 0; _pactActive51 = false; }

        // ---- queries ----
        static int Bit(int i) => 1 << i;
        int SelCount51() { int c = 0; for (int i = 0; i < 31; i++) if ((_pactSel51 & Bit(i)) != 0) c++; return c; }
        bool Has51(PactPath p, int i) => _pactActive51 && _pactPath51 == (int)p && (_pactSel51 & Bit(i)) != 0;
        bool Unlocked51(int p, int i) => (_unlock51[p] & Bit(i)) != 0;
        int MaxKeys51() => _pactPath51 >= 0 && PactData.Nodes[_pactPath51].Length > 10 ? PT.necroMaxKeystones549 : PT.pactMaxKeystones51; // t549u: Necromancer 2
        string KeyMaxLab51 => MaxKeys51() == 1 ? "MAX 1 KEYSTONE" : "MAX " + MaxKeys51() + " KEYSTONES";
        int NodeCost51(PactNode n) => Mathf.RoundToInt(n.cost * PT.pactCostMul51);
        bool ReqMet51(int p, PactNode n)
        {
            if (string.IsNullOrEmpty(n.req)) return true;
            if (n.req.Length == 2 && n.req[0] == '2') { int c = 0; var ns = PactData.Nodes[p]; for (int i = 0; i < ns.Length; i++) if (!ns[i].keystone && (n.req[1] == 'N' || ns[i].id[0] == n.req[1]) && Unlocked51(p, i)) c++; return c >= 2; } // "2N" any 2 Notables; t536u "2A"/"2B" = 2 Notables of that group
            foreach (var r in n.req.Split('/')) { int ix = NodeIx51(p, r); if (ix >= 0 && Unlocked51(p, ix)) return true; }
            return false;
        }
        static int NodeIx51(int p, string id) { var ns = PactData.Nodes[p]; for (int i = 0; i < ns.Length; i++) if (ns[i].id == id) return i; return -1; } // t536u: req ids by name (A2, B4, N3...)
        static bool AnyReq51(string r) => r != null && r.Length == 2 && r[0] == '2';
        int MaxSel51(int p = -9) { if (p == -9) p = _pactPath51; return p >= 0 && PactData.Nodes[p].Length > 10 ? PT.necroMaxSelect536 : PT.pactMaxSelect51; } // t536u: 18-node Paths pick 8
        string ReqText51(PactNode n) => string.IsNullOrEmpty(n.req) ? "" : AnyReq51(n.req) ? (n.req[1] == 'N' ? "NEEDS 2 NOTABLES UNLOCKED" : "NEEDS 2 GROUP " + n.req[1] + " NOTABLES UNLOCKED") : "NEEDS " + n.req.Replace("/", " OR ") + " UNLOCKED";

        // ---- actions ----
        void TryUnlock51(int p, int i)
        {
            var n = PactData.Nodes[p][i];
            if (Unlocked51(p, i)) { Msg51("ALREADY UNLOCKED"); return; }
            if (!ReqMet51(p, n)) { Msg51(ReqText51(n)); return; }
            int c = NodeCost51(n), sg = n.keystone ? PT.pactKeystoneSigils51 : 0;
            if (_souls51 < c || _sigils51 < sg) { Msg51("NOT ENOUGH SOULS" + (sg > 0 ? " OR SIGILS" : "")); return; }
            _souls51 -= c; _sigils51 -= sg; _unlock51[p] |= Bit(i); PactSave51(); Msg51("UNLOCKED " + n.name);
        }
        void TrySelect51(int p, int i)
        {
            var n = PactData.Nodes[p][i];
            if (!Unlocked51(p, i)) { Msg51("UNLOCK FIRST"); return; }
            if (_pactPath51 >= 0 && _pactPath51 != p && SelCount51() > 0) { _confirmSwitch51 = p; return; } // v1.6: switching Path asks + clears
            if (_pactPath51 != p) { _pactPath51 = p; _pactSel51 = 0; }
            if ((_pactSel51 & Bit(i)) != 0) { _pactSel51 &= ~Bit(i); if (_pactSel51 == 0) _pactPath51 = -1; PactSave51(); return; }
            if (SelCount51() >= MaxSel51()) { Msg51("PACT FULL: " + MaxSel51() + " MAX"); return; }
            if (n.keystone) { int k = KeyCount53(); if (k >= MaxKeys51()) { Msg51(KeyMaxLab51); return; } }
            _pactSel51 |= Bit(i); PactSave51();
        }
        int KeyCount53() { int k = 0; if (_pactPath51 < 0) return 0; var ns = PactData.Nodes[_pactPath51]; for (int j = 0; j < ns.Length; j++) if (ns[j].keystone && (_pactSel51 & Bit(j)) != 0) k++; return k; } // t536u: keystones by flag (any index)
        void Msg51(string s) { _altarMsg51 = s; _altarMsgT51 = 2.5f; if (Denied611(s)) HellordAudio.Play("ui_denied"); } // v0.0.61 t611u

        // ---- run hooks ----
        /// <summary>Called when the player Descends: applies the selected Pact to the fresh run stats.</summary>
        void PactApply51()
        {
            SinRecalc737(); // t770u Infernal Sins (before the no-pact early return)
            PactLoad51(); _pactActive51 = _pactPath51 >= 0 && _pactSel51 != 0; if (!_pactActive51) return;
            int n = SelCount51(); bool crest = n >= PT.pactCrestAt51; var p = (PactPath)_pactPath51;
            switch (p)
            {
                case PactPath.Pyromancer: incDmg += n * PT.pyroIncDmgPerNode51 + (crest ? PT.crestPyroIncDmg51 : 0); break;
                // t539u: Bone Warden armor-per-node removed with the Path (Necromancer has no per-node stat)
                case PactPath.Stormcaller: speedMul += n * PT.stormSpeedPerNode51; if (crest) crit += PT.crestStormCrit51; break;
                case PactPath.BloodReaver: maxHp += n * PT.bloodHpPerNode51 + (crest ? PT.crestBloodHp51 : 0); hp = maxHp; break;
            }
            PactApply53(); // v0.0.53: node mechanics (HellordPact53.cs)
        }
        float PactSkillMul51(SkillInst s) => _pactActive51 ? PactDmgMul53(s) : 1f; // v0.0.53: no more flat x1.25 "signature" proxy
        void PactOnKill51(Enemy e, SkillInst s) { if (mode == Mode.Play) PactSkillKill53(e, s); }
        bool PactCheatDeath51()
        {   // v0.0.53 BR-N8 Last Rites: once per ROOM, 1 HP, Empower all skills, short invulnerability
            if (!Has51(PactPath.BloodReaver, 7) || _lastRitesUsed51) return false; _lastRitesUsed51 = true;
            hp = 1; _empT53 = Mathf.Max(_empT53, PT.lastRitesEmpS53); _iframe = Mathf.Max(_iframe, PT.lastRitesInvulnS53); Toast("LAST RITES"); PixelFXPack.HellFX.HellfireNova(HeroCenter, 30); return true;
        }

        void PactTick51()
        {
            if (_altarMsgT51 > 0) _altarMsgT51 -= Time.unscaledDeltaTime;
            if (mode == Mode.Dead || mode == Mode.Won)
            {   // v0.0.53 B14: recomputed while the end screen is up, so late pickups/kills never leave the payout off by one vs the shown stats
                bool won = mode == Mode.Won;
                int se = Mathf.RoundToInt(bones * PT.soulsPerBone51 + kills * PT.soulsPerKill51) + FloorIdx * PT.soulsPerFloor51 + (won ? PT.soulsWinBonus51 + PT.soulsPerFloor51 : 0);
                int ge = (won ? PT.sigilsPerWin51 : 0) + _pactBossKills51 * PT.sigilsPerBoss51;
                if (!_pactPaid51) { _pactPaid51 = true; PactLoad51(); _soulsEarned51 = 0; _sigilsEarned51 = 0; }
                if (se != _soulsEarned51 || ge != _sigilsEarned51) { _souls51 += se - _soulsEarned51; _sigils51 += ge - _sigilsEarned51; _soulsEarned51 = se; _sigilsEarned51 = ge; PactSave51(); }
            }
            if (_tmOpen) { } // pt480u: F1 test menu open on the menu: no descend keys
            else if (mode == Mode.MainMenu && PT.mainMenu69) MenuTick69(); // t503u
            else if (mode == Mode.MainMenu) { if (HInput.Down(HKey.Space)) Descend51(); }
            else if (mode == Mode.Altar && Altar97On && _altarScr53 != 2) AltarTick97(); // t511 (t1179: default again; -pactline = v0.0.53 line altar)
            else if (mode == Mode.Altar && _altarScr53 == 2 && HInput.Down(HKey.Space)) Descend51(); // v0.0.53 seal & descend
            else if (mode == Mode.Altar) { if (HInput.Down(HKey.Escape)) AltarBack53(); if (HInput.Down(HKey.Tab)) _altarSelectMode51 = !_altarSelectMode51; }
        }
        string PactEndLine51() => _pactPaid51 ? "+" + _soulsEarned51 + " SOULS" + (_sigilsEarned51 > 0 ? "  +" + _sigilsEarned51 + " SIGIL" : "") + "  TOTAL " + _souls51 : null;
        void Descend51() { HellordAudio.Play("pact_seal_descend"); mode = Mode.StartPick; PactApply51(); } // v0.0.61 t611u sting; menu music fades out

        // ---- IMGUI (640x360 canvas, placeholder art per step60/v1.6) ----
        static readonly Color CPanel51 = new Color(0.07f, 0.05f, 0.1f, 0.92f), CEdge51 = new Color(0.35f, 0.12f, 0.12f, 1f);
        void Rect51(float x, float y, float w, float h, Color c) { GUI.color = c; GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture); GUI.color = Color.white; }
        void Frame51(float x, float y, float w, float h, Color fill, Color edge) { Rect51(x, y, w, h, edge); Rect51(x + 1, y + 1, w - 2, h - 2, fill); }
        bool Hit51(float x, float y, float w, float h) {
#if HELLORD_AUTOTEST
            if(Event.current.type==EventType.Repaint&&_menuFake91.HasValue&&new Rect(x,y,w,h).Contains(_menuFake91.Value)){_menuFake91=null;return true;}
#endif
            var e = Event.current; return e.type == EventType.MouseDown && e.button == 0 && new Rect(x, y, w, h).Contains(e.mousePosition); }
        bool Over51(float x, float y, float w, float h) => new Rect(x, y, w, h).Contains(Event.current.mousePosition);
        bool Btn51(float x, float y, float w, float h, string label, Color c, bool on = true)
        {
            bool ov = on && Over51(x, y, w, h); Frame51(x, y, w, h, ov ? new Color(0.22f, 0.07f, 0.08f, 1) : CPanel51, ov ? c : CEdge51);
            string s = Sanitize(label); Text(s, Mathf.Round(x + (w - s.Length * 6 + 1) / 2f), Mathf.Round(y + (h - 7) / 2f), on ? c : CDim);
            if (on && Hit51(x, y, w, h)) { Event.current.Use(); return true; } return false;
        }
        // ---- v0.0.53 UI FIX: main menu + Sacrifice Altar drawn to the APPROVED mockups (step57 main menu, step60 pact panels, v1.6 copy).
        // Layout is authored in mockup space (480x270) and scaled x4/3 onto the 640x360 canvas; art baked by tools/pact_ui_v53.py into Resources/HellordArt/PactUI.
        const float K53 = 640f / 480f;
        Matrix4x4 _base53; int _altarScr53, _focus53; readonly Dictionary<string, Texture2D> _tex53 = new Dictionary<string, Texture2D>();
        static readonly Color U_INK = new Color32(0x18, 0x14, 0x25, 255), U_PLUM = new Color32(0x3e, 0x27, 0x31, 255), U_BROWN = new Color32(0x73, 0x3e, 0x39, 255), U_DRED = new Color32(0xa2, 0x26, 0x33, 255),
            U_AMB = new Color32(0xfe, 0xae, 0x34, 255), U_YEL = new Color32(0xfe, 0xe7, 0x61, 255), U_SAND = new Color32(0xea, 0xd4, 0xaa, 255), U_GREY = new Color32(0x8b, 0x9b, 0xb4, 255),
            U_SLATE = new Color32(0x5a, 0x69, 0x88, 255), U_PANEL = new Color32(28, 20, 34, 255), U_LOCK = new Color32(50, 46, 60, 255);
        Texture2D T53(string n, bool smooth = false)
        {
            if (_tex53.TryGetValue(n, out var t)) return t;
            t = Resources.Load<Texture2D>("HellordArt/PactUI/pactui_" + n); if (t) { t.filterMode = smooth ? FilterMode.Bilinear : FilterMode.Point; t.wrapMode = TextureWrapMode.Clamp; }
            _tex53[n] = t; return t;
        }
        void Img53(string n, float x, float y, float w, float h, bool smooth = false) { var t = T53(n, smooth); if (t) GUI.DrawTexture(new Rect(x, y, w, h), t); }
        void R53(float x, float y, float w, float h, Color c) => Rect51(x, y, w, h, c);
        void Ol53(float x, float y, float w, float h, Color c, float t = 1) { R53(x, y, w, t, c); R53(x, y + h - t, w, t, c); R53(x, y, t, h, c); R53(x + w - t, y, t, h, c); }
        static float TW53(string s, int sc = 1) => (Sanitize(s).Length * 6 * sc - sc) / K53;
        void Tx53(string s, float x, float y, Color c, int sc = 1) { var m = GUI.matrix; GUI.matrix = _base53; Text(Sanitize(s), Mathf.Round(x * K53), Mathf.Round(y * K53), c, true, sc); GUI.matrix = m; }
        void TxC53(string s, float cx, float y, Color c, int sc = 1) => Tx53(s, cx - TW53(s, sc) / 2, y, c, sc);
        string Fit53(string s, float w) { s = Sanitize(s); int n = Mathf.FloorToInt((w * K53 + 1) / 6); return s.Length > n ? s.Substring(0, Mathf.Max(0, n - 1)) + "." : s; }
        bool _block53;
        bool Click53(float x, float y, float w, float h) { if (_block53 || !Hit51(x, y, w, h)) return false; if(Event.current.type==EventType.MouseDown)Event.current.Use(); return true; }
        void Icon51(int p, float x, float y, float s, bool dim = false) => Img53("icon_" + p + (dim ? "_dim" : ""), x, y, s, s);
        bool Active51 => _pactPath51 >= 0 && _pactSel51 != 0;
        void OpenAltar53() { mode = Mode.Altar; _altarScr53 = 0; _focus97 = -1; _confirm97 = -1; _altarView51 = Mathf.Max(0, _pactPath51); _confirmSwitch51 = -1; }
        void AltarBack53() { if (_confirmSwitch51 >= 0) _confirmSwitch51 = -1; else if (_altarScr53 > 0) _altarScr53 = 0; else mode = Mode.MainMenu; }

        void DrawPact51()
        {
            PactLoad51(); DimScreen(U_INK);
            _base53 = GUI.matrix; GUI.matrix = _base53 * Matrix4x4.Scale(new Vector3(K53, K53, 1));
            if (mode == Mode.MainMenu && PT.mainMenu69) { GUI.matrix = _base53 * Matrix4x4.Scale(new Vector3(K69, K69, 1)); DrawMainMenu69(); } else if (mode == Mode.MainMenu) DrawMainMenu51(); else if (Altar97On && _altarScr53 != 2) { GUI.matrix = _base53 * Matrix4x4.Scale(new Vector3(K69, K69, 1)); DrawAltar97(); } else if(_altarScr53==2&&!LineUI1176){GUI.matrix=_base53*Matrix4x4.Scale(new Vector3(K69,K69,1));DrawYourPact91();}else DrawAltar51(); // t511 altar art // t503u v69 menu
            GUI.matrix = _base53; GUI.color = Color.white;
        }

        void Currency53(float x0, float y0)
        {
            float x = x0 + 4; Img53("soul", x, y0 + 2, 12, 12); x += 14; Tx53(_souls51.ToString(), x, y0 + 5, U_AMB); x += TW53(_souls51.ToString()) + 3; Tx53("SOULS", x, y0 + 5, U_GREY); x += TW53("SOULS") + 8;
            Img53("sigil", x, y0 + 2, 12, 12); x += 14; Tx53(_sigils51.ToString(), x, y0 + 5, new Color32(181, 80, 136, 255)); x += TW53(_sigils51.ToString()) + 3; Tx53("SIGILS", x, y0 + 5, U_GREY);
        }
        void Toast53(float cy) { if (_altarMsgT51 > 0 && !string.IsNullOrEmpty(_altarMsg51)) { float w = TW53(_altarMsg51) + 10; R53(240 - w / 2, cy - 3, w, 11, U_INK); Ol53(240 - w / 2, cy - 3, w, 11, U_DRED); TxC53(_altarMsg51, 240, cy, U_YEL); } }

        void DrawMainMenu51()
        {
            Img53("menu_bg", 0, 0, 480, 270, true);
            Currency53(318, 6);
            // DESCEND (big red hex, skull + arrow) then SACRIFICE ALTAR / CODEX / SETTINGS / QUIT
            Img53("btn_descend", 66, 82, 174, 26);
            if (Over51(66, 82, 174, 26)) { Img53("skull", 50, 89, 12, 12); Img53("arrow", 244, 88, 12, 14); }
            if (Click53(66, 82, 174, 26)) { Descend51(); return; }
            string[] b = { "sacrifice", "codex", "settings", "quit" }; float y = 114;
            for (int i = 0; i < 4; i++, y += 20)
            {
                bool ov = Over51(84, y, 138, 18); Img53("btn_" + b[i] + (ov ? "_on" : ""), 84, y, 138, 18);
                if (ov) { Img53("skull", 68, y + 3, 12, 12); Img53("arrow", 226, y + 2, 12, 14); }
                if (Click53(84, y, 138, 18))
                {
                    if (i == 0) { OpenAltar53(); return; }
                    if (i == 1) Msg51("CODEX: COMING SOON"); else if (i == 2) Msg51("SETTINGS: COMING SOON"); else Application.Quit();
                }
            }
            // pact ring (PACT n/6) + active Path
            int n = Active51 ? Mathf.Min(6, SelCount51()) : 0; Img53("ring_" + n, 330, 78, 124, 124); // v0.0.53 B11: ring PNGs now have an opaque inner disc (tools/pact_ui_v53.py)
            if (Active51) { TxC53(PactData.PathShort[_pactPath51], 392, 206, PactData.PathCol[_pactPath51]); TxC53(SelCount51() >= PT.pactCrestAt51 ? "CREST ACTIVE" : "CREST AT " + PT.pactCrestAt51 + "+", 392, 215, SelCount51() >= PT.pactCrestAt51 ? U_AMB : U_GREY); }
            else TxC53("NO PACT SEALED", 392, 206, U_GREY);
            Tx53("SPACE: DESCEND  /  CLICK: SACRIFICE ALTAR", 10, 260, U_SAND); // v0.0.53 B01: Sanitize eats double spaces, use separators
            Toast53(236);
        }

        void Panel53(float x, float y, float w, float h, string title, string crumb, int icon, bool tabs, int selPath = -2)
        {
            R53(x - 2, y - 2, w + 4, h + 4, U_DRED); R53(x, y, w, h, U_PANEL); Ol53(x, y, w, h, U_BROWN);
            R53(x - 3, y - 3, 7, 7, U_AMB); R53(x + w - 4, y - 3, 7, 7, U_AMB); R53(x - 3, y + h - 4, 7, 7, U_AMB); R53(x + w - 4, y + h - 4, 7, 7, U_AMB);
            Tx53(crumb, x, y - 12, U_GREY);
            R53(x + 2, y + 2, w - 4, 20, U_PLUM);
            float tx = x + 6; if (icon >= 0) { Icon51(icon, x + 4, y + 1, 20); tx = x + 28; }
            float room = (tabs ? w - 160 : w - 58) - (tx - x); int sc = TW53(title, 2) <= room ? 2 : 1;
            Tx53(title, tx, y + 12 - 3.5f * sc / K53, U_SAND, sc);
            if (tabs)
                for (int i = 0; i < 2; i++)
                {
                    float bx = x + w - 158 + i * 48, by = y + 6; bool on = _altarSelectMode51 == (i == 1);
                    R53(bx, by, 46, 12, on ? U_AMB : U_INK); Ol53(bx, by, 46, 12, on ? U_AMB : U_SLATE); TxC53(i == 0 ? "UNLOCK" : "SELECT", bx + 23, by + 3.5f, on ? U_INK : U_GREY);
                    if (Click53(bx, by, 46, 12)) _altarSelectMode51 = i == 1;
                }
            float sx = x + w - 54; R53(sx, y - 8, 60, 30, U_INK); Ol53(sx, y - 8, 60, 30, U_AMB, 2);
            TxC53("SELECTED", sx + 30, y - 4, U_AMB); TxC53((selPath == -2 || selPath == _pactPath51 ? SelCount51() : 0) + "/" + (selPath >= 0 ? MaxSel51(selPath) : MaxSel51()), sx + 30, y + 5, U_YEL, 2); // t1178: viewed Path's max (Necromancer 8)
        }
        void Hint53(string s) { R53(0, 256, 480, 14, U_INK); TxC53(s, 240, 260, U_SAND); }
        bool SmallBtn53(float x, float y, float w, string s)
        {
            bool ov = Over51(x, y, w, 14); R53(x, y, w, 14, ov ? new Color32(92, 40, 48, 255) : U_PLUM); Ol53(x, y, w, 14, ov ? U_AMB : U_BROWN); TxC53(s, x + w / 2, y + 4, ov ? U_YEL : U_SAND);
            return Click53(x, y, w, 14);
        }

        void DrawAltar51()
        {
            Img53("altar_bg", 0, 0, 480, 270, true);
            R53(318, 6, 156, 16, U_INK); Ol53(318, 6, 156, 16, U_BROWN); Currency53(318, 6);
            bool modal = _confirmSwitch51 >= 0;
            _block53 = modal;
            if (_altarScr53 == 1) DrawPathScreen53(); else if (_altarScr53 == 2) DrawYourPact53(); else DrawOverview53();
            _block53 = false;
            Toast53(244);
            if (modal)
            {
                R53(0, 0, 480, 270, new Color(0, 0, 0, 0.6f)); float x = 140, y = 100, w = 200, h = 66;
                R53(x - 2, y - 2, w + 4, h + 4, U_DRED); R53(x, y, w, h, U_PANEL); Ol53(x, y, w, h, U_AMB);
                TxC53("SWITCH PACT TO " + PactData.PathShort[_confirmSwitch51] + "?", 240, y + 10, U_SAND); TxC53("CLEARS " + SelCount51() + " NODES.", 240, y + 22, U_GREY);
                if (SmallBtn53(x + 20, y + 42, 70, "YES")) { int np = _confirmSwitch51; _pactPath51 = np; _pactSel51 = 0; PactSave51(); _confirmSwitch51 = -1; if (_altarView51 == np) TrySelect51(np, _focus53); } // v0.0.53 B05: add the clicked node
                else if (SmallBtn53(x + 110, y + 42, 70, "NO")) _confirmSwitch51 = -1;
            }
        }

        void DrawOverview53()
        {
            float x = 136, y = 60, w = 208, h = 44 + 2 * 44;
            Panel53(x, y, w, h, "DAMNATION PACT", "MAIN MENU / SACRIFICE ALTAR", -1, false);
            for (int p = 0; p < 4; p++)
            {
                float cx = x + 6 + (p % 2) * 100, cy = y + 26 + (p / 2) * 44; var pn = PactData.Nodes[p]; int own = 0, nn = 0, anyU = 0; for (int i = 0; i < pn.Length; i++) { if (Unlocked51(p, i)) anyU++; if (!pn[i].keystone) { nn++; if (Unlocked51(p, i)) own++; } } // t1178: any node count (Necromancer 14+4)
                bool act = _pactPath51 == p && _pactSel51 != 0, locked = anyU == 0, ov = Over51(cx, cy, 96, 40);
                R53(cx, cy, 96, 40, U_INK); Ol53(cx, cy, 96, 40, act || ov ? Color.white : locked ? U_SLATE : PactData.PathCol[p], act || ov ? 2 : 1);
                Icon51(p, cx + 4, cy + 4, 24, locked);
                string nm = PactData.PathName[p]; int sp = nm.IndexOf(' '); string l1 = sp > 0 ? nm.Substring(0, sp) : nm, l2 = sp > 0 ? nm.Substring(sp + 1) : ""; if (l2.Length > 12 && l2.StartsWith("THE ")) l2 = l2.Substring(4); // t1178: GRAVECALLER fits the card
                Tx53(l1, cx + 32, cy + 6, locked ? U_GREY : U_SAND); Tx53(l2, cx + 32, cy + 15, locked ? U_SLATE : U_GREY);
                { float ps = nn > 8 ? 3 : 5, pw = nn > 8 ? 2 : 3; int k2 = 0; for (int k = 0; k < pn.Length; k++) { if (pn[k].keystone) continue; R53(cx + 4 + k2 * ps, cy + 32, pw, 4, act && (_pactSel51 & Bit(k)) != 0 ? U_YEL : Unlocked51(p, k) ? PactData.PathCol[p] : new Color32(60, 52, 70, 255)); k2++; } }
                if (act) { R53(cx + 48, cy + 29, 45, 9, U_YEL); TxC53(SelCount51() + " IN PACT", cx + 70.5f, cy + 31, U_INK); }
                else Tx53(locked ? "LOCKED" : own + "/" + nn + " OWNED", cx + 48, cy + 31, U_GREY);
                if (Click53(cx, cy, 96, 40)) { _altarView51 = p; _altarScr53 = 1; _focus53 = 0; }
            }
            if (SmallBtn53(x, y + h + 8, 60, "BACK")) mode = Mode.MainMenu;
            if (SmallBtn53(x + w - 80, y + h + 8, 80, "YOUR PACT")) _altarScr53 = 2;
            Hint53("ONE PATH PER PACT. UP TO " + MaxSel51() + " UNLOCKED NODES");
        }

        void DrawPathScreen53()
        {
            float x = 104, y = 52, w = 272, h = 170; int p = _altarView51; var nodes = PactData.Nodes[p]; Color pc = PactData.PathCol[p];
            Panel53(x, y, w, h, PactData.PathShort[p], "SACRIFICE ALTAR / DAMNATION PACT / " + PactData.PathShort[p], p, true, p); // v0.0.53 B07
            _focus53 = Mathf.Clamp(_focus53, 0, nodes.Length - 1); bool big = nodes.Length > 10; int nk = 0, nnot = 0; // t1178: 18-node Necromancer gets a compact grid in the same style
            for (int i = 0; i < nodes.Length; i++)
            {
                bool key = nodes[i].keystone; float tx, ty;
                if (!big) { tx = key ? x + 150 + (i - 8) * 32 : x + 10 + (i % 4) * 32; ty = key ? y + 44 : y + 28 + (i / 4) * 40; }
                else if (key) { tx = x + 150 + (nk % 2) * 32; ty = y + 40 + (nk / 2) * 30; nk++; }
                else { tx = x + 8 + (nnot % 7) * 19; ty = y + 30 + (nnot / 7) * 36; nnot++; }
                if (big && !key) { DrawNodeSmall53(p, i, tx, ty, pc); continue; }
                bool un = Unlocked51(p, i), sel = _pactPath51 == p && (_pactSel51 & Bit(i)) != 0;
                if (key) Img53("key_" + (sel ? "sel" : un ? "own" : "lock"), tx, ty, 23, 23);
                else { R53(tx, ty, 22, 22, sel ? U_YEL : un ? U_INK : U_LOCK); Ol53(tx, ty, 22, 22, un || sel ? pc : U_SLATE, 2); }
                if (!key) Icon51(p, tx + 3, ty + 3, 16, !un); // v0.0.53 B02: path icon on every node (dim when locked)
                if (!un) Img53("lock", tx + 8, ty + 8, 14, 14); else if (sel) Img53("check", tx + 10, ty + 10, 12, 12);
                TxC53(nodes[i].id, tx + 11, ty + 25, un ? U_SAND : U_GREY);
                if (_focus53 == i) Ol53(tx - 2, ty - 2, 26, 26, Color.white);
                if (Click53(tx - 2, ty - 2, 26, 26)) _focus53 = i;
            }
            R53(x + 140, y + 28, 1, 70, U_BROWN); if (!big) TxC53("KEYSTONE: 1 MAX", x + 197, y + 32, U_AMB); else TxC53("KEYSTONES: " + PT.necroMaxKeystones549 + " MAX", x + 197, y + 30, U_AMB);
            // detail strip
            var n = nodes[_focus53]; float dy = y + 104, dw = w - 12; bool fu = Unlocked51(p, _focus53), fs = _pactPath51 == p && (_pactSel51 & Bit(_focus53)) != 0;
            R53(x + 6, dy, dw, 48, U_INK); Ol53(x + 6, dy, dw, 48, pc);
            Tx53(Fit53(n.id + " " + n.name, dw - 104), x + 12, dy + 5, U_SAND);
            Tx53(Fit53(n.desc, dw - 104), x + 12, dy + 15, U_GREY);
            string req = n.req == "" ? "" : ReqText51(n); bool reqOk = ReqMet51(p, n);
            Tx53(Fit53(fu ? (n.live ? (n.partial ? "LIVE: PARTIAL" : "LIVE") : "NOT IN GAME YET") : req != "" ? req : "NO REQUIREMENT", dw - 104), x + 12, dy + 25, fu ? (n.live ? HCyan : U_GREY) : reqOk ? U_GREY : new Color32(0xe4, 0x3b, 0x44, 255));
            int cost = NodeCost51(n); Tx53(fu ? "OWNED" : cost + " SOULS" + (n.keystone ? " + " + PT.pactKeystoneSigils51 + " SIGIL" : ""), x + 12, dy + 35, fu ? pc : U_AMB);
            string lab; bool en;
            if (!_altarSelectMode51) { lab = fu ? "OWNED" : !reqOk ? "LOCKED" : "UNLOCK"; en = !fu && reqOk; }
            else if (!fu) { lab = "UNLOCK FIRST"; en = false; }
            else if (fs && p == (int)PactPath.Necromancer && RemoveBlock130(p, _focus53) >= 0) { lab = "CHILD FIRST"; en = false; } // t580u rule kept (t1178)
            else if (fs) { lab = "REMOVE FROM PACT"; en = true; }
            else if (p == (int)PactPath.Necromancer && !SignReq130(p, n)) { lab = "PARENT FIRST"; en = false; } // t580u rule kept (t1178)
            else if (SelCount51() >= MaxSel51() && _pactPath51 == p) { lab = "PACT FULL"; en = false; }
            else if (n.keystone && _pactPath51 == p && KeyCount53() >= MaxKeys51()) { lab = KeyMaxLab51; en = false; } // v0.0.53 B06
            else { lab = "ADD TO PACT"; en = true; }
            float bx = x + w - 96, by = dy + 24; bool bov = en && Over51(bx, by, 84, 18);
            R53(bx, by, 84, 18, en ? (bov ? U_YEL : U_AMB) : U_SLATE); Ol53(bx, by, 84, 18, en ? U_INK : U_GREY); TxC53(lab, bx + 42, by + 6, en ? U_INK : U_GREY);
            if (Click53(bx, by, 84, 18))
            {
                if (!en && lab == "CHILD FIRST") Msg51("REMOVE " + nodes[RemoveBlock130(p, _focus53)].name + " FIRST");
                else if (!en && lab == "PARENT FIRST") Msg51("SIGN PARENT FIRST: " + n.req);
                else if (!en) Msg51(lab == "PACT FULL" ? "PACT FULL " + MaxSel51() + "/" + MaxSel51() + ": REMOVE ONE TO SWAP" : lab == KeyMaxLab51 ? KeyMaxLab51 + ": REMOVE A KEYSTONE TO SWAP" : lab == "UNLOCK FIRST" ? "UNLOCK FIRST" : lab == "LOCKED" ? req : "ALREADY OWNED");
                else if (_altarSelectMode51) TrySelect51(p, _focus53); else TryUnlock51(p, _focus53);
            }
            Tx53("DETAILS", x + w - 96, dy + 6, U_GREY);
            if (SmallBtn53(x, y + h + 8, 60, "BACK")) _altarScr53 = 0;
            if (SmallBtn53(x + w - 80, y + h + 8, 80, "YOUR PACT")) _altarScr53 = 2;
            Hint53(_altarSelectMode51 ? "ADD UNLOCKED NODES (" + MaxSel51(p) + " MAX)  /  TAB: UNLOCK MODE" : "UNLOCK: SPEND SOULS TO OWN NODES FOREVER  /  TAB: SELECT MODE");
        }

        void DrawNodeSmall53(int p, int i, float tx, float ty, Color pc)
        {   // t1178: compact node (16 px) for 18-node Paths, same colours/lines as the 22 px v0.0.53 node
            bool un = Unlocked51(p, i), sel = _pactPath51 == p && (_pactSel51 & Bit(i)) != 0;
            R53(tx, ty, 16, 16, sel ? U_YEL : un ? U_INK : U_LOCK); Ol53(tx, ty, 16, 16, un || sel ? pc : U_SLATE, 2);
            Icon51(p, tx + 2, ty + 2, 12, !un);
            if (!un) Img53("lock", tx + 8, ty + 8, 9, 9); else if (sel) Img53("check", tx + 8, ty + 8, 9, 9);
            TxC53(PactData.Nodes[p][i].id, tx + 8, ty + 19, un ? U_SAND : U_GREY);
            if (_focus53 == i) Ol53(tx - 2, ty - 2, 20, 20, Color.white);
            if (Click53(tx - 2, ty - 2, 20, 20)) _focus53 = i;
        }

        void DrawYourPact53()
        {
            float x = 104, y = 52, w = 272, h = 170; int pp = _pactPath51;
            Panel53(x, y, w, h, "YOUR DAMNATION PACT", "MAIN MENU / SACRIFICE ALTAR / YOUR PACT", -1, false);
            int slot = 0;
            int ms = MaxSel51(); float sp = ms > 6 ? 31 : 43; // t1178: Necromancer picks 8, same slot style
            if (pp >= 0) for (int i = 0; i < PactData.Nodes[pp].Length && slot < ms; i++) if ((_pactSel51 & Bit(i)) != 0)
                    {
                        float sx = x + 12 + slot * sp, sy = y + 28; var nd = PactData.Nodes[pp][i];
                        if (ms > 6) { if (nd.keystone) Img53("key_pact", sx - 1, sy - 1, 30, 30); else { R53(sx, sy, 28, 28, U_INK); Ol53(sx, sy, 28, 28, PactData.PathCol[pp], 2); } Icon51(pp, sx + 6, sy + 6, 16); TxC53(nd.id, sx + 14, sy + 32, U_SAND); Tx53(Fit53(nd.id + " " + nd.name, 84), x + 12 + (slot % 3) * 88, y + 142 + (slot / 3) * 9, U_GREY); slot++; continue; }
                        if (nd.keystone) Img53("key_pact", sx - 1, sy - 1, 34, 34); else { R53(sx, sy, 32, 32, U_INK); Ol53(sx, sy, 32, 32, PactData.PathCol[pp], 2); }
                        Icon51(pp, sx + 6, sy + 6, 20); TxC53(nd.id, sx + 16, sy + 36, U_SAND); /* v0.0.53 B03: full names listed below */ Tx53(Fit53(nd.id + " " + nd.name, 128), x + 12 + (slot % 2) * 132, y + 142 + (slot / 2) * 9, U_GREY); slot++;
                    }
            for (; slot < ms; slot++) { float sx = x + 12 + slot * sp, sy = y + 28, sz = ms > 6 ? 28 : 32; R53(sx, sy, sz, sz, U_LOCK); Ol53(sx, sy, sz, sz, U_SLATE); TxC53(ms > 6 ? "-" : "EMPTY", sx + sz / 2, sy + sz + 4, U_SLATE); }
            TxC53(MaxSel51() + " SLOTS. ONE PATH. " + MaxKeys51() + " KEYSTONE" + (MaxKeys51() == 1 ? "" : "S") + " MAX", x + w / 2, y + 86, U_GREY);
            bool crest = pp >= 0 && SelCount51() >= PT.pactCrestAt51;
            TxC53(crest ? PactData.PathShort[pp] + " CREST ACTIVE  " + PT.pactCrestAt51 + "+ FROM ONE PATH" : "NO CREST YET. " + PT.pactCrestAt51 + "+ FROM ONE PATH", x + w / 2, y + 97, crest ? U_AMB : U_GREY);
            float bx = x + 87, by = y + 110; bool ov = Over51(bx, by, 98, 26); Img53("btn_seal", bx, by, 98, 26); if (ov) Ol53(bx + 6, by - 2, 86, 30, Color.white);
            if (Click53(bx, by, 98, 26)) { Descend51(); return; }
            if (SmallBtn53(x, y + h + 8, 60, "EDIT")) _altarScr53 = 0;
            Hint53("CLICK SEAL PACT: SEAL AND DESCEND  /  ESC OR EDIT: BACK TO PATHS"); // v0.0.53 B01/B08
        }
    }
}
