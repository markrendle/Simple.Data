namespace Simple.Data.IntegrationTest
{
    using System;
    using Mocking.Ado;
    using NUnit.Framework;

    [TestFixture]
    public class GetCountTest : DatabaseIntegrationContext
    {
        protected override void SetSchema(MockSchemaProvider schemaProvider)
        {
            schemaProvider.SetTables(new[] {"dbo", "Users", "BASE TABLE"});

            schemaProvider.SetColumns(new object[] {"dbo", "Users", "Id", true},
                                      new[] {"dbo", "Users", "Name"},
                                      new[] {"dbo", "Users", "Password"},
                                      new[] {"dbo", "Users", "Age"});
        }
         
        [Test]
        public void GetCountBasic()
        {
            EatException<SimpleDataException>(() => TargetDb.Users.GetCount());
            GeneratedSqlIs("select count(*) from [dbo].[users]");
        }

        [Test]
        public void QueryCountBasic()
        {
            EatException<SimpleDataException>(() => TargetDb.Users.All().Count());
            GeneratedSqlIs("select count(*) from [dbo].[users]");
        }

        [Test]
        public void GetCountByBasic()
        {
            EatException<SimpleDataException>(() => TargetDb.Users.GetCountByAge(42));
            GeneratedSqlIs("select count(*) from [dbo].[users] where [dbo].[users].[age] = @p1");
        }

        [Test]
        public void GetCountByNamedParameters()
        {
            EatException<SimpleDataException>(() => TargetDb.Users.GetCountBy(Age: 42));
            GeneratedSqlIs("select count(*) from [dbo].[users] where [dbo].[users].[age] = @p1");
        }

        [Test]
        public void GetCountWithColumnThrowsException()
        {
            Assert.Throws<BadExpressionException>(() => TargetDb.Users.GetCount(TargetDb.Users.Id));
        }

        [Test]
        public void AssigningToColumnThrowsException()
        {
            Assert.Throws<BadExpressionException>(() => TargetDb.Users.GetCount(TargetDb.Users.Id = 1));
        }
        
        [Test]
        public void MultipleArgumentsThrowsException()
        {
            Assert.Throws<ArgumentException>(() => TargetDb.Users.GetCount(TargetDb.Users.Id == 1, TargetDb.Users.Name == "Bob"));
        }
    }
}