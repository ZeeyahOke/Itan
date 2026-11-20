using System.Collections;
using Itan.Story;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace Itan.World
{
    /// <summary>
    /// The visitor climbs the rope after Ajá. Grip the rope with either hand and pull
    /// down to rise, hand over hand. Reaching the cloud carries them to the feast.
    /// If nobody climbs for a while (or in the editor with auto-climb on) it climbs for them.
    /// </summary>
    public class PlayerClimbMoment : MonoBehaviour, IKeyMoment
    {
        [Tooltip("Eye position at the foot of the rope, facing it.")]
        [SerializeField] Transform ropeBase;
        [Tooltip("The rope; only its X and Z are used, as the climbing line.")]
        [SerializeField] Transform rope;
        [Tooltip("Floor point on the cloud where the visitor arrives, facing the feast.")]
        [SerializeField] Transform skyViewpoint;
        [SerializeField] float seatedEyeHeight = 1.15f;

        [Tooltip("Head height at which the climb is finished (just below the cloud).")]
        [SerializeField] float finishHeight = 3.4f;
        [SerializeField] float grabReach = 0.45f;
        [SerializeField] InputActionReference leftGrip;
        [SerializeField] InputActionReference rightGrip;
        [Tooltip("Climb automatically if there's been no progress for this long.")]
        [SerializeField] float autoClimbAfter = 30f;
        [SerializeField] GameObject prompt;

        /// <summary>Editor/testing: climb without hands.</summary>
        public static bool DebugAutoClimb;

        public static bool IsClimbing { get; private set; }

        XROrigin origin;
        SceneTransition transition;
        Transform leftHand, rightHand;

        void Awake()
        {
            if (prompt != null) prompt.SetActive(false);
        }

        void Find()
        {
            if (origin == null) origin = FindFirstObjectByType<XROrigin>();
            if (transition == null) transition = FindFirstObjectByType<SceneTransition>();
            if (origin != null && (leftHand == null || rightHand == null))
                foreach (var t in origin.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "Left Controller") leftHand = t;
                    else if (t.name == "Right Controller") rightHand = t;
                }
        }

        public IEnumerator Play()
        {
            Find();
            if (origin == null) yield break;
            IsClimbing = true;
            leftGrip?.action.Enable();
            rightGrip?.action.Enable();

            yield return FadeOut();
            PlaceEye(ropeBase.position, ropeBase.forward);
            if (prompt != null) prompt.SetActive(true);
            yield return FadeIn();

            var hands = new[] { new Hand(leftHand, leftGrip, XRNode.LeftHand), new Hand(rightHand, rightGrip, XRNode.RightHand) };
            float lastProgress = Time.time, best = Head.y;
            while (Head.y < finishHeight)
            {
                if (DebugAutoClimb || Time.time - lastProgress > autoClimbAfter) { yield return AutoClimb(); break; }

                float lift = 0f;
                foreach (var h in hands) lift += h.Update(origin, rope.position, grabReach);
                if (lift > 0f) origin.transform.position += Vector3.up * lift;
                // Never sink below the foot of the rope.
                if (Head.y < ropeBase.position.y) origin.transform.position += Vector3.up * (ropeBase.position.y - Head.y);

                if (Head.y > best + 0.05f) { best = Head.y; lastProgress = Time.time; }
                yield return null;
            }

            if (prompt != null) prompt.SetActive(false);
            yield return FadeOut();
            PlaceEye(skyViewpoint.position + Vector3.up * seatedEyeHeight, skyViewpoint.forward);
            yield return FadeIn();
            IsClimbing = false;
        }

        IEnumerator AutoClimb()
        {
            float from = Head.y;
            for (float t = 0f; t < 3f; t += Time.deltaTime)
            {
                float y = Mathf.Lerp(from, finishHeight + 0.01f, Mathf.SmoothStep(0f, 1f, t / 3f));
                origin.transform.position += Vector3.up * (y - Head.y);
                yield return null;
            }
        }

        Vector3 Head => origin.Camera.transform.position;

        void PlaceEye(Vector3 eye, Vector3 forward)
        {
            Vector3 headForward = Vector3.ProjectOnPlane(origin.Camera.transform.forward, Vector3.up);
            Vector3 want = Vector3.ProjectOnPlane(forward, Vector3.up);
            if (headForward.sqrMagnitude > 0.0001f && want.sqrMagnitude > 0.0001f)
                origin.RotateAroundCameraUsingOriginUp(Vector3.SignedAngle(headForward, want, Vector3.up));
            origin.MoveCameraToWorldLocation(eye);
        }

        IEnumerator FadeOut() { if (transition != null) yield return transition.FadeOut(); }
        IEnumerator FadeIn() { if (transition != null) yield return transition.FadeIn(); }

        public void ResetMoment()
        {
            StopAllCoroutines();
            IsClimbing = false;
            if (prompt != null) prompt.SetActive(false);
        }

        /// <summary>One hand on the rope: while gripping near it, pulling down lifts the visitor.</summary>
        class Hand
        {
            readonly Transform hand;
            readonly InputActionReference grip;
            readonly XRNode node;
            bool holding;
            float lastLocalY;

            public Hand(Transform hand, InputActionReference grip, XRNode node) { this.hand = hand; this.grip = grip; this.node = node; }

            public float Update(XROrigin origin, Vector3 ropePoint, float reach)
            {
                if (hand == null || grip == null) return 0f;
                bool pressed = grip.action.ReadValue<float>() > 0.5f;
                float localY = origin.transform.InverseTransformPoint(hand.position).y;
                if (!pressed) { holding = false; return 0f; }

                if (!holding)
                {
                    var flat = hand.position - ropePoint; flat.y = 0f;
                    if (flat.magnitude > reach) return 0f;
                    holding = true;
                    lastLocalY = localY;
                    InputDevices.GetDeviceAtXRNode(node).SendHapticImpulse(0, 0.5f, 0.08f);
                    return 0f;
                }
                float pull = lastLocalY - localY; // hand moving down in the rig = body moving up
                lastLocalY = localY;
                return Mathf.Max(0f, pull) * origin.transform.lossyScale.y;
            }
        }
    }
}
