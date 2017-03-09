using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ServiceStack.Caching;
using ServiceStack.Text;
using ServiceStack.Webhooks.ServiceModel.Types;
using ServiceStack.Webhooks.UnitTesting;

namespace ServiceStack.Webhooks.UnitTests
{
    public class CacheClientSubscriptionStoreSpec
    {
        [TestFixture]
        public class GivenACacheClient
        {
            private Mock<ICacheClient> cacheClient;
            private CacheClientSubscriptionStore store;

            [SetUp]
            public void Initialize()
            {
                cacheClient = new Mock<ICacheClient>();
                cacheClient.As<ICacheClientExtended>().Setup(cc => cc.GetKeysByPattern(It.IsAny<string>()))
                    .Returns(new List<string> {"akey"});

                cacheClient.Setup(cc => cc.Add(It.IsAny<string>(), It.IsAny<WebhookSubscription>()));
                store = new CacheClientSubscriptionStore
                {
                    CacheClient = cacheClient.Object
                };
            }

            [Test, Category("Unit")]
            public void WhenFormatCacheKeyAndNullUserId_ThenReturnsAnonymousUserkey()
            {
                var result = CacheClientSubscriptionStore.FormatCacheKey(null, "aneventname");

                Assert.That(result, Is.EqualTo(CacheClientSubscriptionStore.FormatCacheKey(CacheClientSubscriptionStore.CacheKeyForAnonymousUser, "aneventname")));
            }

            [Test, Category("Unit")]
            public void WhenFormatCacheKeyAndUserId_ThenReturnsUserkey()
            {
                var result = CacheClientSubscriptionStore.FormatCacheKey("auserid", "aneventname");

                Assert.That(result, Is.EqualTo(CacheClientSubscriptionStore.FormatCacheKey("auserid", "aneventname")));
            }

            [Test, Category("Unit")]
            public void WhenAddSubscriptionWithNullSubscription_ThenThrows()
            {
                Assert.That(() => store.Add(null), Throws.ArgumentNullException);
            }

            [Test, Category("Unit")]
            public void WhenAddSubscription_ThenReturnsId()
            {
                var subscription = new WebhookSubscription
                {
                    Event = "aneventname",
                    CreatedById = "auserid"
                };
                var result = store.Add(subscription);

                Assert.That(result.IsEntityId(), Is.True);
                cacheClient.Verify(cc => cc.Add(CacheClientSubscriptionStore.FormatCacheKey("auserid", "aneventname"), It.Is<WebhookSubscription>(sub
                    => sub.Id.IsEntityId())));
            }

            [Test, Category("Unit")]
            public void WhenGetWithNullEventName_ThenThrows()
            {
                Assert.That(() => store.Get("auserid", null), Throws.ArgumentNullException);
            }

            [Test, Category("Unit")]
            public void WhenGet_ThenReturnsSubscriptions()
            {
                var subscription = new WebhookSubscription();
                cacheClient.Setup(cc => cc.Get<object>("akey"))
                    .Returns(subscription);

                var result = store.Get("auserid", "aneventname");

                Assert.That(result, Is.EqualTo(subscription));
                cacheClient.As<ICacheClientExtended>().Verify(cc => cc.GetKeysByPattern(CacheClientSubscriptionStore.FormatCacheKey("auserid", "aneventname") + "*"));
                cacheClient.Verify(cc => cc.Get<object>("akey"));
            }

            [Test, Category("Unit")]
            public void WhenFind_ThenReturnsSubscriptions()
            {
                var subscription = new WebhookSubscription();
                cacheClient.Setup(cc => cc.Get<object>("akey"))
                    .Returns(subscription);

                var result = store.Find("auserid");

                Assert.That(result[0], Is.EqualTo(subscription));
                cacheClient.As<ICacheClientExtended>().Verify(cc => cc.GetKeysByPattern(CacheClientSubscriptionStore.FormatCacheKey("auserid", null) + "*"));
                cacheClient.Verify(cc => cc.Get<object>("akey"));
            }

