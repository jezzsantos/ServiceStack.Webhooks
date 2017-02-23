using System.Net;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace ServiceStack.Webhooks.UnitTesting
{
    public static class ThrowsWebServiceException
    {
        public static EqualConstraint WithStatusCode(HttpStatusCode status)
        {
            return Throws.InstanceOf<WebServiceException>().With.Property(@"StatusCode").EqualTo((int) status);
        }
    }
}