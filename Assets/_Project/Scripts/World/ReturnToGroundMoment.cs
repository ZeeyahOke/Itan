using System.Collections;
using Itan.Player;
using Itan.Story;
using UnityEngine;

namespace Itan.World
{
    /// <summary>Brings the visitor from the cloud back to their seat on the ground (start of beat 3).</summary>
    public class ReturnToGroundMoment : MonoBehaviour, IKeyMoment
    {
        [Tooltip("Drawn back up into the cloud while the visitor is away: a new night begins.")]
        [SerializeField] RopeDropMoment rope;
        [SerializeField] GameObject cloudSea;

        public IEnumerator Play()
        {
            var transition = FindFirstObjectByType<SceneTransition>();
            var seat = FindFirstObjectByType<SeatRecenter>();
            if (seat == null) yield break;
            if (transition != null) yield return transition.FadeOut();
            if (rope != null) rope.Retract();
            if (cloudSea != null) cloudSea.SetActive(false);
            seat.RecenterNow();
            if (Itan.Player.VisitorSpace.Instance != null) Itan.Player.VisitorSpace.Instance.ResetToSeat();
            yield return new WaitForSeconds(0.2f);
            if (transition != null) yield return transition.FadeIn();
        }

        public void ResetMoment() { }
    }
}
