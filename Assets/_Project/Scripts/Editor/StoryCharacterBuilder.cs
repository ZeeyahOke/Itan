using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using static Itan.EditorTools.GreyboxMaterials;

namespace Itan.EditorTools
{
    /// <summary>
    /// Greybox story characters, sized up so they read clearly from the seat:
    /// Ajá the dog, Ìjàpá the tortoise (whole shell, plus a hidden cracked shell
    /// for the fall), and Ajá's mother up in her sky home.
    /// Menu: Itan > Scene > Add Story Characters
    /// </summary>
    public static class StoryCharacterBuilder
    {
        public static readonly Vector3 AjaStart = new Vector3(-1.4f, 0f, 6.3f);
        public static readonly Vector3 IjapaStart = new Vector3(-2.5f, 0f, 5.3f);
        public static readonly Vector3 MotherSpot = new Vector3(0.9f, 5.25f, 7.2f);

        [MenuItem("Itan/Scene/Add Story Characters")]
        public static void Build()
        {
            Transform world = null;
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name == "StoryWorld") world = t;
            if (world == null) { Debug.LogError("[Itan] Add the story world first."); return; }
            if (world.Find("Characters") != null) { Debug.Log("[Itan] Story characters already added."); return; }

            var cast = Group("Characters", world, Vector3.zero);

            var aja = Dog("Aja", cast, Get("Char_AjaFur", new Color(0.55f, 0.33f, 0.16f)), 1.3f);
            Place(aja, AjaStart, new Vector3(StoryWorldBuilder.RopeBottom.x, 0f, StoryWorldBuilder.RopeBottom.z));

            var mother = Dog("AjaMother", cast, Get("Char_MotherFur", new Color(0.82f, 0.7f, 0.52f)), 1.5f);
            mother.position = MotherSpot;
            mother.rotation = Quaternion.LookRotation(Vector3.back); // looks down toward the visitor

