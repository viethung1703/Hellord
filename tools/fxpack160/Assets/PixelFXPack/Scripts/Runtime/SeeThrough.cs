using UnityEngine;

namespace PixelFXPack
{
    /// <summary>
    /// Shows a flat-color silhouette of this sprite where it is hidden behind occluders.
    /// Occluders need a SpriteMask covering them (e.g. a SpriteMask on each wall using the wall sprite).
    /// Works by adding a child SpriteRenderer with mask interaction "Visible Inside Mask" drawn above the walls.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [AddComponentMenu("Pixel FX/See-Through Silhouette")]
    public class SeeThrough : MonoBehaviour
    {
        public Color silhouetteColor = new Color(0.3f, 0.7f, 1f, 0.6f);
        [Tooltip("Sorting order of the silhouette; must be above your walls.")] public int sortingOrder = 100;
        public string sortingLayerName = "Default";

        SpriteRenderer _src, _sil; PixelFX _silFx;

        void OnEnable()
        {
            _src = GetComponent<SpriteRenderer>();
            if (_sil == null)
            {
                var go = new GameObject("SeeThrough Silhouette");
                go.transform.SetParent(transform, false);
                _sil = go.AddComponent<SpriteRenderer>();
                _silFx = go.AddComponent<PixelFX>();
            }
            _sil.gameObject.SetActive(true);
        }

        void OnDisable() { if (_sil != null) _sil.gameObject.SetActive(false); }

        void LateUpdate()
        {
            if (_sil == null || _src == null) return;
            _sil.sprite = _src.sprite;
            _sil.flipX = _src.flipX; _sil.flipY = _src.flipY;
            _sil.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            _sil.sortingLayerName = sortingLayerName;
            _sil.sortingOrder = sortingOrder;
            _silFx.silhouetteColor = silhouetteColor;
            _silFx.silhouetteAmount = 1f;
        }
    }
}
