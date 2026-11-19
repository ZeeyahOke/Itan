using System.IO;
using System.Linq;
using Itan.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Itan.EditorTools
{
    /// <summary>
    /// The hub (fireside) and the story world are separate scenes. At runtime the hub
    /// loads StoryWorld additively; in the editor these helpers open it beside the hub
    /// so the builders can work on both.
    /// </summary>
    public static class StoryScenes
    {
        public const string HubPath = "Assets/_Project/Scenes/Hub.unity";
        public const string StoryPath = "Assets/_Project/Scenes/StoryWorld.unity";

        public static bool StorySceneExists => File.Exists(StoryPath);

        /// <summary>Open the story scene next to whatever is open (no-op if it's open or doesn't exist yet).</summary>
        public static Scene OpenStory()
        {
            var s = SceneManager.GetSceneByPath(StoryPath);
            if (s.isLoaded || !StorySceneExists) return s;
            return EditorSceneManager.OpenScene(StoryPath, OpenSceneMode.Additive);
        }

        /// <summary>Find a GameObject by name in every open scene, including hidden ones.</summary>
        public static GameObject Find(string name)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var t in root.GetComponentsInChildren<Transform>(true))
                        if (t.name == name) return t.gameObject;
            }
            return null;
        }

        public static void SaveAll()
        {
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();
        }

        [MenuItem("Itan/Scene/Open Story World Scene")]
        static void OpenMenu() => OpenStory();

        [MenuItem("Itan/Scene/Close Story World Scene")]
        static void CloseMenu()
        {
            var s = SceneManager.GetSceneByPath(StoryPath);
            if (!s.isLoaded) return;
            EditorSceneManager.SaveScene(s);
            EditorSceneManager.CloseScene(s, true);
        }

        /// <summary>
        /// One-off: move the StoryWorld object out of Hub.unity into its own scene,
        /// with a StoryWorldRoot on top that hands the key moments to the hub.
        /// </summary>
        [MenuItem("Itan/Scene/Split Story World Into Own Scene")]
        public static void Split()
        {
            if (StorySceneExists) { Debug.Log("[Itan] StoryWorld scene already exists."); return; }
            var hub = SceneManager.GetSceneByPath(HubPath);
            if (!hub.isLoaded) hub = EditorSceneManager.OpenScene(HubPath, OpenSceneMode.Single);

            var world = hub.GetRootGameObjects().FirstOrDefault(g => g.name == "StoryWorld");
            if (world == null) { Debug.LogError("[Itan] No StoryWorld object in the hub."); return; }

            var keyMoments = world.transform.Find("KeyMoments");
            var moments = keyMoments == null ? new MonoBehaviour[0] : new MonoBehaviour[]
            {
                keyMoments.GetComponent<RopeDropMoment>(),
                keyMoments.GetComponent<ClimbMoment>(),
                keyMoments.GetComponent<FallMoment>(),
            };

            var story = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            var root = new GameObject("StoryScene");
            SceneManager.MoveGameObjectToScene(root, story);
            SceneManager.MoveGameObjectToScene(world, story);
            world.transform.SetParent(root.transform, true);
            world.SetActive(false);

            var storyRoot = root.AddComponent<StoryWorldRoot>();
            var so = new SerializedObject(storyRoot);
            so.FindProperty("content").objectReferenceValue = world;
            var list = so.FindProperty("keyMoments");
            list.arraySize = moments.Length;
            for (int i = 0; i < moments.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = moments[i];
            so.ApplyModifiedPropertiesWithoutUndo();

            // Its own sky, ambient and fog (same sunset for now).
            SceneManager.SetActiveScene(story);
            SunsetLighting.ApplyEnvironment();
            RenderSettings.sun = null; // the sun light lives in the hub
            SceneManager.SetActiveScene(hub);

            EditorSceneManager.SaveScene(story, StoryPath);
            EditorSceneManager.MarkSceneDirty(hub);
            EditorSceneManager.SaveScene(hub);
            AddToBuild();
            EditorSceneManager.CloseScene(story, true);
            Debug.Log("[Itan] Story world moved to " + StoryPath);
        }

        static void AddToBuild()
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == StoryPath)) return;
            scenes.Add(new EditorBuildSettingsScene(StoryPath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        [MenuItem("Itan/Scene/Apply Sunset Lighting (Story World)")]
        static void StoryLighting()
        {
            var story = OpenStory();
            if (!story.isLoaded) return;
            var previous = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(story);
            SunsetLighting.ApplyEnvironment();
            RenderSettings.sun = null;
            SceneManager.SetActiveScene(previous);
            EditorSceneManager.SaveScene(story);
        }
    }
}
