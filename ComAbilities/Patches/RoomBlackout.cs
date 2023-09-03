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
using PlayerRoles.PlayableScps.Subroutines;
using PlayerRoles;
using System;

// patch to kill 079 even if its in hologram mode after an overcharge
namespace ComAbilities.Patches
{

    [HarmonyPatch(typeof(Scp079BlackoutRoomAbility), nameof(Scp079BlackoutRoomAbility.ServerProcessCmd))]
    internal static class BlackoutRoomCooldown
    {
        private static readonly ComAbilities Instance = ComAbilities.Instance;
        private static void ShowErrorHint(PlayerRoleBase player, RoomLightController controller, Dictionary<uint, double> cooldowns, float cooldown)
        {

        }
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            int index = newInstructions.FindIndex(instruction =>
            instruction.opcode == OpCodes.Ldnull);
           // && (MethodInfo)instruction.operand == PropertyGetter(typeof(Scp079LostSignalHandler), nameof(Scp079LostSignalHandler.Lost)));
            index -= 1;

            Collection<CodeInstruction> collection = new()
            {
                new CodeInstruction(OpCodes.Ldarg_0).MoveLabelsFrom(newInstructions[index]),
                new(OpCodes.Callvirt, PropertyGetter(typeof(ScpSubroutineBase), nameof(ScpSubroutineBase.Role))),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, Field(typeof(Scp079BlackoutRoomAbility), nameof(Scp079BlackoutRoomAbility._roomController))),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, Field(typeof(Scp079BlackoutRoomAbility), nameof(Scp079BlackoutRoomAbility._blackoutCooldowns))),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld, Field(typeof(Scp079BlackoutRoomAbility), nameof(Scp079BlackoutRoomAbility._cooldown))),
  
               new(OpCodes.Call, Method(typeof(BlackoutRoomCooldown), nameof(BlackoutRoomCooldown.ShowErrorHint), new [] { typeof(PlayerRoleBase), typeof(RoomLightController), typeof(Dictionary<uint, double>), typeof(float) }))
               // CodeInstruction.Call(typeof(BlackoutRoomCooldown), nameof(ShowErrorHint))
            };
            newInstructions.InsertRange(index, collection);     

            foreach (CodeInstruction instruction in newInstructions)
                yield return instruction;

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }
    }
}

