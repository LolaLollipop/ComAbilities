
namespace Exiled.ComAbilitiesEvents
{
    using ComAbilities;
    using Exiled.Events.EventArgs.Server;

    internal sealed class ServerHandler
    {

        private readonly ComAbilities Instance = ComAbilities.Instance;

        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            Instance.CompDict.CleanUp();
        }
    }
}
