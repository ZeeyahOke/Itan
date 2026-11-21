using System.Linq;
using Itan.World;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Itan.EditorTools
{
    /// <summary>
    /// Wires the characters' animations and the story beats in the StoryWorld scene:
    /// beat 1 rope drop, beat 2 = Ajá climbs + the visitor climbs + the feast, beat 3
    /// = back to the ground, then Ìjàpá's fall.
    /// Menu: Itan > Story > Wire Animations and Moments
    /// </summary>
    public static class StoryMomentsWiring
    {
        const string AnimDir = "Assets/_Project/Art/Animation";
        const string InputActions = "Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/XRI Default Input Actions.inputactions";

        [MenuItem("Itan/Story/Wire Animations and Moments")]
        public static void Wire()
        {
            var controller = BuildController();
            StoryScenes.OpenStory();

            // Every story character uses the full controller.
            foreach (var actor in Object.FindObjectsByType<CharacterActor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                foreach (var a in actor.GetComponentsInChildren<Animator>(true)) a.runtimeAnimatorController = controller;

            SetStart("Aja", CharacterActor.Idle);
            SetStart("AjaMother", CharacterActor.SitTalk);
            SetStart("Ijapa", CharacterActor.Hide);

            var moments = StoryScenes.Find("KeyMoments").transform;
            var rope = moments.GetComponent<RopeDropMoment>();
            var climb = moments.GetComponent<ClimbMoment>();
            var fall = moments.GetComponent<FallMoment>();
            Set(climb, "walkSeconds", 1.6f); Set(climb, "climbSeconds", 4.5f);
            Set(fall, "walkSeconds", 3.8f); Set(fall, "climbSeconds", 2.4f);
            SetRef(fall, "ropeDrop", rope);

            // Ìjàpá hides forward and to the left, clear of Ajá's way to the rope.
            var ijapaT = StoryScenes.Find("Ijapa").transform;
            ijapaT.position = StoryWorldDresser.IjapaHide;
            var toRope = StoryWorldBuilder.RopeBottom - ijapaT.position; toRope.y = 0f;
            ijapaT.rotation = Quaternion.Slerp(Quaternion.LookRotation(toRope), Quaternion.LookRotation(Vector3.back), 0.45f);
            var cloudSea = StoryScenes.Find("CloudSea");

            // Beat 2 extras.
            var feast = Child<FeastMoment>(moments, "Feast");
            SetRef(feast, "aja", StoryScenes.Find("Aja").GetComponent<CharacterActor>());
            SetRef(feast, "mother", StoryScenes.Find("AjaMother").GetComponent<CharacterActor>());

            var playerClimb = Child<PlayerClimbMoment>(moments, "PlayerClimb");
            var ropeT = StoryScenes.Find("Rope").transform;
            var baseMarker = Marker(moments, "PlayerRopeBase", new Vector3(ropeT.position.x, 1.15f, StoryWorldBuilder.RopeBottom.z - 0.42f), Vector3.forward);
            SetRef(playerClimb, "ropeBase", baseMarker);
            SetRef(playerClimb, "rope", ropeT);
            SetRef(playerClimb, "skyViewpoint", StoryScenes.Find("SkyViewpoint").transform);
            SetRef(playerClimb, "leftGrip", Action("XRI Left Interaction/Select Value"));
            SetRef(playerClimb, "rightGrip", Action("XRI Right Interaction/Select Value"));
            SetRef(playerClimb, "prompt", ClimbPrompt(moments, baseMarker));
            SetRef(playerClimb, "cloudSea", cloudSea);

            var beat2 = Child<SequenceMoment>(moments, "Beat2_ClimbAndFeast");
            var so = new SerializedObject(beat2);
            var steps = so.FindProperty("steps");
            steps.arraySize = 3;
            steps.GetArrayElementAtIndex(0).objectReferenceValue = climb;
            steps.GetArrayElementAtIndex(1).objectReferenceValue = playerClimb;
            steps.GetArrayElementAtIndex(2).objectReferenceValue = feast;
            so.ApplyModifiedPropertiesWithoutUndo();

            var back = Child<ReturnToGroundMoment>(moments, "Beat3_BackToGround");
            SetRef(back, "rope", rope);
            SetRef(back, "cloudSea", cloudSea);

            var root = Object.FindFirstObjectByType<StoryWorldRoot>(FindObjectsInactive.Include);
            var rso = new SerializedObject(root);
            var km = rso.FindProperty("keyMoments");
            km.arraySize = 3;
            km.GetArrayElementAtIndex(0).objectReferenceValue = rope;
            km.GetArrayElementAtIndex(1).objectReferenceValue = beat2;
            km.GetArrayElementAtIndex(2).objectReferenceValue = fall;
            var intros = rso.FindProperty("beatIntros");
            intros.arraySize = 3;
            intros.GetArrayElementAtIndex(0).objectReferenceValue = null;
            intros.GetArrayElementAtIndex(1).objectReferenceValue = null;
            intros.GetArrayElementAtIndex(2).objectReferenceValue = back;
            rso.ApplyModifiedPropertiesWithoutUndo();

            StoryScenes.SaveAll();
            Debug.Log("[Itan] Story animations and moments wired.");
        }

        static void SetStart(string who, string state)
        {
            var actor = StoryScenes.Find(who).GetComponent<CharacterActor>();
            var so = new SerializedObject(actor);
            so.FindProperty("startState").stringValue = state;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static T Child<T>(Transform parent, string name) where T : Component
        {
            var t = parent.Find(name);
            if (t == null) { t = new GameObject(name).transform; t.SetParent(parent, false); }
            return t.TryGetComponent<T>(out var c) ? c : t.gameObject.AddComponent<T>();
        }

        static Transform Marker(Transform parent, string name, Vector3 pos, Vector3 forward)
        {
            var t = parent.Find(name);
            if (t == null) t = new GameObject(name).transform;
            t.SetParent(parent, true);
            t.position = pos;
            t.rotation = Quaternion.LookRotation(forward);
            return t;
        }

        static GameObject ClimbPrompt(Transform parent, Transform ropeBase)
        {
            var existing = parent.Find("ClimbPrompt");
            if (existing != null) Object.DestroyImmediate(existing.gameObject);
            var go = new GameObject("ClimbPrompt");
            go.transform.SetParent(parent, false);
            go.transform.position = ropeBase.position + new Vector3(0.55f, -0.1f, 0.45f);
            go.transform.rotation = Quaternion.LookRotation(go.transform.position - ropeBase.position);
            var text = go.AddComponent<TextMeshPro>();
            text.text = "Grip the rope and pull down\nto climb after Ajá";
            text.fontSize = 0.9f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(1f, 0.92f, 0.7f);
            text.rectTransform.sizeDelta = new Vector2(1.2f, 0.4f);
            var font = AssetDatabase.FindAssets("t:TMP_FontAsset AlegreyaSans-Bold").Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>).FirstOrDefault();
            if (font != null) text.font = font;
            go.SetActive(false);
            return go;
        }

        static InputActionReference Action(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(InputActions).OfType<InputActionReference>()
                .FirstOrDefault(r => r.action != null && $"{r.action.actionMap.name}/{r.action.name}" == path);
        }

        static void Set(Object o, string field, float v)
        {
            var so = new SerializedObject(o);
            so.FindProperty(field).floatValue = v;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetRef(Object o, string field, Object v)
        {
            var so = new SerializedObject(o);
            so.FindProperty(field).objectReferenceValue = v;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // ---------------- Animator controller ----------------

        public static AnimatorController BuildController()
        {
            AssetDatabase.DeleteAsset(StoryCastBuilder.ControllerPath);
            var c = AnimatorController.CreateAnimatorControllerAtPath(StoryCastBuilder.ControllerPath);
            var sm = c.layers[0].stateMachine;

            AnimatorState S(string name, Motion m)
            {
                var st = sm.AddState(name);
                st.motion = m;
                return st;
            }
            var idle = S(CharacterActor.Idle, AnimationLibrarySetup.Clip("Rig|Idle_Loop"));
            sm.defaultState = idle;
            S(CharacterActor.Walk, AnimationLibrarySetup.Clip("Rig|Walk_Loop"));
            S(CharacterActor.Sneak, AnimationLibrarySetup.Clip("Rig|Crouch_Fwd_Loop"));
            S(CharacterActor.Hide, AnimationLibrarySetup.Clip("Rig|Crouch_Idle_Loop"));
            S(CharacterActor.Climb, RopeClimbClip());
            S(CharacterActor.Fall, AnimationLibrarySetup.Clip("Rig|Jump_Loop"));
            S(CharacterActor.Land, AnimationLibrarySetup.Clip("Rig|Death01"));
            S(CharacterActor.Sing, AnimationLibrarySetup.Clip("Rig|Idle_Talking_Loop"));
            S(CharacterActor.Talk, AnimationLibrarySetup.Clip("Rig|Idle_Talking_Loop"));
            S(CharacterActor.Sit, AnimationLibrarySetup.Clip("Rig|Sitting_Idle_Loop"));
            S(CharacterActor.SitTalk, AnimationLibrarySetup.Clip("Rig|Sitting_Talking_Loop"));
            S(CharacterActor.Nod, AnimationLibrarySetup.Clip("Armature|Yes"));

            // Upper body only: eating while sitting.
            c.AddLayer("UpperBody");
            var layers = c.layers;
            layers[1].avatarMask = UpperBodyMask();
            layers[1].defaultWeight = 0f;
            layers[1].blendingMode = AnimatorLayerBlendingMode.Override;
            c.layers = layers;
            var upper = c.layers[1].stateMachine;
            var none = upper.AddState(CharacterActor.UpperNone);
            upper.defaultState = none;
            var eat = upper.AddState(CharacterActor.Eat);
            eat.motion = AnimationLibrarySetup.Clip("Armature|Consume");

            AssetDatabase.SaveAssets();
            return c;
        }

        static AvatarMask UpperBodyMask()
        {
            var path = AnimDir + "/Mask_UpperBody.mask";
            var mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(path);
            if (mask == null) { mask = new AvatarMask(); AssetDatabase.CreateAsset(mask, path); }
            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
            {
                var part = (AvatarMaskBodyPart)i;
                bool upper = part == AvatarMaskBodyPart.Head || part == AvatarMaskBodyPart.LeftArm || part == AvatarMaskBodyPart.RightArm
                          || part == AvatarMaskBodyPart.LeftFingers || part == AvatarMaskBodyPart.RightFingers;
                mask.SetHumanoidBodyPartActive(part, upper);
            }
            EditorUtility.SetDirty(mask);
            return mask;
        }

        /// <summary>Hand-over-hand rope climbing, made from humanoid muscle curves (there's no rope climb in the libraries).</summary>
        static AnimationClip RopeClimbClip()
        {
            var path = AnimDir + "/Climb_Rope.anim";
            AssetDatabase.DeleteAsset(path);
            var clip = new AnimationClip { name = "Climb_Rope" };
            const float T = 1.1f;
            void Curve(string muscle, float a, float b)
            {
                // a at the start, b halfway, back to a: one full hand-over-hand cycle.
                var curve = new AnimationCurve(new Keyframe(0f, a), new Keyframe(T * 0.5f, b), new Keyframe(T, a));
                for (int i = 0; i < curve.length; i++) AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                clip.SetCurve("", typeof(Animator), muscle, curve);
            }
            void Const(string muscle, float v) => clip.SetCurve("", typeof(Animator), muscle, AnimationCurve.Constant(0f, T, v));

            Curve("Left Arm Down-Up", 1.0f, 0.15f);   Curve("Right Arm Down-Up", 0.15f, 1.0f);
            Curve("Left Forearm Stretch", 0.9f, -0.4f); Curve("Right Forearm Stretch", -0.4f, 0.9f);
            Const("Left Arm Front-Back", 0.75f);        Const("Right Arm Front-Back", 0.75f);
            Curve("Left Upper Leg Front-Back", 0.1f, 0.65f); Curve("Right Upper Leg Front-Back", 0.65f, 0.1f);
            Curve("Left Lower Leg Stretch", 0.8f, -0.5f);    Curve("Right Lower Leg Stretch", -0.5f, 0.8f);
            Const("Spine Front-Back", 0.15f);
            Const("Head Nod Down-Up", 0.35f); // looking up the rope

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }
    }
}
