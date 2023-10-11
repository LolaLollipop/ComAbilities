// A way to handle coroutine safely (ensures that they don't keep running)
namespace ComAbilities.Types.RueTasks
{
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
    public class PeriodicTask : TaskBase
    {
        public float Interval { get; set; }
        public Action Action { get; set; }
        public Action? OnFinished { get; set; }
        public bool PersistGC { get; set; } = false;

        public PeriodicTask(float time, float interval, Action action, Action? onFinished = null) : base(time)
        {
            Interval = interval;
            Action = action;
            OnFinished = onFinished;
        }

        public override void Run()
        {
            cH?.Kill();
            cH = Timing.RunCoroutine(RunActionPeriodically(Action, OnEnd, Time, Interval));

            base.Run();
        }

        protected override void OnEnd()
        {
            if (OnFinished != null)
            {
                OnFinished();
            }

            base.OnEnd();
        }

        private static IEnumerator<float> RunActionPeriodically(Action action, Action onFinished, float time, float interval)
        {
            int numTimes = (int)Math.Floor((time / interval) + 0.005); // account for floating point errors

            for (int i = 0; i < numTimes; i++)
            {
                // interval: 100, time left: 250 (run)
                // interval: 100, time left: 150 (run)
                // interval: 100, time left: 50 (don't run)
                float remainingTime = time - (interval * i);
                yield return Timing.WaitForSeconds(Math.Min(interval, remainingTime));
                if (remainingTime > interval)
                {
                    action();
                }
            }

            onFinished();
        }
    }
    /// <summary>
    /// Task that runs a function after a certain period of time
    /// </summary>
    public class UpdateTask : TaskBase
    {
        public Action EndAction { get; }

        public UpdateTask(float time, Action endAction) : base(time)
        {
            EndAction = endAction;
        }

        public override void Run()
        {
            cH?.Kill();

            cH = Timing.CallDelayed(Time, OnEnd);

            base.Run();
        }

        protected override void OnEnd()
        {
            EndAction();
            base.OnEnd();
        }

    }

    public abstract class TaskBase : IKillable
    {
        protected CoroutineHandle? cH;

        public bool Enabled { get; protected set; } = false;

        public long? StartedAt { get; private set; }
        public float Time { get; }

        public TaskBase(float time)
        {
            Time = time;
        }

        ~TaskBase()
        {
            CleanUp();
        }

        /// <summary>
        /// Starts this task.
        /// </summary>
        public virtual void Run()
        {
            Enabled = true;
            StartedAt = new DateTimeOffset().ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Ends this task and calls OnEnd.
        /// </summary>
        public virtual void End()
        {
            CleanUp();
            OnEnd();
        }

        /// <summary>
        /// Ends the task without calling OnEnd.
        /// </summary>
        public virtual void CleanUp()
        {
            cH?.Kill();
            Reset();
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

        protected virtual void OnEnd() { }

        private void Reset()
        {
            Enabled = false;
            cH = null;
            StartedAt = null;
        }
        //public void Pool(TaskPool pool) => pool.
    }
}
