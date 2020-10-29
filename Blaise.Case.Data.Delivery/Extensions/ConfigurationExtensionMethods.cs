using System.Configuration;

namespace Blaise.Case.Data.Delivery.Extensions
{
    public static class ConfigurationExtensionMethods
    {
        public static void ThrowExceptionIfNull(this string environmentVariable, string variableName)
        {
            if (string.IsNullOrWhiteSpace(environmentVariable))
            {
                throw new ConfigurationErrorsException($"No value found for environment variable '{variableName}'");
            }
        }
    }
}
