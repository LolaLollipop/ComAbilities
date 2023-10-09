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
        private static GoToT GoToT => Instance.Localization.GoTo;
        private static GoToScpConfig config => Instance.Config.GoToScp;

        private readonly Cooldown cooldown = new();

        public GoTo(CompManager compManager) : base(compManager) { }

        public override string Name => GoToT.Name;
        public override string Description => GoToT.Description;
        public override float AuxCost => config.AuxCost;
        public override int ReqLevel => config.Level;
        public override string DisplayText => string.Format(GoToT.DisplayText, AuxCost, Instance.Config.PlayerTracker.GoToCost);
        public override bool Enabled => config.Enabled;

        public bool OnCooldown => cooldown.Active;
        public float CooldownLength => config.Cooldown;

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
                    CompManager.DeductAux(config.AuxCost);
                    cooldown.Start(config.Cooldown);
                    break;
                case GoToType.TrackedPlayer:
                    CompManager.DeductAux(Instance.Config.PlayerTracker.GoToCost);
                    cooldown.Start(Instance.Config.PlayerTracker.GoToCooldown);
                    break;
            }
        }

        public float GetDisplayETA() => cooldown.GetDisplayETA();

        public override void CleanUp()
        {
            // cooldownTask.AttemptKill();
        }
    }
}
