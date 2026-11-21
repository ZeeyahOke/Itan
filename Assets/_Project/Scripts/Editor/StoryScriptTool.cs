using System.Collections.Generic;
using Itan.Interaction;
using Itan.Quiz;
using Itan.Story;
using Itan.UI;
using Itan.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>
    /// The whole script of the slice in one place, plus the audio wiring.
    /// "Apply Story Script" writes every line into the data assets, uses F's
    /// recordings for the words, the call and Ajá's song, and hooks up the
    /// ambience, drum and stings in the scene. Narration for the remaining
    /// lines is then generated with Itan > Narrator > Generate Narration.
    ///
    /// subtitle = what the visitor reads. spoken = what the narrator voice reads
    /// (Yoruba names spelled out so the English voice says them properly).
    /// </summary>
    public static class StoryScriptTool
    {
        const string Data = "Assets/_Project/Data/";
        const string Voice = "Assets/_Project/Audio/Voice/";
        const string Sfx = "Assets/_Project/Audio/SFX/";

        // ---------------- The script ----------------
        // Line(subtitle, spoken): the subtitle keeps the Yoruba words; the narrator speaks English.

        static void WriteOpening(OpeningScript o)
        {
            o.welcome = Line("Welcome, my child. Come, sit by the fire with us.");
            o.bowPrompt = Line("Before a story is told, we greet our elders. Bow your head to greet me.");
            o.calabashPrompt = Line("Good. Now take this calabash. It will hold the words you learn tonight.");
            o.callIntro = Line("Every story begins with a call. When I say Àlọ́ o, you answer Ààlọ̀. Listen.",
                               "Every story begins with a call. When I call out to you, you answer me back. Listen.");
            o.answerExample = Recorded("Ààlọ̀!", "Answer_Aalo");
            o.call = Recorded("Àlọ́ o!   (Answer: Ààlọ̀!)", "Call_AloO");
        }

        static void WriteBeats(StoryBeat famine, StoryBeat rope, StoryBeat fall)
        {
            famine.title = "The Famine";
            famine.narration = Lines(
                ("Long ago, a great famine came to the land. Every pot was empty, and every field was dry.", null),
                ("But Ajá the dog never looked hungry. Each night he slipped away to a quiet place, and sang a secret song to his mother in the sky.",
                 "But the dog never looked hungry. Each night he slipped away to a quiet place, and sang a secret song to his mother in the sky."));
            famine.afterMoment = Lines(("And down from the clouds came a rope.", null));

            rope.title = "The Rope to the Sky";
            rope.narration = Lines(
                ("Ajá climbed the rope, up and up, all the way to his mother's home.",
                 "The dog climbed the rope, up and up, all the way to his mother's home."));
            rope.afterMoment = Lines(
                ("His mother, his ìyá, fed him well, and every morning he came home strong.",
                 "His mother fed him well, and every morning he came home strong."));

            fall.title = "Ìjàpá Falls";
            fall.narration = Lines(
                ("Ìjàpá the tortoise was curious. Why was Ajá so strong, when everyone else was hungry?",
                 "The tortoise was curious. Why was the dog so strong, when everyone else was hungry?"),
                ("One night he followed Ajá, hid in the long grass, and heard the secret song. The next night, Ìjàpá sang it himself.",
                 "One night he followed the dog, hid in the long grass, and heard the secret song. The next night, the tortoise sang it himself."));
            fall.afterMoment = Lines(
                ("But Ajá's mother knew her son's voice. When the tortoise was halfway up, she cut the rope.",
                 "But the dog's mother knew her son's voice. When the tortoise was halfway up, she cut the rope."),
                ("Down fell Ìjàpá, and his shell cracked into pieces.",
                 "Down fell the tortoise, and his shell cracked into pieces."));
        }

        static void WriteClosing(ClosingScript c)
        {
            c.quizIntro = Line("Now, let us see what is in your calabash. Listen to each word, and choose the card that matches.");
            c.lesson = Line("And that, my child, is why Ìjàpá's shell is cracked to this day. Being curious is good, but taking what is not yours will always let you fall.",
                            "And that, my child, is why the tortoise's shell is cracked to this day. Being curious is good, but taking what is not yours will always let you fall.");
            c.farewell = Line("You carry three new words home tonight: ajá, ìyá and ìjàpá. Ó dàbọ̀, go well.",
                              "You carry three new words home tonight: dog, mother, and tortoise. Goodbye, go well.");
        }

        // ---------------- Apply ----------------

        [MenuItem("Itan/Build/Apply Story Script and Audio")]
        public static void Apply()
        {
            var opening = AssetDatabase.LoadAssetAtPath<OpeningScript>(Data + "Opening.asset");
            var closing = AssetDatabase.LoadAssetAtPath<ClosingScript>(Data + "Closing.asset");
            var famine = AssetDatabase.LoadAssetAtPath<StoryBeat>(Data + "Beat_1_Famine.asset");
            var rope = AssetDatabase.LoadAssetAtPath<StoryBeat>(Data + "Beat_2_Rope.asset");
            var fall = AssetDatabase.LoadAssetAtPath<StoryBeat>(Data + "Beat_3_Fall.asset");

            WriteOpening(opening);
            WriteBeats(famine, rope, fall);
            WriteClosing(closing);

            // The words the visitor learns use F's own recordings.
            SetWordClip("Word_Aja");
            SetWordClip("Word_Iya");
            SetWordClip("Word_Ijapa");

            foreach (Object o in new Object[] { opening, closing, famine, rope, fall }) EditorUtility.SetDirty(o);
            AssetDatabase.SaveAssets();

            ApplyImportSettings();
            WireSceneAudio();
            Debug.Log("[Script] Story script and audio applied.");
        }

        static void SetWordClip(string name)
        {
            var card = AssetDatabase.LoadAssetAtPath<WordCard>(Data + name + ".asset");
            card.elderClip = Clip(Voice + name + ".wav");
            EditorUtility.SetDirty(card);
        }

        // ---------------- Scene audio ----------------

        static void WireSceneAudio()
        {
            // Songs: from Ajá and Ìjàpá themselves (they live in the StoryWorld scene).
            StoryScenes.OpenStory();
            var aja = Find("Aja");
            var ijapa = Find("Ijapa");
            var ropeMoment = Object.FindFirstObjectByType<RopeDropMoment>(FindObjectsInactive.Include);
            var fallMoment = Object.FindFirstObjectByType<FallMoment>(FindObjectsInactive.Include);
            Set(ropeMoment, "singer", Voice3D(aja, 1.2f));
            Set(ropeMoment, "song", Clip(Voice + "Song_Aja.wav"));
            Set(fallMoment, "singer", Voice3D(ijapa, 1.2f));
            Set(fallMoment, "song", Clip(Voice + "Song_Ijapa.wav"));
            Set(fallMoment, "crack", Clip(Sfx + "Crack.wav"));

            // Word card sting.
            var moment = Object.FindFirstObjectByType<WordCardMoment>();
            var stingSource = moment.GetComponent<AudioSource>();
            stingSource.clip = Clip(Sfx + "Sting_Word.wav");

            // Quiz stings.
            var quiz = Object.FindFirstObjectByType<CardQuiz>();
            var quizSfx = GetOrAdd(quiz.gameObject);
            Flat(quizSfx);
            Set(quiz, "sfx", quizSfx);
            Set(quiz, "rightSting", Clip(Sfx + "Sting_Right.wav"));
            Set(quiz, "wrongSting", Clip(Sfx + "Sting_Wrong.wav"));

            // Summary sting.
            var summary = Object.FindFirstObjectByType<SummaryPanel>(FindObjectsInactive.Include);
            var sumSfx = GetOrAdd(summary.gameObject);
            Flat(sumSfx);
            sumSfx.clip = Clip(Sfx + "Sting_Finish.wav");
            Set(summary, "sting", sumSfx);

            // Talking drum with every smoke journey.
            var transition = Object.FindFirstObjectByType<SceneTransition>();
            var drum = GetOrAdd(transition.gameObject);
            Flat(drum);
            drum.clip = Clip(Sfx + "Drum_Talking.wav");
            drum.volume = 0.8f;
            Set(transition, "drum", drum);

            // Night ambience everywhere (2D loop) and fire crackle at the fire (hidden with the hub).
            var ambience = GameObject.Find("Ambience");
            if (ambience == null) ambience = new GameObject("Ambience");
            var night = GetOrAdd(ambience);
            Flat(night);
            night.clip = Clip(Sfx + "Ambience_Night.wav");
            night.loop = true;
            night.playOnAwake = true;
            night.volume = 0.35f;

            var firePit = Find("FirePit");
            var fire = GetOrAdd(firePit);
            fire.clip = Clip(Sfx + "Fire_Crackle.wav");
            fire.loop = true;
            fire.playOnAwake = true;
            fire.spatialBlend = 1f;
            fire.rolloffMode = AudioRolloffMode.Logarithmic;
            fire.minDistance = 1.2f;
            fire.maxDistance = 12f;
            fire.volume = 0.55f;
            fire.dopplerLevel = 0f;

            StoryScenes.SaveAll();
        }

        static AudioSource Voice3D(GameObject who, float height)
        {
            var t = who.transform.Find("Voice");
            if (t == null)
            {
                t = new GameObject("Voice").transform;
                t.SetParent(who.transform, false);
                t.localPosition = new Vector3(0f, height * 0.6f, 0.3f);
            }
            var src = GetOrAdd(t.gameObject);
            src.playOnAwake = false;
            src.spatialBlend = 1f;
            src.rolloffMode = AudioRolloffMode.Logarithmic;
            src.minDistance = 3f;      // still clear from the seat, 6 m away
            src.maxDistance = 25f;
            src.dopplerLevel = 0f;
            return src;
        }

        // Unity returns a "fake null" for missing components in the editor, so ?? can't be used here.
        static AudioSource GetOrAdd(GameObject go) =>
            go.TryGetComponent(out AudioSource s) ? s : go.AddComponent<AudioSource>();

        static void Flat(AudioSource s)
        {
            s.playOnAwake = false;
            s.spatialBlend = 0f;
        }

        // ---------------- Import settings (task 4.6) ----------------

        /// <summary>Vorbis everywhere, mono; long ambience streams from disk.</summary>
        public static void ApplyImportSettings()
        {
            foreach (var folder in new[] { "Assets/_Project/Audio/Voice", "Assets/_Project/Audio/SFX", "Assets/_Project/Audio/Narration" })
            {
                if (!AssetDatabase.IsValidFolder(folder)) continue;
                foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new[] { folder }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var importer = (AudioImporter)AssetImporter.GetAtPath(path);
                    var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                    bool longLoop = path.Contains("Ambience") || path.Contains("Fire_Crackle");

                    var s = importer.defaultSampleSettings;
                    s.compressionFormat = AudioCompressionFormat.Vorbis;
                    s.quality = 0.7f;
                    s.loadType = longLoop ? AudioClipLoadType.Streaming
                               : clip != null && clip.length > 5f ? AudioClipLoadType.CompressedInMemory
                               : AudioClipLoadType.DecompressOnLoad;
                    s.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
                    importer.defaultSampleSettings = s;
                    importer.forceToMono = true;
                    importer.loadInBackground = longLoop;
                    importer.SaveAndReimport();
                }
            }
        }

        // ---------------- Helpers ----------------

        static NarrationLine Line(string subtitle, string spoken = null) =>
            new NarrationLine { subtitle = subtitle, spokenText = spoken ?? "" };

        static NarrationLine Recorded(string subtitle, string voiceClip) =>
            new NarrationLine { subtitle = subtitle, clip = Clip(Voice + voiceClip + ".wav") };

        static List<NarrationLine> Lines(params (string subtitle, string spoken)[] lines)
        {
            var list = new List<NarrationLine>();
            foreach (var (subtitle, spoken) in lines) list.Add(Line(subtitle, spoken));
            return list;
        }

        static AudioClip Clip(string path)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null) Debug.LogWarning("[Script] Missing audio: " + path);
            return clip;
        }

        static void Set(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject Find(string name) => StoryScenes.Find(name);
    }
}
