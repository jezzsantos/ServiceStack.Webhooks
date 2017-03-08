using System;
using System.Collections.Generic;
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
                Assert.Throws<ArgumentNullException>(() => webhooks.Publish(null, new TestClass()));
            }

            [Test, Category("Unit")]
            public void WhenPublish_ThenStoresEvent()
            {
                webhooks.Publish("aneventname", new Dictionary<string, string> {{"akey", "avalue"}});

                eventSink.Verify(es => es.Write("aneventname", It.Is<Dictionary<string, string>>(dic => dic["akey"] == "avalue")));
            }
        }
    }

    public class TestClass
    {
    }
}