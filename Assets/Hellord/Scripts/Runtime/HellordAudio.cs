using System.Collections.Generic;
using UnityEngine;

// v0.0.29: batch-1 SFX (Hellord Sound, AUDIO_MAP.md). Clips load from Resources/Audio/SFX/<folder>/<hook>_NN.
// Pooled 2D voices, random variant (never the same twice in a row), per-hook volume/pitch ranges, voice caps and rate limits.
namespace Hellord
{
    public class HellordAudio : MonoBehaviour
    {
        class Hook { public string folder; public float v0, v1, p0, p1, minGap; public int cap; public AudioClip[] clips; public int last = -1; public float lastT = -99; public bool ui; }
        static HellordAudio _i;
        public static HellordAudio I { get { if (_i == null) { var go = new GameObject("HellordAudio"); DontDestroyOnLoad(go); _i = go.AddComponent<HellordAudio>(); } return _i; } }
        readonly Dictionary<string, Hook> _h = new Dictionary<string, Hook>();
        readonly List<AudioSource> _pool = new List<AudioSource>(); readonly Dictionary<AudioSource, string> _owner = new Dictionary<AudioSource, string>();
        AudioSource _heart; float _heartV; int _xpStep; float _xpLast = -99;
        const int PoolSize = 24;

        void Add(string hook, string folder, float v0, float v1, float p0, float p1, int cap, float minGap = 0)
        {
            var list = new List<AudioClip>();
            for (int k = 1; k <= 9; k++) { var c = Resources.Load<AudioClip>($"Audio/SFX/{folder}/{hook}_{k:00}"); if (c == null) break; list.Add(c); }
            if (list.Count == 0) { var c1 = Resources.Load<AudioClip>($"Audio/SFX/{folder}/{hook}"); if (c1 != null) list.Add(c1); } // v0.0.34 single files without _NN
            if (list.Count == 0) Debug.LogWarning("[HellordAudio] no clips for " + hook);
            _h[hook] = new Hook { folder = folder, v0 = v0, v1 = v1, p0 = p0, p1 = p1, cap = cap, minGap = minGap, clips = list.ToArray() };
        }
        void Awake()
        {
            // AUDIO_MAP.md batch 1 values
            Add("enemy_hit", "enemy", 0.55f, 0.7f, 0.92f, 1.08f, 4, 0.04f);
            Add("enemy_death", "enemy", 0.7f, 0.85f, 0.9f, 1.1f, 6, 0.05f);   // ~20/s, cap 6
            Add("elite_death", "enemy", 1f, 1f, 0.95f, 1.05f, 2);
            Add("fireball_cast", "fireball", 0.6f, 0.75f, 0.93f, 1.07f, 3, 0.05f);
            Add("fireball_impact", "fireball", 0.7f, 0.85f, 0.9f, 1.1f, 4, 0.03f);
            Add("frost_cleave", "skills", 0.6f, 0.75f, 0.95f, 1.05f, 3);
            Add("storm_ring", "skills", 0.5f, 0.65f, 0.9f, 1.1f, 2, 0.1f);      // once per ring pulse
            Add("bone_orbit_hit", "skills", 0.5f, 0.65f, 0.9f, 1.1f, 3, 1f / 12f); // ~12/s
            Add("curse_rune_arm", "skills", 0.7f, 0.7f, 0.97f, 1.03f, 3);
            Add("curse_rune_burst", "skills", 0.8f, 0.9f, 0.93f, 1.07f, 3, 0.03f);
            Add("hero_hurt", "hero", 0.85f, 1f, 0.95f, 1.05f, 1, 0.25f);
            Add("hero_death", "hero", 1f, 1f, 1f, 1f, 1);
            Add("xp_pickup", "pickups", 0.45f, 0.55f, 0.97f, 1.03f, 4);
            // v0.0.34 batch 2 (events)
            Add("levelup_sting", "events", 0.8f, 0.8f, 1f, 1f, 1);
            Add("ui_card_select", "events", 0.5f, 0.5f, 0.95f, 1.05f, 2, 0.05f);
            Add("ui_upgrade_confirm", "events", 0.7f, 0.7f, 0.97f, 1.03f, 1);
            Add("room_cleared_sting", "events", 0.7f, 0.7f, 1f, 1f, 1);
            Add("door_unlock", "events", 0.7f, 0.7f, 0.95f, 1.05f, 1);
            Add("banner_whoosh_floor", "events", 0.6f, 0.6f, 1f, 1f, 1);
            Add("banner_whoosh_boss", "events", 0.7f, 0.7f, 1f, 1f, 1);
            Add("chest_open", "events", 0.7f, 0.7f, 0.95f, 1.05f, 1);
            Add("shrine_use", "events", 0.7f, 0.7f, 0.97f, 1.03f, 1);
            Add("shop_purchase", "events", 0.6f, 0.6f, 0.95f, 1.05f, 2);
            // v0.0.34 batch 3 (enemies, boss)
            Add("hook_windup_rattle", "enemies", 0.6f, 0.6f, 0.95f, 1.05f, 3, 0.05f);
            Add("hook_chain_throw", "enemies", 0.6f, 0.6f, 0.93f, 1.07f, 3, 0.05f);
            Add("hook_hit", "enemies", 0.8f, 0.8f, 0.95f, 1.05f, 1, 0.1f);
            Add("gossip_shout_warning", "enemies", 0.6f, 0.6f, 0.9f, 1.1f, 2, 0.05f);
            Add("gossip_shout_blast", "enemies", 0.75f, 0.75f, 0.95f, 1.05f, 2, 0.05f);
            Add("oxhead_roar", "boss", 0.9f, 0.9f, 0.97f, 1.03f, 1);
            Add("oxhead_horn_windup", "boss", 0.8f, 0.8f, 1f, 1f, 1);
            Add("oxhead_horn_strike", "boss", 0.9f, 0.9f, 0.96f, 1.04f, 2);
            Add("oxhead_stomp_windup", "boss", 0.8f, 0.8f, 0.97f, 1.03f, 1);
            Add("oxhead_stomp_strike", "boss", 1f, 1f, 0.95f, 1.05f, 2);
            Add("oxhead_lunge_dash", "boss", 0.8f, 0.8f, 0.95f, 1.05f, 1);
            Add("oxhead_phase_change", "boss", 1f, 1f, 1f, 1f, 1);
            // v0.0.38: quiet aliases of existing clips for the three skills that had no sound (pitch comes from HellordTuning at the call site)
            Alias("spear_cast", "fireball_cast", 0.28f, 0.36f, 1.3f, 1.4f, 2, 0.08f);
            Alias("whip_cast", "frost_cleave", 0.3f, 0.4f, 1.2f, 1.3f, 2, 0.1f);
            Alias("pool_cast", "curse_rune_arm", 0.25f, 0.33f, 0.78f, 0.84f, 2, 0.2f);
            // v0.0.61 t611u menu UI hooks (AUDIO_MAP MENU_AUDIO volumes via HellordTuning; restart copied into ui/)
            var T = HellordTuning.I;
            Add("ui_hover", "ui", T.uiHoverVol611, T.uiHoverVol611, 0.98f, 1.02f, 2, T.uiHoverGap611);
            Add("ui_confirm", "ui", T.uiConfirmVol611, T.uiConfirmVol611, 1f, 1f, 1, 0.05f);
            Add("ui_back", "ui", T.uiBackVol611, T.uiBackVol611, 1f, 1f, 1, 0.05f);
            Add("ui_denied", "ui", T.uiDeniedVol611, T.uiDeniedVol611, 1f, 1f, 1, 0.15f);
            Add("seal_press", "ui", T.sealPressVol611, T.sealPressVol611, 1f, 1f, 1);
            Add("altar_carousel", "ui", T.altarCarouselVol611, T.altarCarouselVol611, 0.98f, 1.02f, 2, 0.06f);
            Add("pact_unlock", "ui", T.pactUnlockVol611, T.pactUnlockVol611, 1f, 1f, 1);
            Add("pact_sign", "ui", T.pactSignVol611, T.pactSignVol611, 1f, 1f, 1);
            Add("pact_remove", "ui", T.pactRemoveVol611, T.pactRemoveVol611, 1f, 1f, 1);
            Add("path_switch", "ui", T.pathSwitchVol611, T.pathSwitchVol611, 1f, 1f, 1);
            Add("pact_seal_descend", "ui", T.sealDescendVol611, T.sealDescendVol611, 1f, 1f, 1);
            Add("card_reroll", "ui", T.cardRerollVol611, T.cardRerollVol611, 1f, 1f, 1);
            Add("card_skip", "ui", T.cardSkipVol611, T.cardSkipVol611, 1f, 1f, 1);
            Add("restart", "ui", T.restartVol611, T.restartVol611, 1f, 1f, 1);
            foreach (var kv in _h) if (kv.Value.folder == "ui") kv.Value.ui = true;
            { var sb = new System.Text.StringBuilder("[HellordAudio] v0.0.64 menu hooks:"); foreach (var kv in _h) if (kv.Value.folder == "ui") sb.Append(' ').Append(kv.Key).Append('=').Append(kv.Value.clips.Length); sb.Append(" sfxEnabled=").Append(T.sfxEnabled).Append(" sfxVolume=").Append(T.sfxVolume).Append(" music=").Append(T.musicEnabled).Append(" menuVol=").Append(T.menuMusicVol611); Debug.Log(sb.ToString()); } // v0.0.62 t613u diagnostics
            foreach (var n in new[] { "ui_card_select", "ui_upgrade_confirm", "levelup_sting", "chest_open", "shop_purchase" }) if (_h.TryGetValue(n, out var uh)) uh.ui = true;
            for (int k = 0; k < PoolSize; k++) { var s = gameObject.AddComponent<AudioSource>(); s.playOnAwake = false; s.spatialBlend = 0; _pool.Add(s); }
            _heart = gameObject.AddComponent<AudioSource>(); _heart.playOnAwake = false; _heart.loop = true; _heart.spatialBlend = 0; _heart.volume = 0;
            _heart.clip = Resources.Load<AudioClip>("Audio/SFX/hero/low_hp_heartbeat_loop");
        }
        void Alias(string hook, string src, float v0, float v1, float p0, float p1, int cap, float minGap)
        {
            if (!_h.TryGetValue(src, out var h) || h.clips.Length == 0) return;
            _h[hook] = new Hook { folder = h.folder, v0 = v0, v1 = v1, p0 = p0, p1 = p1, cap = cap, minGap = minGap, clips = h.clips };
        }
        // v0.0.50 ambient hell screams: one dedicated 2D voice (one at a time), random variant without immediate repeat,
        // chance per room enter, global cooldown, side pan (off-screen feel), muted/faded during boss fights. All numbers in HellordTuning (…50).
        AudioSource _scream; AudioClip[] _screamClips; int _screamLast = -1; float _screamReadyT, _screamFade = -1f; Coroutine _screamCo;
        public static bool ScreamPlaying => _i != null && _i._scream != null && _i._scream.isPlaying;
        /// <summary>v0.0.50: call on every room enter. bossFight = boss room / boss alive.</summary>
        public static void RoomEnterScream(bool bossFight)
        {
            var T = HellordTuning.I; if (!T.ambientScreamsOn50 || !T.sfxEnabled) return;
            if (bossFight && T.screamMuteInBoss50) return;
            var a = I; float now = Time.unscaledTime;
            if (now < a._screamReadyT || ScreamPlaying || a._screamCo != null) return;
            if (Random.value >= T.screamChance50) return;
            a._screamCo = a.StartCoroutine(a.ScreamAfter(Random.Range(T.screamDelayMin50, Mathf.Max(T.screamDelayMin50, T.screamDelayMax50))));
        }
        System.Collections.IEnumerator ScreamAfter(float d)
        {
            yield return new WaitForSecondsRealtime(d); _screamCo = null;
            var T = HellordTuning.I; if (BossTrack >= 0 && T.screamMuteInBoss50) yield break;
            if (_screamClips == null) { var l = new List<AudioClip>(); for (int k = 1; k <= 9; k++) { var c = Resources.Load<AudioClip>($"Audio/SFX/ambient/hell_scream_{k:00}"); if (c != null) l.Add(c); } _screamClips = l.ToArray(); if (l.Count == 0) Debug.LogWarning("[HellordAudio] no hell_scream clips"); }
            if (_screamClips.Length == 0 || (_scream != null && _scream.isPlaying)) yield break;
            if (_scream == null) { _scream = gameObject.AddComponent<AudioSource>(); _scream.playOnAwake = false; _scream.spatialBlend = 0; }
            int v = Random.Range(0, _screamClips.Length); if (_screamClips.Length > 1 && v == _screamLast) v = (v + 1 + Random.Range(0, _screamClips.Length - 1)) % _screamClips.Length;
            _screamLast = v; _scream.clip = _screamClips[v];
            _scream.volume = Random.Range(T.screamVolMin50, Mathf.Max(T.screamVolMin50, T.screamVolMax50)) * Master;
            _scream.pitch = Random.Range(T.screamPitchMin50, Mathf.Max(T.screamPitchMin50, T.screamPitchMax50));
            _scream.panStereo = Random.Range(-T.screamPanMax50, T.screamPanMax50); _screamFade = -1f; _scream.Play();
            _screamReadyT = Time.unscaledTime + Random.Range(T.screamCooldownMin50, Mathf.Max(T.screamCooldownMin50, T.screamCooldownMax50));
        }
        void Update()
        {
            if (_scream == null || !_scream.isPlaying) return;
            var T = HellordTuning.I;
            if ((BossTrack >= 0 && T.screamMuteInBoss50) || !T.sfxEnabled)
            {
                if (_screamFade < 0) _screamFade = _scream.volume;
                _scream.volume -= _screamFade * Time.unscaledDeltaTime / Mathf.Max(0.01f, T.screamBossFadeS50);
                if (_scream.volume <= 0.001f) _scream.Stop();
            }
        }
        static float Master => HellordTuning.I.sfxEnabled ? HellordTuning.I.sfxVolume : 0f;
        /// <summary>v0.0.62 t613u: UI/menu hooks use their own gain (not scaled by sfxVolume 0.35, which made -24 dBFS hover clips ~-41 dBFS = inaudible).</summary>
        static float UiMaster => HellordTuning.I.sfxEnabled ? HellordTuning.I.uiSfxGain612 : 0f;

