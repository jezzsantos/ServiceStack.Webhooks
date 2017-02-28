using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace ServiceStack.Webhooks.Azure.Queue
{
    internal class AzureQueueStorage : IAzureQueueStorage
    {
        private const int CreationTimeoutSecs = 60;
        private const int MaxPeekableMessages = 32;
        private readonly string connectionString;
        private readonly object mutex = new object();

        private readonly string queueName;
        private DateTime lastCreationCheck = DateTime.MinValue;

        public AzureQueueStorage(string connectionString, string queueName)
        {
            Guard.AgainstNullOrEmpty(() => connectionString, connectionString);
            Guard.AgainstNullOrEmpty(() => queueName, queueName);

            this.queueName = queueName;
            this.connectionString = connectionString;
        }

        protected CloudQueue Queue { get; private set; }

        public void Enqueue(WebhookEvent @event)
        {
            EnsureQueue();

            var message = new CloudQueueMessage(@event.ToJson());
            Queue.AddMessage(message);
            ResetLastCreationCheckTime();
        }

        public List<WebhookEvent> Peek()
        {
            EnsureQueue();

            var count = GetMessageCount(Queue);
            if (count > 0)
            {
                var peekableCount = Math.Min(count, MaxPeekableMessages);

                var messages = Queue.PeekMessages(peekableCount)
                    .Select(msg => msg.AsString.FromJson<WebhookEvent>());
                ResetLastCreationCheckTime();
                return messages.ToList();
            }

            return new List<WebhookEvent>();
        }

        public void Empty()
        {
            EnsureQueue();

            Queue.Clear();
            ResetLastCreationCheckTime();
        }

        protected void EnsureQueue()
        {
            if (Queue == null)
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var client = storageAccount.CreateCloudQueueClient();
                Queue = client.GetQueueReference(queueName);
                InitLastCreationCheckTime();
            }

            if (RequiresCreationCheck())
            {
                Queue.CreateIfNotExists();
            }
        }

        private static int GetMessageCount(CloudQueue queue)
        {
            queue.FetchAttributes();
            return queue.ApproximateMessageCount ?? 0;
        }

        private void InitLastCreationCheckTime()
        {
            lock (mutex)
            {
                lastCreationCheck = DateTime.MinValue;
            }
        }

        private void ResetLastCreationCheckTime()
        {
            lock (mutex)
            {
                lastCreationCheck = DateTime.UtcNow;
            }
        }

        private bool RequiresCreationCheck()
        {
            TimeSpan sinceLast;
            lock (mutex)
            {
                sinceLast = DateTime.UtcNow.Subtract(lastCreationCheck);
            }

            return sinceLast >= TimeSpan.FromSeconds(CreationTimeoutSecs);
        }
    }
}