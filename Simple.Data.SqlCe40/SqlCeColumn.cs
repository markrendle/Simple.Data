namespace Shitty.Data.SqlCe40
{
    using System.Data;
    using Ado.Schema;

    public class SqlCeColumn : Column
    {
        private readonly SqlDbType _sqlDbType;

        public SqlCeColumn(string actualName, Table table) : base(actualName, table)
        {
        }

        public SqlCeColumn(string actualName, Table table, SqlDbType sqlDbType) : base(actualName, table)
        {
            _sqlDbType = sqlDbType;
        }

        public SqlCeColumn(string actualName, Table table, bool isIdentity) : base(actualName, table, isIdentity)
        {
        }

        public SqlCeColumn(string actualName, Table table, bool isIdentity, SqlDbType sqlDbType, int maxLength) : base(actualName, table, isIdentity, default(DbType), maxLength)
        {
            _sqlDbType = sqlDbType;
        }

        public SqlDbType SqlDbType
        {
            get { return _sqlDbType; }
        }


        public override bool IsBinary
        {
            get
            {
                return SqlDbType == SqlDbType.Binary || SqlDbType == SqlDbType.Image ||
                       SqlDbType == SqlDbType.VarBinary;
            }
        }
    }
}