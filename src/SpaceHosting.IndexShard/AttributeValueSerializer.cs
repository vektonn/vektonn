using System;
using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;
using SpaceHosting.Contracts;

namespace SpaceHosting.IndexShard
{
    internal static class AttributeValueSerializer
    {
        static AttributeValueSerializer()
        {
            var meta = RuntimeTypeModel.Default.Add<AttributeValue>(applyDefaultBehaviour: false, CompatibilityLevel.Level300);
            meta.UseConstructor = false;

            meta.Add(1, nameof(AttributeValue.String));
            meta.Add(2, nameof(AttributeValue.Guid));
            meta.Add(3, nameof(AttributeValue.Bool));
            meta.Add(4, nameof(AttributeValue.Int64));
            meta.Add(5, nameof(AttributeValue.DateTime));

            // note (andrew, 28.07.2021): enforce compact Guid encoding in 16 bytes (http://protobuf-net.github.io/protobuf-net/compatibilitylevel.html)
            meta[2].DataFormat = DataFormat.FixedSize;

            Serializer.PrepareSerializer<AttributeValue[]>();
        }

        public static byte[] Serialize(AttributeValue[] values)
        {
            using var ms = new MemoryStream();
            Serializer.Serialize(ms, values);
            return ms.ToArray();
        }

        public static AttributeValue[] Deserialize(byte[] bytes)
        {
            return Serializer.Deserialize<AttributeValue[]>((ReadOnlySpan<byte>)bytes);
        }
    }
}
