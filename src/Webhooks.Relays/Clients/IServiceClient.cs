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
        ///     Gets or sets the filter to run when the request is about to be serialized
        /// </summary>
        Action<HttpWebRequest, byte[], object> OnSerializeRequest { get; set; }

        /// <summary>
        ///     Gets or sets an action to perform when authentication is required by the request
        /// </summary>
        Action OnAuthenticationRequired { get; set; }

        /// <summary>
        ///     Gets or sets the cookies to use in requests
        /// </summary>
        CookieContainer CookieContainer { get; set; }

        /// <summary>
        ///     Gets or sets the bearer token to use in requests
        /// </summary>
        string BearerToken { get; set; }

        /// <summary>
        ///     Posts the specified data to the specified URL
        /// </summary>
        HttpWebResponse Post<TRequest>(string url, TRequest data);

        /// <summary>
        ///     Returns the response for a GET request
        /// </summary>
        TResponse Get<TResponse>(IReturn<TResponse> request);

        /// <summary>
        ///     Puts the specified data to the specified URL
        /// </summary>
        TResponse Put<TResponse>(IReturn<TResponse> request);
    }
}