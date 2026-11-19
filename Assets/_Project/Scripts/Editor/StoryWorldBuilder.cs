using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static Itan.EditorTools.GreyboxMaterials;

namespace Itan.EditorTools
{
    /// <summary>
    /// The story world, built in the same scene and hidden until the smoke
    /// transition. Everything sits in a half circle in front of the seat
    /// (player at the origin facing +Z), so nothing needs turning around for:
    ///   famine land   front-left   (~30° left)
    ///   rope spot     straight ahead
    ///   mother's home up in the sky, slightly right (~30° up)
    /// Menu: Itan > Scene > Add Story World
    /// </summary>
    public static class StoryWorldBuilder
    {
        public static readonly Vector3 FamineLand = new Vector3(-3.4f, 0f, 5.6f);
        public static readonly Vector3 RopeTop = new Vector3(0f, 5.0f, 7f);
        public static readonly Vector3 RopeBottom = new Vector3(0f, 0.15f, 7f);
        public static readonly Vector3 SkyHome = new Vector3(1.3f, 5.0f, 7.8f);

        [MenuItem("Itan/Scene/Add Story World")]
        public static void Build()
        {
            if (StoryScenes.StorySceneExists) { Debug.Log("[Itan] Story world already has its own scene."); return; }
            if (GameObject.Find("StoryWorld") != null || FindInactive("StoryWorld") != null)
            {
                Debug.Log("[Itan] Story world already added.");
                return;
            }

            MovePlayerAnchorsOutOfHub();

            var world = new GameObject("StoryWorld").transform;

            // Dry, cracked earth all around.
            var dryEarth = Get("Story_DryEarth", new Color(0.55f, 0.41f, 0.24f));
            Part("Ground", PrimitiveType.Cylinder, world, new Vector3(0f, -0.02f, 4f), new Vector3(30f, 0.02f, 30f), dryEarth, default, true);

            BuildFamineLand(world);
            BuildRopeSpot(world);
            BuildSkyHome(world);

            foreach (Transform t in world.GetComponentsInChildren<Transform>(true))
                if (t.name != "RopePivot" && t.name != "Rope")
                    GameObjectUtility.SetStaticEditorFlags(t.gameObject, (StaticEditorFlags)~0);

            world.gameObject.SetActive(false); // shown by the smoke transition
            Save();
            Debug.Log("[Itan] Story world added (hidden).");
        }

        static void BuildFamineLand(Transform world)
        {
            var land = Group("FamineLand", world, FamineLand);
            var deadWood = Get("Story_DeadWood", new Color(0.35f, 0.3f, 0.26f));
            var crack = Get("Story_Crack", new Color(0.22f, 0.16f, 0.1f));
            var clay = Get("Story_Clay", new Color(0.62f, 0.33f, 0.2f));

            // Bare, leafless trees.
            Vector3[] trees = { new Vector3(-1.6f, 0f, 1.2f), new Vector3(0.9f, 0f, 2.0f), new Vector3(-2.6f, 0f, -0.6f) };
            for (int i = 0; i < trees.Length; i++)
            {
                var tree = Group("DeadTree_" + i, land, trees[i]);
                Part("Trunk", PrimitiveType.Cylinder, tree, new Vector3(0f, 1.1f, 0f), new Vector3(0.18f, 1.1f, 0.18f), deadWood);
                Part("BranchA", PrimitiveType.Cylinder, tree, new Vector3(0.3f, 1.9f, 0f), new Vector3(0.07f, 0.5f, 0.07f), deadWood, new Vector3(0f, 0f, -40f));
                Part("BranchB", PrimitiveType.Cylinder, tree, new Vector3(-0.25f, 1.6f, 0.1f), new Vector3(0.06f, 0.4f, 0.06f), deadWood, new Vector3(20f, 0f, 45f));
            }

            // Cracks in the ground.
            for (int i = 0; i < 9; i++)
            {
                float a = i * 40f;
                var p = Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0.01f, 0.6f + (i % 3) * 0.7f);
                Part("Crack_" + i, PrimitiveType.Cube, land, p, new Vector3(0.05f, 0.01f, 0.9f), crack, new Vector3(0f, a + 25f, 0f));
            }

