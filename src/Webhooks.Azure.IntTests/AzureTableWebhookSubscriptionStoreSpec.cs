using System.Linq;
using NUnit.Framework;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.Azure.IntTests
{
    public class AzureTableWebhookSubscriptionStoreSpec
    {
        [TestFixture]
        public class GivenATableAndNoUser : AzureIntegrationTestBase
        {
            private AzureTableWebhookSubscriptionStore store;

            [SetUp]
            public void Initialize()
            {
                store = new AzureTableWebhookSubscriptionStore();
                store.Clear();
            }

            [Test, Category("Integration")]
            public void WhenAdd_ThenReturnsId()
            {
                var id = store.Add(new WebhookSubscription
                {
                    Event = "aneventname"
                });

                Assert.That(id.IsEntityId());

                var result = store.Get(null, "aneventname");

                Assert.That(result.Id, Is.EqualTo(id));
            }

            [Test, Category("Integration")]
            public void WhenGetAndUnknownSubscription_ThenReturnsNull()
            {
                var result = store.Get(null, "aneventname");

                Assert.That(result, Is.Null);
            }

            [Test, Category("Integration")]
            public void WhenFind_ThenReturnsNoSubscriptions()
            {
                var results = store.Find(null);

                Assert.That(results.Count, Is.EqualTo(0));
            }

            [Test, Category("Integration")]
            public void WhenGetAndExistingSubscription_ThenReturnsSubscription()
            {
                store.Add(new WebhookSubscription
                {
                    CreatedById = null,
                    Event = "aneventname"
                });

                var result = store.Get(null, "aneventname");

                Assert.That(result.Event, Is.EqualTo("aneventname"));
            }

            [Test, Category("Integration")]
            public void WhenFindAndExistingSubscription_ThenReturnsSubscription()
            {
                store.Add(new WebhookSubscription
                {
                    CreatedById = null,
                    Event = "aneventname"
                });

                var results = store.Find(null)
                    .OrderBy(e => e.Event)
                    .ToList();

                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(results[0].Event, Is.EqualTo("aneventname"));
            }

            [Test, Category("Integration")]
            public void WhenFindAndExistingSubscriptions_ThenReturnsSubscriptions()
            {
                store.Add(new WebhookSubscription
                {
                    CreatedById = null,
                    Event = "aneventname1"
                });
                store.Add(new WebhookSubscription
                {
                    CreatedById = null,
                    Event = "aneventname2"
                });

                var results = store.Find(null)
                    .OrderBy(e => e.Event)
                    .ToList();

                Assert.That(results.Count, Is.EqualTo(2));
                Assert.That(results[0].Event, Is.EqualTo("aneventname1"));
                Assert.That(results[1].Event, Is.EqualTo("aneventname2"));
            }

            [Test, Category("Integration")]
            public void WhenUpdateAndExists_ThenUpdates()
            {
                var subscription = new WebhookSubscription
                {
                    CreatedById = null,
                    Event = "aneventname1"
                };

                var subscriptionId = store.Add(subscription);
                subscription.Name = "anewname";

                store.Update(subscriptionId, subscription);

                var result = store.Get(null, "aneventname1");

                Assert.That(result.Id, Is.EqualTo(subscriptionId));
                Assert.That(result.Event, Is.EqualTo("aneventname1"));
                Assert.That(result.Name, Is.EqualTo("anewname"));
            }

            [Test, Category("Integration")]
            public void WhenDeleteAndExists_ThenDeletes()
            {
                var subscription = new WebhookSubscription
                {
                    CreatedById = null,
                    Event = "aneventname1"
                };

                var subscriptionId = store.Add(subscription);
                subscription.Name = "anewname";

                store.Delete(subscriptionId);

                var result = store.Get(null, "aneventname1");

                Assert.That(result, Is.Null);
            }
        }

        [TestFixture]
        public class GivenATableAndAUser : AzureIntegrationTestBase
        {
            private AzureTableWebhookSubscriptionStore store;

            [SetUp]
            public void Initialize()
            {
                store = new AzureTableWebhookSubscriptionStore();
                store.Clear();
            }

            [Test, Category("Integration")]
            public void WhenAdd_ThenReturnsId()
            {
                var id = store.Add(new WebhookSubscription
                {
                    Event = "aneventname",
                    CreatedById = "auserid"
                });

                Assert.That(id.IsEntityId());

                var result = store.Get("auserid", "aneventname");

                Assert.That(result.Id, Is.EqualTo(id));
            }

            [Test, Category("Integration")]
            public void WhenGetAndUnknownSubscription_ThenReturnsNull()
            {
                var result = store.Get("auserid", "aneventname");

                Assert.That(result, Is.Null);
            }

            [Test, Category("Integration")]
            public void WhenFind_ThenReturnsNoSubscriptions()
            {
                var results = store.Find("auserid");

                Assert.That(results.Count, Is.EqualTo(0));
            }

            [Test, Category("Integration")]
            public void WhenGetAndExistingSubscription_ThenReturnsSubscription()
            {
                store.Add(new WebhookSubscription
                {
                    CreatedById = "auserid",
                    Event = "aneventname"
                });

                var result = store.Get("auserid", "aneventname");

                Assert.That(result.Event, Is.EqualTo("aneventname"));
            }

            [Test, Category("Integration")]
            public void WhenFindAndExistingSubscription_ThenReturnsSubscription()
            {
                store.Add(new WebhookSubscription
                {
                    CreatedById = "auserid",
                    Event = "aneventname"
                });

                var results = store.Find("auserid")
                    .OrderBy(e => e.Event)
                    .ToList();

                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(results[0].Event, Is.EqualTo("aneventname"));
            }

            [Test, Category("Integration")]
            public void WhenFindAndExistingSubscriptions_ThenReturnsSubscriptions()
            {
                store.Add(new WebhookSubscription
                {
                    CreatedById = "auserid",
                    Event = "aneventname1"
                });
                store.Add(new WebhookSubscription
                {
                    CreatedById = "auserid",
                    Event = "aneventname2"
                });

                var results = store.Find("auserid")
                    .OrderBy(e => e.Event)
                    .ToList();

                Assert.That(results.Count, Is.EqualTo(2));
                Assert.That(results[0].Event, Is.EqualTo("aneventname1"));
                Assert.That(results[1].Event, Is.EqualTo("aneventname2"));
            }

            [Test, Category("Integration")]
            public void WhenUpdateAndExists_ThenUpdates()
            {
                var subscription = new WebhookSubscription
                {
                    CreatedById = "auserid",
                    Event = "aneventname1"
                };

                var subscriptionId = store.Add(subscription);
                subscription.Name = "anewname";

                store.Update(subscriptionId, subscription);

                var result = store.Get("auserid", "aneventname1");

                Assert.That(result.Id, Is.EqualTo(subscriptionId));
                Assert.That(result.Event, Is.EqualTo("aneventname1"));
                Assert.That(result.Name, Is.EqualTo("anewname"));
                Assert.That(result.CreatedById, Is.EqualTo("auserid"));
            }

            [Test, Category("Integration")]
            public void WhenDeleteAndExists_ThenDeletes()
            {
                var subscription = new WebhookSubscription
                {
                    CreatedById = "auserid",
                    Event = "aneventname1"
                };

                var subscriptionId = store.Add(subscription);
                subscription.Name = "anewname";

                store.Delete(subscriptionId);

                var result = store.Get("auserid", "aneventname1");

                Assert.That(result, Is.Null);
            }
        }
    }
}