using ComAbilities.Types;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.API.Interfaces;
using MEC;

namespace ComAbilities.Objects
{
    public class GeneratorEffects
    {
        private GeneratorEffects() { }

        private static GeneratorEffects singleton { get; set; } = new();
        public static GeneratorEffects Singleton => singleton;

        private static GeneratorEffectsConfigs _config => ComAbilities.Instance.Config.GeneratorEffectsConfigs;


        private const int minTimeUntilExplode = 3;

        private int _lastCount { get; set; } = -1;
        public CoroutineHandle? CH { get; set; }

        public void Kill()
        {
            if (CH.HasValue)
            {
                Timing.KillCoroutines(CH.Value);
                CH = null;
            }
        }

        public void Update()
        {
            if (!_config.DoDoorExploding) return;
            int activatedGens = Generator.Get(Exiled.API.Enums.GeneratorState.Engaged).Count();
            
            // update if number of gens changed
            if (activatedGens > 0 && activatedGens != _lastCount)
            {
                _lastCount = activatedGens;

                if (_config.DoorExplodeInterval.TryGetValue(activatedGens, out Range explodeInterval))
                {
                    if (CH.HasValue) Timing.KillCoroutines(CH.Value);
                    CH = Timing.RunCoroutine(DestroyDoors(explodeInterval));   
                }
            }
        }

        private IEnumerator<float> DestroyDoors(Range range)
        {
            //Random random = new((int)new DateTimeOffset().ToUnixTimeMilliseconds());
            IEnumerable<Door> doors = Door.Get(x => x.IsDamageable && !_config.BlacklistedDoors.Contains(x.Type));
            if (!_config.AllowKeycardDoors) doors = doors.Where(x => !x.IsKeycardDoor);

            var (min, max) = range;
            while (true) {

                yield return Timing.WaitForSeconds(Math.Max(UnityEngine.Random.Range(min, max), minTimeUntilExplode));
                if (_config.FilterAlreadyDestroyed) doors = doors.Where(x => (x is IDamageableDoor door) && !door.IsDestroyed);

                if (doors.Count() == 0)
                {
                    CH = null;
                    Timing.KillCoroutines(Timing.CurrentCoroutine);
                    yield break;
                }
                if (doors.ToList().RandomItem() is IDamageableDoor selectedDoor)
                {
                    selectedDoor.Break();
                }
            }
        }

        internal static void RefreshSingleton() => singleton = new();
    }
}
