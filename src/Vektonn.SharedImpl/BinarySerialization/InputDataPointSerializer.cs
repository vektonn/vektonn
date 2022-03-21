using System;
using System.IO;
using ProtoBuf;
using Vektonn.SharedImpl.Contracts;

namespace Vektonn.SharedImpl.BinarySerialization
{
    public static class InputDataPointSerializer
    {
        static InputDataPointSerializer()
        {
            // note (andrew, 30.09.2021): make sure protobuf serializer for AttributeValue is initialized prior to InputDataPointSerializer first usage
            AttributeValueSerializer.Touch();
        }

        public static byte[] SerializeDataPoint(InputDataPoint value)
        {
            using var ms = new MemoryStream();
            Serializer.Serialize(ms, value);
            return ms.ToArray();
        }

        public static InputDataPoint DeserializeDataPoint(byte[] bytes)
        {
            return Serializer.Deserialize<InputDataPoint>((ReadOnlySpan<byte>)bytes);
        }
    }
}
