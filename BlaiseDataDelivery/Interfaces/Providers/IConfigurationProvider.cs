﻿
namespace BlaiseDataDelivery.Interfaces.Providers
{
    public interface IConfigurationProvider
    {
        string ProjectId { get; }

        string SubscriptionTopicId { get; }

        string SubscriptionId { get; }

        string VmName { get; }

        string BucketName { get; }

        string EncryptionKey { get; }

        string DeadletterTopicId { get; }

        string LocalProcessFolder { get; }
    }
}
