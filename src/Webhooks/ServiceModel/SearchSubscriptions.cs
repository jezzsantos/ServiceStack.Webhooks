using ServiceStack.FluentValidation;
using ServiceStack.Webhooks.Properties;

namespace ServiceStack.Webhooks.ServiceModel
{
    public class SearchSubscriptionsValidator : AbstractValidator<SearchSubscriptions>
    {
        public SearchSubscriptionsValidator()
        {
            RuleFor(dto => dto.EventName).NotEmpty()
                .WithMessage(Resources.SearchSubscriptionsValidator_InvalidEventName);
            RuleFor(dto => dto.EventName).Matches(DataFormats.Subscriptions.Event.Expression)
                .WithMessage(Resources.SearchSubscriptionsValidator_InvalidEventName);
        }
    }
}