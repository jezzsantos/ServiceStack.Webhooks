using System;
using Moq;
using NUnit.Framework;

namespace ServiceStack.Webhooks.UnitTests
{
    public class WebhooksSpec
    {
        [TestFixture]
        public class GivenAContext
        {
            private Mock<IWebhookEventStore> eventStore;
            private WebhooksClient webhooks;

            [SetUp]
            public void Initialize()
            {
                eventStore = new Mock<IWebhookEventStore>();
                webhooks = new WebhooksClient
                {
                    EventStore = eventStore.Object
                };
            }

            [Test, Category("Unit")]
            public void WhenPublishWithNullEventName_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() => webhooks.Publish(null, "adata"));
            }

            [Test, Category("Unit")]
            public void WhenPublish_ThenStoresEvent()
            {
                webhooks.Publish("aneventname", "adata");

                eventStore.Verify(es => es.Create("aneventname", "adata"));
            }
        }
    }
}