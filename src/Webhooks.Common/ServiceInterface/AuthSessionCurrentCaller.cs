using System.Collections.Generic;
using System.Linq;
using ServiceStack.Auth;
using ServiceStack.Web;

namespace ServiceStack.Webhooks.ServiceInterface
{
    public class AuthSessionCurrentCaller : ICurrentCaller, IRequiresRequest
    {
        private IRequest currentRequest;
        private IAuthSession currentSession;

        public string Username
        {
            get
            {
                var sess = GetSession();
                if (sess == null)
                {
                    return null;
                }

                return sess.UserName;
            }
        }

        public string UserId
        {
            get
            {
                var sess = GetSession();
                if (sess == null)
                {
                    return null;
                }

                return sess.UserAuthId;
            }
        }

        public IEnumerable<string> Roles
        {
            get
            {
                var sess = GetSession();
                if (sess == null)
                {
                    return Enumerable.Empty<string>();
                }

                return sess.Roles.Safe();
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                var sess = GetSession();
                if (sess == null)
                {
                    return false;
                }

                return sess.IsAuthenticated;
            }
        }

        public bool IsInRole(string role)
        {
            return Roles.Any(r => r.EqualsIgnoreCase(role));
        }

        public IRequest Request
        {
            get { return currentRequest; }
            set
            {
                currentRequest = value;
                currentSession = null;
            }
        }

        private IAuthSession GetSession()
        {
            if (currentRequest == null)
            {
                return null;
            }

            if (currentSession == null)
            {
                currentSession = currentRequest.GetSession();
            }

            return currentSession;
        }
    }
}