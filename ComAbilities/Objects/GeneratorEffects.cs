using ComAbilities.Types;
using CommandSystem.Commands.RemoteAdmin.Broadcasts;
using Exiled.API.Features;
using Exiled.API.Features.Doors;
using Exiled.API.Interfaces;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ComAbilities.Objects
{
    public static class GeneratorEffects
    {
        public static CoroutineHandle? CH { get; set; }

        private static GeneratorEffectsConfigs _config { get; } = ComAbilities.Instance.Config.GeneratorEffectsConfigs;

        private static int _lastCount { get; set; } = -1;

        private const int minTimeUntilExplode = 3;
        public static void Kill()
        {
            if (CH.HasValue)
            {
                Timing.KillCoroutines(CH.Value);
                CH = null;
            }
        }
        public static void Update()
        {
            if (!_config.DoDoorExploding) return;
            int activatedGens = Generator.Get(Exiled.API.Enums.GeneratorState.Engaged).Count();
            
            if (activatedGens > 0 && activatedGens != _lastCount)
            {
                _lastCount = activatedGens;
                Log.Debug(_config.DoorExplodeInterval.ContainsKey(activatedGens));
                if (_config.DoorExplodeInterval.TryGetValue(activatedGens, out Range explodeInterval))
                {
                    if (CH.HasValue) Timing.KillCoroutines(CH.Value);
                    CH = Timing.RunCoroutine(DestroyDoors(explodeInterval));   
                }
            }
        }

        private static IEnumerator<float> DestroyDoors(Range range)
        {
            Random random = new((int)new DateTimeOffset().ToUnixTimeMilliseconds());
            IEnumerable<Door> doors = Door.Get(x => x.IsDamageable && !_config.BlacklistedDoors.Contains(x.Type));
            if (!_config.AllowKeycardDoors) doors = doors.Where(x => !x.IsKeycardDoor);

            var (min, max) = range;
            while (true) {

                yield return Timing.WaitForSeconds(Math.Max(random.Next(min, max), minTimeUntilExplode));
                if (_config.FilterAlreadyDestroyed) doors = doors.Where(x => (x is IDamageableDoor door) && !door.IsDestroyed);

                if (doors.Count() == 0)
                {
                    CH = null;
                    Timing.CurrentCoroutine.Kill();
                    yield break;
                }
                if (doors.ToList().RandomItem() is IDamageableDoor selectedDoor)
                {
                    selectedDoor.Break();
                }
            }
        }
    }

}
