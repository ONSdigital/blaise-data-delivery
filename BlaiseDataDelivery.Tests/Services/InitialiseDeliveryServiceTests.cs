using Blaise.Nuget.PubSub.Contracts.Interfaces;
using BlaiseDataDelivery.Interfaces.Services;
using BlaiseDataDelivery.Services;
using Moq;
using NUnit.Framework;

namespace BlaiseDataDelivery.Tests.Services
{
    public class InitialiseDeliveryServiceTests
    {
        private Mock<IQueueService> _subscriptionMock;
        private Mock<IMessageHandler> _messageHandlerMock;

        private InitialiseService _sut;

        [SetUp]
        public void SetUpTests()
        {
            _subscriptionMock = new Mock<IQueueService>();
            _messageHandlerMock = new Mock<IMessageHandler>();

            _sut = new InitialiseService(
                _subscriptionMock.Object, 
                _messageHandlerMock.Object);
        }

        [Test]
        public void Given_I_Call_SetupSubscription_Then_Subscription_To_The_Data_Delivery_Queue_Is_Setup()
        {
            //act
            _sut.Start();

            //assert
            _subscriptionMock.Verify(v => v.Subscribe(_messageHandlerMock.Object), Times.Once);
        }

        [Test]
        public void Given_I_Call_CancelSubscription_Then_Subscription_To_The_Data_Delivery_Queue_Is_Cancelled()
        {
            //act
            _sut.Stop();

            //assert
            _subscriptionMock.Verify(v => v.CancelAllSubscriptions(), Times.Once);
        }
    }
}
