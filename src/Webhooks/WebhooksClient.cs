using ServiceStack.Logging;

namespace ServiceStack.Webhooks
{
    internal class WebhooksClient : IWebhooks
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(WebhooksClient));

        public IWebhookEventStore EventStore { get; set; }

        /// <summary>
        ///     Publishes webhook events to the <see cref="IWebhookEventStore" />
        /// </summary>
        public void Publish<TDto>(string eventName, TDto data)
        {
            Guard.AgainstNullOrEmpty(() => eventName, eventName);

            logger.InfoFormat(@"Publishing webhook event {0}, with data {1}", eventName, data.ToJson());

            EventStore.Create(eventName, data);
        }
    }
}