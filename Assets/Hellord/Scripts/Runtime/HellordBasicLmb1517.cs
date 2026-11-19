using UnityEngine;

namespace Hellord
{
    // =====================================================================================================================
    // t1517 (VH "go with all 4"): the run's BASIC MAIN ATTACK is player-controlled in normal mode too.
    //  * Basic = skills[0] (the start pick = belt slot 1) when its id is in HellordTuning.lmbBasicIds1517 (aimed attacks only).
    //    It fires only while LMB is held (never auto-fires), aimed at the cursor; every other slot keeps auto-casting as before.
    //    Manual mode (manualControl720) is unchanged: its own key bindings decide (Want716).
    //  * The hero faces the cursor while attacking; HeroAttackPose(kind, dir) runs at fire time (recoil now, pose sheets when HA delivers).
    //  * Cooldown read-out: a pixel ring around the cursor drains while the basic is on cooldown and pops when ready, the belt icon
    //    sweeps from the first frame, and clicking during the cooldown plays a short dull "not ready" click (code-synth PLACEHOLDER).
    // =====================================================================================================================
    public partial class HellordGame
    {
        public enum AttackPoseKind1517 { Cast, Swing }
        SkillInst _basic1517; bool _basicAim1517, _lmbPrev1517; float _atkFaceT1517, _notReadyT1517, _readyFlash1517, _basicCdPrev1517;

        /// <summary>The run's basic main attack (null = none: Manual mode, toggle off, or the start pick is not an aimed attack).</summary>
        SkillInst Basic1517()
        {
            var T = HellordTuning.I; if (T == null || !T.lmbBasic1517 || Manual716 || skills.Count == 0) return null;
            var s = skills[0]; return IsLmbBasic1563(s.def.id) ? s : null; // t1563: + Chain Lightning (chainLmb1563)
        }
        bool LmbFree1517() => mode == Mode.Play && !_tmOpen && !MouseOverBelt716() && _pickCool716 <= 0f;
        bool BasicWant1517() => HInput.LmbHeld716() && LmbFree1517();
        static bool IsMeleeBasic1517(SkillId id) => id == SkillId.FrostCleave || id == SkillId.BloodWhip;

        /// <summary>Called at the top of UpdateSkills: picks the basic, keeps the hero facing the cursor while LMB is held, plays the
        /// not-ready click on a press during the cooldown, arms the ready pop.</summary>
        void BasicTick1517(float dt)
        {
            var T = HellordTuning.I; _basic1517 = Basic1517(); _atkFaceT1517 -= dt; _notReadyT1517 -= dt; _readyFlash1517 -= dt; _poseT1517 += dt;
            bool lmb = HInput.LmbHeld716(), down = lmb && !_lmbPrev1517; _lmbPrev1517 = lmb;
            var b = _basic1517 ?? FbLmbBasic1517(); if (b == null) return;
            if (lmb && LmbFree1517()) _atkFaceT1517 = Mathf.Max(_atkFaceT1517, T.lmbFaceHoldS1517);
            PreWind1528(b, lmb && LmbFree1517()); // t1528 tail-cast wind-up while held
            if (down && LmbFree1517() && b.cd > T.lmbNotReadyMinS1517 && _notReadyT1517 <= 0f) { _notReadyT1517 = T.lmbNotReadyGapS1517; HellordAudio.Play("basic_not_ready"); }
            float cdNow = b.cd - dt; // the skill loop subtracts dt after this tick; with LMB held the basic re-fires that same frame, so judge the pop on cd - dt
            _basicCdPrev1517 = Mathf.Max(_basicCdPrev1517, b.cd); // peak of the running cooldown
            if (cdNow <= 0f && _basicCdPrev1517 > 0f) { if (_basicCdPrev1517 > T.lmbReadyMinCdS1517) _readyFlash1517 = T.lmbReadyFlashS1517; _basicCdPrev1517 = 0f; }
        }
        /// <summary>The LMB fire basic (Molten Spit / Infernal Boulder / Fire Pillar, t1481) when one is slot 1, for the cursor ring.</summary>
        SkillInst FbLmbBasic1517() { if (Manual716 || skills.Count == 0 || !IsFb1481(skills[0].def.id)) return null; return skills[0]; }
        SkillInst RingSkill1517() => _basic1517 ?? FbLmbBasic1517();

