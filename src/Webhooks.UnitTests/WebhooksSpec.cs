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
            private Mock<IWebhookEventSink> eventSink;
            private WebhooksClient webhooks;

            [SetUp]
            public void Initialize()
            {
                eventSink = new Mock<IWebhookEventSink>();
                webhooks = new WebhooksClient
                {
                    EventSink = eventSink.Object
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

                eventSink.Verify(es => es.Write("aneventname", "adata"));
            }
        }
    }
}