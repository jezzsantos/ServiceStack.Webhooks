using ServiceStack.Web;

namespace ServiceStack.Webhooks.ServiceInterface
{
    public static class RequestExtensions
    {
        /// <summary>
        ///     Returns an instance of the registered <see cref="ICurrentCaller" /> in the IOC container
        /// </summary>
        public static ICurrentCaller ToCaller(this IRequest request)
        {
            Guard.AgainstNull(() => request, request);

            var caller = request.TryResolve<ICurrentCaller>();
            if (caller == null)
            {
                return new AnonymousCurrentCaller();
            }

            var requiresrequest = caller as IRequiresRequest;
            if (requiresrequest != null)
            {
                caller.InjectRequestIfRequires(request);
            }

            return caller;
        }
    }
}