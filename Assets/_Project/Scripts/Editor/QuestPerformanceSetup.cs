using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.OpenXR;

namespace Itan.EditorTools
{
    /// <summary>
    /// Day 5 performance pass for Quest 2, written as code so it can be re-applied:
    /// texture compression and sizes, URP quality, Vulkan, multiview and batching.
    /// Menu: Itan > Setup > Apply Quest Performance Settings
    /// </summary>
    public static class QuestPerformanceSetup
    {
        static readonly string[] TextureFolders = { "Assets/_Project/Art", "Assets/_ThirdParty" };

        [MenuItem("Itan/Setup/Apply Quest Performance Settings")]
        public static void Apply()
        {
            int textures = CompressTextures();
            TuneUrp();
            TunePlayer();
            AssetDatabase.SaveAssets();
            Debug.Log($"[Itan] Quest performance settings applied ({textures} texture overrides changed).");
        }

        /// <summary>
        /// ASTC on Android. Scenery and characters top out at 1024 (Quest 2 shows about
        /// 20 pixels per degree, so bigger maps are wasted at story distance); UI and fonts
        /// keep 2048 with a finer block so text stays crisp.
        /// </summary>
        static int CompressTextures()
        {
            int changed = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", TextureFolders))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!(AssetImporter.GetAtPath(path) is TextureImporter ti)) continue;
                bool ui = ti.textureType == TextureImporterType.Sprite || path.Contains("/UI/") || path.Contains("/Fonts/");
                var want = new TextureImporterPlatformSettings
                {
                    name = "Android",
                    overridden = true,
                    maxTextureSize = ui ? 2048 : 1024,
                    format = ui ? TextureImporterFormat.ASTC_4x4 : TextureImporterFormat.ASTC_6x6,
                    compressionQuality = 50,
                    resizeAlgorithm = TextureResizeAlgorithm.Mitchell,
                };
                var have = ti.GetPlatformTextureSettings("Android");
                bool mips = !ui;
                if (have.overridden && have.maxTextureSize == want.maxTextureSize && have.format == want.format && ti.mipmapEnabled == mips) continue;
                ti.SetPlatformTextureSettings(want);
                ti.mipmapEnabled = mips;
                ti.SaveAndReimport();
                changed++;
            }
            return changed;
        }

        static void TuneUrp()
        {
            var asset = GraphicsSettings.defaultRenderPipeline;
            if (asset == null) return;
            var so = new SerializedObject(asset);
            void Set(string prop, int v) { var p = so.FindProperty(prop); if (p != null) p.intValue = v; }
            void SetF(string prop, float v) { var p = so.FindProperty(prop); if (p != null) p.floatValue = v; }
            void SetB(string prop, bool v) { var p = so.FindProperty(prop); if (p != null) p.boolValue = v; }

            Set("m_MSAA", 4);                         // 4x MSAA is close to free on the Quest's tiled GPU
            SetB("m_SupportsHDR", false);
            SetF("m_RenderScale", 1f);
            SetB("m_RequireDepthTexture", false);
            SetB("m_RequireOpaqueTexture", false);
            // Scenery shadows are baked; real-time shadows are only for the characters,
            // who stand up to about 10 m away.
            SetB("m_MainLightShadowsSupported", true);
            Set("m_MainLightShadowmapResolution", 1024);
            SetF("m_ShadowDistance", 12f);
            Set("m_ShadowCascadeCount", 1);
            SetB("m_SoftShadowsSupported", false);
            // The fire is the one extra light: per pixel, one per object, no shadows.
            Set("m_AdditionalLightsRenderingMode", 1);
            Set("m_AdditionalLightsPerObjectLimit", 1);
            SetB("m_AdditionalLightShadowsSupported", false);
            SetB("m_UseSRPBatcher", true);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
        }

        static void TunePlayer()
        {
            var android = NamedBuildTarget.Android;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.Vulkan });
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;
            PlayerSettings.SetManagedStrippingLevel(android, ManagedStrippingLevel.Low);
            PlayerSettings.SetIl2CppCompilerConfiguration(android, Il2CppCompilerConfiguration.Release);
            PlayerSettings.stripEngineCode = true;

            var openXr = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if (openXr != null)
            {
                openXr.renderMode = OpenXRSettings.RenderMode.SinglePassInstanced;   // multiview: both eyes in one pass
                openXr.depthSubmissionMode = OpenXRSettings.DepthSubmissionMode.Depth16Bit;
                EditorUtility.SetDirty(openXr);
            }
        }
    }
}
