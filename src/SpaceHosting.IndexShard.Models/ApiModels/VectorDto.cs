namespace SpaceHosting.IndexShard.Models.ApiModels
{
    public abstract record VectorDto(bool IsSparse, double[] Coordinates);
}
