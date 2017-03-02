using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ServiceStack.Webhooks.Azure.Worker;

namespace ServiceStack.Webhooks.Azure.UnitTests.Worker
{
    public class BasicContinuousProcessorSpec
    {
        [TestFixture]
        public class GivenAContext
        {
            private TestBasicProcessor processor;
            private CancellationTokenSource tokenSource;

            [SetUp]
            public void Initialize()
            {
                tokenSource = new CancellationTokenSource();
                processor = new TestBasicProcessor
                {
                    TokenSource = tokenSource
                };
            }

            [Test, Category("Unit")]
            public void WhenRun_ThenRanContinuouslyUntilCancelled()
            {
                try
                {
                    processor.Run(tokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                }

                Assert.That(processor.RanCount, Is.EqualTo(10));
            }
        }
    }

    internal class TestBasicProcessor : BasicContinuousProcessor
    {
        public int RanCount { get; set; }

        public CancellationTokenSource TokenSource { get; set; }

        public override Action GetAction(CancellationToken cancellation)
        {
            return () =>
            {
                RanCount++;
                if (RanCount == 10)
                {
                    //Stop asyncronously
                    Task.Run(() => { TokenSource.Cancel(); });

                    Thread.Sleep(200);
                }
            };
        }

        public override TimeSpan GetInterval()
        {
            return TimeSpan.FromMilliseconds(50);
        }
    }
}