        /// <summary>Casts the basic aimed at the cursor; on a real cast: pose + recoil + face the cursor.</summary>
        void BasicCast1517(SkillInst s, float mul)
        {
            float lc = _lastCast[(int)s.def.id]; _basicAim1517 = true;
            try { Cast(s, mul); } finally { _basicAim1517 = false; }
            if (_lastCast[(int)s.def.id] != lc) { _poseLight1567 = CastLight1567(s.def.id); _pose1567 = PoseSetFor1567(s.def.id); Vector2 d = MouseWorld() - HeroCenter; HeroAttackPose(AttackPoseKind1517.Cast, /* t1528: melee uses the same tail cast (t1519) */ d.sqrMagnitude > 0.01f ? d.normalized : _lastDir, CastFrost1528(s.def.id)); }
        }

        // ---------------------------------------------------------------- hero attack pose (t1517 hook; t1528: VH's cast variant B "TAIL cast")
        // Body hero_castB_tail_body_36 (5 frames 36x36, pivot 18,35, east, flipX west), blink hero_castB_tail_blink_36 (the release frame lit,
        // first 60 ms of f2), aura fx_castB_aura_<el>_44 (4 frames 44x44 BEHIND the body, pivot 22,39, frames f0-f3), spark fx_cast_spark_<el>_13
        // (3 frames 13x13 on f2-f4 at the tail tip, above the projectile). Holds 70/90/100/80/70 ms, release f2 at +160 ms.
        // Click-to-fire stays instant: the shot fires on the click frame and the pose JUMPS to the release frame (f2). While LMB is held and
        // the next shot is <160 ms away, f0-f1 (lift + squash/curl) pre-play so the release lands on the fire frame (castPreWind1528).
        // Every left-click basic uses it (melee too, t1519). The PixelLab strips hero_cast/melee_east_pl.png are no longer loaded.
        AttackPoseKind1517 _poseKind1517; Vector2 _poseDir1517 = Vector2.right; float _poseT1517 = 99f; bool _poseTried1517, _preWind1528, _poseFrost1528; int _poseCalls1517;
        int _walkF1528 = -1; Sprite[] _castWalk1528, _castWalkOl1528; // t1528 v2: walk-based cast frames (tools/gen_castB_walk_1528.py)
        Sprite[] _castBody1528, _castBodyOl1528, _castBlink1528, _auraFire1528, _auraFrost1528, _sparkFire1528, _sparkFrost1528; SpriteRenderer _aura1528, _spark1528;
        float CastReleaseS1528 { get { var h = PHolds1567; int r = PRel1567; float t = 0; for (int i = 0; i < r && i < h.Length; i++) t += h[i]; return t / 1000f; } }
        float CastTotalS1528 { get { float t = 0; foreach (var x in PHolds1567) t += x; return t / 1000f; } }
        bool _poseLight1567; Sprite[] _sparkLight1567, _auraLight1567;
        bool CastLight1567(SkillId id) { var a = HellordTuning.I.castLightningIds1567; return a != null && System.Array.IndexOf(a, id) >= 0; }
        bool CastFrost1528(SkillId id) { var a = HellordTuning.I.castFrostIds1528; return a != null && System.Array.IndexOf(a, id) >= 0; }
        /// <summary>Starts the cast on the fire frame (jumps to / snaps to the release frame so the shot stays instant).</summary>
        public void HeroAttackPose(AttackPoseKind1517 kind, Vector2 dir) => HeroAttackPose(kind, dir, _poseFrost1528);
        public void HeroAttackPose(AttackPoseKind1517 kind, Vector2 dir, bool frost)
        {
            _poseCalls1517++; var T = HellordTuning.I; if (!T.heroPose1517) return; _poseKind1517 = kind; _poseFrost1528 = frost; _poseDir1517 = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right;
            _poseT1517 = CastReleaseS1528; _preWind1528 = false; _atkFaceT1517 = Mathf.Max(_atkFaceT1517, T.lmbFaceHoldS1517);
            // show the release frame this frame (UpdateHero already ran before the skills), so it lands on the fire frame
            var ps = HeroPose1517(out var po); if (ps == null || _heroBase == null) return; bool flip = HeroFaceCursor1517() ?? _heroBase.flipX;
            _heroBase.sprite = ps; _heroBase.flipX = flip; if (_heroOutline != null && po != null) { _heroOutline.sprite = po; _heroOutline.flipX = flip; }
            int lf = _walkF1528 >= 0 && PWalk1567 != null ? HArt.HeroWalk0 + _walkF1528 : 0; foreach (var l in _heroLayers) { if (l.f != null && l.f.Length > lf) l.sr.sprite = l.f[lf]; l.sr.flipX = flip; }
            UpdateCastFx1528(flip, _heroBase.sortingOrder);
        }
        /// <summary>BasicTick: while LMB is held and the basic's next shot is under the release lead, pre-play f0-f1 so f2 meets the shot.</summary>
        void PreWind1528(SkillInst b, bool lmbFree)
        {
            var T = HellordTuning.I; float rel = CastReleaseS1528;
            if (_preWind1528 && !lmbFree) { _preWind1528 = false; _poseT1517 = 99f; return; } // released during the wind-up: drop it
            if (!T.castPreWind1528 || !T.heroPose1517 || !lmbFree || b != _basic1517 || _preWind1528 || _poseT1517 < CastTotalS1528) return;
            if (b.cd > 0.0001f && b.cd <= rel) { _poseLight1567 = CastLight1567(b.def.id); _pose1567 = PoseSetFor1567(b.def.id); _preWind1528 = true; _poseFrost1528 = CastFrost1528(b.def.id); _poseKind1517 = AttackPoseKind1517.Cast; _poseT1517 = Mathf.Max(0f, rel - b.cd); }
        }
        void PoseLoad1517()
        {
            if (_poseTried1517) return; _poseTried1517 = true; var T = HellordTuning.I; int c = T.hero36CellPx; var pv = T.hero36PivotPx;
            string body = Art47 + T.castBodySheet1528, blink = Art47 + T.castBlinkSheet1528; int n = Mathf.Max(1, T.castHoldMs1528.Length);
            if (!HArt.Missing(body)) { _castBody1528 = HArt.Strip(body, n, c, c, pv); _castBodyOl1528 = HArt.HeroOutline(body, c, c, pv.x, pv.y, n); }
            if (!HArt.Missing(blink)) _castBlink1528 = HArt.Strip(blink, 1, c, c, pv);
            string walk = Art47 + T.castWalkSheet1528; int wn = 8 * (n + 1); if (T.castWalk1528 && !HArt.Missing(walk)) { _castWalk1528 = HArt.Strip(walk, wn, c, c, pv); _castWalkOl1528 = HArt.HeroOutline(walk, c, c, pv.x, pv.y, wn); }
            Sprite[] L(string nm, int fr, int px, Vector2 piv) => HArt.Missing(Art47 + nm) ? null : HArt.Strip(Art47 + nm, fr, px, px, piv);
            _auraFire1528 = L(T.castAuraFireSheet1528, 4, 44, T.castAuraPivotPx1528); _auraFrost1528 = L(T.castAuraFrostSheet1528, 4, 44, T.castAuraPivotPx1528);
            _sparkFire1528 = L(T.castSparkFireSheet1528, 3, 13, new Vector2(6.5f, 6.5f)); _sparkFrost1528 = L(T.castSparkFrostSheet1528, 3, 13, new Vector2(6.5f, 6.5f));
            _sparkLight1567 = L(T.castSparkLightningSheet1567, 3, 13, new Vector2(6.5f, 6.5f)); _auraLight1567 = L(T.castAuraLightningSheet1567, 4, 44, T.castAuraPivotPx1528); // t1567 HFX / HA lightning
        }
        bool PoseSheets1517 { get { PoseLoad1517(); return _castBody1528 != null; } }
        /// <summary>Recoil offset (px table at 30 fps, away from the attack); only when the cast strip is missing (or heroRecoilWithStrips).</summary>
        Vector3 HeroRecoil1517()
        {
            var T = HellordTuning.I; var tab = T.heroRecoilPx1517; int fi = (int)((_poseT1517 - CastReleaseS1528) * 30f); if (!T.heroPose1517 || _preWind1528 || tab == null || fi < 0 || fi >= tab.Length) return Vector3.zero;
            if (Hero47On && PoseSheets1517 && !T.heroRecoilWithStrips1517) return Vector3.zero;
            Vector2 o = -_poseDir1517 * tab[fi]; return new Vector3(Mathf.Round(o.x), Mathf.Round(o.y), 0f) / HArt.PPU;
        }
        /// <summary>Cast frame index now (-1 = none). A pre-wind that reached the release without a shot (e.g. Boulder's one-ball cap) holds f1, then drops.</summary>
        int CastIdx1528()
        {
            var T = HellordTuning.I; if (!T.heroPose1517 || !Hero47On || _poseT1517 > 5f) return -1; var h = PHolds1567; float rel = CastReleaseS1528;
            if (_preWind1528 && _poseT1517 >= rel) { if (_poseT1517 > rel + T.castPreWindMaxHoldS1528) { _preWind1528 = false; _poseT1517 = 99f; return -1; } return Mathf.Max(0, PRel1567 - 1); }
            float t = _poseT1517 * 1000f; for (int i = 0; i < h.Length; i++) { if (t < h[i]) return i; t -= h[i]; } return -1;
        }
        /// <summary>The cast sprite replacing the hero base this frame (null = none), with its outline.</summary>
        Sprite HeroPose1517(out Sprite outline)
        {
            outline = null; int i = CastIdx1528(); if (i < 0) return null; PoseLoad1517(); var body = PBody1567; if (body == null || i >= body.Length) return null;
            var T = HellordTuning.I; bool blink = i == PRel1567 && !_preWind1528 && _poseT1517 - CastReleaseS1528 < T.castBlinkMs1528 / 1000f;
            int per = body.Length + 1; var walk = PWalk1567; // t1528 v2: walking -> the walk frame's legs + bob with the cast tail / squash / blink (w * 6 + k, k 5 = blink)
            if (_walkF1528 >= 0 && walk != null) { var wol = PWalkOl1567; int wi = _walkF1528 * per + (blink ? per - 1 : i); if (wi < walk.Length) { outline = wol != null && wi < wol.Length ? wol[wi] : null; return walk[wi]; } }
            var bol = PBodyOl1567; outline = bol != null && i < bol.Length ? bol[i] : null; var bl = PBlink1567;
            if (blink && bl != null) return bl[0];
            return body[i];
        }
        /// <summary>Aura (behind the body, f0-f3) and spark (tail tip, f2-f4, above the projectiles), element-tinted. Called from UpdateHero.</summary>
        void UpdateCastFx1528(bool flip, int ord)
        {
            if (_hero == null) return; var T = HellordTuning.I; int i = CastIdx1528(); PoseLoad1517();
            if (_aura1528 == null && _auraFire1528 != null) { _aura1528 = new GameObject("castAura1528").AddComponent<SpriteRenderer>(); _aura1528.transform.SetParent(_hero, false); }
            if (_spark1528 == null && _sparkFire1528 != null) { _spark1528 = new GameObject("castSpark1528").AddComponent<SpriteRenderer>(); _spark1528.transform.SetParent(_hero, false); }
            var au = _poseFrost1528 ? _auraFrost1528 : _auraFire1528; var sp = _poseFrost1528 ? _sparkFrost1528 : _sparkFire1528;
            if (_poseLight1567 && T.castLightning1567) { if (_auraLight1567 != null) au = _auraLight1567; if (_sparkLight1567 != null) sp = _sparkLight1567; } // t1567
            if (_pose1567 != null) { au = _pose1567.aura ?? au; sp = _pose1567.noSpark ? null : (_pose1567.spark ?? sp); } // t1567 per-skill pose set (HA)
            if (_aura1528 != null) { bool on = T.castAura1528 && au != null && i >= 0 && i < au.Length; _aura1528.enabled = on; if (on) { _aura1528.sprite = au[i]; _aura1528.flipX = flip; _aura1528.sortingOrder = ord - 3; } }
            if (_spark1528 != null)
            {
                int si = i - PRel1567; bool on = T.castSpark1528 && sp != null && !_preWind1528 && si >= 0 && si < sp.Length; _spark1528.enabled = on;
                if (on) { _spark1528.sprite = sp[si]; _spark1528.flipX = flip; _spark1528.sortingOrder = T.castSparkSort1528; var o = T.castTipPx1528; _spark1528.transform.localPosition = new Vector3((flip ? -o.x : o.x) / HArt.PPU, -o.y / HArt.PPU, 0f); }
            }
        }
        /// <summary>World position of the tail tip for the current facing (pivot + castTipPx, y px down, mirrored on flipX).</summary>
        Vector2 TailTip1528(bool west) { var o = HellordTuning.I.castTipPx1528; return new Vector2(Snap(_heroPos.x), Snap(_heroPos.y)) + new Vector2((west ? -o.x : o.x) / HArt.PPU, -o.y / HArt.PPU); }
        /// <summary>Projectile origin for a left-click basic: the tail tip, and dir re-aimed from the tip to the cursor. Other casts keep 'fallback'.</summary>
        Vector2 CastOrigin1528(Vector2 fallback, ref Vector2 dir)
        {
            var T = HellordTuning.I; if (!_basicAim1517 || !T.castFromTail1528 || !T.heroPose1517 || !Hero47On) return fallback;
            Vector2 m = MouseWorld(); bool west = m.x < _heroPos.x; Vector2 tip = TailTip1528(west), d = m - tip; if (d.magnitude > T.castReaimMinU1528) dir = d.normalized; return tip;
        }
        /// <summary>Facing override: true / false = face left / right (the cursor side) while attacking; null = the normal facing.</summary>
        bool? HeroFaceCursor1517() => _atkFaceT1517 > 0f ? MouseWorld().x < _heroPos.x : (bool?)null;

