namespace Simple.Data.SqlTest
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Ado.Metadata;
    using Xunit;

    public class MetadataTests
    {
        public MetadataTests()
        {
            DatabaseHelper.Reset();
        }

        private static AdoMetadata Get()
        {
            var db = DatabaseHelper.Open();
            return Database.GetMetadata(db) as AdoMetadata;
        }

        [Fact]
        public void GetsMetadata()
        {
            Assert.NotNull(Get());
        }

        [Fact]
        public async void GetsTables()
        {
            var metadata = Get();
            var tables = await metadata.GetTables();
            Assert.NotNull(tables);
            Assert.NotEqual(0, tables.Count);
        }

        [Fact]
        public async void IncludesUserTable()
        {
            var metadata = Get();
            var userTable =
                (await metadata.GetTables()).FirstOrDefault(
                    t => t.Schema.Equals("dbo", StringComparison.OrdinalIgnoreCase) && t.Name.Equals("Users", StringComparison.OrdinalIgnoreCase));
            Assert.NotNull(userTable);
        }

        [Fact]
        public async void GetsColumns()
        {
            var columns = await GetUserColumns();
            Assert.NotNull(columns);
            Assert.NotEqual(0, columns.Count);
        }

        [Fact]
        public async void ColumnsAreCorrect()
        {
            var columns = await GetUserColumns();

            var idColumn = columns.Single(c => c.Name.Equals("id", StringComparison.OrdinalIgnoreCase));
            Assert.Equal(DbType.Int32, idColumn.DbType);
            Assert.True(idColumn.IsIdentity);

            var nameColumn = columns.Single(c => c.Name.Equals("name", StringComparison.OrdinalIgnoreCase));
            Assert.Equal(DbType.String, nameColumn.DbType);
            Assert.Equal(100, nameColumn.MaxLength);

            var passwordColumn = columns.Single(c => c.Name.Equals("password", StringComparison.OrdinalIgnoreCase));
            Assert.Equal(DbType.String, passwordColumn.DbType);
            Assert.Equal(100, passwordColumn.MaxLength);

            var ageColumn = columns.Single(c => c.Name.Equals("age", StringComparison.OrdinalIgnoreCase));
            Assert.Equal(DbType.Int32, ageColumn.DbType);
            Assert.False(ageColumn.IsIdentity);
        }

        private static async Task<IList<Column>> GetUserColumns()
        {
            var metadata = Get();
            var userTable =
                (await metadata.GetTables()).FirstOrDefault(
                    t => t.Schema.Equals("dbo", StringComparison.OrdinalIgnoreCase) && t.Name.Equals("Users", StringComparison.OrdinalIgnoreCase));
            var columns = await metadata.GetColumns(userTable);
            return columns;
        }
    }
}