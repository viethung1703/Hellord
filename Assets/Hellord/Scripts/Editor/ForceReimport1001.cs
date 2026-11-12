#if UNITY_EDITOR
using UnityEditor;
public static class ForceReimport1001 {
  public static void Run() {
    string[] paths = {
      "Assets/Hellord/Resources/HellordFX928/fx928_ramp_fireball.png",
      "Assets/Hellord/Resources/HellordShaders/HellordPixelAoE.shader",
      "Assets/Hellord/Resources/HellordFX928/fx928_fireball.mat",
    };
    foreach (var p in paths) {
      AssetDatabase.ImportAsset(p, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
      UnityEngine.Debug.Log("[t1001u] reimported " + p + " exists=" + System.IO.File.Exists(p));
    }
    AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
  }
}
#endif
