using System;
using System.Collections.Generic;
using System.Net;
using Moq;
using NUnit.Framework;
using ServiceStack.Text;
using ServiceStack.Web;
using ServiceStack.Webhooks.ServiceInterface;
using ServiceStack.Webhooks.ServiceModel;
using ServiceStack.Webhooks.ServiceModel.Types;
using ServiceStack.Webhooks.UnitTesting;

namespace ServiceStack.Webhooks.UnitTests.ServiceInterface
{
    public class SubscriptionServiceSpec
    {
        [TestFixture]
        public class GivenAContext
        {
            private Mock<IRequest> request;
            private SubscriptionService service;
            private Mock<ISubscriptionStore> store;

            [SetUp]
            public void Initialize()
            {
                store = new Mock<ISubscriptionStore>();
                store.Setup(s => s.Add(It.Is<WebhookSubscription>(sub => sub.Event == "anevent1")))
                    .Returns("asubscriptionid1");
                store.Setup(s => s.Add(It.Is<WebhookSubscription>(sub => sub.Event == "anevent2")))
                    .Returns("asubscriptionid2");
                request = new Mock<IRequest>();
                request.Setup(req => req.TryResolve<ICurrentCaller>())
                    .Returns(Mock.Of<ICurrentCaller>(cc => cc.UserId == "auserid"));
                service = new SubscriptionService
                {
                    Store = store.Object,
                    Request = request.Object
                };
            }

