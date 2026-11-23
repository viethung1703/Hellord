using PixelFXPack;
using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    // t1618 (VH GO): Zhurong's 4-slot skill kit. Design: research/t1614_zhurong/report.md (+ the t1616 addendum).
    //  Slots per run:  LMB  basic (t1517 path: Fireball / Infernal Boulder / Fire Pillar / Fire Wall / Hydra = the start pick; t1625: Hydra replaced Volcanic Orb)
    //                  SPACE movement = a layer on the normal dash with its OWN cooldown; on cooldown the dash is a plain dash
    //                  RMB  special (pressed, longer cooldown)      R  ultimate (longest cooldown, armed by its condition, then pressed;
    //                                                               Rebirth Flame is automatic when a hit would kill you)
    //  Picking (t1639): level-up cards (HellordKitCards1639.cs); the tuning / F1 selector (kitMove1618 / kitSpecial1618 / kitUlt1618) is a testing override (kitSlotOverride1639).
    //  Active only in a Zhurong run (pact path 0) unless kitAnyPact1618. Damage source = SkillId.ZhurongKit (never offered, not in the belt).
    //  FX HOOK (t1625): every effect goes through ZhFx1625 (HellordZhurongFx1625.cs), shaped to HFX's fx_v2 shader spec; shader FX are
    //            off until approved (zhFxV2_1625), so the procedural Endesga-32 placeholders + existing fire HFX strips draw (no new art).
    //            (OLD t1618 strip hook Resources/HellordArt/Zhurong1618/<key>_strip.png removed: that strip set was rejected.)
    //  ICON HOOK: Resources/HellordArt/Zhurong1618/icon_<key>_24.png shows in the HUD readout (PixelLab icons_v2 later). Keys: ZhKeys1618.
    // =====================================================================================================================
    public partial class HellordGame
    {
        static HellordTuning TZ => HellordTuning.I;
        public static readonly string[] ZhKeys1618 = { "embereffigy", "emberanchor", "backdraft", "pyremark", "flammability", "warcry", "smolderring", "hearthblessing", "sunswrath", "rebirthflame", "moltenheart", "vermilionbird", "firewall", "hydra" }; // icon keys = HA's icons_v2 file names (t1625)
        public static readonly string[] ZhMoveNames1618 = { "NONE", "EMBER EFFIGY", "EMBER ANCHOR", "BACKDRAFT" };
        public static readonly string[] ZhSpecNames1618 = { "NONE", "PYRE MARK", "FLAMMABILITY", "WAR CRY", "SMOLDER RING", "HEARTH BLESSING" };
        public static readonly string[] ZhUltNames1618 = { "NONE", "SUN'S WRATH", "REBIRTH FLAME", "MOLTEN HEART", "VERMILION BIRD" };
        static readonly Color ZY = new Color32(0xfe, 0xe7, 0x61, 255), ZO = new Color32(0xf7, 0x76, 0x22, 255), ZR = new Color32(0xe4, 0x3b, 0x44, 255), ZD = new Color32(0xa2, 0x26, 0x33, 255),
            ZK = new Color32(0x3e, 0x27, 0x31, 255), ZG = new Color32(0x8b, 0x9b, 0xb4, 255), ZW = new Color32(0xff, 0xff, 0xff, 255), ZA = new Color32(0xfe, 0xae, 0x34, 255), ZB = new Color32(0x73, 0x3e, 0x39, 255);

        // ---------------------------------------------------------------- state
        public bool ZhOn1618 => TZ != null && TZ.kit1618 && (TZ.kitAnyPact1618 || _pactPath51 == 0);
        public int ZhMove1618 => ZhOn1618 ? Mathf.Clamp(ZhSlotOverride1639 ? TZ.kitMove1618 : ZhRunMove1639, 0, 3) : 0; // t1639: run picks (level-up) unless the testing override
        public int ZhSpec1618 => ZhOn1618 ? Mathf.Clamp(ZhSlotOverride1639 ? TZ.kitSpecial1618 : ZhRunSpec1639, 0, 5) : 0;
        public int ZhUlt1618 => ZhOn1618 ? Mathf.Clamp(ZhSlotOverride1639 ? TZ.kitUlt1618 : ZhRunUlt1639, 0, 4) : 0;
        [System.NonSerialized] public float ZhMoveCd, ZhSpecCd, ZhUltCd; // seconds left
        SkillInst _zk1618; SkillInst ZK1618 => _zk1618 ?? (_zk1618 = new SkillInst { def = HData.Skill(SkillId.ZhurongKit), level = 1 });
        float ZkHit1618 => SkillDamage(ZK1618) * TZ.kitDmgMul1618;
        readonly List<int> _zq1618 = new List<int>(64), _zqCry1625 = new List<int>(32); // _zqCry1625: death blasts run inside other loops' Hit
        // counters for the autotests
        [System.NonSerialized] public int ZhCasts1618, ZhBoltsShot1625, ZhBoltHits1625, ZhLured1618, ZhSnaps1618, ZhPulled1618, ZhPyrePays1618, ZhCursed1618, ZhCryBooms1625, ZhCryPowered1625, ZhHeartEchoes1618, ZhBirdHits1618, ZhSunHits1618, ZhRebirths1618, ZhStormHits1618, ZhRingHits1618;
        [System.NonSerialized] public float ZhCryT1625 = -9f, ZhCryUseT1625 = -9f; // t1625 War Cry: time of the last roar / last consumed charge (fx)

        // movement
        [System.NonSerialized] public Vector2 ZhEffP1618, ZhAncP1618, ZhBdP1618; [System.NonSerialized] public float ZhEffT1618, ZhAncT1618, ZhBdT1618;
        // specials
        [System.NonSerialized] public Enemy ZhMark1618; int _zMarkId1618; [System.NonSerialized] public float ZhMarkT1618, ZhMarkGauge1618, ZhPyreBuffT1618;
        class ZFlam { public Vector2 p; public float t; public bool done; }
        readonly List<ZFlam> _zFlam1618 = new List<ZFlam>(); readonly List<Enemy> _zCursed1618 = new List<Enemy>();
        [System.NonSerialized] public int ZhCryCharges1625; // t1625 War Cry (replaced Molten Shell): powered basics left
        class ZRingS { public Vector2 p; public float t, tick; }
        readonly List<ZRingS> _zRings1618 = new List<ZRingS>();
        [System.NonSerialized] public float ZhHearthT1618;
        // ultimates
        [System.NonSerialized] public float ZhSunMeter1618; [System.NonSerialized] public int ZhBurning1618, ZhNearMiss1618; [System.NonSerialized] public float ZhHeartT1618;
        class ZSun { public Vector2 p; public float t; }
        readonly List<ZSun> _zSuns1618 = new List<ZSun>();
        class ZBird { public Vector2 a, dir, p; public float d, drop; public HashSet<Enemy> hit = new HashSet<Enemy>(); }
        readonly List<ZBird> _zBirds1618 = new List<ZBird>();
        class ZFx { public Vector2 p; public float t, life, r; public int kind; public string mat; public Vector4 rc; public int ord = int.MinValue, drive; public float seed = 0.37f; } // t1625: + the spec material it stands in for
        readonly List<ZFx> _zFx1618 = new List<ZFx>(); // short placeholder rings / bursts
        // Hydra (t1625; replaced Volcanic Orb)
        class ZHy { public Vector2 p; public float t, dmg, seed; public SkillInst s; public float[] next = new float[3], spitT = { -10f, -10f, -10f }, aim = new float[3]; public bool[] armed = new bool[3]; } // fx_v2 §14: 3 heads, own clocks / targets
        class ZBolt { public Vector2 p, v; public float t, life, dmg, seed; public SkillInst s; }
        readonly List<ZHy> _zHy1625 = new List<ZHy>(); readonly List<ZBolt> _zBolt1625 = new List<ZBolt>();
        public int ZhHydras1625 => _zHy1625.Count;

        bool ZhNoOffer1618(SkillDef d) => d.id == SkillId.ZhurongKit || (d.id == SkillId.FireWall || d.id == SkillId.Hydra) && HellordTuning.I.pbNoLevelUp1567;
        bool ZhNoSlow1618 => ZhHeartT1618 > 0f || ZhHearthT1618 > 0f;
        bool ZhCleanse1618 => ZhHearthT1618 > 0f;

        void ClearZh1618()
        {
            ZhMoveCd = ZhSpecCd = ZhUltCd = 0f; ZhEffT1618 = ZhAncT1618 = ZhBdT1618 = 0f; ZhMark1618 = null; ZhMarkT1618 = ZhMarkGauge1618 = ZhPyreBuffT1618 = 0f;
            _zFlam1618.Clear(); _zCursed1618.Clear(); ZhCryCharges1625 = 0; ZhCryT1625 = ZhCryUseT1625 = -9f; _zRings1618.Clear(); ZhHearthT1618 = 0f;
            ZhSunMeter1618 = 0f; ZhNearMiss1618 = 0; ZhBurning1618 = 0; ZhHeartT1618 = 0f; _zSuns1618.Clear(); _zBirds1618.Clear(); _zFx1618.Clear(); _zHy1625.Clear(); _zBolt1625.Clear();
        }

        // ---------------------------------------------------------------- icon hook (PixelLab icons_v2 later); FX hook: HellordZhurongFx1625.cs
        readonly Dictionary<string, Texture2D> _zIcon1618 = new Dictionary<string, Texture2D>();
        /// <summary>Icon hook: Resources/&lt;zhIconDir1625&gt;/icon_&lt;key&gt;_24.png (swap the set by changing the folder; "" = no icons, text only).</summary>
        Texture2D ZhIcon1618(string key)
        {
            string dir = TZ != null ? TZ.zhIconDir1625 : ""; if (key == null || string.IsNullOrEmpty(dir)) return null; string k = dir + "/" + key;
            if (_zIcon1618.TryGetValue(k, out var t)) return t; t = Resources.Load<Texture2D>(dir + "/icon_" + key + "_24"); if (t != null) t.filterMode = FilterMode.Point; return _zIcon1618[k] = t;
        }
        /// <summary>Picker-card icon for the t1618 / t1625 basics that have no sheet column (null = the old fallback).</summary>
        Texture2D ZhCardIcon1625(SkillId id) => id == SkillId.FireWall ? ZhIcon1618("firewall") : id == SkillId.Hydra ? ZhIcon1618("hydra") : null;
        static Vector2 ZSnap1618(Vector2 p) => new Vector2(Mathf.Round(p.x * 16f) / 16f, Mathf.Round(p.y * 16f) / 16f);
        void ZPx1618(Vector2 p, int px, Color c, int order) => Z37SR(HArt.Pixel(), ZSnap1618(p), new Vector2(px, px), 0f, c, order - Z37Order);
        void ZDiscO1618(Vector2 p, float r, Color c, int order, float flat = 1f) => Z37SR(_z37Disc, ZSnap1618(p), new Vector2(r * 2f, r * 2f * flat), 0f, c, order - Z37Order);
        void ZRingO1618(Vector2 p, float r, Color c, int order, float flat = 1f) => Z37SR(_z37Ring, ZSnap1618(p), new Vector2(r * 2f, r * 2f * flat), 0f, c, order - Z37Order);
        void ZFx1618(int kind, Vector2 p, float r, float life, string mat = null, Vector4 rc = default, int ord = int.MinValue, int drive = 0) => _zFx1618.Add(new ZFx { kind = kind, p = p, r = r, life = life, mat = mat, rc = rc, ord = ord, drive = drive });
        void ZTiles1618(Vector2 c, float r, float life)
        {   // burning ground picture: the existing Infernal Boulder fire patch tiles (HFX), spread over the area
            FireBasicsFX1481.Tile("infernal_boulder_fire_patch", c, FbGround + YSort(c.y), life);
            int n = Mathf.Clamp(Mathf.RoundToInt(r * 2.2f), 0, 10);
            for (int i = 0; i < n; i++) { float a = i * Mathf.PI * 2f / n + 0.3f; Vector2 q = c + new Vector2(Mathf.Cos(a) * r * 0.62f, Mathf.Sin(a) * r * 0.62f * 0.6f); FireBasicsFX1481.Tile("infernal_boulder_fire_patch", q, FbGround + YSort(q.y), life, i * 2); }
        }

        // ---------------------------------------------------------------- update (after the skills, inside Z37Begin / End)
        void UpdateZh1618(float dt)
        {
            ZhFxBegin1625(); UpdateHydras1625(dt);
            if (!ZhOn1618) { DrawZhFx1618(dt); ZhFxEnd1625(); return; }
            ZhMoveCd -= dt; ZhSpecCd -= dt; ZhUltCd -= dt; if (ZhMoveCd < 0) ZhMoveCd = 0; if (ZhSpecCd < 0) ZhSpecCd = 0; if (ZhUltCd < 0) ZhUltCd = 0;
            bool free = mode == Mode.Play && !_tmOpen && _pickCool716 <= 0f;
            if (free && HInput.SpecialDown1618()) ZhSpecial1618();
            if (free && HInput.UltDown1618()) ZhUltimate1618();
            // movement timers
            ZhEffT1618 -= dt; ZhAncT1618 -= dt; ZhBdT1618 -= dt;
            if (ZhEffT1618 > 0f) ZhDrawEffigy1618(); else if (ZhEffT1618 > -dt * 1.5f && ZhEffT1618 <= 0f) { HordeFX.CheapBurst(ZhEffP1618 + Vector2.up * 0.6f, ZO, 10); ZhEffT1618 = -1f; }
            if (ZhAncT1618 > 0f) ZhDrawAnchor1618();
            if (ZhBdT1618 > 0f) ZhDrawBackdraft1618();
            // specials
            TickPyre1618(dt); TickFlam1618(dt); TickCry1625(dt); TickRings1618(dt); TickHearth1618(dt);
            // ultimates
            ZhBurning1618 = 0; if (_en != null) for (int i = 0; i < _en.Length; i++) { var e = _en[i]; if (e != null && e.alive && !e.dying && e.burnT > 0f) ZhBurning1618++; }
            if (ZhHeartT1618 > 0f) { ZhHeartT1618 -= dt; _heroSlowT = 0f; _chill44 = 0; _frozen44 = 0f; _holdT49 = 0f; ZhDrawHeart1618(); } // can't be slowed: slows / chill / freeze / ghoul hold do not stick (the speed product is also ignored)
            TickSuns1618(dt); TickBirds1618(dt);
            DrawZhFx1618(dt); ZhFxEnd1625();
        }
        void DrawZhFx1618(float dt)
        {
            for (int i = _zFx1618.Count - 1; i >= 0; i--)
            {
                var f = _zFx1618[i]; f.t += dt; if (f.t >= f.life) { _zFx1618.RemoveAt(i); continue; } float k = f.t / f.life;
                int ord = FbGround + YSort(f.p.y) + 40;
                if (f.mat != null)
                {   // t1625 spec drive: 0 = _Progress over the life; 1 = area _Ignite 0.4 s / _Cool over the last 0.5-0.8 s; 2 = 0.2 s hit-hold then _Progress
                    bool sh = f.drive == 1 ? ZhFx1625(f.mat, f.p, f.rc.x, f.rc.y, f.rc.z, f.rc.w, f.ord != int.MinValue ? f.ord : ord, f.t, 0.37f, ZP.F("_Ignite", Z01(f.t / 0.4f)), ZP.F("_Cool", Z01((f.t - (f.life - Mathf.Min(0.8f, f.life * 0.3f))) / Mathf.Min(0.8f, f.life * 0.3f))))
                           : f.drive == 4 ? ZhFx1625(f.mat, f.p, f.rc.x, f.rc.y, f.rc.z, f.rc.w, f.ord != int.MinValue ? f.ord : ord, f.t, f.seed, ZP.F("_Ignite", 1f), ZP.F("_Cool", Z01((f.t - 0.3f) / 1.4f)))
                           : f.drive == 3 ? ZhFx1625(f.mat, f.p, f.rc.x, f.rc.y, f.rc.z, f.rc.w, f.ord != int.MinValue ? f.ord : ord, f.t, 0.37f, ZP.F("_Intensity", (0.9f * Mathf.Exp(-f.t / 0.25f) + 0.7f * Z01(f.t / 0.4f)) * (1f - Z01((f.t - (f.life - 0.8f)) / 0.8f)))) // ground light
                           : f.drive == 2 ? ZhFx1625(f.mat, f.p, f.rc.x, f.rc.y, f.rc.z, f.rc.w, f.ord != int.MinValue ? f.ord : ord, f.t, 0.37f, ZP.F("_Progress", Z01((f.t - 0.2f) / Mathf.Max(0.05f, f.life - 0.2f))))
                           : ZhFx1625(f.mat, f.p, f.rc.x, f.rc.y, f.rc.z, f.rc.w, f.ord != int.MinValue ? f.ord : ord, f.t, 0.37f, ZP.F("_Progress", k));
                    if (sh || f.kind < 0) continue;
                }
                switch (f.kind)
                {
                    case 0: ZRingO1618(f.p, f.r * (0.3f + 0.7f * k), new Color(ZO.r, ZO.g, ZO.b, 1f - k), ord, 0.6f); ZRingO1618(f.p, f.r * (0.2f + 0.6f * k), new Color(ZY.r, ZY.g, ZY.b, (1f - k) * 0.8f), ord + 1, 0.6f); break; // expanding fire ring
                    case 1: ZRingO1618(f.p, f.r * (1f - k), new Color(ZA.r, ZA.g, ZA.b, 0.5f + 0.5f * k), ord, 0.6f); break; // inward pull
                    case 2: ZDiscO1618(f.p, f.r * (0.5f + 0.5f * k), new Color(ZY.r, ZY.g, ZY.b, (1f - k) * 0.55f), ord, 0.6f); break; // flash disc
                }
            }
        }

        // ---------------------------------------------------------------- MOVEMENT (layer on Dash49)
        /// <summary>Called by Dash49 on a real dash start (before the hero moves). Vermilion Bird near-miss count + the movement slot effect when its own cooldown is ready.</summary>
        void ZhDashStart1618(Vector2 p0)
        {
            if (!ZhOn1618) return;
            if (ZhUlt1618 == 4 && ZhNearMissNow1618()) { ZhNearMiss1618++; ZFx1618(2, HeroCenter, 0.7f, 0.25f); if (ZhNearMiss1618 == Mathf.Max(1, TZ.vbNeed1618)) HellordAudio.Play("pact_unlock", 1.3f); }
            int m = ZhMove1618; if (m == 0 || ZhMoveCd > 0f) return; // on cooldown = plain dash
            ZhCasts1618++;
            if (m == 1) { ZhEffP1618 = p0; ZhEffT1618 = TZ.effLife1618; ZhMoveCd = TZ.effCd1618; HellordAudio.Play("fireball_cast", 0.7f); HordeFX.CheapBurst(p0 + Vector2.up * 0.6f, ZO, 8); }
            else if (m == 2) { ZhAncP1618 = p0; ZhAncT1618 = TZ.ancWindow1618; ZhMoveCd = TZ.ancCd1618; HellordAudio.Play("curse_rune_arm", 1.4f); }
            else if (m == 3)
            {
                ZhMoveCd = TZ.bdCd1618; ZhBdP1618 = p0; ZhBdT1618 = 1.0f; ZFx1618(1, p0, TZ.bdR1618, 0.4f); // t1625: vortex visual 1.0 s (spec); pull unchanged HellordAudio.Play("storm_ring", 0.6f);
                Query(p0, TZ.bdR1618, _zq1618);
                foreach (int i in _zq1618) { var e = _en[i]; if (!e.alive || e.dying || e.boss) continue; Vector2 d = p0 - e.pos; if (d.magnitude > TZ.bdR1618) continue; e.push = d * TZ.bdPull1618 * 8f; ZhPulled1618++; } // push decays at 8/s: total move = push / 8
            }
        }
        /// <summary>Ember Anchor: a dash press while the ember is lit snaps you back to it (free: no dash cooldown, no new charge).</summary>
        bool ZhAnchorSnap1618()
        {
            if (!ZhOn1618 || ZhMove1618 != 2 || ZhAncT1618 <= 0f || mode != Mode.Play || !DashPressed716()) return false;
            Vector2 from = _heroPos; _heroPos = ZhAncP1618; ZhAncT1618 = 0f; ZhSnaps1618++; _iframe = Mathf.Max(_iframe, T49.dashIFrameS49); _heroVel37 = Vector2.zero; _holdT49 = 0;
            HordeFX.CheapBurst(from + Vector2.up * 0.5f, ZO, 6); HordeFX.CheapBurst(_heroPos + Vector2.up * 0.5f, ZY, 10); ZFx1618(0, _heroPos, 1.2f, 0.5f, "ZFX_EmberAnchor_Snap", new Vector4(-22, -16 + 8, 44, 40), YSort(_heroPos.y) + 200); HellordAudio.Play("oxhead_lunge_dash", 1.5f);
            return true;
        }
        void ZhLure1618(Enemy e, ref Vector2 v, float sp)
        {
            if (ZhEffT1618 <= 0f || e.boss) return; Vector2 d = ZhEffP1618 - e.pos; float m = d.magnitude; if (m > TZ.effR1618) return;
            v = m > 0.3f ? d / m * sp : Vector2.zero; ZhLured1618++;
        }
        bool ZhNearMissNow1618()
        {   // "dash out of an attack in its last moment": an enemy within its touch reach + pad, or a live enemy bullet within its radius + pad
            float pad = TZ.vbPad1618; Query(_heroPos, 3f, _zq1618);
            foreach (int i in _zq1618) { var e = _en[i]; if (!e.alive || e.dying || e.stun53 > 0 || e.stag1434 > 0) continue; if ((_heroPos - e.pos).magnitude < e.radius + 0.4f * HeroHitK47 + pad) return true; }
            foreach (var b in _eb49) { if (b == null || !b.alive || b.vis) continue; if (Mathf.Min((b.p - _heroPos).magnitude, (b.p - HeroCenter).magnitude) < b.r + 0.4f + pad) return true; }
            return false;
        }
        void ZhDrawEffigy1618()
        {
            var p = ZhEffP1618; float a = Mathf.Clamp01(ZhEffT1618 / 0.3f) * (0.75f + 0.25f * Mathf.Sin(runTime * 30f)); int ord = YSort(p.y);
            float age = TZ.effLife1618 - ZhEffT1618;
            ZhFx1625("ZFX_HearthBlessing_Light", p, -20, -8, 40, 16, FbGround + YSort(p.y) + 3, age, 0.21f, ZP.F("_Row", 6f), ZP.F("_Intensity", 0.9f * (1f - Z01((age - (TZ.effLife1618 - 0.6f)) / 0.6f)))); // GLight
            if (!ZhFx1625("ZFX_EmberEffigy", p, -26, -6, 52, 50, ord, age, 0.21f, ZP.F("_HeroSprite", 1), ZP.F("_Form", Z01(age / 0.15f)), ZP.F("_Fade", Z01((age - (TZ.effLife1618 - 0.6f)) / 0.6f))))
            {   // placeholder: the hero's current frame, flame-tinted, + rising ember pixels
                if (_heroBase != null && _heroBase.sprite != null) { var sr = Z37SR(_heroBase.sprite, ZSnap1618(p), (Vector2)_heroBase.transform.lossyScale, 0f, new Color(ZO.r, ZO.g * 0.9f, ZO.b, a), ord - Z37Order); sr.flipX = _heroBase.flipX; }
                for (int k = 0; k < 5; k++) { float ph = Mathf.Repeat(runTime * 1.3f + k * 0.21f, 1f); ZPx1618(p + new Vector2(Mathf.Sin(k * 2.1f + runTime * 3f) * 0.3f, 0.2f + ph * 1.5f), 1, ph < 0.5f ? ZY : ZO, ord + 2); }
            }
            ZDiscO1618(p, 0.5f, new Color(ZD.r, ZD.g, ZD.b, 0.35f * a), FbGround + YSort(p.y), 0.5f);
        }
        void ZhDrawAnchor1618()
        {
            var p = ZhAncP1618; float k = ZhAncT1618 / Mathf.Max(0.1f, TZ.ancWindow1618); int ord = YSort(p.y) + 5;
            float age = TZ.ancWindow1618 - ZhAncT1618;
            if (!ZhFx1625("ZFX_EmberAnchor_Marker", p, -14, -8, 28, 24, ord, age, 0.5f, ZP.F("_Ignite", Z01(age / 0.2f)), ZP.F("_Cool", 0f)))
            {
                float pu = 0.8f + 0.2f * Mathf.Sin(runTime * 12f); ZDiscO1618(p + Vector2.up * 0.15f, 0.14f * pu, ZY, ord + 1); ZDiscO1618(p + Vector2.up * 0.15f, 0.22f * pu, new Color(ZO.r, ZO.g, ZO.b, 0.8f), ord);
            }
            ZRingO1618(p, 0.25f + 0.35f * k, new Color(ZO.r, ZO.g, ZO.b, 0.7f), FbGround + YSort(p.y) + 2, 0.6f); // window ring shrinks as it runs out
        }
        void ZhDrawBackdraft1618()
        {
            var p = ZhBdP1618; float k = 1f - ZhBdT1618 / 1.0f; int ord = YSort(p.y) + 10;
            if (ZhFx1625("ZFX_Backdraft_Vortex", p, -40, -22, 80, 44, FbGround + YSort(p.y) + 4, k, 0.6f, ZP.F("_Progress", k))) return;
            k = Z01(k / 0.4f); // placeholder keeps its 0.4 s implosion
            for (int i = 0; i < 12; i++) { float a = i * 30f * Mathf.Deg2Rad + k * 3f; float r = TZ.bdR1618 * (1f - k); ZPx1618(p + new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r * 0.6f + 0.3f), 2, i % 2 == 0 ? ZA : ZO, ord); }
        }

        // ---------------------------------------------------------------- SPECIALS (RMB)
        void ZhSpecial1618()
        {
            int s = ZhSpec1618; if (s == 0) return;
            if (ZhSpecCd > 0f) { HellordAudio.Play("basic_not_ready", 0.8f); return; }
            bool ok = false; Vector2 cur = MouseWorld(); ClampToRoom(ref cur, 4);
            switch (s)
            {
                case 1: // Pyre Mark: the ONE enemy nearest the cursor
                {
                    Enemy best = null; float bd = TZ.pmPickR1618; Query(cur, TZ.pmPickR1618, _zq1618);
                    foreach (int i in _zq1618) { var e = _en[i]; if (!e.alive || e.dying) continue; float d = (BodyCentre1595(e) - cur).magnitude; if (d < bd) { bd = d; best = e; } }
                    if (best == null) { HellordAudio.Play("basic_not_ready", 0.8f); return; } // no target: no cooldown
                    ZhMark1618 = best; _zMarkId1618 = best.id; ZhMarkT1618 = TZ.pmLife1618; ZhMarkGauge1618 = 0f; ok = true; ZhSpecCd = TZ.pmCd1618; HellordAudio.Play("curse_rune_arm", 1.2f); ZFx1618(0, best.pos, 0.9f, 0.3f);
                    break;
                }
                case 2: { Vector2 at = GroundAim716(); _zFlam1618.Add(new ZFlam { p = at, t = 0f }); ok = true; ZhSpecCd = TZ.flCd1618; HellordAudio.Play("curse_rune_arm", 0.8f); break; }
                case 3: { ZhWarCry1625(); ok = true; ZhSpecCd = TZ.wcCd1625; break; } // t1625 War Cry (replaced Molten Shell)
                case 4: { Vector2 at = GroundAim716(); _zRings1618.Add(new ZRingS { p = at, t = 0f, tick = 0f }); ok = true; ZhSpecCd = TZ.srCd1618; HellordAudio.Play("pool_cast", 0.9f); ZFx1618(0, at, TZ.srR1618, 0.3f); break; }
                case 5: { ZhHearthT1618 = TZ.hbDur1618; ZhHearthClear1618(); ok = true; ZhSpecCd = TZ.hbCd1618; HellordAudio.Play("shrine_use", 1.1f); ZFx1618(-1, _heroPos, 0.8f, 0.4f, "ZFX_HearthBlessing_Cleanse", new Vector4(-26, -20 + 10, 52, 48), ZhHeroOrd1625 - 1); break; }
            }
            if (ok) ZhCasts1618++;
        }
        void TickPyre1618(float dt)
        {
            ZhPyreBuffT1618 -= dt; if (ZhPyreBuffT1618 > 0f) { float k = 0.5f + 0.5f * Mathf.Sin(runTime * 10f); ZRingO1618(_heroPos, 0.55f, new Color(ZO.r, ZO.g, ZO.b, 0.4f + 0.3f * k), FbGround + YSort(_heroPos.y) + 3, 0.55f); }
            var e = ZhMark1618; if (e == null) return;
            if (!e.alive || e.id != _zMarkId1618) { ZhMark1618 = null; return; } // (death is handled in ZhOnKill1618)
            ZhMarkT1618 -= dt; if (ZhMarkT1618 <= 0f) { ZhMark1618 = null; return; } // expired unused
            if (ZhMarkGauge1618 >= TZ.pmThresh1618 * e.maxHp) { ZhPyrePay1618(e.pos); return; }
            Vector2 top = e.pos + Vector2.up * (BodySize1595(e).y + 0.35f); int ord = YSort(e.pos.y) + 60;
            float mAge = TZ.pmLife1618 - ZhMarkT1618, gauge = Z01(ZhMarkGauge1618 / Mathf.Max(1f, TZ.pmThresh1618 * e.maxHp));
            if (!ZhFx1625("ZFX_PyreMark", top, -10, -4, 20, 26, ord, mAge, 0.3f, ZP.F("_Progress", Z01(mAge / 0.2f)), ZP.F("_Intensity", gauge)))
            {   // placeholder: a flame diamond over the head + gauge pixels, ring at the feet
                float b = Mathf.Floor(Mathf.Sin(runTime * 8f) * 1.5f) / 16f; Vector2 c = top + Vector2.up * b;
                ZPx1618(c, 3, ZR, ord); ZPx1618(c + new Vector2(0, 2f / 16f), 2, ZO, ord + 1); ZPx1618(c + new Vector2(0, 3f / 16f), 1, ZY, ord + 2);
                float g = Mathf.Clamp01(ZhMarkGauge1618 / Mathf.Max(1f, TZ.pmThresh1618 * e.maxHp)); int n = Mathf.RoundToInt(g * 8);
                for (int i = 0; i < 8; i++) ZPx1618(c + new Vector2((i - 3.5f) / 16f, -4f / 16f), 1, i < n ? ZY : ZK, ord);
            }
            ZRingO1618(e.pos, e.radius + 0.15f, new Color(ZR.r, ZR.g, ZR.b, 0.85f), FbGround + YSort(e.pos.y) + 4, 0.55f);
        }
        void ZhPyrePay1618(Vector2 at) { ZhMark1618 = null; ZhPyreBuffT1618 = TZ.pmBuffS1618; ZhPyrePays1618++; HellordAudio.Play("fire_pillar_erupt", 1.25f); ZFx1618(0, at, 1.6f, 0.45f, "ZFX_PyreMark_Burst", new Vector4(-24, -18 + 10, 48, 48), YSort(at.y) + 200); ZFx1618(2, _heroPos, 0.9f, 0.3f); Toast("PYRE MARK: +FIRE DAMAGE"); }
        void ZhOnKill1618(Enemy e) { if (e != null && e == ZhMark1618 && e.id == _zMarkId1618 && ZhOn1618) ZhPyrePay1618(e.pos); }
        float ZhPreHit1618(float dmg, SkillInst s) => ZhPyreBuffT1618 > 0f && s != null && (s.def.tags & STag.Fire) != 0 ? dmg * TZ.pmBuffMul1618 : dmg;
        float ZhBurnK1618(Enemy e)
        {
            float k = 1f; if (!ZhOn1618 || e == null) return k;
            if (e == ZhMark1618) k *= TZ.pmBurnK1618; if (e.zFlam1618 > runTime) k *= TZ.flBurnK1618; return k;
        }
        /// <summary>Burn tick (BurnFX.OnTick): Flammability multiplier; feeds the Pyre Mark gauge and the Sun's Wrath meter.</summary>
        float ZhBurnTick1618(Enemy e, float dmg)
        {
            if (!ZhOn1618) return 1f; float m = e.zFlam1618 > runTime ? TZ.flBurnDmg1618 : 1f; float d = dmg * m;
            if (e == ZhMark1618) ZhMarkGauge1618 += d;
            if (ZhUlt1618 == 1) ZhSunMeter1618 = Mathf.Min(TZ.swMeter1618, ZhSunMeter1618 + d);
            return m;
        }
        void TickFlam1618(float dt)
        {
            for (int i = _zFlam1618.Count - 1; i >= 0; i--)
            {
                var f = _zFlam1618[i]; f.t += dt; float R = TZ.flR1618, D = TZ.flDelay1618; int og = FbGround + YSort(f.p.y) + 6;
                float cool = Z01((f.t - D - TZ.flDur1618) / 0.6f);
                bool shd = ZhFx1625("ZFX_Flammability_Curse", f.p, -46, -26, 92, 52, og, f.t, 0.45f, ZP.F("_Ignite", Z01(f.t / Mathf.Max(0.05f, D))), ZP.F("_Cool", cool));
                if (f.done) { if (cool >= 1f) _zFlam1618.RemoveAt(i); continue; } // t1625: the cursed ground stays for the spec's sustain + cool (no gameplay)
                {
                    float k = Mathf.Clamp01(f.t / Mathf.Max(0.05f, D));
                    if (!shd)
                    {   // rune circle: outer ring + a filling disc + 6 turning glyph pixels (the delay is the counterplay)
                        ZRingO1618(f.p, R, new Color(ZR.r, ZR.g, ZR.b, 0.9f), og, 0.6f); ZDiscO1618(f.p, R * k, new Color(ZD.r, ZD.g, ZD.b, 0.35f), og - 1, 0.6f);
                        for (int g = 0; g < 6; g++) { float a = g * 60f * Mathf.Deg2Rad + f.t * 1.5f; ZPx1618(f.p + new Vector2(Mathf.Cos(a) * R * 0.8f, Mathf.Sin(a) * R * 0.8f * 0.6f), 2, ZO, og + 1); }
                    }
                    if (f.t >= D)
                    {
                        f.done = true; HellordAudio.Play("curse_rune_burst", 1.1f); ZFx1618(0, f.p, R, 0.35f); Query(f.p, R + 0.5f, _zq1618);
                        foreach (int ei in _zq1618) { var e = _en[ei]; if (!e.alive || e.dying) continue; Vector2 d = e.pos - f.p; float nx = d.x / R, ny = d.y / (R * 0.6f + 0.4f); if (nx * nx + ny * ny > 1f) continue; e.zFlam1618 = runTime + TZ.flDur1618; if (!_zCursed1618.Contains(e)) _zCursed1618.Add(e); ZhCursed1618++; }
                    }
                }
            }
            for (int i = _zCursed1618.Count - 1; i >= 0; i--)
            {
                var e = _zCursed1618[i]; if (!e.alive || e.zFlam1618 <= runTime) { _zCursed1618.RemoveAt(i); continue; }
                int o = FbGround + YSort(e.pos.y) + 5; float tk = Mathf.Repeat(runTime + e.id * 0.13f, 0.7f);
                if (!ZhFx1625("ZFX_Flammability_Tick", BodyCentre1595(e), -6, -2, 12, 14, YSort(e.pos.y) + 50, tk, (e.id % 7) / 7f, ZP.F("_Progress", Z01(tk / 0.36f)))) { ZRingO1618(e.pos, e.radius + 0.05f, new Color(ZD.r, ZD.g, ZD.b, 0.9f), o, 0.5f); ZPx1618(e.pos + Vector2.up * (BodySize1595(e).y + 0.15f), 2, ZD, YSort(e.pos.y) + 50); }
            }
        }
        // ---------------------------------------------------------------- WAR CRY (t1625; replaced Molten Shell)
        /// <summary>Roar: the next wcCharges1625 basic casts deal x wcMul1625; enemies in wcR1625 explode if they die within wcMarkS1625.</summary>
        void ZhWarCry1625()
        {
            var T = TZ; ZhCryCharges1625 = T.wcCharges1625; ZhCryT1625 = runTime; HellordAudio.Play("oxhead_roar", 1.0f); HellordAudio.Play("storm_ring", 0.5f); ScreenShake.Add(1.5f, 0.15f);
            if (!ZhFxLive1625("ZFX_WarCry_Roar")) { ZFx1618(0, _heroPos, T.wcR1625, 0.4f); ZFx1618(2, HeroCenter, 0.8f, 0.2f); }
            Query(_heroPos, T.wcR1625, _zq1618); foreach (int i in _zq1618) { var e = _en[i]; if (!e.alive || e.dying || (e.pos - _heroPos).magnitude > T.wcR1625) continue; e.zCry1625 = runTime + T.wcMarkS1625; }
        }
        /// <summary>BasicCast1517: damage multiplier for this basic cast (a War Cry charge); ZhCryUsed1625 spends it after a real cast.</summary>
        float ZhCryMul1625() => ZhOn1618 && ZhSpec1618 == 3 && ZhCryCharges1625 > 0 ? TZ.wcMul1625 : 1f;
        void ZhCryUsed1625()
        {
            if (ZhCryCharges1625 <= 0) return; ZhCryCharges1625--; ZhCryPowered1625++; ZhCryUseT1625 = runTime;
            Vector2 hand = _heroPos + new Vector2((_heroBase != null && _heroBase.flipX ? -9f : 9f) / 16f, 11f / 16f); // fx_v2 §15: consume burst at the hand point
            ZFx1618(-1, hand, 0.5f, 0.3f, "ZFX_WarCry_Consume", new Vector4(-14, -10, 28, 26), ZhHeroOrd1625 + 200);
        }
        void ZhCryKill1625(Enemy e)
        {
            if (e == null || e.zCry1625 <= runTime) return; e.zCry1625 = 0f; var T = TZ; Vector2 c = e.pos; float R = T.wcBoomR1625; ZhCryBooms1625++;
            HellordAudio.Play("magma_rock_hit", 1.2f); ZFx1618(0, c, R, 0.45f, "ZFX_WarCry_DeathPop", new Vector4(-24, -18 + 10, 48, 48), YSort(c.y) + 200); if (!ZhFxLive1625("ZFX_WarCry_DeathPop")) FireBasicsFX1481.Once("molten_spit_pop_v1", c + Vector2.up * 0.4f, YSort(c.y) + 40);
            Query(c, R + 0.5f, _zqCry1625); var hit = new List<Enemy>(); foreach (int i in _zqCry1625) { var o = _en[i]; if (o != e && o.alive && !o.dying && (o.pos - c).magnitude <= R) hit.Add(o); }
            foreach (var o in hit) Hit(o, ZkHit1618 * T.wcBoomDmg1625, ZK1618, (o.pos - c).normalized * 2f); // marked enemies killed by a blast explode too (chain)
        }
        void TickCry1625(float dt)
        {
            if (!ZhOn1618 || ZhSpec1618 != 3) return; float age = runTime - ZhCryT1625; int ho = ZhHeroOrd1625; var T = TZ;
            // fx_v2 §15 Roar (0.6 s): wave + light; the front runs out to the gameplay radius (marks are applied at the cast)
            if (age < 0.6f)
            {
                float Rp = T.wcR1625 * 16f, pr = age / 0.6f, q = Z01((pr - 0.22f) / 0.78f), front = Rp * ZEo(Z01(1.15f * q)) + 2f;
                ZhFx1625("ZFX_WarCry_Roar", _heroPos, -(Rp + 8f), -(Rp * 0.55f + 6f), 2f * (Rp + 8f), 2f * (Rp * 0.55f + 6f), FbGround + YSort(_heroPos.y) + 6, age, 0.5f, ZP.F("_Progress", pr), ZP.F("_Radius", Rp));
                ZhFx1625("ZFX_WarCry_Light", _heroPos, -(Rp + 12f), -(Rp * 0.55f + 6f), 2f * (Rp + 12f), 2f * (Rp * 0.55f + 6f), FbGround + YSort(_heroPos.y) + 7, age, 0.5f, ZP.F("_Radius", front + 8f), ZP.F("_Intensity", pr < 0.22f ? 0.4f * pr / 0.22f : 1.1f * Mathf.Pow(1f - q, 1.5f)));
            }
            // Empowered hero while charged (+0.25 s fade after the last charge)
            float sinceUse = runTime - ZhCryUseT1625; bool aura = ZhCryCharges1625 > 0 || ZhCryUseT1625 > ZhCryT1625 && sinceUse < 0.25f;
            if (aura)
            {
                float form = ZhCryCharges1625 > 0 ? Z01((age - 0.1f) / 0.15f) : 1f - Z01(sinceUse / 0.25f);
                float pulse = ZhCryUseT1625 > ZhCryT1625 && sinceUse < 0.2f ? 1f - sinceUse / 0.2f : Z01(age / 0.13f) * (age < 0.33f ? 1f - Z01((age - 0.13f) / 0.2f) : 0f);
                if (!ZhFx1625("ZFX_WarCry_Empowered", _heroPos, -26, -6, 52, 52, ho - 1, age, 0.5f, ZP.F("_HeroSprite", 1), ZP.F("_Charges", ZhCryCharges1625), ZP.F("_HeadY", 27f), ZP.F("_Form", form), ZP.F("_Pulse", pulse)))
                    for (int k = 0; k < ZhCryCharges1625; k++) ZPx1618(HeroCenter + new Vector2((k - (ZhCryCharges1625 - 1) * 0.5f) * (3f / 16f), 0.95f), 2, ZO, ho + 2); // placeholder charge pips
            }
            // Marked enemies (4 s)
            if (_en != null) for (int i = 0; i < _en.Length; i++)
            {
                var e = _en[i]; if (e == null || !e.alive || e.dying || e.zCry1625 <= runTime) continue; float left = e.zCry1625 - runTime;
                int eo = e.sr != null ? e.sr.sortingOrder + 1 : YSort(e.pos.y) + 1;
                if (!ZhFx1625("ZFX_WarCry_Mark", e.pos, -10, -2, 20, 32, eo, T.wcMarkS1625 - left, (e.id % 7) / 7f, ZP.F("_Intensity", Z01(left / 0.5f))))
                    ZPx1618(e.pos + Vector2.up * (BodySize1595(e).y + 0.2f), 2, ZR, YSort(e.pos.y) + 50); // placeholder doomed mark
            }
        }
        void TickRings1618(float dt)
        {
            for (int i = _zRings1618.Count - 1; i >= 0; i--)
            {
                var r = _zRings1618[i]; r.t += dt; if (r.t >= TZ.srLife1618) { _zRings1618.RemoveAt(i); continue; } float R = TZ.srR1618;
                Query(r.p, R + 0.6f, _zq1618); r.tick -= dt; bool tk = r.tick <= 0f; if (tk) r.tick = TZ.srTick1618;
                foreach (int ei in _zq1618)
                {
                    var e = _en[ei]; if (!e.alive || e.dying) continue; Vector2 d = e.pos - r.p; float nx = d.x / R, ny = d.y / (R * 0.6f + 0.4f); if (nx * nx + ny * ny > 1f) continue;
                    if (!e.boss) { e.slowAmt = e.slowT > 0f ? Mathf.Max(e.slowAmt, TZ.srSlow1618) : TZ.srSlow1618; e.slowT = Mathf.Max(e.slowT, 0.15f); }
                    if (tk) { Hit(e, ZkHit1618 * TZ.srDmg1618, ZK1618, Vector2.zero, true); ZhRingHits1618++; }
                }
                int og = FbGround + YSort(r.p.y) + 6; float a = r.t > TZ.srLife1618 - 0.4f ? (TZ.srLife1618 - r.t) / 0.4f : 1f;
                if (!ZhFx1625("ZFX_SmolderRing", r.p, -50, -28, 100, 52, og, r.t, 0.55f, ZP.F("_Ignite", Z01(r.t / 0.4f)), ZP.F("_Cool", Z01((r.t - (TZ.srLife1618 - 0.5f)) / 0.5f))))
                {   // placeholder: smoky disc + glowing rim + rim embers going round
                    ZDiscO1618(r.p, R, new Color(ZK.r, ZK.g, ZK.b, 0.35f * a), og - 1, 0.6f); ZRingO1618(r.p, R, new Color(ZO.r, ZO.g, ZO.b, 0.9f * a), og, 0.6f);
                    for (int k = 0; k < 10; k++) { float an = k * 36f * Mathf.Deg2Rad + r.t * 0.8f; ZPx1618(r.p + new Vector2(Mathf.Cos(an) * R, Mathf.Sin(an) * R * 0.6f + 0.1f), 2, k % 2 == 0 ? ZY : ZR, og + 1); }
                }
            }
        }
        void ZhHearthClear1618()
        {
            _heroSlowT = 0f; _chill44 = 0; _frozen44 = 0f; _holdT49 = 0f;
            if (_dragBy != null) { if (_dragBy.hookH >= 0) HookChainFX.Release(_dragBy.hookH); _dragBy = null; }
        }
        void TickHearth1618(float dt)
        {
            if (ZhHearthT1618 <= 0f) return; float step = Mathf.Min(dt, ZhHearthT1618); ZhHearthT1618 -= dt;
            if (hp > 0f) hp = Mathf.Min(maxHp, hp + TZ.hbHeal1618 * maxHp * step / Mathf.Max(0.1f, TZ.hbDur1618));
            ZhHearthClear1618(); // window: debuffs are removed and new ones blocked (AddChill44 / ApplyFrostSlow37 return early)
            int ord = YSort(_heroPos.y) + 35;
            float age = TZ.hbDur1618 - ZhHearthT1618, form = Mathf.Min(Z01(age / 0.2f), Z01(ZhHearthT1618 / 0.3f));
            ZhFx1625("ZFX_HearthBlessing_Light", _heroPos, -20, -8, 40, 16, FbGround + YSort(_heroPos.y) + 3, age, 0.6f, ZP.F("_Intensity", form + (age < 0.15f ? 0.5f : 0f))); // GLight (+ the cleanse spike)
            bool shd = ZhFx1625("ZFX_HearthBlessing", _heroPos, -26, -6, 52, 46, ZhHeroOrd1625 - 1, age, 0.6f, ZP.F("_HeroSprite", 1), ZP.F("_Form", form), ZP.F("_Cleanse", 1f - Z01(age / 0.2f)));
            if (!shd)
            {
                ZDiscO1618(_heroPos, 0.7f, new Color(ZA.r, ZA.g, ZA.b, 0.3f), FbGround + YSort(_heroPos.y) + 3, 0.55f);
                for (int k = 0; k < 6; k++) { float ph = Mathf.Repeat(runTime * 0.9f + k / 6f, 1f); ZPx1618(_heroPos + new Vector2(Mathf.Sin(k * 1.7f) * 0.5f, ph * 1.8f), 1, ph < 0.5f ? ZY : ZA, ord); }
            }
        }

        // ---------------------------------------------------------------- ULTIMATES (R; Rebirth is automatic)
        public bool ZhUltArmed1618
        {
            get
            {
                if (ZhUltCd > 0f) return false;
                switch (ZhUlt1618) { case 1: return ZhSunMeter1618 >= TZ.swMeter1618 - 0.01f; case 2: return true; case 3: return ZhBurning1618 >= TZ.mhNeed1618; case 4: return ZhNearMiss1618 >= TZ.vbNeed1618; default: return false; }
            }
        }
        void ZhUltimate1618()
        {
            int u = ZhUlt1618; if (u == 0 || u == 2) return; // Rebirth Flame is not pressed
            if (!ZhUltArmed1618) { HellordAudio.Play("basic_not_ready", 0.6f); return; }
            ZhCasts1618++;
            if (u == 1) { Vector2 at = MouseWorld(); ClampToRoom(ref at, 4); _zSuns1618.Add(new ZSun { p = at, t = 0f }); ZhSunMeter1618 = 0f; ZhUltCd = TZ.swCd1618; HellordAudio.Play("fire_pillar_telegraph", 0.6f); }
            else if (u == 3) { ZhHeartT1618 = TZ.mhDur1618; ZhUltCd = TZ.mhCd1618; HellordAudio.Play("oxhead_roar", 1.3f); ZFx1618(0, _heroPos, 1.8f, 0.4f); Toast("MOLTEN HEART"); }
            else if (u == 4)
            {
                Vector2 d = MouseWorld() - _heroPos; if (d.sqrMagnitude < 0.01f) d = _lastDir; d.Normalize();
                _zBirds1618.Add(new ZBird { a = _heroPos, dir = d, p = _heroPos, d = 0f, drop = 0f }); ZhNearMiss1618 = 0; ZhUltCd = TZ.vbCd1618; HellordAudio.Play("magma_throw_cast", 0.6f); HellordAudio.Play("oxhead_roar", 1.6f);
            }
        }
        /// <summary>HurtHero: a hit would kill you -> Rebirth Flame (if it is the ult and off cooldown): revive at rbHp1618 of max + firestorm.</summary>
        bool ZhRebirth1618()
        {
            if (ZhUlt1618 != 2 || ZhUltCd > 0f) return false;
            hp = maxHp * TZ.rbHp1618; _iframe = Mathf.Max(_iframe, TZ.rbInv1618); ZhUltCd = TZ.rbCd1618; ZhRebirths1618++; ZhCasts1618++;
            Vector2 c = _heroPos; float R = TZ.rbR1618; HellordAudio.Play("fire_pillar_erupt", 0.7f); HellordAudio.Play("pact_unlock", 0.9f); ScreenShake.Add(3f, 0.3f); HitStop.Play(0.1f, 0.2f, 0.4f); Toast("REBIRTH FLAME");
            ZFx1618(-1, c, R, 1.6f, "ZFX_RebirthFlame_Pillar", new Vector4(-30, -14, 60, 104), YSort(c.y) + 30); // t1625 spec: pillar 1.6 s, firestorm from 0.65 s
            After(0.65f, () => ZFx1618(-1, c, R, 1.4f, "ZFX_RebirthFlame_Storm", new Vector4(-62, -32, 124, 72), FbGround + YSort(c.y) + 6, 1));
            if (!ZhFxLive1625("ZFX_RebirthFlame_Pillar")) { ZFx1618(0, c, R, 0.6f); ZFx1618(2, c, R * 0.6f, 0.4f); }
            if (!ZhFxLive1625("ZFX_RebirthFlame_Pillar")) for (int k = 0; k < 8; k++) { float a = k * 45f * Mathf.Deg2Rad; Vector2 q = c + new Vector2(Mathf.Cos(a) * R * 0.6f, Mathf.Sin(a) * R * 0.6f * 0.6f); FireBasicsFX1481.Once("fire_pillar_erupt", q, YSort(q.y) + 5, 0f, 1f, 0, false, k * 1.5f); }
            Query(c, R, _zq1618); foreach (int i in _zq1618) { var e = _en[i]; if (!e.alive || (e.pos - c).magnitude > R) continue; Hit(e, ZkHit1618 * TZ.rbDmg1618, ZK1618, (e.pos - c).normalized * 4f); ZhStormHits1618++; }
            return true;
        }
        /// <summary>Molten Heart: the basic's real cast is followed by a second one mhGap1618 later (its cooldown is left as the first cast set it).</summary>
        void ZhHeartEcho1618(SkillInst s, float mul)
        {
            if (ZhHeartT1618 <= 0f || _zEchoing1618) return;
            After(TZ.mhGap1618, () =>
            {
                if (mode != Mode.Play || !skills.Contains(s)) return; float cd = s.cd; _zEchoing1618 = true; _basicAim1517 = true;
                try { float lc = _lastCast[(int)s.def.id]; Cast(s, mul); if (_lastCast[(int)s.def.id] != lc) ZhHeartEchoes1618++; } finally { _basicAim1517 = false; _zEchoing1618 = false; s.cd = cd; }
            });
        }
        bool _zEchoing1618;
        void ZhDrawHeart1618()
        {
            int ord = YSort(_heroPos.y) + 36; float k = 0.5f + 0.5f * Mathf.Sin(runTime * 14f);
            float age = TZ.mhDur1618 - ZhHeartT1618; int ho = ZhHeroOrd1625; var hp1625 = new[] { ZP.F("_HeroSprite", 1), ZP.F("_Form", Mathf.Min(Z01(age / 0.3f), Z01(ZhHeartT1618 / 0.3f))), ZP.F("_ChestY", 7f) };
            ZhFx1625("ZFX_HearthBlessing_Light", _heroPos, -20, -8, 40, 16, FbGround + YSort(_heroPos.y) + 3, age, 0.7f, ZP.F("_Row", 6f), ZP.F("_Intensity", 0.7f * Mathf.Min(Z01(age / 0.3f), Z01(ZhHeartT1618 / 0.3f))));
            bool shd = ZhFx1625("ZFX_MoltenHeart_Back", _heroPos, -26, -6, 52, 46, ho - 1, age, 0.7f, hp1625); shd &= ZhFx1625("ZFX_MoltenHeart_Front", _heroPos, -26, -6, 52, 46, ho + 1, age, 0.7f, hp1625);
            if (shd) return;
            ZRingO1618(_heroPos, 0.6f + 0.06f * k, new Color(ZR.r, ZR.g, ZR.b, 0.8f), FbGround + YSort(_heroPos.y) + 3, 0.55f);
            ZPx1618(HeroCenter + new Vector2(0f, 0.05f), 2 + Mathf.RoundToInt(k), ZR, ord); // the heart glows on the chest
        }
        void TickSuns1618(float dt)
        {
            for (int i = _zSuns1618.Count - 1; i >= 0; i--)
            {
                var s = _zSuns1618[i]; s.t += dt; float k = Mathf.Clamp01(s.t / Mathf.Max(0.05f, TZ.swFall1618)), R = TZ.swR1618;
                if (k < 1f)
                {
                    int og = FbGround + YSort(s.p.y) + 8; Vector2 sky = s.p + Vector2.up * (9f * (1f - k) * (1f - k));
                    float f = Mathf.Pow(Z01((k - 0.5625f) / 0.4375f), 2f); bool blink = k >= 0.85f && k < 0.9167f || k >= 0.9583f; // spec timeline (0.8 s) scaled to swFall1618
                    if (!ZhFx1625("ZFX_SunsWrath_Telegraph", s.p, -60, -34, 120, 68, og, s.t, 0.8f, ZP.F("_Progress", k), ZP.F("_Shadow", Z01((k - 0.5625f) / 0.4375f)), ZP.F("_Blink", blink ? 1f : 0f))) { ZDiscO1618(s.p, R * (0.3f + 0.7f * k), new Color(ZK.r, ZK.g, ZK.b, 0.25f + 0.35f * k), og, 0.6f); ZRingO1618(s.p, R, new Color(ZR.r, ZR.g, ZR.b, 0.7f), og + 1, 0.6f); }
                    if (!ZhFx1625("ZFX_SunsWrath_Sun", s.p + new Vector2(40f * (1f - f), 12f + 130f * (1f - f)) / 16f, -30, -24, 60, 110, YSort(s.p.y) + 200, s.t, 0.8f, ZP.F("_Fall", f), ZP.V("_Vel", new Vector2(-60f, -360f)))) { ZDiscO1618(sky, 1.1f, new Color(ZO.r, ZO.g, ZO.b, 0.9f), YSort(s.p.y) + 200); ZDiscO1618(sky, 0.8f, ZA, YSort(s.p.y) + 201); ZDiscO1618(sky, 0.5f, ZY, YSort(s.p.y) + 202); }
                    continue;
                }
                _zSuns1618.RemoveAt(i); ScreenShake.Add(4f, 0.35f); HitStop.Play(0.06f, 0.2f, 0.4f); HellordAudio.Play("fireball_impact", 0.5f); HellordAudio.Play("fire_pillar_erupt", 0.6f);
                ZFx1618(-1, s.p, R, 1.0f, "ZFX_SunsWrath_Impact", new Vector4(-60, -30, 120, 100), YSort(s.p.y) + 200, 2); ZFx1618(-1, s.p, R, TZ.swAreaS1618, "ZFX_SunsWrath_Burn", new Vector4(-62, -34, 124, 70), FbGround + YSort(s.p.y) + 6, 1); ZFx1618(-1, s.p, R, TZ.swAreaS1618, "ZFX_SunsWrath_Light", new Vector4(-64, -34, 128, 68), FbGround + YSort(s.p.y) + 8, 3); // t1625 spec hooks
                bool shSun = ZhFxLive1625("ZFX_SunsWrath_Impact");
                if (!shSun) { FireBasicsFX1481.Once("infernal_boulder_end_blast", s.p, YSort(s.p.y) + 30, 0f, 1f, 0, false, 0f, R / 3f, R / 3f); ZFx1618(0, s.p, R * 1.2f, 0.5f); ZFx1618(2, s.p, R, 0.3f); }
                Query(s.p, R + 0.6f, _zq1618); foreach (int ei in _zq1618) { var e = _en[ei]; if (!e.alive) continue; Vector2 d = e.pos - s.p; float nx = d.x / R, ny = d.y / (R * 0.6f + 0.5f); if (nx * nx + ny * ny > 1f) continue; Hit(e, ZkHit1618 * TZ.swDmg1618, ZK1618, d.normalized * 3f); ZhSunHits1618++; }
                FbPatch1481(s.p, TZ.swAreaR1618, TZ.swAreaR1618 * 0.6f, TZ.swAreaS1618, 0f, 0.5f, ZkHit1618 * TZ.swAreaDmg1618, ZK1618); if (!shSun) ZTiles1618(s.p, TZ.swAreaR1618, TZ.swAreaS1618);
            }
        }
        void TickBirds1618(float dt)
        {
            for (int i = _zBirds1618.Count - 1; i >= 0; i--)
            {
                var b = _zBirds1618[i]; float step = TZ.vbSpeed1618 * dt; Vector2 p0 = b.p; b.d += step; b.p = b.a + b.dir * b.d; Vector2 n = new Vector2(-b.dir.y, b.dir.x);
                Query((p0 + b.p) * 0.5f, step * 0.5f + TZ.vbHalfW1618 + 0.8f, _zq1618);
                foreach (int ei in _zq1618) { var e = _en[ei]; if (!e.alive || b.hit.Contains(e)) continue; Vector2 ap = e.pos - p0; float t = Mathf.Clamp01(Vector2.Dot(ap, b.p - p0) / Mathf.Max(1e-4f, (b.p - p0).sqrMagnitude)); if ((p0 + (b.p - p0) * t - e.pos).magnitude > TZ.vbHalfW1618 + e.radius * 0.5f) continue; b.hit.Add(e); Hit(e, ZkHit1618 * TZ.vbDmg1618, ZK1618, b.dir * 3f); ZhBirdHits1618++; }
                b.drop -= step; if (b.drop <= 0f) { b.drop = 1.1f; Vector2 q = b.p; ClampToRoom(ref q, 4); FbPatch1481(q, 0.9f, 0.55f, TZ.vbStripS1618, 0f, 0.5f, ZkHit1618 * TZ.vbStripDmg1618, ZK1618); FireBasicsFX1481.Tile("infernal_boulder_fire_patch", q, FbGround + YSort(q.y), TZ.vbStripS1618); }
                if (b.d >= TZ.vbLen1618) { _zBirds1618.RemoveAt(i); HordeFX.CheapBurst(b.p + Vector2.up * 1.2f, ZO, 14); continue; }
                Vector2 c = b.p + Vector2.up * 1.3f; int ord = YSort(b.p.y) + 220; float deg = Mathf.Atan2(b.dir.y, b.dir.x) * Mathf.Rad2Deg;
                float fl0 = 60f / 16f;
                if (!ZhFx1625("ZFX_VermilionBird", b.p + Vector2.up * (14f / 16f), -36, -28, 72, 56, ord, b.d / TZ.vbSpeed1618, 0.9f, ZP.V("_Dir", b.dir), ZP.F("_Ignite", Z01(b.d / Mathf.Max(0.1f, 0.2f * TZ.vbSpeed1618))), ZP.F("_Fade", Z01((b.d - (TZ.vbLen1618 - fl0)) / fl0)), ZP.F("_FlapHz", 3.2f)))
                {   // placeholder bird: body wedge pointing along the aim + flapping wing bars + tail embers
                    ZWedgeO1618(c, deg, 22f, 1.0f, ZR, ord); ZWedgeO1618(c + b.dir * 0.15f, deg, 14f, 0.8f, ZO, ord + 1); ZWedgeO1618(c + b.dir * 0.3f, deg, 8f, 0.55f, ZY, ord + 2);
                    float fl = Mathf.Sin(b.d * 2.2f) * 0.5f; ZSegO1618(c, c + n * (0.9f + fl) - b.dir * 0.4f, 0.22f, ZR, ord); ZSegO1618(c, c - n * (0.9f - fl) - b.dir * 0.4f, 0.22f, ZR, ord);
                }
                for (int k = 0; k < 4; k++) ZPx1618(c - b.dir * (0.6f + k * 0.35f) + n * Mathf.Sin(b.d * 3f + k) * 0.2f, 2, k < 2 ? ZY : ZO, ord - 1);
            }
        }
        void ZWedgeO1618(Vector2 o, float deg, float half, float reach, Color c, int order) => Z37SR(Z37WedgeSprite(half), ZSnap1618(o), new Vector2(reach * 2f, reach * 2f), deg + 180f, c, order - Z37Order); // tip forward: the wedge opens backwards
        void ZSegO1618(Vector2 a, Vector2 b, float w, Color c, int order) { Vector2 d = b - a; float len = d.magnitude; if (len < 0.01f) return; Z37SR(HArt.Pixel(), (a + b) * 0.5f, new Vector2(len * HArt.PPU, w * HArt.PPU), Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, c, order - Z37Order); }

        // ---------------------------------------------------------------- HYDRA (LMB basic; t1625, D2-style, replaced Volcanic Orb; timing = HFX fx_v2 spec §14)
        /// <summary>Places a hydra at the cursor (clamped to hyRange1625). At the cap the oldest one burns out to make room.</summary>
        bool CastHydra1625(SkillInst s, float dmg)
        {
            if (mode == Mode.StartPick) return false; var T = TZ;
            Vector2 to = MouseWorld(), d = to - _heroPos; if (d.magnitude > T.hyRange1625) to = _heroPos + d.normalized * T.hyRange1625; ClampToRoom(ref to, 4);
            while (_zHy1625.Count >= Mathf.Max(1, T.hyMax1625)) { var o = _zHy1625[0]; _zHy1625.RemoveAt(0); ZhHydraGone1625(o); }
            var h = new ZHy { p = to, t = 0f, dmg = dmg, s = s, seed = Random.value }; for (int i = 0; i < 3; i++) h.next[i] = T.hyFirst1625 + i * 0.25f; _zHy1625.Add(h);
            After(0.18f, () => ZFx1618(-1, to, 0.5f, 0.45f, "ZFX_Hydra_Erupt", new Vector4(-28, -18 + 3, 56, 50), YSort(to.y) + 200)); // summon eruption t 0.18-0.63
            HellordAudio.Play("magma_throw_cast", 0.9f); if (!ZhFxLive1625("ZFX_Hydra2")) HordeFX.CheapBurst(to + Vector2.up * 0.3f, ZO, 6); return true;
        }
        void ZhHydraGone1625(ZHy o)
        {   // fade-out end (or replaced): the scorch cools over 1.4 s from fade + 0.3 s
            ZFx1618(-1, o.p, 0.5f, 1.7f, "ZFX_Hydra_Scorch", new Vector4(-22, -12, 44, 28), FbGround + YSort(o.p.y) + 2, 4); _zFx1618[_zFx1618.Count - 1].seed = o.seed; // t1647 §17b: the scorch uses the Hydra's own seed, so its edge matches the Hydra2 pool
            if (!ZhFxLive1625("ZFX_Hydra2")) HordeFX.CheapBurst(o.p + Vector2.up * 0.5f, ZO, 8); HellordAudio.Play("magma_throw_splat", 1.3f);
        }
        static readonly Vector2[] ZHyB1625 = { new Vector2(-5, 3), new Vector2(0, 4), new Vector2(5, 3) }, ZHyR1625 = { new Vector2(-15, 17), new Vector2(1, 25), new Vector2(15, 19) }; // t1632 Hydra v2 rest points (§14b)
        static float ZEo(float x) => 1f - (1f - x) * (1f - x); static float ZEi(float x) => x * x;
        /// <summary>HFX's HydraMouthPx, v2 (spec §14b, mirrors the Hydra2 shader pose): the jaw of head i in art px from the pivot.</summary>
        static Vector2 HydraMouthPx1625(int i, float fxTime, float rise, float fade, float spitTime, float aim, float W = 0.27f)
        {
            float t = Mathf.Floor(fxTime * 15f) / 15f, s = t - spitTime;
            Vector2 ad = new Vector2(Mathf.Cos(aim), Mathf.Sin(aim)); float face = ad.x >= 0 ? 1 : -1;
            float tilt = Mathf.Floor(Mathf.Clamp(Mathf.Atan2(ad.y, Mathf.Abs(ad.x)), -0.6f, 0.4f) / 0.2f + 0.5f) * 0.2f;
            Vector2 hd = new Vector2(face * Mathf.Cos(tilt), Mathf.Sin(tilt)), yax = new Vector2(-hd.y * face, hd.x * face);
            float st = i == 0 ? 0.08f : (i == 1 ? 0f : 0.15f);
            float r = ZEo(Mathf.Clamp01((rise - 0.3f - st) / 0.5f)) * (1 - 0.55f * ZEi(Mathf.Clamp01(fade / 0.8f)));
            Vector2 sway = new Vector2(Mathf.Sin(t * 1.6f + i * 2.1f) * 1.5f, Mathf.Sin(t * 2.2f + i * 1.3f));
            Vector2 H = ZHyB1625[i] + (ZHyR1625[i] - ZHyB1625[i]) * r + sway * r;
            float wu = (s >= 0 && s < W) ? s / W : 0, lg = (s >= W && s < W + 0.1f) ? 1 : ((s >= W + 0.1f && s < W + 0.45f) ? 1 - ZEo((s - W - 0.1f) / 0.35f) : 0);
            H += -hd * 3f * ZEo(wu) + Vector2.up * wu + hd * 4f * lg;
            H = new Vector2(Mathf.Floor(H.x + 0.5f), Mathf.Floor(H.y + 0.5f));
            return H + hd * 7.5f - yax * 2.5f; // art px from the pivot
        }
        const float ZHyW1625 = 0.27f;
        float ZHyRise1625(ZHy o) => Z01(o.t / 0.7f);
        float ZHyFade1625(ZHy o) => Z01((o.t - (TZ.hyLife1625 - 0.6f)) / 0.6f);
        void UpdateHydras1625(float dt)
        {
            var T = TZ;
            for (int i = _zHy1625.Count - 1; i >= 0; i--)
            {
                var o = _zHy1625[i]; o.t += dt;
                if (o.t >= T.hyLife1625) { _zHy1625.RemoveAt(i); ZhHydraGone1625(o); continue; }
                bool fading = o.t >= T.hyLife1625 - 0.6f;
                for (int h = 0; h < 3; h++)
                {
                    if (o.armed[h] && o.t - o.spitT[h] >= ZHyW1625)
                    {   // 0.27 s after the windup started: the bolt leaves the jaw
                        o.armed[h] = false; Vector2 m = o.p + HydraMouthPx1625(h, o.t, ZHyRise1625(o), ZHyFade1625(o), o.spitT[h], o.aim[h]) / 16f; Vector2 dir = new Vector2(Mathf.Cos(o.aim[h]), Mathf.Sin(o.aim[h]));
                        _zBolt1625.Add(new ZBolt { p = m, v = dir * T.hyBoltSpeed1625, t = 0f, life = T.hySight1625 / Mathf.Max(0.1f, T.hyBoltSpeed1625), dmg = o.dmg * T.hyBoltDmg1625, s = o.s, seed = Random.value });
                        ZhBoltsShot1625++; HellordAudio.Play("fireball_cast", 1.6f);
                    }
                    if (fading || o.armed[h] || o.t < o.next[h]) continue;
                    // this head picks its own target: the nearest enemy in sight not already taken by another head (else the nearest)
                    Vector2 mouth = o.p + HydraMouthPx1625(h, o.t, ZHyRise1625(o), 0f, -10f, o.aim[h]) / 16f; Enemy best = null, any = null; float bd = T.hySight1625, ad = T.hySight1625;
                    Query(o.p, T.hySight1625, _zq1618);
                    foreach (int ei in _zq1618)
                    {
                        var e = _en[ei]; if (!e.alive || e.dying) continue; float dd = (e.pos - o.p).magnitude; if (dd > T.hySight1625) continue;
                        if (dd < ad) { ad = dd; any = e; }
                        bool taken = false; for (int k = 0; k < 3; k++) if (k != h && o.armed[k] && _zHyTgt1625[k] == e) taken = true;
                        if (!taken && dd < bd) { bd = dd; best = e; }
                    }
                    best = best ?? any; if (best == null) { o.next[h] = o.t + 0.1f; continue; }
                    _zHyTgt1625[h] = best; Vector2 to = BodyCentre1595(best) - mouth; o.aim[h] = Mathf.Atan2(to.y, to.x); o.spitT[h] = o.t; o.armed[h] = true;
                    o.next[h] = o.t + Random.Range(T.hyHeadMin1625, T.hyHeadMax1625);
                }
                ZhDrawHydra1625(o);
            }
            for (int i = _zBolt1625.Count - 1; i >= 0; i--)
            {
                var e = _zBolt1625[i]; Vector2 a = e.p; e.p += e.v * dt; e.t += dt; bool hit = false;
                _zq1618.Clear(); QueryBody1595(a, e.p, T.hyBoltR1625, _zq1618);
                foreach (int ei in _zq1618) { var en = _en[ei]; if (!en.alive || en.dying) continue; Hit(en, e.dmg, e.s, e.v.normalized * 0.4f); ZhBoltHits1625++; hit = true; break; }
                if (hit || e.t >= e.life)
                {   // pop on hit or at max range
                    ZFx1618(-1, e.p, 0.3f, 0.3f, "ZFX_Hydra_BoltPop", new Vector4(-16, -12, 32, 30), YSort(e.p.y - 0.6f) + 200); if (hit && !ZhFxLive1625("ZFX_Hydra_BoltPop")) FireBasicsFX1481.Once("molten_spit_pop_v1", e.p, YSort(e.p.y) + 40);
                    _zBolt1625.RemoveAt(i); continue;
                }
                int ord = YSort(e.p.y - 0.6f) + 200; Vector2 dn = e.v.normalized;
                if (!ZhFx1625("ZFX_Hydra_Bolt", e.p, -16, -16, 32, 32, ord, e.t, e.seed, ZP.V("_Dir", dn), ZP.F("_TravelPx", e.t * e.v.magnitude * 16f)))
                {   // placeholder fire bolt: 2 px hot head + 2 cooler tail pixels
                    ZPx1618(e.p, 2, ZY, ord); ZPx1618(e.p - dn * (2.5f / 16f), 1, ZO, ord - 1); ZPx1618(e.p - dn * (4.5f / 16f), 1, ZR, ord - 1);
                }
            }
        }
        readonly Enemy[] _zHyTgt1625 = new Enemy[3];
        void ZhDrawHydra1625(ZHy o)
        {
            var T = TZ; float rise = ZHyRise1625(o), fade = ZHyFade1625(o); int ord = YSort(o.p.y);
            float flash = 0f; for (int h = 0; h < 3; h++) { float sd = o.t - o.spitT[h] - ZHyW1625; if (sd >= 0f && sd < 0.15f) flash = Mathf.Max(flash, 1f - sd / 0.15f); }
            float breath = 0.5f + 0.5f * Mathf.Sin(2.6f * o.t), summon = Mathf.Exp(-Mathf.Abs(o.t - 0.3f) / 0.1f);
            ZhFx1625("ZFX_Hydra_Light", o.p + Vector2.up / 16f, -26, -12, 52, 24, FbGround + YSort(o.p.y) + 3, o.t, o.seed, ZP.F("_Intensity", (0.55f + 0.15f * breath + 0.6f * flash + 0.6f * summon) * (1f - fade)));
            if (ZhFx1625("ZFX_Hydra2", o.p, -36, -10, 72, 50, ord, o.t, o.seed, ZP.F("_Rise", rise), ZP.F("_Fade", fade), ZP.F("_Windup", ZHyW1625),
                ZP.V4("_SpitTime", new Vector4(o.spitT[0], o.spitT[1], o.spitT[2], 0f)), ZP.V4("_AimAngle", new Vector4(o.aim[0], o.aim[1], o.aim[2], 0f)))) return;
            // placeholder: scorched base + 3 necks / heads at HFX's mouth positions
            float g = Mathf.Max(0.15f, rise) * (1f - fade);
            ZDiscO1618(o.p, 0.42f * g, new Color(ZK.r, ZK.g, ZK.b, 0.55f), FbGround + YSort(o.p.y) + 2, 0.5f); ZRingO1618(o.p, 0.42f * g, new Color(ZD.r, ZD.g, ZD.b, 0.8f), FbGround + YSort(o.p.y) + 3, 0.5f);
            for (int k = 0; k < 3; k++)
            {
                Vector2 hp = o.p + HydraMouthPx1625(k, o.t, rise, fade, o.spitT[k], o.aim[k]) / 16f; bool sp = flash > 0f && o.t - o.spitT[k] < ZHyW1625 + 0.15f;
                ZSegO1618(o.p + Vector2.up * 0.1f, hp, 0.16f, ZD, ord + k); ZDiscO1618(hp, 0.13f, sp ? ZO : ZR, ord + 4 + k); ZPx1618(hp + new Vector2(0f, 0.03f), 1, sp ? ZW : ZY, ord + 8 + k);
            }
        }

        // t1641: the HUD readout (DrawKit1618 / KitLine1618 debug text) was removed; the kit shows in the 4-slot skill bar (HellordKitBar1641.cs). Icon keys:
        string MoveKey1618() => ZhMove1618 == 1 ? "embereffigy" : ZhMove1618 == 2 ? "emberanchor" : ZhMove1618 == 3 ? "backdraft" : null;
        string SpecKey1618() => ZhSpec1618 == 1 ? "pyremark" : ZhSpec1618 == 2 ? "flammability" : ZhSpec1618 == 3 ? "warcry" : ZhSpec1618 == 4 ? "smolderring" : ZhSpec1618 == 5 ? "hearthblessing" : null;
        string UltKey1618() => ZhUlt1618 == 1 ? "sunswrath" : ZhUlt1618 == 2 ? "rebirthflame" : ZhUlt1618 == 3 ? "moltenheart" : ZhUlt1618 == 4 ? "vermilionbird" : null;
    }
}
