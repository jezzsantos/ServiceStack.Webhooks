using System.Collections.Generic;
using System.Linq;
using ServiceStack.Caching;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks
{
    internal class MemoryWebhookSubscriptionStore : IWebhookSubscriptionStore
    {
        internal const string CachekeyFormat = @"subscriptions:{0}:{1}";
        internal const string CacheKeyForAnonymousUser = @"everyone";

        public ICacheClient CacheClient { get; set; }

        public string Add(WebhookSubscription subscription)
        {
            Guard.AgainstNull(() => subscription, subscription);

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
            Guard.AgainstNullOrEmpty(() => eventName, eventName);

            var keys = CacheClient.GetKeysStartingWith(FormatCacheKey(userId, eventName));

            var subscription = CacheClient.GetAll<WebhookSubscription>(keys)
                .Select(pair => pair.Value)
                .FirstOrDefault();

            return subscription;
        }

        public void Update(string subscriptionId, WebhookSubscription subscription)
        {
            Guard.AgainstNullOrEmpty(() => subscriptionId, subscriptionId);
            Guard.AgainstNull(() => subscription, subscription);

            var keys = CacheClient.GetAllKeys();
            var subscriptions = CacheClient.GetAll<WebhookSubscription>(keys);
            var persistedSubscription = subscriptions.FirstOrDefault(sub => sub.Value.Id.EqualsIgnoreCase(subscriptionId));
            if (persistedSubscription.Value != null)
            {
                var key = persistedSubscription.Key;
                CacheClient.Set(key, subscription);
            }
        }

        public void Delete(string subscriptionId)
        {
            Guard.AgainstNullOrEmpty(() => subscriptionId, subscriptionId);

            var keys = CacheClient.GetAllKeys();
            var subscriptions = CacheClient.GetAll<WebhookSubscription>(keys);
            var persistedSubscription = subscriptions.FirstOrDefault(sub => sub.Value.Id.EqualsIgnoreCase(subscriptionId));
            if (persistedSubscription.Value != null)
            {
                var key = persistedSubscription.Key;
                CacheClient.Remove(key);
            }
        }

        internal static string FormatCacheKey(string userId, string eventName)
        {
            var usrId = userId.HasValue() ? userId : CacheKeyForAnonymousUser;

            return CachekeyFormat.Fmt(usrId, eventName);
        }
    }
}