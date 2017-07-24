using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shitty.Data
{
    public sealed class ActionDisposable : IDisposable
    {
        public static readonly IDisposable NoOp = new ActionDisposable();
        private readonly Action _action;

        public ActionDisposable() : this(null)
        {
        }

        public ActionDisposable(Action action)
        {
            _action = action ?? (() => { });
        }

        ~ActionDisposable()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            _action();
        }
    }
}
