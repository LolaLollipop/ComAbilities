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
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComAbilities.Abilities
{
    public sealed class GoTo : Ability, ICooldownAbility
    {
        private static GoToT Translation => Instance.Localization.GoTo;
        private static GoToScpConfig SCPConfig => Instance.Config.GoToScp;
        private static PlayerTrackerConfig TrackerConfig => Instance.Config.PlayerTracker;

        private readonly Cooldown cooldown = new();

        public GoTo(CompManager compManager) : base(compManager) { }

        public override string Name => Translation.Name;
        public override string Description => Translation.Description;
        public override float AuxCost { get; } = 0;
        public override int ReqLevel => CalculateReqLevel();
        public override string DisplayText => string.Format(Translation.DisplayText, SCPConfig.AuxCost, TrackerConfig.GoToCost);
        public override bool Enabled => SCPConfig.Enabled || TrackerConfig.Enabled;

        public bool OnCooldown => cooldown.Active;

        public void Trigger(Player player, GoToType goToType)
        {
            Room playerRoom = player.CurrentRoom;
            IEnumerable<Camera> cameras = playerRoom.Cameras;
            if (!cameras.Any()) return;

            Camera? chosenCamera = Helper.GetClosest(player.Position, cameras);
            if (chosenCamera == null) return;
            CompManager.Role!.Camera = chosenCamera;

            switch (goToType)
            {
                case GoToType.SCP:
                    if (CompManager.Role != null)
                        CompManager.Role.Energy -= SCPConfig.AuxCost;
                    cooldown.Start(SCPConfig.Cooldown);
                    break;
                case GoToType.TrackedPlayer:
                    if (CompManager.Role != null)
                        CompManager.Role.Energy -= SCPConfig.AuxCost;
                    cooldown.Start(TrackerConfig.GoToCooldown);
                    break;
            }
        }

        private int CalculateReqLevel() => Math.Min(SCPConfig.Enabled ? SCPConfig.Level : 6, TrackerConfig.Enabled ? TrackerConfig.Level : 6);

        public float GetDisplayETA() => cooldown.GetDisplayETA();

        public override void CleanUp() { }
    }
}
