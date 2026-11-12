using System.Collections.Generic;
using UnityEngine;

// v1.4 stress test: 300+ (default 400) enemies chasing a small demon on a lava floor, dying constantly with
// batched flash/dissolve, pooled numbers, budgeted bursts, XP gems, novas and chain lightning. FPS readout top-left.
// Code-made art only. UNTESTED IN EDITOR - numbers in the README are estimates; measure on your target device.
namespace PixelFXPack.Demo
{
    [AddComponentMenu("Pixel FX/Demo/Horde Stress Test")]
    public class HordeStressTest : MonoBehaviour
    {
        public float pixelsPerUnit = 16f;
        [Tooltip("Optional; the Uber material (auto-loaded from Resources if empty).")] public Material uberMaterial;
        [Range(50, 2000)] public int enemyCount = 400;
        public bool effects = true, numbers = true, dissolve = true;

        static readonly Dictionary<char, Color32> Pal = new Dictionary<char, Color32>
        {
            { 'k', new Color32(18, 6, 10, 255) }, { 'r', new Color32(200, 36, 40, 255) }, { 'R', new Color32(120, 16, 28, 255) },
            { 'y', new Color32(255, 222, 96, 255) }, { 'o', new Color32(240, 120, 30, 255) }, { 'w', new Color32(255, 246, 220, 255) },
            { 'p', new Color32(120, 70, 150, 255) }, { 'P', new Color32(70, 36, 96, 255) }, { 'g', new Color32(150, 230, 90, 255) },
            { 'b', new Color32(60, 40, 52, 255) }, { 'B', new Color32(150, 120, 120, 255) },
        };
        internal static readonly string[] Demon = {
            "..k......k..", ".kyk....kyk.", ".kyok..koyk.", "..kkkkkkkk..", ".kRrrrrrrRk.", "kRrrrrrrrrRk",
            "kRrwkrrwkrRk", "kRrrrrrrrrRk", ".kRrryyrrRk.", "..kRRRRRRk..", ".kRrRRRRrRk.", "kRk.kRRk.kRk", "kk..kRRk..kk", "....kk.kk..." };
        internal static readonly string[] DemonLord = {
            "k..k......k..k", "kyk.k....k.kyk", ".kyokk..kkoyk.", "..kyykkkkyyk..", ".kRrrrrrrrrRk.", "kRrrryrryrrrRk",
            "kRrrwkrrwkrrRk", "kRrrrrrrrrrrRk", ".kRrrkwwkrrRk.", "kokRRRRRRRRkok", "korRrRRRRrRrok", "kok.kRRRRk.kok",
            "kk..kRRRRk..kk", "....kRk.kRk...", "....kk...kk..." };
        static readonly string[][] Imp = {
            new[] { ".k....k.", "kpk..kpk", "kppkkppk", "kpgppgpk", "kppppppk", ".kpPPpk.", "kpk..kpk", "kk....kk" },
            new[] { ".k....k.", "kpk..kpk", "kppkkppk", "kpgppgpk", "kppppppk", ".kpPPpk.", ".kpk.kp.", "..kk.kk." } };

        Camera _cam; Transform _player; PixelFX _playerFX; Sprite[] _impFrames;
        SpriteRenderer[] _sr; Vector2[] _pos; float[] _hp; bool[] _dead; int _live;
        readonly List<HellFXRunner.Job> _gems = new List<HellFXRunner.Job>();
        float _tNova, _tChain, _tMeteor, _fps = 60, _fpsMin = 999, _minReset; int _kills, _streak; bool _evolved;
        Vector2 _wander;