            [Test, Category("Unit")]
            public void WhenSearch_ThenReturnsSubscriptions()
            {
                var config = new SubscriptionConfig();
                var subscription = new WebhookSubscription
                {
                    Event = "aneventname",
                    Config = config
                };
                cacheClient.Setup(cc => cc.Get<object>("akey"))
                    .Returns(subscription);

                var result = store.Search("aneventname");

                Assert.That(result[0].Config, Is.EqualTo(config));
                cacheClient.As<ICacheClientExtended>().Verify(cc => cc.GetKeysByPattern(CacheClientSubscriptionStore.CachekeyPrefix + "*"));
                cacheClient.Verify(cc => cc.Get<object>("akey"));
            }

            [Test, Category("Unit")]
            public void WhenSearchAndIsActive_ThenReturnsActiveSubscriptionsOnly()
            {
                var config1 = new SubscriptionConfig();
                var config2 = new SubscriptionConfig();
                var subscription1 = new WebhookSubscription
                {
                    Event = "aneventname",
                    Config = config1,
                    IsActive = false
                };
                var subscription2 = new WebhookSubscription
                {
                    Event = "aneventname",
                    Config = config2,
                    IsActive = true
                };
                cacheClient.As<ICacheClientExtended>().Setup(cc => cc.GetKeysByPattern(It.IsAny<string>()))
                    .Returns(new List<string> {"akey1", "akey2"});
                cacheClient.Setup(cc => cc.Get<object>("akey1"))
                    .Returns(subscription1);
                cacheClient.Setup(cc => cc.Get<object>("akey2"))
                    .Returns(subscription2);

                var result = store.Search("aneventname", true);

                Assert.That(result[0].Config, Is.EqualTo(config2));
                cacheClient.As<ICacheClientExtended>().Verify(cc => cc.GetKeysByPattern(CacheClientSubscriptionStore.CachekeyPrefix + "*"));
                cacheClient.Verify(cc => cc.Get<object>("akey1"));
                cacheClient.Verify(cc => cc.Get<object>("akey2"));
            }

            [Test, Category("Unit")]
            public void WhenUpdateWithNullSubscriptionId_ThenThrows()
            {
                Assert.That(() => store.Update(null, new WebhookSubscription()), Throws.ArgumentNullException);
            }

            [Test, Category("Unit")]
            public void WhenUpdateWithNullSubscription_ThenThrows()
            {
                Assert.That(() => store.Update("asubscriptionid", null), Throws.ArgumentNullException);
            }

            [Test, Category("Unit")]
            public void WhenUpdateAndNotExists_ThenDoesNotUpdateSubscription()
            {
                store.Update("asubscriptionid", new WebhookSubscription());

                cacheClient.Verify(cc => cc.Set(It.IsAny<string>(), It.IsAny<WebhookSubscription>()), Times.Never);
            }

            [Test, Category("Unit")]
            public void WhenUpdateAndExists_ThenUpdatesSubscription()
            {
                var subscription = new WebhookSubscription
                {
                    Id = "asubscriptionid"
                };
                cacheClient.Setup(cc => cc.Get<object>("akey"))
                    .Returns(subscription);

                store.Update("asubscriptionid", subscription);

                cacheClient.Verify(cc => cc.Set("akey", It.Is<WebhookSubscription>(sub
                    => sub.Id == "asubscriptionid")));
            }

            [Test, Category("Unit")]
            public void WhenDeleteWithNullSubscriptionId_ThenThrows()
            {
                Assert.That(() => store.Delete(null), Throws.ArgumentNullException);
            }

            [Test, Category("Unit")]
            public void WhenDeleteAndNotExists_ThenDoesNotDeleteSubscription()
            {
                store.Delete("asubscriptionid");

                cacheClient.Verify(cc => cc.Remove(It.IsAny<string>()), Times.Never);
            }

