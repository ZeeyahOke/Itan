using UnityEngine;

namespace Itan.World
{
    /// <summary>Makes a point light breathe like a wood fire: slow swell plus quick flickers.</summary>
    [RequireComponent(typeof(Light))]
    public class FireFlicker : MonoBehaviour
    {
        [SerializeField] float baseIntensity = 2.2f;
        [SerializeField] float amount = 0.45f;
        [SerializeField] float speed = 7f;

        Light lamp;
        float seed;

        void Awake()
        {
            lamp = GetComponent<Light>();
            seed = Random.value * 100f;
        }

        void Update()
        {
            float t = Time.time * speed + seed;
            float slow = Mathf.PerlinNoise(t * 0.15f, 0.3f) - 0.5f;
            float fast = Mathf.PerlinNoise(t, 1.7f) - 0.5f;
            lamp.intensity = baseIntensity * (1f + amount * (slow + fast * 0.8f));
        }
    }
}
