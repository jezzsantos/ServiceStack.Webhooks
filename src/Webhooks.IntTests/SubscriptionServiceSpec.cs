using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NUnit.Framework;
using ServiceStack.Caching;
using ServiceStack.Logging;
using ServiceStack.Webhooks.IntTests.Services;
using ServiceStack.Webhooks.ServiceModel;
using ServiceStack.Webhooks.ServiceModel.Types;
using ServiceStack.Webhooks.UnitTesting;

namespace ServiceStack.Webhooks.IntTests
{
    public class SubscriptionServiceSpec
    {
        [TestFixture]
        public class GivenTheRegistrationServiceAsASubscriber
        {
            private static AppHostForTesting appHost;
            private static JsonServiceClient client;
            private const string BaseUrl = "http://localhost:5567/";
            private string userId;

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
                userId = appHost.LoginUser(client, "asubscriber", WebhookFeature.DefaultSubscriberRoles);
            }

            [Test, Category("Integration")]
            public void WhenCreateSubscriptionWithNullEvents_ThenThrowsBadRequest()
            {
                Assert.That(() => client.Post(new CreateSubscription
                {
                    Name = "aname",
                    Events = null,
                    Config = new SubscriptionConfig
                    {
                        Url = "http://localhost:3333"
                    }
                }), ThrowsWebServiceException.WithStatusCode(HttpStatusCode.BadRequest));
            }

            [Test, Category("Integration")]
            public void WhenCreateSubscriptionWithNullConfig_ThenThrowsBadRequest()
            {
                Assert.That(() => client.Post(new CreateSubscription
                {
                    Name = "aname",
                    Events = new List<string> {"aneventname"},
                    Config = null
                }), ThrowsWebServiceException.WithStatusCode(HttpStatusCode.BadRequest));
            }

            [Test, Category("Integration")]
            public void WhenCreateSubscription_ThenCreatesSubscription()
            {
                var subscriptions = client.Post(new CreateSubscription
                {
                    Name = "aname",
                    Events = new List<string> {"anevent1", "anevent2"},
                    Config = new SubscriptionConfig
                    {
                        Url = "http://localhost:3333"
                    }
                }).Subscriptions;

                Assert.That(2, Is.EqualTo(subscriptions.Count));
                AssertSubscriptionCreated(subscriptions[0], "anevent1", userId);
                AssertSubscriptionCreated(subscriptions[1], "anevent2", userId);
            }

            [Test, Category("Integration")]
            public void WhenCreateSubscriptionWithSameEventNameAndUrl_ThenThrowsConflict()
            {
                client.Post(new CreateSubscription
                {
                    Name = "aname",
                    Events = new List<string> {"anevent1", "anevent2"},
                    Config = new SubscriptionConfig
                    {
                        Url = "http://localhost:3333"
                    }
                });

                Assert.That(() => client.Post(new CreateSubscription
                {
                    Name = "aname",
                    Events = new List<string> {"anevent3", "anevent2"},
                    Config = new SubscriptionConfig
                    {
                        Url = "http://localhost:3333"
                    }
                }), ThrowsWebServiceException.WithStatusCode(HttpStatusCode.Conflict));
            }

            [Test, Category("Integration")]
            public void WhenGetSubscriptionWithUnknownId_ThenThrowsNotFound()
            {
                Assert.That(() => client.Get(new GetSubscription
                {
                    Id = DataFormats.CreateEntityIdentifier()
                }), ThrowsWebServiceException.WithStatusCode(HttpStatusCode.NotFound));
            }

            [Test, Category("Integration")]
            public void WhenGetSubscription_ThenReturnsSubscription()
            {
                var subscription = client.Post(new CreateSubscription
                    {
                        Name = "aname",
                        Events = new List<string> {"anevent1"},
                        Config = new SubscriptionConfig
                        {
                            Url = "http://localhost:3333"
                        }
                    }).Subscriptions
                    .First();

                var result = client.Get(new GetSubscription
                {
                    Id = subscription.Id
                });

                Assert.That(result.Subscription.Id, Is.EqualTo(subscription.Id));
            }

