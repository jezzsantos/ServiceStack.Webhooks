using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.Configuration;
using ServiceStack.Webhooks.Azure.IntTests.Services;
using ServiceStack.Webhooks.Azure.Queue;
using ServiceStack.Webhooks.ServiceModel;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.Azure.IntTests
{
    public class EventRelayWorkerSpec
    {
        [TestFixture]
        public class GivenTheRelayWorkerHostedInAzureRole : AzureIntegrationTestBase
        {
            private static AppHostForAzureTesting appHost;
            private static JsonServiceClient client;
            private const string BaseUrl = "http://localhost:5567/";
            private IAzureQueueStorage<WebhookEvent> queue;

            [OneTimeSetUp]
            public void InitializeContext()
            {
                appHost = new AppHostForAzureTesting();
                appHost.Init();
                appHost.Start(BaseUrl);

                client = new JsonServiceClient(BaseUrl);

                var settings = appHost.Resolve<IAppSettings>();
                queue = new AzureQueueStorage<WebhookEvent>(settings.GetString(AzureQueueWebhookEventSink.AzureConnectionStringSettingName), settings.GetString(AzureQueueWebhookEventSink.QueueNameSettingName));
                SetupSubscription("aneventname");
            }

            [OneTimeTearDown]
            public void CleanupContext()
            {
                appHost.Dispose();
            }

            [SetUp]
            public void Initialize()
            {
                client.Put(new ResetConsumedEvents());
                queue.Empty();
            }

            [TearDown]
            public void Cleanup()
            {
            }

            [Test, Category("Integration")]
            public void WhenNoEventOnQueue_ThenNoSubscribersNotified()
            {
                WaitFor(10);

                var consumed = client.Get(new GetConsumedEvents()).Events;

                Assert.That(consumed.Count, Is.EqualTo(0));
            }

            [Test, Category("Integration")]
            public void WhenEventQueued_ThenSubscriberNotified()
            {
                SetupEvent("aneventname");
                WaitFor(10);

                var consumed = client.Get(new GetConsumedEvents()).Events;

                Assert.That(consumed.Count, Is.EqualTo(1));
                AssertSunkEvent(consumed[0]);
            }

            private static void SetupSubscription(string eventName)
            {
                client.Post(new CreateSubscription
                {
                    Name = "aname",
                    Events = new List<string> {eventName},
                    Config = new SubscriptionConfig
                    {
                        Url = BaseUrl.WithoutTrailingSlash() + new ConsumeEvent().ToPostUrl()
                    }
                });
            }

            private static void SetupEvent(string eventName)
            {
                client.Post(new RaiseEvent
                {
                    EventName = "aneventname",
                    Data = new TestEvent
                    {
                        A = 1,
                        B = 2,
                        C = 3
                    }
                });
            }

            private static void AssertSunkEvent(ConsumedEvent consumedEvent)
            {
                Assert.That(consumedEvent.EventName, Is.EqualTo("aneventname"));
                Assert.That(consumedEvent.Data.A, Is.EqualTo("1"));
                Assert.That(consumedEvent.Data.B, Is.EqualTo("2"));
                Assert.That(consumedEvent.Data.C, Is.EqualTo("3"));
            }

            private static void WaitFor(int seconds)
            {
                Task.Delay(TimeSpan.FromSeconds(seconds)).Wait();
            }
        }
    }
}