namespace ComAbilities.Commands
{
    using System;
    using CommandSystem;
    using Exiled.API.Features;
    using Exiled.API.Features.Roles;
    using global::ComAbilities.Abilities;
    using global::ComAbilities.Localizations;
    using global::ComAbilities.Objects;
    using global::ComAbilities.Types;
    using PlayerRoles;

    [CommandHandler(typeof(ClientCommandHandler))]
    public sealed class GoToCommand : ICommand
    {
        public string Command { get; } = "goto";
        public string[] Aliases { get; } = new[] { "gt" , "go-to", "go"};
        public string Description { get; } = string.Format(Instance.Localization.Shared.CommandFormat, Instance.Localization.GoTo.Description);

        private static ComAbilities Instance => ComAbilities.Instance;
        private static GoToT GoToT => Instance.Localization.GoTo;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            if (Guards.NotComputer(player.Role, out response)) return false;

            CompManager comp = Instance.CompDict.GetOrError(player);
            GoTo gt = comp.GoTo;

            Scp079Role role = player.Role.As<Scp079Role>();
            if (Guards.SignalLost(role, out response)) return false;
            if (Guards.OnCooldown(gt, out response)) return false;

            RoleTypeId? chosenScp = GetSCP(arguments.First());
            if (chosenScp == null)
            {
                bool didParse = int.TryParse(arguments.First(), out int result);

                if (!didParse)
                {
                    response = GoToT.InvalidSearch;
                    return false;
                }

                PlayerTrackerConfig trackerConfig = Instance.Config.PlayerTracker;
                if (Guards.NotEnoughAux(role, trackerConfig.GoToCost, out response)) return false;
                if (Guards.InvalidLevel(role, gt.ReqLevel, out response)) return false;

                Player? trackedPlayer = comp.PlayerTracker.GetTrackerPlayer(result);
                if (trackedPlayer != null)
                {
                    gt.Trigger(trackedPlayer, GoToType.TrackedPlayer);
                }
            } else
            {
                GoToScpConfig config = Instance.Config.GoToScp;
                if (Guards.NotEnabled(config, out response)) return false;

                if (Guards.NotEnoughAux(role, config.AuxCost, out response)) return false;
                if (Guards.InvalidLevel(role, config.Level, out response)) return false;
                List<Player> filteredScpList = Player.List.Where((x) => x.Role == chosenScp).ToList();

                if (filteredScpList.Any())
                {
                    Player chosenPlayer = filteredScpList.RandomItem();
                    gt.Trigger(chosenPlayer, GoToType.SCP);
                } else
                {
                    response = GoToT.NoSCPFound;
                    return false;
                }
            }

            response = GoToT.Success;
            return true;
        }

        private RoleTypeId? GetSCP(string value)
        {
            return value.ToLower() switch
            {
                "173" or "peanut" or "SCP173" or "SCP-173" or "SCP-049" => RoleTypeId.Scp173,
                "049" or "doctor" or "doc" or "SCP049" or "SCP-049" or "SCP049" => RoleTypeId.Scp049,
                "096" or "shy guy" or "shyguy" or "SCP-096" or "SCP096"=> RoleTypeId.Scp096,
                "106" or "larry" or "SCP-106" or "SCP106" => RoleTypeId.Scp106,
                "939" or "dog" or "SCP-939" or "SCP939" => RoleTypeId.Scp939,
                _ => null
            };
        }
    }
}