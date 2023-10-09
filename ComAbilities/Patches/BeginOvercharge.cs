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
using PlayerRoles;


// patch to kill 079 even if its in hologram mode after an overcharge
namespace ComAbilities.Patches
{

    [HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.BeginOvercharge))]
    internal static class Scp079Overcharge
    {
        private static readonly ComAbilities Instance = ComAbilities.Instance;

        [HarmonyPrefix]
        private static void Prefix(Scp079Recontainer __instance)
        {

            foreach (Player player in Player.List)
            {
                if (player.Role == RoleTypeId.Scp106 && player.SessionVariables["ComAbilities_hologram"] != null)
                {
                    player.Role.Set(RoleTypeId.Scp079);
                }
            }
        }
    }
}

