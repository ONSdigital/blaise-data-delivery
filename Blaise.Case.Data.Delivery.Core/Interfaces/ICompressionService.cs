
using System.Collections.Generic;

namespace Blaise.Case.Data.Delivery.Core.Interfaces
{
    public interface ICompressionService
    {
        void CreateZipFile(IList<string> files, string filePath);
    }
}
