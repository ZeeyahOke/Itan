using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Itan.EditorTools
{
    /// <summary>
    /// Builds a TextMesh Pro font asset from Noto Sans (SIL Open Font License),
    /// which has every Yoruba letter and tone mark (ọ, ẹ, ṣ, combining tones),
    /// then makes it the default and applies it to every text in the open scene.
    /// Menu: Itan > Build > Apply Yoruba Font
    /// </summary>
    public static class YorubaFontSetup
    {
        const string Folder = "Assets/_Project/Art/Fonts";
        const string RegularTtf = Folder + "/NotoSans-Regular.ttf";
        const string BoldTtf = Folder + "/NotoSans-Bold.ttf";
        const string RegularAsset = Folder + "/NotoSans-Regular SDF.asset";
        const string BoldAsset = Folder + "/NotoSans-Bold SDF.asset";

        // Characters we know the slice shows, baked in up front so nothing has to be added on the headset.
        const string Preload =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 .,:;!?'\"()-…–—‘’“”/&" +
            "ÀÁÈÉÌÍÒÓÙÚàáèéìíòóùúẸẹỌọṢṣ̀́";

        [MenuItem("Itan/Build/Apply Yoruba Font")]
        public static void Apply()
        {
            var regular = GetOrCreate(RegularTtf, RegularAsset);
            var bold = GetOrCreate(BoldTtf, BoldAsset);
            if (regular == null || bold == null) return;

            // Bold text uses the real bold face instead of a faked one.
            regular.fontWeightTable[7].regularTypeface = bold;
            EditorUtility.SetDirty(regular);

            // New text defaults to Noto.
            var settings = new SerializedObject(TMP_Settings.instance);
            settings.FindProperty("m_defaultFontAsset").objectReferenceValue = regular;
            settings.ApplyModifiedPropertiesWithoutUndo();

            // Existing text in the scene switches to Noto.
            int count = 0;
            foreach (var text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                text.font = regular;
                // Noto's tall line spacing sits text low in its box; centre on the letters instead.
                if (text.alignment == TextAlignmentOptions.Center)
                    text.alignment = TextAlignmentOptions.CenterGeoAligned;
                EditorUtility.SetDirty(text);
                count++;
            }

            AssetDatabase.SaveAssets();
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Itan] Yoruba font applied to " + count + " text objects.");
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
