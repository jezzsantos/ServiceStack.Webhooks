using System;
using System.Collections.Generic;

namespace ServiceStack.Webhooks
{
    public interface IWebhookEventSink
    {
        /// <summary>
        ///     Creates a new event with data
        /// </summary>
        void Create<TDto>(string eventName, TDto data);

        /// <summary>
        ///     Returns the events waiting to be published to subscribers
        /// </summary>
        List<WebhookEvent> Peek();
    }

    public class WebhookEvent
    {
        public string EventName { get; set; }

        public DateTime CreatedDateUtc { get; set; }

        public object Data { get; set; }
    }
}