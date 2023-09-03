using Exiled.API.Features;
using HarmonyLib;
using MapGeneration.Distributors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComAbilities;
using Exiled.Loader;
using PlayerRoles;
using ComAbilities.Objects;

namespace ComAbilities.Patches
{
    [HarmonyPatch(typeof(Scp079Generator), nameof(Scp079Generator.Engaged), MethodType.Setter)]
    internal static class GeneratorEffectsPatch
    {
        [HarmonyPostfix]
        private static void Postfix(Scp079Generator __instance)
        {
            GeneratorEffects.Update();
        }
    }
}   

