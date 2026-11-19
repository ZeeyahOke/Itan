using System;
using System.Collections;
using UnityEngine;

namespace Itan.World
{
    /// <summary>
    /// Moves the visit between the fireside hub and the story world with
    /// rising smoke and a fade. The camera never moves: we fade out, swap
    /// which world is shown, and fade back in.
    /// </summary>
    public class SceneTransition : MonoBehaviour
    {
        [Tooltip("Everything that belongs to the fireside and is hidden during the story.")]
        [SerializeField] GameObject[] hubObjects;
        [SerializeField] GameObject storyWorld;
        [SerializeField] ParticleSystem smoke;

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

        void Awake()
        {
            block = new MaterialPropertyBlock();
            FitFadeQuadToCamera();
            SetFade(0f);
        }

        public void SmokeToStory(Action onDone = null) => StartCoroutine(Swap(true, onDone));
        public void SmokeToFire(Action onDone = null) => StartCoroutine(Swap(false, onDone));

        IEnumerator Swap(bool toStory, Action onDone)
        {
            IsPlaying = true;
            smoke.Play();
            yield return new WaitForSeconds(smokeLead);

            yield return Fade(0f, 1f);
            ShowWorld(toStory);
            yield return new WaitForSeconds(holdSeconds);

            smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            yield return Fade(1f, 0f);

            IsPlaying = false;
            Debug.Log("[Transition] Now in " + (toStory ? "story world" : "hub"));
            onDone?.Invoke();
        }

        /// <summary>Show one world and hide the other, straight away.</summary>
        public void ShowWorld(bool story)
        {
            foreach (var go in hubObjects) if (go != null) go.SetActive(!story);
            storyWorld.SetActive(story);
            InStory = story;
        }

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
