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

            schemaProvider.SetPrimaryKeys(new object[] {"dbo", "Employee", "Id", 0},
                                          new object[] {"dbo", "Department", "Id", 0});

            schemaProvider.SetForeignKeys(new object[] { "FK_Employee_Department", "dbo", "Employee", "DepartmentId", "dbo", "Department", "Id", 0 });
        }

        [Test]
        public void SingleWithClauseUsingMagicMethodShouldUseWith1Join()
        {
            const string expectedSql = "select [dbo].[employee].[id],[dbo].[employee].[name],"+
                "[dbo].[employee].[managerid],[dbo].[employee].[departmentid],"+
                "[dbo].[department].[id] as [__with1__department__id],[dbo].[department].[name] as [__with1__department__name]"+
                " from [dbo].[employee] left join [dbo].[department] on ([dbo].[department].[id] = [dbo].[employee].[departmentid])";

            var q = _db.Employees.All().WithDepartment();

            EatException(() => q.ToList());

            GeneratedSqlIs(expectedSql);
        }

        [Test]
        public void SingleWithClauseUsingMagicMethodShouldUseWithNJoin()
        {
            const string expectedSql = "select " +
                "[dbo].[department].[id],[dbo].[department].[name],"+
                "[dbo].[employee].[id] as [__withn__employees__id],[dbo].[employee].[name] as [__withn__employees__name],"+
                "[dbo].[employee].[managerid] as [__withn__employees__managerid],[dbo].[employee].[departmentid] as [__withn__employees__departmentid]"+
                " from [dbo].[department] left join [dbo].[employee] on ([dbo].[department].[id] = [dbo].[employee].[departmentid])";

            var q = _db.Departments.All().WithEmployees();

            EatException(() => q.ToList());

            GeneratedSqlIs(expectedSql);
        }

        [Test]
        public void SingleWithClauseUsingReferenceShouldUseJoin()
        {
            const string expectedSql = "select [dbo].[employee].[id],[dbo].[employee].[name]," +
                "[dbo].[employee].[managerid],[dbo].[employee].[departmentid]," +
                "[dbo].[department].[id] as [__with1__department__id],[dbo].[department].[name] as [__with1__department__name]" +
                " from [dbo].[employee] left join [dbo].[department] on ([dbo].[department].[id] = [dbo].[employee].[departmentid])";

            var q = _db.Employees.All().With(_db.Employees.Department);

            EatException(() => q.ToList());

            GeneratedSqlIs(expectedSql);
        }

        [Test]
        public void SingleWithOneClauseUsingReferenceShouldUseJoinAndForceOne()
        {
            const string expectedSql = "select " +
                "[dbo].[department].[id],[dbo].[department].[name]," +
                "[dbo].[employee].[id] as [__with1__employee__id],[dbo].[employee].[name] as [__with1__employee__name]," +
                "[dbo].[employee].[managerid] as [__with1__employee__managerid],[dbo].[employee].[departmentid] as [__with1__employee__departmentid]" +
                " from [dbo].[department] left join [dbo].[employee] on ([dbo].[department].[id] = [dbo].[employee].[departmentid])";

            var q = _db.Departments.All().WithOne(_db.Departments.Employee);

            EatException(() => q.ToList());

            GeneratedSqlIs(expectedSql);
        }

        [Test]
        public void SingleWithClauseUsingReferenceWithAliasShouldApplyAliasToSql()
        {
            const string expectedSql = "select [dbo].[employee].[id],[dbo].[employee].[name]," +
                "[dbo].[employee].[managerid],[dbo].[employee].[departmentid]," +
                "[foo].[id] as [__with1__foo__id],[foo].[name] as [__with1__foo__name]" +
                " from [dbo].[employee] left join [dbo].[department] [foo] on ([foo].[id] = [dbo].[employee].[departmentid])";

            var q = _db.Employees.All().With(_db.Employees.Department.As("Foo"));

            EatException(() => q.ToList());

            GeneratedSqlIs(expectedSql);
        }

        [Test]
        public void SingleWithClauseUsingExplicitJoinShouldApplyAliasToSql()
        {
            const string expectedSql = "select [dbo].[employee].[id],[dbo].[employee].[name]," +
                "[dbo].[employee].[managerid],[dbo].[employee].[departmentid]," +
                "[manager].[id] as [__withn__manager__id],[manager].[name] as [__withn__manager__name]," +
                "[manager].[managerid] as [__withn__manager__managerid],[manager].[departmentid] as [__withn__manager__departmentid]" +
                " from [dbo].[employee] left join [dbo].[employee] [manager] on ([manager].[id] = [dbo].[employee].[managerid])";

            dynamic manager;
            var q = _db.Employees.All()
                .OuterJoin(_db.Employees.As("Manager"), out manager).On(Id: _db.Employees.ManagerId)
                .With(manager);

            EatException(() => q.ToList());

            GeneratedSqlIs(expectedSql);
        }

        [Test]
        public void SingleWithOneClauseUsingExplicitJoinShouldApplyAliasToSql()
        {
            const string expectedSql = "select [dbo].[employee].[id],[dbo].[employee].[name]," +
                "[dbo].[employee].[managerid],[dbo].[employee].[departmentid]," +
                "[manager].[id] as [__with1__manager__id],[manager].[name] as [__with1__manager__name]," +
                "[manager].[managerid] as [__with1__manager__managerid],[manager].[departmentid] as [__with1__manager__departmentid]" +
                " from [dbo].[employee] left join [dbo].[employee] [manager] on ([manager].[id] = [dbo].[employee].[managerid])";

            dynamic manager;
            var q = _db.Employees.All()
                .OuterJoin(_db.Employees.As("Manager"), out manager).On(Id: _db.Employees.ManagerId)
                .WithOne(manager);

            EatException(() => q.ToList());

            GeneratedSqlIs(expectedSql);
        }

        [Test]
        public void MultipleWithClauseJustDoesEverythingYouWouldHope()
        {
            const string expectedSql = "select [dbo].[employee].[id],[dbo].[employee].[name]," +
                "[dbo].[employee].[managerid],[dbo].[employee].[departmentid]," +
                "[manager].[id] as [__withn__manager__id],[manager].[name] as [__withn__manager__name]," +
                "[manager].[managerid] as [__withn__manager__managerid],[manager].[departmentid] as [__withn__manager__departmentid]," +
                "[dbo].[department].[id] as [__with1__department__id],[dbo].[department].[name] as [__with1__department__name]" +
                " from [dbo].[employee] left join [dbo].[employee] [manager] on ([manager].[id] = [dbo].[employee].[managerid])" +
                " left join [dbo].[department] on ([dbo].[department].[id] = [dbo].[employee].[departmentid])";

            dynamic manager;
            var q = _db.Employees.All()
                .OuterJoin(_db.Employees.As("Manager"), out manager).On(Id: _db.Employees.ManagerId)
                .With(manager)
                .WithDepartment();

            EatException(() => q.ToList());

            GeneratedSqlIs(expectedSql);
        }

        /// <summary>
        /// Test for issue #157
        /// </summary>
        [Test]
        public void CriteriaReferencesShouldNotBeDuplicatedInSql()
        {
            const string expectedSql = "select [dbo].[employee].[id],[dbo].[employee].[name]," +
                "[dbo].[employee].[managerid],[dbo].[employee].[departmentid]," +
                "[dbo].[department].[id] as [__with1__department__id],[dbo].[department].[name] as [__with1__department__name]" +
                " from [dbo].[employee] join [dbo].[department] on ([dbo].[department].[id] = [dbo].[employee].[departmentid])" +
                " where [dbo].[department].[name] = @p1";

            var q = _db.Employees.FindAll(_db.Employees.Department.Name == "Dev").WithDepartment();
            EatException(() => q.ToList());

            GeneratedSqlIs(expectedSql);
        }
    }
}