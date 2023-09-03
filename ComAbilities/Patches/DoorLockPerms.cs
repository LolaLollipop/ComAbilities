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
                    Door.Get(door).PlaySound(DoorBeepType.PermissionDenied);
                    CompManager compManager = Instance.CompDict.GetOrError(player);
                    compManager.TryShowErrorHint(Instance.Localization.Errors.DisplayAccessDenied);

                    return false;
                }   
                return true;
            } catch(Exception e)
            {
                Log.Debug(e);
                return false;
            }
        }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            int index = newInstructions.FindIndex(instruction =>
            instruction.opcode == OpCodes.Call
            && (MethodInfo)instruction.operand == PropertyGetter(typeof(Scp079AbilityBase), nameof(Scp079AbilityBase.LostSignalHandler)));
            index -= 1;

            Label returnLabel = generator.DefineLabel();

            Collection<CodeInstruction> collection = new()
            {
                new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(newInstructions[index]),
                // if (!ValidateDoor(this.LastDoor, base.Owner)) return;
                new(OpCodes.Callvirt, PropertyGetter(typeof(Scp079DoorLockChanger), nameof(Scp079DoorLockChanger.Owner))),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, Field(typeof(Scp079DoorLockChanger), nameof(Scp079DoorLockChanger.LastDoor))),
                new(OpCodes.Call, Method(typeof(LockPatch), nameof(ValidDoor), new [] { typeof(ReferenceHub), typeof(DoorVariant) })),
                new(OpCodes.Brfalse_S, returnLabel),
            };
            newInstructions.InsertRange(index, collection);

            newInstructions[^1].labels.Add(returnLabel);

            foreach (CodeInstruction instruction in newInstructions)
                yield return instruction;

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

    }
}

