﻿

using System;
using System.Configuration;
using System.IO.Abstractions;
using Blaise.Nuget.Api;
using Blaise.Nuget.Api.Contracts.Interfaces;
using Blaise.Nuget.PubSub.Api;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using BlaiseDataDelivery.Interfaces.Mappers;
using BlaiseDataDelivery.Interfaces.Services;
using BlaiseDataDelivery.Mappers;
using BlaiseDataDelivery.MessageHandlers;
using BlaiseDataDelivery.Services;
using BlaiseDataDelivery.Interfaces.Providers;
using log4net;
using Unity;

namespace BlaiseDataDelivery.Providers
{
    public class UnityProvider
    {
        private readonly IUnityContainer _unityContainer;

        public UnityProvider()
        {
            _unityContainer = new UnityContainer();

            //register dependencies
            _unityContainer.RegisterSingleton<IFluentQueueApi, FluentQueueApi>();
            _unityContainer.RegisterFactory<ILog>(f => LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType));
            _unityContainer.RegisterType<IConfigurationProvider, ConfigurationProvider>();

            _unityContainer.RegisterType<IQueueService, QueueService>();

            //blaise services
            _unityContainer.RegisterType<IBlaiseApi, BlaiseApi>();

            //mappers
            _unityContainer.RegisterType<IMessageModelMapper, MessageModelMapper>();

            //services
            _unityContainer.RegisterType<IBlaiseService, BlaiseService>();
            _unityContainer.RegisterType<IDeliveryService, DeliveryService>();
            _unityContainer.RegisterType<IFileSystem, FileSystem>();
            _unityContainer.RegisterType<IEncryptionService, EncryptionService>();
            _unityContainer.RegisterType<ICompressionService, CompressionService>();
            _unityContainer.RegisterType<IFileService,FileService>();

            // If running in Debug, get the credentials file that has access to bucket and place it in a directory of your choice. 
            // Update the credFilePath variable with the full path to the file.
#if (DEBUG)
            var credentialKey = ConfigurationManager.AppSettings["GOOGLE_APPLICATION_CREDENTIALS"];

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialKey);
#endif

            //providers
            _unityContainer.RegisterType<IStorageClientProvider, StorageClientProvider>();
            _unityContainer.RegisterType<IBucketService, BucketService>();

            //main service classes
            _unityContainer.RegisterType<IInitialiseService, InitialiseService>();
            _unityContainer.RegisterType<IMessageHandler, MessageHandler>();
        }

        public T Resolve<T>()
        {
            return _unityContainer.Resolve<T>();
        }
    }
}