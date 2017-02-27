using System;
using System.Text;
using NUnit.Framework;
using ServiceStack.FluentValidation;
using ServiceStack.Webhooks.Properties;
using ServiceStack.Webhooks.ServiceModel;

namespace ServiceStack.Webhooks.UnitTests.ServiceModel
{
    public class UpdateSubscriptionValidatorSpec
    {
        [TestFixture]
        public class GivenADto
        {
            private UpdateSubscription dto;
            private UpdateSubscriptionValidator validator;

            [SetUp]
            public void Initialize()
            {
                dto = new UpdateSubscription
                {
                    Id = DataFormats.CreateEntityIdentifier(),
                    Url = "http://localhost",
                    ContentType = MimeTypes.Json,
                    Secret = Convert.ToBase64String(Encoding.Default.GetBytes("asecret"))
                };
                validator = new UpdateSubscriptionValidator();
            }

            [Test, Category("Unit")]
            public void WhenAllPropertiesValid_ThenSucceeds()
            {
                validator.ValidateAndThrow(dto);
            }

            [Test, Category("Unit")]
            public void WhenIdIsNull_ThenThrows()
            {
                dto.Id = null;

                Assert.That(() => validator.ValidateAndThrow(dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.UpdateSubscriptionValidator_InvalidId));
            }

            [Test, Category("Unit")]
            public void WhenUrlIsNull_ThenSucceeds()
            {
                dto.Url = null;

                validator.ValidateAndThrow(dto);
            }

            [Test, Category("Unit")]
            public void WhenUrlIsNotAUrl_ThenThrows()
            {
                dto.Url = "aurl";

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