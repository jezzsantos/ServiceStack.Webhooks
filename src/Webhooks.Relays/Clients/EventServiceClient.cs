using System;
using System.Net;
using ServiceStack.Logging;
using ServiceStack.Webhooks.Clients;
using ServiceStack.Webhooks.Relays.Properties;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.Relays.Clients
{
    public class EventServiceClient : IEventServiceClient
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

        public SubscriptionDeliveryResult Relay(SubscriptionRelayConfig subscription, string eventName, object data)
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
                    using (var response = serviceClient.Post(subscription.Config.Url, data))
                    {
                        return CreateDeliveryResult(subscription.SubscriptionId, response.StatusCode, response.StatusDescription);
                    }
                }
                catch (WebServiceException ex)
                {
                    if (HasNoMoreRetries(attempts) || ex.IsAny400())
                    {
                        logger.Warn(Resources.EventServiceClient_FailedDelivery.Fmt(subscription.Config.Url, attempts), ex);
                        return CreateDeliveryResult(subscription.SubscriptionId, (HttpStatusCode) ex.StatusCode, ex.StatusDescription);
                    }
                }
                catch (Exception ex)
                {
                    // Timeout (WebException) or other Exception
                    if (HasNoMoreRetries(attempts))
                    {
                        var message = Resources.EventServiceClient_FailedDelivery.Fmt(subscription.Config.Url, attempts);
                        logger.Warn(message, ex);
                        return CreateDeliveryResult(subscription.SubscriptionId, HttpStatusCode.ServiceUnavailable, message);
                    }
                }
            }

            return null;
        }

        private SubscriptionDeliveryResult CreateDeliveryResult(string subscriptionId, HttpStatusCode statusCode, string statusDescription)
        {
            return new SubscriptionDeliveryResult
            {
                Id = DataFormats.CreateEntityIdentifier(),
                AttemptedDateUtc = DateTime.UtcNow.ToNearestMillisecond(),
                SubscriptionId = subscriptionId,
                StatusDescription = statusDescription,
                StatusCode = statusCode
            };
        }

        private bool HasNoMoreRetries(int attempts)
        {
            return attempts == Retries;
        }

        /// <summary>
        ///     Creates an instance of a serviceclient and configures it to send an event notification.
        ///     See: https://developer.github.com/webhooks/#payloads for specification
        /// </summary>
        private IServiceClient CreateServiceClient(SubscriptionRelayConfig relayConfig, string eventName, TimeSpan? timeout)
        {
            try
            {
                var client = ServiceClientFactory.Create(relayConfig.Config.Url);
                client.Timeout = timeout;
                client.RequestFilter = request =>
                {
                    request.ContentType = MimeTypes.Json;
                    request.Headers.Remove(WebhookEventConstants.SecretSignatureHeaderName);
                    request.Headers.Remove(WebhookEventConstants.RequestIdHeaderName);
                    request.Headers.Remove(WebhookEventConstants.EventNameHeaderName);

                    if (relayConfig.Config.ContentType.HasValue())
                    {
                        request.ContentType = relayConfig.Config.ContentType;
                    }
                    if (relayConfig.Config.Secret.HasValue())
                    {
                        request.Headers.Add(WebhookEventConstants.SecretSignatureHeaderName, CreateContentHmacSignature(request, relayConfig.Config.Secret));
                    }
                    request.Headers.Add(WebhookEventConstants.RequestIdHeaderName, CreateRequestIdentifier());
                    request.Headers.Add(WebhookEventConstants.EventNameHeaderName, eventName);
                };
                return client;
            }
            catch (Exception ex)
            {
                logger.Error(@"Failed to connect to subscriber: {0}, this URL is not valid".Fmt(relayConfig.Config.Url), ex);
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