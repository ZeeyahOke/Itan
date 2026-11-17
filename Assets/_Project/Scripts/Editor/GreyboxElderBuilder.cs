using Itan.Characters;
using Itan.Narration;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>
    /// A seated greybox elder made of primitives, with simple Idle (breathing)
    /// and Talk (nodding, one hand gesturing) clips. Replace the body with the
    /// Mixamo model later and drop its clips into the same Idle/Talk states.
    /// Menu: Itan > Scene > Add Greybox Elder
    /// </summary>
    public static class GreyboxElderBuilder
    {
        const string AnimFolder = "Assets/_Project/Art/Animation";
        const string MaterialFolder = "Assets/_Project/Art/Materials";

        [MenuItem("Itan/Scene/Add Greybox Elder")]
        public static void Build()
        {
            var elder = GameObject.Find("Elder");
            var narration = Object.FindFirstObjectByType<NarrationManager>();
            if (elder == null || narration == null) { Debug.LogError("[Itan] Add Narration first (it creates the Elder)."); return; }
            if (elder.transform.Find("Body") != null) { Debug.Log("[Itan] Elder already has a body."); return; }

            Material robe = Mat("Greybox_Robe", new Color(0.86f, 0.8f, 0.68f));
            Material skin = Mat("Greybox_Skin", new Color(0.33f, 0.21f, 0.14f));
            Material cap = Mat("Greybox_Fila", new Color(0.16f, 0.2f, 0.5f));

            // Local space: +Z faces the player. The stool top is at y 0.4, z -0.15.
            var body = Part("Body", PrimitiveType.Capsule, elder.transform, new Vector3(0f, 0.75f, -0.15f), new Vector3(0.5f, 0.36f, 0.38f), robe);
            var head = Part("Head", PrimitiveType.Sphere, body, new Vector3(0f, 1.3f, 0f), new Vector3(0.48f, 0.64f, 0.6f), skin);
            Part("Fila", PrimitiveType.Cylinder, head, new Vector3(0f, 0.45f, -0.05f), new Vector3(0.95f, 0.22f, 0.95f), cap);

            Transform armL = Pivot("ArmL", body, new Vector3(-0.55f, 0.55f, 0f));
            Transform armR = Pivot("ArmR", body, new Vector3(0.55f, 0.55f, 0f));
            Part("Sleeve", PrimitiveType.Capsule, armL, new Vector3(0f, -0.55f, 0.25f), new Vector3(0.28f, 0.6f, 0.28f), robe).localRotation = Quaternion.Euler(35f, 0f, 0f);
            Part("Sleeve", PrimitiveType.Capsule, armR, new Vector3(0f, -0.55f, 0.25f), new Vector3(0.28f, 0.6f, 0.28f), robe).localRotation = Quaternion.Euler(35f, 0f, 0f);

            // Seated legs: thighs forward from the stool, shins down to the mat.
            Part("ThighL", PrimitiveType.Cube, elder.transform, new Vector3(-0.12f, 0.45f, 0.08f), new Vector3(0.16f, 0.14f, 0.45f), robe);
            Part("ThighR", PrimitiveType.Cube, elder.transform, new Vector3(0.12f, 0.45f, 0.08f), new Vector3(0.16f, 0.14f, 0.45f), robe);
            Part("ShinL", PrimitiveType.Cube, elder.transform, new Vector3(-0.12f, 0.22f, 0.3f), new Vector3(0.13f, 0.44f, 0.13f), skin);
            Part("ShinR", PrimitiveType.Cube, elder.transform, new Vector3(0.12f, 0.22f, 0.3f), new Vector3(0.13f, 0.44f, 0.13f), skin);

            // Animator with Idle and Talk, switched by the "Talking" bool.
            var controller = BuildController(body, head, armR);
            var animator = elder.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            var elderAnim = elder.AddComponent<ElderAnimator>();
            var so = new SerializedObject(elderAnim);
            so.FindProperty("narration").objectReferenceValue = narration;
            so.ApplyModifiedPropertiesWithoutUndo();

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Itan] Greybox elder added.");
        }

        static AnimatorController BuildController(Transform body, Transform head, Transform armR)
        {
            string bodyPath = "Body";
            string headPath = "Body/Head";
            string armPath = "Body/ArmR";

            // Idle: slow breathing and a slight head drift, 4 s loop.
            var idle = new AnimationClip { name = "Elder_Idle" };
            Curve(idle, bodyPath, "localScale.y", 0.36f, (0f, 0f), (2f, 0.012f), (4f, 0f));
            Curve(idle, headPath, "localEulerAnglesRaw.z", 0f, (0f, 0f), (2f, 3f), (4f, 0f));
            Loop(idle);
            AssetDatabase.CreateAsset(idle, AnimFolder + "/Elder_Idle.anim");

            // Talk: nodding head and a raised, waving right hand, 1.2 s loop.
            var talk = new AnimationClip { name = "Elder_Talk" };
            Curve(talk, headPath, "localEulerAnglesRaw.x", 0f, (0f, 0f), (0.3f, 8f), (0.6f, 0f), (0.9f, 5f), (1.2f, 0f));
            Curve(talk, armPath, "localEulerAnglesRaw.x", 0f, (0f, -40f), (0.6f, -60f), (1.2f, -40f));
            Curve(talk, armPath, "localEulerAnglesRaw.z", 0f, (0f, 10f), (0.6f, 25f), (1.2f, 10f));
            Loop(talk);
            AssetDatabase.CreateAsset(talk, AnimFolder + "/Elder_Talk.anim");

            var controller = AnimatorController.CreateAnimatorControllerAtPath(AnimFolder + "/Elder.controller");
            controller.AddParameter("Talking", AnimatorControllerParameterType.Bool);
            var sm = controller.layers[0].stateMachine;
            var idleState = sm.AddState("Idle");
            idleState.motion = idle;
            var talkState = sm.AddState("Talk");
            talkState.motion = talk;
            sm.defaultState = idleState;

            var toTalk = idleState.AddTransition(talkState);
            toTalk.AddCondition(AnimatorConditionMode.If, 0, "Talking");
            toTalk.hasExitTime = false;
            toTalk.duration = 0.2f;

            var toIdle = talkState.AddTransition(idleState);
            toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "Talking");
            toIdle.hasExitTime = false;
            toIdle.duration = 0.3f;

            AssetDatabase.SaveAssets();
            return controller;
        }

        // Adds a curve whose keys are offsets from a base value.
        static void Curve(AnimationClip clip, string path, string property, float baseValue, params (float t, float v)[] keys)
        {
            var curve = new AnimationCurve();
            foreach (var (t, v) in keys) curve.AddKey(t, baseValue + v);
            for (int i = 0; i < curve.length; i++) AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve(path, typeof(Transform), property), curve);
        }

        static void Loop(AnimationClip clip)
        {
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }

        static Transform Part(string name, PrimitiveType type, Transform parent, Vector3 localPos, Vector3 localScale, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            return go.transform;
        }

        static Transform Pivot(string name, Transform parent, Vector3 localPos)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.localPosition = localPos;
            return t;
        }

        static Material Mat(string name, Color color)
        {
            string path = MaterialFolder + "/" + name + ".mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetColor("_BaseColor", color);
            mat.SetFloat("_Smoothness", 0.1f);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }
    }
}
