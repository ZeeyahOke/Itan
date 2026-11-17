using Itan.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Itan.EditorTools
{
    /// <summary>
    /// Gives the start menu an indigo adire border: the panel is filled with the
    /// tiled adire pattern, and a dark inner panel sits on top, leaving a patterned frame.
    /// Menu: Itan > Scene > Add Adire Menu Frame
    /// </summary>
    public static class AdireMenuFrame
    {
        const string HubPath = "Assets/_Project/Scenes/Hub.unity";
        const string TilePath = "Assets/_Project/Art/UI/Adire_Tile.png";
        const float FrameWidth = 40f;            // canvas px, about 4 cm in VR
        const float TilesPerSprite = 4f;         // one motif square = 32 px, so whole squares fit in the border
        const float TextWidth = 860f;            // keeps the title clear of the border

        [MenuItem("Itan/Scene/Add Adire Menu Frame")]
        public static void Add()
        {
            if (EditorSceneManager.GetActiveScene().path != HubPath)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                EditorSceneManager.OpenScene(HubPath);
            }

            var menu = Object.FindFirstObjectByType<StartMenuUI>(FindObjectsInactive.Include);
            if (menu == null) { Debug.LogError("[Itan] No start menu in Hub."); return; }
            Sprite adire = ImportAsTiledSprite(TilePath);
            if (adire == null) { Debug.LogError("[Itan] Adire tile missing at " + TilePath); return; }

            // Whole panel becomes adire cloth...
            var panel = menu.GetComponent<Image>();
            Color darkPanel = panel.sprite == null ? panel.color : new Color(0.06f, 0.05f, 0.14f);
            panel.sprite = adire;
            panel.type = Image.Type.Tiled;
            panel.pixelsPerUnitMultiplier = TilesPerSprite;
            panel.color = Color.white;

            // ...and a dark inner panel covers the middle, so only a border shows.
            // (Re-running updates the existing frame instead of adding a second one.)
            var existing = menu.transform.Find("Inner");
            var inner = existing != null ? existing.gameObject : new GameObject("Inner", typeof(RectTransform), typeof(Image));
            var r = (RectTransform)inner.transform;
            r.SetParent(menu.transform, false);
            r.SetAsFirstSibling(); // drawn before the title, subtitle and button
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(FrameWidth, FrameWidth);
            r.offsetMax = new Vector2(-FrameWidth, -FrameWidth);
            var innerImage = inner.GetComponent<Image>();
            innerImage.color = new Color(darkPanel.r, darkPanel.g, darkPanel.b, 1f);
            innerImage.raycastTarget = false;

            foreach (string textName in new[] { "Title", "Subtitle" })
            {
                var text = menu.transform.Find(textName) as RectTransform;
                if (text != null) text.sizeDelta = new Vector2(TextWidth, text.sizeDelta.y);
            }

            // Slightly smaller title so it stays on one line inside the border.
            var title = menu.transform.Find("Title")?.GetComponent<TMPro.TextMeshProUGUI>();
            if (title != null) title.fontSize = 72f;

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Itan] Adire frame added to the start menu.");
        }

        static Sprite ImportAsTiledSprite(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return null;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.mipmapEnabled = false;             // UI texture (matches the 5.1 rule)
            importer.maxTextureSize = 256;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; // required for tiling
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
    }
}
