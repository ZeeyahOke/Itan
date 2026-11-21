using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Itan.Story;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Itan.EditorTools
{
    /// <summary>
    /// The elder's narration. The spoken lines are English; the Yoruba words live in the
    /// subtitles and in F's own recordings (the words, the call, the answer, the songs),
    /// which are left alone.
    ///
    /// Two ways to make the audio:
    ///   Neural voice (used now): Itan > Narrator > Export Narration Script writes
    ///   Logs/narration_script.json; Kokoro (Apache 2.0, voice bm_george)
    ///   speaks it into Audio/Narration; then Itan > Narrator >
    ///   Link Narration Clips points each line at its file.
    ///   Quick fallback: Itan > Narrator > Generate Narration uses the Mac's "say" voice.
    /// </summary>
    public static class NarratorVoiceTool
    {
        // The elder's voice and pace. Change these and regenerate to try another voice.
        public const string VoiceName = "Daniel";
        public const int WordsPerMinute = 150;

        const string Data = "Assets/_Project/Data/";
        const string OutFolder = "Assets/_Project/Audio/Narration";
        const string RecordedFolder = "Assets/_Project/Audio/Voice/";

        [MenuItem("Itan/Narrator/List Mac Voices")]
        public static void ListVoices()
        {
            string output = Run("/usr/bin/say", "-v ?");
            Directory.CreateDirectory("Logs");
            File.WriteAllText(Path.Combine("Logs", "mac_voices.txt"), output);
            Debug.Log("[Narrator] " + output.Split('\n').Length + " voices listed in Logs/mac_voices.txt");
        }

        [MenuItem("Itan/Narrator/Generate Narration")]
        public static void Generate()
        {
            Directory.CreateDirectory(OutFolder);
            var jobs = Jobs();

            // 1. Speak every line to a WAV file.
            string textFile = Path.Combine("Temp", "itan_say.txt");
            foreach (var job in jobs)
            {
                string text = Spoken(job.line);
                File.WriteAllText(textFile, text);
                string wav = Path.GetFullPath(Path.Combine(OutFolder, job.file + ".wav"));
                Run("/usr/bin/say", "-v \"" + VoiceName + "\" -r " + WordsPerMinute +
                    " --file-format=WAVE --data-format=LEI16@44100 -o \"" + wav + "\" -f \"" + Path.GetFullPath(textFile) + "\"");
            }

            // 2. Import them and point each line at its new clip.
            AssetDatabase.Refresh();
            foreach (var job in jobs)
            {
                job.line.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(OutFolder + "/" + job.file + ".wav");
                if (job.line.clip == null) Debug.LogWarning("[Narrator] No clip for " + job.file);
                EditorUtility.SetDirty(job.asset);
            }
            AssetDatabase.SaveAssets();
            StoryScriptTool.ApplyImportSettings();
            Debug.Log("[Narrator] Generated " + jobs.Count + " narration lines with the voice " + VoiceName);
        }

        static List<(Object asset, NarrationLine line, string file)> Jobs()
        {
            var jobs = new List<(Object asset, NarrationLine line, string file)>();
            var opening = AssetDatabase.LoadAssetAtPath<OpeningScript>(Data + "Opening.asset");
            Add(jobs, opening, opening.welcome, "Opening_Welcome");
            Add(jobs, opening, opening.bowPrompt, "Opening_Bow");
            Add(jobs, opening, opening.calabashPrompt, "Opening_Calabash");
            Add(jobs, opening, opening.callIntro, "Opening_CallIntro");
            Add(jobs, opening, opening.answerExample, "Opening_AnswerExample");
            Add(jobs, opening, opening.call, "Opening_Call");

            foreach (var name in new[] { "Beat_1_Famine", "Beat_2_Rope", "Beat_3_Fall" })
            {
                var beat = AssetDatabase.LoadAssetAtPath<StoryBeat>(Data + name + ".asset");
                for (int i = 0; i < beat.narration.Count; i++) Add(jobs, beat, beat.narration[i], name + "_Line" + (i + 1));
                for (int i = 0; i < beat.afterMoment.Count; i++) Add(jobs, beat, beat.afterMoment[i], name + "_After" + (i + 1));
            }

            var closing = AssetDatabase.LoadAssetAtPath<ClosingScript>(Data + "Closing.asset");
            Add(jobs, closing, closing.quizIntro, "Closing_QuizIntro");
            Add(jobs, closing, closing.lesson, "Closing_Lesson");
            Add(jobs, closing, closing.farewell, "Closing_Farewell");
            return jobs;
        }

        static string Spoken(NarrationLine line) => string.IsNullOrWhiteSpace(line.spokenText) ? line.subtitle : line.spokenText;

        /// <summary>Writes every narrated line (file name and what is spoken) for an external voice.</summary>
        [MenuItem("Itan/Narrator/Export Narration Script")]
        public static void Export()
        {
            var sb = new System.Text.StringBuilder("[\n");
            var jobs = Jobs();
            for (int i = 0; i < jobs.Count; i++)
                sb.Append("  {\"file\": \"" + jobs[i].file + "\", \"text\": \"" + Spoken(jobs[i].line).Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"}" + (i < jobs.Count - 1 ? ",\n" : "\n"));
            sb.Append("]\n");
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/narration_script.json", sb.ToString());
            Debug.Log("[Narrator] Exported " + jobs.Count + " lines to Logs/narration_script.json");
        }

        /// <summary>Points each narrated line at the WAV of the same name in Audio/Narration.</summary>
        [MenuItem("Itan/Narrator/Link Narration Clips")]
        public static void Link()
        {
            AssetDatabase.Refresh();
            var jobs = Jobs();
            foreach (var job in jobs)
            {
                job.line.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(OutFolder + "/" + job.file + ".wav");
                if (job.line.clip == null) Debug.LogWarning("[Narrator] No clip for " + job.file);
                EditorUtility.SetDirty(job.asset);
            }
            AssetDatabase.SaveAssets();
            StoryScriptTool.ApplyImportSettings();
            Debug.Log("[Narrator] Linked " + jobs.Count + " narration clips.");
        }

        // Only lines that don't already use one of F's recordings.
        static void Add(List<(Object, NarrationLine, string)> jobs, Object asset, NarrationLine line, string file)
        {
            if (line == null || string.IsNullOrWhiteSpace(line.subtitle)) return;
            if (line.clip != null && AssetDatabase.GetAssetPath(line.clip).StartsWith(RecordedFolder)) return;
            jobs.Add((asset, line, file));
        }

        static string Run(string file, string args)
        {
            var psi = new ProcessStartInfo(file, args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using var p = Process.Start(psi);
            string stdout = p.StandardOutput.ReadToEnd();
            string stderr = p.StandardError.ReadToEnd();
            p.WaitForExit();
            if (p.ExitCode != 0) Debug.LogWarning("[Narrator] " + file + " exited " + p.ExitCode + ": " + stderr);
            return stdout;
        }
    }
}
