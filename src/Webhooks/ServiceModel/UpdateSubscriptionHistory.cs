using ServiceStack.FluentValidation;
using ServiceStack.Webhooks.Properties;
using ServiceStack.Webhooks.ServiceModel.Types;

namespace ServiceStack.Webhooks.ServiceModel
{
    public interface ISubscriptionDeliveryResultValidator : IValidator<SubscriptionDeliveryResult>
    {
    }

    public class SubscriptionDeliveryResultValidator : AbstractValidator<SubscriptionDeliveryResult>, ISubscriptionDeliveryResultValidator
    {
        public SubscriptionDeliveryResultValidator()
        {
            RuleFor(dto => dto.Id).IsEntityId()
                .WithMessage(Resources.SubscriptionDeliveryResultValidator_InvalidId);
            RuleFor(dto => dto.SubscriptionId).IsEntityId()
                .WithMessage(Resources.SubscriptionDeliveryResultValidator_InvalidSubscriptionId);
        }
    }

    public class UpdateSubscriptionHistoryValidator : AbstractValidator<UpdateSubscriptionHistory>
    {
        public UpdateSubscriptionHistoryValidator(ISubscriptionDeliveryResultValidator deliveryResultValidator)
        {
            RuleFor(dto => dto.Results)
                .SetCollectionValidator(deliveryResultValidator);
        }
    }
}