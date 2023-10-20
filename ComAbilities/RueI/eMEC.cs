using System.Collections.ObjectModel;
using System.Collections;
using MEC;
using ComAbilities.Types.RueTasks;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
/*

+-+-+-+-+
|R|u|e|I|
+-+-+-+-+

*/
namespace eMEC
{
    public static class MECExtensions
    {
        /// <summary>
        /// Kills a coroutine.
        /// </summary>
        /// <param name="handle">The handle to kill.</param>
        public static void Kill(this CoroutineHandle handle) => Timing.KillCoroutines(handle);

        /// <summary>
        /// Gets whether or not a coroutine is running.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns>A bool indicating whether or not the coroutine is running.</returns>
        public static bool IsRunning(this CoroutineHandle handle) => Timing.IsRunning(handle);

        /// <summary>
        /// Gets whether or not a coroutine is running or paused.
        /// </summary>
        /// <param name="handle">The handle.</param>
        /// <returns>A bool indicating whether or not the coroutine is running or paused.</returns>
        public static bool IsRunningOrPaused(this CoroutineHandle handle) => Timing.IsRunning(handle) || Timing.IsAliveAndPaused(handle);
    }
    
    /// <summary>
    /// Represents anything that deals with Tasks.
    /// </summary>
    public interface ITaskable : IKillable
    {
        /// <summary>
        /// Recursively loops through the pool, or performs the action if this is a Task.
        /// </summary>
        /// <param name="action">The <see cref="Action{TaskBase}"/> to perform.</param>
        public void DescendOrPerform(Action<TaskBase> action);
    }

    public class UpdateTask : TaskBase
    {
        private Stopwatch stopwatch = new();
       // private DateTimeOffset mostRecentOffset;
       // private TimeSpan? internalClock;

        public Action? Action { get; set; }
        public TimeSpan? Length { get; private set; }
        public TimeSpan? TimeLeft => Length - stopwatch.Elapsed;
        public TimeSpan? ElapsedTime => stopwatch.Elapsed;

        [MemberNotNullWhen(returnValue: true, nameof(TimeLeft))]
        [MemberNotNullWhen(returnValue: true, nameof(ElapsedTime))]
        [MemberNotNullWhen(returnValue: true, nameof(Action))]
        [MemberNotNullWhen(returnValue: true, nameof(Length))]
        public override bool IsRunning => ch?.IsRunningOrPaused() ?? false;

        public void Start(TimeSpan length, Action action)
        {
            if (hasBeenDisposed) return;

            End();
            Action = action;
            Length = length;
            stopwatch.Start();
            ch = Timing.CallDelayed((float)length.TotalSeconds, () =>
            {
                Action();
                ResetState();
            });
        }

        public void Start(float length, Action action) => Start(TimeSpan.FromSeconds(length), action);

        protected override void ResetState()
        {
            stopwatch.Reset();
            base.ResetState();
        }

        public void AddLength(TimeSpan toAdd)
        {
            if (IsRunning) ChangeLength(Length.Value + toAdd);
        }

        public void SubtractLength(TimeSpan toSubtract)
        {
            if (IsRunning) ChangeLength(Length.Value - toSubtract);
        }

        public void ChangeLength(TimeSpan newLength)
        {   
            if (hasBeenDisposed || !IsRunning) return;

            TimeSpan newTime = Length.Value - newLength;
            if (newTime > TimeSpan.Zero)
            {
                ch?.Kill();
                this.Start(newTime, Action);
            } else
            {
                Action();
                ResetState();
            }
        }

        public void Pause()
        {
            if (hasBeenDisposed || !IsRunning) return;

            Timing.PauseCoroutines(ch.Value);
            stopwatch.Stop();
        }

        public void Resume()
        {
            if (hasBeenDisposed || !IsRunning) return;

            Timing.ResumeCoroutines(ch.Value);
            stopwatch.Start();
        }
    }

    public abstract class TaskBase : ITaskable
    {
        protected CoroutineHandle? ch;
        protected bool hasBeenDisposed = false;

        /// <summary>
        /// Gets a bool indicating whether or not this task is currently running.
        /// </summary>
        [MemberNotNullWhen(returnValue: true, nameof(ch))]
        public abstract bool IsRunning { get; }

        /// <summary>
        /// Disposes this task, killing the coroutine. 
        /// </summary>
        public void Dispose()
        {
            if (!hasBeenDisposed && ch.HasValue)
            {
                End();
            }   
        }

        ~TaskBase()
        {
            Dispose();
        }

        public virtual void End()
        {
            ch?.Kill();
            ResetState();
        }

        protected virtual void ResetState()
        {
            ch = null;
        }

        public void DescendOrPerform(Action<TaskBase> action) => action(this);
    }



    public interface IKillable : IDisposable { }

    public class TaskPool : Collection<ITaskable>, ITaskable
    {
        protected bool hasBeenDisposed = false;  

        public void Dispose()
        {
            if (!hasBeenDisposed) {
                foreach (IKillable killable in this)
                {
                    killable.Dispose();
                }
            }
        }

        public void DescendOrPerform(Action<TaskBase> action)
        {
            foreach (ITaskable taskable in this)
            {
                taskable.DescendOrPerform(action);
            }
        }

        ~TaskPool()
        {
            Dispose();
        }
    }
}
