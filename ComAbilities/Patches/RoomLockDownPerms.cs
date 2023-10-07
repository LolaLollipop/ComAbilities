using Exiled.API.Features;
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp079;
using KeycardPermissions = Interactables.Interobjects.DoorUtils.KeycardPermissions;
using Interactables.Interobjects.DoorUtils;
using ComAbilities.Objects;
using Exiled.API.Enums;
using Exiled.API.Features.Doors;

namespace ComAbilities.Patches
{
    [HarmonyPatch(typeof(Scp079LockdownRoomAbility), nameof(Scp079LockdownRoomAbility.ValidateDoor))]
    internal static class LockdownRoomPatch
    {
        private static readonly ComAbilities Instance = ComAbilities.Instance;
        private static bool ValidDoor(ReferenceHub refHub, DoorVariant door)
        {
            if (!Instance.Config.DoComputerPerms) return true;
            Player player = Player.Get(refHub);
            CompManager manager = Instance.CompDict.GetOrError(player);

            KeycardPermissions doorPerms = door.RequiredPermissions.RequiredPermissions;
            if (!manager.CachedKeycardPermissions.HasFlag(doorPerms))
            {
                Door.Get(door).PlaySound(DoorBeepType.PermissionDenied);
                manager.ShowErrorHint(Instance.Localization.Errors.DisplayAccessDenied);

                return false;
            }
            return true;
        }
        private static bool Prefix(Scp079LockdownRoomAbility __instance, DoorVariant dv)
        {
            return ValidDoor(__instance.Owner, dv);
        }

    }
}

