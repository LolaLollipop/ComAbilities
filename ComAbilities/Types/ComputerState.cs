using Exiled.API.Features;
using Exiled.API.Features.Roles;

// Used for saving and sending hologram information


namespace ComAbilities.Types
{
    public struct HologramState
    {
        public int Level;
        public int XP;
        public float Aux;
        public Camera CurrentCam;
        public float BlackoutZoneCooldown;
        public float RoomLockdownCooldown;

        public HologramState(Scp079Role role)
        {
            this.Level = role.Level;
            this.XP = role.Experience;
            this.Aux = role.Energy;
            this.CurrentCam = role.Camera;
            this.BlackoutZoneCooldown = role.BlackoutZoneCooldown;
            this.RoomLockdownCooldown = role.RoomLockdownCooldown;
        }
        public HologramState(Scp079Role role, float deductAux)
        {
            this.Level = role.Level;
            this.XP = role.Experience;
            this.Aux = role.Energy - deductAux;
            this.CurrentCam = role.Camera;
            this.BlackoutZoneCooldown = role.BlackoutZoneCooldown;
            this.RoomLockdownCooldown = role.RoomLockdownCooldown;
        }
    }
}