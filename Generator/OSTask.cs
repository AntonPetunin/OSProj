namespace OSProj.Generator
{
    public class OSTask
    {
        private int _id;
        private Task? _activeTask;

        public int Id { get { return _id; } }
        public int Priority { get; }
        public TaskType TaskType { get; }
        public Action TaskFunction { get; }
        public OSTaskStatus TaskStatus { get; set; } = OSTaskStatus.Created;
        private CancellationTokenSource? CancelTokenSource { get; set; }
        private bool IsRunning { get; set; } = false;


        public OSTask(int id, int priority, TaskType taskType, Action taskFunc)
        {
            _id = id;
            Priority = priority;
            TaskType = taskType;
            TaskFunction = taskFunc;
        }

        public void Cancel()
        {
            if (CancelTokenSource != null)
            {
                CancelTokenSource.Cancel();
                CancelTokenSource.Dispose();
                IsRunning = false;
            }
        }

        public void Run()
        {
            if (!IsRunning)
            {
                CancelTokenSource = new CancellationTokenSource();
                _activeTask = Task.Run(TaskFunction, CancelTokenSource.Token);
                IsRunning = true;
            }
        }

        public void Wait()
        {
            if (_activeTask != null)
                _activeTask.Wait();
        }

        public void Dispose()
        {
            if (CancelTokenSource != null)
                CancelTokenSource.Dispose();
        }
    }
}