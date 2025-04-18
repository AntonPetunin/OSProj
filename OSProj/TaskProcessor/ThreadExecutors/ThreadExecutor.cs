﻿namespace OSProj.TaskProcessor.ThreadExecutors
{
  public class ThreadExecutor
  {
    protected long _loopingCount;
    protected bool _isRunning = false;
    protected Task? _task;

    public bool IsRunning { get { return _isRunning; } }
    protected Action ThreadFunction { get; }
    protected CancellationTokenSource? CancelTokenSource { get; set; }

    public ThreadExecutor(Action threadFunc, uint loopingCount = 0)
    {
      ThreadFunction = threadFunc;
      _loopingCount = loopingCount;
    }

    public virtual void Run()
    {
      if (!_isRunning)
      {
        CancelTokenSource = new CancellationTokenSource();
        Action action = () =>
        {
          while (!CancelTokenSource.Token.IsCancellationRequested)
            ThreadFunction();

          CancelTokenSource.Dispose();
        };

        _task = Task.Run(action, CancelTokenSource.Token);
        _isRunning = true;
      }
    }

    public virtual void Cancel()
    {
      if (CancelTokenSource != null)
      {
        CancelTokenSource.Cancel();
        _isRunning = false;
      }
    }

    public virtual void Wait()
    {
      _task?.Wait();
    }

    public virtual void Dispose()
    {
      CancelTokenSource?.Dispose();
    }
  }
}
