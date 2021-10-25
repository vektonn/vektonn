using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Vektonn.ApiContracts;
using Vektonn.SharedImpl.Contracts;

namespace Vektonn.SharedImpl.ApiContracts.Validation
{
    public class SplitFilterValidator : AbstractValidator<AttributeDto[]>
    {
        public SplitFilterValidator(Dictionary<string, AttributeValueTypeCode> splitAttributes)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(splitFilter => splitFilter)
                .Must(splitFilter => !splitFilter.Select(x => x.Key).Except(splitAttributes.Keys).Any())
                .WithMessage($"SplitFilter attribute keys must be in SplitAttributes: {{{string.Join(", ", splitAttributes.Keys)}}}")
                .Must(splitFilter => splitFilter.Select(x => x.Key).Distinct().Count() == splitFilter.Length)
                .WithMessage("SplitFilter attribute keys must be unique");

            var attributeValidator = new AttributeValidator(
                knownAttributes: splitAttributes,
                errorMessagePrefix: "SplitFilter");

            RuleForEach(splitFilter => splitFilter)
                .SetValidator(attributeValidator);
        }
    }
}
