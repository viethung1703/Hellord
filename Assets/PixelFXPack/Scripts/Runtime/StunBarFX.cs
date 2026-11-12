using System;
using UnityEngine;

namespace PixelFXPack
{
    /// <summary>
    /// 114. Boss Stun Bar (v1.8.8). Shows on a boss HP bar that the boss is STUNNED / VULNERABLE without looking like lost HP:
    /// no red, no chip segment, the fill is never touched or resized. It is a separate overlay drawn into a small Texture2D
    /// (bar + 14 px each side, 14 px above, 5 px below) and redrawn only while active (a few thousand pixels, no GC).
    ///  - Intro pop (0.25 s): white flash on the track, border bulges 1 px out, "STUN" badge overshoots up, gold stripes wipe in, 6-spark burst.
    ///  - Sustain: gold hazard stripes march across the WHOLE track (full and empty parts alike), border blinks yellow/white at 4 Hz,
    ///    a glint runs along the top edge, and the same 2 stars + birdie as in-world StunStars #104 orbit the badge.
    ///  - Optional timer: a 1 px yellow line under the bar shrinks toward the centre (stun time left), blinking in the last 25%.
    ///  - End recovery (0.3 s): stripes wipe out left to right, border flashes white once and settles, badge drops and fades, stars fly off.
    /// SpriteRenderer bars: leave <see cref="createSpriteRenderer"/> on; the overlay child is placed so the fill's bottom-left is at
    /// <see cref="fillOriginLocal"/>. UI bars: put <see cref="Texture"/> in a RawImage sized <see cref="TextureSizePx"/> x your UI scale,
    /// with pivot <see cref="FillPivot01"/> placed on the fill's bottom-left (no UnityEngine.UI dependency in this file).
    /// </summary>
    [AddComponentMenu("PixelFX/Stun Bar FX (#114)")]
    public class StunBarFX : MonoBehaviour
    {
        [Tooltip("Inner fill size of your HP bar in pixels (not counting its frame).")] public int barWidthPx = 160, barHeightPx = 6;
        [Tooltip("Local position (world units) of the fill's bottom-left corner, for the SpriteRenderer overlay.")] public Vector2 fillOriginLocal;
        public bool createSpriteRenderer = true;
        public string sortingLayer = "Default"; public int sortingOrder = 500;
        public float pixelsPerUnit = HellFX.PPU;
        [Tooltip("Optional: also start/stop in-world StunStars #104 on this transform with BeginWithStars.")] public Transform boss;
        public float bossHeadYPx = 24;

        public const int PadX = 14, PadTop = 14, PadBottom = 5;
        public const float IntroT = 0.25f, OutroT = 0.3f, StripeSpeed = 16f, StripeAlpha = 0.55f;
        static readonly Color32 Y = new Color32(0xfe, 0xe7, 0x61, 255), G = new Color32(0xfe, 0xae, 0x34, 255), Wt = new Color32(255, 255, 255, 255), K = new Color32(0x3e, 0x27, 0x31, 255);
        static readonly string[] Letters = { "WWW W.. WWW ..W WWW", "WWW .W. .W. .W. .W.", "W.W W.W W.W W.W WWW", "WWW W.W W.W W.W W.W" }; // S T U N
        static readonly string[][] Rows = Array.ConvertAll(Letters, s => s.Split(' '));

        public Texture2D Texture { get; private set; }
        public Vector2Int TextureSizePx => new Vector2Int(barWidthPx + 2 * PadX, barHeightPx + PadTop + PadBottom);
        public Vector2 FillPivot01 => new Vector2((float)PadX / TextureSizePx.x, (float)PadBottom / TextureSizePx.y);
        public bool IsStunned => _on && !_ending;
        public bool IsVisible => _on;
        /// <summary>1 at Begin -> 0 at the end of the given duration (1 when no duration).</summary>
        public float Remaining01 => _dur > 0 ? Mathf.Clamp01(1f - _tb / _dur) : 1f;
        public event Action OnStunEnd;

        bool _on, _ending, _timer; float _tb, _te, _dur; int _stars = -1; Color32[] _px; SpriteRenderer _sr; int _w, _h;

