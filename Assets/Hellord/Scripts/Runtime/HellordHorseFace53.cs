using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

// t501u: Horse-Face (PixelLab v138, Art/previews/step34_pounding_mortar/v138_horseface_throw_travel) = Mortar Hell boss.
// boss_horseface_pestle.png = the 106-frame 96x96 strip re-packed 21x6 (row-major; WebGL 2048 cap), pivot (48,88), ppu 16.
// The boss runs on the Ox-Head state machine (Spear Warden kit), so its frames are remapped into the Ox-Head slot layout (HArt.BossAnims).
// Attacks face SE only: the existing flipX handles SW/W. Rice sack: thrown on the stomp strike, flies from the hand (JSON offsets) and lands on the hero.
namespace Hellord
{
    public partial class HellordGame
    {
        // Ox-Head slot (HArt.BossAnims order, 80 frames) -> Horse-Face v138 frame (boss_horseface_pixellab.json)
        static readonly int[] HorseMap53 = {
            0, 1, 2, 3, 4, 5,                         // idle 0-5            <- idle (SE) 0-5
            12, 13, 14, 15, 16, 17, 18, 19,           // move_S 6-13         <- walk_S
            20, 21, 22, 23, 24, 25, 26, 27,           // move_E 14-21        <- walk_SE (walk_E has no pestle)
            36, 37, 38, 39, 40, 41, 42, 43,           // move_N 22-29        <- walk_N
            66, 67, 68, 69, 70, 71, 72,               // gore_windup_S 30-32 + gore_loop_S 33-36 <- spin_loop (attack sweep)
            66, 67, 68, 69, 70, 71, 72,               // gore_windup_E 37-39 + gore_loop_E 40-43 <- spin_loop
            58, 59, 61, 62, 63, 64,                   // horn_windup 44-46 + horn_strike 47-49   <- pestle_overhead_slam
            98, 100, 101, 103,                        // stomp_windup 50-51 + stomp_strike 52-53 <- rice_sack_throw (sack leaves at f3 = 101)
            82, 84, 86, 89,                           // roar 54-57          <- leap_in
            75, 77,                                   // hurt 58-59          <- dizzy recoil (t502u: hurt 44-49 has no pestle)
            90, 92, 94, 96,                           // stun_loop 60-63     <- stuck_pestle_stun
            74, 75, 76, 77, 78, 79,                   // phase_change 64-69  <- dizzy
            50, 51, 52, 53, 54, 55, 56, 57, 57, 57,   // death 70-79         <- death (7f + hold)
        };
        static Sprite[] _horseF53, _sackF53;
        /// <summary>Horse-Face frames in Ox-Head slot order (null = art missing -> caller keeps the old sheet).</summary>
        static Sprite[] HorseFrames53()
        {
            if (_horseF53 != null) return _horseF53;
            var src = HArt.Strip("boss_horseface_pestle", 106, 96, 96, new Vector2(48, 88));
            if (src == null || src.Length < 106 || src[0] == src[105]) return null; // placeholder fallback = same sprite in every slot
            var a = new Sprite[HorseMap53.Length]; for (int i = 0; i < a.Length; i++) a[i] = src[HorseMap53[i]]; return _horseF53 = a;
        }

        class Sack53 { public SpriteRenderer sr; public Vector2 a, b; public float t, dur, h; public bool west; }
        readonly List<Sack53> _sacks53 = new List<Sack53>();
        bool HorseFace53 => MortarBoss53 && _boss != null && _bossF != null && _bossF == _bossFPestle53;

        /// <summary>Pestle slam impact circle from the JSON: offset (18,4) px from the pivot (SE; mirrored when facing west), r 16 px.</summary>
        bool HorseSlamHit53(bool west)
        {
            var T = HellordTuning.I; if (!HorseFace53) return false;
            var c = _boss.pos + new Vector2((west ? -18f : 18f) / HArt.PPU, -4f / HArt.PPU);
            return (_heroPos - c).magnitude < T.horseSlamR53 / HArt.PPU;
        }

