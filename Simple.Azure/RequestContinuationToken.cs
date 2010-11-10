using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;

namespace Simple.Azure
{
    public class RequestContinuationToken
    {
        public string NextPartitionKey { get; set; }
        public string NextRowKey { get; set; }
        public string NextTableName {get;set;}

        private RequestContinuationToken()
        {
            NextPartitionKey = null;
            NextRowKey = null;
            NextTableName = null; 
        }

        public static RequestContinuationToken Parse(NameValueCollection headers)
        {
            var candidate = new RequestContinuationToken()
            {
                NextPartitionKey = headers["x-ms-continuation-NextPartitionKey"],
                NextRowKey = headers["x-ms-continuation-NextRowKey"],
                NextTableName = headers["x-ms-continuation-NextTable"]
            };

            if (false == candidate.IsEmpty())
            {
                return candidate;
            }

            return null;
        }

        private bool IsEmpty()
        {
            return string.IsNullOrEmpty(NextPartitionKey)
                && string.IsNullOrEmpty(NextRowKey)
                && string.IsNullOrEmpty(NextTableName);
        }

        public string ToRequestUriFragment(char initialCharacter)
        {
            StringBuilder toReturn = new StringBuilder();
            var joiningCharacter = initialCharacter;

            if (false == string.IsNullOrEmpty(NextPartitionKey))
            {
                toReturn.AppendFormat("{0}NextPartitionKey={1}", joiningCharacter, NextPartitionKey);
                joiningCharacter = '&';
            }
            if (false == string.IsNullOrEmpty(NextRowKey))
            {
                toReturn.AppendFormat("{0}NextRowKey={1}", joiningCharacter, NextRowKey);
                joiningCharacter = '&';
            }
            if (false == string.IsNullOrEmpty(NextTableName))
            {
                toReturn.AppendFormat("{0}NextTableName={1}", joiningCharacter, NextTableName);
                joiningCharacter = '&';
            }

            return toReturn.ToString();
        }
    }
}
