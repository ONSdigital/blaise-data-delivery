using System;
using Blaise.Data.Delivery.Extensions;
using Blaise.Data.Delivery.Interfaces.Providers;

namespace Blaise.Data.Delivery.Providers
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        public string ProjectId => GetVariable("ENV_PROJECT_ID");
        
        public string SubscriptionId => GetVariable("ENV_BDD_SUB_SUBS");

        public string EncryptionKey => GetVariable("ENV_BDD_PUBLIC_KEY");

        public string DeadletterTopicId => GetVariable("ENV_DEADLETTER_TOPIC");

        public string LocalProcessFolder => GetVariable("ENV_BDD_LOCAL_PROCESS_DIR");

        public string BucketName => GetVariable("ENV_NCP_BUCKET_NAME");
        public string VmName => GetVariable("VmName");

        private static string GetVariable(string variableName)
        {
            var value = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Machine);

            value.ThrowExceptionIfNull(variableName);

            return value;
        }
    }
}
