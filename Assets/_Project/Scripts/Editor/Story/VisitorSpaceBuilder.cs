using Itan.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>
    /// Gathers the visitor's own things (subtitles, word card, listening prompt, status label
    /// and the lap with the calabash) under a VisitorSpace, so they go wherever the visitor goes.
    /// Menu: Itan > Hub > Set Up Visitor Space
    /// </summary>
    public static class VisitorSpaceBuilder
    {
        static readonly string[] Members = { "Subtitles", "WordCard", "StatusLabel", "VoicePrompt", "LapPoint" };

        [MenuItem("Itan/Hub/Set Up Visitor Space")]
        public static void Build()
        {
            var existing = Object.FindFirstObjectByType<VisitorSpace>(FindObjectsInactive.Include);
            var space = existing != null ? existing.gameObject : new GameObject("VisitorSpace");
            space.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            var vs = existing != null ? existing : space.AddComponent<VisitorSpace>();
            var so = new SerializedObject(vs);
            so.FindProperty("seatEye").objectReferenceValue = StoryScenes.Find("SeatEye").transform;
            so.ApplyModifiedPropertiesWithoutUndo();

            foreach (var name in Members)
            {
                var go = StoryScenes.Find(name);
                if (go == null) { Debug.LogWarning("[Itan] Not found: " + name); continue; }
                go.transform.SetParent(space.transform, true);
            }
            EditorSceneManager.MarkSceneDirty(space.scene);
            EditorSceneManager.SaveScene(space.scene);
            Debug.Log("[Itan] Visitor space set up.");
        }
    }
}
