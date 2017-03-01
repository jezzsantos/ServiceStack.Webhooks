using System;
using System.Net;

namespace ServiceStack.Webhooks.Clients
{
    public interface IServiceClient
    {
        TimeSpan? Timeout { get; set; }

        Action<HttpWebRequest> RequestFilter { get; set; }

        void Post<TRequest>(string subscriptionUrl, TRequest data);
    }
}