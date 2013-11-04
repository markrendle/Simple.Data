namespace Simple.Data.Ado.Test
{
    using System;
    using System.Collections.Generic;
    using NUnit.Framework;

    [TestFixture]
    public class GraphNodeTest
    {
        [Test]
        public void FindsChildNode()
        {
            var root = new SingleRecordGraphNode("");
            var path = new[] {"with1", "foo", "bar"};
            var actual = root.Find(path);
            Assert.IsNotNull(actual);
        }

        [Test]
        public void FindsNestedChildNode()
        {
            var root = new SingleRecordGraphNode("");
            var path = new[] {"with1", "foo", "with1", "bar", "quux"};
            var actual = root.Find(path);
            Assert.IsNotNull(actual);
        }

        [Test]
        public void ChildNodeIsSingleForWith1()
        {
            var root = new SingleRecordGraphNode("");
            var path = new[] {"with1", "foo", "bar"};
            var actual = root.Find(path);
            Assert.IsInstanceOf<SingleRecordGraphNode>(actual);
        }

        [Test]
        public void ChildNodeIsCollectionForWithN()
        {
            var root = new SingleRecordGraphNode("");
            var path = new[] {"withn", "foo", "bar"};
            var actual = root.Find(path);
            Assert.IsInstanceOf<CollectionGraphNode>(actual);
        }

        [Test]
        public void MapsRowWithSingleChild()
        {
            var root = new SingleRecordGraphNode("");
            root.AddField("id");
            const string path = "__with1__foo__bar";
            root.AddNode(path);

            var dict = new Dictionary<string, object>
                {
                    {"id", 1},
                    {path, "Pass"}
                };

            root.SetFields(dict);

            var actual = root.GetResult();
            Assert.AreEqual(1, actual["id"]);
            var child = (IDictionary<string, object>) actual["foo"];
            Assert.AreEqual("Pass", child["bar"]);
        }
        
        [Test]
        public void DoesNotCreateRowForAllNullValues()
        {
            var root = new SingleRecordGraphNode("");
            root.AddField("id");
            const string path = "__with1__foo__bar";
            root.AddNode(path);

            var dict = new Dictionary<string, object>
                {
                    {"id", 1},
                    {path, null}
                };

            root.SetFields(dict);

            var actual = root.GetResult();
            Assert.AreEqual(1, actual["id"]);
            Assert.False(actual.ContainsKey("foo"));
        }

        [Test]
        public void MapsRowWithNChild()
        {
            var root = new SingleRecordGraphNode("");
            root.AddField("id");
            const string path = "__withn__foo__bar";
            root.AddNode(path);

            var dict = new Dictionary<string, object>
                {
                    {"id", 1},
                    {path, "Pass"}
                };

            root.SetFields(dict);

            var actual = root.GetResult();
            Assert.AreEqual(1, actual["id"]);
            var child = (IList<IDictionary<string, object>>)actual["foo"];
            Assert.AreEqual("Pass", child[0]["bar"]);
        }
        
        [Test]
        public void MapsRowWithNChildren()
        {
            var root = new SingleRecordGraphNode("");
            root.AddField("id");
            const string path = "__withn__foo__bar";
            root.AddNode(path);

            root.SetFields(Dict("id", 1, path, "Pass"));
            root.SetFields(Dict("id", 1, path, "Pass2"));
            
            var actual = root.GetResult();
            Assert.AreEqual(1, actual["id"]);
            var child = (IList<IDictionary<string, object>>)actual["foo"];
            Assert.AreEqual("Pass", child[0]["bar"]);
            Assert.AreEqual("Pass2", child[1]["bar"]);
        }
        
        [Test]
        public void MapsRowWithNChildrenWithSingleChild()
        {
            var root = new SingleRecordGraphNode("");
            root.AddField("id");
            const string foo = "__withn__foo__bar";
            const string wib = "__withn__foo__with1__wib__wob";
            root.AddNode(foo);
            root.AddNode(wib);

            root.SetFields(Dict("id", 1, foo, "Pass", wib, "WibPass"));
            root.SetFields(Dict("id", 1, foo, "Pass2", wib, "WibPass2"));
            
            var actual = root.GetResult();
            Assert.AreEqual(1, actual["id"]);
            var child = (IList<IDictionary<string, object>>)actual["foo"];
            Assert.AreEqual("Pass", child[0]["bar"]);
            var childWib = (IDictionary<string, object>) child[0]["wib"];
            Assert.AreEqual("WibPass", childWib["wob"]);
            Assert.AreEqual("Pass2", child[1]["bar"]);
            childWib = (IDictionary<string, object>) child[1]["wib"];
            Assert.AreEqual("WibPass2", childWib["wob"]);
        }

        public static IDictionary<string, object> Dict(params object[] data)
        {
            var dict = new Dictionary<string, object>();
            dict.AddRow(data);
            return dict;
        }
    }

    static class Hack
    {
        public static void AddRow(this IDictionary<string, object> dictionary, params object[] data)
        {
            if (data.Length % 2 != 0) throw new InvalidOperationException("Odd number of parameters.");
            for (int i = 0; i < data.Length; i+=2)
            {
                dictionary[data[i].ToString()] = data[i + 1];
            }
        }
    }
}