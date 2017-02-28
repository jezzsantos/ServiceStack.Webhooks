using System;
using System.Collections.Generic;
using ServiceStack.Configuration;
using ServiceStack.Webhooks.Azure.Queue;

namespace ServiceStack.Webhooks.Azure
{
    public class AzureQueueEventStore : IWebhookEventStore
    {
        internal const string DefaultQueueName = "webhookevents";
        internal const string AzureConnectionStringSettingName = "AzureQueueEventStore.AzureConnectionString";
        internal const string DefaultAzureConnectionString = @"UseDevelopmentStorage=true";
        private IAzureQueueStorage queueStorage;

        public AzureQueueEventStore()
        {
            QueueName = DefaultQueueName;

            AzureConnectionString = DefaultAzureConnectionString;
        }

        public AzureQueueEventStore(IAppSettings settings)
            : this()
        {
            Guard.AgainstNull(() => settings, settings);

            AzureConnectionString = settings.Get(AzureConnectionStringSettingName, DefaultAzureConnectionString);
        }

        /// <summary>
        ///     For testing only
        /// </summary>
        internal IAzureQueueStorage QueueStorage
        {
            get { return queueStorage ?? (queueStorage = new AzureQueueStorage(AzureConnectionString, QueueName)); }
            set { queueStorage = value; }
        }

        public string AzureConnectionString { get; set; }

        public string QueueName { get; set; }

        public void Create<TDto>(string eventName, TDto data)
        {
            Guard.AgainstNullOrEmpty(() => eventName, eventName);

            QueueStorage.Enqueue(new WebhookEvent
            {
                EventName = eventName,
                Data = data,
                CreatedDateUtc = DateTime.UtcNow.ToNearestMillisecond()
            });
        }

        public List<WebhookEvent> Peek()
        {
            return QueueStorage.Peek();
        }

        public void Clear()
        {
            QueueStorage.Empty();
        }
    }
}