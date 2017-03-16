using System;
using Moq;
using NUnit.Framework;

namespace ServiceStack.Webhooks.UnitTests
{
    public class WebhooksClientSpec
    {
        [TestFixture]
        public class GivenAContext
        {
            private Mock<IEventSink> eventSink;
            private WebhooksClient webhooks;

            [SetUp]
            public void Initialize()
            {
                eventSink = new Mock<IEventSink>();
                webhooks = new WebhooksClient
                {
                    EventSink = eventSink.Object
                };
            }

            [Test, Category("Unit")]
            public void WhenPublishWithNullEventName_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() => webhooks.Publish(null, new TestPoco()));
            }

            [Test, Category("Unit")]
            public void WhenPublish_ThenSinksEvent()
            {
                var poco = new TestPoco();
                webhooks.Publish("aneventname", poco);

                eventSink.Verify(es => es.Write(It.Is<WebhookEvent>(whe =>
                    whe.EventName == "aneventname"
                    && whe.Data == poco)));
            }
        }
    }

    public class TestPoco
    {
    }
}