using FluentValidation;

namespace Vektonn.Contracts.ApiModels.Validation
{
    public class DenseVectorValidator : AbstractValidator<VectorDto>
    {
        public DenseVectorValidator(int vectorDimension)
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(v => v)
                .Must(v => !v.IsSparse)
                .WithMessage("Vector must be dense");

            RuleFor(v => ((DenseVectorDto)v).Coordinates)
                .Must(coordinates => coordinates.Length == vectorDimension)
                .WithMessage($"Vector coordinates must have dimension: {vectorDimension}");
        }
    }
}