        public static void Play(string hook, float pitchOverride = -1) { if (HellordTuning.I.sfxEnabled) I.PlayI(hook, pitchOverride); }
        /// <summary>v0.0.34: stop every voice currently playing a hook (e.g. horn wind-up cut short).</summary>
        public static void Stop(string hook) { if (_i == null) return; foreach (var s in _i._pool) if (s.isPlaying && _i._owner.TryGetValue(s, out var o) && o == hook) s.Stop(); }
        /// <summary>v0.0.34: play after a delay (unscaled time).</summary>
        public static void PlayDelayed(string hook, float delay) { if (HellordTuning.I.sfxEnabled) I.StartCoroutine(I.Delayed(hook, delay)); }
        System.Collections.IEnumerator Delayed(string hook, float d) { yield return new WaitForSecondsRealtime(d); PlayI(hook, -1); }
        /// <summary>v0.0.34: duck music by dB for a duration.</summary>
        float _duckMul = 1f, _duckUntil = -99;
        public static void DuckMusic(float db, float dur) { var a = I; a._duckMul = Mathf.Pow(10f, -db / 20f); a._duckUntil = Time.unscaledTime + dur; }
        readonly HashSet<string> _dbg613 = new HashSet<string>();
        void PlayI(string hook, float pitchOverride)
        {
            if (!_h.TryGetValue(hook, out var h) || h.clips.Length == 0) { if (_dbg613.Add("miss:" + hook)) Debug.LogWarning("[HellordAudio] Play(" + hook + ") has no clips"); return; }
            if (h.ui && _dbg613.Add("ui:" + hook)) Debug.Log("[HellordAudio] first play " + hook + " vol=" + (h.v0 * (h.ui ? UiMaster : Master))); // v0.0.62 t613u
            float now = Time.unscaledTime; if (now - h.lastT < h.minGap) return;
            int active = 0; AudioSource free = null;
            foreach (var s in _pool) { if (s.isPlaying) { if (_owner.TryGetValue(s, out var o) && o == hook) active++; } else if (free == null) free = s; }
            if (active >= h.cap || free == null) return;
            int v = Random.Range(0, h.clips.Length); if (h.clips.Length > 1 && v == h.last) v = (v + 1 + Random.Range(0, h.clips.Length - 1)) % h.clips.Length;
            h.last = v; h.lastT = now; _owner[free] = hook;
            free.clip = h.clips[v]; free.volume = Mathf.Clamp01(Random.Range(h.v0, h.v1) * (h.ui ? UiMaster : Master));
            free.pitch = pitchOverride > 0 ? pitchOverride : Random.Range(h.p0, h.p1); free.Play();
        }
        /// <summary>XP combo: +1 semitone per pickup within 0.5 s (cap +12), pitch = 2^(step/12) * rand(0.97..1.03).</summary>
        public static void XpPickup()
        {
            if (!HellordTuning.I.sfxEnabled) return; var a = I; float now = Time.unscaledTime;
            a._xpStep = now - a._xpLast < 0.5f ? Mathf.Min(a._xpStep + 1, 12) : 0; a._xpLast = now;
            a.PlayI("xp_pickup", Mathf.Pow(2f, a._xpStep / 12f) * Random.Range(0.97f, 1.03f));
        }
        /// <summary>Hero death: duck (stop) the other one-shots, then play the death sting.</summary>
        // v0.0.33 music: floor 1 combat loop. Plays during the run (quieter on level-up/pause), fades out on death.
        AudioSource _music; float _musicV;
        public static float MusicPitch = 1f; // v0.0.42: per-floor-theme pitch (HellordTuning.themeMusicPitch); only one music loop exists
        // v0.0.48: boss music. BossTrack is set every frame by the game (-1 = floor loop); switches crossfade between two sources,
        // boss-to-boss switches (Horse-Face P1 -> P2) wait for the next bar line of the playing track.
        public static int BossTrack = -1;
        static readonly string[] BossClipNames = { "music_boss_oxhead_180bpm", "music_boss_horseface_140bpm", "music_boss_horseface_p2_155bpm", "music_boss_icegorilla_150bpm", "music_boss_qilin_160bpm" };
        AudioSource _musicB; AudioClip _floorClip; AudioClip[] _bossClips; int _curTrack = -1, _prevTrack = -1, _pendTrack = -2; float _pendAt, _xf = 1f;
        // v0.0.53: floor music playlists per theme (0 Ox Pit, 1 Frozen, 2 Burning, 3 Pounding Mortar). Each new floor picks a track
        // from a per-run shuffle; with musicRotate53 a track plays musicLoopsPerTrack53 loops then rotates to the next one of its theme.
        public static int FloorTheme53, FloorKey53;
        static readonly string[][] FloorPlaylists53 = {
            new[] { "music_floor1_combat", "Floor/music_oxpit_optA_128bpm" },
            new[] { "Floor/music_frozen_optA_100bpm", "Floor/music_frozen_optB_110bpm" },
            new[] { "Floor/music_burning_optA_140bpm", "Floor/music_burning_optB_150bpm" },
            new[] { "Floor/music_mortar_optA_80bpm", "Floor/music_mortar_optB_90bpm" } };
        int _flTheme = -1, _flKey = -999, _flPos, _flSalt = -1; float _flLoopT;
        AudioClip FloorPick53(int theme, int pos)
        {
            var list = FloorPlaylists53[Mathf.Clamp(theme, 0, 3)]; if (_flSalt < 0) _flSalt = Random.Range(0, 1000);
            for (int i = 0; i < list.Length; i++)
            {
                var c = Resources.Load<AudioClip>("Audio/Music/" + list[(pos + _flSalt + i) % list.Length]); if (c != null) return c;
            }
            return Resources.Load<AudioClip>("Audio/Music/music_floor1_combat");
        }
        void FloorMusic53()
        {
            var T = HellordTuning.I; if (!T.floorMusic53 || _music == null) return;
            if (FloorKey53 == 0 && _flKey > 0) _flSalt = Random.Range(0, 1000); // new run: reshuffle
            bool change = FloorTheme53 != _flTheme || FloorKey53 != _flKey; bool rotate = false;
            if (!change && T.musicRotate53 && _curTrack < 0 && _floorClip != null && _music.isPlaying && _music.clip == _floorClip)
            { _flLoopT += Time.unscaledDeltaTime * Mathf.Abs(_music.pitch); if (_flLoopT >= _floorClip.length * Mathf.Max(1, T.musicLoopsPerTrack53)) rotate = true; }
            if (!change && !rotate) return;
            _flPos = change ? FloorKey53 : _flPos + 1; _flTheme = FloorTheme53; _flKey = FloorKey53; _flLoopT = 0;
            var c = FloorPick53(_flTheme, _flPos); if (c == _floorClip) return; _floorClip = c;
            if (_curTrack < 0) { var tmp = _music; _music = _musicB; _musicB = tmp; _prevTrack = -1; _music.clip = c; _music.time = 0; if (_musicV > 0.001f) _music.Play(); _xf = 0f; }
        }
        AudioClip ClipFor(int k)
        {
            if (k < 0) return _floorClip;
            if (_bossClips == null) _bossClips = new AudioClip[BossClipNames.Length];
            if (k >= _bossClips.Length) return null;
            if (_bossClips[k] == null) _bossClips[k] = Resources.Load<AudioClip>("Audio/Music/Boss/" + BossClipNames[k]);
            return _bossClips[k];
        }
        /// <summary>Beat length in seconds of a boss track from HellordTuning.bossMusicBpm (for bar sync and later pestle sync).</summary>
        public static float BossBeatS(int k) { var b = HellordTuning.I.bossMusicBpm; float bpm = b != null && k >= 0 && k < b.Length ? b[k] : 0f; return bpm > 1f ? 60f / bpm : 0f; }
        AudioSource NewMusicSource() { var s = gameObject.AddComponent<AudioSource>(); s.playOnAwake = false; s.loop = true; s.spatialBlend = 0; s.volume = 0; return s; }
        void TickMusic(bool inRun)
        {
            var T = HellordTuning.I;
            if (_music == null) { _music = NewMusicSource(); _musicB = NewMusicSource(); _floorClip = Resources.Load<AudioClip>("Audio/Music/music_floor1_combat"); _music.clip = _floorClip; }
            FloorMusic53();
            int want = T.bossMusicV48 && BossTrack >= 0 && ClipFor(BossTrack) != null ? BossTrack : -1;
            if (want != _curTrack && want != _pendTrack)
            {
                _pendTrack = want; _pendAt = Time.unscaledTime; float beat = BossBeatS(_curTrack);
                if (T.bossMusicBarSync && _curTrack >= 0 && want >= 0 && beat > 0f && _music.isPlaying && _music.clip != null)
                {
                    float bar = beat * Mathf.Max(1, T.bossMusicBeatsPerBar), wait = bar - (_music.time % bar); if (wait < 0.02f) wait += bar;
                    _pendAt += wait;
                }
            }
            else if (want == _curTrack) _pendTrack = -2;
            if (_pendTrack != -2 && Time.unscaledTime >= _pendAt)
            {
                var tmp = _music; _music = _musicB; _musicB = tmp; _prevTrack = _curTrack; _curTrack = _pendTrack; _pendTrack = -2;
                _music.clip = ClipFor(_curTrack); _music.time = 0f; if (_music.clip != null && _musicV > 0.001f) _music.Play();
                bool hard = _prevTrack >= 0 && _curTrack >= 0;
                _xf = hard && T.bossMusicPhaseXfadeS <= 0f ? 1f : 0f; if (_xf >= 1f) _musicB.Stop();
            }
            float xs = _prevTrack >= 0 && _curTrack >= 0 ? T.bossMusicPhaseXfadeS : T.bossMusicXfadeS;
            _xf = Mathf.MoveTowards(_xf, 1f, xs > 0f ? Time.unscaledDeltaTime / xs : 1f);
            bool paused = Time.timeScale == 0;
            float target = (T.musicEnabled && inRun) ? T.musicVolume * (paused ? T.musicPausedMul : 1f) : 0f;
            if (_curTrack >= 0) target *= T.bossMusicVolumeMul;
            _musicV = Mathf.MoveTowards(_musicV, target, Time.unscaledDeltaTime * Mathf.Max(0.05f, T.musicVolume) / 1.0f); // ~1 s fade
            float v = _musicV * (Time.unscaledTime < _duckUntil ? _duckMul : 1f);
            _music.volume = v * _xf; _musicB.volume = v * (1f - _xf);
            _music.pitch = _curTrack < 0 ? Mathf.Clamp(MusicPitch, 0.5f, 2f) : 1f; // v0.0.42 floor pitch; boss tracks play as written
            _musicB.pitch = _prevTrack < 0 ? Mathf.Clamp(MusicPitch, 0.5f, 2f) : 1f;
            if (_xf >= 1f && _musicB.isPlaying) _musicB.Stop();
            if (_music.clip == null) return;
            if (_musicV > 0.001f) { if (!_music.isPlaying) _music.Play(); }
            else if (target == 0f) { if (_music.isPlaying) _music.Stop(); if (_musicB.isPlaying) _musicB.Stop(); }
        }
        // ---- v0.0.61 t611u menu music: own two crossfading loop sources + one stinger source, independent of floor/boss music ----
        public static int MenuMusic611; // set by the game each frame: 0 none, 1 MainMenu, 2 Altar, 3 Dead, 4 Won
        AudioSource _mA611, _mB611, _st611; int _mCur611, _mEnd611; float _mXf611 = 1f, _mV611, _mHold611;
        bool IsUi611(AudioSource s) => _owner.TryGetValue(s, out var o) && _h.TryGetValue(o, out var h) && h.ui;
        void TickMenu611()
        {
            var T = HellordTuning.I; float now = Time.unscaledTime, dt = Time.unscaledDeltaTime;
            if (_mA611 == null) { _mA611 = NewMusicSource(); _mB611 = NewMusicSource(); _st611 = gameObject.AddComponent<AudioSource>(); _st611.playOnAwake = false; _st611.spatialBlend = 0; }
            int m = MenuMusic611;
            if (m >= 3 && _mEnd611 != m)
            {   // Dead/Won: one-shot stinger, the main menu loop fades in after menuStingerHoldS611
                _mEnd611 = m; _mHold611 = now + T.menuStingerHoldS611;
                var c = Resources.Load<AudioClip>(m == 3 ? "Audio/Music/music_stinger_dead" : "Audio/Music/music_stinger_won");
                if (c != null && T.musicEnabled) { _st611.clip = c; _st611.volume = T.menuStingerVol611; _st611.Play(); }
            }
            if (m < 3) _mEnd611 = 0;
            int track = m == 0 ? 0 : m == 2 ? 2 : 1;
            if (track != 0 && track != _mCur611)
            {
                var c = Resources.Load<AudioClip>(track == 2 ? "Audio/Music/music_menu_altar" : "Audio/Music/music_menu_main");
                if (c != null) { var t = _mA611; _mA611 = _mB611; _mB611 = t; _mA611.clip = c; _mA611.time = 0; _mA611.Play(); _mXf611 = _mV611 > 0.001f ? 0f : 1f; if (_mXf611 >= 1f) _mB611.Stop(); }
                _mCur611 = track; Debug.Log("[HellordAudio] menu music -> " + (track == 2 ? "altar" : "main") + (c == null ? " (CLIP MISSING)" : " ok")); // v0.0.62
            }
            _mXf611 = Mathf.MoveTowards(_mXf611, 1f, dt / Mathf.Max(0.05f, T.menuMusicXfadeS611));
            float target = T.musicEnabled && track != 0 && now >= _mHold611 ? T.menuMusicVol611 : 0f;
            float fs = target > _mV611 ? T.menuMusicFadeInS611 : T.menuMusicFadeOutS611;
            _mV611 = Mathf.MoveTowards(_mV611, target, dt * Mathf.Max(0.05f, T.menuMusicVol611) / Mathf.Max(0.05f, fs));
            _mA611.volume = _mV611 * _mXf611; _mB611.volume = _mV611 * (1f - _mXf611);
            if (_mXf611 >= 1f && _mB611.isPlaying) _mB611.Stop();
            if (track == 0 && _mV611 <= 0.001f && _mCur611 != 0) { _mA611.Stop(); _mB611.Stop(); _mCur611 = 0; }
            if (!T.musicEnabled && _st611.isPlaying) _st611.Stop();
        }
        public static void HeroDeath() { if (!HellordTuning.I.sfxEnabled) return; foreach (var s in I._pool) s.Stop(); I._heart.volume = 0; I._heart.Stop(); I.PlayI("hero_death", -1); }
        /// <summary>Called every frame: heartbeat fades in while the low-HP pulse is active (same threshold), out otherwise / when paused.</summary>
        public static void TickHeart(bool lowHp, bool playing, bool paused)
        {
            var a = I; a.TickMusic(playing || paused);
            a.TickMenu611(); // v0.0.62 t613u: menu music no longer depends on the heart clip loading
            if (a._heart.clip == null) return;
            float target = (lowHp && playing && !paused) ? 0.5f * Master : 0f;
            a._heartV = Mathf.MoveTowards(a._heartV, target, Time.unscaledDeltaTime * 0.5f / 0.6f); // ~0.6 s fade
            a._heart.volume = a._heartV;
            if (a._heartV > 0.001f) { if (!a._heart.isPlaying) a._heart.Play(); } else if (a._heart.isPlaying) a._heart.Pause();
            foreach (var s in a._pool) { if (paused) { if (s.isPlaying && !(HellordTuning.I.uiUnpaused611 && a.IsUi611(s))) s.Pause(); } else s.UnPause(); } // v0.0.61: UI hooks never paused
        }
    }
}
