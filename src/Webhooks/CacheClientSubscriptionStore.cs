using System.Collections.Generic;
using System.Linq;
using ServiceStack.Caching;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks
{
    public class CacheClientSubscriptionStore : ISubscriptionStore
    {
        internal const string CachekeyPrefix = @"subscriptions";
        internal const string CachekeyFormat = CachekeyPrefix + @":{0}:{1}";
        internal const string HistoryCachekeyFormat = CachekeyFormat + @":{2}";
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

            return GetObjects<WebhookSubscription>(keys)
                .Select(pair => pair.Value)
                .ToList();
        }

        public List<SubscriptionRelayConfig> Search(string eventName, bool? isActive = null)
        {
            var keys = CacheClient.GetKeysStartingWith(CachekeyPrefix);

            return GetObjects<WebhookSubscription>(keys)
                .Where(pair => pair.Value.Event.EqualsIgnoreCase(eventName)
                               && (!isActive.HasValue || (pair.Value.IsActive == isActive.Value)))
                .Select(pair => new SubscriptionRelayConfig
                {
                    SubscriptionId = pair.Value.Id,
                    Config = pair.Value.Config
                })
                .ToList();
        }

        public WebhookSubscription Get(string userId, string eventName)
        {
            Guard.AgainstNullOrEmpty(() => eventName, eventName);

            var keys = CacheClient.GetKeysStartingWith(FormatCacheKey(userId, eventName));

            var subscription = GetObjects<WebhookSubscription>(keys)
                .Select(pair => pair.Value)
                .FirstOrDefault();

            return subscription;
        }

        public WebhookSubscription Get(string subscriptionId)
        {
            Guard.AgainstNullOrEmpty(() => subscriptionId, subscriptionId);

            var persistedSubscription = GetSubscription(subscriptionId);
            return persistedSubscription.Value;
        }

        public void Update(string subscriptionId, WebhookSubscription subscription)
        {
            Guard.AgainstNullOrEmpty(() => subscriptionId, subscriptionId);
            Guard.AgainstNull(() => subscription, subscription);

            var persistedSubscription = GetSubscription(subscriptionId);
            if (persistedSubscription.Value != null)
            {
                var key = persistedSubscription.Key;
                CacheClient.Set(key, subscription);
            }
        }

        public void Delete(string subscriptionId)
        {
            Guard.AgainstNullOrEmpty(() => subscriptionId, subscriptionId);

            var subscription = GetSubscription(subscriptionId);
            if (subscription.Value != null)
            {
                var key = subscription.Key;
                CacheClient.Remove(key);
            }
        }

        public void Add(string subscriptionId, SubscriptionDeliveryResult result)
        {
            Guard.AgainstNullOrEmpty(() => subscriptionId, subscriptionId);
            Guard.AgainstNull(() => result, result);

            var subscription = GetSubscription(subscriptionId);
            if (subscription.Value != null)
            {
                var cacheKey = FormatHistoryCacheKey(subscription.Value.CreatedById, subscription.Value.Event, result.Id);
                CacheClient.Add(cacheKey, result);
            }
        }

        public List<SubscriptionDeliveryResult> Search(string subscriptionId, int top)
        {
            var subscription = GetSubscription(subscriptionId);
            if (subscription.Value != null)
            {
                var cacheKeyPrefix = FormatCacheKey(subscription.Value.CreatedById, subscription.Value.Event);

                var keys = CacheClient.GetKeysStartingWith(cacheKeyPrefix);

                return GetObjects<SubscriptionDeliveryResult>(keys)
                    .Select(pair => pair.Value)
                    .OrderByDescending(r => r.AttemptedDateUtc)
                    .Take(top)
                    .ToList();
            }

            return new List<SubscriptionDeliveryResult>();
        }

        private KeyValuePair<string, WebhookSubscription> GetSubscription(string subscriptionId)
        {
            var keys = CacheClient.GetKeysStartingWith(CachekeyPrefix);
            var subscriptions = GetObjects<WebhookSubscription>(keys);
            return subscriptions.FirstOrDefault(sub => sub.Value.Id.EqualsIgnoreCase(subscriptionId));
        }

        internal static string FormatCacheKey(string userId, string eventName)
        {
            var usrId = userId.HasValue() ? userId : CacheKeyForAnonymousUser;

            return CachekeyFormat.Fmt(usrId, eventName);
        }

        internal static string FormatHistoryCacheKey(string userId, string eventName, string historyId)
        {
            var usrId = userId.HasValue() ? userId : CacheKeyForAnonymousUser;

            return HistoryCachekeyFormat.Fmt(usrId, eventName, historyId);
        }

        private Dictionary<string, TObject> GetObjects<TObject>(IEnumerable<string> keys) where TObject : class
        {
            var dictionary = new Dictionary<string, TObject>();
            keys.ToList().ForEach(key =>
            {
                var item = CacheClient.Get<object>(key);
                var tObject = item as TObject;
                if (tObject != null)
                {
                    dictionary[key] = tObject;
                }
            });

            return dictionary;
        }
    }
}