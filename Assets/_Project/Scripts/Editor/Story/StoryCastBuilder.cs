using Itan.World;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>
    /// Puts the real characters into the story world in place of the greybox animals:
    /// Ajá and his mother (Polygonal Mind's Wolfman, repainted as dogs in Yoruba clothes)
    /// and Ìjàpá (Polygonal Mind's Cool Turtle, repainted as a tortoise, plus a cracked copy).
    /// Menu: Itan > Story > Build Story Cast
    /// </summary>
    public static class StoryCastBuilder
    {
        const string CharArt = "Assets/_Project/Art/Characters";
        const string AnimDir = "Assets/_Project/Art/Animation";
        public const string ControllerPath = AnimDir + "/StoryCharacter.controller";

        [MenuItem("Itan/Story/Build Story Cast")]
        public static void Build()
        {
            StoryScenes.OpenStory();
            var aja = StoryScenes.Find("Aja")?.transform;
            var mother = StoryScenes.Find("AjaMother")?.transform;
            var ijapa = StoryScenes.Find("Ijapa")?.transform;
            if (aja == null || mother == null || ijapa == null) { Debug.LogError("[Itan] Story characters not found."); return; }

            var controller = EnsureController();

            // Ajá: brown dog in an indigo adire dàńṣíkí, with a fila.
            var ajaModel = Replace(aja, "Wolfman", Mat("Char_Aja", "Aja_Dress"), 1.35f, controller);
            AddFila(ajaModel);

            // Ìyá: golden dog in aso-oke, with a gèlè. She waits by the feast on the cloud.
            var motherModel = Replace(mother, "Wolfman", Mat("Char_Iya", "Iya_Dress"), 1.45f, controller);
            AddGele(motherModel);
            mother.position = StoryWorldDresser.MotherSeat;
            mother.rotation = FacingFeast(StoryWorldDresser.MotherSeat);

            // Ìjàpá: two copies, whole and cracked, swapped by the fall.
            var old = ijapa.Find("Model");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            var holder = new GameObject("Model").transform;
            holder.SetParent(ijapa, false);
            var whole = Spawn("CoolTurtle", holder, Mat("Char_Ijapa", "Ijapa_Dress"), 1.2f, controller, "ShellWhole");
            var cracked = Spawn("CoolTurtle", holder, Mat("Char_IjapaCracked", "Ijapa_Cracked"), 1.2f, controller, "ShellCracked");
            cracked.gameObject.SetActive(false);

            foreach (var who in new[] { aja, mother, ijapa })
                if (who.GetComponent<CharacterActor>() == null) who.gameObject.AddComponent<CharacterActor>();

            // The fall swaps these two.
            var fall = Object.FindFirstObjectByType<FallMoment>(FindObjectsInactive.Include);
            if (fall != null)
            {
                var so = new SerializedObject(fall);
                so.FindProperty("shellWhole").objectReferenceValue = whole.gameObject;
                so.FindProperty("shellCracked").objectReferenceValue = cracked.gameObject;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // Ajá now arrives at his place by the feast.
            var arrival = StoryScenes.Find("AjaArrival");
            if (arrival != null) arrival.transform.position = StoryWorldDresser.AjaSeat;

            foreach (var t in new[] { aja, mother, ijapa })
                foreach (var c in t.GetComponentsInChildren<Transform>(true))
                    GameObjectUtility.SetStaticEditorFlags(c.gameObject, 0);

            StoryScenes.SaveAll();
            Debug.Log("[Itan] Story cast built.");
        }

        static Quaternion FacingFeast(Vector3 from)
        {
            var d = StoryWorldDresser.FeastCentre - from; d.y = 0f;
            // Turn halfway between the feast and the visitor so their faces can be seen.
            return Quaternion.Slerp(Quaternion.LookRotation(d), Quaternion.LookRotation(Vector3.back), 0.35f);
        }

        static Transform Replace(Transform root, string model, Material mat, float height, RuntimeAnimatorController controller)
        {
            var old = root.Find("Model");
            if (old != null) Object.DestroyImmediate(old.gameObject);
            return Spawn(model, root, mat, height, controller, "Model");
        }

        static Transform Spawn(string model, Transform parent, Material mat, float height, RuntimeAnimatorController controller, string name)
        {
            var prefab = StoryWorldDresser.Model(model);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.name = name;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            foreach (var r in go.GetComponentsInChildren<Renderer>())
            {
                var mats = r.sharedMaterials;
                for (int i = 0; i < mats.Length; i++) mats[i] = mat;
                r.sharedMaterials = mats;
                if (r is SkinnedMeshRenderer smr) smr.updateWhenOffscreen = false;
            }
            var b = StoryWorldDresser.Bounds(go.transform);
            if (b.size.y > 0.01f) go.transform.localScale *= height / b.size.y;

            if (!go.TryGetComponent<Animator>(out var animator)) animator = go.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            return go.transform;
        }

        static void AddFila(Transform model)
        {
            var head = model.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
            if (head == null) return;
            var cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            Object.DestroyImmediate(cap.GetComponent<Collider>());
            cap.name = "Fila";
            cap.SetParent(head, false);
            float s = 1f / head.lossyScale.x;
            cap.localPosition = new Vector3(0f, 0.2f, -0.02f) * s;
            cap.localRotation = Quaternion.Euler(-8f, 0f, 14f);
            cap.localScale = new Vector3(0.17f, 0.045f, 0.17f) * s;
            cap.GetComponent<Renderer>().sharedMaterial = StoryWorldDresser.Flat("Char_Fila", new Color(0.13f, 0.16f, 0.4f), 0.2f);
        }

        static void AddGele(Transform model)
        {
            var head = model.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
            if (head == null) return;
            float s = 1f / head.lossyScale.x;
            var gele = new GameObject("Gele").transform;
            gele.SetParent(head, false);
            gele.localPosition = new Vector3(0f, 0.2f, -0.03f) * s;
            var mat = StoryWorldDresser.Flat("Char_Gele", new Color(0.86f, 0.42f, 0.14f), 0.35f);
            // Wrapped base and a fanned crest, the way a gèlè is tied for a celebration.
            var wrap = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            wrap.name = "Wrap"; wrap.SetParent(gele, false);
            wrap.localScale = new Vector3(0.26f, 0.14f, 0.24f) * s;
            for (int i = 0; i < 3; i++)
            {
                var fan = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
                fan.name = "Fan_" + i; fan.SetParent(gele, false);
                fan.localPosition = new Vector3((i - 1) * 0.05f, 0.07f, -0.03f) * s;
                fan.localRotation = Quaternion.Euler(-20f, 0f, (i - 1) * 28f);
                fan.localScale = new Vector3(0.05f, 0.2f, 0.16f) * s;
            }
            foreach (var r in gele.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
            foreach (var c in gele.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
        }

        static Material Mat(string name, string texture)
        {
            var path = $"Assets/_Project/Art/Materials/Story/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(m, path); }
            m.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>($"{CharArt}/{texture}.png"));
            m.SetColor("_BaseColor", Color.white);
            m.SetFloat("_Smoothness", 0.12f);
            EditorUtility.SetDirty(m);
            return m;
        }

        // ---------------- Animation ----------------

        /// <summary>
        /// One controller for every story character. Until the Mixamo clips are in, Idle is a
        /// generated "arms relaxed" pose so nobody stands in a T-pose.
        /// </summary>
        public static AnimatorController EnsureController()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            if (controller != null) return controller;
            controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var idle = controller.layers[0].stateMachine.AddState(CharacterActor.Idle);
            idle.motion = RelaxedPose();
            controller.layers[0].stateMachine.defaultState = idle;
            return controller;
        }

        static AnimationClip RelaxedPose()
        {
            var path = AnimDir + "/Pose_Relaxed.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip != null) return clip;
            clip = new AnimationClip { name = "Pose_Relaxed" };
            void M(string muscle, float v) => clip.SetCurve("", typeof(Animator), muscle, AnimationCurve.Constant(0f, 1f, v));
            foreach (var side in new[] { "Left", "Right" })
            {
                M($"{side} Arm Down-Up", -0.65f);
                M($"{side} Arm Front-Back", 0.15f);
                M($"{side} Forearm Stretch", 0.85f);
                M($"{side} Upper Leg In-Out", 0f);
                M($"{side} Lower Leg Stretch", 1f);
                M($"{side} Upper Leg Front-Back", 0f);
            }
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }
    }
}
