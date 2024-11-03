using OSProj.TaskProcessor.ThreadExecutors;

namespace OSProj.TaskProcessor
{
    public class TaskContainer
  {
    public int MaxMainContainerSize { get; set; } = 8;

    private TaskQueue _mainTasksCollection = new();
    private TaskQueue _priority0Tasks = new();
    private TaskQueue _priority1Tasks = new();
    private TaskQueue _priority2Tasks = new();
    private TaskQueue _priority3Tasks = new();

    private TaskQueue _waitingCollection = new();
    private TaskQueue _suspendedCollection = new();

    public delegate void UpdateQueuesInfo(TaskQueue mainTasks, TaskQueue waitingTasks, TaskQueue suspendedTasks);
    private UpdateQueuesInfo? _updateDelegate;

    public TaskContainer()
    {
    }

    public bool IsMainContainerFull() { return _mainTasksCollection.Count >= MaxMainContainerSize; }

    public void SetUpdateQueuesInfo(UpdateQueuesInfo updateDelegate)
    {
      _updateDelegate = updateDelegate;
      UpdateSubscriber();
    }

    public void UpdateSubscriber()
    {
      if (_updateDelegate != null)
        _updateDelegate(_mainTasksCollection, _waitingCollection, _suspendedCollection);
    }

    public int? GetNextTaskPriority()
    {
      return _mainTasksCollection.Next?.Priority;
    }

    public IOSTask? PopMainTask()
    {
      lock (_mainTasksCollection)
      {
        return _mainTasksCollection.Pop();
      }
    }

    public bool AddTaskToMain(IOSTask task)
    {
      bool result = _mainTasksCollection.Count < MaxMainContainerSize;

      if (result)
        _mainTasksCollection.Push(task);

      return result;
    }

    public void AddTask(IOSTask task)
    {
      switch (task.Priority)
      {
        case 0:
          _priority0Tasks.Push(task);
          break;
        case 1:
          _priority1Tasks.Push(task);
          break;
        case 2:
          _priority2Tasks.Push(task);
          break;
        case 3:
          _priority3Tasks.Push(task);
          break;
      }
    }

    public void AddTasks(List<IOSTask> tasks)
    {
      foreach (IOSTask task in tasks)
        AddTask(task);

      FillMainContainerFromSource();
    }


    public void FillMainContainerFromSource()
    {
      bool isAllEmpty = false;

      while (!IsMainContainerFull() && !isAllEmpty)
      {
        if (!_priority3Tasks.Empty)
        {
          _mainTasksCollection.Push(_priority3Tasks.Pop());
        }
        else if (!_priority2Tasks.Empty)
        {
          _mainTasksCollection.Push(_priority2Tasks.Pop());
        }
        else if (!_priority1Tasks.Empty)
        {
          _mainTasksCollection.Push(_priority1Tasks.Pop());
        }
        else if (!_priority0Tasks.Empty)
        {
          _mainTasksCollection.Push(_priority0Tasks.Pop());
        }
        else
        {
          isAllEmpty = true;
        }
      }
    }

    public void FillMainContainerFromWaiting()
    {

    }

    public int GetMaxWaitingPriority()
    {
      int res = -1;

      if (_waitingCollection.Count > 0)
        res = _waitingCollection.Get().Select(x => x.Priority).Max();

      return res;
    }

    public int GetMaxSourceTasksPriority()
    {
      int res = -1;

      if (!_priority3Tasks.Empty)
        res = 3;
      else if (!_priority2Tasks.Empty)
        res = 2;
      else if (!_priority1Tasks.Empty)
        res = 1;
      else if (!_priority0Tasks.Empty)
        res = 0;

      return res;
    }


    private IOSTask? PopWaitingTask()
    {
      return _waitingCollection.Pop();
    }

    private IOSTask? PopSuspendedTask()
    {
      return _suspendedCollection.Pop();
    }
  }
}