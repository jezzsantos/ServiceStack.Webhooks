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
            private CreateSubscription dto;
            private Mock<ISubscriptionEventsValidator> eventsValidator;
            private Mock<ISubscriptionConfigValidator> subscriptionConfigValidator;
            private CreateSubscriptionValidator validator;

            [SetUp]
            public void Initialize()
            {
                dto = new CreateSubscription
                {
                    Name = "aname",
                    Events = new List<string>(),
                    Config = new SubscriptionConfig()
                };
                subscriptionConfigValidator = new Mock<ISubscriptionConfigValidator>();
                subscriptionConfigValidator.Setup(val => val.Validate(It.IsAny<ValidationContext>()))
                    .Returns(new ValidationResult());
                eventsValidator = new Mock<ISubscriptionEventsValidator>();
                eventsValidator.Setup(val => val.Validate(It.IsAny<ValidationContext>()))
                    .Returns(new ValidationResult());
                validator = new CreateSubscriptionValidator(eventsValidator.Object, subscriptionConfigValidator.Object);
            }

            [Test, Category("Unit")]
            public void WhenAllPropertiesValid_ThenSucceeds()
            {
                validator.ValidateAndThrow(dto);
            }

            [Test, Category("Unit")]
            public void WhenNameIsNull_ThenThrows()
            {
                dto.Name = null;

                validator.Validate(dto);

                Assert.That(() => validator.ValidateAndThrow(dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.CreateSubscriptionValidator_InvalidName));
            }

            [Test, Category("Unit")]
            public void WhenNameIsInvalid_ThenThrows()
            {
                dto.Name = "^";

                Assert.That(() => validator.ValidateAndThrow(dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.CreateSubscriptionValidator_InvalidName));
            }

            [Test, Category("Unit")]
            public void WhenEventsIsNull_ThenThrows()
            {
                dto.Events = null;

                Assert.That(() => validator.ValidateAndThrow(dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.CreateSubscriptionValidator_InvalidEvents));
            }

            [Test, Category("Unit")]
            public void WhenConfigIsNull_ThenThrows()
            {
                dto.Config = null;

                Assert.That(() => validator.ValidateAndThrow(dto), Throws.TypeOf<ValidationException>().With.Message.Contain(Resources.CreateSubscriptionValidator_InvalidConfig));
            }
        }
    }
}