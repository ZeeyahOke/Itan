using System.Collections;
using Itan.Story;
using UnityEngine;

namespace Itan.World
{
    /// <summary>Plays several key moments one after another as a single beat moment.</summary>
    public class SequenceMoment : MonoBehaviour, IKeyMoment
    {
        [Tooltip("Each must implement IKeyMoment.")]
        [SerializeField] MonoBehaviour[] steps = new MonoBehaviour[0];

        public IEnumerator Play()
        {
            foreach (var s in steps)
                if (s is IKeyMoment m) yield return m.Play();
        }

        public void ResetMoment()
        {
            foreach (var s in steps)
                if (s is IKeyMoment m) m.ResetMoment();
        }
    }
}
