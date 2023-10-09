using Exiled.API.Features;
using HarmonyLib;
using ComAbilities.Objects;
using System.Reflection.Emit;
using NorthwoodLib.Pools;
using System.Reflection;
using static HarmonyLib.AccessTools;
using Mono.Collections.Generic;
using PlayerRoles.PlayableScps.Scp079;
using Exiled.API.Enums;
using KeycardPermissions = Interactables.Interobjects.DoorUtils.KeycardPermissions;
using Interactables.Interobjects.DoorUtils;
using Exiled.API.Features.Doors;

namespace ComAbilities.Patches
{

    [HarmonyPatch(typeof(Scp079DoorLockChanger), nameof(Scp079DoorLockChanger.ServerProcessCmd))]
    internal static class LockPatch
    {
        private static ComAbilities Instance => ComAbilities.Instance;

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

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            if (Instance.Config.DoComputerPerms)
            {
                int index = newInstructions.FindIndex(instruction =>
                    instruction.opcode == OpCodes.Call
                    && (MethodInfo)instruction.operand == PropertyGetter(typeof(Scp079AbilityBase), nameof(Scp079AbilityBase.LostSignalHandler)));

                index -= 1;

                Label returnLabel = generator.DefineLabel();

                Collection<CodeInstruction> collection = new()
                {
                    // if (!ValidateDoor(this.LastDoor, base.Owner)) return;
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new(OpCodes.Callvirt, PropertyGetter(typeof(Scp079DoorLockChanger), nameof(Scp079DoorLockChanger.Owner))),
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, Field(typeof(Scp079DoorLockChanger), nameof(Scp079DoorLockChanger.LastDoor))),
                    new(OpCodes.Call, Method(typeof(LockPatch), nameof(ValidDoor), new [] { typeof(ReferenceHub), typeof(DoorVariant) })),
                    new(OpCodes.Brfalse_S, returnLabel),
                };

                newInstructions.InsertRange(index, collection);

                newInstructions[newInstructions.Count - 1].labels.Add(returnLabel);

            }

            foreach (CodeInstruction instruction in newInstructions)
                yield return instruction;

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

    }
}

