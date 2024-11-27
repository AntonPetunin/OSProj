using OSProj.TaskProcessor;
using OSProj.TaskProcessor.ThreadExecutors;
using OSProj.Generator;
using Xunit;
using NLog;

namespace OSProjTest.Tests
{
    public class ProcessorTest
    {
        private int _terminatedTaskId = 0;
        private int _waitingTaskId = 0;
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
        public void OSTaskProcessor_TerminateExcution()
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

                if (activeTask != null && _terminatedTaskId == 0)
                {
                    Assert.Equal(4, mainTasks.Count);
                    _terminatedTaskId = activeTask.Id;
                    processor.TerminateActiveTask();
                }

                if (activeTask != null && _terminatedTaskId != 0 && activeTask.Id != _terminatedTaskId)
                {
                    Assert.Equal(3, mainTasks.Count);
                    Assert.NotEqual(activeTask.Id, _terminatedTaskId);
                    Assert.NotEmpty(suspendedTasks.Get());
                    Assert.Equal(_terminatedTaskId, suspendedTasks.Next?.Id);

                    processor.Stop();
                    _waitingEvent.Set();
                }
            };

            processor.SetUpdateProcessorInfoDelegate(updateInfo);
            _waitingEvent.Reset();
            _waitingEvent.WaitOne();
        }

        [Fact]
        public void OSTaskProcessor_WaitExcution()
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

                if (activeTask != null && _waitingTaskId == 0)
                {
                    if (activeTask.TaskType == TaskType.Extended)
                    {
                        _waitingTaskId = activeTask.Id;
                        processor.PauseActiveTask();
                    }
                }
                else if (activeTask != null && _waitingTaskId != 0 && activeTask.TaskType == TaskType.Extended)
                {
                    Assert.Equal(_waitingTaskId, activeTask.Id);
                    Assert.NotEmpty(waitingTasks.Get());
                    processor.Stop();
                    _waitingEvent.Set();
                }
            };

            processor.SetUpdateProcessorInfoDelegate(updateInfo);
            _waitingEvent.Reset();
            _waitingEvent.WaitOne();
        }
    }
}
