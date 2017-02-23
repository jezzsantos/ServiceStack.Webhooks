using ServiceStack.FluentValidation;

namespace ServiceStack.Webhooks.ServiceModel
{
    /// <summary>
    ///     Extensions for validators
    /// </summary>
    public static class ValidatorExtensions
    {
        /// <summary>
        ///     Defines a validator that will fail if the property is not a valid URL.
        /// </summary>
        public static IRuleBuilderOptions<T, TProperty> IsUrl<T, TProperty>(
            this IRuleBuilder<T, TProperty> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new UrlValidator());
        }

        /// <summary>
        ///     Defines a validator that will fail if the property is not a valid ID of an entity.
        /// </summary>
        public static IRuleBuilderOptions<T, TProperty> IsEntityId<T, TProperty>(
            this IRuleBuilder<T, TProperty> ruleBuilder)
        {
            return ruleBuilder.SetValidator(new EntityIdValidator());
        }
    }
}