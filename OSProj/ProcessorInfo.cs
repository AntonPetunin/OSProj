using NLog.Config;
using NLog;
using OSProj.TaskProcessor;
using OSProj.TaskProcessor.ThreadExecutors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace OSProj.View
{
  public class ProcessorInfo
  {
    public static readonly Logger logger = LogManager.GetCurrentClassLogger();

    private OSTaskProcessor _processor;
    private IOSTask? _activeTask = null;
    private TaskQueue _mainTasksCollection = new();
    private TaskQueue _waitingCollection = new();
    private TaskQueue _suspendedCollection = new();
    private Dispatcher _dispatcher;

    private ProgressBar _progressBar;

    //public TaskQueue? MainTasks { get { return _mainTasksCollection; } }
    //public TaskQueue? WaitingTask { get { return _waitingCollection; } }
    //public TaskQueue? SuspendedTasks { get { return _suspendedCollection; } }

    public ObservableCollection<IOSTask> Tasks { get; } = new();


    public ProcessorInfo(OSTaskProcessor processor, ProgressBar progressBar, Dispatcher dispatcher)
    {
      _dispatcher = dispatcher;
      _progressBar = progressBar;
      _processor = processor;
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
            Tasks.Add(_activeTask);

          foreach (var task in refreshTasks)
            Tasks.Add(task);
          Tasks.GroupBy(element => element.TaskType);
        }
      });
    }

    private void ConfigureLogging()
    {
      var config = new LoggingConfiguration();
      var textBoxTarget = new TextBoxTarget();
      textBoxTarget.Layout = "${level:uppercase=true} ${message} ${exception:format=toString} ${longdate}\n\n";
      config.AddRule(LogLevel.Info, LogLevel.Fatal, textBoxTarget);
      LogManager.Configuration = config;
    }
  }
}
