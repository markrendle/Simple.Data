using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data.Ado.Schema
{
    class StoredProcedureCollection : Collection<StoredProcedure>
    {
        public StoredProcedureCollection()
        {
            
        }

        public StoredProcedureCollection(IEnumerable<StoredProcedure> storedProcedures) : base(storedProcedures.ToList())
        {
            
        }

        /// <summary>
        /// Finds the StoredProcedure with a name most closely matching the specified storedProcedure name.
        /// This method will try an exact match first, then a case-insensitve search, then a pluralized or singular version.
        /// </summary>
        /// <param name="storedProcedureName">Name of the storedProcedure.</param>
        /// <returns>A <see cref="StoredProcedure"/> if a match is found; otherwise, <c>null</c>.</returns>
        public StoredProcedure Find(string storedProcedureName)
        {
            if (storedProcedureName.Contains('.'))
            {
                var schemaDotStoredProcedure = storedProcedureName.Split('.');
                if (schemaDotStoredProcedure.Length != 2) throw new InvalidOperationException("Could not resolve qualified storedProcedure name.");
                return Find(schemaDotStoredProcedure[1], schemaDotStoredProcedure[0]);
            }
            var storedProcedure = FindStoredProcedureWithName(storedProcedureName.Homogenize())
                   ?? FindStoredProcedureWithPluralName(storedProcedureName.Homogenize())
                   ?? FindStoredProcedureWithSingularName(storedProcedureName.Homogenize());

            if (storedProcedure == null)
            {
                throw new UnresolvableObjectException(storedProcedureName, "No matching storedProcedure found, or insufficient permissions.");
            }

            return storedProcedure;
        }

        /// <summary>
        /// Finds the StoredProcedure with a name most closely matching the specified storedProcedure name.
        /// This method will try an exact match first, then a case-insensitve search, then a pluralized or singular version.
        /// </summary>
        /// <param name="storedProcedureName">Name of the storedProcedure.</param>
        /// <param name="schemaName"></param>
        /// <returns>A <see cref="StoredProcedure"/> if a match is found; otherwise, <c>null</c>.</returns>
        public StoredProcedure Find(string storedProcedureName, string schemaName)
        {
            var storedProcedure = FindStoredProcedureWithName(storedProcedureName.Homogenize(), schemaName.Homogenize())
                   ?? FindStoredProcedureWithPluralName(storedProcedureName.Homogenize(), schemaName.Homogenize())
                   ?? FindStoredProcedureWithSingularName(storedProcedureName.Homogenize(), schemaName.Homogenize());

            if (storedProcedure == null)
            {
                throw new UnresolvableObjectException(schemaName + '.' + storedProcedureName, "No matching storedProcedure found, or insufficient permissions.");
            }

            return storedProcedure;
        }

        private StoredProcedure FindStoredProcedureWithSingularName(string storedProcedureName, string schemaName)
        {
            return FindStoredProcedureWithName(storedProcedureName.Singularize(), schemaName);
        }

        private StoredProcedure FindStoredProcedureWithPluralName(string storedProcedureName, string schemaName)
        {
            return FindStoredProcedureWithName(storedProcedureName.Pluralize(), schemaName);
        }

        private StoredProcedure FindStoredProcedureWithName(string storedProcedureName, string schemaName)
        {
            return this
                .Where(sp => sp.HomogenizedName.Equals(storedProcedureName) && (sp.Schema == null || sp.Schema.Homogenize().Equals(schemaName)))
                .SingleOrDefault();
        }

        private StoredProcedure FindStoredProcedureWithName(string storedProcedureName)
        {
            return this
                .Where(t => t.HomogenizedName.Equals(storedProcedureName))
                .SingleOrDefault();
        }

        private StoredProcedure FindStoredProcedureWithPluralName(string storedProcedureName)
        {
            return FindStoredProcedureWithName(storedProcedureName.Pluralize());
        }

        private StoredProcedure FindStoredProcedureWithSingularName(string storedProcedureName)
        {
            if (storedProcedureName.IsPlural())
            {
                return FindStoredProcedureWithName(storedProcedureName.Singularize());
            }

            return null;
        }

        public StoredProcedure Find(ObjectName storedProcedureName)
        {
            return Find(storedProcedureName.Name, storedProcedureName.Schema);
        }
    }
}
