using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Simple.Data.IntegrationTest.Query
{
    using Mocking.Ado;
    using NUnit.Framework;

    [TestFixture]
    public class ExplicitJoinTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(new[] { "dbo", "Employee", "BASE TABLE" },
                                     new[] { "dbo", "Department", "BASE TABLE" },
                                     new[] { "dbo", "Activity", "BASE TABLE"},
                                     new[] { "dbo", "Activity_Join", "BASE TABLE" },
                                     new[] { "dbo", "Location", "BASE_TABLE"});

            schemaProvider.SetColumns(new[] { "dbo", "Employee", "Id" },
                                      new[] { "dbo", "Employee", "Name" },
                                      new[] { "dbo", "Employee", "ManagerId" },
                                      new[] { "dbo", "Employee", "DepartmentId" },
                                      new[] { "dbo", "Department", "Id" },
                                      new[] { "dbo", "Department", "Name" },
                                      new[] { "dbo", "Activity", "ID_Activity"},
                                      new[] { "dbo", "Activity", "ID_Trip" },
                                      new[] { "dbo", "Activity", "Activity_Time" },
                                      new[] { "dbo", "Activity", "Is_Public" },
                                      new[] { "dbo", "Activity_Join", "ID_Activity" },
                                      new[] { "dbo", "Activity_Join", "ID_Location"},
                                      new[] { "dbo", "Location", "ID_Location"}
                                      );
        }

        [Test]
        public void JoinWithExplicitClauseUsingNamedParameters()
        {
            var q = _db.Employees.Query()
                .Join(_db.Department, Id: _db.Employees.DepartmentId)
                .Select(_db.Employees.Name, _db.Department.Name.As("Department"));

            try
            {
                q.ToList();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select [dbo].[employee].[name],[dbo].[department].[name] as [Department] from [dbo].[employee]" +
                " join [dbo].[department] on ([dbo].[department].[id] = [dbo].[employee].[departmentid])");
        }

        [Test]
        public void JoinWithExplicitClauseUsingExpression()
        {
            var q = _db.Employees.Query()
                .Join(_db.Department).On(_db.Department.Id == _db.Employees.DepartmentId)
                .Select(_db.Employees.Name, _db.Department.Name.As("Department"));

            try
            {
                q.ToList();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select [dbo].[employee].[name],[dbo].[department].[name] as [Department] from [dbo].[employee]" +
                " join [dbo].[department] on ([dbo].[department].[id] = [dbo].[employee].[departmentid])");
        }

        [Test]
        public void SelfJoinWithExplicitClauseUsingNamedParameters()
        {
            var q = _db.Employees.Query()
                .Join(_db.Employees.As("Manager"), Id: _db.Employees.ManagerId);

            q = q.Select(_db.Employees.Name, q.Manager.Name.As("Manager"));

            try
            {
                q.ToList();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select [dbo].[employee].[name],[manager].[name] as [Manager] from [dbo].[employee]" +
                " join [dbo].[employee] [manager] on ([manager].[id] = [dbo].[employee].[managerid])");
        }

        [Test]
        public void SelfJoinWithExplicitClauseUsingOutParameterAndNamedParameters()
        {
            dynamic manager;
            var q = _db.Employees.Query()
                .Join(_db.Employees.As("Manager"), out manager).On(Id: _db.Employees.ManagerId)
                .Select(_db.Employees.Name, manager.Name.As("Manager"));

            try
            {
                q.ToList();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select [dbo].[employee].[name],[manager].[name] as [Manager] from [dbo].[employee]" +
                " join [dbo].[employee] [manager] on ([manager].[id] = [dbo].[employee].[managerid])");
        }

        [Test]
        public void SelfJoinWithExplicitClauseUsingExpression()
        {
            var q = _db.Employees.Query();
            q = q.Join(_db.Employees.As("Manager")).On(q.Manager.Id == _db.Employees.ManagerId);
            q = q.Select(_db.Employees.Name, q.Manager.Name.As("Manager"));

            try
            {
                q.ToList();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select [dbo].[employee].[name],[manager].[name] as [Manager] from [dbo].[employee]" +
                " join [dbo].[employee] [manager] on ([manager].[id] = [dbo].[employee].[managerid])");
        }

        [Test]
        public void SelfJoinWithExplicitClauseUsingExpressionAndOutParameter()
        {
            dynamic manager;
            var q = _db.Employees.Query()
                .Join(_db.Employees.As("Manager"), out manager).On(manager.Id == _db.Employees.ManagerId)
                .Select(_db.Employees.Name, manager.Name.As("Manager"));

            EatException(() => q.ToList());

            GeneratedSqlIs("select [dbo].[employee].[name],[manager].[name] as [Manager] from [dbo].[employee]" +
                " join [dbo].[employee] [manager] on ([manager].[id] = [dbo].[employee].[managerid])");
        }

        [Test]
        public void TwoJoins()
        {
            _db.Activity.Query()
                .Join(_db.Activity_Join).On(_db.Activity.ID_Activity == _db.Activity_Join.ID_Activity)
                .Join(_db.Location).On(_db.Activity_Join.ID_Location == _db.Location.ID_Location)
                .Where(_db.Activity.ID_trip == 1 &&
                       _db.Activity.Activity_Time == 'D' &&
                       _db.Activity.Is_Public == true)
                .Select(
                    _db.Activity.ID_Activity
                    , _db.Activity.ID_Trip
                    , _db.Activity.Activity_Time
                    , _db.Activity.Is_Public)
                .ToList<Activity>();

            GeneratedSqlIs("select [dbo].[Activity].[ID_Activity],[dbo].[Activity].[ID_Trip],[dbo].[Activity].[Activity_Time],[dbo].[Activity].[Is_Public] " +
                "from [dbo].[Activity] JOIN [dbo].[Activity_Join] ON ([dbo].[Activity].[ID_Activity] = [dbo].[Activity_Join].[ID_Activity]) " +
                "JOIN [dbo].[Location] ON ([dbo].[Activity_Join].[ID_Location] = [dbo].[Location].[ID_Location]) " +
                "WHERE (([dbo].[Activity].[ID_Trip] = @p1 AND [dbo].[Activity].[Activity_Time] = @p2) AND [dbo].[Activity].[Is_Public] = @p3)");
        }

        class Activity
        {
            public int ID_Activity { get; set; }
        }
    }
}
