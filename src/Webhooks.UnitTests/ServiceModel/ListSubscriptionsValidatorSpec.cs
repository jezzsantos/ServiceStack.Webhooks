using NUnit.Framework;
using ServiceStack.FluentValidation;
using ServiceStack.Webhooks.ServiceModel;

namespace ServiceStack.Webhooks.UnitTests.ServiceModel
{
    public class ListSubscriptionsValidatorSpec
    {
        [TestFixture]
        public class GivenADto
        {
            private ListSubscriptions dto;
            private ListSubscriptionsValidator validator;

            [SetUp]
            public void Initialize()
            {
                dto = new ListSubscriptions();
                validator = new ListSubscriptionsValidator();
            }

            [Test, Category("Unit")]
            public void WhenAllPropertiesValid_ThenSucceeds()
            {
                validator.ValidateAndThrow(dto);
            }
        }
    }
}