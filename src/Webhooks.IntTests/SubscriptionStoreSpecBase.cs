using System.Linq;
using NUnit.Framework;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.IntTests
{
    [TestFixture]
    public abstract class GivenNoUserWithSubscriptionStoreBase
    {
        private ISubscriptionStore store;

        [SetUp]
        public void Initialize()
        {
            store = GetSubscriptionStore();
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
        public void WhenFind_ThenReturnsNoSubscriptions()
        {
            var results = store.Find(null);

            Assert.That(results.Count, Is.EqualTo(0));
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
        public void WhenSearch_ThenReturnsNoSubscriptions()
        {
            var results = store.Search("aneventname", null);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test, Category("Integration")]
        public void WhenSearchAndExistingSubscription_ThenReturnsSubscription()
        {
            store.Add(new WebhookSubscription
            {
                CreatedById = null,
                Event = "aneventname",
                Config = new SubscriptionConfig
                {
                    Url = "aurl"
                }
            });

            var results = store.Search("aneventname", null)
                .ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Config.Url, Is.EqualTo("aurl"));
        }

        [Test, Category("Integration")]
        public void WhenSearchAndExistingSubscriptions_ThenReturnsSubscriptions()
        {
            store.Add(new WebhookSubscription
            {
                CreatedById = null,
                Event = "aneventname1",
                Config = new SubscriptionConfig
                {
                    Url = "aurl1"
                }
            });
            store.Add(new WebhookSubscription
            {
                CreatedById = null,
                Event = "aneventname2",
                Config = new SubscriptionConfig
                {
                    Url = "aurl2"
                }
            });

            var results = store.Search("aneventname1", null)
                .ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Config.Url, Is.EqualTo("aurl1"));
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

        [Test, Category("Integration")]
        public void WhenAddHistory_ThenAdds()
        {
            var subscription = new WebhookSubscription
            {
                CreatedById = null,
                Event = "aneventname1"
            };

            var subscriptionId = store.Add(subscription);

            store.Add(subscriptionId, new SubscriptionDeliveryResult
            {
                Id = "aresultid",
                SubscriptionId = subscriptionId
            });

            var result = store.Search(subscriptionId, 100);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo("aresultid"));
        }

        public abstract ISubscriptionStore GetSubscriptionStore();
    }

    [TestFixture]
    public abstract class GivenAUserWithSubscriptionStoreBase
    {
        private ISubscriptionStore store;

        [SetUp]
        public void Initialize()
        {
            store = GetSubscriptionStore();
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
        public void WhenFind_ThenReturnsNoSubscriptions()
        {
            var results = store.Find("auserid");

            Assert.That(results.Count, Is.EqualTo(0));
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
        public void WhenSearch_ThenReturnsNoSubscriptions()
        {
            var results = store.Search("aneventname", null);

            Assert.That(results.Count, Is.EqualTo(0));
        }

        [Test, Category("Integration")]
        public void WhenSearchAndExistingSubscription_ThenReturnsSubscription()
        {
            store.Add(new WebhookSubscription
            {
                CreatedById = "auserid1",
                Event = "aneventname",
                Config = new SubscriptionConfig
                {
                    Url = "aurl"
                }
            });

            var results = store.Search("aneventname", null)
                .ToList();

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Config.Url, Is.EqualTo("aurl"));
        }

        [Test, Category("Integration")]
        public void WhenSearchAndExistingSubscriptions_ThenReturnsSubscriptions()
        {
            store.Add(new WebhookSubscription
            {
                CreatedById = "auserid1",
                Event = "aneventname",
                Config = new SubscriptionConfig
                {
                    Url = "aurl1"
                }
            });
            store.Add(new WebhookSubscription
            {
                CreatedById = "auserid2",
                Event = "aneventname",
                Config = new SubscriptionConfig
                {
                    Url = "aurl2"
                }
            });

            var results = store.Search("aneventname", null)
                .ToList();

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results[0].Config.Url, Is.EqualTo("aurl1"));
            Assert.That(results[1].Config.Url, Is.EqualTo("aurl2"));
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

        [Test, Category("Integration")]
        public void WhenAddHistory_ThenAdds()
        {
            var subscription = new WebhookSubscription
            {
                CreatedById = null,
                Event = "aneventname1"
            };

            var subscriptionId = store.Add(subscription);

            store.Add(subscriptionId, new SubscriptionDeliveryResult
            {
                Id = "aresultid",
                SubscriptionId = subscriptionId
            });

            var result = store.Search(subscriptionId, 100);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo("aresultid"));
        }

        public abstract ISubscriptionStore GetSubscriptionStore();
    }
}