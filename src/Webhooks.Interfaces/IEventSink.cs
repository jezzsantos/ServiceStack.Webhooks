using System.Collections.Generic;

namespace ServiceStack.Webhooks
{
    public interface IEventSink
    {
        /// <summary>
        ///     Writes a new event with data
        /// </summary>
        void Write(WebhookEvent webhookEvent);
    }
}