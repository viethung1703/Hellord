using System.Collections.Generic;
using PixelFXPack;
using UnityEngine;

// v0.0.53 torture breakables (VH pt442u): the player can shoot torture decor, the devices (with their baked-in
// tortured humans) and the watching demons / rope Monk. Every hit flashes + wobbles + chips; the kill plays a
// material-specific break burst (debris / grain / blood / demon ember / ice) and leaves a small remnant decal.
namespace Hellord
{
    public partial class HellordGame
    {
        // fx kinds
        const int Brk53Wood = 0, Brk53Grain = 1, Brk53Flesh = 2, Brk53Demon = 3, Brk53Ice = 4, Brk53Fire = 5;
        class Brk53
        {
            public List<SpriteRenderer> parts = new List<SpriteRenderer>(3); public SpriteRenderer main;
            public Rect hit; public Rect col, col2; public bool hasCol; public float hp, maxHp, hitT, deadT; public int kind; public bool dead, big;
            public Vector3 baseScale, basePos; public float[] aoeCd = new float[24];
        }
        readonly List<Brk53> _brk53 = new List<Brk53>(16); readonly List<SpriteRenderer> _brkDecal53 = new List<SpriteRenderer>(8);
        static Sprite _brkDecalS53;
        static PixelBurstSettings _bDebris53, _bGrain53, _bBlood53, _bEmber53, _bIce53, _bChip53;

        /// <summary>Register a breakable. hit = world rect (x,y = bottom-left). col = the _blocks42 footprint to drop on break.</summary>
        Brk53 AddBrk53(SpriteRenderer main, Rect hit, float hp, int kind, Rect col, bool big = false, params SpriteRenderer[] extra)
        {
            var T = HellordTuning.I; if (!T.breakables53 || !main || hp <= 0) return null;
            var b = new Brk53 { main = main, hit = hit, hp = hp, maxHp = hp, kind = kind, col = col, hasCol = col.width > 0, big = big, baseScale = main.transform.localScale, basePos = main.transform.localPosition };
            b.parts.Add(main); foreach (var e in extra) if (e) b.parts.Add(e);
            HordeFX.Prepare(main); _brk53.Add(b); return b;
        }
        static Rect BrkRect53(Vector2 feet, float wU, float hU, float down = 0f) => new Rect(feet.x - wU / 2f, feet.y - down, wU, hU);

        /// <summary>Damage the first breakable in range (extra = blast radius). True = something was hit.</summary>
        bool BrkHit53(Vector2 p, float extra, float dmg, bool all)
        {
            if (_brk53.Count == 0) return false; var T = HellordTuning.I; bool any = false;
            for (int i = _brk53.Count - 1; i >= 0; i--)
            {
                var b = _brk53[i]; if (b.dead || !b.main) continue;
                var r = b.hit; if (extra > 0) r = new Rect(r.x - extra, r.y - extra, r.width + 2 * extra, r.height + 2 * extra);
                if (!r.Contains(p)) continue;
                float d = Mathf.Max(1f, dmg * T.brkDmgMul53); b.hp -= d; b.hitT = T.brkWobbleS53; any = true;
                HordeFX.Hit(b.main, Mathf.RoundToInt(d), false, T.brkNumbers53);
                ChipFX53(new Vector2(Mathf.Clamp(p.x, b.hit.xMin, b.hit.xMax), Mathf.Clamp(p.y, b.hit.yMin, b.hit.yMax)), b.kind);
                if (b.hp <= 0) Break53(b);
                if (!all) break;
            }
            return any;
        }

