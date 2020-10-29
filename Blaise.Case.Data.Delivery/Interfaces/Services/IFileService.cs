using System.Collections.Generic;

namespace Blaise.Case.Data.Delivery.Interfaces.Services
{
    public interface IFileService
    {
        string CreateEncryptedZipFile(IList<string> files, string instrumentName, string filePath);

        void DeleteFile(string filePath);

        void DeleteFiles(IEnumerable<string> files);

        IEnumerable<string> GetFiles(string filePath);
    }
}