            var ijapa = Tortoise(cast);
            Place(ijapa, IjapaStart, AjaStart);

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Itan] Story characters added.");
        }

        static void Place(Transform character, Vector3 position, Vector3 lookAt)
        {
            character.position = position;
            Vector3 dir = lookAt - position;
            dir.y = 0f;
            // Turn three quarters toward the target so the visitor still sees the side of the face.
            character.rotation = Quaternion.Slerp(Quaternion.LookRotation(Vector3.back), Quaternion.LookRotation(dir), 0.75f);
        }

        // Local +Z is the front of the animal.
        static Transform Dog(string name, Transform parent, Material fur, float size)
        {
            var root = Group(name, parent, Vector3.zero);
            var model = Group("Model", root, Vector3.zero);
            model.localScale = Vector3.one * size;
            var dark = Get("Char_DarkNose", new Color(0.08f, 0.06f, 0.05f));

            Part("Body", PrimitiveType.Capsule, model, new Vector3(0f, 0.42f, 0f), new Vector3(0.3f, 0.32f, 0.3f), fur, new Vector3(90f, 0f, 0f));
            var head = Group("Head", model, new Vector3(0f, 0.62f, 0.36f));
            Part("Skull", PrimitiveType.Sphere, head, Vector3.zero, new Vector3(0.22f, 0.2f, 0.24f), fur);
            Part("Snout", PrimitiveType.Cube, head, new Vector3(0f, -0.03f, 0.14f), new Vector3(0.1f, 0.08f, 0.14f), fur);
            Part("Nose", PrimitiveType.Sphere, head, new Vector3(0f, -0.01f, 0.21f), new Vector3(0.05f, 0.04f, 0.04f), dark);
            Part("EarL", PrimitiveType.Cube, head, new Vector3(-0.08f, 0.12f, -0.02f), new Vector3(0.05f, 0.1f, 0.03f), fur, new Vector3(0f, 0f, 15f));
            Part("EarR", PrimitiveType.Cube, head, new Vector3(0.08f, 0.12f, -0.02f), new Vector3(0.05f, 0.1f, 0.03f), fur, new Vector3(0f, 0f, -15f));
            foreach (var (n, x, z) in new[] { ("LegFL", -0.09f, 0.2f), ("LegFR", 0.09f, 0.2f), ("LegBL", -0.09f, -0.2f), ("LegBR", 0.09f, -0.2f) })
                Part(n, PrimitiveType.Cylinder, model, new Vector3(x, 0.15f, z), new Vector3(0.07f, 0.15f, 0.07f), fur);
            Part("Tail", PrimitiveType.Cylinder, model, new Vector3(0f, 0.55f, -0.36f), new Vector3(0.04f, 0.12f, 0.04f), fur, new Vector3(-50f, 0f, 0f));
            return root;
        }

        static Transform Tortoise(Transform parent)
        {
            var root = Group("Ijapa", parent, Vector3.zero);
            var model = Group("Model", root, Vector3.zero);
            model.localScale = Vector3.one * 1.4f;
            var skin = Get("Char_IjapaSkin", new Color(0.45f, 0.42f, 0.3f));
            var shell = Get("Char_IjapaShell", new Color(0.32f, 0.26f, 0.14f));
            var cracked = Get("Char_IjapaShellCracked", new Color(0.26f, 0.2f, 0.11f));
            var crackLine = Get("Story_Crack", new Color(0.22f, 0.16f, 0.1f));

            var head = Group("Head", model, new Vector3(0f, 0.18f, 0.36f));
            Part("Skull", PrimitiveType.Sphere, head, Vector3.zero, new Vector3(0.13f, 0.12f, 0.16f), skin);
            Part("Neck", PrimitiveType.Cylinder, head, new Vector3(0f, -0.02f, -0.08f), new Vector3(0.08f, 0.06f, 0.08f), skin, new Vector3(70f, 0f, 0f));
            foreach (var (n, x, z) in new[] { ("LegFL", -0.17f, 0.17f), ("LegFR", 0.17f, 0.17f), ("LegBL", -0.17f, -0.17f), ("LegBR", 0.17f, -0.17f) })
                Part(n, PrimitiveType.Cylinder, model, new Vector3(x, 0.06f, z), new Vector3(0.09f, 0.06f, 0.09f), skin);

            // Whole shell: one dome with a few raised plates.
            var whole = Group("ShellWhole", model, Vector3.zero);
            Part("Dome", PrimitiveType.Sphere, whole, new Vector3(0f, 0.2f, 0f), new Vector3(0.52f, 0.3f, 0.62f), shell);
            for (int i = 0; i < 5; i++)
            {
                var p = new Vector3((i % 2 == 0 ? -1 : 1) * 0.1f * (i / 2), 0.33f, -0.16f + i * 0.08f);
                Part("Plate_" + i, PrimitiveType.Sphere, whole, p, new Vector3(0.12f, 0.04f, 0.12f), cracked);
            }

            // Cracked shell: the same dome in pieces with gaps, shown after the fall.
            var broken = Group("ShellCracked", model, Vector3.zero);
            var pieces = new[]
            {
                (new Vector3(-0.1f, 0.2f, 0.12f), new Vector3(0f, 0f, 12f)),
                (new Vector3(0.11f, 0.19f, 0.1f), new Vector3(0f, 0f, -14f)),
                (new Vector3(-0.12f, 0.18f, -0.13f), new Vector3(8f, 0f, 10f)),
                (new Vector3(0.12f, 0.19f, -0.12f), new Vector3(-6f, 0f, -12f)),
                (new Vector3(0f, 0.26f, 0f), new Vector3(0f, 25f, 0f)),
            };
            for (int i = 0; i < pieces.Length; i++)
                Part("Piece_" + i, PrimitiveType.Sphere, broken, pieces[i].Item1, new Vector3(0.24f, 0.2f, 0.3f), cracked, pieces[i].Item2);
            for (int i = 0; i < 4; i++)
                Part("CrackLine_" + i, PrimitiveType.Cube, broken, new Vector3(0f, 0.33f, -0.18f + i * 0.12f),
                     new Vector3(0.4f, 0.015f, 0.015f), crackLine, new Vector3(0f, i * 45f, 0f));
            broken.gameObject.SetActive(false);

            return root;
        }
    }
}
