using System.Configuration;
using Blaise.Case.Data.Delivery.Core.Interfaces;

namespace Blaise.Case.Data.Delivery.Core.Configuration
{
    public class LocalConfigurationProvider : IConfigurationProvider
    {
        public string ProjectId => ConfigurationManager.AppSettings["ProjectId"];

        public string SubscriptionId => ConfigurationManager.AppSettings["SubscriptionId"];

        public string VmName => ConfigurationManager.AppSettings["VmName"];

        public string BucketName => ConfigurationManager.AppSettings["BucketName"];

        public string EncryptionKey => ConfigurationManager.AppSettings["EncryptionKey"];

        public string DeadletterTopicId => ConfigurationManager.AppSettings["DeadletterTopicId"];

        public string LocalProcessFolder => ConfigurationManager.AppSettings["LocalProcessFolder"];
    }
}
