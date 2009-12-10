using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Timer = System.Timers.Timer;
using System.ComponentModel;
using System.Threading;
using System.Net.Sockets;

namespace RichardSzalay.HostsFileExtension
{
    public class DnsBulkHostResolver : IBulkHostResolver
    {
        public DnsBulkHostResolver()
        {
        }

        public IAsyncResult BeginGetHostEntries(IEnumerable<string> hosts, ISynchronizeInvoke synchronizingObject, TimeSpan timeout, AsyncCallback asyncCallback, object stateObject)
        {
            AsyncCallback threadSafeAsyncCallback = (synchronizingObject == null)
                ? asyncCallback
                : new AsyncCallback(
                    c => synchronizingObject.Invoke(asyncCallback, new object[] { c })
                    );

            Timer timer = new Timer(timeout.TotalMilliseconds);
            timer.SynchronizingObject = synchronizingObject;

            ResolverAsyncResult result = new ResolverAsyncResult(hosts.ToList(), threadSafeAsyncCallback, timer);

            timer.Elapsed += (s, e) =>
                {
                    timer.Stop();
                    result.MarkComplete();
                };
            timer.Start();

            foreach (string host in hosts)
            {
                IAsyncResult getHostEntryResult = Dns.BeginGetHostEntry(host, new AsyncCallback(c =>
                    {
                        IPHostEntry ipHostEntry = null;

                        try
                        {
                            ipHostEntry = Dns.EndGetHostEntry(c);

                            result.AddHostEntry((string)c.AsyncState, ipHostEntry);
                        }
                        catch(SocketException)
                        {
                        }

                    }), host);

                result.AddAsyncResult(getHostEntryResult);
            }

            result.CheckComplete();

            return result;
        }

        public IPHostEntry[] EndGetHostEntries(IAsyncResult result)
        {
            ResolverAsyncResult resolverResult = result as ResolverAsyncResult;

            if (resolverResult == null)
            {
                throw new ArgumentException("EndGetHostEntries may only be called with the return result from BeginGetHostEntries");
            }

            resolverResult.AsyncWaitHandle.WaitOne(0);

            return resolverResult.GetCompletedEntries();
        }

        private class ResolverAsyncResult : IAsyncResult
        {
            private bool calledCallback = false;

            private object lockObject = new object();

            private List<IPHostEntry> hostEntries = new List<IPHostEntry>();

            private AsyncCallback asyncCallback;
            private System.Threading.ManualResetEvent waitHandle = new System.Threading.ManualResetEvent(false);

            private IList<string> hosts;
            private IList<IAsyncResult> asyncResults = new List<IAsyncResult>();

            private int hostCount;

            private Timer timer;

            public ResolverAsyncResult(IList<string> hosts, AsyncCallback asyncCallback, Timer timer)
            {
                this.asyncCallback = asyncCallback;
                this.hosts = hosts;
                this.hostCount = hosts.Count;

                this.timer = timer;
                this.timer.Elapsed += (s, e) => this.MarkComplete();
            }

            #region IAsyncResult Members

            public object AsyncState
            {
                get { throw new NotImplementedException(); }
            }

            public System.Threading.WaitHandle AsyncWaitHandle
            {
                get { return waitHandle; }
            }

            public bool CompletedSynchronously
            {
                get
                {
                    if (this.hosts.Count > 0)
                    {
                        return true;
                    }

                    lock (lockObject)
                    {
                        foreach (IAsyncResult result in asyncResults)
                        {
                            if (!result.CompletedSynchronously)
                            {
                                return false;
                            }
                        }
                    }

                    return true;
                }
            }

            public bool IsCompleted
            {
                get
                {
                    if (this.hosts.Count > 0)
                    {
                        return true;
                    }

                    lock (lockObject)
                    {
                        return (hostCount == 0);
                    }
                }
            }

            #endregion

            public void AddAsyncResult(IAsyncResult result)
            {
                lock (lockObject)
                {
                    this.asyncResults.Add(result);

                    if (result.CompletedSynchronously)
                    {
                        CheckComplete();
                    }
                }
            }

            public void AddHostEntry(string host, IPHostEntry entry)
            {
                lock(lockObject)
                {
                    Interlocked.Decrement(ref hostCount);

                    // Can be different
                    entry.HostName = host;

                    hostEntries.Add(entry);

                    this.CheckComplete();
                }
            }

            public void CheckComplete()
            {
                if (!calledCallback)
                {
                    lock (lockObject)
                    {
                        if (!calledCallback)
                        {
                            if (this.IsCompleted)
                            {
                                MarkComplete();
                            }
                        }
                    }
                }
            }

            public void MarkComplete()
            {
                if (!calledCallback)
                {
                    lock (lockObject)
                    {
                        if (!calledCallback)
                        {
                            calledCallback = true;

                            if (asyncCallback != null)
                            {
                                this.timer.Stop();

                                asyncCallback(this);
                            }

                            waitHandle.Set();
                        }
                    }
                }
            }

            public IPHostEntry[] GetCompletedEntries()
            {
                return hostEntries.ToArray();
            }
        }
    }
}
