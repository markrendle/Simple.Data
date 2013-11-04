namespace Simple.Data.Ado
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    class GraphMap
    {
        private static readonly string[] Delimiter = {"__"};
        private readonly Dictionary<string,GraphNode> _nodes = new Dictionary<string, GraphNode>(StringComparer.OrdinalIgnoreCase); 
        public static GraphNode Create(ICollection<string> keys)
        {
            var root = new SingleRecordGraphNode("");

            foreach (var rootKey in keys.Where(k => !k.StartsWith("__with")))
            {
                root.AddField(rootKey);
            }

            var childKeys = keys.Where(k => k.StartsWith("__with"))
                .OrderBy(k => k.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries).Length)
                .ThenBy(k => k);

            return root;
        }

    }
}