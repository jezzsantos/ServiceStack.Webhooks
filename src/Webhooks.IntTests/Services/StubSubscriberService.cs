using System.Collections.Generic;

namespace ServiceStack.Webhooks.IntTests.Services
{
    internal class StubSubscriberService : Service
    {
        private static readonly List<ConsumedEvent> Events = new List<ConsumedEvent>();

        public void Post(ConsumeEvent request)
        {
            Events.Add(new ConsumedEvent
            {
                EventName = Request.Headers[WebhookEventConstants.EventNameHeaderName],
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

    [Route("/consume", "POST")]
    public class ConsumeEvent : IReturnVoid
    {
        public object A { get; set; }

        public object B { get; set; }

        public ConsumedNestedObject C { get; set; }
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
    }
}