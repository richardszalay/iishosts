using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RichardSzalay.HostsFileExtension.Model;
using System.Net;

namespace RichardSzalay.HostsFileExtension.Tests
{
    [TestFixture]
    public class SiteHostEntryViewModelStrategyFixture
    {
        private const string DefaultAddress = "default_address";

        [Test]
        public void GetEntryModels_MissingEntries_EntriesAreAdded()
        {
            List<SiteBinding> bindings = new List<SiteBinding>()
            {
                new SiteBinding() { Host = "host1", BindingInformation = "address1" },
                new SiteBinding() { Host = "host2", BindingInformation = "address2" }
            };

            List<HostEntry> hostEntries = new List<HostEntry>()
            {
                new HostEntry("host2", "address2", null)
            };

            SiteHostEntryViewModelStrategy strategy = new SiteHostEntryViewModelStrategy(bindings, new string[] { DefaultAddress });

            List<HostEntryViewModel> outputHostEntries = strategy.GetEntryModels(hostEntries).ToList();

            Assert.AreEqual(2, outputHostEntries.Count);

            Assert.IsFalse(outputHostEntries[1].Conflicted);
            Assert.IsNotNull(outputHostEntries[1].HostEntry);
            Assert.AreEqual("host1", outputHostEntries[1].HostEntry.Hostname);
            Assert.AreEqual("address1", outputHostEntries[1].HostEntry.Address);
        }

        [Test]
        public void GetEntryModels_AddressesDoNotMatch_MarkedAsConflicted()
        {
            List<SiteBinding> bindings = new List<SiteBinding>()
            {
                new SiteBinding() { Host = "host1", BindingInformation = "address1" }
            };

            List<HostEntry> hostEntries = new List<HostEntry>()
            {
                new HostEntry("host1", "address2", null)
            };

            SiteHostEntryViewModelStrategy strategy = new SiteHostEntryViewModelStrategy(bindings, new string[] { DefaultAddress });

            List<HostEntryViewModel> outputHostEntries = strategy.GetEntryModels(hostEntries).ToList();

            Assert.AreEqual(1, outputHostEntries.Count);

            Assert.IsTrue(outputHostEntries[0].Conflicted);
            Assert.IsNotNull(outputHostEntries[0].HostEntry);
            Assert.AreEqual("host1", outputHostEntries[0].HostEntry.Hostname);
            Assert.AreEqual("address2", outputHostEntries[0].HostEntry.Address);
        }

        [Test]
        public void GetEntryModels_IsAnyAddress_MarkedAsConflicted()
        {
            List<SiteBinding> bindings = new List<SiteBinding>()
            {
                new SiteBinding() { Host = "host1", BindingInformation = "*" }
            };

            List<HostEntry> hostEntries = new List<HostEntry>()
            {
                new HostEntry("host1", "address2", null)
            };

            SiteHostEntryViewModelStrategy strategy = new SiteHostEntryViewModelStrategy(bindings, new string[] { DefaultAddress });

            List<HostEntryViewModel> outputHostEntries = strategy.GetEntryModels(hostEntries).ToList();

            Assert.AreEqual(1, outputHostEntries.Count);

            Assert.IsFalse(outputHostEntries[0].Conflicted);
            Assert.IsNotNull(outputHostEntries[0].HostEntry);
            Assert.AreEqual("host1", outputHostEntries[0].HostEntry.Hostname);
            Assert.AreEqual("address2", outputHostEntries[0].HostEntry.Address);
        }

        [Test]
        public void GetEntryModels_MissingButResolved_NotIncludedInOutput()
        {
            List<SiteBinding> bindings = new List<SiteBinding>()
            {
                new SiteBinding() { Host = "host1", BindingInformation = "192.168.0.1" }
            };

            List<HostEntry> hostEntries = new List<HostEntry>()
            {
            };

            List<IPHostEntry> ipHostEntries = new List<IPHostEntry>()
            {
                new IPHostEntry()
                {
                    HostName = "host1", 
                    AddressList = new IPAddress[]
                    {
                        IPAddress.Parse("127.0.0.1"),
                        IPAddress.Parse("192.168.0.1")
                    }
                }
            };

            SiteHostEntryViewModelStrategy strategy = new SiteHostEntryViewModelStrategy(bindings, new string[] { DefaultAddress });

            List<HostEntryViewModel> outputHostEntries = strategy.GetEntryModels(hostEntries, ipHostEntries).ToList();

            Assert.AreEqual(0, outputHostEntries.Count);
        }

        [Test]
        public void GetEntryModels_BindingUsesSpecificAddress_AddressIsNotLocalAddress_NotIncludedInOutput()
        {
            List<SiteBinding> bindings = new List<SiteBinding>()
            {
                new SiteBinding() { Host = "host1", BindingInformation = "192.168.0.1" }
            };

            List<HostEntry> hostEntries = new List<HostEntry>()
            {
            };

            List<IPHostEntry> ipHostEntries = new List<IPHostEntry>()
            {
                new IPHostEntry()
                {
                    HostName = "host1", 
                    AddressList = new IPAddress[]
                    {
                        IPAddress.Parse("192.168.0.1")
                    }
                }
            };

            SiteHostEntryViewModelStrategy strategy = new SiteHostEntryViewModelStrategy(bindings, new string[] { DefaultAddress });

            List<HostEntryViewModel> outputHostEntries = strategy.GetEntryModels(hostEntries, ipHostEntries).ToList();

            Assert.AreEqual(0, outputHostEntries.Count);
        }

