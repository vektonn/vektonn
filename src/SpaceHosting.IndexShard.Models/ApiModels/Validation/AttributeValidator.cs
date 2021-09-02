using System;
using System.Collections.Generic;
using FluentValidation;

namespace SpaceHosting.IndexShard.Models.ApiModels.Validation
{
    public class AttributeValidator : AbstractValidator<AttributeDto>
    {
        public AttributeValidator(Dictionary<string, AttributeValueTypeCode> knownAttributes, string errorMessagePrefix)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(attribute => attribute)
                .Must(attribute => !string.IsNullOrWhiteSpace(attribute.Key))
                .WithMessage($"{errorMessagePrefix} attribute key is null or whitespace")
                .Must(attribute => knownAttributes.ContainsKey(attribute.Key))
                .WithMessage(attribute => $"{errorMessagePrefix} attribute key is unknown: '{attribute.Key}'")
                .Must(attribute => AttributeValueIsValid(attribute.Value, knownAttributes[attribute.Key]))
                .WithMessage(attribute => $"{errorMessagePrefix} attribute '{attribute.Key}' has invalid value: '{attribute.Value}'");
        }

        private static bool AttributeValueIsValid(AttributeValue attributeValue, AttributeValueTypeCode attributeValueType)
        {
            return HasExactlyOneFieldSet(attributeValue) && attributeValueType switch
            {
                AttributeValueTypeCode.String => attributeValue.String != null,
                AttributeValueTypeCode.Guid => attributeValue.Guid != null,
                AttributeValueTypeCode.Bool => attributeValue.Bool != null,
                AttributeValueTypeCode.Int64 => attributeValue.Int64 != null,
                AttributeValueTypeCode.DateTime => attributeValue.DateTime != null,
                _ => throw new InvalidOperationException($"Invalid {nameof(attributeValueType)}: {attributeValueType}")
            };
        }

        private static bool HasExactlyOneFieldSet(AttributeValue attributeValue)
        {
            var fieldsSet = 0;

            if (attributeValue.String != null)
                fieldsSet++;
            if (attributeValue.Guid != null)
                fieldsSet++;
            if (attributeValue.Bool != null)
                fieldsSet++;
            if (attributeValue.Int64 != null)
                fieldsSet++;
            if (attributeValue.DateTime != null)
                fieldsSet++;

            return fieldsSet == 1;
        }
    }
}
