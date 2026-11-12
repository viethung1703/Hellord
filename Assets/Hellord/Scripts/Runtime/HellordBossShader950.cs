using System.Collections.Generic;
using UnityEngine;

// t950/t970u: VH-approved boss shader FX (fx_audit/boss_shader/wire). Behind HellordTuning.bossShaderTells950 (ON).
// Swaps the shader onto the existing TI46 tell quads (same bbox, same sorting under the boss, same hitboxes/timings - only the look changes),
// draws Ox/Gorilla/Qilin/Horse boss bullets as PixelBossOrb quads (positions/hits untouched), and the Horse-Face rice sack gets the slate tint + wind wrap.
// Skipped on purpose (VH): Frost Ox only. t986u: Gorilla Shard Fan KCone + Flame Breath v2 numbers now ON.
namespace Hellord
{
    public partial class HellordGame
    {
        bool _bs950Tried; Material _bsWarn950, _bsOrb950, _bsTint950, _bsWrap950, _bsGround950; Sprite _bsQuad950; Material _bsSprite950; MaterialPropertyBlock _bsMpb950;
        SpriteRenderer _bsBreathSr950; // t986u Qilin breath flame ground quad
        public int bossShaderTellFrames950, bossShaderOrbs950, bossShaderSacks950;
        readonly Dictionary<BBullet, SpriteRenderer> _bsOrbs950 = new Dictionary<BBullet, SpriteRenderer>();
        readonly Stack<SpriteRenderer> _bsOrbPool950 = new Stack<SpriteRenderer>();

        Material Bs950Mat(string res, string find)
        {
            var sh = Resources.Load<Shader>("HellordShaders/" + res); if (sh == null) sh = Shader.Find(find);
            if (sh == null || !sh.isSupported) { Debug.LogWarning("[t950] boss shader missing/unsupported: " + res); return null; }
            var m = new Material(sh) { name = res + "_950" };
            Tex928(m, "_RampTex", "fx950_boss_ramps"); if (m.HasProperty("_NoiseTex")) Tex928(m, "_NoiseTex", "fx928_noise128");
            if (m.HasProperty("_RampRows")) m.SetFloat("_RampRows", 20); if (m.HasProperty("_PPU")) m.SetFloat("_PPU", HArt.PPU); if (m.HasProperty("_Fps")) m.SetFloat("_Fps", 30);
            if (m.HasProperty("_Dim")) m.SetFloat("_Dim", HellordTuning.I.bossWarnDim985 ? 1 : 0); // t975u
            return m;
        }
        bool Bs950Ok
        {
            get
            {
                if (!HellordTuning.I.bossShaderTells950) return false;
                if (!_bs950Tried)
                {
                    _bs950Tried = true; _bsMpb950 = new MaterialPropertyBlock();
                    _bsWarn950 = Bs950Mat("HellordPixelBossWarn", "Hellord/PixelBossWarn"); _bsOrb950 = Bs950Mat("HellordPixelBossOrb", "Hellord/PixelBossOrb");
                    _bsTint950 = Bs950Mat("HellordPixelBossSackTint", "Hellord/PixelBossSackTint"); _bsWrap950 = Bs950Mat("HellordPixelBossSackWrap", "Hellord/PixelBossSackWrap");
                    _bsGround950 = Bs950Mat("HellordPixelBossGround", "Hellord/PixelBossGround");
                    var tx = new Texture2D(1, 1, TextureFormat.RGBA32, false); tx.SetPixel(0, 0, Color.white); tx.Apply();
                    _bsQuad950 = Sprite.Create(tx, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f, 0, SpriteMeshType.FullRect);
                }
                return _bsWarn950 != null;
            }
        }