            [Test, Category("Integration")]
            public void WhenListSubscriptions_ThenReturnsSubscriptions()
            {
                var subscription = client.Post(new CreateSubscription
                    {
                        Name = "aname",
                        Events = new List<string> {"anevent1"},
                        Config = new SubscriptionConfig
                        {
                            Url = "http://localhost:3333"
                        }
                    }).Subscriptions
                    .First();

                var subscriptions = client.Get(new ListSubscriptions()).Subscriptions;

                Assert.That(subscriptions.Count, Is.EqualTo(1));
                Assert.That(subscriptions[0].Id, Is.EqualTo(subscription.Id));
            }

            [Test, Category("Integration")]
            public void WhenDeleteSubscriptionWithUnknownId_ThenThrowsNotFound()
            {
                Assert.That(() => client.Delete(new DeleteSubscription
                {
                    Id = DataFormats.CreateEntityIdentifier()
                }), ThrowsWebServiceException.WithStatusCode(HttpStatusCode.NotFound));
            }

            [Test, Category("Integration")]
            public void WhenDeleteSubscription_ThenDeletesSubscription()
            {
                var subscription = client.Post(new CreateSubscription
                    {
                        Name = "aname",
                        Events = new List<string> {"anevent1"},
                        Config = new SubscriptionConfig
                        {
                            Url = "http://localhost:3333"
                        }
                    }).Subscriptions
                    .First();

                client.Delete(new DeleteSubscription
                {
                    Id = subscription.Id
                });

                Assert.That(() => client.Get(new GetSubscription
                {
                    Id = subscription.Id
                }), ThrowsWebServiceException.WithStatusCode(HttpStatusCode.NotFound));
            }

            [Test, Category("Integration")]
            public void WhenUpdateSubscriptionWithUnknownId_ThenThrowsNotFound()
            {
                Assert.That(() => client.Put(new UpdateSubscription
                {
                    Id = DataFormats.CreateEntityIdentifier()
                }), ThrowsWebServiceException.WithStatusCode(HttpStatusCode.NotFound));
            }

            [Test, Category("Integration")]
            public void WhenUpdateSubscription_ThenUpdatesSubscription()
            {
                var subscription = client.Post(new CreateSubscription
                    {
                        Name = "aname",
                        Events = new List<string> {"anevent1"},
                        Config = new SubscriptionConfig
                        {
                            Url = "http://localhost:3333"
                        }
                    }).Subscriptions
                    .First();

                var result = client.Put(new UpdateSubscription
                {
                    Id = subscription.Id,
                    Url = "http://localhost:3333/newurl"
                }).Subscription;

                Assert.That(result.Id, Is.EqualTo(subscription.Id));
                Assert.That(result.Config.Url, Is.EqualTo("http://localhost:3333/newurl"));
            }

            [Test, Category("Integration")]
            public void WhenSearchSubscriptions_ThenThrowsForbidden()
            {
                Assert.That(() => client.Get(new SearchSubscriptions
                {
                    EventName = "aneventname"
                }), ThrowsWebServiceException.WithStatusCode(HttpStatusCode.Forbidden));
            }

            [Test, Category("Integration")]
            public void WhenUpdateSubscriptionHistory_ThenThrowsForbidden()
            {
                Assert.That(() => client.Put(new UpdateSubscriptionHistory()), ThrowsWebServiceException.WithStatusCode(HttpStatusCode.Forbidden));
            }

            private static void AssertSubscriptionCreated(WebhookSubscription subscription, string eventName, string userId)
            {
                Assert.That(eventName, Is.EqualTo(subscription.Event));
                Assert.That(subscription.CreatedById, Is.EqualTo(userId));
                Assert.That(DateTime.UtcNow, Is.EqualTo(subscription.CreatedDateUtc).Within(5).Seconds);
                Assert.That(DateTime.UtcNow, Is.EqualTo(subscription.LastModifiedDateUtc).Within(5).Seconds);
                Assert.That(subscription.Id.HasValue());
            }
        }

