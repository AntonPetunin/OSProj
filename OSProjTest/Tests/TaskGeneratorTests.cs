using OSProj.Generator;
using OSProj.TaskProcessor;
using OSProj.TaskProcessor.ThreadExecutors;
using Xunit;

namespace OSProjTest.Tests
{
  public class TaskGeneratorTests
  {
    [Fact]
    public void TaskGenerator_ShouldGenerateSpecifiedNumberOfTasks()
    {
      var generator = new TaskGenerator { GenerationCount = 5 };
      generator.Generate();

      var tasks = generator.PopGenerated();

      Assert.Equal(5, tasks.Count);
    }

    [Fact]
    public void TaskGenerator_ShouldGenerateBothBaseAndExtendedTasks()
    {
      var generator = new TaskGenerator { GenerationCount = 50 };
      generator.Generate();

      var tasks = generator.PopGenerated();

      Assert.Contains(tasks, task => task.GetType() == typeof(BaseOSTask));
      Assert.Contains(tasks, task => task.GetType() == typeof(ExtendedOSTask));
    }
  }
}
