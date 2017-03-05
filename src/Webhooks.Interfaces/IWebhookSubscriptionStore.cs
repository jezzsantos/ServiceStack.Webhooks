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

        /// <summary>
        ///     Updates the subscription
        /// </summary>
        void Update(string subscriptionId, WebhookSubscription subscription);

        /// <summary>
        ///     Deletes the subscription
        /// </summary>
        void Delete(string subscriptionId);

        /// <summary>
        ///     Returns all subscription configurations for all users for the specified event,
        ///     and optionally whether they are currently active or not
        /// </summary>
        List<SubscriptionConfig> Search(string eventName, bool? isActive);
    }
}