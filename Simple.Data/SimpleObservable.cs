using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    public static class ColdObservable
    {
        public static IObservable<T> Create<T>(Func<IObserver<T>, IDisposable> func)
        {
            return new ColdObservable<T>(func);
        }

        public static IObservable<T> ToObservable<T>(this IEnumerable<T> source)
        {
            return Create<T>(o =>
                                 {
                                     try
                                     {
                                         foreach (var item in source)
                                         {
                                             o.OnNext(item);
                                         }
                                         o.OnCompleted();
                                     }
                                     catch (Exception ex)
                                     {
                                         o.OnError(ex);
                                     }
                                     return EmptyDisposable;
                                 });
        }

        public static readonly IDisposable EmptyDisposable = new _EmptyDisposable();

        private class _EmptyDisposable : IDisposable
        {
            public void Dispose()
            {
                
            }
        }
    }

    internal class ColdObservable<T> : IObservable<T>
    {
        private readonly Func<IObserver<T>, IDisposable> _func;

        public ColdObservable(Func<IObserver<T>, IDisposable> func)
        {
            _func = func;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _func(observer);
        }
    }

    static class MapObservable
    {
        public static IObservable<TOut> Map<TIn,TOut>(this IObservable<TIn> source, Func<TIn,TOut> mapFunc)
        {
            return new MapObservable<TIn, TOut>(source, mapFunc);
        }
    }

    class MapObservable<TIn, TOut> : IObservable<TOut>
    {
        private readonly IObservable<TIn> _source;
        private readonly Func<TIn, TOut> _map;

        public MapObservable(IObservable<TIn> source, Func<TIn,TOut> map)
        {
            _source = source;
            _map = map;
        }

        public IDisposable Subscribe(IObserver<TOut> observer)
        {
            return _source.Subscribe(new MapObserver(observer, _map));
        }

        private class MapObserver : IObserver<TIn>
        {
            private readonly IObserver<TOut> _observer;
            private readonly Func<TIn, TOut> _map; 

            public MapObserver(IObserver<TOut> observer, Func<TIn, TOut> map)
            {
                _observer = observer;
                _map = map;
            }

            public void OnNext(TIn value)
            {
                _observer.OnNext(_map(value));
            }

            public void OnError(Exception error)
            {
                _observer.OnError(error);
            }

            public void OnCompleted()
            {
                _observer.OnCompleted();
            }
        }
    }
}
