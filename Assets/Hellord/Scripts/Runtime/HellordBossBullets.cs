using PixelFXPack;
using UnityEngine;

// v0.0.17 (VH-approved direction "bigger AOE, shoots everywhere"): Ox-Head bullet-hell layer.
// Patterns run between and over the melee cycle so there is always a threat on screen:
//   radial burst (ring with fixed open gaps), rotating 2-arm spiral (phase 2+), aimed 5-way fan (gaps between bullets),
//   and a ring burst out of every stomp. Every pattern has a 0.45 s read-in (boss crouch + dust puff at the muzzle) before
//   the first bullet, bullets are slow (4-5.5 u/s) and spaced wider than the hero (~1.2 u), so a dodge gap always exists.
// Art: reuses the proj_curse strip tinted blood-red (placeholder) - see design doc "FX needs for MoneyMaker".
namespace Hellord
{
    public partial class HellordGame
    {
        class BBullet { public bool alive, st53; public Vector2 pos, vel; public float life, animT; public int h = -1; } // v0.0.21: drawn by BossBulletFX #112 (h = handle)
        const int BBulletMax = 220; const float BBulletR = 0.32f; static float BBulletDmg => HellordTuning.I.bossBulletDamage; static float BBulletTell => HellordTuning.I.bossPatternLead;
        BBullet[] _bb;
        // v0.0.26: harder phase 1 - everything below reads HellordTuning (old values in the tooltips)
        float BSpeedMul => HellordTuning.I.bossBulletSpeedMul * (1f + 0.05f * FloorIdx);
        int BFan => HellordTuning.I.bossP1FanCount + (_raging ? 2 : 0);
        int BArms => _phase2 || _raging ? HellordTuning.I.bossP1SpiralArms + 1 : HellordTuning.I.bossP1SpiralArms;
        float BSpiralInt => _raging ? HellordTuning.I.bossP1SpiralInterval * 0.75f : _phase2 ? HellordTuning.I.bossP1SpiralInterval * 0.85f : HellordTuning.I.bossP1SpiralInterval;
        float BPatternCd => (_raging ? HellordTuning.I.bossRagePatternCooldown : _phase2 ? HellordTuning.I.bossP2PatternCooldown : HellordTuning.I.bossP1PatternCooldown) / (1f + HellordTuning.I.floorBossPatternRate * FloorIdx) / BossCdDiv49;
        float _bbRot; float _bbCd = 2.5f, _bbTellT = -1, _spiralA, _spiralCd; int _bbPat, _bbPending = -1;
        public int bossBulletsFired, bossBulletsLive, bossBulletHits;

