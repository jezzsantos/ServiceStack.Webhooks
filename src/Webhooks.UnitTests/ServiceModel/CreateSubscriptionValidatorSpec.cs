using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ServiceStack.FluentValidation;
using ServiceStack.FluentValidation.Results;
using ServiceStack.Webhooks.Properties;
using ServiceStack.Webhooks.ServiceModel;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.UnitTests.ServiceModel
{
    public class CreateSubscriptionValidatorSpec
    {
        [TestFixture]
        public class GivenADto
        {
            private CreateSubscription _dto;
            private Mock<ISubscriptionEventsValidator> _eventsValidator;
            private Mock<ISubscriptionConfigValidator> _subscriptionConfigValidator;
            private CreateSubscriptionValidator _validator;

            [SetUp]
            public void Initialize()
            {
                _dto = new CreateSubscription
                {
                    Name = "aname",
                    Events = new List<string>(),
                    Config = new SubscriptionConfig()
                };
                _subscriptionConfigValidator = new Mock<ISubscriptionConfigValidator>();
                _subscriptionConfigValidator.Setup(val => val.Validate(It.IsAny<ValidationContext>()))
                    .Returns(new ValidationResult());
                _eventsValidator = new Mock<ISubscriptionEventsValidator>();
                _eventsValidator.Setup(val => val.Validate(It.IsAny<ValidationContext>()))
                    .Returns(new ValidationResult());
                _validator = new CreateSubscriptionValidator(_eventsValidator.Object, _subscriptionConfigValidator.Object);
            }

            [Test, Category("Unit")]
            public void WhenAllPropertiesValid_ThenSucceeds()
            {
                _validator.ValidateAndThrow(_dto);
            }

            [Test, Category("Unit")]
            public void WhenNameIsNull_ThenThrows()
            {
                _dto.Name = null;

                _validator.Validate(_dto);

                Assert.That(() => _validator.ValidateAndThrow(_dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.CreateSubscriptionValidator_InvalidName));
            }

            [Test, Category("Unit")]
            public void WhenNameIsInvalid_ThenThrows()
            {
                _dto.Name = "^";

                Assert.That(() => _validator.ValidateAndThrow(_dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.CreateSubscriptionValidator_InvalidName));
            }

            [Test, Category("Unit")]
            public void WhenEventsIsNull_ThenThrows()
            {
                _dto.Events = null;

                Assert.That(() => _validator.ValidateAndThrow(_dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.CreateSubscriptionValidator_InvalidEvents));
            }

            [Test, Category("Unit")]
            public void WhenConfigIsNull_ThenThrows()
            {
                _dto.Config = null;

                Assert.That(() => _validator.ValidateAndThrow(_dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.CreateSubscriptionValidator_InvalidConfig));
            }
        }
    }
}