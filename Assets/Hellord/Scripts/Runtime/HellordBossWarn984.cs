using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

// t974u v0.0.84: the last two non-shader boss telegraphs move onto PixelBossWarn (fx_audit/boss_shader/wire, Ox Stomp v3 mock-up).
//  1) Stomp (Ox; any non-Horse boss that stomps): TellFX.StompRings (crimson rim + gold clock ring + star, then white/gold shock rings) is
//     replaced by a dissolve grain ellipse at the 7 u reach during the 0.8 s windup, then one dissolve wave band (+-0.6 u, y*0.6) per shock
//     ring that follows the REAL hitbox radius (7 u * t/0.55). Strike times are the same as StompRings (0.8 s, then every 0.3 s), so
//     OnTellFire's work (ring hitbox start, squash, strike anim/audio, Horse sack) runs on the identical beat. Horse-Face stays ring-free.
//  2) PatternWarnFX (radial burst / ring / fan / spiral / stomp burst warns): replaced by self-timed PixelBossWarn quads for the same lead.
// All under the boss (bossWarnOrder948). Hitboxes, timings, sizes, bullets: unchanged. Frost Ox keeps the old look (VH: Frost Ox skipped).
namespace Hellord
{
    public partial class HellordGame
    {
        class W984 { public int shape, seed; public Vector2 a; public float R, sq = 1, hw, deg, half, t, tot; }
        readonly List<W984> _w984 = new List<W984>();
        readonly List<SpriteRenderer> _w984Q = new List<SpriteRenderer>();
        bool _st984; float _st984T; int _st984Fired;
        public int stompShaderFrames984, patternWarnShader984, oldStompFrames984, oldPatternWarnFrames984;
        const float St984Tell = 0.8f, St984Gap = 0.3f, St984Grow = 0.55f, St984Rpx = 112f, St984Sq = 0.6f, St984HW = 9.6f;

        bool Stomp984On => HellordTuning.I.bossStompShader984 && Bs950Ok && !IsFrostOx;
        bool PWarn984On => HellordTuning.I.bossPatternWarnShader984 && Bs950Ok && !IsFrostOx;

        void Stomp984Begin() { _st984 = true; _st984T = 0; _st984Fired = 0; }
        /// <summary>BS.Stomp tick: fires ring i at windup + i * gap (same beat TellFX.StompRings used), then the juice StompRings gave.</summary>
        void Stomp984Tick(float dt)
        {
            if (!_st984) return; _st984T += dt;
            while (_st984Fired < 3 && _st984T - St984Tell >= _st984Fired * St984Gap)
            {
                int i = _st984Fired++; StompStrike(i);
                if (i == 0)
                {
                    var m = ScreenShake.Main; if (m != null && ProjectileFX.ShakeScale > 0) m.Kick(Vector2.down * 2.5f * ProjectileFX.ShakeScale);
                    if (TellFX.JuiceHitStop && ProjectileFX.HitStopScale > 0) HitStop.Play(0.05f * ProjectileFX.HitStopScale);
                    Shock934(_stompC, St984Rpx, 0f, St984Grow, 3f, St984HW, false, St984Sq); TellFX.DustPuff(_stompC, 6, 12);
                }
                if (ProjectileFX.ShakeScale > 0) ScreenShake.Add(1.5f * ProjectileFX.ShakeScale, 0.1f);
            }
        }
        /// <summary>PatternWarnFX replacement. kind 0 = band around c (radial / ring / spiral / stomp burst), 1 = wedge (fan). False = shader off, caller uses PatternWarnFX.</summary>
        bool PWarn984(int kind, Vector2 c, float lead, float rPx, float aimDeg = 0, float arcDeg = 0)
        {
            if (!PWarn984On) return false;
            var w = new W984 { a = c, tot = lead > 0 ? lead : BBulletTell, seed = patternWarnShader984++ & 7, R = rPx };
            if (kind == 1) { w.shape = 2; w.deg = aimDeg; w.half = arcDeg * 0.5f + 6f; } else { w.shape = 4; w.hw = 5f; }
            _w984.Add(w); return true;
        }
        void Clear984() { _w984.Clear(); _st984 = false; foreach (var q in _w984Q) if (q) q.enabled = false; }

