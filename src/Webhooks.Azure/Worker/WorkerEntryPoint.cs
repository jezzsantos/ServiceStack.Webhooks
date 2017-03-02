using System.Threading;

namespace ServiceStack.Webhooks.Azure.Worker
{
    public abstract class WorkerEntryPoint
    {
        public virtual void Run(CancellationToken cancellationToken)
        {
        }
    }

    public abstract class WorkerEntryPoint<TProcessor> : WorkerEntryPoint where TProcessor : IProcessor
    {
        public TProcessor Processor { get; set; }

        public override void Run(CancellationToken cancellationToken)
        {
            Processor.Run(cancellationToken);
        }
    }
}