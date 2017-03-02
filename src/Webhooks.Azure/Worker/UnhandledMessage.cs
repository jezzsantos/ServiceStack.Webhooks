namespace ServiceStack.Webhooks.Azure.Worker
{
    internal class UnhandledMessage : IUnhandledMessage
    {
        /// <summary>
        ///     Gets or sets teh identifier of the message
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets the content of the message
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        ///     Gets or sets the type of the message
        /// </summary>
        public string MessageType { get; set; }
    }
}