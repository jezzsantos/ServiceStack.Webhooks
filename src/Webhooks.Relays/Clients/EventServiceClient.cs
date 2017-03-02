using System;
using System.Net;
using ServiceStack.Logging;
using ServiceStack.Webhooks.Clients;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.Relays.Clients
{
    public class EventServiceClient : IWebhookEventServiceClient
    {
        internal const int DefaultRetries = 3;
        internal const int DefaultTimeout = 60;
        private readonly ILog logger = LogManager.GetLogger(typeof(EventServiceClient));

        public EventServiceClient()
        {
            Retries = DefaultRetries;
            Timeout = TimeSpan.FromSeconds(DefaultTimeout);
        }

        public IEventServiceClientFactory ServiceClientFactory { get; set; }

        public TimeSpan? Timeout { get; set; }

        public int Retries { get; set; }

        public void Post(SubscriptionConfig subscription, string eventName, object data)
        {
            Guard.AgainstNull(() => subscription, subscription);
            Guard.AgainstNullOrEmpty(() => eventName, eventName);

            var serviceClient = CreateServiceClient(subscription, eventName, Timeout);

            var attempts = 0;

            while (attempts <= Retries)
            {
                attempts++;

                try
                {
                    serviceClient.Post(subscription.Url, data);
                    return;
                }
                catch (WebServiceException ex)
                {
                    if (HasNoMoreRetries(attempts) || ex.IsAny400())
                    {
                        logger.Warn("Failed to notify subscriber at {0} (after {1} attempts)".Fmt(subscription.Url, attempts), ex);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    // Timeout (WebException) or other Exception
                    if (HasNoMoreRetries(attempts))
                    {
                        logger.Warn("Failed to notify subscriber at {0} (after {1} attempts)".Fmt(subscription.Url, attempts), ex);
                        return;
                    }
                }
            }
        }

        private bool HasNoMoreRetries(int attempts)
        {
            return attempts == Retries;
        }

        /// <summary>
        ///     Creates an instance of a serviceclient and configures it to send an event notification.
        ///     See: https://developer.github.com/webhooks/#payloads for specification
        /// </summary>
        private IServiceClient CreateServiceClient(SubscriptionConfig config, string eventName, TimeSpan? timeout)
        {
            try
            {
                var client = ServiceClientFactory.Create(config.Url);
                client.Timeout = timeout;
                client.RequestFilter = request =>
                {
                    request.ContentType = MimeTypes.Json;
                    request.Headers.Remove(WebhookEventConstants.SecretSignatureHeaderName);
                    request.Headers.Remove(WebhookEventConstants.RequestIdHeaderName);
                    request.Headers.Remove(WebhookEventConstants.EventNameHeaderName);

                    if (config.ContentType.HasValue())
                    {
                        request.ContentType = config.ContentType;
                    }
                    if (config.Secret.HasValue())
                    {
                        request.Headers.Add(WebhookEventConstants.SecretSignatureHeaderName, CreateContentHmacSignature(request, config.Secret));
                    }
                    request.Headers.Add(WebhookEventConstants.RequestIdHeaderName, CreateRequestIdentifier());
                    request.Headers.Add(WebhookEventConstants.EventNameHeaderName, eventName);
                };
                return client;
            }
            catch (Exception ex)
            {
                logger.Error(@"Failed to connect to subscriber: {0}, this URL is not valid".Fmt(config.Url), ex);
                return null;
            }
        }

        private static string CreateRequestIdentifier()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        ///     Returns the computed HMAC hex digest of the body (RFC3174), using the secret as the key.
        ///     See https://developer.github.com/v3/repos/hooks/#example, and
        ///     https://pubsubhubbub.github.io/PubSubHubbub/pubsubhubbub-core-0.4.html#authednotify
        /// </summary>
        private static string CreateContentHmacSignature(HttpWebRequest request, string secret)
        {
            //TODO: calculate the HMAC SHA1 digest

            return string.Empty;
        }
    }
}