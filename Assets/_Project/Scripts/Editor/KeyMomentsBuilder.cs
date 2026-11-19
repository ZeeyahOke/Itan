using Itan.Story;
using Itan.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>
    /// Adds the three key moments (rope drop, Ajá's climb, Ìjàpá's fall) to the
    /// story world and lines them up with the beats in the StoryManager.
    /// Menu: Itan > Scene > Add Key Moments
    /// </summary>
    public static class KeyMomentsBuilder
    {
        [MenuItem("Itan/Scene/Add Key Moments")]
        public static void Build()
        {
            var story = Object.FindFirstObjectByType<StoryManager>();
            var world = Find("StoryWorld");
            if (story == null || world == null) { Debug.LogError("[Itan] Need the StoryManager and story world."); return; }
            if (world.transform.Find("KeyMoments") != null) { Debug.Log("[Itan] Key moments already added."); return; }

            var pivot = Find("RopePivot").transform;
            var aja = Find("Aja").transform;
            var ijapa = Find("Ijapa").transform;
            var mother = Find("AjaMother").transform;

            var root = new GameObject("KeyMoments").transform;
            root.SetParent(world.transform, false);

            // Rope ends and the arrival point beside the mother.
            var bottom = Marker("RopeBottom", root, StoryWorldBuilder.RopeBottom - new Vector3(0f, 0.15f, 0.25f));
            var top = Marker("RopeTop", root, StoryWorldBuilder.RopeTop + new Vector3(0f, 0.15f, -0.25f));
            var arrival = Marker("AjaArrival", root, mother.position + new Vector3(-0.9f, 0f, 0f));

            var rope = root.gameObject.AddComponent<RopeDropMoment>();
            Set(rope, "ropePivot", pivot);

            var climb = root.gameObject.AddComponent<ClimbMoment>();
            Set(climb, "climber", aja);
            Set(climb, "ropeBottom", bottom);
            Set(climb, "ropeTop", top);
            Set(climb, "arrival", arrival);

            var fall = root.gameObject.AddComponent<FallMoment>();
            Set(fall, "climber", ijapa);
            Set(fall, "ropeBottom", bottom);
            Set(fall, "ropeTop", top);
            Set(fall, "ropePivot", pivot);
            Set(fall, "shellWhole", ijapa.Find("Model/ShellWhole").gameObject);
            Set(fall, "shellCracked", ijapa.Find("Model/ShellCracked").gameObject);

            var so = new SerializedObject(story);
            var list = so.FindProperty("keyMoments");
            list.arraySize = 3;
            list.GetArrayElementAtIndex(0).objectReferenceValue = rope;
            list.GetArrayElementAtIndex(1).objectReferenceValue = climb;
            list.GetArrayElementAtIndex(2).objectReferenceValue = fall;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Characters and rope move now, so they can't be static.
            foreach (var t in new[] { aja, ijapa, pivot })
                foreach (Transform c in t.GetComponentsInChildren<Transform>(true))
                    GameObjectUtility.SetStaticEditorFlags(c.gameObject, 0);

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Itan] Key moments added.");
        }

        static Transform Marker(string name, Transform parent, Vector3 worldPos)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.position = worldPos;
            return t;
        }

        static void Set(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject Find(string name)
        {
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name == name) return t.gameObject;
            return null;
        }
    }
}
