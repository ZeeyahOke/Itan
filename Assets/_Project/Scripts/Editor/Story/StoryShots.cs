using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Itan.EditorTools
{
    /// <summary>
    /// Renders the story world from the visitor's points of view without entering Play mode.
    /// Writes Logs/story_*.png. Menu: Itan > Debug > Capture Story Shots
    /// </summary>
    public static class StoryShots
    {
        [MenuItem("Itan/Debug/Capture Story Shots")]
        public static void Capture()
        {
            var story = StoryScenes.OpenStory();
            var world = StoryScenes.Find("StoryWorld");
            bool worldWas = world.activeSelf;
            world.SetActive(true);
            // Only the story scene should be in the picture: hide every root of the other open scenes.
            var hidden = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s == story || !s.isLoaded) continue;
                foreach (var r in s.GetRootGameObjects())
                    if (r.activeSelf && !(r.TryGetComponent<Light>(out var l) && l.type == LightType.Directional)) { r.SetActive(false); hidden.Add(r); }
            }
            var previous = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(story);

            var eye = new Vector3(0f, 1.15f, 0f);
            var sky = StoryWorldDresser.SkyViewpoint + Vector3.up * 1.15f;
            Shot("story_seat", eye, eye + Vector3.forward * 10f + Vector3.up * 0.4f, 85f);
            Shot("story_up", eye, new Vector3(0f, 5.2f, 8f), 85f);
            Shot("story_left", eye, eye + new Vector3(-10f, 0.3f, 4f), 85f);
            Shot("story_right", eye, eye + new Vector3(10f, 0.3f, 4f), 85f);
            Shot("story_sky", sky, StoryWorldDresser.FeastCentre + Vector3.up * 0.3f, 80f);
            Shot("story_cast_ground", eye, new Vector3(-1.9f, 0.75f, 5.9f), 32f);
            Shot("story_cast_sky", sky, StoryWorldDresser.FeastCentre + Vector3.up * 0.6f, 45f);
            Shot("story_overview", new Vector3(0f, 9f, -6f), new Vector3(0f, 1.5f, 8f), 70f);

            SceneManager.SetActiveScene(previous);
            world.SetActive(worldWas);
            foreach (var r in hidden) r.SetActive(true);
        }

        static void Shot(string name, Vector3 from, Vector3 lookAt, float fov)
        {
            var go = new GameObject("ShotCam");
            var cam = go.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = fov;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 300f;
            go.transform.position = from;
            go.transform.LookAt(lookAt);
            var rt = new RenderTexture(1280, 800, 24);
            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            File.WriteAllBytes($"Logs/{name}.png", tex.EncodeToPNG());
            RenderTexture.active = null;
            cam.targetTexture = null;
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(go);
        }
    }
}
