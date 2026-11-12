using PixelFXPack;
using System.Collections.Generic;
using UnityEngine;

// v0.0.49: combat redesign v0.2 first pass (Docs/Hellord_enemies_combat_redesign_v0.2.md).
// Camera / room size / density switches live in HellordGame / HellordV42Floors / HellordFloor (all gated by combatV49).
// This file: Intensity, dash, mercy valve, enemy bullets + grenades + ground patches, per-demon patterns, range-kit recolours, crossfire spawns.
namespace Hellord
{
    public partial class HellordGame
    {
        static HellordTuning T49 => HellordTuning.I;
        bool V49 => T49.combatV49;

        // ---------------- Intensity ----------------
        /// <summary>Player-side multipliers read by SkillInst (static so HellordData can see them).</summary>
        public static float PArea49 = 1f, PCd49 = 1f, PSpeed49 = 1f, PRange49 = 1f; public static int PCount49 = 0;
        float IntensityE49 => FloorIdx == 0 && V49 ? T49.floor0EnemyIntensity49d : V49 && T49.soften679 ? 1f + T49.enemyIPerFloor679 * FloorIdx : 1f + (Intensity49 - 1f) * T49.enemyIntensityMul49b;
        float EArea49 => V49 ? T49.enemyAreaMul49b : 1f;
        public float Intensity49 => V49 ? T49.intensityBase49 + T49.intensityPerFloor49 * FloorIdx : 1f;
        /// <summary>Enemy cooldowns tick this much faster (cooldown divided by I^exp, slowed by the mercy valve).</summary>
        float ECdDiv49 => (T49.soften679 ? Mathf.Min(T49.enemyCdDivMax679, Mathf.Pow(IntensityE49, T49.enemyCdExp49) / Mathf.Max(0.1f, T49.enemyCdMul49b)) : Mathf.Pow(IntensityE49, T49.enemyCdExp49) / Mathf.Max(0.1f, T49.enemyCdMul49b)) * (_mercyT49 > 0 ? T49.mercyFireMul49 : 1f);
        int EBullets49(int n) => Mathf.Max(1, Mathf.RoundToInt(n * (T49.soften679 ? Mathf.Min(T49.enemyBulletMaxMul679, Mathf.Pow(IntensityE49, T49.enemyBulletsExp49) * T49.enemyCountMul49b) : Mathf.Pow(IntensityE49, T49.enemyBulletsExp49) * T49.enemyCountMul49b)));
        float EDmgI49 => Mathf.Pow(IntensityE49, T49.enemyDmgExp49) * T49.enemyDmgMul49b;
        float EDmg49 => EDmgI49 * HellordTuning.I.contactDamageMul * (1 + (T49.soften679 ? T49.enemyDmgGrowth679 : HellordTuning.I.floorEnemyDmgGrowth) * FloorIdx);
        float BossCdDiv49 => V49 ? Mathf.Pow(IntensityE49, T49.bossCdExp49) / Mathf.Max(0.1f, T49.enemyCdMul49b) : 1f;
        float PDmg49() => V49 ? T49.playerDmgMul49 * T49.playerDmgMul49b * Mathf.Pow(PIntensity49, T49.playerDmgExp49) : 1f;
        /// <summary>v0.0.49f player-side Intensity: 1 on early floors, then relative to the base.</summary>
        float PIntensity49 => !V49 || FloorIdx < T49.playerScaleFloor49f ? 1f : T49.playerRelIntensity49f ? Intensity49 / Mathf.Max(0.1f, T49.intensityBase49) : Intensity49;
        float Cd44_49 => V49 ? ECdDiv49 : 1f;
        float FwMul49 => V49 ? T49.fwSpeedMul49 : 1f;
        float HookSpeedPx49 => V49 ? T49.hookSpeedPx49 : HellordTuning.I.hookSpeedPx;
        float HookLaneTell49 => V49 ? T49.hookLaneTellS49 : HellordTuning.I.hookLaneTellSeconds;
        float GossipTell49 => V49 ? T49.gossipTellS49 : HellordTuning.I.gossipShoutTellSeconds;
        float GossipR49 => V49 ? T49.gossipRadiusU49 : HellordTuning.I.gossipShoutRadiusU;
        float GossipScr49 => V49 ? T49.gossipScrambleS49 : HellordTuning.I.gossipScrambleSeconds;

        void Tick49Statics()
        {
            float I = PIntensity49;
            PArea49 = V49 ? T49.playerAreaMul49 * T49.playerAreaMul49b * Mathf.Pow(I, T49.playerAreaExp49) : 1f;
            PCd49 = V49 ? 1f / Mathf.Max(0.1f, Mathf.Pow(I, T49.playerCdExp49)) : 1f;
            if (V49 && FloorIdx < T49.playerScaleFloor49f) { PArea49 = T49.playerAreaMul49b; PCd49 = 1f; }
            PSpeed49 = V49 ? T49.playerProjSpeedMul49b : 1f; PRange49 = V49 ? T49.playerRangeMul49b : 1f;
            PCount49 = !V49 || FloorIdx < T49.playerCountFloor49g ? 0 : I >= T49.playerCountAtI49.y ? 2 : I >= T49.playerCountAtI49.x ? 1 : 0;
        }

