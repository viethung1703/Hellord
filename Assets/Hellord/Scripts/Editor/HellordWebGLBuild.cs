using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Hellord.EditorTools
{
    /// Batchmode WebGL build: -executeMethod Hellord.EditorTools.HellordWebGLBuild.Build -hellordOut <dir>
    public static class HellordWebGLBuild
    {
        public static void Build()
        {
            string outDir = Arg("-hellordOut") ?? "Builds/web";
            // Ensure generated scene exists
            if (!File.Exists("Assets/Hellord/Scenes/Hellord_Prototype_Generated.unity"))
                HellordMenu.CreateScene();
            string scene = "Assets/Hellord/Scenes/Hellord_Prototype_Generated.unity";
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            PlayerSettings.WebGL.template = "PROJECT:Hellord";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = false;
            var opts = new BuildPlayerOptions {
                scenes = new[] { scene }, locationPathName = outDir,
                target = BuildTarget.WebGL, options = BuildOptions.None };
            BuildReport r = BuildPipeline.BuildPlayer(opts);
            Debug.Log($"[HellordBuild] result={r.summary.result} size={r.summary.totalSize} errors={r.summary.totalErrors} out={outDir}");
            if (Application.isBatchMode) EditorApplication.Exit(r.summary.result == BuildResult.Succeeded ? 0 : 1);
        }
        static string Arg(string n) { var a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, n); return i >= 0 && i + 1 < a.Length ? a[i + 1] : null; }
    }
}
