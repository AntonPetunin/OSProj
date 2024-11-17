
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
    private Logger _logger;
    private volatile IOSTask? _activeTask = null;

    public bool Running { get { return _processingThread != null ? _processingThread.IsRunning : false; } }

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

          _activeTask = _taskContainer.PopMainTask();

          if (_activeTask != null)
          {
            Task.Delay(200);
            _activeTask.Run();
            _logger.Info($"Task {_activeTask.Id} is running.");

            if (OnTaskWaitCheck != null && OnTaskWaitCheck.Invoke(_activeTask))
            {
              _activeTask.Wait();
              _activeTask.Dispose();
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

    public void PauseActiveTask()
    {
      if (_activeTask != null && _activeTask.TaskType == TaskType.Extended)
      {
        ((ExtendedOSTask)_activeTask).Pause();
        Wait(_activeTask);
      }
    }

    public void TerminateActiveTask()
    {
      if (_activeTask != null)
      {
        Terminate(_activeTask);
      }
    }


    private bool OnTaskWaitCheckHandler(IOSTask task)
    {
      bool needWait = true;

      // if (activateEventReceived)
      // Activate()
      int waitPriority = _taskContainer.GetMaxWaitingPriority();

      if (_taskContainer.GetMaxSourceTasksPriority() > waitPriority)
      {
        _taskContainer.FillMainContainerFromSource();
        _logger.Info("Ready queue is filling by source containers.");
      }
      else if (waitPriority > -1)
        Release();

      if (_taskContainer.GetNextTaskPriority() > task.Priority)
      {
        Preempt(task);
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

    private void Wait(IOSTask task)
    {
      task.TaskStatus = OSTaskStatus.Waiting;
      _taskContainer.AddTaskToWaiting(task);
      _logger.Info($"Task {task.Id} has been placed in the waiting queue.");
    }

    private void Release()
    {
      _taskContainer.FillMainContainerFromWaiting();
      _logger.Info("Release. Now tasks comes from waiting to ready queue.");
    }

    private void Terminate(IOSTask task)
    {
      _taskContainer.AddTaskToSuspended(task);
      _logger.Info($"Task {task.Id} was terminated.");
    }

    private void Activate()
    {
      //_taskContainer.FillMainContainerFromSuspended();
      //_logger.Info($"Task {task.Id} was activated.");
    }
  }
}