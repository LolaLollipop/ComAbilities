// A way to handle coroutine safely (ensures that they don't keep running)
namespace ComAbilities.Types
{
    using Exiled.API.Features;
    using MEC;

    public static class MECExtensions
    {
        public static void Kill(this CoroutineHandle handle)
        {
            Timing.KillCoroutines(handle);
        }
    }
    /// <summary>
    /// Task that runs an action every interval, optionally doing another action on finished
    /// </summary>
    public class PeriodicTask : Task
    {
        public bool Enabled => (CH != null) && Timing.IsRunning((CoroutineHandle)CH); 

        public float Interval { get; private set; }
        public Action Action { get; private set; }
        public Action? OnFinished { get; private set; }
        public bool PersistGC { get; set; } = false;

        private CoroutineHandle? Killer { get; set; }

        public PeriodicTask(float time, float interval, Action action, Action? onFinished = null)
        {
            Time = time;
            Interval = interval;
            Action = action;
            OnFinished = onFinished;
        }

        ~PeriodicTask()
        {
            if (PersistGC) return;
            if (Killer != null) Timing.KillCoroutines(Killer.Value);
            if (CH != null) Timing.KillCoroutines(CH.Value);
        }

        public override CoroutineHandle Run(float? time = null)
        {
            Log.Debug("Yeah");
            Killer = Timing.CallDelayed(time ?? Time, () =>
            {
                CH?.Kill();
                OnEnd();
            });
            CH = Timing.RunCoroutine(RunAction(Action, Interval));
            return CH.Value;
        }
        private IEnumerator<float> RunAction(Action action, float interval)
        {
            while (true)
            {
                Log.Debug("RUNNING");
                action();
                yield return Timing.WaitForSeconds(interval);
            }
        }

        protected override void OnEnd()
        {
            if (OnFinished != null)
            {
                OnFinished();
            }
            base.OnEnd();
        }
        public override void AttemptKill()
        {
            Killer?.Kill();
            base.AttemptKill();
        }
    }
    /// <summary>
    /// Task that runs a function after a certain period of time
    /// </summary>
    public class UpdateTask : Task
    {
        public bool Enabled { get { return (CH != null) && Timing.IsRunning((CoroutineHandle)CH); } }
        public bool IsRunning { get; private set; } = false;
        public Action EndAction { get; }
        public bool PersistGC { get; set; } = false;

        ~UpdateTask() {
            if (CH != null) Timing.KillCoroutines(CH.Value);
        }

        public UpdateTask(float time, Action endAction)
        {
            Time = time;
            EndAction = endAction;
        }

        public override CoroutineHandle Run(float? time = null)
        {
            IsRunning = true;
            CH = Timing.CallDelayed(time ?? this.Time, OnEnd);
            return CH.Value;
        }
        protected override void OnStart()
        {
            base.OnStart();
        }
        protected override void OnEnd()
        {
            this.IsRunning = false;
            EndAction();
            base.OnEnd();
        }
    }
    /// <summary>
    /// Task that does nothing except wait - use with .Enabled
    /// </summary>
    public class CooldownTask : Task
    {
        public bool Enabled { get { return (CH is not null) && Timing.IsRunning((CoroutineHandle)CH); } }

        public CooldownTask(float time)
        {
            Time = time;
        }

        public override CoroutineHandle Run(float? time = null)
        {
            CH = Timing.CallDelayed(time ?? this.Time, OnEnd);
            return CH.Value;
        }
        /*   public override void OnStart()
           {
               base.OnStart();
           }

           public override void OnEnd()
           {
               base.OnEnd();
           } */
    }
    public abstract class Task
    {
        public CoroutineHandle? CH { get; protected set; }
        public long? StartedAt { get; private set; }
        public float Time { get; protected set; }
        public abstract CoroutineHandle Run(float? time = null);

        protected virtual void OnEnd() { }
        
        protected virtual void OnStart()
        {
            CH = Run(Time);
            StartedAt = new DateTimeOffset().ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Ends the task and calls the OnEnd function
        /// </summary>
        public void Interrupt()
        {
            if (CH != null)
            {
                Timing.KillCoroutines((CoroutineHandle)CH);
                StartedAt = null;
            }

            StartedAt = null;
            OnEnd();
        }

        /// <summary>
        /// Ends the task and does NOT call the OnEnd function
        /// </summary>
        public virtual void AttemptKill()
        {
            CH?.Kill();
            StartedAt = null;
        }
        public long? GetETA()
        {
            if (StartedAt == null) return null;
            
            return (long?)((Time * 1000) - (new DateTimeOffset().ToUnixTimeMilliseconds() - StartedAt));
        }
        public long? RunningFor()
        {
            if (StartedAt == null) return null;

            return new DateTimeOffset().ToUnixTimeMilliseconds() - StartedAt;
        }


    }
}
