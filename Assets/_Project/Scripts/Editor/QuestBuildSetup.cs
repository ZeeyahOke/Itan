using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.XR.OpenXR;

namespace Itan.EditorTools
{
    /// <summary>
    /// One-click Quest 2 build setup, so the settings are written down in code
    /// instead of only living in the Project Settings window.
    /// Menu: Itan > Setup > Apply Quest Build Settings
    /// </summary>
    public static class QuestBuildSetup
    {
        const string AppId = "com.ZeeyahOke.Itan";

        [MenuItem("Itan/Setup/Apply Quest Build Settings")]
        public static void Apply()
        {
            // 1. Android is the active platform.
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            // 2. Quest needs IL2CPP and 64-bit ARM.
            var android = NamedBuildTarget.Android;
            PlayerSettings.SetScriptingBackend(android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // 3. App identity and Android API levels (Quest 2 runs Android 12L or newer).
            PlayerSettings.companyName = "ZeeyahOke";
            PlayerSettings.productName = "Itan";
            PlayerSettings.SetApplicationIdentifier(android, AppId);
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel32;

            // 4. OpenXR on Android: keep the Meta Quest feature group, turn off the
            //    Android XR (Samsung/Google headset) support that the template adds,
            //    because this build only targets Quest.
            var openXr = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if (openXr != null)
            {
                foreach (var feature in openXr.GetFeatures())
                {
                    string name = feature.GetType().Name;
                    if (name == "MetaQuestFeature" || name == "OculusTouchControllerProfile")
                        feature.enabled = true;
                    else if (name == "AndroidXRSupportFeature")
                        feature.enabled = false;
                }
                EditorUtility.SetDirty(openXr);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[Itan] Quest build settings applied: Android, IL2CPP, ARM64, min API 32, Meta Quest OpenXR feature on.");
        }

        /// <summary>
        /// Builds the release APK to Builds/Itan.apk (signed with the debug key, which is
        /// what sideloading needs). Menu: Itan > Build > Build Quest APK
        /// </summary>
        [MenuItem("Itan/Build/Build Quest APK")]
        public static void BuildApk()
        {
            Apply();
            Directory.CreateDirectory("Builds");
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "Builds/Itan.apk",
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None,
            };
            EditorUserBuildSettings.buildAppBundle = false;
            var report = BuildPipeline.BuildPlayer(options);
            var sum = report.summary;
            Debug.Log($"[Itan] Build {sum.result}: {sum.outputPath}, {sum.totalSize / 1048576f:F1} MB, {sum.totalErrors} error(s), {sum.totalWarnings} warning(s), {sum.totalTime.TotalMinutes:F1} min.");
        }
    }
}
