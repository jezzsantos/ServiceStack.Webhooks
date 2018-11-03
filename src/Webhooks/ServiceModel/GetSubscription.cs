using ServiceStack.FluentValidation;
using ServiceStack.Webhooks.Properties;

namespace ServiceStack.Webhooks.ServiceModel
{
    public class GetSubscriptionValidator : AbstractValidator<GetSubscription>
    {
        public GetSubscriptionValidator()
        {
            RuleFor(dto => dto.Id).IsEntityId()
                .WithMessage(Resources.GetSubscriptionValidator_InvalidId);
        }
    }
}