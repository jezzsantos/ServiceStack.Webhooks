using ServiceStack.Caching;

namespace ServiceStack.Webhooks.IntTests
{
    public class MemoryWebhookSubscriptionStoreSpec
    {
        public class GivenMemoryWebhookSubscriptionStoreAndNoUser : GivenNoUserWithSubscriptionStoreBase
        {
            public override IWebhookSubscriptionStore GetSubscriptionStore()
            {
                return new MemoryWebhookSubscriptionStore
                {
                    CacheClient = new MemoryCacheClient()
                };
            }
        }

        public class GivenMemoryWebhookSubscriptionStoreAndAUser : GivenAUserWithSubscriptionStoreBase
        {
            public override IWebhookSubscriptionStore GetSubscriptionStore()
            {
                return new MemoryWebhookSubscriptionStore
                {
                    CacheClient = new MemoryCacheClient()
                };
            }
        }
    }
}