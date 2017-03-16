using ServiceStack.Logging;
using ServiceStack.Text;

namespace ServiceStack.Webhooks
{
    internal class WebhooksClient : IWebhooks
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(WebhooksClient));

        public IEventSink EventSink { get; set; }

        /// <summary>
        ///     Publishes webhook events to the <see cref="IEventSink" />
        /// </summary>
        public void Publish<TDto>(string eventName, TDto data) where TDto : class, new()
        {
            Guard.AgainstNullOrEmpty(() => eventName, eventName);

            logger.InfoFormat(@"Publishing webhook event {0}, with data {1}", eventName, data.ToJson());

            EventSink.Write(new WebhookEvent
            {
                Id = DataFormats.CreateEntityIdentifier(),
                EventName = eventName,
                Data = data,
                CreatedDateUtc = SystemTime.UtcNow.ToNearestMillisecond()
            });
        }
    }
}