using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ServiceStack.Webhooks.Clients;
using ServiceStack.Webhooks.Relays;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.UnitTests
{
    public class AppHostEventSinkSpec
    {
        [TestFixture]
        public class GivenACacheAndServiceClient
        {
            private Mock<IEventServiceClient> serviceClient;
            private AppHostEventSink sink;
            private Mock<IEventSubscriptionCache> subscriptionCache;
            private Mock<ISubscriptionService> subscriptionService;

            [SetUp]
            public void Initialize()
            {
                serviceClient = new Mock<IEventServiceClient>();
                subscriptionCache = new Mock<IEventSubscriptionCache>();
                subscriptionService = new Mock<ISubscriptionService>();
                sink = new AppHostEventSink
                {
                    ServiceClient = serviceClient.Object,
                    SubscriptionCache = subscriptionCache.Object,
                    SubscriptionService = subscriptionService.Object
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
                    .Returns(new List<SubscriptionRelayConfig>());

                sink.Write("aneventname", new Dictionary<string, string> {{"akey", "avalue"}});

                subscriptionCache.Verify(sc => sc.GetAll("aneventname"));
                serviceClient.Verify(sc => sc.Relay(It.IsAny<SubscriptionRelayConfig>(), It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            }

            [Test, Category("Unit")]
            public void WhenWrite_ThenPostsEventToSubscribers()
            {
                var config = new SubscriptionRelayConfig();
                subscriptionCache.Setup(sc => sc.GetAll(It.IsAny<string>()))
                    .Returns(new List<SubscriptionRelayConfig>
                    {
                        config
                    });

                sink.Write("aneventname", new Dictionary<string, string> {{"akey", "avalue"}});

                subscriptionCache.Verify(sc => sc.GetAll("aneventname"));
                serviceClient.VerifySet(sc => sc.Retries = AppHostEventSink.DefaultServiceClientRetries);
                serviceClient.VerifySet(sc => sc.Timeout = TimeSpan.FromSeconds(AppHostEventSink.DefaultServiceClientTimeoutSeconds));
                serviceClient.Verify(sc => sc.Relay(config, "aneventname", It.Is<Dictionary<string, string>>(dic => dic["akey"] == "avalue")));
            }

            [Test, Category("Unit")]
            public void WhenWrite_ThenPostsEventToSubscribersAndUpdatesResults()
            {
                var config = new SubscriptionRelayConfig();
                subscriptionCache.Setup(sc => sc.GetAll(It.IsAny<string>()))
                    .Returns(new List<SubscriptionRelayConfig>
                    {
                        config
                    });
                var data = new Dictionary<string, string> {{"akey", "avalue"}};
                var result = new SubscriptionDeliveryResult();
                serviceClient.Setup(sc => sc.Relay(config, "aneventname", data))
                    .Returns(result);

                sink.Write("aneventname", data);

                subscriptionService.Verify(ss => ss.UpdateResults(It.Is<List<SubscriptionDeliveryResult>>(results =>
                    (results.Count == 1)
                    && (results[0] == result))));
            }
        }
    }
}