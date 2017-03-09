using System;
using NUnit.Framework;
using ServiceStack.Text;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.UnitTests
{
    public class MemorySubscriptionStoreSpec
    {
        [TestFixture]
        public class GivenAContext
        {
            private MemorySubscriptionStore store;

            [SetUp]
            public void Initialize()
            {
                store = new MemorySubscriptionStore();
            }

            [Test, Category("Unit")]
            public void WhenAddWithNullSubscription_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    store.Add(null));
            }

            [Test, Category("Unit")]
            public void WhenAdd_ThenReturnsEntityId()
            {
                var result = store.Add(new WebhookSubscription());

                Assert.That(result.IsEntityId);
            }

            [Test, Category("Unit")]
            public void WhenFindWithNullUserId_ThenReturnsMatchingSubscriptions()
            {
                var subscriptionId1 = store.Add(new WebhookSubscription
                {
                    CreatedById = null
                });
                var subscriptionId2 = store.Add(new WebhookSubscription
                {
                    CreatedById = "auserid"
                });

                var results = store.Find(null);

                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(results[0].Id, Is.EqualTo(subscriptionId1));
            }

            [Test, Category("Unit")]
            public void WhenFindWithUserId_ThenReturnsMatchingSubscriptions()
            {
                var subscriptionId1 = store.Add(new WebhookSubscription
                {
                    CreatedById = null
                });
                var subscriptionId2 = store.Add(new WebhookSubscription
                {
                    CreatedById = "auserid"
                });

                var results = store.Find("auserid");

                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(results[0].Id, Is.EqualTo(subscriptionId2));
            }

            [Test, Category("Unit")]
            public void WhenGetByEventNameWithNullEventName_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    store.Get("auserid", null));
            }

            [Test, Category("Unit")]
            public void WhenGetByEventNameAndUnknownSubscription_ThenReturnsNull()
            {
                var results = store.Get("anunknwownuserid", "anevent1");

                Assert.That(results, Is.Null);
            }

            [Test, Category("Unit")]
            public void WhenGetByEventName_ThenReturnsSubscription()
            {
                var subscriptionId1 = store.Add(new WebhookSubscription
                {
                    CreatedById = null,
                    Event = "anevent1"
                });
                var subscriptionId2 = store.Add(new WebhookSubscription
                {
                    CreatedById = "auserid",
                    Event = "anevent1"
                });

                var results = store.Get("auserid", "anevent1");

                Assert.That(results.Id, Is.EqualTo(subscriptionId2));
            }

            [Test, Category("Unit")]
            public void WhenGetBySubscriptionIdWithNullSubscriptionId_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    store.Get(null));
            }

            [Test, Category("Unit")]
            public void WhenGetBySubscriptionIdAndUnknownSubscription_ThenReturnsNull()
            {
                var results = store.Get("anunknownuserid", "anevent1");

                Assert.That(results, Is.Null);
            }

            [Test, Category("Unit")]
            public void WhenUpdateBySubscriptionIdWithNullSubscriptionId_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    store.Update(null, new WebhookSubscription()));
            }

            [Test, Category("Unit")]
            public void WhenUpdateBySubscriptionIdWithNullSubscription_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    store.Update("asubscriptionid", null));
            }

            [Test, Category("Unit")]
            public void WhenUpdateAndUnknownSubscription_ThenNotUpdate()
            {
                store.Update("anunknownuserid", new WebhookSubscription());
            }

            [Test, Category("Unit")]
            public void WhenUpdate_ThenUpdatesSubscription()
            {
                var subscription = new WebhookSubscription
                {
                    CreatedById = null,
                    Event = "anevent1"
                };
                var subscriptionId = store.Add(subscription);
                subscription.Event = "anevent2";
                store.Update(subscriptionId, subscription);

                var results = store.Get(subscriptionId);

                Assert.That(results.Event, Is.EqualTo("anevent2"));
            }

            [Test, Category("Unit")]
            public void WhenDeleteBySubscriptionIdWithNullSubscriptionId_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    store.Delete(null));
            }

            [Test, Category("Unit")]
            public void WhenDeleteAndUnknownSubscription_ThenNotDelete()
            {
                store.Delete("anunknownuserid");
            }

            [Test, Category("Unit")]
            public void WhenDelete_ThenDeletesSubscription()
            {
                var subscription = new WebhookSubscription
                {
                    CreatedById = null,
                    Event = "anevent1"
                };
                var subscriptionId = store.Add(subscription);
                store.Delete(subscriptionId);

                var results = store.Get(subscriptionId);

                Assert.That(results, Is.Null);
            }

            [Test, Category("Unit")]
            public void WhenSearchForConfigWithNullEventName_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    store.Search(null, null));
            }

            [Test, Category("Unit")]
            public void WhenSearchForConfig_ThenReturnsMatchingRelayConfigs()
            {
                var subscriptionId1 = store.Add(new WebhookSubscription
                {
                    Event = "anevent1",
                    Config = new SubscriptionConfig
                    {
                        Url = "aurl1"
                    }
                });
                var subscriptionId2 = store.Add(new WebhookSubscription
                {
                    Event = "anevent2",
                    Config = new SubscriptionConfig
                    {
                        Url = "aurl2"
                    }
                });

                var results = store.Search("anevent1", null);

                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(results[0].SubscriptionId, Is.EqualTo(subscriptionId1));
                Assert.That(results[0].Config.Url, Is.EqualTo("aurl1"));
            }

            [Test, Category("Unit")]
            public void WhenSearchForConfigWithIsActive_ThenReturnsMatchingRelayConfigs()
            {
                var subscriptionId1 = store.Add(new WebhookSubscription
                {
                    Event = "anevent1",
                    Config = new SubscriptionConfig
                    {
                        Url = "aurl1"
                    },
                    IsActive = true
                });
                var subscriptionId2 = store.Add(new WebhookSubscription
                {
                    Event = "anevent1",
                    Config = new SubscriptionConfig
                    {
                        Url = "aurl2"
                    },
                    IsActive = false
                });

                var results = store.Search("anevent1", true);

                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(results[0].SubscriptionId, Is.EqualTo(subscriptionId1));
                Assert.That(results[0].Config.Url, Is.EqualTo("aurl1"));
            }

            [Test, Category("Unit")]
            public void WhenAddDeliveryResultAndNullSubscriptionId_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    store.Add(null, new SubscriptionDeliveryResult()));
            }

            [Test, Category("Unit")]
            public void WhenAddDeliveryResultAndNullDeliveryResult_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    store.Add("asubscriptionid", null));
            }

            [Test, Category("Unit")]
            public void WhenAddDeliveryResultAndUnknownSubscriptionId_ThenDoesNotAddResults()
            {
                store.Add("anunknownsubscriptionid", new SubscriptionDeliveryResult());
            }

            [Test, Category("Unit")]
            public void WhenAddDeliveryResult_ThenAddResults()
            {
                var subscription = new WebhookSubscription();
                var subscriptionId = store.Add(subscription);
                var result = new SubscriptionDeliveryResult
                {
                    Id = "aresultid",
                    SubscriptionId = subscriptionId
                };

                store.Add(subscriptionId, result);

                var results = store.Search(subscriptionId, 1);

                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(results[0].Id, Is.EqualTo("aresultid"));
            }

            [Test, Category("Unit")]
            public void WhenSearchForDeliveryResultsWithNullSubscriptionId_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    store.Search(null, 1));
            }

            [Test, Category("Unit")]
            public void WhenSearchForDeliveryResultsWithZeroTop_ThenThrows()
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    store.Search("asubscriptionid", 0));
            }

            [Test, Category("Unit")]
            public void WhenSearchForDeliveryResults_ThenReturnsMatchingResults()
            {
                var subscription = new WebhookSubscription();
                var subscriptionId = store.Add(subscription);
                var result = new SubscriptionDeliveryResult
                {
                    Id = "aresultid",
                    SubscriptionId = subscriptionId
                };

                store.Add(subscriptionId, result);

                var results = store.Search(subscriptionId, 1);

                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(results[0].Id, Is.EqualTo("aresultid"));
            }

            [Test, Category("Unit")]
            public void WhenSearchForDeliveryResults_ThenReturnsMatchingResultsInOrder()
            {
                var subscription = new WebhookSubscription();
                var subscriptionId = store.Add(subscription);

                var datum1 = SystemTime.UtcNow.ToNearestSecond();
                var datum2 = datum1.AddDays(1);
                var result1 = new SubscriptionDeliveryResult
                {
                    Id = "aresultid1",
                    SubscriptionId = subscriptionId,
                    AttemptedDateUtc = datum1
                };
                var result2 = new SubscriptionDeliveryResult
                {
                    Id = "aresultid2",
                    SubscriptionId = subscriptionId,
                    AttemptedDateUtc = datum2
                };
                store.Add(subscriptionId, result1);
                store.Add(subscriptionId, result2);

                var results = store.Search(subscriptionId, 1);

                Assert.That(results.Count, Is.EqualTo(1));
                Assert.That(results[0].Id, Is.EqualTo("aresultid2"));
            }
        }
    }
}