using Itan.Narration;
using Itan.Player;
using Itan.Story;
using Itan.Session;
using Itan.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Unity.XR.CoreUtils;

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

        // Eye point for a seated visitor, above the player's stool, facing the fire.
        static readonly Vector3 SeatEyePosition = new Vector3(0f, 1.15f, 0f);

        [MenuItem("Itan/Scene/Add Seat Recenter")]
        public static void AddSeatRecenter()
        {
            var session = Object.FindFirstObjectByType<SessionManager>();
            var origin = Object.FindFirstObjectByType<XROrigin>();
            if (session == null || origin == null) { Debug.LogError("[Itan] Need a SessionManager and an XR Origin."); return; }
            if (origin.GetComponent<SeatRecenter>() != null) { Debug.Log("[Itan] Seat recenter already added."); return; }

            var eye = new GameObject("SeatEye").transform;
            var playerSpot = GameObject.Find("PlayerSpot");
            if (playerSpot != null) eye.SetParent(playerSpot.transform);
            eye.SetPositionAndRotation(SeatEyePosition, Quaternion.LookRotation(Vector3.forward)); // toward the fire

            var seat = origin.gameObject.AddComponent<SeatRecenter>();
            var seatSo = new SerializedObject(seat);
            seatSo.FindProperty("origin").objectReferenceValue = origin;
            seatSo.FindProperty("seatEye").objectReferenceValue = eye;
            seatSo.ApplyModifiedPropertiesWithoutUndo();

            var sessionSo = new SerializedObject(session);
            sessionSo.FindProperty("seat").objectReferenceValue = seat;
            sessionSo.ApplyModifiedPropertiesWithoutUndo();

            SaveScene();
            Debug.Log("[Itan] Seat recenter added.");
        }

        // The elder sits on the stool across the fire; his voice comes from head height.
        static readonly Vector3 ElderHeadOffset = new Vector3(0f, 1.05f, 0.05f);
        // Subtitles sit low, between the player and the fire, tilted up toward the eyes.
        static readonly Vector3 SubtitlePosition = new Vector3(0f, 0.82f, 1.35f);

        [MenuItem("Itan/Scene/Add Narration")]
        public static void AddNarration()
        {
            var session = Object.FindFirstObjectByType<SessionManager>();
            if (session == null) { Debug.LogError("[Itan] Add the Session Manager first."); return; }
            if (Object.FindFirstObjectByType<NarrationManager>() != null) { Debug.Log("[Itan] Narration already added."); return; }

            // Elder root on his stool, with a spatial voice source at head height.
            var elderSpot = GameObject.Find("ElderSpot");
            var elder = new GameObject("Elder").transform;
            if (elderSpot != null) elder.SetPositionAndRotation(elderSpot.transform.position, elderSpot.transform.rotation);

            var voiceGo = new GameObject("Voice");
            voiceGo.transform.SetParent(elder, false);
            voiceGo.transform.localPosition = ElderHeadOffset;
            var voice = voiceGo.AddComponent<AudioSource>();
            voice.playOnAwake = false;
            voice.spatialBlend = 1f;                       // fully 3D
            voice.rolloffMode = AudioRolloffMode.Logarithmic;
            voice.minDistance = 1.5f;                      // full volume at the player's seat (~3.4 m away is still clear)
            voice.maxDistance = 20f;
            voice.dopplerLevel = 0f;

            // Subtitle panel.
            var canvasGo = new GameObject("Subtitles", typeof(RectTransform));
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGo.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 2f;
            var rect = (RectTransform)canvasGo.transform;
            rect.sizeDelta = new Vector2(1100f, 180f);
            rect.SetPositionAndRotation(SubtitlePosition, Quaternion.Euler(20f, 0f, 0f));
            rect.localScale = Vector3.one * PixelsToMetres;

            var panelGo = new GameObject("Panel", typeof(RectTransform));
            var panelRect = (RectTransform)panelGo.transform;
            panelRect.SetParent(rect, false);
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
            var bg = panelGo.AddComponent<Image>();
            bg.color = new Color(0.04f, 0.04f, 0.1f, 0.78f);
            bg.raycastTarget = false;
            AddText(panelRect, "Text", "", 44, FontStyles.Normal, new Color(0.96f, 0.94f, 0.88f), Vector2.zero, new Vector2(1040f, 160f));

            var subtitles = canvasGo.AddComponent<SubtitlePanel>();
            var subSo = new SerializedObject(subtitles);
            subSo.FindProperty("panel").objectReferenceValue = panelGo;
            subSo.FindProperty("text").objectReferenceValue = panelRect.Find("Text").GetComponent<TextMeshProUGUI>();
            subSo.ApplyModifiedPropertiesWithoutUndo();

            // Narration manager.
            var narration = new GameObject("Narration").AddComponent<NarrationManager>();
            var narSo = new SerializedObject(narration);
            narSo.FindProperty("voice").objectReferenceValue = voice;
            narSo.FindProperty("subtitles").objectReferenceValue = subtitles;
            narSo.ApplyModifiedPropertiesWithoutUndo();

            var sessionSo = new SerializedObject(session);
            sessionSo.FindProperty("narration").objectReferenceValue = narration;
            sessionSo.FindProperty("opening").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<OpeningScript>("Assets/_Project/Data/Opening.asset");
            sessionSo.ApplyModifiedPropertiesWithoutUndo();

            SaveScene();
            Debug.Log("[Itan] Narration, elder voice and subtitles added.");
        }

        static void SaveScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
