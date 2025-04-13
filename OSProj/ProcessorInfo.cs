using NLog;
using NLog.Config;
using OSProj.TaskProcessor;
using OSProj.TaskProcessor.ThreadExecutors;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Threading;

namespace OSProj
{
  public class ProcessorInfo
  {
    public static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private OSTaskProcessor _processor;
    private IOSTask? _activeTask = null;
    private TaskQueue _mainTasksCollection = new();
    private TaskQueue _waitingCollection = new();
    private TaskQueue _suspendedCollection = new();
    private readonly Dispatcher _dispatcher;

    private readonly ProgressBar _progressBar;
    private readonly Button _pauseButton;

    public ObservableCollection<IOSTask> Tasks { get; } = new();


    public ProcessorInfo(OSTaskProcessor processor, Button pauseButton, ProgressBar progressBar, Dispatcher dispatcher)
    {
      _dispatcher = dispatcher;
      _progressBar = progressBar;
      _processor = processor;
      _pauseButton = pauseButton;
      _processor.SetUpdateProcessorInfoDelegate(UpdateQueuesInfo);
      _processor.SetUpdateProgressBarDelegate(UpdateActiveTaskProgress);
      ConfigureLogging();
    }

    private void UpdateQueuesInfo(IOSTask? activeTask, TaskQueue mainTasks, TaskQueue waitingTasks, TaskQueue suspendedTasks)
    {

      //lock (_waitingCollection) lock (_suspendedCollection) lock (_mainTasksCollection)
      //{
      _activeTask = activeTask;
      _mainTasksCollection = mainTasks;
      _waitingCollection = waitingTasks;
      _suspendedCollection = suspendedTasks;
      Update();
      //}
    }

    private void UpdateActiveTaskProgress(double percent)
    {
      _dispatcher.Invoke(() =>
      {
        lock (_progressBar)
        {
          _progressBar.Value = percent * 100;
        }
      });
    }

    private void Update()
    {
      _dispatcher.Invoke(() =>
      {
        lock (Tasks)
        {
          ObservableCollection<IOSTask> refreshTasks = [.. _waitingCollection.Get(), .. _suspendedCollection.Get(), .. _mainTasksCollection.Get()];
          Tasks.Clear();

          if (_activeTask != null)
          {
            Tasks.Add(_activeTask);
            _pauseButton.IsEnabled = _activeTask.TaskType == Generator.TaskType.Extended;
          }

          foreach (var task in refreshTasks)
            Tasks.Add(task);

          _ = Tasks.GroupBy(element => element.TaskType);
        }
      });
    }

    public static void ConfigureLogging()
    {
      var textBoxTarget = new TextBoxTarget
      {
        Layout = "${level:uppercase=true} ${message} ${exception:format=toString} ${longdate}\n\n"
      };

      var config = new LoggingConfiguration();
      config.AddRule(LogLevel.Info, LogLevel.Fatal, textBoxTarget);
      LogManager.Configuration = config;
    }
  }
}
