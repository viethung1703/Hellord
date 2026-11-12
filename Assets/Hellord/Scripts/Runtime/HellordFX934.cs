// t934u v0.0.79: Nezha #19 Fire Wheel mechanic + VH-approved shader batch 5/6 wiring (PixelFX934 / PixelShock934).
// Every switch in HellordTuning (fx...934 / sin19...934) falls back to the old FX/mechanic when off.
using PixelFXPack;
using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    public partial class HellordGame
    {
        class Q934 { public SpriteRenderer sr; public Material m; public bool shock, on; public int mode; public float age, life; public Vector2 at; public Proj proj; public float flash; public System.Action<Q934> tick; }
        readonly List<Q934> _q934 = new List<Q934>();
        Shader _sh934, _shk934; bool _tried934, _ok934, _okShock934; Sprite _quad934;
        public static string Fx934Status = "not loaded";
        static readonly int P934Life = Shader.PropertyToID("_Life"), P934Center = Shader.PropertyToID("_Center"), P934Age = Shader.PropertyToID("_Age");

        bool Fx934Ok(bool shock = false)
        {
            if (!HellordTuning.I.fxShaderOn) return false;
            if (!_tried934)
            {
                _tried934 = true;
                _sh934 = Resources.Load<Shader>("HellordShaders/HellordPixelFX934"); if (_sh934 == null) _sh934 = Shader.Find("Hellord/PixelFX934");
                _shk934 = Resources.Load<Shader>("HellordShaders/HellordPixelShock934"); if (_shk934 == null) _shk934 = Shader.Find("Hellord/PixelShock934");
                _ok934 = _sh934 != null && _sh934.isSupported; _okShock934 = _shk934 != null && _shk934.isSupported;
                var tex = new Texture2D(4, 4, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point }; var px = new Color32[16]; for (int i = 0; i < 16; i++) px[i] = new Color32(255, 255, 255, 255); tex.SetPixels32(px); tex.Apply();
                _quad934 = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
                { var pipe = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline; bool urp = pipe != null; Fx934Status = "fx " + (_ok934 ? "ok" : "missing") + ", shock " + (!_okShock934 ? "missing" : urp ? "skipped-URP(GrabPass)" : "ok") + ", pipeline " + (urp ? pipe.name : "Built-in"); }
                Debug.Log("[t934u] PixelFX934 " + Fx934Status);
            }
            return shock ? _okShock934 : _ok934;
        }
        Q934 Q934Get(bool shock, int mode, Vector2 at, float wU, float hU, float life, int order)
        {
            if (!Fx934Ok(shock)) return null; var T = HellordTuning.I;
            Q934 q = null; foreach (var o in _q934) if (!o.on && o.shock == shock) { q = o; break; }
            if (q == null)
            {
                if (_q934.Count >= Mathf.Max(8, T.fxPool934)) return null;
                q = new Q934 { shock = shock }; q.sr = new GameObject("fx934_" + (shock ? "shock" : "q")).AddComponent<SpriteRenderer>(); q.sr.sprite = _quad934;
                q.m = new Material(shock ? _shk934 : _sh934); q.sr.sharedMaterial = q.m; _q934.Add(q);
                if (!shock) { Tex928(q.m, "_NoiseTex", T.fxNoiseTex928); Tex928(q.m, "_CellTex", T.fxCellTex928); q.m.SetFloat("_TimeStep", T.fxTimeStep928); }
                q.m.SetFloat("_PPU", HArt.PPU);
            }
            q.on = true; q.mode = mode; q.age = 0; q.life = life; q.at = at; q.proj = null; q.flash = 0; q.tick = null;
            q.sr.gameObject.SetActive(true); q.sr.sortingOrder = order; q.sr.transform.position = at; q.sr.transform.localScale = new Vector3(wU, hU, 1);
            q.m.SetVector(P934Center, at); if (!shock) { q.m.SetFloat("_Mode", mode); q.m.SetFloat("_Seed", Random.value); q.m.SetFloat("_Squash", T.fxSquash928); q.m.SetFloat(P934Life, 0); q.m.SetFloat("_Bias", 0); }
            return q;
        }
        void Fx934Tick(float dt)
        {
            for (int i = 0; i < _q934.Count; i++)
            {
                var q = _q934[i]; if (!q.on) continue; q.age += dt;
                if (q.age >= q.life) { q.on = false; q.sr.gameObject.SetActive(false); continue; }
                if (q.shock) q.m.SetFloat(P934Age, Mathf.Floor(q.age * 30f)); else q.m.SetFloat(P934Life, q.age / Mathf.Max(0.01f, q.life));
                q.tick?.Invoke(q);
            }
            Nezha934Tick(dt);
        }
        public static int ShockLive934Count; // t991u FPS probe
        int CountShock934() { int n = 0; foreach (var o in _q934) if (o.on && o.shock) n++; return ShockLive934Count = n; }

        /// <summary>PixelShock934 ground ripple (displacement only, no colour). rPx = full radius, r0 = start fraction, lifeS seconds.</summary>
        void Shock934(Vector2 at, float rPx, float r0, float lifeS, float amp, float widthPx, bool inward = false, float squash = -1f)
        {
            var T = HellordTuning.I; if (!T.fxShock934) return;
            // t1002u: GrabPass PixelShock is Built-in only. Under URP/SRP GrabPass fails (VH console) — skip until a URP-safe rewrite.
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null) return;
            // t988u FPS: PixelShock uses a named GrabPass (full-screen copy while any shock is live). Cap concurrent shocks.
            if (T.fxShockMax988 > 0)
            {
                int live = 0; foreach (var o in _q934) if (o.on && o.shock) live++;
                if (live >= T.fxShockMax988) return;
            }
            float sq = squash > 0 ? squash : T.fxShockSquash934;
            float w = (rPx * 1.2f + widthPx + amp + 2) * 2f / HArt.PPU;
            var q = Q934Get(true, 20, at, w, w * sq + 0.3f, lifeS, T.fxShockOrder934); if (q == null) return;
            q.m.SetFloat("_RadiusPx", rPx); q.m.SetFloat("_R0", r0); q.m.SetFloat("_Squash", sq); q.m.SetFloat("_LifeF", Mathf.Max(1f, lifeS * 30f)); q.m.SetFloat("_Amp", amp); q.m.SetFloat("_Width", widthPx); q.m.SetFloat("_Inward", inward ? 1 : 0); q.m.SetFloat(P934Age, 0);
        }

        // ================= Hellfire Pillar (mode 10 tell + crust, PixelShock impact) =================
        void PillarTell934(Vector2 at, float r)
        {
            var T = HellordTuning.I; HeroSkillFX.PillarGpuGround = T.fxPillarGround934 && Fx934Ok();
            if (!HeroSkillFX.PillarGpuGround) return; float rPx = r * HArt.PPU, s = r * 2f + 0.6f;
            var q = Q934Get(false, 10, at, s, s * T.fxSquash928 + 0.4f, T.pillarTellSeconds, T.fxShaderOrder928 + 2); if (q == null) return;
            q.m.SetFloat("_RadiusPx", rPx); q.m.SetFloat("_Phase", 0);
        }
        void PillarImpact934(Vector2 at, float r)
        {
            var T = HellordTuning.I; float rPx = r * HArt.PPU;
            Shock934(at, rPx * T.fxPillarShockR1_934, T.fxPillarShockR0_934 / Mathf.Max(0.01f, T.fxPillarShockR1_934), T.fxPillarShockS934, T.fxPillarShockAmp934, T.fxPillarShockW934);
            if (!HeroSkillFX.PillarGpuGround) return; float s = r * 2f + 0.6f;
            var q = Q934Get(false, 10, at, s, s * T.fxSquash928 + 0.4f, T.fxPillarCrustS934, T.fxShaderOrder928 + 2); if (q == null) return;
            q.m.SetFloat("_RadiusPx", rPx); q.m.SetFloat("_Phase", 2);
            q.tick = o => { float e = o.age; float sk = e < 1.4f ? 1 : Mathf.Max(0, 1 - (e - 1.4f) / 0.8f); o.m.SetFloat("_Grow", sk); o.m.SetFloat(P934Life, Mathf.Clamp01((e - 0.85f) / 1.2f)); };
            q.tick(q);
        }

        // ================= #19 Nezha Fire Wheel =================
        class Trail934 { public Vector2 a, b; public float t, life, w, tick, dmg; public SkillInst s; public Q934 q; }
        readonly List<Trail934> _trail934 = new List<Trail934>();
        readonly Dictionary<int, float> _wheelHit934 = new Dictionary<int, float>();
        Trail934 _trailCur934; Q934 _wheelB934, _wheelF934; float _wheelSpin934;
        bool Nezha934 => SinOn818(19) && ST737.sin19Wheel934;
        /// <summary>Called from SinDash818 at dash start. Returns true when the Fire Wheel replaces the old burn patch.</summary>
        bool NezhaDashStart934()
        {
            if (!Nezha934) return false; var T = ST737; var fb = SinInst818(SkillId.Fireball); float area = Mathf.Max(0.1f, fb.AreaMul);
            _wheelHit934.Clear();
            _trailCur934 = new Trail934 { a = _heroPos, b = _heroPos, life = Mathf.Max(0.3f, T.sin19TrailS934) * SinDuration737, w = T.sin19TrailW934 * area, dmg = SkillDamage(fb) * P818(19, 1), s = fb };
            _trailCur934.t = _trailCur934.life; _trail934.Add(_trailCur934);
            if (T.fxNezha934 && Fx934Ok())
            {
                float rU = T.sin19WheelR934 * area * 0.5f, s = rU * 2f + 0.5f;
                _wheelB934 = Q934Get(false, 11, _heroPos, s, s, 9f, 0); _wheelF934 = Q934Get(false, 11, _heroPos, s, s, 9f, 0);
                foreach (var q in new[] { _wheelB934, _wheelF934 }) if (q != null) { q.m.SetFloat("_RadiusPx", rU * HArt.PPU); q.m.SetFloat("_Squash", T.fxNezhaSquash934); }
                if (_wheelB934 != null) _wheelB934.m.SetFloat("_Side", 0); if (_wheelF934 != null) _wheelF934.m.SetFloat("_Side", 1);
                var tq = Q934Get(false, 21, _heroPos, 1, 1, _trailCur934.life, T.fxShaderOrder928 + 3); _trailCur934.q = tq; if (tq != null) tq.m.SetFloat("_TW", _trailCur934.w * HArt.PPU);
            }
            return true;
        }
        void Nezha934Tick(float dt)
        {
            var T = ST737; bool dashing = _dashT49 > 0 && _trailCur934 != null && Nezha934;
            if (dashing)
            {
                var fb = _trailCur934.s; float area = Mathf.Max(0.1f, fb.AreaMul), R = T.sin19WheelR934 * area * 0.5f, dmg = SkillDamage(fb) * T.sin19WheelMul934;
                _trailCur934.b = _heroPos; Query(_heroPos, R, _sinQ818);
                foreach (int i in _sinQ818)
                {
                    var e = _en[i]; if (_wheelHit934.TryGetValue(e.id, out float until) && until > runTime) continue; _wheelHit934[e.id] = runTime + T.sin19ReHit934;
                    Vector2 dir = e.pos - _heroPos; if (dir.sqrMagnitude < 1e-4f) dir = _dashDir49; Hit(e, dmg, fb, dir.normalized * T.sin19Knock934 * 8f);
                }
                _wheelSpin934 += dt * T.fxNezhaSpin934 * (_dashDir49.x >= 0 ? -1 : 1);
                Vector2 c = _heroPos + Vector2.up * T.fxNezhaY934;
                foreach (var q in new[] { _wheelB934, _wheelF934 }) if (q != null && q.on) { q.at = c; q.sr.transform.position = c; q.m.SetVector(P934Center, c); q.m.SetFloat("_Spin", _wheelSpin934); q.life = q.age + 0.2f; }
                if (_wheelB934 != null) _wheelB934.sr.sortingOrder = YSort(_heroPos.y) - 2; if (_wheelF934 != null) _wheelF934.sr.sortingOrder = YSort(_heroPos.y) + 2;
            }
            else if (_trailCur934 != null)
            {
                _trailCur934 = null; foreach (var q in new[] { _wheelB934, _wheelF934 }) if (q != null) { q.on = false; q.sr.gameObject.SetActive(false); } _wheelB934 = _wheelF934 = null;
            }
            for (int i = _trail934.Count - 1; i >= 0; i--)
            {
                var z = _trail934[i]; z.t -= dt; if (z.t <= 0) { if (z.q != null && z.q.on) { z.q.on = false; z.q.sr.gameObject.SetActive(false); } _trail934.RemoveAt(i); continue; }
                if (z.q != null && z.q.on)
                {
                    Vector2 mid = (z.a + z.b) * 0.5f, ext = new Vector2(Mathf.Abs(z.b.x - z.a.x), Mathf.Abs(z.b.y - z.a.y)) + Vector2.one * (z.w + 0.4f);
                    z.q.sr.transform.position = mid; z.q.sr.transform.localScale = new Vector3(ext.x, ext.y, 1); z.q.m.SetVector("_A", z.a); z.q.m.SetVector("_B", z.b); z.q.life = z.q.age + z.t + 0.05f;
                    z.q.m.SetFloat(P934Life, 1f - z.t / z.life);
                }
                else if (!T.fxNezha934 || !Fx934Ok()) ZSeg(z.a, z.b, z.w, new Color(1f, 0.4f, 0.08f, 0.25f), 1); // fallback look
                if ((z.tick -= dt) <= 0)
                {
                    z.tick = Mathf.Max(0.05f, T.sin19Tick818); Vector2 ab = z.b - z.a; float L = ab.magnitude; Query((z.a + z.b) * 0.5f, L * 0.5f + z.w, _sinQ818);
                    foreach (int k in _sinQ818) { var e = _en[k]; float s = L > 1e-4f ? Mathf.Clamp01(Vector2.Dot(e.pos - z.a, ab) / (L * L)) : 0; if ((e.pos - (z.a + ab * s)).magnitude <= z.w * 0.5f + e.radius * 0.5f) Hit(e, z.dmg * z.tick, z.s, Vector2.zero, true); }
                }
            }
        }

        // ================= #30 Nine-Forked split node, #44 Tribulation v2 =================
        void SplitNode934(Vector2 at)
        {
            var T = HellordTuning.I; if (!T.fxNineFork934) return; float s = T.fxNodeRPx934 * 2f / HArt.PPU + 0.4f;
            var q = Q934Get(false, 13, at, s, s, T.fxNodeS934, T.fxCityFrontOrder930); if (q == null) return; q.m.SetFloat("_RadiusPx", T.fxNodeRPx934); q.m.SetFloat("_Squash", 1f);
        }
        void Tribulation934(Vector2 at, float rU)
        {
            var T = HellordTuning.I; if (!T.fxTrib934) return;
            Shock934(at, T.fxTribShockRPx934, 0f, T.fxTribShockF934 / 30f, T.fxTribShockAmp934, T.fxTribShockW934, false, T.fxTribSquash934);
            HordeFX.CheapBurst(at + Vector2.up * 0.2f, new Color(0.55f, 0.6f, 0.7f), T.fxTribChips934);
            float s = rU * 2f + 0.4f; var q = Q934Get(false, 14, at, s, s * T.fxSquash928 + 0.3f, T.fxTribScorchS934, T.fxShaderOrder928 + 2); if (q == null) return; q.m.SetFloat("_RadiusPx", rU * HArt.PPU);
        }

        // ================= #14 Death Bell v2 (no rings: inward tremor -> outward shock) =================
        bool Bell934(Vector2 at, int radiusPx, float seconds, System.Action onCast)
        {
            var T = HellordTuning.I; if (!T.fxBell934 || !Fx934Ok(true)) return false; float toll = seconds * T.fxBellFlareK930;
            Shock934(at, radiusPx, 0.15f, toll, T.fxBellTollAmp934, T.fxBellTollW934, true);
            HordeFX.CheapBurst(at + Vector2.up * 0.2f, new Color(0.92f, 0.83f, 0.67f), T.fxBellMotes934);
            After(toll, () =>
            {
                onCast?.Invoke(); if (mode != Mode.Play && mode != Mode.LevelUp) return;
                Shock934(at, radiusPx, 0f, T.fxBellGongF934 / 30f, T.fxBellGongAmp934, T.fxBellGongW934);
                HordeFX.CheapBurst(at + Vector2.up * 0.2f, new Color(0.92f, 0.83f, 0.67f), T.fxBellChips934);
                float rU = radiusPx / (float)HArt.PPU; Query(at, rU, _sinQ818);
                foreach (int i in _sinQ818) { var e = _en[i]; if (!e.alive || e.elite || e.boss) continue; Vector2 d = e.pos - at; if (d.sqrMagnitude < 1e-4f) d = Vector2.up; e.push += d.normalized * (T.fxBellJoltPx934 / HArt.PPU) * 8f; }
            });
            return true;
        }

        // ================= #38 Golden Crow v2 sun (mode 15) =================
        void Sun934(Proj p)
        {
            var T = HellordTuning.I; if (p == null || !T.fxSun934) return; float s = 14f * 1.6f * 2f / HArt.PPU;
            var q = Q934Get(false, 15, p.pos, s * 2.2f, s * 2.2f, 999f, T.fxSunOrder934); if (q == null) return;
            q.proj = p; if (p.fx44 >= 0) { HeroSkillFX.SpearEnd(p.fx44); p.fx44 = -1; } if (p.trail != null) { HellFXRunner.I.Kill(p.trail); p.trail = null; } if (p.sr) p.sr.enabled = false;
            q.tick = o =>
            {
                var pp = o.proj; if (pp == null || !pp.alive || pp.sin818 != 3) { o.on = false; o.sr.gameObject.SetActive(false); return; }
                float sc = 1.6f + 0.05f * pp.sinN818, rPx = T.fxSunRPx934 * sc; o.life = o.age + 1f; o.at = pp.pos; o.sr.transform.position = pp.pos;
                float w = rPx * 2f * 2.4f / HArt.PPU; o.sr.transform.localScale = new Vector3(w, w, 1);
                o.m.SetVector(P934Center, pp.pos); o.m.SetFloat("_RadiusPx", rPx); o.m.SetFloat("_Squash", 1f); Vector2 v = pp.vel.sqrMagnitude > 1e-6f ? pp.vel.normalized : Vector2.right; o.m.SetVector("_A", new Vector4(v.x, v.y, 0, 0));
                o.flash -= Time.deltaTime; o.m.SetFloat("_Bias", o.flash > 0 ? 1 : 0);
            };
            q.tick(q);
        }
        void SunPierce934(Proj p) { foreach (var q in _q934) if (q.on && q.proj == p && q.mode == 15) q.flash = HellordTuning.I.fxSunFlashS934; }

        // ================= Death Burst (mode 17) + Hit shards (mode 19) =================
        void DeathBurst934(Vector2 at)
        {
            var T = HellordTuning.I; if (!T.fxDeathBurst934) return; float s = T.fxGoreRPx934 * 2.6f / HArt.PPU;
            var q = Q934Get(false, 17, at, s, s, T.fxGoreS934, T.fxShaderOrder928 + 4); if (q == null) return; q.m.SetFloat("_RadiusPx", T.fxGoreRPx934); q.m.SetFloat("_Squash", 1f);
            var pop = Q934Get(false, 19, at, 1.4f, 1.4f, T.fxGorePopF934 / 30f, T.fxHitOrder934); if (pop != null) pop.m.SetFloat("_Tick", 1); // 2-frame pop
        }
        float _shardT934; int _shardN934;
        void HitShards934(Vector2 at, bool crit)
        {
            var T = HellordTuning.I; if (!T.fxHitShards934 || (!crit && !T.fxHitShardsNormal934)) return;
            if (Time.unscaledTime - _shardT934 > 0.05f) { _shardT934 = Time.unscaledTime; _shardN934 = 0; } if (++_shardN934 > T.fxHitShardCap934) return;
            var q = Q934Get(false, 19, at, 1.6f, 1.6f, T.fxHitShardS934, T.fxHitOrder934); if (q == null) return; q.m.SetFloat("_Tick", crit ? 1 : 0); q.m.SetFloat("_Squash", 1f);
        }
    }
}
