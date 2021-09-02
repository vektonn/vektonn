namespace SpaceHosting.Contracts.ApiModels
{
    public abstract record VectorDto(bool IsSparse, double[] Coordinates);
}
