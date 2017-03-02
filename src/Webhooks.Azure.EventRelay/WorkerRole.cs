using System.Collections.Generic;
using Funq;
using ServiceStack.Configuration;
using ServiceStack.Webhooks.Azure.Settings;
using ServiceStack.Webhooks.Azure.Worker;

namespace ServiceStack.Webhooks.Azure.EventRelay
{
    public class WorkerRole : AzureWorkerRoleEntryPoint
    {
        private List<WorkerEntryPoint> workers;

        protected override IEnumerable<WorkerEntryPoint> Workers
        {
            get { return workers; }
        }

        public override void Configure(Container container)
        {
            base.Configure(container);

            container.Register<IAppSettings>(new CloudAppSettings());
            container.Register(new EventRelayWorker(container));

            workers = new List<WorkerEntryPoint>
            {
                container.Resolve<EventRelayWorker>()
            };
        }
    }
}