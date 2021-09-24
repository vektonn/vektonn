using System;
using System.Collections.Generic;
using System.Linq;
using Vektonn.Contracts;

namespace Vektonn.IndexShard
{
    internal class AttributesAccessor
    {
        private readonly string[] indexIdAttributes;
        private readonly string[] splitAttributes;
        private readonly string[] indexPayloadAttributes;

        public AttributesAccessor(IndexMeta indexMeta)
        {
            indexIdAttributes = indexMeta.IndexIdAttributes.OrderBy(x => x).ToArray();
            splitAttributes = indexMeta.SplitAttributes.OrderBy(x => x).ToArray();
            indexPayloadAttributes = indexMeta.IndexPayloadAttributes.OrderBy(x => x).ToArray();
        }

        public AttributeValue[] GetIndexId(Dictionary<string, AttributeValue> attributes)
        {
            if (!indexIdAttributes.Any())
                throw new InvalidOperationException($"{nameof(indexIdAttributes)} is empty");

            return GetValues(indexIdAttributes, attributes);
        }

        public AttributeValue[] GetSplitKey(Dictionary<string, AttributeValue> attributes)
        {
            var splitKey = GetPartialSplitKey(attributes);

            if (splitKey.Any(x => x == null))
                throw new InvalidOperationException($"{nameof(attributes)} does not contain all {nameof(splitAttributes)}");

            return splitKey!;
        }

        public AttributeValue?[] GetPartialSplitKey(Dictionary<string, AttributeValue>? attributes)
        {
            if (!splitAttributes.Any())
                throw new InvalidOperationException($"{nameof(splitAttributes)} is empty");

            attributes ??= new Dictionary<string, AttributeValue>();
            return splitAttributes.Select(attributes.GetValueOrDefault).ToArray();
        }

        public AttributeValue[]? TryGetPayload(Dictionary<string, AttributeValue> attributes)
        {
            if (!indexPayloadAttributes.Any())
                return null;

            return GetValues(indexPayloadAttributes, attributes);
        }

        public Dictionary<string, AttributeValue> GetAttributes(AttributeValue[] indexId, AttributeValue[]? splitKey, AttributeValue[]? payload)
        {
            var keys = indexIdAttributes.Concat(splitAttributes).Concat(indexPayloadAttributes).ToArray();
            var values = indexId.Concat(splitKey ?? Array.Empty<AttributeValue>()).Concat(payload ?? Array.Empty<AttributeValue>()).ToArray();

            if (keys.Length != values.Length)
                throw new InvalidOperationException($"keys.Length ({keys.Length}) != values.Length ({values.Length})");

            return keys.Zip(values).ToDictionary(t => t.First, t => t.Second);
        }

        private static AttributeValue[] GetValues(string[] keys, Dictionary<string, AttributeValue> attributes)
        {
            return keys.Select(key => attributes[key]).ToArray();
        }
    }
}
