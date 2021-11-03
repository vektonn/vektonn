using System.Collections.Generic;
using FluentValidation;
using Vektonn.ApiContracts;
using Vektonn.SharedImpl.Contracts.Sharding.DataSource;

namespace Vektonn.SharedImpl.ApiContracts.Validation
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
                    .Must(attribute => shardersByAttributeKey[attribute.Key].IsValueAcceptable(attribute.Value.ToAttributeValue()))
                    .WithMessage(attribute => $"{errorMessagePrefix} attribute '{attribute.Key}' has unacceptable value '{attribute.Value}' for corresponding sharder")
            );
        }
    }
}
