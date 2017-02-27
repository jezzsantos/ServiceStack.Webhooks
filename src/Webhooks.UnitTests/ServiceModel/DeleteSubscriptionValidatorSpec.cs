using NUnit.Framework;
using ServiceStack.FluentValidation;
using ServiceStack.Webhooks.Properties;
using ServiceStack.Webhooks.ServiceModel;

namespace ServiceStack.Webhooks.UnitTests.ServiceModel
{
    public class DeleteSubscriptionValidatorSpec
    {
        [TestFixture]
        public class GivenADto
        {
            private DeleteSubscription dto;
            private DeleteSubscriptionValidator validator;

            [SetUp]
            public void Initialize()
            {
                dto = new DeleteSubscription
                {
                    Id = DataFormats.CreateEntityIdentifier()
                };
                validator = new DeleteSubscriptionValidator();
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

                Assert.That(() => validator.ValidateAndThrow(dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.DeleteSubscriptionValidator_InvalidId));
            }
        }
    }
}