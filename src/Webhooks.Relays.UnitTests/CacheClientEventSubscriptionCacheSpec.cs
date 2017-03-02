using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ServiceStack.Caching;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.Relays.UnitTests
{
    public class CacheClientEventSubscriptionCacheSpec
    {
        [TestFixture]
        public class GivenAContext
        {
            private CacheClientEventSubscriptionCache cache;
            private Mock<ICacheClient> cacheClient;
            private Mock<ISubscriptionService> subscriptionService;

            [SetUp]
            public void Initialize()
            {
                subscriptionService = new Mock<ISubscriptionService>();
                cacheClient = new Mock<ICacheClient>();
                cache = new CacheClientEventSubscriptionCache
                {
                    SubscriptionService = subscriptionService.Object,
                    CacheClient = cacheClient.Object
                };
            }

            [Test, Category("Unit")]
            public void WhenGetAllWithNullEventName_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() => cache.GetAll(null));
            }

            [Test, Category("Unit")]
            public void WhenGetAllAndNothingInCache_ThenCachesSubscriptions()
            {
                var config = new SubscriptionConfig();
                var subscribers = new List<SubscriptionConfig>
                {
                    config
                };
                cacheClient.Setup(cc => cc.Get<CachedSubscription>(It.IsAny<string>()))
                    .Returns((CachedSubscription) null);
                subscriptionService.Setup(ss => ss.Search(It.IsAny<string>()))
                    .Returns(subscribers);

                var result = cache.GetAll("aneventname");

                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result[0], Is.EqualTo(config));

                cacheClient.Verify(cc => cc.Get<CachedSubscription>(CacheClientEventSubscriptionCache.FormatCacheKey("aneventname")));
                subscriptionService.Verify(ss => ss.Search("aneventname"));
                cacheClient.Verify(cc => cc.Set(CacheClientEventSubscriptionCache.FormatCacheKey("aneventname"), It.Is<CachedSubscription>(cs =>
                        cs.Subscribers == subscribers), TimeSpan.FromSeconds(cache.ExpiryTimeSeconds)));
            }

            [Test, Category("Unit")]
            public void WhenGetAllAndCached_ThenReturnsSubscriptions()
            {
                var config = new SubscriptionConfig();
                var subscribers = new List<SubscriptionConfig>
                {
                    config
                };
                cacheClient.Setup(cc => cc.Get<CachedSubscription>(It.IsAny<string>()))
                    .Returns(new CachedSubscription
                    {
                        Subscribers = subscribers
                    });

                var result = cache.GetAll("aneventname");

                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result[0], Is.EqualTo(config));

                cacheClient.Verify(cc => cc.Get<CachedSubscription>(CacheClientEventSubscriptionCache.FormatCacheKey("aneventname")));
                subscriptionService.Verify(ss => ss.Search(It.IsAny<string>()), Times.Never);
                cacheClient.Verify(cc => cc.Set(It.IsAny<string>(), It.IsAny<CachedSubscription>()), Times.Never);
            }
        }
    }
}