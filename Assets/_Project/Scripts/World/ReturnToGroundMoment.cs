using System.Collections;
using Itan.Player;
using Itan.Story;
using UnityEngine;

namespace Itan.World
{
    /// <summary>Brings the visitor from the cloud back to their seat on the ground (start of beat 3).</summary>
    public class ReturnToGroundMoment : MonoBehaviour, IKeyMoment
    {
        public IEnumerator Play()
        {
            var transition = FindFirstObjectByType<SceneTransition>();
            var seat = FindFirstObjectByType<SeatRecenter>();
            if (seat == null) yield break;
            if (transition != null) yield return transition.FadeOut();
            seat.RecenterNow();
            yield return new WaitForSeconds(0.2f);
            if (transition != null) yield return transition.FadeIn();
        }

        public void ResetMoment() { }
    }
}
