using Blaise.Case.Data.Delivery.Providers;
using NUnit.Framework;

namespace Blaise.Case.Data.Delivery.Tests.Unit.Providers
{
    public class LocalConfigurationProviderTests
    {
        /// <summary>
        /// Please ensure the app.config in the test project has values that relate to the tests
        /// </summary>

        [Test]
        public void Given_I_Call_ProjectId_I_Get_The_Correct_Value_Back()
        {
            //arrange
            var configurationProvider = new LocalConfigurationProvider();

            //act
            var result = configurationProvider.ProjectId;

            //assert
            Assert.AreEqual("ProjectIdTest", result);
        }

        [Test]
        public void Given_I_Call_SubscriptionId_I_Get_The_Correct_Value_Back()
        {
            //arrange
            var configurationProvider = new LocalConfigurationProvider();

            //act
            var result = configurationProvider.SubscriptionId;

            //assert
            Assert.AreEqual("SubscriptionIdTest", result);
        }

        [Test]
        public void Given_I_Call_DeadletterTopicId_I_Get_The_Correct_Value_Back()
        {
            //arrange
            var configurationProvider = new LocalConfigurationProvider();

            //act
            var result = configurationProvider.DeadletterTopicId;

            //assert
            Assert.AreEqual("DeadletterTopicIdTest", result);
        }

        [Test]
        public void Given_I_Call_BucketName_I_Get_The_Correct_Value_Back()
        {
            //arrange
            var configurationProvider = new LocalConfigurationProvider();

            //act
            var result = configurationProvider.BucketName;

            //assert
            Assert.AreEqual("BucketNameTest", result);
        }

        [Test]
        public void Given_I_Call_EncryptionKey_I_Get_The_Correct_Value_Back()
        {
            //arrange
            var configurationProvider = new LocalConfigurationProvider();

            //act
            var result = configurationProvider.EncryptionKey;

            //assert
            Assert.AreEqual("EncryptionKeyTest", result);
        }

        [Test]
        public void Given_I_Call_LocalProcessFolder_I_Get_The_Correct_Value_Back()
        {
            //arrange
            var configurationProvider = new LocalConfigurationProvider();

            //act
            var result = configurationProvider.LocalProcessFolder;

            //assert
            Assert.AreEqual("LocalProcessFolderTest", result);
        }
    }
}
