using Itan.Voice;
using NUnit.Framework;

namespace Itan.Tests
{
    public class VoiceCheckTests
    {
        const float Threshold = 0.05f;
        const float Listen = 4f;
        const float MinLoud = 0.15f;
        const float Frame = 0.02f; // ~50 fps

        static VoiceCheck NewCheck() => new VoiceCheck(Threshold, Listen, MinLoud, 2);

        // Feed the same volume for a number of seconds; returns true if the try ended.
        static bool Feed(VoiceCheck check, float volume, float seconds)
        {
            for (float t = 0f; t < seconds; t += Frame)
                if (check.Update(volume, Frame)) return true;
            return false;
        }

        [Test]
        public void SpeakingAboveThreshold_IsHeard()
        {
            var check = NewCheck();
            check.StartTry();
            Assert.IsTrue(Feed(check, 0.2f, 0.5f));
            Assert.IsTrue(check.Heard);
            Assert.AreEqual(0, check.Misses);
        }

        [Test]
        public void VolumeExactlyAtThreshold_Counts()
        {
            var check = NewCheck();
            check.StartTry();
            Assert.IsTrue(Feed(check, Threshold, 0.5f));
            Assert.IsTrue(check.Heard);
        }

        [Test]
        public void QuietBelowThreshold_IsAMiss()
        {
            var check = NewCheck();
            check.StartTry();
            Assert.IsTrue(Feed(check, 0.01f, Listen + 0.1f));
            Assert.IsFalse(check.Heard);
            Assert.AreEqual(1, check.Misses);
        }

        [Test]
        public void ShortSpike_DoesNotCount()
        {
            var check = NewCheck();
            check.StartTry();
            check.Update(0.9f, 0.05f);   // a clap or a knock, shorter than MinLoud
            Feed(check, 0f, Listen);
            Assert.IsFalse(check.Heard);
            Assert.AreEqual(1, check.Misses);
        }

        [Test]
        public void TwoMisses_NeedFallback()
        {
            var check = NewCheck();
            check.StartTry();
            Feed(check, 0f, Listen + 0.1f);
            Assert.IsFalse(check.NeedsFallback);
            check.StartTry();
            Feed(check, 0f, Listen + 0.1f);
            Assert.AreEqual(2, check.Misses);
            Assert.IsTrue(check.NeedsFallback);
        }

        [Test]
        public void NoMoreTries_AfterFallback()
        {
            var check = NewCheck();
            for (int i = 0; i < 2; i++) { check.StartTry(); Feed(check, 0f, Listen + 0.1f); }
            check.StartTry();
            Assert.IsFalse(check.IsListening);
        }

        [Test]
        public void Reset_ClearsMissesAndHeard()
        {
            var check = NewCheck();
            check.StartTry();
            Feed(check, 0f, Listen + 0.1f);
            check.Reset();
            Assert.AreEqual(0, check.Misses);
            Assert.IsFalse(check.Heard);
            Assert.IsFalse(check.NeedsFallback);
        }

        [Test]
        public void NotListening_IgnoresSound()
        {
            var check = NewCheck();
            Assert.IsFalse(check.Update(1f, 1f));
            Assert.IsFalse(check.Heard);
        }
    }
}
