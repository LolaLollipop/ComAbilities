using ComAbilities.Objects;
using ComAbilities.Types;
using Exiled.API.Extensions;
using Exiled.API.Features;
using PlayerRoles;
using UnityEngine;
using Exiled.API.Features.Roles;
using ComAbilities.Localizations;
using MEC;
using Exiled.API.Features.Doors;
using ComAbilities.Types.RueTasks;

namespace ComAbilities.Abilities
{
    public sealed class Hologram : Ability, IReductionAbility, ICooldownAbility
    {
        private static HologramT HologramT => Instance.Localization.Hologram;
        private static HologramConfig config => Instance.Config.Hologram;

        private readonly Cooldown cooldown = new();
        private readonly Cooldown _expireConfirmation = new();
        private PeriodicTask _hologramTask;

        private const int _broadcastTime = 3;
        private const int _timeUntilExpire = 5;

        public Hologram(CompManager compManager) : base(compManager) {
            _hologramTask = new(config.Length, 1f, UpdateText, ChangeBack);
        }

        public override string Name { get; } = HologramT.Name;
        public override string Description { get; } = HologramT.Description;
        public override float AuxCost { get; } = 0f;
        public override int ReqLevel { get; } = config.Level;
        public override string DisplayText => string.Format(HologramT.DisplayText, GetLowestAux(), GetHighestAux());
        public override bool Enabled => config.Enabled;
        public string ActiveDisplayText { get; } = "";

        public float CooldownLength => config.Cooldown;    

        public float AuxModifier { get; } = 0f;

        public bool OnCooldown => cooldown.Active;

        public bool IsActive => _hologramTask.Enabled;
        public bool ConfirmationPressed => _expireConfirmation.Active;

        public const string SessionVariable = "ComAbilities_hologram";

        public void Trigger(HologramRoleConfig roleConfig)
        {

            Player player = this.CompManager.Role!.Owner;

            // get safe position
            Vector3 camPos = this.CompManager.Role.Camera.Position;
            IEnumerable<Door> doors = this.CompManager.Role.Camera.Room.Doors;

            Door? chosenDoor = Helper.GetClosest(camPos, doors);

            if (chosenDoor != null)
            {
                Transform doorTransform = chosenDoor.GameObject.transform;

                Vector3 offset = new(0, 1f, 0);
                Vector3 forward = doorTransform.position + (doorTransform.forward * 1);
                Vector3 backwards = doorTransform.position + (doorTransform.forward * -1);
                Vector3 roomCenter = CompManager.Role.Camera.Room.Position;

                player.SessionVariables[SessionVariable] = new HologramState(CompManager.Role, roleConfig.Cost);
                player.Role.Set(RoleTypeId.Scp106);

                player.ChangeAppearance(roleConfig.Role, false, 0);

                player.Scale = new Vector3(0, 1, 0);
                player.ReferenceHub.transform.localScale = new Vector3(1, 1, 1);
                foreach (Player myPlayer in Player.List)
                {
                    if (player.NetworkIdentity == null) continue;
                    Server.SendSpawnMessage?.Invoke(null, new object[] { player.NetworkIdentity, myPlayer.Connection });
                }

                player.ReferenceHub.transform.localScale = new Vector3(0, 1, 0);
                player.ChangeAppearance(roleConfig.Role, false, 0);

                if (Vector3.Distance(forward, roomCenter) < Vector3.Distance(backwards, roomCenter))
                {
                    player.Teleport(forward + offset);
                }
                else
                {
                    player.Teleport(backwards + offset);
                }

                _hologramTask.Run();
                cooldown.Start(CooldownLength);

                Exiled.API.Features.Broadcast bc = new(GetHologramBroadcastText(roleConfig.Role), _broadcastTime, true, Broadcast.BroadcastFlags.Normal);
                player.Broadcast(bc, true);
                player.ChangeAppearance(roleConfig.Role, false, 0);
            }
        }

        public void ActivateConfirmation()
        {
            _expireConfirmation.Start(_timeUntilExpire);
        }
        internal void ChangeBack()
        {
            Player player = this.CompManager.AscPlayer;
            cooldown.Start(config.Cooldown);
            _hologramTask.CleanUp();

            // player.Role.Set(RoleTypeId.Scp079); // setting twice avoids the animation
            // player.Role.Set(RoleTypeId.Scp079);
            player.ReferenceHub.roleManager.ServerSetRole(RoleTypeId.Scp079, RoleChangeReason.LateJoin, (RoleSpawnFlags)3);
            Timing.CallDelayed(2, () =>
            {
                player.ReferenceHub.roleManager.ServerSetRole(RoleTypeId.Scp079, RoleChangeReason.LateJoin);

                Scp079Role role = player.Role.As<Scp079Role>();

                HologramState state = (HologramState)this.CompManager.AscPlayer.SessionVariables[Hologram.SessionVariable];

                role.Energy = state.Aux;
                role.Level = state.Level;
                role.Camera = state.CurrentCam;
                role.Experience = state.XP;
                role.BlackoutZoneCooldown = state.BlackoutZoneCooldown;
                role.RoomLockdownCooldown = state.RoomLockdownCooldown;

                this.CompManager.AscPlayer.SessionVariables.Remove(Hologram.SessionVariable);
            });
        }
        internal void UpdateText()
        {
            Exiled.API.Features.Broadcast bc = new(GetHologramBroadcastText(CompManager.AscPlayer.Role), _broadcastTime, true, Broadcast.BroadcastFlags.Normal);
            CompManager.AscPlayer.Broadcast(bc, true);
        }

        public override void CleanUp()
        {
            CompManager.AscPlayer.SessionVariables.Remove(Hologram.SessionVariable);
            _hologramTask.CleanUp();
        }

        public float GetDisplayETA() => cooldown.GetDisplayETA();

        private static float GetLowestAux()
        {
            IEnumerable<float> vals = config.RoleLevels.Select((val, index) => val.Cost);
            return vals.Min();
        }
        private static float GetHighestAux()
        {
            IEnumerable<float> vals = config.RoleLevels.Select((val, index) => val.Cost);
            return vals.Max();
        }
        private string GetHologramBroadcastText(RoleTypeId role)
        {
            // string text = Instance.Config.Hologram.BroadcastText;

            string confirmationMessage;
            if (ConfirmationPressed)
            {
                confirmationMessage = HologramT.CancelMessageAfter;
            }
            else
            {
                confirmationMessage = HologramT.CancelMessageBefore;
            }

            string roleText = $"<color={Helper.RoleColors[role]}>{Instance.Localization.Shared.RoleNames[role]}</color>";
            return string.Format(HologramT.ActiveText, roleText, (int)(_hologramTask.GetETA() ?? 0), confirmationMessage);
        }
    }
}
