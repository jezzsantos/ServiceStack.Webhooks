using System;
using System.Net;
using ServiceStack.Auth;
using ServiceStack.Configuration;
using ServiceStack.Text;
using ServiceStack.Web;
using ServiceStack.Webhooks.Security;
using ServiceStack.Webhooks.Subscribers.Properties;

namespace ServiceStack.Webhooks.Subscribers.Security
{
    public class HmacAuthProvider : AuthProvider, IAuthWithRequest
    {
        public const string Name = "hmac";
        public const string Realm = "/auth/hmac";
        public const string SecretSettingsName = "hmac.Secret";

        public HmacAuthProvider()
            : base(null, Realm, Name)
        {
            RequireSecureConnection = true;
        }

        public HmacAuthProvider(IAppSettings settings)
            : this()
        {
            Guard.AgainstNull(() => settings, settings);

            Secret = settings.GetString(SecretSettingsName);

            RequireSecureConnection = true;
        }

        public string Secret { get; set; }

        public Func<IRequest, string, string> OnGetSecret { get; set; }

        public bool RequireSecureConnection { get; set; }

        public void PreAuthenticate(IRequest req, IResponse res)
        {
            var signature = req.Headers[WebhookEventConstants.SecretSignatureHeaderName];
            if (signature.HasValue())
            {
                if (RequireSecureConnection && !req.IsSecureConnection)
                {
                    throw HttpError.Forbidden(Resources.HmacAuthProvider_NotHttps);
                }

                var eventName = req.Headers[WebhookEventConstants.EventNameHeaderName];
                if (OnGetSecret != null)
                {
                    Secret = OnGetSecret(req, eventName);
                }
                if (!Secret.HasValue())
                {
                    throw HttpError.Unauthorized(Resources.HmacAuthProvider_IncorrectlyConfigured);
                }

                var isValidSecret = req.VerifySignature(signature, Secret);
                if (!isValidSecret)
                {
                    throw new HttpError(HttpStatusCode.Unauthorized);
                }

                var requestId = req.Headers[WebhookEventConstants.RequestIdHeaderName];
                var userId = requestId.HasValue() ? requestId : Guid.NewGuid().ToString("N");
                var username = req.GetUrlHostName();

                var sessionId = SessionExtensions.CreateRandomSessionId();
                var session = SessionFeature.CreateNewSession(req, sessionId);
                session.UserAuthId = userId;
                session.UserAuthName = username;
                session.UserName = username;
                session.IsAuthenticated = true;
                session.CreatedAt = SystemTime.UtcNow;

                HostContext.AppHost.OnSessionFilter(session, sessionId);

                req.Items[Keywords.Session] = session;
            }
        }

        public override bool IsAuthorized(IAuthSession session, IAuthTokens tokens, Authenticate request = null)
        {
            return session != null
                   && session.IsAuthenticated
                   && !session.UserAuthId.IsNullOrEmpty();
        }

        public override object Authenticate(IServiceBase authService, IAuthSession session, Authenticate request)
        {
            throw new NotImplementedException(@"HmacAuthProvider.Authenticate should never be called.");
        }
    }
}