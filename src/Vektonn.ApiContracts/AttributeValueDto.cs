using System;

namespace Vektonn.ApiContracts
{
    public record AttributeValueDto(
        string? String = null,
        Guid? Guid = null,
        bool? Bool = null,
        long? Int64 = null,
        DateTime? DateTime = null)
    {
        public override string ToString()
        {
            return String
                   ?? Guid?.ToString("D")
                   ?? Bool?.ToString()
                   ?? Int64?.ToString()
                   ?? DateTime?.ToString("O")
                   ?? string.Empty;
        }
    }
}
