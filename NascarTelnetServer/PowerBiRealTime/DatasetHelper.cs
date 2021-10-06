using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using PowerBiRealTime.PowerBi;

namespace PowerBiRealTime
{
    public class DatasetHelper
    {
        private readonly string _datasetsUri;
        private readonly string _datasetName;
        private readonly ITokenService _tokenService;
        private string _datasetId;

        public string AccessToken => _tokenService.Get();

        public DatasetHelper(string datasetsUri, string datasetName, ITokenService tokenService)
        {
            _datasetsUri = datasetsUri;
            _datasetName = datasetName;
            _tokenService = tokenService;
        }

        public void CreateDataset<T>() where T : new()
        {
            try
            {
                var request = DatasetRequest($"{_datasetsUri}/datasets", "POST", AccessToken);
                var ds = GetDatasets().value.GetDataset(_datasetName);
                if (ds != null)
                {
                    _datasetId = ds.Id;
                }
                else
                {
                    PostRequest(request, new T().ToDatasetJson(_datasetName));
                    SetDatasetId();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Create: {ex.Message}");
            }
        }

        public void DeleteDataset()
        {
            try
            {
                var request = DatasetRequest($"{_datasetsUri}/datasets/{_datasetName}", "DELETE", AccessToken);
                Console.WriteLine($"deleted {_datasetName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public Datasets GetDatasets()
        {
            Datasets response = null;
            try
            {
                var request = DatasetRequest($"{_datasetsUri}/datasets", "GET", AccessToken);
                var responseContent = GetResponse(request);

                var json = new JavaScriptSerializer();
                response = (Datasets)json.Deserialize(responseContent, typeof(Datasets));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return response;
        }

        public void SetDatasetId()
        {
            try
            {
                var request = DatasetRequest($"{_datasetsUri}/datasets", "GET", AccessToken);
                var responseContent = GetResponse(request);

                var json = new JavaScriptSerializer();
                var ds = (Datasets)json.Deserialize(responseContent, typeof(Datasets));
                _datasetId = ds.value.Single(x => x.Name == _datasetName).Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public Tables GetTables()
        {
            Tables response = null;
            try
            {
                var request = DatasetRequest($"{_datasetsUri}/datasets/{_datasetId}/tables", "GET", AccessToken);
                var responseContent = GetResponse(request);

                var json = new JavaScriptSerializer();
                response = (Tables)json.Deserialize(responseContent, typeof(Tables));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return response;
        }

        public void AddRows<T>(string tableName, IEnumerable<T> newRows) where T : new()
        {
            try
            {
                var request = DatasetRequest($"{_datasetsUri}/datasets/{_datasetId}/tables/{tableName}/rows", "POST", AccessToken);
                PostRequest(request, newRows.ToJson(JavaScriptConverter<T>.GetSerializer()));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void DeleteRows(string tableName)
        {
            try
            {
                var request = DatasetRequest($"{_datasetsUri}/datasets/{_datasetId}/tables/{tableName}/rows", "DELETE", AccessToken);
                request.ContentLength = 0;
                GetResponse(request);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static string PostRequest(WebRequest request, string json)
        {
            var byteArray = Encoding.UTF8.GetBytes(json);
            request.ContentLength = byteArray.Length;

            using (var writer = request.GetRequestStream())
            {
                writer.Write(byteArray, 0, byteArray.Length);
            }

            return GetResponse(request);
        }

        private static string GetResponse(WebRequest request)
        {
            string response;
            try
            {
                using (var httpResponse = request.GetResponse() as System.Net.HttpWebResponse)
                {
                    using (var reader = new StreamReader(httpResponse?.GetResponseStream()))
                    {
                        response = reader.ReadToEnd();
                    }
                }
                return response;
            }
            catch (WebException e)
            {
                using (var reader = new StreamReader(e.Response.GetResponseStream()))
                {
                    response = reader.ReadToEnd();
                }
                Console.WriteLine(e);
            }
            return response;
        }

        private static HttpWebRequest DatasetRequest(string datasetsUrl, string method, string accessToken)
        {
            var request = System.Net.WebRequest.Create(datasetsUrl) as System.Net.HttpWebRequest;
            request.KeepAlive = true;
            request.Method = method;
            request.ContentLength = 0;
            request.ContentType = "application/json";
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            return request;
        }
    }
}
