using System.IO;
using UnityEditor;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>Helpers for the downloaded Mixamo characters (kept in _ThirdParty, not in git).</summary>
    public static class MixamoTools
    {
        public const string CharDir = "Assets/_ThirdParty/Mixamo/Characters";

        [MenuItem("Itan/Characters/Reimport Mixamo")]
        public static void Reimport()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { "Assets/_ThirdParty/Mixamo" }))
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                var m = (ModelImporter)AssetImporter.GetAtPath(p);
                m.userData = "";
                m.SaveAndReimport();
            }
        }

        [MenuItem("Itan/Characters/Extract Mixamo Textures")]
        public static void ExtractTextures()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { CharDir }))
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                var dir = $"{CharDir}/{Path.GetFileNameWithoutExtension(p)}_Tex";
                if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder(CharDir, Path.GetFileName(dir));
                ((ModelImporter)AssetImporter.GetAtPath(p)).ExtractTextures(dir);
            }
            AssetDatabase.Refresh();
            Reimport();
        }

        /// <summary>Renders each character model to Logs/char_NAME.png so they can be checked without the editor.</summary>
        [MenuItem("Itan/Characters/Capture Character Previews")]
        public static void CapturePreviews()
        {
            var origin = new Vector3(500, 0, 500);
            var camGo = new GameObject("PreviewCam");
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.55f, 0.58f);
            cam.fieldOfView = 30;
            cam.transform.position = origin + new Vector3(0, 1.0f, 4.2f);
            cam.transform.LookAt(origin + Vector3.up * 0.95f);
            var lightGo = new GameObject("PreviewLight");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            lightGo.transform.rotation = Quaternion.Euler(30, 200, 0);
            var rt = new RenderTexture(600, 900, 24);
            cam.targetTexture = rt;
            try
            {
                foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { CharDir, "Assets/_ThirdParty/PolygonalMind" }))
                {
                    var p = AssetDatabase.GUIDToAssetPath(guid);
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                    var go = (GameObject)Object.Instantiate(prefab, origin, Quaternion.identity);
                    // Fit the camera to the model's height.
                    var bounds = new Bounds(origin, Vector3.zero);
                    foreach (var r in go.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds);
                    cam.transform.position = bounds.center + new Vector3(0, 0, bounds.size.y * 2.1f + 0.5f);
                    cam.transform.LookAt(bounds.center);
                    // Polygonal Mind ships its texture beside the FBX; show it on the preview.
                    var png = System.IO.Path.ChangeExtension(p, ".png");
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(png);
                    if (tex != null)
                        foreach (var r in go.GetComponentsInChildren<Renderer>())
                        {
                            var m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                            m.SetTexture("_BaseMap", tex);
                            r.sharedMaterials = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Repeat(m, r.sharedMaterials.Length));
                        }
                    cam.Render();
                    RenderTexture.active = rt;
                    var shot = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
                    shot.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                    File.WriteAllBytes($"Logs/char_{Path.GetFileNameWithoutExtension(p)}.png", shot.EncodeToPNG());
                    RenderTexture.active = null;
                    Object.DestroyImmediate(shot);
                    Object.DestroyImmediate(go);
                }
            }
            finally
            {
                cam.targetTexture = null;
                Object.DestroyImmediate(rt);
                Object.DestroyImmediate(camGo);
                Object.DestroyImmediate(lightGo);
            }
        }
    }
}
