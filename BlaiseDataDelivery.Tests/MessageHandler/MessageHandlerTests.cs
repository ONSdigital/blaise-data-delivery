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
                ServerParkName = "SourcePath",
                InstrumentName = "InstrumentName",
            };

            _loggerMock = new Mock<ILog>();

            _configurationMock = new Mock<IConfigurationProvider>();
            _configurationMock.Setup(c => c.LocalProcessFolder).Returns(_localProcessFolder);
            _configurationMock.Setup(c => c.BucketName).Returns(_bucketName);

            _mapperMock = new Mock<IMessageModelMapper>();
            _mapperMock.Setup(m => m.MapToMessageModel(_message)).Returns(_messageModel);

            _deliveryServiceMock = new Mock<IDeliveryService>();
            _deliveryServiceMock.Setup(f => f.UploadInstrumentFileToBucket(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

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
        public void Given_An_Instrument_Is_Provided_But_Does_Not_Exist_When_The_Message_Is_Handled_Then_False_Is_returned()
        {
            //arrange
            _blaiseServiceMock.Setup(b => b.InstrumentExists(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            //act
            var result = _sut.HandleMessage(_message);

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Given_An_Instrument_Is_Provided_But_Does_Not_Exist_When_The_Message_Is_Handled_Then_An_Error_Is_Logged_And_Nothing_Is_processed()
        {
            //arrange
            _blaiseServiceMock.Setup(b => b.InstrumentExists(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            //act
            _sut.HandleMessage(_message);

            //assert
            _loggerMock.Verify(v => v.Error(It.IsAny<string>()), Times.Once);
            _blaiseServiceMock.Verify(b => b.ServerParkExists(_messageModel.ServerParkName), Times.Once);
            _blaiseServiceMock.Verify(b => b.InstrumentExists(_messageModel.ServerParkName, _messageModel.InstrumentName), Times.Once);
            _blaiseServiceMock.VerifyNoOtherCalls();
            _deliveryServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_An_Instrument_Is_Provided_When_The_Message_Is_Handled_Then_True_Is_returned()
        {
            //arrange
            var deliveryFile = "OPN2004A.bdbx";
            _blaiseServiceMock.Setup(b => b.CreateDeliveryFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(deliveryFile);
            _deliveryServiceMock.Setup(f => f.UploadInstrumentFileToBucket(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            //act
            var result = _sut.HandleMessage(_message);

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Given_An_Instrument_Is_Provided_When_The_Message_Is_Handled_Then_The_Instrument_Is_Delivered()
        {
            //arrange
            var deliveryFile = "OPN2004A.bdbx";
            _blaiseServiceMock.Setup(b => b.CreateDeliveryFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(deliveryFile);
            _deliveryServiceMock.Setup(f => f.UploadInstrumentFileToBucket(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            //act
            _sut.HandleMessage(_message);

            //assert
            _blaiseServiceMock.Verify(v => v.CreateDeliveryFile(_messageModel.ServerParkName, _messageModel.InstrumentName, _localProcessFolder), Times.Once);
            _deliveryServiceMock.Verify(v => v.UploadInstrumentFileToBucket(deliveryFile, _messageModel.InstrumentName, _bucketName), Times.Once);
        }

        [Test]
        public void Given_An_Instrument_Is_Not_Provided_When_The_Message_Is_Handled_Then_True_Is_returned()
        {
            //arrange
            _messageModel.InstrumentName = null;
            var instrumentName1 = "OPN2004A";
            var deliveryFile1 = "OPN2004A.bdbx";

            var instrumentName2 = "OPN2007";
            var deliveryFile2 = "OPN2007.bdbx";

            _blaiseServiceMock.Setup(b => b.GetInstrumentsInstalled(It.IsAny<string>())).Returns(new List<string> { instrumentName1, instrumentName2 });
            _blaiseServiceMock.Setup(b => b.CreateDeliveryFile(It.IsAny<string>(), instrumentName1, It.IsAny<string>())).Returns(deliveryFile1);
            _blaiseServiceMock.Setup(b => b.CreateDeliveryFile(It.IsAny<string>(), instrumentName2, It.IsAny<string>())).Returns(deliveryFile2);
            _deliveryServiceMock.Setup(f => f.UploadInstrumentFileToBucket(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            //act
            var result = _sut.HandleMessage(_message);

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Given_An_Instrument_Is_Not_Provided_When_The_Message_Is_Handled_Then_All_Instruments_Are_Delivered_For_That_ServerPark()
        {
            //arrange
            _messageModel.InstrumentName = null;
            var instrumentName1 = "OPN2004A";
            var deliveryFile1 = "OPN2004A.bdbx";

            var instrumentName2 = "OPN2007";
            var deliveryFile2 = "OPN2007.bdbx";

            _blaiseServiceMock.Setup(b => b.GetInstrumentsInstalled(It.IsAny<string>())).Returns(new List<string> { instrumentName1, instrumentName2 });
            _blaiseServiceMock.Setup(b => b.CreateDeliveryFile(It.IsAny<string>(), instrumentName1, It.IsAny<string>())).Returns(deliveryFile1);
            _blaiseServiceMock.Setup(b => b.CreateDeliveryFile(It.IsAny<string>(), instrumentName2, It.IsAny<string>())).Returns(deliveryFile2);
            _deliveryServiceMock.Setup(f => f.UploadInstrumentFileToBucket(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            //act
            _sut.HandleMessage(_message);

            //assert
            _blaiseServiceMock.Verify(v => v.CreateDeliveryFile(_messageModel.ServerParkName, instrumentName1, _localProcessFolder), Times.Once);
            _deliveryServiceMock.Verify(v => v.UploadInstrumentFileToBucket(deliveryFile1, instrumentName1, _bucketName), Times.Once);

            _blaiseServiceMock.Verify(v => v.CreateDeliveryFile(_messageModel.ServerParkName, instrumentName2, _localProcessFolder), Times.Once);
            _deliveryServiceMock.Verify(v => v.UploadInstrumentFileToBucket(deliveryFile2, instrumentName2, _bucketName), Times.Once);
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
        public void Given_No_Instruments_Are_Installed_On_The_ServerPark_When_The_Message_Is_Handled_Then_An_Error_Is_Logged_And_No_Files_Are_Delivered()
        {
            //arrange
            _messageModel.InstrumentName = null;

            _blaiseServiceMock.Setup(b => b.GetInstrumentsInstalled(It.IsAny<string>())).Returns(new List<string>());

            //act
            _sut.HandleMessage(_message);

            //assert
            _loggerMock.Verify(v => v.Error(It.IsAny<string>()), Times.Once);
            _blaiseServiceMock.Verify(v => v.CreateDeliveryFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _deliveryServiceMock.Verify(v => v.UploadInstrumentFileToBucket(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
