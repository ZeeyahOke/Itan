using Itan.World;
using UnityEditor;

namespace Itan.EditorTools
{
    /// <summary>Editor toggles for testing the story without a headset.</summary>
    public static class StoryDebugToggles
    {
        const string AutoClimb = "Itan/Debug/Story/Auto Climb (toggle)";

        [MenuItem(AutoClimb)]
        static void ToggleAutoClimb()
        {
            PlayerClimbMoment.DebugAutoClimb = !PlayerClimbMoment.DebugAutoClimb;
            UnityEngine.Debug.Log("[Debug] Auto climb " + (PlayerClimbMoment.DebugAutoClimb ? "on" : "off"));
        }

        [MenuItem(AutoClimb, true)]
        static bool ValidateAutoClimb()
        {
            Menu.SetChecked(AutoClimb, PlayerClimbMoment.DebugAutoClimb);
            return true;
        }
    }
}