        // ---------------- room limits (static: PickArt42 is static) ----------------
        /// <summary>v0.0.49i: rerolls failed (e.g. no art under the floor-1 max ignored the limit and kept a huge room). Pick a random fitting fight art, else the smallest.</summary>
        static string FitArt49i(RoomType t, string a, ref int cls)
        {
            var ok = new System.Collections.Generic.List<int>(); int best = 0; float bestA = float.MaxValue;
            for (int i = 0; i < 6; i++) { float ar = ArtAreaU2_42(SizeArt[i / 2][i % 2]); if (ar >= RoomMin49(t) && ar <= RoomMax49(t)) ok.Add(i); if (ar < bestA) { bestA = ar; best = i; } }
            int k = ok.Count > 0 ? ok[Random.Range(0, ok.Count)] : best; cls = k / 2;
            return SizeArt[k / 2][k % 2] + (Random.value < 0.5f ? "_a" : "_b");
        }
        /// <summary>v0.0.49i hard cap on demons alive at once (floor 1 by fight-room index, floor 2+ by floor).</summary>
        int AliveCap49i => !V49 ? 9999 : FloorIdx == 0 ? Mathf.Min(T49.aliveCapF0Max49i, T49.aliveCapF0Start49i + T49.aliveCapF0PerRoom49i * FightsCleared49()) : T49.aliveCapF2_49i + T49.aliveCapPerFloor49i * (FloorIdx - 1);
        /// <summary>v0.0.49i hard cap on kill goals (Hunt / Treasure).</summary>
        int KillGoalCap49i => !V49 ? 9999 : FloorIdx == 0 ? Mathf.Min(T49.killGoalF0Max49i, T49.killGoalF0Start49i + T49.killGoalF0PerRoom49i * FightsCleared49()) : T49.killGoalF2_49i + T49.killGoalPerFloor49i * (FloorIdx - 1);
        static int _genFloor49;
        static float RoomMin49(RoomType t) { var T = HellordTuning.I; if (!T.combatV49) return T.roomWalkAreaMinU2; return _genFloor49 == 0 ? 0f : T.floorNRoomMinU2_49d; }
        static float RoomMax49(RoomType t)
        {
            var T = HellordTuning.I; if (!T.combatV49) return T.roomWalkAreaMaxU2;
            if (_genFloor49 == 0) return T.floor0RoomMaxU2_49i * (t == RoomType.Survive || t == RoomType.Nests || t == RoomType.Elite ? T.floor0BigRoomMul49i : 1f); // v0.0.49i
            float g = T.roomMaxPerFloor49d * (_genFloor49 - 1); return (t == RoomType.Survive ? T.roomSurviveMaxU2_49 : T.roomWalkMaxU2_49) + g;
        }
        float Density49 => T49.density49hOn ? (FloorIdx == 0 ? T49.floor0Density49h : T49.floor2Density49h + (T49.soften679 ? T49.densityPerFloor679 : T49.densityPerFloor49h) * (FloorIdx - 1)) : FloorIdx == 0 ? T49.floor0Density49d : T49.density49 + T49.densityPerFloor49d * (FloorIdx - 1);
        bool Complex49 => FloorIdx >= T49.complexFloor49d;
        int FightsCleared49() { int n = 0; foreach (var r in floor) if (r.IsFight && r.cleared && r.type != RoomType.Boss) n++; return n; }
        /// <summary>v0.0.49d floor-1 ladder: melee first, then simple range, then other types.</summary>
        bool Floor0MeleeOnly49 => V49 && floorMode && FloorIdx == 0 && FightsCleared49() < T49.floor0MeleeOnlyRooms49d;
        bool Floor0NoOthers49 => V49 && floorMode && FloorIdx == 0 && FightsCleared49() < T49.floor0OtherTypesRooms49d;

        // ---------------- dash ----------------
        float _dashCd49, _dashT49, _holdT49, _mercyT49; Vector2 _dashDir49;
        readonly Queue<Vector2> _dmgLog49 = new Queue<Vector2>();
        float HoldMul49() => !V49 ? 1f : _holdT49 > 0 ? T49.ghoulHoldMoveMul49 : InZone49(_heroPos, 2) ? 0.6f : 1f;
        void Dash49(Vector2 mv, float dt)
        {
            _dashCd49 -= dt;
            if (!V49 || mode != Mode.Play) { _dashT49 = 0; return; }
            if (_dashT49 <= 0 && _dashCd49 <= 0 && DashPressed716())
            {
                _dashDir49 = DashDir716(mv); _dashT49 = T49.dashTimeS49; _dashCd49 = T49.dashCooldownS49 * PactDashCdMul53() * _sinDashMul737;
                _iframe = Mathf.Max(_iframe, T49.dashIFrameS49); _heroVel37 = Vector2.zero; _holdT49 = 0;
                if (T49.dashBreaksHook49 && _dragBy != null) { if (_dragBy.hookH >= 0) HookChainFX.Release(_dragBy.hookH); _dragBy = null; }
                HordeFX.CheapBurst(_heroPos + Vector2.up * 0.4f, E32(0xc0cbdc), 6); HellordAudio.Play("oxhead_lunge_dash", 2f); PactDash53(); SinDash818(); // t818u #19 // v0.0.53 pact dash hooks
            }
            if (_dashT49 > 0)
            {
                float step = Mathf.Min(dt, _dashT49); _dashT49 -= dt;
                ZDisc(_heroPos, 0.45f, E32(0xc0cbdc, 0.35f), 0.6f, 3);
                _heroPos += _dashDir49 * (T49.dashDistU49 / Mathf.Max(0.02f, T49.dashTimeS49)) * step;
            }
            if (_dashCd49 > 0) { float p = 1f - Mathf.Clamp01(_dashCd49 / Mathf.Max(0.05f, T49.dashCooldownS49)); Vector2 a = _heroPos + new Vector2(-0.5f, -0.3f); ZSeg(a, a + Vector2.right, 0.1f, E32(0x181425, 0.6f), 4); ZSeg(a, a + Vector2.right * p, 0.1f, E32(0x2ce8f5, 0.9f), 5); }
        }

        // ---------------- mercy valve ----------------
        void Mercy49(float dmg)
        {
            if (!V49) return; float now = runTime; _dmgLog49.Enqueue(new Vector2(now, dmg));
            while (_dmgLog49.Count > 0 && _dmgLog49.Peek().x < now - T49.mercyWindowS49) _dmgLog49.Dequeue();
            float sum = 0; foreach (var d in _dmgLog49) sum += d.y;
            if (sum > T49.mercyHpFrac49 * maxHp && _mercyT49 <= 0) { _mercyT49 = T49.mercyDurationS49; _dmgLog49.Clear(); if (showDebug) Debug.Log("[v49] mercy valve on"); }
        }

