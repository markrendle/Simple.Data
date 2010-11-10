using System;
namespace Simple.Azure
{
    public interface IRetryPolicy
    {
        IRetryPolicyActor GetActor();
    }
}
