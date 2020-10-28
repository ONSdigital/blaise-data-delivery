
using System.Collections.Generic;

namespace Blaise.Data.Delivery.Interfaces.Services
{
    public interface ICompressionService
    {
        void CreateZipFile(IList<string> files, string filePath);
    }
}
