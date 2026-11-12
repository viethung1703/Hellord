using System.Collections;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>Flashes the sprite a solid color. Call Play() from your damage code.</summary>
    [RequireComponent(typeof(PixelFX))]
    [AddComponentMenu("Pixel FX/Hit Flash")]
    public class HitFlash : MonoBehaviour
    {
        public Color color = Color.white;
        [Min(0.01f)] public float duration = 0.12f;
        [Tooltip("Number of on/off blinks. 1 = single flash.")] [Min(1)] public int blinks = 1;
        public bool useUnscaledTime = false;

        PixelFX _fx; Coroutine _co;

        void Awake() { _fx = GetComponent<PixelFX>(); }

        public void Play() { Play(color); }

        public void Play(Color c)
        {
            if (_fx == null) _fx = GetComponent<PixelFX>();
            if (!isActiveAndEnabled) return;
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(Run(c));
        }

        IEnumerator Run(Color c)
        {
            _fx.flashColor = c;
            float per = duration / blinks;
            for (int b = 0; b < blinks; b++)
            {
                _fx.flashAmount = 1f;
                float t = 0;
                while (t < per * 0.5f) { t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime; yield return null; }
                _fx.flashAmount = 0f;
                t = 0;
                while (t < per * 0.5f) { t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime; yield return null; }
            }
            _fx.flashAmount = 0f; _co = null;
        }

        void OnDisable() { if (_fx != null) _fx.flashAmount = 0; _co = null; }
    }
}
