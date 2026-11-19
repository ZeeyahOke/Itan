using System.IO;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine;

namespace Itan.EditorTools
{
    /// <summary>
    /// Editor helper: renders what the visitor sees (from the XR camera, with a
    /// headset-like field of view) to Logs/view.png, plus a few fixed angles
    /// (looking down, left, right, up). Works in Play mode and in the editor.
    /// </summary>
    public static class ViewCapture
    {
        [MenuItem("Itan/Debug/Capture View")]
        public static void CaptureForward() => Capture("view", 0f, 0f);

        [MenuItem("Itan/Debug/Capture Views (all angles)")]
        public static void CaptureAll()
        {
            Capture("view", 0f, 0f);
            Capture("view_down", 45f, 0f);
            Capture("view_left", 0f, -60f);
            Capture("view_right", 0f, 60f);
            Capture("view_up", -35f, 0f);
        }

        static void Capture(string name, float pitch, float yaw)
        {
            var origin = Object.FindFirstObjectByType<XROrigin>();
            if (origin == null) return;
            var source = origin.Camera;

            var go = new GameObject("CaptureCam") { hideFlags = HideFlags.HideAndDontSave };
            var cam = go.AddComponent<Camera>();
            cam.CopyFrom(source);
            cam.stereoTargetEye = StereoTargetEyeMask.None;
            cam.fieldOfView = 80f;   // close to one Quest 2 eye
            go.transform.SetPositionAndRotation(source.transform.position,
                Quaternion.Euler(pitch, source.transform.eulerAngles.y + yaw, 0f));

            var rt = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            cam.targetTexture = rt;
            cam.Render();

            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;

            Directory.CreateDirectory("Logs");
            File.WriteAllBytes(Path.Combine("Logs", name + ".png"), tex.EncodeToPNG());

            cam.targetTexture = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(tex);
            Object.DestroyImmediate(go);
        }
    }
}