        void EnsureBBullets()
        {
            if (_bb != null) return;
            _bb = new BBullet[BBulletMax]; for (int i = 0; i < BBulletMax; i++) _bb[i] = new BBullet();
            // v0.0.21: Pixel FX Pack 1.8.7 BossBulletFX #112 + PatternWarnFX #113, on the game's own sprite material, above actors (wash at 2000)
            var tmp = NewSR("matprobe", null, null, 0); var mat = new Material(tmp.sharedMaterial); Destroy(tmp.gameObject);
            BossBulletFX.MaterialOverride = mat; BossBulletFX.SortingOrder = 2001; BossBulletFX.Prewarm(256);
            PatternWarnFX.MaterialOverride = new Material(mat); PatternWarnFX.SortingOrder = HellordTuning.I.bossWarnOrder948; /* t948u was 2002 (over the boss) */ PatternWarnFX.Prewarm(16);
        }
        void FireBB(Vector2 p, Vector2 v)
        {
            EnsureBBullets();
            foreach (var b in _bb) if (!b.alive)
            {
                v *= BSpeedMul; bool sh950 = Bs950Bullets; b.h = sh950 ? -1 : BossBulletFX.Spawn(p, v, BossBulletSize.Small, 9.5f, true); if (b.h < 0 && !sh950) return;
                b.alive = true; b.st53 = false; b.pos = p; b.vel = v; b.life = 9f; b.animT = 0; bossBulletsFired++; if (sh950) Bs950Orb(b, bossBulletsFired & 63); return;
            }
        }
        void KillBB(BBullet b, bool pop) { if (_bsOrbs950.ContainsKey(b)) { if (pop) HitShards934(b.pos, false); Bs950Release(b); } b.alive = false; if (b.h >= 0) BossBulletFX.Despawn(b.h, pop); b.h = -1; }
        void ClearBBullets() { Clear46(); if (_bb == null) return; foreach (var b in _bb) if (b.alive) KillBB(b, false); PatternWarnFX.Clear(); Clear984(); _bbTellT = -1; _bbPending = -1; _stompWarned = false; }
        float _spiralHold; bool _spiralWas, _stompWarned;
        // Every pattern gets a PatternWarnFX telegraph (lead = BBulletTell) with fire = false: the game fires its own bullets when the lead ends.
        void WarnPattern(int pat, float rot)
        {
            EnsureBBullets(); float lead = BBulletTell;
            if (PWarn984On) // t974u: PixelBossWarn instead of the PatternWarnFX polygon rings / fan lines
            {
                Vector2 d = _heroPos + Vector2.up * 0.6f - BBMuzzle;
                if (pat == 0) PWarn984(0, BBMuzzle, lead, 24f);
                else if (pat == 1) PWarn984(1, BBMuzzle, lead, 64f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, (_raging ? 11f : 12f) * (BFan - 1));
                else { PWarn984(0, BBMuzzle, lead, 18f); PWarn984(0, BBMuzzle, lead, 30f); }
                return;
            }
            switch (pat)
            {
                case 0: PatternWarnFX.WarnRadialBurst(BBMuzzle, _raging ? 32 : 26, 4.8f * 16, rot, lead, false); break;
                case 1: PatternWarnFX.WarnFan(BBMuzzle, _heroPos + Vector2.up * 0.6f, BFan, (_raging ? 11f : 12f) * (BFan - 1), (_raging ? 7.2f : 6.8f) * 16, BossBulletSize.Small, lead, false); break;
                case 2: PatternWarnFX.WarnRing(BBMuzzle, 18, 3.6f * 16, rot, BossBulletSize.Small, lead, false); PatternWarnFX.WarnRing(BBMuzzle, 18, 5f * 16, rot + 10f, BossBulletSize.Small, lead, false); break;
            }
        }
        Vector2 BBMuzzle => _boss.pos + Vector2.up * 1.6f;

        /// <summary>Radial ring of n bullets; 'gaps' consecutive slots removed at 'gapCount' places (rotating) so a lane is always open.</summary>
        void BBRadial(Vector2 c, int n, float spd, float rot, int gapCount, int gapW)
        {
            for (int i = 0; i < n; i++)
            {
                bool gap = false; for (int g = 0; g < gapCount; g++) { int s = (g * n / gapCount + (int)(rot / 360f * n)) % n; int d = (i - s + n) % n; if (d < gapW) gap = true; }
                if (gap) continue; float a = (rot + i * 360f / n) * Mathf.Deg2Rad; FireBB(c, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * spd);
            }
        }
        void BBFan(int n, float spread, float spd)
        {
            Vector2 d = (_heroPos + Vector2.up * 0.6f - BBMuzzle); float a0 = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            for (int i = 0; i < n; i++) { float a = (a0 + (i - (n - 1) / 2f) * spread) * Mathf.Deg2Rad; FireBB(BBMuzzle, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * spd); }
        }

