using OSProj.Generator;

namespace OSProj.TaskProcessor.ThreadExecutors
{
  public interface IOSTask : IBaseTaskStateSetter
  {
    public int Id { get; }
    public int Priority { get; }
    public TaskType TaskType { get; }
    public OSTaskStatus TaskStatus { get; set; }
    public DateTime CreationTime { get; }

    public void Run();
    public void Cancel();
    public void Wait();
    public void Dispose();
    void SetUpdateProgressBarDelegate(OSTaskProcessor.UpdateProgressBarDelegate updateProgressBar);
  }
}
