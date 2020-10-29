using log4net;

namespace Blaise.Case.Data.Delivery
{
    static class Program
    {
        // Instantiate logger.
        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main()
        {
            // Call the service class if run in debug mode so no need to install service for testing.
#if DEBUG           
            Logger.Info("Blaise data delivery service starting in DEBUG mode.");
            var dataDeliveryService = new BlaiseDataDelivery();
            dataDeliveryService.OnDebug();
#else
            Logger.Info("Blaise data delivery service starting in RELEASE mode.");
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new BlaiseDataDelivery()
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}
