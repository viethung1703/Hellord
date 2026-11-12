using UnityEngine;
using System.Collections.Generic;

namespace Hellord
{
    /// <summary>t1006u: Diablo-style boss room — smaller arena clamp, rim torches, theme trash adds.</summary>
    public partial class HellordGame
    {
        bool _bossArenaOn1006;
        Vector2 _bossArenaLo1006, _bossArenaHi1006;
        readonly List<SpriteRenderer> _bossArenaGlow1006 = new List<SpriteRenderer>();
        readonly List<SpriteRenderer> _bossArenaProp1026 = new List<SpriteRenderer>(); // t1026u: visible brazier under each glow
        static Sprite[] _bossBrazF1026;
        float _bossArenaPropT1026;
        float _bossAddsT1006;

        void SetupBossArena1006()
        {
            ClearBossArena1006();
            var T = HellordTuning.I;
            if (!T.bossArenaClamp1006 || _room == null) { _bossArenaOn1006 = false; return; }
            _bossArenaOn1006 = true;
            _room.Walkable(8, out var flo, out var fhi);
            // t1022u: when Ox Pit is really scaled, arena = full walkable (no nested 960 box inside still-huge art)
            if (HellordRoom.BossSX677 < 0.999f)
            {
                _bossArenaLo1006 = flo; _bossArenaHi1006 = fhi;
            }
            else
            {
                Vector2 mid = (flo + fhi) * 0.5f;
                if (BossAlive) mid = Vector2.Lerp(mid, _boss.pos, 0.35f);
                float hw = (T.bossArenaWPx1006 * 0.5f) / HArt.PPU;
                float hh = (T.bossArenaHPx1006 * 0.5f) / HArt.PPU;
                _bossArenaLo1006 = new Vector2(Mathf.Max(flo.x, mid.x - hw), Mathf.Max(flo.y, mid.y - hh));
                _bossArenaHi1006 = new Vector2(Mathf.Min(fhi.x, mid.x + hw), Mathf.Min(fhi.y, mid.y + hh));
            }
            ClampToArena(ref _heroPos); if (_hero) _hero.position = _heroPos;
            if (BossAlive) { var bp = _boss.pos; ClampToArena(ref bp); _boss.pos = bp; if (_bossRoot) _bossRoot.position = bp; }
            if (T.bossArenaLights1006) PlaceBossArenaLights1006();
            _bossAddsT1006 = 0.8f;
            if (T.bossAdds1006) SpawnBossAdds1006(T.bossAddsStart1006);
            Debug.Log($"[Hellord] t1006u boss arena [{_bossArenaLo1006.x:0.0},{_bossArenaLo1006.y:0.0}]-[{_bossArenaHi1006.x:0.0},{_bossArenaHi1006.y:0.0}] lights={_bossArenaGlow1006.Count} props={_bossArenaProp1026.Count}");
        }

