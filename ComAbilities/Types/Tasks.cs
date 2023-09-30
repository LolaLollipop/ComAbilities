// A way to handle coroutine safely (ensures that they don't keep running)
namespace ComAbilities.Types.RueTasks
{
    using Exiled.API.Features;
    using global::ComAbilities.UI;
    using MEC;
    using System.Diagnostics.CodeAnalysis;

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
    public class PeriodicTask : TaskBase
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
        public override void CleanUp()
        {
            Killer?.Kill();
            base.CleanUp();
        }
    }
    /// <summary>
    /// Task that runs a function after a certain period of time
    /// </summary>
    public class UpdateTask : TaskBase
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
            if (CH.HasValue) Timing.KillCoroutines(CH.Value);

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

    public abstract class TaskBase : IKillable
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
            CH?.Kill();

            StartedAt = null;
            OnEnd();
        }

        /// <summary>
        /// Ends the task and does NOT call the OnEnd function
        /// </summary>
        public virtual void CleanUp()
        {
            CH?.Kill();
            StartedAt = null;
        }

        public float? GetETA()
        {
            if (StartedAt == null) return null;
            
            return ((Time * 1000) - (new DateTimeOffset().ToUnixTimeMilliseconds() - StartedAt));
        }

        public float? RunningFor()
        {
            if (StartedAt == null) return null;

            return new DateTimeOffset().ToUnixTimeMilliseconds() - StartedAt;
        }

        //public void Pool(TaskPool pool) => pool.
    }
}
