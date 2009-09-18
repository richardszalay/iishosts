using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ComponentModel;
using System.Threading;
using System.Net;

namespace RichardSzalay.HostsFileExtension.Tests
{
    [TestFixture]
    public class DnsBulkHostResolverFixture
    {
        [Test]
        public void BeginGetHostEntries_ValidHosts_InvokesCallback()
        {
            DnsBulkHostResolver resolver = new DnsBulkHostResolver();

            bool completed = false;

            IAsyncResult result = resolver.BeginGetHostEntries(
                new string[] { "localhost", Environment.MachineName },
                null,
                TimeSpan.FromSeconds(5),
                c => completed = true,
                null
                );

            result.AsyncWaitHandle.WaitOne();

            Assert.IsTrue(completed);
        }

        [Test]
        public void BeginGetHostEntries_ValidHosts_InvokesCallbackOnlyOnce()
        {
            DnsBulkHostResolver resolver = new DnsBulkHostResolver();

            int invokeCount = 0;

            IAsyncResult result = resolver.BeginGetHostEntries(
                new string[] { "localhost", Environment.MachineName },
                null,
                TimeSpan.FromMilliseconds(250),
                c => Interlocked.Increment(ref invokeCount),
                null
                );

            Thread.Sleep(500);

            Assert.AreEqual(1, invokeCount);
        }

        [Test]
        public void BeginGetHostEntries_InvalidHosts_TimesOutAndReturnsValidHostsOnly()
        {
            DnsBulkHostResolver resolver = new DnsBulkHostResolver();

            int invokeCount = 0;

            string validHostname = "localhost";
            string invalidHostname = Guid.NewGuid().ToString();

            IAsyncResult result = resolver.BeginGetHostEntries(
                new string[] { validHostname, invalidHostname },
                null,
                TimeSpan.FromMilliseconds(250),
                c => Interlocked.Increment(ref invokeCount),
                null
                );

            result.AsyncWaitHandle.WaitOne();

            IPHostEntry[] entries = resolver.EndGetHostEntries(result);

            Assert.AreEqual(1, entries.Length);
            Assert.AreEqual(validHostname, entries[0].HostName);
        }
    }
}
