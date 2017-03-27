using System;
using System.Net;
using ServiceStack.Text;

namespace ServiceStack.Webhooks.Relays.Clients
{
    internal class ServiceClient : JsonServiceClient, IServiceClient
    {
        public ServiceClient(string url) : base(url)
        {
        }

        public HttpWebResponse Post<TRequest>(string url, TRequest request)
        {
            return Post<HttpWebResponse>(url, request);
        }

        public Action<HttpWebRequest, byte[], object> OnSerializeRequest { get; set; }

        protected override WebRequest SendRequest(string httpMethod, string requestUri, object request)
        {
            if (OnSerializeRequest == null)
            {
                return base.SendRequest(httpMethod, requestUri, request);
            }

            return PrepareWebRequest(httpMethod, requestUri, request, client =>
            {
                using (var tempStream = MemoryStreamFactory.GetStream())
                using (var requestStream = PclExport.Instance.GetRequestStream(client))
                {
                    SerializeRequestToStream(request, tempStream, true);
                    var bytes = tempStream.ToArray();

                    OnSerializeRequest(client, bytes, request);

                    requestStream.Write(bytes, 0, bytes.Length);
                }
            });
        }
    }
}