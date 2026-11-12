using UnityEngine;

// t1154 (VH: "give the ox and horse on the pillar glow eyes, make like they are looking").
// Ember eyes on the Sins-screen guardian statues baked into ui_s77_bg (no new art, statues untouched).
// Pixel-crisp native-px boxes in the Endesga fire ramp; banded breathing; the hot core steps 1 px toward the cursor,
// idles with slow glances + pauses, returns to the player; rare slow blink. All values in SinEyesTune.
namespace Hellord
{
    public static class SinEyesTune
    {
        public static bool On = true;
        public static float BreathS = 2.4f;            // breathing period (banded into 3 steps)
        public static float IdleAfterS = 2.5f;          // cursor still this long -> idle glances
        public static float GlanceMinS = 1.4f, GlanceMaxS = 3.2f; // hold time per idle gaze
        public static float BlinkMinS = 9f, BlinkMaxS = 18f, BlinkS = 0.32f;
        public static float LookDeadZone = 0.18f;       // fraction of panel width around the eye column that counts as "straight"
        public static float Glow1 = 0.20f, Glow2 = 0.09f; // halo band alphas at full breath
    }

    public partial class HellordGame
    {
        // eye sockets in native 480x270 bg px: x,y = left/top of a 3x2 socket (ox left eye, ox right eye, horse eye)
        static readonly int[,] Eyes1154 = { { 32, 79 }, { 46, 79 }, { 438, 76 } };
        Vector2 _e54LastMouse; float _e54MovedT, _e54NextGlanceT, _e54NextBlinkT = 6f, _e54BlinkT = -9f; int _e54IdleDx; System.Random _e54Rng = new System.Random(1154);
        float R54(float a, float b) => a + (float)_e54Rng.NextDouble() * (b - a);

        int LookDx1154(float now)
        {
            var m = Event.current.mousePosition;
            if ((m - _e54LastMouse).sqrMagnitude > 1f) { _e54LastMouse = m; _e54MovedT = now; }
            if (now - _e54MovedT < SinEyesTune.IdleAfterS)
            {   // follow the cursor: thirds of the screen relative to the middle
                float u = m.x / 700f - 0.5f; return u < -SinEyesTune.LookDeadZone ? -1 : u > SinEyesTune.LookDeadZone ? 1 : 0;
            }
            if (now >= _e54NextGlanceT)
            {   // idle: glance left/right, pause, come back to the player
                _e54IdleDx = _e54IdleDx != 0 ? 0 : (_e54Rng.Next(2) == 0 ? -1 : 1);
                _e54NextGlanceT = now + (_e54IdleDx == 0 ? R54(SinEyesTune.GlanceMinS * 1.5f, SinEyesTune.GlanceMaxS * 1.5f) : R54(SinEyesTune.GlanceMinS, SinEyesTune.GlanceMaxS));
            }
            return _e54IdleDx;
        }

        /// <summary>Draw after the background/fire, before panels (modal dim covers them).</summary>
        void SinEyes1154()
        {
            if (!SinEyesTune.On || Event.current.type != EventType.Repaint) return;
            float now = Time.unscaledTime; int dx = LookDx1154(now);
            if (now >= _e54NextBlinkT) { _e54BlinkT = now; _e54NextBlinkT = now + R54(SinEyesTune.BlinkMinS, SinEyesTune.BlinkMaxS); }
            float bk = now - _e54BlinkT; float lid = bk >= 0 && bk < SinEyesTune.BlinkS ? Mathf.Abs(bk / SinEyesTune.BlinkS * 2f - 1f) : 1f; // 1 -> 0 -> 1
            int ne = _t46Draw ? Eyes1246.Length : Eyes1154.GetLength(0);
            for (int e = 0; e < ne; e++)
            {
                float x = _t46Draw ? Eyes1246[e][0] : Eyes1154[e, 0], y = _t46Draw ? Eyes1246[e][1] : Eyes1154[e, 1];
                float br = 0.5f + 0.5f * Mathf.Sin((now / SinEyesTune.BreathS + e * 0.37f) * 2f * Mathf.PI);
                int step = br > 0.66f ? 2 : br > 0.33f ? 1 : 0;                 // banded, not smooth
                float I = (0.72f + 0.14f * step) * lid; if (I < 0.08f) continue;
                // soft banded glow onto the surrounding stone (two hard-edged rounded bands)
                Box74(x - 1, y - 1, 5, 4, A48(E_HOT, SinEyesTune.Glow2 * I)); Box74(x - 2, y, 7, 2, A48(E_HOT, SinEyesTune.Glow2 * I));
                Box74(x - 1, y, 5, 2, A48(E_RED, SinEyesTune.Glow1 * I)); Box74(x, y - 1, 3, 4, A48(E_RED, SinEyesTune.Glow1 * 0.7f * I));
                // socket: deep ember, hotter row at the bottom
                Box74(x, y, 3, 2, A48(E_RED, 0.95f * I)); Box74(x, y + 1, 3, 1, A48(E_HOT, 0.75f * I));
                // core / pupil: 1 px column stepping toward the gaze, yellow speck only at the peak of the breath
                float cx = x + 1 + dx; if (lid < 0.5f) continue;
                Box74(cx, y, 1, 2, A48(E_ORANGE, I)); Box74(cx, y, 1, 1, step == 2 ? E_YEL : A48(E_AMBER, I));
            }
        }
    }
}
