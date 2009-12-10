using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RichardSzalay.HostsFileExtension.Threading
{
    public class ThreadPoolTaskManager : ITaskManager
    {
        public void QueueTask(Action action)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(s => action()));
        }
    }
}
