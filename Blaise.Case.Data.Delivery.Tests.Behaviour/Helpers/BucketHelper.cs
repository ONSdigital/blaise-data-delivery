using Google.Cloud.Storage.V1;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            if (availableObjectsInBucket.Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public IEnumerable<string> GetFilesInBucket(string bucketName)
        {
            var storageClient = StorageClient.Create();
            var availableObjectsInBucket = storageClient.ListObjects(bucketName, "");

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
    }
}