        // ---------------- enemy bullets ----------------
        class EB49 { public bool alive, vis, st53; public Vector2 p, v; public float r, dmg, life; public int chill; public string src; public SpriteRenderer sr, core; }
        readonly List<EB49> _eb49 = new List<EB49>(64); Sprite _disc49; int _ebAlive49;
        Sprite Disc49()
        {
            if (_disc49 != null) return _disc49;
            var t = new Texture2D(16, 16, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < 16; y++) for (int x = 0; x < 16; x++) { float dx = x - 7.5f, dy = y - 7.5f; t.SetPixel(x, y, dx * dx + dy * dy <= 60f ? Color.white : Color.clear); }
            t.Apply(); _disc49 = Sprite.Create(t, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f); return _disc49;
        }
        EB49 NewBullet49(bool vis)
        {
            if (!vis && _ebAlive49 >= T49.bulletMax49) return null;
            EB49 b = null; foreach (var x in _eb49) if (!x.alive) { b = x; break; }
            if (b == null)
            {
                b = new EB49(); var go = new GameObject("eb49"); b.sr = go.AddComponent<SpriteRenderer>(); b.sr.sprite = Disc49();
                var g2 = new GameObject("core"); g2.transform.SetParent(go.transform, false); b.core = g2.AddComponent<SpriteRenderer>(); b.core.sprite = Disc49(); _eb49.Add(b);
            }
            b.alive = true; b.st53 = false; b.vis = vis; b.sr.gameObject.SetActive(true); b.sr.sortingOrder = T49.bulletSort49; b.core.sortingOrder = T49.bulletSort49 + 1; if (!vis) _ebAlive49++;
            return b;
        }
        void Fire49(Vector2 p, Vector2 v, float r, float dmg, Color col, string src, int chill = 0)
        {
            var b = NewBullet49(false); if (b == null) return;
            b.p = p; b.v = v; b.r = r; b.dmg = dmg; b.life = T49.bulletLifeS49; b.chill = chill; b.src = src;
            b.sr.color = col; b.core.color = new Color(1f, 1f, 1f, 0.95f); b.sr.transform.localScale = Vector3.one * r * 2.6f; b.core.transform.localScale = Vector3.one * 0.5f;
            PlaceBullet49(b);
        }
        void PlaceBullet49(EB49 b) { b.sr.transform.position = new Vector3(b.p.x, b.p.y + 0.6f, 0); }
        void KillBullet49(EB49 b, bool puff) { if (!b.alive) return; if (puff) HordeFX.CheapBurst(b.p + Vector2.up * 0.6f, b.sr.color, 3); b.alive = false; b.sr.gameObject.SetActive(false); if (!b.vis) _ebAlive49--; }
        void EraseCone49(Vector2 c, Vector2 dir, float r, float half)
        {
            if (!V49 || !T49.frostEraseBullets49) return;
            foreach (var b in _eb49) { if (!b.alive || b.vis) continue; Vector2 to = b.p + Vector2.up * 0.6f - c; if (to.magnitude < r && (half >= 179f || Vector2.Angle(dir, to) < half)) KillBullet49(b, true); }
        }
        void UpdateBullets49(float dt)
        {
            bool ring = _aura != null && _auraR > 0;
            foreach (var b in _eb49)
            {
                if (!b.alive || b.vis) continue;
                b.life -= dt; b.p += b.v * dt; if (b.life <= 0) { KillBullet49(b, false); continue; }
                Vector2 q = b.p; ClampToRoom(ref q, 0); if ((q - b.p).sqrMagnitude > 0.01f) { KillBullet49(b, true); continue; }
                float dh = (_heroPos - b.p).magnitude;
                if (ring && Mathf.Abs(dh - _auraR) < T49.stormEraseBandU49) { KillBullet49(b, true); continue; }
                if (mode == Mode.Play && dh < b.r + T49.bulletHeroR49 * HeroHitK47)
                {
                    int h0 = heroHits; HurtHero(b.dmg, b.src); if (heroHits > h0) { if (b.chill > 0) AddChill44(b.chill); KillBullet49(b, true); continue; }
                }
                PlaceBullet49(b);
            }
        }

