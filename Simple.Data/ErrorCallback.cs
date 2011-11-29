using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data
{
    /// <summary>
    /// Represents a method to be called when an error occurs in a batch process.
    /// </summary>
    /// <param name="item">The data item which caused the error.</param>
    /// <param name="exception">The <see cref="Exception"/> that was thrown.</param>
    /// <returns>Return <c>true</c> to continue processing the batch; return <c>false</c> to abort.</returns>
    /// <remarks>If <c>false</c> is returned, the exception will be re-thrown from the original point in the code.</remarks>
    public delegate bool ErrorCallback(dynamic item, Exception exception);
}
