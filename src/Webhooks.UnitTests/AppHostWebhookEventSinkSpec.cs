using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ServiceStack.Webhooks.Clients;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.UnitTests
{
    public class AppHostWebhookEventSinkSpec
    {
        [TestFixture]
        public class GivenACacheAndServiceClient
        {
            private Mock<IWebhookEventServiceClient> serviceClient;
            private AppHostWebhookEventSink sink;
            private Mock<IWebhookEventSubscriptionCache> subscriptionCache;

            [SetUp]
            public void Initialize()
            {
                serviceClient = new Mock<IWebhookEventServiceClient>();
                subscriptionCache = new Mock<IWebhookEventSubscriptionCache>();
                sink = new AppHostWebhookEventSink
                {
                    ServiceClient = serviceClient.Object,
                    SubscriptionCache = subscriptionCache.Object
                };
            }

            [Test, Category("Unit")]
            public void WhenWriteWithNullEventName_ThenThrows()
            {
                Assert.That(() => sink.Write(null, new Dictionary<string, string> {{"akey", "avalue"}}), Throws.ArgumentNullException);
            }

            [Test, Category("Unit")]
            public void WhenWriteWithNoSubscriptions_ThenIgnoresEvent()
            {
                subscriptionCache.Setup(sc => sc.GetAll(It.IsAny<string>()))
                    .Returns(new List<SubscriptionConfig>());

                sink.Write("aneventname", new Dictionary<string, string> {{"akey", "avalue"}});

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

                sink.Write("aneventname", new Dictionary<string, string> {{"akey", "avalue"}});

                subscriptionCache.Verify(sc => sc.GetAll("aneventname"));
                serviceClient.VerifySet(sc => sc.Retries = AppHostWebhookEventSink.DefaultServiceClientRetries);
                serviceClient.VerifySet(sc => sc.Timeout = TimeSpan.FromSeconds(AppHostWebhookEventSink.DefaultServiceClientTimeoutSeconds));
                serviceClient.Verify(sc => sc.Post(config, "aneventname", It.Is<Dictionary<string, string>>(dic => dic["akey"] == "avalue")));
            }
        }
    }
}