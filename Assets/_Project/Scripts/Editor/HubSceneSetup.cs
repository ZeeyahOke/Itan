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
    /// Small, repeatable steps that add one system at a time to the open Hub scene.
    /// Each item does nothing if the object is already there.
    /// </summary>
    public static class HubSceneSetup
    {
        [MenuItem("Itan/Scene/Add Session Manager")]
        public static void AddSessionManager()
        {
            if (Object.FindFirstObjectByType<SessionManager>() != null)
            {
                Debug.Log("[Itan] Scene already has a SessionManager.");
                return;
            }

            var go = new GameObject("SessionManager");
            go.AddComponent<SessionManager>();
            Undo.RegisterCreatedObjectUndo(go, "Add Session Manager");
            SaveScene();
        }

        // Title panel sits 1.5 m in front of the seat, a little above eye height,
        // between the player and the fire.
        static readonly Vector3 MenuPosition = new Vector3(0f, 1.3f, 1.5f);
        const float PixelsToMetres = 0.001f; // 1000 px wide canvas = 1 m wide panel

        [MenuItem("Itan/Scene/Add Start Menu")]
        public static void AddStartMenu()
        {
            var session = Object.FindFirstObjectByType<SessionManager>();
            if (session == null)
            {
                Debug.LogError("[Itan] Add the Session Manager first.");
                return;
            }
            if (Object.FindFirstObjectByType<StartMenuUI>(FindObjectsInactive.Include) != null)
            {
                Debug.Log("[Itan] Scene already has a start menu.");
                return;
            }

            // World-space canvas the controller rays can point at.
            var canvasGo = new GameObject("StartMenu", typeof(RectTransform));
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGo.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 2f;
            canvasGo.AddComponent<TrackedDeviceGraphicRaycaster>();
            var rect = (RectTransform)canvasGo.transform;
            rect.sizeDelta = new Vector2(1000f, 600f);
            rect.position = MenuPosition;
            rect.localScale = Vector3.one * PixelsToMetres;

            // Panel background.
            var panel = canvasGo.AddComponent<Image>();
            panel.color = new Color(0.06f, 0.05f, 0.14f, 0.88f);

            AddText(rect, "Title", "Ìtàn: Moonlight Tales", 84, FontStyles.Bold,
                    new Color(0.98f, 0.86f, 0.55f), new Vector2(0f, 150f), new Vector2(940f, 120f));
            AddText(rect, "Subtitle", "A Yoruba tale by the fire", 40, FontStyles.Italic,
                    new Color(0.85f, 0.85f, 0.95f), new Vector2(0f, 50f), new Vector2(940f, 60f));

            // Begin button.
            var buttonGo = new GameObject("BeginButton", typeof(RectTransform));
            var buttonRect = (RectTransform)buttonGo.transform;
            buttonRect.SetParent(rect, false);
            buttonRect.anchoredPosition = new Vector2(0f, -140f);
            buttonRect.sizeDelta = new Vector2(360f, 120f);
            var buttonImage = buttonGo.AddComponent<Image>();
            buttonImage.color = new Color(0.85f, 0.45f, 0.15f);
            var button = buttonGo.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.75f, 0.45f);
            colors.pressedColor = new Color(0.7f, 0.35f, 0.1f);
            button.colors = colors;
            AddText(buttonRect, "Label", "Begin", 56, FontStyles.Bold, Color.white, Vector2.zero, buttonRect.sizeDelta);

            // Hook it up.
            var menu = canvasGo.AddComponent<StartMenuUI>();
            var menuSo = new SerializedObject(menu);
            menuSo.FindProperty("beginButton").objectReferenceValue = button;
            menuSo.ApplyModifiedPropertiesWithoutUndo();

            var sessionSo = new SerializedObject(session);
            sessionSo.FindProperty("startMenu").objectReferenceValue = menu;
            sessionSo.ApplyModifiedPropertiesWithoutUndo();

            Undo.RegisterCreatedObjectUndo(canvasGo, "Add Start Menu");
            SaveScene();
            Debug.Log("[Itan] Start menu added.");
        }

        static void AddText(RectTransform parent, string name, string text, float size, FontStyles style,
                            Color color, Vector2 position, Vector2 boxSize)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var r = (RectTransform)go.transform;
            r.SetParent(parent, false);
            r.anchoredPosition = position;
            r.sizeDelta = boxSize;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
        }

        static void SaveScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
