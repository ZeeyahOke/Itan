using Itan.Interaction;
using Itan.Narration;
using Itan.Story;
using Itan.UI;
using Itan.Voice;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Itan.EditorTools
{
    /// <summary>
    /// Builds the floating word card (adire frame, glow, word, pronunciation,
    /// meaning) and the WordCardMoment that runs it, then hooks it into the StoryManager.
    /// Menu: Itan > Scene > Add Word Card Moment
    /// </summary>
    public static class WordCardBuilder
    {
        // In front of the seat, at eye height, closer than the story so it reads as "for you".
        static readonly Vector3 CardPosition = new Vector3(0f, 1.2f, 1.15f);
        const float PixelsToMetres = 0.001f;
        const float FrameWidth = 32f;

        [MenuItem("Itan/Scene/Add Word Card Moment")]
        public static void Build()
        {
            var story = Object.FindFirstObjectByType<StoryManager>();
            var narration = Object.FindFirstObjectByType<NarrationManager>();
            var voice = Object.FindFirstObjectByType<MicResponseDetector>(FindObjectsInactive.Include);
            var calabash = Object.FindFirstObjectByType<Calabash>(FindObjectsInactive.Include);
            if (story == null || narration == null || voice == null || calabash == null)
            {
                Debug.LogError("[Itan] Need Story, Narration, Voice check and Calabash first.");
                return;
            }
            if (Object.FindFirstObjectByType<WordCardMoment>() != null) { Debug.Log("[Itan] Word card moment already added."); return; }

            var view = BuildCard();

            var momentGo = new GameObject("WordCardMoment");
            var sting = momentGo.AddComponent<AudioSource>();
            sting.playOnAwake = false;
            sting.spatialBlend = 0f; // a "you did it" sound, not from anywhere in the world
            sting.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/Placeholder/PH_Sting.wav");

            var moment = momentGo.AddComponent<WordCardMoment>();
            var so = new SerializedObject(moment);
            so.FindProperty("card").objectReferenceValue = view;
            so.FindProperty("narration").objectReferenceValue = narration;
            so.FindProperty("voice").objectReferenceValue = voice;
            so.FindProperty("calabash").objectReferenceValue = calabash.transform;
            so.FindProperty("sting").objectReferenceValue = sting;
            so.ApplyModifiedPropertiesWithoutUndo();

            var storySo = new SerializedObject(story);
            storySo.FindProperty("wordMoment").objectReferenceValue = moment;
            storySo.ApplyModifiedPropertiesWithoutUndo();

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Itan] Word card moment added.");
        }

        static WordCardView BuildCard()
        {
            var canvasGo = new GameObject("WordCard", typeof(RectTransform));
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvasGo.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 2f;
            var rect = (RectTransform)canvasGo.transform;
            rect.sizeDelta = new Vector2(600f, 420f);
            rect.position = CardPosition;
            rect.localScale = Vector3.one * PixelsToMetres;

            // Glow behind everything.
            var glow = Img("Glow", rect, new Color(1f, 0.78f, 0.35f, 0f));
            Stretch(glow.rectTransform, -50f);

            // Adire frame with a dark centre.
            var frame = Img("Frame", rect, Color.white);
            Stretch(frame.rectTransform, 0f);
            frame.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/UI/Adire_Tile.png");
            frame.type = Image.Type.Tiled;
            frame.pixelsPerUnitMultiplier = 4f;
            var inner = Img("Inner", rect, new Color(0.06f, 0.05f, 0.14f, 1f));
            Stretch(inner.rectTransform, FrameWidth);

            var yoruba = Text("Yoruba", rect, 110, FontStyles.Bold, new Color(0.98f, 0.86f, 0.55f), new Vector2(0f, 55f), 120f);
            var say = Text("Pronunciation", rect, 40, FontStyles.Italic, new Color(0.85f, 0.85f, 0.95f), new Vector2(0f, -35f), 50f);
            var meaning = Text("Meaning", rect, 52, FontStyles.Normal, Color.white, new Vector2(0f, -115f), 60f);

            var view = canvasGo.AddComponent<WordCardView>();
            var so = new SerializedObject(view);
            so.FindProperty("yoruba").objectReferenceValue = yoruba;
            so.FindProperty("pronunciation").objectReferenceValue = say;
            so.FindProperty("meaning").objectReferenceValue = meaning;
            so.FindProperty("glow").objectReferenceValue = glow;
            so.ApplyModifiedPropertiesWithoutUndo();
            return view;
        }

        static Image Img(string name, RectTransform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        static void Stretch(RectTransform r, float inset)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(inset, inset);
            r.offsetMax = new Vector2(-inset, -inset);
        }

        static TextMeshProUGUI Text(string name, RectTransform parent, float size, FontStyles style, Color color, Vector2 pos, float height)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var r = (RectTransform)go.transform;
            r.SetParent(parent, false);
            r.anchoredPosition = pos;
            r.sizeDelta = new Vector2(540f, height);
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