        /// <summary>v0.0.53 pt45x: every AoE / melee / beam / orbit / pool / chain hit also damages breakables, souls and ice
        /// in its area. key = skill id (or 20+ for blasts); each breakable has a per-key cooldown so ticking areas don't shred instantly.</summary>
        bool SkillArea53(Vector2 c, float r, float dmg, int key)
        {
            var T = HellordTuning.I; if (!T.brkAoe53) return false; bool any = false; key = Mathf.Clamp(key, 0, 23);
            float cd = Mathf.Max(0f, T.brkAoeCd53), now = Time.time;
            for (int i = _brk53.Count - 1; i >= 0; i--)
            {
                var b = _brk53[i]; if (b.dead || !b.main) continue;
                var h = b.hit; float dx = Mathf.Max(h.xMin - c.x, 0, c.x - h.xMax), dy = Mathf.Max(h.yMin - c.y, 0, c.y - h.yMax);
                if (dx * dx + dy * dy > r * r) continue;
                if (b.aoeCd[key] > now) continue; b.aoeCd[key] = now + cd;
                float d = Mathf.Max(1f, dmg * T.brkDmgMul53 * T.brkAoeDmgMul53); b.hp -= d; b.hitT = T.brkWobbleS53; any = true;
                HordeFX.Hit(b.main, Mathf.RoundToInt(d), false, T.brkNumbers53);
                ChipFX53(new Vector2(Mathf.Clamp(c.x, h.xMin, h.xMax), Mathf.Clamp(c.y, h.yMin, h.yMax)), b.kind);
                if (b.hp <= 0) Break53(b);
            }
            if (_souls53.Count > 0 && (_soulAoeT53 <= now)) { if (SoulHit53(c, r, dmg)) { any = true; _soulAoeT53 = now + cd * 0.5f; } }
            if (_iceB52.Count > 0 && HellordTuning.I.iceAnyProj52) IceHit52(c, r);
            return any;
        }
        float _soulAoeT53;
        /// <summary>v0.0.53: a live breakable / soul / ice block within range counts as a target, so skills fire at props when no demon is near.</summary>
        bool BrkNear53(Vector2 p, float range)
        {
            if (!HellordTuning.I.brkAoe53) return false; float r2 = range * range;
            foreach (var b in _brk53) if (!b.dead && b.main && (b.hit.center - p).sqrMagnitude < r2) return true;
            foreach (var s in _souls53) if (!s.dead && s.sr && (s.c - p).sqrMagnitude < r2) return true;
            return false;
        }
        /// <summary>Nearest live breakable centre within range (chain lightning target when no demon is left).</summary>
        int NearBrk53(Vector2 p, float range, HashSet<int> skip)
        {
            int best = -1; float bd = range * range;
            for (int i = 0; i < _brk53.Count; i++) { var b = _brk53[i]; if (b.dead || !b.main || skip.Contains(i)) continue; float q = (b.hit.center - p).sqrMagnitude; if (q < bd) { bd = q; best = i; } }
            return best;
        }
        readonly HashSet<int> _chainBrk53 = new HashSet<int>();
        /// <summary>Chain Lightning: keep jumping into breakables after (or instead of) demons.</summary>
        void ChainBrk53(Vector2 from, float range, int jumps, float dmg, List<Vector3> pts)
        {
            _chainBrk53.Clear();
            for (int k = 0; k < jumps; k++)
            {
                int i = NearBrk53(from, range, _chainBrk53); if (i < 0) break; _chainBrk53.Add(i); var b = _brk53[i];
                SkillArea53(b.hit.center, 0.05f, dmg, (int)SkillId.ChainLightning); if (pts != null && pts.Count < 7) pts.Add(b.hit.center); from = b.hit.center;
            }
        }

