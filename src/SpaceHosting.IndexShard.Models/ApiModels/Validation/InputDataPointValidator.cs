using System.Linq;
using FluentValidation;

namespace SpaceHosting.IndexShard.Models.ApiModels.Validation
{
    public class InputDataPointValidator : AbstractValidator<InputDataPointDto>
    {
        public InputDataPointValidator(DataSourceMeta dataSourceMeta)
        {
            RuleFor(p => p.Attributes)
                .Must(attributes => attributes.Select(x => x.Key).Distinct().Count() == attributes.Length)
                .WithMessage("Attribute keys must be unique")
                .Must(attributes => attributes.Select(x => x.Key).ToHashSet().SetEquals(dataSourceMeta.AttributeValueTypes.Keys))
                .WithMessage("All attributes defined for data source (and no other ones) must be specified");

            var attributeValidator = new AttributeValidator(
                knownAttributes: dataSourceMeta.AttributeValueTypes,
                errorMessagePrefix: "InputDataPoint");

            var attributeValueShardingValidator = new AttributeValueShardingValidator(
                dataSourceMeta.DataSourceShardingMeta.ShardersByAttributeKey,
                errorMessagePrefix: "InputDataPoint");

            RuleForEach(p => p.Attributes)
                .SetValidator(attributeValidator)
                .SetValidator(attributeValueShardingValidator);

            RuleFor(p => p)
                .Must(p => p.IsDeleted ^ (p.Vector != null))
                .WithMessage("IsDeleted is inconsistent with Vector");

            var vectorValidator = dataSourceMeta.VectorsAreSparse
                ? new SparseVectorValidator(dataSourceMeta.VectorDimension)
                : (AbstractValidator<VectorDto>)new DenseVectorValidator(dataSourceMeta.VectorDimension);

            When(
                p => p.Vector != null,
                () => RuleFor(p => p.Vector!)
                    .SetValidator(vectorValidator)
            );
        }
    }
}
