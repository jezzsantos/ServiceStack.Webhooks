using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ServiceStack.Caching;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.UnitTests
{
    public class MemoryWebhookEventStoreSpec
    {
        [TestFixture]
        public class GivenACacheClient
        {
            private Mock<ICacheClient> cacheClient;
            private MemoryWebhookEventStore store;

            [SetUp]
            public void Initialize()
            {
                cacheClient = new Mock<ICacheClient>();
                cacheClient.As<ICacheClientExtended>().Setup(cc => cc.GetKeysByPattern(It.IsAny<string>()))
                    .Returns(new List<string>());

                cacheClient.Setup(cc => cc.Add(It.IsAny<string>(), It.IsAny<WebhookSubscription>()));
                store = new MemoryWebhookEventStore
                {
                    CacheClient = cacheClient.Object
                };
            }

            [Test, Category("Unit")]
            public void WhenFormatCacheKey_ThenReturnsEventKey()
            {
                var result = MemoryWebhookEventStore.FormatCacheKey("aneventname");

                var keyPrefix = RemoveDateSuffix(MemoryWebhookEventStore.FormatCacheKey("aneventname"));
                var now = DateTime.UtcNow.ToNearestMillisecond().Subtract(TimeSpan.FromSeconds(1));
                var keys = Enumerable.Range(0, 2000)
                    .Select(ms => "{0}:{1}".Fmt(keyPrefix, now.AddMilliseconds(ms).ToIso8601()));

                Assert.That(keys, Has.Member(result));
            }

            [Test, Category("Unit")]
            public void WhenCreateWithNullEventName_ThenThrows()
            {
                Assert.That(() => store.Create(null, "adata"), Throws.ArgumentNullException);
            }

            [Test, Category("Unit")]
            public void WhenCreateWithNullData_ThenAddsEvent()
            {
                store.Create("aneventname", (string) null);

                cacheClient.Verify(cc => cc.Add(It.Is<string>(s => s.StartsWith(RemoveDateSuffix(MemoryWebhookEventStore.FormatCacheKey("aneventname")))), It.Is<WebhookEvent>(whe =>
                    whe.CreatedDateUtc.IsNear(DateTime.UtcNow)
                    && (whe.EventName == "aneventname")
                    && (whe.Data == null))));
            }

            [Test, Category("Unit")]
            public void WhenCreate_ThenAddsEvent()
            {
                store.Create("aneventname", "adata");

                cacheClient.Verify(cc => cc.Add(It.Is<string>(s => s.StartsWith(RemoveDateSuffix(MemoryWebhookEventStore.FormatCacheKey("aneventname")))), It.Is<WebhookEvent>(whe =>
                    whe.CreatedDateUtc.IsNear(DateTime.UtcNow)
                    && (whe.EventName == "aneventname")
                    && ((string) whe.Data == "adata"))));
            }

            [Test, Category("Unit")]
            public void WhenPeek_ThenReturnsAllEvents()
            {
                cacheClient.As<ICacheClientExtended>().Setup(cc => cc.GetKeysByPattern(It.IsAny<string>()))
                    .Returns(new List<string>());
                cacheClient.Setup(cc => cc.GetAll<WebhookEvent>(It.IsAny<List<string>>()))
                    .Returns(new Dictionary<string, WebhookEvent>());

                var results = store.Peek();

                cacheClient.As<ICacheClientExtended>().Verify(cc => cc.GetKeysByPattern(It.IsAny<string>()));
                cacheClient.Verify(cc => cc.GetAll<WebhookEvent>(It.IsAny<List<string>>()));
            }

            [Test, Category("Unit")]
            public void WhenPeek_ThenReturnsAllEventsSortedByCreatedDateDescending()
            {
                var now = DateTime.UtcNow.ToNearestSecond();
                cacheClient.As<ICacheClientExtended>().Setup(cc => cc.GetKeysByPattern(It.IsAny<string>()))
                    .Returns(new List<string>());
                cacheClient.Setup(cc => cc.GetAll<WebhookEvent>(It.IsAny<List<string>>()))
                    .Returns(new Dictionary<string, WebhookEvent>
                    {
                        {
                            "anevent1", new WebhookEvent
                            {
                                EventName = "anevent1",
                                CreatedDateUtc = now.Subtract(TimeSpan.FromSeconds(1))
                            }
                        },
                        {
                            "anevent2", new WebhookEvent
                            {
                                EventName = "anevent2",
                                CreatedDateUtc = now.Add(TimeSpan.FromSeconds(1))
                            }
                        }
                    });

                var results = store.Peek();

                Assert.That(results.Count, Is.EqualTo(2));
                Assert.That(results[0].EventName, Is.EqualTo("anevent2"));
                cacheClient.As<ICacheClientExtended>().Verify(cc => cc.GetKeysByPattern(It.IsAny<string>()));
                cacheClient.Verify(cc => cc.GetAll<WebhookEvent>(It.IsAny<List<string>>()));
            }

            private static string RemoveDateSuffix(string key)
            {
                var index = key.IndexOf(":", key.IndexOf(":") + 1);

                return key.Substring(0, index);
            }
        }
    }
}