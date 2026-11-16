using Itan.Session;
using UnityEditor;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>
    /// Editor-only shortcut for testing the session flow in Play mode,
    /// same as pressing N in the Game view.
    /// </summary>
    public static class SessionDebugMenu
    {
        [MenuItem("Itan/Debug/Skip To Next State")]
        static void SkipState()
        {
            var session = Object.FindFirstObjectByType<SessionManager>();
            session?.CurrentState?.Skip();
        }

        [MenuItem("Itan/Debug/Skip To Next State", true)]
        static bool CanSkipState() => Application.isPlaying;
    }
}
