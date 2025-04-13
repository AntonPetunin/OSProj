using NLog;
using OSProj.Generator;
using OSProj.TaskProcessor;

namespace OSProjTest.Tests
{
  public class ProcessorTests
  {
    private int _terminatedTaskId = 0;
    private int _waitingTaskId = 0;
    bool _canActivate = false;
    bool _canRelease = false;

    private readonly ManualResetEvent _waitingEvent = new(false);

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
    public void OSTaskProcessor_TerminateExtendedTaskExecution()
    {
      OSTaskProcessor processor = new(LogManager.GetCurrentClassLogger());
      processor.SetGenerator(new TaskGenerator { GenerationCount = 100 });
      processor.Generate();
      processor.Start();

      TaskContainer.UpdateQueuesInfo updateInfo = (activeTask, mainTasks, waitingTasks, suspendedTasks) =>
      {
        Assert.NotNull(mainTasks.Get());
        Assert.NotNull(waitingTasks.Get());
        Assert.NotNull(suspendedTasks.Get());

        if (activeTask != null && activeTask.TaskType == TaskType.Extended)
        {
          if (_terminatedTaskId == 0)
          {
            _terminatedTaskId = activeTask.Id;
            processor.TerminateActiveTask();
          }
          else if (!suspendedTasks.Empty && !_canActivate)
          {
            Assert.Equal(activeTask.Id, _terminatedTaskId);
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
    public void OSTaskProcessor_TerminateBaseTaskExecution()
    {
      OSTaskProcessor processor = new(LogManager.GetCurrentClassLogger());
      processor.SetGenerator(new TaskGenerator { GenerationCount = 100 });
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
          else if (!suspendedTasks.Empty && !_canActivate)
          {
            Assert.Equal(activeTask.Id, _terminatedTaskId);
            Assert.Equal(_terminatedTaskId, suspendedTasks.Next?.Id);

            _canActivate = true;
            processor.Activate();
          }
          else if (_canActivate && suspendedTasks.Empty)
          {
            Assert.Empty(suspendedTasks.Get());
            processor.Activate();
            _canActivate = false;
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
    public void OSTaskProcessor_WaitExtendedTaskExecution()
    {
      OSTaskProcessor processor = new(LogManager.GetCurrentClassLogger());
      processor.SetGenerator(new TaskGenerator { GenerationCount = 100 });
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
            _waitingTaskId = activeTask.Id;
            processor.PauseActiveTask();

          }
          else if (!waitingTasks.Empty && !_canRelease)
          {
            Assert.Equal(_waitingTaskId, activeTask.Id);
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