            [Test, Category("Unit")]
            public void WhenDeleteAndExists_ThenDeleteSubscription()
            {
                var subscription = new WebhookSubscription
                {
                    Id = "asubscriptionid"
                };
                cacheClient.Setup(cc => cc.Get<object>("akey"))
                    .Returns(subscription);

                store.Delete("asubscriptionid");

                cacheClient.Verify(cc => cc.Remove("akey"));
            }

            [Test, Category("Unit")]
            public void WhenAddHistoryWithNullSubscriptionId_ThenThrows()
            {
                Assert.That(() => store.Add(null, new SubscriptionDeliveryResult()), Throws.ArgumentNullException);
            }

            [Test, Category("Unit")]
            public void WhenAddHistoryWithNullResult_ThenThrows()
            {
                Assert.That(() => store.Add("asubscriptionid", null), Throws.ArgumentNullException);
            }

            [Test, Category("Unit")]
            public void WhenAddHistory_ThenAddsHistory()
            {
                var datum = SystemTime.UtcNow.ToNearestSecond();
                cacheClient.Setup(cc => cc.Get<object>("akey"))
                    .Returns(new WebhookSubscription
                    {
                        Id = "asubscriptionid",
                        Event = "aneventname",
                        CreatedById = "auserid"
                    });
                var result = new SubscriptionDeliveryResult
                {
                    Id = "aresultid",
                    AttemptedDateUtc = datum
                };

                store.Add("asubscriptionid", result);

                cacheClient.Verify(cc => cc.Add(CacheClientSubscriptionStore.FormatHistoryCacheKey("auserid", "aneventname", "aresultid"), It.Is<SubscriptionDeliveryResult>(sub =>
                    sub.Id == "aresultid"
                    && sub.AttemptedDateUtc == datum)));
            }

            [Test, Category("Unit")]
            public void WhenSearchHistory_ThenReturnsSortedResults()
            {
                var datum1 = SystemTime.UtcNow;
                var datum2 = SystemTime.UtcNow.AddDays(1);
                var subscription = new WebhookSubscription
                {
                    Id = "asubscriptionid",
                    Event = "aneventname",
                    CreatedById = "auserid"
                };
                cacheClient.As<ICacheClientExtended>().Setup(cc => cc.GetKeysByPattern(It.IsAny<string>()))
                    .ReturnsInOrder(new List<string> {"asubscriptionkey"}, new List<string> {"aresultkey1", "aresultkey2"});
                cacheClient.Setup(cc => cc.Get<object>("asubscriptionkey"))
                    .Returns(subscription);
                cacheClient.Setup(cc => cc.Get<object>("aresultkey1"))
                    .Returns(new SubscriptionDeliveryResult
                    {
                        Id = "aresultid1",
                        AttemptedDateUtc = datum1
                    });
                cacheClient.Setup(cc => cc.Get<object>("aresultkey2"))
                    .Returns(new SubscriptionDeliveryResult
                    {
                        Id = "aresultid2",
                        AttemptedDateUtc = datum2
                    });

                var result = store.Search("asubscriptionid", 1);

                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result[0].Id, Is.EqualTo("aresultid2"));
                cacheClient.As<ICacheClientExtended>().Verify(cc => cc.GetKeysByPattern(CacheClientSubscriptionStore.CachekeyPrefix + "*"));
                cacheClient.As<ICacheClientExtended>().Verify(cc => cc.GetKeysByPattern(CacheClientSubscriptionStore.CachekeyFormat.Fmt("auserid", "aneventname") + "*"));
                cacheClient.Verify(cc => cc.Get<object>("asubscriptionkey"));
                cacheClient.Verify(cc => cc.Get<object>("aresultkey1"));
                cacheClient.Verify(cc => cc.Get<object>("aresultkey2"));
            }
        }
    }
}