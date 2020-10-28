using System.ServiceProcess;
using Blaise.Data.Delivery.Interfaces.Services;
using Blaise.Data.Delivery.Providers;
using log4net;

namespace Blaise.Data.Delivery
{
    public partial class BlaiseDataDelivery : ServiceBase
    {
        // Instantiate logger.
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public IInitialiseService InitialiseService;

        public BlaiseDataDelivery()
        {
            InitializeComponent();
            var unityProvider = new UnityProvider();

            InitialiseService = unityProvider.Resolve<IInitialiseService>();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Info("Start - data delivery service started.");
            InitialiseService.Start();
            Logger.Info("End - data delivery service started.");
        }

        protected override void OnStop()
        {
            Logger.Info("Start - data delivery service stopped.");
            InitialiseService.Stop();
            Logger.Info("Stop - data delivery service stopped.");
        }

        public void OnDebug()
        {
            OnStart(null);
        }
    }
}
