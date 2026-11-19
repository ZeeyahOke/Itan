using Itan.Narration;
using Itan.Quiz;
using Itan.Session;
using Itan.Story;
using Itan.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>
    /// Replaces the test card with three quiz cards fanned out in front of the
    /// seat, creates the Closing data, and adds the CardQuiz.
    /// Menu: Itan > Scene > Add Card Quiz
    /// </summary>
    public static class CardQuizBuilder
    {
        // Fan of three cards at arm's length, each turned to face the visitor's eyes.
        static readonly Vector3 Eye = new Vector3(0f, 1.15f, 0f);
        static readonly Vector3[] CardPositions =
        {
            new Vector3(-0.42f, 1.08f, 0.95f),
            new Vector3(0f, 1.12f, 1.05f),
            new Vector3(0.42f, 1.08f, 0.95f),
        };

        [MenuItem("Itan/Scene/Add Card Quiz")]
        public static void Build()
        {
            var session = Object.FindFirstObjectByType<SessionManager>();
            var story = Object.FindFirstObjectByType<StoryManager>();
            var narration = Object.FindFirstObjectByType<NarrationManager>();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(QuizCardBuilder.PrefabPath);
            if (session == null || story == null || narration == null || prefab == null) { Debug.LogError("[Itan] Need Session, Story, Narration and the quiz card prefab."); return; }
            if (Object.FindFirstObjectByType<CardQuiz>() != null) { Debug.Log("[Itan] Card quiz already added."); return; }

            // Fresh set of three cards.
            var root = GameObject.Find("QuizCards");
            if (root != null) Object.DestroyImmediate(root);
            root = new GameObject("QuizCards");

            var cards = new QuizCard[CardPositions.Length];
            for (int i = 0; i < CardPositions.Length; i++)
            {
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.transform);
                go.name = "QuizCard_" + i;
                go.transform.position = CardPositions[i];
                go.transform.rotation = Quaternion.LookRotation(CardPositions[i] - Eye); // canvas faces the eyes
                cards[i] = go.GetComponent<QuizCard>();
            }

            var quiz = root.AddComponent<CardQuiz>();
            var so = new SerializedObject(quiz);
            so.FindProperty("story").objectReferenceValue = story;
            so.FindProperty("narration").objectReferenceValue = narration;
            var list = so.FindProperty("cards");
            list.arraySize = cards.Length;
            for (int i = 0; i < cards.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = cards[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            var sessionSo = new SerializedObject(session);
            sessionSo.FindProperty("quiz").objectReferenceValue = quiz;
            sessionSo.FindProperty("closing").objectReferenceValue = SliceDataBuilder.BuildClosing();
            sessionSo.ApplyModifiedPropertiesWithoutUndo();

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Itan] Card quiz added with three cards.");
        }
    }
}
