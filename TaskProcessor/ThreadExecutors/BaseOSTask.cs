using OSProj.Generator;

namespace OSProj.TaskProcessor.ThreadExecutors
{
  public class BaseOSTask : ThreadExecutor, IOSTask
  {
    public int Id { get; }
    public int Priority { get; }
    public TaskType TaskType { get; }
    public OSTaskStatus TaskStatus { get; set; } = OSTaskStatus.Created;

    public BaseOSTask(int id, int priority, TaskType taskType, Action taskFunc)
      : base(taskFunc)
    {
      Id = id;
      Priority = priority;
      TaskType = taskType;
      OnTerminateTask += Cancel;
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