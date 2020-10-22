using Blaise.Nuget.PubSub.Contracts.Interfaces;
using BlaiseDataDelivery.Interfaces.Services;
using BlaiseDataDelivery.Interfaces.Services.Files;
using BlaiseDataDelivery.Providers;
using NUnit.Framework;

namespace BlaiseDataDelivery.Tests.Providers
{
    public class UnityProviderTests
    {
        [Test]
        public void Given_I_Create_A_New_Instance_Of_UnityProvider_Then_No_Exceptions_Are_Thrown()
        {
            //act && assert
            Assert.DoesNotThrow(() =>
            {
                var unityProvider = new UnityProvider();
  
            });
        }

        [Test]
        public void
            Given_I_Create_A_New_Instance_Of_UnityProvider_Then_All_Dependencies_Are_Registered_And_Resolved()
        {
            //arrange
            var sut = new UnityProvider();

            //act
            var result = sut.Resolve<IInitialiseService>();

            //assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<IInitialiseService>(result);
        }

        [Test]
        public void
            Given_I_Create_A_New_Instance_Of_MessageHandler_Then_All_Dependencies_Are_Registered_And_Resolved()
        {
            //arrange
            var sut = new UnityProvider();

            //act
            var result = sut.Resolve<IMessageHandler>();

            //assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<IMessageHandler>(result);
        }

        [Test]
        public void
            Given_I_Create_A_New_Instance_Of_FileService_Then_All_Dependencies_Are_Registered_And_Resolved()
        {
            //arrange
            var sut = new UnityProvider();

            //act
            var result = sut.Resolve<IFileService>();

            //assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<IFileService>(result);
        }
    }
}
