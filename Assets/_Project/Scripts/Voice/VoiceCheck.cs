namespace Itan.Voice
{
    /// <summary>
    /// Pure logic for "did the visitor say something?", kept free of Unity
    /// objects so it can be unit tested. Feed it the mic volume every frame.
    /// A try passes when the volume stays above the threshold for a short
    /// moment; it fails when the listening time runs out. After MaxTries
    /// failed tries, NeedsFallback becomes true.
    /// </summary>
    public class VoiceCheck
    {
        public float VolumeThreshold { get; }
        public float ListenSeconds { get; }
        public float MinLoudSeconds { get; }
        public int MaxTries { get; }

        public int Misses { get; private set; }
        public bool Heard { get; private set; }
        public bool IsListening { get; private set; }
        public bool NeedsFallback => Misses >= MaxTries;

        float listenedFor;
        float loudFor;

        public VoiceCheck(float volumeThreshold, float listenSeconds, float minLoudSeconds, int maxTries)
        {
            VolumeThreshold = volumeThreshold;
            ListenSeconds = listenSeconds;
            MinLoudSeconds = minLoudSeconds;
            MaxTries = maxTries;
        }

        /// <summary>Forget everything and get ready for a new word.</summary>
        public void Reset()
        {
            Misses = 0;
            Heard = false;
            IsListening = false;
        }

        public void StartTry()
        {
            if (Heard || NeedsFallback) return;
            listenedFor = 0f;
            loudFor = 0f;
            IsListening = true;
        }

        /// <returns>True on the frame the current try ends, either heard or missed.</returns>
        public bool Update(float volume, float deltaTime)
        {
            if (!IsListening) return false;

            loudFor = volume >= VolumeThreshold ? loudFor + deltaTime : 0f;
            if (loudFor >= MinLoudSeconds)
            {
                Heard = true;
                IsListening = false;
                return true;
            }

            listenedFor += deltaTime;
            if (listenedFor >= ListenSeconds)
            {
                Misses++;
                IsListening = false;
                return true;
            }

            return false;
        }
    }
}
