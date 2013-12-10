using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    internal interface IDatabase
    {
        DynamicTable SetMemberAsTable(DynamicReference reference);
        DynamicSchema SetMemberAsSchema(DynamicReference reference);
    }
}
