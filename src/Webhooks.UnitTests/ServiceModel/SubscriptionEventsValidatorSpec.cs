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
            private List<string> dto;
            private SubscriptionEventsValidator validator;

            [SetUp]
            public void Initialize()
            {
                dto = new List<string> {"anevent"};
                validator = new SubscriptionEventsValidator();
            }

            [Test, Category("Unit")]
            public void WhenAllPropertiesValid_ThenSucceeds()
            {
                validator.ValidateAndThrow(dto);
            }

            [Test, Category("Unit")]
            public void WhenNoEvents_ThenThrows()
            {
                dto = new List<string>();

                Assert.That(() => validator.ValidateAndThrow(dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.SubscriptionEventsValidator_NoName));
            }

            [Test, Category("Unit")]
            public void WhenAnEventNameIsEmpty_ThenThrows()
            {
                dto = new List<string> {""};

                Assert.That(() => validator.ValidateAndThrow(dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.SubscriptionEventsValidator_EmptyName));
            }

            [Test, Category("Unit")]
            public void WhenAnEventNameIsInvalid_ThenThrows()
            {
                dto = new List<string> {"^"};

                Assert.That(() => validator.ValidateAndThrow(dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.SubscriptionEventsValidator_InvalidName));
            }
        }
    }
}