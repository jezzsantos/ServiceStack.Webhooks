using ServiceStack.Caching;

namespace ServiceStack.Webhooks.IntTests
{
    public class CacheClientSubscriptionStoreSpec
    {
        public class GivenCacheClientSubscriptionStoreAndNoUser : GivenNoUserWithSubscriptionStoreBase
        {
            public override IWebhookSubscriptionStore GetSubscriptionStore()
            {
                return new CacheClientSubscriptionStore
                {
                    CacheClient = new MemoryCacheClient()
                };
            }
        }

        public class GivenCacheClientSubscriptionStoreAndAUser : GivenAUserWithSubscriptionStoreBase
        {
            public override IWebhookSubscriptionStore GetSubscriptionStore()
            {
                return new CacheClientSubscriptionStore
                {
                    CacheClient = new MemoryCacheClient()
                };
            }
        }
    }
}