namespace Simple.Data.IntegrationTest.Query
{
    using System;
    using Mocking.Ado;
    using NUnit.Framework;

    [TestFixture]
    public class WithTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(new[] { "dbo", "Employee", "BASE TABLE" },
                                     new[] { "dbo", "Department", "BASE TABLE" },
                                     new[] { "dbo", "Activity", "BASE TABLE" },
                                     new[] { "dbo", "Activity_Join", "BASE TABLE" },
                                     new[] { "dbo", "Location", "BASE_TABLE" });

            schemaProvider.SetColumns(new[] { "dbo", "Employee", "Id" },
                                      new[] { "dbo", "Employee", "Name" },
                                      new[] { "dbo", "Employee", "ManagerId" },
                                      new[] { "dbo", "Employee", "DepartmentId" },
                                      new[] { "dbo", "Department", "Id" },
                                      new[] { "dbo", "Department", "Name" },
                                      new[] { "dbo", "Activity", "ID_Activity" },
                                      new[] { "dbo", "Activity", "ID_Trip" },
                                      new[] { "dbo", "Activity", "Activity_Time" },
                                      new[] { "dbo", "Activity", "Is_Public" },
                                      new[] { "dbo", "Activity_Join", "ID_Activity" },
                                      new[] { "dbo", "Activity_Join", "ID_Location" },
                                      new[] { "dbo", "Location", "ID_Location" }
                                      );
            schemaProvider.SetPrimaryKeys(
                new object[] { "dbo", "Employee", "Id", 0 },
                new object[] { "dbo", "Department", "Id", 0 }
                );
            schemaProvider.SetForeignKeys(new object[] { "FK_Employee_Department", "dbo", "Employee", "DepartmentId", "dbo", "Department", "Id", 0 });
        }

        [Test]
        public void SingleWithClauseShouldUseJoin()
        {
            const string expectedSql = "select [dbo].[employee].[id] as [__with__employee__id],[dbo].[employee].[name] as [__with__employee__name],"+
                "[dbo].[employee].[managerid] as [__with__employee__managerid],[dbo].[employee].[departmentid] as [__with__employee__departmentid],"+
                "[dbo].[department].[id] as [__with__department__id],[dbo].[department].[name] as [__with__department__name]"+
                " from [dbo].[employee] left join [dbo].[department] on ([dbo].[department].[id] = [dbo].[employee].[departmentid])";

            var q = _db.Employees.All().WithDepartment();

            EatException(() => q.ToList());

            GeneratedSqlIs(expectedSql);
        }
    }
}