using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSProj.TaskProcessor.ThreadExecutors
{
  public interface ITaskStateSetter
  {
    public void SetWaitingState();
    public void SetActivatedState();
    public void SetReadyFromSuspended();
    public void SetReadyFromWaiting();
    public void SetReadyFromRunning();
    public void SetRunningState();
  }
}
