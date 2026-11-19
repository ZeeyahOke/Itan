using System;
using System.IO;
using UnityEngine;

namespace Itan.Session
{
    /// <summary>
    /// Saves each visit's results as a small JSON file in
    /// Application.persistentDataPath/sessions, one file per visit.
    /// On the Quest that is /sdcard/Android/data/com.ZeeyahOke.Itan/files/sessions.
    /// </summary>
    public class SessionLogger : MonoBehaviour
    {
        public string Save(SessionResults results)
        {
            try
            {
                string folder = Path.Combine(Application.persistentDataPath, "sessions");
                Directory.CreateDirectory(folder);
                string path = Path.Combine(folder, "session_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".json");
                string json = JsonUtility.ToJson(results, true);
                File.WriteAllText(path, json);
                Debug.Log("[Log] Session saved to " + path);
#if UNITY_EDITOR
                Debug.Log("[Log] " + json);
#endif
                return path;
            }
            catch (Exception e)
            {
                // Logging must never break a visit.
                Debug.LogWarning("[Log] Could not save session: " + e.Message);
                return null;
            }
        }
    }
}
