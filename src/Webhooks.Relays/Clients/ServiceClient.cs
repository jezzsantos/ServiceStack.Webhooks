using System;
using System.Net;

namespace ServiceStack.Webhooks.Relays.Clients
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

        public HttpWebResponse Post<TRequest>(string url, TRequest request)
        {
            return jsonClient.Post<HttpWebResponse>(url, request);
        }

        public TResponse Get<TResponse>(IReturn<TResponse> request)
        {
            return jsonClient.Get(request);
        }

        public TResponse Put<TResponse>(IReturn<TResponse> request)
        {
            return jsonClient.Put(request);
        }
    }
}