using HarmonyLib;
using MapGeneration.Distributors;
using ComAbilities.Objects;

namespace ComAbilities.Patches
{
    [HarmonyPatch(typeof(Scp079Generator), nameof(Scp079Generator.Engaged), MethodType.Setter)]
    internal static class GeneratorEffectsPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Scp079Generator __instance)
        {
            GeneratorEffects.Singleton.Update();
        }
    }
}   


