using OSProj.Generator;

namespace OSProj.TaskProcessor.ThreadExecutors
{
    public class OSTask : ThreadExecutor
    {
        private int _id;
        public int Id { get { return _id; } }
        public int Priority { get; }
        public TaskType TaskType { get; }
        public OSTaskStatus TaskStatus { get; set; } = OSTaskStatus.Created;


        public OSTask(int id, int priority, TaskType taskType, Action taskFunc)
          : base(taskFunc)
        {
            _id = id;
            Priority = priority;
            TaskType = taskType;
        }
    }
}