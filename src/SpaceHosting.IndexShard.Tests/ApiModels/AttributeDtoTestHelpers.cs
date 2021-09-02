using System;
using SpaceHosting.IndexShard.Models;
using SpaceHosting.IndexShard.Models.ApiModels;

namespace SpaceHosting.IndexShard.Tests.ApiModels
{
    public static class AttributeDtoTestHelpers
    {
        public static AttributeDto Attribute(string key, bool value)
        {
            return new AttributeDto(key, new AttributeValue(Bool: value));
        }

        public static AttributeDto Attribute(string key, long value)
        {
            return new AttributeDto(key, new AttributeValue(Int64: value));
        }

        public static AttributeDto Attribute(string key, string? value)
        {
            return new AttributeDto(key, new AttributeValue(String: value));
        }

        public static AttributeDto Attribute(string key, DateTime value)
        {
            return new AttributeDto(key, new AttributeValue(DateTime: value));
        }

        public static AttributeDto Attribute(string key, Guid value)
        {
            return new AttributeDto(key, new AttributeValue(Guid: value));
        }

        public static (string Key, AttributeValueTypeCode Type)[] KnownAttributes(params (string Key, AttributeValueTypeCode Type)[] knownAttributes)
        {
            return knownAttributes;
        }
    }
}
