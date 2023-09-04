using ComAbilities.Types;
using CommandSystem.Commands.RemoteAdmin.Broadcasts;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComAbilities.Objects
{
    /// <summary>
    /// EXtensions for handling CompDicts for other plugins
    /// </summary>
    public static class CAExtensions
    {
        private static ComAbilities Instance = ComAbilities.Instance;

        public static bool TryGetCM(this Player player, out CompManager manager) => Instance.CompDict.TryGet(player, out manager);
        public static CompManager GetCM(this Player player) => Instance.CompDict.GetOrError(player);
    }

}
