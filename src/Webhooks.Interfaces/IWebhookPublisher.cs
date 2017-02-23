namespace ServiceStack.Webhooks.Interfaces
{
    public interface IWebhookPublisher
    {
        /// <summary>
        ///     Publishes the specified event, with its data
        /// </summary>
        void Publish<TDto>(string eventName, TDto data);
    }
}