        /// <summary>Called from UpdateBoss every frame after the intro.</summary>
        void UpdateBossPatterns(float dt)
        {
            if (HellordTuning.I.bossPatternsV46) { UpdateBossPatterns46(dt); return; }
            if (_bs == BS.Stun) { _bbTellT = -1; _bbPending = -1; return; } // stunned = punish window, no new bullets
            // read-in: crouch + muzzle dust, then fire
            if (_bbTellT >= 0)
            {
                _bbTellT -= dt; _squash = Mathf.MoveTowards(_squash, -0.1f, dt);
                if (_bbTellT < 0)
                {
                    float rot = _bbRot;
                    switch (_bbPending)
                    {
                        case 0: BBRadial(BBMuzzle, _raging ? 32 : 26, 4.8f, rot, 2, 3); break;
                        case 1: BBFan(BFan, _raging ? 11f : 12f, _raging ? 7.2f : 6.8f); break;
                        case 2: BBRadial(BBMuzzle, 18, 3.6f, rot, 2, 3); BBRadial(BBMuzzle, 18, 5.0f, rot + 10f, 2, 3); break; // double ring
                    }
                    _bbPending = -1; ScreenShake.Add(0.8f, 0.12f);
                }
            }
            else if (_bs == BS.Walk || _bs == BS.Recover || _bs == BS.Sweep)
            {
                _bbCd -= dt;
                if (_bbCd <= 0)
                {
                    _bbPending = (_phase2 || HellordTuning.I.bossP1DoubleRing) ? _bbPat++ % 3 : _bbPat++ % 2; _bbTellT = BBulletTell; _bbRot = Random.Range(0f, 360f); WarnPattern(_bbPending, _bbRot);
                    TellFX.DustPuff(BBMuzzle, 6, 12); _bbCd = BPatternCd;
                }
            }
            // rotating spiral while walking/recovering: 2 arms, 3 arms from phase 2 (continuous pressure, wide angular gaps)
            bool spiralOn = _bs == BS.Walk || _bs == BS.Recover;
            if (spiralOn && !_spiralWas) { EnsureBBullets(); _spiralHold = BBulletTell; int arms0 = BArms; if (!PWarn984(0, BBMuzzle, BBulletTell, 20f)) PatternWarnFX.WarnSpiral(BBMuzzle, arms0, _spiralA + BBulletTell * (_raging ? 70f : 50f), _raging ? 70f : 50f, 1f, BSpiralInt, (_raging ? 4.4f : 3.8f) * 16, BossBulletSize.Small, BBulletTell, false); }
            _spiralWas = spiralOn;
            if (spiralOn && _spiralHold > 0) { _spiralHold -= dt; _spiralA += dt * (_raging ? 70f : 50f); } // v0.0.21: spiral (re)start waits for its warning
            else if (spiralOn)
            {
                _spiralCd -= dt; _spiralA += dt * (_raging ? 70f : 50f);
                if (_spiralCd <= 0) { int arms = BArms; _spiralCd = BSpiralInt; for (int k = 0; k < arms; k++) { float a = (_spiralA + k * 360f / arms) * Mathf.Deg2Rad; FireBB(BBMuzzle, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (_raging ? 4.4f : 3.8f)); } }
            }
        }
        float _stompRot;
        void BBStompWarn() { EnsureBBullets(); _stompRot = Random.Range(0f, 360f); _stompWarned = true; if (!PWarn984(0, _stompC + Vector2.up * 0.3f, BBulletTell, 24f)) PatternWarnFX.WarnRadialBurst(_stompC + Vector2.up * 0.3f, _raging ? 24 : 18, 4.0f * 16, _stompRot, BBulletTell, false); }
        void BBStompBurst() { if (!_stompWarned) _stompRot = Random.Range(0f, 360f); _stompWarned = false; BBRadial(_stompC + Vector2.up * 0.3f, _raging ? 24 : 18, 4.0f, _stompRot, 2, 3); }

        void UpdateBBullets(float dt)
        {
            int l46 = UpdateP46(dt);
            if (_bb == null) { bossBulletsLive = l46; return; }
            if (!BossAlive) { if (bossBulletsLive > 0) { ClearBBullets(); bossBulletsLive = 0; } return; }
            int live = 0; Rect room = _room != null ? _room.WorldRect : new Rect(-1e4f, -1e4f, 2e4f, 2e4f);
            Vector2 hc = _heroPos + Vector2.up * 0.6f;
            foreach (var b in _bb)
            {
                if (!b.alive) continue; b.life -= dt; b.animT += dt;
                if (b.h >= 0 && BossBulletFX.IsAlive(b.h)) b.pos = BossBulletFX.GetPosition(b.h); else b.pos += b.vel * dt; // the FX moves it; hits use its position
                if (b.life <= 0 || !room.Contains(b.pos)) { KillBB(b, false); continue; }
                if ((b.pos - hc).sqrMagnitude < (BBulletR + 0.35f * HeroHitK47) * (BBulletR + 0.35f * HeroHitK47) && mode == Mode.Play)
                {
                    int h0 = heroHits; HurtHero(BBulletDmg, "boss_bullet", true); if (heroHits > h0) bossBulletHits++;
                    KillBB(b, true); continue;
                }
                live++;
            }
            bossBulletsLive = live + l46;
        }
    }
}
