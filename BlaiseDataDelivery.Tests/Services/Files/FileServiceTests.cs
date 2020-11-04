using BlaiseDataDelivery.Interfaces.Services.Files;
using BlaiseDataDelivery.Models;
using BlaiseDataDelivery.Services.Files;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace BlaiseDataDelivery.Tests.Services.Files
{
    public class FileServiceTests
    {
        private Mock<IFileDirectoryService> _fileDirectoryMock;
        private Mock<IFileEncryptionService> _encryptionServiceMock;
        private Mock<IFileZipService> _zipServiceMock;
        private Mock<IFileCloudStorageService> _storageServiceMock;

        private FileService _sut;

        [SetUp]
        public void SetUpTests()
        {
            _fileDirectoryMock = new Mock<IFileDirectoryService>();
            _fileDirectoryMock.Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(It.IsAny<IEnumerable<string>>);

            _encryptionServiceMock = new Mock<IFileEncryptionService>();
            _zipServiceMock = new Mock<IFileZipService>();
            _storageServiceMock = new Mock<IFileCloudStorageService>();

            _sut = new FileService(
                _fileDirectoryMock.Object, _encryptionServiceMock.Object, _zipServiceMock.Object, _storageServiceMock.Object);
        }

        [Test]
        public void Given_Valid_Arguments_When_I_Call_GetFiles_Then_The_Correct_Method_Is_Called()
        {
            //arrange
            var path = "temp";
            var instrumentName = "OPN2004";
            var filePattern = "*.*";

            //act
            _sut.GetFiles(path, instrumentName, filePattern);

            //assert
            _fileDirectoryMock.Verify(v => v.GetFiles(path, $"{instrumentName}{filePattern}"), Times.Once);
        }

        [Test]
        public void Given_Valid_Arguments_When_I_Call_GetFiles_Then_A_List_Of_Files_Are_Returned()
        {
            //arrange
            var path = "temp";
            var instrumentName = "OPN2004";
            var filePattern = "*.*";
            var files = new List<string> { "File1", "File2" };


            _fileDirectoryMock.Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(files);

            //act
            var result = _sut.GetFiles(path, instrumentName, filePattern);

            //assert
            Assert.NotNull(result);
            Assert.AreEqual(files, result);
        }

        [Test]
        public void Given_Valid_Arguments_When_I_Call_CreateEncryptedZipFiles_Then_I_Get_The_Path_Of_The_Encrypted_Zip_Back()
        {
            //arrange
            var files = new List<string> { "File1", "File2" };
            var messageModel = new MessageModel
            {
                SourceFilePath = "SourcePath",
                InstrumentName = "InstrumentName"
            };

            var encryptedZipPath = string.Empty;

            _zipServiceMock.Setup(f => f.CreateZipFile(It.IsAny<IList<string>>(), It.IsAny<string>()));
            _encryptionServiceMock.Setup(e => e.EncryptFile(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((input, output) => encryptedZipPath = output);

            //act
            var result =_sut.CreateEncryptedZipFile(files, messageModel);

            //assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<string>(result);
            Assert.AreEqual(encryptedZipPath, result);
        }

        [Test]
        public void Given_Valid_Arguments_When_I_Call_CreateEncryptedZipFiles_Then_The_Correct_Methods_Are_Called()
        {
            //arrange
            var files = new List<string> { "File1", "File2" };
            var messageModel = new MessageModel { 
                SourceFilePath = "SourcePath",
                InstrumentName = "InstrumentName"
            };

            var tempZipPath = string.Empty;
            var encryptedZipPath = string.Empty;

            _zipServiceMock.Setup(f => f.CreateZipFile(It.IsAny<IList<string>>(), It.IsAny<string>())).Callback<IEnumerable<string>, string>((input, output) => tempZipPath = output);
            _encryptionServiceMock.Setup(e => e.EncryptFile(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((input, output) => encryptedZipPath = output);

            //act
            _sut.CreateEncryptedZipFile(files, messageModel);

            //assert
            _zipServiceMock.Verify(v => v.CreateZipFile(files, tempZipPath), Times.Once);
            _encryptionServiceMock.Verify(v => v.EncryptFile(tempZipPath, encryptedZipPath), Times.Once);
            _fileDirectoryMock.Verify(v => v.DeleteFile(tempZipPath), Times.Once);
        }

        [Test]
        public void Given_Valid_Arguments_When_I_Call_UploadFileToBucket_Then_The_Correct_Method_Is_Called()
        {
            //arrange
            var file = "File1";
            var bucketName = "BucketName";


            _storageServiceMock.Setup(f => f.UploadFileToBucket(It.IsAny<string>(), It.IsAny<string>()));

            //act
            _sut.UploadFileToBucket(file, bucketName);

            //assert
            _storageServiceMock.Verify(v => v.UploadFileToBucket(file, bucketName), Times.Once);
        }

        [Test]
        public void Given_Valid_Arguments_When_I_Call_DeleteFiles_Then_The_Correct_Method_Is_Called()
        {
            //arrange
            var files = new List<string> { "File1", "File2" };

            _fileDirectoryMock.Setup(f => f.DeleteFile(It.IsAny<string>()));

            //act
            _sut.DeleteFiles(files);

            //assert
            foreach (var file in files)
            {
                _fileDirectoryMock.Verify(v => v.DeleteFile(file), Times.Once);
            }
        }

        [Test]
        public void Given_Valid_Arguments_When_I_Call_DeleteFile_Then_The_Correct_Method_Is_Called()
        {
            //arrange
            var file = "File1";

            _fileDirectoryMock.Setup(f => f.DeleteFile(It.IsAny<string>()));

            //act
            _sut.DeleteFile(file);

            //assert
            _fileDirectoryMock.Verify(v => v.DeleteFile(file), Times.Once);
        }

        [Test]
        public void Given_Valid_Arguments_When_I_Call_GenerateUniqueFileName_Then_I_Get_The_Expected_Format_Back()
        {
            //arrange
            var expectedFileName = "dd_OPN2004A_08042020_034000";

            var instrumentName = "OPN2004A";
            var dateTime = DateTime.ParseExact("2020-04-08 15:40:00,000", "yyyy-MM-dd HH:mm:ss,fff",
                                       System.Globalization.CultureInfo.InvariantCulture);

            //act
            var result = _sut.GenerateUniqueFileName(instrumentName, dateTime);

            //assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<string>(result);
            Assert.AreEqual(expectedFileName, result);
        }
    }
}
