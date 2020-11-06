using Blaise.Nuget.Api;
using Blaise.Nuget.Api.Contracts.Interfaces;
using Blaise.Nuget.Api.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StatNeth.Blaise.Instrument;
using System.Security.Cryptography.X509Certificates;
using StatNeth.Blaise.Data.SQLParser;
using System.Security;

namespace Blaise.Case.Data.Delivery.Tests.Behaviour.Helpers
{
    public class CaseHelper
    {
        private readonly IBlaiseApi _blaiseApi;
        private readonly ConfigurationHelper _configurationHelper;
        private readonly ConnectionModel _connectionModel;
        private int _primaryKey;

        public CaseHelper()
        {
            _blaiseApi = new BlaiseApi();
            _configurationHelper = new ConfigurationHelper();
            _connectionModel = _blaiseApi.GetDefaultConnectionModel();
            _primaryKey = 90000;
        }

        public void CreateCasesForAnInstrument(int numberOfCases, bool completed = true)
        {
            for (var i = 0; i < numberOfCases; i++)
            {
                _primaryKey++;
                CreateCaseInAnInstrument(_primaryKey, completed);
            }
        }

        public void CreateCaseInAnInstrument(int primaryKey, bool completed)
        {
            var dic = new Dictionary<string, string>();
            dic.Add("serial_number", primaryKey.ToString());
            dic.Add("completed", completed.ToString());
            _blaiseApi.CreateNewDataRecord(_connectionModel, $"{primaryKey}", dic, _configurationHelper.InstrumentName, _configurationHelper.ServerParkName);
        }

        public int GetCasesInAnInstrument(string questionnaire)
        {
            var casesInDatabase = _blaiseApi.GetDataSet(_connectionModel,
                questionnaire, _configurationHelper.ServerParkName);

            return casesInDatabase.RecordCount;
        }

        public void DeleteCasesInDatabase(string questionnaire)
        {
            var cases = _blaiseApi.GetDataSet(_connectionModel,
                questionnaire, _configurationHelper.ServerParkName);

            while (!cases.EndOfSet)
            {
                var primaryKey = _blaiseApi.GetPrimaryKeyValue(cases.ActiveRecord);

                _blaiseApi.RemoveCase(_connectionModel, primaryKey,
                    questionnaire, _configurationHelper.ServerParkName);

                cases.MoveNext();
            }
        }
    }
}
