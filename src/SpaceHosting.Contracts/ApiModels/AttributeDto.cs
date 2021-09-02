namespace SpaceHosting.Contracts.ApiModels
{
    public record AttributeDto(string Key, AttributeValue Value)
    {
        public override string ToString()
        {
            return $"{Key}:{Value}";
        }
    }
}
