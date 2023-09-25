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
    //[Hotkey]
    public struct GoToArgs
    {
        public Player Player;
        public GoToType GoToType;
    }
    public sealed class GoTo : Ability, ICooldownAbility
    {
        private readonly static ComAbilities Instance = ComAbilities.Instance;

        private static GoToT GoToT => Instance.Localization.GoTo;
        private static GoToScpConfig _config => Instance.Config.GoToScp;
        public GoTo(CompManager compManager) : base(compManager) { }

        public override string Name => GoToT.Name;
        public override string Description => GoToT.Description;
        public override float AuxCost => _config.AuxCost;
        public override int ReqLevel => _config.Level;
        public override string DisplayText => string.Format(GoToT.DisplayText, AuxCost, Instance.Config.PlayerTracker.GoToCost);

        public override bool Enabled => _config.Enabled;
        public bool OnCooldown => _cooldown.Active;

        public float CooldownLength => _config.Cooldown;

        private Cooldown _cooldown { get; } = new();


        public void Trigger(Player player, GoToType goToType)
        {
            Room playerRoom = player.CurrentRoom;
            IEnumerable<Camera> cameras = playerRoom.Cameras;
            if (!cameras.Any()) return;

            Camera? chosenCamera = Helper.GetClosest(player.Position, cameras);
            if (chosenCamera == null) return;
            CompManager.Role!.Camera = chosenCamera;

            if (goToType == GoToType.SCP)
            {
                CompManager.DeductAux(Instance.Config.GoToScp.AuxCost);
                _cooldown.Start(Instance.Config.GoToScp.Cooldown);
            }
            else if (goToType == GoToType.TrackedPlayer)
            {
                CompManager.DeductAux(Instance.Config.PlayerTracker.GoToCost);
                _cooldown.Start(Instance.Config.PlayerTracker.GoToCooldown);
            }
        }

        public int GetETA()
        {
            if (_cooldown == null) throw new Exception("Attempt to get ETA of a null rateLimitTask");
            float? eta = _cooldown.GetETA();
            if (!eta.HasValue) throw new Exception("Attempt to get ETA of a null rateLimitTask");
            return (int)eta;
        }

        public override void KillTasks()
        {
            // _cooldownTask.AttemptKill();
        }
    }
}
