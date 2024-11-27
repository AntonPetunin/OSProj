﻿using OSProj.Generator;
using OSProj.TaskProcessor.ThreadExecutors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSProjTest.Tests
{
  public class IOTaskTests
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

      var action = () => { Thread.Sleep(5000); };
      var baseTask1 = new BaseOSTask(id, priority, action);

      baseTask1.Run();
      baseTask1.Cancel();
      baseTask1.Dispose();

      Assert.False(baseTask1.IsRunning);
    }


    [Fact]
    public void ExtendedOSTask_PropTest()
    {
      int id = 1;
      int priority = 3;
      uint loopCount = 1;
      var taskFunc = () => { id++; };

      var extendedTask = new ExtendedOSTask(id, priority, taskFunc, loopCount);

      Assert.Equal(id, extendedTask.Id);
      Assert.Equal(priority, extendedTask.Priority);
      Assert.Equal(TaskType.Extended, extendedTask.TaskType);
      extendedTask.Run();
      Thread.Sleep(200);
      Assert.Equal(extendedTask.Id + 1, id);

      var action1 = () => { Thread.Sleep(5000); };
      var extendedTask1 = new ExtendedOSTask(id + 1, priority, action1);

      extendedTask1.Run();
      extendedTask1.Pause();
      Assert.True(extendedTask1.Paused);
      extendedTask1.Dispose();

      var action2 = () => { Thread.Sleep(5000); };
      var extendedTask2 = new ExtendedOSTask(id + 2, priority, action2);
      extendedTask2.Run();
      extendedTask2.Cancel();
      extendedTask2.Dispose();
      Assert.False(extendedTask2.IsRunning);
    }
  }
}