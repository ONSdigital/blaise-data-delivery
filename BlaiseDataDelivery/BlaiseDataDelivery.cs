using System;
using System.Configuration;
using BlaiseDataDelivery.Interfaces.Mappers;
using BlaiseDataDelivery.Interfaces.Providers;
using BlaiseDataDelivery.Interfaces.Services;
using BlaiseDataDelivery.Interfaces.Services.Files;
using BlaiseDataDelivery.Interfaces.Services.Queue;
using BlaiseDataDelivery.Mappers;
using BlaiseDataDelivery.MessageHandlers;
using BlaiseDataDelivery.Providers;
using BlaiseDataDelivery.Services;
using BlaiseDataDelivery.Services.Files;
using BlaiseDataDelivery.Services.Queue;
using log4net;
using System.ServiceProcess;
using Blaise.Nuget.Api;
using Blaise.Nuget.Api.Contracts.Interfaces;
using Blaise.Nuget.PubSub.Api;
using Blaise.Nuget.PubSub.Contracts.Interfaces;
using Unity;

namespace BlaiseDataDelivery
{
    partial class BlaiseDataDelivery : ServiceBase
    {
        // Instantiate logger.
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IInitialiseService _dataDeliveryService;

        public BlaiseDataDelivery()
        {
            InitializeComponent();

            //IOC container
            IUnityContainer unityContainer = new UnityContainer().EnableDiagnostic();

            //register dependencies
            unityContainer.RegisterSingleton<IFluentQueueApi, FluentQueueApi>();
            unityContainer.RegisterFactory<ILog>(f => LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType));
            unityContainer.RegisterType<IConfigurationProvider, ConfigurationProvider>();

            unityContainer.RegisterType<IQueueService, QueueService>();

            unityContainer.RegisterType<IMessageModelMapper, MessageModelMapper>();

            unityContainer.RegisterType<IFileService, FileService>();
            unityContainer.RegisterType<IFileDirectoryService, FileDirectoryService>();
            unityContainer.RegisterType<IFileEncryptionService, FileEncryptionService>();
            unityContainer.RegisterType<IFileZipService, FileZipService>();

            // If running in Debug, get the credentials file that has access to bucket and place it in a directory of your choice. 
            // Update the credFilePath variable with the full path to the file.
#if (DEBUG)
            var credentialKey = ConfigurationManager.AppSettings["GOOGLE_APPLICATION_CREDENTIALS"];

            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialKey);
#endif
            unityContainer.RegisterType<IStorageClientProvider, StorageClientProvider>();
            unityContainer.RegisterType<IFileCloudStorageService, FileCloudStorageService>();

            //blaise services
            unityContainer.RegisterType<IBlaiseApi, BlaiseApi>();

            //main service classes
            unityContainer.RegisterType<IInitialiseService, InitialiseService>();
            unityContainer.RegisterType<IMessageHandler, DataDeliveryMessageHandler>();

            //resolve all dependencies as DataDeliveryService class is the main service entry point
            _dataDeliveryService = unityContainer.Resolve<IInitialiseService>();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Info("Start - data delivery service started.");
            _dataDeliveryService.Start();
            Logger.Info("End - data delivery service started.");
        }

        protected override void OnStop()
        {
            Logger.Info("Start - data delivery service stopped.");
            _dataDeliveryService.Stop();
            Logger.Info("Stop - data delivery service stopped.");
        }

        public void OnDebug()
        {
            OnStart(null);
        }
    }
}