        /// <summary>Every frame from Bs950Frame: draws the stomp windup / wave bands and the timed pattern warns.</summary>
        void Bs984Frame()
        {
            int n = 0;
            if (_boss != null && PatternWarnFX.ActiveCount > 0) oldPatternWarnFrames984++;
            if (_bs == BS.Stomp && _tellH >= 0 && TellFX.IsAlive(_tellH)) oldStompFrames984++;
            if (_st984 && (_bs != BS.Stomp || !BossAlive)) _st984 = false;
            if (_st984 && Bs950Ok)
            {
                if (_st984T < St984Tell) Q984(n++, 0, _stompC, St984Rpx, St984Sq, 0, 0, 0, _st984T / St984Tell, 6);
                for (int i = 0; i < 3; i++)
                {
                    float rt = _ringT[i]; if (rt < 0 || rt > St984Grow) continue; float k = rt / St984Grow, r = St984Rpx * k;
                    if (r > 2) Q984(n++, 4, _stompC, r, St984Sq, St984HW, 0, 0, 1f - 0.25f * k, 7 + i);
                }
                if (n > 0) stompShaderFrames984++;
            }
            float dt = Time.deltaTime;
            for (int i = _w984.Count - 1; i >= 0; i--) { var w = _w984[i]; w.t += dt; if (w.t >= w.tot || !BossAlive) _w984.RemoveAt(i); }
            if (Bs950Ok) foreach (var w in _w984) Q984(n++, w.shape, w.a, w.R, w.sq, w.hw, w.deg, w.half, Mathf.Clamp01(w.t / w.tot), w.seed);
            for (int i = n; i < _w984Q.Count; i++) if (_w984Q[i]) _w984Q[i].enabled = false;
        }
        void Q984(int slot, int shape, Vector2 a, float R, float sq, float hw, float deg, float half, float p, int seed)
        {
            while (_w984Q.Count <= slot) _w984Q.Add(null);
            var sr = _w984Q[slot];
            if (sr == null)
            {
                var go = new GameObject("bosswarn984_" + slot); go.transform.SetParent(_room != null ? _room.Root : null, false);
                sr = go.AddComponent<SpriteRenderer>(); sr.sprite = _bsQuad950; sr.sharedMaterial = _bsWarn950; _w984Q[slot] = sr;
            }
            sr.sortingOrder = HellordTuning.I.bossWarnOrder948; sr.enabled = true;
            float ext = (shape == 2 ? R : R + hw) + 4f, hx = ext, hy = shape == 2 ? ext : ext * sq + 4f;
            sr.transform.position = new Vector3(Mathf.Round(a.x * HArt.PPU) / HArt.PPU, Mathf.Round(a.y * HArt.PPU) / HArt.PPU, 0);
            sr.transform.localScale = new Vector3(2f * hx / HArt.PPU, 2f * hy / HArt.PPU, 1);
            var m = _bsMpb950; sr.GetPropertyBlock(m);
            m.SetFloat("_Shape", shape); m.SetVector("_A", a); m.SetVector("_B", a); m.SetVector("_Ctr", a);
            m.SetFloat("_R", R); m.SetFloat("_HW", hw); m.SetFloat("_Deg", deg); m.SetFloat("_Half", half); m.SetFloat("_Gap", 0); m.SetFloat("_Sq", sq); m.SetFloat("_Ym", 1);
            m.SetFloat("_P", Mathf.Clamp01(p)); m.SetFloat("_Seed", seed * 0.37f);
            if (IsHorse) { m.SetFloat("_Mode", 3); m.SetFloat("_Row", 6); m.SetFloat("_RampN", 4); m.SetFloat("_Depth", 10); m.SetFloat("_WindAng", shape == 2 ? 1 : 0); m.SetFloat("_WindDeg", deg); }
            else { m.SetFloat("_Mode", 2); m.SetFloat("_Row", IsGorilla ? 3 : IsQilin ? 4 : 2); m.SetFloat("_RampN", 6); m.SetFloat("_Depth", 10); m.SetFloat("_WindAng", 0); }
            sr.SetPropertyBlock(m);
        }
    }
}
