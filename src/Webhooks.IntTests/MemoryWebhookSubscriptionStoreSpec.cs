using System.Linq;
using NUnit.Framework;
using ServiceStack.Caching;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.IntTests
{
    public class MemoryWebhookSubscriptionStoreSpec
    {
        [TestFixture]
        public class GivenNoUser
        {
            private MemoryWebhookSubscriptionStore _store;

            [SetUp]
            public void Initialize()
            {
                _store = new MemoryWebhookSubscriptionStore
                {
                    CacheClient = new MemoryCacheClient()
                };
            }

            [Test, Category("Integration")]
            public void WhenAdd_ThenReturnsId()
            {
                var id = _store.Add(new WebhookSubscription
                {
                    Event = "aneventname"
                });

                Assert.That(id.IsGuid());

                var result = _store.Get(null, "aneventname");

                Assert.That(result.Id, Is.EqualTo(id));
            }

            [Test, Category("Integration")]
            public void WhenGetAndUnknownSubscription_ThenReturnsNull()
            {
                var result = _store.Get(null, "aneventname");

                Assert.That(result, Is.Null);
            }

            [Test, Category("Integration")]
            public void WhenFind_ThenReturnsNoSubscriptions()
            {
                var results = _store.Find(null);

                Assert.That(results.Count, Is.EqualTo(0));
            }

            [Test, Category("Integration")]
            public void WhenGetAndExistingSubscription_ThenReturnsSubscription()
            {
                _store.Add(new WebhookSubscription
                {
                    CreatedById = null,
                    Event = "aneventname"
                });

                var result = _store.Get(null, "aneventname");

                Assert.That(result.Event, Is.EqualTo("aneventname"));
            }

            [Test, Category("Integration")]
            public void WhenFindAndExistingSubscription_ThenReturnsSubscription()
            {
                _store.Add(new WebhookSubscription
                {
                    CreatedById = null,
                    Event = "aneventname"
                });

                var results = _store.Find(null)
                    .OrderBy(e => e.Event)
                    .ToList();

                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(results[0].Event, Is.EqualTo("aneventname"));
            }

            [Test, Category("Integration")]
            public void WhenFindAndExistingSubscriptions_ThenReturnsSubscriptions()
            {
                _store.Add(new WebhookSubscription
                {
                    CreatedById = null,
                    Event = "aneventname1"
                });
                _store.Add(new WebhookSubscription
                {
                    CreatedById = null,
                    Event = "aneventname2"
                });

                var results = _store.Find(null)
                    .OrderBy(e => e.Event)
                    .ToList();

                Assert.That(results.Count, Is.EqualTo(2));
                Assert.That(results[0].Event, Is.EqualTo("aneventname1"));
                Assert.That(results[1].Event, Is.EqualTo("aneventname2"));
            }
        }

        [TestFixture]
        public class GivenAUser
        {
            private MemoryWebhookSubscriptionStore _store;

            [SetUp]
            public void Initialize()
            {
                _store = new MemoryWebhookSubscriptionStore
                {
                    CacheClient = new MemoryCacheClient()
                };
            }

            [Test, Category("Integration")]
            public void WhenAdd_ThenReturnsId()
            {
                var id = _store.Add(new WebhookSubscription
                {
                    Event = "aneventname",
                    CreatedById = "auserid"
                });

                Assert.That(id.IsGuid());

                var result = _store.Get("auserid", "aneventname");

                Assert.That(result.Id, Is.EqualTo(id));
            }

            [Test, Category("Integration")]
            public void WhenGetAndUnknownSubscription_ThenReturnsNull()
            {
                var result = _store.Get("auserid", "aneventname");

                Assert.That(result, Is.Null);
            }

            [Test, Category("Integration")]
            public void WhenFind_ThenReturnsNoSubscriptions()
            {
                var results = _store.Find("auserid");

                Assert.That(results.Count, Is.EqualTo(0));
            }

            [Test, Category("Integration")]
            public void WhenGetAndExistingSubscription_ThenReturnsSubscription()
            {
                _store.Add(new WebhookSubscription
                {
                    CreatedById = "auserid",
                    Event = "aneventname"
                });

                var result = _store.Get("auserid", "aneventname");

                Assert.That(result.Event, Is.EqualTo("aneventname"));
            }

            [Test, Category("Integration")]
            public void WhenFindAndExistingSubscription_ThenReturnsSubscription()
            {
                _store.Add(new WebhookSubscription
                {
                    CreatedById = "auserid",
                    Event = "aneventname"
                });

                var results = _store.Find("auserid")
                    .OrderBy(e => e.Event)
                    .ToList();

                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(results[0].Event, Is.EqualTo("aneventname"));
            }

            [Test, Category("Integration")]
            public void WhenFindAndExistingSubscriptions_ThenReturnsSubscriptions()
            {
                _store.Add(new WebhookSubscription
                {
                    CreatedById = "auserid",
                    Event = "aneventname1"
                });
                _store.Add(new WebhookSubscription
                {
                    CreatedById = "auserid",
                    Event = "aneventname2"
                });

                var results = _store.Find("auserid")
                    .OrderBy(e => e.Event)
                    .ToList();

                Assert.That(results.Count, Is.EqualTo(2));
                Assert.That(results[0].Event, Is.EqualTo("aneventname1"));
                Assert.That(results[1].Event, Is.EqualTo("aneventname2"));
            }
        }
    }
}