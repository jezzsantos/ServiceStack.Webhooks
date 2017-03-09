using NUnit.Framework;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.Webhooks;
using ServiceStack.Webhooks.IntTests;
using WebHooks.OrmLite;

namespace ServiceStack.WebHooks.OrmLite.Tests
{
    //For some reason R# wont run any tests if it can't find one
    public class Dummy
    {
        [Test] public void test() {}
    }

    public class OrmLiteSubscriptionStoreSpec
    {
        public class GivenMemorySubscriptionStoreAndNoUser : GivenNoUserWithSubscriptionStoreBase
        {
            private IDbConnectionFactory dbFactory;

            public override ISubscriptionStore GetSubscriptionStore()
            {
                return new OrmLiteSubscriptionStore(
                    new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider));
            }
        }

        public class GivenMemorySubscriptionStoreAndAUser : GivenAUserWithSubscriptionStoreBase
        {
            public override ISubscriptionStore GetSubscriptionStore()
            {
                return new OrmLiteSubscriptionStore(
                    new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider));
            }
        }
    }
}