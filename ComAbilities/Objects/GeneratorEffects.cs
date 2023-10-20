using ComAbilities.Types;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.API.Interfaces;
using MEC;

namespace ComAbilities.Objects
{
    // TODO: ensure that this shit doesnt work unless the plugin is enabledw
    public class GeneratorEffects : IKillable
    {
        private static GeneratorEffectsConfigs config => ComAbilities.Instance.Config.GeneratorEffectsConfigs;

        private GeneratorEffects() {
            IEnumerable<Door> doors = Door.List.Where(x => config.BlacklistedDoors.Contains(x.Type));
            if (!config.AllowKeycardDoors) doors = doors.Where(x => !x.IsKeycardDoor);
            availableDoors = doors.OfType<IDamageableDoor>();
        }

        private const int minTimeUntilExplode = 3;
        private int _lastCount = -1;
        private CoroutineHandle? CH;
        private IEnumerable<IDamageableDoor> availableDoors;

        private static GeneratorEffects singleton = new();
        public static GeneratorEffects Singleton => singleton;

        public void CleanUp()
        {
            if (CH.HasValue)
            {
                Timing.KillCoroutines(CH.Value);
                CH = null; 
            }
        }

        public void Update()
        {
            if (!config.DoDoorExploding) return;
            int activatedGens = Generator.Get(Exiled.API.Enums.GeneratorState.Engaged).Count();
            
            // update if number of gens changed
            if (activatedGens > 0 && activatedGens != _lastCount)
            {
                _lastCount = activatedGens;

                if (config.DoorExplodeInterval.TryGetValue(activatedGens, out Range explodeInterval))
                {
                    if (CH.HasValue) Timing.KillCoroutines(CH.Value);
                    CH = Timing.RunCoroutine(_DestroyDoors(explodeInterval));   
                }
            }
        }

        private IEnumerator<float> _DestroyDoors(Range range)
        {
            var (min, max) = range;
            while (true) {

                yield return Timing.WaitForSeconds(Math.Max(UnityEngine.Random.Range(min, max), minTimeUntilExplode));
                if (config.FilterAlreadyDestroyed) availableDoors = availableDoors.Where(x => !x.IsDestroyed);

                if (availableDoors.Count() == 0)
                {
                    CH = null;
                    Timing.KillCoroutines(Timing.CurrentCoroutine);
                    yield break;
                }

                availableDoors.GetRandomValue().Break();
            }
        }

        internal static void RefreshSingleton()
        {
            singleton.CleanUp();
            singleton = new();
                
        }
    }
}