        void PlaceBossArenaLights1006()
        {
            var T = HellordTuning.I;
            if (_room == null || _room.Root == null) return;
            EnsureBossBrazFrames1026();
            Vector2 lo = _bossArenaLo1006, hi = _bossArenaHi1006;
            int seed = 0;
            if (T.bossArenaRegular1046)
            {
                // t1046u: mirrored wall rows — equal count + spacing on opposite walls (no perimeter crawl)
                int nLong = Mathf.Max(2, T.bossArenaRimLong1046);
                int nShort = Mathf.Max(2, T.bossArenaRimShort1046);
                float inset = Mathf.Max(0.2f, T.bossArenaRimInset1046);
                float mx = Mathf.Clamp01(T.bossArenaRimMargin1046) * Mathf.Max(0.1f, hi.x - lo.x);
                float my = Mathf.Clamp01(T.bossArenaRimMargin1046) * Mathf.Max(0.1f, hi.y - lo.y);
                float x0 = lo.x + mx, x1 = hi.x - mx, y0 = lo.y + my, y1 = hi.y - my;
                void Rim(float x, float y)
                {
                    float floorCx = x * HArt.PPU, floorCy = -y * HArt.PPU;
                    PlaceBossBrazierGrounded1027(floorCx, floorCy, seed, T.bossArenaLightRPx1006, T.bossArenaLightK1006, HellordRoom.LightOrder + 250 + seed);
                    seed++;
                }
                for (int i = 0; i < nLong; i++)
                {
                    float t = (i + 0.5f) / nLong; float x = Mathf.Lerp(x0, x1, t);
                    Rim(x, lo.y + inset); // South
                    Rim(x, hi.y - inset); // North (same X — mirrored)
                }
                for (int i = 0; i < nShort; i++)
                {
                    float t = (i + 0.5f) / nShort; float y = Mathf.Lerp(y0, y1, t);
                    Rim(lo.x + inset, y); // West
                    Rim(hi.x - inset, y); // East (same Y — mirrored)
                }
            }
            else
            {
                // legacy: equal spacing along perimeter (can look uneven on rectangles)
                int n = Mathf.Max(4, T.bossArenaLightN1006);
                float wlen = Mathf.Max(0.1f, hi.x - lo.x), hlen = Mathf.Max(0.1f, hi.y - lo.y);
                float peri = 2f * (wlen + hlen), inset = 0.55f;
                for (int i = 0; i < n; i++)
                {
                    float d = ((i + 0.5f) / n) * peri;
                    Vector2 w;
                    if (d < wlen) w = new Vector2(lo.x + d, lo.y);
                    else if ((d -= wlen) < hlen) w = new Vector2(hi.x, lo.y + d);
                    else if ((d -= hlen) < wlen) w = new Vector2(hi.x - d, hi.y);
                    else { d -= wlen; w = new Vector2(lo.x, hi.y - Mathf.Clamp(d, 0, hlen)); }
                    Vector2 mid = (lo + hi) * 0.5f;
                    Vector2 toward = mid - w; if (toward.sqrMagnitude > 1e-6f) w += toward.normalized * inset;
                    PlaceBossBrazierGrounded1027(w.x * HArt.PPU, -w.y * HArt.PPU, seed++, T.bossArenaLightRPx1006, T.bossArenaLightK1006, HellordRoom.LightOrder + 250 + i);
                }
            }
            // t1021u/t1041u/t1046u: symmetric L/R fills only (fillN=2)
            int fill = Mathf.Max(0, T.bossArenaFillN1021);
            if (fill > 0)
            {
                Vector2 mid = (lo + hi) * 0.5f;
                float ix = (hi.x - lo.x) * 0.28f, iy = (hi.y - lo.y) * 0.28f;
                var pts = new List<Vector2> { mid + new Vector2(-ix, 0), mid + new Vector2(ix, 0), mid + new Vector2(0, -iy), mid + new Vector2(0, iy), mid };
                for (int i = 0; i < fill && i < pts.Count; i++)
                {
                    float floorCx = pts[i].x * HArt.PPU, floorCy = -pts[i].y * HArt.PPU;
                    PlaceBossBrazierGrounded1027(floorCx, floorCy, seed + i, T.bossArenaFillRPx1021, T.bossArenaFillK1021, HellordRoom.LightOrder + 280 + i);
                }
            }
        }

        /// <summary>t1026u/t1027u: animated oxpit floor brazier with feet on the floor; glow at flame tip only.
        /// Strip is top-left pivot 26x38; opaque bottom ~y37; flame ~(13,19) from top → ~18px above feet.</summary>
        void PlaceBossBrazierGrounded1027(float floorCx, float floorCy, int seed, int glowR, float glowK, int glowOrder)
        {
            if (_room == null || _room.Root == null) return;
            EnsureBossBrazFrames1026();
            if (_bossBrazF1026 == null || _bossBrazF1026.Length == 0) return;
            int fi = ((seed % 4) + 4) % 4;
            float psx = HellordRoom.BossSX677 < 0.999f ? HellordRoom.BossSX677 : 1f;
            float psy = HellordRoom.BossSY677 < 0.999f ? HellordRoom.BossSY677 : 1f;
            // t1028u: baked shadow lip (y34-37) cleared — solid bowl ends at y33; seat solid feet on floor
            const float bw = 26f, bh = 34f, flameFromTop = 19f;
            float bx = floorCx - (bw * 0.5f) * psx;
            float by = floorCy - bh * psy;
            var go = new GameObject("boss_brazier_1026"); go.transform.SetParent(_room.Root, false);
            go.transform.position = HellordRoom.Px(bx, by);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _bossBrazF1026[fi];
            if (psx < 0.999f || psy < 0.999f) sr.transform.localScale = new Vector3(psx, psy, 1);
            sr.sortingOrder = HellordRoom.PropOrder + 20 + (seed & 7);
            sr.spriteSortPoint = SpriteSortPoint.Pivot;
            _bossArenaProp1026.Add(sr);
            // glow at flame tip above feet (do NOT lift the prop)
            float flameCx = floorCx;
            float flameCy = floorCy - (bh - flameFromTop) * psy;
            var glow = HellordRoom.SourceGlow46(_room.Root, flameCx, flameCy, glowR, glowK, glowOrder);
            if (glow) _bossArenaGlow1006.Add(glow);
        }

