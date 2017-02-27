using NUnit.Framework;
using ServiceStack.FluentValidation;
using ServiceStack.Webhooks.Properties;
using ServiceStack.Webhooks.ServiceModel;

namespace ServiceStack.Webhooks.UnitTests.ServiceModel
{
    public class GetSubscriptionValidatorSpec
    {
        [TestFixture]
        public class GivenADto
        {
            private GetSubscription dto;
            private GetSubscriptionValidator validator;

            [SetUp]
            public void Initialize()
            {
                dto = new GetSubscription
                {
                    Id = DataFormats.CreateEntityIdentifier()
                };
                validator = new GetSubscriptionValidator();
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

                Assert.That(() => validator.ValidateAndThrow(dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.GetSubscriptionValidator_InvalidId));
            }
        }
    }
}