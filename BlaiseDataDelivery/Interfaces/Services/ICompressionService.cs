
using System.Collections.Generic;

namespace BlaiseDataDelivery.Interfaces.Services
{
    public interface ICompressionService
    {
        void CreateZipFile(IList<string> files, string filePath);
    }
}
