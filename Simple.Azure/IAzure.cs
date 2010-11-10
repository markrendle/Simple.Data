using System;
namespace Simple.Azure
{
    public interface IAzure
    {
        string Account { get; set; }
        string HashMD5(string source);
        string SharedKey { get; set; }
        System.Net.HttpWebRequest CreateTableRequest(string command, string method, string content = null, RequestContinuationToken requestContinuationToken = null);
        IRetryPolicy RetryPolicy {get;set;}
    }
}
