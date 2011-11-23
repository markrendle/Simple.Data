using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public class ActionDisposable : IDisposable
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

        public void Dispose()
        {
            _action();
        }
    }
}