        /// <summary>pt483u: decor drops loot like a demon kill (xp souls + a bone chance). No AI, it never attacks.</summary>
        void BrkDrop53(Vector2 at, bool big, float mul = 1f)
        {
            var T = HellordTuning.I; if (!T.brkDrops53) return;
            int xp = Mathf.Max(0, Mathf.RoundToInt((big ? T.brkDropXpBig53 : T.brkDropXp53) * mul)); int n = big ? 4 : 1;
            for (int i = 0; i < n && xp > 0; i++) DropPickup(at + (n > 1 ? Random.insideUnitCircle * 0.8f : Vector2.zero), false, Mathf.Max(1, xp / n));
            int bones = big ? T.brkDropBonesBig53 : (Random.value < T.brkDropBoneChance53 * mul ? 1 : 0);
            for (int i = 0; i < bones; i++) DropPickup(at + Random.insideUnitCircle * (big ? 1f : 0.4f), true, 1);
        }
        /// <summary>pt483u: put an already-broken prop back as rubble on room re-entry (no FX, no drops, no sound).</summary>
        /// <summary>pt484u: remove a decor footprint from the feet collision (hero + demons share _blocks42). Tolerant match, not exact float ==.</summary>
        void DropCol53(Rect c)
        {
            if (c.width <= 0 || c.height <= 0) return;
            _blocks42.RemoveAll(k => Mathf.Abs(k.col.x - c.x) < 0.01f && Mathf.Abs(k.col.y - c.y) < 0.01f && Mathf.Abs(k.col.width - c.width) < 0.01f && Mathf.Abs(k.col.height - c.height) < 0.01f);
        }
        void BreakSilent53(Brk53 b)
        {
            b.dead = true; b.hp = 0; b.deadT = 999f;
            DropCol53(b.col); DropCol53(b.col2); b.hasCol = false; // pt484u: rubble never blocks
            foreach (var s in b.parts) if (s) s.gameObject.SetActive(false);
            if (HellordTuning.I.brkDecal53) Decal53(new Vector2(b.hit.center.x, b.hit.yMin + 0.1f), b.kind, Mathf.Clamp(b.hit.width * HArt.PPU * 0.9f, 10f, 220f));
        }

        // ---- pt483u: per-room decor state for the run (FloorRoom objects are rebuilt per floor / run, so it resets with them) ----
        FloorRoom _brkRoom53;
        void SaveBrk53()
        {
            var r = _brkRoom53; _brkRoom53 = null; if (r == null || !HellordTuning.I.decorPersist53) return;
            r.brkHp53 = new float[_brk53.Count]; for (int i = 0; i < _brk53.Count; i++) r.brkHp53[i] = _brk53[i].dead ? 0f : _brk53[i].hp;
            r.iceBroken53 = new bool[_iceB52.Count]; for (int i = 0; i < _iceB52.Count; i++) r.iceBroken53[i] = _iceB52[i].broken;
            r.soulDead53 = new bool[_souls53.Count]; for (int i = 0; i < _souls53.Count; i++) r.soulDead53[i] = _souls53[i].dead;
        }
        void RestoreBrk53(FloorRoom r)
        {
            _brkRoom53 = r; if (r == null || !HellordTuning.I.decorPersist53) return;
            if (r.brkHp53 != null && r.brkHp53.Length == _brk53.Count) for (int i = 0; i < _brk53.Count; i++) { if (r.brkHp53[i] <= 0) BreakSilent53(_brk53[i]); else _brk53[i].hp = Mathf.Min(_brk53[i].hp, r.brkHp53[i]); }
            if (r.iceBroken53 != null && r.iceBroken53.Length == _iceB52.Count) for (int i = 0; i < _iceB52.Count; i++) if (r.iceBroken53[i]) { var b = _iceB52[i]; b.broken = true; b.t = 999f; b.regrowT = 0; DropCol53(b.col); }
            if (r.soulDead53 != null && r.soulDead53.Length == _souls53.Count) for (int i = 0; i < _souls53.Count; i++) if (r.soulDead53[i]) { var s = _souls53[i]; s.dead = true; s.deadT = 999f; if (s.sr) s.sr.sprite = _soulF53[5]; if (s.bar) s.bar.enabled = false; if (s.barBg) s.barBg.enabled = false; }
        }

        void Break53(Brk53 b)
        {
            var T = HellordTuning.I; b.dead = true; b.deadT = 0; BrkDrop53(new Vector2(b.hit.center.x, b.hit.yMin + 0.3f), b.big, b.kind == Brk53Demon ? 1.5f : 1f);
            var c = b.hit.center; BreakFX53(c, b.kind, b.big ? T.brkBigMul53 : 1f, b.hit.width);
            if (b.big && b.kind != Brk53Demon) BreakFX53(c + new Vector2(b.hit.width * 0.25f, 0), Brk53Flesh, 0.7f, b.hit.width * 0.5f); // humans baked in the device
            DropCol53(b.col); DropCol53(b.col2); b.hasCol = false; // pt484u: rubble never blocks
            if (b.big) BigBreak53(b); // v0.0.53 B15: set pieces get a real collapse (gore across the width, rubble, hit-stop)
            if (T.brkDecal53) Decal53(new Vector2(c.x, b.hit.yMin + 0.1f), b.kind, Mathf.Clamp(b.hit.width * HArt.PPU * 0.9f, 10f, 220f));
            HellordAudio.Play(b.kind == Brk53Demon || b.kind == Brk53Flesh ? "bone_orbit_hit" : "fireball_impact"); HellordAudio.Play("fireball_impact");
        }

