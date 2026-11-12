using System.Collections;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>Sweeps a shine band across the sprite. Good for pickups, coins and UI-like sprites.</summary>
    [RequireComponent(typeof(PixelFX))]
    [AddComponentMenu("Pixel FX/Shine Sweep")]
    public class ShineSweep : MonoBehaviour
    {
        [Min(0.01f)] public float sweepTime = 0.5f;
        [Tooltip("Seconds between automatic sweeps. 0 = only when Play() is called.")] [Min(0)] public float loopInterval = 2f;
        public bool playOnEnable = true;

        PixelFX _fx; Coroutine _co;

        void OnEnable()
        {
            _fx = GetComponent<PixelFX>();
            if (playOnEnable) _co = StartCoroutine(Loop());
        }

        void OnDisable() { if (_fx != null) _fx.shinePosition = -1; _co = null; }

        public void Play()
        {
            if (!isActiveAndEnabled) return;
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(Loop(once: true));
        }

        IEnumerator Loop(bool once = false)
        {
            do
            {
                float t = 0;
                while (t < sweepTime) { t += Time.deltaTime; _fx.shinePosition = Mathf.Lerp(-0.3f, 1.3f, t / sweepTime); yield return null; }
                _fx.shinePosition = -1;
                if (loopInterval <= 0 || (once && !playOnEnable)) break; // v1.1: a manual Play() no longer stops the auto loop
                yield return new WaitForSeconds(loopInterval);
            } while (true);
            _co = null;
        }
    }
}
