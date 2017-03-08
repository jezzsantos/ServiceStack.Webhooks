using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.Caching;
using ServiceStack.Logging;
using ServiceStack.Webhooks.IntTests.Services;
using ServiceStack.Webhooks.ServiceModel;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.IntTests
{
    public class WebhookClientSpec
    {
        [TestFixture]
        public class GivenASubscriber
        {
            private static AppHostForTesting appHost;
            private static JsonServiceClient client;
            private const string BaseUrl = "http://localhost:5567/";
            private static ICacheClient cacheClient;

            [OneTimeTearDown]
            public void CleanupContext()
            {
                appHost.Dispose();
            }

            [OneTimeSetUp]
            public void InitializeContext()
            {
                LogManager.LogFactory = new ConsoleLogFactory();
                appHost = new AppHostForTesting();
                appHost.Init();
                appHost.Start(BaseUrl);

                client = new JsonServiceClient(BaseUrl);
                cacheClient = appHost.Resolve<ICacheClient>();

                client.Put(new ResetConsumedEvents());
            }

            [SetUp]
            public void Initialize()
            {
                cacheClient.FlushAll();
                client.Put(new ResetConsumedEvents());
                var subscriberUrl = BaseUrl.WithoutTrailingSlash() + new ConsumeEvent().ToPostUrl();

                appHost.LoginUser(client, "asubscriber", WebhookFeature.DefaultSubscriberRoles);
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

            [Test, Category("Integration")]
            public void WhenRaiseEvents_ThenDeliveryResultsUpdated()
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

                Task.Delay(TimeSpan.FromSeconds(5)).Wait();

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

                var subscriptionId = client.Get(new ListSubscriptions()).Subscriptions[0].Id;

                var history = client.Get(new GetSubscription
                {
                    Id = subscriptionId
                }).History;

                Assert.That(history.Count, Is.EqualTo(2));
                Assert.That(history[0].AttemptedDateUtc, Is.EqualTo(DateTime.UtcNow).Within(5).Seconds);
                Assert.That(history[0].StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
                Assert.That(history[0].StatusDescription, Is.EqualTo("No Content"));
                Assert.That(history[0].SubscriptionId, Is.EqualTo(subscriptionId));
                Assert.That(history[1].AttemptedDateUtc, Is.EqualTo(DateTime.UtcNow).Within(10).Seconds);
                Assert.That(history[1].StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
                Assert.That(history[1].StatusDescription, Is.EqualTo("No Content"));
                Assert.That(history[1].SubscriptionId, Is.EqualTo(subscriptionId));
            }
        }
    }
}