﻿using System;
using BlaiseDataDelivery.Interfaces.Providers;
using System.Configuration;

namespace BlaiseDataDelivery.Providers
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        public string ProjectId => ConfigurationManager.AppSettings["ProjectId"];

        public string SubscriptionTopicId => ConfigurationManager.AppSettings["SubscriptionTopicId"];

        public string SubscriptionId => ConfigurationManager.AppSettings["SubscriptionId"];

        public string VmName => Environment.MachineName;

        public string FilePattern => ConfigurationManager.AppSettings["FilePattern"];

        public string BucketName => ConfigurationManager.AppSettings["BucketName"];

        public string CloudStorageKey => ConfigurationManager.AppSettings["CloudStorageKey"];

        public string EncryptionKey => ConfigurationManager.AppSettings["EncryptionKey"];

        public string DeadletterTopicId => ConfigurationManager.AppSettings["DeadletterTopicId"];
    }
}
