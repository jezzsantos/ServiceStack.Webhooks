using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ServiceStack.Configuration;
using ServiceStack.Webhooks.Relays.Clients;
using ServiceStack.Webhooks.ServiceModel;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.Azure.UnitTests
{
    public class SubscriptionServiceClientSpec
    {
        [TestFixture]
        public class GivenAContext
        {
            private SubscriptionServiceClient client;
            private Mock<IEventServiceClientFactory> serviceClientFactory;
            private Mock<IAppSettings> settings;

            [SetUp]
            public void Initialize()
            {
                settings = new Mock<IAppSettings>();
                settings.Setup(s => s.GetString(It.IsAny<string>()))
                    .Returns("aurl");
                serviceClientFactory = new Mock<IEventServiceClientFactory>();
                client = new SubscriptionServiceClient(settings.Object)
                {
                    ServiceClientFactory = serviceClientFactory.Object
                };
            }

            [Test, Category("Unit")]
            public void WhenCtorWithNullSettings_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    // ReSharper disable once ObjectCreationAsStatement
                        new SubscriptionServiceClient(null));
            }

            [Test, Category("Unit")]
            public void WhenSearch_ThenGetsSubscriptionsFromService()
            {
                var subscribers = new List<SubscriptionConfig>();
                var serviceClient = new Mock<Relays.Clients.IServiceClient>();
                serviceClient.Setup(sc => sc.Get(It.IsAny<SearchSubscriptions>()))
                    .Returns(new SearchSubscriptionsResponse
                    {
                        Subscribers = subscribers
                    });
                serviceClientFactory.Setup(scf => scf.Create("aurl"))
                    .Returns(serviceClient.Object);

                var results = client.Search("aneventname");

                Assert.That(results, Is.EqualTo(subscribers));
                serviceClientFactory.Verify(scf => scf.Create("aurl"));
                serviceClient.Verify(sc => sc.Get(It.Is<SearchSubscriptions>(ss => ss.EventName == "aneventname")));
            }
        }
    }
}