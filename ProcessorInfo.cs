using OSProj.Generator;
using OSProj.TaskProcessor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace OSProj
{
  public class ProcessorInfo
  {
    private OSTaskProcessor _processor;
    private TaskQueue _mainTasksCollection = new();
    private TaskQueue _waitingCollection = new();
    private TaskQueue _suspendedCollection = new();
    private Dispatcher _dispatcher;

    //public TaskQueue? MainTasks { get { return _mainTasksCollection; } }
    //public TaskQueue? WaitingTask { get { return _waitingCollection; } }
    //public TaskQueue? SuspendedTasks { get { return _suspendedCollection; } }

    public ObservableCollection<OSTask> Tasks { get; } = new ObservableCollection<OSTask>();

    public ProcessorInfo(OSTaskProcessor processor, Dispatcher dispatcher)
    {
      _dispatcher = dispatcher;
      _processor = processor;
      _processor.SetUpdateProcessorInfoDelegate(UpdateQueuesInfo);
    }

    private void UpdateQueuesInfo(TaskQueue mainTasks, TaskQueue waitingTasks, TaskQueue suspendedTasks)
    {
      
      lock (_waitingCollection) lock (_suspendedCollection) lock (_mainTasksCollection)
          {
            _mainTasksCollection = mainTasks;
            _waitingCollection = waitingTasks;
            _suspendedCollection = suspendedTasks;
            Update();
          }
    }

    private void Update()
    {
      _dispatcher.Invoke(() =>
      {
        lock (Tasks)
        {
          ObservableCollection<OSTask> refreshTasks = [.. _waitingCollection.Get(), .. _suspendedCollection.Get(), .. _mainTasksCollection.Get()];
          Tasks.Clear();

          foreach (var task in refreshTasks)
            Tasks.Add(task);
        }
      });
    }
  }
}
