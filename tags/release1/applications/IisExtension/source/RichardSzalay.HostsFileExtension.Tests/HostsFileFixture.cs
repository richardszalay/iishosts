using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RichardSzalay.HostsFileExtension.Tests.Properties;

namespace RichardSzalay.HostsFileExtension.Tests
{
    [TestFixture]
    public class HostsFileFixture
    {
        [Test]
        public void Load_LoadsValues()
        {
            string inputValue = "127.0.0.1    host1 # comment 1\n192.168.0.1    host2";

            HostEntry[] expectedEntries = new HostEntry[]
            {
                new HostEntry(0, "127.0.0.1    host1 # comment 1", "    ", true, "host1", "127.0.0.1", "comment 1"),
                new HostEntry(1, "192.168.0.1    host2", "    ", true, "host2", "192.168.0.1", null),
            };

            StringResource resource = new StringResource(inputValue);

            HostsFile file = new HostsFile(resource);

            Assert.IsTrue(expectedEntries.SequenceEqual(file.Entries));
        }

        [Test]
        public void Load_ParsesSampleFile()
        {
            StringResource resource = new StringResource(Resources.SampleHostsFile);

            HostEntry[] expectedEntries = new HostEntry[]
            {
                new HostEntry(22, "127.0.0.1		host1.localhost # comment 1", "		", true, "host1.localhost", "127.0.0.1", "comment 1"),
                new HostEntry(23, "# 192.168.0.1		host2.localhost # comment 2", "		", false, "host2.localhost", "192.168.0.1", "comment 2"),
            };

            HostsFile file = new HostsFile(resource);

            Assert.IsTrue(expectedEntries.SequenceEqual(file.Entries));
        }

        [Test]
        public void Load_HostnameCanIncludeHyphen()
        {
            StringResource resource = new StringResource("127.0.0.1		host1-localhost # comment 1");

            HostsFile file = new HostsFile(resource);

            Assert.AreEqual("host1-localhost", file.Entries.First().Hostname);
        }

        [Test]
        public void Load_ExcludesIgnoredHosts()
        {
            StringResource resource = new StringResource(Resources.DefaultHostsFile);

            HostsFile file = new HostsFile(resource);

            Assert.AreEqual(0, file.Entries.ToList().Count);
        }

        [Test]
        public void Save_AppliesEnabled()
        {
            StringResource resource = new StringResource(Resources.SampleHostsFile);

            HostsFile file = new HostsFile(resource);

            file.Entries.Where(c => c.Hostname == "host1.localhost").First().Enabled = false;
            file.Entries.Where(c => c.Hostname == "host2.localhost").First().Enabled = true;

            file.Save();

            string actualOutput = resource.ToString();

            Assert.AreEqual(Resources.SampleHostsFile_Disable, actualOutput);
        }

        [Test]
        public void Save_AppliesReordering()
        {
            StringResource resource = new StringResource(Resources.SampleHostsFile);

            HostsFile file = new HostsFile(resource);

            var entryA = file.Entries.Where(c => c.Hostname == "host1.localhost").First();
            var entryB = file.Entries.Where(c => c.Hostname == "host2.localhost").First();

            entryA.SwapLine(entryB);

            file.Save();

            string actualOutput = resource.ToString();

            Assert.AreEqual(Resources.SampleHostsFile_Reorder, actualOutput);
        }

        [Test]
        public void Save_AppliesDeletion()
        {
            StringResource resource = new StringResource(Resources.SampleHostsFile);

            HostsFile file = new HostsFile(resource);

            var entryA = file.Entries.Where(c => c.Hostname == "host1.localhost").First();

            file.DeleteEntry(entryA);

            file.Save();

            string actualOutput = resource.ToString();

            Assert.AreEqual(Resources.SampleHostsFile_Delete, actualOutput);
        }

