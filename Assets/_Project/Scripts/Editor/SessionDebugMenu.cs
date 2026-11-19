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

        [MenuItem("Itan/Debug/Play Test Narration")]
        static void PlayTestLine()
        {
            var session = Object.FindFirstObjectByType<SessionManager>();
            if (session != null && session.Narration != null && session.Opening != null)
                session.Narration.Play(session.Opening.welcome, () => Debug.Log("[Narration] Test line finished."));
        }

        [MenuItem("Itan/Debug/Play Test Narration", true)]
        static bool CanPlayTestLine() => Application.isPlaying;

        [MenuItem("Itan/Debug/Transition/Smoke To Story")]
        static void ToStory() => Object.FindFirstObjectByType<Itan.World.SceneTransition>()?.SmokeToStory();

        [MenuItem("Itan/Debug/Transition/Smoke To Fire")]
        static void ToFire() => Object.FindFirstObjectByType<Itan.World.SceneTransition>()?.SmokeToFire();

        [MenuItem("Itan/Debug/Transition/Smoke To Story", true)]
        [MenuItem("Itan/Debug/Transition/Smoke To Fire", true)]
        static bool CanTransition() => Application.isPlaying;

        // Without a headset the editor camera can't look around; tilt it to check the lap.
        [MenuItem("Itan/Debug/Look At Lap (toggle)")]
        static void LookAtLap()
        {
            var origin = Object.FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
            if (origin == null) return;
            var offset = origin.CameraFloorOffsetObject.transform;
            bool down = offset.localEulerAngles.x < 1f;
            offset.localRotation = Quaternion.Euler(down ? 50f : 0f, 0f, 0f);
        }

        [MenuItem("Itan/Debug/Look At Lap (toggle)", true)]
        static bool CanLookAtLap() => Application.isPlaying;

        [MenuItem("Itan/Debug/Simulate Bow")]
        static void SimulateBow() => Object.FindFirstObjectByType<Itan.Detection.GreetingDetector>()?.SimulateBow();

        [MenuItem("Itan/Debug/Simulate Bow", true)]
        static bool CanSimulateBow() => Application.isPlaying;

        // Same result as grabbing the offered calabash with a controller ray.
        [MenuItem("Itan/Debug/Simulate Grab Calabash")]
        static void SimulateGrab()
        {
            var calabash = Object.FindFirstObjectByType<Itan.Interaction.Calabash>();
            if (calabash != null && calabash.gameObject.activeInHierarchy) calabash.SnapToLap();
        }

        [MenuItem("Itan/Debug/Simulate Grab Calabash", true)]
        static bool CanSimulateGrab() => Application.isPlaying;

        static Itan.Voice.MicResponseDetector Voice => Object.FindFirstObjectByType<Itan.Voice.MicResponseDetector>();

        [MenuItem("Itan/Debug/Voice/Start Test Listen")]
        static void TestListen()
        {
            var v = Voice;
            if (v == null) return;
            v.Missed -= OnTestMiss;
            v.Missed += OnTestMiss;
            v.Listen(() => Debug.Log("[Voice] Test answered"));
        }

        static void OnTestMiss(int misses) => Voice.ListenAgain();

        [MenuItem("Itan/Debug/Voice/Simulate Voice")]
        static void SimulateVoice() => Voice?.SimulateVoice();

        [MenuItem("Itan/Debug/Voice/Simulate Silence")]
        static void SimulateSilence() => Voice?.SimulateSilence();

        [MenuItem("Itan/Debug/Voice/Press Fallback Button")]
        static void PressFallback() => Voice?.PressFallback();

        [MenuItem("Itan/Debug/Voice/Start Test Listen", true)]
        [MenuItem("Itan/Debug/Voice/Simulate Voice", true)]
        [MenuItem("Itan/Debug/Voice/Simulate Silence", true)]
        [MenuItem("Itan/Debug/Voice/Press Fallback Button", true)]
        static bool CanUseVoiceDebug() => Application.isPlaying;
    }
}
