using System;
using System.Collections.Generic;
using Funq;
using Moq;
using NUnit.Framework;
using ServiceStack.Webhooks.ServiceInterface;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.UnitTests
{
    public class WebhookFeatureSpec
    {
        [TestFixture]
        public class GivenNoContext
        {
            [Test, Category("Unit")]
            public void WhenRegister_ThenDependenciesRegistered()
            {
                var appHost = new Mock<IAppHost>();
                var container = new Container();
                appHost.As<IHasContainer>().Setup(ah => ah.Container)
                    .Returns(container);

                new WebhookFeature().Register(appHost.Object);

                Assert.That(container.GetService(typeof(ICurrentCaller)), Is.TypeOf<AuthSessionCurrentCaller>());
            }

            [Test, Category("Unit")]
            public void WhenRegisterAndSubscriptionStoreNotRegistered_ThenRegistersMemoryStoreByDefault()
            {
                var appHost = new Mock<IAppHost>();
                var container = new Container();
                appHost.As<IHasContainer>().Setup(ah => ah.Container)
                    .Returns(container);

                new WebhookFeature().Register(appHost.Object);

                Assert.That(container.GetService(typeof(IWebhookSubscriptionStore)), Is.TypeOf<MemoryWebhookSubscriptionStore>());
            }

            [Test, Category("Unit")]
            public void WhenRegisterAndSubscriptionStoreAlreadyRegistered_ThenDoesNotRegisterMemoryStoreByDefault()
            {
                var appHost = new Mock<IAppHost>();
                var container = new Container();
                container.Register<IWebhookSubscriptionStore>(new TestSubscriptionStore());
                appHost.As<IHasContainer>().Setup(ah => ah.Container)
                    .Returns(container);

                new WebhookFeature().Register(appHost.Object);

                Assert.That(container.GetService(typeof(IWebhookSubscriptionStore)), Is.TypeOf<TestSubscriptionStore>());
            }
        }

        public class TestSubscriptionStore : IWebhookSubscriptionStore
        {
            public string Add(WebhookSubscription subscription)
            {
                throw new NotImplementedException();
            }

            public List<WebhookSubscription> Find(string userId)
            {
                throw new NotImplementedException();
            }

            public WebhookSubscription Get(string userId, string eventName)
            {
                throw new NotImplementedException();
            }

            public void Update(string subscriptionId, WebhookSubscription subscription)
            {
                throw new NotImplementedException();
            }

            public void Delete(string subscriptionId)
            {
                throw new NotImplementedException();
            }
        }
    }
}