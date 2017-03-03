using System.Collections.Generic;
using ServiceStack.Logging;
using ServiceStack.Text;

namespace ServiceStack.Webhooks
{
    internal class WebhooksClient : IWebhooks
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(WebhooksClient));

        public IWebhookEventSink EventSink { get; set; }

        /// <summary>
        ///     Publishes webhook events to the <see cref="IWebhookEventSink" />
        /// </summary>
        public void Publish<TDto>(string eventName, TDto data) where TDto : class, new()
        {
            Guard.AgainstNullOrEmpty(() => eventName, eventName);

            logger.InfoFormat(@"Publishing webhook event {0}, with data {1}", eventName, data.ToJson());

            EventSink.Write(eventName, CreateDictionary(data));
        }

        private static Dictionary<string, string> CreateDictionary(object data)
        {
            //ISSUE: if not a POCO of some kind (i.e. string or int etc)
            return data.ToStringDictionary();
        }
    }
}