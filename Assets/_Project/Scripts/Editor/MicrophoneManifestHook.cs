using System.IO;
using System.Xml;
using UnityEditor.Android;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>
    /// Makes sure the Android manifest asks for RECORD_AUDIO, so the Quest
    /// can show the microphone prompt. Runs on every Android build.
    /// </summary>
    public class MicrophoneManifestHook : IPostGenerateGradleAndroidProject
    {
        const string AndroidNs = "http://schemas.android.com/apk/res/android";
        const string RecordAudio = "android.permission.RECORD_AUDIO";

        public int callbackOrder => 999; // after Unity's and OpenXR's own manifest changes

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string manifestPath = Path.Combine(path, "src", "main", "AndroidManifest.xml");
            if (!File.Exists(manifestPath)) return;

            var doc = new XmlDocument();
            doc.Load(manifestPath);
            var manifest = doc.DocumentElement;

            foreach (XmlNode node in manifest.SelectNodes("uses-permission"))
            {
                var name = node.Attributes?["android:name"];
                if (name != null && name.Value == RecordAudio) return; // already there
            }

            var permission = doc.CreateElement("uses-permission");
            permission.SetAttribute("name", AndroidNs, RecordAudio);
            manifest.PrependChild(permission);
            doc.Save(manifestPath);
            Debug.Log("[Itan] Added RECORD_AUDIO to " + manifestPath);
        }
    }
}
