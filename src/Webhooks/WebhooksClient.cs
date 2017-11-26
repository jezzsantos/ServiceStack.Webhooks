using System;
using ServiceStack.Logging;
using ServiceStack.Text;

namespace ServiceStack.Webhooks
{
    internal class WebhooksClient : IWebhooks
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(WebhooksClient));

        public IEventSink EventSink { get; set; }

        public Action<WebhookEvent> PublishFilter { get; set; }

        /// <summary>
        ///     Publishes webhook events to the <see cref="T:ServiceStack.Webhooks.IEventSink" />
        /// </summary>
        public void Publish<TDto>(string eventName, TDto data) where TDto : class, new()
        {
            Guard.AgainstNullOrEmpty(() => eventName, eventName);

            logger.InfoFormat(@"Publishing webhook event {0}, with data {1}", eventName, data.ToJson());

            var @event = CreateEvent(eventName, data);
            if (@event != null)
            {
                EventSink.Write(@event);
            }
        }

        public virtual WebhookEvent CreateEvent<TDto>(string eventName, TDto data) where TDto : class, new()
        {
            var @event = new WebhookEvent
            {
                Id = DataFormats.CreateEntityIdentifier(),
                EventName = eventName,
                Data = data,
                CreatedDateUtc = SystemTime.UtcNow.ToNearestMillisecond(),
                Origin = null,
            };

            PublishFilter?.Invoke(@event);

            return @event;
        }
    }
}