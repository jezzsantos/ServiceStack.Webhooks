using System;
using System.Net;
using ServiceStack.Logging;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.Clients
{
    public class WebhookEventServiceClient : IWebhookEventServiceClient
    {
        internal const int DefaultRetries = 3;
        internal const int DefaultTimeout = 60;
        private readonly ILog logger = LogManager.GetLogger(typeof(WebhookEventServiceClient));

        public WebhookEventServiceClient()
        {
            Retries = DefaultRetries;
            Timeout = TimeSpan.FromSeconds(DefaultTimeout);
        }

        public IWebhookEventServiceClientFactory ServiceClientFactory { get; set; }

        public TimeSpan? Timeout { get; set; }

        public int Retries { get; set; }

        public void Post(SubscriptionConfig subscription, string eventName, object data)
        {
            Guard.AgainstNull(() => subscription, subscription);
            Guard.AgainstNullOrEmpty(() => eventName, eventName);

            var serviceClient = CreateServiceClient(subscription, eventName, Timeout);

            var attempts = Retries;

            while (attempts > 0)
            {
                attempts--;

                try
                {
                    serviceClient.Post(subscription.Url, data);
                    return;
                }
                catch (WebServiceException ex)
                {
                    if ((attempts == 0)
                        || (ex.StatusCode == (int) HttpStatusCode.BadRequest)
                        || (ex.StatusCode == (int) HttpStatusCode.Unauthorized))
                    {
                        logger.Error("Failed to notify subscriber {0} (after {1} attempts)".Fmt(subscription.Url, Retries - attempts), ex);
                        return;
                    }

                    logger.Warn("Failed to notify subscriber {0}".Fmt(subscription.Url), ex);
                }
                catch (WebException ex)
                {
                    logger.Warn("Failed to notify subscriber {0}".Fmt(subscription.Url), ex);

                    // Timeout?
                    if (attempts == 0)
                    {
                        logger.Error("Failed to notify subscriber {0} (after {1} attempts)".Fmt(subscription.Url, Retries), ex);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn("Failed to notify subscriber {0}".Fmt(subscription.Url), ex);
                    // Other problem
                    if (attempts == 0)
                    {
                        logger.Error("Failed to notify subscriber {0} (after {1} attempts)".Fmt(subscription.Url, Retries), ex);
                        return;
                    }
                }
            }
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
                    request.Headers.Remove(WebhookEventConstants.SecretHeaderName);
                    request.Headers.Remove(WebhookEventConstants.RequestIdHeaderName);
                    request.Headers.Remove(WebhookEventConstants.EventNameHeaderName);

                    if (config.ContentType.HasValue())
                    {
                        request.ContentType = config.ContentType;
                    }
                    if (config.Secret.HasValue())
                    {
                        request.Headers.Add(WebhookEventConstants.SecretHeaderName, CreateContentHmacSignature(request, config.Secret));
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
        ///     Returns the computed HMAC hex digest of the body, using the secret as the key.
        ///     See https://developer.github.com/v3/repos/hooks/#example
        /// </summary>
        private static string CreateContentHmacSignature(HttpWebRequest request, string secret)
        {
            //TODO: calculate the digest

            return string.Empty;
        }
    }
}