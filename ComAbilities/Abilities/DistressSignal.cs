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
        private static DistressSignalT DistressSignalT => Instance.Localization.DistressSignal;

        private static DistressSignalConfig config => Instance.Config.DistressSignal;
        private readonly Cooldown cooldown = new();

        public DistressSignal(CompManager compManager) : base(compManager) { }

        public override string Name => DistressSignalT.Name;
        public override string Description => DistressSignalT.Description;
        public override float AuxCost => config.AuxCost;
        public override int ReqLevel => config.Level;
        public override string DisplayText => string.Format(DistressSignalT.DisplayText, AuxCost);
        public override bool Enabled => config.Enabled;

        public float CooldownLength => config.Cooldown;
        public bool OnCooldown => cooldown.Active;

        public float GetDisplayETA() => cooldown.GetDisplayETA();

        public void Trigger(SpawnableTeamType team)
        {
            Respawn.GrantTickets(team, Instance.Config.DistressSignal.Tickets);

            CompManager.DeductAux(AuxCost);
            cooldown.Start(CooldownLength);
        }

        public override void CleanUp() { }
    }
}
