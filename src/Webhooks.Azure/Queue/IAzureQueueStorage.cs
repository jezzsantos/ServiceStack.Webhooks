using System.Collections.Generic;

namespace ServiceStack.Webhooks.Azure.Queue
{
    internal interface IAzureQueueStorage<TEntity>
    {
        string QueueName { get; }

        /// <summary>
        ///     Queues the specified entity to storage
        /// </summary>
        void Enqueue(TEntity @event);

        /// <summary>
        ///     Gets all the events from storage
        /// </summary>
        List<TEntity> Peek();

        /// <summary>
        ///     Empties all event from storage
        /// </summary>
        void Empty();

        List<TEntity> RemoveMessages(int count);
    }
}