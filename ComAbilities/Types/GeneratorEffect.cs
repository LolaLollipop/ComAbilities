using ComAbilities.Objects;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.API.Features.Roles;
using GameCore;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utf8Json.Resolvers.Internal;

namespace ComAbilities.Types
{
    // public struct EmptyArgs { }
    public abstract class GeneratorEffect : IKillable
    {
        protected static GeneratorEffectsConfigs config => ComAbilities.Instance.Config.GeneratorEffectsConfigs;

        private const int minTimeUntilExplode = 3;
        private static int _lastCount = -1;

        protected CoroutineHandle? CH;

        public abstract bool Enabled { get; }

        public GeneratorEffect() { }

        public void CleanUp()
        {
            if (CH.HasValue)
            {
                Timing.KillCoroutines(CH.Value);
                CH = null;
            }
        }

        public abstract void OnUpdate(int newGens);
    }
}
