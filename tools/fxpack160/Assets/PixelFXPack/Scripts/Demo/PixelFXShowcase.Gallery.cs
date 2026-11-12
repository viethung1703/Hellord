using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelFXPack.Demo
{
    // v1.3.1 gallery mode: G toggles. Plays 10 short gameplay vignettes (animated knight: idle/run/jump/attack/hurt/dead
    // frames from PixelFXAnimFrames) that together use all 54 effects; each vignette lists its effects on screen.
    // Left/Right step. Fullscreen effects (CRT, Screen Shockwave/Chromatic, Heat Haze) need the URP Full Screen Pass
    // Renderer Feature. UNTESTED IN EDITOR (compile-checked only).
    public partial class PixelFXShowcase
    {
        class Vignette { public string name; public string[] effects; public float secs; public Func<IEnumerator> play; }
        readonly List<Vignette> _g = new List<Vignette>();
        bool _gallery; int _gi; float _gT;
        Transform _gk, _gs; SpriteRenderer _gkSR, _gsSR; PixelFX _gkFX, _gsFX; string _gkDefault, _gsDefault;
        readonly List<UnityEngine.Object> _gTemp = new List<UnityEngine.Object>();
        readonly List<Coroutine> _gCo = new List<Coroutine>();
        readonly Dictionary<string, Sprite> _frames = new Dictionary<string, Sprite>();
        string _kState = "idle"; float _kAnimT; bool _kFlip;
        public float galleryEntrySeconds = 5f; // minimum; a vignette's own length wins if longer
        const int GScale = 2;
        Color cyan = new Color(0.17f, 0.91f, 0.96f), amber = new Color(1f, 0.68f, 0.2f), poison = new Color(0.39f, 0.78f, 0.3f);

        void V(string name, float secs, Func<IEnumerator> play, params string[] fx)
            => _g.Add(new Vignette { name = name, effects = fx, secs = secs, play = play });
        T Temp<T>(T o) where T : UnityEngine.Object { _gTemp.Add(o); return o; }
        void Co(IEnumerator r) { _gCo.Add(StartCoroutine(r)); }
        Vector3 Feet(float x) => P(x, GY);
        Vector3 ChestOf(Transform t) => t.position + Vector3.up * 10 * GScale * U;
        float ppu => pixelsPerUnit;

        void BuildGallery()
        {
            foreach (var n in new[] { "idle0", "idle1", "run0", "run1", "run2", "run3", "jump", "fall", "atk0", "atk1", "atk2", "hurt", "dead" })
                _frames[n] = PixelSpriteFactory.FromRows(PixelFXAnimFrames.Get(n), PixelFXDemo.Pal, pixelsPerUnit, 2, "Knight_" + n);
            _gkSR = Character("GalleryKnight", PixelFXAnimFrames.Idle0, out _gk, out _gkFX, 10);
            _gsSR = Character("GallerySlime", PixelFXDemo.Slime, out _gs, out _gsFX, 9);
            _gk.localScale = _gs.localScale = new Vector3(GScale, GScale, 1);
            _gkDefault = JsonUtility.ToJson(_gkFX); _gsDefault = JsonUtility.ToJson(_gsFX);
            _gk.gameObject.SetActive(false); _gs.gameObject.SetActive(false);

            V("Loot pickup", 4, V1, "Outline", "Inner Outline", "Shine Sweep", "Emission / Glow", "Squash & Stretch", "Light Pop", "Sparkle", "Dust");
            V("Combo on a slime", 5, V2, "Slash Arc", "Hit Flash", "Hit-Stop", "Screen Shake", "Damage Numbers", "Hit Sparks", "Shockwave Ring");
            V("Dash finisher", 4, V3, "Afterimage Trail", "Motion Blur", "Impact Frame", "Pixel Dissolve", "Embers");
            V("Status effects", 5, V4, "Palette Swap", "Frozen", "Tint Pulse", "Burn", "Embers");
            V("Power-up", 6, V5, "Heal", "Level-Up", "Buff Aura", "Rainbow", "Glow Sparks", "Shine Sweep", "Light Pop");
            V("Waterside", 5, V6, "Water Reflection", "Wave", "Dither Fade", "Sparkle");
            V("Windy meadow camp", 5, V7, "Wind Sway", "Pixel Glow", "Heat Haze*", "Drop Shadow", "Rim Light");
            V("Ruins: see-through & teleport", 5, V8, "Silhouette", "Hologram", "Teleport Beam", "Pixelate", "Chromatic Aberration");
            V("Boss fight", 5, V9, "Lightning", "Invincibility Blink", "Hue Shift", "Negative", "Glitch", "Screen Shockwave*", "Screen Chromatic*", "Explosion");
            V("Hellord horde (v1.4.1)", 9, V11, "Lava Ground", "Spawn Portal", "Aura Field", "Orbit Weapons", "Rune Circle", "Hellfire Nova", "Chain Lightning", "Horde Flash/Dissolve", "Blood & Bone + Magnet", "Blood Stream", "Soul Stream", "Telegraph", "Meteor Rain", "Crit Pop", "Kill-Streak Pop", "Demon Evolution", "Level-Up Burst", "Heat Distortion*");
            V("Hellord projectiles (v1.5)", 11, V12, "Fireball", "Blood Bolt", "Bone Shard", "Soul Orb", "Curse Skull", "Hellfire Spear", "Lightning Bolt", "Magic Missile", "Returning Blade", "Piercing Beam", "Bouncing", "Splitting", "Cast Flash", "Element Trails", "Impacts", "Pierce/Bounce/Split Hits", "Spread/Ring/Spiral/Homing");
            V("Chains & poison (v1.5.1)", 6, V13, "Chain Lightning Jump", "Chain Whip Sweep", "Poison Puddle", "Hit Spark");
            V("Frost & reactions (v1.6)", 7, V14, "Frost Cleave", "Shock Status", "Overload", "Bleed Status", "Frost Shatter", "Elite Chest Open");
            V("Game over", 5, V10, "Gradient Map", "Greyscale", "Sepia", "CRT*");
        }

        // ---------------- building blocks ----------------
        void KAnim()
        {
            _kAnimT += Time.deltaTime; string f;
            switch (_kState)
            {
                case "idle": f = Mathf.FloorToInt(_kAnimT / 0.47f) % 2 == 0 ? "idle0" : "idle1"; break;
                case "run": f = "run" + (Mathf.FloorToInt(_kAnimT / 0.11f) % 4); break;
                default: f = _kState; break;
            }
            if (_frames.TryGetValue(f, out var s) && _gkSR.sprite != s) _gkSR.sprite = s;
            _gkSR.flipX = _kFlip;
        }
        void K(string state) { if (_kState != state) { _kState = state; _kAnimT = 0; } }
        IEnumerator Wait(float s) { float t = 0; while (t < s) { t += Time.deltaTime; yield return null; } }
        IEnumerator RunTo(Transform t, float xPx, float pxPerSec, bool knight = true, bool dust = true)
        {
            if (knight) K("run");
            float dustT = 0;
            while (true)
            {
                float cur = t.position.x / U + referenceResolution.x * 0.5f, step = pxPerSec * Time.deltaTime;
                if (Mathf.Abs(xPx - cur) <= step) break;
                t.position = new Vector3(t.position.x + Mathf.Sign(xPx - cur) * step * U, t.position.y, 0);
                if (dust && (dustT += Time.deltaTime) > 0.25f) { dustT = 0; PixelParticles.Burst(PixelBurstSettings.Dust(), t.position); }
                yield return null;
            }
            t.position = Snap(new Vector3((xPx - referenceResolution.x * 0.5f) * U, t.position.y, 0));
            if (knight) K("idle");
        }
        IEnumerator Jump(Transform t, float heightPx, float secs, float dxPx = 0, bool knight = true)
        {
            Vector3 a = t.position; float tt = 0;
            if (knight) K("jump");
            while (tt < secs)
            {
                tt += Time.deltaTime; float k = Mathf.Clamp01(tt / secs);
                if (knight && k > 0.5f) K("fall");
                t.position = Snap(a + new Vector3(dxPx * k * U, 4 * heightPx * k * (1 - k) * U, 0));
                yield return null;
            }
            t.position = Snap(a + new Vector3(dxPx * U, 0, 0));
            var sq = t.GetComponent<SquashStretch>(); if (sq) sq.Punch(-0.3f, 0.2f);
            PixelParticles.Burst(PixelBurstSettings.Dust(), t.position);
            if (knight) K("idle");
        }
        IEnumerator Hop(Transform t, float secs, float period, float heightPx, float dxPxPerHop)
        {
            float end = Time.time + secs;
            while (Time.time < end) yield return Jump(t, heightPx, period * 0.7f, dxPxPerHop, false);
        }
        IEnumerator Attack(Transform target, PixelFX targetFX, int dmg, bool crit)
        {
            K("atk0"); yield return Wait(0.09f);
            K("atk1"); PixelFXSpawn.Slash(ChestOf(_gk) + new Vector3((_kFlip ? -14 : 14) * U, 0, 0), _kFlip, 0.035f, 18, 70, ppu);
            if (target) Hit(target, targetFX, dmg, crit);
            yield return Wait(0.07f); K("atk2"); yield return Wait(0.16f); K("idle");
        }
        void Hit(Transform target, PixelFX targetFX, int dmg, bool crit)
        {
            var hf = targetFX.GetComponent<HitFlash>(); if (hf) hf.Play();
            HitStop.Play(crit ? 0.14f : 0.07f, 0.4f, 0.15f);
            ScreenShake.Add(crit ? 6 : 3, 0.25f); if (ScreenShake.Main) ScreenShake.Main.Kick(new Vector2(_kFlip ? -2 : 2, 0));
            DamageNumbers.Spawn(ChestOf(target) + Vector3.up * 8 * U, dmg, crit ? new Color32(254, 231, 97, 255) : new Color32(255, 255, 255, 255), 90, ppu, crit);
            PixelParticles.Burst(PixelBurstSettings.HitSparks(), ChestOf(target));
            var sq = target.GetComponent<SquashStretch>(); if (sq) sq.Punch(-0.35f, 0.2f);
            if (crit) PixelFXSpawn.Shockwave(ChestOf(target), 30, 0.35f, 60, ppu);
        }
        Transform Prop(string n, string[] rows, float xPx, float yPx, int order, out PixelFX fx, int scale = GScale)
        {
            var root = Temp(new GameObject(n)).transform; root.SetParent(transform, false);
            var sr = new GameObject(n + "Sprite").AddComponent<SpriteRenderer>(); sr.transform.SetParent(root, false);
            sr.sprite = Temp(PixelSpriteFactory.FromRows(rows, PixelFXDemo.Pal, pixelsPerUnit, 2, n));
            sr.transform.localPosition = new Vector3(0, (rows.Length * 0.5f) * U, 0);
            if (uberMaterial != null) sr.sharedMaterial = uberMaterial;
            sr.sortingOrder = order; fx = sr.gameObject.AddComponent<PixelFX>();
            root.localScale = new Vector3(scale, scale, 1); root.position = Snap(P(xPx, yPx));
            return root;
        }
        Transform Slime(string n, float xPx, out PixelFX fx)
        {
            var sr = Character(n, PixelFXDemo.Slime, out var root, out fx, 9); Temp(root.gameObject);
            sr.color = Color.white; root.localScale = new Vector3(GScale, GScale, 1); root.position = Snap(Feet(xPx));
            root.gameObject.AddComponent<SquashStretch>(); Temp(fx.gameObject.AddComponent<HitFlash>()).duration = 0.12f;
            return root;
        }
        void PlaceKnight(float xPx) { _gk.position = Snap(Feet(xPx)); _kFlip = false; K("idle"); }
        PaletteSwap AddPalettes(PixelFX fx)
        {
            var ps = fx.gameObject.AddComponent<PaletteSwap>(); ps.enabled = false;
            var Pal = PixelFXDemo.Pal;
            ps.sourceColors = new Color[] { Pal['L'], Pal['l'], Pal['g'], Pal['G'], Pal['h'] };
            Color[] Cs(params Color32[] c) { var r = new Color[c.Length]; for (int i = 0; i < c.Length; i++) r[i] = c[i]; return r; }
            ps.palettes = new[]
            {
                new PaletteSwap.Palette { name = "Fire", colors = Cs(new Color32(254, 231, 97, 255), new Color32(247, 118, 34, 255), new Color32(228, 59, 68, 255), new Color32(162, 38, 51, 255), new Color32(62, 39, 49, 255)) },
                new PaletteSwap.Palette { name = "Ice", colors = Cs(new Color32(255, 255, 255, 255), new Color32(44, 232, 245, 255), new Color32(0, 153, 219, 255), new Color32(18, 78, 137, 255), new Color32(24, 20, 37, 255)) },
                new PaletteSwap.Palette { name = "Void", colors = Cs(new Color32(246, 117, 122, 255), new Color32(181, 80, 136, 255), new Color32(104, 56, 108, 255), new Color32(62, 39, 49, 255), new Color32(24, 20, 37, 255)) },
            };
            ps.enabled = true; return ps;
        }
        IEnumerator Ramp(Action<float> set, float d, bool pingPong = false)
        {
            float t = 0;
            while (t < d) { t += Time.deltaTime; set(Mathf.Floor(Mathf.Clamp01(t / d) * 12) / 12); yield return null; }
            if (!pingPong) yield break;
            t = 0;
            while (t < d) { t += Time.deltaTime; set(1 - Mathf.Floor(Mathf.Clamp01(t / d) * 12) / 12); yield return null; }
            set(0);
        }
        IEnumerator After(float s, Action a) { yield return new WaitForSeconds(s); a(); }

        // ---------------- vignettes ----------------
        IEnumerator V1() // loot pickup
        {
            PlaceKnight(24);
            var gem = Prop("Gem", PixelFXAnimFrames.Gem, 150, GY - 14, 11, out var gfx);
            Temp(gfx.gameObject.AddComponent<ShineSweep>()).loopInterval = 0.9f;
            gfx.emissionAmount = 1.1f; gfx.emissionColor = cyan;
            gem.gameObject.AddComponent<SquashStretch>();
            Co(Bob(gem, 2, 4));
            Co(Every(0.35f, () => { if (gem) PixelParticles.Burst(PixelBurstSettings.Sparkle(), ChestOf(gem)); }));
            Co(RunTo(_gk, 100, 70));
            yield return Wait(1.0f);
            gfx.outlineSize = 1; gfx.outlineColor = Color.white; gfx.outlineDiagonals = true; // hover / in range
            yield return RunTo(_gk, 136, 70);
            gfx.innerOutline = true; gfx.outlineColor = amber;
            Co(Jump(_gk, 14, 0.5f));
            yield return Wait(0.22f);
            gem.GetComponent<SquashStretch>().Punch(0.4f, 0.15f);
            PixelFXSpawn.LightPop(ChestOf(gem), cyan, 40, 0.25f, 45, ppu);
            PixelParticles.Burst(PixelBurstSettings.Sparkle(), ChestOf(gem));
            yield return Ramp(v => { if (gem) gem.localScale = new Vector3(GScale * (1 - v), GScale * (1 + v * 0.5f), 1); }, 0.25f);
            gem.gameObject.SetActive(false);
        }
        IEnumerator V2() // combo
        {
            PlaceKnight(20); var s = Slime("Slime", 190, out var sfx);
            Co(Hop(s, 1.1f, 0.4f, 10, -14));
            yield return RunTo(_gk, 110, 80);
            int[] dmg = { 12, 17, 48 };
            for (int i = 0; i < 3; i++)
            {
                float sx = s.position.x / U + referenceResolution.x * 0.5f;
                if (sx - 36 > _gk.position.x / U + referenceResolution.x * 0.5f) yield return RunTo(_gk, sx - 36, 90);
                yield return Attack(s, sfx, dmg[i], i == 2);
                Co(Knock(s, i == 2 ? 26 : 10));
                yield return Wait(0.35f);
            }
        }
        IEnumerator V3() // dash finisher
        {
            PlaceKnight(30); var s = Slime("Slime", 182, out var sfx);
            var dis = Temp(sfx.gameObject.AddComponent<DissolveFX>()); dis.duration = 0.9f;
            sfx.dissolveEdgeColor = amber; sfx.dissolveEdgeWidth = 0.1f; sfx.dissolveEdgeGlow = 2f;
            Co(Hop(s, 0.6f, 0.4f, 8, 0));
            yield return Wait(0.6f);
            var trail = Temp(_gkSR.gameObject.AddComponent<AfterimageTrail>()); trail.interval = 0.03f; trail.lifetime = 0.25f; trail.emitting = true;
            _gkFX.motionBlur = 3; _gkFX.motionBlurDirection = new Vector2(1, 0);
            yield return RunTo(_gk, 150, 260, true, true);
            trail.emitting = false; _gkFX.motionBlur = 0;
            ImpactFrame.Play(_cam, 0.07f, sfx);
            yield return Attack(s, sfx, 99, true);
            dis.PlayOut();
            for (int i = 0; i < 6; i++) { PixelParticles.Burst(PixelBurstSettings.Embers(), s.position); yield return Wait(0.12f); }
        }
        IEnumerator V4() // status effects
        {
            PlaceKnight(36);
            var a = Slime("FireSlime", 150, out var afx); var b = Slime("IceSlime", 190, out var bfx); var c = Slime("PoisonSlime", 228, out var cfx);
            var ps = AddPalettes(afx); Temp(ps);
            bfx.frozenAmount = 1; cfx.pulseAmount = 0.7f; cfx.pulseColor = poison;
            Co(Hop(a, 2.2f, 0.32f, 10, -8)); Co(Hop(b, 5f, 0.9f, 5, -4)); Co(Hop(c, 5f, 0.45f, 10, -9));
            Co(Every(0.15f, () => { if (a && afx.burnAmount < 1) PixelParticles.Burst(PixelBurstSettings.Embers(), a.position + Vector3.up * 14 * U); }));
            Co(Every(0.5f, () => { if (c) DamageNumbers.Spawn(ChestOf(c) + Vector3.up * 10 * U, 1, new Color32(99, 199, 77, 255), 90, ppu, false); }));
            yield return Wait(2.3f);
            float ax = a.position.x / U + referenceResolution.x * 0.5f;
            yield return RunTo(_gk, ax - 30, 120);
            yield return Attack(a, afx, 35, false);
            yield return Ramp(v => afx.burnAmount = v, 1.1f);
        }
        IEnumerator V5() // power-up
        {
            PlaceKnight(110);
            PixelVFX.Heal(_gk.position, 75, ppu);
            DamageNumbers.Spawn(ChestOf(_gk) + Vector3.up * 14 * U, 25, new Color32(99, 199, 77, 255), 90, ppu, false);
            yield return Wait(1.4f);
            PixelVFX.LevelUp(_gk.position, 75, ppu); Co(Jump(_gk, 6, 0.35f));
            yield return Wait(0.5f);
            Temp(PixelVFX.BuffAura(_gk, amber, 12, 10, 74, ppu));
            yield return Wait(0.9f);
            // v1.4: a visible star pickup instead of an invisible "star power"
            var star = Prop("Star", PixelFXAnimFrames.Star, 160, GY - 22, 40, out var sfx);
            sfx.outlineSize = 1; sfx.outlineColor = Color.white; sfx.emissionColor = amber; sfx.emissionAmount = 0.4f;
            Co(Bob(star, 2f, 2.5f));
            Co(Every(0.25f, () => { if (star) PixelParticles.Burst(PixelBurstSettings.Sparkle(), star.position + new Vector3(0, 10 * U, 0)); }));
            Co(Every(1.1f, () => { if (sfx) Co(Ramp(v => { if (sfx) sfx.shinePosition = v; }, 0.4f)); }));
            yield return RunTo(_gk, 150, 60);
            var at = star.position + new Vector3(0, 10 * U, 0);
            star.gameObject.SetActive(false);
            PixelFXSpawn.LightPop(at, amber, 40, 0.3f, 41, ppu); PixelFXSpawn.Shockwave(at, 36, 0.35f, 60, ppu);
            PixelParticles.Burst(PixelBurstSettings.GlowSparks(), at); HitStop.Play(0.06f);
            _gkFX.rainbow = 0.8f; // star power
            Co(Every(0.2f, () => PixelParticles.Burst(PixelBurstSettings.GlowSparks(), ChestOf(_gk))));
            yield return RunTo(_gk, 200, 70);
        }
        IEnumerator V6() // waterside (the scene's floor stands in for the dock; reflection line = feet)
        {
            PlaceKnight(20);
            var refl = Temp(_gkSR.gameObject.AddComponent<WaterReflection>()); refl.waterLineY = Feet(0).y;
            var under = Slime("Underwater", 190, out var ufx); under.position = Snap(P(190, GY + 20)); ufx.waveAmplitude = 2; ufx.waveFrequency = 0.8f; ufx.waveSpeed = 6;
            // v1.4: clearly visible underwater slime - stronger tint, dark outline, rising bubbles
            ufx.GetComponent<SpriteRenderer>().color = new Color(0.55f, 0.85f, 1f, 0.9f); ufx.outlineSize = 1; ufx.outlineColor = new Color(0.05f, 0.15f, 0.35f, 1f);
            Co(Hop(under, 4.5f, 1.3f, 2, 0));
            Co(Every(0.35f, () => { if (under) PixelParticles.Burst(new PixelBurstSettings { count = 1, speed = new Vector2(0.6f, 1.2f), spreadDegrees = 20, directionDegrees = 90, lifetime = new Vector2(0.5f, 0.9f), sizePixels = new Vector2Int(1, 2), sway = 4f, shrink = false, colorOverLife = PixelBurstSettings.DefaultGradient(Color.white, new Color(0.5f, 0.85f, 1f)) }, under.position + new Vector3(0, 6 * U, 0)); }));
            var wisp = Slime("Wisp", 160, out var wfx); AddPalettes(wfx).Next(); wfx.ditherFade = 1;
            Co(Every(0.3f, () => PixelParticles.Burst(PixelBurstSettings.Sparkle(), P(UnityEngine.Random.Range(100f, 236f), GY + 8))));
            Co(RunTo(_gk, 125, 45));
            yield return Wait(1.6f);
            yield return Ramp(v => wfx.ditherFade = 1 - v, 1.3f);
            Co(Hop(wisp, 1.2f, 0.9f, 4, 0));
            yield return Wait(0.6f);
            yield return Ramp(v => wfx.ditherFade = v, 1.0f);
        }
        IEnumerator V7() // windy meadow camp
        {
            PlaceKnight(10);
            _gkFX.shadowOffset = new Vector2Int(2, -2);
            _gkFX.rimAmount = 1; _gkFX.rimColor = amber; _gkFX.rimDirection = new Vector2Int(1, 0);
            // v1.4: denser, taller two-layer grass with gusts (was a barely visible sway)
            var tufts = new List<PixelFX>();
            for (float x = 4; x < 240; x += 8) { Prop("GrassBack", PixelFXAnimFrames.TallGrass, x, GY - 2, 4, out var g); g.windSpeed = 3.2f; tufts.Add(g); }
            for (float x = 9; x < 240; x += 11) { Prop("GrassFront", PixelFXAnimFrames.TallGrass, x, GY + 3, 60, out var g); g.windSpeed = 3.8f; tufts.Add(g); }
            var glow = Temp(new GameObject("Campfire")).AddComponent<PixelGlow>(); glow.transform.position = P(196, GY - 8);
            glow.pixelsPerUnit = ppu; glow.radiusPixels = 56; glow.color = amber; glow.flicker = 0.2f;
            Co(Every(0.08f, () => PixelParticles.Burst(PixelBurstSettings.Embers(), P(196, GY - 6))));
            Vector3 c0 = _cam.WorldToViewportPoint(P(180, GY - 55)), c1 = _cam.WorldToViewportPoint(P(212, GY - 6));
            PixelScreenFX.SetHeatHaze(new Rect(c0.x, c1.y, c1.x - c0.x, c0.y - c1.y), 2f);
            Co(Every(0f, () =>
            {
                float gust = 0.22f + 0.2f * Mathf.Max(0, Mathf.Sin(Time.time * 1.7f)); // rolling gusts
                foreach (var g in tufts) { float d = g.transform.position.x - _gk.position.x; g.windAmount = Mathf.Abs(d) < 12 * U * GScale ? 0.5f * Mathf.Sign(d) : gust; }
            }));
            Co(Every(0.15f, () => PixelParticles.Burst(new PixelBurstSettings { count = 1, speed = new Vector2(2f, 3.5f), spreadDegrees = 20, directionDegrees = 10, lifetime = new Vector2(0.8f, 1.4f), sizePixels = new Vector2Int(1, 1), sway = 6f, shrink = false, colorOverLife = PixelBurstSettings.DefaultGradient(new Color32(99, 199, 77, 255), new Color32(38, 92, 66, 255)) }, P(0, GY - UnityEngine.Random.Range(10f, 50f)))));
            yield return RunTo(_gk, 160, 45);
        }
        IEnumerator V8() // ruins
        {
            PlaceKnight(14);
            var see = Temp(_gkSR.gameObject.AddComponent<SeeThrough>()); // silhouette while behind the foreground pillar
            var pillarRows = new string[60]; for (int i = 0; i < 60; i++) pillarRows[i] = i < 3 ? "DDDDDDDDDDDDDDDDDDDDDD" : ".dKKKKKKKKKKKKKKKKKKK.";
            Prop("Pillar", pillarRows, 92, GY, 30, out _, 1);
            var holo = Slime("Hologram", 205, out var hfx); holo.position = Snap(P(205, GY - 14)); hfx.hologramAmount = 1;
            Co(Bob(holo, 1.5f, 3));
            yield return RunTo(_gk, 150, 70);
            yield return Wait(0.3f);
            PixelVFX.TeleportBeam(_gk.position, cyan, 80, 0.9f, 75, ppu);
            yield return Ramp(v => { _gkFX.pixelateSize = 1 + Mathf.FloorToInt(v * 3); _gkFX.chromaticOffset = Mathf.Round(v * 3); }, 0.8f);
            _gkSR.enabled = false; yield return Wait(0.2f);
            PlaceKnight(40); _gkSR.enabled = true;
            PixelVFX.TeleportBeam(_gk.position, cyan, 80, 0.7f, 75, ppu);
            yield return Ramp(v => { _gkFX.pixelateSize = 4 - Mathf.FloorToInt(v * 3); _gkFX.chromaticOffset = Mathf.Round((1 - v) * 3); }, 0.7f);
            _gkFX.pixelateSize = 0; _gkFX.chromaticOffset = 0;
            if (see) see.enabled = true;
        }
        IEnumerator V9() // boss
        {
            PlaceKnight(50);
            var boss = Slime("Boss", 180, out var bfx); boss.localScale = new Vector3(4, 4, 1);
            var crown = Prop("Crown", PixelFXAnimFrames.Crown, 0, 0, 10, out _, 4); crown.SetParent(boss, true); crown.position = boss.position + Vector3.up * 19 * 4 * U;
            var blink = Temp(_gkSR.gameObject.AddComponent<InvincibilityBlink>());
            Co(Every(0f, () => { if (bfx) bfx.hueShift = (bfx.hueShift + Time.deltaTime * 0.25f) % 1f; }));
            yield return Wait(0.8f);
            PixelVFX.Lightning(_gk.position + Vector3.up * 6, ChestOf(_gk), cyan, 0.3f, 80, ppu);
            yield return Wait(0.3f);
            K("hurt"); ScreenShake.Add(4, 0.2f); DamageNumbers.Spawn(ChestOf(_gk) + Vector3.up * 10 * U, 8, new Color32(255, 255, 255, 255), 90, ppu, false);
            blink.Play(1.4f);
            yield return Wait(0.4f);
            yield return RunTo(_gk, 110, 90);
            Co(Jump(_gk, 30, 0.75f, 30));
            yield return Wait(0.35f);
            yield return Attack(boss, bfx, 120, true);
            bfx.negative = 1; Co(After(0.12f, () => bfx.negative = 0));
            PixelScreenFX.Shockwave(_cam, ChestOf(boss), 1f, 0.5f, 0.5f, 0.06f); PixelScreenFX.ChromaticPulse(3, 0.35f);
            yield return Wait(0.6f);
            bfx.glitchAmount = 0.7f;
            for (int i = 0; i < 3; i++)
            {
                PixelParticles.Burst(PixelBurstSettings.Explosion(), ChestOf(boss) + new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), UnityEngine.Random.Range(-0.5f, 1.5f), 0));
                ScreenShake.Add(3, 0.2f); PixelScreenFX.ChromaticPulse(2, 0.2f);
                yield return Wait(0.27f);
            }
            bfx.glitchAmount = 0;
        }
        bool _gameOver, _gameOverText;
        IEnumerator V11() // v1.4 hell survivor showcase (top-down-ish, on the gallery stage)
        {
            PlaceKnight(-80);
            HellFX.PPU = ppu; HordeFX.I.pixelsPerUnit = ppu;
            var lava = Temp(HellFX.LavaGround(P(120, 68), 5, 3, 8f, -50));
            var demon = Temp(new GameObject("Demon")).transform; demon.SetParent(transform, false);
            var dsr = demon.gameObject.AddComponent<SpriteRenderer>(); dsr.sprite = Temp(PixelSpriteFactory.FromRows(HordeStressTest.Demon, PixelFXDemo.Pal, ppu, 2, "Demon"));
            if (uberMaterial) dsr.sharedMaterial = uberMaterial; dsr.sortingOrder = 50;
            var dfx = demon.gameObject.AddComponent<PixelFX>(); dfx.outlineSize = 1; dfx.outlineColor = HellPalette.Black;
            demon.localScale = new Vector3(2, 2, 1); demon.position = Snap(P(120, 70));
            var heat = demon.gameObject.AddComponent<HeatDistortionZone>(); heat.pixelsPerUnit = ppu; heat.amplitudePixels = 1f;
            HellFX.AuraField(demon, 40, HellPalette.Crimson, 8.5f, 0.4f);
            HellFX.OrbitWeapons(demon, PixelFXTextures.Glow(4, 3, ppu), 3, 34, 0.7f, 8.5f, HellPalette.Ember);
            var imps = new List<SpriteRenderer>(); var gems = new List<HellFXRunner.Job>();
            var impSprite = Temp(PixelSpriteFactory.FromRows(new[] { ".k....k.", "kRk..kRk", "kRRkkRRk", "kRyRRyRk", "kRRRRRRk", ".kRrrRk.", "kRk..kRk", "kk....kk" }, PixelFXDemo.Pal, ppu, 1, "Imp"));
            for (int i = 0; i < 28; i++)
            {
                float a = i / 28f * Mathf.PI * 2; var at = P(120 + Mathf.Cos(a) * 105, 70 + Mathf.Sin(a) * 55);
                HellFX.SpawnPortal(at, 7, 0.9f);
                var sr = Temp(new GameObject("imp")).AddComponent<SpriteRenderer>(); sr.sprite = impSprite; sr.sortingOrder = 30; sr.transform.position = Snap(at);
                sr.transform.localScale = new Vector3(2, 2, 1); HordeFX.Prepare(sr); imps.Add(sr);
            }
            float t0 = Time.time; int kills = 0;
            void KillNear(Vector3 c, float r)
            {
                foreach (var sr in imps)
                {
                    if (!sr || !sr.gameObject.activeSelf || (sr.transform.position - c).sqrMagnitude > r * r) continue;
                    HordeFX.Hit(sr, UnityEngine.Random.Range(12, 30)); var at = sr.transform.position;
                    HordeFX.Kill(sr); kills++; var g = kills % 9 == 0 ? HellFX.BonePickup(at, kills % 27 == 0) : HellFX.BloodDrop(at, kills % 7 == 0 ? 2 : kills % 3 == 0 ? 1 : 0); if (g != null) gems.Add(g);
                    if (kills % 4 == 0) HellFX.BloodStream(at, demon, 3);
                }
            }
            Co(Every(0f, () =>
            {
                foreach (var sr in imps) if (sr && sr.gameObject.activeSelf) { var d = demon.position - sr.transform.position; sr.transform.position = Snap(sr.transform.position + d.normalized * 1.8f * Time.deltaTime); }
                if (demon) HellFX.MagnetAll(gems, demon, 60);
            }));
            yield return Wait(1.6f);
            HellFX.RuneCircle(demon.position, 30, HellPalette.Crimson, 1.0f, () => { HellFX.HellfireNova(demon.position, 64); KillNear(demon.position, 64 * U); });
            yield return Wait(1.3f);
            var chain = new List<Vector3> { demon.position };
            foreach (var sr in imps) if (sr && sr.gameObject.activeSelf && chain.Count < 5) chain.Add(sr.transform.position);
            HellFX.ChainLightning(chain, HellPalette.Soul);
            for (int i = 1; i < chain.Count; i++) KillNear(chain[i], 6 * U);
            yield return Wait(0.5f);
            HellFX.CritPop(demon.position + new Vector3(30 * U, 10 * U, 0), 999);
            HellFX.Telegraph(P(60, 60), 16, 0.8f); HellFX.MeteorRain(demon.position, 110, 6, 1.0f, p => KillNear(p, 20 * U));
            yield return Wait(1.4f);
            KillNear(demon.position, 400 * U);
            HellFX.KillStreakPop(demon.position + new Vector3(0, 10 * U, 0), 25);
            yield return Wait(0.8f);
            HellFX.DemonEvolve(dfx, () => { if (dsr) dsr.sprite = Temp(PixelSpriteFactory.FromRows(HordeStressTest.DemonLord, PixelFXDemo.Pal, ppu, 2, "DemonLord")); });
            yield return Wait(2.2f);
            if (heat) heat.enabled = false;
        }

        IEnumerator V12() // v1.5 projectile weapons (Vampire-Survivors-style auto weapons)
        {
            PlaceKnight(-80);
            HellFX.PPU = ppu; HordeFX.I.pixelsPerUnit = ppu;
            Temp(HellFX.LavaGround(P(120, 68), 5, 3, 6f, -50));
            var demon = Temp(new GameObject("Demon")).transform; demon.SetParent(transform, false);
            var dsr = demon.gameObject.AddComponent<SpriteRenderer>(); dsr.sprite = Temp(PixelSpriteFactory.FromRows(HordeStressTest.Demon, PixelFXDemo.Pal, ppu, 2, "Demon"));
            if (uberMaterial) dsr.sharedMaterial = uberMaterial; dsr.sortingOrder = 50;
            demon.localScale = new Vector3(2, 2, 1); demon.position = Snap(P(120, 70));
            var imps = new List<SpriteRenderer>();
            var impSprite = Temp(PixelSpriteFactory.FromRows(new[] { ".k....k.", "kRk..kRk", "kRRkkRRk", "kRyRRyRk", "kRRRRRRk", ".kRrrRk.", "kRk..kRk", "kk....kk" }, PixelFXDemo.Pal, ppu, 1, "Imp"));
            for (int i = 0; i < 40; i++)
            {
                float a = i / 40f * Mathf.PI * 2; var at = P(120 + Mathf.Cos(a) * (95 + (i % 3) * 10), 70 + Mathf.Sin(a) * 52);
                var sr = Temp(new GameObject("imp")).AddComponent<SpriteRenderer>(); sr.sprite = impSprite; sr.sortingOrder = 30; sr.transform.position = Snap(at);
                sr.transform.localScale = new Vector3(2, 2, 1); sr.gameObject.AddComponent<CircleCollider2D>().isTrigger = true; HordeFX.Prepare(sr); imps.Add(sr);
            }
            ProjectileFX.FindTarget = (Vector2 from, float r, out Vector2 tgt) =>
            {
                float best = r * r; tgt = default; bool found = false;
                for (int i = 0; i < imps.Count; i++) { var sr = imps[i]; if (!sr || !sr.gameObject.activeSelf) continue; float d = ((Vector2)sr.transform.position - from).sqrMagnitude; if (d < best) { best = d; tgt = sr.transform.position; found = true; } }
                return found;
            };
            ProjectileFX.OnHit = (col, dmg, el) =>
            {
                var sr = col.GetComponent<SpriteRenderer>(); if (!sr || !sr.gameObject.activeSelf) return;
                col.enabled = false; HordeFX.Hit(sr, UnityEngine.Random.Range(8, 24)); var at = sr.transform.position;
                HordeFX.Kill(sr); if (el == ProjElement.Bone) HellFX.BonePickup(at); else HellFX.BloodDrop(at);
            };
            Co(Every(0f, () => { foreach (var sr in imps) if (sr && sr.gameObject.activeSelf && sr.GetComponent<Collider2D>().enabled) { var d = demon.position - sr.transform.position; sr.transform.position = Snap(sr.transform.position + d.normalized * 1.2f * Time.deltaTime); } }));
            Vector2 D() => demon.position;
            Vector2 ToNear() => ProjectileFX.FindTarget(D(), 400 * U, out var t) ? (t - D()) : Vector2.right;
            ProjectileFX.Fireball(D(), ToNear(), demon); yield return Wait(0.5f);
            ProjectileFX.BloodBolt(D(), ToNear(), demon); yield return Wait(0.4f);
            ProjectileFX.Spread(ProjectileFX.Preset(ProjElement.Bone), D(), ToNear(), 5, 50, demon); yield return Wait(0.6f);
            ProjectileFX.HomingVolley(ProjElement.Soul, D(), Vector2.up, 4, demon); yield return Wait(0.7f);
            ProjectileFX.CurseSkull(D(), Vector2.left, demon); yield return Wait(0.5f);
            ProjectileFX.HellfireSpear(D(), ToNear(), demon); yield return Wait(0.4f);
            ProjectileFX.LightningBolt(D(), ToNear(), demon); yield return Wait(0.5f);
            ProjectileFX.HomingVolley(ProjElement.Arcane, D(), Vector2.down, 6, demon); yield return Wait(0.6f);
            ProjectileFX.ReturningBlade(D(), Vector2.right, demon); yield return Wait(0.5f);
            ProjectileFX.Beam(D(), Vector2.left, 140, ProjElement.Hellfire, 1, 6); yield return Wait(0.5f);
            ProjectileFX.Bouncing(ProjElement.Soul, 5, D(), ToNear(), demon); yield return Wait(0.4f);
            ProjectileFX.Splitting(ProjElement.Fire, 6, D(), ToNear(), demon); yield return Wait(0.6f);
            ProjectileFX.Ring(ProjectileFX.Preset(ProjElement.Blood), D(), 16, 0, demon); yield return Wait(0.5f);
            var sp = ProjectileFX.Preset(ProjElement.Hellfire); sp.pierce = -1; sp.speedPx = 120;
            for (float t = 0; t < 1.6f; t += 0.06f) { ProjectileFX.SpiralStep(sp, D(), t, 3, 260, demon); yield return Wait(0.06f); }
            yield return Wait(1.2f);
            ProjectileFX.OnHit = null; ProjectileFX.FindTarget = null;
        }

        IEnumerator V13() // v1.5.1 chain lightning jump, chain whip, poison puddle
        {
            PlaceKnight(-80);
            HellFX.PPU = ppu; HordeFX.I.pixelsPerUnit = ppu; ChainFXRunner.I.Prewarm();
            var hero = Temp(new GameObject("Hero")).transform; hero.SetParent(transform, false); hero.position = Snap(P(120, 68));
            var hsr = hero.gameObject.AddComponent<SpriteRenderer>(); hsr.sprite = Temp(PixelSpriteFactory.FromRows(HordeStressTest.Demon, PixelFXDemo.Pal, ppu, 1, "Hero")); hsr.sortingOrder = 50;
            var imps = new List<SpriteRenderer>();
            var impSprite = Temp(PixelSpriteFactory.FromRows(new[] { ".k....k.", "kRk..kRk", "kRRkkRRk", "kRyRRyRk", "kRRRRRRk", ".kRrrRk.", "kRk..kRk", "kk....kk" }, PixelFXDemo.Pal, ppu, 1, "Imp"));
            for (int i = 0; i < 24; i++)
            {
                float a = i / 24f * Mathf.PI * 2; var at = P(120 + Mathf.Cos(a) * (34 + (i % 3) * 14), 68 + Mathf.Sin(a) * (20 + (i % 3) * 8));
                var sr = Temp(new GameObject("imp")).AddComponent<SpriteRenderer>(); sr.sprite = impSprite; sr.sortingOrder = 30; sr.transform.position = Snap(at);
                sr.gameObject.AddComponent<CircleCollider2D>().isTrigger = true; HordeFX.Prepare(sr); imps.Add(sr);
            }
            System.Action<Collider2D> hit = col => { var sr = col.GetComponent<SpriteRenderer>(); if (!sr || !sr.gameObject.activeSelf) return; col.enabled = false; HordeFX.Hit(sr, UnityEngine.Random.Range(6, 18)); HordeFX.Kill(sr); };
            ChainFX.OnChainHit = (c, h) => hit(c); ChainFX.OnWhipHit = hit; ChainFX.OnPuddleTick = c => { var sr = c.GetComponent<SpriteRenderer>(); if (sr) HordeFX.Hit(sr, 2); };
            Vector2 H() => hero.position;
            for (int i = 0; i < 3; i++) { ChainFX.ChainLightning(H(), 5, 50, 90); yield return Wait(0.7f); }
            ChainFX.ChainWhip(H(), 150, 30, 40, 0.32f, hero); yield return Wait(0.6f);
            ChainFX.ChainWhip(H(), -30, -150, 40, 0.32f, hero); yield return Wait(0.6f);
            ChainFX.ChainWhip(H(), 200, -160, 46, 0.5f, hero); yield return Wait(0.8f);
            var p1 = ChainFX.PoisonPuddle(P(90, 60), 12, 0); ChainFX.PoisonPuddle(P(150, 76), 8, 2.5f); ChainFX.PoisonPuddle(P(120, 50), 20, 3f);
            yield return Wait(2.2f); ChainFX.StopPuddle(p1); yield return Wait(1f);
            ChainFX.OnChainHit = null; ChainFX.OnWhipHit = null; ChainFX.OnPuddleTick = null;
        }

        IEnumerator V14() // v1.6.0 frost cleave, shock, overload, bleed, frost shatter, elite chest
        {
            PlaceKnight(-80);
            HellFX.PPU = ppu; HordeFX.I.pixelsPerUnit = ppu; ReactionFXRunner.I.Prewarm();
            var hero = Temp(new GameObject("Hero")).transform; hero.SetParent(transform, false); hero.position = Snap(P(120, 68));
            var hsr = hero.gameObject.AddComponent<SpriteRenderer>(); hsr.sprite = Temp(PixelSpriteFactory.FromRows(HordeStressTest.Demon, PixelFXDemo.Pal, ppu, 1, "Hero")); hsr.sortingOrder = 50;
            var imps = new List<SpriteRenderer>();
            var impSprite = Temp(PixelSpriteFactory.FromRows(new[] { ".k....k.", "kRk..kRk", "kRRkkRRk", "kRyRRyRk", "kRRRRRRk", ".kRrrRk.", "kRk..kRk", "kk....kk" }, PixelFXDemo.Pal, ppu, 1, "Imp"));
            for (int i = 0; i < 24; i++)
            {
                float a = i / 24f * Mathf.PI * 2; var at = P(120 + Mathf.Cos(a) * (16 + (i % 3) * 14), 68 + Mathf.Sin(a) * (10 + (i % 3) * 8));
                var sr = Temp(new GameObject("imp")).AddComponent<SpriteRenderer>(); sr.sprite = impSprite; sr.sortingOrder = 30; sr.transform.position = Snap(at);
                sr.gameObject.AddComponent<CircleCollider2D>().isTrigger = true; HordeFX.Prepare(sr); imps.Add(sr);
            }
            System.Action<Collider2D> kill = col => { var sr = col.GetComponent<SpriteRenderer>(); if (!sr || !sr.gameObject.activeSelf || !col.enabled) return; col.enabled = false; HordeFX.Hit(sr, UnityEngine.Random.Range(8, 20)); ReactionFX.FrostShatter(sr.transform.position, 8, 10); HordeFX.Kill(sr); };
            ReactionFX.OnCleaveHit = kill; ReactionFX.OnOverloadHit = col => { var sr = col.GetComponent<SpriteRenderer>(); if (sr && col.enabled) { col.enabled = false; HordeFX.Hit(sr, 30); HordeFX.Kill(sr); } };
            Vector2 H() => hero.position;
            ReactionFX.FrostCleave(H(), 0, 18, 150, 0.22f, true, hero); yield return Wait(0.6f);
            ReactionFX.FrostCleave(H(), 180, 18, 150, 0.22f, false, hero); yield return Wait(0.6f);
            var shocks = new List<int>(); var bleeds = new List<int>();
            for (int i = 0; i < imps.Count; i++)
            {
                if (!imps[i] || !imps[i].GetComponent<Collider2D>().enabled) continue;
                if (i % 2 == 0) shocks.Add(ReactionFX.ShockStatus(imps[i].transform, 0)); else bleeds.Add(ReactionFX.BleedStatus(imps[i].transform, 0));
            }
            yield return Wait(1.6f);
            foreach (var h in bleeds) ReactionFX.StopStatus(h);
            ReactionFX.Overload(P(140, 72)); yield return Wait(0.7f);
            ReactionFX.Overload(P(100, 60)); yield return Wait(0.7f);
            foreach (var h in shocks) ReactionFX.StopStatus(h);
            ReactionFX.FrostShatter(P(80, 80), 14, 18); yield return Wait(0.6f);
            ReactionFX.EliteChestOpen(P(170, 64)); yield return Wait(1.8f);
            ReactionFX.OnCleaveHit = null; ReactionFX.OnOverloadHit = null;
        }

        IEnumerator V10() // game over
        {
            PlaceKnight(90); var s = Slime("Slime", 190, out var sfx);
            Co(Every(0f, () => { if (_gkFX && !_gameOver) _gkFX.gradientMapAmount = 0.55f + 0.45f * Mathf.Sin(Time.time * 8); }));
            yield return Hop(s, 1.6f, 0.4f, 10, -18);
            _gameOver = true; _gkFX.gradientMapAmount = 0;
            K("hurt"); var hf = Temp(_gkSR.gameObject.AddComponent<HitFlash>()); hf.Play(); HitStop.Play(0.1f, 0.4f, 0.15f);
            DamageNumbers.Spawn(ChestOf(_gk) + Vector3.up * 10 * U, 30, new Color32(255, 255, 255, 255), 90, ppu, false);
            Co(Knock(s, 20)); Co(Knock(_gk, -10));
            yield return Wait(0.3f); K("dead");
            yield return Ramp(v => { _gkFX.greyscale = v; sfx.greyscale = v; }, 0.8f);
            yield return Wait(0.4f);
            _gkFX.greyscale = 0; sfx.greyscale = 0; _gkFX.sepia = 1; sfx.sepia = 1;
            _gameOverText = true; // CRT itself is the Full Screen Pass feature material (enable it on the renderer to see it)
        }
        IEnumerator Knock(Transform t, float px)
        {
            float d = 0, tt = 0; while (tt < 0.25f) { tt += Time.deltaTime; float k = 1 - Mathf.Pow(1 - Mathf.Clamp01(tt / 0.25f), 3); t.position += new Vector3((k - d) * px * U, 0, 0); d = k; yield return null; }
            t.position = Snap(t.position);
        }
        IEnumerator Bob(Transform t, float px, float speed)
        {
            Vector3 a = t.position; float tt = 0;
            while (t) { tt += Time.deltaTime; t.position = Snap(a + Vector3.up * Mathf.Sin(tt * speed) * px * U); yield return null; }
        }
        IEnumerator Every(float s, Action a)
        {
            float t = 0;
            while (true) { t += Time.deltaTime; if (t >= s) { t = 0; a(); } yield return null; }
        }

        // ---------------- mode plumbing ----------------
        void ToggleGallery()
        {
            _gallery = !_gallery;
            if (_gallery)
            {
                if (_loop != null) { StopCoroutine(_loop); _loop = null; }
                _knight.gameObject.SetActive(false); _slime.gameObject.SetActive(false);
                foreach (var c in _coins) if (c.t) c.t.gameObject.SetActive(false);
                GalleryEnter(0);
            }
            else
            {
                GalleryReset();
                _gk.gameObject.SetActive(false);
                _knight.gameObject.SetActive(true); _slime.gameObject.SetActive(true);
                ResetMoment();
                if (autoLoop) _loop = StartCoroutine(Loop());
            }
        }
        void GalleryStep(int d) { GalleryEnter((_gi + d + _g.Count) % _g.Count); }
        void GalleryReset()
        {
            foreach (var c in _gCo) if (c != null) StopCoroutine(c);
            _gCo.Clear();
            foreach (var o in _gTemp) if (o) Destroy(o);
            _gTemp.Clear();
            JsonUtility.FromJsonOverwrite(_gkDefault, _gkFX);
            _gkSR.enabled = true; _gk.localScale = new Vector3(GScale, GScale, 1);
            _gameOver = _gameOverText = false; Time.timeScale = 1f;
            PixelScreenFX.ClearAll();
        }
        void GalleryEnter(int i)
        {
            GalleryReset();
            _gi = i; _gT = 0;
            _gk.gameObject.SetActive(true); _gs.gameObject.SetActive(false);
            PlaceKnight(110);
            Co(_g[i].play());
        }
        void GalleryUpdate(float dt)
        {
            if (!_gallery || _g.Count == 0) return;
            KAnim();
            _gT += Time.unscaledDeltaTime; // hit-stop must not stall the gallery clock
            if (_gT >= Mathf.Max(galleryEntrySeconds, _g[_gi].secs + 0.6f)) GalleryStep(1);
        }
        GUIStyle _gStyle, _gSmall;
        void GalleryGUI(float s)
        {
            if (!_gallery || _g.Count == 0) return;
            if (_gStyle == null)
            {
                _gStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
                _gSmall = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true };
            }
            var e = _g[_gi];
            float w = Screen.width / s, h = Screen.height / s;
            if (_gameOverText)
            {
                var big = new GUIStyle(_gStyle) { fontSize = 48, alignment = TextAnchor.MiddleCenter }; big.normal.textColor = new Color(0.89f, 0.23f, 0.27f);
                GUI.Label(new Rect(0, h * 0.5f - 60, w, 60), "GAME OVER", big);
                GUI.Label(new Rect(0, h * 0.5f, w, 30), "continue?  9", new GUIStyle(_gStyle) { alignment = TextAnchor.MiddleCenter });
            }
            GUI.Label(new Rect(12, 8, w, 30), $"{_gi + 1}/{_g.Count}  {e.name}", _gStyle);
            GUI.Label(new Rect(12, 36, w - 24, 40), string.Join("  •  ", e.effects) + "        (* needs URP Full Screen Pass feature · Left/Right step · G exit)", _gSmall);
        }
    }
}
