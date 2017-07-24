using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Shitty.Data.IntegrationTest
{
    [TestFixture]
    public class TraceSettingsTest
    {
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
    }
}
