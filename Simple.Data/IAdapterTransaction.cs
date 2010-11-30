using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public interface IAdapterTransaction : IDisposable
    {
        void Commit();
        void Rollback();
        string Name { get; }
    }
}