        // ---------------------------------------------------------------- cursor cooldown ring (HUD canvas, 1 px = 1 art px)
        Texture2D[] _ringTex1517; Texture2D _ringIdle1517, _ringPop1517; int _ringR1517 = -1;
        static Color32 Q1517(uint c, byte a = 255) => new Color32((byte)(c >> 16), (byte)(c >> 8), (byte)c, a);
        void RingTex1517()
        {
            var T = HellordTuning.I; int r = Mathf.Clamp(T.lmbRingRadiusPx1517, 3, 16), nf = Mathf.Clamp(T.lmbRingFrames1517, 8, 48); if (_ringTex1517 != null && _ringR1517 == r && _ringTex1517.Length == nf + 1) return;
            _ringR1517 = r; int s = r * 2 + 5; _ringTex1517 = new Texture2D[nf + 1];
            Texture2D Mk(System.Func<float, float, Color32> px)
            {   // ring texels: 1 px thick at radius r (midpoint distance in [r-0.5, r+0.5)), plus the same at r+1 for a 2 px band
                var t = new Texture2D(s, s, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp }; var a = new Color32[s * s];
                for (int y = 0; y < s; y++) for (int x = 0; x < s; x++)
                {
                    float dx = x + 0.5f - s * 0.5f, dy = s * 0.5f - (y + 0.5f), d = Mathf.Sqrt(dx * dx + dy * dy); float ang = Mathf.Atan2(dx, dy); if (ang < 0) ang += Mathf.PI * 2;
                    a[y * s + x] = px(d, ang / (Mathf.PI * 2));
                }
                t.SetPixels32(a); t.Apply(false, true); return t;
            }
            Color32 clear = new Color32(0, 0, 0, 0);
            for (int f = 0; f <= nf; f++)
            {   // frame f: f/nf of the cooldown still to go = lit arc clockwise from 12 o'clock (Endesga f77622 on a 181425 shadow ring); the rest dim 3a4466
                float left = f / (float)nf;
                _ringTex1517[f] = Mk((d, fr) => d >= r - 0.5f && d < r + 0.5f ? (fr < left ? Q1517(0xf77622) : Q1517(0x3a4466, 170)) : d >= r + 0.5f && d < r + 1.5f ? Q1517(0x181425, 200) : clear);
            }
            _ringIdle1517 = Mk((d, fr) => d >= r - 0.5f && d < r + 0.5f && ((int)(fr * 16f) & 1) == 0 ? Q1517(0xead4aa, 150) : clear);                          // ready: dashed light ring
            _ringPop1517 = Mk((d, fr) => d >= r - 0.5f && d < r + 1.5f ? Q1517(0xfee761) : clear);                                                              // ready pop: 2 px bright ring
        }
        /// <summary>Draws the cooldown ring around the cursor (HUD canvas). Call inside the HUD OnGUI after the belt.</summary>
        void DrawCursorRing1517()
        {
            var T = HellordTuning.I; if (!T.lmbRing1517 || mode != Mode.Play || _cam == null) return; var s = RingSkill1517(); if (s == null) return; RingTex1517();
            Vector2 c = WorldToHud(MouseWorld()); c = new Vector2(Mathf.Round(c.x), Mathf.Round(c.y)); int r = _ringR1517, sz = r * 2 + 5; var g = GUI.color;
            float full = Mathf.Max(0.05f, s.def.cooldown * s.CdMul), left = Mathf.Clamp01(s.cd / full);
            if (_readyFlash1517 > 0f)
            {   // pop: the bright ring grows 0 -> +2 px and fades over lmbReadyFlashS
                float k = 1f - _readyFlash1517 / Mathf.Max(0.01f, T.lmbReadyFlashS1517); int grow = Mathf.RoundToInt(k * 2f); GUI.color = new Color(1, 1, 1, 1f - k * 0.7f);
                GUI.DrawTexture(new Rect(c.x - sz / 2f - grow, c.y - sz / 2f - grow, sz + grow * 2, sz + grow * 2), _ringPop1517);
            }
            else if (s.cd > 0.001f) { int nf = _ringTex1517.Length - 1; GUI.color = Color.white; GUI.DrawTexture(new Rect(c.x - sz / 2f, c.y - sz / 2f, sz, sz), _ringTex1517[Mathf.Clamp(Mathf.CeilToInt(left * nf), 0, nf)]); }
            else if (T.lmbRingIdle1517) { GUI.color = Color.white; GUI.DrawTexture(new Rect(c.x - sz / 2f, c.y - sz / 2f, sz, sz), _ringIdle1517); }
            GUI.color = g;
        }
        /// <summary>Belt: the basic's icon sweeps from the first frame of its cooldown (other slots keep the 0.15 s threshold) and flashes when ready.</summary>
        bool BeltSweep1517(SkillInst k) => k != null && k == RingSkill1517() && k.cd > 0.001f;
        float BeltReady1517(SkillInst k) => k != null && k == RingSkill1517() && _readyFlash1517 > 0f ? _readyFlash1517 / Mathf.Max(0.01f, HellordTuning.I.lmbReadyFlashS1517) : 0f;
    }
}
