using OSProj.TaskProcessor.ThreadExecutors;

namespace OSProj.Generator
{
  public class TaskGenerator
  {
    private List<IOSTask> _tasks = new();
    private Tuple<int, int> _durationFork = Tuple.Create(3, 7);

    public int GenerationCount { get; set; } = 8;

    public void Generate()
    {
      Random rnd = new Random();
      for (int i = 0; i < GenerationCount; i++)
      {
        TaskType taskType = CreateTaskType(rnd);
        IOSTask? task = null;

        if (taskType == TaskType.Base)
        {
          Action action = () =>
          {
            int duration = rnd.Next(_durationFork.Item1, _durationFork.Item2) * 1000;
            Thread.Sleep(rnd.Next(duration));
          };

          task = new BaseOSTask(IdGenerator.Id, rnd.Next(0, 4), taskType, action);
        }
        else if (taskType == TaskType.Extended)
        {
          Action action = () => Thread.Sleep(1000);
          uint duration = (uint)rnd.Next(_durationFork.Item1, _durationFork.Item2);
          task = new ExtendedOSTask(IdGenerator.Id, rnd.Next(0, 4), taskType, action, duration);
        }

        if (task != null)
          _tasks.Add(task);
      }
    }

    public List<IOSTask> PopGenerated()
    {
      var res = new List<IOSTask>(_tasks);
      _tasks.Clear();
      return res;
    }

    private TaskType CreateTaskType(Random random)
    {
      double rndNum = random.Next(0, 101);
      return (TaskType)Math.Round(rndNum / 100);
    }
  }
}