using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.Caching;

namespace ServiceStack.Webhooks
{
    internal class AppHostWebhookEventSink : IWebhookEventSink
    {
        internal const string CachekeyFormat = @"events:{0}:{1}";

        public ICacheClient CacheClient { get; set; }

        public void Create<TDto>(string eventName, TDto data)
        {
            Guard.AgainstNullOrEmpty(() => eventName, eventName);

            CacheClient.Add(FormatCacheKey(eventName), new WebhookEvent
            {
                CreatedDateUtc = DateTime.UtcNow.ToNearestMillisecond(),
                EventName = eventName,
                Data = data
            });
        }

        public List<WebhookEvent> Peek()
        {
            var keys = CacheClient.GetAllKeys();
            var events = CacheClient.GetAll<WebhookEvent>(keys)
                .OrderByDescending(whe => whe.Value.CreatedDateUtc);

            return events.Select(pair => pair.Value)
                .ToList();
        }

        internal static string FormatCacheKey(string eventName)
        {
            var time = DateTime.UtcNow.ToNearestMillisecond().ToIso8601();

            return CachekeyFormat.Fmt(eventName, time);
        }
    }
}