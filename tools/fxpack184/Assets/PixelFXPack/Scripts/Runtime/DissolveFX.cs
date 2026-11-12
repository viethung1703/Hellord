using System;
using System.Collections;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>Pixel-step dissolve in/out. PlayOut() for death, PlayIn() for spawn.</summary>
    [RequireComponent(typeof(PixelFX))]
    [AddComponentMenu("Pixel FX/Dissolve")]
    public class DissolveFX : MonoBehaviour
    {
        [Min(0.01f)] public float duration = 0.6f;
        [Range(1, 8)] public int cellSize = 1;
        [Tooltip("Quantise progress to N steps for a choppy retro look (0 = smooth).")] [Min(0)] public int steps = 12;
        public bool destroyOnDissolvedOut = false;
        public bool randomSeed = true;

        public event Action Completed;
        PixelFX _fx; Coroutine _co;

        void Awake() { _fx = GetComponent<PixelFX>(); }

        public void PlayOut() { Run(0f, 1f, destroyOnDissolvedOut); }
        public void PlayIn() { Run(1f, 0f, false); }

        void Run(float from, float to, bool destroyAfter)
        {
            if (_fx == null) _fx = GetComponent<PixelFX>();
            if (!isActiveAndEnabled) return;
            if (_co != null) StopCoroutine(_co);
            if (randomSeed) _fx.dissolveSeed = UnityEngine.Random.Range(0f, 100f);
            _fx.dissolveCell = cellSize;
            _co = StartCoroutine(Co(from, to, destroyAfter));
        }

        IEnumerator Co(float from, float to, bool destroyAfter)
        {
            float t = 0;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);
                if (steps > 0) k = Mathf.Floor(k * steps) / steps;
                _fx.dissolveAmount = Mathf.Lerp(from, to, k);
                yield return null;
            }
            _fx.dissolveAmount = to; _co = null;
            Completed?.Invoke();
            if (destroyAfter) Destroy(gameObject);
        }
    }
}
