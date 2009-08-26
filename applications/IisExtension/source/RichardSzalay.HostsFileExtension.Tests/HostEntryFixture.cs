using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace RichardSzalay.HostsFileExtension.Tests
{
    [TestFixture]
    public class HostEntryFixture
    {
        [Test]
        public void Constructor_AssignsProperties()
        {
            HostEntry entry = new HostEntry("hostname", "address", "comment");
            
            Assert.AreEqual("hostname", entry.Hostname);
            Assert.AreEqual("address", entry.Address);
            Assert.AreEqual("comment", entry.Comment);
            Assert.AreEqual(-1, entry.Line);
            Assert.AreEqual(true, entry.Enabled);
        }

        [Test]
        public void InternalConstructor_AssignsProperties()
        {
            HostEntry entry = new HostEntry(5, "original-line", " ", false, "hostname", "address", "comment");

            Assert.AreEqual("hostname", entry.Hostname);
            Assert.AreEqual("address", entry.Address);
            Assert.AreEqual("comment", entry.Comment);
            Assert.AreEqual(5, entry.Line);
            Assert.AreEqual(false, entry.Enabled);
        }

        [Test]
        public void Constructor_IsEnabled()
        {
            HostEntry entry = new HostEntry("hostname", "address", "comment");

            Assert.IsTrue(entry.Enabled);
        }

        [Test]
        public void Constructor_IsDirty()
        {
            HostEntry entry = new HostEntry("hostname", "address", "comment");

            Assert.IsTrue(entry.IsDirty);
        }

        [Test]
        public void Constructor_IsNew()
        {
            HostEntry entry = new HostEntry("hostname", "address", "comment");

            Assert.IsTrue(entry.IsNew);
        }

        [TestCase(-1, Result = true)]
        [TestCase(0, Result = false)]
        [TestCase(20, Result = false)]
        public bool IsNew_DependsOnLine(int line)
        {
            HostEntry entry = new HostEntry(line, "original-line", " ", false, "hostname", "address", "comment");

            return entry.IsNew;
        }

        [Test]
        public void IsDirty_ValuesNotChanged_ReturnsFalse()
        {
            HostEntry entry = new HostEntry(5, "original-line", " ", false, "hostname", "address", "comment");

            entry.Enabled = false;
            entry.Hostname = "hostname";
            entry.Address = "address";
            entry.Comment = "comment";

            Assert.IsFalse(entry.IsDirty);
        }

        [Test]
        public void IsDirty_LineSwapped_ReturnsTrue()
        {
            HostEntry entryA = new HostEntry(5, "original-line", " ", false, "hostname", "address", "comment");
            HostEntry entryB = new HostEntry(10, "original-line", " ", false, "hostname", "address", "comment");

            entryA.SwapLine(entryB);

            Assert.IsTrue(entryA.IsDirty);
            Assert.IsTrue(entryB.IsDirty);
        }

        [Test]
        public void IsDirty_LineSwapped_SwapsLineNumbers()
        {
            HostEntry entryA = new HostEntry(5, "original-line", " ", false, "hostname", "address", "comment");
            HostEntry entryB = new HostEntry(10, "original-line", " ", false, "hostname", "address", "comment");

            entryA.SwapLine(entryB);

            Assert.AreEqual(10, entryA.Line);
            Assert.AreEqual(5, entryB.Line);
        }

        [Test]
        public void IsDirty_LineSwapped_SwapsNewStatus()
        {
            HostEntry entryA = new HostEntry(-1, "original-line", " ", false, "hostname", "address", "comment");
            HostEntry entryB = new HostEntry(10, "original-line", " ", false, "hostname", "address", "comment");

            entryA.SwapLine(entryB);

            Assert.IsFalse(entryA.IsNew);
            Assert.IsTrue(entryB.IsNew);
        }

        [TestCase("Enabled", true)]
        [TestCase("Hostname", "hostname2")]
        [TestCase("Address", "address2")]
        [TestCase("Comment", "comment2")]
        public void IsDirty_AnyProprtyChanged_ReturnsTrue(string propertyName, object newValue)
        {
            HostEntry entry = new HostEntry(5, "original-line", " ", false, "hostname", "address", "comment");

            entry.GetType().GetProperty(propertyName).SetValue(entry, newValue, null);

            Assert.IsTrue(entry.IsDirty);
        }

        [Test]
        public void ToString_IsDirty_FormatsValues()
        {
            HostEntry entry = new HostEntry(5, "original-line", "    ", false, "hostname", "address", "comment");

            entry.Hostname = "hostname2";

            string expectedValue = "# address    hostname2 # comment";
            string actualValue = entry.ToString();

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void ToString_IsNotDirty_HasOriginalString_ReturnsOriginalString()
        {
            HostEntry entry = new HostEntry(5, "original-line", " ", false, "hostname", "address", "comment");

            string actualValue = entry.ToString();

            Assert.AreEqual("original-line", actualValue);
        }

        [Test]
        public void ToString_IsNotDirty_DoesNotHaveOriginalString_FormatsValues()
        {
            HostEntry entry = new HostEntry(5, null, "    ", false, "hostname", "address", "comment");

            string expectedValue = "# address    hostname # comment";
            string actualValue = entry.ToString();

            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void Equals_SameValues_ReturnsTrue()
        {
            HostEntry entryA = new HostEntry(5, null, "    ", false, "hostname", "address", "comment");
            HostEntry entryB = new HostEntry(5, null, "    ", false, "hostname", "address", "comment");

            Assert.IsTrue(entryA.Equals(entryB));
        }

        [TestCase("Enabled", true)]
        [TestCase("Hostname", "hostname2")]
        [TestCase("Address", "address2")]
        [TestCase("Comment", "comment2")]
        public void Equals_DifferentProperty_ReturnsFalse(string propertyName, object newValue)
        {
            HostEntry entryA = new HostEntry(5, null, "    ", false, "hostname", "address", "comment");
            HostEntry entryB = new HostEntry(5, null, "    ", false, "hostname", "address", "comment");

            entryB.GetType().GetProperty(propertyName).SetValue(entryB, newValue, null);

            Assert.IsFalse(entryA.Equals(entryB));
        }

        [TestCase("rhino.acme.com", Result = true)]
        [TestCase("x.acme.com", Result = true)]
        [TestCase("localhost", Result = true)]
        [TestCase("host.localhost", Result = false)]
        public bool IsIgnoredHostname_ReturnsCorrectValue(string hostname)
        {
            return HostEntry.IsIgnoredHostname(hostname);
        }
    }
}
