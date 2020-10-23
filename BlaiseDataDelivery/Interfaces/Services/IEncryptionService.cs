

namespace BlaiseDataDelivery.Interfaces.Services
{
    public interface IEncryptionService
    {
        void EncryptFile(string inputFilePath, string outputFilePath);
    }
}
