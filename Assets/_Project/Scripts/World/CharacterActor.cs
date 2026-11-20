using UnityEngine;

namespace Itan.World
{
    /// <summary>
    /// Plays named animation states on a character. Ìjàpá has two copies of his model
    /// (whole and cracked shell), so this drives every Animator under the character.
    /// States can live on the base layer (whole body) or the upper-body layer (Eat, while
    /// sitting). States a controller doesn't have are simply skipped.
    /// </summary>
    public class CharacterActor : MonoBehaviour
    {
        public const string Idle = "Idle", Walk = "Walk", Sneak = "Sneak", Hide = "Hide", Climb = "Climb",
                            Fall = "Fall", Land = "Land", Sing = "Sing", Sit = "Sit", SitTalk = "SitTalk",
                            Eat = "Eat", Talk = "Talk", Nod = "Nod", UpperNone = "UpperNone";

        [Tooltip("State played when the character first appears and after each reset.")]
        [SerializeField] string startState = Idle;

        Animator[] animators;

        public string StartState => startState;

        void Awake() => animators = GetComponentsInChildren<Animator>(true);

        void OnEnable() => ResetPose();

        public void ResetPose()
        {
            Play(startState, 0f);
            Play(UpperNone, 0f);
        }

        public void Play(string state, float fade = 0.25f)
        {
            if (animators == null) Awake();
            int hash = Animator.StringToHash(state);
            foreach (var a in animators)
            {
                if (a == null || !a.gameObject.activeInHierarchy || a.runtimeAnimatorController == null) continue;
                // The upper-body layer (eating) is switched on and off by weight.
                if (state == UpperNone) { if (a.layerCount > 1) a.SetLayerWeight(1, 0f); continue; }
                for (int layer = 0; layer < a.layerCount; layer++)
                    if (a.HasState(layer, hash))
                    {
                        if (layer > 0) a.SetLayerWeight(layer, 1f);
                        a.CrossFadeInFixedTime(hash, fade, layer);
                        break;
                    }
            }
        }
    }
}
