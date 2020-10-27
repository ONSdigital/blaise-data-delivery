using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using BlaiseDataDelivery.Interfaces.Services;
using BlaiseDataDelivery.Services;
using Moq;
using NUnit.Framework;

namespace BlaiseDataDelivery.Tests.Services
{
    public class FileServiceTests
    {
        private Mock<IEncryptionService> _encryptionServiceMock;
        private Mock<ICompressionService> _compressionServiceMock;
        private Mock<IFileSystem> _fileSystemMock;

        private FileService _sut;

        [SetUp]
        public void SetUpTests()
        {
            _encryptionServiceMock = new Mock<IEncryptionService>();
            _compressionServiceMock = new Mock<ICompressionService>();

            _fileSystemMock = new Mock<IFileSystem>();
            _fileSystemMock.Setup(f => f.File.Delete(It.IsAny<string>()));

            _sut = new FileService(_encryptionServiceMock.Object, _compressionServiceMock.Object, _fileSystemMock.Object);
        }

        [Test]
        public void Given_Valid_Arguments_When_I_Call_CreateEncryptedZipFiles_Then_I_Get_The_Path_Of_The_Encrypted_Zip_Back()
        {
            //arrange
            var files = new List<string> { "File1", "File2" };
            var instrumentName = "InstrumentName";
            var encryptedZipPath = string.Empty;

            _compressionServiceMock.Setup(f => f.CreateZipFile(It.IsAny<IList<string>>(), It.IsAny<string>()));
            _encryptionServiceMock.Setup(e => e.EncryptFile(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((input, output) => encryptedZipPath = output);

            //act
            var result = _sut.CreateEncryptedZipFile(files, instrumentName);

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
            var instrumentName = "InstrumentName";
            var tempZipPath = string.Empty;
            var encryptedZipPath = string.Empty;

            _compressionServiceMock.Setup(f => f.CreateZipFile(It.IsAny<IList<string>>(), It.IsAny<string>())).Callback<IEnumerable<string>, string>((input, output) => tempZipPath = output);
            _encryptionServiceMock.Setup(e => e.EncryptFile(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((input, output) => encryptedZipPath = output);

            //act
            _sut.CreateEncryptedZipFile(files, instrumentName);

            //assert
            _compressionServiceMock.Verify(v => v.CreateZipFile(files, tempZipPath), Times.Once);
            _encryptionServiceMock.Verify(v => v.EncryptFile(tempZipPath, encryptedZipPath), Times.Once);
        }

        [Test]
        public void Given_Valid_Arguments_When_I_Call_GenerateUniqueFileName_Then_I_Get_The_Expected_Format_Back()
        {
            //arrange
            var expectedFileName = "dd_OPN2004A_08402020_034000";
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

        [Test]
        public void Given_Valid_Arguments_When_I_Call_DeleteFiles_Then_The_Correct_Method_Is_Called()
        {
            //arrange
            var files = new List<string> { "File1", "File2" };

            //act
            _sut.DeleteFiles(files);

            //assert
            foreach (var file in files)
            {
                _fileSystemMock.Verify(v => v.File.Delete(file), Times.Once);
            }
        }

        [Test]
        public void Given_Valid_Arguments_When_I_Call_DeleteFile_Then_The_Correct_Method_Is_Called()
        {
            //arrange
            var file = "File1";

            //act
            _sut.DeleteFile(file);

            //assert
            _fileSystemMock.Verify(v => v.File.Delete(file), Times.Once);
        }

        [Test]
        public void Given_Valid_Arguments_When_I_Call_GetFiles_Then_The_Correct_Method_Is_Called()
        {
            //arrange
            var filePath = "File1";

            var files = new List<string>
            {
                "File1",
                "File2"
            };

            _fileSystemMock.Setup(f => f.Directory.GetFiles(filePath)).Returns(files.ToArray());

            //act
            _sut.GetFiles(filePath);

            //assert
            _fileSystemMock.Verify(v => v.Directory.GetFiles(filePath), Times.Once);
        }

        [Test]
        public void Given_Valid_Arguments_When_I_Call_GetFiles_Then_The_Correct_File_Are_Returned()
        {
            //arrange
            var filePath = "File1";

            var files = new List<string>
            {
                "File1",
                "File2"
            };

            _fileSystemMock.Setup(f => f.Directory.GetFiles(filePath)).Returns(files.ToArray());

            //act
            var result =_sut.GetFiles(filePath).ToList();

            //assert
           Assert.IsNotNull(result);
           Assert.IsNotEmpty(result);
           Assert.AreEqual(2, result.Count);
           Assert.AreEqual(files, result);
        }
    }
}
