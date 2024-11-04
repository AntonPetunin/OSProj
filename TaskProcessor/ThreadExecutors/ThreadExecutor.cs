using OSProj.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OSProj.TaskProcessor.ThreadExecutors
{
  public class ThreadExecutor
  {
    protected bool _isRunning = false;
    protected Task? _task;

    public bool IsRunning { get { return IsRunning; } }
    protected Action ThreadFunction { get; }
    protected CancellationTokenSource? CancelTokenSource { get; set; }

    public ThreadExecutor(Action threadFunc)
    {
      ThreadFunction = threadFunc;
    }

    public virtual void Run()
    {
      if (!_isRunning)
      {
        CancelTokenSource = new CancellationTokenSource();
        _task = Task.Run(ThreadFunction, CancelTokenSource.Token);
        _isRunning = true;
      }
    }

    public virtual void Cancel()
    {
      if (CancelTokenSource != null)
      {
        CancelTokenSource.Cancel();
        CancelTokenSource.Dispose();
        _isRunning = false;
      }
    }

    public virtual void Wait()
    {
      _task?.Wait();
    }

    public virtual void Dispose()
    {
      if (CancelTokenSource != null)
        CancelTokenSource.Dispose();
    }
  }
}
