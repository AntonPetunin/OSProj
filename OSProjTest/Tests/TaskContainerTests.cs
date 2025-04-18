﻿using OSProj.Generator;
using OSProj.TaskProcessor;
using OSProj.TaskProcessor.ThreadExecutors;

namespace OSProjTest.Tests
{
  public class TaskContainerTests
  {
    [Fact]
    public void TaskContainer_UnsortedQueueTest()
    {
      int id = 1;
      int priority = 0;
      uint loopCount = 1;
      Action action = () => Thread.Sleep(5000);

      BaseOSTask baseTask = new(id, priority, action, loopCount);

      id = 2;
      priority = 2;
      loopCount = 1;

      ExtendedOSTask extendedTask = new(id, priority, action, loopCount);

      TaskQueue queue = new();
      queue.Push(baseTask);
      queue.Push(extendedTask);

      Assert.True(queue.Next?.Priority == queue.Get().Max(task => task.Priority));
    }

    [Fact]
    public void TaskContainer_PushSortedQueue()
    {
      TaskContainer container = new();

      IOSTask? task = GetTaskWithPriority(3);

      if (task != null)
        container.AddTaskToMain(task);

      task = GetTaskWithPriority(2);

      if (task != null)
        container.AddTaskToMain(task);

      task = GetTaskWithPriority(1);

      if (task != null)
        container.AddTaskToMain(task);

      task = GetTaskWithPriority(0);

      if (task != null)
        container.AddTaskToMain(task);

      Assert.Equal(3, container.GetNextTaskPriority());
    }

    [Fact]
    public void TaskContainer_FillMainTask()
    {
      TaskContainer container = new();
      TaskGenerator generator = new() { GenerationCount = container.MaxMainContainerSize };
      container.AddTasks(generator.PopGenerated());

      IOSTask? task = GetTaskWithPriority(0);

      if (task != null)
      {
        container.AddTask(task);
        Assert.Equal(0, container.GetMaxSourceTasksPriority());
      }

      task = GetTaskWithPriority(1);

      if (task != null)
      {
        container.AddTask(task);
        Assert.Equal(1, container.GetMaxSourceTasksPriority());
      }

      task = GetTaskWithPriority(2);

      if (task != null)
      {
        container.AddTask(task);
        Assert.Equal(2, container.GetMaxSourceTasksPriority());
      }

      task = GetTaskWithPriority(3);

      if (task != null)
      {
        container.AddTask(task);
        Assert.Equal(3, container.GetMaxSourceTasksPriority());
      }
    }

    private static IOSTask? GetTaskWithPriority(int priority)
    {
      TaskGenerator generator = new() { GenerationCount = 1 };
      IOSTask? task = null;

      if (priority <= 3)
      {
        while (task == null || task != null && task.Priority != priority)
        {
          generator.Generate();
          task = generator.PopGenerated()[0];
        }
      }

      return task;
    }
  }
}
