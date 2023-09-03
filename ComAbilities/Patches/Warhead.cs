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
    [HarmonyPatch(typeof(AlphaWarheadController), nameof(AlphaWarheadController.Detonate))]
    internal static class KillHologramNuke
    {
        private static readonly ComAbilities Instance = ComAbilities.Instance;
        [HarmonyPostfix]
        private static void Postfix()
        {
            foreach (CompManager player in Instance.CompDict.All())
            {
                if (player.Hologram.IsActive && player.Role != null)
                {
                    player.Role.Set(RoleTypeId.Scp079);
                    player.AscPlayer.Kill(Exiled.API.Enums.DamageType.Warhead);
                }
            }
        }
    }
}

