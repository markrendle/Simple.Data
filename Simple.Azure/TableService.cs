using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Simple.Azure.Helpers;
using System.Net;

namespace Simple.Azure
{
    public class TableService
    {
        private IAzure _azure;

        public TableService(IAzure azure)
        {
            this._azure = azure;
        }

        public IEnumerable<string> ListFirstTables()
        {
            RequestContinuationToken ignored;
            return ListNextTables(null, out ignored);
        }

        /// <summary>
        /// Warning! Calling this method is not advised if you have a lot of tables.
        /// It can fill up your RAM
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ListAllTables()
        {
            List<string> toReturn = new List<string>();
            RequestContinuationToken currentContinuationToken = null;
            RequestContinuationToken nextContinuationToken = null;

            do
            {
                currentContinuationToken = nextContinuationToken;
                nextContinuationToken = null;

                toReturn.AddRange(ListNextTables(currentContinuationToken, out nextContinuationToken));
            } while (nextContinuationToken != null);

            return toReturn;
        }

        public IEnumerable<string> ListNextTables(RequestContinuationToken requestContinuationToken, out RequestContinuationToken nextRequestContinuationToken)
        {
            var request = _azure.CreateTableRequest("tables", RestVerbs.GET, requestContinuationToken:requestContinuationToken);

            return new WebRequester(_azure.RetryPolicy).RequestStringList(request, out nextRequestContinuationToken);
        }

        public void CreateTable(string tableName)
        {
            var dict = new Dictionary<string, object> { { "TableName", tableName } };
            var data = DataServicesHelper.CreateDataElement(dict);

            DoRequest(data, "tables", RestVerbs.POST);
        }

        private void DoRequest(XElement element, string command, string method)
        {
            var request = _azure.CreateTableRequest(command, method, element.ToString());

            new WebRequester(_azure.RetryPolicy).Request(request);
        }
    }
}
