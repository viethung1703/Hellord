using System.Collections.Generic;
using UnityEngine;
using PixelFXPack;

// t586u (VH via Hellord Art, step38v option V2): white light on the hero and on every light source, full floor colour.
// The floor is no longer darkened by d2Ambient606 and the orange glow sprites are hidden. Instead, each frame a small CPU
// light mask is built (default 160x90 at a 640x360 view):
//   hero pool  = flat to v2HeroR0_586, then linear to black at v2HeroR1_586 (fractions of view width, y x v2YScale586)
//   torch pool = sum of k*(1-d/r)^1.3 over sources, saturated as 1-exp(-sum), so overlapping sources do not stack
//   L = amb + (1 - (1-hero)(1-torch))  (screen union, never above 1)
// The mask is drawn over the low-res world as dark colour with alpha (1-L), which equals world*L. A second white texture
// adds the small source cores. Both are drawn under the HUD.
// Gap vs a real light buffer: the mask is applied to the whole world (monsters too, like D2), at 1/4 res with bilinear
// filtering, and there is no occlusion or wall shadows.
namespace Hellord
{
    public partial class HellordGame
    {
        Texture2D _lm586, _lc586, _lt661; Color32[] _lmPx586, _lcPx586, _ltPx661;
        float[] _st586, _ss586, _sc586, _tw586, _tr586, _tg586, _tb586; // t991u stamp accum
        readonly List<Color> _scol661 = new List<Color>(); // t661u: per-source tint (a = 0 untinted)
        readonly List<Vector3> _src586 = new List<Vector3>(); // z = light weight (t660u)
        readonly List<Vector3> _flash660 = new List<Vector3>(); readonly List<Color> _flashC661 = new List<Color>(); // t660u impact flashes: xy world, z = time left; t661u colour
        int _flF677 = -1, _flN677; void Flash661(Vector2 p, Color c) { var T = HellordTuning.I; if (Time.frameCount != _flF677) { _flF677 = Time.frameCount; _flN677 = 0; } if (T.v2Light586 && T.v2ProjLight660 && _flash660.Count < 32 && _flN677++ < T.flashPerFrame677) { _flash660.Add(new Vector3(p.x, p.y, T.v2FlashS660)); _flashC661.Add(c); } }
        /// <summary>t661u (VH t591u): light colour per skill element; alpha 0 = NO light (Blood Whip, Skeleton Warriors / Archers, basic/physical).</summary>
        static Color ElemCol661(SkillInst s)
        {
            if (s == null || s.def == null) return Color.clear;
            switch (s.def.id)
            {
                case SkillId.Fireball: case SkillId.HellfirePillar: return new Color(1f, 0.55f, 0.2f, 1);   // fire warm
                case SkillId.FrostCleave: return new Color(0.5f, 0.8f, 1f, 1);                                  // ice cool
                case SkillId.StormRing: case SkillId.ChainLightning: return new Color(0.7f, 0.82f, 1f, 1);       // lightning cool blue-white
                case SkillId.BoneOrbit: return new Color(1f, 0.92f, 0.75f, 1);                                   // bone
                case SkillId.SoulSpears: return new Color(0.55f, 1f, 0.85f, 1);                                  // soul
                case SkillId.CurseRune: return new Color(0.75f, 0.4f, 1f, 1);                                    // curse / necro purple
                case SkillId.GravePool: return new Color(0.45f, 1f, 0.45f, 1);                                   // grave necro green
                case SkillId.Frostbolt: case SkillId.IceNova: case SkillId.FrostBomb: case SkillId.Hurricane: return new Color(0.5f, 0.8f, 1f, 1); // t1393u ice cool
                default: return Color.clear;
            }
        }
        void HitLight661(Enemy e, SkillInst s, bool tick)
        {
            if (tick) return; var c = ElemCol661(s); if (c.a <= 0) return; Vector2 hp = e.pos + Vector2.up * e.hitY; if (s.def.id == SkillId.HellfirePillar || s.def.id == SkillId.StormRing) c.a = HellordTuning.I.aoeFlashMul677; Flash661(hp, c); // t677u: c.a = flash weight
            if (Time.frameCount != _ihFrame667) { _ihFrame667 = Time.frameCount; _ihN667 = 0; }
            if (_ihN667++ < 24) { string h = ImpactHook667(s); if (h != null) SL667(h, hp); } // t667u: Hellord FX pixel light (cap 24 / frame)
        }
        // ===== t667u: SkillLightFX (PixelFXPack v2.14.0 wip) hooks
        /// <summary>t669u/t670u: VH paused the SkillLightFX v2.14.0 pixel-light look. Gate: off unless HellordTuning.skillLightFX667. Returns -1 (no-op handle) when off.</summary>
        static int SL667(string h, Vector2 p, Vector2 d = default) => HellordTuning.I != null && HellordTuning.I.skillLightFX667 ? Hook674(h, p, d) : -1;
        static int Hook674(string h, Vector2 p, Vector2 d) { SkillLightFX.Intensity = HellordTuning.I.skillLightIntensity674; return SkillLightFX.Hook(h, p, d); } // v0.0.64 t674u dimmer pack lights
        int _ihFrame667 = -1, _ihN667; readonly List<int> _orbLg667 = new List<int>();
        static string ImpactHook667(SkillInst s)
        {
            switch (s.def.id)
            {
                case SkillId.Fireball: return SkillLightHooks.FireballImpact; case SkillId.HellfirePillar: return SkillLightHooks.HellfirePillarImpact;
                case SkillId.FrostCleave: return SkillLightHooks.FrostCleaveImpact; case SkillId.StormRing: return SkillLightHooks.StormRingImpact;
                case SkillId.ChainLightning: return SkillLightHooks.ChainLightningImpact; case SkillId.BoneOrbit: return SkillLightHooks.BoneOrbitImpact;
                case SkillId.SoulSpears: return SkillLightHooks.SoulSpearsImpact; case SkillId.CurseRune: return SkillLightHooks.CurseRuneImpact;
                case SkillId.GravePool: return SkillLightHooks.GravePoolImpact; default: return null;
            }
        }
        static string FlightHook667(SkillInst s)
        {
            if (s == null || s.def == null) return null;
            switch (s.def.id) { case SkillId.Fireball: return SkillLightHooks.FireballFlight; case SkillId.SoulSpears: return SkillLightHooks.SoulSpearsFlight; case SkillId.BoneOrbit: return SkillLightHooks.MarrowLaunchFlight; default: return null; }
        }
        void UpdateOrbLight667()
        {   // one bone glow per live Bone Orbit orb
            int n = 0; foreach (var o in _orbs) if (o && o.gameObject.activeInHierarchy) n++;
            while (_orbLg667.Count > n) { SkillLightFX.GlowEnd(_orbLg667[_orbLg667.Count - 1]); _orbLg667.RemoveAt(_orbLg667.Count - 1); }
            int k = 0; foreach (var o in _orbs)
            {
                if (!o || !o.gameObject.activeInHierarchy) continue; Vector2 p = o.transform.position;
                if (k >= _orbLg667.Count) _orbLg667.Add(SL667(SkillLightHooks.BoneOrbitGlow, p));
                else if (_orbLg667[k] < 0) _orbLg667[k] = SL667(SkillLightHooks.BoneOrbitGlow, p); else SkillLightFX.GlowMove(_orbLg667[k], p);
                k++;
            }
        }

