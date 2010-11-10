using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Simple.Azure.Helpers
{
    class CountingRetryPolicy : IRetryPolicy
    {
        private readonly int tryCount = 0;

        public CountingRetryPolicy(int tryCount)
        {
            this.tryCount = tryCount;
        }

        public IRetryPolicyActor GetActor()
        {
            return new CountingRetryPolicyActorFor<WebException>(tryCount);
        }
    }
}
