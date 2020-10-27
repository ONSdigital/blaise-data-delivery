using System;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using BlaiseDataDelivery.Interfaces.Providers;
using BlaiseDataDelivery.Interfaces.Services;
using BlaiseDataDelivery.Services;
using log4net;
using Moq;
using NUnit.Framework;

namespace BlaiseDataDelivery.Tests.Services
{
    public class InitialiseServiceTests
    {
        private Mock<ILog> _loggingMock;
        private Mock<IQueueService> _queueServiceMock;
        private Mock<IMessageHandler> _messageHandlerMock;
        private Mock<IConfigurationProvider> _configurationProviderMock;

        private InitialiseService _sut;


        [SetUp]
        public void SetUpTests()
        {
            _loggingMock = new Mock<ILog>();
            _queueServiceMock = new Mock<IQueueService>();
            _messageHandlerMock = new Mock<IMessageHandler>();
            _configurationProviderMock = new Mock<IConfigurationProvider>();

            _sut = new InitialiseService(
                _loggingMock.Object,
                _queueServiceMock.Object,
                _messageHandlerMock.Object,
                _configurationProviderMock.Object);
        }

        [Test]
        public void Given_I_Call_Start_Then_The_Correct_Methods_Are_Called_To_Setup_And_Subscribe_To_The_Appropriate_Queues()
        {
            //act
            _sut.Start();

            //assert
            _queueServiceMock.Verify(v => v.Subscribe(It.IsAny<IMessageHandler>()), Times.Once);
        }

        [Test]
        public void Given_I_Call_Start_And_An_Exception_Is_Thrown_During_The_Process_Then_The_Exception_Is_Logged()
        {
            //arrange
            var exceptionThrown = new Exception("Error message");
            _queueServiceMock.Setup(s => s.Subscribe(It.IsAny<IMessageHandler>())).Throws(exceptionThrown);
            _loggingMock.Setup(l => l.Error(It.IsAny<Exception>()));

            //act
            Assert.Throws<Exception>(() => _sut.Start());

            //assert
            _loggingMock.Verify(v => v.Error(exceptionThrown), Times.Once);
        }

        [Test]
        public void Given_I_Call_Stop_Then_The_Appropriate_Service_Is_Called()
        {
            //act
            _sut.Stop();

            //assert
            _queueServiceMock.Verify(v => v.CancelAllSubscriptions(), Times.Once);
        }

        [Test]
        public void Given_I_Call_Stop_And_An_Exception_Is_Thrown_During_The_Process_Then_The_Exception_Is_Logged()
        {
            //arrange
            var exceptionThrown = new Exception("Error message");
            _queueServiceMock.Setup(s => s.CancelAllSubscriptions()).Throws(exceptionThrown);
            _loggingMock.Setup(l => l.Error(It.IsAny<Exception>()));

            //act
            Assert.Throws<Exception>(() => _sut.Stop());

            //assert
            _loggingMock.Verify(v => v.Error(exceptionThrown), Times.Once);
        }
    }
}
