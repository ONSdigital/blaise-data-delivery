

namespace Blaise.Case.Data.Delivery.Core.Interfaces
{
    public interface IEncryptionService
    {
        void EncryptFile(string inputFilePath, string outputFilePath);
    }
}