            // Empty pots.
            Vector3[] pots = { new Vector3(0.6f, 0.18f, -0.4f), new Vector3(1.1f, 0.15f, -0.1f), new Vector3(0.2f, 0.16f, 0.3f) };
            for (int i = 0; i < pots.Length; i++)
            {
                var pot = Part("EmptyPot_" + i, PrimitiveType.Sphere, land, pots[i], new Vector3(0.36f, 0.32f, 0.36f), clay);
                Part("Rim", PrimitiveType.Cylinder, pot, new Vector3(0f, 0.5f, 0f), new Vector3(0.6f, 0.08f, 0.6f), clay);
            }
        }

        static void BuildRopeSpot(Transform world)
        {
            var spot = Group("RopeSpot", world, new Vector3(RopeBottom.x, 0f, RopeBottom.z));
            var stone = Get("Greybox_Stone", new Color(0.4f, 0.4f, 0.42f));
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * 2f / 8f;
                Part("MarkerStone_" + i, PrimitiveType.Sphere, spot, new Vector3(Mathf.Cos(a) * 0.7f, 0.05f, Mathf.Sin(a) * 0.7f),
                     new Vector3(0.18f, 0.1f, 0.15f), stone);
            }

            // Rope hangs from a pivot at the top so scaling it makes it "drop" down.
            var rope = Get("Story_Rope", new Color(0.78f, 0.66f, 0.42f));
            var pivot = Group("RopePivot", world, RopeTop);
            float length = RopeTop.y - RopeBottom.y;
            var r = Part("Rope", PrimitiveType.Cylinder, pivot, new Vector3(0f, -length / 2f, 0f), new Vector3(0.05f, length / 2f, 0.05f), rope);
            r.gameObject.SetActive(true);
        }

        static void BuildSkyHome(Transform world)
        {
            var home = Group("SkyHome", world, SkyHome);
            var cloud = Get("Story_Cloud", new Color(0.78f, 0.82f, 0.95f), unlit: true);
            var mud = Get("Greybox_Mud", new Color(0.55f, 0.38f, 0.26f));
            var thatch = Get("Greybox_Thatch", new Color(0.62f, 0.49f, 0.27f));

            // Cloud platform that the rope hangs from.
            Vector3[] puffs = { new Vector3(0f, 0f, 0f), new Vector3(-1.2f, -0.1f, -0.5f), new Vector3(1.1f, -0.05f, 0.2f),
                                new Vector3(-0.3f, -0.15f, 1.0f), new Vector3(0.5f, -0.1f, -0.9f) };
            for (int i = 0; i < puffs.Length; i++)
                Part("Cloud_" + i, PrimitiveType.Sphere, home, puffs[i], new Vector3(2.4f, 0.6f, 1.8f), cloud);

            // Mother's round hut.
            var hut = Group("Hut", home, new Vector3(0.4f, 0.25f, 0.4f));
            Part("Walls", PrimitiveType.Cylinder, hut, new Vector3(0f, 0.45f, 0f), new Vector3(1.1f, 0.45f, 1.1f), mud);
            Part("Roof", PrimitiveType.Sphere, hut, new Vector3(0f, 1.05f, 0f), new Vector3(1.5f, 0.7f, 1.5f), thatch);
            Part("Door", PrimitiveType.Cube, hut, new Vector3(0f, 0.35f, -0.55f), new Vector3(0.35f, 0.6f, 0.05f),
                 Get("Story_Crack", new Color(0.22f, 0.16f, 0.1f)));
        }

        // Lap and eye points must stay visible/usable when the hub is hidden.
        static void MovePlayerAnchorsOutOfHub()
        {
            var anchors = GameObject.Find("PlayerAnchors");
            if (anchors == null) anchors = new GameObject("PlayerAnchors");
            foreach (var name in new[] { "LapPoint", "SeatEye" })
            {
                var t = FindInactive(name);
                if (t != null) t.transform.SetParent(anchors.transform, true);
            }
        }

        static GameObject FindInactive(string name)
        {
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name == name) return t.gameObject;
            return null;
        }

        static void Save()
        {
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