        [TestFixture]
        public class GivenTheRegistrationServiceAsARelay
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
                appHost.LoginUser(client, "aservice", WebhookFeature.DefaultRelayRoles);
            }

            [Test, Category("Integration")]
            public void WhenCreateSubscription_ThenThrowsForbidden()
            {
                Assert.That(() => client.Post(new CreateSubscription
                {
                    Name = "aname",
                    Events = new List<string> {"anevent1", "anevent2"},
                    Config = new SubscriptionConfig
                    {
                        Url = "http://localhost:3333"
                    }
                }), ThrowsWebServiceException.WithStatusCode(HttpStatusCode.Forbidden));
            }

            [Test, Category("Integration")]
            public void WhenGetSubscription_ThenThrowsForbidden()
            {
                Assert.That(() => client.Get(new GetSubscription
                {
                    Id = DataFormats.CreateEntityIdentifier()
                }), ThrowsWebServiceException.WithStatusCode(HttpStatusCode.Forbidden));
            }

            [Test, Category("Integration")]
            public void WhenListSubscription_ThenThrowsForbidden()
            {
                Assert.That(() => client.Get(new ListSubscriptions()), ThrowsWebServiceException.WithStatusCode(HttpStatusCode.Forbidden));
            }

            [Test, Category("Integration")]
            public void WhenDeleteSubscription_ThenThrowsForbidden()
            {
                Assert.That(() => client.Delete(new DeleteSubscription
                {
                    Id = DataFormats.CreateEntityIdentifier()
                }), ThrowsWebServiceException.WithStatusCode(HttpStatusCode.Forbidden));
            }

            [Test, Category("Integration")]
            public void WhenUpdateSubscription_ThenThrowsForbidden()
            {
                Assert.That(() => client.Put(new UpdateSubscription
                {
                    Id = DataFormats.CreateEntityIdentifier()
                }), ThrowsWebServiceException.WithStatusCode(HttpStatusCode.Forbidden));
            }

            [Test, Category("Integration")]
            public void WhenSearchSubscription_ThenReturnsSubscription()
            {
                appHost.LoginUser(client, "asubscriber", WebhookFeature.DefaultSubscriberRoles);
                var url = "http://localhost:3333";
                client.Post(new CreateSubscription
                {
                    Name = "aname",
                    Events = new List<string> {"anevent1"},
                    Config = new SubscriptionConfig
                    {
                        Url = url
                    }
                });

                appHost.LoginUser(client, "arelay", WebhookFeature.DefaultRelayRoles);
                var result = client.Get(new SearchSubscriptions
                {
                    EventName = "anevent1"
                }).Subscribers;

                Assert.That(result[0].Config.Url, Is.EqualTo(url));
            }

            [Test, Category("Integration")]
            public void WhenUpdateSubscriptionHistoryWithRepeats_ThenReturnsSubscriptionHistory()
            {
                appHost.LoginUser(client, "asubscriber", WebhookFeature.DefaultSubscriberRoles);
                var subscription = client.Post(new CreateSubscription
                    {
                        Name = "aname",
                        Events = new List<string> {"anevent1"},
                        Config = new SubscriptionConfig
                        {
                            Url = "http://localhost:3333"
                        }
                    }).Subscriptions
                    .First();

                var datum1 = DateTime.UtcNow.ToNearestSecond();
                var datum2 = datum1.AddDays(1);
                var datum3 = datum1.AddDays(2);
                var result1 = new SubscriptionDeliveryResult
                {
                    Id = DataFormats.CreateEntityIdentifier(),
                    SubscriptionId = subscription.Id,
                    AttemptedDateUtc = datum1
                };
                var result2 = new SubscriptionDeliveryResult
                {
                    Id = DataFormats.CreateEntityIdentifier(),
                    SubscriptionId = subscription.Id,
                    AttemptedDateUtc = datum2
                };

                appHost.LoginUser(client, "arelay", WebhookFeature.DefaultRelayRoles);
                client.Put(new UpdateSubscriptionHistory
                {
                    Results = new List<SubscriptionDeliveryResult>
                    {
                        result1,
                        result2
                    }
                });
                client.Put(new UpdateSubscriptionHistory
                {
                    Results = new List<SubscriptionDeliveryResult>
                    {
                        result1,
                        result2,
                        new SubscriptionDeliveryResult
                        {
                            Id = DataFormats.CreateEntityIdentifier(),
                            SubscriptionId = subscription.Id,
                            AttemptedDateUtc = datum3
                        }
                    }
                });

                appHost.LoginUser(client, "asubscriber", WebhookFeature.DefaultSubscriberRoles);
                var result = client.Get(new GetSubscription
                {
                    Id = subscription.Id
                }).History;

                Assert.That(result.Count, Is.EqualTo(3));
                Assert.That(result[0].AttemptedDateUtc, Is.EqualTo(datum3));
                Assert.That(result[1].AttemptedDateUtc, Is.EqualTo(datum2));
                Assert.That(result[2].AttemptedDateUtc, Is.EqualTo(datum1));
            }

            [Test, Category("Integration")]
            public void WhenUpdateSubscriptionHistory_ThenReturnsSubscriptionHistory()
            {
                appHost.LoginUser(client, "asubscriber", WebhookFeature.DefaultSubscriberRoles);
                var subscription = client.Post(new CreateSubscription
                    {
                        Name = "aname",
                        Events = new List<string> {"anevent1"},
                        Config = new SubscriptionConfig
                        {
                            Url = "http://localhost:3333"
                        }
                    }).Subscriptions
                    .First();

                var datum1 = DateTime.UtcNow.ToNearestSecond();
                var datum2 = datum1.AddDays(1);
                appHost.LoginUser(client, "arelay", WebhookFeature.DefaultRelayRoles);
                client.Put(new UpdateSubscriptionHistory
                {
                    Results = new List<SubscriptionDeliveryResult>
                    {
                        new SubscriptionDeliveryResult
                        {
                            Id = DataFormats.CreateEntityIdentifier(),
                            SubscriptionId = subscription.Id,
                            AttemptedDateUtc = datum1
                        },
                        new SubscriptionDeliveryResult
                        {
                            Id = DataFormats.CreateEntityIdentifier(),
                            SubscriptionId = subscription.Id,
                            AttemptedDateUtc = datum2
                        }
                    }
                });

                appHost.LoginUser(client, "asubscriber", WebhookFeature.DefaultSubscriberRoles);
                var result = client.Get(new GetSubscription
                {
                    Id = subscription.Id
                }).History;

                Assert.That(result.Count, Is.EqualTo(2));
                Assert.That(result[0].AttemptedDateUtc, Is.EqualTo(datum2));
                Assert.That(result[1].AttemptedDateUtc, Is.EqualTo(datum1));
            }

            [Test, Category("Integration")]
            public void WhenUpdateSubscriptionHistoryWithA4XX_ThenSubscriptionDeActivated()
            {
                appHost.LoginUser(client, "asubscriber", WebhookFeature.DefaultSubscriberRoles);
                var subscription = client.Post(new CreateSubscription
                    {
                        Name = "aname",
                        Events = new List<string> {"anevent1"},
                        Config = new SubscriptionConfig
                        {
                            Url = "http://localhost:3333"
                        }
                    }).Subscriptions
                    .First();

                var datum = DateTime.UtcNow.ToNearestSecond();
                appHost.LoginUser(client, "arelay", WebhookFeature.DefaultRelayRoles);
                client.Put(new UpdateSubscriptionHistory
                {
                    Results = new List<SubscriptionDeliveryResult>
                    {
                        new SubscriptionDeliveryResult
                        {
                            Id = DataFormats.CreateEntityIdentifier(),
                            SubscriptionId = subscription.Id,
                            AttemptedDateUtc = datum,
                            StatusCode = HttpStatusCode.BadRequest,
                            StatusDescription = "adescription"
                        }
                    }
                });

                appHost.LoginUser(client, "asubscriber", WebhookFeature.DefaultSubscriberRoles);
                var result = client.Get(new GetSubscription
                {
                    Id = subscription.Id
                });

                Assert.That(result.History.Count, Is.EqualTo(1));
                Assert.That(result.History[0].AttemptedDateUtc, Is.EqualTo(datum));
                Assert.That(result.History[0].StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
                Assert.That(result.History[0].StatusDescription, Is.EqualTo("adescription"));
                Assert.That(result.Subscription.IsActive, Is.False);
            }
        }
    }
}