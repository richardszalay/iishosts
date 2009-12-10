using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RichardSzalay.HostsFileExtension.Threading
{
    public interface ITaskManager
    {
        void QueueTask(Action action);
    }
}
