using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.Logging;
using ServiceStack.Webhooks.Azure.Worker;

namespace ServiceStack.Webhooks.Azure.UnitTests.Worker
{
    public class AzureWorkerRoleEntryPointSpec
    {
        [TestFixture]
        public class GivenNoContext
        {
            private TestWorkerRole role;

            [SetUp]
            public void Initialize()
            {
                role = new TestWorkerRole();
            }

            [Test, Category("Unit")]
            public void WhenOnStart_ThenConfigured()
            {
                var result = role.OnStart();

                Assert.True(result);
                Assert.False(role.IsStoppedInternal);
            }
        }

        [TestFixture]
        public class GivenARunningRole
        {
            private TestWorkerRole role;

            [SetUp]
            public void Initialize()
            {
                role = new TestWorkerRole();
            }

            [Test, Category("Unit")]
            public void WhenOnStopAndNoWorkers_ThenIsStoppedImmediately()
            {
                StartWorkerRole();

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                role.OnStop();
                stopwatch.Stop();

                Assert.True(role.IsStoppedInternal);
                Assert.True(stopwatch.Elapsed.TotalSeconds < 1, "Not stopped within 1 second");
            }

            [Test, Category("Unit")]
            public void WhenOnStopAndShortRunningUnresponsiveWorker_ThenIsStoppedWhenDone()
            {
                role.WorkersInternal.Add(new UnresponsiveTestWorker());
                StartWorkerRole();

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                role.OnStop();
                stopwatch.Stop();

                Assert.True(role.IsStoppedInternal);
                Assert.True(stopwatch.Elapsed.TotalSeconds > 1, "Stopped too soon");
                Assert.True(stopwatch.Elapsed.TotalSeconds < 10, "Not stopped within 10 seconds");
            }

            [Test, Category("Unit")]
            public void WhenOnStopAndLongRunningResponsiveWorker_ThenIsStoppedImmediately()
            {
                role.WorkersInternal.Add(new ResponsiveTestWorker());
                StartWorkerRole();

                var stopwatch = new Stopwatch();
                stopwatch.Start();
                role.OnStop();
                stopwatch.Stop();

                Assert.True(role.IsStoppedInternal);
                Assert.True(stopwatch.Elapsed.TotalSeconds < 1, "Not stopped within 1 second");
            }

            [Test, Category("Unit")]
            public void WhenWorkerThrows_ThenCrashRecorded()
            {
                role.WorkersInternal.Add(new BuggyTestWorker());
                StartWorkerRole();

                // Wait till all workers have completed
                Task.Delay(TimeSpan.FromSeconds(5)).Wait();
            }

            private void StartWorkerRole()
            {
                role.OnStart();
                Task.Run(() => { role.Run(); });
                Task.Delay(500).Wait();
            }
        }
    }

    public class TestWorkerRole : AzureWorkerRoleEntryPoint
    {
        public TestWorkerRole()
        {
            WorkersInternal = new List<WorkerEntryPoint>();
        }

        public bool IsStoppedInternal
        {
            get { return Stopped; }
        }

        public List<WorkerEntryPoint> WorkersInternal { get; private set; }

        protected override IEnumerable<WorkerEntryPoint> Workers
        {
            get { return WorkersInternal; }
        }
    }

    public class UnresponsiveTestWorker : WorkerEntryPoint
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(UnresponsiveTestWorker));

        public override void Run(CancellationToken cancellation)
        {
            logger.Debug("UnresponsiveTestWorker.Pausing");
            Task.Delay(TimeSpan.FromSeconds(5)).Wait();
            logger.Debug("UnresponsiveTestWorker.Resuming");
        }
    }

    public class ResponsiveTestWorker : WorkerEntryPoint
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(ResponsiveTestWorker));

        public override void Run(CancellationToken cancellation)
        {
            logger.Debug("ResponsiveTestWorker.Pausing");
            Task.Delay(TimeSpan.FromSeconds(50), cancellation).Wait(cancellation);
            logger.Debug("ResponsiveTestWorker.Resuming");
        }
    }

    public class BuggyTestWorker : WorkerEntryPoint
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(BuggyTestWorker));

        public override void Run(CancellationToken cancellation)
        {
            logger.Debug("BuggyTestWorker.Running");
            Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            throw new InvalidOperationException("BuggyTestWorkerException");
        }
    }
}