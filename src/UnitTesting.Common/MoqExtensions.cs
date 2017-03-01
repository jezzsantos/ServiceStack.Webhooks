using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Moq.Language;
using Moq.Language.Flow;

namespace ServiceStack.Webhooks.UnitTesting
{
    /// <summary>
    ///     Extensions to the <see cref="Mock" /> class.
    /// </summary>
    public static class MoqExtensions
    {
        /// <summary>
        ///     Setup an expectation to return a sequence of values
        /// </summary>
        /// <remarks>
        ///     Credit to: http://haacked.com/archive/2009/09/29/moq-sequences.aspx/
        /// </remarks>
        public static void ReturnsInOrder<T, TResult>(this ISetup<T, TResult> setup,
            params TResult[] results) where T : class
        {
            setup.Returns(new Queue<TResult>(results).Dequeue);
        }

        /// <summary>
        ///     Setup an expectation to return a sequence of values
        /// </summary>
        /// <remarks>
        ///     Credit to: http://haacked.com/archive/2010/11/24/moq-sequences-revisited.aspx/
        /// </remarks>
        public static void ReturnsInOrder<T, TResult>(this ISetup<T, TResult> setup,
            params object[] results) where T : class
        {
            var queue = new Queue(results);
            setup.Returns(() =>
            {
                var result = queue.Dequeue();
                if (result is Exception)
                {
                    throw result as Exception;
                }
                return (TResult) result;
            });
        }

        /// <summary>
        ///     Throws the specified exception for the specified <see cref="Task" />
        /// </summary>
        public static IReturnsResult<TMock> ThrowsAsync<TMock>(this IReturns<TMock, Task> mock, Exception exception) where TMock : class
        {
            var completionSource = new TaskCompletionSource<bool>();
            completionSource.SetException(exception);
            return mock.Returns(completionSource.Task);
        }
    }
}