        static Sprite _brkChunkS53;
        /// <summary>v0.0.53 B15: extra punch for set pieces: blood bursts across the width (baked-in humans), a gore + rubble remnant, hit-stop and shake.</summary>
        void BigBreak53(Brk53 b)
        {
            var T = HellordTuning.I; var c = b.hit.center; float w = b.hit.width;
            int n = Mathf.Clamp(T.brkBigGoreBursts53, 0, 8);
            for (int i = 0; i < n; i++)
            {
                var p = new Vector2(b.hit.xMin + w * (i + 0.5f) / Mathf.Max(1, n), b.hit.yMin + b.hit.height * Random.Range(0.25f, 0.7f));
                if (b.kind != Brk53Demon) BreakFX53(p, Brk53Flesh, T.brkBigGoreMul53, w / Mathf.Max(1, n));
                if (b.kind != Brk53Demon && T.brkDecal53) Decal53(new Vector2(p.x + Random.Range(-0.4f, 0.4f), b.hit.yMin + Random.Range(-0.2f, 0.3f)), Brk53Flesh, Mathf.Clamp(w * HArt.PPU * 0.35f, 10f, 90f));
            }
            if (T.brkRubble53 > 0)
            {
                if (_brkChunkS53 == null) { var tx = new Texture2D(1, 1) { filterMode = FilterMode.Point }; tx.SetPixel(0, 0, Color.white); tx.Apply(); _brkChunkS53 = Sprite.Create(tx, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), HArt.PPU); }
                for (int i = 0; i < T.brkRubble53; i++)
                {
                    var p = new Vector2(c.x + Random.Range(-w * 0.45f, w * 0.45f), b.hit.yMin + Random.Range(-0.25f, 0.45f));
                    var d = SR52("brk53_rubble", _brkChunkS53, p, HellordRoom.BaseOrder + 90);
                    float g = Random.Range(0.6f, 1f); bool red = b.kind != Brk53Demon && Random.value < 0.35f;
                    d.color = red ? new Color(0.5f * g, 0.04f, 0.06f, 1f) : b.kind == Brk53Fire ? new Color(0.18f * g, 0.12f * g, 0.1f * g, 1f) : new Color(0.42f * g, 0.3f * g, 0.2f * g, 1f);
                    d.transform.localScale = new Vector3(Random.Range(2, 6), Random.Range(1, 4), 1); d.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-30f, 30f)); _brkDecal53.Add(d);
                }
            }
            if (T.brkBigHitStop53 > 0 && !HitStop.Active) HitStop.Play(T.brkBigHitStop53);
            if (T.brkBigShake53 > 0) ScreenShake.Add(T.brkBigShake53, 0.3f);
        }

        /// <summary>Small per-hit chips in the material colour.</summary>
        void ChipFX53(Vector2 p, int kind)
        {
            if (_bChip53 == null) _bChip53 = new PixelBurstSettings { count = 5, speed = new Vector2(1f, 3f), directionDegrees = 90, spreadDegrees = 140, lifetime = new Vector2(0.2f, 0.4f), gravity = 8f, drag = 1f, sizePixels = new Vector2Int(1, 2), colorSteps = 2 };
            _bChip53.colorOverLife = Grad53(kind); _bChip53.count = Mathf.Max(1, HellordTuning.I.brkChip53); PixelParticles.Burst(_bChip53, p);
        }
        static Gradient Grad53(int kind)
        {
            switch (kind)
            {
                case Brk53Grain: return PixelBurstSettings.DefaultGradient(new Color(1f, 0.97f, 0.85f), new Color(0.62f, 0.48f, 0.3f));
                case Brk53Flesh: return HellPalette.BloodGradient();
                case Brk53Demon: return HellPalette.BloodGradient();
                case Brk53Ice: return PixelBurstSettings.DefaultGradient(Color.white, new Color(0.45f, 0.85f, 1f));
                case Brk53Fire: return HellPalette.FireGradient();
                default: return PixelBurstSettings.DefaultGradient(new Color(0.72f, 0.6f, 0.48f), new Color(0.25f, 0.18f, 0.14f));
            }
        }

        /// <summary>The break burst. widthU = object width (world units) to spread big objects.</summary>
        void BreakFX53(Vector2 c, int kind, float mul, float widthU)
        {
            var T = HellordTuning.I; mul *= T.brkFxMul53; if (mul <= 0) return;
            int N(float n) => Mathf.Max(1, Mathf.RoundToInt(n * mul));
            float spread = Mathf.Clamp(widthU * 0.35f, 0f, 6f);
            if (_bDebris53 == null) _bDebris53 = new PixelBurstSettings { speed = new Vector2(2f, 6f), directionDegrees = 90, spreadDegrees = 160, lifetime = new Vector2(0.35f, 0.8f), gravity = 12f, drag = 1.2f, sizePixels = new Vector2Int(1, 3), colorOverLife = Grad53(Brk53Wood), colorSteps = 3 };
            if (_bGrain53 == null) _bGrain53 = new PixelBurstSettings { speed = new Vector2(1.5f, 4.5f), directionDegrees = 90, spreadDegrees = 170, lifetime = new Vector2(0.4f, 0.9f), gravity = 9f, drag = 1.5f, sizePixels = new Vector2Int(1, 1), colorOverLife = Grad53(Brk53Grain), colorSteps = 2 };
            if (_bBlood53 == null) _bBlood53 = new PixelBurstSettings { speed = new Vector2(2f, 5.5f), directionDegrees = 90, spreadDegrees = 150, lifetime = new Vector2(0.3f, 0.7f), gravity = 11f, drag = 1f, sizePixels = new Vector2Int(1, 2), colorOverLife = HellPalette.BloodGradient(), colorSteps = 3 };
            if (_bEmber53 == null) _bEmber53 = new PixelBurstSettings { speed = new Vector2(0.8f, 3f), directionDegrees = 90, spreadDegrees = 80, lifetime = new Vector2(0.5f, 1.1f), gravity = 1.5f, drag = 1.5f, sizePixels = new Vector2Int(1, 1), colorOverLife = HellPalette.FireGradient(), colorSteps = 4, additive = true, sway = 3f };
            if (_bIce53 == null) _bIce53 = new PixelBurstSettings { speed = new Vector2(2f, 5f), spreadDegrees = 360, lifetime = new Vector2(0.3f, 0.6f), gravity = 7f, drag = 1.5f, sizePixels = new Vector2Int(1, 2), colorOverLife = Grad53(Brk53Ice), colorSteps = 3 };
            int puffs = spread > 1.5f ? 3 : 1;
            for (int i = 0; i < puffs; i++)
            {
                var p = c + new Vector2(puffs == 1 ? 0 : (i - 1) * spread, 0);
                switch (kind)
                {
                    case Brk53Grain: _bGrain53.count = N(T.brkGrainN53); PixelParticles.Burst(_bGrain53, p); _bDebris53.count = N(T.brkDebrisN53 * 0.4f); PixelParticles.Burst(_bDebris53, p); break;
                    case Brk53Flesh: _bBlood53.count = N(T.brkBloodN53); PixelParticles.Burst(_bBlood53, p); _bDebris53.count = N(T.brkDebrisN53 * 0.5f); PixelParticles.Burst(_bDebris53, p); break;
                    case Brk53Demon: _bBlood53.count = N(T.brkBloodN53 * 0.8f); PixelParticles.Burst(_bBlood53, p); _bEmber53.count = N(T.brkEmberN53); PixelParticles.Burst(_bEmber53, p); break;
                    case Brk53Ice: _bIce53.count = N(T.brkDebrisN53); PixelParticles.Burst(_bIce53, p); break;
                    case Brk53Fire: _bEmber53.count = N(T.brkEmberN53); PixelParticles.Burst(_bEmber53, p); _bDebris53.count = N(T.brkDebrisN53 * 0.7f); PixelParticles.Burst(_bDebris53, p); break;
                    default: _bDebris53.count = N(T.brkDebrisN53); PixelParticles.Burst(_bDebris53, p); break;
                }
            }
            switch (kind)
            {
                case Brk53Flesh: ProjectileFX.Impact(c, ProjElement.Blood, Vector2.zero); HordeFX.CheapBurst(c, new Color(0.75f, 0.05f, 0.08f), N(10)); break;
                case Brk53Demon: ProjectileFX.Impact(c, ProjElement.Curse, Vector2.zero); PixelFXSpawn.LightPop(c, new Color(1f, 0.45f, 0.1f), 22); break;
                case Brk53Ice: HFX.Shatter(c); break;
                case Brk53Fire: ProjectileFX.Impact(c, ProjElement.Fire, Vector2.zero); PixelFXSpawn.LightPop(c, new Color(1f, 0.5f, 0.1f), 26); break;
                default: ProjectileFX.Impact(c, ProjElement.Bone, Vector2.zero); break;
            }
            PixelFXSpawn.Shockwave(c, Mathf.Clamp(widthU * HArt.PPU * 0.5f, 12f, 48f), 0.3f);
            if (T.brkShake53 > 0) ScreenShake.Add(T.brkShake53 * Mathf.Clamp(mul, 0.5f, 2f), 0.18f);
        }

        void Decal53(Vector2 p, int kind, float wPx)
        {
            if (_brkDecalS53 == null) _brkDecalS53 = Soft52(ref _brkDecalS53, 32, 16, Color.white, 1.4f);
            var d = SR52("brk53_decal", _brkDecalS53, p, HellordRoom.BaseOrder + 89);
            d.color = kind == Brk53Flesh || kind == Brk53Demon ? new Color(0.45f, 0.03f, 0.05f, 0.7f) : kind == Brk53Grain ? new Color(0.9f, 0.85f, 0.7f, 0.55f) : kind == Brk53Ice ? new Color(0.6f, 0.9f, 1f, 0.45f) : new Color(0.12f, 0.08f, 0.06f, 0.6f);
            d.transform.localScale = new Vector3(wPx / 32f, Mathf.Max(0.6f, wPx / 64f), 1); _brkDecal53.Add(d);
        }

        void UpdateBrk53(float dt)
        {
            if (_brk53.Count == 0) return; var T = HellordTuning.I;
            foreach (var b in _brk53)
            {
                if (!b.main) continue;
                if (b.dead)
                {
                    // squash + pop: 0.12 s flat squash, then gone
                    // pt483u: NO scale change on pixel art (the old 1.25x / 0.2x squash stretched 400 px set pieces = "pixels break / zooms out").
                    // Now: sink by whole pixels + darken + fade, scale stays at baseScale.
                    b.deadT += dt; float k = Mathf.Clamp01(b.deadT / Mathf.Max(0.01f, b.big ? T.brkBigCollapseS53 : T.brkPopS53));
                    b.main.transform.localScale = b.baseScale;
                    b.main.transform.localPosition = b.basePos + new Vector3(0, -Mathf.Round(k * (b.big ? 6f : 3f)) / HArt.PPU, 0);
                    b.main.color = new Color(1f - 0.6f * k, 1f - 0.75f * k, 1f - 0.75f * k, 1f - k * k);
                    if (k >= 1) foreach (var s in b.parts) if (s) s.gameObject.SetActive(false);
                    continue;
                }
                if (b.hitT > 0)
                {
                    // pt483u: hit = 1 px horizontal shake (whole pixels), never a scale wobble
                    b.hitT -= dt; float px = b.hitT > 0 ? Mathf.Sign(Mathf.Sin(b.hitT * 60f)) : 0f;
                    b.main.transform.localScale = b.baseScale; b.main.transform.localPosition = b.basePos + new Vector3(px / HArt.PPU, 0, 0);
                }
            }
        }
        void ClearBrk53()
        {
            foreach (var d in _brkDecal53) if (d) Object.Destroy(d.gameObject);
            _brk53.Clear(); _brkDecal53.Clear();
        }
    }
}
