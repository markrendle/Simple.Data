namespace Simple.Data.IntegrationTest.Query
{
    using Mocking.Ado;
    using NUnit.Framework;

    [TestFixture]
    public class WithTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
// ReSharper disable CoVariantArrayConversion
            schemaProvider.SetTables(new[] { "dbo", "Employee", "BASE TABLE" },
                                     new[] { "dbo", "Department", "BASE TABLE" },
                                     new[] { "dbo", "Activity", "BASE TABLE" },
                                     new[] { "dbo", "Customer", "BASE TABLE" },
                                     new[] { "dbo", "Order", "BASE TABLE" },
                                     new[] { "dbo", "Note", "BASE TABLE" },
                                     new[] { "dbo", "Activity_Join", "BASE TABLE" },
                                     new[] { "dbo", "Location", "BASE_TABLE" });

            schemaProvider.SetColumns(new[] { "dbo", "Employee", "Id" },
                                      new[] { "dbo", "Employee", "Name" },
                                      new[] { "dbo", "Employee", "ManagerId" },
                                      new[] { "dbo", "Employee", "DepartmentId" },
                                      new[] { "dbo", "Department", "Id" },
                                      new[] { "dbo", "Department", "Name" },
                                      new[] { "dbo", "Customer", "Id" },
                                      new[] { "dbo", "Customer", "Name" },
                                      new[] { "dbo", "Order", "Id" },
                                      new[] { "dbo", "Order", "CustomerId" },
                                      new[] { "dbo", "Order", "Description" },
                                      new[] { "dbo", "Note", "Id" },
                                      new[] { "dbo", "Note", "CustomerId" },
                                      new[] { "dbo", "Note", "Text" },
                                      new[] { "dbo", "Activity", "ID" },
                                      new[] { "dbo", "Activity", "Description" },
                                      new[] { "dbo", "Activity_Join", "ID_Activity" },
                                      new[] { "dbo", "Activity_Join", "ID_Location" },
                                      new[] { "dbo", "Location", "ID" },
                                      new[] { "dbo", "Location", "Address" }
                                      );

            schemaProvider.SetPrimaryKeys(new object[] {"dbo", "Employee", "Id", 0},
                                          new object[] {"dbo", "Department", "Id", 0},
                                          new object[] {"dbo", "Customer", "Id", 0},
                                          new object[] {"dbo", "Order", "Id", 0},
                                          new object[] {"dbo", "Note", "Id", 0}
                                          );

            schemaProvider.SetForeignKeys(
                new object[] { "FK_Employee_Department", "dbo", "Employee", "DepartmentId", "dbo", "Department", "Id", 0 },
                new object[] { "FK_Activity_Join_Activity", "dbo", "Activity_Join", "ID_Activity", "dbo", "Activity", "ID", 0 },
                new object[] { "FK_Activity_Join_Location", "dbo", "Activity_Join", "ID_Location", "dbo", "Location", "ID", 0 },
                new object[] { "FK_Order_Customer", "dbo", "Order", "CustomerId", "dbo", "Customer", "Id", 0 },
                new object[] { "FK_Note_Customer", "dbo", "Note", "CustomerId", "dbo", "Customer", "Id", 0 }
                );
// ReSharper restore CoVariantArrayConversion
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
        public void SingleWithClauseUsingTwoStepReference()
        {
            const string expectedSql = "select "+
                "[dbo].[activity].[id],"+
                "[dbo].[activity].[description],"+
                "[dbo].[location].[id] as [__withn__location__id]," +
                "[dbo].[location].[address] as [__withn__location__address]" +
                " from [dbo].[activity] "+
                "left join [dbo].[activity_join] on ([dbo].[activity].[id] = [dbo].[activity_join].[id_activity]) "+
                "left join [dbo].[location] on ([dbo].[location].[id] = [dbo].[activity_join].[id_location])";

            var q = _db.Activity.All().With(_db.Activity.ActivityJoin.Location);

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
        /// Test for multiple child tables...
        /// </summary>
        [Test]
        public void CustomerWithOrdersAndNotes()
        {
            const string expectedSql = "select [dbo].[customer].[id],[dbo].[customer].[name]," +
                                       "[dbo].[order].[id] as [__withn__orders__id],[dbo].[order].[customerid] as [__withn__orders__customerid],[dbo].[order].[description] as [__withn__orders__description]," +
                                       "[dbo].[note].[id] as [__withn__notes__id],[dbo].[note].[customerid] as [__withn__notes__customerid],[dbo].[note].[text] as [__withn__notes__text]" +
                                       " from [dbo].[customer]" +
                                       " left join [dbo].[order] on ([dbo].[customer].[id] = [dbo].[order].[customerid])" +
                                       " left join [dbo].[note] on ([dbo].[customer].[id] = [dbo].[note].[customerid])";

            var q = _db.Customers.All().WithOrders().WithNotes();
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
        
        /// <summary>
        /// Test for issue #184
        /// </summary>
        [Test]
        public void CriteriaReferencesShouldUseWithAliasOutValue()
        {
            const string expectedSql = "select [dbo].[employee].[id],[dbo].[employee].[name]," +
                "[dbo].[employee].[managerid],[dbo].[employee].[departmentid]," +
                "[foo].[id] as [__with1__foo__id],[foo].[name] as [__with1__foo__name]" +
                " from [dbo].[employee] join [dbo].[department] [foo] on ([foo].[id] = [dbo].[employee].[departmentid])" +
                " where ([dbo].[employee].[name] like @p1" +
                " and [foo].[name] = @p2)";

            dynamic foo;
            var q = _db.Employees.Query()
                .Where(_db.Employees.Name.Like("A%"))
                .WithOne(_db.Employees.Department.As("Foo"), out foo)
                .Where(foo.Name == "Admin");
            EatException(() => q.ToList());

            GeneratedSqlIs(expectedSql);
        }

        /// <summary>
        /// Test for issue #184
        /// </summary>
        [Test]
        public void CriteriaReferencesShouldUseSeparateJoinFromWithAlias()
        {
            const string expectedSql = "select [dbo].[employee].[id],[dbo].[employee].[name]," +
                "[dbo].[employee].[managerid],[dbo].[employee].[departmentid]," +
                "[foo].[id] as [__with1__foo__id],[foo].[name] as [__with1__foo__name]" +
                " from [dbo].[employee]" +
                " join [dbo].[department] on ([dbo].[department].[id] = [dbo].[employee].[departmentid])" +
                " left join [dbo].[department] [foo] on ([foo].[id] = [dbo].[employee].[departmentid])" +
                " where ([dbo].[employee].[name] like @p1" +
                " and [dbo].[department].[name] = @p2)";

            var q = _db.Employees.Query()
                .Where(_db.Employees.Name.Like("A%"))
                .WithOne(_db.Employees.Department.As("Foo"))
                .Where(_db.Employees.Department.Name == "Admin");
            EatException(() => q.ToList());

            GeneratedSqlIs(expectedSql);
        }
    }
}