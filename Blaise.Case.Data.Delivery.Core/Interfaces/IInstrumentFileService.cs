using System.Collections.Generic;

namespace Blaise.Case.Data.Delivery.Core.Interfaces
{
    public interface IInstrumentFileService
    {
        string CreateEncryptedZipFile(IList<string> files, string instrumentName, string outputPath);

        void DeleteFile(string filePath);

        void DeleteFiles(IEnumerable<string> files);

        IEnumerable<string> GetFiles(string filePath);
    }
}