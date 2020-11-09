using System.IO;
using Ionic.Zip;

namespace Blaise.Case.Data.Delivery.Tests.Behaviour.Helpers
{
    public class CompressionHelper
    {
        private ConfigurationHelper _configurationHelper;
        private string outputDirectory;

        public CompressionHelper()
        {
            _configurationHelper = new ConfigurationHelper();
            outputDirectory = _configurationHelper.LocalOutputPath;
        }

        public void ExtractFileToDirectory(string zipFileName)
        {
            var zip = ZipFile.Read(outputDirectory + zipFileName);
            Directory.CreateDirectory(outputDirectory + zipFileName.Split('.')[0]);
            zip.ExtractAll(outputDirectory, ExtractExistingFileAction.OverwriteSilently);
        }
    }
}
