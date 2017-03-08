namespace ServiceStack.Webhooks.IntTests
{
    public class MemorySubscriptionStoreSpec
    {
        public class GivenMemorySubscriptionStoreAndNoUser : GivenNoUserWithSubscriptionStoreBase
        {
            public override ISubscriptionStore GetSubscriptionStore()
            {
                return new MemorySubscriptionStore();
            }
        }

        public class GivenMemorySubscriptionStoreAndAUser : GivenAUserWithSubscriptionStoreBase
        {
            public override ISubscriptionStore GetSubscriptionStore()
            {
                return new MemorySubscriptionStore();
            }
        }
    }
}