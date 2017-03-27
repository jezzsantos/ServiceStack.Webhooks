using System.Text.RegularExpressions;
using ServiceStack.FluentValidation.Validators;
using ServiceStack.Webhooks.Properties;

namespace ServiceStack.Webhooks.ServiceModel
{
    public interface IEntityIdValidator : IPropertyValidator
    {
    }

    /// <summary>
    ///     A validator for a entity's identifier property value.
    /// </summary>
    public class EntityIdValidator : PropertyValidator, IEntityIdValidator
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="EntityIdValidator" /> class.
        /// </summary>
        public EntityIdValidator()
            : base(Resources.EntityIdValidator_ErrorMessage)
        {
        }

        /// <summary>
        ///     Whether the property value of the context is valid
        /// </summary>
        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null)
            {
                return false;
            }

            var propertyValue = context.PropertyValue.ToString();

            return Regex.IsMatch(propertyValue, DataFormats.EntityIdentifier.Expression);
        }
    }
}