using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ServiceStack.Data;
using ServiceStack.Web;
using ServiceStack.Webhooks.Properties;
using ServiceStack.Webhooks.ServiceInterface;
using ServiceStack.Webhooks.ServiceModel;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.UnitTests.ServiceInterface
{
    public class SubscriptionServiceSpec
    {
        [TestFixture]
        public class GivenAContext
        {
            private Mock<IRequest> _request;
            private SubscriptionService _service;
            private Mock<IWebhookSubscriptionStore> _store;

            [SetUp]
            public void Initialize()
            {
                _store = new Mock<IWebhookSubscriptionStore>();
                _store.Setup(s => s.Add(It.Is<WebhookSubscription>(sub => sub.Event == "anevent1")))
                    .Returns("asubscriptionid1");
                _store.Setup(s => s.Add(It.Is<WebhookSubscription>(sub => sub.Event == "anevent2")))
                    .Returns("asubscriptionid2");
                _request = new Mock<IRequest>();
                _request.Setup(req => req.TryResolve<ICurrentCaller>())
                    .Returns(Mock.Of<ICurrentCaller>(cc => cc.UserId == "auserid"));
                _service = new SubscriptionService
                {
                    Store = _store.Object,
                    Request = _request.Object
                };
            }

            [Test, Category("Unit")]
            public void WhenPost_ThenReturnsCreatedSubscription()
            {
                var results = _service.Post(new CreateSubscription
                {
                    Name = "aname",
                    Events = new List<string> {"anevent1", "anevent2"},
                    Config = new SubscriptionConfig
                    {
                        Url = "aurl"
                    }
                });

                Assert.That(2, Is.EqualTo(results.Subscriptions.Count));
                _store.Verify(s => s.Add(It.Is<WebhookSubscription>(whs =>
                        (whs.Id == "asubscriptionid1")
                        && (whs.Name == "aname")
                        && whs.IsActive
                        && (whs.Event == "anevent1")
                        && (whs.CreatedById == "auserid")
                        && whs.CreatedDateUtc.IsNear(DateTime.UtcNow)
                        && (whs.Config.Url == "aurl")
                        && whs.LastModifiedDateUtc.IsNear(DateTime.UtcNow)
                )));
                _store.Verify(s => s.Add(It.Is<WebhookSubscription>(whs =>
                        (whs.Id == "asubscriptionid2")
                        && (whs.Name == "aname")
                        && whs.IsActive
                        && (whs.Event == "anevent2")
                        && (whs.CreatedById == "auserid")
                        && whs.CreatedDateUtc.IsNear(DateTime.UtcNow)
                        && (whs.Config.Url == "aurl")
                        && whs.LastModifiedDateUtc.IsNear(DateTime.UtcNow)
                )));
            }

            [Test, Category("Unit")]
            public void WhenPostAndSameUserIdSameEventAndSameUrl_ThenThrowsConflict()
            {
                _store.Setup(s => s.Get("auserid", "anevent1"))
                    .Returns(new WebhookSubscription
                    {
                        CreatedById = "auserid",
                        Event = "anevent1"
                    });

                Assert.Throws<OptimisticConcurrencyException>(() =>
                {
                    _service.Post(new CreateSubscription
                    {
                        Name = "aname",
                        Events = new List<string> {"anevent1"},
                        Config = new SubscriptionConfig
                        {
                            Url = "aurl"
                        }
                    });
                }, Resources.SubscriptionService_DuplicateRegistration.Fmt("anevent1"));

                _store.Verify(s => s.Add(It.IsAny<WebhookSubscription>()), Times.Never);
            }
        }
    }
}