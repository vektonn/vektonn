using System.Collections.Generic;
using System.Linq;

namespace Vektonn.SharedImpl.Contracts
{
    public record IndexAlgorithm(string Type, Dictionary<string, string>? Params = null)
    {
        public override string ToString()
        {
            if (Params == null || !Params.Any())
                return Type;

            return $"{Type}({string.Join(", ", Params.Select(t => $"{t.Key}={t.Value}"))})";
        }
    }
}
