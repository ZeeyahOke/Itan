using System.Collections.Generic;
using Itan.Story;
using UnityEditor;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>
    /// Creates the vertical slice data: 3 word cards and 3 story beats,
    /// wired to placeholder audio. Skips any asset that already exists,
    /// so hand edits in the Inspector are never overwritten.
    /// Menu: Itan > Build > Create Slice Data
    /// </summary>
    public static class SliceDataBuilder
    {
        const string DataFolder = "Assets/_Project/Data";
        const string AudioFolder = "Assets/_Project/Audio/Placeholder";

        [MenuItem("Itan/Build/Create Slice Data")]
        public static void Build()
        {
            var aja = CreateWord("Word_Aja", "ajá", "dog", "ah-JAH", "PH_Word_Aja");
            var iya = CreateWord("Word_Iya", "ìyá", "mother", "ee-YAH", "PH_Word_Iya");
            var ijapa = CreateWord("Word_Ijapa", "ìjàpá", "tortoise", "ee-jah-PAH", "PH_Word_Ijapa");

            CreateBeat("Beat_1_Famine", 1, "The Famine", aja,
                ("PH_Beat1_Line1", "Long ago, a great famine came to the land. Every pot was empty."),
                ("PH_Beat1_Line2", "But Ajá, the dog, never looked hungry. Ìjàpá, the tortoise, wanted to know why."));

            CreateBeat("Beat_2_Rope", 2, "The Rope to the Sky", iya,
                ("PH_Beat2_Line1", "Each night Ajá sang to his mother, who lived up in the sky."),
                ("PH_Beat2_Line2", "His mother let down a rope, and Ajá climbed up to eat with her."));

            CreateBeat("Beat_3_Fall", 3, "Ìjàpá Falls", ijapa,
                ("PH_Beat3_Line1", "Ìjàpá copied the song, and the rope came down for him too."),
                ("PH_Beat3_Line2", "Halfway up, the rope was cut. Ìjàpá fell, and his shell cracked into pieces."));

            AssetDatabase.SaveAssets();
            Debug.Log("[Itan] Slice data ready in " + DataFolder);
        }

        [MenuItem("Itan/Build/Create Opening Data")]
        public static void BuildOpening()
        {
            string path = DataFolder + "/Opening.asset";
            if (AssetDatabase.LoadAssetAtPath<OpeningScript>(path) != null) { Debug.Log("[Itan] Opening data already exists."); return; }

            var opening = ScriptableObject.CreateInstance<OpeningScript>();
            opening.welcome = Line("PH_Welcome", "Welcome, my child. Come, sit by the fire with us.");
            opening.bowPrompt = Line("PH_BowPrompt", "Before a story is told, we greet our elders. Bow your head to greet me.");
            opening.calabashPrompt = Line("PH_CalabashPrompt", "Good. Now take this calabash. It will hold the words you learn tonight.");
            opening.call = Line("PH_Call", "Àlọ́ o!   (Answer: Ààlọ̀!)");
            AssetDatabase.CreateAsset(opening, path);
            AssetDatabase.SaveAssets();
            Debug.Log("[Itan] Opening data created at " + path);
        }

        static NarrationLine Line(string clip, string subtitle) =>
            new NarrationLine { clip = LoadClip(clip), subtitle = subtitle };

        static WordCard CreateWord(string fileName, string word, string meaning, string pronunciation, string clipName)
        {
            string path = DataFolder + "/" + fileName + ".asset";
            var existing = AssetDatabase.LoadAssetAtPath<WordCard>(path);
            if (existing != null) return existing;

            var card = ScriptableObject.CreateInstance<WordCard>();
            card.yorubaWord = word;
            card.englishMeaning = meaning;
            card.pronunciation = pronunciation;
            card.elderClip = LoadClip(clipName);
            AssetDatabase.CreateAsset(card, path);
            return card;
        }

        static void CreateBeat(string fileName, int number, string title, WordCard word,
                               params (string clip, string subtitle)[] lines)
        {
            string path = DataFolder + "/" + fileName + ".asset";
            if (AssetDatabase.LoadAssetAtPath<StoryBeat>(path) != null) return;

            var beat = ScriptableObject.CreateInstance<StoryBeat>();
            beat.beatNumber = number;
            beat.title = title;
            beat.wordCard = word;
            beat.narration = new List<NarrationLine>();
            foreach (var (clip, subtitle) in lines)
                beat.narration.Add(new NarrationLine { clip = LoadClip(clip), subtitle = subtitle });
            AssetDatabase.CreateAsset(beat, path);
        }

        static AudioClip LoadClip(string name)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder + "/" + name + ".wav");
            if (clip == null) Debug.LogWarning("[Itan] Placeholder clip missing: " + name);
            return clip;
        }
    }
}
