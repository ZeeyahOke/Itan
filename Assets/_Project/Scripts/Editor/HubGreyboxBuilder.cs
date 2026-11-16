using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Itan.EditorTools
{
    /// <summary>
    /// Builds the greybox hub scene: a seated XR rig facing a fire pit,
    /// with mats, stools and low compound walls. Everything is simple
    /// primitives so the layout can be checked before real art goes in.
    /// Menu: Itan > Build > Create Hub Greybox Scene
    ///
    /// Layout (metres, player at the origin facing +Z):
    ///   player mat and stool  (0, 0, 0)
    ///   fire pit              (0, 0, 2)
    ///   elder stool           (0, 0, 3.4), facing the player
    /// </summary>
    public static class HubGreyboxBuilder
    {
        const string ScenePath = "Assets/_Project/Scenes/Hub.unity";
        const string MaterialFolder = "Assets/_Project/Art/Materials";
        const string RigPrefabPath = "Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab";

        // Seated eye height used before the session recenters the view (task 2.1).
        const float SeatedEyeHeight = 1.15f;

        static readonly Vector3 FirePosition = new Vector3(0f, 0f, 2f);

        [MenuItem("Itan/Build/Create Hub Greybox Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            SetUpNightLighting();
            CreateSeatedRig();
            CreateUiEventSystem();

            var hub = new GameObject("Hub").transform;
            CreateGround(hub);
            CreateFirePit(hub);
            CreateMatsAndStools(hub);
            CreateCompoundWalls(hub);
            MarkStatic(hub.gameObject);

            EditorSceneManager.SaveScene(scene, ScenePath);
            SetAsOnlyBuildScene(ScenePath);
            Debug.Log("[Itan] Hub greybox scene created at " + ScenePath);
        }

        // ---------- Rig ----------

        static void CreateSeatedRig()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RigPrefabPath);
            if (prefab == null)
            {
                Debug.LogError("[Itan] XR Origin prefab not found at " + RigPrefabPath);
                return;
            }

            var rig = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            rig.name = "XR Origin (Seated)";
            rig.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            // Seated: track the head relative to where the headset starts,
            // and lift the camera to a sitting eye height.
            var origin = rig.GetComponent<XROrigin>();
            origin.RequestedTrackingOriginMode = XROrigin.TrackingOriginMode.Device;
            origin.CameraYOffset = SeatedEyeHeight;

            // Night sky: no skybox, just a deep indigo background.
            var cam = origin.Camera;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.03f, 0.03f, 0.09f);

            // The player stays on their stool: no walking, turning, teleporting or climbing.
            foreach (Transform t in rig.GetComponentsInChildren<Transform>(true))
            {
                string n = t.name;
                if (n == "Locomotion" || n.Contains("Teleport Interactor"))
                    t.gameObject.SetActive(false);
            }

            if (Object.FindFirstObjectByType<XRInteractionManager>() == null)
                new GameObject("XR Interaction Manager").AddComponent<XRInteractionManager>();
        }

        static void CreateUiEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<XRUIInputModule>();
        }

        // ---------- Lighting ----------

        static void SetUpNightLighting()
        {
            RenderSettings.skybox = null;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.10f, 0.11f, 0.24f); // indigo night

            var moon = new GameObject("Moonlight").AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = new Color(0.62f, 0.68f, 1f);
            moon.intensity = 0.35f;
            moon.shadows = LightShadows.Soft;
            moon.transform.rotation = Quaternion.Euler(40f, -30f, 0f);
        }

        // ---------- Hub geometry ----------

        static void CreateGround(Transform parent)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(parent);
            ground.transform.localScale = new Vector3(3f, 1f, 3f); // 30 x 30 m
            Paint(ground, "Greybox_Earth", new Color(0.36f, 0.24f, 0.16f));
        }

        static void CreateFirePit(Transform parent)
        {
            var pit = new GameObject("FirePit").transform;
            pit.SetParent(parent);
            pit.position = FirePosition;

            // Ring of stones.
            const int stones = 10;
            const float ringRadius = 0.55f;
            for (int i = 0; i < stones; i++)
            {
                float a = i * Mathf.PI * 2f / stones;
                var stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                stone.name = "Stone_" + i;
                stone.transform.SetParent(pit, false);
                stone.transform.localPosition = new Vector3(Mathf.Cos(a) * ringRadius, 0.06f, Mathf.Sin(a) * ringRadius);
                stone.transform.localScale = new Vector3(0.24f, 0.16f, 0.2f);
                Paint(stone, "Greybox_Stone", new Color(0.4f, 0.4f, 0.42f));
            }

            // Crossed logs.
            for (int i = 0; i < 3; i++)
            {
                var log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                log.name = "Log_" + i;
                log.transform.SetParent(pit, false);
                log.transform.localPosition = new Vector3(0f, 0.08f, 0f);
                log.transform.localRotation = Quaternion.Euler(90f, i * 60f, 0f);
                log.transform.localScale = new Vector3(0.1f, 0.3f, 0.1f);
                Paint(log, "Greybox_Wood", new Color(0.3f, 0.18f, 0.1f));
            }

            // Placeholder fire glow (real fire VFX comes in task 4.7).
            var glow = new GameObject("FireLight").AddComponent<Light>();
            glow.transform.SetParent(pit, false);
            glow.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            glow.type = LightType.Point;
            glow.color = new Color(1f, 0.55f, 0.2f);
            glow.intensity = 3f;
            glow.range = 8f;
        }

        static void CreateMatsAndStools(Transform parent)
        {
            var seating = new GameObject("Seating").transform;
            seating.SetParent(parent);

            // Player's spot at the origin, elder across the fire, and four guests around it.
            var spots = new List<(string name, Vector3 pos)>
            {
                ("Player", Vector3.zero),
                ("Elder", FirePosition + new Vector3(0f, 0f, 1.4f)),
                ("Guest_A", FirePosition + new Vector3(-1.4f, 0f, 0.3f)),
                ("Guest_B", FirePosition + new Vector3(1.4f, 0f, 0.3f)),
                ("Guest_C", FirePosition + new Vector3(-1.1f, 0f, -1.1f)),
                ("Guest_D", FirePosition + new Vector3(1.1f, 0f, -1.1f)),
            };

            foreach (var (name, pos) in spots)
            {
                var spot = new GameObject(name + "Spot").transform;
                spot.SetParent(seating);
                spot.position = pos;
                Vector3 toFire = FirePosition - pos;
                toFire.y = 0f;
                spot.rotation = Quaternion.LookRotation(toFire.normalized, Vector3.up);

                var mat = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mat.name = "Mat";
                mat.transform.SetParent(spot, false);
                mat.transform.localPosition = new Vector3(0f, 0.01f, 0f);
                mat.transform.localScale = new Vector3(1f, 0.02f, 1.2f);
                Paint(mat, "Greybox_Mat", new Color(0.72f, 0.58f, 0.32f));

                var stool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stool.name = "Stool";
                stool.transform.SetParent(spot, false);
                stool.transform.localPosition = new Vector3(0f, 0.2f, -0.15f);
                stool.transform.localScale = new Vector3(0.4f, 0.2f, 0.4f);
                Paint(stool, "Greybox_Wood", new Color(0.3f, 0.18f, 0.1f));
            }
        }

        static void CreateCompoundWalls(Transform parent)
        {
            var walls = new GameObject("CompoundWalls").transform;
            walls.SetParent(parent);

            // Low mud walls in a ring around the fire, with a gap behind the player.
            const int segments = 14;
            const float radius = 7f;
            for (int i = 0; i < segments; i++)
            {
                float angle = i * 360f / segments;
                if (angle > 160f && angle < 200f) continue; // gap behind the player
                Quaternion rot = Quaternion.Euler(0f, angle, 0f);
                Vector3 pos = FirePosition + rot * new Vector3(0f, 0f, radius);

                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = "Wall_" + i;
                wall.transform.SetParent(walls);
                wall.transform.position = pos + Vector3.up * 0.9f;
                wall.transform.rotation = rot;
                wall.transform.localScale = new Vector3(3.2f, 1.8f, 0.35f);
                Paint(wall, "Greybox_Mud", new Color(0.55f, 0.38f, 0.26f));
            }
        }

        // ---------- Helpers ----------

        static readonly Dictionary<string, Material> MaterialCache = new Dictionary<string, Material>();

        static void Paint(GameObject go, string materialName, Color color)
        {
            go.GetComponent<Renderer>().sharedMaterial = GetMaterial(materialName, color);
        }

        static Material GetMaterial(string name, Color color)
        {
            if (MaterialCache.TryGetValue(name, out var cached) && cached != null) return cached;

            string path = MaterialFolder + "/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                if (!AssetDatabase.IsValidFolder(MaterialFolder))
                    AssetDatabase.CreateFolder("Assets/_Project/Art", "Materials");
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.SetColor("_BaseColor", color);
                mat.SetFloat("_Smoothness", 0.1f);
                AssetDatabase.CreateAsset(mat, path);
            }
            MaterialCache[name] = mat;
            return mat;
        }

        static void MarkStatic(GameObject root)
        {
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.GetComponent<Light>() != null) continue;
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, (StaticEditorFlags)~0);
            }
        }

        static void SetAsOnlyBuildScene(string path)
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
        }
    }
}
