using System.Collections;
using UnityEngine;

namespace Itan.Interaction
{
    /// <summary>
    /// The golden glow inside the calabash's mouth. It grows a third for each
    /// word collected, so the visitor can see their progress by looking down.
    /// </summary>
    public class CalabashFill : MonoBehaviour
    {
        [Tooltip("Gold disc inside the mouth; its width shows how full the calabash is.")]
        [SerializeField] Transform contents;
        [SerializeField] float growSeconds = 0.6f;

        Vector3 fullScale;
        bool initialised;
        float current;

        void Awake() => Apply(0f);

        // Read the disc's full size once, before anything shrinks it. Other scripts can
        // reset the fill before this calabash has woken up, so don't rely on Awake order.
        void EnsureInitialised()
        {
            if (initialised) return;
            fullScale = contents.localScale;
            initialised = true;
        }

        public float Fill => current;

        public void SetFill(float target)
        {
            StopAllCoroutines();
            if (isActiveAndEnabled) StartCoroutine(Grow(Mathf.Clamp01(target)));
            else Apply(Mathf.Clamp01(target));
        }

        public void ResetFill()
        {
            StopAllCoroutines();
            Apply(0f);
        }

        IEnumerator Grow(float target)
        {
            float from = current;
            for (float t = 0f; t < growSeconds; t += Time.deltaTime)
            {
                Apply(Mathf.Lerp(from, target, Mathf.SmoothStep(0f, 1f, t / growSeconds)));
                yield return null;
            }
            Apply(target);
        }

        void Apply(float amount)
        {
            EnsureInitialised();
            current = amount;
            contents.gameObject.SetActive(amount > 0.001f);
            contents.localScale = new Vector3(fullScale.x * amount, fullScale.y, fullScale.z * amount);
        }
    }
}
