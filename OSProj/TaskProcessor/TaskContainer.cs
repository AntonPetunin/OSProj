using OSProj.TaskProcessor.ThreadExecutors;

namespace OSProj.TaskProcessor
{
  public class TaskContainer
  {
    public int MaxMainContainerSize { get; set; } = 8;

    private IOSTask? _activeTask;
    private TaskQueue _mainTasksCollection = new();
    private TaskQueue _priority0Tasks = new();
    private TaskQueue _priority1Tasks = new();
    private TaskQueue _priority2Tasks = new();
    private TaskQueue _priority3Tasks = new();

    private TaskQueue _waitingCollection = new();
    private TaskQueue _suspendedCollection = new();

    public delegate void UpdateQueuesInfo(IOSTask? activeTask, TaskQueue mainTasks, TaskQueue waitingTasks, TaskQueue suspendedTasks);
    private UpdateQueuesInfo? _updateDelegate;

    public TaskContainer()
    {
    }

    public bool IsMainContainerFull() { return _mainTasksCollection.Count >= MaxMainContainerSize; }

    public void SetUpdateQueuesInfo(UpdateQueuesInfo updateDelegate)
    {
      _updateDelegate = updateDelegate;
      UpdateCollectionView();
    }

    public void UpdateCollectionView()
    {
      if (_updateDelegate != null)
      {
        lock (_mainTasksCollection) lock (_waitingCollection) lock (_suspendedCollection)
            {
              _updateDelegate(_activeTask, _mainTasksCollection, _waitingCollection, _suspendedCollection);
            }
      }
    }

    public int? GetNextTaskPriority()
    {
      lock (_mainTasksCollection)
      {
        return _mainTasksCollection.Next?.Priority;
      }
    }

    public IOSTask? GetTopTaskAsActive()
    {
      _activeTask = PopMainTask();
      return _activeTask;
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
      {
        _mainTasksCollection.Push(task);
        UpdateCollectionView();
      }

      return result;
    }

    public bool AddTaskToWaiting(IOSTask task)
    {
      bool res = task.TaskType == Generator.TaskType.Extended && task.TaskStatus == Generator.OSTaskStatus.Running;

      if (res)
      {
        lock (_waitingCollection)
        {
          _waitingCollection.Push(task);
          ((IExtendedTasksStateSetter)task)?.SetWaitingState();
          UpdateCollectionView();
        }
      }

      return res;
    }

    public bool AddTaskToSuspended(IOSTask task)
    {
      bool res = task.TaskStatus == Generator.OSTaskStatus.Running;

      if (res)
      {
        lock (_suspendedCollection)
        {
          task.SetSuspendedState();
          _suspendedCollection.Push(task);
          UpdateCollectionView();
        }
      }

      return res;
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

      if (!isAllEmpty)
        UpdateCollectionView();
    }

    public void FillMainContainerFromWaiting()
    {
      int emptyPlacesCount = MaxMainContainerSize - _mainTasksCollection.Count;

      if (emptyPlacesCount > 0)
      {
        emptyPlacesCount = _waitingCollection.Count < emptyPlacesCount ? _waitingCollection.Count : emptyPlacesCount;

        for (int i = 0; i < emptyPlacesCount; i++)
        {
          IOSTask? task = _waitingCollection.Pop();

          if (task != null)
          {
            lock (_mainTasksCollection)
            {
              _mainTasksCollection.Push(task);
              ((IExtendedTasksStateSetter)task).SetReadyFromWaiting();
              UpdateCollectionView();
            }
          }
        }
      }
    }

    public void FillMainContainerFromSuspended()
    {
      int emptyPlacesCount = MaxMainContainerSize - _mainTasksCollection.Count;

      if (emptyPlacesCount > 0)
      {
        emptyPlacesCount = _suspendedCollection.Count < emptyPlacesCount ? _suspendedCollection.Count : emptyPlacesCount;

        for (int i = 0; i < emptyPlacesCount; i++)
        {
          IOSTask? task = _suspendedCollection.Pop();

          if (task != null)
          {
            lock (_mainTasksCollection)
            {
              _mainTasksCollection.Push(task);
              task.SetReadyFromSuspended();
              UpdateCollectionView();
            }
          }
        }
      }
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