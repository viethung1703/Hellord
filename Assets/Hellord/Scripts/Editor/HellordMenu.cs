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
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Hellord/")) return;
            var ti = (TextureImporter)assetImporter;
            ti.filterMode = FilterMode.Point; ti.textureCompression = TextureImporterCompression.Uncompressed; ti.mipmapEnabled = false;
            ti.npotScale = TextureImporterNPOTScale.None; ti.alphaIsTransparency = true; ti.wrapMode = TextureWrapMode.Clamp;
        }
    }
}
