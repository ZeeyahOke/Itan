using System;
using UnityEngine;
using UnityEngine.UI;

namespace Itan.Voice
{
    /// <summary>
    /// Listens for the visitor to answer out loud. Each try lasts listenSeconds.
    /// After a miss it fires Missed so the caller can repeat the prompt and call
    /// ListenAgain(). After maxTries misses (or with no microphone) it shows a
    /// button instead, so nobody gets stuck.
    /// </summary>
    public class MicResponseDetector : MonoBehaviour
    {
        [Tooltip("Loudness (RMS, 0 to 1) that counts as speaking. Tune on the headset in a real room.")]
        [SerializeField] float volumeThreshold = 0.05f;
        [SerializeField] float listenSeconds = 4f;
        [Tooltip("How long the voice must stay loud, so a single bang or click doesn't count.")]
        [SerializeField] float minLoudSeconds = 0.15f;
        [SerializeField] int maxTries = 2;

        [Header("UI")]
        [SerializeField] GameObject listeningCue;
        [SerializeField] GameObject fallbackPanel;
        [SerializeField] Button fallbackButton;

        /// <summary>A try ended without hearing anything. Argument: misses so far.</summary>
        public event Action<int> Missed;

        const int SampleRate = 16000;
        const int WindowSamples = 512;   // ~30 ms of audio per volume reading

        VoiceCheck check;
        Action onAnswered;
        AudioClip micClip;
        string micDevice;
        readonly float[] window = new float[WindowSamples];

        public float CurrentVolume { get; private set; }

#if UNITY_EDITOR
        /// <summary>Editor testing only: answer every voice check as soon as it starts listening.</summary>
        public static bool DebugAutoAnswer;
#endif

        /// <summary>How many listening tries the last answer took (misses + the try that worked).</summary>
        public int LastTries { get; private set; }
        /// <summary>True if the last answer came from the fallback button rather than the voice.</summary>
        public bool LastAnsweredByButton { get; private set; }

        void Awake()
        {
            check = new VoiceCheck(volumeThreshold, listenSeconds, minLoudSeconds, maxTries);
            fallbackButton.onClick.AddListener(() => Answer(true));
            HideUi();
        }

        /// <summary>Start listening for an answer. onAnswered runs once, by voice or by button.</summary>
        public void Listen(Action answered)
        {
            onAnswered = answered;
            check.Reset();

            if (!MicPermission.Granted || !StartMic())
            {
                Debug.Log("[Voice] No microphone, showing the button");
                ShowFallback();
                return;
            }
            ListenAgain();
        }

        /// <summary>Start the next try after a miss (usually after the prompt is repeated).</summary>
        public void ListenAgain()
        {
            if (check.NeedsFallback) { ShowFallback(); return; }
            check.StartTry();
            listeningCue.SetActive(true);
        }

        public void Cancel()
        {
            onAnswered = null;
            check.Reset();
            StopMic();
            HideUi();
        }

        void Update()
        {
            if (!check.IsListening) return;
#if UNITY_EDITOR
            if (DebugAutoAnswer) { listeningCue.SetActive(false); Answer(false); return; }
#endif

            CurrentVolume = ReadVolume();
            if (!check.Update(CurrentVolume, Time.deltaTime)) return;

            listeningCue.SetActive(false);
            if (check.Heard)
            {
                Debug.Log("[Voice] Heard an answer");
                Answer(false);
            }
            else if (check.NeedsFallback)
            {
                Debug.Log("[Voice] Missed " + check.Misses + " times, showing the button");
                ShowFallback();
            }
            else
            {
                Debug.Log("[Voice] Missed try " + check.Misses);
                Missed?.Invoke(check.Misses);
            }
        }

        void Answer(bool byButton)
        {
            LastAnsweredByButton = byButton;
            LastTries = check.Misses + (byButton ? 0 : 1);
            var callback = onAnswered;
            onAnswered = null;
            check.Reset();
            StopMic();
            HideUi();
            callback?.Invoke();
        }

        void ShowFallback()
        {
            listeningCue.SetActive(false);
            fallbackPanel.SetActive(true);
        }

        void HideUi()
        {
            listeningCue.SetActive(false);
            fallbackPanel.SetActive(false);
        }

        // ---------- Microphone ----------

        bool StartMic()
        {
            if (micClip != null) return true;
            if (Microphone.devices.Length == 0) return false;
            micDevice = Microphone.devices[0];
            micClip = Microphone.Start(micDevice, true, 1, SampleRate);
            return micClip != null;
        }

        void StopMic()
        {
            if (micClip == null) return;
            Microphone.End(micDevice);
            micClip = null;
        }

        float ReadVolume()
        {
            if (micClip == null) return 0f;
            int position = Microphone.GetPosition(micDevice) - WindowSamples;
            if (position < 0) position += micClip.samples; // the mic buffer loops
            micClip.GetData(window, position);

            float sum = 0f;
            for (int i = 0; i < window.Length; i++) sum += window[i] * window[i];
            return Mathf.Sqrt(sum / window.Length);  // RMS loudness
        }

        void OnDisable() => StopMic();

        // ---------- Editor testing ----------

        public void SimulateVoice()
        {
            if (check.IsListening) { listeningCue.SetActive(false); Answer(false); }
        }

        public void SimulateSilence()
        {
            // Burn the rest of the current try.
            if (check.IsListening && check.Update(0f, listenSeconds))
            {
                listeningCue.SetActive(false);
                if (check.NeedsFallback) ShowFallback(); else Missed?.Invoke(check.Misses);
            }
        }

        public void PressFallback()
        {
            if (fallbackPanel.activeSelf) fallbackButton.onClick.Invoke();
        }
    }
}
