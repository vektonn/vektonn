namespace SpaceHosting.IndexShard.Models.ApiModels
{
    public record AttributeDto(string Key, AttributeValue Value)
    {
        public override string ToString()
        {
            return $"{Key}:{Value}";
        }
    }
}
