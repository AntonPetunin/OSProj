
using OSProj.Generator;
using OSProj.TaskProcessor;

namespace OSProj.TaskProcessor
{
  public partial class OSTaskProcessor
  {
    private TaskGenerator _taskGenerator = new TaskGenerator();
    private TaskContainer _taskContainer = new TaskContainer();
    private Thread? _processingThread;
    public bool Running { get { return _processingThread != null ? _processingThread.IsAlive : false; } }
    //private ILogger<HomeController> _logger;

    public delegate bool TaskWaitCheck(OSTask task);
    public event TaskWaitCheck? OnTaskWaitCheck;

    public OSTaskProcessor(/*ILogger<HomeController> logger*/)
    {
      OnTaskWaitCheck += OnTaskWaitCheckHandler;
      //_logger = logger;
    }

    public void Generate()
    {
      _taskGenerator.Generate();
      List<OSTask> tasks = _taskGenerator.PopGenerated();
      _taskContainer.AddTasks(tasks);
      _taskContainer.UpdateSubscriber();
    }

    public void Start()
    {
      if (_processingThread == null)
      {
        _processingThread = new Thread(() =>
        {
          Thread.CurrentThread.IsBackground = true;

          while (true)
          {
            OSTask? task = _taskContainer.PopMainTask();

            if (task != null)
            {
              task.Run();
              //_logger.LogInformation("Task {id} is running.", task.Id);

              if (OnTaskWaitCheck != null && OnTaskWaitCheck.Invoke(task))
              {
                task.Wait();
                task.Dispose();
              }

              _taskContainer.UpdateSubscriber();
            }
            else
            {
              Thread.Sleep(1000);
            }
          }
        });

        _processingThread.Start();
        //_logger.LogInformation("Task processor was started.");
      }
    }

    public void SetUpdateProcessorInfoDelegate(TaskContainer.UpdateQueuesInfo updateDelegate)
    {
      _taskContainer.SetUpdateQueuesInfo(updateDelegate);
    }

    private bool OnTaskWaitCheckHandler(OSTask task)
    {
      bool needWait = true;

      if (_taskContainer.GetMaxSourceTasksPriority() > _taskContainer.GetMaxWaitingPriority())
      {
        _taskContainer.FillMainContainerFromSource();
        //_logger.LogInformation("Ready queue is filling by source containers.");
      }
      else
        Release();

      if (_taskContainer.GetNextTaskPriority() > task.Priority)
      {
        if (task.TaskType == TaskType.Base)
          Preempt(task);
        else if (task.TaskType == TaskType.Extended)
          Wait(task);

        needWait = false;
      }

      return needWait;
    }

    private void Preempt(OSTask task)
    {
      if (task.TaskType == TaskType.Extended)
      {
        // TODO Сделать реализация для extended
        task.Cancel(); // Временно
      }
      else
      {
        // TODO Сделать реализацию для base
        task.Cancel();
      }

      task.TaskStatus = OSTaskStatus.Ready;
      _taskContainer.AddTaskToMain(task);
      //_logger.LogInformation("Task {id} was preempted.", task.Id);
    }

    private void Wait(OSTask task)
    {

      //_logger.LogInformation("Task {id} has been placed in the waiting queue.", task.Id);
    }

    private void Release()
    {
      _taskContainer.FillMainContainerFromWaiting();
      //_logger.LogInformation("Release. Now tasks comes from waiting to ready queue.");
    }

    private void Terminate(OSTask task)
    {

      //_logger.LogInformation("Task {id} was terminated.", task.Id);
    }

    private void Activate(OSTask task)
    {

      //_logger.LogInformation("Task {id} was activated.", task.Id);
    }
  }
}