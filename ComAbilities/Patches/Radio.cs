using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;
using ComAbilities.Abilities;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles.PlayableScps;
using PlayerRoles.PlayableScps.Scp079;
using PlayerRoles.Spectating;
using PlayerRoles.Voice;
using VoiceChat;
using VoiceChat.Networking;
using static HarmonyLib.AccessTools;

[HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
public static class VoiceTransceiverPatch
{
    public static VoiceChatChannel ModifyRadio(VoiceChatChannel channel, IVoiceRole listener)
    {
        return channel == VoiceChatChannel.Radio            
            && listener is Scp079Role role
            && RadioScanner.ActiveScanners.Contains(role) ? VoiceChatChannel.RoundSummary : channel;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);
        int index = newInstructions.FindIndex(instruction =>    
        instruction.opcode == OpCodes.Callvirt
        && (MethodInfo)instruction.operand == Method(typeof(VoiceModuleBase), nameof(VoiceModuleBase.ValidateReceive)));
        index += 1;
        Collection<CodeInstruction> collection = new()
        {
            new CodeInstruction(OpCodes.Ldloc, 4),
            CodeInstruction.Call(typeof(VoiceTransceiverPatch), nameof(ModifyRadio)),
        };
        newInstructions.InsertRange(index, collection);

        foreach (CodeInstruction instruction in newInstructions)
            yield return instruction;
        
        ListPool<CodeInstruction>.Shared.Return(newInstructions);
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