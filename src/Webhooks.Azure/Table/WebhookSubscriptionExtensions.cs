using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.Azure.Table
{
    internal static class WebhookSubscriptionExtensions
    {
        /// <summary>
        ///     Converts the specified subscription DTO to a <see cref="WebhookSubscriptionEntity" /> entity
        /// </summary>
        public static WebhookSubscriptionEntity ToEntity(this WebhookSubscription subscription)
        {
            Guard.AgainstNull(() => subscription, subscription);

            var sub = subscription.ConvertTo<WebhookSubscriptionEntity>();
            sub.PartitionKey = string.Empty;
            sub.RowKey = sub.Id;
            sub.IsActive = subscription.IsActive.ToString().ToLowerInvariant();
            sub.Config = subscription.Config.ToJson();
            sub.CreatedDateUtc = subscription.CreatedDateUtc.ToSafeAzureDateTime();
            sub.LastModifiedDateUtc = subscription.LastModifiedDateUtc.ToSafeAzureDateTime();
            return sub;
        }

        /// <summary>
        ///     Converts the specified subscription entity to a <see cref="WebhookSubscription" /> DTO
        /// </summary>
        /// <param name="subscription"></param>
        /// <returns></returns>
        public static WebhookSubscription FromEntity(this WebhookSubscriptionEntity subscription)
        {
            Guard.AgainstNull(() => subscription, subscription);

            var sub = subscription.ConvertTo<WebhookSubscription>();
            sub.IsActive = subscription.IsActive.ToBool();
            sub.Config = subscription.Config.FromJson<SubscriptionConfig>();

            return sub;
        }
    }
}