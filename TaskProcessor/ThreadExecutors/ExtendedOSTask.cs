using OSProj.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OSProj.TaskProcessor.ThreadExecutors
{
  public class ExtendedOSTask : LoopExecutor, IOSTask
  {
    private bool _paused = false;

    public int Id { get; }
    public int Priority { get; }
    public TaskType TaskType { get; }
    public OSTaskStatus TaskStatus { get; set; } = OSTaskStatus.Created;
    public bool Paused { get { return _paused; } }

    public delegate void PauseTaskHandler();
    public event PauseTaskHandler? OnTaskPause;
    public delegate void ResumeTaskHandler();
    public event ResumeTaskHandler? OnTaskResume;

    public ExtendedOSTask(int id, int priority, TaskType taskType, Action threadFunc, uint loopingCount = 0) : base(threadFunc, loopingCount)
    {
      Id = id;
      Priority = priority;
      TaskType = taskType;

      OnTaskPause += () => _manualResetEvent.Reset();
      OnTaskResume += () => _manualResetEvent.Set();
    }

    public override void Run()
    {
      base.Run();
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

    public void Pause()
    {
      OnTaskPause?.Invoke();
      _paused = true;
    }

    public void Resume()
    {
      OnTaskResume?.Invoke();
      _paused = false;
    }

    public void SetWaitingState()
    {
      throw new NotImplementedException();
    }

    public void SetActivatedState()
    {
      throw new NotImplementedException();
    }

    public void SetReadyFromSuspended()
    {
      throw new NotImplementedException();
    }

    public void SetReadyFromWaiting()
    {
      throw new NotImplementedException();
    }

    public void SetReadyFromRunning()
    {
      throw new NotImplementedException();
    }

    public void SetRunningState()
    {
      throw new NotImplementedException();
    }
  }
}
