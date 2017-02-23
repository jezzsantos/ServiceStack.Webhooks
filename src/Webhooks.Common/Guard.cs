using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using ServiceStack.Webhooks.Properties;

namespace ServiceStack.Webhooks
{
    /// <summary>
    ///     Validates common arguments.
    /// </summary>
    [DebuggerStepThrough]
    public static class Guard
    {
        /// <summary>
        ///     Ensures the given <paramref name="value" /> is not null, otherwise throws <see cref="ArgumentNullException" />.
        /// </summary>
        /// <typeparam name="TValue"> The type of the value to be validated. </typeparam>
        /// <param name="reference"> The expression used to extract the name of the parameter. </param>
        /// <param name="value"> The value to check. </param>
        public static void AgainstNull<TValue>(Expression<Func<TValue>> reference, TValue value)
        {
            if (value == null)
            {
                AgainstNull<TValue, ArgumentNullException>(reference, default(TValue));
            }
        }

        /// <summary>
        ///     Ensures the given <paramref name="value" /> is not null, otherwise throws <see cref="ArgumentNullException" />.
        /// </summary>
        /// <typeparam name="TValue"> The type of the value to be validated. </typeparam>
        /// <typeparam name="TException"> The type of the exception to throw </typeparam>
        /// <param name="reference"> The expression used to extract the name of the parameter. </param>
        /// <param name="value"> The value to check. </param>
        public static void AgainstNull<TValue, TException>(Expression<Func<TValue>> reference, TValue value)
        {
            if (value == null)
            {
                throw CreateException(typeof(TException), GetParameterName(reference),
                    Resources.Guard_ArgumentCanNotBeNull);
            }
        }

        /// <summary>
        ///     Ensures the given <paramref name="value" /> is not null, otherwise throws <see cref="ArgumentNullException" />.
        /// </summary>
        /// <typeparam name="TValue"> The type of the value to be validated. </typeparam>
        /// <param name="reference"> The expression used to extract the name of the parameter. </param>
        /// <param name="value"> The value to check. </param>
        /// <param name="messageOrFormat"> Message to display if the value is null. </param>
        /// <param name="formatArgs"> Optional arguments to format the <paramref name="messageOrFormat" />. </param>
        public static void AgainstNull<TValue>(Expression<Func<TValue>> reference, TValue value, string messageOrFormat,
            params object[] formatArgs)
        {
            if (value == null)
            {
                if (messageOrFormat.HasValue())
                {
                    if ((formatArgs != null) && (formatArgs.Length != 0))
                    {
                        throw new ArgumentNullException(GetParameterName(reference),
                            messageOrFormat.Fmt(formatArgs));
                    }
                    throw new ArgumentNullException(GetParameterName(reference), messageOrFormat);
                }
                throw new ArgumentNullException(GetParameterName(reference), Resources.Guard_ArgumentCanNotBeNull);
            }
        }

        /// <summary>
        ///     Ensures the given <paramref name="value" /> is not null, otherwise throws <see cref="ArgumentNullException" />.
        /// </summary>
        /// <typeparam name="TValue"> The type of the value to be validated. </typeparam>
        /// <typeparam name="TException"> The type of the exception to throw </typeparam>
        /// <param name="reference"> The expression used to extract the name of the parameter. </param>
        /// <param name="value"> The value to check. </param>
        /// <param name="messageOrFormat"> Message to display if the value is null. </param>
        /// <param name="formatArgs"> Optional arguments to format the <paramref name="messageOrFormat" />. </param>
        public static void AgainstNull<TValue, TException>(Expression<Func<TValue>> reference, TValue value,
            string messageOrFormat,
            params object[] formatArgs)
        {
            if (value == null)
            {
                if (messageOrFormat.HasValue())
                {
                    if ((formatArgs != null) && (formatArgs.Length != 0))
                    {
                        throw CreateException(typeof(TException), GetParameterName(reference),
                            messageOrFormat.Fmt(formatArgs));
                    }
                    throw CreateException(typeof(TException), GetParameterName(reference), messageOrFormat);
                }
                throw CreateException(typeof(TException), GetParameterName(reference),
                    Resources.Guard_ArgumentCanNotBeNull);
            }
        }

