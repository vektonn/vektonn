using Vektonn.ApiContracts;
using Vektonn.SharedImpl.Contracts;

namespace Vektonn.SharedImpl.ApiContracts
{
    public static class AttributeValueConversions
    {
        public static AttributeValue ToAttributeValue(this AttributeValueDto dto)
        {
            return new AttributeValue(
                dto.String,
                dto.Guid,
                dto.Bool,
                dto.Int64,
                dto.Float64,
                dto.DateTime);
        }

        public static AttributeValueDto ToAttributeValueDto(this AttributeValue attributeValue)
        {
            return new AttributeValueDto(
                attributeValue.String,
                attributeValue.Guid,
                attributeValue.Bool,
                attributeValue.Int64,
                attributeValue.Float64,
                attributeValue.DateTime);
        }
    }
}
