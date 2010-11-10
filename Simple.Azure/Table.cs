using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Linq;
using System.Diagnostics;
using Simple.Azure.Helpers;
using Simple.NExtLib.Xml;
using Simple.NExtLib.IO;
using System.Net.Cache;

namespace Simple.Azure
{
    /// <summary>
    /// Represents an Azure table and provides CRUD operations against it.
    /// </summary>
    public class Table
    {
        // ReSharper disable InconsistentNaming
        private const string GET = "GET";
        private const string POST = "POST";
        private const string PUT = "PUT";
        private const string MERGE = "MERGE";
        private const string DELETE = "DELETE";
        // ReSharper restore InconsistentNaming

        private readonly string _tableName;
        private readonly bool _autoCreate;
        private readonly IAzure _azure;
        private readonly TableService _tableService;

        public Table(IAzure azure, string tableName) : this(azure, tableName, IfTableDoesNotExist.ThrowAnException) { }

        public Table(IAzure azure, string tableName, IfTableDoesNotExist doesNotExistAction)
        {
            _tableName = tableName;
            _autoCreate = doesNotExistAction == IfTableDoesNotExist.CreateIt;
            _azure = azure;
            _tableService = new TableService(_azure);
        }

        public IEnumerable<IDictionary<string, object>> GetFirstBlockOfRows()
        {
            RequestContinuationToken ignoredNextToken;
            return GetBlockOfRows(null, out ignoredNextToken);
        }

        /// <summary>
        /// WARNING - CALLING THIS CAN FILL UP YOUR MEMORY LEVEL!
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IDictionary<string, object>> GetAllRows()
        {
            RequestContinuationToken currentContinuationToken = null;
            RequestContinuationToken nextContinuationToken = null;
            List<IDictionary<string, object>> toReturn = new List<IDictionary<string, object>>();
            do
            {
                currentContinuationToken = nextContinuationToken;
                nextContinuationToken = null;
                toReturn.AddRange(GetBlockOfRows(currentContinuationToken, out nextContinuationToken));
            } while (nextContinuationToken != null);

            return toReturn;
        }

        public IEnumerable<IDictionary<string, object>> GetBlockOfRows(RequestContinuationToken requestContinuationToken, out RequestContinuationToken nextRequestContinuationToken)
        {
            return Get(_tableName, requestContinuationToken, out nextRequestContinuationToken);
        }

        public IDictionary<string, object> Get(string partitionKey, string rowKey)
        {
            RequestContinuationToken ignoredContinuationToken;
            return Get(partitionKey, rowKey, null, out ignoredContinuationToken);
        }

        public IDictionary<string, object> Get(string partitionKey, string rowKey, RequestContinuationToken requestContinuationToken, out RequestContinuationToken nextRequestContinuationToken)
        {
            return Get(BuildEntityUri(_tableName, partitionKey, rowKey), requestContinuationToken, out nextRequestContinuationToken).SingleOrDefault();
        }

        public void Delete(IDictionary<string, object> row)
        {
            ThrowIfMissing(row, "PartitionKey", "RowKey");

            Delete(row["PartitionKey"].ToString(), row["RowKey"].ToString());
        }

        public void Delete(string partitionKey, string rowKey)
        {
            Delete(BuildEntityUri(_tableName, partitionKey, rowKey));
        }

        // TODO: Implement querying using LINQ, IQueryable<IDictionary<string, object>>
        public IEnumerable<IDictionary<string, object>> QueryFirstBlock(string filter)
        {
            RequestContinuationToken ignoredContinuationToken;
            return QueryNextBlock(filter, null, out ignoredContinuationToken);
        }

        /// <summary>
        /// WARNING - CALLING THIS MAY FILL UP YOUR RAM!
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public IEnumerable<IDictionary<string, object>> QueryAll(string filter)
        {
            RequestContinuationToken currentContinuationToken = null;
            RequestContinuationToken nextContinuationToken = null;
            List<IDictionary<string, object>> toReturn = new List<IDictionary<string, object>>();

            do
            {
                currentContinuationToken = nextContinuationToken;
                nextContinuationToken = null;
                toReturn.AddRange(QueryNextBlock(filter, currentContinuationToken, out nextContinuationToken));
            } while (null != nextContinuationToken);

            return toReturn;
        }

        public IEnumerable<IDictionary<string, object>> QueryNextBlock(string filter, RequestContinuationToken requestContinuationToken, out RequestContinuationToken nextRequestContinuationToken)
        {
            return Get(_tableName + "?$filter=" + HttpUtility.UrlEncode(filter), requestContinuationToken, out nextRequestContinuationToken);
        }

