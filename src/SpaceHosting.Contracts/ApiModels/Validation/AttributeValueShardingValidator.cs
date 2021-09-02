using System.Collections.Generic;
using FluentValidation;
using SpaceHosting.Contracts.Sharding.DataSource;

namespace SpaceHosting.Contracts.ApiModels.Validation
{
    public class AttributeValueShardingValidator : AbstractValidator<AttributeDto>
    {
        public AttributeValueShardingValidator(
            Dictionary<string, IDataSourceAttributeValueSharder> shardersByAttributeKey,
            string errorMessagePrefix)
        {
            When(
                attribute => shardersByAttributeKey.ContainsKey(attribute.Key),
                () => RuleFor(attribute => attribute)
                    .Must(attribute => shardersByAttributeKey[attribute.Key].IsValueAcceptable(attribute.Value))
                    .WithMessage(attribute => $"{errorMessagePrefix} attribute '{attribute.Key}' has unacceptable value '{attribute.Value}' for corresponding sharder")
            );
        }
    }
}
