using ComAbilities.Localizations;
using ComAbilities.Objects;
using ComAbilities.Types.RueTasks;
using Exiled.API.Features;
using System.Text;

namespace ComAbilities.Types
{
    public sealed class TrackerManager : List<ActiveTracker>, IKillable
    {
        private static ComAbilities Instance => ComAbilities.Instance;

        private static TrackerT TrackerT => Instance.Localization.Tracker;
        private static SharedT SharedT => Instance.Localization.Shared;

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

                string color = (tracker.Enabled && tracker.Player != null) ? 
                    (Helper.GetRoleColor(tracker.Player.Role) ?? "#660753") 
                    : "#858784";

                string roleName = tracker?.Player?.Role != null ? SharedT.RoleNames[tracker.Player.Role].ToUpper() : TrackerT.EmptySlot;
                string trackerString = string.Format(string.Format(TrackerT.TrackerFormat, i, color, roleName));
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
        public void CleanUp()
        {
            foreach (ActiveTracker tracker in this)
            {
                tracker.CleanUp();
            }

            base.Clear();
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
    public class ActiveTracker : IKillable
    {
        private readonly UpdateTask _expireTask;
        private readonly Action _killer;

        public Player? Player { get; private set; }
        public int Level { get; }
        public bool Enabled => _expireTask.IsRunning;

        public ActiveTracker(float duration, Action killer, int level)
        {
            _killer = killer;
            _expireTask = new UpdateTask(duration, () => { Player = null; _killer(); });
            this.Level = level;
        }

        public void Start(Player player)
        {
            Player = player;
            _expireTask.Run();
        }

        public void CleanUp()
        {
            _killer();
            _expireTask.CleanUp();
        }
    }
}
