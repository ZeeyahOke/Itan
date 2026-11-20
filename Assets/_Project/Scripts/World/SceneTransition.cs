using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Itan.World
{
    /// <summary>
    /// Moves the visit between the fireside hub and the story world with
    /// rising smoke and a fade. The camera never moves: we fade out, swap
    /// which world is shown, and fade back in.
    /// The story world is its own scene. It is loaded additively in the
    /// background when the hub starts, kept hidden, and made the active scene
    /// while it is shown so its own sky, ambient light and fog apply.
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        [Tooltip("Everything that belongs to the fireside and is hidden during the story.")]
        [SerializeField] GameObject[] hubObjects;
        [Tooltip("Name of the story world scene (must be in the build settings).")]
        [SerializeField] string storyScene = "StoryWorld";
        [SerializeField] ParticleSystem smoke;
        [Tooltip("Talking drum that announces each journey through the smoke.")]
        [SerializeField] AudioSource drum;

        [Tooltip("Quad in front of the eyes, used for the fade.")]
        [SerializeField] Renderer fadeQuad;
        [SerializeField] Color fadeColor = new Color(0.1f, 0.1f, 0.18f, 1f);

        [SerializeField] float smokeLead = 1.4f;   // smoke rises before the fade starts
        [SerializeField] float fadeSeconds = 0.8f;
        [SerializeField] float holdSeconds = 0.4f; // fully faded, while the worlds swap

        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public bool InStory { get; private set; }
        public bool IsPlaying { get; private set; }

        MaterialPropertyBlock block;
        Scene hubScene;

        /// <summary>True once the story world scene has loaded and can be shown.</summary>
        public bool StoryReady => StoryWorldRoot.Current != null;

        void Awake()
        {
            block = new MaterialPropertyBlock();
            hubScene = gameObject.scene;
            FitFadeQuadToCamera();
            SetFade(0f);
        }

        void Start() => StartCoroutine(LoadStoryWorld());

        // Load the story scene alongside the hub while the visitor is at the fire,
        // so there is no hitch when the smoke rises.
        IEnumerator LoadStoryWorld()
        {
            if (SceneManager.GetSceneByName(storyScene).isLoaded) yield break; // already open in the editor
            var op = SceneManager.LoadSceneAsync(storyScene, LoadSceneMode.Additive);
            if (op == null) { Debug.LogError("[Transition] Story scene '" + storyScene + "' is not in the build settings."); yield break; }
            yield return op;
            Debug.Log("[Transition] Story world loaded");
        }

        public void SmokeToStory(Action onDone = null) => StartCoroutine(Swap(true, onDone));
        public void SmokeToFire(Action onDone = null) => StartCoroutine(Swap(false, onDone));

        IEnumerator Swap(bool toStory, Action onDone)
        {
            IsPlaying = true;
            smoke.Play();
            if (drum != null) drum.Play();
            yield return new WaitForSeconds(smokeLead);

            yield return Fade(0f, 1f);
            if (toStory) yield return new WaitUntil(() => StoryReady);
            ShowWorld(toStory);
            yield return new WaitForSeconds(holdSeconds);

            smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            yield return Fade(1f, 0f);

            IsPlaying = false;
            Debug.Log("[Transition] Now in " + (toStory ? "story world" : "hub"));
            onDone?.Invoke();
        }

        /// <summary>Stop a transition part-way (used when a step is skipped) and clear the fade.</summary>
        public void Cancel()
        {
            StopAllCoroutines();
            smoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            SetFade(0f);
            IsPlaying = false;
        }

        /// <summary>Show one world and hide the other, straight away.</summary>
        public void ShowWorld(bool story)
        {
            foreach (var go in hubObjects) if (go != null) go.SetActive(!story);
            var world = StoryWorldRoot.Current;
            if (world != null)
            {
                world.Show(story);
                // The active scene decides the sky, ambient light and fog.
                var active = story ? world.gameObject.scene : hubScene;
                if (active.IsValid() && active.isLoaded && SceneManager.GetActiveScene() != active)
                {
                    SceneManager.SetActiveScene(active);
                    DynamicGI.UpdateEnvironment();
                }
            }
            else if (story) Debug.LogWarning("[Transition] Story world isn't loaded yet.");
            // Back at the fire, the visitor is always in their seat (even if they left from the cloud).
            if (!story && InStory)
            {
                var seat = FindFirstObjectByType<Itan.Player.SeatRecenter>();
                if (seat != null) seat.RecenterNow();
            }
            InStory = story;
        }

        public IEnumerator FadeOut() => Fade(0f, 1f);
        public IEnumerator FadeIn() => Fade(1f, 0f);

        IEnumerator Fade(float from, float to)
        {
            for (float t = 0f; t < fadeSeconds; t += Time.deltaTime)
            {
                SetFade(Mathf.Lerp(from, to, t / fadeSeconds));
                yield return null;
            }
            SetFade(to);
        }

        void SetFade(float alpha)
        {
            fadeQuad.enabled = alpha > 0.001f;
            block.SetColor(BaseColor, new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha));
            fadeQuad.SetPropertyBlock(block);
        }

        // Sit the quad just past the camera's near clip plane, big enough to cover both eyes.
        void FitFadeQuadToCamera()
        {
            var cam = fadeQuad.GetComponentInParent<Camera>();
            float distance = (cam != null ? cam.nearClipPlane : 0.05f) + 0.03f;
            fadeQuad.transform.localPosition = new Vector3(0f, 0f, distance);
            fadeQuad.transform.localRotation = Quaternion.identity;
            fadeQuad.transform.localScale = Vector3.one * Mathf.Max(0.4f, distance * 6f);
        }
    }
}
