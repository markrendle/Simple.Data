using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    internal abstract class Composer
    {
        private static Composer _composer = new MefHelper();

        public static Composer Default
        {
            get { return _composer; }
        }

        public abstract T Compose<T>();
        public abstract T Compose<T>(string contractName);

        internal static void SetDefault(Composer composer)
        {
            _composer = composer;
        }
    }
}
