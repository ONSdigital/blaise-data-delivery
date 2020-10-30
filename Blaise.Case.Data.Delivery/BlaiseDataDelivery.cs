using System.ServiceProcess;
using Blaise.Case.Data.Delivery.Interfaces.Services;
using Blaise.Case.Data.Delivery.Providers;

namespace Blaise.Case.Data.Delivery
{
    public partial class BlaiseDataDelivery : ServiceBase
    {
        public IInitialiseService InitialiseService;

        public BlaiseDataDelivery()
        {
            InitializeComponent();
            var unityProvider = new UnityProvider();

            InitialiseService = unityProvider.Resolve<IInitialiseService>();
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
