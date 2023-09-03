using ComAbilities.Localizations;
using ComAbilities.Objects;
using ComAbilities.Types;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using PlayerRoles;
using PlayerRoles.PlayableScps.HumeShield;
using PlayerRoles.PlayableScps.Scp079;
using Respawning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComAbilities.Abilities
{
    //[Hotkey]
    public sealed class DistressSignal : Ability, ICooldownAbility
    {
        private readonly static ComAbilities Instance = ComAbilities.Instance;
        private static DistressSignalT DistressSignalT => Instance.Localization.DistressSignal;
        private static DistressSignalConfig _config => Instance.Config.DistressSignal;

        public DistressSignal(CompManager compManager) : base(compManager) { }

        public override string Name => DistressSignalT.Name;
        public override string Description => DistressSignalT.Description;
        public override float AuxCost => _config.AuxCost;
        public override int ReqLevel => _config.Level;
        public override string DisplayText => string.Format(DistressSignalT.DisplayText, AuxCost);

        public AllHotkeys hotkeyButton => Instance.Config.RealityScrambler.Hotkey;
        public override bool Enabled => _config.Enabled;

        public float CooldownLength => _config.Cooldown;
        public bool OnCooldown => _cooldown.Active;

        private Cooldown _cooldown => new();

        public int GetETA()
        {
            if (_cooldown == null) throw new Exception("Attempt to get ETA of a null rateLimitTask");
            float? eta = _cooldown.GetETA();
            if (!eta.HasValue) throw new Exception("Attempt to get ETA of a null rateLimitTask");
            return (int)eta;
        }

        public void Trigger(SpawnableTeamType team)
        {
            Respawn.GrantTickets(team, Instance.Config.DistressSignal.Tickets);

            CompManager.DeductAux(AuxCost);
            _cooldown.Start(CooldownLength);
        }

        public override void KillTasks()
        {
           // _cooldownTask.AttemptKill();
        }
    }
}
