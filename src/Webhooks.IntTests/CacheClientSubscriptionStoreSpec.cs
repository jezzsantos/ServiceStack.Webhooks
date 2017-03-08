using ServiceStack.Caching;

namespace ServiceStack.Webhooks.IntTests
{
    public class CacheClientSubscriptionStoreSpec
    {
        public class GivenCacheClientSubscriptionStoreAndNoUser : GivenNoUserWithSubscriptionStoreBase
        {
            public override ISubscriptionStore GetSubscriptionStore()
            {
                return new CacheClientSubscriptionStore
                {
                    CacheClient = new MemoryCacheClient()
                };
            }
        }

        public class GivenCacheClientSubscriptionStoreAndAUser : GivenAUserWithSubscriptionStoreBase
        {
            public override ISubscriptionStore GetSubscriptionStore()
            {
                return new CacheClientSubscriptionStore
                {
                    CacheClient = new MemoryCacheClient()
                };
            }
        }
    }
}