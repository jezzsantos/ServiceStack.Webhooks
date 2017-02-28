using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace ServiceStack.Webhooks.Azure.Table
{
    internal class WebhookSubscriptionEntity : TableEntity
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Event { get; set; }

        public string IsActive { get; set; }

        public DateTime CreatedDateUtc { get; set; }

        public string CreatedById { get; set; }

        public DateTime LastModifiedDateUtc { get; set; }

        public string Config { get; set; }
    }
}