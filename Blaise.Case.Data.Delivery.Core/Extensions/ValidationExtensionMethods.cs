using System;
using System.Configuration;

namespace Blaise.Case.Data.Delivery.Core.Extensions
{
    public static class ValidationExtensionMethods
    {
        public static void ThrowExceptionIfNull(this string environmentVariable, string variableName)
        {
            if (string.IsNullOrWhiteSpace(environmentVariable))
            {
                throw new ConfigurationErrorsException($"No value found for environment variable '{variableName}'");
            }
        }

        public static void ThrowExceptionIfNullOrEmpty(this string parameter, string parameterName)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (string.IsNullOrWhiteSpace(parameter))
            {

                throw new ArgumentException($"A value for the argument '{parameterName}' must be supplied");
            }
        }
    }
}
