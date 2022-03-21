using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vektonn.SharedImpl.BinarySerialization;
using Vektonn.SharedImpl.Contracts;

namespace Vektonn.Tests.SharedImpl.BinarySerialization
{
    public class InputDataPointSerializerTests
    {
        [Test]
        public void Serialize_Deserialize()
        {
            var inputDataPoint = new InputDataPoint(
                Attributes: new Dictionary<string, AttributeValue>
                {
                    ["str"] = new AttributeValue(String: "la-la-la"),
                    ["guid"] = new AttributeValue(Guid: Guid.Parse("39b146ec-9803-4b7e-aa35-f3550ac0e5ed")),
                    ["bool"] = new AttributeValue(Bool: true),
                    ["int64"] = new AttributeValue(Int64: long.MaxValue),
                    ["float64"] = new AttributeValue(Float64: double.MinValue),
                    ["date_time"] = new AttributeValue(DateTime: new DateTime(2021, 07, 31, 13, 52, 59, DateTimeKind.Utc)),
                },
                VectorCoordinates: new[] {1.2, 3.4, -5.6},
                VectorCoordinateIndices: new[] {7, 13, 27}
            );

            var bytes = InputDataPointSerializer.SerializeDataPoint(inputDataPoint);
            bytes.Length.Should().Be(158);

            InputDataPointSerializer
                .DeserializeDataPoint(bytes)
                .Should()
                .BeEquivalentTo(inputDataPoint, options => options.WithStrictOrdering());

            Convert.ToBase64String(bytes)
                .Should()
                .Be("ChEKA3N0chIKCghsYS1sYS1sYQoaCgRndWlkEhISEDmxRuyYA0t+qjXzVQrA5e0KCgoEYm9vbBICGAEKEwoFaW50NjQSCiD//////////38KFAoHZmxvYXQ2NBIJKf///////+//ChUKCWRhdGVfdGltZRIIMgYIu6uViAYRMzMzMzMz8z8RMzMzMzMzC0ARZmZmZmZmFsAYBxgNGBs=");
        }
    }
}