        /// <summary>Throw a rice sack: spawns at the JSON hand offset (frame 3 cell centre) and arcs down onto where the hero stands now.</summary>
        void HorseThrowSack53(bool west)
        {
            var T = HellordTuning.I; if (!HorseFace53) return;
            if (_sackF53 == null) _sackF53 = HArt.Strip("boss_horseface_rice_sack", 5, 52, 46, new Vector2(26, 23));
            // offset_from_boss_pivot [21,-91] (top-left of a 52x46 cell) -> cell centre [47,-68] px, y up in world
            var a = _boss.pos + new Vector2((west ? -47f : 47f) / HArt.PPU, 68f / HArt.PPU);
            var go = new GameObject("horse_sack53"); go.transform.SetParent(_room != null ? _room.Root : null, false);
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = _sackF53[0]; sr.flipX = west; sr.sortingOrder = 30000;
            go.transform.position = a;
            float d = (_heroPos - a).magnitude;
            _sacks53.Add(new Sack53 { sr = sr, a = a, b = _heroPos, dur = Mathf.Clamp(d / Mathf.Max(1f, T.horseSackSpeed53), 0.45f, 1.4f), h = T.horseSackArc53, west = west });
        }

        void UpdateSacks53(float dt)
        {
            var T = HellordTuning.I;
            for (int i = _sacks53.Count - 1; i >= 0; i--)
            {
                var s = _sacks53[i]; s.t += dt; float k = Mathf.Clamp01(s.t / s.dur);
                if (s.sr)
                {
                    var p = Vector2.Lerp(s.a, s.b, k) + Vector2.up * (4f * s.h * k * (1f - k)); // rises then falls onto the target
                    s.sr.transform.position = new Vector3(Mathf.Round(p.x * HArt.PPU) / HArt.PPU, Mathf.Round(p.y * HArt.PPU) / HArt.PPU, 0);
                    s.sr.sprite = _sackF53[Mathf.Min(_sackF53.Length - 1, (int)(k * _sackF53.Length))];
                }
                if (k < 1f) continue;
                if ((_heroPos - s.b).magnitude < T.horseSackHitR53) HurtHero(T.horseSackDmg53, "boss_sack", true);
                TellFX.DustPuff(s.b, 7, 14); ScreenShake.Add(1.5f, 0.2f); HellordAudio.Play("oxhead_stomp_strike");
                if (s.sr) Object.Destroy(s.sr.gameObject); _sacks53.RemoveAt(i);
            }
        }
        float _spinCd53;
        /// <summary>t502u: per-frame Horse-Face extras after AnimateBoss: leap-in arc, death x offset, spin sweep hitbox, JSON body hurtbox.</summary>
        void AnimateHorse53(float dt, bool faceWest)
        {
            if (_bossBody == null || _boss == null) return;
            if (!HorseFace53) return;
            var T = HellordTuning.I; var e = _boss; float x = 0, y = 0;
            if (_anim == "roar" && T.horseLeapH53 > 0) { float k = Mathf.Clamp01(_animT / Mathf.Max(0.05f, T.horseLeapS53)); y = Mathf.Sin(k * Mathf.PI) * T.horseLeapH53; }
            if (e.dying) x = (faceWest ? -T.horseDeathDx53 : T.horseDeathDx53) / HArt.PPU;
            _bossBody.transform.localPosition = new Vector3(Mathf.Round(x * HArt.PPU) / HArt.PPU, Mathf.Round(y * HArt.PPU) / HArt.PPU, 0);
            if (T.horseJsonBody53) { e.radius = 13f / HArt.PPU; e.hitY = 23f / HArt.PPU; } // body rect [-13,-46,26,46] from the pivot: half-width 13, centre 23 px up
            if (_spinCd53 > 0) _spinCd53 -= dt;
            if (!e.dying && _anim.StartsWith("gore_loop") && _spinCd53 <= 0)
            {
                var c = e.pos + Vector2.up * (23f / HArt.PPU);
                if ((_heroPos + Vector2.up * 0.5f - c).magnitude < T.horseSpinR53 / HArt.PPU) { HurtHero(T.horseSpinDmg53, "boss_spin", true); _spinCd53 = T.horseSpinCd53; }
            }
        }
        void ClearSacks53() { foreach (var s in _sacks53) if (s.sr) Object.Destroy(s.sr.gameObject); _sacks53.Clear(); }
    }
}
