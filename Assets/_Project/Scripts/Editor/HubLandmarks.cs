using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>
    /// Greybox blockouts for the big shapes that frame the view from the seat:
    /// the elder's tree, the moon, a packed-earth courtyard and thatch roof edges
    /// on the compound walls. Real art replaces these in task 4.7.
    /// Menu: Itan > Scene > Add Hub Landmarks
    /// </summary>
    public static class HubLandmarks
    {
        const string MaterialFolder = "Assets/_Project/Art/Materials";

        static readonly Vector3 FirePosition = new Vector3(0f, 0f, 2f);
        static readonly Vector3 TreePosition = new Vector3(0f, 0f, 5.4f);      // behind the elder's stool (z 3.4)
        static readonly Vector3 MoonPosition = new Vector3(14f, 30f, 60f);    // high, front-right, clear of the tree
        const float CourtyardRadius = 6.8f;

        [MenuItem("Itan/Scene/Add Hub Landmarks")]
        public static void Add()
        {
            const string hubPath = "Assets/_Project/Scenes/Hub.unity";
            if (EditorSceneManager.GetActiveScene().path != hubPath)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                EditorSceneManager.OpenScene(hubPath);
            }

            var hub = GameObject.Find("Hub");
            if (hub == null) { Debug.LogError("[Itan] No Hub object in the open scene."); return; }
            if (hub.transform.Find("Landmarks") != null) { Debug.Log("[Itan] Landmarks already added."); return; }

            var root = new GameObject("Landmarks").transform;
            root.SetParent(hub.transform);

            CreateCourtyard(root);
            CreateTree(root);
            CreateMoon(root);
            AddThatchToWalls(hub.transform);
            AimMoonlightFromMoon();

            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, (StaticEditorFlags)~0);

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Itan] Hub landmarks added.");
        }

        static void CreateCourtyard(Transform parent)
        {
            // Packed, swept earth inside the walls, a shade lighter than the ground outside.
            var yard = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            yard.name = "Courtyard";
            yard.transform.SetParent(parent);
            yard.transform.position = FirePosition + new Vector3(0f, 0.005f, 0f);
            yard.transform.localScale = new Vector3(CourtyardRadius * 2f, 0.005f, CourtyardRadius * 2f);
            Object.DestroyImmediate(yard.GetComponent<Collider>());
            Paint(yard, "Greybox_Courtyard", new Color(0.5f, 0.36f, 0.24f));
        }

        static void CreateTree(Transform parent)
        {
            var tree = new GameObject("ElderTree").transform;
            tree.SetParent(parent);
            tree.position = TreePosition;

            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.SetParent(tree, false);
            trunk.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            trunk.transform.localScale = new Vector3(1.3f, 2.5f, 1.3f); // 5 m tall
            Paint(trunk, "Greybox_Bark", new Color(0.24f, 0.17f, 0.12f));

            // Wide, spreading canopy so it reads as a big old shade tree.
            var canopy = new (Vector3 pos, Vector3 size)[]
            {
                (new Vector3(0f, 6.2f, 0f),     new Vector3(7f, 3.2f, 6f)),
                (new Vector3(-2.6f, 5.6f, 0.4f), new Vector3(4.5f, 2.6f, 4f)),
                (new Vector3(2.7f, 5.8f, -0.3f), new Vector3(4.8f, 2.8f, 4.2f)),
                (new Vector3(0.4f, 7.4f, 0.8f), new Vector3(4.5f, 2.4f, 4f)),
            };
            for (int i = 0; i < canopy.Length; i++)
            {
                var leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leaves.name = "Canopy_" + i;
                leaves.transform.SetParent(tree, false);
                leaves.transform.localPosition = canopy[i].pos;
                leaves.transform.localScale = canopy[i].size;
                Paint(leaves, "Greybox_Leaves", new Color(0.12f, 0.2f, 0.14f));
            }
        }

        static void CreateMoon(Transform parent)
        {
            var moon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            moon.name = "Moon";
            moon.transform.SetParent(parent);
            moon.transform.position = MoonPosition;
            moon.transform.localScale = Vector3.one * 9f; // large in the sky
            Object.DestroyImmediate(moon.GetComponent<Collider>());

            var mat = GetMaterial("Greybox_Moon", new Color(0.92f, 0.93f, 1f), unlit: true);
            moon.GetComponent<Renderer>().sharedMaterial = mat;
            moon.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        static void AddThatchToWalls(Transform hub)
        {
            var walls = hub.Find("CompoundWalls");
            if (walls == null) return;

            // Thatch eaves on the walls to the left and right of the fire,
            // leaving the sky clear in front (moon) and behind (open gap).
            var straw = GetMaterial("Greybox_Thatch", new Color(0.62f, 0.49f, 0.27f), unlit: false);

            // Copy the wall list first: thatch is added under the same parent,
            // so looping over the live children would never end.
            var wallList = new System.Collections.Generic.List<Transform>();
            foreach (Transform child in walls)
                if (child.name.StartsWith("Wall_")) wallList.Add(child);

            foreach (Transform wall in wallList)
            {
                Vector3 fromFire = wall.position - FirePosition;
                float angle = Vector3.SignedAngle(Vector3.forward, new Vector3(fromFire.x, 0f, fromFire.z), Vector3.up);
                float side = Mathf.Abs(angle);
                if (side < 45f || side > 135f) continue;

                // Lift side walls to house height, then add a sloped roof edge leaning into the courtyard.
                Vector3 s = wall.localScale;
                wall.localScale = new Vector3(s.x, 2.6f, s.z);
                wall.position = new Vector3(wall.position.x, 1.3f, wall.position.z);

                var thatch = GameObject.CreatePrimitive(PrimitiveType.Cube);
                thatch.name = "Thatch_" + wall.name;
                thatch.transform.SetParent(walls);
                Vector3 inward = -new Vector3(fromFire.x, 0f, fromFire.z).normalized;
                thatch.transform.position = wall.position + Vector3.up * 1.45f + inward * 0.45f;
                thatch.transform.rotation = wall.rotation * Quaternion.Euler(-25f, 0f, 0f);
                thatch.transform.localScale = new Vector3(3.4f, 0.22f, 1.6f);
                thatch.GetComponent<Renderer>().sharedMaterial = straw;
                GameObjectUtility.SetStaticEditorFlags(thatch, (StaticEditorFlags)~0);
            }
        }

        static void AimMoonlightFromMoon()
        {
            var moonlight = GameObject.Find("Moonlight");
            if (moonlight == null) return;
            // Shine from the moon's direction so shadows fall away from it.
            Vector3 dir = (Vector3.zero - MoonPosition).normalized;
            moonlight.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
        }

        // ---------- Materials ----------

        static void Paint(GameObject go, string name, Color color)
        {
            go.GetComponent<Renderer>().sharedMaterial = GetMaterial(name, color, unlit: false);
        }

        static Material GetMaterial(string name, Color color, bool unlit)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;

            string shader = unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit";
            mat = new Material(Shader.Find(shader));
            mat.SetColor("_BaseColor", color);
            if (!unlit) mat.SetFloat("_Smoothness", 0.05f);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }
    }
}
