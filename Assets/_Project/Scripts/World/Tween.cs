using System.Collections;
using UnityEngine;

namespace Itan.World
{
    /// <summary>Tiny movement helpers for the story's simple animations.</summary>
    public static class Tween
    {
        public static IEnumerator Move(Transform t, Vector3 to, float seconds)
        {
            Vector3 from = t.position;
            for (float e = 0f; e < seconds; e += Time.deltaTime)
            {
                t.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, e / seconds));
                yield return null;
            }
            t.position = to;
        }

        /// <summary>Turn on the spot to face a point (flat, no tilting).</summary>
        public static IEnumerator Face(Transform t, Vector3 point, float seconds)
        {
            Vector3 dir = point - t.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) yield break;
            Quaternion from = t.rotation, to = Quaternion.LookRotation(dir);
            for (float e = 0f; e < seconds; e += Time.deltaTime)
            {
                t.rotation = Quaternion.Slerp(from, to, e / seconds);
                yield return null;
            }
            t.rotation = to;
        }

        public static IEnumerator Wait(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }
    }
}
