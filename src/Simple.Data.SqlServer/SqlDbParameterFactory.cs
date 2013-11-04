namespace Simple.Data.SqlServer
﻿{
﻿    using System.ComponentModel.Composition;
﻿    using System.Data;
﻿    using System.Data.SqlClient;
﻿    using Ado;
﻿    using Ado.Schema;

﻿    [Export(typeof (IDbParameterFactory))]
﻿    public class SqlDbParameterFactory : IDbParameterFactory
﻿    {
﻿        public IDbDataParameter CreateParameter(string name)
﻿        {
﻿            return new SqlParameter
﻿                       {
﻿                           ParameterName = name
﻿                       };
﻿        }

﻿        public IDbDataParameter CreateParameter(string name, Column column)
﻿        {
﻿            var sqlColumn = (SqlColumn) column;
﻿            return new SqlParameter(name, sqlColumn.SqlDbType, sqlColumn.SqlDbType == SqlDbType.Char || sqlColumn.SqlDbType == SqlDbType.NChar ? 0 : column.MaxLength, column.ActualName);
﻿        }

﻿        public IDbDataParameter CreateParameter(string name, DbType dbType, int maxLength)
﻿        {
﻿            IDbDataParameter parameter = new SqlParameter
﻿                                             {
﻿                                                 ParameterName = name,
﻿                                                 Size = maxLength
﻿                                             };
﻿            parameter.DbType = dbType;
﻿            return parameter;
﻿        }
﻿    }
﻿}