        void Start()
        {
            _cam = Camera.main;
            if (!_cam) _cam = new GameObject("Camera").AddComponent<Camera>();
            _cam.orthographic = true; _cam.orthographicSize = 135f / pixelsPerUnit; _cam.backgroundColor = HellPalette.Black;
            _cam.transform.position = new Vector3(0, 0, -10);
            if (!_cam.GetComponent<ScreenShake>()) _cam.gameObject.AddComponent<ScreenShake>();
            HellFX.PPU = pixelsPerUnit;
            HordeFX.I.pixelsPerUnit = pixelsPerUnit;
            PixelParticles.Instance.pixelsPerUnit = pixelsPerUnit; PixelParticles.Instance.maxParticles = 768;
            HellFX.LavaGround(Vector3.zero, 8, 5, 10f);

            _player = new GameObject("Demon").transform;
            var psr = _player.gameObject.AddComponent<SpriteRenderer>();
            psr.sprite = PixelSpriteFactory.FromRows(Demon, Pal, pixelsPerUnit, 2, "Demon");
            if (uberMaterial) psr.sharedMaterial = uberMaterial;
            psr.sortingOrder = 50;
            _playerFX = _player.gameObject.AddComponent<PixelFX>(); _playerFX.outlineSize = 1; _playerFX.outlineColor = HellPalette.Black;
            HellFX.AuraField(_player, 34, HellPalette.Crimson, 1e6f, 0.35f, AuraTick);
            HellFX.OrbitWeapons(_player, PixelFXTextures.Glow(3, 3, pixelsPerUnit), 3, 26, 0.8f, 1e6f, HellPalette.Ember);

            _impFrames = Atlas(Imp);
            _sr = new SpriteRenderer[2000]; _pos = new Vector2[2000]; _hp = new float[2000]; _dead = new bool[2000];
            for (int i = 0; i < _sr.Length; i++)
            {
                var sr = new GameObject("imp").AddComponent<SpriteRenderer>();
                sr.transform.SetParent(transform, false); sr.sprite = _impFrames[0]; sr.sortingOrder = 30;
                HordeFX.Prepare(sr);
                _sr[i] = sr; Respawn(i); sr.gameObject.SetActive(i < enemyCount);
            }
        }

        // all imp frames share ONE texture so they batch
        Sprite[] Atlas(string[][] frames)
        {
            int w = frames[0][0].Length, h = frames[0].Length, n = frames.Length;
            var t = new Texture2D(w * n, h, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, name = "ImpAtlas" };
            var px = new Color32[w * n * h];
            for (int f = 0; f < n; f++) for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
            { char c = frames[f][h - 1 - y][x]; px[y * w * n + f * w + x] = Pal.TryGetValue(c, out var col) ? col : new Color32(0, 0, 0, 0); }
            t.SetPixels32(px); t.Apply();
            var s = new Sprite[n];
            for (int f = 0; f < n; f++) s[f] = Sprite.Create(t, new Rect(f * w, 0, w, h), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            return s;
        }

        void Respawn(int i)
        {
            float a = Random.value * Mathf.PI * 2, r = Random.Range(17f, 22f);
            _pos[i] = (Vector2)(_player ? _player.position : Vector3.zero) + new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r * 0.6f);
            _hp[i] = 3; _dead[i] = false;
            if (effects && Random.value < 0.05f && Mathf.Abs(_pos[i].x) < 14) HellFX.SpawnPortal(_pos[i], 8, 0.8f);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            _fps = Mathf.Lerp(_fps, 1f / Mathf.Max(1e-4f, Time.unscaledDeltaTime), 0.05f);
            if (Time.time > 2 && 1f / Time.unscaledDeltaTime < _fpsMin) _fpsMin = 1f / Time.unscaledDeltaTime;
            if (Time.time > _minReset) { _minReset = Time.time + 5; _fpsMin = 999; }

            _wander = new Vector2(Mathf.Cos(Time.time * 0.4f), Mathf.Sin(Time.time * 0.7f)) * 3f;
            _player.position = new Vector3(Mathf.Round(_wander.x * pixelsPerUnit) / pixelsPerUnit, Mathf.Round(_wander.y * pixelsPerUnit) / pixelsPerUnit, 0);
            Vector2 pp = _player.position; float unit = 1f / pixelsPerUnit;
            int frame = Mathf.FloorToInt(Time.time * 6) & 1; _live = 0;
            for (int i = 0; i < enemyCount; i++)
            {
                if (_dead[i]) continue;
                var sr = _sr[i]; if (!sr.gameObject.activeSelf) sr.gameObject.SetActive(true);
                _live++;
                var d = pp - _pos[i]; float m = d.magnitude;
                if (m > 0.9f) _pos[i] += d / m * 1.6f * dt;
                sr.sprite = _impFrames[(frame + i) & 1];
                sr.flipX = d.x < 0;
                sr.transform.localPosition = new Vector3(Mathf.Round(_pos[i].x / unit) * unit, Mathf.Round(_pos[i].y / unit) * unit, 0);
            }
            for (int i = enemyCount; i < _sr.Length; i++) if (_sr[i].gameObject.activeSelf && !_dead[i]) _sr[i].gameObject.SetActive(false);

            if (!effects) { KillInRadius(pp, 2.2f, 99, false); return; }
            if ((_tNova += dt) > 1.4f) { _tNova = 0; HellFX.HellfireNova(_player.position, 56, 0.45f, false); KillInRadius(pp, 3.5f, 99, false); }
            if ((_tChain += dt) > 0.7f) { _tChain = 0; Chain(pp); }
            if ((_tMeteor += dt) > 4f) { _tMeteor = 0; HellFX.MeteorRain(_player.position, 90, 8, 1.2f, p => KillInRadius(p, 1.2f, 99, false)); }
            HellFX.MagnetAll(_gems, _player, 44);
            if (!_evolved && _kills > 400) { _evolved = true; HellFX.DemonEvolve(_playerFX, () => _playerFX.SpriteRenderer.sprite = PixelSpriteFactory.FromRows(DemonLord, Pal, pixelsPerUnit, 2, "DemonLord")); }
        }

