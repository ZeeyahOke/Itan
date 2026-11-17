using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace Itan.Voice
{
    /// <summary>
    /// Asks for microphone access when the app starts, so the Quest shows its
    /// permission prompt before the visitor reaches the voice check.
    /// If access is refused, the voice check falls back to its button.
    /// </summary>
    public class MicPermission : MonoBehaviour
    {
        public static bool Granted
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                return Permission.HasUserAuthorizedPermission(Permission.Microphone);
#else
                return Microphone.devices.Length > 0;
#endif
            }
        }

        void Start()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Debug.Log("[Mic] Permission already granted");
                return;
            }

            var callbacks = new PermissionCallbacks();
            callbacks.PermissionGranted += _ => Debug.Log("[Mic] Permission granted");
            callbacks.PermissionDenied += _ => Debug.Log("[Mic] Permission denied, voice check will use the button");
            Permission.RequestUserPermission(Permission.Microphone, callbacks);
#else
            Debug.Log("[Mic] Editor: " + Microphone.devices.Length + " microphone(s) found");
#endif
        }
    }
}
