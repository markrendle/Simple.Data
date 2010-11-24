using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Simple.Data.Ado
{
    class ObservableDataReader : IObservable<IDictionary<string, object>>
    {
        private readonly IDbCommand _command;

        public ObservableDataReader(IDbCommand command)
        {
            _command = command;
        }

        /// <summary>
        /// Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <returns>
        /// The observer's interface that enables resources to be disposed.
        /// </returns>
        /// <param name="observer">The object that is to receive notifications.</param>
        public IDisposable Subscribe(IObserver<IDictionary<string, object>> observer)
        {
            return DataReaderObservableRunner.RunAsync(_command, observer);
        }
    }
}