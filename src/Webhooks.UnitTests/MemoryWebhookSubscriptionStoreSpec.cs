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
            public void WhenAdd_ThenReturnsId()
            {
                var subscription = new WebhookSubscription
                {
                    Event = "aneventname",
                    CreatedById = "auserid"
                };
                var result = store.Add(subscription);

                Assert.That(result.IsGuid(), Is.True);
                cacheClient.Verify(cc => cc.Add(MemoryWebhookSubscriptionStore.FormatCacheKey("auserid", "aneventname"), It.Is<WebhookSubscription>(sub
                    => sub.Id.IsGuid())));
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
        }
    }
}