using ServiceStack.FluentValidation;
using ServiceStack.Webhooks.Properties;

namespace ServiceStack.Webhooks.ServiceModel
{
    public class UpdateSubscriptionValidator : AbstractValidator<UpdateSubscription>
    {
        public UpdateSubscriptionValidator()
        {
            RuleFor(dto => dto.Id).IsEntityId()
                .WithMessage(Resources.GetSubscriptionValidator_InvalidId);
            When(dto => dto.Url.HasValue(), () =>
            {
                RuleFor(dto => dto.Url).NotEmpty()
                    .WithMessage(Resources.SubscriptionConfigValidator_InvalidUrl);
                RuleFor(dto => dto.Url).IsUrl()
                    .WithMessage(Resources.SubscriptionConfigValidator_InvalidUrl);
            });
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
}