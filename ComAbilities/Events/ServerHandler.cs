
namespace Exiled.ComAbilitiesEvents
{
    using ComAbilities;
    using ComAbilities.Objects;
    using ComAbilities.Types.RueTasks;
    using Exiled.Events.EventArgs.Server;

    internal sealed class ServerHandler
    {
        private static ComAbilities Instance => ComAbilities.Instance;

        public static TaskPool RoundTaskPool { get; set; } = new();

        public void OnRoundEnded(RoundEndedEventArgs _)
        {
            Instance.CompDict.CleanUp();
            RoundTaskPool.CleanUp();
            GeneratorEffects.RefreshSingleton();
        }
    }
}
