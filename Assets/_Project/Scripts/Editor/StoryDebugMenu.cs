using UnityEditor;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>Editor helpers to look at the story world without playing.</summary>
    public static class StoryDebugMenu
    {
        [MenuItem("Itan/Debug/Preview Story World (toggle)")]
        static void Toggle()
        {
            GameObject story = null, hub = GameObject.Find("Hub");
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name == "StoryWorld") story = t.gameObject;
            if (story == null || hub == null) return;
            bool showStory = !story.activeSelf;
            story.SetActive(showStory);
            hub.SetActive(!showStory);
        }
    }
}
