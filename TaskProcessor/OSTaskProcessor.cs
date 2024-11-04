
using NLog;
using OSProj.Generator;
using OSProj.TaskProcessor.ThreadExecutors;
using System.Threading.Tasks;

namespace OSProj.TaskProcessor
{
  public partial class OSTaskProcessor
  {
    private TaskGenerator _taskGenerator = new TaskGenerator();
    private TaskContainer _taskContainer = new TaskContainer();
    private ThreadExecutor? _processingThread;
    public bool Running { get { return _processingThread != null ? _processingThread.IsRunning : false; } }
    private Logger _logger;

    public delegate bool TaskWaitCheck(IOSTask task);
    public event TaskWaitCheck? OnTaskWaitCheck;

    public OSTaskProcessor(Logger logger)
    {
      OnTaskWaitCheck += OnTaskWaitCheckHandler;
      _logger = logger;
    }

    public void Generate()
    {
      _taskGenerator.Generate();
      List<IOSTask> tasks = _taskGenerator.PopGenerated();
      _taskContainer.AddTasks(tasks);
      _taskContainer.UpdateSubscriber();
    }

    public void Start()
    {
      if (_processingThread == null)
      {
        Action threadAction = () =>
        {
          Thread.CurrentThread.IsBackground = true;

          IOSTask? task = _taskContainer.PopMainTask();

          if (task != null)
          {
            Task.Delay(200);
            task.Run();
            _logger.Info($"Task {task.Id} is running.");

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
        };

        _processingThread = new LoopExecutor(threadAction);
        _processingThread.Run();
        _logger.Info("Task processor was started.");
      }
    }

    public void Stop()
    {
      if (_processingThread != null)
      {
        _processingThread.Cancel();
        _processingThread = null;
      }
    }

    public void SetUpdateProcessorInfoDelegate(TaskContainer.UpdateQueuesInfo updateDelegate)
    {
      _taskContainer.SetUpdateQueuesInfo(updateDelegate);
    }

    private bool OnTaskWaitCheckHandler(IOSTask task)
    {
      bool needWait = true;

      if (_taskContainer.GetMaxSourceTasksPriority() > _taskContainer.GetMaxWaitingPriority())
      {
        _taskContainer.FillMainContainerFromSource();
        _logger.Info("Ready queue is filling by source containers.");
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

    private void Preempt(IOSTask task)
    {
      task.Cancel(); // Временно
      task.TaskStatus = OSTaskStatus.Ready;
      _taskContainer.AddTaskToMain(task);
      _logger.Info($"Task {task.Id} was preempted.");
    }

    public void Wait(IOSTask task)
    {
      task.TaskStatus = OSTaskStatus.Waiting;
      _logger.Info($"Task {task.Id} has been placed in the waiting queue.");
    }

    private void Release()
    {
      _taskContainer.FillMainContainerFromWaiting();
      _logger.Info("Release. Now tasks comes from waiting to ready queue.");
    }

    public void Terminate(BaseOSTask task)
    {
      _logger.Info($"Task {task.Id} was terminated.");
    }

    public void Activate(BaseOSTask task)
    {
      _logger.Info($"Task {task.Id} was activated.");
    }

  }
}