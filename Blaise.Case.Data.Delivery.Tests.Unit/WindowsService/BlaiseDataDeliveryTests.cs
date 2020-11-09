using Blaise.Case.Data.Delivery.WindowsService;
using NUnit.Framework;

namespace Blaise.Case.Data.Delivery.Tests.Unit.WindowsService
{
    public class BlaiseDataDeliveryTests
    {
        [Test]
        public void Given_I_Create_A_New_Instance_Of_BlaiseDataDelivery_Then_No_Exceptions_Are_Thrown()
        {
            //act && assert
            Assert.DoesNotThrow(() =>
            {
                // ReSharper disable once ObjectCreationAsStatement - needed to test unity resolution
                new BlaiseDataDelivery();
            });
        }

        [Test]
        public void Given_I_Create_A_New_Instance_Of_BlaiseDataDelivery_Then_All_Dependencies_Are_Registered_And_Resolved()
        {
            //arrange

            //act
            var result = new BlaiseDataDelivery();

            //assert
            Assert.NotNull(result.InitialiseService);
            Assert.IsInstanceOf<InitialiseWindowsService>(result.InitialiseService);
        }

        [Test]
        public void Given_I_Create_A_New_Instance_Of_MessageHandler_Then_All_Dependencies_Are_Registered_And_Resolved()
        {
            //arrange

            //act
            var result = new BlaiseDataDelivery();

            //assert
            Assert.NotNull(result.InitialiseService);
            Assert.IsInstanceOf<InitialiseWindowsService>(result.InitialiseService);
        }
    }
}
