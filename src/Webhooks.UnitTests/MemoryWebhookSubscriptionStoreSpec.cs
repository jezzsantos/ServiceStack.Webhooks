using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ServiceStack.Caching;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.UnitTests
{
    public class MemoryWebhookSubscriptionStoreSpec
    {
        [TestFixture]
        public class GivenACacheClient
        {
            private Mock<ICacheClient> cacheClient;
            private MemoryWebhookSubscriptionStore store;

            [SetUp]
            public void Initialize()
            {
                cacheClient = new Mock<ICacheClient>();
                cacheClient.As<ICacheClientExtended>().Setup(cc => cc.GetKeysByPattern(It.IsAny<string>()))
                    .Returns(new List<string>());

                cacheClient.Setup(cc => cc.Add(It.IsAny<string>(), It.IsAny<WebhookSubscription>()));
                store = new MemoryWebhookSubscriptionStore
                {
                    CacheClient = cacheClient.Object
                };
            }

            [Test, Category("Unit")]
            public void WhenFormatCacheKeyAndNullUserId_ThenReturnsAnonymousUserkey()
            {
                var result = MemoryWebhookSubscriptionStore.FormatCacheKey(null, "aneventname");

                Assert.That(result, Is.EqualTo(MemoryWebhookSubscriptionStore.FormatCacheKey(MemoryWebhookSubscriptionStore.CacheKeyForAnonymousUser, "aneventname")));
            }

            [Test, Category("Unit")]
            public void WhenFormatCacheKeyAndUserId_ThenReturnsUserkey()
            {
                var result = MemoryWebhookSubscriptionStore.FormatCacheKey("auserid", "aneventname");

                Assert.That(result, Is.EqualTo(MemoryWebhookSubscriptionStore.FormatCacheKey("auserid", "aneventname")));
            }

            [Test, Category("Unit")]
            public void WhenAddWithNullSubscription_ThenThrows()
            {
                Assert.That(() => store.Add(null), Throws.ArgumentNullException);
            }

            [Test, Category("Unit")]
            public void WhenAdd_ThenReturnsId()
            {
                var subscription = new WebhookSubscription
                {
                    Event = "aneventname",
                    CreatedById = "auserid"
                };
                var result = store.Add(subscription);

                Assert.That(result.IsEntityId(), Is.True);
                cacheClient.Verify(cc => cc.Add(MemoryWebhookSubscriptionStore.FormatCacheKey("auserid", "aneventname"), It.Is<WebhookSubscription>(sub
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
                cacheClient.Setup(cc => cc.GetAll<WebhookSubscription>(It.IsAny<List<string>>()))
                    .Returns(new Dictionary<string, WebhookSubscription>
                    {
                        {"akey", subscription}
                    });

                var result = store.Get("auserid", "aneventname");

                Assert.That(result, Is.EqualTo(subscription));
                cacheClient.As<ICacheClientExtended>().Verify(cc => cc.GetKeysByPattern(MemoryWebhookSubscriptionStore.FormatCacheKey("auserid", "aneventname") + "*"));
                cacheClient.Verify(cc => cc.GetAll<WebhookSubscription>(It.IsAny<List<string>>()));
            }

            [Test, Category("Unit")]
            public void WhenFind_ThenReturnsSubscriptions()
            {
                var subscription = new WebhookSubscription();
                cacheClient.Setup(cc => cc.GetAll<WebhookSubscription>(It.IsAny<List<string>>()))
                    .Returns(new Dictionary<string, WebhookSubscription>
                    {
                        {"akey", subscription}
                    });

                var result = store.Find("auserid");

                Assert.That(result[0], Is.EqualTo(subscription));
                cacheClient.As<ICacheClientExtended>().Verify(cc => cc.GetKeysByPattern(MemoryWebhookSubscriptionStore.FormatCacheKey("auserid", null) + "*"));
                cacheClient.Verify(cc => cc.GetAll<WebhookSubscription>(It.IsAny<List<string>>()));
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
                cacheClient.Setup(cc => cc.GetAll<WebhookSubscription>(It.IsAny<List<string>>()))
                    .Returns(new Dictionary<string, WebhookSubscription>
                    {
                        {"akey", subscription}
                    });

                var result = store.Search("aneventname");

                Assert.That(result[0], Is.EqualTo(config));
                cacheClient.As<ICacheClientExtended>().Verify(cc => cc.GetKeysByPattern(MemoryWebhookSubscriptionStore.CachekeyPrefix + "*"));
                cacheClient.Verify(cc => cc.GetAll<WebhookSubscription>(It.IsAny<List<string>>()));
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
                cacheClient.As<ICacheClientExtended>().Setup(cc => cc.GetKeysByPattern(It.IsAny<string>()))
                    .Returns(new List<string>());
                cacheClient.Setup(cc => cc.GetAll<WebhookSubscription>(It.IsAny<IEnumerable<string>>()))
                    .Returns(new Dictionary<string, WebhookSubscription>());

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
                cacheClient.As<ICacheClientExtended>().Setup(cc => cc.GetKeysByPattern(It.IsAny<string>()))
                    .Returns(new List<string> {"akey"});
                cacheClient.Setup(cc => cc.GetAll<WebhookSubscription>(It.IsAny<IEnumerable<string>>()))
                    .Returns(new Dictionary<string, WebhookSubscription>
                    {
                        {"akey", subscription}
                    });

                store.Update("asubscriptionid", subscription);

                cacheClient.Verify(cc => cc.Set("akey", It.Is<WebhookSubscription>(sub
                    => sub.Id == "asubscriptionid")));
            }

            [Test, Category("Unit")]
            public void WhenDeleteWithNullSubscription_ThenThrows()
            {
                Assert.That(() => store.Delete("asubscriptionid"), Throws.ArgumentNullException);
            }

            [Test, Category("Unit")]
            public void WhenDeleteAndNotExists_ThenDoesNotDeleteSubscription()
            {
                cacheClient.As<ICacheClientExtended>().Setup(cc => cc.GetKeysByPattern(It.IsAny<string>()))
                    .Returns(new List<string>());
                cacheClient.Setup(cc => cc.GetAll<WebhookSubscription>(It.IsAny<IEnumerable<string>>()))
                    .Returns(new Dictionary<string, WebhookSubscription>());

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
                cacheClient.As<ICacheClientExtended>().Setup(cc => cc.GetKeysByPattern(It.IsAny<string>()))
                    .Returns(new List<string> {"akey"});
                cacheClient.Setup(cc => cc.GetAll<WebhookSubscription>(It.IsAny<IEnumerable<string>>()))
                    .Returns(new Dictionary<string, WebhookSubscription>
                    {
                        {"akey", subscription}
                    });

                store.Delete("asubscriptionid");

                cacheClient.Verify(cc => cc.Remove("akey"));
            }
        }
    }
}