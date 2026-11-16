namespace Itan.Session
{
    /// <summary>
    /// One step of a visit: menu, greeting, call, story, quiz or summary.
    /// SessionManager runs exactly one state at a time and swaps between them.
    /// </summary>
    public interface ISessionState
    {
        /// <summary>Called once when the session moves into this state.</summary>
        void Enter();

        /// <summary>Called every frame while this state is active.</summary>
        void Tick();

        /// <summary>Called once just before the session leaves this state.</summary>
        void Exit();

        /// <summary>
        /// Finish this state straight away and move to the next one.
        /// Used by the debug key now, and by fallbacks later.
        /// </summary>
        void Skip();
    }
}
