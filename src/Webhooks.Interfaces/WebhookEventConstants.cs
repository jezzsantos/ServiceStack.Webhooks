namespace ServiceStack.Webhooks
{
    public static class WebhookEventConstants
    {
        public const string SecretSignatureHeaderName = @"X-Hub-Signature";
        public const string RequestIdHeaderName = @"X-Webhook-Delivery";
        public const string EventNameHeaderName = @"X-Webhook-Event";
    }
}