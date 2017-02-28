using System.Collections.Generic;

namespace ServiceStack.Webhooks.Azure.Queue
{
    internal interface IAzureQueueStorage
    {
        /// <summary>
        ///     Queues the specified entity to storage
        /// </summary>
        void Enqueue(WebhookEvent @event);

        /// <summary>
        ///     Gets all the events from storage
        /// </summary>
        List<WebhookEvent> Peek();

        /// <summary>
        ///     Empties all event from storage
        /// </summary>
        void Empty();
    }
}