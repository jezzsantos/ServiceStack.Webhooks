using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace ServiceStack.Webhooks.Azure.IntTests
{
    public class AzureQueueWebhookEventSinkSpec
    {
        [TestFixture]
        public class GivenAQueue : AzureIntegrationTestBase
        {
            private AzureQueueWebhookEventSink sink;

            [SetUp]
            public void Initialize()
            {
                sink = new AzureQueueWebhookEventSink();
                sink.Clear();
            }

            [Test, Category("Integration")]
            public void WhenCreate_ThenQueuesEvent()
            {
                sink.Write("aneventname", new Dictionary<string, string> {{"akey", "avalue"}});

                var result = sink.Peek();

                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result[0].CreatedDateUtc, Is.EqualTo(DateTime.UtcNow).Within(1).Seconds);
                Assert.That(result[0].EventName, Is.EqualTo("aneventname"));
                Assert.That(result[0].Data, Is.EqualTo(new Dictionary<string, string> {{"akey", "avalue"}}));
            }
        }
    }
}