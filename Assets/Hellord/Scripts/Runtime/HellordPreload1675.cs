using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hellord
{
    /// <summary>t1675 (VH t1675u "do it properly, with a load step"): the ONE central preloader / registry for the run loading phase.
    /// Systems register a provider (HellordPreload.Register) that, for the run's pact, queues steps (one unit of loading work each),
    /// materials to warm, and audio clips to load. HellordGame (HellordPreloadGame1675.cs) runs a plan between Descend and the run:
    ///   1. steps   - fill each system's own cache (the same lazy getters the game uses, so the cache layout is unchanged),
    ///   2. audio   - AudioClip.LoadAudioData for the queued SFX clips (the metas keep preloadAudioData 0; this loads them at load time),
    ///   3. warmup  - every queued material + every registered shader drawn once by a hidden camera into a tiny RenderTexture,
    ///                so the shader programs are compiled / uploaded here (Editor: async shader compilation is switched off for that render).
    /// The lazy first-use paths are untouched and still work as the fallback for anything not preloaded.</summary>
    public static class HellordPreload
    {
        public sealed class Ctx
        {
            public int pact;
            internal readonly List<KeyValuePair<string, System.Action>> steps = new List<KeyValuePair<string, System.Action>>();
            internal readonly List<Material> warm = new List<Material>(); readonly HashSet<Material> _warmSet = new HashSet<Material>();
            internal readonly List<Shader> shaders = new List<Shader>(); readonly HashSet<Shader> _shSet = new HashSet<Shader>();
            internal readonly List<AudioClip> clips = new List<AudioClip>(); readonly HashSet<AudioClip> _clipSet = new HashSet<AudioClip>();
            internal string provider;
            /// <summary>Queue one unit of loading work (run later, spread over frames).</summary>
            public void Step(string label, System.Action a) { if (a != null) { string p = provider; steps.Add(new KeyValuePair<string, System.Action>(p + ": " + label, a)); } }
            /// <summary>Draw this material once during the warmup (compiles its shader passes).</summary>
            public void Warm(Material m) { if (m != null && m.shader != null && _warmSet.Add(m)) warm.Add(m); }
            /// <summary>Warm a shader that has no shared material (a temp material is drawn once).</summary>
            public void WarmShader(Shader s) { if (s != null && _shSet.Add(s)) shaders.Add(s); }
            /// <summary>Load this clip's audio data during loading.</summary>
            public void Audio(AudioClip c) { if (c != null && c.loadState != AudioDataLoadState.Loaded && _clipSet.Add(c)) clips.Add(c); }
        }
        public delegate void Provider(Ctx c);
        struct Reg { public string name; public Provider p; }
        static readonly List<Reg> _reg = new List<Reg>();

        /// <summary>Register (or replace, same name) a provider. Providers run in registration order.</summary>
        public static void Register(string name, Provider p)
        {
            for (int i = 0; i < _reg.Count; i++) if (_reg[i].name == name) { _reg[i] = new Reg { name = name, p = p }; return; }
            _reg.Add(new Reg { name = name, p = p });
        }
        public static int Registered => _reg.Count;

        // ---------------------------------------------------------------- state (read by the loading screen + tests)
        public static bool Busy { get; private set; }
        public static float Progress { get; private set; }
        public static string Phase { get; private set; } = "";
        public static int Runs { get; private set; }
        /// <summary>Last run: total ms, per-phase ms, counts (logged + read by the t1675 test).</summary>
        public static float LastMs, LastStepMs, LastAudioMs, LastWarmMs; public static int LastSteps, LastClips, LastWarm, LastFrames, LastPact = -1;
        public static string LastReport = "";

        public static Ctx Plan(int pact)
        {
            var c = new Ctx { pact = pact };
            foreach (var r in _reg)
            {
                c.provider = r.name;
                try { r.p(c); } catch (System.Exception e) { Debug.LogWarning("[t1675 preload] provider " + r.name + " failed: " + e.Message); }
            }
            c.provider = null; return c;
        }

        /// <summary>Runs a plan over frames (budgetMs of work per frame). Coroutine; Busy while running.</summary>
        public static IEnumerator Run(Ctx c, float budgetMs, bool warmShaders, float minSec = 0f)
        {
            Busy = true; Progress = 0f; Phase = "LOADING"; LastPact = c.pact; float t0 = Time.realtimeSinceStartup; int frames = 0;
            var sw = System.Diagnostics.Stopwatch.StartNew(); double phase0;
            budgetMs = Mathf.Max(1f, budgetMs);
            // 1. steps (each system's caches)
            phase0 = sw.Elapsed.TotalMilliseconds; double frame0 = phase0;
            for (int i = 0; i < c.steps.Count; i++)
            {
                RunStep(c.steps[i]); Progress = 0.6f * (i + 1) / Mathf.Max(1, c.steps.Count); Phase = "LOADING";
                if (sw.Elapsed.TotalMilliseconds - frame0 > budgetMs) { frames++; yield return null; frame0 = sw.Elapsed.TotalMilliseconds; }
            }
            LastStepMs = (float)(sw.Elapsed.TotalMilliseconds - phase0);
            // 2. audio
            phase0 = sw.Elapsed.TotalMilliseconds; frame0 = phase0; Phase = "SOUNDS";
            for (int i = 0; i < c.clips.Count; i++)
            {
                LoadClip(c.clips[i]); Progress = 0.6f + 0.2f * (i + 1) / Mathf.Max(1, c.clips.Count);
                if (sw.Elapsed.TotalMilliseconds - frame0 > budgetMs) { frames++; yield return null; frame0 = sw.Elapsed.TotalMilliseconds; }
            }
            for (int g = 0; g < 600 && AnyLoading(c.clips); g++) { frames++; yield return null; } // background loads (loadInBackground clips) finish here
            LastAudioMs = (float)(sw.Elapsed.TotalMilliseconds - phase0);
            // 3. shader warmup
            phase0 = sw.Elapsed.TotalMilliseconds; Phase = "SHADERS"; int nw = 0;
            if (warmShaders)
            {
                var all = WarmList(c); int per = 8;
                for (int i = 0; i < all.Count; i += per)
                {
                    nw += ShaderWarmer.Draw(all, i, Mathf.Min(per, all.Count - i)); Progress = 0.8f + 0.2f * Mathf.Min(all.Count, i + per) / Mathf.Max(1, all.Count);
                    frames++; yield return null;
                }
                ShaderWarmer.End(all);
            }
            LastWarmMs = (float)(sw.Elapsed.TotalMilliseconds - phase0);
            Progress = 1f; Phase = "READY";
            while (Time.realtimeSinceStartup - t0 < minSec) { frames++; yield return null; }
            Finish(c, sw, frames, nw);
        }

        /// <summary>The same plan, all at once (no bar): tests / tools that need the run ready this frame.</summary>
        public static void RunNow(Ctx c, bool warmShaders)
        {
            Busy = true; LastPact = c.pact; var sw = System.Diagnostics.Stopwatch.StartNew(); double p0 = 0;
            foreach (var s in c.steps) RunStep(s); LastStepMs = (float)(sw.Elapsed.TotalMilliseconds - p0); p0 = sw.Elapsed.TotalMilliseconds;
            foreach (var a in c.clips) LoadClip(a); LastAudioMs = (float)(sw.Elapsed.TotalMilliseconds - p0); p0 = sw.Elapsed.TotalMilliseconds;
            int nw = 0; if (warmShaders) { var all = WarmList(c); nw = ShaderWarmer.Draw(all, 0, all.Count); ShaderWarmer.End(all); }
            LastWarmMs = (float)(sw.Elapsed.TotalMilliseconds - p0); Progress = 1f; Phase = "READY";
            Finish(c, sw, 0, nw);
        }
        /// <summary>Abort (e.g. the run was left while loading): nothing is undone, caches keep what was loaded.</summary>
        public static void Cancel() { Busy = false; Phase = ""; }

        static void RunStep(KeyValuePair<string, System.Action> s)
        {
            var w = System.Diagnostics.Stopwatch.StartNew();
            try { s.Value(); } catch (System.Exception e) { Debug.LogWarning("[t1675 preload] step " + s.Key + " failed: " + e.Message); }
            if (w.Elapsed.TotalMilliseconds > 40.0) Debug.Log($"[t1675 preload] slow step {s.Key}: {w.Elapsed.TotalMilliseconds:0} ms");
        }
        static void LoadClip(AudioClip a) { if (a != null && a.loadState == AudioDataLoadState.Unloaded) a.LoadAudioData(); }
        static bool AnyLoading(List<AudioClip> l) { foreach (var a in l) if (a != null && a.loadState == AudioDataLoadState.Loading) return true; return false; }
        /// <summary>Shaders already compiled by a warmup this session (no shader here has multi_compile / shader_feature keywords,
        /// so one draw per shader covers its only variant; later runs skip them).</summary>
        static readonly HashSet<Shader> _warmed = new HashSet<Shader>();
        public static int WarmedShaders => _warmed.Count;
        static List<Material> WarmList(Ctx c)
        {
            var all = new List<Material>(); var seen = new HashSet<Shader>();
            foreach (var m in c.warm) if (m != null && m.shader != null && !_warmed.Contains(m.shader) && seen.Add(m.shader)) all.Add(m); // one real material per shader (its own property values)
            foreach (var s in c.shaders) if (s != null && s.isSupported && !_warmed.Contains(s) && seen.Add(s)) all.Add(new Material(s) { name = "t1675warm_" + s.name, hideFlags = HideFlags.HideAndDontSave });
            return all;
        }
        static void Finish(Ctx c, System.Diagnostics.Stopwatch sw, int frames, int nw)
        {
            LastMs = (float)sw.Elapsed.TotalMilliseconds; LastSteps = c.steps.Count; LastClips = c.clips.Count; LastWarm = nw; LastFrames = frames; Runs++;
            LastReport = $"pact {c.pact}: {LastSteps} steps {LastStepMs:0} ms, {LastClips} clips {LastAudioMs:0} ms, {nw} shaders warmed ({c.warm.Count} FX materials queued) {LastWarmMs:0} ms, total {LastMs:0} ms over {frames} frames";
            Debug.Log("[t1675 preload] " + LastReport);
            Busy = false;
        }

        /// <summary>Hidden camera far off the play area renders each material once on a quad into a 16x16 RenderTexture.
        /// Real MeshRenderers + camera render (not a command buffer) so GrabPass shaders take their real path.</summary>
        static class ShaderWarmer
        {
            static Camera _cam; static RenderTexture _rt; static Mesh _quad; static readonly List<GameObject> _gos = new List<GameObject>();
            const int Layer = 31; static readonly Vector3 Far = new Vector3(-100000f, -100000f, 0f);
            static void Ensure()
            {
                if (_quad == null)
                {
                    _quad = new Mesh { name = "t1675warm_quad", hideFlags = HideFlags.HideAndDontSave };
                    _quad.vertices = new[] { new Vector3(-0.5f, -0.5f), new Vector3(0.5f, -0.5f), new Vector3(-0.5f, 0.5f), new Vector3(0.5f, 0.5f) };
                    _quad.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
                    _quad.colors32 = new[] { new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 255), new Color32(255, 255, 255, 255) };
                    _quad.triangles = new[] { 0, 2, 1, 2, 3, 1 }; _quad.RecalculateBounds();
                }
                if (_rt == null) { _rt = new RenderTexture(16, 16, 16) { name = "t1675warm_rt", hideFlags = HideFlags.HideAndDontSave }; _rt.Create(); }
                if (_cam == null)
                {
                    var go = new GameObject("t1675 shader warmup camera") { hideFlags = HideFlags.DontSave }; go.transform.position = Far + new Vector3(0, 0, -10f);
                    _cam = go.AddComponent<Camera>(); _cam.enabled = false; _cam.orthographic = true; _cam.orthographicSize = 1f; _cam.nearClipPlane = 0.1f; _cam.farClipPlane = 20f;
                    _cam.clearFlags = CameraClearFlags.SolidColor; _cam.backgroundColor = Color.clear; _cam.cullingMask = 1 << Layer; _cam.targetTexture = _rt; _cam.allowHDR = false; _cam.allowMSAA = false;
                }
            }
            /// <summary>Draws mats[from..from+n) once. Returns how many were drawn.</summary>
            public static int Draw(List<Material> mats, int from, int n)
            {
                if (n <= 0) return 0; Ensure(); int got = 0;
                for (int i = from; i < from + n && i < mats.Count; i++)
                {
                    var m = mats[i]; if (m == null || m.shader == null) continue;
                    var go = new GameObject("t1675warm") { layer = Layer, hideFlags = HideFlags.DontSave }; go.transform.position = Far + new Vector3(((got % 4) - 1.5f) * 0.25f, ((got / 4) - 1.5f) * 0.25f, 0f); go.transform.localScale = Vector3.one * 0.2f;
                    go.AddComponent<MeshFilter>().sharedMesh = _quad; var r = go.AddComponent<MeshRenderer>(); r.sharedMaterial = m; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false;
                    _gos.Add(go); got++; _warmed.Add(m.shader);
                }
#if UNITY_EDITOR
                bool async = UnityEditor.ShaderUtil.allowAsyncCompilation; UnityEditor.ShaderUtil.allowAsyncCompilation = false; // Editor: compile now, not as a cyan placeholder on the first cast
                try { _cam.Render(); } finally { UnityEditor.ShaderUtil.allowAsyncCompilation = async; }
#else
                _cam.Render();
#endif
                foreach (var g in _gos) Object.Destroy(g); _gos.Clear();
                return got;
            }
            /// <summary>Frees the camera + RT and the temp shader materials (the preloaded FX materials stay in their caches).</summary>
            public static void End(List<Material> all)
            {
                foreach (var m in all) if (m != null && m.name.StartsWith("t1675warm_")) Object.Destroy(m);
                if (_cam != null) { _cam.targetTexture = null; Object.Destroy(_cam.gameObject); _cam = null; }
                if (_rt != null) { _rt.Release(); Object.Destroy(_rt); _rt = null; }
            }
        }
    }
}
