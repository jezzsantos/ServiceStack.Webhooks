using NUnit.Framework;
using ServiceStack.FluentValidation;
using ServiceStack.Webhooks.Properties;
using ServiceStack.Webhooks.ServiceModel;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.UnitTests.ServiceModel
{
    public class SubscriptionConfigValidatorSpec
    {
        [TestFixture]
        public class GivenADto
        {
            private SubscriptionConfig _dto;
            private SubscriptionConfigValidator _validator;

            [SetUp]
            public void Initialize()
            {
                _dto = new SubscriptionConfig
                {
                    Url = "http://localhost"
                };
                _validator = new SubscriptionConfigValidator();
            }

            [Test, Category("Unit")]
            public void WhenAllPropertiesValid_ThenSucceeds()
            {
                _validator.ValidateAndThrow(_dto);
            }

            [Test, Category("Unit")]
            public void WhenUrlIsNull_ThenThrows()
            {
                _dto.Url = null;

                _validator.Validate(_dto);

                Assert.That(() => _validator.ValidateAndThrow(_dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.SubscriptionConfigValidator_InvalidUrl));
            }

            [Test, Category("Unit")]
            public void WhenContentTypeIsNull_ThenSucceeds()
            {
                _dto.ContentType = null;

                _validator.ValidateAndThrow(_dto);
            }

            [Test, Category("Unit")]
            public void WhenContentTypeIsJson_ThenSucceeds()
            {
                _dto.ContentType = MimeTypes.Json;

                _validator.Validate(_dto);

                _validator.ValidateAndThrow(_dto);
            }

            [Test, Category("Unit")]
            public void WhenContentTypeIsNotJson_ThenThrows()
            {
                _dto.ContentType = "notjson";

                _validator.Validate(_dto);

                Assert.That(() => _validator.ValidateAndThrow(_dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.SubscriptionConfigValidator_UnsupportedContentType));
            }

            [Test, Category("Unit")]
            public void WhenSecretIsNull_ThenSucceeds()
            {
                _dto.Secret = null;

                _validator.ValidateAndThrow(_dto);
            }

            [Test, Category("Unit")]
            public void WhenSecretIsBase64_ThenSucceeds()
            {
                _dto.Secret = new string('A', 1000);

                _validator.ValidateAndThrow(_dto);
            }

            [Test, Category("Unit")]
            public void WhenSecretIsInvalid_ThenThrows()
            {
                _dto.Secret = new string('A', 1000 + 1);

                Assert.That(() => _validator.ValidateAndThrow(_dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.SubscriptionConfigValidator_InvalidSecret));
            }
        }
    }
}