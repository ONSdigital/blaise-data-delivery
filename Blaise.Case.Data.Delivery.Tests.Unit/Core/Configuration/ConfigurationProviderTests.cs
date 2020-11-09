using System.Configuration;
using Blaise.Case.Data.Delivery.Core.Configuration;
using NUnit.Framework;

namespace Blaise.Case.Data.Delivery.Tests.Unit.Core.Configuration
{
    public class ConfigurationProviderTests
    {
        /// <summary>
        /// Please ensure the app.config in the test project has values that relate to the tests
        /// </summary>

        [Test]
        public void Given_I_Call_ProjectId_And_The_Env_Variable_Is_Not_Set_Then_A_ConfigurationErrorsException_Is_Thrown()
        {
            //arrange
            var configurationProvider = new ConfigurationProvider();

            //act && assert
            var exception = Assert.Throws<ConfigurationErrorsException>(() =>
            {
                var result = configurationProvider.ProjectId;
            });
            Assert.AreEqual($"No value found for environment variable 'ENV_PROJECT_ID'", exception.Message);
        }

        [Test]
        public void Given_I_Call_SubscriptionId_And_The_Env_Variable_Is_Not_Set_Then_A_ConfigurationErrorsException_Is_Thrown()
        {
            //arrange
            var configurationProvider = new ConfigurationProvider();

            //act && assert
            var exception = Assert.Throws<ConfigurationErrorsException>(() =>
            {
                var result = configurationProvider.SubscriptionId;
            });
            Assert.AreEqual($"No value found for environment variable 'ENV_BDD_SUB_SUBS'", exception.Message);
        }

        [Test]
        public void Given_I_Call_BucketName_And_The_Env_Variable_Is_Not_Set_Then_A_ConfigurationErrorsException_Is_Thrown()
        {
            //arrange
            var configurationProvider = new ConfigurationProvider();

            //act && assert
            var exception = Assert.Throws<ConfigurationErrorsException>(() =>
            {
                var result = configurationProvider.BucketName;
            });
            Assert.AreEqual($"No value found for environment variable 'ENV_NCP_BUCKET_NAME'", exception.Message);
        }

        [Test]
        public void Given_I_Call_EncryptionKey_And_The_Env_Variable_Is_Not_Set_Then_A_ConfigurationErrorsException_Is_Thrown()
        {
            //arrange
            var configurationProvider = new ConfigurationProvider();

            //act && assert
            var exception = Assert.Throws<ConfigurationErrorsException>(() =>
            {
                var result = configurationProvider.EncryptionKey;
            });
            Assert.AreEqual($"No value found for environment variable 'ENV_BDD_PUBLIC_KEY'", exception.Message);
        }

        [Test]
        public void Given_I_Call_LocalProcessFolder_And_The_Env_Variable_Is_Not_Set_Then_A_ConfigurationErrorsException_Is_Thrown()
        {
            //arrange
            var configurationProvider = new ConfigurationProvider();

            //act && assert
            var exception = Assert.Throws<ConfigurationErrorsException>(() =>
            {
                var result = configurationProvider.LocalProcessFolder;
            });
            Assert.AreEqual($"No value found for environment variable 'ENV_BDD_LOCAL_PROCESS_DIR'", exception.Message);
        }

        [Test]
        public void Given_I_Call_DeadletterTopicId_And_The_Env_Variable_Is_Not_Set_Then_A_ConfigurationErrorsException_Is_Thrown()
        {
            //arrange
            var configurationProvider = new ConfigurationProvider();

            //act && assert
            var exception = Assert.Throws<ConfigurationErrorsException>(() =>
            {
                var result = configurationProvider.DeadletterTopicId;
            });
            Assert.AreEqual($"No value found for environment variable 'ENV_DEADLETTER_TOPIC'", exception.Message);
        }
    }
}

