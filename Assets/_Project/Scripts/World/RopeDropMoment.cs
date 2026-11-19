using System.Collections;
using Itan.Story;
using UnityEngine;

namespace Itan.World
{
    /// <summary>Beat 1: Ajá sings and the rope uncoils down from the sky.</summary>
    public class RopeDropMoment : MonoBehaviour, IKeyMoment
    {
        [Tooltip("Pivot at the top of the rope; scaling it on Y lowers the rope.")]
        [SerializeField] Transform ropePivot;
        [SerializeField] float dropSeconds = 1.8f;

        void Awake() => ResetMoment();

        public IEnumerator Play()
        {
            ropePivot.gameObject.SetActive(true);
            for (float t = 0f; t < dropSeconds; t += Time.deltaTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t / dropSeconds);
                ropePivot.localScale = new Vector3(1f, Mathf.Max(0.001f, k), 1f);
                // A little sway as it falls.
                ropePivot.localRotation = Quaternion.Euler(Mathf.Sin(t * 6f) * 4f * (1f - k), 0f, 0f);
                yield return null;
            }
            ropePivot.localScale = Vector3.one;
            ropePivot.localRotation = Quaternion.identity;
            yield return new WaitForSeconds(0.5f);
        }

        public void ResetMoment()
        {
            ropePivot.localScale = new Vector3(1f, 0.001f, 1f);
            ropePivot.localRotation = Quaternion.identity;
            ropePivot.gameObject.SetActive(false);
        }
    }
}
