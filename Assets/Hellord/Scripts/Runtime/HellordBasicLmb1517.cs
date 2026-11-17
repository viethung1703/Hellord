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
            var s = skills[0]; return T.lmbBasicIds1517 != null && System.Array.IndexOf(T.lmbBasicIds1517, s.def.id) >= 0 ? s : null;
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
            if (_lastCast[(int)s.def.id] != lc) { Vector2 d = MouseWorld() - HeroCenter; HeroAttackPose(IsMeleeBasic1517(s.def.id) ? AttackPoseKind1517.Swing : AttackPoseKind1517.Cast, d.sqrMagnitude > 0.01f ? d.normalized : _lastDir); }
        }

        // ---------------------------------------------------------------- hero attack pose hook (t1517 point 4)
        AttackPoseKind1517 _poseKind1517; Vector2 _poseDir1517 = Vector2.right; float _poseT1517 = 99f; Sprite[] _poseCast1517, _poseSwing1517, _poseCastOl1517, _poseSwingOl1517; bool _poseTried1517; int _poseCalls1517;
        /// <summary>Attack override on the hero at fire time. Plays HA's strips Resources/HellordArt/Hero36/hero_cast_east_pl.png (spell / projectile basics)
        /// or hero_melee_east_pl.png (melee basics): 7 frames of 36x36 in one row, pivot (18,35), authored facing east (flipX when the cursor is west),
        /// release at index 3. The pose starts at heroPoseStartIdx (2) on the fire frame so the shot stays instant. Recoil plays only without the strips.</summary>
        public void HeroAttackPose(AttackPoseKind1517 kind, Vector2 dir)
        {
            _poseCalls1517++; if (!HellordTuning.I.heroPose1517) return; _poseKind1517 = kind; _poseDir1517 = dir.sqrMagnitude > 0.0001f ? dir.normalized : Vector2.right; _poseT1517 = 0f; _atkFaceT1517 = Mathf.Max(_atkFaceT1517, HellordTuning.I.lmbFaceHoldS1517);
            // show the first pose frame this frame (UpdateHero already ran before the skills), so the pose lands on the fire frame
            int pi = HeroPose1517(out var pf, out var po); if (pi < 0 || _heroBase == null) return; bool flip = HeroFaceCursor1517() ?? _heroBase.flipX;
            _heroBase.sprite = pf[pi]; _heroBase.flipX = flip; if (_heroOutline != null && po != null) { _heroOutline.sprite = po[pi]; _heroOutline.flipX = flip; }
            foreach (var l in _heroLayers) { if (l.f != null && l.f.Length > 0) l.sr.sprite = l.f[0]; l.sr.flipX = flip; }
        }
        void PoseLoad1517()
        {
            if (_poseTried1517) return; _poseTried1517 = true; var T = HellordTuning.I; int n = Mathf.Max(1, T.heroPoseFrames1517), c = T.hero36CellPx; var pv = T.hero36PivotPx;
            string cs = Art47 + T.heroPoseCastSheet1517, ms = Art47 + T.heroPoseMeleeSheet1517;
            if (!HArt.Missing(cs)) { _poseCast1517 = HArt.Strip(cs, n, c, c, pv); _poseCastOl1517 = HArt.HeroOutline(cs, c, c, pv.x, pv.y, n); }
            if (!HArt.Missing(ms)) { _poseSwing1517 = HArt.Strip(ms, n, c, c, pv); _poseSwingOl1517 = HArt.HeroOutline(ms, c, c, pv.x, pv.y, n); }
        }
        bool PoseSheets1517 { get { PoseLoad1517(); return (_poseKind1517 == AttackPoseKind1517.Swing ? _poseSwing1517 : _poseCast1517) != null; } }
        /// <summary>Recoil offset for the hero transform this frame (px table at 30 fps, away from the attack), only while no pose strip exists (or heroRecoilWithStrips).</summary>
        Vector3 HeroRecoil1517()
        {
            var T = HellordTuning.I; var tab = T.heroRecoilPx1517; int fi = (int)(_poseT1517 * 30f); if (!T.heroPose1517 || tab == null || fi < 0 || fi >= tab.Length) return Vector3.zero;
            if (Hero47On && PoseSheets1517 && !T.heroRecoilWithStrips1517) return Vector3.zero;
            Vector2 o = -_poseDir1517 * tab[fi]; return new Vector3(Mathf.Round(o.x), Mathf.Round(o.y), 0f) / HArt.PPU;
        }
        /// <summary>The pose frame index this frame (-1 = no pose: none running, no strip, 32 px hero or a transformation), with its frames and outline.</summary>
        int HeroPose1517(out Sprite[] frames, out Sprite[] outline)
        {
            frames = outline = null; var T = HellordTuning.I; if (!T.heroPose1517 || !Hero47On || _poseT1517 > 5f) return -1; PoseLoad1517();
            bool sw = _poseKind1517 == AttackPoseKind1517.Swing; frames = sw ? _poseSwing1517 : _poseCast1517; outline = sw ? _poseSwingOl1517 : _poseCastOl1517; if (frames == null || frames.Length == 0) return -1;
            int i = Mathf.Clamp(T.heroPoseStartIdx1517, 0, frames.Length - 1) + (int)(_poseT1517 * T.heroPoseFps1517); return i < frames.Length ? i : -1;
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
