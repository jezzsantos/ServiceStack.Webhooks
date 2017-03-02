using System;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Logging;

namespace ServiceStack.Webhooks.Azure.Worker
{
    /// <summary>
    ///     Defines a base class for continuous looping processes, which run then wait for an interval, then run again, until
    ///     cancelled
    /// </summary>
    public abstract class BasicContinuousProcessor : IProcessor
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(BasicContinuousProcessor));

        public virtual void Run(CancellationToken cancellation)
        {
            while (!cancellation.IsCancellationRequested)
            {
                var processorName = GetType().FullName;
                logger.Info(@"Running Processor: '{0}'".Fmt(processorName));
                GetAction(cancellation)();

                Task.Delay(GetInterval(), cancellation).Wait(cancellation);
            }
        }

        public abstract Action GetAction(CancellationToken cancellation);

        public abstract TimeSpan GetInterval();
    }
}