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
    /// Builds the QuizCard prefab: an adire-framed card with the meaning in the
    /// middle, the whole card a button the controller ray can press.
    /// Menu: Itan > Build > Create Quiz Card Prefab
    /// </summary>
    public static class QuizCardBuilder
    {
        public const string PrefabPath = "Assets/_Project/Prefabs/QuizCard.prefab";
        static readonly Vector3 TestCardPosition = new Vector3(0f, 1.15f, 1.05f);

        [MenuItem("Itan/Build/Create Quiz Card Prefab")]
        public static void CreatePrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null) { Debug.Log("[Itan] Quiz card prefab already exists."); return; }

            var root = new GameObject("QuizCard", typeof(RectTransform));
            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            root.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 2f;
            root.AddComponent<TrackedDeviceGraphicRaycaster>();
            var rect = (RectTransform)root.transform;
            rect.sizeDelta = new Vector2(340f, 440f);
            rect.localScale = Vector3.one * 0.001f;

            // Adire frame is the button's target graphic (it brightens on hover).
            var frameGo = new GameObject("Frame", typeof(RectTransform));
            var fr = (RectTransform)frameGo.transform;
            fr.SetParent(rect, false);
            Stretch(fr, 0f);
            var frame = frameGo.AddComponent<Image>();
            frame.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/Adire_Tile.png");
            frame.type = Image.Type.Tiled;
            frame.pixelsPerUnitMultiplier = 4f;
            var button = frameGo.AddComponent<Button>();
            button.targetGraphic = frame;
            var colors = button.colors;
            colors.normalColor = new Color(0.8f, 0.8f, 0.8f);
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(1f, 0.85f, 0.5f);
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f);
            button.colors = colors;

            var innerGo = new GameObject("Inner", typeof(RectTransform));
            var ir = (RectTransform)innerGo.transform;
            ir.SetParent(fr, false);
            Stretch(ir, 30f);
            var inner = innerGo.AddComponent<Image>();
            inner.color = new Color(0.06f, 0.05f, 0.14f, 1f);
            inner.raycastTarget = false;

            var textGo = new GameObject("Meaning", typeof(RectTransform));
            var tr = (RectTransform)textGo.transform;
            tr.SetParent(ir, false);
            Stretch(tr, 16f);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = "meaning";
            text.fontSize = 56;
            text.fontStyle = FontStyles.Bold;
            text.color = new Color(0.98f, 0.86f, 0.55f);
            text.alignment = TextAlignmentOptions.CenterGeoAligned;
            text.raycastTarget = false;

            var card = root.AddComponent<QuizCard>();
            var so = new SerializedObject(card);
            so.FindProperty("button").objectReferenceValue = button;
            so.FindProperty("meaning").objectReferenceValue = text;
            so.FindProperty("inner").objectReferenceValue = inner;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            Debug.Log("[Itan] Quiz card prefab saved to " + PrefabPath);
        }

        [MenuItem("Itan/Scene/Add Test Quiz Card")]
        public static void AddTestCard()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null) { Debug.LogError("[Itan] Create the quiz card prefab first."); return; }
            var quizRoot = GameObject.Find("QuizCards") ?? new GameObject("QuizCards");
            if (quizRoot.transform.childCount > 0) { Debug.Log("[Itan] Quiz cards already in the scene."); return; }

            var card = (GameObject)PrefabUtility.InstantiatePrefab(prefab, quizRoot.transform);
            card.name = "QuizCard_Test";
            card.transform.position = TestCardPosition;

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Itan] Test quiz card added.");
        }

        static void Stretch(RectTransform r, float inset)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(inset, inset);
            r.offsetMax = new Vector2(-inset, -inset);
        }
    }
}
