using System.Collections.Generic;
using UnityEngine;
using PixelFXPack;

namespace Hellord
{
    // =====================================================================================================================
    // t1736 (VH GO): wire ZhongKuiFX (Art/fx_zhongkui_kit_v3/spec.md) into the t1713 kit.
    // t1790 (VH GO): v4 Shader FX for Bone Colossus, Grave Legion, Ossuary Nova (v4b), Bone Wall (v4c),
    //  Bone Armour (v4b Back/Front + tBreak fix), Decrepify (v4c area + per-demon mark). Same quad route
    //  (ZhFxQuad1625 + _Glow twin). Missing material / nkFx1736 off = placeholders.
    //  Kept at v3: Soul Plague, Ghost Step, Skeletal Mage soul bolt, Jiangshi/Clay slam.
    //  NOT wired (VH rejected ghost-based FX): Army of the Dead wave, Banquet ghosts, Hell Gate ghost.
    // =====================================================================================================================
    public partial class HellordGame
    {
        class NkFxO { public string mat; public Vector2 p, q; public float t, life, seed = 0.41f; public Vector4 rc; public int layer, ordAdd; public System.Func<NkFxO, Vector2> follow; public System.Func<NkFxO, ZP[]> pf; public bool done; }
        readonly List<NkFxO> _nkFx1736 = new List<NkFxO>();
        readonly Dictionary<string, Material> _nkMat1736 = new Dictionary<string, Material>();
        SpriteRenderer _nkColSr1736;
        Vector2 _nkColFace1736 = Vector2.down;
        bool _nkColMoving1736;
        float _nkDecrFxT1736; Vector2 _nkDecrFxP1736;
        NkFxO _nkArmFx1736, _nkWallFx1736, _nkGateFx1736, _nkGateFl1736, _nkColBodyWait1736, _nkLegionArmedFx1736;
        float _nkArmBreakAt1736 = -1f, _nkWallBreakAt1736 = -1f, _nkLegionCrumbleN1736;

        Material NkFxMat1736(string n)
        {
            if (string.IsNullOrEmpty(n)) return null;
            if (_nkMat1736.TryGetValue(n, out var m)) return m;
            string dir = TZ != null && !string.IsNullOrEmpty(TZ.nkFxDir1736) ? TZ.nkFxDir1736 : "ZhongKuiFX/Materials/";
            m = Resources.Load<Material>(dir + n);
            return _nkMat1736[n] = m;
        }
        bool NkFxLive1736(string mat) => TZ != null && TZ.nkFx1736 && NkFxMat1736(mat) != null;
        bool NkFx1736(string mat, Vector2 pivot, float l, float b, float w, float h, int order, float fxTime, float seed, params ZP[] ps)
        {
            if (ZhFxLogOn1625) { _zfxSeen1625.Add(mat); _zfxLog1625[mat + "|_FxTime"] = fxTime; if (ps != null) foreach (var p in ps) _zfxLog1625[mat + "|" + p.n] = p.v.x; }
            if (TZ == null || !TZ.nkFx1736) return false; var m = NkFxMat1736(mat); if (m == null) return false;
            ZhFxQuad1625(m, pivot, l, b, w, h, order, fxTime, seed, ps); var g = NkFxMat1736(mat + "_Glow"); if (g != null) ZhFxQuad1625(g, pivot, l, b, w, h, order + 1, fxTime, seed, ps);
            return true;
        }
        int NkFxOrd1736(int layer, Vector2 p) => layer == 0 ? FbGround + YSort(p.y) + 40 : layer == 1 ? YSort(p.y) : YSort(p.y) + 60;
        NkFxO NkFxAdd1736(string mat, Vector2 p, Vector4 rc, float life, int layer, System.Func<NkFxO, ZP[]> pf = null, System.Func<NkFxO, Vector2> follow = null, float seed = -1f, int ordAdd = 0)
        {
            var o = new NkFxO { mat = mat, p = p, rc = rc, life = life, layer = layer, pf = pf, follow = follow, ordAdd = ordAdd, seed = seed >= 0f ? seed : Mathf.Repeat(p.x * 0.37f + p.y * 0.13f + runTime, 1f) };
            _nkFx1736.Add(o); return o;
        }
        void ClearNkFx1736()
        {
            _nkFx1736.Clear(); _nkArmFx1736 = _nkWallFx1736 = _nkGateFx1736 = _nkGateFl1736 = _nkLegionArmedFx1736 = null;
            _nkArmBreakAt1736 = _nkWallBreakAt1736 = -1f; _nkDecrFxT1736 = 0f;
            if (_nkColSr1736 != null) _nkColSr1736.gameObject.SetActive(false);
        }
        void NkFxTick1736(float dt)
        {
            NkFxLegionArmedTick1736();
            NkFxDecrMarks1736();
            NkFxDecrTint1736();
            for (int i = _nkFx1736.Count - 1; i >= 0; i--)
            {
                var o = _nkFx1736[i]; o.t += dt;
                if (o.done || o.t >= o.life) { _nkFx1736.RemoveAt(i); if (o == _nkLegionArmedFx1736) _nkLegionArmedFx1736 = null; continue; }
                if (o.follow != null) o.p = o.follow(o);
                var ps = o.pf != null ? o.pf(o) : null;
                NkFx1736(o.mat, o.p, o.rc.x, o.rc.y, o.rc.z, o.rc.w, NkFxOrd1736(o.layer, o.p) + o.ordAdd, o.t, o.seed, ps);
            }
        }

