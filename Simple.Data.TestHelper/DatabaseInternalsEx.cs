using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shitty.Data.TestHelper
{
    public static class DatabaseInternalsEx
    {
        public static Adapter GetAdapter(this Database database)
        {
            return database.GetAdapter();
        }
    }
}
