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
    using global::ComAbilities.Localizations;
    using global::ComAbilities.Objects;
    using PlayerRoles;
    using Respawning;
    using UnityEngine;

    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class BroadcastMessageCommand : MonoBehaviour, ICommand
    {
        public string Command { get; } = "broadcast-message";
        public string[] Aliases { get; } = new[] { "bc" };
        public string Description { get; } = string.Format(SharedT.CommandFormat, BroadcastMessageT.Description);

        private readonly static ComAbilities Instance = ComAbilities.Instance;

        public static BroadcastMessageConfig _config => Instance.Config.BroadcastMessage;

        private readonly static BroadcastMessageT BroadcastMessageT = Instance.Localization.BroadcastMessage;
        private readonly static SharedT SharedT = Instance.Localization.Shared;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (Guards.NotEnabled(_config, out response)) return false;
            if (Guards.NotComputer(player.Role, out response)) return false;

            CompManager comp = Instance.CompDict.GetOrError(player);
            BroadcastMsg bc = comp.BroadcastMessage;

            Scp079Role role = player.Role.As<Scp079Role>();

            if (Guards.SignalLost(role, out response)) return false;
            if (Guards.NotEnoughAux(role, bc.AuxCost, out response)) return false;
            if (Guards.InvalidLevel(role, bc.ReqLevel, out response)) return false;
            if (Guards.OnCooldown(bc, out response)) return false;

            if (!arguments.Any())
            {
                response = BroadcastMessageT.NoMessageProvided;
                return false;
            }

            bc.Trigger(string.Join(" ", arguments));
            response = string.Format(BroadcastMessageT.Success, _config.Cooldown);
            return true;
        }
    }
    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class ShowHint : MonoBehaviour, ICommand
    {
        public string Command { get; } = "show-hint";
        public string[] Aliases { get; } = new[] { "sh" };
        public string Description { get; } = "Shows a hint to you";

        private readonly static ComAbilities Instance = ComAbilities.Instance;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            Hint hint = new Hint(string.Join(" ", arguments), 5000, true);
            player.ShowHint(hint);
            response = "true";
            return true;
        }
    }
}
