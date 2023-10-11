using ComAbilities.Localizations;
using ComAbilities.Objects;
using ComAbilities.Types;
using Exiled.API.Features;
using Respawning;

namespace ComAbilities.Abilities
{
    //[Hotkey]
    public sealed class DistressSignal : Ability, ICooldownAbility
    {
        private static DistressSignalT Translation => Instance.Localization.DistressSignal;
        private static DistressSignalConfig Config => Instance.Config.DistressSignal;

        private readonly Cooldown cooldown = new();

        public DistressSignal(CompManager compManager) : base(compManager) { }

        public override string Name => Translation.Name;
        public override string Description => Translation.Description;
        public override float AuxCost => Config.AuxCost;
        public override int ReqLevel => Config.Level;
        public override string DisplayText => string.Format(Translation.DisplayText, AuxCost);
        public override bool Enabled => Config.Enabled;

        public float CooldownLength => Config.Cooldown;
        public bool OnCooldown => cooldown.Active;

        public float GetDisplayETA() => cooldown.GetDisplayETA();

        public void Trigger(SpawnableTeamType team)
        {
            Respawn.GrantTickets(team, Config.Tickets);

            // TODO: set up faux aux manager system
            if (CompManager.Role != null) CompManager.Role.Energy -= AuxCost;
            cooldown.Start(CooldownLength);
        }

        public override void CleanUp() { }
    }
}