        /// <summary>Called first in Render46: true = the shader drew this tell (CPU pixels skipped).</summary>
        bool Bs950Tell(TI46 t, Vector3 pos)
        {
            if (!Bs950Ok || IsFrostOx) { if (_bsWarn950 != null && t.sr.sharedMaterial == _bsWarn950) { t.sr.sharedMaterial = _bsSprite950; t.sr.SetPropertyBlock(null); } return false; } // t986u: Gorilla KCone (Shard Fan) now uses PixelBossWarn
            if (t.rel >= 0) { t.sr.enabled = false; return true; } // released: the hit itself (shock/dust) is the read, as in the mock-ups
            if (t.sr.sharedMaterial != _bsWarn950) { if (_bsSprite950 == null) _bsSprite950 = t.sr.sharedMaterial; t.sr.sharedMaterial = _bsWarn950; }
            var m = _bsMpb950; t.sr.GetPropertyBlock(m);
            Vector2 a = new Vector2(t.ox, t.oy) / Px46; bool horse = IsHorse;
            int shape = 0; float R = t.a1, HW = 0, deg = 0, half = 0, gap = 0, ym = 1; Vector2 b = a;
            if (t.kind == KLane) { shape = 1; float r = t.a3 * Mathf.Deg2Rad; b = a + new Vector2(Mathf.Cos(r), Mathf.Sin(r)) * t.a1 / Px46; HW = t.a2; deg = t.a3; }
            else if (t.kind == KCone) { shape = 2; deg = t.a2; half = t.a3; if (IsQilin) ym = 0.35f; }
            else if (t.kind == KRing) { shape = 4; HW = 9.6f; if (t.a3 > 0.5f) { gap = 1; deg = t.a2; half = t.a3; } }
            m.SetFloat("_Shape", shape); m.SetVector("_A", a); m.SetVector("_B", b); m.SetVector("_Ctr", a);
            m.SetFloat("_R", R); m.SetFloat("_HW", HW); m.SetFloat("_Deg", deg); m.SetFloat("_Half", half); m.SetFloat("_Gap", gap); m.SetFloat("_Sq", 1); m.SetFloat("_Ym", ym);
            m.SetFloat("_P", Mathf.Clamp01(t.p)); m.SetFloat("_Seed", (t.GetHashCode() & 15) * 0.37f);
            if (horse) { m.SetFloat("_Mode", 3); m.SetFloat("_Row", 6); m.SetFloat("_RampN", 4); m.SetFloat("_Depth", 10); m.SetFloat("_WindAng", shape == 1 || shape == 2 ? 1 : 0); m.SetFloat("_WindDeg", deg); }
            else { m.SetFloat("_Mode", 2); m.SetFloat("_Row", t.pal == 1 ? 3 : t.pal == 2 ? 4 : 2); m.SetFloat("_RampN", 6); m.SetFloat("_Depth", 10); m.SetFloat("_WindAng", 0); }
            t.sr.SetPropertyBlock(m);
            t.sr.transform.position = pos; t.sr.enabled = true; bossShaderTellFrames950++;
            return true;
        }

        bool Bs950Bullets => Bs950Ok && _bsOrb950 != null && !IsFrostOx;
        void Bs950Frame()
        {
            Bs984Frame(); // t974u
            if (_bsOrbs950.Count > 0)
            {
                List<BBullet> dead = null;
                foreach (var kv in _bsOrbs950)
                {
                    var b = kv.Key; var sr = kv.Value; if (!b.alive || sr == null) { (dead ??= new List<BBullet>()).Add(b); continue; }
                    sr.transform.position = new Vector3(Mathf.Round(b.pos.x * HArt.PPU) / HArt.PPU, Mathf.Round(b.pos.y * HArt.PPU) / HArt.PPU, 0);
                    var m = _bsMpb950; sr.GetPropertyBlock(m); m.SetVector("_Pos", b.pos); m.SetVector("_Vel", b.vel);
                    m.SetFloat("_Ang", Mathf.Atan2(b.vel.y, b.vel.x) * Mathf.Rad2Deg); sr.SetPropertyBlock(m);
                }
                if (dead != null) foreach (var b in dead) Bs950Release(b);
            }
            if (_sacks53.Count > 0 && Bs950Ok && _bsTint950 != null) foreach (var s in _sacks53) if (s.sr) Bs950Sack(s);
        }
        void Bs950Orb(BBullet b, int idx)
        {
            var sr = _bsOrbPool950.Count > 0 ? _bsOrbPool950.Pop() : null;
            if (sr == null) { var go = new GameObject("bossorb950"); go.transform.SetParent(_room != null ? _room.Root : null, false); sr = go.AddComponent<SpriteRenderer>(); sr.sprite = _bsQuad950; sr.sharedMaterial = _bsOrb950; }
            sr.gameObject.SetActive(true); sr.sortingOrder = 2001;
            var m = _bsMpb950; m.Clear(); bool horse = IsHorse; float rpx = BBulletR * HArt.PPU;
            m.SetFloat("_Mode", horse ? 1 : 0); m.SetFloat("_Row", IsGorilla ? 9 : IsQilin ? 11 : 7); m.SetFloat("_Rpx", rpx); m.SetFloat("_I", idx);
            m.SetFloat("_RX", 3.5f); m.SetFloat("_RY", 2.2f); m.SetFloat("_Ntr", 1); m.SetVector("_Pos", b.pos); m.SetVector("_Vel", b.vel);
            sr.SetPropertyBlock(m);
            float px = horse ? 2 * (3.5f + 22f) : 2 * (rpx + 6f); sr.transform.localScale = new Vector3(px / HArt.PPU, px / HArt.PPU, 1);
            _bsOrbs950[b] = sr; bossShaderOrbs950++;
        }
        void Bs950Release(BBullet b)
        {
            if (!_bsOrbs950.TryGetValue(b, out var sr)) return; _bsOrbs950.Remove(b);
            if (sr != null) { sr.gameObject.SetActive(false); _bsOrbPool950.Push(sr); }
        }
        void Bs950ClearOrbs() { foreach (var kv in _bsOrbs950) if (kv.Value) { kv.Value.gameObject.SetActive(false); _bsOrbPool950.Push(kv.Value); } _bsOrbs950.Clear(); }

