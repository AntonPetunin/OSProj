using OSProj.Generator;

namespace OSProj.TaskProcessor.ThreadExecutors
{
  public class BaseOSTask : ThreadExecutor, IOSTask
  {
    protected OSTaskProcessor.UpdateProgressBarDelegate? _updateProgressBar;

    public int Id { get; }
    public int Priority { get; }
    public virtual TaskType TaskType { get { return TaskType.Base; } }
    public OSTaskStatus TaskStatus { get; set; } = OSTaskStatus.Ready;
    public DateTime CreationTime { get; }

    public BaseOSTask(int id, int priority, Action threadFunc, uint loopingCount = 0)
      : base(threadFunc, loopingCount)
    {
      Id = id;
      Priority = priority;
      CreationTime = DateTime.UtcNow;
    }

    public override void Run()
    {
      if (!_isRunning)
      {
        CancelTokenSource = new CancellationTokenSource();
        Action action;

        if (_loopingCount > 0)
        {
          action = () =>
          {
            for (long i = 1; i <= _loopingCount && !CancelTokenSource.Token.IsCancellationRequested; i++)
            {
              ThreadFunction();
              if (_updateProgressBar != null)
              {
                double persentage = (double)i / (_loopingCount);
                _updateProgressBar.Invoke(persentage);
              }
            }
          };
        }
        else
        {
          action = () =>
          {
            while (!CancelTokenSource.Token.IsCancellationRequested)
              ThreadFunction();
          };
        }

        if (_updateProgressBar != null)
          _updateProgressBar.Invoke(0);

        _task = Task.Run(action, CancelTokenSource.Token);
        _isRunning = true;
      }

      SetRunningState();
    }

    public override void Cancel()
    {
      base.Cancel();
    }

    public override void Wait()
    {
      base.Wait();
    }

    public override void Dispose()
    {
      base.Dispose();
    }

    public virtual void SetSuspendedState()
    {
      TaskStatus = OSTaskStatus.Suspended;
      View.ProcessorInfo.logger.Info($"BASE: id={Id} with priority={Priority} has been placed in the SUSPENDED queue. TERMINATE.");
    }

    public virtual void SetReadyFromSuspended()
    {
      TaskStatus = OSTaskStatus.Ready;
      View.ProcessorInfo.logger.Info($"BASE: id={Id} with priority={Priority} has been placed in the READY queue from SUSPENDED. ACTIVATE.");
    }

    public virtual void SetReadyFromRunning()
    {
      TaskStatus = OSTaskStatus.Ready;
      View.ProcessorInfo.logger.Info($"BASE: id={Id} with priority={Priority} has been placed in the READY queue from RUNNING. PREEMPT.");
    }

    public virtual void SetRunningState()
    {
      TaskStatus = OSTaskStatus.Running;
      View.ProcessorInfo.logger.Info($"BASE: id={Id} with priority={Priority} start executing. RUNNING.");
    }

    public void SetUpdateProgressBarDelegate(OSTaskProcessor.UpdateProgressBarDelegate updateProgressBar)
    {
      _updateProgressBar = updateProgressBar;
    }
  }
}