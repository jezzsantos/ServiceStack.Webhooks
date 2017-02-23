using ServiceStack.Web;

namespace ServiceStack.Webhooks.ServiceInterface
{
    public static class CurrentCallerExtensions
    {
        public static void InjectRequestIfRequires(this ICurrentCaller caller, IRequest request)
        {
            Guard.AgainstNull(() => caller, caller);
            Guard.AgainstNull(() => request, request);

            var requiresRequest = caller as IRequiresRequest;
            if (requiresRequest != null)
            {
                requiresRequest.Request = request;
            }
        }
    }
}