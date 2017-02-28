using System;
using System.Collections.Generic;

namespace ServiceStack.Webhooks.Azure
{
    public class AzureQueueEventStore : IWebhookEventStore
    {
        public void Create<TDto>(string eventName, TDto data)
        {
            Guard.AgainstNullOrEmpty(() => eventName, eventName);

            throw new NotImplementedException();
        }

        public List<WebhookEvent> Peek()
        {
            throw new NotImplementedException();
        }
    }
}