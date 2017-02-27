using NUnit.Framework;
using ServiceStack.Caching;

namespace ServiceStack.Webhooks.IntTests
{
    public class WebhookClientSpec
    {
        [TestFixture]
        public class GivenPluginIsRegistered
        {
            private static AppSelfHostBase appHost;
            private static JsonServiceClient client;
            private const string BaseUrl = "http://localhost:8080/";
            private static IWebhookEventStore eventStore;

            [OneTimeTearDown]
            public void CleanupContext()
            {
                appHost.Dispose();
            }

            [OneTimeSetUp]
            public void InitializeContext()
            {
                appHost = new AppHostForTesting();
                appHost.Init();
                appHost.Start(BaseUrl);

                client = new JsonServiceClient(BaseUrl);
                eventStore = appHost.Resolve<IWebhookEventStore>();
            }

            [SetUp]
            public void Initialize()
            {
                appHost.Resolve<ICacheClient>().FlushAll();
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