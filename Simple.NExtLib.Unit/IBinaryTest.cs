using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NExtLib.TestExtensions
{
    public interface IBinaryTest
    {
        void Run<T>(T expected, T actual);
    }
}
