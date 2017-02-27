namespace ServiceStack.Webhooks
{
    /// <summary>
    ///     Defines the publisher of webhook events
    /// </summary>
    public interface IWebhooks
    {
        /// <summary>
        ///     Publishes an event to all webhook subscribers
        /// </summary>
        void Publish<TDto>(string eventName, TDto data);
    }
}