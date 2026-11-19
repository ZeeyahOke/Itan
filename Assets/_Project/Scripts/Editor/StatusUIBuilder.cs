using Itan.Interaction;
using Itan.Session;
using Itan.Story;
using Itan.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static Itan.EditorTools.GreyboxMaterials;

namespace Itan.EditorTools
{
    /// <summary>
    /// Gives the calabash its adire skin and an open mouth that fills with gold,
    /// and adds the small status label above it.
    /// Menu: Itan > Scene > Add Status UI
    /// </summary>
    public static class StatusUIBuilder
    {
        // Above the lap, tilted up toward the eyes.
        static readonly Vector3 LabelPosition = new Vector3(0f, 0.8f, 0.48f);
        static readonly Quaternion LabelRotation = Quaternion.Euler(40f, 0f, 0f);

        [MenuItem("Itan/Scene/Add Status UI")]
        public static void Build()
        {
            var session = Object.FindFirstObjectByType<SessionManager>();
            var story = Object.FindFirstObjectByType<StoryManager>();
            var moment = Object.FindFirstObjectByType<WordCardMoment>();
            var calabash = Object.FindFirstObjectByType<Calabash>(FindObjectsInactive.Include);
            if (session == null || story == null || moment == null || calabash == null) { Debug.LogError("[Itan] Need Session, Story, Word card moment and Calabash."); return; }
            if (Object.FindFirstObjectByType<StatusUI>(FindObjectsInactive.Include) != null) { Debug.Log("[Itan] Status UI already added."); return; }

            var fill = DressCalabash(calabash.transform);

            // Label panel.
            var canvasGo = new GameObject("StatusLabel", typeof(RectTransform));
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGo.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 2f;
            var rect = (RectTransform)canvasGo.transform;
            rect.sizeDelta = new Vector2(420f, 130f);
            rect.SetPositionAndRotation(LabelPosition, LabelRotation);
            rect.localScale = Vector3.one * 0.001f;

            var panel = new GameObject("Panel", typeof(RectTransform));
            var pr = (RectTransform)panel.transform;
            pr.SetParent(rect, false);
            pr.anchorMin = Vector2.zero; pr.anchorMax = Vector2.one; pr.offsetMin = pr.offsetMax = Vector2.zero;
            var frame = panel.AddComponent<Image>();
            frame.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/Adire_Tile.png");
            frame.type = Image.Type.Tiled;
            frame.pixelsPerUnitMultiplier = 6f;
            frame.raycastTarget = false;
            var inner = new GameObject("Inner", typeof(RectTransform));
            var ir = (RectTransform)inner.transform;
            ir.SetParent(pr, false);
            ir.anchorMin = Vector2.zero; ir.anchorMax = Vector2.one; ir.offsetMin = new Vector2(18f, 18f); ir.offsetMax = new Vector2(-18f, -18f);
            var innerImg = inner.AddComponent<Image>();
            innerImg.color = new Color(0.06f, 0.05f, 0.14f, 1f);
            innerImg.raycastTarget = false;

            var word = Label(pr, "WordLabel", 40, FontStyles.Bold, new Color(0.98f, 0.86f, 0.55f), new Vector2(0f, 20f));
            var beat = Label(pr, "BeatLabel", 30, FontStyles.Italic, new Color(0.85f, 0.85f, 0.95f), new Vector2(0f, -25f));

            var status = canvasGo.AddComponent<StatusUI>();
            var so = new SerializedObject(status);
            so.FindProperty("story").objectReferenceValue = story;
            so.FindProperty("wordMoment").objectReferenceValue = moment;
            so.FindProperty("fill").objectReferenceValue = fill;
            so.FindProperty("panel").objectReferenceValue = panel;
            so.FindProperty("wordLabel").objectReferenceValue = word;
            so.FindProperty("beatLabel").objectReferenceValue = beat;
            so.ApplyModifiedPropertiesWithoutUndo();

            var sessionSo = new SerializedObject(session);
            sessionSo.FindProperty("status").objectReferenceValue = status;
            sessionSo.ApplyModifiedPropertiesWithoutUndo();

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Itan] Status UI and calabash fill added.");
        }

        static CalabashFill DressCalabash(Transform calabash)
        {
            // Adire skin.
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/UI/Adire_Calabash.png");
            var gourd = calabash.GetComponent<Renderer>().sharedMaterial;
            gourd.SetTexture("_BaseMap", tex);
            gourd.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(gourd);

            // Swap the closed neck for an open mouth with gold "contents" inside.
            var neck = calabash.Find("Neck");
            if (neck != null) Object.DestroyImmediate(neck.gameObject);
            var mouth = Get("Calabash_Mouth", new Color(0.12f, 0.07f, 0.04f));
            var gold = Get("Calabash_Contents", new Color(1f, 0.8f, 0.35f), unlit: true);
            Part("Mouth", PrimitiveType.Cylinder, calabash, new Vector3(0f, 0.47f, 0f), new Vector3(0.62f, 0.03f, 0.62f), mouth);
            var contents = Part("Contents", PrimitiveType.Cylinder, calabash, new Vector3(0f, 0.5f, 0f), new Vector3(0.58f, 0.02f, 0.58f), gold);

            var fill = calabash.gameObject.AddComponent<CalabashFill>();
            var so = new SerializedObject(fill);
            so.FindProperty("contents").objectReferenceValue = contents;
            so.ApplyModifiedPropertiesWithoutUndo();
            return fill;
        }

        static TextMeshProUGUI Label(RectTransform parent, string name, float size, FontStyles style, Color color, Vector2 pos)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var r = (RectTransform)go.transform;
            r.SetParent(parent, false);
            r.anchoredPosition = pos;
            r.sizeDelta = new Vector2(380f, 45f);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.fontSize = size;
            t.fontStyle = style;
            t.color = color;
            t.alignment = TextAlignmentOptions.CenterGeoAligned;
            t.raycastTarget = false;
            return t;
        }
    }
}