            [Test, Category("Unit")]
            public void WhenPost_ThenReturnsCreatedSubscription()
            {
                var results = service.Post(new CreateSubscription
                {
                    Name = "aname",
                    Events = new List<string> {"anevent1", "anevent2"},
                    Config = new SubscriptionConfig
                    {
                        Url = "aurl"
                    }
                });

                Assert.That(2, Is.EqualTo(results.Subscriptions.Count));
                store.Verify(s => s.Add(It.Is<WebhookSubscription>(whs =>
                        (whs.Id == "asubscriptionid1")
                        && (whs.Name == "aname")
                        && whs.IsActive
                        && (whs.Event == "anevent1")
                        && (whs.CreatedById == "auserid")
                        && whs.CreatedDateUtc.IsNear(DateTime.UtcNow)
                        && (whs.Config.Url == "aurl")
                        && whs.LastModifiedDateUtc.IsNear(DateTime.UtcNow)
                )));
                store.Verify(s => s.Add(It.Is<WebhookSubscription>(whs =>
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
                store.Setup(s => s.Get("auserid", "anevent1"))
                    .Returns(new WebhookSubscription
                    {
                        CreatedById = "auserid",
                        Event = "anevent1"
                    });

                Assert.That(() => service.Post(new CreateSubscription
                {
                    Name = "aname",
                    Events = new List<string> {"anevent1"},
                    Config = new SubscriptionConfig
                    {
                        Url = "aurl"
                    }
                }), ThrowsHttpError.WithStatusCode(HttpStatusCode.Conflict));

                store.Verify(s => s.Add(It.IsAny<WebhookSubscription>()), Times.Never);
            }

            [Test, Category("Unit")]
            public void WhenGetWithUnknownId_ThenThrowsNotFound()
            {
                store.Setup(s => s.Find("auserid"))
                    .Returns(new List<WebhookSubscription>());

                Assert.That(() => service.Get(new GetSubscription
                {
                    Id = "anunknownid"
                }), ThrowsHttpError.WithStatusCode(HttpStatusCode.NotFound));
            }

            [Test, Category("Unit")]
            public void WhenGet_ThenReturnsSubscription()
            {
                var subscription = new WebhookSubscription
                {
                    Id = "asubscriptionid",
                    CreatedById = "auserid",
                    Event = "anevent1"
                };
                store.Setup(s => s.Get("asubscriptionid"))
                    .Returns(subscription);
                var datum1 = SystemTime.UtcNow.ToNearestSecond();
                var datum2 = datum1.AddDays(1);
                store.Setup(s => s.Search("asubscriptionid", It.IsAny<int>()))
                    .Returns(new List<SubscriptionDeliveryResult>
                    {
                        new SubscriptionDeliveryResult
                        {
                            Id = "aresultid1",
                            AttemptedDateUtc = datum1
                        },
                        new SubscriptionDeliveryResult
                        {
                            Id = "aresultid2",
                            AttemptedDateUtc = datum2
                        }
                    });

                var result = service.Get(new GetSubscription
                {
                    Id = "asubscriptionid"
                });

                Assert.That(result.Subscription, Is.EqualTo(subscription));
                Assert.That(result.History[0].Id, Is.EqualTo("aresultid2"));
                Assert.That(result.History[1].Id, Is.EqualTo("aresultid1"));
                store.Verify(s => s.Get("asubscriptionid"));
                store.Verify(s => s.Search("asubscriptionid", 100));
            }

            [Test, Category("Unit")]
            public void WhenList_ThenReturnsSubscriptions()
            {
                var subscription = new WebhookSubscription
                {
                    Id = "asubscriptionid",
                    CreatedById = "auserid",
                    Event = "anevent1"
                };
                store.Setup(s => s.Find("auserid"))
                    .Returns(new List<WebhookSubscription>
                    {
                        subscription
                    });

                var result = service.Get(new ListSubscriptions());

                Assert.That(result.Subscriptions.Count, Is.EqualTo(1));
                Assert.That(result.Subscriptions[0], Is.EqualTo(subscription));
                store.Verify(s => s.Find("auserid"));
            }

            [Test, Category("Unit")]
            public void WhenUpdateWithUnknownId_ThenThrowsNotFound()
            {
                store.Setup(s => s.Get(It.IsAny<string>()))
                    .Returns((WebhookSubscription) null);

                Assert.That(() => service.Put(new UpdateSubscription
                {
                    Id = "anunknownid"
                }), ThrowsHttpError.WithStatusCode(HttpStatusCode.NotFound));
            }

            [Test, Category("Unit")]
            public void WhenUpdate_ThenReturnsSubscription()
            {
                var subscription = new WebhookSubscription
                {
                    Id = "asubscriptionid",
                    CreatedById = "auserid",
                    Event = "anevent1",
                    Config = new SubscriptionConfig
                    {
                        Url = "aurl",
                        Secret = "asecret",
                        ContentType = "acontenttype"
                    },
                    IsActive = false
                };
                store.Setup(s => s.Get("asubscriptionid"))
                    .Returns(subscription);

                var result = service.Put(new UpdateSubscription
                {
                    Id = "asubscriptionid",
                    Url = "anewurl",
                    Secret = "anewsecret",
                    ContentType = "anewcontenttype",
                    IsActive = true
                });

                Assert.That(result.Subscription, Is.EqualTo(subscription));
                Assert.That(result.Subscription.Config.Url, Is.EqualTo("anewurl"));
                store.Verify(s => s.Get("asubscriptionid"));
                store.Verify(s => s.Update("asubscriptionid", It.Is<WebhookSubscription>(whs =>
                    (whs.Config.Url == "anewurl")
                    && (whs.Config.Secret == "anewsecret")
                    && (whs.Config.ContentType == "anewcontenttype")
                    && whs.IsActive
                    && whs.LastModifiedDateUtc.IsNear(DateTime.UtcNow))));
            }

            [Test, Category("Unit")]
            public void WhenDeleteWithUnknownId_ThenThrowsNotFound()
            {
                store.Setup(s => s.Find("auserid"))
                    .Returns(new List<WebhookSubscription>());

                Assert.That(() => service.Delete(new DeleteSubscription
                {
                    Id = "anunknownid"
                }), ThrowsHttpError.WithStatusCode(HttpStatusCode.NotFound));
            }

            [Test, Category("Unit")]
            public void WhenDelete_ThenDeletesSubscription()
            {
                var subscription = new WebhookSubscription
                {
                    Id = "asubscriptionid",
                    CreatedById = "auserid",
                    Event = "anevent1"
                };
                store.Setup(s => s.Get("asubscriptionid"))
                    .Returns(subscription);

                service.Delete(new DeleteSubscription
                {
                    Id = "asubscriptionid"
                });

                store.Verify(s => s.Get("asubscriptionid"));
                store.Verify(s => s.Delete("asubscriptionid"));
            }

            [Test, Category("Unit")]
            public void WhenSearch_ThenReturnsActiveSubscriptions()
            {
                var config = new SubscriptionRelayConfig
                {
                    Config = new SubscriptionConfig
                    {
                        Url = "aurl"
                    }
                };
                store.Setup(s => s.Search("aneventname", true))
                    .Returns(new List<SubscriptionRelayConfig>
                    {
                        config
                    });

                var result = service.Get(new SearchSubscriptions
                {
                    EventName = "aneventname"
                });

                Assert.That(result.Subscribers.Count, Is.EqualTo(1));
                Assert.That(result.Subscribers[0], Is.EqualTo(config));
                store.Verify(s => s.Search("aneventname", true));
            }

            [Test, Category("Unit")]
            public void WhenUpdateHistoryAndExistingHistory_ThenNotAddHistory()
            {
                var history = new List<SubscriptionDeliveryResult>
                {
                    new SubscriptionDeliveryResult
                    {
                        Id = "aresultid",
                        SubscriptionId = "asubscriptionid"
                    }
                };
                store.Setup(s => s.Search("asubscriptionid", It.IsAny<int>()))
                    .Returns(history);

                service.Put(new UpdateSubscriptionHistory
                {
                    Results = history
                });

                store.Verify(s => s.Search("asubscriptionid", 1));
                store.Verify(s => s.Add(It.IsAny<string>(), It.IsAny<SubscriptionDeliveryResult>()), Times.Never);
                store.Verify(s => s.Get(It.IsAny<string>()), Times.Never);
                store.Verify(s => s.Update(It.IsAny<string>(), It.IsAny<WebhookSubscription>()), Times.Never);
            }

            [Test, Category("Unit")]
            public void WhenUpdateHistory_ThenAddHistory()
            {
                var history = new List<SubscriptionDeliveryResult>
                {
                    new SubscriptionDeliveryResult
                    {
                        Id = "aresultid1",
                        SubscriptionId = "asubscriptionid"
                    },
                    new SubscriptionDeliveryResult
                    {
                        Id = "aresultid2",
                        SubscriptionId = "asubscriptionid"
                    }
                };
                store.Setup(s => s.Search("asubscriptionid", It.IsAny<int>()))
                    .Returns(new List<SubscriptionDeliveryResult>
                    {
                        new SubscriptionDeliveryResult
                        {
                            Id = "aresultid1",
                            SubscriptionId = "asubscriptionid"
                        }
                    });
                service.Put(new UpdateSubscriptionHistory
                {
                    Results = history
                });

                store.Verify(s => s.Search("asubscriptionid", 2));
                store.Verify(s => s.Add("asubscriptionid", It.Is<SubscriptionDeliveryResult>(sdr =>
                        sdr.Id == "aresultid2")));
                store.Verify(s => s.Get(It.IsAny<string>()), Times.Never);
                store.Verify(s => s.Update(It.IsAny<string>(), It.IsAny<WebhookSubscription>()), Times.Never);
            }

            [Test, Category("Unit")]
            public void WhenUpdateHistoryAndResultIncludes2XX_ThenDoesNotDeactiveSubscription()
            {
                var history = new List<SubscriptionDeliveryResult>
                {
                    new SubscriptionDeliveryResult
                    {
                        Id = "aresultid",
                        SubscriptionId = "asubscriptionid",
                        StatusCode = HttpStatusCode.OK
                    }
                };
                store.Setup(s => s.Search("asubscriptionid", It.IsAny<int>()))
                    .Returns(new List<SubscriptionDeliveryResult>());
                store.Setup(s => s.Get("asubscriptionid"))
                    .Returns(new WebhookSubscription
                    {
                        Id = "asubscriptionid"
                    });

                service.Put(new UpdateSubscriptionHistory
                {
                    Results = history
                });

                store.Verify(s => s.Search("asubscriptionid", 1));
                store.Verify(s => s.Add("asubscriptionid", It.Is<SubscriptionDeliveryResult>(sdr =>
                        sdr.Id == "aresultid")));
                store.Verify(s => s.Get(It.IsAny<string>()), Times.Never);
                store.Verify(s => s.Update("asubscriptionid", It.IsAny<WebhookSubscription>()), Times.Never);
            }

            [Test, Category("Unit")]
            public void WhenUpdateHistoryAndResultIncludes4XX_ThenDeactivatesSubscription()
            {
                var history = new List<SubscriptionDeliveryResult>
                {
                    new SubscriptionDeliveryResult
                    {
                        Id = "aresultid",
                        SubscriptionId = "asubscriptionid",
                        StatusCode = HttpStatusCode.BadRequest
                    }
                };
                store.Setup(s => s.Search("asubscriptionid", It.IsAny<int>()))
                    .Returns(new List<SubscriptionDeliveryResult>());
                store.Setup(s => s.Get("asubscriptionid"))
                    .Returns(new WebhookSubscription
                    {
                        Id = "asubscriptionid",
                        IsActive = true
                    });

                service.Put(new UpdateSubscriptionHistory
                {
                    Results = history
                });

                store.Verify(s => s.Search("asubscriptionid", 1));
                store.Verify(s => s.Add("asubscriptionid", It.Is<SubscriptionDeliveryResult>(sdr =>
                        sdr.Id == "aresultid")));
                store.Verify(s => s.Get("asubscriptionid"));
                store.Verify(s => s.Update("asubscriptionid", It.Is<WebhookSubscription>(sub =>
                    (sub.Id == "asubscriptionid")
                    && (sub.IsActive == false))));
            }
        }
    }
}