        // ---------------- grenades + ground patches ----------------
        class Gren49 { public Vector2 a, b; public float t, T, r, dmg, delay; public int kind; public EB49 vis; }
        struct Zone49 { public Vector2 c; public float r, t; public int kind; }
        readonly List<Gren49> _gr49 = new List<Gren49>(); readonly List<Zone49> _zones49 = new List<Zone49>();
        static readonly int[] GrenCol49 = { 0xc0cbdc, 0xf77622, 0xead4aa };
        void Lob49(Vector2 from, Vector2 to, float r, float dmg, int kind, float delay = 0f)
        {
            Vector2 q = to; ClampToRoom(ref q, 8);
            _gr49.Add(new Gren49 { a = from, b = q, t = -delay, T = T49.gnFlightS49, r = r, dmg = dmg, kind = kind });
        }
        bool InZone49(Vector2 p, int kind) { foreach (var z in _zones49) if (z.kind == kind && (p - z.c).magnitude < z.r) return true; return false; }
        void UpdateGrenades49(float dt)
        {
            for (int i = _gr49.Count - 1; i >= 0; i--)
            {
                var g = _gr49[i]; g.t += dt; if (g.t < 0) continue;
                float p = Mathf.Clamp01(g.t / Mathf.Max(0.05f, g.T)); Color c = E32(GrenCol49[g.kind]);
                ZDisc(g.b, g.r, new Color(c.r, c.g, c.b, 0.12f + 0.2f * p), 0.7f, 0); ZDisc(g.b, g.r * p, E32(0xe43b44, 0.18f), 0.7f, 1); ZRing(g.b, g.r, new Color(c.r, c.g, c.b, 0.85f), 0.7f, 2);
                if (g.vis == null) { g.vis = NewBullet49(true); if (g.vis != null) { g.vis.sr.color = c; g.vis.core.color = Color.white; g.vis.sr.transform.localScale = Vector3.one * 0.7f; g.vis.core.transform.localScale = Vector3.one * 0.45f; } }
                Vector2 pos = Vector2.Lerp(g.a, g.b, p); float h = 4f * p * (1f - p) * 3f;
                if (g.vis != null) g.vis.sr.transform.position = new Vector3(pos.x, pos.y + 0.6f + h, 0);
                if (p < 1f) continue;
                if (g.vis != null) KillBullet49(g.vis, false);
                _gr49.RemoveAt(i);
                HordeFX.CheapBurst(g.b + Vector2.up * 0.3f, c, 8); HellordAudio.Play("fireball_impact", 0.8f);
                if (mode == Mode.Play && (_heroPos - g.b).magnitude < g.r + 0.2f * HeroHitK47) { int h0 = heroHits; HurtHero(g.dmg, g.kind == 0 ? "snowball" : g.kind == 1 ? "ember" : "rice_sack"); if (heroHits > h0 && g.kind == 0) AddChill44(1); }
                AddZone49(g.b, g.r * 0.8f, g.kind);
            }
            for (int i = _zones49.Count - 1; i >= 0; i--)
            {
                var z = _zones49[i]; z.t -= dt;
                if (z.t <= 0) { RemoveIce49(z); _zones49.RemoveAt(i); continue; }
                _zones49[i] = z; float a = Mathf.Clamp01(z.t / 0.5f);
                if (z.kind != 1) ZDisc(z.c, z.r, E32(z.kind == 0 ? 0xc0cbdc : 0xead4aa, 0.3f * a), 0.7f, 0);
            }
        }
        void AddZone49(Vector2 c, float r, int kind)
        {
            _zones49.Add(new Zone49 { c = c, r = r, t = kind == 0 && r < 1f ? T49.fbSlickS49 : T49.gnPatchS49, kind = kind });
            if (kind == 0) _ice.Add(new Ice37 { c = c, r = r, sr = null, fx = -1 });
            else if (kind == 1) AddTrail44(c, 0, T49.gnPatchS49, r, T49.lavaDmg49 * EDmg49);
        }
        void RemoveIce49(Zone49 z)
        {
            if (z.kind != 0) return;
            for (int k = _ice.Count - 1; k >= 0; k--) if (_ice[k].sr == null && _ice[k].fx < 0 && (_ice[k].c - z.c).sqrMagnitude < 1e-4f && Mathf.Abs(_ice[k].r - z.r) < 1e-3f) { _ice.RemoveAt(k); break; }
        }

        void Clear49()
        {
            PArea49 = PCd49 = PSpeed49 = PRange49 = 1f; PCount49 = 0; // v0.0.49g: no stale statics at run start
            foreach (var b in _eb49) KillBullet49(b, false); _ebAlive49 = 0;
            foreach (var z in _zones49) RemoveIce49(z);
            _gr49.Clear(); _zones49.Clear(); _dmgLog49.Clear(); _holdT49 = _mercyT49 = _dashT49 = _dashCd49 = 0;
        }

        /// <summary>Called once per Play frame (after the boss bullets).</summary>
        void Update49(float dt)
        {
            Tick49Statics(); if (!V49) return;
            if (_mercyT49 > 0) _mercyT49 -= dt; if (_holdT49 > 0) { _holdT49 -= dt; ZRing(_heroPos, 0.6f, E32(0xa22633, 0.8f), 0.6f, 3); }
            UpdateBullets49(dt); UpdateGrenades49(dt);
        }

        // ---------------- helpers ----------------
        Vector2 Lead49(Vector2 from, float speed) { float d = (_heroPos - from).magnitude; float t = speed > 0.1f ? d / speed : 0f; return _heroPos + _heroVel37 * t * T49.leadAim49; }
        Vector2 LeadDir49(Vector2 from, float speed) { Vector2 d = Lead49(from, speed) - from; return d.sqrMagnitude > 1e-4f ? d.normalized : Vector2.right; }
        static float SegDist49(Vector2 p, Vector2 a, Vector2 b) { Vector2 ab = b - a; float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(1e-5f, ab.sqrMagnitude)); return (a + ab * t - p).magnitude; }
        static float Deg49(Vector2 d) => Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        static Vector2 Rot49(Vector2 d, float deg) => (Vector2)(Quaternion.Euler(0, 0, deg) * d);
        void Ring49(Vector2 c, int n, int gaps, float speed, float rotDeg, float dmg, Color col, string src)
        {
            n = Mathf.Max(1, n); int step = gaps > 0 ? Mathf.Max(2, n / gaps) : 0, gw = Mathf.Max(1, Mathf.RoundToInt(n / 16f));
            for (int i = 0; i < n; i++) { if (step > 0 && i % step < gw) continue; Vector2 d = Rot49(Vector2.right, rotDeg + 360f * i / n); Fire49(c, d * speed, 0.2f, dmg, col, src); }
        }

        // ---------------- per-demon patterns ----------------
        // k49: 0 base demon, 1 Imp Gunner, 2 Snowball Imp, 3 Ember Spitter, 4 Flame Wraith, 5 Ice Sniper Wraith, 6 Frost Brute, 7 Sack Lobber, 8 Archer
        void Think49(Enemy e, ref Vector2 v, Vector2 to, float dist, float sp, float dt, ref Color tint)
        {
            if (e.type == EType.HookThrower || e.type == EType.GossipImp || e.nest) return;
            if (e.freezeT > 0) return;
            var T = T49; Vector2 dir = dist > 0.01f ? to / dist : Vector2.right;
            e.cd49 -= dt * ECdDiv49;
            if (e.k49 >= 1 && e.k49 != 6) { ThinkRange49(e, ref v, dir, dist, sp, dt, ref tint); return; }
            if (e.k49 == 6) { ThinkFrostBrute49(e, ref v, dir, dist, sp, dt, ref tint); return; }
            if (!Complex49) return; // v0.0.49d floor 1: plain chase melee
            switch (e.type)
            {
                case EType.Imp: case EType.SplitChild: ThinkLunge49(e, ref v, dir, dist, dt, ref tint); break;
                case EType.Ghoul: ThinkGrab49(e, ref v, dist, dt, ref tint); break;
                case EType.Brute: ThinkBrute49(e, ref v, dir, dist, dt, ref tint); break;
                case EType.Elite: ThinkElite49(e, ref v, dir, dist, dt, ref tint); break;
            }
        }