        void AuraTick() => KillInRadius(_player.position, 34f / pixelsPerUnit, 1, true);

        void KillInRadius(Vector2 c, float r, float dmg, bool crits)
        {
            float r2 = r * r;
            for (int i = 0; i < enemyCount; i++)
            {
                if (_dead[i] || (_pos[i] - c).sqrMagnitude > r2) continue;
                bool crit = crits && Random.value < 0.1f;
                float d = crit ? dmg * 3 : dmg;
                _hp[i] -= d;
                if (_hp[i] > 0) { HordeFX.Hit(_sr[i], Mathf.RoundToInt(d * 7), crit, numbers); continue; }
                Die(i);
            }
        }

        void Die(int i)
        {
            _dead[i] = true; _kills++; _streak++;
            var at = _sr[i].transform.position;
            if (dissolve) HordeFX.Kill(_sr[i], sr => { sr.gameObject.SetActive(false); Respawn(i); });
            else { HordeFX.CheapBurst(at, HellPalette.Ember); _sr[i].gameObject.SetActive(false); Respawn(i); }
            if (_gems.Count < 150 && Random.value < 0.5f) { float r = Random.value; var g = r < 0.06f ? HellFX.BonePickup(at, Random.value < 0.25f) : HellFX.BloodDrop(at, r < 0.12f ? 2 : r < 0.35f ? 1 : 0); if (g != null) _gems.Add(g); }
            if (Random.value < 0.03f) { if (Random.value < 0.5f) HellFX.BloodStream(at, _player, 3); else HellFX.SoulStream(at, _player, 2, 0.5f); }
            if (_streak % 50 == 0) HellFX.KillStreakPop(_player.position, _streak);
        }

        readonly List<Vector3> _chain = new List<Vector3>(8);
        void Chain(Vector2 from)
        {
            _chain.Clear(); _chain.Add(_player.position);
            var cur = from;
            for (int hop = 0; hop < 5; hop++)
            {
                int best = -1; float bd = 36f;
                for (int i = 0; i < enemyCount; i++) { if (_dead[i]) continue; float d = (_pos[i] - cur).sqrMagnitude; if (d < bd && d > 0.25f) { bd = d; best = i; } }
                if (best < 0) break;
                _chain.Add(_sr[best].transform.position); cur = _pos[best];
                _hp[best] = 0; Die(best);
            }
            if (_chain.Count > 1) HellFX.ChainLightning(_chain, HellPalette.Soul);
        }

        void OnGUI()
        {
            GUI.color = Color.white;
            var pp = PixelParticles.Instance; var h = HordeFX.I;
            GUI.Label(new Rect(8, 6, 600, 22), $"FPS {_fps:0}  (min 5s {(_fpsMin > 998 ? 0 : _fpsMin):0})   enemies {_live}/{enemyCount}   kills {_kills}");
            GUI.Label(new Rect(8, 24, 600, 22), $"particles {pp.LiveCount}/{pp.maxParticles}  fx sprites {HellFXRunner.I.Alive}  dissolves {h.ActiveDissolves}  numbers {h.ActiveNumbers}");
            if (GUI.Button(new Rect(8, 48, 70, 22), "-100")) enemyCount = Mathf.Max(50, enemyCount - 100);
            if (GUI.Button(new Rect(82, 48, 70, 22), "+100")) enemyCount = Mathf.Min(2000, enemyCount + 100);
            effects = GUI.Toggle(new Rect(160, 48, 80, 22), effects, "effects");
            numbers = GUI.Toggle(new Rect(240, 48, 80, 22), numbers, "numbers");
            dissolve = GUI.Toggle(new Rect(320, 48, 90, 22), dissolve, "dissolve");
            GUI.Label(new Rect(8, 72, 700, 22), "Profile a DEVELOPMENT BUILD on the target device; Editor FPS is not representative.");
        }
    }
}
