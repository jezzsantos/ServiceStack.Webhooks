using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ServiceStack.Webhooks
{
    /// <summary>
    ///     Provides formatting of strings using object properties.
    /// </summary>
    [DebuggerStepThrough]
    public static class StringExtensions
    {
        /// <summary>
        ///     Whether the specified value is string representation of a <see cref="DateTime" />
        /// </summary>
        public static bool IsDateTime(this string value)
        {
            DateTime dateValue;
            return DateTime.TryParse(value, out dateValue);
        }

        /// <summary>
        ///     Whether the specified value is a string representation of a <see cref="Guid" />.
        /// </summary>
        public static bool IsGuid(this string value)
        {
            Guid guid;
            return Guid.TryParse(value, out guid);
        }

        /// <summary>
        ///     Whether the specified value in not null and not empty
        /// </summary>
        public static bool HasValue(this string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        /// <summary>
        ///     Whether the specified value is exactly equal to other value
        /// </summary>
        public static bool EqualsOrdinal(this string value, string other)
        {
            return string.Equals(value, other, StringComparison.Ordinal);
        }

        /// <summary>
        ///     Whether the specified value is not exactly equal to other value
        /// </summary>
        public static bool NotEqualsOrdinal(this string value, string other)
        {
            return !value.EqualsOrdinal(other);
        }

        /// <summary>
        ///     Whether the specified value is not equal to other value
        /// </summary>
        public static bool NotEqualsIgnoreCase(this string value, string other)
        {
            return !value.EqualsIgnoreCase(other);
        }

        /// <summary>
        ///     Makes a string camel cased.
        /// </summary>
        /// <param name="identifier"> The identifier to camel case </param>
        public static string MakeCamel(this string identifier)
        {
            if (identifier.Length <= 2)
            {
                return identifier.ToLowerInvariant();
            }
            if (char.IsUpper(identifier[0]))
            {
                return char.ToLowerInvariant(identifier[0]) + identifier.Substring(1);
            }
            return identifier;
        }

        /// <summary>
        ///     Whether the <see cref="formattedString" /> has been formatted from the specified <see cref="formatString" />.
        /// </summary>
        /// <remarks>
        ///     This function is useful for comparing two strings where the <see cref="formattedString" /> is the result of a
        ///     String.Format operation on
        ///     the <see cref="formatString" />, with one or more format substitutions.
        ///     For example: Calling this function with a string "My code is 5" and a resource string "My code is {0}" that
        ///     contains one or more formatting arguments, return
        ///     <c> true </c>
        /// </remarks>
        public static bool IsFormattedFrom(this string formattedString, string formatString)
        {
            var escapedPattern = formatString
                .Replace("[", "\\[")
                .Replace("]", "\\]")
                .Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace(".", "\\.")
                .Replace("<", "\\<")
                .Replace(">", "\\>");

            var pattern = Regex.Replace(escapedPattern, @"\{\d+\}", ".*")
                .Replace(" ", @"\s");

            return new Regex(pattern).IsMatch(formattedString);
        }

        /// <summary>
        ///     Ensures the specified path has no trailing slash.
        /// </summary>
        public static string WithoutTrailingSlash(this string path)
        {
            return path.Trim('/');
        }

        /// <summary>
        ///     Ensures that the specified path has a leading slash
        /// </summary>
        public static string WithLeadingSlash(this string path)
        {
            return "/{0}".Fmt(path.TrimStart('/'));
        }

        /// <summary>
        ///     Returns an array of values split from the specified string, by the specified delimiters
        /// </summary>
        public static string[] SafeSplit(this string value, char[] delimiters, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            if (!value.HasValue())
            {
                return new string[]
                {
                };
            }

            return value.Split(delimiters, options);
        }

        /// <summary>
        ///     Returns an array of values split from the specified string, by the specified delimiters
        /// </summary>
        public static string[] SafeSplit(this string value, string[] delimiters, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            if (!value.HasValue())
            {
                return new string[]
                {
                };
            }

            return value.Split(delimiters, options);
        }

        /// <summary>
        ///     Returns an array of values split from the specified string, by the specified delimiters
        /// </summary>
        public static string[] SafeSplit(this string value, string delimiter, StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries)
        {
            return value.SafeSplit(new[]
            {
                delimiter
            }, options);
        }
    }
}