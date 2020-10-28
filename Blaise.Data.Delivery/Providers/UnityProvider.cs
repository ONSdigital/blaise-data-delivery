using System;
using System.Configuration;
using System.IO.Abstractions;
using Blaise.Data.Delivery.Interfaces.Mappers;
using Blaise.Data.Delivery.Interfaces.Providers;
using Blaise.Data.Delivery.Interfaces.Services;
using Blaise.Data.Delivery.Mappers;
using Blaise.Data.Delivery.MessageHandlers;
using Blaise.Data.Delivery.Services;
using Blaise.Nuget.Api;
using Blaise.Nuget.Api.Contracts.Interfaces;
using Blaise.Nuget.PubSub.Api;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using log4net;
using Unity;

namespace Blaise.Data.Delivery.Providers
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

            _unityContainer.RegisterType<IQueueService, QueueService>();

            //blaise services
            _unityContainer.RegisterType<IBlaiseApi, BlaiseApi>();

            //mappers
            _unityContainer.RegisterType<IMessageModelMapper, MessageModelMapper>();

            //services
            _unityContainer.RegisterType<IBlaiseApiService, BlaiseApiService>();
            _unityContainer.RegisterType<IDeliveryService, DeliveryService>();
            _unityContainer.RegisterType<IFileSystem, FileSystem>();
            _unityContainer.RegisterType<IEncryptionService, EncryptionService>();
            _unityContainer.RegisterType<ICompressionService, CompressionService>();
            _unityContainer.RegisterType<IFileService,FileService>();

            // If running in Debug, get the credentials file that has access to bucket and place it in a directory of your choice. 
            // Update the credFilePath variable with the full path to the file.
#if (DEBUG)
            // When running in Release, the service will be running as compute account which will have access to all buckets. In test we need to get credentials
            var credentialKey = ConfigurationManager.AppSettings["GOOGLE_APPLICATION_CREDENTIALS"];
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialKey);

            _unityContainer.RegisterType<IConfigurationProvider, LocalConfigurationProvider>();
#else
            _unityContainer.RegisterType<IConfigurationProvider, ConfigurationProvider>();
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
