using System;
using System.Collections.Generic;
using System.Threading;
using Funq;
using NUnit.Framework;
using ServiceStack.Configuration;
using ServiceStack.Webhooks.Azure.IntTests.Services;
using ServiceStack.Webhooks.Azure.Worker;
using ServiceStack.Webhooks.ServiceModel;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.Azure.IntTests
{
    public class EventRelayWorkerSpec
    {
        [TestFixture]
        public class GivenAnEventQueue : AzureIntegrationTestBase
        {
            private static AppHostForAzureTesting appHost;
            private static JsonServiceClient client;
            private const string BaseUrl = "http://localhost:8080/";
            private CancellationToken cancellation;
            private CancellationTokenSource cancellationToken;
            private AzureQueueWebhookEventSink sink;
            private EventRelayWorker worker;

            [OneTimeSetUp]
            public void InitializeContext()
            {
                appHost = new AppHostForAzureTesting();
                appHost.Init();
                appHost.Start(BaseUrl);

                client = new JsonServiceClient(BaseUrl);

                cancellationToken = new CancellationTokenSource();
                cancellation = cancellationToken.Token;

                var container = new Container();
                container.Register<IAppSettings>(new AppSettings());

                worker = new EventRelayWorker(container);
                sink = new AzureQueueWebhookEventSink();
            }

            [OneTimeTearDown]
            public void CleanupContext()
            {
                appHost.Dispose();
            }

            [SetUp]
            public void Initialize()
            {
                client.Put(new ResetConsumedEvents());
            }

            [TearDown]
            public void Cleanup()
            {
                cancellationToken.Cancel();
            }

            [Test, Category("Integration")]
            public void WhenNoEventOnQueue_ThenNoSubscribersNotified()
            {
                RunWorker(10);

                var consumed = client.Get(new GetConsumedEvents()).Events;

                Assert.That(consumed.Count, Is.EqualTo(0));
            }

            [Test, Category("Integration")]
            public void WhenEventQueued_ThenSubscriberNotified()
            {
                SetupSubscription("aneventname");
                SetupEvent("aneventname");

                RunWorker(10);

                var consumed = client.Get(new GetConsumedEvents()).Events;

                Assert.That(consumed.Count, Is.EqualTo(1));
                AssertSunkEvent(consumed[0]);
            }

            private static void SetupSubscription(string eventName)
            {
                client.Post(new CreateSubscription
                {
                    Name = "aname",
                    Events = new List<string> {eventName},
                    Config = new SubscriptionConfig
                    {
                        Url = BaseUrl.AppendPath(new ConsumeEvent().ToPostUrl())
                    }
                });
            }

            private void RunWorker(int seconds)
            {
                cancellationToken.CancelAfter(TimeSpan.FromSeconds(seconds));
                try
                {
                    worker.Run(cancellation);
                }
                catch (OperationCanceledException)
                {
                }
            }

            private void SetupEvent(string eventName)
            {
                sink.Write(eventName, new TestEvent
                {
                    A = 1,
                    B = 2,
                    C = 3
                });
            }

            private static void AssertSunkEvent(ConsumedEvent consumedEvent)
            {
                Assert.That(consumedEvent.EventName, Is.EqualTo("aneventname"));
                Assert.That(consumedEvent.Data.A, Is.EqualTo("1"));
                Assert.That(consumedEvent.Data.B, Is.EqualTo("2"));
                Assert.That(consumedEvent.Data.C, Is.EqualTo("3"));
            }
        }
    }
}