using ServiceStack.Model;

namespace ServiceStack.Webhooks.Azure.Worker
{
    /// <summary>
    ///     Defines an unhandled message on a queue
    /// </summary>
    public interface IUnhandledMessage : IHasStringId
    {
        /// <summary>
        ///     Gets or sets the content of the message
        /// </summary>
        string Content { get; set; }

        /// <summary>
        ///     Gets or sets the type of the message
        /// </summary>
        string MessageType { get; set; }
    }
}