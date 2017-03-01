using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Caching;
using ServiceStack.Webhooks.ServiceInterface;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks
{
    /// <summary>
    ///     This cache stores subscriptions (by eventname) for some TTL before fetching them again from
    ///     <see cref="SubscriptionService" />.
    /// </summary>
    internal class CacheClientEventSubscriptionCache : IWebhookEventSubscriptionCache
    {
        internal const string CachekeyPrefix = @"subscribers";
        internal const string CachekeyFormat = CachekeyPrefix + @":{0}";
        internal const int DefaultCacheExpirySeconds = 60;

        public CacheClientEventSubscriptionCache()
        {
            ExpiryTimeSeconds = DefaultCacheExpirySeconds;
        }

        public ICacheClient CacheClient { get; set; }

        public ISubscriptionService SubscriptionService { get; set; }

        public int ExpiryTimeSeconds { get; set; }

        public List<SubscriptionConfig> GetAll(string eventName)
        {
            Guard.AgainstNullOrEmpty(() => eventName, eventName);

            var cached = CacheClient.Get<CachedSubscription>(FormatCacheKey(eventName));
            if (cached != null)
            {
                return cached.Subscribers;
            }

            var fetched = SubscriptionService.Search(eventName);

            if (fetched.Any())
            {
                var expiry = TimeSpan.FromSeconds(ExpiryTimeSeconds);
                CacheClient.Set(FormatCacheKey(eventName), new CachedSubscription
                {
                    Subscribers = fetched
                }, expiry);
            }

            return fetched;
        }

        internal static string FormatCacheKey(string eventName)
        {
            return CachekeyFormat.Fmt(eventName);
        }
    }

    internal class CachedSubscription
    {
        public List<SubscriptionConfig> Subscribers { get; set; }
    }
}