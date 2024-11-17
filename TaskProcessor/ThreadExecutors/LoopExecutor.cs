using OSProj.Generator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSProj.TaskProcessor.ThreadExecutors
{
  public class LoopExecutor : ThreadExecutor
  {
    protected ManualResetEvent _manualResetEvent = new(false);
    private long _loopingCount;

    public LoopExecutor(Action threadFunc, uint loopingCount = 0) : base(threadFunc)
    {
      _loopingCount = loopingCount;
      _manualResetEvent.Set();
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
            for (long i = 0; i < _loopingCount && !CancelTokenSource.Token.IsCancellationRequested; i++)
            {
              _manualResetEvent.WaitOne();
              ThreadFunction();
            }
          };
        }
        else
        {
          action = () =>
          {
            while (!CancelTokenSource.Token.IsCancellationRequested)
            {
              _manualResetEvent.WaitOne();
              ThreadFunction();
            }
          };
        }

        action += () => { CancelTokenSource.Dispose(); };

        _task = Task.Run(action, CancelTokenSource.Token);
        _isRunning = true;
      }
    }

    public override void Cancel()
    {
      if (CancelTokenSource != null)
      {
        CancelTokenSource.Cancel();
        _isRunning = false;
      }
    }
  }
}
