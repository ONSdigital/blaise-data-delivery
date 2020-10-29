using System.Collections.Generic;
using System.Linq;
using Blaise.Case.Data.Delivery.Services;
using Blaise.Nuget.Api.Contracts.Interfaces;
using Blaise.Nuget.Api.Contracts.Models;
using Moq;
using NUnit.Framework;
using StatNeth.Blaise.API.ServerManager;

namespace Blaise.Case.Data.Delivery.Tests.Unit.Services
{
    public class BlaiseApiServiceTests
    {
        private Mock<IBlaiseApi> _blaiseApiMock;
        
        private BlaiseApiService _sut;

        [SetUp]
        public void SetUpTests()
        {
            _blaiseApiMock = new Mock<IBlaiseApi>();
            _blaiseApiMock.Setup(b => b.ServerParkExists(It.IsAny<ConnectionModel>(), It.IsAny<string>()));

            _sut = new BlaiseApiService(_blaiseApiMock.Object);
        }

        [Test]
        public void Given_A_ServerParkName_When_I_Call_ServerParkExists_Then_The_Correct_Method_Is_Called()
        {
            //arrange
            var serverParkName = "name1";

            //act
            _sut.ServerParkExists(serverParkName);

            //assert
            _blaiseApiMock.Verify(v => v.ServerParkExists(It.IsAny<ConnectionModel>(), serverParkName));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void Given_A_ServerParkName_When_I_Call_ServerParkExists_Then_The_Correct_Value_Is_Returned(bool exists)
        {
            //arrange
            var serverParkName = "name1";

            _blaiseApiMock.Setup(b => b.ServerParkExists(It.IsAny<ConnectionModel>(), It.IsAny<string>())).Returns(exists);

            //act
            var result = _sut.ServerParkExists(serverParkName);

            //assert
            Assert.AreEqual(exists, result);
        }

        [Test]
        public void Given_A_ServerParkName_When_I_Call_GetInstrumentsInstalled_Then_The_Correct_Method_Is_Called()
        {
            //arrange
            var serverParkName = "name1";

            var survey1Name = "Survey1";
            var survey2Name = "Survey2";

            var survey1Mock = new Mock<ISurvey>();
            survey1Mock.Setup(s => s.Name).Returns(survey1Name);

            var survey2Mock = new Mock<ISurvey>();
            survey2Mock.Setup(s => s.Name).Returns(survey2Name);

            _blaiseApiMock.Setup(b => b.GetSurveys(It.IsAny<ConnectionModel>(), serverParkName))
                .Returns(new List<ISurvey> {survey1Mock.Object, survey2Mock.Object});

            //act
            _sut.GetInstrumentsInstalled(serverParkName);

            //assert
            _blaiseApiMock.Verify(v => v.GetSurveys(It.IsAny<ConnectionModel>(), serverParkName));
        }

        [TestCase]
        public void Given_A_ServerParkName_When_I_Call_ServerParkExists_Then_The_Correct_Value_Is_Returned()
        {
            var serverParkName = "name1";
            var survey1Name = "Survey1";
            var survey2Name = "Survey2";

            var survey1Mock = new Mock<ISurvey>();
            survey1Mock.Setup(s => s.Name).Returns(survey1Name);

            var survey2Mock = new Mock<ISurvey>();
            survey2Mock.Setup(s => s.Name).Returns(survey2Name);

            _blaiseApiMock.Setup(b => b.GetSurveys(It.IsAny<ConnectionModel>(), serverParkName))
                .Returns(new List<ISurvey> { survey1Mock.Object, survey2Mock.Object });

            //act
            var result = _sut.GetInstrumentsInstalled(serverParkName).ToList();

            //assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<string>>(result);
            Assert.IsNotEmpty(result);
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains(survey1Name));
            Assert.IsTrue(result.Contains(survey2Name));
        }

        [Test]
        public void Given_A_ServerParkName_And_InstrumentName_When_I_Call_InstrumentExists_Then_The_Correct_Method_Is_Called()
        {
            //arrange
            var serverParkName = "name1";
            var instrumentName = "instrument1";

            //act
            _sut.InstrumentExists(serverParkName, instrumentName);

            //assert
            _blaiseApiMock.Verify(v => v.SurveyExists(It.IsAny<ConnectionModel>(), instrumentName, serverParkName));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GGiven_A_ServerParkName_And_InstrumentName_When_I_Call_InstrumentExists_Then_The_Correct_Value_Is_Returned(bool exists)
        {
            //arrange
            var serverParkName = "name1";
            var instrumentName = "instrument1";

            _blaiseApiMock.Setup(b => b.SurveyExists(It.IsAny<ConnectionModel>(), It.IsAny<string>(), It.IsAny<string>())).Returns(exists);

            //act
            var result = _sut.InstrumentExists(serverParkName, instrumentName);

            //assert
            Assert.AreEqual(exists, result);
        }

        [Test]
        public void Given_A_ServerParkName_And_InstrumentName_When_I_Call_CreateDeliveryFiles_Then_The_Correct_Method_Is_Called()
        {
            //arrange
            var serverParkName = "name1";
            var instrumentName = "instrument1";
            var outputPath = "Path1";

            //act
            _sut.CreateDeliveryFiles(serverParkName, instrumentName, outputPath);

            //assert
            _blaiseApiMock.Verify(v => v.BackupSurveyToFile(It.IsAny<ConnectionModel>(), serverParkName, instrumentName, outputPath));
        }
    }
}
