using OSProj.TaskProcessor;
using OSProj.TaskProcessor.ThreadExecutors;
using OSProj.Generator;
using Xunit;
using NLog;

namespace OSProjTest.Tests
{
  public class ProcessorTests
  {
    private int _terminatedTaskId = 0;
    private int _waitingTaskId = 0;
    bool _canActivate = false;
    bool _canRelease = false;

    private ManualResetEvent _waitingEvent = new(false);

    [Fact]
    public void OSTaskProcessor_StartedStopProcessor()
    {
      OSTaskProcessor processor = new(LogManager.GetCurrentClassLogger());
      processor.Start();
      Assert.True(processor.Running);

      processor.Stop();
      Assert.False(processor.Running);
    }

    [Fact]
    public void OSTaskProcessor_TerminateExtendedTaskExcution()
    {
      OSTaskProcessor processor = new(LogManager.GetCurrentClassLogger());
      processor.SetGenerator(new TaskGenerator { GenerationCount = 5 });
      processor.Generate();
      processor.Start();

      TaskContainer.UpdateQueuesInfo updateInfo = (activeTask, mainTasks, waitingTasks, suspendedTasks) =>
      {
        Assert.NotNull(mainTasks.Get());
        Assert.NotNull(waitingTasks.Get());
        Assert.NotNull(suspendedTasks.Get());

        if (activeTask != null && activeTask.TaskType == TaskType.Extended)
        {
          if (activeTask != null && _terminatedTaskId == 0)
          {
            _terminatedTaskId = activeTask.Id;
            processor.TerminateActiveTask();
          }
          else if (activeTask != null && _terminatedTaskId != 0 && activeTask.Id != _terminatedTaskId && !_canActivate)
          {
            Assert.NotEqual(activeTask.Id, _terminatedTaskId);
            Assert.NotEmpty(suspendedTasks.Get());
            Assert.Equal(_terminatedTaskId, suspendedTasks.Next?.Id);

            _canActivate = true;
            processor.Activate();
          }
          else if (_canActivate)
          {
            Assert.Empty(suspendedTasks.Get());

            processor.Stop();
            _waitingEvent.Set();
          }
        }
      };

      processor.SetUpdateProcessorInfoDelegate(updateInfo);
      _waitingEvent.Reset();
      _waitingEvent.WaitOne();
      Clear();
    }

    [Fact]
    public void OSTaskProcessor_TerminateBaseTaskExcution()
    {
      OSTaskProcessor processor = new(LogManager.GetCurrentClassLogger());
      processor.SetGenerator(new TaskGenerator { GenerationCount = 5 });
      processor.Generate();
      processor.Start();

      TaskContainer.UpdateQueuesInfo updateInfo = (activeTask, mainTasks, waitingTasks, suspendedTasks) =>
      {
        Assert.NotNull(mainTasks.Get());
        Assert.NotNull(waitingTasks.Get());
        Assert.NotNull(suspendedTasks.Get());

        if (activeTask != null && activeTask.TaskType == TaskType.Base)
        {
          if (_terminatedTaskId == 0)
          {
            _terminatedTaskId = activeTask.Id;
            processor.TerminateActiveTask();
          }
          else if (_terminatedTaskId != 0 && activeTask.Id != _terminatedTaskId && !_canActivate)
          {
            Assert.NotEqual(activeTask.Id, _terminatedTaskId);
            Assert.NotEmpty(suspendedTasks.Get());
            Assert.Equal(_terminatedTaskId, suspendedTasks.Next?.Id);

            _canActivate = true;
            processor.Activate();
          }
          else if (_canActivate)
          {
            Assert.Empty(suspendedTasks.Get());

            processor.Stop();
            _waitingEvent.Set();
          }
        }
      };

      processor.SetUpdateProcessorInfoDelegate(updateInfo);
      _waitingEvent.Reset();
      _waitingEvent.WaitOne();
      Clear();
    }

    [Fact]
    public void OSTaskProcessor_WaitExtendedTaskExcution()
    {
      OSTaskProcessor processor = new(LogManager.GetCurrentClassLogger());
      processor.SetGenerator(new TaskGenerator { GenerationCount = 10 });
      processor.Generate();
      processor.Start();

      TaskContainer.UpdateQueuesInfo updateInfo = (activeTask, mainTasks, waitingTasks, suspendedTasks) =>
      {
        Assert.NotNull(mainTasks.Get());
        Assert.NotNull(waitingTasks.Get());
        Assert.NotNull(suspendedTasks.Get());

        if (activeTask != null && activeTask.TaskType == TaskType.Extended)
        {
          if (_waitingTaskId == 0)
          {
            if (activeTask.TaskType == TaskType.Extended)
            {
              _waitingTaskId = activeTask.Id;
              processor.PauseActiveTask();
            }
          }
          else if (_waitingTaskId != 0 && activeTask.TaskType == TaskType.Extended && !_canRelease)
          {
            Assert.Equal(_waitingTaskId, activeTask.Id);
            Assert.NotEmpty(waitingTasks.Get());

            _canRelease = true;
            processor.Release();
          }
          else if (_canRelease)
          {
            Assert.Empty(waitingTasks.Get());

            processor.Stop();
            _waitingEvent.Set();
          }
        }
      };

      processor.SetUpdateProcessorInfoDelegate(updateInfo);
      _waitingEvent.Reset();
      _waitingEvent.WaitOne();
      Clear();
    }

    private void Clear()
    {
      _waitingTaskId = 0;
      _canRelease = false;
      _terminatedTaskId = 0;
      _canActivate = false;
    }
  }
}
