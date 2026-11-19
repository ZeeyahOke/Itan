using UnityEngine;

namespace Itan.World
{
    /// <summary>
    /// Plays named animation states on a character. Ìjàpá has two copies of his model
    /// (whole and cracked shell), so this drives every Animator under the character.
    /// States that a controller doesn't have are simply skipped.
    /// </summary>
    public class CharacterActor : MonoBehaviour
    {
        public const string Idle = "Idle", Walk = "Walk", Sneak = "Sneak", Climb = "Climb", Fall = "Fall",
                            Sing = "Sing", Sit = "Sit", Eat = "Eat", Talk = "Talk", Wave = "Wave";

        Animator[] animators;

        void Awake() => animators = GetComponentsInChildren<Animator>(true);

        public void Play(string state, float fade = 0.25f)
        {
            if (animators == null) Awake();
            int hash = Animator.StringToHash(state);
            foreach (var a in animators)
            {
                if (a == null || !a.isActiveAndEnabled || a.runtimeAnimatorController == null) continue;
                if (a.HasState(0, hash)) a.CrossFadeInFixedTime(hash, fade);
            }
        }
    }
}
