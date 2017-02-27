namespace ServiceStack.Webhooks
{
    internal class WebhooksClient : IWebhooks
    {
        public IWebhookEventStore EventStore { get; set; }

        /// <summary>
        ///     Publishes webhook events to the <see cref="IWebhookEventStore" />
        /// </summary>
        public void Publish<TDto>(string eventName, TDto data)
        {
            Guard.AgainstNullOrEmpty(() => eventName, eventName);

            EventStore.Create(eventName, data);
        }
    }
}