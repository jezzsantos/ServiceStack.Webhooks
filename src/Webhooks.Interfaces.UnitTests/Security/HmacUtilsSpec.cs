using System;
using System.Text;
using Moq;
using NUnit.Framework;
using ServiceStack.Web;
using ServiceStack.Webhooks.Security;

namespace ServiceStack.Webhooks.Interfaces.UnitTests.Security
{
    public class HmacUtilsSpec
    {
        [TestFixture]
        public class GivenAContext
        {
            private string body;
            private Mock<IRequest> request;

            [SetUp]
            public void Initialize()
            {
                body = "abody";
                request = new Mock<IRequest>();
                request.Setup(req => req.GetRawBody())
                    .Returns(body);
            }

            [Test, Category("Unit")]
            public void WhenCreateHmacSignatureWithNullRequestBytes_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    ((byte[]) null).CreateHmacSignature("asecret"));
            }

            [Test, Category("Unit")]
            public void WhenCreateHmacSignatureWithNullSecret_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    new byte[] {}.CreateHmacSignature(null));
            }

            [Test, Category("Unit")]
            public void WhenCreateHmacSignatureWithEmptyBytes_ThenReturnsSignature()
            {
                var result = new byte[] {}.CreateHmacSignature("asecret");

                Assert.That(result, Does.Match("[sha1\\=]".Fmt(DataFormats.Base64Text().Expression)));
            }

            [Test, Category("Unit")]
            public void WhenCreateHmacSignatureWithAnyBytes_ThenReturnsSignature()
            {
                var result = Encoding.UTF8.GetBytes("abody").CreateHmacSignature("asecret");

                Assert.That(result, Does.Match("[sha1\\=]".Fmt(DataFormats.Base64Text().Expression)));
            }

            [Test, Category("Unit")]
            public void WhenVerifySignatureWithNullRequest_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    ((IRequest) null).VerifySignature("asignature", "asecret"));
            }

            [Test, Category("Unit")]
            public void WhenVerifySignatureWithNullSignature_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    request.Object.VerifySignature(null, "asecret"));
            }

            [Test, Category("Unit")]
            public void WhenVerifySignatureWithNullSecret_ThenThrows()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    request.Object.VerifySignature("asignature", null));
            }

            [Test, Category("Unit")]
            public void WhenVerifySignatureWithEmptySignature_ThenReturnsFalse()
            {
                var result = request.Object.VerifySignature(string.Empty, "asecret");

                Assert.That(result, Is.False);
            }

            [Test, Category("Unit")]
            public void WhenVerifySignatureWithWrongSignature_ThenReturnsFalse()
            {
                var result = request.Object.VerifySignature("awrongsignature", "asecret");

                Assert.That(result, Is.False);
            }

            [Test, Category("Unit")]
            public void WhenVerifySignatureWithWrongSecret_ThenReturnsFalse()
            {
                var signature = Encoding.UTF8.GetBytes(body).CreateHmacSignature("asecret");

                var result = request.Object.VerifySignature(signature, "awrongsecret");

                Assert.That(result, Is.False);
            }

            [Test, Category("Unit")]
            public void WhenVerifySignature_ThenReturnsTrue()
            {
                var signature = Encoding.UTF8.GetBytes(body).CreateHmacSignature("asecret");

                var result = request.Object.VerifySignature(signature, "asecret");

                Assert.That(result, Is.True);
            }
        }
    }
}