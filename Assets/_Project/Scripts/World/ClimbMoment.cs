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
        [SerializeField] float walkSeconds = 1.4f;
        [SerializeField] float climbSeconds = 2.6f;

        Vector3 startPosition;
        Quaternion startRotation;

        void Awake()
        {
            startPosition = climber.position;
            startRotation = climber.rotation;
        }

        public IEnumerator Play()
        {
            yield return Tween.Face(climber, ropeBottom.position, 0.3f);
            yield return Tween.Move(climber, ropeBottom.position, walkSeconds);
            yield return Tween.Move(climber, ropeTop.position, climbSeconds);
            yield return Tween.Move(climber, arrival.position, 0.5f);
            yield return Tween.Face(climber, arrival.position + Vector3.back, 0.3f); // turn toward the visitor
            yield return new WaitForSeconds(0.4f);
        }

        public void ResetMoment()
        {
            climber.SetPositionAndRotation(startPosition, startRotation);
        }
    }
}
