using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    // t948u (v0.0.80): Fire Wall replaces Hellfire Pillar (design v0.4 sec 5b, VH lock t763u). Same SkillId / slot / card / level-ups;
    // HellordTuning.fireWallOn948 = false restores the old Pillar code (kept intact in HellordV36.cs).
    //  - wall across the aim direction, centred fireWallDist737 ahead, fireWallLen737 long, fireWallThick737 thick, ticks every fireWallTick737 (Burn via innate).
    //  - per level +12% dmg (replaces the generic level step), +0.4 u length, +0.3 s. Lv in fireWallVLevels737 adds wings at 30 deg (V / cup).
    //  - Echo: the repeat cast lands fireWallEchoGap737 behind (closer to you). Knockback support pushes along the aim. Projectile supports: no effect.
    //  - Pyromancer "Hellfire Lance" (pact node 6): the wall marches forward at fireWallLanceSpeed737.
    //  - #40 Burning City: handled in the cast switch (closed ring around you, -30% dmg).
    //  FX: PixelAoE shader mode 8 (Burning City flame band along a segment), one quad per segment, drawn inside the real wall footprint.
    public partial class HellordGame
    {
        class FWSeg948 { public Vector2 a, b; public SpriteRenderer sr; public Material m; }
        class FW948 { public List<FWSeg948> seg = new List<FWSeg948>(); public List<SpriteRenderer> emb = new List<SpriteRenderer>(); public float[] eSp, ePh, eE, eT; public int[] eSeg; public Vector2 dir; public float thick, life, life0, tickT, speed, dmg; public SkillInst s; }
        readonly List<FW948> _fw948 = new List<FW948>();
        readonly Stack<FWSeg948> _fwPool948 = new Stack<FWSeg948>();
        readonly List<int> _fwQ948 = new List<int>();
        readonly Stack<SpriteRenderer> _fwEmbPool957 = new Stack<SpriteRenderer>(); Material _fwEmbMat957;
        void Embers948(FW948 w)
        {   // t957u: EMBERS.md - 1 px opaque embers, count = wall length px / 4, rise 8+42 px over a 40/sp-frame life, colour by age
            var T = HellordTuning.I; if (!T.fireWallEmbers957 || _sfxQuad928 == null) return; float px = 0; foreach (var g in w.seg) px += (g.b - g.a).magnitude * HArt.PPU;
            float epp = T.fireWallEmberPxPer988 > 0 ? T.fireWallEmberPxPer988 : T.fireWallEmberPxPer957; int n = Mathf.Clamp(Mathf.RoundToInt(px / Mathf.Max(1f, epp)), 1, 48); // t988u cap 48 + sparser
            w.eSp = new float[n]; w.ePh = new float[n]; w.eE = new float[n]; w.eT = new float[n]; w.eSeg = new int[n];
            if (_fwEmbMat957 == null) _fwEmbMat957 = new Material(Shader.Find("Sprites/Default")) { name = "fx957_ember" };
            for (int k = 0; k < n; k++)
            {
                w.eSp[k] = Random.Range(0.6f, 1.2f); w.ePh[k] = Random.value; w.eE[k] = Random.value * 6.28f; w.eT[k] = Random.value; w.eSeg[k] = Random.Range(0, w.seg.Count);
                var sr = _fwEmbPool957.Count > 0 ? _fwEmbPool957.Pop() : null;
                if (sr == null) { sr = new GameObject("fx957_ember").AddComponent<SpriteRenderer>(); sr.sprite = _sfxQuad928; sr.sharedMaterial = _fwEmbMat957; sr.transform.localScale = Vector3.one / HArt.PPU; }
                sr.sortingOrder = T.fireWallEmberOrder957; sr.gameObject.SetActive(false); w.emb.Add(sr);
            }
        }
        static readonly Color EmbY957 = new Color32(0xfe, 0xe7, 0x61, 255), EmbO957 = new Color32(0xf7, 0x76, 0x22, 255), EmbR957 = new Color32(0xa2, 0x26, 0x33, 255);
        void EmbersFx957(FW948 w, float g)
        {
            if (w.emb.Count == 0) return; float ppu = HArt.PPU, f30 = Mathf.Floor(runTime * 12f) / 12f * 30f; // stepped at 12 fps, frame30 units
            for (int k = 0; k < w.emb.Count; k++)
            {
                var sr = w.emb[k]; var sg = w.seg[Mathf.Min(w.eSeg[k], w.seg.Count - 1)]; float age = Mathf.Repeat(f30 * w.eSp[k] / 40f + w.ePh[k], 1f);
                bool vis = age < 0.9f * g; if (sr.gameObject.activeSelf != vis) sr.gameObject.SetActive(vis); if (!vis) continue;
                Vector2 b = Vector2.Lerp(sg.a, sg.b, w.eT[k]) * ppu + new Vector2(Mathf.Sin(f30 * 0.15f + w.eE[k]) * 2f * age, 8f + 42f * age);
                sr.transform.position = new Vector3((Mathf.Floor(b.x) + 0.5f) / ppu, (Mathf.Floor(b.y) + 0.5f) / ppu, 0);
                sr.color = age < 0.35f ? EmbY957 : age < 0.65f ? EmbO957 : EmbR957;
            }
        }
        Material _fwMat948; bool _fwMatTried948; static readonly int P957Scorch = Shader.PropertyToID("_Scorch");
        public int FireWallLive948 => _fw948.Count;
        public string FireWallDebug948() { var sb = new System.Text.StringBuilder(); foreach (var w in _fw948) { sb.Append($"[wall segs {w.seg.Count} life {w.life:0.0}/{w.life0:0.0} spd {w.speed:0.0} thick {w.thick:0.00}"); foreach (var g in w.seg) sb.Append($" {(g.b - g.a).magnitude:0.0}u"); sb.Append("] "); } return sb.ToString(); }

        static bool FireWallOn948 => HellordTuning.I.fireWallOn948;
        static string FireWallDesc948 = "Wall of flame at your cursor (in range), across your aim. Burns everything crossing it. Longer and longer-lasting per level; Lv5 bends into a V.";
        void FireWallDefs948()
        {   // card / codex text follow the toggle (SkillDef objects are shared)
            foreach (var d in HData.Skills) if (d.id == SkillId.HellfirePillar) { if (FireWallOn948) { d.name = "Fire Wall"; d.desc = FireWallDesc948; } else { d.name = "Hellfire Pillar"; d.desc = "Fire pillars erupt under demon clusters. More and wider per level. Burns."; } }
        }

        bool CastFireWall948(SkillInst s, float dmg)
        {
            var T = HellordTuning.I; int L = Mathf.Max(1, s.level);
            Vector2 dir = AimDir(); if (dir.sqrMagnitude < 1e-4f) dir = Vector2.right; dir.Normalize();
            float len = (T.fireWallLen737 + T.fireWallLenPerLvl737 * (L - 1)) * s.AreaMul, thick = T.fireWallThick737 * s.AreaMul;
            float sec = (T.fireWallSec737 + T.fireWallSecPerLvl737 * (L - 1)) * SinDuration737;
            float per = dmg / Mathf.Max(0.01f, HData.LevelMul(L)) * (1f + T.fireWallDmgPerLvl737 * (L - 1)) * T.fireWallTickDmgMul948;
            float dist = _echo738 ? Mathf.Max(0.5f, T.fireWallDist737 - T.fireWallEchoGap737) : T.fireWallDist737;
            Vector2 c;
            if (CurAim720 && T.fireWallCursor986)
            {   // t979u v0.0.86: land at the cursor, clamped to groundAoeRange716 (same as Pillar / Rune / Grave Pool); wall stays across hero->cursor
                Vector2 ga = GroundAim716(), d2 = ga - _heroPos; if (d2.sqrMagnitude > 0.04f) dir = d2.normalized;
                c = _echo738 ? _heroPos + dir * Mathf.Max(0.5f, d2.magnitude - T.fireWallEchoGap737) : ga;
            }
            else c = _heroPos + dir * dist;
            ClampToRoom(ref c, 4);
            while (_fw948.Count >= Mathf.Max(1, T.fireWallMaxLive737)) KillWall948(0);
            var w = new FW948 { dir = dir, thick = thick, life = sec, life0 = sec, tickT = 0f, dmg = per, s = s, speed = Has51(PactPath.Pyromancer, 6) ? T.fireWallLanceSpeed737 : 0f };
            Vector2 p = new Vector2(-dir.y, dir.x), e0 = c - p * len * 0.5f, e1 = c + p * len * 0.5f;
            AddSeg948(w, e0, e1);
            int nV = 0; if (T.fireWallVLevels737 != null) foreach (int lv in T.fireWallVLevels737) if (L >= lv) nV++;
            if (nV > 0)
            {   // wings bent forward by fireWallVAngle737 at both ends -> V / cup opening toward the enemies; 2nd V level doubles wing length
                float wl = len * 0.5f * nV, a = T.fireWallVAngle737 * Mathf.Deg2Rad;
                Vector2 w1 = (p * Mathf.Cos(a) + dir * Mathf.Sin(a)).normalized, w0 = (-p * Mathf.Cos(a) + dir * Mathf.Sin(a)).normalized;
                AddSeg948(w, e1, e1 + w1 * wl); AddSeg948(w, e0, e0 + w0 * wl);
            }
            _fw948.Add(w); Embers948(w); FireWallFx948(w);
            HellordAudio.Play("fireball_impact");
            return true;
        }
        void AddSeg948(FW948 w, Vector2 a, Vector2 b)
        {
            var g = _fwPool948.Count > 0 ? _fwPool948.Pop() : new FWSeg948(); g.a = a; g.b = b; w.seg.Add(g);
            if (g.sr == null && FireWallMat948() != null)
            {
                g.sr = new GameObject("fx948_firewall").AddComponent<SpriteRenderer>(); g.sr.sprite = _sfxQuad928; g.sr.sharedMaterial = new Material(_fwMat948); g.m = g.sr.sharedMaterial;
            }
            if (g.sr != null) { g.sr.gameObject.SetActive(true); g.m.SetFloat(P928Seed, Random.value * 6.28f); g.m.SetFloat(P928T0, Random.value * 10f); }
        }
        Material FireWallMat948()
        {
            if (_fwMatTried948) return _fwMat948; if (!ShaderFx928(3) || _sfxMat930City == null) return null; _fwMatTried948 = true;
            var T = HellordTuning.I;
            if (T.fireWallShader957)
            {   // t957u: approved fw.py port (Resources/HellordShaders/HellordPixelFireWall.shader)
                var sh = Resources.Load<Shader>("HellordShaders/HellordPixelFireWall"); if (sh == null) sh = Shader.Find("Hellord/PixelFireWall");
                if (sh != null && sh.isSupported)
                {
                    _fwMat948 = new Material(sh) { name = "fx957_firewall" }; Tex928(_fwMat948, "_NoiseTex", T.fxNoiseTex928); Tex928(_fwMat948, "_RampTex", "fx948_firewall_ramp");
                    _fwMat948.SetFloat("_RampCount", 7); _fwMat948.SetFloat("_TimeStep", 12); _fwMat948.SetFloat("_PPU", HArt.PPU);
                    _fwMat948.SetFloat("_FlameH", T.fireWallFlameH948); _fwMat948.SetFloat("_Gain", T.fireWallTongue948); _fwMat948.SetFloat("_Scorch", 0); Debug.Log("[t957u] PixelFireWall shader loaded"); return _fwMat948;
                }
                Debug.LogWarning("[t957u] PixelFireWall shader missing/unsupported - PixelAoE mode 8 used");
            }
            _fwMat948 = new Material(_sfxMat930City) { name = "fx948_firewall" }; _fwMat948.SetFloat("_Mode", 8); _fwMat948.SetFloat("_Side", 0);
            _fwMat948.SetFloat("_FlameH", T.fireWallFlameH948); _fwMat948.SetFloat("_Gain", T.fireWallTongue948); return _fwMat948;
        }
        void KillWall948(int i)
        {
            var w = _fw948[i]; foreach (var em in w.emb) { em.gameObject.SetActive(false); _fwEmbPool957.Push(em); } w.emb.Clear(); foreach (var g in w.seg) { if (g.sr) g.sr.gameObject.SetActive(false); _fwPool948.Push(g); } w.seg.Clear(); _fw948.RemoveAt(i);
        }
        void ClearFireWalls948() { for (int i = _fw948.Count - 1; i >= 0; i--) KillWall948(i); }

        void FireWallFx948(FW948 w)
        {
            var T = HellordTuning.I; float ppu = HArt.PPU;
            float g = Mathf.Min(1f, (w.life0 - w.life) / 0.2f); if (w.life < 0.35f) g = Mathf.Min(g, Mathf.Clamp01(w.life / 0.35f));
            foreach (var s in w.seg)
            {
                Vector2 c = (s.a + s.b) * 0.5f, d = s.b - s.a; float half = d.magnitude * 0.5f; Vector2 dn = half > 1e-4f ? d / (half * 2f) : Vector2.right;
                if (s.sr == null)
                {   // shader unavailable: plain ZSeg band (old-style fallback)
                    ZSeg(s.a, s.b, w.thick, new Color(1f, 0.45f + 0.15f * Mathf.Sin(runTime * 9), 0.1f, 0.75f * g), 2); continue;
                }
                float hx = Mathf.Abs(dn.x) * half + w.thick * 0.5f + 0.3f, hy = Mathf.Abs(dn.y) * half + w.thick * 0.5f + (T.fireWallShader957 ? T.fireWallPadDownU957 : 0f); // t957u: pad so the scorch's lower edge isn't clipped
                float up = (T.fireWallFlameH948 + 18f * T.fireWallTongue948) / ppu; // flame tongues rise above the footprint (visual height only)
                float padD = T.fireWallShader957 ? T.fireWallPadDownU957 : 0f; // pad extends downward only
                s.sr.transform.position = new Vector3(c.x, c.y + up * 0.5f - padD * 0.5f, 0); s.sr.transform.localScale = new Vector3(hx * 2f + 0.2f, hy * 2f + up + 0.2f - padD, 1);
                s.sr.sortingOrder = T.fireWallOrder948;
                s.m.SetVector(P928Center, c); s.m.SetVector(P930Dir, dn); s.m.SetFloat(P928Radius, half * ppu); s.m.SetFloat("_BandW", w.thick * 0.5f * ppu); s.m.SetFloat(P928Grow, g);
                s.m.SetFloat(P957Scorch, Mathf.Min(1f, (w.life0 - w.life) / Mathf.Max(1e-3f, T.fireWallScorchS957))); // t957u
            }
            EmbersFx957(w, g);
        }

        void FireWallTick948(float dt)
        {
            if (mode != Mode.Play) { if (mode == Mode.StartPick && _fw948.Count > 0) ClearFireWalls948(); return; }
            var T = HellordTuning.I;
            for (int i = _fw948.Count - 1; i >= 0; i--)
            {
                var w = _fw948[i]; w.life -= dt; if (w.life <= 0 || w.s == null) { KillWall948(i); continue; }
                if (w.speed > 0) { Vector2 mv = w.dir * w.speed * dt; foreach (var g in w.seg) { g.a += mv; g.b += mv; } }
                if ((w.tickT -= dt) <= 0)
                {
                    w.tickT = Mathf.Max(0.05f, T.fireWallTick737); bool kb = w.s.Has(SupportId.Knockback);
                    foreach (var g in w.seg)
                    {
                        Vector2 c = (g.a + g.b) * 0.5f; float half = (g.b - g.a).magnitude * 0.5f; Query(c, half + w.thick + 1f, _fwQ948);
                        foreach (int k in _fwQ948)
                        {
                            var e = _en[k]; if (!e.alive || e.dying) continue;
                            if (SegDist53(e.pos, g.a, g.b) > w.thick * 0.5f + e.radius) continue;
                            Hit(e, w.dmg, w.s, kb ? w.dir * T.fireWallKnock948 : Vector2.zero, true);
                        }
                    }
                }
                FireWallFx948(w);
            }
        }
    }
}
