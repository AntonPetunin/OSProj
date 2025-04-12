using OSProj.TaskProcessor.ThreadExecutors;

namespace OSProj.TaskProcessor
{
  public class TaskQueue
  {
    private LinkedList<IOSTask> _queue = new();
    public bool IsSorted { get; }
    public int Count => _queue.Count;
    public IOSTask? Next => _queue.Count > 0 ? _queue.First() : null;
    public bool Empty => _queue.Count == 0;


    public TaskQueue(bool isSorted = true)
    {
      IsSorted = isSorted;
    }

    public IEnumerable<IOSTask> Get()
    {
      return _queue;
    }

    public void Push(IOSTask? item)
    {
      if (item == null)
        return;

      if (!IsSorted || _queue.Count == 0 || _queue.Select(task => task.Priority).Min() >= item.Priority)
      {
        _queue.AddLast(item);
      }
      else
      {
        var element = _queue.First;
        var added = false;

        while (element != null)
        {
          if (CompareTasks(element.Value, item))
          {
            _queue.AddBefore(element, item);
            added = true;
            break;
          }

          element = element.Next;
        }

        if (!added)
          _queue.AddLast(item);
      }
    }

    public IOSTask? Pop()
    {
      IOSTask? res = _queue.First?.Value;

      if (res != null)
        _queue.RemoveFirst();

      return res;
    }


    private bool CompareTasks(IOSTask task1, IOSTask task2)
    {
      return task1.Priority < task2.Priority || task1.Priority == task2.Priority && task1.CreationTime < task2.CreationTime;
    }
  }
}