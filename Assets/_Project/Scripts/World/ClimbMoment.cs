using System.Collections;
using Itan.Story;
using UnityEngine;

namespace Itan.World
{
    /// <summary>Beat 2: Ajá trots to the rope and climbs up to his mother's home.</summary>
    public class ClimbMoment : MonoBehaviour, IKeyMoment
    {
        [SerializeField] Transform climber;
        [SerializeField] Transform ropeBottom;
        [SerializeField] Transform ropeTop;
        [Tooltip("Where the climber ends up, next to the mother.")]
        [SerializeField] Transform arrival;
        [SerializeField] float walkSeconds = 1.6f;
        [SerializeField] float climbSeconds = 4.5f;

        CharacterActor actor;

        Vector3 startPosition;
        Quaternion startRotation;

        void Awake()
        {
            startPosition = climber.position;
            startRotation = climber.rotation;
            actor = climber.GetComponent<CharacterActor>();
        }

        void Act(string state) { if (actor != null) actor.Play(state); }

        public IEnumerator Play()
        {
            Act(CharacterActor.Walk);
            yield return Tween.Face(climber, ropeBottom.position, 0.3f);
            yield return Tween.Move(climber, ropeBottom.position, walkSeconds);
            // Face the rope and climb, hand over hand.
            yield return Tween.Face(climber, ropeBottom.position + Vector3.forward, 0.2f);
            Act(CharacterActor.Climb);
            yield return Tween.Move(climber, ropeTop.position, climbSeconds);
            // Over the edge of the cloud and across to his place by the feast.
            Act(CharacterActor.Walk);
            yield return Tween.Face(climber, arrival.position, 0.2f);
            yield return Tween.Move(climber, arrival.position, 1.2f);
            yield return Tween.Face(climber, arrival.position + arrival.forward, 0.3f);
            Act(CharacterActor.Sit);
            yield return new WaitForSeconds(0.4f);
        }

        public void ResetMoment()
        {
            climber.SetPositionAndRotation(startPosition, startRotation);
            if (actor != null) actor.ResetPose();
        }
    }
}
