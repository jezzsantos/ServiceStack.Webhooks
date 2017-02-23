using System;
using System.Diagnostics;
using System.Globalization;

namespace ServiceStack.Webhooks
{
    /// <summary>
    ///     Extensions to the <see cref="DateTime" /> class
    /// </summary>
    [DebuggerStepThrough]
    public static class DateTimeExtensions
    {
        /// <summary>
        ///     Returns the <see cref="DateTime.MinValue" /> version in UTC.
        /// </summary>
        public static DateTime MinDateTimeUtc
        {
            get { return new DateTime(1, 1, 1, 0, 0, 0, DateTimeKind.Utc); }
        }

        /// <summary>
        ///     Whether the specified date a value assigned to it
        /// </summary>
        /// <remarks>
        ///     The specified date may be a UTC datetime or not, either way this function determines whether the date is NOT close
        ///     to the
        ///     <see cref="DateTime.MinValue" />
        /// </remarks>
        public static bool HasValue(this DateTime current)
        {
            if (((current.Kind == DateTimeKind.Local)
                 || (current.Kind == DateTimeKind.Unspecified))
                && current.Equals(DateTime.MinValue))
            {
                return false;
            }

            return !(current.Equals(DateTime.MinValue.ToUniversalTime())
                     || current.Equals(MinDateTimeUtc));
        }

        /// <summary>
        ///     Returns the specified <see cref="DateTime" /> as a ISO8601 date and time (i.e. 2016-12-31T06:34:45.0000000)
        /// </summary>
        public static string ToIso8601(this DateTime current)
        {
            if ((current.Kind == DateTimeKind.Local)
                || (current.Kind == DateTimeKind.Unspecified))
            {
                return current.ToString("O");
            }

            return current.Equals(DateTime.MinValue.ToUniversalTime())
                ? MinDateTimeUtc.ToString("O")
                : current.ToString("O");
        }

        /// <summary>
        ///     Returns the specified <see cref="DateTimeOffset" /> as a ISO8601 date and time
        /// </summary>
        public static string ToIso8601(this DateTimeOffset current)
        {
            return current.ToString("O");
        }

        /// <summary>
        ///     Returns the <see cref="DateTime" /> for the specified ISO8601 formatted date.
        /// </summary>
        public static DateTime FromIso8601(this string current)
        {
            DateTime dateTime;
            if (DateTime.TryParse(current, null, DateTimeStyles.RoundtripKind, out dateTime))
            {
                return dateTime;
            }
            return DateTime.MinValue;
        }

        /// <summary>
        ///     Rounds up or down the specified <see cref="DateTime" /> to the nearest second
        /// </summary>
        public static DateTime ToNearestSecond(this DateTime current)
        {
            return current.RoundTo(TimeSpan.FromSeconds(1), DateRounding.Nearest);
        }

        /// <summary>
        ///     Rounds the specified <see cref="DateTime" /> to the nearest second
        /// </summary>
        public static DateTime ToNearestSecond(this DateTime current, DateRounding rounding)
        {
            return current.RoundTo(TimeSpan.FromSeconds(1), rounding);
        }

        /// <summary>
        ///     Rounds up or down the specified <see cref="DateTime" /> to the nearest minute
        /// </summary>
        public static DateTime ToNearestMinute(this DateTime current)
        {
            return current.RoundTo(TimeSpan.FromMinutes(1), DateRounding.Nearest);
        }

        /// <summary>
        ///     Rounds the specified <see cref="DateTime" /> to the nearest minute
        /// </summary>
        public static DateTime ToNearestMinute(this DateTime current, DateRounding rounding)
        {
            return current.RoundTo(TimeSpan.FromMinutes(1), rounding);
        }

        /// <summary>
        ///     Rounds up the specified <see cref="DateTime" /> to the next quarter hour
        /// </summary>
        public static DateTime ToNextQuarterHour(this DateTime current)
        {
            var duration = TimeSpan.FromMinutes(15);
            return current.RoundTo(duration, DateRounding.Up);
        }

