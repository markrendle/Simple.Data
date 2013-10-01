namespace Simple.Data.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;

    public class Migration
    {
        private readonly List<Table> _tables = new List<Table>(); 
        public Table<T> AddTableFor<T>()
        {
            var table = new Table<T>();
            table.CreateDefaultColumns();
            _tables.Add(table);
            return table;
        }
    }

    public class Table
    {
        private readonly Type _type;
        private readonly List<Column> _columns = new List<Column>(); 

        internal Table(Type type)
        {
            _type = type;
        }

        public ReadOnlyCollection<Column> Columns
        {
            get { return _columns.AsReadOnly(); }
        }

        public Table AddColumn(Column column)
        {
            _columns.Add(column);
            return this;
        }

        public Table AddColumns(IEnumerable<Column> columns)
        {
            _columns.AddRange(columns);
            return this;
        }
    }

    public class Table<T> : Table
    {
        internal Table() : base(typeof(T))
        {
        }

        public void CreateDefaultColumns()
        {
            AddColumns(
                typeof (T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(Column.FromProperty)
                );
        }
    }

    public class ColumnCollection : KeyedCollection<string, Column>
    {
        protected override string GetKeyForItem(Column item)
        {
            return item.Name;
        }
    }

    public class Column
    {
        private readonly string _name;

        public Column(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        internal static Column FromProperty(PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException("property");
            return new Column(property.Name);
        }
    }
}
