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
            var q = TargetDb.Employees.Query()
                .Join(TargetDb.Department, Id: TargetDb.Employees.DepartmentId)
                .Select(TargetDb.Employees.Name, TargetDb.Department.Name.As("Department"));

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
        public void OuterJoinWithExplicitClauseUsingNamedParameters()
        {
            var q = TargetDb.Employees.Query()
                .OuterJoin(TargetDb.Department, Id: TargetDb.Employees.DepartmentId)
                .Select(TargetDb.Employees.Name, TargetDb.Department.Name.As("Department"));

            try
            {
                q.ToList();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select [dbo].[employee].[name],[dbo].[department].[name] as [Department] from [dbo].[employee]" +
                " left join [dbo].[department] on ([dbo].[department].[id] = [dbo].[employee].[departmentid])");
        }

        [Test]
        public void LeftJoinWithExplicitClauseUsingNamedParameters()
        {
            var q = TargetDb.Employees.Query()
                .LeftJoin(TargetDb.Department, Id: TargetDb.Employees.DepartmentId)
                .Select(TargetDb.Employees.Name, TargetDb.Department.Name.As("Department"));

            try
            {
                q.ToList();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select [dbo].[employee].[name],[dbo].[department].[name] as [Department] from [dbo].[employee]" +
                " left join [dbo].[department] on ([dbo].[department].[id] = [dbo].[employee].[departmentid])");
        }

        [Test]
        public void JoinWithExplicitClauseUsingExpression()
        {
            var q = TargetDb.Employees.Query()
                .Join(TargetDb.Department).On(TargetDb.Department.Id == TargetDb.Employees.DepartmentId)
                .Select(TargetDb.Employees.Name, TargetDb.Department.Name.As("Department"));

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
        public void LeftJoinWithExplicitClauseUsingExpression()
        {
            var q = TargetDb.Employees.Query()
                .LeftJoin(TargetDb.Department).On(TargetDb.Department.Id == TargetDb.Employees.DepartmentId)
                .Select(TargetDb.Employees.Name, TargetDb.Department.Name.As("Department"));

            try
            {
                q.ToList();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select [dbo].[employee].[name],[dbo].[department].[name] as [Department] from [dbo].[employee]" +
                " left join [dbo].[department] on ([dbo].[department].[id] = [dbo].[employee].[departmentid])");
        }

        [Test]
        public void SelfJoinWithExplicitClauseUsingNamedParameters()
        {
            var q = TargetDb.Employees.Query()
                .Join(TargetDb.Employees.As("Manager"), Id: TargetDb.Employees.ManagerId);

            q = q.Select(TargetDb.Employees.Name, q.Manager.Name.As("Manager"));

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
        public void LeftSelfJoinWithExplicitClauseUsingNamedParameters()
        {
            var q = TargetDb.Employees.Query()
                .LeftJoin(TargetDb.Employees.As("Manager"), Id: TargetDb.Employees.ManagerId);

            q = q.Select(TargetDb.Employees.Name, q.Manager.Name.As("Manager"));

            try
            {
                q.ToList();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select [dbo].[employee].[name],[manager].[name] as [Manager] from [dbo].[employee]" +
                " left join [dbo].[employee] [manager] on ([manager].[id] = [dbo].[employee].[managerid])");
        }

        [Test]
        public void SelfJoinWithExplicitClauseUsingOutParameterAndNamedParameters()
        {
            dynamic manager;
            var q = TargetDb.Employees.Query()
                .Join(TargetDb.Employees.As("Manager"), out manager).On(Id: TargetDb.Employees.ManagerId)
                .Select(TargetDb.Employees.Name, manager.Name.As("Manager"));

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
        public void LeftSelfJoinWithExplicitClauseUsingOutParameterAndNamedParameters()
        {
            dynamic manager;
            var q = TargetDb.Employees.Query()
                .LeftJoin(TargetDb.Employees.As("Manager"), out manager).On(Id: TargetDb.Employees.ManagerId)
                .Select(TargetDb.Employees.Name, manager.Name.As("Manager"));

            try
            {
                q.ToList();
            }
            catch (InvalidOperationException)
            {
                // This won't work on Mock provider, but the SQL should be generated OK
            }

            GeneratedSqlIs("select [dbo].[employee].[name],[manager].[name] as [Manager] from [dbo].[employee]" +
                " left join [dbo].[employee] [manager] on ([manager].[id] = [dbo].[employee].[managerid])");
        }

        [Test]
        public void SelfJoinWithExplicitClauseUsingExpression()
        {
            var q = TargetDb.Employees.Query();
            q = q.Join(TargetDb.Employees.As("Manager")).On(q.Manager.Id == TargetDb.Employees.ManagerId);
            q = q.Select(TargetDb.Employees.Name, q.Manager.Name.As("Manager"));

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
            var q = TargetDb.Employees.Query()
                .Join(TargetDb.Employees.As("Manager"), out manager).On(manager.Id == TargetDb.Employees.ManagerId)
                .Select(TargetDb.Employees.Name, manager.Name.As("Manager"));

            EatException(() => q.ToList());

            GeneratedSqlIs("select [dbo].[employee].[name],[manager].[name] as [Manager] from [dbo].[employee]" +
                " join [dbo].[employee] [manager] on ([manager].[id] = [dbo].[employee].[managerid])");
        }

        [Test]
        public void TwoJoins()
        {
            TargetDb.Activity.Query()
                .Join(TargetDb.Activity_Join).On(TargetDb.Activity.ID_Activity == TargetDb.Activity_Join.ID_Activity)
                .Join(TargetDb.Location).On(TargetDb.Activity_Join.ID_Location == TargetDb.Location.ID_Location)
                .Where(TargetDb.Activity.ID_trip == 1 &&
                       TargetDb.Activity.Activity_Time == 'D' &&
                       TargetDb.Activity.Is_Public == true)
                .Select(
                    TargetDb.Activity.ID_Activity
                    , TargetDb.Activity.ID_Trip
                    , TargetDb.Activity.Activity_Time
                    , TargetDb.Activity.Is_Public)
                .ToList<Activity>();

            GeneratedSqlIs("select [dbo].[Activity].[ID_Activity],[dbo].[Activity].[ID_Trip],[dbo].[Activity].[Activity_Time],[dbo].[Activity].[Is_Public] " +
                "from [dbo].[Activity] JOIN [dbo].[Activity_Join] ON ([dbo].[Activity].[ID_Activity] = [dbo].[Activity_Join].[ID_Activity]) " +
                "JOIN [dbo].[Location] ON ([dbo].[Activity_Join].[ID_Location] = [dbo].[Location].[ID_Location]) " +
                "WHERE (([dbo].[Activity].[ID_Trip] = @p1 AND [dbo].[Activity].[Activity_Time] = @p2) AND [dbo].[Activity].[Is_Public] = @p3)");
        }

        [Test]
        public void PassingTrueToOnThrowsBadExpressionException()
        {
            Assert.Throws<BadExpressionException>(
                () => TargetDb.Activity.All().Join(TargetDb.Location).On(true));
        }

        [Test]
        public void PassingObjectReferenceToOnThrowsBadExpressionException()
        {
            Assert.Throws<BadExpressionException>(
                () => TargetDb.Activity.All().Join(TargetDb.Location).On(TargetDb.Location.ID_Location));
        }

        [Test]
        public void PassingTablesWrongWayRoundThrowsBadExpressionException()
        {
            Assert.Throws<BadJoinExpressionException>(
                () => TargetDb.Activity.All().Join(TargetDb.Activity, LocationId: TargetDb.Location.ID_Location));
        }

        class Activity
        {
            public int ID_Activity { get; set; }
        }
    }
}
