using System;
using ServiceStack.Model;

namespace ServiceStack.Webhooks
{
    public class WebhookEvent : IHasStringId
    {
        public string EventName { get; set; }

        public DateTime CreatedDateUtc { get; set; }

        public object Data { get; set; }

        public string Id { get; set; }
    }
}