namespace Simple.Data
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using Extensions;

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

        /// <summary>
        /// Performs initial setup of the Adapter, allowing an object to be passed in with Adapter-specific settings.
        /// </summary>
        /// <param name="settings">An <see cref="ExpandoObject"/> or an anonymous-typed object with any adapter-specific settings.</param>
        public void Setup(object settings)
        {
            Setup(settings.ObjectToDictionary());
        }

        /// <summary>
        /// Performs initial setup of the Adapter, allowing an object to be passed in with Adapter-specific settings.
        /// </summary>
        /// <param name="settings">A list of name/value pairs with any adapter-specific settings.</param>
        public void Setup(IEnumerable<KeyValuePair<string, object>> settings)
        {
            var settingsAsDictionary = (IDictionary<string, object>) _settings;
            foreach (var keyValuePair in settings)
            {
                settingsAsDictionary.Add(keyValuePair);
            }

            OnSetup();
        }

        /// <summary>
        /// Called when the <see cref="Setup(object)"/> method is called, after the settings have been set.
        /// </summary>
        /// <remarks>It is not necessary to call <c>base.OnSetup()</c> when overriding this method.</remarks>
        protected virtual void OnSetup()
        {
        }

        /// <summary>
        /// Finds data from the specified "table".
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="criteria">The criteria. This may be <c>null</c>, in which case all records should be returned.</param>
        /// <returns>The list of records matching the criteria. If no records are found, return an empty list.</returns>
        public abstract IEnumerable<IDictionary<string, object>> Find(string tableName, SimpleExpression criteria);

        /// <summary>
        /// Runs a <see cref="SimpleQuery"/>.
        /// </summary>
        /// <param name="query">The <see cref="SimpleQuery"/> to run.</param>
        /// <param name="unhandledClauses">Use this to return any clauses your adapter is unable to interpret.</param>
        /// <returns></returns>
        public abstract IEnumerable<IDictionary<string, object>> RunQuery(SimpleQuery query,
                                                                          out IEnumerable<SimpleQueryClauseBase>
                                                                              unhandledClauses);

        /// <summary>
        ///  Inserts a record into the specified "table".
        ///  </summary><param name="tableName">Name of the table.</param>
        /// <param name="data">The values to insert.</param>
        /// <returns>If possible, return the newly inserted row, including any automatically-set values such as primary keys or timestamps.</returns>
        public abstract IDictionary<string, object> Insert(string tableName, IDictionary<string, object> data);

        /// <summary>
        /// Updates the specified "table" according to specified criteria.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="data">The new values.</param>
        /// <param name="criteria">The expression to use as criteria for the update operation.</param>
        /// <returns>The number of records affected by the update operation.</returns>
        public abstract int Update(string tableName, IDictionary<string, object> data, SimpleExpression criteria);

        /// <summary>
        /// Updates the specified "table" according to default keys (to be handled by adapter).
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="data">The new values.</param>
        /// <returns>The number of records affected by the update operation.</returns>
        /// <remarks>For example, the Ado adapter will fulfil this functionality using Primary Key data.</remarks>
        public abstract int Update(string tableName, IDictionary<string, object> data);

        /// <summary>
        ///  Deletes from the specified table.
        ///  </summary><param name="tableName">Name of the table.</param>
        /// <param name="criteria">The expression to use as criteria for the delete operation.</param>
        /// <returns>The number of records which were deleted.</returns>
        public abstract int Delete(string tableName, SimpleExpression criteria);

        /// <summary>
        /// Checks to see whether the passed function name is a valid function for the Adapter.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="args">The values passed (for overload resolution etc).</param>
        /// <returns><c>true</c> if the name represents a function; otherwise, <c>false</c>.</returns>
        public abstract bool IsExpressionFunction(string functionName, params object[] args);

        /// <summary>
        /// Finds a single record.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="criteria">The criteria.</param>
        /// <returns>A dictionary containing the record.</returns>
        /// <remarks>This method has a default implementation based on the <see cref="Find(string,SimpleExpression)"/> method.
        /// You should override this method if your adapter can optimize the operation.</remarks>
        public virtual IDictionary<string, object> FindOne(string tableName, SimpleExpression criteria)
        {
            return Find(tableName, criteria).FirstOrDefault();
        }

        /// <summary>
        /// Inserts multiple records into a table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dataList">The data.</param>
        /// <returns>If possible, return the newly inserted rows, including any automatically-set values such as primary keys or timestamps.</returns>
        /// <remarks>This method has a default implementation based on the <see cref="Insert(string,IDictionary{string, object})"/> method.
        /// You should override this method if your adapter can optimize the operation.</remarks>
        public virtual IEnumerable<IDictionary<string, object>> InsertMany(string tableName,
                                                                           IEnumerable<IDictionary<string, object>> dataList)
        {
            foreach (var row in dataList)
            {
                yield return Insert(tableName, row);
            }
        }

        /// <summary>
        /// Updates multiple records in a table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="data">The data.</param>
        /// <returns>The total number of records affected by the update operations.</returns>
        /// <remarks>This method has a default implementation based on the <see cref="Update(string,IDictionary{string, object})"/> method.
        /// You should override this method if your adapter can optimize the operation.</remarks>
        public virtual int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> data)
        {
            int updateCount = 0;
            foreach (var row in data)
            {
                updateCount += Update(tableName, row);
            }
            return updateCount;
        }

        /// <summary>
        /// Runs the query as an <see cref="IObservable{T}"/>, for Reactive Extension joy.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="unhandledClauses">The unhandled clauses.</param>
        /// <returns>A cold <see cref="IObservable{T}"/> which will start pushing when subscribed.</returns>
        /// <remarks>This method has a default implementation based on the <see cref="RunQuery"/> method.
        /// You should override this method if your adapter can optimize the operation.</remarks>
        public virtual IObservable<IDictionary<string, object>> RunQueryAsObservable(SimpleQuery query,
                                                                                     out
                                                                                         IEnumerable
                                                                                         <SimpleQueryClauseBase>
                                                                                         unhandledClauses)
        {
            return RunQuery(query, out unhandledClauses).ToObservable();
        }

        /// <summary>
        /// Runs multiple queries.
        /// </summary>
        /// <param name="queries">The queries.</param>
        /// <param name="unhandledClauses">The unhandled clauses.</param>
        /// <returns>A list of lists of dictionaries. Data.</returns>
        /// <remarks>This method has a default implementation based on the <see cref="RunQuery"/> method.
        /// You should override this method if your adapter can optimize the operation.</remarks>
        public virtual IEnumerable<IEnumerable<IDictionary<string, object>>> RunQueries(SimpleQuery[] queries,
                                                                                         List
                                                                                             <
                                                                                             IEnumerable
                                                                                             <SimpleQueryClauseBase>>
                                                                                             unhandledClauses)
        {
            foreach (var query in queries)
            {
                IEnumerable<SimpleQueryClauseBase> unhandledClausesForThisQuery;
                var result = RunQuery(query, out unhandledClausesForThisQuery);
                unhandledClauses.Add(unhandledClausesForThisQuery);
                yield return result;
            }
        }

        /// <summary>
        /// Updates the specified "table" according to default keys (to be handled by adapter).
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="dataList">A list of objects to be updated.</param>
        /// <param name="criteriaFieldNames">The list of field names to be used for criteria.</param>
        /// <returns>The number of records affected by the update operation.</returns>
        /// <remarks>For example, the Ado adapter will fulfil this functionality using Primary Key data.</remarks>
        /// <remarks>This method has a default implementation based on the
        /// <see cref="Update(string,System.Collections.Generic.IDictionary{string,object},Simple.Data.SimpleExpression)"/> method.
        /// You should override this method if your adapter can optimize the operation.</remarks>
        public virtual int UpdateMany(string tableName, IEnumerable<IDictionary<string, object>> dataList,
                                      IEnumerable<string> criteriaFieldNames)
        {
            int updatedCount = 0;
            var criteriaFieldNameList = criteriaFieldNames.ToList();
            foreach (var data in dataList)
            {
                updatedCount += Update(tableName, data, GetCriteria(tableName, criteriaFieldNameList, data));
            }
            return updatedCount;
        }

        /// <summary>
        /// Creates a delegate which can accept an array of values to use as criteria against a cached template.
        /// When called, the delegate should return a single record using the passed values as criteria.
        ///  </summary><param name="tableName">Name of the table.</param>
        /// <param name="criteria">The criteria to use as a template for the delegate</param>
        /// <returns>A <c>Func&lt;object[], IDictionary&lt;string, object&gt;&gt;"</c> which finds a record.</returns>
        public virtual Func<object[], IDictionary<string, object>> CreateFindOneDelegate(string tableName,
                                                                                         SimpleExpression criteria)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a delegate which can accept an array of values to use as criteria against a cached template.
        /// When called, the delegate should return zero or more records using the passed values as criteria.
        ///  </summary><param name="tableName">Name of the table.</param>
        /// <param name="criteria">The criteria to use as a template for the delegate</param>
        /// <returns>A <c>Func&lt;object[], IDictionary&lt;string, object&gt;&gt;"</c> which finds a record.</returns>
        public virtual Func<object[], IEnumerable<IDictionary<string, object>>> CreateFindDelegate(string tableName,
                                                                                                   SimpleExpression
                                                                                                       criteria)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a criteria expression.
        /// </summary>
        /// <param name="tableName">The name of the root table for the expression.</param>
        /// <param name="criteriaFieldNames">The names of fields to be used for the criteria.</param>
        /// <param name="record">The name/value pairs to be used as criteria.</param>
        /// <returns>A <see cref="SimpleExpression"/> criteria object.</returns>
        protected static SimpleExpression GetCriteria(string tableName, IEnumerable<string> criteriaFieldNames,
                                                      IDictionary<string, object> record)
        {
            var criteria = new Dictionary<string, object>();

            foreach (var criteriaFieldName in criteriaFieldNames)
            {
                var name = criteriaFieldName;
                var keyValuePair = record.Where(kvp => kvp.Key.Homogenize().Equals(name.Homogenize())).SingleOrDefault();
                if (string.IsNullOrWhiteSpace(keyValuePair.Key))
                {
                    throw new InvalidOperationException("Key field value not set.");
                }

                criteria.Add(criteriaFieldName, keyValuePair.Value);
                record.Remove(keyValuePair);
            }
            return ExpressionHelper.CriteriaDictionaryToExpression(tableName, criteria);
        }
    }
}