using ComAbilities.Objects;
using Exiled.API.Enums;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComAbilities.Types
{
    public sealed class TrackerManager : List<ActiveTracker>
    {
        public int SelectedTracker { get; internal set; } = -1;
       // private List<ActiveTracker> _trackers { get; } = new();
        public TrackerManager() { 
            
        }

        public void StartSelected(Player player)
        {
            this[SelectedTracker].Start(player);
        }
        // Used for quick UI checks
        public TrackerState GetState(int trackerId)
        {
            bool didGet = this.TryGet(trackerId, out ActiveTracker thisTracker);
            if (!didGet) return TrackerState.Nonexistent;
            if (SelectedTracker == trackerId)
            {
                if (thisTracker.Enabled) return TrackerState.SelectedFull;  
                else return TrackerState.Selected;
            } else
            {
                if (thisTracker.Enabled) return TrackerState.Empty;
                else return TrackerState.Full;
            }
        }

        public string ConvertToHintString()
        {
            StringBuilder sb = new StringBuilder();

            for (var i = 0; i < this.Count(); i++)
            {
                ActiveTracker tracker = this[i];

                string color;
                if (tracker.Enabled && tracker.Player != null)
                {
                    color = Helper.RoleColors[tracker.Player.Role] ?? "#660753";
                }
                else
                {
                    color = "#858784";
                }

                string role = tracker.Player?.Role?.Type switch
                {
                    RoleTypeId.ClassD => "CLASS-D",
                    RoleTypeId.ChaosRifleman => "CI RIFLEMAN",
                    RoleTypeId.ChaosConscript => "CI CONSCRIPT",
                    RoleTypeId.ChaosRepressor => "CI REPRESSOR",
                    RoleTypeId.ChaosMarauder => "CI MARAUDER",
                    RoleTypeId.Scientist => "SCIENTIST",
                    RoleTypeId.FacilityGuard => "FACILITY GUARD",
                    RoleTypeId.NtfPrivate => "NTF PRIVATE",
                    RoleTypeId.NtfSergeant => "NTF SERGEANT",
                    RoleTypeId.NtfSpecialist => "NTF SPECIALIST",
                    RoleTypeId.NtfCaptain => "NTF CAPTAIN",
                    RoleTypeId.Tutorial => "TUTORIAL",
                    RoleTypeId.CustomRole => "CANNOT DETECT",
                    _ => "EMPTY"
                };
                // <color=#XXXXX> [ (1) CI RIFLEMAN : PRIMARYFIREARM KEY ]

                string trackerString = $"\n<color={color}[ ({i}) {role} : {tracker.hotkey.ToString().ToUpper()} KEY]</color>";
                if (SelectedTracker == i)
                {
                    trackerString = $"> <b>{trackerString}</b> <";
                }
                sb.Append(trackerString);
            }

            return sb.ToString();
        }

       /* public ActiveTracker this[int index]
        {
            get
            {
                return _trackers[index];
            }
            set
            {
                _trackers[index] = value;
            }
        } */
        /* public void Add(ActiveTracker tracker)
        {
            _trackers.Add(tracker);
        } */
        public void KillAll()
        {
            foreach (ActiveTracker tracker in this)
            {
                tracker.ForceEnd();
            }

        }
      /*  public IEnumerator<ActiveTracker> GetEnumerator()
        {
            return _trackers.GetEnumerator();
        }

       IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        } */
    }
    public class ActiveTracker
    {
        public Player? Player { get; private set; }
        public int Level { get; }
        public bool Enabled => _expireTask.Enabled;
        public AllHotkeys hotkey { get; }

        private UpdateTask _expireTask { get; }
        public ActiveTracker(float duration, Action killer, int level, AllHotkeys hotkey)
        {
            _expireTask = new UpdateTask(duration, () => { Player = null; killer(); });
            this.Level = level;
            this.hotkey = hotkey;
        }

        public void Start(Player player)
        {
            Player = player;
            _expireTask.Run();
        }

        public void ForceEnd()
        {
            _expireTask.Interrupt();
        }
    }
}
