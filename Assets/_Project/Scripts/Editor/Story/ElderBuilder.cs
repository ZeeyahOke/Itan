using System.Linq;
using Itan.Characters;
using Itan.Narration;
using Itan.World;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using static Itan.EditorTools.StoryWorldDresser;

namespace Itan.EditorTools
{
    /// <summary>
    /// Replaces the greybox elder with a Mixamo character (default: David) in a cream
    /// aso-oke agbada and fila, seated on his carved stool with a walking staff. He sits
    /// quietly and gestures while he talks (Quaternius sitting clips, driven by narration).
    /// His repainted texture is kept beside the Mixamo files (not in git).
    /// Menu: Itan > Hub > Build Elder
    /// </summary>
    public static class ElderBuilder
    {
        /// <summary>Change this to swap the elder's body (any Mixamo FBX in _ThirdParty/Mixamo/Characters).</summary>
        public const string ModelName = "Elder_David";
        const string ControllerPath = "Assets/_Project/Art/Animation/ElderSeated.controller";

        [MenuItem("Itan/Hub/Build Elder")]
        public static void Build()
        {
            var elder = StoryScenes.Find("Elder");
            if (elder == null) { Debug.LogError("[Itan] No Elder in the open scene."); return; }
            var root = elder.transform;

            // Old greybox body and its animator go; the Voice stays (narration plays from it).
            foreach (Transform c in root.Cast<Transform>().ToArray())
                if (c.name != "Voice") Object.DestroyImmediate(c.gameObject);
            if (root.TryGetComponent<ElderAnimator>(out var oldDriver)) Object.DestroyImmediate(oldDriver);
            if (root.TryGetComponent<Animator>(out var oldAnim)) Object.DestroyImmediate(oldAnim);

            var prefab = Model(ModelName);
            if (prefab == null) return;
            var model = ((GameObject)PrefabUtility.InstantiatePrefab(prefab, root)).transform;
            model.name = "Model";
            model.localPosition = new Vector3(0f, 0f, 0.16f); // a little forward so he sits on the stool behind him
            model.localRotation = Quaternion.identity;

            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_ThirdParty/Mixamo/Characters/Elder_Dress.png"));
            mat.SetFloat("_Smoothness", 0.15f);
            mat = SaveMat("Char_Elder", mat);
            foreach (var r in model.GetComponentsInChildren<Renderer>())
                r.sharedMaterials = Enumerable.Repeat(mat, r.sharedMaterials.Length).ToArray();
            var b = StoryWorldDresser.Bounds(model);
            if (b.size.y > 0.01f) model.localScale *= 1.78f / b.size.y;

            if (!model.TryGetComponent<Animator>(out var animator)) animator = model.gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = Controller();
            animator.applyRootMotion = false;
            var driver = model.gameObject.AddComponent<ElderAnimator>();
            var so = new SerializedObject(driver);
            so.FindProperty("narration").objectReferenceValue = Object.FindFirstObjectByType<NarrationManager>();
            so.ApplyModifiedPropertiesWithoutUndo();

            Fila(animator);
            Staff(root);

            // Hide him with the rest of the fireside during the story.
            var transition = Object.FindFirstObjectByType<SceneTransition>();
            if (transition != null)
            {
                var tso = new SerializedObject(transition);
                var list = tso.FindProperty("hubObjects");
                for (int i = list.arraySize - 1; i >= 0; i--)
                    if (list.GetArrayElementAtIndex(i).objectReferenceValue == null) list.DeleteArrayElementAtIndex(i);
                foreach (Transform c in root)
                    if (c.name != "Voice")
                    {
                        list.arraySize++;
                        list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = c.gameObject;
                    }
                tso.ApplyModifiedPropertiesWithoutUndo();
            }

            EditorSceneManager.MarkSceneDirty(elder.scene);
            EditorSceneManager.SaveScene(elder.scene);
            Debug.Log("[Itan] Elder built from " + ModelName + ".");
        }

        static Material SaveMat(string name, Material m)
        {
            var path = $"Assets/_Project/Art/Materials/Story/{name}.mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing == null) { AssetDatabase.CreateAsset(m, path); return m; }
            existing.CopyPropertiesFromMaterial(m);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        static RuntimeAnimatorController Controller()
        {
            AssetDatabase.DeleteAsset(ControllerPath);
            var c = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            c.AddParameter("Talking", AnimatorControllerParameterType.Bool);
            var sm = c.layers[0].stateMachine;
            var idle = sm.AddState("Idle"); idle.motion = AnimationLibrarySetup.Clip("Rig|Sitting_Idle_Loop");
            var talk = sm.AddState("Talk"); talk.motion = AnimationLibrarySetup.Clip("Rig|Sitting_Talking_Loop");
            sm.defaultState = idle;
            var toTalk = idle.AddTransition(talk); toTalk.AddCondition(AnimatorConditionMode.If, 0, "Talking"); toTalk.hasExitTime = false; toTalk.duration = 0.4f;
            var toIdle = talk.AddTransition(idle); toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "Talking"); toIdle.hasExitTime = false; toIdle.duration = 0.6f;
            return c;
        }

        /// <summary>
        /// Fila resting on the crown (placed from the rig's head-top bone), then attached
        /// to the head bone so it moves with him.
        /// </summary>
        static void Fila(Animator animator)
        {
            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            if (head == null) return;
            // Mixamo rigs have a bone exactly at the top of the skull.
            Transform headTop = null;
            foreach (var t in animator.GetComponentsInChildren<Transform>())
                if (t.name.EndsWith("HeadTop_End")) { headTop = t; break; }
            Vector3 crown = headTop != null ? headTop.position
                                            : head.position + Vector3.up * 0.2f;
            crown += animator.transform.forward * 0.005f;

            var cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            Object.DestroyImmediate(cap.GetComponent<Collider>());
            cap.name = "Fila";
            cap.position = crown - Vector3.up * 0.02f;                // 7 cm tall, pulled down over the crown
            cap.rotation = animator.transform.rotation * Quaternion.Euler(-4f, 0f, 6f);
            cap.localScale = new Vector3(0.19f, 0.035f, 0.205f);
            cap.GetComponent<Renderer>().sharedMaterial = Flat("Char_ElderFila", new Color(0.45f, 0.1f, 0.12f), 0.3f);
            var band = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            Object.DestroyImmediate(band.GetComponent<Collider>());
            band.name = "Band"; band.SetParent(cap, false);
            band.localPosition = new Vector3(0f, -0.8f, 0f);
            band.localScale = new Vector3(1.03f, 0.22f, 1.03f);
            band.GetComponent<Renderer>().sharedMaterial = Flat("Char_ElderFilaBand", new Color(0.78f, 0.6f, 0.25f), 0.4f);
            cap.SetParent(head, true);
        }

        static void Staff(Transform root)
        {
            var staff = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
            Object.DestroyImmediate(staff.GetComponent<Collider>());
            staff.name = "Staff"; staff.SetParent(root, false);
            staff.localPosition = new Vector3(0.42f, 0.72f, 0.2f);
            staff.localRotation = Quaternion.Euler(-8f, 0f, -10f);
            staff.localScale = new Vector3(0.035f, 0.75f, 0.035f);
            staff.GetComponent<Renderer>().sharedMaterial = TexMat("Hub_Bark", "palm_bark", 1f, new Color(0.55f, 0.4f, 0.3f), new Vector2(1f, 2f));
        }
    }
}
