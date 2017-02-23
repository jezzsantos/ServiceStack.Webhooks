using System.Collections.Generic;

namespace ServiceStack.Webhooks.ServiceInterface
{
    /// <summary>
    ///     Defines the calling user
    /// </summary>
    public interface ICurrentCaller
    {
        /// <summary>
        ///     Gets the name of the user
        /// </summary>
        string Username { get; }

        /// <summary>
        ///     Gets the identifier of the user
        /// </summary>
        string UserId { get; }

        /// <summary>
        ///     Gets the roles of the user
        /// </summary>
        IEnumerable<string> Roles { get; }

        /// <summary>
        ///     Gets whether the user is authenticated
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        ///     Determines whether the user is in the specified role
        /// </summary>
        bool IsInRole(string role);
    }
}