using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Funq;
using Microsoft.WindowsAzure.ServiceRuntime;
using ServiceStack.Logging;

namespace ServiceStack.Webhooks.Azure.Worker
{
    /// <summary>
    ///     Provides a base class for Azure role instances that enables us to host more than one
    ///     <see cref="WorkerEntryPoint" /> in a single 'worker role'.
    /// </summary>
    public abstract class AzureWorkerRoleEntryPoint : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly Container container;
        private readonly ILog logger = LogManager.GetLogger(typeof(AzureWorkerRoleEntryPoint));
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        protected bool Stopped;

        protected AzureWorkerRoleEntryPoint()
        {
            container = new Container();
        }

        protected abstract IEnumerable<WorkerEntryPoint> Workers { get; }

        public override bool OnStart()
        {
            Stopped = false;
            var result = base.OnStart();

            Configure(container);

            return result;
        }

        // ReSharper disable once ParameterHidesMember
        public virtual void Configure(Container container)
        {
        }

        public override void Run()
        {
            var workerName = GetType().FullName;
            logger.Info("{0}.Running".Fmt(workerName));

            try
            {
                // Run and wait synchronously
                RunAsync(cancellationTokenSource.Token).Wait();
            }
            finally
            {
                runCompleteEvent.Set();
            }
        }

        public override void OnStop()
        {
            var workerName = GetType().FullName;
            logger.Info("{0}.OnStop Stopping".Fmt(workerName));

            cancellationTokenSource.Cancel();
            runCompleteEvent.WaitOne();

            base.OnStop();
            Stopped = true;

            logger.Info("{0}.OnStop Stopped".Fmt(workerName));
        }

        private async Task RunAsync(CancellationToken cancellation)
        {
            var workerName = GetType().FullName;
            logger.Info("{0}.RunAsync Running all workers".Fmt(workerName));

            if ((Workers == null) || !Workers.Any())
            {
                logger.Info("{0}.RunAsync No Workers to run".Fmt(workerName));
            }
            else
            {
                var options = new ParallelOptions
                {
                    CancellationToken = cancellation
                };
                try
                {
                    var exceptions = new ConcurrentQueue<Exception>();
                    Parallel.ForEach(Workers, options, worker =>
                    {
                        logger.Info("{0}.RunAsync Running worker '{1}'".Fmt(workerName, worker.GetType().FullName));

                        try
                        {
                            worker.Run(cancellation);
                        }
                        catch (Exception ex)
                        {
                            exceptions.Enqueue(ex);

                            // Continue all other tasks
                        }
                    });

                    if (exceptions.Any())
                    {
                        exceptions.Each(taskException =>
                        {
                            if (!(taskException is OperationCanceledException))
                            {
                                logger.Fatal("{0}.RunAsync Worker crashed".Fmt(workerName), taskException);
                            }
                        });
                    }
                }
                catch (OperationCanceledException)
                {
                    logger.Info("{0}.RunAsync Worker ended because of cancellation".Fmt(workerName));
                }
                catch (Exception ex)
                {
                    logger.Fatal("{0}.RunAsync Worker crashed".Fmt(workerName), ex);
                    throw;
                }
            }

            await Task.Delay(1, cancellation);
        }
    }
}