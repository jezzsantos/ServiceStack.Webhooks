using System;
using ServiceStack.FluentValidation.Validators;
using ServiceStack.Webhooks.Properties;

namespace ServiceStack.Webhooks.ServiceModel
{
    internal interface IUrlValidator : IPropertyValidator
    {
    }

    /// <summary>
    ///     A validator for a URL property value.
    /// </summary>
    internal class UrlValidator : PropertyValidator, IUrlValidator
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="UrlValidator" /> class.
        /// </summary>
        public UrlValidator()
            : base(Resources.UrlValidator_ErrorMessage)
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

            return Uri.IsWellFormedUriString(propertyValue, UriKind.Absolute);
        }
    }
}