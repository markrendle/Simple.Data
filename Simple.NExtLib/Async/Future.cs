using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple.NExtLib.Async;

namespace Simple.NExtLib.Async
{
    public static class Future
    {
        /// <summary>
        /// Creates an instance of <see cref="Future`1"/>.
        /// </summary>
        /// <typeparam name="T">The type of value represented by the Future.</typeparam>
        /// <param name="func">The <see cref="Func`1"/> delegate that will return the value.</param>
        /// <returns>A new instance of <see cref="Future`1"/>.</returns>
        /// <remarks>This method exists only to provide type-inference convenience.</remarks>
        public static Future<T> Create<T>(Func<T> func)
        {
            return new Future<T>(func);
        }
    }
}
