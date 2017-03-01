using System;
using System.Collections.Generic;
using Funq;
using Moq;
using NUnit.Framework;
using ServiceStack.Webhooks.Clients;
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
            public void WhenRegister_ThenSubscriptionServiceDependenciesRegistered()
            {
                var appHost = new Mock<IAppHost>();
                var container = new Container();
                appHost.As<IHasContainer>().Setup(ah => ah.Container)
                    .Returns(container);

                new WebhookFeature().Register(appHost.Object);

                Assert.That(container.GetService(typeof(ISubscriptionService)), Is.TypeOf<SubscriptionService>());
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

            [Test, Category("Unit")]
            public void WhenRegister_ThenClientDependenciesRegistered()
            {
                var appHost = new Mock<IAppHost>();
                var container = new Container();
                appHost.As<IHasContainer>().Setup(ah => ah.Container)
                    .Returns(container);

                new WebhookFeature().Register(appHost.Object);

                Assert.That(container.GetService(typeof(IWebhooks)), Is.TypeOf<WebhooksClient>());
                Assert.That(container.GetService(typeof(IWebhookEventSubscriptionCache)), Is.TypeOf<CacheClientEventSubscriptionCache>());
                Assert.That(container.GetService(typeof(IWebhookEventServiceClientFactory)), Is.TypeOf<WebhookEventServiceClientFactory>());
                Assert.That(container.GetService(typeof(IWebhookEventServiceClient)), Is.TypeOf<WebhookEventServiceClient>());
            }

            [Test, Category("Unit")]
            public void WhenRegisterAndEventSinkNotRegistered_ThenRegistersAppHostSinkByDefault()
            {
                var appHost = new Mock<IAppHost>();
                var container = new Container();
                appHost.As<IHasContainer>().Setup(ah => ah.Container)
                    .Returns(container);

                new WebhookFeature().Register(appHost.Object);

                Assert.That(container.GetService(typeof(IWebhookEventSink)), Is.TypeOf<AppHostWebhookEventSink>());
            }

            [Test, Category("Unit")]
            public void WhenRegisterAndEventSinkAlreadyRegistered_ThenDoesNotRegisterAppHostSinkByDefault()
            {
                var appHost = new Mock<IAppHost>();
                var container = new Container();
                container.Register<IWebhookEventSink>(new TestEventSink());
                appHost.As<IHasContainer>().Setup(ah => ah.Container)
                    .Returns(container);

                new WebhookFeature().Register(appHost.Object);

                Assert.That(container.GetService(typeof(IWebhookEventSink)), Is.TypeOf<TestEventSink>());
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

            public List<SubscriptionConfig> Search(string eventName)
            {
                throw new NotImplementedException();
            }
        }

        public class TestEventSink : IWebhookEventSink
        {
            public void Write<TDto>(string eventName, TDto data)
            {
                throw new NotImplementedException();
            }

            public List<WebhookEvent> Peek()
            {
                throw new NotImplementedException();
            }
        }
    }
}