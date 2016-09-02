using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Sheepishly.IngestPlus
{
    using System.Fabric.Description;
    using System.Threading.Tasks;
    using Microsoft.Owin.Host.HttpListener;

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class IngestPlus : StatelessService
    {
        public IngestPlus(StatelessServiceContext context)
            : base(context)
        { }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return Context.CodePackageActivationContext.GetEndpoints()
                .Where(
                    endpoint =>
                        endpoint.Protocol.Equals(EndpointProtocol.Http) ||
                        endpoint.Protocol.Equals(EndpointProtocol.Https))
                .Select(
                    endpoint =>
                        new ServiceInstanceListener(
                            serviceContext => new OwinCommunicationListener("api", new Startup(), serviceContext)));

        }


    }
}
