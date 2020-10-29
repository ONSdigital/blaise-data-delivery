using System;

namespace Blaise.Case.Data.Delivery.Helpers
{
    internal static class ParameterValidationHelper
    {
        public static void ThrowExceptionIfNullOrEmpty(this string parameter, string parameterName)
        {
            if(parameter == null)
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
