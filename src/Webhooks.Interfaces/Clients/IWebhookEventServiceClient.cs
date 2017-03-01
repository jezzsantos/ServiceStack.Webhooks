using System;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.Clients
{
    public interface IWebhookEventServiceClient
    {
        /// <summary>
        ///     Gets or sets the timeout for requests
        /// </summary>
        TimeSpan? Timeout { get; set; }

        /// <summary>
        ///     Gets or sets the number of retries a timed out request will be attempted
        /// </summary>
        int Retries { get; set; }

        /// <summary>
        ///     Posts the specified event to the specified subscription
        /// </summary>
        void Post(SubscriptionConfig subscription, string eventName, object data);
    }
}