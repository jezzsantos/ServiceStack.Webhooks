using System;
using NUnit.Framework;

namespace ServiceStack.Webhooks.Azure.IntTests
{
    public class AzureQueueEventStoreSpec
    {
        [TestFixture]
        public class GivenAQueue : AzureIntegrationTestBase
        {
            private AzureQueueEventStore store;

            [SetUp]
            public void Initialize()
            {
                store = new AzureQueueEventStore();
                store.Clear();
            }

            [Test, Category("Integration")]
            public void WhenCreate_ThenQueuesEvent()
            {
                store.Create("aneventname", "adata");

                var result = store.Peek();

                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result[0].CreatedDateUtc, Is.EqualTo(DateTime.UtcNow).Within(1).Seconds);
                Assert.That(result[0].EventName, Is.EqualTo("aneventname"));
                Assert.That(result[0].Data, Is.EqualTo("adata"));
            }
        }
    }
}