        [Test]
        public void Save_AppliesDeletionReorderingDisabling()
        {
            StringResource resource = new StringResource(Resources.ComplexHostsFile_Before);

            HostsFile file = new HostsFile(resource);

            var entry1 = file.Entries.Where(c => c.Hostname == "host1.localhost").First();
            var entry2 = file.Entries.Where(c => c.Hostname == "host2.localhost").First();
            var entry3 = file.Entries.Where(c => c.Hostname == "host3.localhost").First();
            var entry4 = file.Entries.Where(c => c.Hostname == "host4.localhost").First();
            var entry5 = file.Entries.Where(c => c.Hostname == "host5.localhost").First();
            var entry6 = new HostEntry("host6.localhost", "127.0.0.1", "comment 6");

            entry1.Enabled = false;
            entry2.Enabled = true;
            entry3.Enabled = false;

            entry3.SwapLine(entry5); // swap two with a deleted in between
            entry6.SwapLine(entry2); // new swapped with existing

            file.DeleteEntry(entry4);
            file.AddEntry(entry6);

            file.Save();

            string actualOutput = resource.ToString();

            Assert.AreEqual(Resources.ComplexHostsFile_Expected, actualOutput);
        }

        [Test]
        public void AddEntry_AddsToEntries()
        {
            StringResource resource = new StringResource();

            HostsFile file = new HostsFile(resource);

            HostEntry entry = new HostEntry("host.localhost", "1.0.0.0", null);

            file.AddEntry(entry);

            Assert.IsTrue(file.Entries.Contains(entry));
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void AddEntry_NullEntry_ThrowsArgumentNullException()
        {
            StringResource resource = new StringResource();

            HostsFile file = new HostsFile(resource);

            file.AddEntry(null);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void AddEntry_HostnameIsIgnored_ThrowsArgumentError()
        {
            StringResource resource = new StringResource();

            HostsFile file = new HostsFile(resource);

            HostEntry entry = new HostEntry("localhost", "1.0.0.0", null);

            file.AddEntry(entry);
        }

        [Test]
        public void DeleteEntry_RemovesFromEntries()
        {
            StringResource resource = new StringResource("127.0.0.1    host1 # comment 1\n192.168.0.1    host2");

            HostsFile file = new HostsFile(resource);

            var entry = file.Entries.First();

            file.DeleteEntry(entry);

            Assert.IsFalse(file.Entries.Contains(entry));
        }

        [Test]
        public void DeleteEntry_DoesNotExist_DoesNotThrowException()
        {
            StringResource resource = new StringResource();

            HostsFile file = new HostsFile(resource);

            HostEntry entry = new HostEntry("host.localhost", "1.0.0.0", null);

            file.DeleteEntry(entry);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void DeleteEntry_NullEntry_ThrowsArgumentNullException()
        {
            StringResource resource = new StringResource();

            HostsFile file = new HostsFile(resource);

            file.DeleteEntry(null);
        }

        [Test]
        public void IsDirty_DefaultState_ReturnsFalse()
        {
            string inputValue = "127.0.0.1    host1 # comment 1\n192.168.0.1    host2";

            StringResource resource = new StringResource(inputValue);

            HostsFile file = new HostsFile(resource);

            Assert.IsFalse(file.IsDirty);
        }

        [Test]
        public void IsDirty_ItemAdded_ReturnsTrue()
        {
            string inputValue = "127.0.0.1    host1 # comment 1\n192.168.0.1    host2";

            StringResource resource = new StringResource(inputValue);

            HostsFile file = new HostsFile(resource);

            HostEntry entry = new HostEntry("host.localhost", "1.0.0.0", null);

            file.AddEntry(entry);

            Assert.IsTrue(file.IsDirty);
        }

        [Test]
        public void IsDirty_ItemDeleted_ReturnsTrue()
        {
            string inputValue = "127.0.0.1    host1 # comment 1\n192.168.0.1    host2";

            StringResource resource = new StringResource(inputValue);

            HostsFile file = new HostsFile(resource);

            file.DeleteEntry(file.Entries.First());

            Assert.IsTrue(file.IsDirty);
        }

        [Test]
        public void IsDirty_ItemDirty_ReturnsTrue()
        {
            string inputValue = "127.0.0.1    host1 # comment 1\n192.168.0.1    host2";

            StringResource resource = new StringResource(inputValue);

            HostsFile file = new HostsFile(resource);

            file.Entries.First().Enabled = false;

            Assert.IsTrue(file.IsDirty);
        }
    }
}