        static void EnsureBossBrazFrames1026()
        {
            if (_bossBrazF1026 != null && _bossBrazF1026.Length == 4 && _bossBrazF1026[0]) return;
            _bossBrazF1026 = HArt.Strip("oxpit_prop_brazier", 4, 26, 38, Vector2.zero);
        }

        void TickBossArenaProps1026(float dt)
        {
            if (_bossArenaProp1026.Count == 0 || _bossBrazF1026 == null || _bossBrazF1026.Length == 0) return;
            _bossArenaPropT1026 += dt; int f = (int)(_bossArenaPropT1026 * 8f);
            for (int i = 0; i < _bossArenaProp1026.Count; i++)
            {
                var sr = _bossArenaProp1026[i]; if (!sr) continue;
                sr.sprite = _bossBrazF1026[(f + i) % 4];
            }
        }

        void ClearBossArena1006()
        {
            foreach (var sr in _bossArenaGlow1006)
            {
                if (!sr) continue;
                HellordRoom.Glow586.Remove(sr);
                if (sr.gameObject) Object.Destroy(sr.gameObject);
            }
            _bossArenaGlow1006.Clear();
            foreach (var sr in _bossArenaProp1026)
            {
                if (!sr) continue;
                if (sr.gameObject) Object.Destroy(sr.gameObject);
            }
            _bossArenaProp1026.Clear();
            _bossArenaPropT1026 = 0;
            _bossArenaOn1006 = false;
            _bossAddsT1006 = 0;
        }

        void UpdateBossAdds1006(float dt)
        {
            TickBossArenaProps1026(dt); // t1026u: animate braziers even if adds are off
            var T = HellordTuning.I;
            if (!T.bossAdds1006 || !_arenaOn || !BossAlive) return;
            _bossAddsT1006 -= dt;
            if (_bossAddsT1006 > 0) return;
            _bossAddsT1006 = Mathf.Max(1f, T.bossAddsEvery1006);
            int need = Mathf.Max(0, T.bossAddsCap1006 - AliveCount());
            if (need <= 0) return;
            SpawnBossAdds1006(Mathf.Min(T.bossAddsWave1006, need));
        }

        void SpawnBossAdds1006(int n)
        {
            if (n <= 0 || _room == null) return;
            for (int i = 0; i < n; i++)
            {
                Vector2 p = BossAddSpawnPoint1006();
                EType t = PickBossAddType1006();
                var e = SpawnEnemyAt(t, p);
                if (e == null) continue;
                ApplyVariant37(e);
                e.dmg *= 0.7f;
                e.maxHp *= 0.85f; e.hp = e.maxHp;
            }
        }

        EType PickBossAddType1006()
        {
            float r = Random.value;
            if (r < 0.55f) return EType.Imp;
            if (r < 0.88f) return EType.Ghoul;
            return EType.Brute;
        }

        Vector2 BossAddSpawnPoint1006()
        {
            Vector2 lo, hi;
            if (_bossArenaOn1006) { lo = _bossArenaLo1006; hi = _bossArenaHi1006; }
            else if (_room != null) _room.Walkable(8, out lo, out hi);
            else return _heroPos + Vector2.right * 3f;
            Vector2 best = (lo + hi) * 0.5f; float bd = -1f;
            for (int k = 0; k < 12; k++)
            {
                Vector2 p = new Vector2(Random.Range(lo.x, hi.x), Random.Range(lo.y, hi.y));
                if (Random.value < 0.7f)
                {
                    int edge = Random.Range(0, 4);
                    if (edge == 0) p.y = lo.y + 0.4f;
                    else if (edge == 1) p.y = hi.y - 0.4f;
                    else if (edge == 2) p.x = lo.x + 0.4f;
                    else p.x = hi.x - 0.4f;
                }
                float d = (p - _heroPos).sqrMagnitude;
                if (BossAlive && (p - _boss.pos).sqrMagnitude < 6.25f) continue;
                if (d > bd) { bd = d; best = p; }
            }
            return best;
        }

        /// <summary>t1006u: camera clamps to the lit arena so the view cannot scroll into the black outside.</summary>
        Rect BossArenaCamRect1006()
        {
            if (!_bossArenaOn1006) return default;
            var lo = _bossArenaLo1006; var hi = _bossArenaHi1006;
            return new Rect(lo.x, lo.y, hi.x - lo.x, hi.y - lo.y);
        }
    }
}
