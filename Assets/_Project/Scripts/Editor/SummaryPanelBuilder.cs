using Itan.Session;
using Itan.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Itan.EditorTools
{
    /// <summary>
    /// Builds the end-of-visit summary panel (adire frame, words learned,
    /// quiz score, Finish button) where the start menu sits.
    /// Menu: Itan > Scene > Add Summary Panel
    /// </summary>
    public static class SummaryPanelBuilder
    {
        static readonly Vector3 Position = new Vector3(0f, 1.3f, 1.5f);

        [MenuItem("Itan/Scene/Add Summary Panel")]
        public static void Build()
        {
            var session = Object.FindFirstObjectByType<SessionManager>();
            if (session == null) { Debug.LogError("[Itan] Add the Session Manager first."); return; }
            if (Object.FindFirstObjectByType<SummaryPanel>(FindObjectsInactive.Include) != null) { Debug.Log("[Itan] Summary panel already added."); return; }

            var root = new GameObject("SummaryPanel", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            root.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 2f;
            root.AddComponent<TrackedDeviceGraphicRaycaster>();
            var rect = (RectTransform)root.transform;
            rect.sizeDelta = new Vector2(1000f, 680f);
            rect.position = Position;
            rect.localScale = Vector3.one * 0.001f;

            var frame = root.AddComponent<Image>();
            frame.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/Adire_Tile.png");
            frame.type = Image.Type.Tiled;
            frame.pixelsPerUnitMultiplier = 4f;
            var inner = new GameObject("Inner", typeof(RectTransform));
            var ir = (RectTransform)inner.transform;
            ir.SetParent(rect, false);
            ir.anchorMin = Vector2.zero; ir.anchorMax = Vector2.one;
            ir.offsetMin = new Vector2(40f, 40f); ir.offsetMax = new Vector2(-40f, -40f);
            var innerImg = inner.AddComponent<Image>();
            innerImg.color = new Color(0.06f, 0.05f, 0.14f, 1f);
            innerImg.raycastTarget = false;

            Text(rect, "Title", "Well done!", 76, FontStyles.Bold, new Color(0.98f, 0.86f, 0.55f), new Vector2(0f, 230f), 100f);
            Text(rect, "Subtitle", "Words in your calabash", 36, FontStyles.Italic, new Color(0.85f, 0.85f, 0.95f), new Vector2(0f, 160f), 50f);
            var words = Text(rect, "Words", "ajá   dog", 48, FontStyles.Normal, Color.white, new Vector2(0f, 40f), 200f);
            words.lineSpacing = 10f;
            var score = Text(rect, "Score", "Quiz: 0 of 3", 44, FontStyles.Bold, new Color(0.98f, 0.86f, 0.55f), new Vector2(0f, -110f), 60f);

            // Finish button.
            var b = new GameObject("FinishButton", typeof(RectTransform));
            var br = (RectTransform)b.transform;
            br.SetParent(rect, false);
            br.anchoredPosition = new Vector2(0f, -220f);
            br.sizeDelta = new Vector2(320f, 100f);
            var bImg = b.AddComponent<Image>();
            bImg.color = new Color(0.85f, 0.45f, 0.15f);
            var button = b.AddComponent<Button>();
            button.targetGraphic = bImg;
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.75f, 0.45f);
            colors.pressedColor = new Color(0.7f, 0.35f, 0.1f);
            button.colors = colors;
            Text(br, "Label", "Finish", 50, FontStyles.Bold, Color.white, Vector2.zero, 100f, 320f);

            var panel = root.AddComponent<SummaryPanel>();
            var so = new SerializedObject(panel);
            so.FindProperty("wordsText").objectReferenceValue = words;
            so.FindProperty("scoreText").objectReferenceValue = score;
            so.FindProperty("finishButton").objectReferenceValue = button;
            so.ApplyModifiedPropertiesWithoutUndo();

            var sessionSo = new SerializedObject(session);
            sessionSo.FindProperty("summary").objectReferenceValue = panel;
            sessionSo.ApplyModifiedPropertiesWithoutUndo();

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Itan] Summary panel added.");
        }

        static TextMeshProUGUI Text(RectTransform parent, string name, string text, float size, FontStyles style,
                                    Color color, Vector2 pos, float height, float width = 880f)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var r = (RectTransform)go.transform;
            r.SetParent(parent, false);
            r.anchoredPosition = pos;
            r.sizeDelta = new Vector2(width, height);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = TextAlignmentOptions.CenterGeoAligned;
            t.raycastTarget = false;
            return t;
        }
    }
}
