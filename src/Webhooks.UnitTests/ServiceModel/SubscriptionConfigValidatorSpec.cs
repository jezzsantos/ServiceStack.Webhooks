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
            private SubscriptionConfig dto;
            private SubscriptionConfigValidator validator;

            [SetUp]
            public void Initialize()
            {
                dto = new SubscriptionConfig
                {
                    Url = "http://localhost"
                };
                validator = new SubscriptionConfigValidator();
            }

            [Test, Category("Unit")]
            public void WhenAllPropertiesValid_ThenSucceeds()
            {
                validator.ValidateAndThrow(dto);
            }

            [Test, Category("Unit")]
            public void WhenUrlIsNull_ThenThrows()
            {
                dto.Url = null;

                validator.Validate(dto);

                Assert.That(() => validator.ValidateAndThrow(dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.SubscriptionConfigValidator_InvalidUrl));
            }

            [Test, Category("Unit")]
            public void WhenContentTypeIsNull_ThenSucceeds()
            {
                dto.ContentType = null;

                validator.ValidateAndThrow(dto);
            }

            [Test, Category("Unit")]
            public void WhenContentTypeIsJson_ThenSucceeds()
            {
                dto.ContentType = MimeTypes.Json;

                validator.Validate(dto);

                validator.ValidateAndThrow(dto);
            }

            [Test, Category("Unit")]
            public void WhenContentTypeIsNotJson_ThenThrows()
            {
                dto.ContentType = "notjson";

                validator.Validate(dto);

                Assert.That(() => validator.ValidateAndThrow(dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.SubscriptionConfigValidator_UnsupportedContentType));
            }

            [Test, Category("Unit")]
            public void WhenSecretIsNull_ThenSucceeds()
            {
                dto.Secret = null;

                validator.ValidateAndThrow(dto);
            }

            [Test, Category("Unit")]
            public void WhenSecretIsBase64_ThenSucceeds()
            {
                dto.Secret = new string('A', 1000);

                validator.ValidateAndThrow(dto);
            }

            [Test, Category("Unit")]
            public void WhenSecretIsInvalid_ThenThrows()
            {
                dto.Secret = new string('A', 1000 + 1);

                Assert.That(() => validator.ValidateAndThrow(dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.SubscriptionConfigValidator_InvalidSecret));
            }
        }
    }
}