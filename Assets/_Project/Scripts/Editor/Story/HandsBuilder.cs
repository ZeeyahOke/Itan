using System.Linq;
using Itan.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Itan.EditorTools
{
    /// <summary>
    /// Gives the visitor dark-skinned hands in place of the controller models, posed by grip and
    /// trigger like in the VR Room project. Menu: Itan > Hub > Add Visitor Hands
    /// </summary>
    public static class HandsBuilder
    {
        const string Actions = "Assets/Samples/XR Interaction Toolkit/3.5.1/Starter Assets/XRI Default Input Actions.inputactions";

        [MenuItem("Itan/Hub/Add Visitor Hands")]
        public static void Build()
        {
            foreach (var side in new[] { "Left", "Right" })
            {
                var prefabPath = $"Assets/_Project/Prefabs/{side}Hand.prefab";
                var contents = PrefabUtility.LoadPrefabContents(prefabPath);
                var driver = contents.GetComponent<HandAnimator>();
                var so = new SerializedObject(driver);
                so.FindProperty("gripAction.m_UseReference").boolValue = true;
                so.FindProperty("gripAction.m_Reference").objectReferenceValue = Action($"XRI {side} Interaction/Select Value");
                so.FindProperty("triggerAction.m_UseReference").boolValue = true;
                so.FindProperty("triggerAction.m_Reference").objectReferenceValue = Action($"XRI {side} Interaction/Activate Value");
                so.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(contents, prefabPath);
                PrefabUtility.UnloadPrefabContents(contents);

                var controller = StoryScenes.Find($"{side} Controller")?.transform;
                if (controller == null) { Debug.LogError("[Itan] No " + side + " Controller in the rig."); continue; }
                var old = controller.Find($"{side} Hand");
                if (old != null) Object.DestroyImmediate(old.gameObject);
                var hand = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath), controller);
                hand.name = $"{side} Hand";
                hand.transform.localPosition = Vector3.zero;
                hand.transform.localRotation = Quaternion.identity;
                var visual = controller.Find($"{side} Controller Visual");
                if (visual != null) visual.gameObject.SetActive(false);
            }
            var scene = StoryScenes.Find("Left Controller").scene;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Itan] Visitor hands added.");
        }

        static InputActionReference Action(string path) =>
            AssetDatabase.LoadAllAssetsAtPath(Actions).OfType<InputActionReference>()
                .FirstOrDefault(r => r.action != null && $"{r.action.actionMap.name}/{r.action.name}" == path);
    }
}