        void ThinkLunge49(Enemy e, ref Vector2 v, Vector2 dir, float dist, float dt, ref Color tint)
        {
            var T = T49; float spd = T.impLungeDistU49 / Mathf.Max(0.05f, T.impLungeTimeS49);
            if (e.type == EType.SplitChild && e.n49 == 0) { e.n49 = 1; e.cd49 = Random.value * 0.3f; }
            float range = T.impLungeRangeU49 * (e.n49 == 1 ? 2f : 1f);
            switch (e.s49)
            {
                case 0: if (e.cd49 <= 0 && dist < range) { e.s49 = 1; e.t49 = T.impLungeTellS49; e.a49 = LeadDir49(e.pos, spd); } break;
                case 1:
                {
                    v = Vector2.zero; e.t49 -= dt; float p = 1f - Mathf.Clamp01(e.t49 / Mathf.Max(0.05f, T.impLungeTellS49));
                    ZSeg(e.pos, e.pos + e.a49 * T.impLungeDistU49, 0.5f, E32(0xe43b44, 0.15f + 0.35f * p), 1); tint = Color.Lerp(Color.white, new Color(1f, 0.5f, 0.45f), p);
                    if (e.t49 <= 0) { e.s49 = 2; e.t49 = T.impLungeTimeS49; }
                    break;
                }
                case 2: v = e.a49 * spd; e.t49 -= dt; if (e.t49 <= 0) { e.s49 = 0; e.cd49 = T.impLungeCdS49; e.n49 = 2; } break;
            }
        }

        void ThinkGrab49(Enemy e, ref Vector2 v, float dist, float dt, ref Color tint)
        {
            var T = T49; float reach = (T.ghoulGrabReachU49 * EArea49);
            if (e.s49 == 0) { if (e.cd49 <= 0 && dist < reach + 0.6f) { e.s49 = 1; e.t49 = T.ghoulGrabTellS49; } return; }
            v *= 0.25f; e.t49 -= dt; float p = 1f - Mathf.Clamp01(e.t49 / Mathf.Max(0.05f, T.ghoulGrabTellS49));
            ZDisc(e.pos, reach, E32(0xa22633, 0.1f + 0.25f * p), 0.7f, 0); ZRing(e.pos, reach, E32(0xe43b44, 0.8f), 0.7f, 1); tint = Color.Lerp(Color.white, new Color(1f, 0.55f, 0.5f), p);
            if (e.t49 > 0) return;
            if (mode == Mode.Play && dist < reach + 0.3f * HeroHitK47) { int h0 = heroHits; HurtHero(e.dmg * T.ghoulGrabDmgMul49 * EDmgI49, "ghoul_grab"); if (heroHits > h0) _holdT49 = T.ghoulHoldS49; }
            e.s49 = 0; e.cd49 = T.ghoulGrabCdS49;
        }

        void ThinkBrute49(Enemy e, ref Vector2 v, Vector2 dir, float dist, float dt, ref Color tint)
        {
            var T = T49; float stepSpd = T.bruteStepU49 / Mathf.Max(0.05f, T.bruteStepS49);
            switch (e.s49)
            {
                case 0: if (e.cd49 <= 0 && dist < T.bruteRangeU49) { e.s49 = 1; e.t49 = T.bruteTellS49; e.a49 = LeadDir49(e.pos, stepSpd); e.b49 = e.pos + e.a49 * Mathf.Min(T.bruteStepU49, RayToBox44(e.pos, e.a49)); } break;
                case 1:
                {
                    v = Vector2.zero; e.t49 -= dt; float p = 1f - Mathf.Clamp01(e.t49 / Mathf.Max(0.05f, T.bruteTellS49));
                    ZSeg(e.pos, e.b49, e.radius * 1.6f, E32(0xa22633, 0.15f + 0.3f * p), 1); ZWedge(e.b49, Deg49(e.a49), (T.bruteCleaveHalfDeg49 * EArea49), (T.bruteCleaveU49 * EArea49), E32(0xe43b44, 0.08f + 0.1f * p), 0);
                    tint = Color.Lerp(Color.white, new Color(1f, 0.6f, 0.4f), p);
                    if (e.t49 <= 0) { e.s49 = 2; e.t49 = T.bruteStepS49; HellordAudio.Play("oxhead_lunge_dash", 1.2f); }
                    break;
                }
                case 2: v = e.a49 * stepSpd; e.t49 -= dt; if (e.t49 <= 0) { e.s49 = 3; e.t49 = T.bruteCleaveTellS49; e.a49 = dir; } break;
                case 3:
                {
                    v = Vector2.zero; e.t49 -= dt; float p = 1f - Mathf.Clamp01(e.t49 / Mathf.Max(0.05f, T.bruteCleaveTellS49));
                    ZWedge(e.pos, Deg49(e.a49), (T.bruteCleaveHalfDeg49 * EArea49), (T.bruteCleaveU49 * EArea49), E32(0xe43b44, 0.25f + 0.4f * p), 1);
                    if (e.t49 <= 0)
                    {
                        if (mode == Mode.Play && dist < (T.bruteCleaveU49 * EArea49) + 0.3f * HeroHitK47 && Vector2.Angle(e.a49, dir) < (T.bruteCleaveHalfDeg49 * EArea49)) HurtHero(e.dmg * T.bruteCleaveDmgMul49 * EDmgI49, "brute_cleave");
                        ScreenShake.Add(2, 0.12f); HellordAudio.Play("oxhead_horn_strike", 1.3f); e.s49 = 4; e.t49 = T.bruteStuckS49;
                    }
                    break;
                }
                case 4: v = Vector2.zero; e.t49 -= dt; tint = new Color(0.75f, 0.75f, 0.75f); if (e.t49 <= 0) { e.s49 = 0; e.cd49 = T.bruteCdS49; } break;
            }
        }

