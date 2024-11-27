using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSProj.TaskProcessor.ThreadExecutors
{
  public interface IExtendedTasksStateSetter
  {
    public void SetWaitingState();
    public void SetReadyFromWaiting();
  }
}
