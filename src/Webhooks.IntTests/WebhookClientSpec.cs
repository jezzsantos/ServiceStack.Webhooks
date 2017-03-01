using System.Collections.Generic;
using NUnit.Framework;
using ServiceStack.Webhooks.ServiceModel;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.IntTests
{
    public class WebhookClientSpec
    {
        [TestFixture]
        public class GivenASubscriber
        {
            private static AppSelfHostBase appHost;
            private static JsonServiceClient client;
            private const string BaseUrl = "http://localhost:8080/";
            private static IWebhookEventSink eventSink;

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
                eventSink = appHost.Resolve<IWebhookEventSink>();

                client.Put(new ResetConsumedEvents());
            }

            [SetUp]
            public void Initialize()
            {
                var subscriberUrl = BaseUrl.WithoutTrailingSlash() + new ConsumeEvent().ToPostUrl();

                client.Post(new CreateSubscription
                {
                    Name = "test",
                    Events = new List<string> {"aneventname"},
                    Config = new SubscriptionConfig
                    {
                        Url = subscriberUrl
                    }
                });
            }

            [Test, Category("Integration")]
            public void WhenRaiseEvent_ThenEventSunk()
            {
                client.Put(new RaiseEvent
                {
                    EventName = "aneventname",
                    Data = new TestEvent
                    {
                        A = 1,
                        B = 2,
                        C = 3
                    }
                });

                var events = client.Get(new GetConsumedEvents()).Events;

                Assert.That(events.Count, Is.EqualTo(1));
                Assert.That(events[0].EventName, Is.EqualTo("aneventname"));
                Assert.That(events[0].Data.A, Is.EqualTo("1"));
                Assert.That(events[0].Data.B, Is.EqualTo("2"));
                Assert.That(events[0].Data.C, Is.EqualTo("3"));
            }
        }
    }
}