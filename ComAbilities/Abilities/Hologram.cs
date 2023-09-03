using ComAbilities.Objects;
using ComAbilities.Types;

using System;
using System.Collections.Generic;
using System.Linq;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Enums;
using PlayerRoles;
using UnityEngine;
using Exiled.API.Features.Roles;
using ComAbilities.Localizations;
using MEC;
using Exiled.API.Features.Doors;

namespace ComAbilities.Abilities
{
    public sealed class Hologram : Ability, IReductionAbility, ICooldownAbility
    {
        private readonly static ComAbilities Instance = ComAbilities.Instance;
        private static HologramT HologramT => Instance.Localization.Hologram;
        private static HologramConfig _config => Instance.Config.Hologram;

        public Hologram(CompManager compManager) : base(compManager) { }

        public static string SessionVariable { get; } = "ComAbilities_hologram";
        public override string Name { get; } = HologramT.Name;
        public override string Description { get; } = HologramT.Description;
        public override float AuxCost { get; } = 0f;
        public override int ReqLevel { get; } = _config.Level;
        public override string DisplayText => string.Format(HologramT.DisplayText, GetLowestAux(), GetHighestAux());
        public override bool Enabled => _config.Enabled;
        public string ActiveDisplayText { get; } = "";

        public float AuxModifier { get; } = 0f;

        public bool OnCooldown => _cooldown.Active;

        public bool IsActive => _hologramTask.Enabled;
        public bool ConfirmationPressed => _expireConfirmation.Active;


        private Cooldown _cooldown { get; } = new();
        public float CooldownLength => _config.Cooldown;
        private Cooldown _expireConfirmation { get; } = new();

        private PeriodicTask _hologramTask => new(_config.Length, 1f, UpdateText, ChangeBack);

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
                _cooldown.Start(CooldownLength);

                Exiled.API.Features.Broadcast bc = new(GetHologramText(roleConfig.Role), 3, true, Broadcast.BroadcastFlags.Normal);
                player.Broadcast(bc, true);
                player.ChangeAppearance(roleConfig.Role, false, 0);
            }
        }

        public int GetETA()
        {
            if (_cooldown == null) throw new Exception("Attempt to get ETA of a null rateLimitTask");
            float? eta = _cooldown.GetETA();
            if (!eta.HasValue) throw new Exception("Attempt to get ETA of a null rateLimitTask");
            return (int)eta;
        }
        public void ActivateConfirmation()
        {
            _expireConfirmation.Start(5);
        }
        public void ChangeBack()
        {
            Player player = this.CompManager.AscPlayer;
            this._cooldown.Start(_config.Cooldown);
            this._hologramTask.AttemptKill();
            //this._expireConfirmationTask.AttemptKill();

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
        public void UpdateText()
        {
            Log.Debug("Updating");
            Exiled.API.Features.Broadcast bc = new(GetHologramText(CompManager.AscPlayer.Role), 3, true, Broadcast.BroadcastFlags.Normal);
            CompManager.AscPlayer.Broadcast(bc, true);
        }

        public override void KillTasks()
        {
            CompManager.AscPlayer.SessionVariables.Remove(Hologram.SessionVariable);
        }

        public void OnFinished()
        {
        }

        private static float GetLowestAux()
        {
            IEnumerable<float> vals = _config.RoleLevels.Select((val, index) => val.Cost);
            return vals.Min();
        }
        private static float GetHighestAux()
        {
            IEnumerable<float> vals = _config.RoleLevels.Select((val, index) => val.Cost);
            return vals.Max();
        }
        private string GetHologramText(RoleTypeId role)
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
            Log.Debug("Trying");
            string roleText = $"<color={Helper.RoleColors[role]}>{Instance.Localization.Shared.RoleNames[role]}</color>";
            return string.Format(HologramT.ActiveText, roleText, (int)(_hologramTask.GetETA() ?? 0), confirmationMessage);
        }
    }
}
