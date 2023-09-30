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
        private readonly static ComAbilities Instance = ComAbilities.Instance;
        private static DistressSignalT DistressSignalT => Instance.Localization.DistressSignal;
        private static DistressSignalConfig _config => Instance.Config.DistressSignal;

        public DistressSignal(CompManager compManager) : base(compManager) { }

        public override string Name { get; } = DistressSignalT.Name;
        public override string Description { get; } = DistressSignalT.Description;
        public override float AuxCost { get; } = _config.AuxCost;
        public override int ReqLevel { get; } = _config.Level;
        public override string DisplayText => string.Format(DistressSignalT.DisplayText, AuxCost);

        public AllHotkeys hotkeyButton => Instance.Config.RealityScrambler.Hotkey;
        public override bool Enabled { get; } = _config.Enabled;

        public float CooldownLength { get; } = _config.Cooldown;
        public bool OnCooldown => _cooldown.Active;

        private Cooldown _cooldown { get; } = new();

        public float GetDisplayETA() => _cooldown.GetDisplayETA();

        public void Trigger(SpawnableTeamType team)
        {
            Respawn.GrantTickets(team, Instance.Config.DistressSignal.Tickets);

            CompManager.DeductAux(AuxCost);
            _cooldown.Start(CooldownLength);
        }

        public override void CleanUp() { }
    }
}
