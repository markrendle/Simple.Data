using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Azure
{
    public interface IRetryPolicyActor
    {
        bool ShouldRetry(Exception exc);
    }
}
