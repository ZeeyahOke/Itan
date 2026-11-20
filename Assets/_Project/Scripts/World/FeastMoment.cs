using System.Collections;
using Itan.Story;
using UnityEngine;

namespace Itan.World
{
    /// <summary>
    /// Beat 2, in the sky: Ajá sits with his mother and eats. The feast keeps going
    /// in the background (they chat, he reaches for more) while the elder narrates.
    /// </summary>
    public class FeastMoment : MonoBehaviour, IKeyMoment
    {
        [SerializeField] CharacterActor aja;
        [SerializeField] CharacterActor mother;
        [Tooltip("How long this moment holds before the narration carries on (the feast keeps going).")]
        [SerializeField] float holdSeconds = 4f;

        Coroutine loop;

        public IEnumerator Play()
        {
            if (loop != null) StopCoroutine(loop);
            loop = StartCoroutine(Feast());
            yield return new WaitForSeconds(holdSeconds);
        }

        IEnumerator Feast()
        {
            mother.Play(CharacterActor.SitTalk);
            aja.Play(CharacterActor.Sit);
            yield return new WaitForSeconds(0.8f);
            while (true)
            {
                aja.Play(CharacterActor.Eat);
                yield return new WaitForSeconds(2.2f);
                aja.Play(CharacterActor.UpperNone, 0.4f);
                mother.Play(CharacterActor.Eat);
                yield return new WaitForSeconds(2.2f);
                mother.Play(CharacterActor.UpperNone, 0.4f);
                yield return new WaitForSeconds(1.2f);
            }
        }

        public void ResetMoment()
        {
            if (loop != null) StopCoroutine(loop);
            loop = null;
            if (aja != null) aja.ResetPose();
            if (mother != null) mother.ResetPose();
        }

        void OnDisable() => loop = null;
    }
}
