using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Hellord.EditorTools
{
    public static class HellordMenu
    {
        const string ScenePath = "Assets/Hellord/Scenes/Hellord_Prototype_Generated.unity";

        [MenuItem("Hellord/Create Prototype Scene", priority = 0)]
        public static void CreateScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cam = new GameObject("Main Camera"); cam.tag = "MainCamera";
            var c = cam.AddComponent<Camera>(); c.orthographic = true; c.orthographicSize = 6.75f; c.transform.position = new Vector3(0, 0, -10);
            c.clearFlags = CameraClearFlags.SolidColor; c.backgroundColor = new Color32(14, 6, 9, 255);
            new GameObject("Hellord").AddComponent<HellordGame>();
            System.IO.Directory.CreateDirectory("Assets/Hellord/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log("[Hellord] Prototype scene created at " + ScenePath + ". Press Play.");
        }

        [MenuItem("Hellord/Create Prototype Scene and Play", priority = 1)]
        public static void CreateAndPlay() { CreateScene(); EditorApplication.isPlaying = true; }

        /// <summary>t1796: batch Play-mode Jiangshi verification. Run Unity with
        /// -executeMethod Hellord.EditorTools.HellordMenu.RunJiangshiPlaytest1796 -jiangshiPlaytest
        /// (needs a GL context; do not pass -nographics). The runtime hook exits the Editor when done.</summary>
        public static void RunJiangshiPlaytest1796()
        {
            string scene = "Assets/Hellord/Scenes/Hellord_Prototype.unity";
            if (!System.IO.File.Exists(scene)) scene = "Assets/Hellord/Scenes/Hellord_Prototype_Generated.unity";
            if (!System.IO.File.Exists(scene)) CreateScene();
            else UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scene);
            ForceGameView1920x1080_1808();
            EditorApplication.isPlaying = true;
        }

        /// <summary>t1812: batch Play-mode kit-bar supports verification. Run Unity with
        /// -executeMethod Hellord.EditorTools.HellordMenu.RunKitBarSupportsPlaytest1812 -kitbarSupportsPlaytest
        /// (needs a GL context; do not pass -nographics). The runtime hook exits the Editor when done.</summary>
        public static void RunKitBarSupportsPlaytest1812()
        {
            string scene = "Assets/Hellord/Scenes/Hellord_Prototype.unity";
            if (!System.IO.File.Exists(scene)) scene = "Assets/Hellord/Scenes/Hellord_Prototype_Generated.unity";
            if (!System.IO.File.Exists(scene)) CreateScene();
            else UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scene);
            ForceGameView1920x1080_1808();
            EditorApplication.isPlaying = true;
        }

        /// <summary>t1815: batch Play-mode skill-range rebalance verification at 1920x1080 (CAM 640x360 SAFE).
        /// -executeMethod Hellord.EditorTools.HellordMenu.RunRangeRebalancePlaytest1815 -rangeRebalancePlaytest
        /// (needs a GL context; do not pass -nographics). The runtime hook exits the Editor when done.</summary>
        public static void RunRangeRebalancePlaytest1815()
        {
            string scene = "Assets/Hellord/Scenes/Hellord_Prototype.unity";
            if (!System.IO.File.Exists(scene)) scene = "Assets/Hellord/Scenes/Hellord_Prototype_Generated.unity";
            if (!System.IO.File.Exists(scene)) CreateScene();
            else UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scene);
            ForceGameView1920x1080_1808();
            EditorApplication.isPlaying = true;
        }

        /// <summary>t1822: batch Play-mode verification of the 2e80984 playtest bugfixes at 1920x1080 (CAM 640x360 SAFE).
        /// -executeMethod Hellord.EditorTools.HellordMenu.RunBugfixPlaytest1822 -bugfixPlaytest1822
        /// (needs a GL context; do not pass -nographics).</summary>
        public static void RunBugfixPlaytest1822()
        {
            string scene = "Assets/Hellord/Scenes/Hellord_Prototype.unity";
            if (!System.IO.File.Exists(scene)) scene = "Assets/Hellord/Scenes/Hellord_Prototype_Generated.unity";
            if (!System.IO.File.Exists(scene)) CreateScene();
            else UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scene);
            ForceGameView1920x1080_1808();
            EditorApplication.isPlaying = true;
        }


        /// <summary>t1808: select a FixedResolution 1920x1080 Game View so Play-mode Screen is 1920x1080
        /// (HUD at 3x / CAM 640x360 SAFE), not the inset pane size.
        /// Unity 6 GameViewSizeGroup uses GetTotalCount (GetCount was removed and NRE'd the first helper).</summary>
        public static void ForceGameView1920x1080_1808()
        {
            try
            {
                UnityEngine.Debug.Log("[t1808] graphicsMemorySize=" + SystemInfo.graphicsMemorySize
                    + " maxTex=" + SystemInfo.maxTextureSize
                    + " renderer=" + SystemInfo.graphicsDeviceName);
                var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                var asm = typeof(Editor).Assembly;
                var sizesType = asm.GetType("UnityEditor.GameViewSizes");
                var singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
                object inst = singleton.GetProperty("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetValue(null, null);
                object group = sizesType.GetProperty("currentGroup").GetValue(inst, null);
                var groupType = group.GetType();
                var getTotal = groupType.GetMethod("GetTotalCount", flags) ?? groupType.GetMethod("GetCount", flags);
                int count = (int)getTotal.Invoke(group, null);
                int found = -1;
                var getSize = groupType.GetMethod("GetGameViewSize", flags);
                for (int i = 0; i < count; i++)
                {
                    object size = getSize.Invoke(group, new object[] { i });
                    var st = size.GetType();
                    int tw = (int)st.GetProperty("width").GetValue(size, null);
                    int th = (int)st.GetProperty("height").GetValue(size, null);
                    object ty = st.GetProperty("sizeType")?.GetValue(size, null);
                    string typ = ty != null ? ty.ToString() : "";
                    if (tw == 1920 && th == 1080 && typ.IndexOf("FixedResolution") >= 0) { found = i; break; }
                }
                if (found < 0)
                {
                    var sizeType = asm.GetType("UnityEditor.GameViewSize");
                    var enumType = asm.GetType("UnityEditor.GameViewSizeType");
                    object fixedRes = System.Enum.Parse(enumType, "FixedResolution");
                    object nv = System.Activator.CreateInstance(sizeType, fixedRes, 1920, 1080, "1920x1080 t1808");
                    groupType.GetMethod("AddCustomSize", flags).Invoke(group, new[] { nv });
                    found = count;
                }
                var gvType = asm.GetType("UnityEditor.GameView");
                var gv = EditorWindow.GetWindow(gvType, false, "Game", true);
                var prop = gvType.GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null) prop.SetValue(gv, found, null);
                // Do not set maximized/minSize/position: Unmaximize hung this Xvfb session
                // ("Maximized serialized file backup not found"). FixedResolution 1920x1080
                // is enough — GetRenderTargetSize uses that size; 128 MB VRAM can hold it.
                object tr = gvType.GetProperty("targetRenderSize", flags)?.GetValue(gv, null);
                object ts = gvType.GetProperty("targetSize", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy)?.GetValue(gv, null);
                UnityEngine.Debug.Log("[t1808] GameView selectedSizeIndex=" + found + "/" + count
                    + " pos=" + gv.position + " targetRenderSize=" + tr + " targetSize=" + ts
                    + " Screen=" + Screen.width + "x" + Screen.height);
            }
            catch (System.Exception e) { UnityEngine.Debug.LogWarning("[t1808] ForceGameView1920x1080 failed: " + e); }
        }

        /// <summary>v0.0.9: full-window WebGL template (Assets/WebGLTemplates/Hellord): fills the window, focuses on load/click, swallows F1-F10.</summary>
        [MenuItem("Hellord/Use Hellord WebGL Template", priority = 20)]
        public static void UseWebGLTemplate()
        {
            PlayerSettings.WebGL.template = "PROJECT:Hellord";
            PlayerSettings.defaultWebScreenWidth = 1280; PlayerSettings.defaultWebScreenHeight = 720;
            Debug.Log("[Hellord] WebGL template set to PROJECT:Hellord.");
        }
    }

    /// <summary>Forces pixel-art import settings on Hellord textures (Point, no compression, no mips).</summary>
    public class HellordTextureImport : AssetPostprocessor
    {
        public override uint GetVersion() { return 1688; } // t1688: bump = reimport once so the GonggongFX exclusion takes effect (other textures import unchanged)
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Hellord/")) return;
            // t1688: GonggongFX shader data textures keep their authored .meta import (noise = Repeat + Bilinear, ramp = Point + Clamp).
            // Forcing Clamp + Point here broke every noise lookup outside 0..1 (Eternal Winter v2 drew a flat white wash with smeared rows).
            if (assetPath.StartsWith("Assets/Hellord/Resources/GonggongFX/") || assetPath.StartsWith("Assets/Hellord/Resources/ZhongKuiFX/")) return;
            var ti = (TextureImporter)assetImporter;
            ti.filterMode = FilterMode.Point; ti.textureCompression = TextureImporterCompression.Uncompressed; ti.mipmapEnabled = false;
            ti.npotScale = TextureImporterNPOTScale.None; ti.alphaIsTransparency = true; ti.wrapMode = TextureWrapMode.Clamp;
        }
    }
}
