using System;
using System.Collections.Generic;
using System.Threading;
using Moq;
using NUnit.Framework;
using ServiceStack.Model;
using ServiceStack.Webhooks.Azure.Queue;
using ServiceStack.Webhooks.Azure.Worker;

namespace ServiceStack.Webhooks.Azure.UnitTests.Worker
{
    public class BaseQueueProcessorSpec
    {
        [TestFixture]
        public class GivenAContext
        {
            private TestMessage message;
            private TestQueueProcessor processor;
            private Mock<IAzureQueueStorage<TestMessage>> queueProvider;
            private Mock<IAzureQueueStorage<IUnhandledMessage>> unhandledQueueProvider;

            [SetUp]
            public void Initialize()
            {
                message = new TestMessage
                {
                    Id = "amessageid"
                };
                queueProvider = new Mock<IAzureQueueStorage<TestMessage>>();
                queueProvider.Setup(qp => qp.RemoveMessages(100))
                    .Returns(new List<TestMessage>
                    {
                        message
                    });
                unhandledQueueProvider = new Mock<IAzureQueueStorage<IUnhandledMessage>>();
                processor = new TestQueueProcessor
                {
                    TargetQueue = queueProvider.Object,
                    UnhandledQueue = unhandledQueueProvider.Object,
                    IntervalSeconds = 1
                };
            }

            [Test, Category("Unit")]
            public void WhenGetInterval_ThenReturnsInterval()
            {
                var result = processor.GetInterval();

                Assert.That(result, Is.EqualTo(TimeSpan.FromSeconds(1)));
            }

            [Test, Category("Unit")]
            public void WhenGetActionAndNoQueuedMessages_ThenNotProcessMessages()
            {
                processor.ProcessMessageResult = true;
                queueProvider.Setup(qp => qp.RemoveMessages(100))
                    .Returns(new List<TestMessage>());

                processor.GetAction(CancellationToken.None)();

                Assert.That(processor.MessageProcessed, Is.False);
                Assert.That(processor.ProcessMessageCount, Is.EqualTo(0));
                unhandledQueueProvider.Verify(uqp => uqp.Enqueue(It.IsAny<IUnhandledMessage>()), Times.Never);
            }

            [Test, Category("Unit")]
            public void WhenGetActionAndManyQueuedMessages_ThenProcessesAllMessages()
            {
                processor.ProcessMessageResult = true;
                queueProvider.Setup(qp => qp.RemoveMessages(100))
                    .Returns(new List<TestMessage>
                    {
                        new TestMessage(),
                        new TestMessage(),
                        new TestMessage()
                    });

                processor.GetAction(CancellationToken.None)();

                Assert.That(processor.MessageProcessed);
                Assert.That(processor.ProcessMessageCount, Is.EqualTo(3));
                unhandledQueueProvider.Verify(uqp => uqp.Enqueue(It.IsAny<IUnhandledMessage>()), Times.Never);
            }

            [Test, Category("Unit")]
            public void WhenGetActionAndMessageNotProcessed_ThenMessageQueuedOnUnhandledQueue()
            {
                processor.ProcessMessageResult = false;
                processor.GetAction(CancellationToken.None)();

                Assert.That(processor.MessageProcessed);
                Assert.That(processor.ProcessMessageCount, Is.EqualTo(1));
                unhandledQueueProvider.Verify(uqp => uqp.Enqueue(It.Is<IUnhandledMessage>(um =>
                        (um.Id == "amessageid")
                        && (um.MessageType == typeof(TestMessage).Name)
                        && um.Content.HasValue()
                )), Times.Once());
            }

            [Test, Category("Unit")]
            public void WhenGetActionAndMessageProcessed_ThenMessageProcessed()
            {
                processor.ProcessMessageResult = true;
                processor.GetAction(CancellationToken.None)();

                Assert.That(processor.MessageProcessed);
                Assert.That(processor.ProcessMessageCount, Is.EqualTo(1));
                unhandledQueueProvider.Verify(uqp => uqp.Enqueue(It.IsAny<IUnhandledMessage>()), Times.Never());
            }

            [Test, Category("Unit")]
            public void WhenGetActionAndProcessMessageThrows_ThenMessageQueuedOnUnhandledQueue()
            {
                processor.ProcessMessageThrows = true;
                processor.GetAction(CancellationToken.None)();

                Assert.That(processor.MessageProcessed, Is.False);
                Assert.That(processor.ProcessMessageCount, Is.EqualTo(1));
                unhandledQueueProvider.Verify(uqp => uqp.Enqueue(It.Is<IUnhandledMessage>(um =>
                        (um.Id == "amessageid")
                        && (um.MessageType == typeof(TestMessage).Name)
                        && um.Content.HasValue()
                )), Times.Once());
            }
        }
    }

    internal interface ITestMessage
    {
    }

    internal class TestMessage : ITestMessage, IHasStringId
    {
        public string Id { get; set; }
    }

    internal class TestQueueProcessor : BaseQueueProcessor<TestMessage>
    {
        public bool ProcessMessageThrows { get; set; }

        public bool ProcessMessageResult { get; set; }

        public int ProcessMessageCount { get; set; }

        public bool MessageProcessed { get; private set; }

        public override int IntervalSeconds { get; set; }

        public override bool ProcessMessage(TestMessage message)
        {
            ProcessMessageCount++;
            if (ProcessMessageThrows)
            {
                throw new Exception("Forced thrown exception");
            }
            MessageProcessed = true;
            return ProcessMessageResult;
        }
    }
}