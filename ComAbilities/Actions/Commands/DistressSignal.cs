using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComAbilities;

namespace ComAbilities.Commands
{
    using System;
    using System.Runtime.Remoting.Messaging;
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.API.Features.Pickups;
    using Exiled.API.Features.Roles;
    using global::ComAbilities.Abilities;
    using global::ComAbilities.Localizations;
    using global::ComAbilities.Objects;
    using PlayerRoles;
    using Respawning;
    using UnityEngine;

    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class DistressSignalCommand : ICommand
    {
        public string Command { get; } = "distress-signal";
        public string[] Aliases { get; } = new[] { "ds" , "distress"};
        public string Description { get; } = string.Format(Instance.Localization.Shared.CommandFormat, DistressSignalT.Description);

        private readonly static ComAbilities Instance = ComAbilities.Instance;

        private readonly static ErrorsT ErrorsT = Instance.Localization.Errors;
        private readonly static DistressSignalT DistressSignalT = Instance.Localization.DistressSignal;
        private readonly static SharedT SharedT = Instance.Localization.Shared;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            DistressSignalConfig dsConfig = Instance.Config.DistressSignal;

            if (Guards.NotEnabled(dsConfig, out response)) return false;
            if (Guards.NotComputer(player.Role, out response)) return false;

            SpawnableTeamType? team = TryParseTeamArgs(arguments);
            if (team == null)
            {
                response = DistressSignalT.InvalidOption;
                return false;
            }

            CompManager comp = Instance.CompDict.GetOrError(player);
            DistressSignal ds = comp.DistressSignal;

            Scp079Role role = player.Role.As<Scp079Role>();

            if (Guards.SignalLost(role, out response)) return false;
            if (Guards.NotEnoughAux(role, ds.AuxCost, out response)) return false;
            if (Guards.InvalidLevel(role, ds.ReqLevel, out response)) return false;
            if (Guards.OnCooldown(ds, out response)) return false;


            ds.Trigger((SpawnableTeamType)team);
            response = string.Format(DistressSignalT.Success, dsConfig.Cooldown);
            return true;
        }
    
        private SpawnableTeamType? TryParseTeamArgs(ArraySegment<string> arguments)
        {
            if (!arguments.Any()) {
                return null;
            }
            return arguments.First().ToLower() switch
            {
                "mtf" or "ntf" => (SpawnableTeamType?)SpawnableTeamType.NineTailedFox,
                "ci" or "chaos" => (SpawnableTeamType?)SpawnableTeamType.ChaosInsurgency,
                _ => null,
            };
        }
    }
}
