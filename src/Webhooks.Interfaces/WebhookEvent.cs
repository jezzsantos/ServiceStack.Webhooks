using System;
using System.Collections.Generic;
using ServiceStack.Model;

namespace ServiceStack.Webhooks
{
    public class WebhookEvent : IHasStringId
    {
        public string EventName { get; set; }

        public DateTime CreatedDateUtc { get; set; }

        public Dictionary<string, string> Data { get; set; }

        public string Id { get; set; }
    }
}