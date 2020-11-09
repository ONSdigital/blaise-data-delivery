using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blaise.Case.Data.Delivery.Core.Extensions;
using Blaise.Case.Data.Delivery.Core.Interfaces;
using Ionic.Zip;

namespace Blaise.Case.Data.Delivery.Core.Files.Compression
{
    public class CompressionService : ICompressionService
    {
        public void CreateZipFile(IList<string> files, string filePath)
        {
            if (!files.Any())
            {
                throw new ArgumentException("No files provided");
            }

            filePath.ThrowExceptionIfNullOrEmpty("filePath");

            //create any folders that may not exist
            // ReSharper disable once AssignNullToNotNullAttribute
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate))
            using (var streamWriter = new StreamWriter(fileStream))
            using (var zip = new ZipFile())
            {
                zip.AddFiles(files, @"\");
                zip.Save(streamWriter.BaseStream);
            }
        }
    }
}
