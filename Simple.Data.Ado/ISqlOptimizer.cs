using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shitty.Data.Ado
{
    using System.Data;

    public class CommandOptimizer
    {
        public virtual IDbCommand OptimizeFindOne(IDbCommand command)
        {
            return command;
        }
    }
}
