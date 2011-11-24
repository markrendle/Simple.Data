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
        [Test]
        public void TraceLevelShouldBePickedUpFromConfig()
        {
            Assert.AreEqual(TraceLevel.Error, Database.TraceLevel);
        }
    }
}
