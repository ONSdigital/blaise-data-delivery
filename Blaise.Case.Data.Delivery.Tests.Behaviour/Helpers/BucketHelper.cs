using Google.Cloud.Storage.V1;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Blaise.Case.Data.Delivery.Tests.Behaviour.Helpers
{
    public class BucketHelper
    {
        private readonly ConfigurationHelper _configurationHelper;
        
        public BucketHelper()
        {
            _configurationHelper = new ConfigurationHelper();

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS",
                _configurationHelper.GoogleCredentials);
        }

        public bool FilesHaveBeenProcessed(string bucketName)
        {
            var storageClient = StorageClient.Create();
            var availableObjectsInBucket = storageClient.ListObjects(bucketName);

            return (availableObjectsInBucket.Count() > 0);
        }

        public IEnumerable<string> GetFilesInBucket(string bucketName)
        {
            var storageClient = StorageClient.Create();
            var availableObjectsInBucket = storageClient.ListObjects(bucketName, "");
            //ToDo: Not all objects are returned if the program runs lazily. 
            Thread.Sleep(3000);
            return availableObjectsInBucket.Select(o => o.Name).ToList();
        }

        public void DeleteFilesInBucket(string bucketName)
        {
            var storageClient = StorageClient.Create();
            var availableObjectsInBucket = storageClient.ListObjects(bucketName, "");

            foreach (var obj in availableObjectsInBucket)
            {
                storageClient.DeleteObject(obj);
            }
        }

        public void DownloadFromBucket(string fileName)
        {
            var client = StorageClient.Create();
            using (var stream = File.Create(_configurationHelper.LocalOutputPath + fileName))
                client.DownloadObject(_configurationHelper.BucketName, fileName, stream);
        }
    }
}
