namespace ServiceStack.Webhooks.Azure.IntTests
{
    internal class TestService : Service
    {
        public IWebhooks Webhooks { get; set; }

        public void Any(RaiseEvent request)
        {
            Webhooks.Publish(request.EventName, new TestEvent());
        }
    }

    [Route("/raise")]
    public class RaiseEvent : IReturnVoid
    {
        public string EventName { get; set; }
    }

    public class TestEvent
    {
    }
}