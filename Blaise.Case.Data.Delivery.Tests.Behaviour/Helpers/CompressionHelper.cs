using System.IO;
using Ionic.Zip;

namespace Blaise.Case.Data.Delivery.Tests.Behaviour.Helpers
{
    public class CompressionHelper
    {
        public void ExtractFileToDirectory(string zipFileName, string outputDirectory)
        {
            var zip = ZipFile.Read(zipFileName);
            Directory.CreateDirectory(outputDirectory);
            zip.ExtractAll(outputDirectory, ExtractExistingFileAction.OverwriteSilently);
        }
    }
}
