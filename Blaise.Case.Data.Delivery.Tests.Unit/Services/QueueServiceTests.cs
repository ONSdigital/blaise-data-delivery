using Blaise.Case.Data.Delivery.Interfaces.Providers;
using Blaise.Case.Data.Delivery.Services;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using log4net;
using Moq;
using NUnit.Framework;

namespace Blaise.Case.Data.Delivery.Tests.Unit.Services
{
    public class QueueServiceTests
    {
        private Mock<ILog> _loggerMock;
        private Mock<IConfigurationProvider> _configurationProviderMock;
        private Mock<IMessageHandler> _messageHandlerMock;
        private Mock<IFluentQueueApi> _queueProviderMock;

        private readonly string _projectId;
        private readonly string _subscriptionId;
        private readonly string _vmName;
        private readonly string _deadLetterTopicId;

        private QueueService _sut;

        public QueueServiceTests()
        {
            _projectId = "ProjectId";
            _subscriptionId = "SubscriptionId";
            _vmName = "VmName";
            _deadLetterTopicId = "deadLetterTopicId";
        }

        [SetUp]
        public void SetUpTests()
        {
            _loggerMock = new Mock<ILog>();

            _configurationProviderMock = new Mock<IConfigurationProvider>();
            _configurationProviderMock.Setup(c => c.ProjectId).Returns(_projectId);
            _configurationProviderMock.Setup(c => c.SubscriptionId).Returns(_subscriptionId);
            _configurationProviderMock.Setup(c => c.VmName).Returns(_vmName);
            _configurationProviderMock.Setup(c => c.DeadletterTopicId).Returns(_deadLetterTopicId);

            _messageHandlerMock = new Mock<IMessageHandler>();

            _queueProviderMock = new Mock<IFluentQueueApi>();

            _sut = new QueueService(
                _loggerMock.Object,
                _configurationProviderMock.Object,
                _queueProviderMock.Object);
        }

        [Test]
        public void Given_I_Call_Subscribe_Then_The_Correct_Calls_Are_Made_And_Subscribes_To_The_Appropriate_Queues()
        {
            //arrange
            _queueProviderMock.Setup(q => q.WithProject(It.IsAny<string>())).Returns(_queueProviderMock.Object);
            _queueProviderMock.Setup(q => q.WithSubscription(It.IsAny<string>())).Returns(_queueProviderMock.Object);
            _queueProviderMock.Setup(q => q.WithExponentialBackOff(It.IsAny<int>(), It.IsAny<int>())).Returns(_queueProviderMock.Object);
            _queueProviderMock.Setup(q => q.WithDeadLetter(It.IsAny<string>(),
                It.IsAny<int>())).Returns(_queueProviderMock.Object);
            _queueProviderMock.Setup(q => q.StartConsuming(It.IsAny<IMessageHandler>(), It.IsAny<bool>()));

            //act
            _sut.Subscribe(_messageHandlerMock.Object);

            //assert
            _queueProviderMock.Verify(v => v.WithProject(_projectId), Times.Once);
            _queueProviderMock.Verify(v => v.WithSubscription(_subscriptionId), Times.Once);
            _queueProviderMock.Verify(v => v.WithExponentialBackOff(60, 600), Times.Once);
            _queueProviderMock.Verify(v => v.WithDeadLetter(_deadLetterTopicId, 5), Times.Once);
            _queueProviderMock.Verify(v => v.StartConsuming(_messageHandlerMock.Object, It.IsAny<bool>()), Times.Once);
        }

        [Test]
        public void Given_I_Call_CancelAllSubscriptions_Then_The_Correct_Call_Is_Made()
        {
            //arrange
            _queueProviderMock.Setup(q => q.StopConsuming());

            //act
            _sut.CancelAllSubscriptions();

            //assert
            _queueProviderMock.Verify(v => v.StopConsuming(), Times.Once);
        }
    }
}