        /// <summary>
        ///     Rounds the current <see cref="DateTime" />to the nearest specified <see cref="interval" />, with the specified
        ///     <see cref="DateRounding" />
        /// </summary>
        public static DateTime RoundTo(this DateTime current, TimeSpan interval, DateRounding rounding)
        {
            if (interval == TimeSpan.Zero)
            {
                return current;
            }

            switch (rounding)
            {
                case DateRounding.Down:
                    return current.RoundDown(interval);

                case DateRounding.Up:
                    return current.RoundUp(interval);

                default:
                    return current.RoundToNearest(interval);
            }
        }

        /// <summary>
        ///     Whether the current <see cref="DateTime" /> is near (in time) to the specified <see cref="time" />, plus/minus
        ///     1000ms
        /// </summary>
        public static bool IsNear(this DateTime current, DateTime time)
        {
            return current.IsNear(time, 1000);
        }

        /// <summary>
        ///     Whether the current <see cref="DateTime" /> is near (in time) to the specified <see cref="time" />, plus/minus the
        ///     specified
        ///     <see cref="withinMilliseconds" />
        /// </summary>
        public static bool IsNear(this DateTime current, DateTime time, int withinMilliseconds)
        {
            return current.IsNear(time, TimeSpan.FromMilliseconds(withinMilliseconds));
        }

        /// <summary>
        ///     Whether the current <see cref="DateTime" /> is near (in time) to the specified <see cref="time" />, plus/minus the
        ///     specified
        ///     <see cref="TimeSpan" />
        /// </summary>
        public static bool IsNear(this DateTime current, DateTime time, TimeSpan within)
        {
            var withinMilliseconds = within.TotalMilliseconds;

            return (current.AddMilliseconds(withinMilliseconds) >= time)
                   && (current.AddMilliseconds(-withinMilliseconds) <= time);
        }

        /// <summary>
        ///     Whether the current <see cref="TimeSpan" /> is near (in time) to the specified <see cref="duration" />, plus/minus
        ///     1000ms
        /// </summary>
        public static bool IsNear(this TimeSpan current, TimeSpan duration)
        {
            return current.IsNear(duration, 1000);
        }

        /// <summary>
        ///     Whether the current <see cref="TimeSpan" /> is near (in time) to the specified <see cref="duration" />, plus/minus
        ///     the specified
        ///     <see cref="withinMilliseconds" />
        /// </summary>
        public static bool IsNear(this TimeSpan current, TimeSpan duration, int withinMilliseconds)
        {
            return (current.Add(TimeSpan.FromMilliseconds(withinMilliseconds)) >= duration)
                   && (current.Subtract(TimeSpan.FromMilliseconds(withinMilliseconds)) <= duration);
        }

        /// <summary>
        ///     Whether the current <see cref="DateTime" /> is after the specified <see cref="after" /> and before the specified
        ///     <see cref="before" />
        /// </summary>
        public static bool IsBetween(this DateTime current, DateTime after, DateTime before)
        {
            return (current >= after)
                   && (current <= before);
        }

        private static DateTime RoundUp(this DateTime dateTime, TimeSpan interval)
        {
            if (dateTime == DateTime.MaxValue)
            {
                return dateTime;
            }

            var delta = (interval.Ticks - dateTime.Ticks%interval.Ticks)%interval.Ticks;
            return new DateTime(dateTime.Ticks + delta, dateTime.Kind);
        }

        private static DateTime RoundDown(this DateTime dateTime, TimeSpan interval)
        {
            if (!dateTime.HasValue())
            {
                return dateTime;
            }

            var delta = dateTime.Ticks%interval.Ticks;
            return new DateTime(dateTime.Ticks - delta, dateTime.Kind);
        }

        private static DateTime RoundToNearest(this DateTime dateTime, TimeSpan interval)
        {
            var delta = dateTime.Ticks%interval.Ticks;
            var roundUp = delta > interval.Ticks/2;

            return roundUp ? dateTime.RoundUp(interval) : dateTime.RoundDown(interval);
        }
    }

    public enum DateRounding
    {
        /// <summary>
        ///     Rounding either up or down to nearest
        /// </summary>
        Nearest = 0,

        /// <summary>
        ///     Rounding up
        /// </summary>
        Up = 1,

        /// <summary>
        ///     Rounding down
        /// </summary>
        Down = 2
    }
}