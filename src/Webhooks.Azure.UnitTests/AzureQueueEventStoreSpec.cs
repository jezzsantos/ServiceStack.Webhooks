using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ServiceStack.Configuration;
using ServiceStack.Webhooks.Azure.Queue;

namespace ServiceStack.Webhooks.Azure.UnitTests
{
    public class AzureQueueEventStoreSpec
    {
        [TestFixture]
        public class GivenAStore
        {
            private Mock<IAzureQueueStorage> queueStorage;
            private AzureQueueEventStore store;

            [SetUp]
            public void Initialize()
            {
                queueStorage = new Mock<IAzureQueueStorage>();
                store = new AzureQueueEventStore
                {
                    QueueStorage = queueStorage.Object
                };
            }

            [Test, Category("Unit")]
            public void WhenCtorWithNoSetting_ThenInitializes()
            {
                store = new AzureQueueEventStore();

                Assert.That(store.QueueName, Is.EqualTo(AzureQueueEventStore.DefaultQueueName));
                Assert.That(store.AzureConnectionString, Is.EqualTo(AzureQueueEventStore.DefaultAzureConnectionString));
            }

            [Test, Category("Unit")]
            public void WhenCtorWithSettings_ThenInitializesFromSettings()
            {
                var appSettings = new Mock<IAppSettings>();
                appSettings.Setup(settings => settings.Get(AzureQueueEventStore.AzureConnectionStringSettingName, It.IsAny<string>()))
                    .Returns("aconnectionstring");

                store = new AzureQueueEventStore(appSettings.Object);

                Assert.That(store.QueueName, Is.EqualTo(AzureQueueEventStore.DefaultQueueName));
                Assert.That(store.AzureConnectionString, Is.EqualTo("aconnectionstring"));
            }

            [Test, Category("Unit")]
            public void WhenCreate_ThenCreatesEvent()
            {
                store.Create("aneventname", "adata");

                queueStorage.Verify(qs => qs.Enqueue(It.Is<WebhookEvent>(whe =>
                        (whe.EventName == "aneventname")
                        && (whe.Data.ToString() == "adata")
                        && whe.CreatedDateUtc.IsNear(DateTime.UtcNow)
                )));
            }

            [Test, Category("Unit")]
            public void WhenPeek_ThenReturnsPeekedEvents()
            {
                var events = new List<WebhookEvent>();
                queueStorage.Setup(qs => qs.Peek())
                    .Returns(events);

                var results = store.Peek();

                Assert.That(results, Is.EqualTo(events));
                queueStorage.Verify(qs => qs.Peek());
            }

            [Test, Category("Unit")]
            public void WhenClear_ThenEmptiesStore()
            {
                store.Clear();

                queueStorage.Verify(qs => qs.Empty());
            }
        }
    }
}