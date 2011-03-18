using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data
{
    /// <summary>
    /// Provides a base class for adapters to persist data to data storage systems.
    /// Authors may derive from this class to create support for all kinds of databases or data stores.
    /// </summary>
    public abstract class Adapter
    {
        private readonly ExpandoObject _settings = new ExpandoObject();

        /// <summary>
        /// Gets an <see cref="ExpandoObject"/> with the settings for this <see cref="Adapter"/>.
        /// This property may be cast to an <see cref="IDictionary{TKey,TValue}"/> with a <see cref="string"/>
        /// key and <see cref="object"/> value for non-dynamic access.
        /// </summary>
        /// <value>The settings.</value>
        protected dynamic Settings
        {
            get { return _settings; }
        }

        public void Setup(object settings)
        {
            Setup(ObjectEx.ObjectToDictionary(settings));
        }

        public void Setup(IEnumerable<KeyValuePair<string,object>> settings)
        {
            var settingsAsDictionary = (IDictionary<string, object>) _settings;
            foreach (var keyValuePair in settings)
            {
                settingsAsDictionary.Add(keyValuePair);
            }

            OnSetup();
        }

        protected virtual void OnSetup()
        {
            
        }

        /// <summary>
        ///  Finds data from the specified "table".
        ///  </summary><param name="tableName">Name of the table.</param><param name="criteria">The criteria. This may be <c>null</c>, in which case all records should be returned.</param><returns>The list of records matching the criteria. If no records are found, return an empty list.</returns>
        public abstract IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria);

        /// <summary>
        ///  Inserts a record into the specified "table".
        ///  </summary><param name="tableName">Name of the table.</param><param name="data">The values to insert.</param><returns>If possible, return the newly inserted row, including any automatically-set values such as primary keys or timestamps.</returns>
        public abstract IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data);

        /// <summary>
        ///  Updates the specified "table" according to specified criteria.
        ///  </summary><param name="tableName">Name of the table.</param><param name="data">The new values.</param><param name="criteria">The expression to use as criteria for the update operation.</param><returns>The number of records affected by the update operation.</returns>
        public abstract int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria);

        /// <summary>
        ///  Deletes from the specified table.
        ///  </summary><param name="tableName">Name of the table.</param><param name="criteria">The expression to use as criteria for the delete operation.</param><returns>The number of records which were deleted.</returns>
        public abstract int Delete(string tableName, SimpleExpression criteria);

        /// <summary>
        ///  Gets the names of the fields which comprise the unique identifier for the specified table.
        ///  </summary><param name="tableName">Name of the table.</param><returns></returns>
        public abstract IEnumerable<string> GetKeyFieldNames(string tableName);

    	public abstract object Max(string tableName, string columnName, SimpleExpression criteria);
    }
}
