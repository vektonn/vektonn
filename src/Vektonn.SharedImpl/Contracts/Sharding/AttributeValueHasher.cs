using System;
using static Farmhash.Sharp.Farmhash;

namespace Vektonn.SharedImpl.Contracts.Sharding
{
    public class AttributeValueHasher : IAttributeValueHasher
    {
        public ulong ComputeHash(AttributeValue attributeValue)
        {
            if (attributeValue.String != null)
                return Hash64(attributeValue.String);

            if (attributeValue.Guid != null)
                return Hash64(attributeValue.Guid.Value.ToByteArray());

            if (attributeValue.Bool != null)
                return (ulong)(attributeValue.Bool.Value ? 1 : 0);

            if (attributeValue.Int64 != null)
                return Hash64(GetPortableBytes(attributeValue.Int64.Value));

            if (attributeValue.Float64 != null)
                return Hash64(GetPortableBytes(attributeValue.Float64.Value));

            if (attributeValue.DateTime != null)
                return Hash64(GetPortableBytes(attributeValue.DateTime.Value.Ticks));

            throw new InvalidOperationException($"Invalid AttributeValue: {attributeValue}");
        }

        private static byte[] GetPortableBytes(long value)
        {
            return GetPortableBytes(value, BitConverter.GetBytes);
        }

        private static byte[] GetPortableBytes(double value)
        {
            return GetPortableBytes(value, BitConverter.GetBytes);
        }

        private static byte[] GetPortableBytes<T>(T value, Func<T, byte[]> getBytes)
        {
            var bytes = getBytes(value);

            // note (andrew, 04.08.2021): true for x86-64
            if (BitConverter.IsLittleEndian)
                return bytes;

            Array.Reverse(bytes);
            return bytes;
        }
    }
}