        /// <summary>Starts the stun signal. <paramref name="duration"/> &gt; 0 auto-ends (and drives the timer line); &lt;= 0 runs until End.
        /// Calling Begin while stunned restarts the pop and the timer.</summary>
        public void Begin(float duration = 0f, bool showTimer = true)
        {
            Ensure(); _on = true; _ending = false; _tb = 0; _te = 0; _dur = duration; _timer = showTimer && duration > 0;
            if (_sr) _sr.enabled = true; Redraw();
        }
        /// <summary>Begin + in-world StunStars #104 on <see cref="boss"/> (or <paramref name="bossOverride"/>) with the same duration.</summary>
        public void BeginWithStars(float duration, Transform bossOverride = null, bool showTimer = true)
        {
            Begin(duration, showTimer); var b = bossOverride ? bossOverride : boss;
            if (b) { if (_stars >= 0) TellFX.Stop(_stars); _stars = TellFX.StunStars(b, duration > 0 ? duration : 0, bossHeadYPx); }
        }
        /// <summary>Plays the recovery and hides. <paramref name="instant"/> skips the recovery.</summary>
        public void End(bool instant = false)
        {
            if (!_on) return; if (_stars >= 0) { TellFX.Stop(_stars); _stars = -1; }
            if (instant) { Hide(); return; }
            if (!_ending) { _ending = true; _te = 0; }
        }
        void Hide() { bool was = _on; _on = false; _ending = false; Clear(); Apply(); if (_sr) _sr.enabled = false; if (was) OnStunEnd?.Invoke(); }

        void Ensure()
        {
            var sz = TextureSizePx;
            if (Texture == null || _w != barWidthPx || _h != barHeightPx)
            {
                _w = barWidthPx; _h = barHeightPx;
                if (Texture) Destroy(Texture);
                Texture = new Texture2D(sz.x, sz.y, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "StunBarFX", hideFlags = HideFlags.DontSave };
                _px = new Color32[sz.x * sz.y];
                if (createSpriteRenderer)
                {
                    if (_sr == null) { var go = new GameObject("StunBarOverlay"); go.transform.SetParent(transform, false); _sr = go.AddComponent<SpriteRenderer>(); }
                    _sr.sprite = Sprite.Create(Texture, new Rect(0, 0, sz.x, sz.y), FillPivot01, pixelsPerUnit, 0, SpriteMeshType.FullRect);
                    _sr.sortingLayerName = sortingLayer; _sr.sortingOrder = sortingOrder; _sr.transform.localPosition = fillOriginLocal;
                }
            }
        }
        void OnDestroy() { if (Texture) Destroy(Texture); if (_stars >= 0) TellFX.Stop(_stars); }

        void Update()
        {
            if (!_on) return; float dt = Time.deltaTime; _tb += dt;
            if (!_ending && _dur > 0 && _tb >= _dur) End();
            if (_ending) { _te += dt; if (_te >= OutroT) { Hide(); return; } }
            Redraw();
        }

        // ------- drawing (texture y-up; fill bottom-left at (PadX, PadBottom)) -------
        void Clear() { if (_px != null) Array.Clear(_px, 0, _px.Length); }
        void Apply() { if (Texture) { Texture.SetPixels32(_px); Texture.Apply(false); } }
        void P(int x, int y, Color32 c, float a = 1f)
        {
            x += PadX; y += PadBottom; int W = _w + 2 * PadX, H = _h + PadTop + PadBottom; if (x < 0 || y < 0 || x >= W || y >= H || a <= 0) return;
            a = Mathf.Clamp01(a) * c.a / 255f; ref var d = ref _px[y * W + x]; float da = d.a / 255f, oa = a + da * (1 - a); if (oa <= 0) return;
            d = new Color32((byte)((c.r * a + d.r * da * (1 - a)) / oa), (byte)((c.g * a + d.g * da * (1 - a)) / oa), (byte)((c.b * a + d.b * da * (1 - a)) / oa), (byte)(oa * 255));
        }
        void Star(int x, int y, bool front, float a) { var arm = front ? Y : G; P(x, y, front ? Wt : Y, a); P(x - 1, y, arm, a); P(x + 1, y, arm, a); P(x, y - 1, arm, a); P(x, y + 1, arm, a); }
        void Bird(int x, int y, bool up, float a) { P(x, y, Wt, a); P(x - 1, y + (up ? 1 : 0), Wt, a); P(x + 1, y + (up ? 1 : 0), Wt, a); P(x - 2, y + (up ? 1 : 0), Y, a * 0.8f); P(x + 2, y + (up ? 1 : 0), Y, a * 0.8f); }

