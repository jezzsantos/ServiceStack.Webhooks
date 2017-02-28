using System;

namespace ServiceStack.Webhooks.Azure.Table
{
    public static class DateTimeExtensions
    {
        public static readonly DateTime MinAzureDateTime = new DateTime(1780, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static bool IsMinAzureDateTime(this DateTime dateTime)
        {
            return dateTime <= MinAzureDateTime;
        }
    }
}