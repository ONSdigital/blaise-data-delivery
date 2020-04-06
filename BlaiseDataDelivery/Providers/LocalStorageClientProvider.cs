﻿using BlaiseDataDelivery.Interfaces.Providers;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using System.IO;

namespace BlaiseDataDelivery.Providers
{
    public class LocalStorageClientProvider : IStorageClientProvider
    {
        private readonly IConfigurationProvider _configuration;

        public LocalStorageClientProvider(IConfigurationProvider configuration)
        {
            _configuration = configuration;
        }

        public StorageClient GetStorageClient()
        {
            var key = _configuration.CloudStorageKey;
            var googleCredStream = GoogleCredential.FromStream(File.OpenRead(key));
            var bucket = StorageClient.Create(googleCredStream);

            return bucket;
        }
    }
}
