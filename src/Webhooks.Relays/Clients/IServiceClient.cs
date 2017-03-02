using System;
using System.Net;

namespace ServiceStack.Webhooks.Relays.Clients
{
    public interface IServiceClient
    {
        /// <summary>
        ///     Gets or sets the timeout allowed for returning a response
        /// </summary>
        TimeSpan? Timeout { get; set; }

        /// <summary>
        ///     Gets or sets the filter to run when a request is made
        /// </summary>
        Action<HttpWebRequest> RequestFilter { get; set; }

        /// <summary>
        ///     Posts the specified data to the specified URL
        /// </summary>
        void Post<TRequest>(string url, TRequest data);

        /// <summary>
        ///     Returns the response for a GET request
        /// </summary>
        TResponse Get<TResponse>(IReturn<TResponse> request);
    }
}