        void ThinkElite49(Enemy e, ref Vector2 v, Vector2 dir, float dist, float dt, ref Color tint)
        {
            var T = T49;
            switch (e.s49)
            {
                case 0:
                    if (e.cd49 <= 0 && dist < 12f) { e.s49 = 1; e.t49 = T.eliteBlinkTellS49; Vector2 b = _heroPos - _lastDir * T.eliteBehindU49; ClampToRoom(ref b, 10); e.b49 = b; HellordAudio.Play("curse_rune_arm", 0.8f); }
                    break;
                case 1:
                {
                    v = Vector2.zero; e.t49 -= dt; float p = 1f - Mathf.Clamp01(e.t49 / Mathf.Max(0.05f, T.eliteBlinkTellS49));
                    ZDisc(e.b49, 0.9f * p, E32(0x68386c, 0.45f), 0.7f, 0); ZRing(e.b49, 0.9f, E32(0xb55088, 0.9f), 0.7f, 1); tint = new Color(1f, 1f, 1f, 1f - 0.6f * p);
                    if (e.t49 <= 0) { HordeFX.CheapBurst(e.pos + Vector2.up * 0.8f, E32(0xb55088), 8); e.pos = e.b49; HordeFX.CheapBurst(e.pos + Vector2.up * 0.8f, E32(0xb55088), 8); e.s49 = 2; e.t49 = T.eliteSlashTellS49; e.b49 = _heroPos; e.a49 = ((_heroPos - e.pos).sqrMagnitude > 0.01f ? (_heroPos - e.pos).normalized : Vector2.right); }
                    break;
                }
                case 2:
                {
                    v = Vector2.zero; e.t49 -= dt; float p = 1f - Mathf.Clamp01(e.t49 / Mathf.Max(0.05f, T.eliteSlashTellS49)); float hl = (T.eliteSlashLenU49 * EArea49) * 0.5f;
                    Vector2 d1 = Rot49(e.a49, 45f), d2 = Rot49(e.a49, -45f);
                    ZSeg(e.b49 - d1 * hl, e.b49 + d1 * hl, (T.eliteSlashHalfWU49 * EArea49) * 2f, E32(0xb55088, 0.2f + 0.4f * p), 1); ZSeg(e.b49 - d2 * hl, e.b49 + d2 * hl, (T.eliteSlashHalfWU49 * EArea49) * 2f, E32(0xb55088, 0.2f + 0.4f * p), 1);
                    if (e.t49 <= 0)
                    {
                        float r = (T.eliteSlashHalfWU49 * EArea49) + 0.3f * HeroHitK47;
                        if (mode == Mode.Play && (SegDist49(_heroPos, e.b49 - d1 * hl, e.b49 + d1 * hl) < r || SegDist49(_heroPos, e.b49 - d2 * hl, e.b49 + d2 * hl) < r)) HurtHero(e.dmg * T.eliteSlashDmgMul49 * EDmgI49, "wraith_xslash");
                        HellordAudio.Play("frost_cleave", 0.7f); Ring49(e.pos, EBullets49(T.eliteRingN49), 0, T.eliteRingSpeed49, Random.value * 360f, e.dmg * 0.5f * EDmgI49, E32(0xb55088), "wraith_ring");
                        e.s49 = 3; e.t49 = T.eliteRingDelayS49; e.n49 = Mathf.RoundToInt(Random.value * 360f);
                    }
                    break;
                }
                case 3:
                    v = Vector2.zero; e.t49 -= dt;
                    if (e.t49 <= 0) { Ring49(e.pos, EBullets49(T.eliteRingN49), 0, T.eliteRingSpeed49, T.eliteRingRotDeg49 + Deg49(e.a49), e.dmg * 0.5f * EDmgI49, E32(0xb55088), "wraith_ring"); e.s49 = 0; e.cd49 = T.eliteCdS49; }
                    break;
            }
        }

        void ThinkFrostBrute49(Enemy e, ref Vector2 v, Vector2 dir, float dist, float sp, float dt, ref Color tint)
        {
            var T = T49; float spd = T.fbDashU49 / Mathf.Max(0.05f, T.fbDashS49);
            switch (e.s49)
            {
                case 0: if (e.cd49 <= 0 && dist < T.fbDashU49 + 2f) { e.s49 = 1; e.t49 = T.fbTellS49; e.a49 = LeadDir49(e.pos, spd); e.b49 = e.pos + e.a49 * Mathf.Max(0.5f, Mathf.Min(T.fbDashU49, RayToBox44(e.pos, e.a49) - 0.5f)); } break;
                case 1:
                {
                    v = Vector2.zero; e.t49 -= dt; float p = 1f - Mathf.Clamp01(e.t49 / Mathf.Max(0.05f, T.fbTellS49));
                    ZSeg(e.pos, e.b49, e.radius * 1.6f, E32(0x0099db, 0.15f + 0.35f * p), 1); ZRing(e.b49, (T.fbSlamR49 * EArea49), E32(0x2ce8f5, 0.3f + 0.4f * p), 0.7f, 2); tint = Color.Lerp(Color.white, new Color(0.6f, 0.95f, 1f), p);
                    if (e.t49 <= 0)
                    {
                        e.s49 = 2; e.t49 = (e.b49 - e.pos).magnitude / spd; HellordAudio.Play("oxhead_lunge_dash", 1f);
                        int n = Mathf.Max(1, Mathf.RoundToInt((e.b49 - e.pos).magnitude)); for (int k = 0; k <= n; k++) AddZone49(Vector2.Lerp(e.pos, e.b49, k / (float)n), 0.7f, 0);
                    }
                    break;
                }
                case 2: v = e.a49 * spd; e.t49 -= dt; if (e.t49 <= 0) { e.s49 = 3; e.t49 = 0.3f; } break;
                case 3:
                {
                    v = Vector2.zero; e.t49 -= dt; float p = 1f - Mathf.Clamp01(e.t49 / 0.3f);
                    ZDisc(e.pos, (T.fbSlamR49 * EArea49) * p, E32(0x2ce8f5, 0.3f), 0.7f, 0); ZRing(e.pos, (T.fbSlamR49 * EArea49), E32(0xffffff, 0.8f), 0.7f, 1);
                    if (e.t49 <= 0) { if (mode == Mode.Play && dist < (T.fbSlamR49 * EArea49) + 0.3f * HeroHitK47) { int h0 = heroHits; HurtHero(e.dmg * T.fbSlamDmgMul49 * EDmgI49, "frost_brute_slam"); if (heroHits > h0) AddChill44(1); } ScreenShake.Add(2, 0.15f); e.s49 = 4; e.t49 = 0.8f; }
                    break;
                }
                case 4: v = Vector2.zero; e.t49 -= dt; if (e.t49 <= 0) { e.s49 = 0; e.cd49 = T.fbCdS49; } break;
            }
        }

