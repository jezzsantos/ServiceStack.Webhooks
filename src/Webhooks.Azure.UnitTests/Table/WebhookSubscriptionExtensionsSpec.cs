using System;
using NUnit.Framework;
using ServiceStack.Webhooks.Azure.Table;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.Azure.UnitTests.Table
{
    public class WebhookSubscriptionExtensionsSpec
    {
        [TestFixture]
        public class GivenAContext
        {
            [SetUp]
            public void Initialize()
            {
            }

            [Test, Category("Unit")]
            public void WhenToEntityWithNullEntity_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() => ((WebhookSubscription) null).ToEntity());
            }

            [Test, Category("Unit")]
            public void WhenToEntity_ThenReturnsEntity()
            {
                var datum = DateTime.UtcNow;
                var config = new SubscriptionConfig
                {
                    Url = "aurl"
                };
                var result = new WebhookSubscription
                {
                    Id = "asubscriptionid",
                    Config = config,
                    IsActive = true,
                    CreatedById = "auserid",
                    CreatedDateUtc = datum,
                    Event = "anevent",
                    Name = "aname",
                    LastModifiedDateUtc = datum
                }.ToEntity();

                Assert.That(result.RowKey, Is.EqualTo("asubscriptionid"));
                Assert.That(result.PartitionKey, Is.EqualTo(string.Empty));
                Assert.That(result.Id, Is.EqualTo("asubscriptionid"));
                Assert.That(result.Config, Is.EqualTo(config.ToJson()));
                Assert.That(result.IsActive, Is.EqualTo(true.ToString().ToLowerInvariant()));
                Assert.That(result.CreatedById, Is.EqualTo("auserid"));
                Assert.That(result.CreatedDateUtc, Is.EqualTo(datum));
                Assert.That(result.Event, Is.EqualTo("anevent"));
                Assert.That(result.Event, Is.EqualTo("anevent"));
                Assert.That(result.Name, Is.EqualTo("aname"));
                Assert.That(result.LastModifiedDateUtc, Is.EqualTo(datum));
            }

            [Test, Category("Unit")]
            public void WhenFromEntityWithNullDto_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() => ((WebhookSubscriptionEntity) null).FromEntity());
            }

            [Test, Category("Unit")]
            public void WhenFromEntity_ThenReturnsDto()
            {
                var datum = DateTime.UtcNow;
                var config = new SubscriptionConfig
                {
                    Url = "aurl"
                };
                var result = new WebhookSubscriptionEntity
                {
                    Id = "asubscriptionid",
                    Config = config.ToJson(),
                    IsActive = true.ToString().ToLowerInvariant(),
                    CreatedById = "auserid",
                    CreatedDateUtc = datum,
                    Event = "anevent",
                    Name = "aname",
                    LastModifiedDateUtc = datum
                }.FromEntity();

                Assert.That(result.Id, Is.EqualTo("asubscriptionid"));
                Assert.That(result.Config.Url, Is.EqualTo("aurl"));
                Assert.That(result.IsActive, Is.EqualTo(true));
                Assert.That(result.CreatedById, Is.EqualTo("auserid"));
                Assert.That(result.CreatedDateUtc, Is.EqualTo(datum));
                Assert.That(result.Event, Is.EqualTo("anevent"));
                Assert.That(result.Event, Is.EqualTo("anevent"));
                Assert.That(result.Name, Is.EqualTo("aname"));
                Assert.That(result.LastModifiedDateUtc, Is.EqualTo(datum));
            }
        }
    }
}