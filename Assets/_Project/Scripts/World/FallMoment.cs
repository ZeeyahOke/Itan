using System.Collections;
using Itan.Story;
using UnityEngine;

namespace Itan.World
{
    /// <summary>
    /// Beat 3: Ìjàpá climbs the rope, it is cut halfway, he falls and his
    /// whole shell is swapped for the cracked one when he hits the ground.
    /// </summary>
    public class FallMoment : MonoBehaviour, IKeyMoment
    {
        [SerializeField] Transform climber;
        [SerializeField] Transform ropeBottom;
        [SerializeField] Transform ropeTop;
        [SerializeField] Transform ropePivot;
        [Tooltip("Lowers the rope when Ìjàpá sings, exactly as it came down for Ajá.")]
        [SerializeField] RopeDropMoment ropeDrop;
        [SerializeField] GameObject shellWhole;
        [SerializeField] GameObject shellCracked;

        [SerializeField] float walkSeconds = 1.6f;
        [SerializeField] float climbSeconds = 2.2f;
        [SerializeField, Range(0.2f, 0.8f)] float cutAt = 0.5f;   // how far up the rope he gets
        [SerializeField] float fallSeconds = 0.7f;

        [Tooltip("Ìjàpá's voice, at Ìjàpá.")]
        [SerializeField] AudioSource singer;
        [Tooltip("Ìjàpá's croaky copy of Ajá's song.")]
        [SerializeField] AudioClip song;
        [Tooltip("Played when he hits the ground.")]
        [SerializeField] AudioClip crack;

        Vector3 startPosition;
        Quaternion startRotation;
        CharacterActor actor;

        void Awake()
        {
            startPosition = climber.position;
            startRotation = climber.rotation;
            actor = climber.GetComponent<CharacterActor>();
        }

        void Act(string state) { if (actor != null) actor.Play(state); }

        public IEnumerator Play()
        {
            // Out of the long grass, creeping, then walking to the rope.
            Act(CharacterActor.Sneak);
            yield return Tween.Face(climber, ropeBottom.position, 0.3f);
            yield return Tween.Move(climber, ropeBottom.position, walkSeconds);
            Act(CharacterActor.Idle);

            // He sings the stolen song at the foot of the rope.
            if (singer != null && song != null)
            {
                singer.clip = song;
                singer.Play();
                Act(CharacterActor.Sing);
                yield return new WaitForSeconds(song.length);
            }

            // The rope comes down for the stolen song, just as it did for Ajá.
            if (ropeDrop != null)
            {
                Act(CharacterActor.Idle);
                yield return ropeDrop.Drop();
                yield return new WaitForSeconds(0.6f);
            }

            yield return Tween.Face(climber, ropeBottom.position + Vector3.forward, 0.2f);
            Act(CharacterActor.Climb);
            Vector3 halfway = Vector3.Lerp(ropeBottom.position, ropeTop.position, cutAt);
            yield return Tween.Move(climber, halfway, climbSeconds);
            yield return new WaitForSeconds(0.3f);

            // Snip: the rope below the cut disappears.
            ropePivot.localScale = new Vector3(1f, 1f - cutAt, 1f);

            // Fall, speeding up, with a tumble.
            Act(CharacterActor.Fall);
            Vector3 from = climber.position;
            Vector3 ground = ropeBottom.position + new Vector3(0.6f, 0f, -0.4f);
            for (float t = 0f; t < fallSeconds; t += Time.deltaTime)
            {
                float k = (t / fallSeconds) * (t / fallSeconds); // gravity-like ease in
                climber.position = Vector3.Lerp(from, ground, k);
                climber.Rotate(Vector3.forward, 540f * Time.deltaTime, Space.Self);
                yield return null;
            }
            climber.position = ground;
            climber.rotation = Quaternion.LookRotation(Vector3.back); // lands facing the visitor

            // Crack!
            if (singer != null && crack != null) singer.PlayOneShot(crack);
            shellWhole.SetActive(false);
            shellCracked.SetActive(true);
            Act(CharacterActor.Land);
            yield return new WaitForSeconds(1.6f);
        }

        public void ResetMoment()
        {
            if (singer != null) singer.Stop();
            climber.SetPositionAndRotation(startPosition, startRotation);
            shellWhole.SetActive(true);
            shellCracked.SetActive(false);
            if (actor != null) actor.ResetPose();
        }
    }
}