        // ----- hooks (true = shader drew, skip placeholder) -----
        bool NkFxDecrepify1736(Vector2 pos, float life, float radiusU = 2.5f)
        {
            if (!NkFxLive1736("ZKFX_Decrepify")) return false;
            _nkDecrFxP1736 = pos; _nkDecrFxT1736 = life;
            float R = radiusU * 16f, pad = 8f;
            NkFxAdd1736("ZKFX_Decrepify", pos, new Vector4(-(R + pad), -(R + pad), 2f * (R + pad), 2f * (R + pad)), life, 0, o =>
            {
                float fade = o.t >= life - 0.4f ? Mathf.InverseLerp(life - 0.4f, life, o.t) : 0f;
                return new[] { ZP.F("_Ignite", 1f), ZP.F("_Fade", fade), ZP.F("_Radius", R) };
            });
            return true;
        }
        float NkDecrHeadPx1736(Enemy e)
        {
            if (e != null && e.sr != null && e.sr.sprite != null)
            {
                var sp = e.sr.sprite; float h = sp.rect.height - sp.pivot.y;
                if (h > 4f) return h;
            }
            return e != null && e.type == EType.Imp ? 36f : (e != null ? Mathf.Max(36f, e.hitY * 16f) : 36f);
        }
        void NkFxDecrMarks1736()
        {
            if (!NkOn1713 || _en == null || !NkFxLive1736("ZKFX_Decrepify_Mark")) return;
            var list = new List<(float d, int i)>(16);
            for (int i = 0; i < _en.Length; i++)
            {
                var e = _en[i]; if (e == null || !e.alive || e.dying || e.nkDecr1713 <= runTime) continue;
                list.Add(((e.pos - _heroPos).sqrMagnitude, i));
            }
            if (list.Count == 0) return;
            list.Sort((a, b) => a.d.CompareTo(b.d));
            int n = Mathf.Min(32, list.Count);
            for (int k = 0; k < n; k++)
            {
                var e = _en[list[k].i];
                float from = e.nkDecrFrom1713 > 0f ? e.nkDecrFrom1713 : e.nkDecr1713 - 6f;
                float fxT = Mathf.Max(0f, runTime - from);
                float fade = runTime >= e.nkDecr1713 - 0.4f ? Mathf.InverseLerp(e.nkDecr1713 - 0.4f, e.nkDecr1713, runTime) : 0f;
                float seed = Mathf.Repeat(list[k].i * 0.618f, 1f);
                var ps = new[] { ZP.F("_Fade", fade), ZP.F("_Head", NkDecrHeadPx1736(e)), ZP.F("_Intensity", 1f) };
                int feetOrd = FbGround + YSort(e.pos.y) + 42;
                int markOrd = YSort(e.pos.y) + 60;
                NkFx1736("ZKFX_Decrepify_MarkFeet", e.pos, -16, -10, 32, 70, feetOrd, fxT, seed, ps);
                NkFx1736("ZKFX_Decrepify_Mark", e.pos, -16, -10, 32, 70, markOrd, fxT, seed, ps);
            }
        }
        void NkFxDecrTint1736()
        {
            if (_en == null) return;
            var curse = new Color(0.45f, 0.30f, 1f, 1f);
            for (int i = 0; i < _en.Length; i++)
            {
                var e = _en[i]; if (e == null || e.sr == null || !e.alive || e.dying) continue;
                if (e.hitT > 0f) { e.sr.color = Color.white; continue; } // hit flash stays on top
                if (e.freezeT > 0f) continue; // Frozen wins
                if (NkOn1713 && e.nkDecr1713 > runTime) e.sr.color = curse;
                else e.sr.color = Color.white;
            }
        }
        bool NkFxPlagueMark1736(Enemy e)
        {
            if (e == null || !NkFxLive1736("ZKFX_SoulPlague")) return false;
            float life = Mathf.Max(0.2f, e.nkPlague1713 - runTime);
            var er = e;
            NkFxAdd1736("ZKFX_SoulPlague", e.pos, new Vector4(-22, -8, 44, 48), life, 2,
                o => {
                    float form = Mathf.Clamp01(o.t / 0.2f), fade = o.t >= life - 0.3f ? Mathf.InverseLerp(life - 0.3f, life, o.t) : 0f;
                    return new[] { ZP.F("_Form", form), ZP.F("_Intensity", 1f), ZP.F("_Fade", fade), ZP.F("_Pulse", o.t < 0.25f ? 1f : 0f) };
                },
                o => er != null && er.alive ? er.pos : o.p);
            return true;
        }
        bool NkFxPlagueSpread1736(Vector2 pos, float radiusU = 2.5f)
        {
            if (!NkFxLive1736("ZKFX_SoulPlague_Spread")) return false;
            // authored 112x76 for 2.5 u spread
            NkFxAdd1736("ZKFX_SoulPlague_Spread", pos, FxScaleRc1815(new Vector4(-56, -38, 112, 76), radiusU / 2.5f), 0.7f, 2, o => new[] { ZP.F("_Progress", Mathf.Clamp01(o.t / 0.7f)), ZP.F("_Radius", radiusU * 16f) });
            return true;
        }
        bool NkFxOssuaryNova1736(Vector2 pos, float radiusU = 8f)
        {
            if (!NkFxLive1736("ZKFX_OssuaryNova_Ground")) return false;
            float S = Random.value; float Rpx = radiusU * 16f; int N = 24;
            float fadeOf(float t) => t >= 0.85f ? Mathf.InverseLerp(0.85f, 1.15f, t) : 0f;
            NkFxAdd1736("ZKFX_OssuaryNova_Ground", pos, new Vector4(-Rpx - 24f, -Rpx - 24f, 2f * Rpx + 48f, 2f * Rpx + 48f), 1.15f, 0,
                o => new[] { ZP.F("_Fade", fadeOf(o.t)), ZP.F("_Radius", Rpx) }, seed: S);
            NkFxAdd1736("ZKFX_OssuaryNova_BurstBack", pos + new Vector2(0f, 0.02f), new Vector4(-24, -10, 48, 40), 0.7f, 1, seed: S);
            NkFxAdd1736("ZKFX_OssuaryNova_BurstFront", pos + new Vector2(0f, -0.02f), new Vector4(-24, -10, 48, 40), 0.7f, 1, seed: S);
            for (int i = 0; i < N; i++)
            {
                float a = (i + S) * (Mathf.PI * 2f) / N;
                Vector2 P = pos + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radiusU;
                Vector2 ho = (pos - P) * 16f;
                float l = Mathf.Min(0f, ho.x) - 22f, b = Mathf.Min(0f, ho.y) - 14f, w = Mathf.Abs(ho.x) + 44f, h = Mathf.Abs(ho.y) + 52f;
                int idx = i;
                NkFxAdd1736("ZKFX_OssuaryNova_Spear", P, new Vector4(l, b, w, h), 1.15f, 1, o => new[]
                {
                    ZP.F("_Fade", fadeOf(o.t)), ZP.F("_Radius", Rpx), ZP.F("_Count", N), ZP.F("_SpearIdx", idx), ZP.V("_HeroOff", ho)
                }, seed: S);
            }
            return true;
        }
        bool NkFxColossusMerge1736(Vector2 pos)
        {
            if (!NkFxLive1736("ZKFX_BoneColossus_Merge")) return false;
            Vector4 srcA = default, srcB = default, srcC = default, srcD = default;
            int n = 0;
            if (_nkStash1713 != null)
            {
                n = Mathf.Min(8, _nkStash1713.Count);
                for (int i = 0; i < n; i++)
                {
                    Vector2 off = (_nkStash1713[i].p - pos) * 16f;
                    if (i == 0) srcA = new Vector4(off.x, off.y, 0, 0);
                    else if (i == 1) { srcA.z = off.x; srcA.w = off.y; }
                    else if (i == 2) srcB = new Vector4(off.x, off.y, 0, 0);
                    else if (i == 3) { srcB.z = off.x; srcB.w = off.y; }
                    else if (i == 4) srcC = new Vector4(off.x, off.y, 0, 0);
                    else if (i == 5) { srcC.z = off.x; srcC.w = off.y; }
                    else if (i == 6) srcD = new Vector4(off.x, off.y, 0, 0);
                    else { srcD.z = off.x; srcD.w = off.y; }
                }
            }
            int srcN = n;
            NkFxAdd1736("ZKFX_BoneColossus_Merge", pos, new Vector4(-70, -16, 140, 110), 0.8f, 1, o => new[]
            {
                ZP.F("_Progress", Mathf.Clamp01(o.t / 0.8f)), ZP.F("_SrcN", srcN),
                ZP.V4("_SrcA", srcA), ZP.V4("_SrcB", srcB), ZP.V4("_SrcC", srcC), ZP.V4("_SrcD", srcD)
            });
            return true;
        }
        bool NkFxColossusSplit1736(Vector2 pos)
        {
            if (!NkFxLive1736("ZKFX_BoneColossus_Split")) return false;
            NkFxAdd1736("ZKFX_BoneColossus_Split", pos, new Vector4(-70, -16, 140, 110), 0.8f, 1, o => new[] { ZP.F("_Progress", Mathf.Clamp01(o.t / 0.8f)) });
            return true;
        }
        bool NkFxColossusStomp1736(Vector2 pos, float radiusU)
        {
            if (!NkFxLive1736("ZKFX_BoneColossus_Stomp_Ground") && !NkFxLive1736("ZKFX_BoneColossus_Stomp")) return false;
            float R = radiusU * 16f; float seed = Mathf.Repeat(pos.x * 0.37f + pos.y * 0.13f + runTime, 1f);
            var rc = new Vector4(-R - 12f, -R - 8f, 2f * R + 24f, 2f * R + 36f);
            System.Func<NkFxO, ZP[]> pf = o => new[] { ZP.F("_Radius", R) };
            if (NkFxLive1736("ZKFX_BoneColossus_Stomp_Ground")) NkFxAdd1736("ZKFX_BoneColossus_Stomp_Ground", pos, rc, 0.85f, 0, pf, seed: seed, ordAdd: -20);
            if (NkFxLive1736("ZKFX_BoneColossus_Stomp_Back")) NkFxAdd1736("ZKFX_BoneColossus_Stomp_Back", pos, rc, 0.85f, 0, pf, seed: seed, ordAdd: -19);
            if (NkFxLive1736("ZKFX_BoneColossus_Stomp")) NkFxAdd1736("ZKFX_BoneColossus_Stomp", pos, rc, 0.85f, 1, pf, seed: seed, ordAdd: 30);
            return true;
        }
        bool NkFxHellGate1736(Vector2 pos, float ang, float life)
        {
            bool arch = NkFxLive1736("ZKFX_HellGate"), floor = NkFxLive1736("ZKFX_HellGate_Floor");
            if (!arch && !floor) return false;
            System.Func<NkFxO, ZP[]> pf = o =>
            {
                float form = Mathf.Clamp01(o.t / 0.35f), fade = o.t >= life - 0.4f ? Mathf.InverseLerp(life - 0.4f, life, o.t) : 0f;
                return new[] { ZP.F("_Form", form), ZP.F("_Fade", fade), ZP.F("_Angle", ang) };
            };
            if (floor) _nkGateFl1736 = NkFxAdd1736("ZKFX_HellGate_Floor", pos, new Vector4(-28, -20, 56, 40), life, 0, pf);
            if (arch) _nkGateFx1736 = NkFxAdd1736("ZKFX_HellGate", pos, new Vector4(-36, -12, 72, 70), life, 1, pf);
            return true;
        }
        bool NkFxBoneArmour1736(float life)
        {
            bool split = NkFxLive1736("ZKFX_BoneArmour_Back") && NkFxLive1736("ZKFX_BoneArmour_Front");
            if (!split && !NkFxLive1736("ZKFX_BoneArmour")) return false;
            _nkArmBreakAt1736 = -1f;
            System.Func<NkFxO, ZP[]> pf = o =>
            {
                float form = Mathf.Clamp01(o.t / 0.25f);
                bool broke = _nkArmT1713 <= 0f || _nkArmHp1713 <= 0f;
                if (broke && _nkArmBreakAt1736 < 0f) _nkArmBreakAt1736 = _nkArmT1713 <= 0f && o.t >= life - 0.02f ? life : o.t;
                if (!broke && o.t >= life && _nkArmBreakAt1736 < 0f) _nkArmBreakAt1736 = life;
                float br = 0f;
                if (_nkArmBreakAt1736 >= 0f && o.t >= _nkArmBreakAt1736)
                    br = Mathf.Clamp01((o.t - _nkArmBreakAt1736) / 0.3f + 0.05f) + 0.15f;
                if (_nkArmBreakAt1736 >= 0f) o.life = Mathf.Max(o.life, _nkArmBreakAt1736 + 0.35f);
                return new[] { ZP.F("_Form", form), ZP.F("_Intensity", 1f), ZP.F("_Break", br), ZP.F("_Fade", br) };
            };
            if (split)
            {
                NkFxAdd1736("ZKFX_BoneArmour_Back", _heroPos + new Vector2(0f, 1f / 16f), new Vector4(-40, -13, 80, 64), life + 0.35f, 1, pf, o => _heroPos + new Vector2(0f, 1f / 16f));
                _nkArmFx1736 = NkFxAdd1736("ZKFX_BoneArmour_Front", _heroPos - new Vector2(0f, 1f / 16f), new Vector4(-40, -11, 80, 64), life + 0.35f, 1, pf, o => _heroPos - new Vector2(0f, 1f / 16f));
            }
            else
                _nkArmFx1736 = NkFxAdd1736("ZKFX_BoneArmour", _heroPos, new Vector4(-40, -12, 80, 64), life + 0.35f, 2, pf, o => _heroPos);
            return true;
        }
        bool NkFxBoneWall1736(Vector2 start, float dashAng, float life)
        {
            if (!NkFxLive1736("ZKFX_BoneWall")) return false;
            _nkWallBreakAt1736 = -1f;
            float wallPx = (TZ != null ? TZ.nkWallLen1713 : 3f) * 16f; // t1815: follow nkWallLen1713 (authored 48 px = 3 u)
            float wallSc = wallPx / 48f;
            float seed = Mathf.Repeat(start.x * 0.37f + start.y * 0.13f + runTime, 1f);
            System.Func<NkFxO, ZP[]> pfCore = o =>
            {
                var w = _nkWall1713; float dmg = w != null ? 1f - w.hp / (float)Mathf.Max(1, TZ.nkWallHits1713) : 1f;
                bool broke = w == null || w.hp <= 0 || w.t >= w.life;
                if (broke && _nkWallBreakAt1736 < 0f)
                    _nkWallBreakAt1736 = (w != null && w.t >= w.life) ? life : o.t;
                float br = 0f;
                if (_nkWallBreakAt1736 >= 0f && o.t >= _nkWallBreakAt1736)
                    br = Mathf.Clamp01((o.t - _nkWallBreakAt1736) / 0.3f + 0.05f);
                if (_nkWallBreakAt1736 >= 0f) o.life = _nkWallBreakAt1736 + 0.3f;
                return new[] { ZP.F("_Angle", dashAng), ZP.F("_Damage", Mathf.Clamp01(dmg)), ZP.F("_Break", br), ZP.F("_Fade", br) };
            };
            if (NkFxLive1736("ZKFX_BoneWall_Ground"))
                NkFxAdd1736("ZKFX_BoneWall_Ground", start, FxScaleRc1815(new Vector4(-44, -36, 88, 72), wallSc), life + 0.3f, 0, pfCore, seed: seed);
            Vector2 dir = new Vector2(Mathf.Cos(dashAng * Mathf.Deg2Rad), Mathf.Sin(dashAng * Mathf.Deg2Rad));
            Vector2 n = new Vector2(-dir.y, dir.x);
            if (Mathf.Abs(n.y) > 0.5f)
            {
                int[,] parts = { { 0, 4 }, { 5, 8 }, { 9, 12 } };
                for (int s = 0; s < 3; s++)
                {
                    int lo = parts[s, 0], hi = parts[s, 1];
                    float sa = ((lo + 0.5f) / 13f - 0.5f) * wallPx;
                    float sb = ((hi + 0.5f) / 13f - 0.5f) * wallPx;
                    float spx = n.y * sa < n.y * sb ? sa : sb;
                    Vector2 cpx = start * 16f;
                    Vector2 pvpx = new Vector2(Mathf.Round(cpx.x + n.x * spx), Mathf.Round(cpx.y + n.y * spx)) - new Vector2(0f, 3f);
                    Vector2 pv = pvpx / 16f;
                    Vector2 wallOff = pvpx - new Vector2(Mathf.Round(cpx.x), Mathf.Round(cpx.y));
                    var part = new Vector4(lo, hi, 0, 0);
                    var rc = FxScaleRc1815(new Vector4(-40f, -32f, 80f, 116f), wallSc); rc.x -= wallOff.x; rc.y -= wallOff.y;
                    _nkWallFx1736 = NkFxAdd1736("ZKFX_BoneWall", pv, rc, life + 0.3f, 1, o =>
                    {
                        var ps = pfCore(o);
                        var list = new List<ZP>(ps) { ZP.V4("_Part", part), ZP.V("_WallOff", wallOff) };
                        return list.ToArray();
                    }, seed: seed);
                }
            }
            else
                _nkWallFx1736 = NkFxAdd1736("ZKFX_BoneWall", start, FxScaleRc1815(new Vector4(-40, -32, 80, 116), wallSc), life + 0.3f, 1, pfCore, seed: seed);
            return true;
        }
        bool NkFxGhostStep1736(Vector2 start, Vector2 end, float dashTime)
        {
            if (!NkFxLive1736("ZKFX_GhostStep")) return false;
            Vector2 tpx = (end - start) * 16f;
            float life = dashTime + TZ.nkGhostAfter1713;
            float l = Mathf.Min(0f, tpx.x) - 22f, b = Mathf.Min(0f, tpx.y) - 30f, w = Mathf.Abs(tpx.x) + 44f, h = Mathf.Abs(tpx.y) + 52f;
            NkFxAdd1736("ZKFX_GhostStep", start, new Vector4(l, b, w, h), life, 2, o =>
            {
                float prog = dashTime > 0.01f ? Mathf.Clamp01(o.t / dashTime) : 1f;
                float fade = o.t > dashTime ? Mathf.InverseLerp(dashTime, life, o.t) : 0f;
                return new[] { ZP.V("_TargetPx", tpx), ZP.F("_Progress", prog), ZP.F("_Fade", fade) };
            });
            return true;
        }
        bool NkFxSoulBolt1736(Vector2 pos, Vector2 dir, float life = 0.35f)
        {
            if (!NkFxLive1736("ZKFX_SoulBolt")) return false;
            Vector2 d = dir.sqrMagnitude > 1e-4f ? dir.normalized : Vector2.right;
            NkFxAdd1736("ZKFX_SoulBolt", pos, new Vector4(-18, -16, 36, 32), life, 2,
                o => new[] { ZP.V("_Dir", d) },
                o => o.p + d * 8f * Time.deltaTime);
            return true;
        }
        bool NkFxSoulBurst1736(Vector2 pos, float radiusU = 2.4f)
        {
            if (!NkFxLive1736("ZKFX_SoulBurst")) return false;
            // authored 84x80 for 1.6 u burst
            NkFxAdd1736("ZKFX_SoulBurst", pos, FxScaleRc1815(new Vector4(-42, -28, 84, 80), radiusU / 1.6f), 0.55f, 2, o => new[] { ZP.F("_Progress", Mathf.Clamp01(o.t / 0.55f)), ZP.F("_Radius", radiusU * 16f) });
            return true;
        }
        bool NkFxClaySlam1736(Vector2 pos, float radiusU = 2.9f)
        {
            if (!NkFxLive1736("ZKFX_ClaySlam")) return false;
            // authored 84x60 (~2.6 u visual) ; now follows live slam / melee radius
            float R = Mathf.Max(0.01f, radiusU) * 16f, y = 60f / 84f, pad = 8f;
            var rc = new Vector4(-(R + pad), -(R * y + pad), 2f * (R + pad), 2f * (R * y + pad));
            NkFxAdd1736("ZKFX_ClaySlam", pos, rc, 0.85f, 0, o =>
            {
                float fade = o.t >= 0.55f ? Mathf.InverseLerp(0.55f, 0.85f, o.t) : 0f;
                return new[] { ZP.F("_Progress", Mathf.Clamp01(o.t / 0.7f)), ZP.F("_Fade", fade), ZP.F("_Radius", R) };
            });
            return true;
        }

