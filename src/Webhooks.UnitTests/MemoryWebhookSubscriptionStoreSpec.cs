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
            private Mock<ICacheClient> _cacheClient;
            private MemoryWebhookSubscriptionStore _store;

            [SetUp]
            public void Initialize()
            {
                _cacheClient = new Mock<ICacheClient>();
                _cacheClient.As<ICacheClientExtended>().Setup(cc => cc.GetKeysByPattern(It.IsAny<string>()))
                    .Returns(new List<string>());

                _cacheClient.Setup(cc => cc.Add(It.IsAny<string>(), It.IsAny<WebhookSubscription>()));
                _store = new MemoryWebhookSubscriptionStore
                {
                    CacheClient = _cacheClient.Object
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
                var result = _store.Add(subscription);

                Assert.That(result.IsGuid(), Is.True);
                _cacheClient.Verify(cc => cc.Add(MemoryWebhookSubscriptionStore.FormatCacheKey("auserid", "aneventname"), It.Is<WebhookSubscription>(sub
                    => sub.Id.IsGuid())));
            }

            [Test, Category("Unit")]
            public void WhenGet_ThenReturnsSubscriptions()
            {
                var subscription = new WebhookSubscription();
                _cacheClient.Setup(cc => cc.GetAll<WebhookSubscription>(It.IsAny<List<string>>()))
                    .Returns(new Dictionary<string, WebhookSubscription>
                    {
                        {"akey", subscription}
                    });

                var result = _store.Get("auserid", "aneventname");

                Assert.That(result, Is.EqualTo(subscription));
                _cacheClient.As<ICacheClientExtended>().Verify(cc => cc.GetKeysByPattern(MemoryWebhookSubscriptionStore.FormatCacheKey("auserid", "aneventname") + "*"));
                _cacheClient.Verify(cc => cc.GetAll<WebhookSubscription>(It.IsAny<List<string>>()));
            }

            [Test, Category("Unit")]
            public void WhenFind_ThenReturnsSubscriptions()
            {
                var subscription = new WebhookSubscription();
                _cacheClient.Setup(cc => cc.GetAll<WebhookSubscription>(It.IsAny<List<string>>()))
                    .Returns(new Dictionary<string, WebhookSubscription>
                    {
                        {"akey", subscription}
                    });

                var result = _store.Find("auserid");

                Assert.That(result[0], Is.EqualTo(subscription));
                _cacheClient.As<ICacheClientExtended>().Verify(cc => cc.GetKeysByPattern(MemoryWebhookSubscriptionStore.FormatCacheKey("auserid", null) + "*"));
                _cacheClient.Verify(cc => cc.GetAll<WebhookSubscription>(It.IsAny<List<string>>()));
            }
        }
    }
}