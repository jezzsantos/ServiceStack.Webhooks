namespace ServiceStack.Webhooks
{
    public interface IEventPublisher
    {
        /// <summary>
        ///     Publishes the specified event, with its data
        /// </summary>
        void Publish<TDto>(string eventName, TDto data);
    }
}