        public void Redraw()
        {
            Clear(); int w = _w, h = _h; float tb = _tb, te = _te; bool end = _ending;
            float endK = end ? te / OutroT : 0;
            // stripes over the whole track
            int scroll = (int)(tb * StripeSpeed); float wipeIn = Mathf.Min(1, tb / (IntroT * 0.8f)); float wipeOut = end ? Mathf.Min(1, te / (OutroT * 0.83f)) : 0;
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
            {
                if (x >= w * wipeIn || x < w * wipeOut) continue;
                if (((x + y + 1000 - scroll) & 7) < 3) P(x, y, G, StripeAlpha);
            }
            if (!end && tb < 0.067f) for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) P(x, y, Wt, 0.8f);
            // border
            Color32 bc; float ba = 1;
            if (end) { bc = te < 0.067f ? Wt : Y; ba = te < 0.067f ? 1 : 1 - (te - 0.067f) / (OutroT - 0.067f); }
            else bc = (((int)(tb * 8)) & 1) == 0 ? Y : Wt;
            Ring(-1, -1, w, h, bc, ba);
            if (!end && tb < 0.15f) Ring(-2, -2, w + 1, h + 1, Y, 1 - tb / 0.15f);
            // glint along the top edge
            if (!end && tb >= IntroT) { float q = ((tb - IntroT) % 0.6f) / 0.6f; int gx = Mathf.RoundToInt(q * (w + 8)) - 4; P(gx, h, Wt); P(gx - 1, h, Wt, 0.7f); P(gx + 1, h, Wt, 0.7f); P(gx, h + 1, Wt, 0.8f); P(gx, h - 1, Wt, 0.6f); }
            // timer line under the bar
            if (_timer)
            {
                float rem = Remaining01; int len = Mathf.RoundToInt(w * rem), x0 = (w - len) / 2; float la = end ? 1 - endK : 1;
                for (int x = 0; x < w; x++) P(x, -3, K, 0.75f * la);
                bool blink = rem < 0.25f && (((int)(tb * 8)) & 1) == 1;
                if (!blink) for (int x = x0; x < x0 + len; x++) P(x, -3, Y, la);
            }
            // badge + orbiting stars (same motif as in-world StunStars #104)
            int cx = w / 2, yoff = 0; float bA = 1;
            if (!end) yoff = tb < 0.06f ? -3 : tb < 0.12f ? 2 : tb < 0.18f ? 1 : 0; else { yoff = -Mathf.RoundToInt(endK * 3); bA = 1 - endK; }
            int by = h + 3 + yoff, ocy = by + 3;
            float rx = 13 + (end ? te * 60 : 0); float sa = end ? 1 - endK : 1;
            for (int pass = 0; pass < 2; pass++)
            {
                if (pass == 1) Badge(cx, by, (!end && tb < 0.067f) ? Wt : Y, bA);
                for (int k = 0; k < 3; k++)
                {
                    float a = tb * 4f + k * 2.0944f; bool front = Mathf.Sin(a) < 0; if (front != (pass == 1)) continue;
                    int sx = cx + Mathf.RoundToInt(Mathf.Cos(a) * rx), sy = ocy + Mathf.RoundToInt(Mathf.Sin(a) * 2);
                    if (k < 2) Star(sx, sy, front, sa * (front ? 1 : 0.7f)); else Bird(sx, sy, (((int)(tb * 8)) & 1) == 0, sa * (front ? 1 : 0.7f));
                }
            }
            if (!end && tb < IntroT)
                for (int k = 0; k < 6; k++) { float a = k * 1.0472f + 0.3f, r = 3 + tb * 40; P(cx + Mathf.RoundToInt(Mathf.Cos(a) * r * 1.6f), ocy + Mathf.RoundToInt(Mathf.Sin(a) * r * 0.6f), tb < 0.1f ? Wt : Y, 1 - tb / IntroT); }
            Apply();
        }
        void Ring(int x0, int y0, int x1, int y1, Color32 c, float a)
        {
            for (int x = x0; x <= x1; x++) { P(x, y0, c, a); P(x, y1, c, a); }
            for (int y = y0 + 1; y < y1; y++) { P(x0, y, c, a); P(x1, y, c, a); }
        }
        void Badge(int cx, int by, Color32 fill, float a)
        {
            int x0 = cx - 7; // 4 letters x 3 px + 3 gaps = 15 px
            for (int pass = 0; pass < 2; pass++)
                for (int l = 0; l < 4; l++)
                {
                    var rows = Rows[l];
                    for (int r = 0; r < 5; r++) for (int x = 0; x < 3; x++)
                    {
                        if (rows[r][x] != 'W') continue; int px = x0 + l * 4 + x, py = by + 1 + (4 - r);
                        if (pass == 0) { for (int dy = -1; dy <= 1; dy++) for (int dx = -1; dx <= 1; dx++) P(px + dx, py + dy, K, a); }
                        else P(px, py, fill, a);
                    }
                }
        }
    }
}
