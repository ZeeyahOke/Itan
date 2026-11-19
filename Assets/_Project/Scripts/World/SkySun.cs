using UnityEngine;

namespace Itan.World
{
    /// <summary>Tells the sunset sky shader where the sun is (it lives on the Sun light).</summary>
    [ExecuteAlways]
    public class SkySun : MonoBehaviour
    {
        static readonly int SunDirection = Shader.PropertyToID("_ItanSunDirection");

        void OnEnable() => Push();
        void Update() { if (transform.hasChanged) { Push(); transform.hasChanged = false; } }

        void Push() => Shader.SetGlobalVector(SunDirection, -transform.forward);
    }
}
