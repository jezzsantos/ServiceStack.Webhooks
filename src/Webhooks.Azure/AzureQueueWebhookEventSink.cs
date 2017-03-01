using System;
using System.Collections.Generic;
using ServiceStack.Configuration;
using ServiceStack.Webhooks.Azure.Queue;

namespace ServiceStack.Webhooks.Azure
{
    public class AzureQueueWebhookEventSink : IWebhookEventSink
    {
        internal const string DefaultQueueName = "webhookevents";
        internal const string AzureConnectionStringSettingName = "AzureQueueWebhookEventSink.ConnectionString";
        internal const string DefaultAzureConnectionString = @"UseDevelopmentStorage=true";
        private IAzureQueueStorage queueStorage;

        public AzureQueueWebhookEventSink()
        {
            QueueName = DefaultQueueName;

            ConnectionString = DefaultAzureConnectionString;
        }

        public AzureQueueWebhookEventSink(IAppSettings settings)
            : this()
        {
            Guard.AgainstNull(() => settings, settings);

            ConnectionString = settings.Get(AzureConnectionStringSettingName, DefaultAzureConnectionString);
        }

        /// <summary>
        ///     For testing only
        /// </summary>
        internal IAzureQueueStorage QueueStorage
        {
            get { return queueStorage ?? (queueStorage = new AzureQueueStorage(ConnectionString, QueueName)); }
            set { queueStorage = value; }
        }

        public string ConnectionString { get; set; }

        public string QueueName { get; set; }

        public void Write<TDto>(string eventName, TDto data)
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