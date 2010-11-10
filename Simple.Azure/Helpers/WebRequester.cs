using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Simple.NExtLib.IO;

namespace Simple.Azure.Helpers
{
    // TODO - catch WebRequests only 
    // - then put the retry policy into place
    public class WebRequester
    {
        private readonly IRetryPolicy policy;

        public WebRequester(IRetryPolicy policy)
        {
            this.policy = policy;
        }

        public static string Request(HttpWebRequest request, IRetryPolicy policy)
        {
            return new WebRequester(policy).Request(request);
        }

        // TODO - not sure about this!
        public static IEnumerable<IDictionary<string, object>> Request(HttpWebRequest request, IRetryPolicy retryPolicy, out RequestContinuationToken nextRequestContinuationToken)
        {
            return new WebRequester(retryPolicy).RequestData(request, out nextRequestContinuationToken);
        }

        public IEnumerable<IDictionary<string, object>> RequestData(HttpWebRequest request, out RequestContinuationToken nextRequestContinuationToken)
        {
            using (var response = TryRequest(request))
            {
                // TODO - this section should go inside the WebRequester class too?
                System.Diagnostics.Trace.WriteLine(response.StatusCode, "HttpResponse");

                // parse the continuation token
                nextRequestContinuationToken = RequestContinuationToken.Parse(response.Headers);

                // TODO - this is a bit problematic...
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return Enumerable.Empty<IDictionary<string, object>>();
                }
                else
                {
                    return DataServicesHelper.GetData(response.GetResponseStream());
                }
            }
        }

        public IEnumerable<string> RequestStringList(HttpWebRequest request, out RequestContinuationToken nextRequestContinuationToken)
        {
            using (var response = TryRequest(request))
            {
                System.Diagnostics.Trace.WriteLine(response.StatusCode, "HttpResponse");
                nextRequestContinuationToken = RequestContinuationToken.Parse(request.Headers);
                return TableHelper.ReadTableList(response.GetResponseStream()).ToList();
            }
        }

        public string Request(HttpWebRequest request)
        {
            using (var response = TryRequest(request))
            {
                return TryGetResponseBody(response);
            }
        }

        public static HttpWebResponse TryRequest(HttpWebRequest request, IRetryPolicy policy)
        {
            return new WebRequester(policy).TryRequest(request);
        }

        private HttpWebResponse TryRequest(HttpWebRequest request)
        {
            try
            {
                // TODO - I really dislike policy being able to be null...
                // - just need to sit back and put the right things in the right places
                IRetryPolicyActor policyActor = null;
                if (null != this.policy)
                {
                    policyActor = policy.GetActor();
                }

                for ( ; ; ) // forever
                {
                    try
                    {
                        return (HttpWebResponse)request.GetResponse();
                    }
                    catch (Exception exc)
                    {
                        if (null == policyActor ||
                            false == policyActor.ShouldRetry(exc))
                        {
                            throw;
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                throw TableServiceException.CreateFromWebException(ex);
            }
        }

        private string TryGetResponseBody(HttpWebResponse response)
        {
            if (response != null)
            {
                var stream = response.GetResponseStream();
                if (stream != null)
                {
                    return QuickIO.StreamToString(stream);
                }
            }

            return string.Empty;
        }

    }
}
