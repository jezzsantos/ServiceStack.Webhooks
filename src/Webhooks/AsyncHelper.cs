using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceStack.Webhooks
{
    /// <summary>
    /// Credit to: https://cpratt.co/async-tips-tricks/
    /// </summary>
    public static class AsyncHelper
    {
        public static void RunSync(Func<Task> func)
        {
            var taskFactory = new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);
            taskFactory
                .StartNew(func)
                .GetAwaiter()
                .GetResult();
        }
    }
}