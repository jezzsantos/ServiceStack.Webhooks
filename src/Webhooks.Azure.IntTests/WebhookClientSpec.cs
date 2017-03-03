using NUnit.Framework;
using ServiceStack.Webhooks.Azure.IntTests.Services;

namespace ServiceStack.Webhooks.Azure.IntTests
{
    public class WebhookClientSpec
    {
        [TestFixture]
        public class GivenAzureConfiguredFeature : AzureIntegrationTestBase
        {
            private static AppSelfHostBase appHost;
            private static JsonServiceClient client;
            private const string BaseUrl = "http://localhost:5567/";
            private static IWebhookSubscriptionStore subscriptionsStore;
            private static IWebhookEventSink eventSink;

            [OneTimeTearDown]
            public void CleanupContext()
            {
                appHost.Dispose();
            }

            [OneTimeSetUp]
            public void InitializeContext()
            {
                appHost = new AppHostForAzureTesting();
                appHost.Init();
                appHost.Start(BaseUrl);

                client = new JsonServiceClient(BaseUrl);
                subscriptionsStore = appHost.Resolve<IWebhookSubscriptionStore>();
                eventSink = appHost.Resolve<IWebhookEventSink>();
            }

            [SetUp]
            public void Initialize()
            {
                ((AzureTableWebhookSubscriptionStore) subscriptionsStore).Clear();
                ((AzureQueueWebhookEventSink) eventSink).Clear();
            }

            [Test, Category("Integration")]
            public void WhenRaiseEvent_ThenEventSunk()
            {
                client.Get(new RaiseEvent
                {
                    EventName = "aneventname"
                });

                var events = ((AzureQueueWebhookEventSink) eventSink).Peek();

                Assert.That(events.Count, Is.EqualTo(1));
                Assert.That(events[0].EventName, Is.EqualTo("aneventname"));
            }
        }
    }
}