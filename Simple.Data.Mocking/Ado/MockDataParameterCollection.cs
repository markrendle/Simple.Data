using System;
using System.Collections.ObjectModel;
using System.Data;

namespace Simple.Data.Mocking.Ado
{
    class MockDataParameterCollection : Collection<IDataParameter>, IDataParameterCollection
    {
        public bool Contains(string parameterName)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(string parameterName)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(string parameterName)
        {
            throw new NotImplementedException();
        }

        public object this[string parameterName]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
