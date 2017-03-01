namespace ServiceStack.Webhooks
{
    public static class WebhookEventConstants
    {
        public const string SecretHeaderName = @"X-Webhook-Signature";
        public const string RequestIdHeaderName = @"X-Webhook-Delivery";
        public const string EventNameHeaderName = @"X-Webhook-Event";
    }
}