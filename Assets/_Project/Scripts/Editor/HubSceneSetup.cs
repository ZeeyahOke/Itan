using Itan.Detection;
using Itan.Interaction;
using Itan.Narration;
using Itan.Player;
using Itan.Story;
using Itan.Voice;
using Itan.Session;
using Itan.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
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
            tmp.alignment = TextAlignmentOptions.CenterGeoAligned;
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

        [MenuItem("Itan/Scene/Add Greeting Detector")]
        public static void AddGreetingDetector()
        {
            var session = Object.FindFirstObjectByType<SessionManager>();
            var origin = Object.FindFirstObjectByType<XROrigin>();
            if (session == null || origin == null) { Debug.LogError("[Itan] Need a SessionManager and an XR Origin."); return; }
            if (Object.FindFirstObjectByType<GreetingDetector>() != null) { Debug.Log("[Itan] Greeting detector already added."); return; }

            var detector = new GameObject("GreetingDetector").AddComponent<GreetingDetector>();
            var so = new SerializedObject(detector);
            so.FindProperty("head").objectReferenceValue = origin.Camera.transform;
            so.ApplyModifiedPropertiesWithoutUndo();

            var sessionSo = new SerializedObject(session);
            sessionSo.FindProperty("greeting").objectReferenceValue = detector;
            sessionSo.ApplyModifiedPropertiesWithoutUndo();

            SaveScene();
            Debug.Log("[Itan] Greeting detector added.");
        }

        // Calabash waits on a stand to the front-right, within arm's reach of the seat.
        static readonly Vector3 CalabashStandPosition = new Vector3(0.45f, 0f, 0.5f);
        // Lap: low and just in front of the seated visitor, easy to see by looking down.
        static readonly Vector3 LapPosition = new Vector3(0f, 0.62f, 0.32f);

        [MenuItem("Itan/Scene/Add Calabash")]
        public static void AddCalabash()
        {
            var session = Object.FindFirstObjectByType<SessionManager>();
            if (session == null) { Debug.LogError("[Itan] Add the Session Manager first."); return; }
            if (Object.FindFirstObjectByType<Calabash>(FindObjectsInactive.Include) != null) { Debug.Log("[Itan] Calabash already added."); return; }

            var gourd = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Greybox_Gourd.mat");
            if (gourd == null)
            {
                gourd = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                gourd.SetColor("_BaseColor", new Color(0.72f, 0.45f, 0.2f));
                gourd.SetFloat("_Smoothness", 0.35f);
                AssetDatabase.CreateAsset(gourd, "Assets/_Project/Art/Materials/Greybox_Gourd.mat");
            }
            var wood = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Materials/Greybox_Wood.mat");

            // Stand (static, always there).
            var stand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stand.name = "CalabashStand";
            stand.transform.position = CalabashStandPosition + new Vector3(0f, 0.3f, 0f);
            stand.transform.localScale = new Vector3(0.22f, 0.3f, 0.22f);
            if (wood != null) stand.GetComponent<Renderer>().sharedMaterial = wood;
            GameObjectUtility.SetStaticEditorFlags(stand, (StaticEditorFlags)~0);

            // Lap point.
            var lap = new GameObject("LapPoint").transform;
            var playerSpot = GameObject.Find("PlayerSpot");
            if (playerSpot != null) lap.SetParent(playerSpot.transform);
            lap.position = LapPosition;

            // Calabash: a squashed gourd with a short neck.
            var calabashGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            calabashGo.name = "Calabash";
            calabashGo.transform.position = CalabashStandPosition + new Vector3(0f, 0.72f, 0f);
            calabashGo.transform.localScale = new Vector3(0.3f, 0.22f, 0.3f);
            calabashGo.GetComponent<Renderer>().sharedMaterial = gourd;
            var neck = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            neck.name = "Neck";
            Object.DestroyImmediate(neck.GetComponent<Collider>());
            neck.transform.SetParent(calabashGo.transform, false);
            neck.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            neck.transform.localScale = new Vector3(0.35f, 0.15f, 0.35f);
            neck.GetComponent<Renderer>().sharedMaterial = gourd;

            var body = calabashGo.AddComponent<Rigidbody>();
            body.isKinematic = true;     // floats on its stand until grabbed
            body.useGravity = false;
            var grab = calabashGo.AddComponent<XRGrabInteractable>();
            grab.throwOnDetach = false;
            var calabash = calabashGo.AddComponent<Calabash>();
            var so = new SerializedObject(calabash);
            so.FindProperty("lapPoint").objectReferenceValue = lap;
            so.ApplyModifiedPropertiesWithoutUndo();
            calabashGo.SetActive(false); // hidden until the elder offers it

            var sessionSo = new SerializedObject(session);
            sessionSo.FindProperty("calabash").objectReferenceValue = calabash;
            sessionSo.ApplyModifiedPropertiesWithoutUndo();

            SaveScene();
            Debug.Log("[Itan] Calabash, stand and lap point added.");
        }

        [MenuItem("Itan/Scene/Add Mic Permission")]
        public static void AddMicPermission()
        {
            var session = Object.FindFirstObjectByType<SessionManager>();
            if (session == null) { Debug.LogError("[Itan] Add the Session Manager first."); return; }
            if (session.GetComponent<MicPermission>() != null) { Debug.Log("[Itan] Mic permission already added."); return; }
            session.gameObject.AddComponent<MicPermission>();
            SaveScene();
            Debug.Log("[Itan] Mic permission request added.");
        }

        // Voice prompt sits under the subtitles, so it never covers the elder.
        static readonly Vector3 VoicePromptPosition = new Vector3(0f, 0.66f, 1.3f);
        static readonly Quaternion VoicePromptRotation = Quaternion.Euler(28f, 0f, 0f);

        [MenuItem("Itan/Scene/Add Voice Check")]
        public static void AddVoiceCheck()
        {
            var session = Object.FindFirstObjectByType<SessionManager>();
            if (session == null) { Debug.LogError("[Itan] Add the Session Manager first."); return; }
            if (Object.FindFirstObjectByType<MicResponseDetector>(FindObjectsInactive.Include) != null) { Debug.Log("[Itan] Voice check already added."); return; }

            var canvasGo = new GameObject("VoicePrompt", typeof(RectTransform));
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGo.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 2f;
            canvasGo.AddComponent<TrackedDeviceGraphicRaycaster>();
            var rect = (RectTransform)canvasGo.transform;
            rect.sizeDelta = new Vector2(520f, 140f);
            rect.SetPositionAndRotation(VoicePromptPosition, VoicePromptRotation);
            rect.localScale = Vector3.one * PixelsToMetres;

            // "Listening" cue.
            var cue = new GameObject("ListeningCue", typeof(RectTransform));
            var cueRect = (RectTransform)cue.transform;
            cueRect.SetParent(rect, false);
            cueRect.sizeDelta = new Vector2(520f, 80f);
            var cueBg = cue.AddComponent<Image>();
            cueBg.color = new Color(0.06f, 0.05f, 0.14f, 0.8f);
            cueBg.raycastTarget = false;
            AddText(cueRect, "Label", "Listening... say it aloud", 40, FontStyles.Italic,
                    new Color(0.98f, 0.86f, 0.55f), Vector2.zero, cueRect.sizeDelta);

            // Fallback button.
            var fallback = new GameObject("FallbackPanel", typeof(RectTransform));
            var fbRect = (RectTransform)fallback.transform;
            fbRect.SetParent(rect, false);
            fbRect.sizeDelta = new Vector2(420f, 120f);
            var fbImage = fallback.AddComponent<Image>();
            fbImage.color = new Color(0.85f, 0.45f, 0.15f);
            var button = fallback.AddComponent<Button>();
            button.targetGraphic = fbImage;
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.75f, 0.45f);
            colors.pressedColor = new Color(0.7f, 0.35f, 0.1f);
            button.colors = colors;
            AddText(fbRect, "Label", "Tap to answer", 48, FontStyles.Bold, Color.white, Vector2.zero, fbRect.sizeDelta);

            var detector = canvasGo.AddComponent<MicResponseDetector>();
            var so = new SerializedObject(detector);
            so.FindProperty("listeningCue").objectReferenceValue = cue;
            so.FindProperty("fallbackPanel").objectReferenceValue = fallback;
            so.FindProperty("fallbackButton").objectReferenceValue = button;
            so.ApplyModifiedPropertiesWithoutUndo();

            var sessionSo = new SerializedObject(session);
            sessionSo.FindProperty("voice").objectReferenceValue = detector;
            sessionSo.ApplyModifiedPropertiesWithoutUndo();

            SaveScene();
            Debug.Log("[Itan] Voice check and fallback button added.");
        }

        /// <summary>
        /// Puts the world-space panels back at their layout positions. Re-run after
        /// changing the positions above, to tune the layout in one place.
        /// </summary>
        [MenuItem("Itan/Scene/Apply UI Layout")]
        public static void ApplyUiLayout()
        {
            Place("StartMenu", MenuPosition, Quaternion.identity);
            Place("Subtitles", SubtitlePosition, Quaternion.Euler(20f, 0f, 0f));
            Place("VoicePrompt", VoicePromptPosition, VoicePromptRotation);
            SaveScene();
            Debug.Log("[Itan] UI layout applied.");
        }

        static void Place(string name, Vector3 position, Quaternion rotation)
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                // GameObject.Find skips inactive objects, so look through all canvases too.
                foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    if (c.name == name) { go = c.gameObject; break; }
            }
            if (go != null) go.transform.SetPositionAndRotation(position, rotation);
        }

        [MenuItem("Itan/Scene/Add Story Manager")]
        public static void AddStoryManager()
        {
            var session = Object.FindFirstObjectByType<SessionManager>();
            var narration = Object.FindFirstObjectByType<NarrationManager>();
            if (session == null || narration == null) { Debug.LogError("[Itan] Need a SessionManager and Narration."); return; }
            if (Object.FindFirstObjectByType<StoryManager>() != null) { Debug.Log("[Itan] Story manager already added."); return; }

            var story = new GameObject("Story").AddComponent<StoryManager>();
            var so = new SerializedObject(story);
            so.FindProperty("narration").objectReferenceValue = narration;
            var beats = so.FindProperty("beats");
            var paths = new[] { "Beat_1_Famine", "Beat_2_Rope", "Beat_3_Fall" };
            beats.arraySize = paths.Length;
            for (int i = 0; i < paths.Length; i++)
                beats.GetArrayElementAtIndex(i).objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<StoryBeat>("Assets/_Project/Data/" + paths[i] + ".asset");
            so.ApplyModifiedPropertiesWithoutUndo();

            var sessionSo = new SerializedObject(session);
            sessionSo.FindProperty("story").objectReferenceValue = story;
            sessionSo.ApplyModifiedPropertiesWithoutUndo();

            SaveScene();
            Debug.Log("[Itan] Story manager added with 3 beats.");
        }

        static void SaveScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
}
