using System;
using System.Net;
using Moq;
using NUnit.Framework;
using ServiceStack.Webhooks.Relays.Clients;
using ServiceStack.Webhooks.Relays.Properties;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.Relays.UnitTests.Clients
{
    public class WebhookEventServiceClientSpec
    {
        [TestFixture]
        public class GivenAContext
        {
            private EventServiceClient client;
            private Mock<Relays.Clients.IServiceClient> serviceClient;
            private Mock<IEventServiceClientFactory> serviceClientFactory;

            [SetUp]
            public void Initialize()
            {
                serviceClientFactory = new Mock<IEventServiceClientFactory>();
                serviceClient = new Mock<Relays.Clients.IServiceClient>();
                var response = new Mock<HttpWebResponse>();
                response.Setup(res => res.StatusCode).Returns(HttpStatusCode.OK);
                response.Setup(res => res.StatusDescription).Returns("astatusdescription");
                serviceClient.Setup(sc => sc.Post(It.IsAny<string>(), It.IsAny<object>()))
                    .Returns(response.Object);
                serviceClientFactory.Setup(scf => scf.Create(It.IsAny<string>()))
                    .Returns(serviceClient.Object);
                client = new EventServiceClient
                {
                    ServiceClientFactory = serviceClientFactory.Object
                };
            }

            [Test, Category("Unit")]
            public void WhenRelayAndNullSubscription_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                        client.Relay(null, "aneventname", "adata"));
            }

            [Test, Category("Unit")]
            public void WhenRelayAndNullEvent_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                        client.Relay(new SubscriptionRelayConfig(), null, "adata"));
            }

            [Test, Category("Unit")]
            public void WhenRelay_ThenRequestIncludesStandardHeaders()
            {
                var request = new Mock<HttpWebRequest>();
                request.Setup(req => req.Headers)
                    .Returns(new WebHeaderCollection());
                serviceClient.SetupSet(sc => sc.RequestFilter = It.IsAny<Action<HttpWebRequest>>())
                    .Callback((Action<HttpWebRequest> action) => { action(request.Object); });

                client.Relay(new SubscriptionRelayConfig
                {
                    Config = new SubscriptionConfig
                    {
                        Url = "aurl"
                    }
                }, "aneventname", "adata");

                serviceClient.VerifySet(sc => sc.Timeout = client.Timeout);
                request.VerifySet(req => req.ContentType = MimeTypes.Json);
                Assert.That(request.Object.Headers[WebhookEventConstants.RequestIdHeaderName].IsGuid());
                Assert.That(request.Object.Headers[WebhookEventConstants.EventNameHeaderName], Is.EqualTo("aneventname"));
                Assert.That(request.Object.Headers[WebhookEventConstants.SecretSignatureHeaderName], Is.Null);
                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Once);
            }

            [Test, Category("Unit")]
            public void WhenRelayAndSubscriptionHasSecret_ThenRequestIncludesSecretSignatureHeader()
            {
                var request = new Mock<HttpWebRequest>();
                request.Setup(req => req.Headers)
                    .Returns(new WebHeaderCollection());
                serviceClient.SetupSet(sc => sc.RequestFilter = It.IsAny<Action<HttpWebRequest>>())
                    .Callback((Action<HttpWebRequest> action) => { action(request.Object); });

                client.Relay(new SubscriptionRelayConfig
                {
                    Config = new SubscriptionConfig
                    {
                        Url = "aurl",
                        Secret = "asecret"
                    }
                }, "aneventname", "adata");

                serviceClient.VerifySet(sc => sc.Timeout = client.Timeout);
                Assert.That(request.Object.Headers[WebhookEventConstants.RequestIdHeaderName].IsGuid());
                Assert.That(request.Object.Headers[WebhookEventConstants.EventNameHeaderName], Is.EqualTo("aneventname"));
                Assert.That(request.Object.Headers[WebhookEventConstants.SecretSignatureHeaderName], Is.EqualTo(string.Empty));
                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Once);
            }

            [Test, Category("Unit")]
            public void WhenRelayAndSubscriptionHasContentType_ThenSetsRequestContentType()
            {
                var request = new Mock<HttpWebRequest>();
                request.Setup(req => req.Headers)
                    .Returns(new WebHeaderCollection());
                serviceClient.SetupSet(sc => sc.RequestFilter = It.IsAny<Action<HttpWebRequest>>())
                    .Callback((Action<HttpWebRequest> action) => { action(request.Object); });

                client.Relay(new SubscriptionRelayConfig
                {
                    Config = new SubscriptionConfig
                    {
                        Url = "aurl",
                        Secret = "asecret",
                        ContentType = "acontenttype"
                    }
                }, "aneventname", "adata");

                serviceClient.VerifySet(sc => sc.Timeout = client.Timeout);
                request.VerifySet(req => req.ContentType = "acontenttype");
                Assert.That(request.Object.Headers[WebhookEventConstants.RequestIdHeaderName].IsGuid());
                Assert.That(request.Object.Headers[WebhookEventConstants.EventNameHeaderName], Is.EqualTo("aneventname"));
                Assert.That(request.Object.Headers[WebhookEventConstants.SecretSignatureHeaderName], Is.EqualTo(string.Empty));
                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Once);
            }

            [Test, Category("Unit")]
            public void WhenRelayAndServiceClientFailsWith400_ThenDoesNotRetry()
            {
                serviceClient.Setup(sc => sc.Post(It.IsAny<string>(), It.IsAny<object>()))
                    .Throws(new WebServiceException
                    {
                        StatusCode = (int) HttpStatusCode.BadRequest,
                        StatusDescription = "astatusdescription"
                    });

                var result = client.Relay(new SubscriptionRelayConfig
                {
                    SubscriptionId = "asubscriptionid",
                    Config = new SubscriptionConfig
                    {
                        Url = "aurl"
                    }
                }, "aneventname", "adata");

                AssertDeliveryResult(result, HttpStatusCode.BadRequest);
                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Once);
            }

            [Test, Category("Unit")]
            public void WhenRelayAndServiceClientFailsWith401_ThenDoesNotRetry()
            {
                serviceClient.Setup(sc => sc.Post(It.IsAny<string>(), It.IsAny<object>()))
                    .Throws(new WebServiceException
                    {
                        StatusCode = (int) HttpStatusCode.Unauthorized,
                        StatusDescription = "astatusdescription"
                    });

                var result = client.Relay(new SubscriptionRelayConfig
                {
                    SubscriptionId = "asubscriptionid",
                    Config = new SubscriptionConfig
                    {
                        Url = "aurl"
                    }
                }, "aneventname", "adata");

                AssertDeliveryResult(result, HttpStatusCode.Unauthorized);
                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Once);
            }

            [Test, Category("Unit")]
            public void WhenRelayAndServiceClientFailsWithAny500_ThenRetries()
            {
                serviceClient.Setup(sc => sc.Post(It.IsAny<string>(), It.IsAny<object>()))
                    .Throws(new WebServiceException
                    {
                        StatusCode = (int) HttpStatusCode.InternalServerError,
                        StatusDescription = "astatusdescription"
                    });

                var result = client.Relay(new SubscriptionRelayConfig
                {
                    SubscriptionId = "asubscriptionid",
                    Config = new SubscriptionConfig
                    {
                        Url = "aurl"
                    }
                }, "aneventname", "adata");

                AssertDeliveryResult(result, HttpStatusCode.InternalServerError);
                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Exactly(3));
            }

            [Test, Category("Unit")]
            public void WhenRelayAndServiceClientFailsWithTimeout_ThenRetries()
            {
                serviceClient.Setup(sc => sc.Post(It.IsAny<string>(), It.IsAny<object>()))
                    .Throws(new WebException());

                var result = client.Relay(new SubscriptionRelayConfig
                {
                    SubscriptionId = "asubscriptionid",
                    Config = new SubscriptionConfig
                    {
                        Url = "aurl"
                    }
                }, "aneventname", "adata");

                AssertDeliveryResult(result, HttpStatusCode.ServiceUnavailable, Resources.EventServiceClient_FailedDelivery.Fmt("aurl", 3));
                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Exactly(3));
            }

            [Test, Category("Unit")]
            public void WhenRelayAndServiceClientSucceedsFirstTime_ThenDoesNotRetry()
            {
                var result = client.Relay(new SubscriptionRelayConfig
                {
                    SubscriptionId = "asubscriptionid",
                    Config = new SubscriptionConfig
                    {
                        Url = "aurl"
                    }
                }, "aneventname", "adata");

                AssertDeliveryResult(result, HttpStatusCode.OK);
                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Exactly(1));
            }

            [Test, Category("Unit")]
            public void WhenRelayAndServiceClientFailsFirstTimeOnly_ThenRetriesOnceOnly()
            {
                var times = 1;
                var response = new Mock<HttpWebResponse>();
                response.Setup(res => res.StatusCode).Returns(HttpStatusCode.OK);
                response.Setup(res => res.StatusDescription).Returns("astatusdescription");
                serviceClient.Setup(sc => sc.Post(It.IsAny<string>(), It.IsAny<object>()))
                    .Callback(() =>
                    {
                        if (times == 1)
                        {
                            times++;
                            throw new WebServiceException();
                        }
                        times++;
                    }).Returns(response.Object);

                client.Relay(new SubscriptionRelayConfig
                {
                    SubscriptionId = "asubscriptionid",
                    Config = new SubscriptionConfig
                    {
                        Url = "aurl"
                    }
                }, "aneventname", "adata");

                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Exactly(2));
            }

            [Test, Category("Unit")]
            public void WhenRelayAndRetriesIsZeroAndServiceClientFailsFirstTime_ThenDoesNotRetry()
            {
                client.Retries = 0;
                serviceClient.Setup(sc => sc.Post(It.IsAny<string>(), It.IsAny<object>()))
                    .Callback(() => { throw new Exception(); });

                client.Relay(new SubscriptionRelayConfig
                {
                    SubscriptionId = "asubscriptionid",
                    Config = new SubscriptionConfig
                    {
                        Url = "aurl"
                    }
                }, "aneventname", "adata");

                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Exactly(1));
            }

            private static void AssertDeliveryResult(SubscriptionDeliveryResult result, HttpStatusCode statusCode, string statusDescription = null)
            {
                Assert.That(result.Id.IsEntityId);
                Assert.That(result.SubscriptionId, Is.EqualTo("asubscriptionid"));
                Assert.That(result.AttemptedDateUtc, Is.EqualTo(DateTime.UtcNow).Within(1).Seconds);
                Assert.That(result.StatusCode, Is.EqualTo(statusCode));
                Assert.That(result.StatusDescription, Is.EqualTo(statusDescription.HasValue() ? statusDescription : "astatusdescription"));
            }
        }
    }
}