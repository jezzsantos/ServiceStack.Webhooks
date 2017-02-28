using System.Collections.Generic;

namespace ServiceStack.Webhooks.Azure.Table
{
    internal interface IAzureTableStorage
    {
        /// <summary>
        ///     Adds the specified entity to storage
        /// </summary>
        void Add(WebhookSubscriptionEntity subscription);

        /// <summary>
        ///     Finds all entities that meet the specified query
        /// </summary>
        List<WebhookSubscriptionEntity> Find(TableStorageQuery query);

        /// <summary>
        ///     Updates the specified entity in storage
        /// </summary>
        void Update(WebhookSubscriptionEntity subscription);

        /// <summary>
        ///     Deletes the specified entity in storage
        /// </summary>
        void Delete(WebhookSubscriptionEntity subscription);

        /// <summary>
        ///     Gets the specified subscription from storage
        /// </summary>
        WebhookSubscriptionEntity Get(string id);

        /// <summary>
        ///     Empties all subscriptions from storage
        /// </summary>
        void Empty();
    }
}