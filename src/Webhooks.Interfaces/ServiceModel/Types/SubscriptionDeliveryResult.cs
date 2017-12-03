using System;
using System.Net;
using ServiceStack.Model;

namespace ServiceStack.Webhooks.ServiceModel.Types
{
    public class SubscriptionDeliveryResult : IHasStringId
    {
        public DateTime AttemptedDateUtc { get; set; }

        public string StatusDescription { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string SubscriptionId { get; set; }

        public string Id { get; set; }

        public string EventId { get; set; }
    }
}