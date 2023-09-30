namespace ComAbilities.Types.RueTasks
{
    /// <summary>
    /// Manages IKillables. 
    /// </summary>
    public class TaskPool : IKillable
    {
        public List<IKillable> Tasks { get; private set; } = new();

        ~TaskPool()
        {
            CleanUp();  
        }


        public void Add(IKillable task)
        {
            Tasks.Add(task);
        }


        public void CleanUp()
        {
            foreach (IKillable task in Tasks)
            {
                task.CleanUp();
            }

            Tasks.Clear();
        }
    }
}

