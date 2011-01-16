using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Simple.Data.Extensions;

namespace Simple.Data.Ado.Schema
{
    class ProcedureCollection : Collection<Procedure>
    {
        public ProcedureCollection()
        {
            
        }

        public ProcedureCollection(IEnumerable<Procedure> procedures) : base(procedures.ToList())
        {
            
        }

        /// <summary>
        /// Finds the procedure with a name most closely matching the specified procedure name.
        /// This method will try an exact match first, then a case-insensitve search, then a pluralized or singular version.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <returns>A <see cref="Procedure"/> if a match is found; otherwise, <c>null</c>.</returns>
        public Procedure Find(string procedureName)
        {
            if (procedureName.Contains('.'))
            {
                var schemaDotprocedure = procedureName.Split('.');
                if (schemaDotprocedure.Length != 2) throw new InvalidOperationException("Could not resolve qualified procedure name.");
                return Find(schemaDotprocedure[1], schemaDotprocedure[0]);
            }
            var procedure = FindprocedureWithName(procedureName.Homogenize())
                   ?? FindprocedureWithPluralName(procedureName.Homogenize())
                   ?? FindprocedureWithSingularName(procedureName.Homogenize());

            if (procedure == null)
            {
                throw new UnresolvableObjectException(procedureName, "No matching procedure found, or insufficient permissions.");
            }

            return procedure;
        }

        /// <summary>
        /// Finds the procedure with a name most closely matching the specified procedure name.
        /// This method will try an exact match first, then a case-insensitve search, then a pluralized or singular version.
        /// </summary>
        /// <param name="procedureName">Name of the procedure.</param>
        /// <param name="schemaName"></param>
        /// <returns>A <see cref="Procedure"/> if a match is found; otherwise, <c>null</c>.</returns>
        public Procedure Find(string procedureName, string schemaName)
        {
            var procedure = FindprocedureWithName(procedureName.Homogenize(), schemaName.Homogenize())
                   ?? FindprocedureWithPluralName(procedureName.Homogenize(), schemaName.Homogenize())
                   ?? FindprocedureWithSingularName(procedureName.Homogenize(), schemaName.Homogenize());

            if (procedure == null)
            {
                throw new UnresolvableObjectException(schemaName + '.' + procedureName, "No matching procedure found, or insufficient permissions.");
            }

            return procedure;
        }

        private Procedure FindprocedureWithSingularName(string procedureName, string schemaName)
        {
            return FindprocedureWithName(procedureName.Singularize(), schemaName);
        }

        private Procedure FindprocedureWithPluralName(string procedureName, string schemaName)
        {
            return FindprocedureWithName(procedureName.Pluralize(), schemaName);
        }

        private Procedure FindprocedureWithName(string procedureName, string schemaName)
        {
            return this
                .Where(sp => sp.HomogenizedName.Equals(procedureName) && (sp.Schema == null || sp.Schema.Homogenize().Equals(schemaName)))
                .SingleOrDefault();
        }

        private Procedure FindprocedureWithName(string procedureName)
        {
            return this
                .Where(t => t.HomogenizedName.Equals(procedureName))
                .SingleOrDefault();
        }

        private Procedure FindprocedureWithPluralName(string procedureName)
        {
            return FindprocedureWithName(procedureName.Pluralize());
        }

        private Procedure FindprocedureWithSingularName(string procedureName)
        {
            if (procedureName.IsPlural())
            {
                return FindprocedureWithName(procedureName.Singularize());
            }

            return null;
        }

        public Procedure Find(ObjectName procedureName)
        {
            return Find(procedureName.Name, procedureName.Schema);
        }
    }
}
