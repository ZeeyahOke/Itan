using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Itan.EditorTools
{
    /// <summary>
    /// Story fonts (SIL Open Font License), both with every Yoruba letter and tone mark
    /// (ọ, ẹ, ṣ, combining tones) and proper mark positioning:
    ///   Alegreya (calligraphic, storybook) for display text: titles, the Yoruba word, the word count.
    ///   Alegreya Sans for everything read at length: subtitles, buttons, prompts.
    /// Menu: Itan > Build > Apply Story Fonts (works on every open scene).
    /// </summary>
    public static class YorubaFontSetup
    {
        const string Folder = "Assets/_Project/Art/Fonts";

        // Characters we know the slice shows, baked in up front so nothing has to be added on the headset.
        const string Preload =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,:;!?'\"()-…–—‘’“”/&·" +
            "ÀÁÈÉÌÍÒÓÙÚàáèéìíòóùúẸẹỌọṢṣ\u0300\u0301";

        // Objects (by name) that use the display face.
        static readonly string[] DisplayNames = { "Title", "Yoruba", "WordLabel" };

        [MenuItem("Itan/Build/Apply Story Fonts")]
        public static void Apply()
        {
            var display = GetOrCreate(Folder + "/Alegreya-Bold.ttf", Folder + "/Alegreya-Bold SDF.asset");
            var body = GetOrCreate(Folder + "/AlegreyaSans-Regular.ttf", Folder + "/AlegreyaSans-Regular SDF.asset");
            var bodyBold = GetOrCreate(Folder + "/AlegreyaSans-Bold.ttf", Folder + "/AlegreyaSans-Bold SDF.asset");
            if (display == null || body == null || bodyBold == null) return;

            // Bold text uses the real bold face instead of a faked one.
            body.fontWeightTable[7].regularTypeface = bodyBold;
            EditorUtility.SetDirty(body);

            var settings = new SerializedObject(TMP_Settings.instance);
            settings.FindProperty("m_defaultFontAsset").objectReferenceValue = body;
            settings.ApplyModifiedPropertiesWithoutUndo();

            int count = 0;
            foreach (var text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                bool isDisplay = System.Array.IndexOf(DisplayNames, text.name) >= 0;
                text.font = isDisplay ? display : body;
                if (isDisplay) text.fontStyle &= ~FontStyles.Bold; // the face is already bold
                if (text.alignment == TextAlignmentOptions.Center)
                    text.alignment = TextAlignmentOptions.CenterGeoAligned;
                EditorUtility.SetDirty(text);
                count++;
            }

            // Prefabs too (scene copies are already done above).
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Project/Prefabs" }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var root = PrefabUtility.LoadPrefabContents(path);
                foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    bool isDisplay = System.Array.IndexOf(DisplayNames, text.name) >= 0;
                    text.font = isDisplay ? display : body;
                }
                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[Itan] Story fonts applied to " + count + " text objects.");
        }

        static TMP_FontAsset GetOrCreate(string ttfPath, string assetPath)
        {
            var existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null) return existing;

            var font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (font == null) { Debug.LogError("[Itan] Font file missing: " + ttfPath); return null; }

            var asset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024,
                                                      AtlasPopulationMode.Dynamic, true);
            asset.name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.CreateAsset(asset, assetPath);

            // The atlas texture and material live inside the font asset file.
            asset.atlasTextures[0].name = asset.name + " Atlas";
            AssetDatabase.AddObjectToAsset(asset.atlasTextures[0], asset);
            asset.material.name = asset.name + " Material";
            AssetDatabase.AddObjectToAsset(asset.material, asset);

            asset.TryAddCharacters(Preload, out string missing);
            if (!string.IsNullOrEmpty(missing)) Debug.LogWarning("[Itan] " + asset.name + " is missing: " + missing);

            // Freeze it: every letter the slice uses is now baked in, so the asset
            // never changes at runtime (no repo churn, no glyph hitches on the Quest).
            // To add letters later, delete the SDF asset and run this menu again.
            asset.atlasPopulationMode = AtlasPopulationMode.Static;

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return asset;
        }
    }
}
