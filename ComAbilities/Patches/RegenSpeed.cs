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
using PlayerRoles.PlayableScps.Scp079;
using ComAbilities.Objects;
using ComAbilities.Types;

namespace ComAbilities.Patches
{

    [HarmonyPatch(typeof(Scp079AuxManager), nameof(Scp079AuxManager.RegenSpeed), MethodType.Getter)]
    internal static class Scp079AuxManagerFix
    {
        private static readonly ComAbilities Instance = ComAbilities.Instance;


        [HarmonyPostfix]
        private static void Postfix(Scp079AuxManager __instance, ref float __result)
        {
            float addMultiplier = 1;
            CompManager manager = Instance.CompDict.GetOrError(Player.Get(__instance.Owner));

            manager.ActiveAbilities.ForEach((IReductionAbility ability) =>
            {
                addMultiplier *= ability.AuxModifier;
            });
            __result *= addMultiplier;
            
        }
    }
}

