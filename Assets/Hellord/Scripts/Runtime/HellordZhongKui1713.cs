using PixelFXPack;
using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    // t1713 (VH GO, Nov 25 Asia/Saigon): Zhong Kui, The Gravecaller — 4-slot kit v3.
    // t1772 (VH GO): Army of the Dead, Hell Gate and Zhong Kui's Banquet cut from the kit pool / R options
    //  (SkillIds + art kept, not offered). New R: Grave Legion. Bone Colossus persists until killed;
    //  its ultimate cooldown starts only after it dies. Ultimates: Ossuary Nova, Bone Colossus, Grave Legion.
    //  Design: research/t1709_zhongkui_kit_v3/report.md, plus VH t1713/t1772 ultimates.
    //  LMB basics (descendBasicsZhongKui1555): Soul Spears (kept, standalone) + Teeth + Skeleton Warriors +
    //    Skeleton Archers (auto-respawn up to cap; when the LMB pick is a summon, the press commands the living army).
    //  SPACE movement (own cooldown, no damage, no army hooks): Ghost Step, Bone Armour, Bone Wall.
    //  RMB special: Decrepify (area curse), Soul Plague (spreading mark), Skeletal Mage, Jiangshi.
    //  R ultimate (armed, then pressed): Ossuary Nova, Bone Colossus, Grave Legion.
    //  Active in a Zhong Kui / Necromancer run (pact path 1) while kit1618 + nkKit1713 are on. Shares Zhurong's run
    //  picks (ZhRun*1639), slot cooldowns (ZhMoveCd / ZhSpecCd / ZhUltCd), level cards and the kit bar.
    //  Kit skill k = NkK0 (37) + index in NkIds1713 (3 movement, 4 specials, 3 ultimates).
    //  Ossuary Tyrant and Crypt King are UNCHANGED. Grave Legion has no FX wiring yet.
    //  ICON HOOK: Resources/<nkIconDir1713>/icon_<key>_24.png; missing = a procedural placeholder.
    // =====================================================================================================================
    public partial class HellordGame
    {
        public static readonly string[] NkKeys1713 = { "ghoststep", "bonearmour", "bonewall", "decrepify", "soulplague", "skeletalmage", "jiangshi", "ossuarynova", "bonecolossus", "gravelegion", "teeth" };
        public static readonly string[] NkMoveNames1713 = { "NONE", "GHOST STEP", "BONE ARMOUR", "BONE WALL" };
        public static readonly string[] NkSpecNames1713 = { "NONE", "DECREPIFY", "SOUL PLAGUE", "SKELETAL MAGE", "JIANGSHI" };
        public static readonly string[] NkUltNames1713 = { "NONE", "OSSUARY NOVA", "BONE COLOSSUS", "GRAVE LEGION" };
        public static readonly string[] NkMoveDesc1713 = { "", "During the dash, and shortly after, you are untargetable and pass through demons.", "For 3 s after a dash, a bone shell absorbs a pool of damage equal to 30% of your max life.", "The dash leaves a bone barrier across your path. It blocks demons and their bullets." };
        public static readonly string[] NkSpecDesc1713 = { "", "Curse an area: demons are slowed and take more damage. No death blast.", "Mark the demon near the cursor with a soul rot. If it dies, the rot spreads.", "Raise back-line mages. They live by their health and respawn after a cooldown if killed. At cap, the press bursts the next bolts.", "Raise one Jiangshi. It lives by its health and respawns after a cooldown if killed. At cap, the press is an immediate slam at the cursor." };
        public static readonly string[] NkUltDesc1713 = { "", "Armed after 30 bone hits, or 4 Marrow Harvest charges: a ring of bone spears bursts out of you.", "Armed while 5 or more minions are alive: they merge into one giant skeleton that lasts until killed, then split back out.", "Armed while at minion cap: for 10 s the cap doubles and every skeleton type you have rises at full strength. Extra over-cap skeletons crumble when it ends." };
        public static readonly SkillId[] NkIds1713 = { SkillId.NkGhostStep, SkillId.NkBoneArmour, SkillId.NkBoneWall, SkillId.NkDecrepify, SkillId.NkSoulPlague, SkillId.NkSkeletalMage, SkillId.NkClayGolem, SkillId.NkOssuaryNova, SkillId.NkBoneColossus, SkillId.NkGraveLegion };
        public const int NkK0 = 37; // after Zhurong 12 + Leizu 12 + Gonggong 13
        public static bool IsNkBasic1713(SkillId id) => id == SkillId.Teeth;
        static readonly Color NB = new Color32(0xe8, 0xd5, 0xa0, 255), NG = new Color32(0x5a, 0xc5, 0x4f, 255), NS = new Color32(0x9a, 0xe0, 0xd0, 255),
            NW = new Color32(0xff, 0xff, 0xff, 255), ND = new Color32(0x26, 0x2b, 0x44, 255), NP = new Color32(0x8b, 0x9b, 0xb4, 255);

        // ---------------------------------------------------------------- slots
        public bool NkOn1713 => TZ != null && TZ.kit1618 && TZ.nkKit1713 && !TZ.kitAnyPact1618 && _pactPath51 == 1;
        public int NkMove1713 => NkOn1713 ? Mathf.Clamp(ZhSlotOverride1639 ? TZ.nkMove1713 : ZhRunMove1639, 0, 3) : 0;
        public int NkSpec1713 => NkOn1713 ? Mathf.Clamp(ZhSlotOverride1639 ? TZ.nkSpecial1713 : ZhRunSpec1639, 0, 4) : 0;
        public int NkUlt1713 => NkOn1713 ? Mathf.Clamp(ZhSlotOverride1639 ? TZ.nkUlt1713 : ZhRunUlt1639, 0, 3) : 0;
        public static int NkK1713(int slot, int pick) => NkK0 + (slot == 0 ? pick - 1 : slot == 1 ? 2 + pick : 6 + pick);
        public bool NkMinionTick1713 => NkOn1713 && (NkSpec1713 >= 3 || _nmin536.Count > 0 || _nkColossus1713 || _nkLegionT1772 > 0f || _nkGateT1713 > 0f || _nkBanquetT1713 > 0f);
        public bool NkColossusOn1713 => _nkColossus1713;

        // meters carry over rooms; NkRunReset1713 per run
        [System.NonSerialized] public int NkBoneHits1713, NkBanquetPts1713, NkCasts1713;
        float _nkGhostT1713, _nkArmT1713, _nkArmHp1713, _nkCmdT1713, _nkFrenzyT1713, _nkArmyT1713, _nkGateT1713, _nkBanquetT1713, _nkNovaOrbit1713;
        Vector2 _nkCmdP1713, _nkDash0_1713, _nkGateP1713, _nkArmyA1713, _nkArmyB1713, _nkColP1713;
        int _nkPendMove1713, _nkColN1713; float _nkColT1713, _nkColAtk1713, _nkGateAcc1713, _nkColHp1713, _nkColMax1713, _nkColCdPend1772, _nkLegionT1772;
        bool _nkColossus1713;
        readonly List<NMin536> _nkStash1713 = new List<NMin536>();
        readonly List<int> _nkq1713 = new List<int>(64);
        class NkWall { public Vector2 a, b; public float t, life, thick; public int hp; public readonly Dictionary<int, float> hitAt = new Dictionary<int, float>(); }
        NkWall _nkWall1713;
        float _nkMageResp1713, _nkGolemResp1713, _nkPlagueTick1713;
        readonly Dictionary<string, Texture2D> _nkIcon1713 = new Dictionary<string, Texture2D>();

        void ClearNk1713()
        {   // per room (ClearZh1618): effects / entities. Meters + ultimate cooldown carry over.
            // t1772: Bone Colossus and Grave Legion persist across rooms (colossus snaps near the hero).
            _nkGhostT1713 = _nkArmT1713 = _nkArmHp1713 = _nkCmdT1713 = _nkFrenzyT1713 = _nkArmyT1713 = _nkGateT1713 = _nkBanquetT1713 = _nkNovaOrbit1713 = 0f;
            _nkPendMove1713 = 0; _nkWall1713 = null; if (_nkColossus1713) _nkColP1713 = _heroPos; ClearNkFx1736();
            // drop temp ghosts of this kit (kind 4) between rooms; permanents teleport with NecroRoomEnter536
            for (int i = _nmin536.Count - 1; i >= 0; i--) if (_nmin536[i].kind >= 4) _nmin536.RemoveAt(i);
        }
        void NkRunReset1713()
        {
            NkBoneHits1713 = 0; NkBanquetPts1713 = 0;
            _nkColCdPend1772 = 0f; NkEndColossus1713(false);
            _nkLegionT1772 = 0f;
            _nkMageResp1713 = _nkGolemResp1713 = 0f; // t1796: first raise of a new run is immediate (death arms the respawn CD)
        }
        void NkFillUlt1713()
        {
            ZhMoveCd = ZhSpecCd = ZhUltCd = 0;
            int u = NkUlt1713; var T = TZ;
            if (u == 1) { NkBoneHits1713 = Mathf.Max(1, T.nkNovaNeed1713); _harvC536 = Mathf.Max(_harvC536, 4); }
        }

        // ---------------------------------------------------------------- icon hook
        public Texture2D NkIcon1713(string key)
        {
            if (key == null) return null; if (_nkIcon1713.TryGetValue(key, out var t)) return t;
            string dir = TZ != null ? TZ.nkIconDir1713 : ""; if (!string.IsNullOrEmpty(dir)) t = Resources.Load<Texture2D>(dir + "/icon_" + key + "_24");
            if (t != null) t.filterMode = FilterMode.Point; else t = NkIconPh1713(key);
            return _nkIcon1713[key] = t;
        }
        static Texture2D NkIconPh1713(string key)
        {   // PLACEHOLDER: 24x24 cell, bone-tan mark tinted by slot
            int ix = System.Array.IndexOf(NkKeys1713, key); Color tint = ix < 3 ? NS : ix < 7 ? NG : ix < 12 ? NB : new Color32(0xe8, 0xd5, 0xa0, 255);
            var t = new Texture2D(24, 24, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, name = "nk_ph" };
            Color bg = ND, fr = new Color32(0x3a, 0x44, 0x66, 255);
            for (int y = 0; y < 24; y++) for (int x = 0; x < 24; x++) t.SetPixel(x, y, x == 0 || y == 0 || x == 23 || y == 23 ? fr : bg);
            for (int i = 4; i < 20; i++) { t.SetPixel(11, i, tint); t.SetPixel(12, i, tint); }
            for (int i = 6; i < 18; i++) { int y = 6 + (i % 5); if (y < 20) { t.SetPixel(i, y, tint); t.SetPixel(i, 17 - (i % 5), tint); } }
            int pip = ix < 0 ? 0 : ix % 5; t.SetPixel(3 + pip, 21, NW); t.Apply(false, true); return t;
        }

        bool NkOk1713(Enemy e) => e != null && e.alive && !e.dying;
        int NkLiveMinions1713() => _nmin536.Count + (_nkColossus1713 ? _nkColN1713 : 0);
        public bool NkUltArmed1713
        {
            get
            {
                if (!NkOn1713 || ZhUltCd > 0.05f) return false; var T = TZ;
                switch (NkUlt1713)
                {
                    case 1: return NkBoneHits1713 >= Mathf.Max(1, T.nkNovaNeed1713) || _harvC536 >= 4;
                    case 2: return NkLiveMinions1713() >= Mathf.Max(1, T.nkColNeed1713) && !_nkColossus1713;
                    case 3: return _nkLegionT1772 <= 0f && NkLiveMinions1713() >= NCapBase536();
                    default: return false;
                }
            }
        }
        public float NkUltProg1713()
        {
            if (!NkOn1713) return 0f; var T = TZ; int u = NkUlt1713;
            if (u == 1) return _harvC536 >= 4 ? 1f : NkBoneHits1713 / (float)Mathf.Max(1, T.nkNovaNeed1713);
            if (u == 2) return _nkColossus1713 && _nkColMax1713 > 0f ? _nkColHp1713 / _nkColMax1713 : NkLiveMinions1713() / (float)Mathf.Max(1, T.nkColNeed1713);
            if (u == 3) return NkLiveMinions1713() / (float)Mathf.Max(1, NCapBase536());
            return 0f;
        }

        // ---------------------------------------------------------------- update
        void UpdateNk1713(float dt)
        {
            TickNkMageGolem1713(dt);
            if (NkOn1713)
            {
                ZhMoveCd = Mathf.Max(0f, ZhMoveCd - dt); ZhSpecCd = Mathf.Max(0f, ZhSpecCd - dt); ZhUltCd = Mathf.Max(0f, ZhUltCd - dt);
                bool free = mode == Mode.Play && !_tmOpen && _pickCool716 <= 0f;
                if (free && HInput.SpecialDown1618()) NkSpecial1713();
                if (free && HInput.UltDown1618()) NkUltimate1713();
                _nkGhostT1713 -= dt; _nkArmT1713 -= dt; _nkCmdT1713 -= dt; _nkFrenzyT1713 -= dt; _nkNovaOrbit1713 -= dt;
                TickNkMove1713(dt); TickNkPlague1713(dt); TickNkUlt1713(dt);
            }
            NkFxTick1736(dt);
        }

        // ---------------------------------------------------------------- LMB Teeth + summon command
        bool CastTeeth1713(SkillInst s, float dmg)
        {
            var T = TZ != null ? TZ : HellordTuning.I;
            if (Nearest(_heroPos, 11, -1) < 0 && !BrkNear53(_heroPos, 11) && !Aiming()) { s.cd = 0.1f; return false; }
            Vector2 dir = AimDir(); Vector2 c = HeroCenter; Vector2 o = CastOrigin1528(c + dir * 0.4f, ref dir);
            int n = 5 + s.ProjExtra; float spread = T.nkTeethFan1713 / Mathf.Max(1, n - 1);
            float spd = T.nkTeethSpd1713; dmg *= T.nkTeethDmg1713;
            int pierce = s.PierceN, bounce = s.BounceN;
            for (int i = 0; i < n; i++)
            {
                float a = (i - (n - 1) * 0.5f) * spread; var d = (Vector2)(Quaternion.Euler(0, 0, a) * dir);
                SpawnProj(s, o, d * spd, dmg, pierce, s.ForkN > 0, bounce, 0f);
                if (_lastProj53 != null) { _lastProj53.life = T.nkTeethRange1713 / Mathf.Max(1f, spd); _lastProj53.sr.color = NB; }
            }
            HellordAudio.Play("spear_cast", 0.85f);
            return true;
        }
        bool CastNkCommand1713(SkillInst s)
        {
            Vector2 cur = MouseWorld(); ClampToRoom(ref cur, 4);
            int t = Nearest(cur, 10f, -1);
            _nkCmdP1713 = t >= 0 ? _en[t].pos : cur; _nkCmdT1713 = TZ.nkCmdDur1713;
            HellordAudio.Play("curse_rune_arm", 1.3f);
            HordeFX.CheapBurst(_nkCmdP1713 + Vector2.up * 0.4f, NG, 8);
            ZRingO1618(_nkCmdP1713, 0.7f, new Color(NG.r, NG.g, NG.b, 0.8f), FbGround + YSort(_nkCmdP1713.y) + 20, 0.6f);
            return true;
        }

        // ---------------------------------------------------------------- MOVEMENT
        void NkDashStart1713(Vector2 p0)
        {
            if (!NkOn1713) return; int m = NkMove1713; if (m == 0 || ZhMoveCd > 0f) return;
            var T = TZ; int k = NkK1713(0, m); NkCasts1713++; _nkPendMove1713 = m; _nkDash0_1713 = p0;
            if (m == 1) {
                float dur = T49.dashTimeS49 + T.nkGhostAfter1713; _nkGhostT1713 = dur; _iframe = Mathf.Max(_iframe, dur); ZhMoveCd = ZkCd1651(k, T.nkGhostCd1713); HellordAudio.Play("oxhead_lunge_dash", 1.6f);
                Vector2 dir = _dashDir49.sqrMagnitude > 1e-4f ? _dashDir49.normalized : Vector2.right;
                NkFxGhostStep1736(p0, p0 + dir * T49.dashDistU49, T49.dashTimeS49);
            }
            else if (m == 2) { ZhMoveCd = ZkCd1651(k, T.nkArmCd1713); }
            else if (m == 3) { ZhMoveCd = ZkCd1651(k, T.nkWallCd1713); }
        }
        void NkDashAfter1713()
        {
            int m = _nkPendMove1713; _nkPendMove1713 = 0; if (m == 0 || !NkOn1713) return; var T = TZ; int k = NkK1713(0, m);
            Vector2 dir = _dashDir49.sqrMagnitude > 1e-4f ? _dashDir49.normalized : Vector2.right, n = new Vector2(-dir.y, dir.x);
            if (m == 2) { _nkArmT1713 = ZkDur1651(k, T.nkArmDur1713); _nkArmHp1713 = maxHp * T.nkArmFrac1713; HellordAudio.Play("curse_rune_arm", 1.5f); NkFxBoneArmour1736(_nkArmT1713); }
            else if (m == 3)
            {
                Vector2 c = _nkDash0_1713 + dir * 0.2f; float L = T.nkWallLen1713;
                _nkWall1713 = new NkWall { a = c - n * L * 0.5f, b = c + n * L * 0.5f, life = ZkDur1651(k, T.nkWallDur1713), thick = T.nkWallThick1713, hp = Mathf.Max(1, T.nkWallHits1713) };
                HellordAudio.Play("frost_cleave", 0.45f);
                float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                NkFxBoneWall1736(c, ang, _nkWall1713.life);
            }
        }
        public bool NkGhostInvuln1713() => NkOn1713 && _nkGhostT1713 > 0f;
        public float NkArmourAbsorb1713(float dmg)
        {
            if (!NkOn1713 || _nkArmT1713 <= 0f || _nkArmHp1713 <= 0f || dmg <= 0f) return dmg;
            float use = Mathf.Min(dmg, _nkArmHp1713); _nkArmHp1713 -= use; dmg -= use;
            if (_nkArmHp1713 <= 0f) _nkArmT1713 = 0f;
            HordeFX.CheapBurst(HeroCenter, NB, 4); return dmg;
        }
        void TickNkMove1713(float dt)
        {
            if (_nkGhostT1713 > 0f && !NkFxLive1736("ZKFX_GhostStep")) { float a = 0.55f + 0.25f * Mathf.Sin(runTime * 20f); ZDiscO1618(_heroPos + Vector2.up * 0.5f, 0.55f, new Color(NS.r, NS.g, NS.b, a * 0.45f), YSort(_heroPos.y) + 40, 0.7f); }
            if (_nkArmT1713 > 0f && _nkArmHp1713 > 0f && !NkFxLive1736("ZKFX_BoneArmour"))
            {
                float pul = 0.85f + 0.15f * Mathf.Sin(runTime * 10f);
                ZRingO1618(_heroPos + Vector2.up * 0.55f, 0.7f * pul, new Color(NB.r, NB.g, NB.b, 0.75f), YSort(_heroPos.y) + 50, 0.6f);
            }
            var w = _nkWall1713;
            if (w != null)
            {
                w.t += dt;
                if (w.t >= w.life || w.hp <= 0) _nkWall1713 = null;
                else if (!NkFxLive1736("ZKFX_BoneWall"))
                {
                    float fade = Mathf.Clamp01((w.life - w.t) / 0.3f); int o = FbGround + YSort((w.a.y + w.b.y) * 0.5f) + 20;
                    ZSegO1618(w.a, w.b, w.thick, new Color(NB.r, NB.g, NB.b, 0.85f * fade), o);
                }
            }
        }
        bool NkBulletBlock1713(Vector2 p)
        {
            var w = _nkWall1713; if (w == null || PbSegDist(p, w.a, w.b) > w.thick + 0.15f) return false;
            return true;
        }
        void NkEnemyMove1713(Enemy e, Vector2 prev)
        {
            var w = _nkWall1713; if (w == null) return;
            if (GgSegBlock1682(e, prev, w.a, w.b, w.thick))
            { if (!w.hitAt.TryGetValue(e.id, out float lt) || runTime - lt >= 0.35f) { w.hitAt[e.id] = runTime; w.hp--; } }
        }

        // ---------------------------------------------------------------- SPECIALS
        void NkSpecial1713()
        {
            int sp = NkSpec1713; if (sp == 0) return; if (ZhSpecCd > 0f) { HellordAudio.Play("basic_not_ready", 0.7f); return; }
            int k = NkK1713(1, sp); var T = TZ; Vector2 cur = MouseWorld(); ClampToRoom(ref cur, 4);
            if (sp == 1)
            {
                float r = ZkR1651(k, T.nkDecrR1713); Query(cur, r, _nkq1713);
                foreach (int ei in _nkq1713)
                {
                    var e = _en[ei]; if (!NkOk1713(e) || (e.pos - cur).magnitude > r + e.radius) continue;
                    if (e.nkDecr1713 <= runTime) e.nkDecrFrom1713 = runTime;
                    e.nkDecr1713 = runTime + ZkDur1651(k, T.nkDecrDur1713);
                    e.slowAmt = e.slowT > 0f ? Mathf.Max(e.slowAmt, T.nkDecrSlow1713) : T.nkDecrSlow1713;
                    e.slowT = Mathf.Max(e.slowT, ZkDur1651(k, T.nkDecrDur1713));
                }
                ZhSpecCd = ZkCd1651(k, T.nkDecrCd1713); HellordAudio.Play("curse_rune_arm", 0.9f);
                if (!NkFxDecrepify1736(cur, ZkDur1651(k, T.nkDecrDur1713), r))
                    ZRingO1618(cur, r, new Color(NG.r, NG.g, NG.b, 0.85f), FbGround + YSort(cur.y) + 30, 0.6f);
            }
            else if (sp == 2)
            {
                Enemy best = null; float bd = T.nkPlaguePick1713; Query(cur, T.nkPlaguePick1713, _nkq1713);
                foreach (int ei in _nkq1713) { var e = _en[ei]; if (!NkOk1713(e)) continue; float d = (BodyCentre1595(e) - cur).magnitude; if (d < bd) { bd = d; best = e; } }
                if (best == null) { HellordAudio.Play("basic_not_ready", 0.7f); return; }
                NkAfflictPlague1713(best, k); ZhSpecCd = ZkCd1651(k, T.nkPlagueCd1713); HellordAudio.Play("pool_cast", 1.1f);
            }
            else if (sp == 3) { NkRaiseMage1713(k, cur); ZhSpecCd = ZkCd1651(k, T.nkMageCd1713); }
            else if (sp == 4) { NkRaiseGolem1713(k, cur); ZhSpecCd = ZkCd1651(k, T.nkGolemCd1713); }
            NkCasts1713++;
        }
        void NkAfflictPlague1713(Enemy e, int k)
        {
            if (!NkOk1713(e)) return; var T = TZ;
            e.nkPlague1713 = runTime + ZkDur1651(k, T.nkPlagueDur1713);
            e.nkPlagueDps1713 = ZkHit1651(k) * T.nkPlagueDps1713;
            if (!NkFxPlagueMark1736(e)) HordeFX.CheapBurst(e.pos + Vector2.up * e.hitY, NS, 6);
        }
        void TickNkPlague1713(float dt)
        {
            _nkPlagueTick1713 -= dt; if (_en == null) return;
            bool tick = _nkPlagueTick1713 <= 0f; if (tick) _nkPlagueTick1713 = 0.5f;
            int k = NkK1713(1, 2); var s = ZkI1651(k);
            for (int i = 0; i < _en.Length; i++)
            {
                var e = _en[i]; if (e == null || !e.alive) continue;
                if (e.nkDecr1713 > runTime && e.slowT < 0.05f) { e.slowT = 0.2f; e.slowAmt = Mathf.Max(e.slowAmt, TZ.nkDecrSlow1713); }
                if (e.nkPlague1713 > runTime)
                {
                    if (!NkFxLive1736("ZKFX_SoulPlague")) ZDiscO1618(e.pos + Vector2.up * 0.4f, 0.18f, new Color(NS.r, NS.g, NS.b, 0.55f), YSort(e.pos.y) + 8, 0.7f);
                    if (tick) { Hit(e, e.nkPlagueDps1713 * 0.5f, s, Vector2.zero, true); if (!e.alive) NkPlagueSpread1713(e); }
                }
            }
        }
        void NkPlagueSpread1713(Enemy src)
        {
            int k = NkK1713(1, 2); float r = ZkR1651(k, TZ.nkPlagueSpread1713); NkFxPlagueSpread1736(src.pos, r);
            Query(src.pos, r, _nkq1713);
            foreach (int ei in _nkq1713)
            {
                var e = _en[ei]; if (e == src || !NkOk1713(e) || (e.pos - src.pos).magnitude > r + e.radius) continue;
                NkAfflictPlague1713(e, k);
                if (src.nkDecr1713 > runTime) { if (e.nkDecr1713 <= runTime) e.nkDecrFrom1713 = runTime; e.nkDecr1713 = Mathf.Max(e.nkDecr1713, src.nkDecr1713); e.slowAmt = Mathf.Max(e.slowAmt, TZ.nkDecrSlow1713); e.slowT = Mathf.Max(e.slowT, src.nkDecr1713 - runTime); }
                if (src.curseT > 0f) e.curseT = Mathf.Max(e.curseT, src.curseT);
            }
        }
        public float NkVuln1713(Enemy e) => e != null && e.nkDecr1713 > runTime ? 1f + TZ.nkDecrVuln1713 : 1f;

        int NkMageCap1713() { if (NHas536(16) || NkSpec1713 != 3) return 0; int lv = Mathf.Max(1, ZkI1651(NkK1713(1, 3)).level); int n = HData.At(TZ.nkMageCount1713, lv, 1); if (n > 0 && NHas536(0)) n++; return n * NkLegionMul1772(); }
        int NkGolemCap1713() { if (NHas536(16) || NkSpec1713 != 4) return 0; return 1 * NkLegionMul1772(); }
        void NkRaiseMage1713(int k, Vector2 cur)
        {
            if (NHas536(16)) { HellordAudio.Play("basic_not_ready", 0.7f); return; }
            int want = NkMageCap1713(), have = NCount536(2, false);
            if (have < want)
            {
                int n = Mathf.Min(want - have, 2); Vector2 at = cur;
                for (int i = 0; i < n; i++) { Vector2 p = at + Random.insideUnitCircle * 0.8f; ClampToRoom(ref p, 6); NSpawn536(2, false, p); }
                HellordAudio.Play("curse_rune_arm", 0.8f);
            }
            else
            {
                foreach (var m in _nmin536) if (m.kind == 2 && !m.temp) m.burst = 1f;
                _nkCmdP1713 = cur; _nkCmdT1713 = TZ.nkCmdDur1713;
                HellordAudio.Play("spear_cast", 0.7f); HordeFX.CheapBurst(cur + Vector2.up * 0.3f, NS, 8);
            }
        }
        void NkRaiseGolem1713(int k, Vector2 cur)
        {
            if (NHas536(16)) { HellordAudio.Play("basic_not_ready", 0.7f); return; }
            int have = NCount536(3, false);
            if (have < NkGolemCap1713()) { Vector2 p = cur; ClampToRoom(ref p, 6); NSpawn536(3, false, p); HellordAudio.Play("oxhead_stomp_strike", 0.7f); }
            else
            {
                float r = ZkR1651(k, TZ.nkGolemSlamR1713); Query(cur, r, _nkq1713); var s = ZkI1651(k); float dmg = ZkHit1651(k) * TZ.nkGolemSlamDmg1713;
                foreach (int ei in _nkq1713) { var e = _en[ei]; if (!NkOk1713(e) || (e.pos - cur).magnitude > r + e.radius) continue; Hit(e, dmg, s, (e.pos - cur).normalized * 0.5f); if (!e.boss) e.stun53 = Mathf.Max(e.stun53, TZ.nkGolemStun1713 * (e.elite ? 0.5f : 1f)); }
                HellordAudio.Play("oxhead_stomp_strike", 0.9f);
                if (!NkFxClaySlam1736(cur, r)) ZRingO1618(cur, r, new Color(NB.r, NB.g, NB.b, 0.8f), FbGround + YSort(cur.y) + 25, 0.55f);
            }
        }
        // t1796: first fill (and a card pick) is immediate. The respawn CD is armed only when a summon dies (NkMinionDied1713).
        void NkOnSpecPicked1713()
        {
            if (!NkOn1713) return;
            if (NkSpec1713 == 3) { _nkMageResp1713 = 0f; NkFillMageGolemNow1713(2); }
            if (NkSpec1713 == 4) { _nkGolemResp1713 = 0f; NkFillMageGolemNow1713(3); }
        }
        void NkFillMageGolemNow1713(int kind)
        {
            if (NHas536(16) || _nkColossus1713) return;
            int want = kind == 2 ? NkMageCap1713() : kind == 3 ? NkGolemCap1713() : 0;
            int have = NCount536(kind, false);
            while (have < want && _nmin536.Count < NCap536())
            {
                Vector2 p = _heroPos + (kind == 2 ? Random.insideUnitCircle.normalized * 2f : new Vector2(1.8f, 0.3f) + Random.insideUnitCircle * 0.15f);
                ClampToRoom(ref p, 6);
                int before = _nmin536.Count;
                NSpawn536(kind, false, p);
                if (_nmin536.Count <= before) break;
                have++;
            }
        }
        void TickNkMageGolem1713(float dt)
        {
            if (NHas536(16)) { _nkMageResp1713 = _nkGolemResp1713 = 0f; return; }
            if (_nkColossus1713) return;
            TickNkSpecFill1713(2, NkMageCap1713(), ref _nkMageResp1713, dt);
            TickNkSpecFill1713(3, NkGolemCap1713(), ref _nkGolemResp1713, dt);
        }
        void TickNkSpecFill1713(int kind, int want, ref float resp, float dt)
        {
            int have = NCount536(kind, false);
            if (have >= want) { resp = 0f; return; }
            if (resp > 0f) { resp -= dt; if (resp > 0f) return; resp = 0f; }
            NkFillMageGolemNow1713(kind);
        }
        // RMB HUD: living summon HP (and the death-respawn CD when none are alive). Press CD stays on ZhSpecCd.
        public bool NkSpecSummonHud1713(out float hp, out float maxHp, out int have, out int want, out float resp)
        {
            hp = maxHp = 0f; have = want = 0; resp = 0f;
            if (!NkOn1713) return false;
            int kind = NkSpec1713 == 3 ? 2 : NkSpec1713 == 4 ? 3 : -1;
            if (kind < 0) return false;
            want = kind == 2 ? NkMageCap1713() : NkGolemCap1713();
            have = NCount536(kind, false); resp = kind == 2 ? _nkMageResp1713 : _nkGolemResp1713;
            foreach (var m in _nmin536) if (m.kind == kind && !m.temp) { hp += m.hp; maxHp += m.maxHp; }
            return true;
        }

        // ---------------------------------------------------------------- ULTIMATES
        void NkUltimate1713()
        {
            int u = NkUlt1713; if (u == 0) return; if (!NkUltArmed1713) { HellordAudio.Play("basic_not_ready", 0.6f); return; }
            int k = NkK1713(2, u); var T = TZ; NkCasts1713++; Vector2 cur = MouseWorld(); ClampToRoom(ref cur, 4);
            if (u == 1)
            {
                NkBoneHits1713 = 0; if (_harvC536 >= 4) _harvC536 = 0;
                float r = ZkR1651(k, T.nkNovaR1713); float dmg = ZkHit1651(k) * T.nkNovaDmg1713; var s = ZkI1651(k);
                Query(_heroPos, r, _nkq1713);
                foreach (int ei in _nkq1713) { var e = _en[ei]; if (!NkOk1713(e) || (e.pos - _heroPos).magnitude > r + e.radius) continue; Hit(e, dmg, s, (e.pos - _heroPos).normalized * 0.6f); }
                _nkNovaOrbit1713 = T.nkNovaOrbit1713; ZhUltCd = ZkCd1651(k, T.nkNovaCd1713);
                HellordAudio.Play("oxhead_stomp_strike", 0.6f); ScreenShake.Add(2f, 0.15f);
                if (!NkFxOssuaryNova1736(_heroPos, r))
                {
                    for (int i = 0; i < 10; i++) { float a = i * 36f * Mathf.Deg2Rad; ZSegO1618(_heroPos, _heroPos + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r, 0.08f, NB, YSort(_heroPos.y) + 80); }
                    ZRingO1618(_heroPos, r, new Color(NB.r, NB.g, NB.b, 0.9f), FbGround + YSort(_heroPos.y) + 30, 0.55f);
                }
            }
            else if (u == 2) { if (!NkBeginColossus1713(k)) { HellordAudio.Play("basic_not_ready", 0.6f); return; } }
            else if (u == 3)
            {
                _nkLegionT1772 = ZkDur1651(k, T.nkLegionDur1772);
                ZhUltCd = ZkCd1651(k, T.nkLegionCd1772);
                NkFxLegionCast1736();
                NkRaiseAllOwned1772();
                HellordAudio.Play("pact_unlock", 0.9f); ScreenShake.Add(2f, 0.15f);
            }
        }
        void TickNkUlt1713(float dt)
        {
            if (_nkArmyT1713 > 0f)
            {
                _nkArmyT1713 -= dt; float fade = Mathf.Clamp01(_nkArmyT1713 / 0.4f);
                ZSegO1618(_nkArmyA1713, _nkArmyB1713, TZ.nkArmyW1713, new Color(NS.r, NS.g, NS.b, 0.5f * fade), FbGround + YSort(_nkArmyA1713.y) + 18);
            }
            if (_nkGateT1713 > 0f)
            {
                _nkGateT1713 -= dt; _nkGateAcc1713 += dt * TZ.nkGateRate1713;
                if (!NkFxLive1736("ZKFX_HellGate") && !NkFxLive1736("ZKFX_HellGate_Floor"))
                    ZRingO1618(_nkGateP1713, 0.7f + 0.15f * Mathf.Sin(runTime * 8f), new Color(NS.r, NS.g, NS.b, 0.7f), FbGround + YSort(_nkGateP1713.y) + 22, 0.55f);
                while (_nkGateAcc1713 >= 1f) { _nkGateAcc1713 -= 1f; NkSpawnGhost1713(_nkGateP1713 + Random.insideUnitCircle * 0.4f, TZ.nkGhostLife1713); }
            }
            if (_nkBanquetT1713 > 0f) _nkBanquetT1713 -= dt;
            if (_nkFrenzyT1713 > 0f) { float pul = 0.5f + 0.2f * Mathf.Sin(runTime * 14f); foreach (var m in _nmin536) ZDiscO1618(m.p, 0.22f, new Color(NG.r, NG.g, NG.b, pul), YSort(m.p.y) + 2, 0.6f); }
            if (_nkLegionT1772 > 0f)
            {
                _nkLegionT1772 -= dt;
                if (_nkLegionT1772 <= 0f || NHas536(16)) NkEndLegion1772();
            }
            if (_nkColossus1713) TickNkColossus1713(dt);
            if (_nkNovaOrbit1713 > 0f && _orbs != null && _orbs.Count > 0)
            {   // placeholder: extra ring of bone pips while the orbit-double window is up
                int n = _orbs.Count; for (int i = 0; i < n; i++) { float a = runTime * 4f + i * Mathf.PI * 2f / n; Vector2 p = _heroPos + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 1.6f; ZDiscO1618(p, 0.12f, NB, YSort(p.y) + 6); }
            }
        }
        void NkSpawnGhost1713(Vector2 at, float life)
        {
            if (NHas536(16) || _nmin536.Count >= NCap536()) return;
            ClampToRoom(ref at, 6); NSpawn536(4, true, at, life > 0f ? life : TZ.nkGhostLife1713);
        }
        bool NkBeginColossus1713(int k)
        {
            if (_nkColossus1713 || NHas536(16)) return false;
            _nkStash1713.Clear(); Vector2 c = Vector2.zero; int n = 0;
            for (int i = _nmin536.Count - 1; i >= 0; i--) { var m = _nmin536[i]; c += m.p; n++; _nkStash1713.Add(m); _nmin536.RemoveAt(i); }
            if (n <= 0) return false;
            _nkColP1713 = c / n; ClampToRoom(ref _nkColP1713, 6); _nkColN1713 = n;
            _nkColT1713 = 0f; _nkColAtk1713 = 0.3f;
            _nkColMax1713 = Mathf.Max(1f, TZ.nkColHpPer1772 * n); _nkColHp1713 = _nkColMax1713;
            _nkColCdPend1772 = ZkCd1651(k, TZ.nkColCd1713);
            _nkColossus1713 = true; _nkColFace1736 = Vector2.down; HellordAudio.Play("oxhead_stomp_strike", 0.5f); ScreenShake.Add(3f, 0.22f);
            NkFxColossusMerge1736(_nkColP1713);
            return true;
        }
        void TickNkColossus1713(float dt)
        {
            _nkColAtk1713 -= dt;
            NkColossusHurt1772(dt);
            if (_nkColHp1713 <= 0f || NHas536(16)) { NkEndColossus1713(!NHas536(16)); return; }
            if ((_nkColP1713 - _heroPos).magnitude > PT.minionTeleport561) _nkColP1713 = _heroPos;
            int t = Nearest(_nkColP1713, 8f, -1); _nkColMoving1736 = false;
            if (t >= 0)
            {
                Vector2 want = _en[t].pos; Vector2 d = want - _nkColP1713;
                if (d.sqrMagnitude > 0.01f) _nkColFace1736 = d;
                if (d.magnitude > 1.2f) { _nkColP1713 = Vector2.MoveTowards(_nkColP1713, want, TZ.nkColSpd1713 * dt); ClampToRoom(ref _nkColP1713, 6); _nkColMoving1736 = true; }
            }
            int k = NkK1713(2, 2); float scale = TZ.nkColScale1713 + TZ.nkColScalePer1713 * (_nkColN1713 - 1);
            if (!NkDrawColossus1736())
            {
                ZDisc(_nkColP1713, 0.45f * scale, new Color(0, 0, 0, 0.4f), 0.5f);
                ZDiscO1618(_nkColP1713 + Vector2.up * 0.35f * scale, 0.55f * scale, NB, YSort(_nkColP1713.y) + 4);
                ZDiscO1618(_nkColP1713 + Vector2.up * 0.9f * scale, 0.35f * scale, NW, YSort(_nkColP1713.y) + 5);
                ZRingO1618(_nkColP1713, 0.7f * scale, new Color(NB.r, NB.g, NB.b, 0.5f), YSort(_nkColP1713.y) + 3, 0.6f);
            }
            // small HP bar (always on, so the body reads as a living unit)
            {
                float w = 0.9f; Vector2 hb = _nkColP1713 + Vector2.up * (1.7f + 0.35f * scale);
                float f = Mathf.Clamp01(_nkColHp1713 / Mathf.Max(1f, _nkColMax1713));
                ZSeg(hb - Vector2.right * (w * 0.5f), hb + Vector2.right * (w * 0.5f), 0.08f, new Color(0, 0, 0, 0.65f), 6);
                ZSeg(hb - Vector2.right * (w * 0.5f), hb + Vector2.right * (f - 0.5f) * w, 0.06f, new Color(0.5f, 1f, 0.5f), 7);
            }
            if (_nkColAtk1713 <= 0f)
            {
                _nkColAtk1713 = TZ.nkColAtk1713; float r = ZkR1651(k, TZ.nkColSlamR1713); float dmg = ZkHit1651(k) * TZ.nkColDmg1713 * (TZ.nkColDmgBase1713 + TZ.nkColDmgPer1713 * _nkColN1713);
                var s = ZkI1651(k); Query(_nkColP1713, r, _nkq1713);
                foreach (int ei in _nkq1713) { var e = _en[ei]; if (!NkOk1713(e) || (e.pos - _nkColP1713).magnitude > r + e.radius) continue; Hit(e, dmg, s, (e.pos - _nkColP1713).normalized * 0.7f); }
                HellordAudio.Play("oxhead_stomp_strike", 1.0f);
                if (!NkFxColossusStomp1736(_nkColP1713, r))
                    ZRingO1618(_nkColP1713, r, new Color(NB.r, NB.g, NB.b, 0.85f), FbGround + YSort(_nkColP1713.y) + 20, 0.55f);
            }
        }
        void NkColossusHurt1772(float dt)
        {
            float hr = TZ.nkColHurtR1772;
            _pq53.Clear(); Query(_nkColP1713, hr + 1.2f, _pq53);
            foreach (var q in _pq53) { var e = _en[q]; if (e.alive && !e.dying && e.stun53 <= 0 && (e.pos - _nkColP1713).magnitude < e.radius + hr) _nkColHp1713 -= e.dmg * PT.minionHurtMul536 * dt; }
            float br = TZ.nkColBlockR1772; Vector2 c = _nkColP1713 + Vector2.up * 0.7f;
            for (int i = _shots44.Count - 1; i >= 0; i--)
            {
                var s = _shots44[i]; if (s.kind == 2) continue;
                if ((s.p - c).magnitude < s.r + br) { FxSpark561(s.p); _nkColHp1713 -= s.dmg * PT.minionBulletHurt561; EndShot44(s, true); _shots44.RemoveAt(i); }
            }
            if (_bb != null) foreach (var b in _bb) if (b.alive && (b.pos - c).magnitude < BBulletR + br) { FxSpark561(b.pos); _nkColHp1713 -= BBulletDmg * PT.minionBulletHurt561; KillBB(b, true); }
        }
        void NkEndColossus1713(bool restore)
        {
            if (!_nkColossus1713 && _nkStash1713.Count == 0) return;
            Vector2 at = _nkColP1713; if (_nkColossus1713) NkFxColossusSplit1736(at); _nkColossus1713 = false; NkHideColossusSpr1736();
            if (restore && !NHas536(16))
            {
                for (int i = 0; i < _nkStash1713.Count; i++)
                {
                    var m = _nkStash1713[i]; Vector2 p = at + (i == 0 ? Vector2.zero : Random.insideUnitCircle * 1.3f); ClampToRoom(ref p, 6);
                    m.p = m.prev = p; if (_nmin536.Count < NCap536()) _nmin536.Add(m);
                }
            }
            _nkStash1713.Clear(); _nkColN1713 = 0;
            if (_nkColCdPend1772 > 0f) { ZhUltCd = Mathf.Max(ZhUltCd, _nkColCdPend1772); _nkColCdPend1772 = 0f; }
        }

        int NkLegionMul1772() => _nkLegionT1772 > 0f ? 2 : 1;
        int NkTypeWant1772(int kind)
        {
            if (kind < 2) return NBase536(kind);
            if (kind == 2) return NkMageCap1713();
            if (kind == 3) return NkGolemCap1713();
            return 0;
        }
        void NkRaiseAllOwned1772()
        {
            int mark = _nmin536.Count;
            for (int kind = 0; kind < 4; kind++)
            {
                int want = NkTypeWant1772(kind); if (want <= 0) continue;
                int have = NCount536(kind, false);
                while (have < want && _nmin536.Count < NCap536())
                {
                    Vector2 p = _heroPos + Random.insideUnitCircle * 1.6f; ClampToRoom(ref p, 6);
                    NSpawn536(kind, false, p); have++;
                }
                foreach (var m in _nmin536) if (m.kind == kind && !m.temp) m.hp = m.maxHp;
            }
            for (int i = mark; i < _nmin536.Count; i++) NkFxLegionRise1736(_nmin536[i]);
        }
        void NkCrumbleAt1772(int i)
        {
            var m = _nmin536[i];
            bool fx = NkFxLegionCrumble1736(m);
            if (!fx) { FxDeath561(m.p); _dead561.Add(new Dead561 { p = m.p, kind = m.kind, t = 0f, face = m.face }); }
            _nmin536.RemoveAt(i);
        }
        void NkEndLegion1772()
        {
            _nkLegionT1772 = 0f;
            if (NHas536(16)) return;
            for (int k = 0; k < 2; k++)
            {
                int allow = Mathf.Max(0, NBase536(k) - NCount536(k, false));
                if (_nrespN536[k] > allow) _nrespN536[k] = allow;
            }
            for (int kind = 0; kind < 4; kind++)
            {
                int want = NkTypeWant1772(kind);
                for (int i = _nmin536.Count - 1; i >= 0 && NCount536(kind, false) > want; i--)
                    if (_nmin536[i].kind == kind) NkCrumbleAt1772(i);
            }
            int cap = NCap536();
            for (int i = _nmin536.Count - 1; i >= 0 && _nmin536.Count > cap; i--)
                if (_nmin536[i].temp) NkCrumbleAt1772(i);
            for (int i = _nmin536.Count - 1; i >= 0 && _nmin536.Count > cap; i--) NkCrumbleAt1772(i);
        }

        // ---------------------------------------------------------------- hooks
        public void NkBoneHit1713(SkillInst s, bool tick)
        {
            if (!NkOn1713 || tick || s == null || !Bone536(s) || s.def.id == SkillId.NkOssuaryNova) return;
            NkBoneHits1713++;
        }
        public void NkOnKill1713(Enemy e, SkillInst s)
        {
            if (e == null) return;
            if (e.nkPlague1713 > runTime) NkPlagueSpread1713(e);
            if (!NkOn1713) return;
            bool minion = s != null && (s.def.tags & STag.Minion) != 0;
            if (minion && NkUlt1713 == 5 && _nkBanquetT1713 <= 0f && ZhUltCd <= 0.05f)
            {
                int add = e.boss ? TZ.nkBanquetBoss1713 : (e.elite ? TZ.nkBanquetElite1713 : TZ.nkBanquetNorm1713);
                NkBanquetPts1713 = Mathf.Min(TZ.nkBanquetNeed1713, NkBanquetPts1713 + add);
            }
            if (minion && _nkBanquetT1713 > 0f) NkSpawnGhost1713(e.pos, Mathf.Max(1.5f, _nkBanquetT1713));
        }
        void NkMinionDied1713(NMin536 m)
        {
            if (m == null) return;
            if (m.kind == 2) _nkMageResp1713 = Mathf.Max(_nkMageResp1713, TZ.nkMageResp1713 * (NHas536(3) ? PT.quickGrave536 : 1f));
            if (m.kind == 3) _nkGolemResp1713 = Mathf.Max(_nkGolemResp1713, TZ.nkGolemResp1713 * (NHas536(3) ? PT.quickGrave536 : 1f));
        }
        public float NkFrenzyDmg1713() => _nkFrenzyT1713 > 0f ? TZ.nkFrenzyDmg1713 : 1f;
        public float NkFrenzyRate1713() => _nkFrenzyT1713 > 0f ? TZ.nkFrenzySpd1713 : 1f;
        int NkCmdSteer1713(NMin536 m, int t, ref float spd)
        {
            if (_nkCmdT1713 <= 0f) return t;
            int ct = Nearest(_nkCmdP1713, 10f, -1);
            if (ct >= 0) { t = ct; m.tgt = ct; }
            else { m.roam = _nkCmdP1713; }
            spd *= 1.35f; return t;
        }

        // t1796: real Play-mode verification (Editor or player). Flag: -jiangshiPlaytest
        // Uses the run pick (ZhRunSpec1639 = 4) via ChooseKit1639, never kitSlotOverride1639.
        System.Collections.IEnumerator JiangshiPlaytestCo1796()
        {
            const string tag = "[t1796]";
            const string shot = "/workspace/tmp/jiangshi_d2_playtest.png";
            void Log(string m) { Debug.Log(tag + " " + m); }
            yield return null;
#if UNITY_EDITOR
            // Reflect into Hellord.Editor — runtime asmdef must not reference the Editor assembly.
            try
            {
                var t = System.Type.GetType("Hellord.EditorTools.HellordMenu, Hellord.Editor");
                t?.GetMethod("ForceGameView1920x1080_1808", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.Invoke(null, null);
            }
            catch (System.Exception e) { Debug.LogWarning("[t1796] ForceGameView invoke failed: " + e.Message); }
            for (int _gv = 0; _gv < 8; _gv++) yield return null;
            Debug.Log("[t1796] screen after GameView force " + Screen.width + "x" + Screen.height);
#endif
            var T = HellordTuning.I;
            T.kit1618 = true; T.nkKit1713 = true; T.kitAnyPact1618 = false; T.kitSlotOverride1639 = false;
            T.nkMove1713 = T.nkSpecial1713 = T.nkUlt1713 = 0;
            PreloadStop1675();
            if (_unlock51 != null) for (int p = 0; p < _unlock51.Length; p++) _unlock51[p] = int.MaxValue;
            _souls51 = 1700; _sigils51 = 15; _pactPath51 = 1; _pactSel51 = 1; _pactSealed51 = true; PactSave51();
            ResetRun();
            _pactPath51 = 1; _pactSel51 = 1; _pactSealed51 = true; PactApply51();
            PreloadStop1675();
            pendingLevels = 0; mode = Mode.Play;
            skills.Clear(); skills.Add(new SkillInst { def = HData.Skill(SkillId.SkeletonWarriors), level = 1 }); SetHeroLayers();
            foreach (var e in _en) if (e != null && e.alive) { e.dmg = 0f; e.speed = 0f; }
            ChooseKit1639(new Card { kind = CardKind.NewSkill, kit = 2, kitPick = 4 });
            SnapCamera();
            Log("picked Jiangshi via ChooseKit1639; kitSlotOverride=" + T.kitSlotOverride1639 + " ZhRunSpec1639=" + ZhRunSpec1639 + " NkSpec1713=" + NkSpec1713 + " NkOn1713=" + NkOn1713 + " path=" + _pactPath51);
            for (int i = 0; i < 30; i++) yield return null;
            int jn = NCount536(3, false);
            NMin536 golem = null; foreach (var m in _nmin536) if (m.kind == 3 && !m.temp) { golem = m; break; }
            if (golem != null) { golem.p = _heroPos + new Vector2(1.85f, 0.15f); golem.prev = golem.p; ClampToRoom(ref golem.p, 6); }
            SnapCamera();
            // Let the normal draw path render one body at the new position (do not call Skel561 here — it would leave a second SR for this frame).
            for (int i = 0; i < 8; i++) yield return null;
            var idle = A561("jiangshi_all_idle_south"); var move = A561("jiangshi_all_move_south"); var atk = A561("jiangshi_all_attack_south");
            bool idleOk = idle != null && idle.f != null && idle.f.Length > 0 && idle.f[0] != null;
            bool moveOk = move != null && move.f != null && move.f.Length > 0 && move.f[0] != null;
            bool atkOk = atk != null && atk.f != null && atk.f.Length > 0 && atk.f[0] != null;
            var ic = NkIcon1713("jiangshi");
            string icName = ic != null ? ic.name : "null";
            System.IO.Directory.CreateDirectory("/workspace/tmp");
            if (System.IO.File.Exists(shot)) System.IO.File.Delete(shot);
            Log("screen " + Screen.width + "x" + Screen.height + " fullscreen=" + Screen.fullScreen);
            int warHave = NCount536(0, false), warWant = NBase536(0);
            Log("D2 strip warriors " + warHave + "/" + warWant + " jiangshi " + jn + " NkSpec=" + NkSpec1713);
            ScreenCapture.CaptureScreenshot(shot);
            yield return new WaitForEndOfFrame();
            for (int i = 0; i < 20 && !System.IO.File.Exists(shot); i++) yield return null;
            Log("screenshot " + (System.IO.File.Exists(shot) ? ("ok " + new System.IO.FileInfo(shot).Length + " bytes") : "MISSING"));
            bool skel = false; if (golem != null) { skel = Skel561(golem, 0, 0f); HideSk561(); }
            Log("jiangshi count=" + jn + " hp=" + (golem != null ? golem.hp.ToString("0.#") : "none") + "/" + (golem != null ? golem.maxHp.ToString("0.#") : "-") + " temp=" + (golem != null && golem.temp) + " life=" + (golem != null ? golem.life.ToString() : "none") + " pos=" + (golem != null ? golem.p.ToString() : "none"));
            Log("sprites idle_south=" + idleOk + " move_south=" + moveOk + " attack_south=" + atkOk + " readable_idle=" + (idleOk && idle.f[0].texture != null && idle.f[0].texture.isReadable));
            Log("Skel561=" + skel + " RMB icon=" + icName + " override=" + T.kitSlotOverride1639 + " ZhSpecCd=" + ZhSpecCd.ToString("0.##") + " golemResp=" + _nkGolemResp1713.ToString("0.##"));

            // Skeletal Mage: immediate spawn on pick, lives by HP
            ChooseKit1639(new Card { kind = CardKind.NewSkill, kit = 2, kitPick = 3 });
            for (int i = 0; i < 8; i++) yield return null;
            int mn = NCount536(2, false); NMin536 mage = null; foreach (var m in _nmin536) if (m.kind == 2 && !m.temp) { mage = m; break; }
            var midle = A561("skel_mage_all_idle_south");
            bool mageSpr = midle != null && midle.f != null && midle.f.Length > 0 && midle.f[0] != null;
            Log("mage pick ZhRunSpec1639=" + ZhRunSpec1639 + " count=" + mn + " hp=" + (mage != null ? mage.hp.ToString("0.#") : "none") + "/" + (mage != null ? mage.maxHp.ToString("0.#") : "-") + " temp=" + (mage != null && mage.temp) + " life=" + (mage != null ? mage.life.ToString() : "none") + " sprites=" + mageSpr + " mageResp=" + _nkMageResp1713.ToString("0.##"));
            bool ok = jn > 0 && golem != null && golem.hp > 0f && !golem.temp && float.IsInfinity(golem.life) && idleOk && moveOk && atkOk && skel && ic != null && icName.IndexOf("jiangshi") >= 0 && T.kitSlotOverride1639 == false && ZhRunSpec1639 == 3 && mn > 0 && mage != null && mage.hp > 0f && !mage.temp && mageSpr;
            Log(ok ? "PASS" : "FAIL");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(ok ? 0 : 1);
#else
            Application.Quit(ok ? 0 : 1);
#endif
        }

        // t1815: 1920x1080 Play-mode check of the 3-pass range rebalance + live-radius FX + kit bar.
        System.Collections.IEnumerator RangeRebalancePlaytestCo1815()
        {
            const string tag = "[t1815]";
            const string shot = "/workspace/tmp/range_rebalance_playtest.png";
            void Log(string m) { Debug.Log(tag + " " + m); }
            string ex = null;
            Application.logMessageReceived += (c, s, typ) =>
            {
                if (typ != LogType.Exception || ex != null) return;
                // Editor SearchDatabase throws on startup in this Linux batch Play-mode (also seen in prior t1796/t1812 logs); ignore editor-only stacks.
                if ((s != null && s.IndexOf("UnityEditor.Search") >= 0) || (c != null && c.IndexOf("UnityEditor.Search") >= 0)) return;
                ex = c;
            };
            yield return null;
#if UNITY_EDITOR
            try
            {
                var t = System.Type.GetType("Hellord.EditorTools.HellordMenu, Hellord.Editor");
                t?.GetMethod("ForceGameView1920x1080_1808", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.Invoke(null, null);
            }
            catch (System.Exception e) { Debug.LogWarning("[t1815] ForceGameView invoke failed: " + e.Message); }
            for (int _gv = 0; _gv < 8; _gv++) yield return null;
            Debug.Log("[t1815] screen after GameView force " + Screen.width + "x" + Screen.height);
#endif
            var T = HellordTuning.I;
            T.kit1618 = true; T.nkKit1713 = true; T.kitAnyPact1618 = false; T.kitSlotOverride1639 = false;
            T.kitBar1641 = true; T.skillBelt720 = true;
            T.nkMove1713 = T.nkSpecial1713 = T.nkUlt1713 = 0;
            PreloadStop1675();
            if (_unlock51 != null) for (int p = 0; p < _unlock51.Length; p++) _unlock51[p] = int.MaxValue;
            _souls51 = 1700; _sigils51 = 15; _pactPath51 = 1; _pactSel51 = 1; _pactSealed51 = true; PactSave51();
            ResetRun();
            _pactPath51 = 1; _pactSel51 = 1; _pactSealed51 = true; PactApply51();
            PreloadStop1675();
            pendingLevels = 0; mode = Mode.Play;
            skills.Clear();
            skills.Add(new SkillInst { def = HData.Skill(SkillId.Teeth), level = 1 });
            skills.Add(new SkillInst { def = HData.Skill(SkillId.SkeletonWarriors), level = 1 });
            SetHeroLayers();
            foreach (var e in _en) if (e != null && e.alive) { e.dmg = 0f; e.speed = 0f; }
            ChooseKit1639(new Card { kind = CardKind.NewSkill, kit = 2, kitPick = 4 });
            SnapCamera();
            Log("picked Jiangshi; NkSpec=" + NkSpec1713 + " kitBar=" + KitBar1641);
            Log("tune teeth=" + T.nkTeethRange1713 + " slam=" + T.nkGolemSlamR1713 + " warReach=" + T.skelWarReach561 + " wall=" + T.nkWallLen1713 + " plagueSpread=" + T.nkPlagueSpread1713 + " mageBurst=" + T.nkMageBurstR1713 + " colStomp=" + T.nkColSlamR1713 + " groundAoe=" + T.groundAoeRange716 + " fpRange=" + T.fpRangeU1481 + " hySight=" + T.hySight1625 + " leash=" + T.minionLeash561 + " seek=" + T.minionSeekR590 + " standoff=" + T.archerStandoff582 + " kite=" + T.archerKiteR582 + " arcShot=" + T.skelArcRange561 + " iceNova=" + T.novaRadiusU[0] + ".." + T.novaRadiusU[T.novaRadiusU.Length - 1]);
            for (int i = 0; i < 30; i++) yield return null;
            int jn = NCount536(3, false);
            NMin536 golem = null; foreach (var m in _nmin536) if (m.kind == 3 && !m.temp) { golem = m; break; }
            if (golem != null) { golem.p = _heroPos + new Vector2(1.85f, 0.15f); golem.prev = golem.p; ClampToRoom(ref golem.p, 6); }
            Enemy dummy = null; foreach (var e in _en) if (e != null && e.alive) { dummy = e; break; }
            if (dummy != null) { dummy.pos = _heroPos + new Vector2(2.4f, 0.1f); dummy.hp = Mathf.Max(dummy.hp, 40f); }
            var teeth = skills.Count > 0 ? skills[0] : new SkillInst { def = HData.Skill(SkillId.Teeth), level = 1 };
            try { CastTeeth1713(teeth, 12f); } catch (System.Exception e) { if (ex == null) ex = e.Message; Log("CastTeeth ex " + e.Message); }
            try { NkRaiseGolem1713(NkK1713(1, 4), _heroPos + new Vector2(1.2f, 0f)); } catch (System.Exception e) { if (ex == null) ex = e.Message; Log("slam ex " + e.Message); }
            SnapCamera();
            for (int i = 0; i < 12; i++) yield return null;
            System.IO.Directory.CreateDirectory("/workspace/tmp");
            if (System.IO.File.Exists(shot)) System.IO.File.Delete(shot);
            var lmb = skills.Count > 0 ? skills[0] : null;
            var rmb = ZkSlotInst1651(1);
            int z = CamZoom();
            Log("screen " + Screen.width + "x" + Screen.height + " zoom=" + z + " camTarget=" + T.camTargetPx49 + " (expect 640x360 @ zoom 3)");
            int warHave = NCount536(0, false), warWant = NBase536(0);
            Log("D2 warriors " + warHave + "/" + warWant + " jiangshi " + jn + " NkSpec=" + NkSpec1713 + " teethLv=" + (lmb != null ? lmb.level : 0) + " RMB=" + (rmb != null ? rmb.level : 0) + " SPACE=" + KitMove1666 + " R=" + KitUlt1666);
            ScreenCapture.CaptureScreenshot(shot);
            yield return new WaitForEndOfFrame();
            for (int i = 0; i < 20 && !System.IO.File.Exists(shot); i++) yield return null;
            Log("screenshot " + (System.IO.File.Exists(shot) ? ("ok " + new System.IO.FileInfo(shot).Length + " bytes") : "MISSING"));
            if (ex != null) Log("exception " + ex);
            bool camOk = Screen.width == 1920 && Screen.height == 1080 && z == 3 && T.camTargetPx49.x == 640f && T.camTargetPx49.y == 360f;
            bool tuneOk = Mathf.Approximately(T.nkTeethRange1713, 5.5f) && Mathf.Approximately(T.nkGolemSlamR1713, 2.9f) && Mathf.Approximately(T.skelWarReach561, 1.5f) && Mathf.Approximately(T.groundAoeRange716, 9f) && Mathf.Approximately(T.minionLeash561, 10f) && Mathf.Approximately(T.minionSeekR590, 10f);
            bool ok = camOk && tuneOk && jn > 0 && golem != null && golem.hp > 0f && lmb != null && lmb.level >= 1 && lmb.def != null && lmb.def.id == SkillId.Teeth && rmb != null && rmb.level >= 1 && KitMove1666 == 0 && KitUlt1666 == 0 && System.IO.File.Exists(shot) && ex == null;
            Log(ok ? "PASS" : "FAIL cam=" + camOk + " tune=" + tuneOk + " ex=" + (ex ?? "none"));
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(ok ? 0 : 1);
#else
            Application.Quit(ok ? 0 : 1);
#endif
        }
    }
}
