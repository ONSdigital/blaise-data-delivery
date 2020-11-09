using System.ServiceProcess;
using Blaise.Case.Data.Delivery.WindowsService.Interfaces;
using Blaise.Case.Data.Delivery.WindowsService.Ioc;

namespace Blaise.Case.Data.Delivery.WindowsService
{
    public partial class BlaiseDataDelivery : ServiceBase
    {
        public IInitialiseWindowsService InitialiseService;

        public BlaiseDataDelivery()
        {
            InitializeComponent();
            var unityProvider = new UnityProvider();

            InitialiseService = unityProvider.Resolve<IInitialiseWindowsService>();
        }

        protected override void OnStart(string[] args)
        {
            InitialiseService.Start();
        }

        protected override void OnStop()
        {
            InitialiseService.Stop();
        }

        public void OnDebug()
        {
            OnStart(null);
        }
    }
}
