using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blaise.Case.Data.Delivery.Tests.Behaviour.Helpers
{
   public class ConfigurationHelper
   {
        public string ProjectId => GetVariable("ProjectId");

        public string BlaiseServerPassword => GetVariable("BlaiseServerPassword");
        public string BlaiseServerHostName => GetVariable("BlaiseServerHostName");
        public string TopicId => GetVariable("TopicId");
        public string BucketName => GetVariable("BucketName");      
        public string ServerParkName => GetVariable("ServerParkName");
        public string GoogleCredentials => GetVariable("GoogleCredentials");

        private static string GetVariable(string variableName)
        {
            var value = ConfigurationManager.AppSettings[variableName];
            return value;
        }
    }
}
