using OSProj.Generator;

namespace OSProj.TaskProcessor
{
  public class TaskQueue
  {
    private LinkedList<OSTask> _queue = new();
    public bool IsSorted { get; }
    public int Count => _queue.Count;
    public OSTask? Next => _queue.Count > 0 ? _queue.First() : null;
    public bool Empty => _queue.Count == 0;


    public TaskQueue(bool isSorted = true)
    {
      IsSorted = isSorted;
    }

    public IEnumerable<OSTask> Get()
    {
      return _queue;
    }

    public void Push(OSTask? item)
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

        if (element != null)
        {
          while (element.Next != null)
          {
            if (element.Value.Priority < item.Priority)
            {
              _queue.AddBefore(element, item);
              break;
            }

            element = element.Next;
          }
        }
      }
    }

    public OSTask? Pop()
    {
      OSTask? res = _queue.First?.Value;

      if (res != null)
        _queue.RemoveFirst();

      return res;
    }

  }
}