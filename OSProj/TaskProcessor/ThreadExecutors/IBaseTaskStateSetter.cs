namespace OSProj.TaskProcessor.ThreadExecutors
{
  public interface IBaseTaskStateSetter
  {
    public void SetSuspendedState();
    public void SetReadyFromSuspended();
    public void SetReadyFromRunning();
    public void SetRunningState();
  }
}
