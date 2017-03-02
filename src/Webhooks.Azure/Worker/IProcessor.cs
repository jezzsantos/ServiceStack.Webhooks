using System.Threading;

namespace ServiceStack.Webhooks.Azure.Worker
{
    /// <summary>
    ///     Defines a processor that does some work
    /// </summary>
    public interface IProcessor
    {
        /// <summary>
        ///     Runs the process
        /// </summary>
        void Run(CancellationToken cancellationToken);
    }
}