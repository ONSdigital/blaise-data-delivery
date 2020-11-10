using Blaise.Case.Data.Delivery.Tests.Behaviour.Helpers;
using Newtonsoft.Json;
using NUnit.Framework;
using StatNeth.Blaise.Data.DataValues;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Blaise.Case.Data.Delivery.MessageBroker.Models;
using TechTalk.SpecFlow;

namespace Blaise.Case.Data.Delivery.Tests.Behaviour.Steps
{
    [Binding]
    public sealed class DataDeliverySteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly ConfigurationHelper _configurationHelper;
        private readonly CaseHelper _caseHelper;
        private readonly PubSubHelper _pubSubHelper;
        private readonly BucketHelper _bucketHelper;
        private readonly CompressionHelper _compressionHelper;
        private readonly List<string> _questionnaires;
        private readonly string keyName = "questionnaire";

        public DataDeliverySteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _configurationHelper = new ConfigurationHelper();
            _caseHelper = new CaseHelper();
            _pubSubHelper = new PubSubHelper();
            _bucketHelper = new BucketHelper();
            _compressionHelper = new CompressionHelper();
            _questionnaires = new List<string>();
            _scenarioContext.Set(_questionnaires, keyName);
        }

        [Given(@"there are '(.*)' cases in the questionnaire '(.*)'")]
        public void GivenThereAreCasesInTheQuestionnaire(int numberOfCases, string questionnaire)
        {
            _caseHelper.DeleteCasesInDatabase(questionnaire);
            _caseHelper.CreateCasesForAnInstrument(questionnaire, numberOfCases, true);
        }

        [Given(@"there are no cases in the questionnaire '(.*)'")]
        public void GivenThereAreNoCasesInTheQuestionnaire(string questionnaire)
        {
            _caseHelper.DeleteCasesInDatabase(questionnaire);
            var cases = _caseHelper.GetCasesInAnInstrument(questionnaire);

            Assert.IsTrue(cases == 0);
        }


        [When(@"the data delivery service processes the questionnaire '(.*)'")]
        public void WhenTheDataDeliveryServiceProcessesTheQuestionnaire(string questionnaire)
        {
            var messageModel = new MessageModel {InstrumentName = questionnaire, ServerParkName = _configurationHelper.ServerParkName };
            var message = ConvertToJsonMessage(messageModel);
            _pubSubHelper.PublishMessage(message);

            var counter = 0;
            while (!_bucketHelper.FilesHaveBeenProcessed(_configurationHelper.BucketName))
            {
                Thread.Sleep(5000);
                counter++;

                if (counter == 20) return;
            }
        }

        [When(@"the data delivery service processes all questionnaires")]
        public void WhenTheDataDeliveryServiceProcessesAllQuestionnaires()
        {
            var messageModel = new MessageModel { ServerParkName = _configurationHelper.ServerParkName };
            var message = ConvertToJsonMessage(messageModel);
            _pubSubHelper.PublishMessage(message);

            var counter = 0;
            while (!_bucketHelper.FilesHaveBeenProcessed(_configurationHelper.BucketName))
            {
                Thread.Sleep(5000);
                counter++;

                if (counter == 20) return;
            }
        }

        [Then(@"No other questionnaires are delivered")]
        public void ThenNoOtherQuestionnairesAreDelivered()
        {
            var filesInBucket = _bucketHelper.GetFilesInBucket(_configurationHelper.BucketName);
            var questionnaires = _scenarioContext.Get<List<string>>(keyName);
            Assert.IsTrue(filesInBucket.Count() == questionnaires.Count());

            foreach (var questionnaire in questionnaires)
            {
                _bucketHelper.DownloadFromBucket(questionnaire);
                //ToDo: need to decrypt the zip folder before extracting.
                //_compressionHelper.ExtractFileToDirectory(questionnaire);
            }
        }

        [Then(@"all the cases are delivered for '(.*)'")]
        public void ThenAllTheCasesAreDelivered(string questionnaire)
        {
            var filesInBucket = _bucketHelper.GetFilesInBucket(_configurationHelper.BucketName);

            Assert.IsNotEmpty(filesInBucket);

            var fileName = filesInBucket.FirstOrDefault(x => x.ToLower().Contains($"dd_{questionnaire.ToLower()}"));

            Assert.IsNotNull(fileName, $"Expected fileName to contain dd_{questionnaire.ToLower()} but found {string.Join(",", filesInBucket)}");

            var questionnaires = _scenarioContext.Get<List<string>>(keyName);
            questionnaires.Add(fileName);
            _scenarioContext.Set(questionnaires, keyName);
        }

        [Then(@"no cases are delivered for '(.*)'")]
        public void ThenNoCasesAreDeliveredFor(string questionnaire)
        {
            var filesInBucket = _bucketHelper.GetFilesInBucket(_configurationHelper.BucketName);
            Assert.IsTrue(filesInBucket.Count() == 0);
        }

        [BeforeScenario]
        public void RemoveCases()
        {
            var bucketHelper = new BucketHelper();
            bucketHelper.DeleteFilesInBucket(_configurationHelper.BucketName);
        }

        private string ConvertToJsonMessage(MessageModel messageModel)
        {
            return JsonConvert.SerializeObject(messageModel,
                   new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
        }
    }
}
