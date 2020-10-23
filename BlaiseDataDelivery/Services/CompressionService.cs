using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlaiseDataDelivery.Helpers;
using BlaiseDataDelivery.Interfaces.Services;
using Ionic.Zip;

namespace BlaiseDataDelivery.Services
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
