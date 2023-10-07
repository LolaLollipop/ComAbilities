using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using ComAbilities.Abilities;
using Exiled.API.Features;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.Voice;
using VoiceChat;
using VoiceChat.Networking;
using static HarmonyLib.AccessTools;

/// implements a patch that redirects radio messages sent to 079 to roundsummary if he is currently scanning
namespace ComAbilities.Patches
{
    [HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
    public static class VoiceTransceiverPatch
    {
        public static VoiceChatChannel ModifyRadio(VoiceChatChannel channel, IVoiceRole listener)
        {
            if (channel == VoiceChatChannel.Radio && listener is Scp079Role role && RadioScanner.ActiveScanners.Contains(role))
            {
                return VoiceChatChannel.RoundSummary;
            }
            else
            {
                return channel;
            }
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
            int index = newInstructions.FindIndex(instruction =>
                instruction.opcode == OpCodes.Callvirt
                && (MethodInfo)instruction.operand == Method(typeof(VoiceModuleBase), nameof(VoiceModuleBase.ValidateReceive)));
            Label doNothingLabel = generator.DefineLabel();
            LocalBuilder localVar = generator.DeclareLocal(typeof(Scp079Role));

            index += 2;
            Collection<CodeInstruction> collection = new()
            {
                //  if (channel == VoiceChatChannel.Radio && listener is Scp079Role role && RadioScanner.ActiveScanners.Contains(role))
                //     channel = VoiceChatChannel.RoundSummary

                // channel == VoiceChatChannel.Radio
                new(OpCodes.Ldloc_S, 5),
                new(OpCodes.Ldc_I4_2),
                new(OpCodes.Bne_Un_S, doNothingLabel),

                // listener is Scp079Role role
                new(OpCodes.Ldloc_S, 4), // IVoiceRole
                new(OpCodes.Isinst, typeof(Scp079Role)),
                new(OpCodes.Stloc_S, localVar), // Scp079Role role
                new(OpCodes.Ldloc_S, localVar),
                new(OpCodes.Brfalse_S, doNothingLabel),  

                // RadioScanner.ActiveScanners.Contains(role))
                new(OpCodes.Call, PropertyGetter(typeof(RadioScanner), nameof(RadioScanner.ActiveScanners))),
                new(OpCodes.Ldloc_S, localVar),
                new(OpCodes.Callvirt, Method(typeof(List<Scp079Role>), nameof(List<Scp079Role>.Contains), new[] { typeof(Scp079Role) })),
                new(OpCodes.Brfalse, doNothingLabel),  
                
                // channel = VoiceChatChannel.RoundSummary
                new(OpCodes.Ldc_I4_5),
                new(OpCodes.Stloc_S, 5),
                new CodeInstruction(OpCodes.Nop).WithLabels(doNothingLabel)  
            };
            newInstructions.InsertRange(index, collection);

            foreach (CodeInstruction instruction in newInstructions)
                yield return instruction;
            
            ListPool<CodeInstruction>.Shared.Return(newInstructions);
        }

    }
}
/* ww
 *     [HarmonyPatch(typeof(Scp079VoiceModule), nameof(Scp079VoiceModule.CurrentChannel), MethodType.Getter)]
    internal static class Radio2
    {
        private static readonly ComAbilities Instance = ComAbilities.Instance;
        [HarmonyPrefix]
        private static bool Prefix(ref VoiceChatChannel __result)
        {
            __result = VoiceChatChannel.Radio;
            return false;  m 
        }
    }*/