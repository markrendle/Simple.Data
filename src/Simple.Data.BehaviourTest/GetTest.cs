using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    [TestFixture]
    public class GetTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(new[] { "dbo", "Users", "BASE TABLE" },
                                     new[] { "dbo", "MyTable", "BASE TABLE" });
            schemaProvider.SetColumns(new object[] { "dbo", "Users", "Id", true },
                                      new[] { "dbo", "Users", "Name" },
                                      new[] { "dbo", "Users", "Password" },
                                      new[] { "dbo", "Users", "Age" },
                                      new[] { "dbo", "MyTable", "Column1"},
                                      new[] { "dbo", "MyTable", "Column2"});
            schemaProvider.SetPrimaryKeys(
                new object[] { "dbo", "Users", "Id", 0 },
                new object[] { "dbo", "MyTable", "Column1", 0 },
                new object[] { "dbo", "MyTable", "Column2", 1 });
        }

        private const string UsersColumns = "[dbo].[Users].[Id], [dbo].[Users].[Name], [dbo].[Users].[Password], [dbo].[Users].[Age]";

        [Test]
        public void TestGetWithSingleColumn()
        {
            _db.Users.Get(1);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[users] where [id] = @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestSchemaQualifiedGetWithSingleColumn()
        {
            _db.dbo.Users.Get(1);
            GeneratedSqlIs("select " + UsersColumns + " from [dbo].[users] where [id] = @p1");
            Parameter(0).Is(1);
        }

        [Test]
        public void TestGetWithTwoColumns()
        {
            _db.MyTable.Get(1,2);
            GeneratedSqlIs("select [dbo].[MyTable].[Column1], [dbo].[MyTable].[Column2] from [dbo].[MyTable] where [Column1] = @p1 and [Column2] = @p2");
            Parameter(0).Is(1);
            Parameter(1).Is(2);
        }
    }
}
