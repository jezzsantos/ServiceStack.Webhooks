using System.Collections.Generic;
using NUnit.Framework;
using ServiceStack.FluentValidation;
using ServiceStack.Webhooks.Properties;
using ServiceStack.Webhooks.ServiceModel;

namespace ServiceStack.Webhooks.UnitTests.ServiceModel
{
    public class SubscriptionEventsValidatorSpec
    {
        [TestFixture]
        public class GivenADto
        {
            private List<string> _dto;
            private SubscriptionEventsValidator _validator;

            [SetUp]
            public void Initialize()
            {
                _dto = new List<string> {"anevent"};
                _validator = new SubscriptionEventsValidator();
            }

            [Test, Category("Unit")]
            public void WhenAllPropertiesValid_ThenSucceeds()
            {
                _validator.ValidateAndThrow(_dto);
            }

            [Test, Category("Unit")]
            public void WhenNoEvents_ThenThrows()
            {
                _dto = new List<string>();

                _validator.Validate(_dto);

                Assert.That(() => _validator.ValidateAndThrow(_dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.SubscriptionEventsValidator_NoName));
            }

            [Test, Category("Unit")]
            public void WhenAnEventNameIsEmpty_ThenThrows()
            {
                _dto = new List<string> {""};

                Assert.That(() => _validator.ValidateAndThrow(_dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.SubscriptionEventsValidator_EmptyName));
            }

            [Test, Category("Unit")]
            public void WhenAnEventNameIsInvalid_ThenThrows()
            {
                _dto = new List<string> {"^"};

                Assert.That(() => _validator.ValidateAndThrow(_dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.SubscriptionEventsValidator_InvalidName));
            }
        }
    }
}