        [Test]
        public void GetEntryModels_BindingUsesAnyAddress_AddressIsLocalAddress_NotIncludedInOutput()
        {
            List<SiteBinding> bindings = new List<SiteBinding>()
            {
                new SiteBinding() { Host = "host1", BindingInformation = "*" }
            };

            List<HostEntry> hostEntries = new List<HostEntry>()
            {
            };

            List<IPHostEntry> ipHostEntries = new List<IPHostEntry>()
            {
                new IPHostEntry()
                {
                    HostName = "host1", 
                    AddressList = new IPAddress[]
                    {
                        IPAddress.Parse("192.168.0.1")
                    }
                }
            };

            SiteHostEntryViewModelStrategy strategy = new SiteHostEntryViewModelStrategy(bindings, new string[] { "192.168.0.1" });

            List<HostEntryViewModel> outputHostEntries = strategy.GetEntryModels(hostEntries, ipHostEntries).ToList();

            Assert.AreEqual(0, outputHostEntries.Count);

        }

        [Test]
        public void GetEntryModels_BindingUsesAnyAddress_AddressIsNotLocalAddress_MarkedAsConflicted()
        {
            List<SiteBinding> bindings = new List<SiteBinding>()
            {
                new SiteBinding() { Host = "host1", BindingInformation = "*" }
            };

            List<HostEntry> hostEntries = new List<HostEntry>()
            {
            };

            List<IPHostEntry> ipHostEntries = new List<IPHostEntry>()
            {
                new IPHostEntry()
                {
                    HostName = "host1", 
                    AddressList = new IPAddress[]
                    {
                        IPAddress.Parse("192.168.0.1")
                    }
                }
            };

            SiteHostEntryViewModelStrategy strategy = new SiteHostEntryViewModelStrategy(bindings, new string[] { DefaultAddress });

            List<HostEntryViewModel> outputHostEntries = strategy.GetEntryModels(hostEntries, ipHostEntries).ToList();

            Assert.AreEqual(1, outputHostEntries.Count);

            Assert.IsTrue(outputHostEntries[0].Conflicted);
            Assert.IsNotNull(outputHostEntries[0].HostEntry);
            Assert.AreEqual("host1", outputHostEntries[0].HostEntry.Hostname);
            Assert.AreEqual(DefaultAddress, outputHostEntries[0].HostEntry.Address);
        }

        [Test]
        public void GetEntryModels_ExistsAndResolved_IsNotAdded()
        {
            List<SiteBinding> bindings = new List<SiteBinding>()
            {
                new SiteBinding() { Host = "host1", BindingInformation = "192.168.0.1" }
            };

            List<HostEntry> hostEntries = new List<HostEntry>()
            {
                new HostEntry("host1", "192.168.0.1", null)
            };

            List<IPHostEntry> ipHostEntries = new List<IPHostEntry>()
            {
                new IPHostEntry()
                {
                    HostName = "host1", 
                    AddressList = new IPAddress[]
                    {
                        IPAddress.Parse("127.0.0.1"),
                        IPAddress.Parse("192.168.0.1")
                    }
                }
            };

            SiteHostEntryViewModelStrategy strategy = new SiteHostEntryViewModelStrategy(bindings, new string[] { DefaultAddress });

            List<HostEntryViewModel> outputHostEntries = strategy.GetEntryModels(hostEntries, ipHostEntries).ToList();

            Assert.AreEqual(0, outputHostEntries.Count);
        }

        [Test]
        public void GetEntryModels_ResolvesToDifferentAddress_MarkedAsConflicted()
        {
            List<SiteBinding> bindings = new List<SiteBinding>()
            {
                new SiteBinding() { Host = "host1", BindingInformation = DefaultAddress }
            };

            List<HostEntry> hostEntries = new List<HostEntry>()
            {
            };

            List<IPHostEntry> ipHostEntries = new List<IPHostEntry>()
            {
                new IPHostEntry()
                {
                    HostName = "host1", 
                    AddressList = new IPAddress[]
                    {
                        IPAddress.Parse("192.168.0.1")
                    }
                }
            };

            SiteHostEntryViewModelStrategy strategy = new SiteHostEntryViewModelStrategy(bindings, new string[] { DefaultAddress });

            List<HostEntryViewModel> outputHostEntries = strategy.GetEntryModels(hostEntries, ipHostEntries).ToList();

            Assert.AreEqual(1, outputHostEntries.Count);

            Assert.IsTrue(outputHostEntries[0].Conflicted);
            Assert.IsNotNull(outputHostEntries[0].HostEntry);
            Assert.AreEqual("host1", outputHostEntries[0].HostEntry.Hostname);
            Assert.AreEqual(DefaultAddress, outputHostEntries[0].HostEntry.Address);
        }
    }
}