        // ----- Grave Legion v4 -----
        bool NkFxLegionArmedNow1736() => NkOn1713 && NkUlt1713 == 3 && ZhUltCd <= 0.05f && _nkLegionT1772 <= 0f && NkLiveMinions1713() >= NCapBase536();
        void NkFxLegionArmedTick1736()
        {
            if (!NkFxLive1736("ZKFX_GraveLegion_Armed")) return;
            bool on = NkFxLegionArmedNow1736();
            if (on)
            {
                if (_nkLegionArmedFx1736 == null || _nkLegionArmedFx1736.done)
                    _nkLegionArmedFx1736 = NkFxAdd1736("ZKFX_GraveLegion_Armed", _heroPos, new Vector4(-24, -14, 48, 28), 999f, 0,
                        o => new[] { ZP.F("_Fade", 0f) }, o => _heroPos, ordAdd: -20);
                else { _nkLegionArmedFx1736.life = 999f; }
            }
            else if (_nkLegionArmedFx1736 != null && !_nkLegionArmedFx1736.done)
            {
                var o = _nkLegionArmedFx1736;
                if (o.life > 10f) o.life = o.t + 0.2f;
                o.pf = fx => new[] { ZP.F("_Fade", Mathf.Clamp01((fx.t - (o.life - 0.2f)) / 0.2f)) };
            }
        }
        bool NkFxLegionCast1736()
        {
            if (!NkFxLive1736("ZKFX_GraveLegion_Cast")) return false;
            NkFxAdd1736("ZKFX_GraveLegion_Cast", _heroPos, new Vector4(-28, -16, 56, 34), 0.6f, 0, ordAdd: -18);
            _nkLegionCrumbleN1736 = 0f;
            return true;
        }
        ZP[] NkFxLegionSheet1736(NMin536 m, float delay, Vector2 grave)
        {
            var ps = new List<ZP> { ZP.F("_Delay", delay), ZP.V("_HeroOff", (_heroPos - grave) * 16f) };
            Texture tex; Vector4 info, info2;
            if (NkSkelSheet1736(m, out tex, out info, out info2))
            { ps.Add(ZP.Tex("_SkelTex", tex)); ps.Add(ZP.V4("_SkelInfo", info)); ps.Add(ZP.V4("_SkelInfo2", info2)); }
            return ps.ToArray();
        }
        bool NkSkelSheet1736(NMin536 m, out Texture tex, out Vector4 info, out Vector4 info2)
        {
            tex = null; info = default; info2 = default;
            if (_an561 == null) Load561();
            string key; int kind;
            if (m.kind == 0) { key = "skel_warrior_all_death_south"; kind = 0; }
            else if (m.kind == 1) { key = "skel_archer_all_idle_south"; kind = 1; }
            else if (m.kind == 2) { key = "skel_mage_all_idle_south"; kind = 1; }
            else if (m.kind == 3) { key = "jiangshi_all_idle_south"; kind = 1; }
            else { key = "skel_warrior_all_idle_south"; kind = 1; }
            var a = A561(key); if (a == null || a.f == null || a.f.Length == 0 || a.f[0] == null) return false;
            var sp = a.f[0]; tex = sp.texture;
            info = new Vector4(a.f.Length, sp.rect.width, sp.rect.height, sp.pivot.x);
            info2 = new Vector4(sp.pivot.y, kind, 0, 0);
            return tex != null;
        }
        void NkFxLegionRise1736(NMin536 m)
        {
            if (m == null || !NkFxLive1736("ZKFX_GraveLegion_Rise")) return;
            Vector2 grave = m.p;
            float delay = Mathf.Min((grave - _heroPos).magnitude * 16f / 240f, 0.5f);
            m.hideUntil = runTime + delay + 0.74f;
            float seed = Mathf.Repeat(grave.x * 0.37f + grave.y * 0.13f + runTime, 1f);
            var sheet = NkFxLegionSheet1736(m, delay, grave);
            float hx = (_heroPos - grave).x * 16f, hy = (_heroPos - grave).y * 16f;
            var grc = new Vector4(Mathf.Min(0f, hx) - 22f, Mathf.Min(0f, hy) - 14f, Mathf.Abs(hx) + 44f, Mathf.Abs(hy) + 28f);
            if (NkFxLive1736("ZKFX_GraveLegion_RiseGround"))
                NkFxAdd1736("ZKFX_GraveLegion_RiseGround", grave, grc, delay + 1.5f, 0, o => sheet, seed: seed, ordAdd: -20);
            NkFxAdd1736("ZKFX_GraveLegion_Rise", grave, new Vector4(-24, -8, 48, 64), delay + 1.0f, 1, o => sheet, seed: seed);
        }
        bool NkFxLegionCrumble1736(NMin536 m)
        {
            if (m == null || !NkFxLive1736("ZKFX_GraveLegion_Crumble")) return false;
            float delay = Mathf.Min(_nkLegionCrumbleN1736 * 0.04f, 0.3f); _nkLegionCrumbleN1736++;
            var sheet = NkFxLegionSheet1736(m, delay, m.p);
            NkFxAdd1736("ZKFX_GraveLegion_Crumble", m.p, new Vector4(-26, -8, 52, 52), 1.1f + delay, 1, o => sheet);
            return true;
        }

