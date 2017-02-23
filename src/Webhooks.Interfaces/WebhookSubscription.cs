using System.Collections.Generic;

namespace ServiceStack.Webhooks.Interfaces
{
    public class WebhookSubscription
    {
        public string Name { get; set; }

        public List<string> Events { get; set; }

        public bool IsActive { get; set; }

        public SubscriptionConfig Config { get; set; }
    }

    public class SubscriptionConfig
    {
        public string Url { get; set; }

        public string ContentType { get; set; }

        public string Secret { get; set; }

        public bool IsInsecureSsl { get; set; }
    }
}