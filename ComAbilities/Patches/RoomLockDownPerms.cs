using Exiled.API.Features;
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp079;
using KeycardPermissions = Interactables.Interobjects.DoorUtils.KeycardPermissions;
using Interactables.Interobjects.DoorUtils;

namespace ComAbilities.Patches
{
    [HarmonyPatch(typeof(Scp079LockdownRoomAbility), nameof(Scp079LockdownRoomAbility.ValidateDoor))]
    internal static class LockdownRoomPatch
    {
        private static readonly ComAbilities Instance = ComAbilities.Instance;
        private static bool ValidDoor(ReferenceHub refHub, DoorVariant door)
        {
            if (!Instance.Config.DoComputerPerms) return true;
            try
            {
                Player player = Player.Get(refHub);
                KeycardPermissions computerPermissions = new();
                KeycardPermissions doorPerms = door.RequiredPermissions.RequiredPermissions;
                int accessLevel = player.Role.As<Exiled.API.Features.Roles.Scp079Role>().Level;
                foreach (KeyValuePair<KeycardPermissions, int> pair in Instance.Config.DoorPermissions)
                {
                    if (accessLevel >= pair.Value)
                    {
                        computerPermissions |= pair.Key;
                    }
                }

                if (!computerPermissions.HasFlag(doorPerms))
                {
                    return false;
                }   
                return true;
            } catch(Exception e)
            {
                Log.Debug(e);
                return false;
            }
        }
        private static bool Prefix(Scp079LockdownRoomAbility __instance, DoorVariant dv)
        {
            return ValidDoor(__instance.Owner, dv);
        }

    }
}

