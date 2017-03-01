using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ServiceStack.Configuration;
using ServiceStack.Webhooks.Azure.Table;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.Azure.UnitTests
{
    public class AzureTableWebhookSubscriptionStoreSpec
    {
        [TestFixture]
        public class GivenAStore
        {
            private AzureTableWebhookSubscriptionStore store;
            private Mock<IAzureTableStorage> tableStorage;

            [SetUp]
            public void Initialize()
            {
                tableStorage = new Mock<IAzureTableStorage>();
                store = new AzureTableWebhookSubscriptionStore
                {
                    TableStorage = tableStorage.Object
                };
            }

            [Test, Category("Unit")]
            public void WhenCtorWithNoSetting_ThenInitializes()
            {
                store = new AzureTableWebhookSubscriptionStore();

                Assert.That(store.TableName, Is.EqualTo(AzureTableWebhookSubscriptionStore.DefaultTableName));
                Assert.That(store.ConnectionString, Is.EqualTo(AzureTableWebhookSubscriptionStore.DefaultAzureConnectionString));
            }

            [Test, Category("Unit")]
            public void WhenCtorWithSettings_ThenInitializesFromSettings()
            {
                var appSettings = new Mock<IAppSettings>();
                appSettings.Setup(settings => settings.Get(AzureTableWebhookSubscriptionStore.AzureConnectionStringSettingName, It.IsAny<string>()))
                    .Returns("aconnectionstring");

                store = new AzureTableWebhookSubscriptionStore(appSettings.Object);

                Assert.That(store.TableName, Is.EqualTo(AzureTableWebhookSubscriptionStore.DefaultTableName));
                Assert.That(store.ConnectionString, Is.EqualTo("aconnectionstring"));
            }

            [Test, Category("Unit")]
            public void WhenAddWithNullSubscription_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() => store.Add(null));
            }

            [Test, Category("Unit")]
            public void WhenAdd_ThenAddsToStorage()
            {
                var subscription = new WebhookSubscription();

                var result = store.Add(subscription);

                Assert.That(result.IsEntityId());
                Assert.That(subscription.Id.IsEntityId());
                Assert.That(subscription.Id, Is.EqualTo(result));
                tableStorage.Verify(ts => ts.Add(It.Is<WebhookSubscriptionEntity>(wse =>
                        wse.Id == result)));
            }

            [Test, Category("Unit")]
            public void WhenFindWithUserId_ThenReturnsAll()
            {
                tableStorage.Setup(ts => ts.Find(It.IsAny<TableStorageQuery>()))
                    .Returns(new List<WebhookSubscriptionEntity>
                    {
                        new WebhookSubscriptionEntity
                        {
                            Id = "asubscriptionentityid"
                        }
                    });

                var result = store.Find("auserid");

                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result[0].Id, Is.EqualTo("asubscriptionentityid"));
                tableStorage.Verify(ts => ts.Find(It.Is<TableStorageQuery>(tsq =>
                    (tsq.Parts.Count == 1)
                    && (tsq.Parts[0].PropertyName == "CreatedById")
                    && (tsq.Parts[0].Operation == QueryOperator.EQ)
                    && (tsq.Parts[0].Value.ToString() == "auserid"))));
            }

            [Test, Category("Unit")]
            public void WhenSearchWithEventName_ThenReturnsAll()
            {
                tableStorage.Setup(ts => ts.Find(It.IsAny<TableStorageQuery>()))
                    .Returns(new List<WebhookSubscriptionEntity>
                    {
                        new WebhookSubscriptionEntity
                        {
                            Id = "asubscriptionentityid",
                            Config = new SubscriptionConfig
                            {
                                Url = "aurl"
                            }.ToJson()
                        }
                    });

                var result = store.Search("aneventname");

                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result[0].Url, Is.EqualTo("aurl"));
                tableStorage.Verify(ts => ts.Find(It.Is<TableStorageQuery>(tsq =>
                    (tsq.Parts.Count == 1)
                    && (tsq.Parts[0].PropertyName == "Event")
                    && (tsq.Parts[0].Operation == QueryOperator.EQ)
                    && (tsq.Parts[0].Value.ToString() == "aneventname"))));
            }

            [Test, Category("Unit")]
            public void WhenGetWithNullEventName_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() => store.Get(null, null));
            }

            [Test, Category("Unit")]
            public void WhenGet_ThenReturnsFirst()
            {
                tableStorage.Setup(ts => ts.Find(It.IsAny<TableStorageQuery>()))
                    .Returns(new List<WebhookSubscriptionEntity>
                    {
                        new WebhookSubscriptionEntity
                        {
                            Id = "asubscriptionentityid"
                        }
                    });

                var result = store.Get("auserid", "aneventname");

                Assert.That(result.Id, Is.EqualTo("asubscriptionentityid"));
                tableStorage.Verify(ts => ts.Find(It.Is<TableStorageQuery>(tsq =>
                        (tsq.Parts.Count == 2)
                        && (tsq.Parts[0].PropertyName == "CreatedById")
                        && (tsq.Parts[0].Operation == QueryOperator.EQ)
                        && (tsq.Parts[0].Value.ToString() == "auserid")
                        && (tsq.Parts[1].PropertyName == "Event")
                        && (tsq.Parts[1].Operation == QueryOperator.EQ)
                        && (tsq.Parts[1].Value.ToString() == "aneventname")
                )));
            }

            [Test, Category("Unit")]
            public void WhenUpdateWithNullId_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() => store.Update(null, new WebhookSubscription()));
            }

            [Test, Category("Unit")]
            public void WhenUpdateWithNullSubscription_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() => store.Update("asubscriptionid", null));
            }

            [Test, Category("Unit")]
            public void WhenUpdateAndNotExists_ThenDoesNotUpdate()
            {
                tableStorage.Setup(ts => ts.Get(It.IsAny<string>()))
                    .Returns((WebhookSubscriptionEntity) null);

                store.Update("asubscriptionid", new WebhookSubscription());

                tableStorage.Verify(ts => ts.Get("asubscriptionid"));
                tableStorage.Verify(ts => ts.Update(It.IsAny<WebhookSubscriptionEntity>()), Times.Never);
            }

            [Test, Category("Unit")]
            public void WhenUpdate_ThenUpdates()
            {
                tableStorage.Setup(ts => ts.Get("asubscriptionid"))
                    .Returns(new WebhookSubscriptionEntity());
                var subscription = new WebhookSubscription
                {
                    Id = "asubscriptionid"
                };

                store.Update("asubscriptionid", subscription);

                tableStorage.Verify(ts => ts.Get("asubscriptionid"));
                tableStorage.Verify(ts => ts.Update(It.Is<WebhookSubscriptionEntity>(wse =>
                        wse.Id == "asubscriptionid")));
            }

            [Test, Category("Unit")]
            public void WhenDeleteWithNullId_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() => store.Delete(null));
            }

            [Test, Category("Unit")]
            public void WhenDeleteAndNotExists_ThenDoesNotDelete()
            {
                tableStorage.Setup(ts => ts.Get(It.IsAny<string>()))
                    .Returns((WebhookSubscriptionEntity) null);

                store.Delete("asubscriptionid");

                tableStorage.Verify(ts => ts.Get("asubscriptionid"));
                tableStorage.Verify(ts => ts.Delete(It.IsAny<WebhookSubscriptionEntity>()), Times.Never);
            }

            [Test, Category("Unit")]
            public void WhenDelete_ThenDeletes()
            {
                tableStorage.Setup(ts => ts.Get("asubscriptionid"))
                    .Returns(new WebhookSubscriptionEntity
                    {
                        Id = "asubscriptionid"
                    });

                store.Delete("asubscriptionid");

                tableStorage.Verify(ts => ts.Get("asubscriptionid"));
                tableStorage.Verify(ts => ts.Delete(It.Is<WebhookSubscriptionEntity>(wse =>
                        wse.Id == "asubscriptionid")));
            }

            [Test, Category("Unit")]
            public void WhenClear_ThenEmptiesStore()
            {
                store.Clear();

                tableStorage.Verify(qs => qs.Empty());
            }
        }
    }
}