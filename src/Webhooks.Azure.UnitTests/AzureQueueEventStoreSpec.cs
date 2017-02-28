using System;
using NUnit.Framework;

namespace ServiceStack.Webhooks.Azure.UnitTests
{
    public class AzureQueueEventStoreSpec
    {
        [TestFixture]
        public class GivenAContext
        {
            private AzureQueueEventStore store;

            [SetUp]
            public void Initialize()
            {
                store = new AzureQueueEventStore();
            }

            [Test, Category("Unit")]
            public void When_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() => store.Create(null, "adata"));
            }
        }
    }
}