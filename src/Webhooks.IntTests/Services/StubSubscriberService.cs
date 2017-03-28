using System.Collections.Generic;
using ServiceStack.Webhooks.Security;
using ServiceStack.Webhooks.Subscribers.Security;

namespace ServiceStack.Webhooks.IntTests.Services
{
    internal class StubSubscriberService : Service
    {
        public const string SubscriberSecret = "asignaturesecret";

        private static readonly List<ConsumedEvent> Events = new List<ConsumedEvent>();

        [Route("/consume", "POST")]
        public void Post(ConsumeEvent request)
        {
            ConsumeEvent(request);
        }

        [Authenticate(HmacAuthProvider.Name), Route("/consume/secured", "POST")]
        public void Post(ConsumeEventWithAuth request)
        {
            ConsumeEvent(request);
        }

        private void ConsumeEvent(ConsumeEvent request)
        {
            var isValidSignature = false;
            var incomingSignature = Request.Headers[WebhookEventConstants.SecretSignatureHeaderName];
            if (incomingSignature != null)
            {
                isValidSignature = Request.VerifySignature(incomingSignature, SubscriberSecret);
            }

            Events.Add(new ConsumedEvent
            {
                EventName = Request.Headers[WebhookEventConstants.EventNameHeaderName],
                Signature = incomingSignature,
                IsAuthenticated = isValidSignature,
                Data = request.ConvertTo<TestEvent>()
            });
        }

        public GetConsumedEventsResponse Get(GetConsumedEvents request)
        {
            return new GetConsumedEventsResponse
            {
                Events = Events
            };
        }

        public void Put(ResetConsumedEvents request)
        {
            Events.Clear();
        }
    }

    [Route("/consumed/reset", "PUT")]
    public class ResetConsumedEvents : IReturnVoid
    {
    }

    [Route("/consumed", "GET")]
    public class GetConsumedEvents : IReturn<GetConsumedEventsResponse>
    {
    }

    public class GetConsumedEventsResponse
    {
        public List<ConsumedEvent> Events { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
    }

    public class ConsumeEvent : IReturnVoid
    {
        public object A { get; set; }

        public object B { get; set; }

        public ConsumedNestedObject C { get; set; }
    }

    public class ConsumeEventWithAuth : ConsumeEvent
    {
    }

    public class ConsumedNestedObject
    {
        public object D { get; set; }

        public object E { get; set; }

        public object F { get; set; }
    }

    public class ConsumedEvent
    {
        public string EventName { get; set; }

        public TestEvent Data { get; set; }

        public string Signature { get; set; }

        public bool IsAuthenticated { get; set; }
    }
}