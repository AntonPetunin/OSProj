using OSProj.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSProj.TaskProcessor.ThreadExecutors
{
  public class ExtendedOSTask : LoopExecutor, IOSTask
  {
    public int Id { get; }
    public int Priority { get; }
    public TaskType TaskType { get; }
    public OSTaskStatus TaskStatus { get; set; } = OSTaskStatus.Created;

    public ExtendedOSTask(int id, int priority, TaskType taskType, Action threadFunc, uint loopingCount = 0) : base(threadFunc, loopingCount)
    {
      Id = id;
      Priority = priority;
      TaskType = taskType;
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
  }
}
