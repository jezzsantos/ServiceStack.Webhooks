using NUnit.Framework;

namespace ServiceStack.Webhooks.Azure.IntTests
{
    public class WebhookClientSpec
    {
        [TestFixture]
        public class GivenAzureConfiguredFeature : AzureIntegrationTestBase
        {
            private static AppSelfHostBase appHost;
            private static JsonServiceClient client;
            private const string BaseUrl = "http://localhost:8080/";
            private static IWebhookSubscriptionStore subscriptionsStore;
            private static IWebhookEventStore eventStore;

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
                eventStore = appHost.Resolve<IWebhookEventStore>();
            }

            [SetUp]
            public void Initialize()
            {
                ((AzureTableWebhookSubscriptionStore) subscriptionsStore).Clear();
                ((AzureQueueWebhookEventStore) eventStore).Clear();
            }

            [Test, Category("Integration")]
            public void WhenRaiseEvent_ThenEventStored()
            {
                client.Get(new RaiseEvent
                {
                    EventName = "aneventname"
                });

                var events = eventStore.Peek();

                Assert.That(events.Count, Is.EqualTo(1));
                Assert.That(events[0].EventName, Is.EqualTo("aneventname"));
            }
        }
    }
}