        /// <summary>Draw the Bone Colossus sprite sheet. False = art missing, keep the disc placeholder.</summary>
        bool NkDrawColossus1736()
        {
            if (_an561 == null) Load561();
            bool atk = _nkColAtk1713 > TZ.nkColAtk1713 - 6f / 12f;
            string dname; bool flip;
            dname = Dir561(_nkColFace1736, true, out flip);
            string act = atk ? "attack_" : _nkColMoving1736 ? "move_" : "idle_";
            var a = A561("bone_colossus_all_" + act + dname);
            if (a == null || a.f == null || a.f.Length == 0) return false;
            if (_nkColSr1736 == null) _nkColSr1736 = NewSR("nkcol1736", null, null, 0);
            var sr = _nkColSr1736; if (!sr.gameObject.activeSelf) sr.gameObject.SetActive(true);
            float t = atk ? (TZ.nkColAtk1713 - _nkColAtk1713) : Time.time;
            int fr = (int)(t * a.fps); fr = atk ? Mathf.Clamp(fr, 0, a.f.Length - 1) : (fr % a.f.Length + a.f.Length) % a.f.Length;
            sr.sprite = a.f[fr]; sr.flipX = flip; sr.color = Color.white;
            sr.transform.position = _nkColP1713; sr.transform.rotation = Quaternion.identity;
            sr.transform.localScale = Vector3.one * PT.skelArtScale561;
            sr.sortingOrder = YSort(_nkColP1713.y);
            return true;
        }
        void NkHideColossusSpr1736() { if (_nkColSr1736 != null && _nkColSr1736.gameObject.activeSelf) _nkColSr1736.gameObject.SetActive(false); }
    }
}
