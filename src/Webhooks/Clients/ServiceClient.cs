using System;
using System.Net;

namespace ServiceStack.Webhooks.Clients
{
    internal class ServiceClient : IServiceClient
    {
        private readonly JsonServiceClient jsonClient;

        public ServiceClient(string url)
        {
            jsonClient = new JsonServiceClient(url);
        }

        public TimeSpan? Timeout
        {
            get { return jsonClient.Timeout; }
            set { jsonClient.Timeout = value; }
        }

        public Action<HttpWebRequest> RequestFilter
        {
            get { return jsonClient.RequestFilter; }
            set { jsonClient.RequestFilter = value; }
        }

        public void Post<TRequest>(string url, TRequest request)
        {
            jsonClient.Post<TRequest>(url, request);
        }
    }
}