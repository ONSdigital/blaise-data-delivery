using System.Collections.Generic;

namespace BlaiseDataDelivery.Interfaces.Services
{
    public interface IFileService
    {
        string CreateEncryptedZipFile(IList<string> files, string instrumentName);

        void DeleteFile(string filePath);

        void DeleteFiles(IEnumerable<string> files);
    }
}