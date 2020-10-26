﻿using System;
using System.Collections.Generic;
using BlaiseDataDelivery.Interfaces.Mappers;
using BlaiseDataDelivery.Interfaces.Providers;
using BlaiseDataDelivery.Models;
using log4net;
using Moq;
using NUnit.Framework;
using BlaiseDataDelivery.Interfaces.Services;

namespace BlaiseDataDelivery.Tests.MessageHandler
{
    public class MessageHandlerTests
    {
        private Mock<ILog> _loggerMock;
        private Mock<IConfigurationProvider> _configurationMock;
        private Mock<IMessageModelMapper> _mapperMock;
        private Mock<IDeliveryService> _deliveryServiceMock;
        private Mock<IBlaiseService> _blaiseServiceMock;

        private readonly string _message;
        private MessageModel _messageModel;
        private readonly string _localProcessFolder;
        private readonly string _bucketName;

        private MessageHandlers.MessageHandler _sut;

        public MessageHandlerTests()
        {
            _message = "Message";

            _localProcessFolder = @"c:\\temp";
            _bucketName = "delivery-bucket";
        }

        [SetUp]
        public void SetUpTests()
        {
            _messageModel = new MessageModel
            {
                ServerParkName = "ServerPark1",
                InstrumentName = "InstrumentName1",
            };

            _loggerMock = new Mock<ILog>();

            _configurationMock = new Mock<IConfigurationProvider>();
            _configurationMock.Setup(c => c.LocalProcessFolder).Returns(_localProcessFolder);
            _configurationMock.Setup(c => c.BucketName).Returns(_bucketName);

            _mapperMock = new Mock<IMessageModelMapper>();
            _mapperMock.Setup(m => m.MapToMessageModel(_message)).Returns(_messageModel);

            _deliveryServiceMock = new Mock<IDeliveryService>();

            _blaiseServiceMock = new Mock<IBlaiseService>();
            _blaiseServiceMock.Setup(b => b.ServerParkExists(It.IsAny<string>())).Returns(true);
            _blaiseServiceMock.Setup(b => b.InstrumentExists(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            _sut = new MessageHandlers.MessageHandler(_loggerMock.Object, _configurationMock.Object, _mapperMock.Object, _deliveryServiceMock.Object, _blaiseServiceMock.Object);
        }

        [Test]
        public void Given_No_ServerParkName_Is_Provided_When_The_Message_Is_Handled_Then_True_Is_returned()
        {
            //arrange
            _messageModel.ServerParkName = null;

            //act
            var result = _sut.HandleMessage(_message);

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Given_No_ServerParkName_Is_Provided_When_The_Message_Is_Handled_Then_A_Warning_Is_Logged_And_Nothing_Is_processed()
        {
            //arrange
            _messageModel.ServerParkName = null;

            //act
            _sut.HandleMessage(_message);

            //assert
            _loggerMock.Verify(v => v.Warn(It.IsAny<string>()), Times.Once);
            _blaiseServiceMock.VerifyNoOtherCalls();
            _deliveryServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_The_ServerPark_Does_Not_Exist_When_The_Message_Is_Handled_Then_False_Is_returned()
        {
            //arrange
            _blaiseServiceMock.Setup(b => b.ServerParkExists(It.IsAny<string>())).Returns(false);

            //act
            var result = _sut.HandleMessage(_message);

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Given_The_ServerPark_Does_Not_Exist_When_The_Message_Is_Handled_Then_An_Error_Is_Logged_And_Nothing_Is_processed()
        {
            //arrange
            _blaiseServiceMock.Setup(b => b.ServerParkExists(It.IsAny<string>())).Returns(false);

            //act
            _sut.HandleMessage(_message);

            //assert
            _loggerMock.Verify(v => v.Error(It.IsAny<string>()), Times.Once);
            _blaiseServiceMock.Verify(b => b.ServerParkExists(_messageModel.ServerParkName), Times.Once);
            _blaiseServiceMock.VerifyNoOtherCalls();
            _deliveryServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_An_Instrument_Is_Provided_When_The_Message_Is_Handled_Then_The_Correct_Service_Is_Called()
        {
            //arrange
            _deliveryServiceMock.Setup(f => f.DeliverSingleInstrument(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()));

            //act
            _sut.HandleMessage(_message);

            //assert
            _deliveryServiceMock.Verify(v => v.DeliverSingleInstrument(_messageModel.ServerParkName,
                _messageModel.InstrumentName, _localProcessFolder, _bucketName), Times.Once);

            _deliveryServiceMock.VerifyNoOtherCalls();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Given_An_Instrument_Is_Provided_When_The_Message_Is_Handled_Then_The_Correct_Value_is_Returned(bool deliveryResult)
        {
            //arrange
            _deliveryServiceMock.Setup(f => f.DeliverSingleInstrument(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>())).Returns(deliveryResult);

            //act
            var result = _sut.HandleMessage(_message);

            //assert
            Assert.AreEqual(deliveryResult, result);
        }

        [Test]
        public void Given_An_Instrument_Is__Not_Provided_When_The_Message_Is_Handled_Then_The_Correct_Service_Is_Called()
        {
            //arrange
            _messageModel.InstrumentName = null;
            _deliveryServiceMock.Setup(f => f.DeliverAllInstruments(It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()));

            //act
            _sut.HandleMessage(_message);

            //assert
            _deliveryServiceMock.Verify(v => v.DeliverAllInstruments(_messageModel.ServerParkName,
                 _localProcessFolder, _bucketName), Times.Once);

            _deliveryServiceMock.VerifyNoOtherCalls();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Given_An_Instrument_Is_Not_Provided_When_The_Message_Is_Handled_Then_The_Correct_Value_is_Returned(bool deliveryResult)
        {
            //arrange
            _messageModel.InstrumentName = null;
            _deliveryServiceMock.Setup(f => f.DeliverAllInstruments(It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>())).Returns(deliveryResult);

            //act
            var result = _sut.HandleMessage(_message);

            //assert
            Assert.AreEqual(deliveryResult, result);
        }

        [Test]
        public void Given_No_Instruments_Are_Installed_On_The_ServerPark_When_The_Message_Is_Handled_Then_False_Is_returned()
        {
            //arrange
            _messageModel.InstrumentName = null;

            _blaiseServiceMock.Setup(b => b.GetInstrumentsInstalled(It.IsAny<string>())).Returns(new List<string>());

            //act
            var result = _sut.HandleMessage(_message);

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Given_An_Exception_Is_Thrown_When_The_Message_Is_Handled_Then_False_is_Returned()
        {
            //arrange
            _blaiseServiceMock.Setup(b => b.ServerParkExists(It.IsAny<string>())).Throws(new Exception());

            //act
            var result =_sut.HandleMessage(_message);

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Given_An_Exception_Is_Thrown_When_The_Message_Is_Handled_Then_An_Error_Is_Logged_And_Nothing_Is_processed()
        {
            //arrange
            _blaiseServiceMock.Setup(b => b.ServerParkExists(It.IsAny<string>())).Throws(new Exception());

            //act
            _sut.HandleMessage(_message);

            //assert
            _loggerMock.Verify(v => v.Error(It.IsAny<string>()), Times.Once);
            _blaiseServiceMock.Verify(b => b.ServerParkExists(_messageModel.ServerParkName), Times.Once);
            _blaiseServiceMock.VerifyNoOtherCalls();
            _deliveryServiceMock.VerifyNoOtherCalls();
        }
    }
}