        /// <summary>
        ///     Ensures the given string <paramref name="value" /> is not null or empty. Throws <see cref="TException" />
        ///     in both cases.
        /// </summary>
        /// <typeparam name="TException"> The type of the exception to throw </typeparam>
        /// <param name="reference"> The expression used to extract the name of the parameter. </param>
        /// <param name="value"> The value to check. </param>
        public static void AgainstNullOrEmpty<TException>(Expression<Func<string>> reference, string value)
        {
            AgainstNull<string, TException>(reference, value);
            if (value.Length == 0)
            {
                throw CreateException(typeof(TException), GetParameterName(reference),
                    Resources.Guard_ArgumentCanNotBeEmpty);
            }
        }

        /// <summary>
        ///     Ensures the given string <paramref name="value" /> is not null or empty. Throws
        ///     <see cref="ArgumentNullException" />
        ///     in the first case, or <see cref="ArgumentException" /> in the latter.
        /// </summary>
        /// <param name="reference"> The expression used to extract the name of the parameter. </param>
        /// <param name="value"> The value to check. </param>
        public static void AgainstNullOrEmpty(Expression<Func<string>> reference, string value)
        {
            AgainstNull(reference, value);
            if (value.Length == 0)
            {
                AgainstNullOrEmpty<ArgumentOutOfRangeException>(reference, value);
            }
        }

        /// <summary>
        ///     Ensures the given string <paramref name="value" /> is not null or empty. Throws
        ///     <see cref="ArgumentNullException" />
        ///     in the first case, or <see cref="ArgumentException" /> in the latter.
        /// </summary>
        /// <param name="reference"> The expression used to extract the name of the parameter. </param>
        /// <param name="value"> The value to check. </param>
        /// <param name="messageOrFormat"> Message to display if the value is null. </param>
        /// <param name="formatArgs"> Optional arguments to format the <paramref name="messageOrFormat" />. </param>
        public static void AgainstNullOrEmpty(Expression<Func<string>> reference, string value, string messageOrFormat,
            params object[] formatArgs)
        {
            AgainstNull(reference, value, messageOrFormat, formatArgs);
            if (value.Length == 0)
            {
                if (messageOrFormat.HasValue())
                {
                    if ((formatArgs != null) && (formatArgs.Length != 0))
                    {
                        throw new ArgumentOutOfRangeException(GetParameterName(reference),
                            messageOrFormat.Fmt(formatArgs));
                    }
                    throw new ArgumentOutOfRangeException(GetParameterName(reference), messageOrFormat);
                }
                throw new ArgumentOutOfRangeException(GetParameterName(reference),
                    Resources.Guard_ArgumentCanNotBeEmpty);
            }
        }

        /// <summary>
        ///     Ensures the given string <paramref name="value" /> is not null or empty. Throws
        ///     <see cref="ArgumentNullException" />
        ///     in the first case, or <see cref="ArgumentException" /> in the latter.
        /// </summary>
        /// <typeparam name="TException"> The type of the exception to throw </typeparam>
        /// <param name="reference"> The expression used to extract the name of the parameter. </param>
        /// <param name="value"> The value to check. </param>
        /// <param name="messageOrFormat"> Message to display if the value is null. </param>
        /// <param name="formatArgs"> Optional arguments to format the <paramref name="messageOrFormat" />. </param>
        public static void AgainstNullOrEmpty<TException>(Expression<Func<string>> reference, string value,
            string messageOrFormat,
            params object[] formatArgs)
        {
            AgainstNull(reference, value, messageOrFormat, formatArgs);
            if (value.Length == 0)
            {
                if (messageOrFormat.HasValue())
                {
                    if ((formatArgs != null) && (formatArgs.Length != 0))
                    {
                        throw CreateException(typeof(TException), GetParameterName(reference),
                            messageOrFormat.Fmt(formatArgs));
                    }
                    throw CreateException(typeof(TException), GetParameterName(reference), messageOrFormat);
                }
                throw CreateException(typeof(TException), GetParameterName(reference),
                    Resources.Guard_ArgumentCanNotBeEmpty);
            }
        }

        private static string GetParameterName<T>(Expression<T> reference)
        {
            return ((MemberExpression) reference.Body).Member.Name;
        }

        private static Exception CreateException(Type exceptionType, string parameterName, string message)
        {
            if (exceptionType.GetTypeInfo().IsAssignableFrom(typeof(ArgumentException).GetTypeInfo()))
            {
                return Activator.CreateInstance(exceptionType, parameterName, message) as ArgumentException;
            }

            return Activator.CreateInstance(exceptionType, message) as Exception;
        }
    }
}