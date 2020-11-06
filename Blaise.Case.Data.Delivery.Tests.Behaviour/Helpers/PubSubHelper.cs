using Blaise.Nuget.PubSub.Api;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaise.Case.Data.Delivery.Tests.Behaviour.Helpers
{
    public class PubSubHelper
    {
        private readonly IFluentQueueApi _queueApi;
        private readonly ConfigurationHelper _configurationHelper;

        public PubSubHelper()
        {
            _queueApi = new FluentQueueApi();
            _configurationHelper = new ConfigurationHelper();

        }
        public void PublishMessage(string message)
        {
            _queueApi
                .WithProject(_configurationHelper.ProjectId)
                .WithTopic(_configurationHelper.TopicId)
                .Publish(message);

        }
    }
}
