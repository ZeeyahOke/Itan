namespace Itan.Detection
{
    /// <summary>
    /// Pure logic for the head-bow greeting, kept free of Unity objects so it
    /// can be unit tested. Feed it the head's downward tilt every frame; it
    /// says yes once the head has stayed past the bow angle long enough.
    /// </summary>
    public class BowCheck
    {
        public float BowAngle { get; }
        public float HoldSeconds { get; }

        float heldFor;

        public BowCheck(float bowAngle, float holdSeconds)
        {
            BowAngle = bowAngle;
            HoldSeconds = holdSeconds;
        }

        public void Reset() => heldFor = 0f;

        /// <param name="pitchDownDegrees">How far the head is tilted down. 0 = level, 90 = straight down.</param>
        /// <returns>True once the bow has been held for HoldSeconds.</returns>
        public bool Update(float pitchDownDegrees, float deltaTime)
        {
            if (pitchDownDegrees >= BowAngle)
            {
                heldFor += deltaTime;
                return heldFor >= HoldSeconds;
            }

            heldFor = 0f; // lifting the head early starts the count again
            return false;
        }
    }
}
