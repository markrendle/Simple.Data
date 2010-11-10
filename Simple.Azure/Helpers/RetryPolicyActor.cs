using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Azure.Helpers
{
    class CountingRetryPolicyActorFor<T> : IRetryPolicyActor where T:Exception
    {
        private readonly int initialCount = 0;
        private int countLeft = 0;

        public CountingRetryPolicyActorFor(int initialCount)
        {
            this.initialCount = initialCount;
            this.countLeft = initialCount;
        }

        public bool ShouldRetry(Exception exc)
        {
            if (false == exc.GetType().IsInstanceOfType(typeof(T)))
            {
                return false;
            }

            countLeft--;

            return countLeft > 0;
        }
    }
}
