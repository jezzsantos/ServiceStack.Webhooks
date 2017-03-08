namespace ServiceStack.Webhooks.IntTests
{
    public class MemorySubscriptionStoreSpec
    {
        public class GivenMemorySubscriptionStoreAndNoUser : GivenNoUserWithSubscriptionStoreBase
        {
            public override IWebhookSubscriptionStore GetSubscriptionStore()
            {
                return new MemorySubscriptionStore();
            }
        }

        public class GivenMemorySubscriptionStoreAndAUser : GivenAUserWithSubscriptionStoreBase
        {
            public override IWebhookSubscriptionStore GetSubscriptionStore()
            {
                return new MemorySubscriptionStore();
            }
        }
    }
}