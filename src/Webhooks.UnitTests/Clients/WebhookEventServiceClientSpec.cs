using System;
using System.Net;
using Moq;
using NUnit.Framework;
using ServiceStack.Webhooks.Clients;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.UnitTests.Clients
{
    public class WebhookEventServiceClientSpec
    {
        [TestFixture]
        public class GivenAContext
        {
            private EventServiceClient client;
            private Mock<Webhooks.Clients.IServiceClient> serviceClient;
            private Mock<IEventServiceClientFactory> serviceClientFactory;

            [SetUp]
            public void Initialize()
            {
                serviceClientFactory = new Mock<IEventServiceClientFactory>();
                serviceClient = new Mock<Webhooks.Clients.IServiceClient>();
                serviceClientFactory.Setup(scf => scf.Create(It.IsAny<string>()))
                    .Returns(serviceClient.Object);
                client = new EventServiceClient
                {
                    ServiceClientFactory = serviceClientFactory.Object
                };
            }

            [Test, Category("Unit")]
            public void WhenPostAndNullSubscription_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                        client.Post(null, "aneventname", "adata"));
            }

            [Test, Category("Unit")]
            public void WhenPostAndNullEvent_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                        client.Post(new SubscriptionConfig(), null, "adata"));
            }

            [Test, Category("Unit")]
            public void WhenPost_ThenRequestIncludesStandardHeaders()
            {
                var request = new Mock<HttpWebRequest>();
                request.Setup(req => req.Headers)
                    .Returns(new WebHeaderCollection());
                serviceClient.SetupSet(sc => sc.RequestFilter = It.IsAny<Action<HttpWebRequest>>())
                    .Callback((Action<HttpWebRequest> action) => { action(request.Object); });

                client.Post(new SubscriptionConfig
                {
                    Url = "aurl"
                }, "aneventname", "adata");

                serviceClient.VerifySet(sc => sc.Timeout = client.Timeout);
                request.VerifySet(req => req.ContentType = MimeTypes.Json);
                Assert.That(request.Object.Headers[WebhookEventConstants.RequestIdHeaderName].IsGuid());
                Assert.That(request.Object.Headers[WebhookEventConstants.EventNameHeaderName], Is.EqualTo("aneventname"));
                Assert.That(request.Object.Headers[WebhookEventConstants.SecretSignatureHeaderName], Is.Null);
                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Once);
            }

            [Test, Category("Unit")]
            public void WhenPostAndSubscriptionHasSecret_ThenRequestIncludesSecretSignatureHeader()
            {
                var request = new Mock<HttpWebRequest>();
                request.Setup(req => req.Headers)
                    .Returns(new WebHeaderCollection());
                serviceClient.SetupSet(sc => sc.RequestFilter = It.IsAny<Action<HttpWebRequest>>())
                    .Callback((Action<HttpWebRequest> action) => { action(request.Object); });

                client.Post(new SubscriptionConfig
                {
                    Url = "aurl",
                    Secret = "asecret"
                }, "aneventname", "adata");

                serviceClient.VerifySet(sc => sc.Timeout = client.Timeout);
                Assert.That(request.Object.Headers[WebhookEventConstants.RequestIdHeaderName].IsGuid());
                Assert.That(request.Object.Headers[WebhookEventConstants.EventNameHeaderName], Is.EqualTo("aneventname"));
                Assert.That(request.Object.Headers[WebhookEventConstants.SecretSignatureHeaderName], Is.EqualTo(string.Empty));
                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Once);
            }

            [Test, Category("Unit")]
            public void WhenPostAndSubscriptionHasContentType_ThenSetsRequestContentType()
            {
                var request = new Mock<HttpWebRequest>();
                request.Setup(req => req.Headers)
                    .Returns(new WebHeaderCollection());
                serviceClient.SetupSet(sc => sc.RequestFilter = It.IsAny<Action<HttpWebRequest>>())
                    .Callback((Action<HttpWebRequest> action) => { action(request.Object); });

                client.Post(new SubscriptionConfig
                {
                    Url = "aurl",
                    Secret = "asecret",
                    ContentType = "acontenttype"
                }, "aneventname", "adata");

                serviceClient.VerifySet(sc => sc.Timeout = client.Timeout);
                request.VerifySet(req => req.ContentType = "acontenttype");
                Assert.That(request.Object.Headers[WebhookEventConstants.RequestIdHeaderName].IsGuid());
                Assert.That(request.Object.Headers[WebhookEventConstants.EventNameHeaderName], Is.EqualTo("aneventname"));
                Assert.That(request.Object.Headers[WebhookEventConstants.SecretSignatureHeaderName], Is.EqualTo(string.Empty));
                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Once);
            }

            [Test, Category("Unit")]
            public void WhenPostAndServiceClientFailsWith400_ThenDoesNotRetry()
            {
                serviceClient.Setup(sc => sc.Post(It.IsAny<string>(), It.IsAny<object>()))
                    .Throws(new WebServiceException
                    {
                        StatusCode = (int) HttpStatusCode.BadRequest
                    });

                client.Post(new SubscriptionConfig
                {
                    Url = "aurl"
                }, "aneventname", "adata");

                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Once);
            }

            [Test, Category("Unit")]
            public void WhenPostAndServiceClientFailsWith401_ThenDoesNotRetry()
            {
                serviceClient.Setup(sc => sc.Post(It.IsAny<string>(), It.IsAny<object>()))
                    .Throws(new WebServiceException
                    {
                        StatusCode = (int) HttpStatusCode.Unauthorized
                    });

                client.Post(new SubscriptionConfig
                {
                    Url = "aurl"
                }, "aneventname", "adata");

                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Once);
            }

            [Test, Category("Unit")]
            public void WhenPostAndServiceClientFailsWithAny500_ThenRetries()
            {
                serviceClient.Setup(sc => sc.Post(It.IsAny<string>(), It.IsAny<object>()))
                    .Throws(new WebServiceException
                    {
                        StatusCode = (int) HttpStatusCode.InternalServerError
                    });

                client.Post(new SubscriptionConfig
                {
                    Url = "aurl"
                }, "aneventname", "adata");

                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Exactly(3));
            }

            [Test, Category("Unit")]
            public void WhenPostAndServiceClientFailsWithTimeout_ThenRetries()
            {
                serviceClient.Setup(sc => sc.Post(It.IsAny<string>(), It.IsAny<object>()))
                    .Throws(new WebException());

                client.Post(new SubscriptionConfig
                {
                    Url = "aurl"
                }, "aneventname", "adata");

                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Exactly(3));
            }

            [Test, Category("Unit")]
            public void WhenPostAndServiceClientSucceedsFirstTime_ThenDoesNotRetry()
            {
                serviceClient.Setup(sc => sc.Post(It.IsAny<string>(), It.IsAny<object>()));

                client.Post(new SubscriptionConfig
                {
                    Url = "aurl"
                }, "aneventname", "adata");

                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Exactly(1));
            }

            [Test, Category("Unit")]
            public void WhenPostAndServiceClientFailsFirstTimeOnly_ThenRetriesOnceOnly()
            {
                var times = 1;
                serviceClient.Setup(sc => sc.Post(It.IsAny<string>(), It.IsAny<object>()))
                    .Callback(() =>
                    {
                        if (times == 1)
                        {
                            times++;
                            throw new WebServiceException();
                        }
                        times++;
                    });

                client.Post(new SubscriptionConfig
                {
                    Url = "aurl"
                }, "aneventname", "adata");

                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Exactly(2));
            }

            [Test, Category("Unit")]
            public void WhenPostAndRetriesIsZeroAndServiceClientFailsFirstTime_ThenDoesNotRetry()
            {
                client.Retries = 0;
                serviceClient.Setup(sc => sc.Post(It.IsAny<string>(), It.IsAny<object>()))
                    .Callback(() => { throw new Exception(); });

                client.Post(new SubscriptionConfig
                {
                    Url = "aurl"
                }, "aneventname", "adata");

                serviceClient.Verify(sc => sc.Post<object>("aurl", "adata"), Times.Exactly(1));
            }
        }
    }
}