using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Itan.EditorTools
{
    /// <summary>
    /// Evening look: the sun is setting low in front-left, the sky glows orange
    /// near the horizon and deepens to indigo overhead, a warm haze softens the
    /// distance, and a pale moon is rising on the other side.
    /// Menu: Itan > Scene > Apply Sunset Lighting
    /// </summary>
    public static class SunsetLighting
    {
        const string SkyPath = "Assets/_Project/Art/Materials/Sky_Sunset.mat";

        // Sun about 7 degrees above the horizon, front-left (west), so faces are lit from the side.
        static readonly Vector3 SunEuler = new Vector3(7f, -35f, 0f);

        [MenuItem("Itan/Scene/Apply Sunset Lighting")]
        public static void Apply()
        {
            ApplyEnvironment();
            ApplySceneObjects();
            DynamicGI.UpdateEnvironment();
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Itan] Sunset lighting applied.");
        }

        /// <summary>Sky, ambient light and fog. These are saved per scene, so the story scene gets them too.</summary>
        public static void ApplyEnvironment()
        {
            // Sky: custom indigo-to-orange gradient with a glow around the sun.
            var skyShader = Shader.Find("Itan/SunsetSky");
            var sky = AssetDatabase.LoadAssetAtPath<Material>(SkyPath);
            if (sky == null) { sky = new Material(skyShader); AssetDatabase.CreateAsset(sky, SkyPath); }
            else sky.shader = skyShader;
            RenderSettings.skybox = sky;

            // Soft three-tone ambient so shadowed sides stay readable.
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.52f, 0.48f, 0.66f);
            RenderSettings.ambientEquatorColor = new Color(0.62f, 0.44f, 0.36f);
            RenderSettings.ambientGroundColor = new Color(0.2f, 0.15f, 0.12f);

            // Warm haze in the distance.
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.86f, 0.56f, 0.42f);
            RenderSettings.fogStartDistance = 14f;
            RenderSettings.fogEndDistance = 70f;
        }

        // The hub's sun, moon, fire and camera.
        static void ApplySceneObjects()
        {
            // Sun: the old moonlight becomes the setting sun.
            var sunGo = GameObject.Find("Moonlight") ?? GameObject.Find("Sun");
            var sun = sunGo.GetComponent<Light>();
            sunGo.name = "Sun";
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.64f, 0.4f);
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = 0.75f;
            sunGo.transform.rotation = Quaternion.Euler(SunEuler);
            RenderSettings.sun = sun;
            // The sky draws the sun where the light comes from.
            Shader.SetGlobalVector("_ItanSunDirection", -sunGo.transform.forward);
            if (sunGo.GetComponent<Itan.World.SkySun>() == null) sunGo.AddComponent<Itan.World.SkySun>();

            // The camera now shows the sky instead of a flat colour.
            var origin = Object.FindFirstObjectByType<XROrigin>();
            if (origin != null)
            {
                origin.Camera.clearFlags = CameraClearFlags.Skybox;
                EditorUtility.SetDirty(origin.Camera);
            }

            // A pale moon rising low on the other side (east), smaller than before.
            var moon = GameObject.Find("Moon");
            if (moon != null)
            {
                moon.transform.position = new Vector3(26f, 14f, 48f);
                moon.transform.localScale = Vector3.one * 4.5f;
                var mat = moon.GetComponent<Renderer>().sharedMaterial;
                mat.SetColor("_BaseColor", new Color(0.95f, 0.92f, 0.88f, 1f));
                EditorUtility.SetDirty(mat);
            }

            // The fire stays the warmest light in the compound.
            var fire = GameObject.Find("FireLight");
            if (fire != null)
            {
                var l = fire.GetComponent<Light>();
                l.intensity = 2.2f;
                l.range = 7f;
            }
        }
    }
}
