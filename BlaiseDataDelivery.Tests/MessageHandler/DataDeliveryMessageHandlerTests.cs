using BlaiseDataDelivery.Interfaces.Mappers;
using BlaiseDataDelivery.Interfaces.Providers;
using BlaiseDataDelivery.Interfaces.Services.Files;
using BlaiseDataDelivery.MessageHandlers;
using BlaiseDataDelivery.Models;
using log4net;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace BlaiseDataDelivery.Tests.MessageHandler
{
    public class DataDeliveryMessageHandlerTests
    {
        private Mock<ILog> _loggerMock;
        private Mock<IConfigurationProvider> _configurationMock;
        private Mock<IMessageModelMapper> _mapperMock;
        private Mock<IFileService> _fileServiceMock;

        private readonly string _message;
        private readonly MessageModel _messageModel;
        private readonly string _localProcessFolder;

        private DataDeliveryMessageHandler _sut;

        public DataDeliveryMessageHandlerTests()
        {
            _message = "Message";

            _messageModel = new MessageModel
            {
                ServerParkName = "SourcePath",
                InstrumentName = "InstrumentName",
            };

            _localProcessFolder = @"c:\\temp";
        }

        [SetUp]
        public void SetUpTests()
        {
            _loggerMock = new Mock<ILog>();

            _configurationMock = new Mock<IConfigurationProvider>();
            _configurationMock.Setup(c => c.LocalProcessFolder).Returns(_localProcessFolder);


            _mapperMock = new Mock<IMessageModelMapper>();
            _mapperMock.Setup(m => m.MapToMessageModel(_message)).Returns(_messageModel);

            _fileServiceMock = new Mock<IFileService>();
            _fileServiceMock.Setup(f => f.CreateDeliveryFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new List<string>());
            _fileServiceMock.Setup(f => f.CreateEncryptedZipFile(It.IsAny<IList<string>>(), It.IsAny<MessageModel>()));
            _fileServiceMock.Setup(f => f.UploadFileToBucket(It.IsAny<string>(), It.IsAny<string>()));
            _fileServiceMock.Setup(f => f.DeleteFile(It.IsAny<string>()));
            _fileServiceMock.Setup(f => f.DeleteFiles(It.IsAny<IEnumerable<string>>()));

            _sut = new DataDeliveryMessageHandler(
                _loggerMock.Object, _configurationMock.Object, _mapperMock.Object, _fileServiceMock.Object);
        }

        [Test]
        public void Given_Valid_Arguments_When_HandleMessage_Is_Called_Then_True_Is_Returned()
        {
            //act
            var result = _sut.HandleMessage(_message);

            //assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [Test]
        public void Given_Valid_Arguments_When_HandleMessage_Is_Called_Then_Correct_Services_Are_Called()
        {
            //arrange

            var bucketName = "BucketName";
            var filesToProcess = new List<string>
            {
                "File1",
                "File2"
            };

            var dateTime = DateTime.Now;
            var encryptedZipFilePath = $"SourcePath\\dd_InstrumentName_{dateTime:ddmmyy}_{dateTime:hhmmss}.zip";

            _configurationMock.Setup(c => c.BucketName).Returns(bucketName);

            _fileServiceMock.Setup(f => f.CreateDeliveryFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(filesToProcess);
            _fileServiceMock.Setup(f => f.CreateEncryptedZipFile(It.IsAny<IList<string>>(), It.IsAny<MessageModel>())).Returns(encryptedZipFilePath);

            //act
            _sut.HandleMessage(_message);

            //assert
            _mapperMock.Verify(v => v.MapToMessageModel(_message), Times.Once);

            _fileServiceMock.Verify(v => v.CreateDeliveryFiles(_messageModel.ServerParkName, _messageModel.InstrumentName, _localProcessFolder));
            _fileServiceMock.Verify(v => v.CreateEncryptedZipFile(filesToProcess, _messageModel), Times.Once);
            _fileServiceMock.Verify(v => v.UploadFileToBucket(encryptedZipFilePath, bucketName), Times.Once);
            _fileServiceMock.Verify(v => v.DeleteFile(encryptedZipFilePath), Times.Once);
            _fileServiceMock.VerifyNoOtherCalls();
        }

        //LU-7600 
        [Test]
        public void Given_Valid_Arguments_When_HandleMessage_Is_Called_Then_The_Source_Files_Are_Not_Deleted()
        {
            //arrange
            var bucketName = "BucketName";
            var filesToProcess = new List<string>
            {
                "File1",
                "File2"
            };

            var dateTime = DateTime.Now;
            var encryptedZipFilePath = $"SourcePath\\dd_InstrumentName_{dateTime:ddmmyy}_{dateTime:hhmmss}.zip";

            _configurationMock.Setup(c => c.BucketName).Returns(bucketName);

            _fileServiceMock.Setup(f => f.CreateDeliveryFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(filesToProcess);
            _fileServiceMock.Setup(f => f.CreateEncryptedZipFile(It.IsAny<IList<string>>(), It.IsAny<MessageModel>())).Returns(encryptedZipFilePath);

            //act
            _sut.HandleMessage(_message);

            //assert
            _fileServiceMock.Verify(v => v.DeleteFiles(filesToProcess), Times.Never);
        }

        [Test]
        public void Given_No_Files_Available_To_Process_When_HandleMessage_Is_Called_Then_True_Is_Returned()
        {
            //arrange
            _fileServiceMock.Setup(f => f.CreateDeliveryFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new List<string>());

            //act
            var result = _sut.HandleMessage(_message);

            //assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result);
        }

        [Test]
        public void Given_No_Files_Available_To_Process_When_HandleMessage_Is_Called_Then_The_Services_Are_Not_Called()
        {
            //arrange
            _fileServiceMock.Setup(f => f.CreateDeliveryFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(new List<string>());

            //act
            _sut.HandleMessage(_message);

            //assert
            _mapperMock.Verify(v => v.MapToMessageModel(_message), Times.Once);

            _fileServiceMock.Verify(v => v.CreateDeliveryFiles(_messageModel.ServerParkName, _messageModel.InstrumentName, _localProcessFolder));
            _fileServiceMock.Verify(v => v.CreateEncryptedZipFile(It.IsAny<IList<string>>(), It.IsAny<MessageModel>()), Times.Never);
            _fileServiceMock.Verify(v => v.UploadFileToBucket(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _fileServiceMock.Verify(v => v.DeleteFile(It.IsAny<string>()), Times.Never);
            _fileServiceMock.Verify(v => v.DeleteFiles(It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [Test]
        public void Given_An_Exception_Occurs_During_Processing_When_HandleMessage_Is_Called_Then_False_Is_Returned()
        {
            //arrange
            _fileServiceMock.Setup(f => f.CreateDeliveryFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

            //act
            var result = _sut.HandleMessage(_message);

            //assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result);
        }

        [Test]
        public void Given_An_Exception_Occurs_During_Processing_When_HandleMessage_Is_Called_Then_The_Exception_Is_Not_Bubbled_Up()
        {
            //arrange
            _fileServiceMock.Setup(f => f.CreateDeliveryFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new Exception());

            //act && assert
            Assert.DoesNotThrow(() => _sut.HandleMessage(_message));
        }
    }
}
