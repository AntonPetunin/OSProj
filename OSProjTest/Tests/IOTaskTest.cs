using OSProj.Generator;
using OSProj.TaskProcessor.ThreadExecutors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSProjTest.Tests
{
  public class IOTaskTest
  {
    [Fact]
    public void BaseOSTask_PropTest()
    {
      int id = 1;
      int priority = 3;
      uint loopCount = 1;
      var taskFunc = () => { id++; };

      var baseTask = new BaseOSTask(id, priority, taskFunc, loopCount);

      Assert.Equal(id, baseTask.Id);
      Assert.Equal(priority, baseTask.Priority);
      Assert.Equal(TaskType.Base, baseTask.TaskType);
      baseTask.Run();
      Thread.Sleep(200);
      Assert.Equal(baseTask.Id + 1, id);
    }


    [Fact]
    public void ExtendedOSTask_PropTest()
    {
      int id = 1;
      int priority = 3;
      uint loopCount = 1;
      var taskFunc = () => { id++; };

      var baseTask = new ExtendedOSTask(id, priority, taskFunc, loopCount);

      Assert.Equal(id, baseTask.Id);
      Assert.Equal(priority, baseTask.Priority);
      Assert.Equal(TaskType.Extended, baseTask.TaskType);
      baseTask.Run();
      Thread.Sleep(200);
      Assert.Equal(baseTask.Id + 1, id);
    }
  }
}