        void Hide586()
        {
            var T = HellordTuning.I; if (!T.v2Light586 || !atmosOn) return;
            foreach (var p in _firePools) if (p.sr) p.sr.enabled = false;
            if (_room != null) foreach (var l in _room.Lights) if (l.sr && l.warm) l.sr.enabled = false;
            for (int i = HellordRoom.Glow586.Count - 1; i >= 0; i--) { var g = HellordRoom.Glow586[i]; if (!g) { HellordRoom.Glow586.RemoveAt(i); continue; } g.enabled = false; }
        }

        // t991u/t992u: FPS probe counters (wall-clock ms of CPU light mask rebuild)
        public static float Light586MsLast, Light586MsAvg; public static int Light586SrcN, Light586Px; static float _lmMsSum; static int _lmMsN;
        void DrawLight586(Rect r)
        {
            var T = HellordTuning.I; if (!T.v2Light586 || !atmosOn || _rt == null || _cam == null || _room == null) { Light586MsLast = 0; Light586SrcN = 0; return; }
            var sw = System.Diagnostics.Stopwatch.StartNew();
            int div = Mathf.Max(1, T.v2MaskDiv586), W = Mathf.Max(8, _rt.width / div), H = Mathf.Max(8, _rt.height / div);
            int nPx = W * H;
            if (_lm586 == null || _lm586.width != W || _lm586.height != H)
            {
                _lm586 = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
                _lc586 = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
                _lmPx586 = new Color32[nPx]; _lcPx586 = new Color32[nPx];
                _lt661 = new Texture2D(W, H, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave }; _ltPx661 = new Color32[nPx];
                _st586 = new float[nPx]; _ss586 = new float[nPx]; _sc586 = new float[nPx]; _tw586 = new float[nPx]; _tr586 = new float[nPx]; _tg586 = new float[nPx]; _tb586 = new float[nPx];
            }
            else if (_st586 == null || _st586.Length != nPx)
            { _st586 = new float[nPx]; _ss586 = new float[nPx]; _sc586 = new float[nPx]; _tw586 = new float[nPx]; _tr586 = new float[nPx]; _tg586 = new float[nPx]; _tb586 = new float[nPx]; }
            // sources in mask px (viewport -> mask; texture row 0 = bottom, same as viewport y)
            _src586.Clear(); _scol661.Clear(); float margin = T.v2TorchR586 * 1.2f;
            void Add(Vector3 wp, float w = 1f, Color tc = default) { var v = _cam.WorldToViewportPoint(wp); if (v.x < -margin || v.x > 1 + margin || v.y < -margin * 2 || v.y > 1 + margin * 2) return; _src586.Add(new Vector3(v.x * W, v.y * H, w)); _scol661.Add(tc); }
            foreach (var p in _firePools) if (p.sr) Add(p.sr.transform.position);
            foreach (var l in _room.Lights) if (l.sr && l.warm) Add(l.sr.transform.position);
            foreach (var g in HellordRoom.Glow586) if (g && g.gameObject.activeInHierarchy) Add(g.transform.position);
            if (T.v2ProjLight660)
            {   // t660u: projectiles / arrows / impact flashes as light sources
                // t991u: cap dynamic skill lights — Fireball cast/hit used to push src to 47+ and O(W*H*S) the mask (~24ms)
                int dynCap = T.v2DynLightCap991 > 0 ? T.v2DynLightCap991 : 48;
                int pc = 0; foreach (var p in _pr) if (p != null && p.alive && pc < dynCap) { var c = ElemCol661(p.s); if (c.a > 0) { pc++; Add(p.pos, T.v2ProjK660, c); } }
                if (_orbs.Count > 0 && ElemCol661(Own53(SkillId.BoneOrbit)).a > 0) { var bc = ElemCol661(Own53(SkillId.BoneOrbit)); foreach (var o in _orbs) if (o && o.gameObject.activeInHierarchy && pc < dynCap) { pc++; Add(o.transform.position, T.v2OrbK661, bc); } }
                float dtf = Time.deltaTime; for (int i = _flash660.Count - 1; i >= 0; i--) { var q = _flash660[i]; q.z -= dtf; if (q.z <= 0) { _flash660.RemoveAt(i); _flashC661.RemoveAt(i); continue; } _flash660[i] = q; if (pc < dynCap) { pc++; Add(new Vector3(q.x, q.y, 0), T.v2FlashK660 * _flashC661[i].a * q.z / Mathf.Max(0.01f, T.v2FlashS660), _flashC661[i]); } }
            }
            var hv = _cam.WorldToViewportPoint(new Vector3(_heroPos.x, _heroPos.y, 0)); Vector2 hp = new Vector2(hv.x * W, hv.y * H);
            // t1041u: optional world-unit hero pool (stable across zoom / full-room). Torch pools stay view-fractional.
            float r0, r1;
            if (T.v2HeroWorld1041)
            {
                var e0 = _cam.WorldToViewportPoint(new Vector3(_heroPos.x + T.v2HeroR0Wu1041, _heroPos.y, 0));
                var e1 = _cam.WorldToViewportPoint(new Vector3(_heroPos.x + T.v2HeroR1Wu1041, _heroPos.y, 0));
                r0 = Mathf.Max(1f, Mathf.Abs(e0.x - hv.x) * W);
                r1 = Mathf.Max(r0 + 1f, Mathf.Abs(e1.x - hv.x) * W);
            }
            else { r0 = T.v2HeroR0_586 * W; r1 = Mathf.Max(r0 + 1, T.v2HeroR1_586 * W); }
            float ys = T.v2YScale586, tr = Mathf.Max(1, T.v2TorchR586 * W), cr = Mathf.Max(0.5f, tr * T.v2CoreR586), amb = T.v2Amb586 * (floorMode && Mortar52 ? T.mortarAmbK677 : 1f);
            float fl = 1 + Mathf.Min(T.flickerAmp, 0.08f) * 0.5f * AtmosFlicker(Time.time, 0.7f);
            var dc = T.v2DarkColor586; byte dr = (byte)(dc.r * 255), dg = (byte)(dc.g * 255), db = (byte)(dc.b * 255);
            float torchK = T.v2TorchK586, coreK = T.v2CoreK586, tintK = T.v2TintK661, heroDamp = T.skillHeroDamp677, skillCap = T.skillLightCap677;
            bool noSkillCore = T.noSkillCore677;
            bool stamp = T.v2LightStamp991;
            if (stamp)
            {
                System.Array.Clear(_st586, 0, nPx); System.Array.Clear(_ss586, 0, nPx); System.Array.Clear(_sc586, 0, nPx);
                System.Array.Clear(_tw586, 0, nPx); System.Array.Clear(_tr586, 0, nPx); System.Array.Clear(_tg586, 0, nPx); System.Array.Clear(_tb586, 0, nPx);
                float invYs = 1f / Mathf.Max(1e-4f, ys);
                int radX = Mathf.CeilToInt(tr), radY = Mathf.CeilToInt(tr * invYs);
                for (int i = 0; i < _src586.Count; i++)
                {
                    float sx = _src586[i].x, sy = _src586[i].y, w = _src586[i].z; var tc = _scol661[i]; bool tinted = tc.a > 0;
                    int x0 = Mathf.Max(0, (int)(sx - radX)), x1 = Mathf.Min(W - 1, (int)(sx + radX));
                    int y0 = Mathf.Max(0, (int)(sy - radY)), y1 = Mathf.Min(H - 1, (int)(sy + radY));
                    for (int y = y0; y <= y1; y++)
                    {
                        float dy = (y + 0.5f - sy) * ys; float dy2 = dy * dy;
                        int row = y * W;
                        for (int x = x0; x <= x1; x++)
                        {
                            float dx = x + 0.5f - sx; float d2 = dx * dx + dy2; if (d2 >= tr * tr) continue;
                            float d = Mathf.Sqrt(d2);
                            float u = 1f - d / tr; float cont = w * torchK * Mathf.Pow(u, 1.3f) * fl;
                            int k = row + x;
                            if (tinted) { _tw586[k] += cont; _ss586[k] += cont; _tr586[k] += cont * tc.r; _tg586[k] += cont * tc.g; _tb586[k] += cont * tc.b; }
                            else _st586[k] += cont;
                            if (d < cr && !(tinted && noSkillCore)) { float c = 1f - d / cr; _sc586[k] += Mathf.Min(1f, w) * c * c; }
                        }
                    }
                }
                for (int y = 0; y < H; y++) for (int x = 0; x < W; x++)
                {
                    float px = x + 0.5f, py = y + 0.5f;
                    float dh = Mathf.Sqrt((px - hp.x) * (px - hp.x) + (py - hp.y) * ys * (py - hp.y) * ys);
                    float lh = dh < r0 ? 1 - 0.15f * dh / r0 : Mathf.Clamp01(0.85f * (r1 - dh) / (r1 - r0));
                    int k = y * W + x;
                    float st = _st586[k], ss677 = _ss586[k], sc = _sc586[k], tw = _tw586[k], tr6 = _tr586[k], tg6 = _tg586[k], tb6 = _tb586[k];
                    float hd677 = 1 - lh * heroDamp, lt = 1 - (1 - (1 - Mathf.Exp(-st))) * (1 - Mathf.Min(skillCap, 1 - Mathf.Exp(-ss677)) * hd677), L = Mathf.Clamp01(amb + (1 - (1 - lh) * (1 - lt)) * (1 - amb));
                    _lmPx586[k] = new Color32(dr, dg, db, (byte)(255 * (1 - L)));
                    _lcPx586[k] = new Color32(255, 255, 255, (byte)(255 * Mathf.Clamp01(sc * coreK)));
                    _ltPx661[k] = tw > 1e-4f ? new Color32((byte)(255 * Mathf.Clamp01(tr6 / tw)), (byte)(255 * Mathf.Clamp01(tg6 / tw)), (byte)(255 * Mathf.Clamp01(tb6 / tw)), (byte)(255 * Mathf.Clamp01((1 - Mathf.Exp(-tw)) * tintK * hd677))) : new Color32(0, 0, 0, 0);
                }
            }
            else
            {   // legacy O(W*H*S) path (toggle off via v2LightStamp991)
                for (int y = 0; y < H; y++) for (int x = 0; x < W; x++)
                {
                    float px = x + 0.5f, py = y + 0.5f;
                    float dh = Mathf.Sqrt((px - hp.x) * (px - hp.x) + (py - hp.y) * ys * (py - hp.y) * ys);
                    float lh = dh < r0 ? 1 - 0.15f * dh / r0 : Mathf.Clamp01(0.85f * (r1 - dh) / (r1 - r0));
                    float st = 0, ss677 = 0, sc = 0, tw = 0, tr6 = 0, tg6 = 0, tb6 = 0;
                    for (int i = 0; i < _src586.Count; i++)
                    {
                        float dx = px - _src586[i].x, dy = (py - _src586[i].y) * ys; float d = Mathf.Sqrt(dx * dx + dy * dy); if (d >= tr) continue;
                        float w = _src586[i].z, cont = w * torchK * Mathf.Pow(1 - d / tr, 1.3f) * fl; var tc = _scol661[i]; if (tc.a > 0) { tw += cont; ss677 += cont; tr6 += cont * tc.r; tg6 += cont * tc.g; tb6 += cont * tc.b; } else st += cont; if (d < cr && !(tc.a > 0 && noSkillCore)) { float c = 1 - d / cr; sc += Mathf.Min(1f, w) * c * c; }
                    }
                    float hd677 = 1 - lh * heroDamp, lt = 1 - (1 - (1 - Mathf.Exp(-st))) * (1 - Mathf.Min(skillCap, 1 - Mathf.Exp(-ss677)) * hd677), L = Mathf.Clamp01(amb + (1 - (1 - lh) * (1 - lt)) * (1 - amb));
                    int k = y * W + x; _lmPx586[k] = new Color32(dr, dg, db, (byte)(255 * (1 - L)));
                    _lcPx586[k] = new Color32(255, 255, 255, (byte)(255 * Mathf.Clamp01(sc * coreK)));
                    _ltPx661[k] = tw > 1e-4f ? new Color32((byte)(255 * Mathf.Clamp01(tr6 / tw)), (byte)(255 * Mathf.Clamp01(tg6 / tw)), (byte)(255 * Mathf.Clamp01(tb6 / tw)), (byte)(255 * Mathf.Clamp01((1 - Mathf.Exp(-tw)) * tintK * hd677))) : new Color32(0, 0, 0, 0);
                }
            }
            _lm586.SetPixels32(_lmPx586); _lm586.Apply(false); _lc586.SetPixels32(_lcPx586); _lc586.Apply(false);
            GUI.DrawTexture(r, _lm586, ScaleMode.StretchToFill, true);
            _lt661.SetPixels32(_ltPx661); _lt661.Apply(false); GUI.DrawTexture(r, _lt661, ScaleMode.StretchToFill, true); // t661u element tint (alpha-blended over the darkness)
            GUI.DrawTexture(r, _lc586, ScaleMode.StretchToFill, true);
            sw.Stop(); Light586MsLast = (float)sw.Elapsed.TotalMilliseconds; Light586SrcN = _src586.Count; Light586Px = nPx;
            _lmMsSum += Light586MsLast; _lmMsN++; Light586MsAvg = _lmMsSum / Mathf.Max(1, _lmMsN);
        }

        public static void Light586MsReset() { _lmMsSum = 0; _lmMsN = 0; Light586MsAvg = 0; Light586MsLast = 0; }
    }
}
