using System;
using Vektonn.ApiContracts;
using Vektonn.SharedImpl.Contracts;

namespace Vektonn.Tests.SharedImpl.ApiContracts
{
    public static class AttributeDtoTestHelpers
    {
        public static AttributeDto Attribute(string key, bool value)
        {
            return new AttributeDto(key, new AttributeValueDto(Bool: value));
        }

        public static AttributeDto Attribute(string key, long value)
        {
            return new AttributeDto(key, new AttributeValueDto(Int64: value));
        }

        public static AttributeDto Attribute(string key, double value)
        {
            return new AttributeDto(key, new AttributeValueDto(Float64: value));
        }

        public static AttributeDto Attribute(string key, string? value)
        {
            return new AttributeDto(key, new AttributeValueDto(String: value));
        }

        public static AttributeDto Attribute(string key, DateTime value)
        {
            return new AttributeDto(key, new AttributeValueDto(DateTime: value));
        }

        public static AttributeDto Attribute(string key, Guid value)
        {
            return new AttributeDto(key, new AttributeValueDto(Guid: value));
        }

        public static (string Key, AttributeValueTypeCode Type)[] KnownAttributes(params (string Key, AttributeValueTypeCode Type)[] knownAttributes)
        {
            return knownAttributes;
        }
    }
}