        public IDictionary<string, object> InsertRow(IDictionary<string, object> row)
        {
            ThrowIfMissing(row, "PartitionKey", "RowKey");

            var entry = DataServicesHelper.CreateDataElement(row);
            var request = _azure.CreateTableRequest(_tableName, POST, entry.ToString());

            string text = string.Empty;

            if (_autoCreate)
            {
                try
                {
                    text = WebRequester.Request(request, _azure.RetryPolicy);
                }
                catch (TableServiceException ex)
                {
                    if (ex.Code == "TableNotFound")
                    {
                        Trace.WriteLine("Auto-creating table");
                        _tableService.CreateTable(_tableName);
                        request = _azure.CreateTableRequest(_tableName, POST, entry.ToString());

                        text = WebRequester.Request(request,_azure.RetryPolicy);
                    }
                }
            }
            else
            {
                text = WebRequester.Request(request, _azure.RetryPolicy);
            }

            return DataServicesHelper.GetData(text).First();
        }

        public void UpdateRow(IDictionary<string, object> row)
        {
            ThrowIfMissing(row, "PartitionKey", "RowKey");

            var dict = row.ToDictionary();
            var command = BuildEntityUri(_tableName, dict["PartitionKey"].ToString(), dict["RowKey"].ToString());

            WriteRowDataRequest(row, command, PUT);
        }

        public void MergeRow(string partitionKey, string rowKey, IDictionary<string, object> row)
        {
            ValidateKeyValue("partitionKey", partitionKey);
            ValidateKeyValue("rowKey", rowKey);

            var dict = row.ToDictionary();
            var command = BuildEntityUri(_tableName, partitionKey, rowKey);

            WriteRowDataRequest(row, command, MERGE);
        }

        public static IDictionary<string, object> NewRow(string partitionKey, string rowKey)
        {
            ValidateKeyValue("partitionKey", partitionKey);
            ValidateKeyValue("rowKey", rowKey);

            return new Dictionary<string, object>
            {
                { "PartitionKey", partitionKey },
                { "RowKey", rowKey }
            };
        }

        private static void ValidateKeyValue(string keyName, string value)
        {
            if (value == null) throw new ArgumentNullException(keyName);
            if (value.Contains("/")) throw new ArgumentException("Key values may not contain forward slashes", keyName);
            if (value.Contains("\\")) throw new ArgumentException("Key values may not contain backslashes", keyName);
            if (value.Contains("#")) throw new ArgumentException("Key values may not contain the hash/pound symbol", keyName);
            if (value.Contains("?")) throw new ArgumentException("Key values may not contain question marks", keyName);
        }

        private IEnumerable<IDictionary<string, object>> Get(string url, RequestContinuationToken requestContinuationToken, out RequestContinuationToken nextRequestContinuationToken)
        {
            var request = _azure.CreateTableRequest(url, GET, requestContinuationToken: requestContinuationToken);

            return WebRequester.Request(request, _azure.RetryPolicy, out nextRequestContinuationToken);
        }

        private void Delete(string url)
        {
            var request = _azure.CreateTableRequest(url, DELETE);

            // TODO - can I check this behaviour - does it really not matter what the content of the reponse is?
            using (WebRequester.TryRequest(request, _azure.RetryPolicy))
            {
                // No action, just disposing the response
            }
        }

        private void WriteRowDataRequest(IDictionary<string, object> row, string command, string verb)
        {
            var entry = DataServicesHelper.CreateDataElement(row);
            entry.Element(null, "id").Value = "http://" + _azure.Account + ".table.core.windows.net/" + command;
            var request = _azure.CreateTableRequest(command, verb, entry.ToString());
            request.Headers.Add(HttpRequestHeader.IfMatch, "*");
            request.Headers.Add("x-ms-version", "2009-09-19");

            // TODO - can I check this behaviour - does it really not matter what the content of the reponse is?
            using (WebRequester.TryRequest(request, _azure.RetryPolicy))
            {
                // No action, just disposing the response
            }
        }

        private static void ThrowIfMissing(IDictionary<string, object> row, params string[] keys)
        {
            foreach (var key in keys)
            {
                if ((!row.ContainsKey(key)) || row[key] == null)
                {
                    throw new DataException("No or null " + key + "specified.");
                }
            }
        }

        private static string BuildEntityUri(string tableName, string partitionKey, string rowKey)
        {
            return string.Format(@"{0}(PartitionKey='{1}',RowKey='{2}')", tableName, partitionKey, rowKey);
        }
    }
}
