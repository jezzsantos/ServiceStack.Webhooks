using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ServiceStack.Webhooks.Azure.Worker;
using ServiceStack.Webhooks.Clients;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.Azure.UnitTests.Worker
{
    public class EventRelayQueueProcessorSpec
    {
        [TestFixture]
        public class GivenAContext
        {
            private EventRelayQueueProcessor processor;
            private Mock<IWebhookEventServiceClient> serviceClient;
            private Mock<IWebhookEventSubscriptionCache> subscriptionCache;

            [SetUp]
            public void Initialize()
            {
                serviceClient = new Mock<IWebhookEventServiceClient>();
                subscriptionCache = new Mock<IWebhookEventSubscriptionCache>();
                processor = new EventRelayQueueProcessor
                {
                    ServiceClient = serviceClient.Object,
                    SubscriptionCache = subscriptionCache.Object
                };
            }

            [Test, Category("Unit")]
            public void WhenWriteWithNoSubscriptions_ThenIgnoresEvent()
            {
                subscriptionCache.Setup(sc => sc.GetAll(It.IsAny<string>()))
                    .Returns(new List<SubscriptionConfig>());

                var result = processor.ProcessMessage(new WebhookEvent
                {
                    EventName = "aneventname",
                    Data = new Dictionary<string, string> {{"akey", "avalue"}}
                });

                Assert.That(result, Is.True);
                subscriptionCache.Verify(sc => sc.GetAll("aneventname"));
                serviceClient.Verify(sc => sc.Post(It.IsAny<SubscriptionConfig>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            }

            [Test, Category("Unit")]
            public void WhenWrite_ThenPostsEventToSubscribers()
            {
                var config = new SubscriptionConfig();
                subscriptionCache.Setup(sc => sc.GetAll(It.IsAny<string>()))
                    .Returns(new List<SubscriptionConfig>
                    {
                        config
                    });

                var result = processor.ProcessMessage(new WebhookEvent
                {
                    EventName = "aneventname",
                    Data = new Dictionary<string, string> {{"akey", "avalue"}}
                });

                Assert.That(result, Is.True);
                subscriptionCache.Verify(sc => sc.GetAll("aneventname"));
                serviceClient.VerifySet(sc => sc.Retries = EventRelayQueueProcessor.DefaultServiceClientRetries);
                serviceClient.VerifySet(sc => sc.Timeout = TimeSpan.FromSeconds(EventRelayQueueProcessor.DefaultServiceClientTimeoutSeconds));
                serviceClient.Verify(sc => sc.Post(config, "aneventname", It.Is<Dictionary<string, string>>(dic => dic["akey"] == "avalue")));
            }
        }
    }
}