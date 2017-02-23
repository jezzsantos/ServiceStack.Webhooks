using System.Collections.Generic;
using System.Linq;
using ServiceStack.Caching;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks
{
    public class MemoryWebhookSubscriptionStore : IWebhookSubscriptionStore
    {
        internal const string CachekeyFormat = @"subscriptions:{0}:{1}";
        internal const string CacheKeyForAnonymousUser = @"everyone";

        public ICacheClient CacheClient { get; set; }

        public string Add(WebhookSubscription subscription)
        {
            var id = DataFormats.CreateEntityIdentifier();
            subscription.Id = id;

            var cacheKey = FormatCacheKey(subscription.CreatedById, subscription.Event);
            CacheClient.Add(cacheKey, subscription);

            return id;
        }

        public List<WebhookSubscription> Find(string userId)
        {
            var keys = CacheClient.GetKeysStartingWith(FormatCacheKey(userId, null));

            return CacheClient.GetAll<WebhookSubscription>(keys)
                .Select(pair => pair.Value)
                .ToList();
        }

        public WebhookSubscription Get(string userId, string eventName)
        {
            var keys = CacheClient.GetKeysStartingWith(FormatCacheKey(userId, eventName));

            var subscription = CacheClient.GetAll<WebhookSubscription>(keys)
                .Select(pair => pair.Value)
                .FirstOrDefault();

            return subscription;
        }

        internal static string FormatCacheKey(string userId, string eventName)
        {
            var usrId = userId.HasValue() ? userId : CacheKeyForAnonymousUser;

            return CachekeyFormat.Fmt(usrId, eventName);
        }
    }
}