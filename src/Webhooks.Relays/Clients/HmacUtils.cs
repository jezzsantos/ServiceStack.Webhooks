using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ServiceStack.Webhooks.Relays.Clients
{
    public static class HmacUtils
    {
        private const int DefaultBufferSize = 1024; // System.IO.StreamReader.DefaultBufferSize

        /// <summary>
        ///     Returns the computed HMAC hex digest of the body (RFC3174), using the secret as the key.
        ///     See https://developer.github.com/v3/repos/hooks/#example, and
        ///     https://pubsubhubbub.github.io/PubSubHubbub/pubsubhubbub-core-0.4.html#authednotify
        /// </summary>
        public static string CreateHmacSignature(this HttpWebRequest request, string secret)
        {
            var encoding = Encoding.UTF8;
            var body = GetRawBody(request, encoding);

            var key = encoding.GetBytes(secret);

            var signature = SignBody(body, key);

            return signature;
        }

        private static byte[] GetRawBody(HttpWebRequest request, Encoding encoding)
        {
            var requestStream = request.GetRequestStream();
            requestStream.Position = 0;
            string body;
            using (var reader = CreateNonClosingStreamReader(requestStream, encoding))
            {
                body = reader.ReadToEnd();
            }
            requestStream.Position = 0;

            return encoding.GetBytes(body);
        }

        private static string SignBody(byte[] body, byte[] key)
        {
            var signature = ServiceStack.HmacUtils.CreateHashAlgorithm(key)
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

        /// <summary>
        ///     Returns a new instance of a <see cref="StreamReader" /> that does not dispose the underlying <see cref="Stream" />
        ///     when it is disposed
        /// </summary>
        private static StreamReader CreateNonClosingStreamReader(Stream stream, Encoding encoding)
        {
            return new StreamReader(stream, encoding, true, DefaultBufferSize, true);
        }
    }
}