using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using Moq;
using NUnit.Framework;
using ServiceStack.Auth;
using ServiceStack.Testing;
using ServiceStack.Text;
using ServiceStack.Webhooks.Subscribers.Security;
using ServiceStack.Webhooks.UnitTesting;

namespace ServiceStack.Webhooks.Subscribers.UnitTests.Security
{
    public class HmacAuthProviderSpec
    {
        [TestFixture]
        public class GivenAContext
        {
            private static ServiceStackHost appHost;
            private HmacAuthProvider provider;

            [OneTimeSetUp]
            public void InitializeAllTests()
            {
                appHost = new BasicAppHost();
                appHost.Init();
            }

            [OneTimeTearDown]
            public void CleanupAllTests()
            {
                appHost.Dispose();
                appHost = null;
            }

            [SetUp]
            public void Initialize()
            {
                provider = new HmacAuthProvider();
            }

            [Test, Category("Unit")]
            public void When_ThenThrows()
            {
                Assert.Throws<NotImplementedException>(() =>
                    provider.Authenticate(null, null, null));
            }

            [Test, Category("Unit")]
            public void WhenIsAuthorizedAndNoSession_ThenReturnsFalse()
            {
                var results = provider.IsAuthorized(null, null, null);

                Assert.That(results, Is.False);
            }

            [Test, Category("Unit")]
            public void WhenIsAuthorizedAndSessionNotAuthenticated_ThenReturnsFalse()
            {
                var session = new Mock<IAuthSession>();
                session.Setup(sess => sess.IsAuthenticated)
                    .Returns(false);

                var results = provider.IsAuthorized(session.Object, null, null);

                Assert.That(results, Is.False);
            }

            [Test, Category("Unit")]
            public void WhenIsAuthorizedAndSessionNoUserAuthId_ThenReturnsFalse()
            {
                var session = new Mock<IAuthSession>();
                session.Setup(sess => sess.IsAuthenticated)
                    .Returns(false);
                session.Setup(sess => sess.UserAuthId)
                    .Returns((string) null);

                var results = provider.IsAuthorized(session.Object, null, null);

                Assert.That(results, Is.False);
            }

            [Test, Category("Unit")]
            public void WhenIsAuthorized_ThenReturnsTrue()
            {
                var session = new Mock<IAuthSession>();
                session.Setup(sess => sess.IsAuthenticated)
                    .Returns(true);
                session.Setup(sess => sess.UserAuthId)
                    .Returns("auserauthid");

                var results = provider.IsAuthorized(session.Object, null, null);

                Assert.That(results, Is.True);
            }

            [Test, Category("Unit")]
            public void WhenPreAuthenticateAndNoSignature_ThenCreatesNoSession()
            {
                var request = new MockHttpRequest();
                var response = new MockHttpResponse(request);

                provider.PreAuthenticate(request, response);

                var session = request.GetSession();
                Assert.That(session.Id, Is.Not.Null);
                Assert.That(session.IsAuthenticated, Is.False);
            }

            [Test, Category("Unit")]
            public void WhenPreAuthenticateAndRequestNotSecureConnection_ThenThrowsForbidden()
            {
                var body = Encoding.UTF8.GetBytes("abody");
                var signature = Webhooks.Security.HmacUtils.CreateHmacSignature(body, "asecret");
                using (var stream = MemoryStreamFactory.GetStream(body))
                {
                    var request = new MockHttpRequest
                    {
                        InputStream = stream,
                        Headers = new NameValueCollectionWrapper(new NameValueCollection
                        {
                            {WebhookEventConstants.SecretSignatureHeaderName, signature}
                        }),
                        IsSecureConnection = false
                    };

                    provider.Secret = null;

                    Assert.That(() => provider.PreAuthenticate(request, new MockHttpResponse(request)),
                        ThrowsHttpError.WithStatusCode(HttpStatusCode.Forbidden));
                }
            }

            [Test, Category("Unit")]
            public void WhenPreAuthenticateAndNoSecret_ThenThrowsUnauthorized()
            {
                var body = Encoding.UTF8.GetBytes("abody");
                var signature = Webhooks.Security.HmacUtils.CreateHmacSignature(body, "asecret");
                using (var stream = MemoryStreamFactory.GetStream(body))
                {
                    var request = new MockHttpRequest
                    {
                        InputStream = stream,
                        Headers = new NameValueCollectionWrapper(new NameValueCollection
                        {
                            {WebhookEventConstants.SecretSignatureHeaderName, signature}
                        }),
                        IsSecureConnection = true
                    };

                    provider.Secret = null;

                    Assert.That(() => provider.PreAuthenticate(request, new MockHttpResponse(request)),
                        ThrowsHttpError.WithStatusCode(HttpStatusCode.Unauthorized));
                }
            }

            [Test, Category("Unit")]
            public void WhenPreAuthenticateAndWrongSignature_ThenThrowsUnauthorized()
            {
                var body = Encoding.UTF8.GetBytes("abody");
                var signature = Webhooks.Security.HmacUtils.CreateHmacSignature(body, "awrongsecret");
                using (var stream = MemoryStreamFactory.GetStream(body))
                {
                    var request = new MockHttpRequest
                    {
                        InputStream = stream,
                        Headers = new NameValueCollectionWrapper(new NameValueCollection
                        {
                            {WebhookEventConstants.SecretSignatureHeaderName, signature}
                        }),
                        IsSecureConnection = true
                    };

                    provider.Secret = "asecret";

                    Assert.That(() => provider.PreAuthenticate(request, new MockHttpResponse(request)),
                        ThrowsHttpError.WithStatusCode(HttpStatusCode.Unauthorized));
                }
            }

            [Test, Category("Unit")]
            public void WhenPreAuthenticateAndRightSignature_ThenPopulatesSession()
            {
                var body = Encoding.UTF8.GetBytes("abody");
                var signature = Webhooks.Security.HmacUtils.CreateHmacSignature(body, "asecret");
                using (var stream = MemoryStreamFactory.GetStream(body))
                {
                    var request = new MockHttpRequest
                    {
                        InputStream = stream,
                        Headers = new NameValueCollectionWrapper(new NameValueCollection
                        {
                            {WebhookEventConstants.SecretSignatureHeaderName, signature},
                            {WebhookEventConstants.RequestIdHeaderName, "arequestid"}
                        }),
                        IsSecureConnection = true
                    };

                    provider.Secret = "asecret";

                    provider.PreAuthenticate(request, new MockHttpResponse(request));

                    var session = request.GetSession();
                    Assert.That(session.Id, Is.Not.Null);
                    Assert.That(session.IsAuthenticated, Is.True);
                    Assert.That(session.UserAuthId, Is.EqualTo("arequestid"));
                    Assert.That(session.UserAuthName, Is.EqualTo("localhost"));
                    Assert.That(session.UserName, Is.EqualTo("localhost"));
                }
            }
        }
    }
}