        void ThinkRange49(Enemy e, ref Vector2 v, Vector2 dir, float dist, float sp, float dt, ref Color tint)
        {
            var T = T49; int k = e.k49;
            if (e.s49 == 0)
            {
                if (dist < T.rangeMinU49) v = -dir * sp; else if (dist > T.rangeMaxU49) v = dir * sp; else v = new Vector2(-dir.y, dir.x) * sp * 0.4f * e.strafe;
                if (e.cd49 > 0 || dist > 13f) return;
                switch (k)
                {
                    case 1: e.s49 = 1; e.t49 = T.gunTellS49; break;
                    case 5: case 8: e.s49 = 1; e.t49 = k == 5 ? T.isLaserS49 : T.arLaserS49; e.a49 = dir; HellordAudio.Play("curse_rune_arm", 1.8f); break;
                    case 4: e.s49 = 1; e.t49 = T.fwrTellS49; break;
                    default:
                    {
                        float dmg = T.gnDmg49 * EDmg49;
                        if (k == 3) { Lob49(e.pos, _heroPos, (T.emberRadiusU49 * EArea49), dmg, 1); Lob49(e.pos, _heroPos + _heroVel37 * (T.gnFlightS49 + 0.35f) * T.leadAim49, (T.emberRadiusU49 * EArea49), dmg, 1, 0.35f); }
                        else Lob49(e.pos, _heroPos + _heroVel37 * T.gnFlightS49 * T.leadAim49, (T.gnRadiusU49 * EArea49), dmg, k == 2 ? 0 : 2);
                        HellordAudio.Play("bone_orbit_hit", 0.7f); e.cd49 = T.gnCdS49;
                        break;
                    }
                }
                return;
            }
            v = Vector2.zero; e.t49 -= dt;
            switch (k)
            {
                case 1:
                    if (e.s49 == 1)
                    {
                        float p = 1f - Mathf.Clamp01(e.t49 / Mathf.Max(0.05f, T.gunTellS49)); Vector2 m = e.pos + dir * 0.6f, side = new Vector2(-dir.y, dir.x) * 0.25f;
                        for (int i = -1; i <= 1; i++) ZDisc(m + side * i, 0.08f + 0.08f * p, E32(0xfee761, 0.5f + 0.5f * p), 1f, 6); tint = Color.Lerp(Color.white, new Color(1f, 0.7f, 0.4f), p);
                        if (e.t49 <= 0) { e.s49 = 2; e.n49 = EBullets49(T.gunBurst49); e.t49 = 0; }
                    }
                    else if (e.t49 <= 0)
                    {
                        Fire49(e.pos, LeadDir49(e.pos, T.gunSpeed49) * T.gunSpeed49, 0.18f, T.gunDmg49 * EDmg49, E32(0xf77622), "imp_gunner"); HellordAudio.Play("fireball_cast", 1.6f);
                        e.t49 = T.gunGapS49; if (!Complex49) e.n49 = Mathf.Min(e.n49, T.floor0GunBurst49d); if (--e.n49 <= 0) { e.s49 = 0; e.cd49 = T.gunCdS49; }
                    }
                    break;
                case 5: case 8:
                {
                    bool ice = k == 5; float L = ice ? T.isLaserS49 : T.arLaserS49, tr = ice ? T.isTrackS49 : T.arTrackS49, spd = ice ? T.isSpeed49 : T.arSpeed49;
                    if (L - e.t49 < tr) e.a49 = dir; float p = 1f - Mathf.Clamp01(e.t49 / Mathf.Max(0.05f, L)); bool locked = L - e.t49 >= tr;
                    Vector2 o = e.pos; int col = ice ? 0x2ce8f5 : 0xe43b44;
                    ZSeg(o, o + e.a49 * 18f, locked ? 0.12f : 0.05f, E32(locked ? 0xffffff : col, locked ? 0.55f + 0.4f * Mathf.Sin(Time.time * 40f) * 0.5f : 0.25f + 0.3f * p), 3);
                    if (e.t49 <= 0)
                    {
                        Fire49(o, e.a49 * spd, 0.2f, (ice ? T.isDmg49 : T.arDmg49) * EDmg49, E32(col), ice ? "ice_sniper" : "archer", ice ? T.isChill49 : 0);
                        HellordAudio.Play(ice ? "frost_cleave" : "fireball_cast", 2f); e.s49 = 0; e.cd49 = ice ? T.isCdS49 : T.arCdS49;
                    }
                    break;
                }
                case 4:
                    if (e.s49 == 1)
                    {
                        float p = 1f - Mathf.Clamp01(e.t49 / Mathf.Max(0.05f, T.fwrTellS49)); ZRing(e.pos, 0.4f + 1.2f * p, E32(0xf77622, 0.8f), 0.7f, 2); tint = Color.Lerp(Color.white, new Color(1f, 0.6f, 0.3f), p);
                        if (e.t49 <= 0) { e.n49 = Mathf.RoundToInt(Random.value * 360f); Ring49(e.pos, EBullets49(T.fwrRingN49), T.fwrGaps49, T.fwrSpeed49, e.n49, T.fwrDmg49 * EDmg49, E32(0xf77622), "flame_wraith"); HellordAudio.Play("fireball_cast", 0.8f); e.s49 = 2; e.t49 = T.fwrDelayS49; }
                    }
                    else if (e.t49 <= 0) { Ring49(e.pos, EBullets49(T.fwrRingN49), T.fwrGaps49, T.fwrSpeed49, e.n49 + T.fwrRotDeg49, T.fwrDmg49 * EDmg49, E32(0xfeae34), "flame_wraith"); e.s49 = 0; e.cd49 = T.fwrCdS49; }
                    break;
                default: e.s49 = 0; break;
            }
        }

