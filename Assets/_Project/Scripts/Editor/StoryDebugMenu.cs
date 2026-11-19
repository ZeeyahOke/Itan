using UnityEditor;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>Editor helpers to look at the story world without playing.</summary>
    public static class StoryDebugMenu
    {
        /// <summary>Opens the StoryWorld scene beside the hub and swaps which one is visible.</summary>
        [MenuItem("Itan/Debug/Preview Story World (toggle)")]
        static void Toggle()
        {
            StoryScenes.OpenStory();
            var story = StoryScenes.Find("StoryWorld");
            var hub = StoryScenes.Find("Hub");
            if (story == null || hub == null) return;
            bool showStory = !story.activeSelf;
            story.SetActive(showStory);
            hub.SetActive(!showStory);
        }
    }
}
