using System.Collections.Generic;
using BlaiseDataDelivery.Interfaces.Services;
using BlaiseDataDelivery.Services;
using log4net;
using Moq;
using NUnit.Framework;

namespace BlaiseDataDelivery.Tests.Services
{
    public class DeliveryServiceTests
    {
        private Mock<ILog> _logMock;
        private Mock<IBucketService> _bucketServiceMock;
        private Mock<IFileService> _fileServiceMock;
        private Mock<IBlaiseService> _blaiseServiceMock;

        private DeliveryService _sut;

        [SetUp]
        public void SetUpTests()
        {
            _logMock = new Mock<ILog>();
            _bucketServiceMock = new Mock<IBucketService>();
            _fileServiceMock = new Mock<IFileService>();

            _blaiseServiceMock = new Mock<IBlaiseService>();
            _blaiseServiceMock.Setup(b => b.InstrumentExists(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            _sut = new DeliveryService(_logMock.Object, _bucketServiceMock.Object, _fileServiceMock.Object, _blaiseServiceMock.Object);
        }

        [Test]
        public void Given_An_Instrument_Exists_When_DeliverSingleInstrument_Is_Called_Then_True_Is_returned()
        {
            //arrange
            var serverParkName = "ServerPark1";
            var instrumentName = "InstrumentName1";
            var tempPath = "TempPath";
            var bucketName = "BucketName";

            _blaiseServiceMock.Setup(b => b.CreateDeliveryFile(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>())).Returns(It.IsAny<string>());
            _fileServiceMock.Setup(f => f.CreateEncryptedZipFile(It.IsAny<IList<string>>(), It.IsAny<string>()))
                .Returns(It.IsAny<string>());
            _bucketServiceMock.Setup(b => b.UploadFileToBucket(It.IsAny<string>(), It.IsAny<string>()));

            //act
            var result = _sut.DeliverSingleInstrument(serverParkName, instrumentName, tempPath, bucketName);

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Given_An_Instrument_Exists_When_DeliverSingleInstrument_Is_Called_Then_The_Instrument_Is_Delivered()
        {
            var serverParkName = "ServerPark1";
            var instrumentName = "InstrumentName1";
            var tempPath = "TempPath";
            var bucketName = "BucketName";
            var encryptedZipFile = "encryptedFile.zip";

            _blaiseServiceMock.Setup(b => b.CreateDeliveryFile(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>())).Returns(It.IsAny<string>());
            _fileServiceMock.Setup(f => f.CreateEncryptedZipFile(It.IsAny<IList<string>>(), It.IsAny<string>()))
                .Returns(encryptedZipFile);
            _bucketServiceMock.Setup(b => b.UploadFileToBucket(It.IsAny<string>(), It.IsAny<string>()));

            //act
            _sut.DeliverSingleInstrument(serverParkName, instrumentName, tempPath, bucketName);

            //assert
            _blaiseServiceMock.Verify(v => v.CreateDeliveryFile(serverParkName, instrumentName, tempPath), Times.Once);
            _bucketServiceMock.Verify(v => v.UploadFileToBucket(encryptedZipFile, bucketName), Times.Once);
        }

        [Test]
        public void Given_An_Instrument_Does_Not_Exist_When_DeliverSingleInstrument_Is_Called_Then_False_Is_Returned()
        {
            //arrange
            var serverParkName = "ServerPark1";
            var instrumentName = "InstrumentName1";
            var tempPath = "TempPath";
            var bucketName = "BucketName";

            _blaiseServiceMock.Setup(b => b.InstrumentExists(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            //act
            var result = _sut.DeliverSingleInstrument(serverParkName, instrumentName, tempPath, bucketName);

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Given_An_Instrument_Does_Not_Exist_When_DeliverSingleInstrument_Is_Called_Then_An_Error_is_Logged_And_No_Files_Are_Delivered()
        {
            //arrange
            var serverParkName = "ServerPark1";
            var instrumentName = "InstrumentName1";
            var tempPath = "TempPath";
            var bucketName = "BucketName";

            _blaiseServiceMock.Setup(b => b.InstrumentExists(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            //act
            _sut.DeliverSingleInstrument(serverParkName, instrumentName, tempPath, bucketName);

            //assert
            _logMock.Verify(v => v.Error(It.IsAny<string>()), Times.Once);
            _blaiseServiceMock.Verify(v => v.InstrumentExists(serverParkName, instrumentName), Times.Once);
            _blaiseServiceMock.VerifyNoOtherCalls();
            _fileServiceMock.VerifyNoOtherCalls();
            _bucketServiceMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Given_Instruments_Are_Installed_When_DeliverAllInstruments_Is_Called_Then_True_Is_returned()
        {
            //arrange
            var serverParkName = "ServerPark1";
            var tempPath = "TempPath";
            var bucketName = "BucketName";

            var instrumentName1 = "OPN2004A";
            var deliveryFile1 = "OPN2004A.bdbx";

            var instrumentName2 = "OPN2007";
            var deliveryFile2 = "OPN2007.bdbx";

            _blaiseServiceMock.Setup(b => b.GetInstrumentsInstalled(It.IsAny<string>())).Returns(new List<string> { instrumentName1, instrumentName2 });
            _blaiseServiceMock.Setup(b => b.CreateDeliveryFile(It.IsAny<string>(), instrumentName1, It.IsAny<string>())).Returns(deliveryFile1);
            _blaiseServiceMock.Setup(b => b.CreateDeliveryFile(It.IsAny<string>(), instrumentName2, It.IsAny<string>())).Returns(deliveryFile2);

            _fileServiceMock.Setup(f => f.CreateEncryptedZipFile(It.IsAny<IList<string>>(), It.IsAny<string>()))
                .Returns(It.IsAny<string>());
            _bucketServiceMock.Setup(b => b.UploadFileToBucket(It.IsAny<string>(), It.IsAny<string>()));

            //act
            var result = _sut.DeliverAllInstruments(serverParkName, tempPath, bucketName);

            //assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Given_Instruments_Are_Installed_When_DeliverAllInstruments_Is_Called_Then_All_Instruments_Are_Delivered_For_That_ServerPark()
        {
            //arrange
            var serverParkName = "ServerPark1";
            var tempPath = "TempPath";
            var bucketName = "BucketName";

            var instrumentName1 = "OPN2004A";
            var deliveryFile1 = "OPN2004A.bdbx";

            var instrumentName2 = "OPN2007";
            var deliveryFile2 = "OPN2007.bdbx";

            var encryptedZipFile1 = "encryptedFile1.zip";
            var encryptedZipFile2 = "encryptedFile2.zip";

            _blaiseServiceMock.Setup(b => b.GetInstrumentsInstalled(It.IsAny<string>())).Returns(new List<string> { instrumentName1, instrumentName2 });
            _blaiseServiceMock.Setup(b => b.CreateDeliveryFile(It.IsAny<string>(), instrumentName1, It.IsAny<string>())).Returns(deliveryFile1);
            _blaiseServiceMock.Setup(b => b.CreateDeliveryFile(It.IsAny<string>(), instrumentName2, It.IsAny<string>())).Returns(deliveryFile2);

            _fileServiceMock.Setup(f => f.CreateEncryptedZipFile(It.IsAny<IList<string>>(), instrumentName1))
                .Returns(encryptedZipFile1);
            _fileServiceMock.Setup(f => f.CreateEncryptedZipFile(It.IsAny<IList<string>>(), instrumentName2))
                .Returns(encryptedZipFile2);

            _bucketServiceMock.Setup(b => b.UploadFileToBucket(It.IsAny<string>(), It.IsAny<string>()));

            //act
            _sut.DeliverAllInstruments(serverParkName, tempPath, bucketName);

            //assert
            _blaiseServiceMock.Verify(v => v.CreateDeliveryFile(serverParkName, instrumentName1, tempPath), Times.Once);
            _bucketServiceMock.Verify(v => v.UploadFileToBucket(encryptedZipFile1, bucketName), Times.Once);

            _blaiseServiceMock.Verify(v => v.CreateDeliveryFile(serverParkName, instrumentName2, tempPath), Times.Once);
            _bucketServiceMock.Verify(v => v.UploadFileToBucket(encryptedZipFile2, bucketName), Times.Once);
        }

        [Test]
        public void Given_Instruments_Are_Not_Installed_When_DeliverAllInstruments_Is_Called_Then_False_Is_Returned()
        {
            //arrange
            var serverParkName = "ServerPark1";
            var tempPath = "TempPath";
            var bucketName = "BucketName";

            _blaiseServiceMock.Setup(b => b.GetInstrumentsInstalled(It.IsAny<string>())).Returns(new List<string>());

            //act
            var result = _sut.DeliverAllInstruments(serverParkName, tempPath, bucketName);

            //assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Given_Instruments_Are_Not_Installed_When_DeliverAllInstruments_Is_Called_Then_An_Error_Is_Logged_And_No_Files_Are_Delivered()
        {
            //arrange
            var serverParkName = "ServerPark1";
            var tempPath = "TempPath";
            var bucketName = "BucketName";

            _blaiseServiceMock.Setup(b => b.GetInstrumentsInstalled(It.IsAny<string>())).Returns(new List<string>());

            //act
            _sut.DeliverAllInstruments(serverParkName, tempPath, bucketName);

            //assert
            _logMock.Verify(v => v.Error(It.IsAny<string>()), Times.Once);
            _blaiseServiceMock.Verify(v => v.CreateDeliveryFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _bucketServiceMock.Verify(v => v.UploadFileToBucket(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Given_Valid_Arguments_When_I_Call_UploadInstrumentFileToBucket_Then_The_File_Is_Zipped_Encrypted_And_Uploaded_To_The_Bucket()
        {
            //arrange
            var file = "File1";
            var instrumentName = "InstrumentName";
            var bucketName = "BucketName";

            var encryptedZipFile = "encryptedFile.zip";

            _fileServiceMock.Setup(f => f.CreateEncryptedZipFile(It.IsAny<IList<string>>(), It.IsAny<string>()))
                .Returns(encryptedZipFile);
            _bucketServiceMock.Setup(b => b.UploadFileToBucket(It.IsAny<string>(), It.IsAny<string>()));

            //act
            _sut.UploadInstrumentFileToBucket(file, instrumentName, bucketName);

            //assert
            _fileServiceMock.Verify(v => v.CreateEncryptedZipFile(It.IsAny<List<string>>(), It.IsAny<string>()), Times.Once);
            _bucketServiceMock.Verify(v => v.UploadFileToBucket(encryptedZipFile, bucketName));
        }

        [Test]
        public void Given_Valid_Arguments_When_I_Call_UploadInstrumentFileToBucket_Then_The_Temp_Files_Are_Deleted()
        {
            //arrange
            var file = "File1";
            var instrumentName = "InstrumentName";
            var bucketName = "BucketName";

            var encryptedZipFile = "encryptedFile.zip";

            _fileServiceMock.Setup(f => f.CreateEncryptedZipFile(It.IsAny<IList<string>>(), It.IsAny<string>()))
                .Returns(encryptedZipFile);
            _bucketServiceMock.Setup(b => b.UploadFileToBucket(It.IsAny<string>(), It.IsAny<string>()));

            //act
            _sut.UploadInstrumentFileToBucket(file, instrumentName, bucketName);

            //assert
            _fileServiceMock.Verify(v => v.DeleteFile(encryptedZipFile), Times.Once);
            _fileServiceMock.Verify(v => v.DeleteFile(file), Times.Once);
        }

        [Test]
        public void Given_Valid_Arguments_When_I_Call_UploadFileToBucket_Then_The_Correct_Method_Is_Called()
        {
            //arrange
            var file = "File1";
            var bucketName = "BucketName";

            _bucketServiceMock.Setup(f => f.UploadFileToBucket(It.IsAny<string>(), It.IsAny<string>()));
            _bucketServiceMock.Setup(f => f.UploadFileToBucket(It.IsAny<string>(), It.IsAny<string>()));

            //act
            _sut.UploadFileToBucket(file, bucketName);

            //assert
            _bucketServiceMock.Verify(v => v.UploadFileToBucket(file, bucketName), Times.Once);
        }
    }
}
