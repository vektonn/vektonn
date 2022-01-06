using System.Linq;
using FluentValidation;
using Vektonn.ApiContracts;
using Vektonn.Index;
using Vektonn.SharedImpl.Contracts;

namespace Vektonn.SharedImpl.ApiContracts.Validation
{
    public class SearchQueryValidator : AbstractValidator<SearchQueryDto>
    {
        public SearchQueryValidator(IndexMeta indexMeta)
        {
            RuleFor(q => q)
                .Must(q => q.K > 0)
                .WithMessage("K must be positive")
                .Must(q => q.QueryVectors.Length > 0)
                .WithMessage("At least one query vector is required");

            var splitFilterValidator = new SplitFilterValidator(
                indexMeta.SplitAttributes.ToDictionary(
                    key => key,
                    key => indexMeta.DataSourceMeta.AttributeValueTypes[key]));

            When(
                q => q.SplitFilter != null,
                () => RuleFor(q => q.SplitFilter!)
                    .SetValidator(splitFilterValidator)
            );

            var vectorValidator = AlgorithmTraits.VectorsAreSparse(indexMeta.IndexAlgorithm.Type)
                ? new SparseVectorValidator(indexMeta.VectorDimension)
                : (AbstractValidator<VectorDto>)new DenseVectorValidator(indexMeta.VectorDimension);

            RuleForEach(q => q.QueryVectors)
                .Cascade(CascadeMode.Stop)
                .SetValidator(vectorValidator);
        }
    }
}