        void Bs950Sack(Sack53 s)
        {
            var sr = s.sr; if (sr.sharedMaterial != _bsTint950) { sr.sharedMaterial = _bsTint950; bossShaderSacks950++; }
            if (_bsWrap950 == null) return;
            var tr = sr.transform; Transform back = tr.Find("wrap950_0"), front = tr.Find("wrap950_1");
            if (back == null)
            {
                for (int f = 0; f < 2; f++)
                {
                    var go = new GameObject("wrap950_" + f); go.transform.SetParent(tr, false); var q = go.AddComponent<SpriteRenderer>();
                    q.sprite = _bsQuad950; q.sharedMaterial = _bsWrap950; q.sortingOrder = sr.sortingOrder + (f == 0 ? -1 : 1);
                    go.transform.localScale = new Vector3(150f / HArt.PPU, 110f / HArt.PPU, 1);
                }
                back = tr.Find("wrap950_0"); front = tr.Find("wrap950_1");
            }
            float k = Mathf.Clamp01(s.t / Mathf.Max(0.01f, s.dur)); Vector2 vel = (s.b - s.a) / Mathf.Max(0.01f, s.dur) + Vector2.up * (4f * s.h * (1f - 2f * k) / Mathf.Max(0.01f, s.dur));
            for (int f = 0; f < 2; f++)
            {
                var q = (f == 0 ? back : front).GetComponent<SpriteRenderer>(); var m = _bsMpb950; q.GetPropertyBlock(m);
                m.SetFloat("_Front", f); m.SetVector("_Ctr", (Vector2)tr.position); m.SetVector("_Vel", vel); q.SetPropertyBlock(m);
            }
        }

        /// <summary>t986u: PixelBossGround mode 3 flame wedge under the Qilin breath. Call each breath active frame; pass on=false to hide.</summary>
        public void Bs950Breath(bool on, Vector2 origin, float curDeg, float reachU, float halfDeg)
        {
            if (!HellordTuning.I.bossBreathGround986 || !Bs950Ok || _bsGround950 == null) { if (_bsBreathSr950) _bsBreathSr950.enabled = false; return; }
            if (_bsBreathSr950 == null)
            {
                var go = new GameObject("bossbreath950"); go.transform.SetParent(_room != null ? _room.Root : null, false);
                _bsBreathSr950 = go.AddComponent<SpriteRenderer>(); _bsBreathSr950.sprite = _bsQuad950; _bsBreathSr950.sharedMaterial = _bsGround950;
                _bsBreathSr950.sortingOrder = HellordTuning.I.bossWarnOrder948;
            }
            if (!on) { _bsBreathSr950.enabled = false; return; }
            float rPx = reachU * HArt.PPU; float side = (rPx * 2f + 8f) / HArt.PPU;
            _bsBreathSr950.transform.position = origin; _bsBreathSr950.transform.localScale = new Vector3(side, side, 1);
            var m = _bsMpb950; _bsBreathSr950.GetPropertyBlock(m);
            m.SetFloat("_Mode", 3); m.SetVector("_Ctr", origin); m.SetFloat("_R", rPx); m.SetFloat("_Deg", curDeg); m.SetFloat("_HF", halfDeg);
            m.SetFloat("_Age", 0); m.SetFloat("_Life", 40); m.SetFloat("_Seed", 0.37f);
            _bsBreathSr950.SetPropertyBlock(m); _bsBreathSr950.enabled = true;
        }
    }
}
