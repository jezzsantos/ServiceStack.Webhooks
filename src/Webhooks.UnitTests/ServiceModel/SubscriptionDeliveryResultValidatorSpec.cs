using NUnit.Framework;
using ServiceStack.FluentValidation;
using ServiceStack.Webhooks.Properties;
using ServiceStack.Webhooks.ServiceModel;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.UnitTests.ServiceModel
{
    public class SubscriptionDeliveryResultValidatorSpec
    {
        [TestFixture]
        public class GivenADto
        {
            private SubscriptionDeliveryResult dto;
            private SubscriptionDeliveryResultValidator validator;

            [SetUp]
            public void Initialize()
            {
                dto = new SubscriptionDeliveryResult
                {
                    Id = DataFormats.CreateEntityIdentifier(),
                    SubscriptionId = DataFormats.CreateEntityIdentifier()
                };
                validator = new SubscriptionDeliveryResultValidator();
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

                validator.Validate(dto);

                Assert.That(() => validator.ValidateAndThrow(dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.SubscriptionDeliveryResultValidator_InvalidId));
            }

            [Test, Category("Unit")]
            public void WhenIdIsInvalid_ThenThrows()
            {
                dto.Id = "anid";

                validator.Validate(dto);

                Assert.That(() => validator.ValidateAndThrow(dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.SubscriptionDeliveryResultValidator_InvalidId));
            }

            [Test, Category("Unit")]
            public void WhenSubscriptionIdIsNull_ThenThrows()
            {
                dto.SubscriptionId = null;

                validator.Validate(dto);

                Assert.That(() => validator.ValidateAndThrow(dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.SubscriptionDeliveryResultValidator_InvalidSubscriptionId));
            }

            [Test, Category("Unit")]
            public void WhenSubscriptionIdIsInvalid_ThenThrows()
            {
                dto.SubscriptionId = "anid";

                validator.Validate(dto);

                Assert.That(() => validator.ValidateAndThrow(dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.SubscriptionDeliveryResultValidator_InvalidSubscriptionId));
            }
        }
    }
}