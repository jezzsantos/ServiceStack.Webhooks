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

                eventSink.Verify(es => es.Write(It.Is<WebhookEvent>(we =>
                    we.EventName == "aneventname"
                    && we.Data == poco)));
            }

            [Test, Category("Unit")]
            public void WhenPublishAndPublishEventFilter_ThenAugmentsPublishedEvent()
            {
                var poco = new TestPoco();
                var poco2 = new TestPoco();
                webhooks.PublishFilter = @event =>
                {
                    @event.Id = "anewid";
                    @event.Data = poco2;
                };
                webhooks.Publish("aneventname", poco);

                eventSink.Verify(es => es.Write(It.Is<WebhookEvent>(we =>
                    we.Id == "anewid"
                    && we.EventName == "aneventname"
                    && we.Data == poco2)));
            }
        }
    }

    public class TestPoco
    {
    }
}