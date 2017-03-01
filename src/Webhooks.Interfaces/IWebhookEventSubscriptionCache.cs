using System.Collections.Generic;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks
{
    public interface IWebhookEventSubscriptionCache
    {
        /// <summary>
        ///     Gets all the subscriptions for the specified event
        /// </summary>
        List<SubscriptionConfig> GetAll(string eventName);
    }
}