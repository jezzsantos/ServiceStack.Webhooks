using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ServiceStack.Web;

namespace ServiceStack.Webhooks.Security
{
    public static class HmacUtils
    {
        internal const string SignatureFormat = @"sha1={0}";

        /// <summary>
        ///     Returns the computed HMAC hex digest of the body (RFC3174), using the secret as the key.
        ///     See https://developer.github.com/v3/repos/hooks/#example, and
        ///     https://pubsubhubbub.github.io/PubSubHubbub/pubsubhubbub-core-0.4.html#authednotify
        /// </summary>
        public static string CreateHmacSignature(this byte[] requestBytes, string secret)
        {
            Guard.AgainstNull(() => requestBytes, requestBytes);
            Guard.AgainstNullOrEmpty(() => secret, secret);

            var encoding = Encoding.UTF8;
            var key = encoding.GetBytes(secret);
            var signature = SignatureFormat.Fmt(SignBody(requestBytes, key));

            return signature;
        }

        public static bool VerifySignature(this IRequest request, string signature, string secret)
        {
            Guard.AgainstNull(() => request, request);
            Guard.AgainstNull(() => signature, signature);
            Guard.AgainstNullOrEmpty(() => secret, secret);

            var expectedSignature = CreateHmacSignature(request, secret);

            return expectedSignature.EqualsOrdinal(signature);
        }

        private static string CreateHmacSignature(IRequest request, string secret)
        {
            var encoding = Encoding.UTF8;
            var body = encoding.GetBytes(request.GetRawBody());

            var key = encoding.GetBytes(secret);

            var signature = SignatureFormat.Fmt(SignBody(body, key));

            return signature;
        }

        private static string SignBody(byte[] body, byte[] key)
        {
            var signature = new HMACSHA256(key)
                .ComputeHash(body);

            return ToHex(signature);
        }

        private static string ToHex(byte[] bytes)
        {
            var builder = new StringBuilder();
            bytes
                .ToList()
                .ForEach(b => { builder.Append(b.ToString("x2")); });

            return builder.ToString();
        }
    }
}