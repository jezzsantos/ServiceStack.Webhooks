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
        public class GivenAnUnsecuredSubscription
        {
            private static AppHostForTesting appHost;
            private static JsonServiceClient client;
            private const string BaseUrl = "http://localhost:5567/";

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
            }

            [SetUp]
            public void Initialize()
            {
                appHost.Resolve<ICacheClient>().FlushAll();
                ((MemorySubscriptionStore) appHost.Resolve<ISubscriptionStore>()).Clear();

                client.Put(new ResetConsumedEvents());

                appHost.LoginUser(client, "asubscriber", WebhookFeature.DefaultSubscriberRoles);

                CreateSubscription(client, BaseUrl);
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
                        C = new TestNestedEvent
                        {
                            D = 3,
                            E = 4,
                            F = 5
                        }
                    }
                });

                var events = client.Get(new GetConsumedEvents()).Events;

                Assert.That(events.Count, Is.EqualTo(1));
                Assert.That(events[0].EventName, Is.EqualTo("aneventname"));
                Assert.That(events[0].Data.A, Is.EqualTo("1"));
                Assert.That(events[0].Data.B, Is.EqualTo("2"));
                Assert.That(events[0].Data.C.D, Is.EqualTo("3"));
                Assert.That(events[0].Data.C.E, Is.EqualTo("4"));
                Assert.That(events[0].Data.C.F, Is.EqualTo("5"));
                Assert.That(events[0].IsAuthenticated, Is.False);
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
                        C = new TestNestedEvent
                        {
                            D = 3,
                            E = 4,
                            F = 5
                        }
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
                        C = new TestNestedEvent
                        {
                            D = 3,
                            E = 4,
                            F = 5
                        }
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
        [TestFixture]
        public class GivenASecuredSubscription
        {
            private static AppHostForTesting appHost;
            private static JsonServiceClient client;
            private const string BaseUrl = "http://localhost:5567/";

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
            }

            [SetUp]
            public void Initialize()
            {
                appHost.Resolve<ICacheClient>().FlushAll();
                ((MemorySubscriptionStore)appHost.Resolve<ISubscriptionStore>()).Clear();

                client.Put(new ResetConsumedEvents());

                appHost.LoginUser(client, "asubscriber", WebhookFeature.DefaultSubscriberRoles);

                CreateSubscription(client, BaseUrl, StubSubscriberService.SubscriberSecret);
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
                        C = new TestNestedEvent
                        {
                            D = 3,
                            E = 4,
                            F = 5
                        }
                    }
                });

                var events = client.Get(new GetConsumedEvents()).Events;

                Assert.That(events.Count, Is.EqualTo(1));
                Assert.That(events[0].EventName, Is.EqualTo("aneventname"));
                Assert.That(events[0].Data.A, Is.EqualTo("1"));
                Assert.That(events[0].Data.B, Is.EqualTo("2"));
                Assert.That(events[0].Data.C.D, Is.EqualTo("3"));
                Assert.That(events[0].Data.C.E, Is.EqualTo("4"));
                Assert.That(events[0].Data.C.F, Is.EqualTo("5"));
                Assert.That(events[0].IsAuthenticated, Is.True);
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
                        C = new TestNestedEvent
                        {
                            D = 3,
                            E = 4,
                            F = 5
                        }
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
                        C = new TestNestedEvent
                        {
                            D = 3,
                            E = 4,
                            F = 5
                        }
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

        private static void CreateSubscription(JsonServiceClient client, string baseUrl, string secret = null)
        {
            var subscriberUrl = baseUrl.WithoutTrailingSlash() + new ConsumeEvent().ToPostUrl();

            client.Post(new CreateSubscription
            {
                Name = "test",
                Events = new List<string> {"aneventname"},
                Config = new SubscriptionConfig
                {
                    Url = subscriberUrl,
                    Secret = secret
                }
            });
        }
    }
}