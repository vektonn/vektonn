namespace Vektonn.ApiContracts
{
    public record AttributeDto(string Key, AttributeValueDto Value)
    {
        public override string ToString()
        {
            return $"{Key}:{Value}";
        }
    }
}
