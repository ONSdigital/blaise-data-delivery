using Blaise.Case.Data.Delivery.WindowsService.Interfaces;
using Blaise.Case.Data.Delivery.WindowsService.Ioc;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using NUnit.Framework;

namespace Blaise.Case.Data.Delivery.Tests.Unit.WindowsService.Ioc
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
            var result = sut.Resolve<IInitialiseWindowsService>();

            //assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<IInitialiseWindowsService>(result);
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
    }
}
