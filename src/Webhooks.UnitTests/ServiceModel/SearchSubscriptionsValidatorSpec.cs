using NUnit.Framework;
using ServiceStack.FluentValidation;
using ServiceStack.Webhooks.Properties;
using ServiceStack.Webhooks.ServiceModel;

namespace ServiceStack.Webhooks.UnitTests.ServiceModel
{
    public class SearchSubscriptionsValidatorSpec
    {
        [TestFixture]
        public class GivenADto
        {
            private SearchSubscriptions dto;
            private SearchSubscriptionsValidator validator;

            [SetUp]
            public void Initialize()
            {
                dto = new SearchSubscriptions
                {
                    EventName = "aneventname"
                };
                validator = new SearchSubscriptionsValidator();
            }

            [Test, Category("Unit")]
            public void WhenAllPropertiesValid_ThenSucceeds()
            {
                validator.ValidateAndThrow(dto);
            }

            [Test, Category("Unit")]
            public void WhenEventNameIsNull_ThenThrows()
            {
                dto.EventName = null;

                validator.Validate(dto);

                Assert.That(() => validator.ValidateAndThrow(dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.SearchSubscriptionsValidator_InvalidEventName));
            }

            [Test, Category("Unit")]
            public void WhenEventNameIsInvalid_ThenThrows()
            {
                dto.EventName = "^";

                validator.Validate(dto);

                Assert.That(() => validator.ValidateAndThrow(dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.SearchSubscriptionsValidator_InvalidEventName));
            }
        }
    }
}