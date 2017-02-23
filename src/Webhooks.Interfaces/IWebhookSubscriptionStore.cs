using System.Collections.Generic;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks
{
    public interface IWebhookSubscriptionStore
    {
        /// <summary>
        ///     Returns the identity of the added subscription
        /// </summary>
        string Add(WebhookSubscription subscription);

        /// <summary>
        ///     Returns all subscription for the specified userId
        /// </summary>
        List<WebhookSubscription> Find(string userId);

        /// <summary>
        ///     Gets the subscription for the specified user and eventName
        /// </summary>
        WebhookSubscription Get(string userId, string eventName);
    }
}