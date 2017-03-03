using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NUnit.Framework;
using ServiceStack.Caching;
using ServiceStack.Webhooks.IntTests.Services;
using ServiceStack.Webhooks.ServiceModel;
using ServiceStack.Webhooks.ServiceModel.Types;
using ServiceStack.Webhooks.UnitTesting;

namespace ServiceStack.Webhooks.IntTests
{
    public class SubscriptionServiceSpec
    {
        [TestFixture]
        public class GivenTheRegistrationService
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
                appHost = new AppHostForTesting();
                appHost.Init();
                appHost.Start(BaseUrl);

                client = new JsonServiceClient(BaseUrl);
            }

            [SetUp]
            public void Initialize()
            {
                appHost.Resolve<ICacheClient>().FlushAll();
                appHost.LoginUser(client);
            }

            [Test, Category("Integration")]
            public void WhenPostSubscriptionWithNullEvents_ThenThrowsBadRequest()
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
            public void WhenPostSubscriptionWithNullConfig_ThenThrowsBadRequest()
            {
                Assert.That(() => client.Post(new CreateSubscription
                {
                    Name = "aname",
                    Events = new List<string> {"aneventname"},
                    Config = null
                }), ThrowsWebServiceException.WithStatusCode(HttpStatusCode.BadRequest));
            }

            [Test, Category("Integration")]
            public void WhenPostSubscription_ThenCreatesSubscription()
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
                AssertSubscriptionCreated(subscriptions[0], "anevent1", "1");
                AssertSubscriptionCreated(subscriptions[1], "anevent2", "1");
            }

            [Test, Category("Integration")]
            public void WhenPostSubscriptionWithSameEventNameAndUrl_ThenThrowsConflict()
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

            private static void AssertSubscriptionCreated(WebhookSubscription subscription, string eventName, string userId)
            {
                Assert.That(eventName, Is.EqualTo(subscription.Event));
                Assert.That(subscription.CreatedById, Is.EqualTo(userId));
                Assert.That(DateTime.UtcNow, Is.EqualTo(subscription.CreatedDateUtc).Within(5).Seconds);
                Assert.That(DateTime.UtcNow, Is.EqualTo(subscription.LastModifiedDateUtc).Within(5).Seconds);
                Assert.That(subscription.Id.HasValue());
            }
        }
    }
}