using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComAbilities;

namespace ComAbilities.Commands
{
    using System;

    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.API.Features.Pickups;
    using Exiled.API.Features.Roles;
    using global::ComAbilities.Abilities;
    using global::ComAbilities.Objects;
    using PlayerRoles;
    using Respawning;
    using UnityEngine;

    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class HologramCommand : ICommand
    {
        public string Command { get; } = "hologram";        
        public string[] Aliases { get; } = new[] { "holo" , "hg"};
        public string Description { get; } = string.Format(Instance.Localization.Shared.CommandFormat, Instance.Localization.Hologram.Description);

        private readonly static ComAbilities Instance = ComAbilities.Instance;

        private readonly static Localizations.ErrorsT ErrorsT = Instance.Localization.Errors;
        private readonly static Localizations.HologramT HologramT = Instance.Localization.Hologram;
        private readonly static Localizations.SharedT SharedT = Instance.Localization.Shared;
        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            HologramConfig holoConfig = Instance.Config.Hologram;

            Log.Debug("Oh my god I hate my life");
            if (Guards.NotEnabled(holoConfig, out response)) return false;
            if (Guards.NotComputer(player.Role, out response)) return false;

            CompManager comp = Instance.CompDict.GetOrError(player);
            Hologram holo = comp.Hologram;

            List<HologramRoleConfig> roleList = holoConfig.RoleLevels;
            if (!arguments.Any() || !int.TryParse(arguments[0], out int index))
            {
                response = GetRoleListString(roleList);
                return false;
            }

            if (!roleList.Any() || !roleList.TryGet(index - 1, out HologramRoleConfig roleSelection))
            {
                response = GetRoleListString(roleList);
                return false;
            }

            Scp079Role role = player.Role.As<Scp079Role>();
            if (Guards.SignalLost(role, out response)) return false;
            if (Guards.NotEnoughAux(role, roleSelection.Cost, out response)) return false;
            if (Guards.InvalidLevel(role, roleSelection.Level, out response)) return false;
            if (Guards.OnCooldown(holo, out response)) return false;

            holo.Trigger(roleSelection);
            response = string.Format(HologramT.Success, holoConfig.Cooldown);
            return true;
        }
    
        private string GetRoleListString(List<HologramRoleConfig> roleList)
        {
            StringBuilder sb = new();

            sb.Append(HologramT.AvailableHologramRoles);
            for (var i = 0; i < roleList.Count; i++)
            {
                HologramRoleConfig roleConfig = roleList.ElementAt(i);
                sb.Append(string.Format(HologramT.HologramRoleFormat, i + 1, roleConfig.Level, SharedT.RoleNames[roleConfig.Role], roleConfig.Cost));
            }

            return sb.ToString();
        }
    }
}
