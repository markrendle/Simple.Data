using System;

namespace Simple.NExtLib.Async
{
    public class Future<T>
    {
        private readonly Action _action;
        private readonly IAsyncResult _asyncResult;
        private readonly object _lock = new object();
        private bool _ended;
        private T _value;
        private Exception _error;

        public Future(Func<T> func)
        {
            if (func == null) throw new ArgumentNullException("func");

            _action = () => Run(func);
            _asyncResult = _action.BeginInvoke(null, null);
        }

        public T Value
        {
            get
            {
                JoinAction();

                if (_error != null) throw new AsyncException(Properties.Resources.Async_AsyncExceptionMessage, _error);
                return _value;
            }
        }

        private void Run(Func<T> func)
        {
            try
            {
                _value = func();
            }
            catch (Exception ex)
            {
                if (ex is SystemException) throw;

                _error = ex;
                _value = default(T);
            }
        }

        private void JoinAction()
        {
            if (!_ended)
            {
                lock (_lock)
                {
                    if (!_ended)
                    {
                        _action.EndInvoke(_asyncResult);
                        _ended = true;
                    }
                }
            }
        }
    }
}
