using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.ComponentModel;

namespace RichardSzalay.HostsFileExtension
{
    public interface IBulkHostResolver
    {
        IAsyncResult BeginGetHostEntries(IEnumerable<string> hosts, ISynchronizeInvoke synchronizingObject, TimeSpan timeout, AsyncCallback asyncCallback, object stateObject);
        IPHostEntry[] EndGetHostEntries(IAsyncResult result);
    }
}
