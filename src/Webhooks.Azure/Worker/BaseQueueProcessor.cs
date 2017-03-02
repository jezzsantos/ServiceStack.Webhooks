using System;
using System.Linq;
using System.Threading;
using ServiceStack.Logging;
using ServiceStack.Model;
using ServiceStack.Webhooks.Azure.Queue;

namespace ServiceStack.Webhooks.Azure.Worker
{
    /// <summary>
    ///     Defines a base class for continuous polling queue processes, which poll for pending messages,
    ///     then wait for an interval, then poll again, until cancelled.
    ///     Messages that fail processing are placed on an 'Unhandled' queue.
    /// </summary>
    internal abstract class BaseQueueProcessor<TMessage> : BasicContinuousProcessor where TMessage : class, IHasStringId
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(BaseQueueProcessor<TMessage>));

        public virtual IAzureQueueStorage<TMessage> TargetQueue { get; set; }

        public virtual IAzureQueueStorage<IUnhandledMessage> UnhandledQueue { get; set; }

        public abstract int IntervalSeconds { get; set; }

        public override Action GetAction(CancellationToken cancellation)
        {
            return () =>
            {
                var nextMessages = TargetQueue.RemoveMessages(100);
                if ((nextMessages != null) && nextMessages.Any())
                {
                    logger.Info(@"Processing {0} messages from queue {1}".Fmt(nextMessages.Count, TargetQueue.QueueName));

                    nextMessages.ForEach(msg =>
                    {
                        var handled = TryProcessMessage(msg);
                        if (!handled)
                        {
                            PlaceMessageOnUnhandled(msg);
                        }
                    });
                }
            };
        }

        private bool TryProcessMessage(TMessage msg)
        {
            try
            {
                logger.Info(@"Message {0} removed from queue".Fmt(msg.Id));
                return ProcessMessage(msg);
            }
            catch (Exception)
            {
                logger.Info(@"Message {0} failed processing".Fmt(msg.Id));
                return false;
            }
        }

        private void PlaceMessageOnUnhandled(TMessage msg)
        {
            try
            {
                logger.Info(@"Message {0} placed on unhandled queue".Fmt(msg.Id));
                UnhandledQueue.Enqueue(CreateUnhandledMessage(msg));
            }
            catch (Exception)
            {
                logger.Error(@"Message {0} failed placement on unhandled queue".Fmt(msg.Id));
            }
        }

        public override TimeSpan GetInterval()
        {
            return TimeSpan.FromSeconds(IntervalSeconds);
        }

        public abstract bool ProcessMessage(TMessage message);

        private static IUnhandledMessage CreateUnhandledMessage(TMessage message)
        {
            var unHandled = new UnhandledMessage
            {
                Id = message.Id,
                MessageType = typeof(TMessage).Name,
                Content = message.ToJson()
            };

            return unHandled;
        }
    }
}