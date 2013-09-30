using Simple.Data.Operations;

namespace Simple.Data
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
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

        public OptionsBase Options { get; set; }

        /// <summary>
        /// Called when the <see cref="Setup(object)"/> method is called, after the settings have been set.
        /// </summary>
        /// <remarks>It is not necessary to call <c>base.OnSetup()</c> when overriding this method.</remarks>
        protected virtual void OnSetup()
        {
        }

        public abstract IEqualityComparer<string> KeyComparer { get; } 

        /// <summary>
        /// Gets the key value(s) for the record.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="record">The record.</param>
        /// <returns>An <c>IDictionary&lt;string,object&gt;</c> containing the key that uniquely identifies the record in the database.</returns>
        public abstract IReadOnlyDictionary<string, object> GetKey(string tableName, IReadOnlyDictionary<string, object> record);

        /// <summary>
        /// Gets the key name(s) for the table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>An <c>IList&lt;string&gt;</c> containing the key names that uniquely identify a record in the database.</returns>
        public abstract IList<string> GetKeyNames(string tableName);

        public abstract OperationResult Execute(IOperation operation);

        /// <summary>
        /// Checks to see whether the passed function name is a valid function for the Adapter.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="args">The values passed (for overload resolution etc).</param>
        /// <returns><c>true</c> if the name represents a function; otherwise, <c>false</c>.</returns>
        public abstract bool IsExpressionFunction(string functionName, params object[] args);

        /// <summary>
        /// Creates a criteria expression.
        /// </summary>
        /// <param name="tableName">The name of the root table for the expression.</param>
        /// <param name="criteriaFieldNames">The names of fields to be used for the criteria.</param>
        /// <param name="record">The name/value pairs to be used as criteria.</param>
        /// <returns>A <see cref="SimpleExpression"/> criteria object.</returns>
        public static SimpleExpression GetCriteria(string tableName, IEnumerable<string> criteriaFieldNames,
                                                      ref IReadOnlyDictionary<string, object> record)
        {
            var criteria = new Dictionary<string, object>();
            var data = record.ToDictionary();

            foreach (var criteriaFieldName in criteriaFieldNames)
            {
                var name = criteriaFieldName;
                var keyValuePair = record.SingleOrDefault(kvp => kvp.Key.Homogenize().Equals(name.Homogenize()));
                if (string.IsNullOrWhiteSpace(keyValuePair.Key))
                {
                    throw new InvalidOperationException("Key field value not set.");
                }

                criteria.Add(criteriaFieldName, keyValuePair.Value);
                data.Remove(keyValuePair.Key);
            }
            record = new ReadOnlyDictionary<string, object>(data);
            return ExpressionHelper.CriteriaDictionaryToExpression(tableName, criteria);
        }

        public void Reset()
        {
            OnReset();
        }

        protected virtual void OnReset()
        {
        }

        public virtual bool IsValidFunction(string functionName)
        {
            return false;
        }
    }
}