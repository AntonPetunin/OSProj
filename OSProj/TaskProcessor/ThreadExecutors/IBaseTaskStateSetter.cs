using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
