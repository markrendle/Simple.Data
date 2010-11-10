using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Globalization;
using System.Security.Cryptography;
using System.IO;

namespace Simple.Azure
{
    public class Azure : IAzure
    {
        string _account;
        byte[] _sharedKey;

        public string Account
        {
            get { return _account; }
            set { _account = value; }
        }

        public string SharedKey
        {
            get { return Convert.ToBase64String(_sharedKey); }
            set { _sharedKey = Convert.FromBase64String(value); }
        }

        public IRetryPolicy RetryPolicy
        {
            get;
            set;
        }

        public HttpWebRequest CreateTableRequest(string command, string method, string content = null, RequestContinuationToken requestContinuationToken = null)
        {
            var uri = new StringBuilder();
            uri.AppendFormat("http://{0}.table.core.windows.net/{1}", _account, command);
            if (null != requestContinuationToken)
            {
                var joiningCharacter = command.Contains('?') ? '&' : '?';
                uri.Append(requestContinuationToken.ToRequestUriFragment(joiningCharacter));
            }

            var request = WebRequest.Create(uri.ToString());
            request.Method = method;
            request.ContentLength = (content ?? string.Empty).Length;
            request.Headers.Add("x-ms-date", DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture));

            if (method == "PUT" || method == "DELETE" || method == "MERGE")
            {
                request.Headers.Add("If-Match", "*");
            }

            SignAndAuthorize(request);

            if (content != null)
            {
                AddContent(content, request);
            }

            return (HttpWebRequest)request;
        }

        private void AddContent(string content, WebRequest request)
        {
            request.Headers.Add("Content-MD5", HashMD5(content));
            request.ContentType = "application/atom+xml";
            request.SetContent(content);
        }

        private void SignAndAuthorize(WebRequest request)
        {
            var resource = request.RequestUri.PathAndQuery;
            if (resource.Contains("?"))
            {
                resource = resource.Substring(0, resource.IndexOf("?"));
            }

            string stringToSign = GetStringToSign(request, resource);

            var hasher = new HMACSHA256(_sharedKey);

            string signedSignature = Convert.ToBase64String(hasher.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));

            string authorizationHeader = string.Format("{0} {1}:{2}", "SharedKeyLite", _account, signedSignature);

            request.Headers.Add("Authorization", authorizationHeader);
        }

        public string HashMD5(string source)
        {
            var md5 = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(source));

            return md5.Select(b => b.ToString("x2")).Aggregate((agg, next) => agg + next);
        }

        private string GetStringToSign(WebRequest request, string resource)
        {
            string stringToSign = string.Format("{0}\n/{1}{2}",
                    request.Headers["x-ms-date"],
                    _account,
                    resource
                );
            return stringToSign;
        }
    }
}
