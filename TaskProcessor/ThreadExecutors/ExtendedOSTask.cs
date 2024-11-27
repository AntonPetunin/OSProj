using OSProj.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OSProj.TaskProcessor.ThreadExecutors
{
  public class ExtendedOSTask : BaseOSTask, IExtendedTasksStateSetter
  {
    private ManualResetEventSlim _processorThreadEvent = new(false);
    private ManualResetEventSlim _taskEvent = new(false);

    private bool _paused = false;
    public override TaskType TaskType { get { return TaskType.Extended; } }
    public bool Paused { get { return _paused; } }


    public ExtendedOSTask(int id, int priority, Action threadFunc, uint loopingCount = 0)
      : base(id, priority, threadFunc, loopingCount)
    {
      _processorThreadEvent.Reset();
      _taskEvent.Set();
    }

    public override void Run()
    {
      if (!Paused)
      {
        if (!_isRunning)
        {
          CancelTokenSource = new CancellationTokenSource();
          Action action = () =>
          {
            _processorThreadEvent.Reset();
          };

          if (_loopingCount > 0)
          {
            action += () =>
            {
              for (long i = 0; i < _loopingCount && !CancelTokenSource.Token.IsCancellationRequested; i++)
              {
                _taskEvent.Wait();
                ThreadFunction();
              }
            };
          }
          else
          {
            action += () =>
            {
              while (!CancelTokenSource.Token.IsCancellationRequested)
              {
                _taskEvent.Wait();
                ThreadFunction();
              }
            };
          }

          action += () =>
          {
            _processorThreadEvent.Set();
            CancelTokenSource.Dispose();
          };

          _task = Task.Run(action, CancelTokenSource.Token);
          _isRunning = true;
        }
      }
      else
        Resume();

      SetRunningState();
    }

    public override void Cancel()
    {
      base.Cancel();
      _processorThreadEvent.Set();
    }

    public override void Wait()
    {
      _processorThreadEvent.Wait();
    }

    public override void Dispose()
    {
      base.Dispose();
    }

    public void Pause()
    {
      _processorThreadEvent.Set();
      _taskEvent.Reset();
      _paused = true;
    }

    public void Resume()
    {
      _taskEvent.Set();
      _processorThreadEvent.Reset();
      _paused = false;
    }

    public void SetWaitingState()
    {
      TaskStatus = OSTaskStatus.Waiting;
      ProcessorInfo.logger.Info($"EXTENDED: id={Id} with priority={Priority} has been placed in the WAITING queue.");
    }

    public override void SetSuspendedState()
    {
      TaskStatus = OSTaskStatus.Suspended;
      ProcessorInfo.logger.Info($"EXTENDED: id={Id} with priority={Priority} has been placed in the SUSPENDED queue.");
    }

    public override void SetReadyFromSuspended()
    {
      TaskStatus = OSTaskStatus.Ready;
      ProcessorInfo.logger.Info($"EXTENDED: id={Id} with priority={Priority} has been placed in the READY queue.");
    }

    public void SetReadyFromWaiting()
    {
      TaskStatus = OSTaskStatus.Ready;
      ProcessorInfo.logger.Info($"EXTENDED: id={Id} with priority={Priority} has been placed in the READY queue from WAITING.");
    }

    public override void SetReadyFromRunning()
    {
      TaskStatus = OSTaskStatus.Ready;
      ProcessorInfo.logger.Info($"EXTENDED: id={Id} with priority={Priority} has been placed in the READY queue from RUNNING.");
    }

    public override void SetRunningState()
    {
      TaskStatus = OSTaskStatus.Running;
      ProcessorInfo.logger.Info($"EXTENDED: id={Id} with priority={Priority} start executing (RUNNING).");
    }
  }
}
