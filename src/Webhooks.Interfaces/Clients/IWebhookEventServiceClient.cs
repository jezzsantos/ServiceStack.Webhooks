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
        ///     Relays the specified event to the specified subscription configuration
        /// </summary>
        SubscriptionDeliveryResult Relay(SubscriptionRelayConfig subscription, string eventName, object data);
    }
}