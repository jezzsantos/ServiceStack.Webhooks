using ServiceStack.FluentValidation;
using ServiceStack.Webhooks.Properties;

namespace ServiceStack.Webhooks.ServiceModel
{
    internal class DeleteSubscriptionValidator : AbstractValidator<DeleteSubscription>
    {
        public DeleteSubscriptionValidator()
        {
            RuleFor(dto => dto.Id).IsEntityId()
                .WithMessage(Resources.GetSubscriptionValidator_InvalidId);
        }
    }
}