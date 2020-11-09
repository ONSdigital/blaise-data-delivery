using System;
using Blaise.Case.Data.Delivery.MessageBroker.Mappers;
using Blaise.Case.Data.Delivery.MessageBroker.Models;
using NUnit.Framework;

namespace Blaise.Case.Data.Delivery.Tests.Unit.MessageBroker
{
    public class MessageModelMapperTests
    {
        private MessageModelMapper _sut;

        [SetUp]
        public void SetUpTests()
        {
            _sut = new MessageModelMapper();
        }

        [Test]
        public void Given_Valid_Arguments_When_I_Call_MapToMessageModel_Then_A_MessageModel_Is_Returned()
        {
            //arrange
            const string message = @"{ ""instrument"":""OPN2004A"", ""serverpark"":""tel""}";

            //act
            var result = _sut.MapToMessageModel(message);

            //assert
            Assert.NotNull(result);
            Assert.IsInstanceOf<MessageModel>(result);
        }

        [Test]
        public void Given_Valid_Arguments_When_I_Call_MapToMessageModel_Then_A_MessageModel_With_The_Correct_Data_Is_Returned()
        {
            //arrange
            const string message = @"{ ""instrument"":""OPN2004A"", ""serverpark"":""tel""}";

            //act
            var result = _sut.MapToMessageModel(message);

            //assert
            Assert.AreEqual("OPN2004A", result.InstrumentName);
            Assert.AreEqual("tel", result.ServerParkName);
        }

        [Test]
        public void Given_A_Null_Message_WhenI_Call_MapToMessageModel_Then_An_ArgumentNullException_Is_Thrown()
        {
            //arrange
            const string paramName = "message";

            //act && assert
            var result = Assert.Throws<ArgumentNullException>(() => _sut.MapToMessageModel(null));
            Assert.AreEqual(paramName, result.ParamName);
        }

        [Test]
        public void Given_An_Empty_Message_WhenI_Call_MapToMessageModel_Then_An_ArgumentException_Is_Thrown()
        {
            //arrange
            var message = string.Empty;
            const string errorMessage = "A value for the argument 'message' must be supplied";

            //act && assert
            var result = Assert.Throws<ArgumentException>(() => _sut.MapToMessageModel(message));
            Assert.AreEqual(errorMessage, result.Message);
        }
    }
}
