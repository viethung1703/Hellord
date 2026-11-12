using System; using UnityEditor; using UnityEditor.Build.Reporting; using UnityEngine;
namespace Hellord.EditorTools {
/// <summary>Non-Development Linux64 player (same defines as web build; debug keys are not dev-gated).</summary>
public static class HellordLinuxBuild {
  [MenuItem("Hellord/Debug/Build Linux Player (Release)", priority = 91)]
  public static void Build() {
    string outDir = "/workspace/Hellord/Builds/linux_out"; var a = Environment.GetCommandLineArgs();
    for (int i=0;i<a.Length-1;i++) if (a[i]=="-hellordOut") outDir=a[i+1];
    PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
    PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
    PlayerSettings.resizableWindow = true;
    var o = new BuildPlayerOptions { scenes = new[]{"Assets/Hellord/Scenes/Hellord_Prototype.unity"},
      locationPathName = outDir + "/Hellord.x86_64", target = BuildTarget.StandaloneLinux64, options = BuildOptions.None };
    var r = BuildPipeline.BuildPlayer(o);
    Debug.Log("[HellordBuild] linux result="+r.summary.result+" errors="+r.summary.totalErrors+" warnings="+r.summary.totalWarnings+" size="+r.summary.totalSize+" time="+r.summary.totalTime);
    if (Application.isBatchMode) EditorApplication.Exit(r.summary.result==BuildResult.Succeeded?0:1);
  }
}}
