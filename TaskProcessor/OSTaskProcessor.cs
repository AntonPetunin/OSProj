
using NLog;
using OSProj.Generator;
using OSProj.TaskProcessor.ThreadExecutors;
using System.Threading.Tasks;

namespace OSProj.TaskProcessor
{
  public partial class OSTaskProcessor
  {
    public delegate void UpdateProgressBarDelegate(double percentage);
    private UpdateProgressBarDelegate? _updateProgressBar;

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

          _activeTask = _taskContainer.GetTopTaskAsActive();
          _taskContainer.UpdateSubscriber();

          if (_activeTask != null)
          {
            Task.Delay(200);

            if (_updateProgressBar != null)
              _activeTask.SetUpdateProgressBaseDelegate(_updateProgressBar);

            _activeTask.Run();

            if (OnTaskWaitCheck != null && OnTaskWaitCheck.Invoke(_activeTask))
            {
              _activeTask.Wait();
            }
          }
          else
          {
            Thread.Sleep(1000);
          }
        };

        _processingThread = new ThreadExecutor(threadAction);
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

    public void SetUpdateProgressBarDelegate(UpdateProgressBarDelegate updateProgressBar)
    {
      _updateProgressBar = updateProgressBar;
    }

    public void PauseActiveTask()
    {
      if (_activeTask != null && _activeTask.TaskType == TaskType.Extended)
        Wait(_activeTask);
    }

    public void TerminateActiveTask()
    {
      if (_activeTask != null)
        Terminate(_activeTask);
    }


    private bool OnTaskWaitCheckHandler(IOSTask task)
    {
      bool needWait = true;

      int waitPriority = _taskContainer.GetMaxWaitingPriority();

      if (_taskContainer.GetMaxSourceTasksPriority() > waitPriority)
      {
        _taskContainer.FillMainContainerFromSource();
        _logger.Info("Ready queue is filling by source containers.");

        if (_taskContainer.GetNextTaskPriority() > task.Priority)
        {
          Preempt(task);
          needWait = false;
        }
      }
      else if (waitPriority > -1)
      { 
        Release();
      }

      return needWait;
    }

    private void Preempt(IOSTask task)
    {
      task.Cancel(); // Временно
      task.SetReadyFromRunning();
      _taskContainer.AddTaskToMain(task);
    }

    private void Wait(IOSTask task)
    {
      if (_taskContainer.AddTaskToWaiting(task))
      {
        ((ExtendedOSTask)task).Pause();
      }
    }

    public void Release()
    {
      _taskContainer.FillMainContainerFromWaiting();
    }

    private void Terminate(IOSTask task)
    {
      if (_taskContainer.AddTaskToSuspended(task))
        task.Cancel();
    }

    public void Activate()
    {
      _taskContainer.FillMainContainerFromSuspended();
    }
  }
}