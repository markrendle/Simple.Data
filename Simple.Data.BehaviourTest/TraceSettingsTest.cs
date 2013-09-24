using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Simple.Data.IntegrationTest
{
    [TestFixture]
    public class TraceSettingsTest
    {
#pragma warning disable 618
        [Test]
        public void TraceLevelShouldBePickedUpFromConfig()
        {
            Assert.AreEqual(TraceLevel.Error, Database.TraceLevel);
        }

        [Test]
        public void TraceLevelShouldBeSettableFromCode()
        {
            Database.TraceLevel = TraceLevel.Off;
            Assert.AreEqual(TraceLevel.Off, Database.TraceLevel);

        }
#pragma warning restore 618
    }
}
