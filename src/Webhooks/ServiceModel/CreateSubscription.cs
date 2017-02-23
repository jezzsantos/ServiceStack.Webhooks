using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ServiceStack.FluentValidation;
using ServiceStack.Webhooks.Properties;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.ServiceModel
{
    public interface ISubscriptionConfigValidator : IValidator<SubscriptionConfig>
    {
    }

    public class SubscriptionConfigValidator : AbstractValidator<SubscriptionConfig>, ISubscriptionConfigValidator
    {
        public SubscriptionConfigValidator()
        {
            RuleFor(dto => dto.Url).NotEmpty()
                .WithMessage(Resources.SubscriptionConfigValidator_InvalidUrl);
            RuleFor(dto => dto.Url).IsUrl()
                .WithMessage(Resources.SubscriptionConfigValidator_InvalidUrl);
            When(dto => dto.ContentType.HasValue(), () =>
            {
                RuleFor(dto => dto.ContentType).Must(dto => dto.EqualsIgnoreCase(MimeTypes.Json))
                    .WithMessage(Resources.SubscriptionConfigValidator_UnsupportedContentType);
            });
            When(dto => dto.Secret.HasValue(), () =>
            {
                RuleFor(dto => dto.Secret).Matches(DataFormats.Subscriptions.Secret.Expression)
                    .WithMessage(Resources.SubscriptionConfigValidator_InvalidSecret);
            });
        }
    }

    public interface ISubscriptionEventsValidator : IValidator<List<string>>
    {
    }

    public class SubscriptionEventsValidator : AbstractValidator<List<string>>, ISubscriptionEventsValidator
    {
        public SubscriptionEventsValidator()
        {
            RuleFor(dto => dto).NotNull().WithName("Events")
                .WithMessage(Resources.SubscriptionEventsValidator_NoName);
            RuleFor(dto => dto).Must(names => names.Any()).WithName("Events")
                .WithMessage(Resources.SubscriptionEventsValidator_NoName);
            RuleFor(dto => dto).Must(name => name.TrueForAll(x => x.HasValue())).WithName("Events")
                .WithMessage(Resources.SubscriptionEventsValidator_EmptyName);
            RuleFor(dto => dto).Must(name => name.TrueForAll(x => Regex.IsMatch(x, DataFormats.Subscriptions.Event.Expression))).WithName("Events")
                .WithMessage(Resources.SubscriptionEventsValidator_InvalidName);
        }
    }

    public class CreateSubscriptionValidator : AbstractValidator<CreateSubscription>
    {
        public CreateSubscriptionValidator(ISubscriptionEventsValidator eventsValidator, ISubscriptionConfigValidator subscriptionConfigValidator)
        {
            RuleFor(dto => dto.Config).NotNull()
                .WithMessage(Resources.CreateSubscriptionValidator_InvalidConfig);
            RuleFor(dto => dto.Config)
                .SetValidator(subscriptionConfigValidator);
            RuleFor(dto => dto.Events).NotNull()
                .WithMessage(Resources.CreateSubscriptionValidator_InvalidEvents);
            RuleFor(dto => dto.Events)
                .SetValidator(eventsValidator);
            RuleFor(dto => dto.Name).NotEmpty()
                .WithMessage(Resources.CreateSubscriptionValidator_InvalidName);
            RuleFor(dto => dto.Name).Matches(DataFormats.Subscriptions.Name.Expression)
                .WithMessage(Resources.CreateSubscriptionValidator_InvalidName);
        }
    }
}