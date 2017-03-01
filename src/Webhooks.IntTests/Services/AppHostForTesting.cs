using System.Linq;
using Funq;
using ServiceStack.Auth;
using ServiceStack.Logging;
using ServiceStack.Validation;

namespace ServiceStack.Webhooks.IntTests.Services
{
    public class AppHostForTesting : AppSelfHostBase
    {
        public AppHostForTesting() : base("AppHostForTesting", typeof(AppHostForTesting).Assembly)
        {
        }

        public override void Configure(Container container)
        {
            Licensing.LicenseServiceStackRuntime();
            LogManager.LogFactory = new ConsoleLogFactory();
            RegisterAuthentication(container);
            Plugins.Add(new ValidationFeature());
            Plugins.Add(new WebhookFeature());
        }

        private void RegisterAuthentication(Container container)
        {
            Plugins.Add(new AuthFeature(() => new AuthUserSession(), new IAuthProvider[]
            {
                new BasicAuthProvider(),
                new CredentialsAuthProvider()
            }));
            Plugins.Add(new RegistrationFeature());
            container.Register<IUserAuthRepository>(new InMemoryAuthRepository());
        }

        public void LoginUser(JsonServiceClient client)
        {
            var username = "ausername";
            var password = "apassword";
            var userRepo = Resolve<IAuthRepository>();
            if (userRepo.GetUserAuthByUserName(username) == null)
            {
                var webhookFeature = GetPlugin<WebhookFeature>();
                var roles = webhookFeature.SubscriptionAccessRoles.SafeSplit(WebhookFeature.RoleDelimiters).ToList();
                string hash;
                string salt;
                new SaltedHash().GetHashAndSaltString(password, out hash, out salt);

                userRepo.CreateUserAuth(new UserAuth
                {
                    UserName = username,
                    Roles = roles,
                    PasswordHash = hash,
                    Salt = salt
                }, password);
            }

            client.Post(new Authenticate
            {
                UserName = username,
                Password = password,
                provider = CredentialsAuthProvider.Name
            });
        }
    }
}