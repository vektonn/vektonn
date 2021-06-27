namespace SpaceHosting.ApiModels
{
    public abstract class VectorDto
    {
        public bool IsSparse { get; init; }
        public int Dimension { get; init; }
        public double[] Coordinates { get; init; } = null!;
    }
}
