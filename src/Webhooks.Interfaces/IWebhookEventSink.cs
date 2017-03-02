namespace ServiceStack.Webhooks
{
    public interface IWebhookEventSink
    {
        /// <summary>
        ///     Writes a new event with data
        /// </summary>
        void Write<TDto>(string eventName, TDto data);
    }
}