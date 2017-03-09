using NUnit.Framework;
using ServiceStack.OrmLite;
using ServiceStack.Webhooks.IntTests;

namespace ServiceStack.Webhooks.OrmLite.IntTests
{
    public class OrmLiteSubscriptionStoreSpec
    {
        //For some reason R# wont run any tests if it can't find one
        [Test, Category("Integration")]
        public void OrmLiteTest()
        {
        }

        public class GivenOrmLiteSubscriptionStoreAndNoUser : GivenNoUserWithSubscriptionStoreBase
        {
            public override ISubscriptionStore GetSubscriptionStore()
            {
                return new OrmLiteSubscriptionStore(
                    new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider));
            }
        }

        public class GivenOrmLiteSubscriptionStoreAndAUser : GivenAUserWithSubscriptionStoreBase
        {
            public override ISubscriptionStore GetSubscriptionStore()
            {
                return new OrmLiteSubscriptionStore(
                    new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider));
            }
        }
    }
}