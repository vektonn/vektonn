using System;

namespace SpaceHosting.Service
{
    public static class EnvironmentVariables
    {
        public static string Get(string varName)
        {
            return TryGet(varName) ?? throw new InvalidOperationException($"{varName} env variable is not set");
        }

        public static string? TryGet(string varName)
        {
            return Environment.GetEnvironmentVariable(varName);
        }

        public static T Get<T>(string varName, Func<string, T> parse)
            where T: notnull
        {
            return parse(Get(varName));
        }

        public static T TryGet<T>(string varName, Func<string, T> parse, T defaultValue)
            where T : notnull
        {
            var valueStr = TryGet(varName);
            return valueStr == null ? defaultValue : parse(valueStr);
        }
    }
}
