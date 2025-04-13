namespace OSProj.TaskProcessor.ThreadExecutors
{
  public interface IExtendedTasksStateSetter
  {
    public void SetWaitingState();
    public void SetReadyFromWaiting();
  }
}
