using System;

namespace ServiceStack.Webhooks.Azure.Table
{
    public static class DateTimeExtensions
    {
        /// <summary>
        ///     Minimum <see cref="DateTime" /> that Azure table storage supports
        /// </summary>
        public static readonly DateTime MinAzureDateTime = new DateTime(1780, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        ///     Returns whether the specified <see cref="DateTime" /> is below the minimum Azure supports
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static bool IsMinAzureDateTime(this DateTime dateTime)
        {
            return dateTime <= MinAzureDateTime;
        }

        /// <summary>
        ///     Returns a <see cref="DateTime" /> safe for Azure storage
        /// </summary>
        public static DateTime ToSafeAzureDateTime(this DateTime dateTime)
        {
            return dateTime.IsMinAzureDateTime() ? MinAzureDateTime : dateTime;
        }
    }
}