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

        // Fires the Begin button's click, the same event a controller ray sends.
        [MenuItem("Itan/Debug/Press Begin Button")]
        static void PressBegin()
        {
            var menu = Object.FindFirstObjectByType<Itan.UI.StartMenuUI>();
            if (menu != null && menu.BeginButton.interactable)
                menu.BeginButton.onClick.Invoke();
        }

        [MenuItem("Itan/Debug/Press Begin Button", true)]
        static bool CanPressBegin() => Application.isPlaying;
    }
}
