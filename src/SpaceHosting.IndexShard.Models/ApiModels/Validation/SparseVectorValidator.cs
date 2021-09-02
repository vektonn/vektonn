using System.Linq;
using FluentValidation;

namespace SpaceHosting.IndexShard.Models.ApiModels.Validation
{
    public class SparseVectorValidator : AbstractValidator<VectorDto>
    {
        public SparseVectorValidator(int vectorDimension)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(v => v)
                .Must(v => v.IsSparse)
                .WithMessage("Vector must be sparse");

            RuleFor(v => ((SparseVectorDto)v).Coordinates)
                .Must(coordinates => coordinates.Length > 0)
                .WithMessage("Vector must have coordinates");

            RuleFor(v => v)
                .Must(v => v.Coordinates.Length == ((SparseVectorDto)v).CoordinateIndices.Distinct().Count())
                .WithMessage("Each vector coordinate value must have unique corresponding index");

            RuleForEach(v => ((SparseVectorDto)v).CoordinateIndices)
                .Must(i => i >= 0 && i < vectorDimension)
                .WithMessage($"Vector coordinates must have dimension: {vectorDimension}");
        }
    }
}
