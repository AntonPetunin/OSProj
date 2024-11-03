

namespace OSProj.Generator
{
    public class TaskGenerator
    {
        private List<OSTask> _tasks = new();
        private Tuple<int, int> _durationFork = Tuple.Create(2, 6);

        public int GenerationCount { get; set; } = 8; // default

        public void Generate()
        {
            Random rnd = new Random();

            for (int i = 0; i < GenerationCount; i++)
            {
                Action task = () =>
                {
                    int duration = rnd.Next(_durationFork.Item1, _durationFork.Item2) * 1000;
                    Thread.Sleep(rnd.Next(duration));
                };

                OSTask exTask = new OSTask(IdGenerator.Id, rnd.Next(0, 4), TaskType.Base/*CreateTaskType(ref rnd)*/, task);
                _tasks.Add(exTask);
            }
        }

        public List<OSTask> PopGenerated()
        {
            var res = new List<OSTask>(_tasks);
            _tasks.Clear();
            return res;
        }

        private TaskType CreateTaskType(ref Random random)
        {
            double rndNum = random.Next(0, 101);
            return (TaskType)Math.Round(rndNum / 100);
        }
    }
}