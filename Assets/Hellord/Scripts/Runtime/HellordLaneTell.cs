using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

// v0.7: Gore Charge lane tell drawn game-side in the TellFX v1.8.4 "outline" vocabulary from the approved mock-up
// (review/v184/oxpit_v184.py draw_lane): 1px dark OUTLINE ring (not a filled box), a thin dark-crimson wash (14% ahead of the
// clock, 32% behind it), 1px crimson rim + end line, a 1px gold clock front reaching the end at 85% of the tell, 3 chevrons trailing
// the front, rim blinks white in the last 15%; on fire a 2-frame 55% white flash, then the rim fades over 0.2 s.
// Why not TellFX.LaneTelegraph: in the v1.8.4 C# its "dark outline" is a FULL (len+2)x(w+2) box at 75% under the wash, so in Unity
// the 40px Gore lane reads as a filled dark-red strip. (Reported upstream; pack files are bundled unchanged.)
namespace Hellord
{
    public class LaneTell
    {
        static readonly Color Ink = new Color32(24, 20, 37, 255), Crim = new Color32(0xe4, 0x3b, 0x44, 255), DCrim = new Color32(0xa2, 0x26, 0x33, 255),
            Yel = new Color32(0xfe, 0xe7, 0x61, 255), Orng = new Color32(0xfe, 0xae, 0x34, 255);
        const float U = 1f / 16f;
        readonly Transform _root; public Transform Root => _root; readonly List<SpriteRenderer> _pool = new List<SpriteRenderer>(); int _used; readonly int _order;
        Vector2 _a, _b; float _w, _tell, _t = -1; bool _fired;
        public bool Active => _t >= 0;
        public LaneTell(int order) { _order = order; _root = new GameObject("GoreLaneTell").transform; }
        public void Begin(Vector2 from, Vector2 to, float widthPx, float seconds) { _a = from; _b = to; _w = widthPx; _tell = Mathf.Max(0.2f, seconds); _t = 0; _fired = false; }
        public void Stop() { _t = -1; Hide(); }
        public void Tick(float dt)
        {
            _used = 0;
            if (_t >= 0) { _t += dt; if (!Draw()) _t = -1; }
            for (int i = _used; i < _pool.Count; i++) if (_pool[i].enabled) _pool[i].enabled = false;
        }
        void Hide() { foreach (var s in _pool) s.enabled = false; }
        bool Draw()
        {
            Vector2 d = _b - _a; float L = d.magnitude * 16f; if (L < 1) return false;
            Vector2 f = d.normalized, n = new Vector2(-f.y, f.x); float deg = Mathf.Atan2(f.y, f.x) * Mathf.Rad2Deg, hw = _w * 0.5f;
            Vector2 P(float u, float v) => _a + (f * u + n * v) * U;
            if (_t >= _tell)
            {
                if (!_fired) { _fired = true; if (ScreenShake.Main) ScreenShake.Main.Kick(-f * 1.5f); }
                float k = _t - _tell;
                if (k < 0.05f) { Box(P(L / 2, 0), L, _w, deg, A(Color.white, 0.55f), 1); return true; }
                float al = 1 - k / 0.2f; if (al <= 0) return false;
                Box(P(L / 2, hw - 0.5f), L, 1, deg, A(Crim, al), 2); Box(P(L / 2, -hw + 0.5f), L, 1, deg, A(Crim, al), 2);
                return true;
            }
            float p = _t / _tell, front = L * Mathf.Min(1, p / 0.85f);
            // outline ring (1px, 75% ink), outside the lane
            Box(P(L / 2, hw + 0.5f), L + 2, 1, deg, A(Ink, 0.75f), 0); Box(P(L / 2, -hw - 0.5f), L + 2, 1, deg, A(Ink, 0.75f), 0);
            Box(P(L + 0.5f, 0), 1, _w, deg, A(Ink, 0.75f), 0);
            // wash: thin ahead of the clock, stronger behind it
            if (front > 0.5f) Box(P(front / 2, 0), front, _w, deg, A(DCrim, 0.32f), 0);
            if (L - front > 0.5f) Box(P((front + L) / 2, 0), L - front, _w, deg, A(DCrim, 0.14f), 0);
            bool blink = p > 0.85f && ((int)((p - 0.85f) / 0.15f * 4) % 2) == 0; Color ec = blink ? Color.white : Crim;
            Box(P(L / 2, hw - 0.5f), L, 1, deg, ec, 2); Box(P(L / 2, -hw + 0.5f), L, 1, deg, ec, 2); Box(P(L - 0.5f, 0), 1, _w, deg, ec, 2);
            if (p < 0.85f) Box(P(front, 0), 1, _w - 2, deg, Yel, 3);
            float ch = Mathf.Min(3, _w * 0.25f);
            for (int i = 1; i <= 3; i++)
            {
                float cu = front - i * 7; if (cu < 3) break; Color cc = A(i == 1 ? Orng : Crim, 0.9f - 0.2f * i);
                // ">" pointing along f: two 1px strokes from (cu-2, +-ch) to (cu, 0)
                float sl = Mathf.Sqrt(4 + ch * ch), sa = Mathf.Atan2(ch, 2) * Mathf.Rad2Deg;
                Box(P(cu - 1, ch / 2), sl, 1, deg - sa, cc, 3); Box(P(cu - 1, -ch / 2), sl, 1, deg + sa, cc, 3);
            }
            return true;
        }
        static Color A(Color c, float a) { c.a *= a; return c; }
        void Box(Vector2 c, float lenPx, float thickPx, float deg, Color col, int ord)
        {
            if (_used == _pool.Count) { var go = new GameObject("lt"); go.transform.SetParent(_root, false); var s = go.AddComponent<SpriteRenderer>(); s.sprite = HArt.Pixel(); _pool.Add(s); }
            var sr = _pool[_used++]; sr.enabled = true; sr.color = col; sr.sortingOrder = _order + ord;
            sr.transform.position = c; sr.transform.rotation = Quaternion.Euler(0, 0, deg); sr.transform.localScale = new Vector3(Mathf.Max(1, Mathf.Round(lenPx)), Mathf.Max(1, Mathf.Round(thickPx)), 1);
        }
    }
}
