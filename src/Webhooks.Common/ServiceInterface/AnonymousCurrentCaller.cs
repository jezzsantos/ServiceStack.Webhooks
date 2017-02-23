using System.Collections.Generic;
using System.Linq;

namespace ServiceStack.Webhooks.ServiceInterface
{
    public class AnonymousCurrentCaller : ICurrentCaller
    {
        public string AccessToken
        {
            get { return null; }
        }

        public string Username
        {
            get { return null; }
        }

        public string UserId
        {
            get { return null; }
        }

        public IEnumerable<string> Roles
        {
            get { return Enumerable.Empty<string>(); }
        }

        public bool IsAuthenticated
        {
            get { return false; }
        }

        public bool IsInRole(string role)
        {
            return false;
        }
    }
}