        /// <summary>Frost Caster v0.0.49: a 7-bolt fan (x Intensity) at 9 u/s instead of one bolt.</summary>
        void FcFan49(Enemy e, Vector2 dir, Vector2 o)
        {
            var T = HellordTuning.I; int n = T.fcVolley679 > 0 ? T.fcVolley679 : EBullets49(T.fcFanN49); float spread = T.fcFanDeg49; // t679u: 3 = left edge, middle, right edge
            for (int i = 0; i < n; i++)
            {
                float a = n > 1 ? -spread * 0.5f + spread * i / (n - 1) : 0f; Vector2 d = Rot49(dir, a);
                _shots44.Add(new Shot44 { p = e.pos, v = d * T.fcBoltSpeed49, life = T.frostBoltLifeS, r = T.frostBoltRadiusU, dmg = T.frostBoltDmg * FloorDmg44 * EDmgI49, kind = 0, fx = FX44 ? MinionFX.FrostBolt(o, d) : -1 });
            }
        }

        // ---------------- range kits: recolours + crossfire spawns ----------------
        readonly Dictionary<int, Sprite[]> _f49 = new Dictionary<int, Sprite[]>(); float _lastRangedAng49, _lastRangedT49 = -99f;
        Sprite[] Frames49(int k)
        {
            if (_f49.TryGetValue(k, out var f)) return f; float g = T37.spriteRecolorGain;
            switch (k)
            {
                case 1: f = V37Recolor.Sprites(_eSprites[EType.Imp], V37Recolor.Fire, g, true); break;
                case 2: f = V37Recolor.Sprites(_eSprites[EType.Imp], V37Recolor.Frost, g * 1.2f, true); break;
                case 3: f = V37Recolor.Sprites(_eSprites[EType.Imp], V37Recolor.Fire, g * 1.5f, false); break;
                case 4: f = V37Recolor.Sprites(_eSprites[EType.Elite], V37Recolor.Fire, g * 1.2f, false); break;
                case 5: f = V37Recolor.Sprites(_eSprites[EType.Elite], V37Recolor.Frost, g * 1.35f, false); break;
                case 6: f = V37Recolor.Sprites(_eSprites[EType.Brute], V37Recolor.Frost, g * 1.35f, false); break;
                case 7: f = V37Recolor.Sprites(_eSprites[EType.Imp], V37Recolor.Bone, g, true); break;
                default: try { f = HArt.Sheet("enemy_archer"); } catch (System.Exception) { f = null; } if (f == null || f.Length < 24) f = V37Recolor.Sprites(_eSprites[EType.Imp], V37Recolor.Bone, g * 0.8f, false); break;
            }
            _f49[k] = f; return f;
        }
        void SetKind49(Enemy e, int k)
        {
            e.k49 = (byte)k; e.variant = 0; e.s49 = 0; e.frames = Frames49(k); e.sr.sprite = e.frames[0]; e.cd49 = 1.2f + Random.value * 1.2f;
            if (k == 4 || k == 5) { e.radius = 0.6f; e.hitY = 1.3f; }
            if (k != 6) e.speed *= 0.9f;
        }
        /// <summary>After ApplyVariant37 on a floor spawn: Frost Brute roll.</summary>
        void ApplyVariant49(Enemy e)
        {
            if (e == null || !V49 || !floorMode) return;
            if (e.type == EType.Brute && e.variant <= 1 && Complex49 && Theme == 1 && Random.value < T49.frostBruteChance49) SetKind49(e, 6);
        }
        int RangeKitAlive49() { int n = 0; for (int i = 0; i < maxEnemies; i++) { var e = _en[i]; if (e.alive && e.k49 >= 1 && e.k49 != 6) n++; } return n; }
        bool TryRangeKit49()
        {
            if (!V49 || !floorMode) return false;
            bool f0 = FloorIdx == 0;
            if (f0 && Floor0MeleeOnly49) return false;
            float ch = f0 ? T49.floor0RangeChance49d + T49.floor0RangeRamp49d * (FightsCleared49() - T49.floor0MeleeOnlyRooms49d) : T49.rangeKitChance49 + T49.rangeKitPerFloor49d * (FloorIdx - 1);
            if (Random.value >= ch) return false;
            int cap = f0 ? T49.floor0RangeMax49d : T49.rangeKitMax49 + Mathf.FloorToInt(T49.rangeKitMaxPerFloor49 * FloorIdx);
            if (RangeKitAlive49() >= cap) return false;
            int th = Theme, k;
            float r = Random.value;
            if (f0) k = r < 0.5f ? 8 : 1;
            else if (th == 0) k = T49.sackOnOx49 && r < 0.25f ? 7 : r < 0.6f ? 8 : 1;
            else if (th == 1) k = r < 0.55f ? 2 : 5;
            else if (th == 2) k = r < 0.35f ? 3 : r < 0.65f ? 4 : 1;
            else k = r < 0.6f ? 7 : 8;
            EType bt = k == 4 || k == 5 ? EType.Ghoul : EType.Imp;
            float ang = runTime - _lastRangedT49 < 8f ? _lastRangedAng49 + 180f + Random.Range(-T49.crossJitterDeg49, T49.crossJitterDeg49) : Random.value * 360f;
            _lastRangedAng49 = ang; _lastRangedT49 = runTime;
            Vector2 p = _heroPos + Rot49(Vector2.right, ang) * T49.crossDistU49; ClampToRoom(ref p, 10);
            var e = SpawnEnemyAt(bt, p); if (e == null) return false;
            SetKind49(e, k); HordeFX.CheapBurst(e.pos + Vector2.up * 0.6f, E32(0x181425), 8);
            return true;
        }
    }
}
