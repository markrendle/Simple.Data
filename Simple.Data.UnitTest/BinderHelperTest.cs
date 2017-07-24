using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Shitty.Data.Extensions;
using NUnit.Framework;

namespace Shitty.Data.UnitTest
{
    [TestFixture]
    public class BinderHelperTest
    {
        [Test]
        public void NamedArgumentsToDictionaryShouldApplyNamesToLastArguments()
        {
            var binder = new TestInvokeMemberBinder("Test", false, new CallInfo(2, "Foo"));
            var actual = binder.NamedArgumentsToDictionary(new object[] {1, 2});
            Assert.AreEqual(2, actual["Foo"]);
        }

        [Test]
        public void ArgumentsToDictionaryShouldApplyOrdinalNamesToFirstArguments()
        {
            var binder = new TestInvokeMemberBinder("Test", false, new CallInfo(2, "Foo"));
            var actual = binder.ArgumentsToDictionary(new object[] { 1, 2 });
            Assert.AreEqual("_0", actual.First().Key);
        }

        [Test]
        public void ArgumentsToDictionaryShouldReturnInputParameterWhenInputParameterIsDictionary()
        {
            var binder = new TestInvokeMemberBinder("Test", false, new CallInfo(1, "Foo"));
            var input = new Dictionary<string, object> { { "Test1", 1 }, { "Test2", 2 } };
            var actual = binder.ArgumentsToDictionary(new object[] { input });
            Assert.AreEqual(input["Test2"], actual["Test2"]);
        }
    }

    class TestInvokeMemberBinder : InvokeMemberBinder
    {
        public TestInvokeMemberBinder(string name, bool ignoreCase, CallInfo callInfo) : base(name, ignoreCase, callInfo)
        {
        }

        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            throw new NotImplementedException();
        }
    }
}
