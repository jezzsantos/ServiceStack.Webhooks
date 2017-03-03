using System;
using ServiceStack.Configuration;
using ServiceStack.Webhooks.Azure.Queue;
using ServiceStack.Webhooks.Clients;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.Azure.Worker
{
    /// <summary>
    ///     Provides a queue processor for relaying webhook events from queue to subscribers
    /// </summary>
    public class EventRelayQueueProcessor : BaseQueueProcessor<WebhookEvent>
    {
        internal const int DefaultServiceClientRetries = 3;
        internal const int DefaultServiceClientTimeoutSeconds = 60;
        private const int DefaultPollingIntervalSeconds = 5;
        private const string DefaultTargetQueueName = AzureQueueWebhookEventSink.DefaultQueueName;
        private const string DefaultUnhandledQueueName = "unhandled" + AzureQueueWebhookEventSink.DefaultQueueName;
        private const string PollingIntervalSettingName = "EventRelayQueueProcessor.Polling.Interval.Seconds";
        internal const string AzureConnectionStringSettingName = "EventRelayQueueProcessor.ConnectionString";
        internal const string TargetQueueNameSettingName = "EventRelayQueueProcessor.TargetQueue.Name";
        internal const string UnhandledQueneNameStringSettingName = "EventRelayQueueProcessor.UnhandledQueue.Name";
        internal const string SeviceClientRetriesSettingName = "EventRelayQueueProcessor.ServiceClient.Retries";
        internal const string DefaultSeviceClientTimeoutSettingName = "EventRelayQueueProcessor.ServiceClient.Timeout.Seconds";

        private int pollingInterval;
        private IAzureQueueStorage<WebhookEvent> targetQueue;
        private IAzureQueueStorage<IUnhandledMessage> unhandledQueue;

        public EventRelayQueueProcessor()
        {
            ServiceClientRetries = DefaultServiceClientRetries;
            SeviceClientTimeoutSeconds = DefaultServiceClientTimeoutSeconds;
            pollingInterval = DefaultPollingIntervalSeconds;
            ConnectionString = AzureStorage.AzureEmulatorConnectionString;
        }

        public EventRelayQueueProcessor(IAppSettings settings) : this()
        {
            Guard.AgainstNull(() => settings, settings);

            ServiceClientRetries = settings.Get(SeviceClientRetriesSettingName, DefaultServiceClientRetries);
            SeviceClientTimeoutSeconds = settings.Get(DefaultSeviceClientTimeoutSettingName, DefaultServiceClientTimeoutSeconds);
            pollingInterval = settings.Get(PollingIntervalSettingName, DefaultPollingIntervalSeconds);
            ConnectionString = settings.Get(AzureConnectionStringSettingName, AzureStorage.AzureEmulatorConnectionString);
            TargetQueueName = settings.Get(TargetQueueNameSettingName, DefaultTargetQueueName);
            UnhandledQueueName = settings.Get(UnhandledQueneNameStringSettingName, DefaultUnhandledQueueName);
        }

        /// <summary>
        ///     For testing only
        /// </summary>
        public override IAzureQueueStorage<WebhookEvent> TargetQueue
        {
            get { return targetQueue ?? (targetQueue = new AzureQueueStorage<WebhookEvent>(ConnectionString, TargetQueueName)); }
            set { targetQueue = value; }
        }

        /// <summary>
        ///     For testing only
        /// </summary>
        public override IAzureQueueStorage<IUnhandledMessage> UnhandledQueue
        {
            get { return unhandledQueue ?? (unhandledQueue = new AzureQueueStorage<IUnhandledMessage>(ConnectionString, UnhandledQueueName)); }
            set { unhandledQueue = value; }
        }

        public IWebhookEventSubscriptionCache SubscriptionCache { get; set; }

        public IWebhookEventServiceClient ServiceClient { get; set; }

        public override int IntervalSeconds
        {
            get { return pollingInterval; }
            set { pollingInterval = value; }
        }

        public string ConnectionString { get; set; }

        public string TargetQueueName { get; set; }

        public string UnhandledQueueName { get; set; }

        public int ServiceClientRetries { get; set; }

        public int SeviceClientTimeoutSeconds { get; set; }

        public override bool ProcessMessage(WebhookEvent message)
        {
            var subscriptions = SubscriptionCache.GetAll(message.EventName);
            subscriptions.ForEach(sub =>
                    NotifySubscription(sub, message.EventName, message.Data));

            return true;
        }

        private void NotifySubscription<TDto>(SubscriptionConfig subscription, string eventName, TDto data)
        {
            ServiceClient.Retries = ServiceClientRetries;
            ServiceClient.Timeout = TimeSpan.FromSeconds(SeviceClientTimeoutSeconds);
            ServiceClient.Post(subscription, eventName, data);
        }
    }
}