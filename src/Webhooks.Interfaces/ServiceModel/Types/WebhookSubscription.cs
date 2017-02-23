using System;

namespace ServiceStack.Webhooks.ServiceModel.Types
{
    public class WebhookSubscription
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Event { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDateUtc { get; set; }

        public string CreatedById { get; set; }

        public DateTime LastModifiedDateUtc { get; set; }

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