using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>
    /// Editor only: copies every Console message during Play mode into
    /// Logs/itan_play.log (ignored by Git), so a test run can be read back
    /// as plain text even when the Console window isn't visible.
    /// </summary>
    [InitializeOnLoad]
    public static class PlayLogMirror
    {
        static readonly string LogPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "itan_play.log");
        static readonly object Gate = new object();

        static PlayLogMirror()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            Application.logMessageReceivedThreaded += OnLog;
        }

        static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath));
                File.WriteAllText(LogPath, "=== Play started " + DateTime.Now.ToString("HH:mm:ss") + " ===\n");
            }
        }

        static void OnLog(string message, string stackTrace, LogType type)
        {
            if (!Application.isPlaying) return;
            string line = DateTime.Now.ToString("HH:mm:ss.f") + " [" + type + "] " + message + "\n";
            if (type == LogType.Error || type == LogType.Exception) line += stackTrace + "\n";
            lock (Gate) File.AppendAllText(LogPath, line);
        }
    }
}
