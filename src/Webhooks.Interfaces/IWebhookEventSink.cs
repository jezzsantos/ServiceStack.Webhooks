using System;

namespace ServiceStack.Webhooks
{
    public interface IWebhookEventSink
    {
        /// <summary>
        ///     Writes a new event with data
        /// </summary>
        void Write<TDto>(string eventName, TDto data);
    }

    public class WebhookEvent
    {
        public string EventName { get; set; }

        public DateTime CreatedDateUtc { get; set; }

        public object Data { get; set; }
    }
}